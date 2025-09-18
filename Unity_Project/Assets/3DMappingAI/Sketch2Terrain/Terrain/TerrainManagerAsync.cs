using Mapbox.Unity.Map;
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using Mapbox.Utils;
using TMPro;
using MappingAI;
using System.Threading.Tasks;
using Mapbox.Unity.MeshGeneration.Data;

using Mapbox.Unity.MeshGeneration.Enums;
using static Mapbox.Map.Tile;

public class TerrainManagerAsync : MonoBehaviour
{
    private List<Vector2d> Lat_lon_list = new List<Vector2d>();
    private List<int> Zoom_list = new List<int>();
    private List<MeshRenderer> MeshRenderer_list = new List<MeshRenderer>();
    private Vector2d Current_Lat_lon = Vector2d.zero;
    public static System.Random random;
    private int index = -1;
    private int indexForGivenTerrain = 0;
    private int indexForLandmark = 0;
    private int indexForExample = 0;
    [SerializeField]
    private GameObject SketchBox;

    [SerializeField]
    private Transform TerrainContainer;
    [SerializeField]
    private Transform landmarkContainer;
    [SerializeField]
    private Transform _3DExampleContainer;
    [SerializeField]
    private Transform _2DExampleContainer;
    [SerializeField]
    private float defaultZoom = 14f;
    [SerializeField]
    private bool useGivenCoord = true;
    [SerializeField]
    private Vector2d givenCoord = new Vector2d(46.2330324523126, 7.87139176567208);

    [HideInInspector]
    private int previousIndexForGivenTerrain = 0;
    private int previousIndexForLandmark = 0;
    private int previousIndexForExample = 0;
    private AbstractMap Terrain;
    private StudyScenario scenario;
    private InputController inputController;

    private GameObject CurrentTerrainGameObject;
    private GameObject CurrentLandmarkGameObject;
    private GameObject CurrentExampleGameObject;
    private bool isGenerateMeshSuceesss = false;
    private float HeightDiff = 0;

    private int selfDeterminedScene = -1;
    private int statredIndex = 0;

