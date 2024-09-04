using System;
using System.Text;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.Map;
using Mapbox.Json;
using MappingAI;
using System.Linq;

/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */

public class ExportController : MonoBehaviour
{
    //[SerializeField]
    private DrawingCanvas canvas = null;


    //public ExportMode exportMode = ExportMode.Curves;

    [Header("Export formats")]

    [SerializeField]
    private bool ExportFinalStrokes = true;

    [SerializeField]
    private bool ExportInputStrokes = true;

    [SerializeField]
    private bool ExportSketchOBJ = true;

    [SerializeField]
    private bool ExportGraphData = true;

    [SerializeField]
    private bool ExportAIGeneratedMeshOBJ = true;    
    
    [SerializeField]
    private bool ExportAIHeightMap = true;    

    [SerializeField]
    private bool ExportLandMarks = true;

    private static int textureSize = 171; //the size for the export PNG for height
    private static int terrainTextureSize = 1024; //the size for the export PNG for terrain texture

    [Header("Export options")]

    //public float StrokeWidth;
    [SerializeField]
    private int SubdivisionsPerUnit;    
    
    [SerializeField]
    private GameObject LandmarkContainer;

    [SerializeField]
    private bool ClearAfterExport = false;

    [Header("Export folder name")]

    [SerializeField]
    private static string ExportFolderName = "SketchData~"; // Tilde makes Unity ignore this folder, so it doesn' t try to "import" the content as Assets (which breaks with the custom .curves format)
    
    string path;
    [SerializeField]
    private string TerrainPath;

    private AbstractMap TerrainAbstractMap;

    [SerializeField]
    private GameObject SketchingBox;
    [SerializeField]
    private MeshFilter AIModelMeshFilter;

    private AIModelManager aIModelManager;

    [SerializeField]
    private float LowerBound = 0;

    [SerializeField]
    private float UpperBound = 1f;

    private TerrainManagerAsync terrainManagerAsync;
    private InputController inputController;
    private void Start()
    {
        aIModelManager = ComponentManager.Instance.GetAIModelManager();
        inputController = ComponentManager.Instance.GetInputController();
        TerrainAbstractMap = ComponentManager.Instance.GetTerrainManagerAsync().GetComponent<AbstractMap>();
        terrainManagerAsync = ComponentManager.Instance.GetTerrainManagerAsync().GetComponent<TerrainManagerAsync>();
        canvas = ComponentManager.Instance.GetDrawingCanvas();

        //path = Path.Combine(Application.persistentDataPath, ExportFolderName);
#if UNITY_EDITOR
        path = Path.Combine(Application.dataPath, ExportFolderName);
#else
        path = Path.Combine(Application.persistentDataPath, ExportFolderName);
#endif
    }

    public string GetTerrainPath()
    {
        return TerrainPath;
    }

    public static int GetTerrainTextureSize()
    {
        return terrainTextureSize;
    }    
    
    public static string GetExportFolderName()
    {
        return ExportFolderName;
    }

    public void ExportSketch(string fileName = null)
    {
        TerrainPath = GenerateFileName();
        if (ExportFinalStrokes)
        {
            //Debug.Log("[EXPORT] Exporting sketch as .curves (final strokes).");
            ExportToCurves(TerrainPath, finalStrokes: true); // default file name

        }
        if (ExportSketchOBJ)
        {
            //Debug.Log("[EXPORT] Exporting sketch as OBJ.");
            ExportToOBJ(TerrainPath); // default file name
        }

        if (ExportInputStrokes)
        {
            //Debug.Log("[EXPORT] Exporting sketch as .curves (input strokes).");
            ExportToCurves(TerrainPath, finalStrokes: false); // default file name
        }

        if (ExportAIHeightMap)
        {
            ExportAIHeightmap();
        }

        if (ExportGraphData)
        {
            //Debug.Log("[EXPORT] Exporting graph data as JSON file.");
            ExportCurveNetwork(TerrainPath);
        }

        if (ClearAfterExport)
        {
            //Debug.Log("[EXPORT] Clearing the scene.");
            canvas.Clear();
        }
    }

