using Unity.Barracuda;
using UnityEngine;
using System.Linq;
using System;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;


namespace MappingAI
{
    public struct Prediction
    {
        public float[] predicted;
        public void SetPrediction(Tensor t)
        {
            predicted = t.AsFloats();
        }
    }
    public enum FilterType
    {
        Laplacian, HC
    };
    public enum GradientTheme
    {
        SnowyMountains,
        EarthyMountains,
        ForestedMountains,
        SelfDefined
    }

    public class AIModelManager : MonoBehaviour
    {
        protected ParametersManager parameters = null;
        [SerializeField]
        protected NNModel modelAsset;
        [SerializeField]
        protected Gradient heightmapGradient;
        // Smooth the terrain mesh after generated
        [SerializeField] FilterType smoothType = FilterType.Laplacian;
        [SerializeField] int smoothTimes = 5;

        protected IWorker _engine;
        protected Prediction prediction;
        [SerializeField]
        protected string inputFilePath; // Path to the TXT file
        protected float[] predicted_result;
        protected float[] input_public;
        protected Texture2D inputDataTexture;
        protected static float[,] rescaled_predicted_heightmap;
        protected Model _runtimeModel;
        [SerializeField]
        protected GradientTheme gradientTheme = GradientTheme.SelfDefined;
        [SerializeField]
        protected GameObject AIGeneratedModelContainer;
        [SerializeField]
        protected GameObject StrokeModelContainer;
        protected MeshRenderer AIGeneratedModelContainerMeshRenderer;
        [SerializeField]
        protected Transform SketchingBoundary;
        protected DrawingCanvas drawingCanvas;
        protected ExportController exportController;
        [SerializeField]
        protected float scaleRatio = 0.5f; //scale down the ratio for the height map, e.g., from 512 * 512 * 1 to 256 * 256 * 1
        protected int inputChunkSize = 512;

        protected bool CanExecuteInference = true;
        protected int previousStrokeNum = 0;
        protected InputController inputController;

        protected bool materialChangedByDraw = false;
        protected Texture2D heightMapTexture;
        protected Texture2D heightmapGradientTexture;

        //AIModelTestManager aIModelTestManager;
        void Start()
        {
            _runtimeModel = ModelLoader.Load(modelAsset);
            // Check if GPU is available and create the appropriate worker
            _engine = SystemInfo.supportsComputeShaders
                ? WorkerFactory.CreateWorker(WorkerFactory.Type.Compute, _runtimeModel)
                : WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, _runtimeModel);

            inputController = ComponentManager.Instance.GetInputController();
            drawingCanvas = ComponentManager.Instance.GetDrawingCanvas();
            exportController = ComponentManager.Instance.GetExportController();
            parameters = ComponentManager.Instance.GetCASSIEParametersProvider();
            prediction = new Prediction();

            AIGeneratedModelContainerMeshRenderer = AIGeneratedModelContainer.GetComponent<MeshRenderer>();
            heightmapGradient = ColorManager.InitializeGradient(gradientTheme);

            //aIModelTestManager = new AIModelTestManager();