    private ComponentManager componentManager;
    private void OnEnable()
    {
        Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => { if (scenario.GetCurrentStep().Zoom > 10) { NextTerrain(scenario.GetCurrentStep().Zoom); } else { NextTerrain(defaultZoom); } });
        Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => { if (scenario.GetCurrentStep().Zoom > 10) { NextTerrain(scenario.GetCurrentStep().Zoom); } else { NextTerrain(defaultZoom); } });
    }

    private void OnDisable()
    {
        Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, () => { if (scenario.GetCurrentStep().Zoom > 10) { NextTerrain(scenario.GetCurrentStep().Zoom); } else { NextTerrain(defaultZoom); } });
        Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SpatialAnchorloaded, () => { if (scenario.GetCurrentStep().Zoom > 10) { NextTerrain(scenario.GetCurrentStep().Zoom); } else { NextTerrain(defaultZoom); } });
    }



    public void ActivateExampleAndLandmark(bool flag)
    {
        GetCurrentLandmarkGameObject().SetActive(flag);
        if (CurrentExampleGameObject != null)
            GetCurrentExampleGameObject().SetActive(flag);

    }


    // Start is called before the first frame update
    void Start()
    {
        componentManager = FindAnyObjectByType<ComponentManager>();
        inputController = componentManager.GetInputController();
        Terrain = GetComponent<AbstractMap>();
        random = new System.Random();
        index = -1;
        indexForGivenTerrain = 0;
        scenario = componentManager.GetStudyScenario();
        DeactiveContainers();
    }

    public int GetPreviousIndexForLandmark()
    {
        return previousIndexForLandmark;
    }
    private void DeactiveContainers()
    {
        for (int i = 0; i < TerrainContainer.childCount; i++)
        {
            TerrainContainer.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < landmarkContainer.childCount; i++)
        {
            landmarkContainer.GetChild(i).gameObject.SetActive(false);
        }
        for (int i = 0; i < _3DExampleContainer.childCount; i++)
        {
            _3DExampleContainer.GetChild(i).gameObject.SetActive(false);
        }        
        for (int i = 0; i < _2DExampleContainer.childCount; i++)
        {
            _2DExampleContainer.GetChild(i).gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (transform.localPosition != Vector3.zero)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        //if (isGenerateMeshSuceesss)
        //{
        //    isGenerateMeshSuceesss = false;
        //    UnityTile[] allTiles = Terrain.GetComponentsInChildren<UnityTile>();
        //    //CommitMesh(allTiles, TerrainContainer);
        //    StartCoroutine(CommitMeshIEnumerable(allTiles, TerrainContainer));
        //}
    }

    private (bool, UnityTile[], float) IsTerrainValidate(bool notCheck = true)
    {
        UnityTile[] allTiles;
        float lowestH;
        bool flag;
        allTiles = Terrain.GetComponentsInChildren<UnityTile>();
        lowestH = float.MaxValue;
        float highestH = float.MinValue;

        foreach (var tile in allTiles)
        {
            for (float i = 0f; i <= 1f; i += 0.1f)
            {
                for (float j = 0f; j <= 1f; j += 0.1f)
                {
                    float currentH = tile.QueryHeightDataNonclamped(i, j);
                    if (lowestH > currentH)
                        lowestH = currentH;
                    if (highestH < currentH)
                        highestH = currentH;
                }
            }
        }
        flag = (highestH - lowestH) >= 0.3 * SketchBox.transform.localScale.y;
        if (flag || !notCheck)
        {
            //Debug.LogError("TerrainValidate" + (highestH - lowestH));
            TerrainGenerateSuccess(allTiles, lowestH);
        }
        return (flag, allTiles, lowestH);
    }

    public float getHeightDiff()
    {
        return HeightDiff;
    }
    private void TerrainGenerateSuccess(UnityTile[] allTiles, float lowestH)
    {
        //transform.position = new Vector3(0, HeightMapExport.heightDrift - lowestH, 0);
        HeightDiff = HeightMapExport.heightDrift - lowestH + 0.001f;
        foreach (var tile in allTiles)
        {
            tile.transform.localPosition = new Vector3(
                                            tile.transform.localPosition.x,
                                            HeightDiff,
                                            tile.transform.localPosition.z);
            //var MeshCollider = tile.transform.gameObject.AddComponent<MeshCollider>();
            //MeshCollider.convex = false;
        }

        //foreach (var tile in allTiles)
        //{
        //    //tile.transform.localPosition = new Vector3(
        //    //                                tile.transform.localPosition.x,
        //    //                                HeightMapExport.heightDrift - lowestH,
        //    //                                tile.transform.localPosition.z);
        //    var MeshCollider = tile.transform.gameObject.AddComponent<MeshCollider>();
        //    MeshCollider.convex = false;
        //}

        isGenerateMeshSuceesss = true;
    }


    void GenerateTerrainCombinedMeshAsyncCallback(GameObject gameObject, UnityTile[] allTiles)
    {
        CurrentTerrainGameObject = gameObject;
        if (MeshRenderer_list.Count == 0)
            MeshRenderer_list.Add(CurrentTerrainGameObject.GetComponent<MeshRenderer>());
        else
        {
            DisableMeshRendererByIndex(index - 1);
        }
        foreach (var tile in allTiles)
        {
            MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

        }
    }
    public static T ChooseRandom<T>(params T[] items)
    {
        if (items == null || items.Length == 0)
        {
            throw new ArgumentException("At least one item must be provided");
        }

        int randomIndex = random.Next(0, items.Length);
        return items[randomIndex];
    }
    /// <summary>
    /// </summary>
    /// <returns></returns>
    public Vector2d RandomCoordinatesGenerator()
    {
        // Switzerland's approximate latitude and longitude bounds
        random.NextDouble();

        Tuple<float, float, float, float>[] Lat_Lon_pool = new Tuple<float, float, float, float>[16];
        // African
        Tuple<float, float, float, float> Lat_lon_Abderes = new Tuple<float, float, float, float>(0.0f, 1.0f, 36.0f, 38.0f);
        Tuple<float, float, float, float> Lat_lon_Atlas = new Tuple<float, float, float, float>(30.0f, 37.0f, -10.0f, 0.0f);
        Tuple<float, float, float, float> Lat_lon_Rwenzori = new Tuple<float, float, float, float>(-1.0f, 1.0f, 29.0f, 30.0f);
        Tuple<float, float, float, float> Lat_lon_Virunga = new Tuple<float, float, float, float>(-2.0f, 1.0f, 28.5f, 30.0f);

        // Oceania
        Tuple<float, float, float, float> Lat_lon_Barisan = new Tuple<float, float, float, float>(-6.0f, -3.0f, 101.0f, 104.0f);
        Tuple<float, float, float, float> Lat_lon_GreatDividingRange = new Tuple<float, float, float, float>(-37.0f, -27.0f, 142.0f, 152.0f);
        Tuple<float, float, float, float> Lat_lon_Bismarck = new Tuple<float, float, float, float>(-6.5f, -4.5f, 144.0f, 146.0f);

        // Asian
        Tuple<float, float, float, float> Lat_lon_Caucasus = new Tuple<float, float, float, float>(40.0f, 45.0f, 42.0f, 47.0f);
        Tuple<float, float, float, float> Lat_lon_Himalayas = new Tuple<float, float, float, float>(27.0f, 36.0f, 78.0f, 88.0f);
        Tuple<float, float, float, float> Lat_lon_Kunlun = new Tuple<float, float, float, float>(35.0f, 40.0f, 74.0f, 84.0f);
        Tuple<float, float, float, float> Lat_lon_Zagros = new Tuple<float, float, float, float>(30.0f, 37.0f, 45.0f, 50.0f);

        // European
        Tuple<float, float, float, float> Lat_lon_Switerland = new Tuple<float, float, float, float>(45.818f, 47.808f, 5.956f, 10.492f);
        Tuple<float, float, float, float> Lat_lon_Scandinavian = new Tuple<float, float, float, float>(60.0f, 70.0f, 5.0f, 20.0f);
        Tuple<float, float, float, float> Lat_lon_Alps = new Tuple<float, float, float, float>(45.0f, 48.0f, 7.0f, 12.0f);
        Tuple<float, float, float, float> Lat_lon_Pyrenees = new Tuple<float, float, float, float>(42.0f, 44.0f, -1.0f, 3.0f);

        // American
        Tuple<float, float, float, float> Lat_lon_Andes = new Tuple<float, float, float, float>(-35.0f, 0.0f, -80.0f, -60.0f);

        // Add all tuples to the array
        Lat_Lon_pool[0] = Lat_lon_Abderes;
        Lat_Lon_pool[1] = Lat_lon_Atlas;
        Lat_Lon_pool[2] = Lat_lon_Rwenzori;
        Lat_Lon_pool[3] = Lat_lon_Virunga;
        Lat_Lon_pool[4] = Lat_lon_Barisan;
        Lat_Lon_pool[5] = Lat_lon_GreatDividingRange;
        Lat_Lon_pool[6] = Lat_lon_Bismarck;
        Lat_Lon_pool[7] = Lat_lon_Caucasus;
        Lat_Lon_pool[8] = Lat_lon_Himalayas;
        Lat_Lon_pool[9] = Lat_lon_Kunlun;
        Lat_Lon_pool[10] = Lat_lon_Zagros;

        Lat_Lon_pool[11] = Lat_lon_Scandinavian;
        Lat_Lon_pool[12] = Lat_lon_Alps;
        Lat_Lon_pool[13] = Lat_lon_Pyrenees;
        Lat_Lon_pool[14] = Lat_lon_Andes;
        Lat_Lon_pool[15] = Lat_lon_Switerland;
        Tuple<float, float, float, float>[] Lat_Lon_pool2 = new Tuple<float, float, float, float>[1];
        Lat_Lon_pool2[0] = Lat_lon_Switerland;
        //Tuple<float, float, float, float> random_Lat_lon = ChooseRandom(Lat_Lon_pool);
        Tuple<float, float, float, float> random_Lat_lon = ChooseRandom(Lat_Lon_pool2);


        float minLatitude = random_Lat_lon.Item1;
        float maxLatitude = random_Lat_lon.Item2;
        float minLongitude = random_Lat_lon.Item3;
        float maxLongitude = random_Lat_lon.Item4;

        float latitude = minLatitude + (float)random.NextDouble() * (maxLatitude - minLatitude);
        float longitude = minLongitude + (float)random.NextDouble() * (maxLongitude - minLongitude);

        return new Vector2d(latitude, longitude);
    }
    public async void NextTerrain(float zoom)
    {
        if (inputController.GetMode() == InteractionMode.Tutorial)
        {
            WorkspaceManager.Instance.EnableUpperBody(false);
            WorkspaceManager.Instance.EnableLowerBody(true);
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
            {
                LoadGivenTerrain(scenario.GetCurrentStep(), scenario.GetCurrentStep().Zoom);
            }
            else
            {
                await Task.WhenAll(InitializeTerrain(new Vector2d(scenario.GetCurrentStep().terrainCoord.x, scenario.GetCurrentStep().terrainCoord.y), zoom), checkTileState(Terrain.GetComponentsInChildren<UnityTile>()));
                IsTerrainValidate(false);
                UpdateExampleAndLandmark();
            }
            return;
        }
        // if in Tutotial interaction mode, then show an consist tutorial terrain
        if (inputController.GetMode() != InteractionMode.FreeCreation)
        {
            return;
        }
        else
        {
            await Task.WhenAll(GenerateNewTerrain(zoom, useGivenCoord));
        }

    }

    private void UpdateExampleAndLandmark()
    {
        UpdateLandmark(scenario.GetCurrentStep());

        indexForExample++;
        if (ApplicationSettings.Instance.ExperimentCondition == ExperimentCondition._2D)
        {
            Update2DExample(scenario.GetCurrentStep());
        }
        else
        {
            Update3DExample(scenario.GetCurrentStep());
        }
    }

    public async void LoadGivenTerrain(StudyStep CurrentStep, float zoom)
    {
        indexForGivenTerrain += 1;
        if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation)
        {
            // The first is the tutorial
            if (indexForGivenTerrain == 1)
            {
                previousIndexForGivenTerrain = 0;
                CurrentTerrainGameObject = TerrainContainer.GetChild(previousIndexForGivenTerrain).gameObject;
                CurrentTerrainGameObject.SetActive(true);                
            }
            else
            {
                int index = CurrentStep.terrainSequence[indexForGivenTerrain - 2];
                CurrentTerrainGameObject.SetActive(false);
                previousIndexForGivenTerrain = index;
                if (selfDeterminedScene > 0)
                    previousIndexForGivenTerrain = selfDeterminedScene;
                CurrentTerrainGameObject = TerrainContainer.GetChild(previousIndexForGivenTerrain).gameObject;
                CurrentTerrainGameObject.SetActive(true);
            }
        }
        else
        {
            Vector2 coords = CurrentStep.terrainCoord;
            await Task.WhenAll(InitializeTerrain(new Vector2d(coords.x, coords.y), zoom), checkTileState(Terrain.GetComponentsInChildren<UnityTile>()));
            IsTerrainValidate(false);
        }

        UpdateExampleAndLandmark();
    }

    private void UpdateLandmark(StudyStep CurrentStep)
    {
        indexForLandmark++;
        if (indexForLandmark == 1)
        {
            previousIndexForLandmark = 0;
            CurrentLandmarkGameObject = landmarkContainer.GetChild(previousIndexForLandmark).gameObject;
            CurrentLandmarkGameObject.SetActive(true);
        }
        else
        {
            int index = CurrentStep.terrainSequence[indexForLandmark - 2];
            CurrentLandmarkGameObject.SetActive(false);
            previousIndexForLandmark = index;

            if (selfDeterminedScene > 0)
                previousIndexForGivenTerrain = selfDeterminedScene;

            CurrentLandmarkGameObject = landmarkContainer.GetChild(previousIndexForLandmark).gameObject;
            CurrentLandmarkGameObject.SetActive(true);
        }
    }

    private void Update3DExample(StudyStep CurrentStep)
    {
        if (indexForExample == 1)
        {
            previousIndexForExample = 0;
            CurrentExampleGameObject = _3DExampleContainer.GetChild(previousIndexForExample).gameObject;
            CurrentExampleGameObject.SetActive(true);
        }
        else
        {
            if (CurrentExampleGameObject!=null)
                CurrentExampleGameObject.SetActive(false);
            CurrentExampleGameObject = null;
            //int index = CurrentStep.terrainSequence[indexForLandmark - 2];
            //CurrentExampleGameObject.SetActive(false);
            //previousIndexForExample = index;
            //CurrentExampleGameObject = _3DExampleContainer.GetChild(previousIndexForExample).gameObject;
            //CurrentExampleGameObject.SetActive(true);
        }
    }

    private void Update2DExample(StudyStep CurrentStep)
    {
        if (indexForExample == 1)
        {
            previousIndexForExample = 0;
            CurrentExampleGameObject = _2DExampleContainer.GetChild(previousIndexForExample).gameObject;
            CurrentExampleGameObject.SetActive(true);
        }
        else
        {
            if (CurrentExampleGameObject != null)
                CurrentExampleGameObject.SetActive(false);
            CurrentExampleGameObject = null;
            //int index = CurrentStep.terrainSequence[indexForLandmark - 2];
            //CurrentExampleGameObject.SetActive(false);
            //previousIndexForExample = index;
            //CurrentExampleGameObject = _2DExampleContainer.GetChild(previousIndexForExample).gameObject;
            //CurrentExampleGameObject.SetActive(true);
        }
    }

    public async void PrevioustTerrain()
    {
        // if in Tutotial interaction mode, then show an consist tutorial terrain
        if (inputController.GetMode() != InteractionMode.FreeCreation)
        {
            return;
        }

        if (index == 0)
        {
            return;
        }

        if (index > 0)
        {
            //DisableMeshRendererByIndex(index);
            //index -= 1;
            //UpdateAttributeByIndex(index);

            index -= 1;
            await Task.WhenAll(LoadOldTerrain(Lat_lon_list[index], Zoom_list[index]));

            inputController.UpdateInstructions();
        }
    }
    private async Task InitializeTerrain(Vector2d Coord, float zoom)
    {
        Current_Lat_lon = Coord;
        Terrain.Initialize(Coord, (int)zoom);
        await Task.Yield();
        inputController.UpdateInstructions();


    }
    private async Task LoadOldTerrain(Vector2d Coord, float zoom)
    {
        Current_Lat_lon = Coord;
        Terrain.UpdateMap(Coord, zoom);
        await Task.Yield();
        inputController.UpdateInstructions();
    }



    private async Task checkTileState(UnityTile[] allTiles)
    {
        bool flag = true;
        int count = 0;
        do
        {
            count++;
            flag = true;
            foreach (var tile in allTiles)
            {
                if (tile.RasterDataState != TilePropertyState.Loaded)
                {
                    flag = false;
                    break;
                }
            }
            await Task.Yield();
        } while (count > 200 || flag == false);
        // Invoke the callback with the combined mesh and texture

        await Task.Yield();
    }

    private async Task GenerateNewTerrain(float zoom, bool IsGivenCoord = false)
    {
        //List<Vector2d> lists = new List<Vector2d>() {
        //    new Vector2d(37.8235 ,-109.4639 ),  new Vector2d(37.8417,-109.4876), new Vector2d(38.0078,-109.6082),  new Vector2d(38.0158,-109.6205),new Vector2d(38.061,-109.6506),
        //    new Vector2d(38.0786, -109.7056), new Vector2d(38.088, -109.7098),  new Vector2d(38.1933,-109.8013),  new Vector2d(38.1987,-109.804),
        //    new Vector2d(37.7095,-110.185),  new Vector2d(37.7222, -110.162),  new Vector2d(37.7555, -110.1485 ),new Vector2d(37.8072, -110.1433),  new Vector2d(37.8487, -110.1253),  new Vector2d(37.967, -110.01),
        //    new Vector2d(38.0414, -109.9535),  new Vector2d(38.1948, -109.8246),  new Vector2d(38.2278, -109.823),new Vector2d(38.6319,-110.0454),
        //    new Vector2d(38.489, -109.9497),  new Vector2d(38.489, -109.9497 ),new Vector2d(38.4647,-109.9367),  new Vector2d(38.3213,-109.8214),  new Vector2d(40.1182,-111.5545),new Vector2d(40.0666,-111.4963),
        //    new Vector2d(40.0335,-111.4006),  new Vector2d(40.0165,-111.311),new Vector2d(39.9247,-111.3071),  new Vector2d(39.8891,-111.2164),
        //    new Vector2d(39.7963,-111.2085),new Vector2d(39.6192,-110.9009 ),new Vector2d(38.4211,-110.024),new Vector2d(38.3448,-110.0178),
        //    new Vector2d(38.324,-109.9358),new Vector2d(38.3075,-109.9162),new Vector2d(38.761,-109.5214),new Vector2d(38.5651,-109.5896),new Vector2d(38.4518,-109.7154),new Vector2d(38.4286,-109.5619 )
        //};        
        
        List<Vector2d> lists = new List<Vector2d>() {
            new Vector2d(46.2330324523126, 7.87139176567208)
            //new Vector2d(37.7095, -110.1859)
        };

  
        if (index == Lat_lon_list.Count - 1)
        {
            Vector2d randomCoord = Vector2d.zero;
            if (IsGivenCoord)
            {
                //givenCoord = new Vector2d(givenCoord.x + 0.02 * random.NextDouble(), givenCoord.y + 0.02 * random.NextDouble());
                zoom = 13;
                //zoom = zoom_lists[statredIndex];
                givenCoord = lists[statredIndex];
                Terrain.Initialize(givenCoord, (int)zoom);
                await Task.WhenAll(checkTileState(Terrain.GetComponentsInChildren<UnityTile>()));
                IsTerrainValidate(false);
                randomCoord = givenCoord;
                statredIndex ++;
            }
            else
            {
                randomCoord = await GenerateRandomTerrain(zoom);
            }

            Lat_lon_list.Add(randomCoord);
            Zoom_list.Add((int)zoom);

            // disable the previous terrain mesh

            index += 1;

            Current_Lat_lon = randomCoord;
            inputController.UpdateInstructions();
        }
        else
        {
            //DisableMeshRendererByIndex(index);
            //index += 1;
            //UpdateAttributeByIndex(index);
            //inputController.UpdateInstructions();

            index += 1;
            await Task.WhenAll(LoadOldTerrain(Lat_lon_list[index], zoom));
            await Task.Yield();
        }
        //SetMaterialsToFade(transform);
        await Task.Yield();
    }

    private async Task<Vector2d> GenerateRandomTerrain(float zoom)
    {
        int count = 0;
        bool flag = false;
        Vector2d randomCoord = new Vector2d();
        do
        {
            flag = false;
            randomCoord = RandomCoordinatesGenerator();
            Terrain.Initialize(randomCoord, (int)zoom);
            //await Task.Yield();
            count++;

            await Task.WhenAll(checkTileState(Terrain.GetComponentsInChildren<UnityTile>()));
            flag = IsTerrainValidate().Item1;

        }
        while (flag == false && count < 1);
        if (!flag)
        {
            randomCoord = RandomCoordinatesGenerator();
            Terrain.Initialize(randomCoord, (int)zoom);
            await Task.WhenAll(checkTileState(Terrain.GetComponentsInChildren<UnityTile>()));
            IsTerrainValidate(false);
        }

        return randomCoord;
    }

    private void DisableMeshRendererByIndex(int index)
    {
        MeshRenderer_list[index].enabled = false;
    }


    public string SetInstuction(int index, double x, double y)
    {
        return string.Format("Terrain No.{0}:\nlat:{1,8:N}\tlon:{2,8:N}", index + 1, x, y);
    }
    private void UpdateAttributeByIndex(int index)
    {
        MeshRenderer_list[index].enabled = true;
        CurrentTerrainGameObject = MeshRenderer_list[index].gameObject;
        Current_Lat_lon = Lat_lon_list[index];
    }
    public string SetInstuction(string name, int index, double x, double y)
    {
        return string.Format("Terrain No.{0}:\n{1}\nlat:{2,8:N}\tlon:{3,8:N}", index + 1, name, x, y);
    }

    public string SetInstuction(string name, double x, double y)
    {
        return string.Format("Terrain {0}:\nlat:{1,8:N}\tlon:{2,8:N}", name, x, y);
    }


    IEnumerator CommitMeshIEnumerable(UnityTile[] allTiles, Transform parent)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 3)
        {
            elapsedTime += Time.deltaTime;
            yield return null; // Wait until the next frame
        }
        CurrentTerrainGameObject = ObjExporterScript.GenerateTerrainCombinedMesh(allTiles, index);
        if (MeshRenderer_list.Count == 0)
            MeshRenderer_list.Add(CurrentTerrainGameObject.GetComponent<MeshRenderer>());
        else
        {
            DisableMeshRendererByIndex(index - 1);
        }
        foreach (var tile in allTiles)
        {
            MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;

        }
        yield return null;

    }

    private async void CommitMesh(UnityTile[] allTiles, Transform parent)
    {
        await Task.WhenAll(ObjExporterScript.GenerateTerrainCombinedMeshAsync(allTiles, parent, index, GenerateTerrainCombinedMeshAsyncCallback));

    }

    #region Get Attributes

    public int GetPreviousIndexForGivenTerrain()
    {
        return previousIndexForGivenTerrain;
    }

    public GameObject GetCurrentTerrainGameObject()
    {
        return CurrentTerrainGameObject;
    }
    public GameObject GetCurrentLandmarkGameObject()
    {
        return CurrentLandmarkGameObject;
    }
    public GameObject GetCurrentExampleGameObject()
    {
        return CurrentExampleGameObject;
    }

    public Vector2d GetCurrent_Lat_lon()
    {
        return Current_Lat_lon;
    }
    public int GetIndex()
    {
        return index;
    }

    public int GetIndexForGivenTerrain()
    {
        return indexForGivenTerrain;
    }
    #endregion
}

