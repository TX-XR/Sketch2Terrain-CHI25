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
    private Vector2d givenCoord = new Vector2d(38.23, -109.82);

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
            //new Vector2d(60.4963,7.0497),
            //new Vector2d(60.4694,7.0941),
            //new Vector2d(60.4575,7.0953),
            //new Vector2d(60.4323,7.1071),
            //new Vector2d(60.4032,7.135),
            //new Vector2d(60.5537,7.0243),
            //new Vector2d(60.6447,7.0662),
            //new Vector2d(60.755,7.1308)
            new Vector2d(37.7095, -110.1859)
        };

        //List<Vector2d> lists = new List<Vector2d>() {
        //new Vector2d(46.1467,10.735),
        //new Vector2d(62.4038,7.9417),
        //new Vector2d(39.0905,74.2088),
        //new Vector2d(45.9516,9.846),
        //new Vector2d(42.9243,46.7007),
        //new Vector2d(37.0475,75.9241),
        //new Vector2d(46.9472,10.9904),
        //new Vector2d(27.5341,86.6405),
        //new Vector2d(46.0083,8.606),
        //new Vector2d(42.2957,2.5014),
        //new Vector2d(46.7744,9.6483),
        //new Vector2d(32.5162,49.1896),
        //new Vector2d(68.0349,18.5624),
        //new Vector2d(45.2474,7.2788),
        //new Vector2d(37.5788,75.8194),
        //new Vector2d(-5.751,145.1774),
        //new Vector2d(32.1321,49.8396),
        //new Vector2d(0.4813,29.9182),
        //new Vector2d(46.112,10.5484),
        //new Vector2d(0.3591,29.8174),
        //new Vector2d(37.2063,75.354),
        //new Vector2d(42.4181,1.4343),
        //new Vector2d(47.3159,10.8006),
        //new Vector2d(66.7875,14.4248),
        //new Vector2d(36.7693,79.1794),
        //new Vector2d(32.6594,49.6399),
        //new Vector2d(42.5738,0.187),
        //new Vector2d(42.9988,43.9545),
        //new Vector2d(41.626,42.04),
        //new Vector2d(45.7396,10.1664),
        //new Vector2d(0.34,29.8405),
        //new Vector2d(-4.982,144.1751),
        //new Vector2d(47.4328,10.917),
        //new Vector2d(46.2906,8.0402),
        //new Vector2d(47.0552,10.9644),
        //new Vector2d(-4.3883,103.3547),
        //new Vector2d(62.449,9.4585),
        //new Vector2d(-11.2884,-75.2398),
        //new Vector2d(46.7233,10.0614),
        //new Vector2d(-13.2224,-73.7576),
        //new Vector2d(43.0114,43.43),
        //new Vector2d(45.9341,10.9804),
        //new Vector2d(42.3934,44.7293),
        //new Vector2d(47.1085,10.0019),
        //new Vector2d(42.7469,1.5169),
        //new Vector2d(35.807,79.7207),
        //new Vector2d(42.6057,46.0714),
        //new Vector2d(46.7156,10.9383),
        //new Vector2d(-5.2361,144.6038),
        //new Vector2d(38.8811,75.0729),
        //new Vector2d(47.155,10.7722),
        //new Vector2d(39.7151,74.4357),
        //new Vector2d(0.5156,29.866),
        //new Vector2d(32.7908,47.962),
        //new Vector2d(-24.3703,-70.5446),
        //new Vector2d(-0.9598,29.178),
        //new Vector2d(46.2285,7.3426),
        //new Vector2d(47.7998,7.0482),
        //new Vector2d(-28.797,-66.7619),
        //new Vector2d(45.843,11.3996),
        //new Vector2d(36.7052,-5.0841),
        //new Vector2d(41.8312,44.5286),
        //new Vector2d(34.3507,47.2405),
        //new Vector2d(40.0718,46.0509),
        //new Vector2d(65.2912,12.7421),
        //new Vector2d(32.3249,87.0659),
        //new Vector2d(-1.6789,28.9296),
        //new Vector2d(27.7253,86.7413),
        //new Vector2d(45.7213,8.0841),
        //new Vector2d(47.6209,11.6231),
        //new Vector2d(43.5369,2.8208),
        //new Vector2d(-0.1806,29.4749),
        //new Vector2d(0.4252,29.9307),
        //new Vector2d(46.1293,8.638),
        //new Vector2d(29.1211,84.1821),
        //new Vector2d(36.3274,82.6558),
        //new Vector2d(35.4046,74.949),
        //new Vector2d(29.0526,81.2189),
        //new Vector2d(46.7153,10.0591),
        //new Vector2d(32.4535,87.5277),
        //new Vector2d(-1.1686,29.2899),
        //new Vector2d(-1.0355,28.9549),
        //new Vector2d(35.9306,81.8934),
        //new Vector2d(40.9562,45.0372),
        //new Vector2d(-6.1014,144.38),
        //new Vector2d(42.6409,43.4087),
        //new Vector2d(35.4041,75.1588),
        //new Vector2d(28.4463,83.8799),
        //new Vector2d(45.9339,8.3672),
        //new Vector2d(-1.6584,28.7716),
        //new Vector2d(29.7144,82.987),
        //new Vector2d(42.56,-0.1511),
        //new Vector2d(41.9343,42.6993),
        //new Vector2d(-6.125,144.9947),
        //new Vector2d(27.3568,85.6532),
        //new Vector2d(29.7634,82.0069),
        //new Vector2d(32.9006,49.6114),
        //new Vector2d(42.3469,44.918),
        //new Vector2d(36.7202,79.6215),
        //new Vector2d(47.044,9.8798),
        //new Vector2d(46.2803,5.9897),
        //new Vector2d(45.9905,9.2999),
        //new Vector2d(45.8978,7.3187),
        //new Vector2d(46.1266,9.0087),
        //new Vector2d(45.9829,6.5335),
        //new Vector2d(47.3771,10.3007),
        //new Vector2d(46.4996,8.2046),
        //new Vector2d(46.0599,6.9105),
        //new Vector2d(47.1831,9.9693),
        //new Vector2d(45.832,7.9879),
        //new Vector2d(47.5455,10.3202),
        //new Vector2d(46.5159,7.0793),
        //new Vector2d(47.0809,9.6061),
        //new Vector2d(47.3255,10.4909),
        //new Vector2d(46.8142,9.0048),
        //new Vector2d(47.1783,9.5221),
        //new Vector2d(42.6164,45.4523),
        //new Vector2d(42.2195,45.6104),
        //new Vector2d(46.3469,8.8771),
        //new Vector2d(40.2465,46.3556),
        //new Vector2d(-6.7087,-78.1973),
        //new Vector2d(42.7945,1.1811),
        //new Vector2d(42.649,43.642),
        //new Vector2d(32.739,48.9251),
        //new Vector2d(36.3625,82.7152),
        //new Vector2d(42.6517,45.4789),
        //new Vector2d(28.0698,86.1026),
        //new Vector2d(42.2564,44.1606),
        //new Vector2d(29.625,82.6777),
        //new Vector2d(46.0168,9.3556),
        //new Vector2d(46.0466,9.8242),
        //new Vector2d(0.421,29.8564),
        //new Vector2d(46.7076,8.9514),
        //new Vector2d(42.6131,45.1523),
        //new Vector2d(42.4362,2.28),
        //new Vector2d(-5.6682,144.657),
        //new Vector2d(46.294,8.2139),
        //new Vector2d(46.3433,7.3581),
        //new Vector2d(47.2274,11.3265),
        //new Vector2d(38.1991,74.5178),
        //new Vector2d(47.0845,10.2701),
        //new Vector2d(36.2136,74.7139),
        //new Vector2d(0.3656,29.7931),
        //new Vector2d(28.9148,81.7645),
        //new Vector2d(42.2283,1.4609),
        //new Vector2d(36.8478,76.0588),
        //new Vector2d(42.5742,2.7088),
        //new Vector2d(36.5398,49.7545),
        //new Vector2d(46.3835,8.1122),
        //new Vector2d(-0.2184,29.3971),
        //new Vector2d(40.4312,45.7656),
        //new Vector2d(41.6728,42.005),
        //new Vector2d(61.6166,6.6919),
        //new Vector2d(47.0578,9.585),
        //new Vector2d(32.7792,79.0967),
        //new Vector2d(0.3045,29.8727),
        //new Vector2d(-5.8294,145.2242),
        //new Vector2d(42.4289,2.2473),
        //new Vector2d(46.4657,11.627),
        //new Vector2d(36.6274,79.9229),
        //new Vector2d(68.604,16.4168),
        //new Vector2d(47.255,10.7892),
        //new Vector2d(46.0519,11.5137),
        //new Vector2d(30.5389,-7.814),
        //new Vector2d(45.8553,7.3933),
        //new Vector2d(36.5356,77.5709),
        //new Vector2d(42.3993,45.6475),
        //new Vector2d(27.5827,85.965),
        //new Vector2d(46.6675,8.6555),
        //new Vector2d(36.4205,79.582),
        //new Vector2d(42.3042,1.7717),
        //new Vector2d(28.1834,83.8052),
        //new Vector2d(36.576,79.0753),
        //new Vector2d(42.8772,46.0578),
        ////new Vector2d(28.2652,84.898),
        //new Vector2d(-33.1642,-69.6529),
        //new Vector2d(46.5155,10.5255),
        //new Vector2d(42.7546,-0.192),
        //new Vector2d(36.6828,75.6043),
        //new Vector2d(33.2147,47.9959),
        //new Vector2d(46.9484,11.085),
        //new Vector2d(43.3292,43.2225),
        //new Vector2d(33.2023,49.2272),
        //new Vector2d(-9.1086,-76.9728),
        //new Vector2d(37.9866,75.0206),
        //new Vector2d(42.5313,0.2386),
        //new Vector2d(-5.9296,145.45),
        //new Vector2d(35.2558,75.3829),
        //new Vector2d(47.0668,11.9711),
        //new Vector2d(46.6545,7.2789),
        //new Vector2d(42.7385,42.6307),
        //new Vector2d(-5.9792,144.5286),
        //new Vector2d(-5.8327,145.9998),
        //new Vector2d(47.1544,9.0699),
        //new Vector2d(35.4176,74.8709),
        //new Vector2d(67.9336,15.5388),
        //new Vector2d(36.8306,74.0621),
        //new Vector2d(-0.0494,29.488),
        //new Vector2d(36.8178,76.233),
        //new Vector2d(41.5756,42.1948),
        //new Vector2d(42.2781,1.6345),
        //new Vector2d(36.2863,76.5912),
        //new Vector2d(30.2329,81.9233),
        //new Vector2d(30.4613,80.2183),
        //new Vector2d(-5.0639,144.5264),
        //new Vector2d(37.1856,76.3312),
        //new Vector2d(36.4153,83.0611),
        //};

        //List<int> zoom_lists = new List<int>() {
        //13,
        //13,
        //13,
        //14,
        //13,
        //13,
        //13,
        //13,
        //13,
        //15,
        //13,
        //14,
        //14,
        //14,
        //15,
        //13,
        //14,
        //15,
        //14,
        //14,
        //14,
        //15,
        //14,
        //15,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //        14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //        14,


        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,
        //14,

        //};

        if (index == Lat_lon_list.Count - 1)
        {
            Vector2d randomCoord = Vector2d.zero;
            if (IsGivenCoord)
            {
                //givenCoord = new Vector2d(givenCoord.x + 0.02 * random.NextDouble(), givenCoord.y + 0.02 * random.NextDouble());
                zoom = 15;
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