    private string GenerateFileName()
    {
        string terrainPathName = "";
        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
        {
            string name = "_Con" + ApplicationSettings.Instance.ExperimentCondition + "_Scene_" + terrainManagerAsync.GetPreviousIndexForGivenTerrain();
            if (inputController.FreeCreationModeInExperiment())
            {
                name = "_FreeCreation";
            }
            terrainPathName = "ID_" + ApplicationSettings.Instance.participantID + name + "_" + DefaultFileName();
        }
        else
        {
            terrainPathName = "ID_" + ApplicationSettings.Instance.participantID + "_" + DefaultFileName() + "_Con_" + ApplicationSettings.Instance.ExperimentCondition + "_Cood_" + Math.Round(TerrainAbstractMap.CenterLatitudeLongitude.x, 4) + "," + Math.Round(TerrainAbstractMap.CenterLatitudeLongitude.y, 4);
        }
        return Path.Combine(path, terrainPathName);
    }

    public void ExportAIHeightmap()
    {
        string fileNamHeightMapTexture = "AI_heightmap" + ".png";
        if (TerrainPath.Length == 0)
        {
            var terrainPathName = "ID_" + ApplicationSettings.Instance.participantID + "_Test_" + DefaultFileName();
            TerrainPath = Path.Combine(path, terrainPathName);
        }
        TryCreateDirectory(TerrainPath);
        string fullfileNameHeightMapTexture = Path.Combine(TerrainPath, fileNamHeightMapTexture);
        //string fullfileNameGradientHeightMapTexture = Path.Combine(TerrainPath, "Gradient_" + fileNamHeightMapTexture);
        string fullfileNameInputHeightMapTexture = Path.Combine(TerrainPath, "Input_" + fileNamHeightMapTexture);
        string fullfileNameInputTxtHeightMapTexture = Path.Combine(TerrainPath, "Input_Txt_" + "AI_heightmap" + ".txt");
        string fullfileNameInputPredicted_resultTxtHeightMapTexture = Path.Combine(TerrainPath, "Predicted_result_Txt_" + "AI_heightmap" + ".txt");

        //var heightMapGradientTexture = aIModelManager.GetHeightMapGradientTexture();
        var heightMapTexture = aIModelManager.GetHeightMapTexture();
        var inputTexture = aIModelManager.GetInputDataTexture();
        var inputPublic = aIModelManager.GetInput_public();
        var predicted_result = aIModelManager.GetPredicted_result();
        if (heightMapTexture != null)
        {
            byte[] bytes = heightMapTexture.EncodeToPNG();
            File.WriteAllBytes(fullfileNameHeightMapTexture, bytes);
        }

        if (inputTexture != null)
        {
            byte[] bytes = inputTexture.EncodeToPNG();
            File.WriteAllBytes(fullfileNameInputHeightMapTexture, bytes);
        }
        if (inputPublic != null)
        {
            WriteArrayToFile(inputPublic, fullfileNameInputTxtHeightMapTexture);

        }

        if (predicted_result != null)
        {
            WriteArrayToFile(predicted_result, fullfileNameInputPredicted_resultTxtHeightMapTexture);
        }
    }
    void WriteArrayToFile(float[] array, string path)
    {
        using (StreamWriter writer = new StreamWriter(path))
        {
            foreach (float value in array)
            {
                writer.WriteLine(value);
            }
        }
    }
    public void ExportToOBJ(string TerrainPath = null)
    {
        List<int> strokeIDs = new List<int>();
        List<int> patchIDs = new List<int>();


        // Try to create the directory
        TryCreateDirectory(TerrainPath);

        string fileNameStrokes =  "Strokes" + ".obj";
        string fullfileNameStrokes = Path.Combine(TerrainPath, fileNameStrokes);
        string fullfileName2DStrokes = Path.Combine(TerrainPath, "2D_"+fileNameStrokes);

        string fullfileNameLandmarks = Path.Combine(TerrainPath, "Landmarks.obj");
        string fullfileNameLandmarksTransforms = Path.Combine(TerrainPath, "LandmarksTransform.json");

        string fileNameMaterials = "Material.mtl";
        string fullfileNameMaterials = Path.Combine(TerrainPath, fileNameMaterials);
        //string fileNamePatches = name + "_Terrain_Mesh" + ".obj";
        //string fullfileNamePatches = Path.Combine(TerrainPath, fileNamePatches);

        string fileNameDEM = "Terrain_heightmap" + ".png";
        string fullfileNameDEM = Path.Combine(TerrainPath, fileNameDEM);

        string fileNameAIDEM = "AI_Terrain_heightmap" + ".png";
        string fullfileNameAIDEM = Path.Combine(TerrainPath, fileNameAIDEM);

        var sketchDataName = "Sketch.json";
        var fullfileNameSketch = Path.Combine(TerrainPath, sketchDataName);

        // STROKES && TEXT
        if (canvas.TerrainStrokes.Count > 0 || canvas.TextStrokes.Count > 0 || canvas.LandmarkStrokes.Count > 0)
        {
            int strokeID = 0;
            string curves = "";
            string mtlContent = "";
            // Dictionary to track processed materials
            Dictionary<string, string> processedMaterials = new Dictionary<string, string>();

            File.Create(fullfileNameStrokes).Dispose();
            ObjExporterScript.Start();

            foreach (FinalStroke s in canvas.TerrainStrokes)
            {
                // First compute the mesh for the stroke
                //Mesh mesh = StrokeBrush.Solidify(s.Curve);
                if (s == null)
                    continue;
                Mesh mesh = s.gameObject.GetComponent<MeshCollider>().sharedMesh;
                Material mat = s.GetComponent<LineRenderer>().material;
                // Add stroke ID as a group name
                strokeID = s.ID;
                curves += string.Format("g {0}\n", s.ID);

                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        mat,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                curves += objString;
                // Generate MTL content if material has not been processed
                string materialName = ObjExporterScript.GenerateMaterialName(mat);
                if (!processedMaterials.ContainsKey(materialName))
                {
                    string materialString = ObjExporterScript.MaterialToString(mat);
                    mtlContent += materialString;
                    processedMaterials[materialName] = materialString;
                }
                // Store ID
                if (s as FinalStroke != null)
                    strokeIDs.Add(((FinalStroke)s).ID);
            }
            foreach (FinalStroke s in canvas.LandmarkStrokes)
            {
                // First compute the mesh for the stroke
                if (s == null)
                    continue;
                Mesh mesh = s.gameObject.GetComponent<MeshCollider>().sharedMesh;
                Material mat = s.GetComponent<LineRenderer>().material;
                // Add stroke ID as a group name
                strokeID = s.ID;
                curves += string.Format("g {0}\n", s.ID);

                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        mat,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                curves += objString;
                // Generate MTL content if material has not been processed
                string materialName = ObjExporterScript.GenerateMaterialName(mat);
                if (!processedMaterials.ContainsKey(materialName))
                {
                    string materialString = ObjExporterScript.MaterialToString(mat);
                    mtlContent += materialString;
                    processedMaterials[materialName] = materialString;
                }
                // Store ID
                if (s as FinalStroke != null)
                    strokeIDs.Add(((FinalStroke)s).ID);
            }
            foreach (TextStroke s in canvas.TextStrokes)
            {
                // First compute the mesh for the stroke
                Mesh mesh = s.gameObject.GetComponent<MeshCollider>().sharedMesh;
                Material mat = s.GetComponent<LineRenderer>().material;
                // Add stroke ID as a group name
                strokeID++;
                curves += string.Format("g {0}\n", strokeID);
                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        mat,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                curves += objString;
                // Generate MTL content if material has not been processed
                string materialName = ObjExporterScript.GenerateMaterialName(mat);
                if (!processedMaterials.ContainsKey(materialName))
                {
                    string materialString = ObjExporterScript.MaterialToString(mat);
                    mtlContent += materialString;
                    processedMaterials[materialName] = materialString;
                }
            }
            if (ExportAIGeneratedMeshOBJ && ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._AI)
            {
                strokeID++;
                curves += string.Format("g {0}\n", strokeID);
                Material mat = AIModelMeshFilter.GetComponent<MeshRenderer>().material;

                Texture2D texture2D = (Texture2D)mat.mainTexture;
                string textureFilePath = Path.Combine(TerrainPath, "AI_Texture" + texture2D.name + ".png");
                // Save the combined texture as a PNG file
                File.WriteAllBytes(textureFilePath, texture2D.EncodeToPNG());

                string objString = ObjExporterScript.MeshToStringFromWorld2LocalMirrorFlip(
                                        AIModelMeshFilter.sharedMesh, mat, AIModelMeshFilter.transform);
                curves += objString;
                string materialName = ObjExporterScript.GenerateMaterialName(mat);
                if (!processedMaterials.ContainsKey(materialName))
                {
                    string materialString = ObjExporterScript.MaterialToString(mat);
                    mtlContent += materialString;
                    processedMaterials[materialName] = materialString;
                }
            }
            // Include the reference to the .mtl file in the .obj content
            curves = "mtllib " + Path.GetFileName(fullfileNameMaterials) + "\n" + curves;

            ObjExporterScript.End();
            File.WriteAllText(fullfileNameStrokes, curves);

            // Save MTL file
            File.WriteAllText(fullfileNameMaterials, mtlContent);
        }

        if (canvas._2DStrokes.Count > 0)
        {
            string curves = "";
            string mtlContent = "";
            // Dictionary to track processed materials
            Dictionary<string, string> processedMaterials = new Dictionary<string, string>();

            File.Create(fullfileName2DStrokes).Dispose();
            ObjExporterScript.Start();
            foreach (_2DFinalStroke s in canvas._2DStrokes)
            {
                // First compute the mesh for the stroke
                Mesh mesh = s.gameObject.GetComponent<MeshCollider>().sharedMesh;
                Material mat = s.GetComponent<LineRenderer>().material;
                // Add stroke ID as a group name
                curves += string.Format("g {0}\n", s.ID);

                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        mat,
                                        s.GetComponent<Transform>(),
                                        objectSpace: true);
                curves += objString;
                // Generate MTL content if material has not been processed
                string materialName = ObjExporterScript.GenerateMaterialName(mat);
                if (!processedMaterials.ContainsKey(materialName))
                {
                    string materialString = ObjExporterScript.MaterialToString(mat);
                    mtlContent += materialString;
                    processedMaterials[materialName] = materialString;
                }
            }
            // Include the reference to the .mtl file in the .obj content
            curves = "mtllib " + Path.GetFileName(fullfileNameMaterials) + "\n" + curves;

            ObjExporterScript.End();
            File.WriteAllText(fullfileName2DStrokes, curves);

            // Save MTL file
            File.WriteAllText(fullfileNameMaterials, mtlContent);
        }