            WarmupExecuteInferenceAsync();
        }

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.ExecuteInferenceHot, () => { CanExecuteInference = true; });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.ExecuteInferenceCold, () => { CanExecuteInference = false; });
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.ExecuteInferenceHot, () => { CanExecuteInference = true; });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.ExecuteInferenceCold, () => { CanExecuteInference = false; });
            // Release the resources
            _engine?.Dispose();
        }


        private void LateUpdate()
        {
            //if (Input.GetKeyDown(KeyCode.Space)) //  && CanExecuteInference
            //{
            //    aIModelTestManager.ExecuteInferenceAsyncTest();
            //}
            UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            bool isDrawingOrErasing = inputController.Draw() || inputController.Erase();

            if (isDrawingOrErasing != materialChangedByDraw)
            {
                materialChangedByDraw = isDrawingOrErasing;
                //var newMaterial = materialManager.GetReliefShading();

                if (MaterialManager.Instance.GetColorProterty() == ColorProperty.Terrain && isDrawingOrErasing)
                {
                    //newMaterial = materialManager.GetAITerrainMaterialDuringDraw();
                    CanAIModelRender();
                }

                //AIGeneratedModelContainerMeshRenderer.material = newMaterial;
            }
        }
        public async void WarmupExecuteInferenceAsync()
        {
            await AsyncWarmupExecuteInferenceTask();
        }
        private async Task AsyncWarmupExecuteInferenceTask()
        {
            if (ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._AI)
                return;
            float[] inputData = new float[inputChunkSize * inputChunkSize];
            await RunInference(inputData);
            await Task.Yield();
        }
        public async void ExecuteInferenceAsync(bool withoutCheck = false)
        {
            await Task.WhenAll(AsyncExecuteInferenceTask(withoutCheck));
        }

        public void CanAIModelRender(bool flag = false)
        {
            AIGeneratedModelContainerMeshRenderer.enabled = flag;
        }

        private async Task AsyncExecuteInferenceTask(bool withoutCheck = false)
        {
            if (ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._AI)
                return;

            float[] inputData = new float[inputChunkSize * inputChunkSize];

            if (drawingCanvas.TerrainStrokes.Count == 0)
            {
                previousStrokeNum = 0;
                ResetAIModelMesh();
                return;
            }
            else
            {
                if (!withoutCheck)
                {
                    if (previousStrokeNum == drawingCanvas.TerrainStrokes.Count || !CanExecuteInference)
                        return;
                }
                previousStrokeNum = drawingCanvas.TerrainStrokes.Count;

                float[] inputDataFrequency = new float[inputChunkSize * inputChunkSize];
                StringBuilder LerpCurves = new StringBuilder();
                foreach (FinalStroke s in drawingCanvas.TerrainStrokes)
                {
                    // only export terrain strokes, no landmark
                    if (s.GetColorProperty() == ColorProperty.Terrain)
                    {
                        string LerpCurvesString = CurvesExport.CurveToPolylineLerp(s.Curve, parameters.Current.SubdivisionsPerUnit * 5, exportController.GetYBounds().Item1, exportController.GetYBounds().Item2, showHeader: false);
                        LerpCurves.Append(LerpCurvesString);
                    }
                }
                var points = LerpCurves.ToString().Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(parts => new { X = float.Parse(parts[0]), Y = float.Parse(parts[1]), Z = float.Parse(parts[2]) })
                .ToList();

                foreach (var point in points)
                {
                    var offset = (exportController.GetXZBounds().Item2 - exportController.GetXZBounds().Item1);
                    float x = (point.X + 0.5f * offset) / offset * 512;
                    float y = (point.Y + 0.5f * offset) / offset * 512;
                    //float z = point.Z * 255 / 512;
                    float z = point.Z;
                    //float z = point.Z;
                    InsertData1D(x, y, z, ref inputData, ref inputDataFrequency);
                }
            }
            await Task.Yield();
            input_public = inputData;
            await ConductPrediction();
        }

        public async Task ConductPrediction()
        {
            // Example input data (should be populated with actual data)
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            await RunInference(input_public);
            await Task.Yield();
            stopwatch.Stop();
            TimeSpan elapsed = stopwatch.Elapsed;
            UnityEngine.Debug.Log($"Execution Time RunInference: {elapsed.TotalMilliseconds} ms");
            predicted_result = prediction.predicted;
            stopwatch = new Stopwatch();
            stopwatch.Start();
            Mesh mesh;
            (heightMapTexture, heightmapGradientTexture, inputDataTexture, rescaled_predicted_heightmap, mesh) = PostProcess(input_public, predicted_result);
            stopwatch.Stop();
            elapsed = stopwatch.Elapsed;
            UnityEngine.Debug.Log($"PostProcess: {elapsed.TotalMilliseconds} ms");
            await Task.Yield();

            //if (drawingCanvas.Strokes.Count % 10 == 0)
            //    exportController.ExportAIHeightmap();

            CanAIModelRender(true);
            Material m;
            if (inputController.Draw() || inputController.Erase())
            {
                m = MaterialManager.Instance.GetAITerrainMaterialDuringDraw();
                m.SetTexture("_MainTex", heightmapGradientTexture);
                //m.SetTexture("_MainTex", heightMapTexture);
            }
            else
            {
                m = MaterialManager.Instance.GetReliefShading();
                m.SetTexture("_MainTex", heightmapGradientTexture);
                //m.SetTexture("_MainTex", heightMapTexture);
                m.SetTexture("_HeightMap", heightMapTexture);
            }
            await Task.WhenAll(AsyncGenerateTerrainTask(mesh, AIGeneratedModelContainer, rescaled_predicted_heightmap.GetLength(0), rescaled_predicted_heightmap.GetLength(1)));
            AIGeneratedModelContainerMeshRenderer.material = m;
        }

        public void ResetAIModelMesh()
        {
            AIGeneratedModelContainer.GetComponent<MeshFilter>().sharedMesh = null;
            CanAIModelRender();
        }
  
        protected (Texture2D, Texture2D, Texture2D, float[,], Mesh) PostProcess(float[] inputData, float[] predicted_result)
        {
            (var heightMapTexture, var heightmapGradientTexture, var inputDataTexture) = ColorManager.Create1DHeightMapTexture(predicted_result, inputChunkSize, heightmapGradient, inputData);
            (var rescaled_predicted_heightmap, Mesh mesh) = HeightMapProcessor.ScaleHeightMap_GenerateMesh(inputData, predicted_result, inputChunkSize, scaleRatio);
            return (heightMapTexture, heightmapGradientTexture, inputDataTexture, rescaled_predicted_heightmap, mesh);
        }

        protected (Texture2D, Texture2D, Texture2D, float[,], Mesh) PostProcess(float[] predicted_result)
        {
            float[,] heightmap = ReshapeArray(predicted_result);
            (var heightMapTexture, var heightmapGradientTexture, var inputDataTexture) = ColorManager.Create2DHeightMapTexture(heightmap, heightmapGradient, input_public);
            (var rescaled_predicted_heightmap, Mesh mesh) = HeightMapProcessor.ScaleHeightMap_GenerateMesh(input_public, predicted_result, inputChunkSize, scaleRatio);
            return (heightMapTexture, heightmapGradientTexture, inputDataTexture, rescaled_predicted_heightmap, mesh);
        }

        protected async Task AsyncGenerateTerrainTask(float[,] heightMap, GameObject gameObject)
        {
            Mesh mesh = MeshGenerator.GenerateMeshFlip(heightMap, out int width, out int height);
            await Task.Yield();
            if (smoothTimes > 0)
            {
                mesh = MeshGenerator.Smooth(mesh, smoothType, smoothTimes);
                await Task.Yield();
            }
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.transform.localScale = Vector3.one;
            //gameObject.transform.localScale = new Vector3((float)SketchingBoundary.localScale.x / width, SketchingBoundary.localScale.y, (float)SketchingBoundary.localScale.z / height);

            float offset = exportController.GetXZBounds().Item2 - exportController.GetXZBounds().Item1;
            // reset the position and localscale
            gameObject.transform.localPosition = Vector3.zero;
            gameObject.transform.localPosition = new Vector3(0 - 0.5f * offset, 0, 0 - 0.5f * offset); //offset the model into center
        }

        async Task AsyncGenerateTerrainTask(Mesh mesh, GameObject gameObject, int width, int height)
        {
            if (smoothTimes > 0)
            {
                mesh = MeshGenerator.Smooth(mesh, smoothType, smoothTimes);
                await Task.Yield();
            }
            gameObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            gameObject.transform.localScale = new Vector3((float)SketchingBoundary.localScale.x / width, SketchingBoundary.localScale.y, (float)SketchingBoundary.localScale.z / height);

            float offset = exportController.GetXZBounds().Item2 - exportController.GetXZBounds().Item1;
            // reset the position and localscale
            gameObject.transform.localPosition = new Vector3(0 - 0.5f * offset, 0, 0 - 0.5f * offset); //offset the model into center
        }

        protected float[,] ReshapeArray(float[] data1D)
        {
            float[,] data2D = new float[inputChunkSize, inputChunkSize];
            for (int i = 0; i < inputChunkSize; i++)
            {
                for (int j = 0; j < inputChunkSize; j++)
                {
                    data2D[i, j] = data1D[i * inputChunkSize + j];
                }
            }
            return data2D;
        }

        public async Task RunInference(float[] inputData)
        {
            // Create the input tensor
            int[] inputShape = new int[8] { 1, 1, 1, 1, 1, 512, 512, 1 };
            using (var inputTensor = new Tensor(inputShape, inputData))
            {
                // Execute the model
                _engine.Execute(inputTensor);
                await Task.Yield();
                // Get the output tensor
                Tensor outputTensor = _engine.PeekOutput();
                await Task.Yield();
                // Set and return the prediction
                prediction.SetPrediction(outputTensor);

                // Dispose the output tensor
                outputTensor.Dispose();
                inputTensor.Dispose();
            }
        }

       
        // Method to insert (x, y, z) data into the heightMap using Mathf.Clamp
        public void InsertData2D(float x, float y, float z, ref float[,] heightMap)
        {
            // Clamp the x and y coordinates to be within the valid range
            x = Mathf.Clamp(x, 0, 511);
            y = Mathf.Clamp(y, 0, 511);
            heightMap[(int)x, (int)y] = z;
        }
        public void InsertData1D(float x, float y, float z, ref float[] data1D, ref float[] dataFrequency)
        {
            // Clamp the x and y coordinates to be within the valid range
            x = Mathf.Clamp(x, 0, 511);
            y = Mathf.Clamp(y, 0, 511);

            int index = (int)x + (int)y * inputChunkSize;
            if (data1D[index] == 0)
            {
                dataFrequency[index] = 1;
                data1D[index] = z;
            }

            else
            {
                dataFrequency[index] += 1;
                var previous_data = data1D[index];
                if (z >= previous_data)
                {
                    data1D[index] = z;
                }

                //data1D[index] = (previous_data + z) / dataFrequency[index];
            }
        }
        #region Getting Attributes
        public static float[,] Get_Rescaled_predicted_result()
        {
            return rescaled_predicted_heightmap;
        }

        public Texture2D GetHeightMapTexture()
        {
            return heightMapTexture;
        }
        public float[] GetInput_public()
        {
            return input_public;
        }
        public float[] GetPredicted_result()
        {
            return predicted_result;
        }
        public Texture2D GetInputDataTexture()
        {
            return inputDataTexture;
        }
        public Texture2D GetHeightMapGradientTexture()
        {
            return heightmapGradientTexture;
        }
        #endregion
    }
}