        if (ExportLandMarks)
        {
            string curves = "";
            // Dictionary to track processed materials
            Dictionary<string, string> processedMaterials = new Dictionary<string, string>();
            File.Create(fullfileNameLandmarks).Dispose();
            File.Create(fullfileNameLandmarksTransforms).Dispose();
            ObjExporterScript.Start();

            TransformDataList dataList = new TransformDataList();

            Transform landmarkFather = LandmarkContainer.transform.GetChild(terrainManagerAsync.GetPreviousIndexForLandmark());
            List<MeshFilter> meshFilters = landmarkFather.GetComponentsInChildren<MeshFilter>().ToList();
            for (int i = 0; i < meshFilters.Count; i++)
            {
                MeshFilter filter = meshFilters[i];
                // First compute the mesh for the stroke
                Mesh mesh = filter.sharedMesh;


                // Add stroke ID as a group name
                curves += string.Format("g {0}\n", i);

                string objString = ObjExporterScript.MeshToString(
                                        mesh,
                                        filter.transform,
                                        parent: canvas.transform,
                                        objectSpace: false);
                curves += objString;
            }

            for (int i = 0; i < landmarkFather.childCount; i++)
            {
                TransformData data = new TransformData
                {
                    name = landmarkFather.GetChild(i).gameObject.name,
                    localPosition = landmarkFather.GetChild(i).transform.localPosition
                };
                dataList.transforms.Add(data);
            }
            SaveTransforms(dataList, fullfileNameLandmarksTransforms);

            ObjExporterScript.End();
            File.WriteAllText(fullfileNameLandmarks, curves);
        }

        // TERRAINS
        UnityTile[] allTiles = TerrainAbstractMap.GetComponentsInChildren<UnityTile>();
        if (allTiles.Length > 0)
        {
            allTiles = allTiles.OrderBy(tile => tile.CanonicalTileId.Y)
                           .ThenBy(tile => tile.CanonicalTileId.X)
                           .ToArray();
            ObjExporterScript.Start();
            GameObject go = ObjExporterScript.GenerateTerrainCombinedMesh(allTiles, terrainManagerAsync.GetIndex());
            ObjExporterScript.ExportMeshAndTexture(go.GetComponent<MeshFilter>().sharedMesh, go.transform, (Texture2D)go.GetComponent<Renderer>().material.mainTexture, TerrainPath, "Terrain");
            Destroy(go);
            ObjExporterScript.End();
            // DEM
            HeightMapExport.ExportHeightmaps(fullfileNameDEM, allTiles.ToList(), TerrainAbstractMap, textureSize, terrainManagerAsync.getHeightDiff(), GetYBounds().Item1, GetYBounds().Item2);
        }

        // SKETCH END DATA
        SketchEndData sketchData = new SketchEndData(strokeIDs, patchIDs);

        File.WriteAllText(fullfileNameSketch, JsonConvert.SerializeObject(sketchData, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));
    }

    [System.Serializable]
    public class TransformData
    {
        public string name;
        public Vector3 localPosition;
    }

    public void SaveTransforms(TransformDataList dataList, String filePath)
    {
        string json = JsonUtility.ToJson(dataList);
        Debug.Log(json);
        File.WriteAllText(filePath, json);
    }
    [System.Serializable]
    public class TransformDataList
    {
        public List<TransformData> transforms = new List<TransformData>();
    }
    public void WriteJsonToFile(string fullPath, object data)
    {
        // Serialize the data to a JSON string with specific culture settings
        JsonSerializerSettings settings = new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        };
        string jsonString = JsonConvert.SerializeObject(data, settings);

        // Write the JSON string to a file
        File.WriteAllText(fullPath, jsonString);
    }
    /// <summary>
    ///  only export terrain strokes, no landmark
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="finalStrokes"></param>
    public void ExportToCurves(string TerrainPath = null, bool finalStrokes = true)
    {
        //string name = fileName ?? DefaultFileName();

        // Try to create the directory
        TryCreateDirectory(TerrainPath);

        string fileNameCurve = "Strokes";
        string fileNameLerpCurve = "LerpStrokes";

        if (!finalStrokes)
        {
            fileNameCurve += "-input";
            fileNameLerpCurve += "-input";

        }
        fileNameCurve += ".curves";
        fileNameLerpCurve += ".curves";

        string fullfileNameCurve = Path.Combine(TerrainPath, fileNameCurve);
        string fullfileNameLerpCurve = Path.Combine(TerrainPath, fileNameLerpCurve);

        File.Create(fullfileNameCurve).Dispose();
        File.Create(fullfileNameLerpCurve).Dispose();

        StringBuilder curves = new StringBuilder();
        StringBuilder LerpCurves = new StringBuilder();

        
        foreach (FinalStroke s in canvas.TerrainStrokes)
        {
            if (finalStrokes)
            {
                // only export terrain strokes, no landmark
                if (s.GetColorProperty() == ColorProperty.Terrain)
                {
                    string curveString = CurvesExport.CurveToPolyline(s.Curve, SubdivisionsPerUnit);
                    string LerpCurvesString = CurvesExport.CurveToPolylineLerp(s.Curve, SubdivisionsPerUnit, GetYBounds().Item1, GetYBounds().Item2);
                    curves.Append(curveString);
                    LerpCurves.Append(LerpCurvesString);
                }
            }
            else
            {
                string stroke = CurvesExport.SamplesToPolyline(s.inputSamples);
                curves.Append(stroke);
            }
        }

        File.WriteAllText(fullfileNameCurve, curves.ToString());
        File.WriteAllText(fullfileNameLerpCurve, LerpCurves.ToString());
    }



    public Tuple<float, float> GetYBounds()
    {

        return new Tuple<float, float>(LowerBound * SketchingBox.transform.localScale.y, UpperBound * SketchingBox.transform.localScale.y);
        
    }

    public Tuple<float, float> GetXZBounds()
    {

        return new Tuple<float, float>(LowerBound * SketchingBox.transform.localScale.x, UpperBound * SketchingBox.transform.localScale.x);

    }

    public void ExportCurveNetwork(string TerrainPath = null)
    {
        //string name = fileName ?? DefaultFileName();
        // Try to create the directory
        TryCreateDirectory(TerrainPath);

        string fileNameNet = "graph.json";
        string fullfileNameNet = Path.Combine(TerrainPath, fileNameNet);

        List<SerializableStrokeInGraph> strokes = new List<SerializableStrokeInGraph>();
        List<SerializableSegment> segments = new List<SerializableSegment>();
        List<SerializableNode> nodes = canvas.Graph.GetNodesData();

        foreach (FinalStroke s in canvas.TerrainStrokes)
        {
            if (s.GetColorProperty() == ColorProperty.Terrain)
            {
                (SerializableStrokeInGraph strokeData, List<SerializableSegment> strokeSegments) = s.GetGraphData();
                strokes.Add(strokeData);
                segments.AddRange(strokeSegments);
            }
        }

        CurveNetworkData graphData = new CurveNetworkData(strokes, segments, nodes);

        File.WriteAllText(fullfileNameNet, JsonConvert.SerializeObject(graphData, new JsonSerializerSettings
        {
            Culture = new System.Globalization.CultureInfo("en-US")
        }));
    }

    private string DefaultFileName()
    {
        
        return String.Format("{0:yyMMddHHmmss}", DateTime.Now);
    }

    private void TryCreateDirectory(string path)
    {
        // Try to create the directory
        try
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

        }
        catch (IOException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}
