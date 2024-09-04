//using Emgu.CV.CvEnum;
//using Emgu.CV;
//using System;
//using System.IO;
//using UnityEngine;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using UnityEngine.InputSystem.XInput;

namespace MappingAI
{
    //public class AIModelTestManager: AIModelManager
    //{
    //    public async void ExecuteInferenceAsyncTest()
    //    {
    //        //await Task.WhenAll(AsyncExecuteInferenceTaskFromPNG());
    //        await Task.WhenAll(AsyncVisualizeMeshFromPNG(AIGeneratedModelContainer, StrokeModelContainer, "Assets/3DMappingAI/AI Model/1.png", "Assets/3DMappingAI/AI Model/6.png"));
    //    }
    //    private async Task AsyncExecuteInferenceTaskFromPNG()
    //    {
    //        if (ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._3DMappingAI)
    //            return;
    //        string path = "Assets/3DMappingAI/AI Model/1.png";
    //        //input_public = ConvertPNGToGrayscaleArray1D(path);
    //        input_public = AIModelTestManager.LoadPNGTo1DArray(path);

    //        // Example input data (should be populated with actual data)
    //        Stopwatch stopwatch = new Stopwatch();
    //        stopwatch.Start();
    //        await RunInference(input_public);

    //        stopwatch.Stop();
    //        TimeSpan elapsed = stopwatch.Elapsed;
    //        UnityEngine.Debug.Log($"Execution Time RunInference: {elapsed.TotalMilliseconds} ms");
    //        predicted_result = prediction.predicted;
    //        Mesh mesh;
    //        (heightMapTexture, heightmapGradientTexture, inputDataTexture, rescaled_predicted_heightmap, mesh) = PostProcess(input_public, predicted_result);
    //        //(heightMapTexture, heightmapGradientTexture, inputDataTexture, rescaled_predicted_heightmap, mesh) = PostProcess(predicted_result);
    //        await Task.Yield();

    //        exportController.ExportAIHeightmap();

    //        ManageAIGeneratedModelContainerMeshRenderer(true);
    //        Material m;
    //        if (inputController.Draw() || inputController.Erase())
    //        {
    //            m = MaterialManager.Instance.GetAITerrainMaterialDuringDraw();
    //            m.SetTexture("_MainTex", heightmapGradientTexture);
    //        }
    //        else
    //        {
    //            m = MaterialManager.Instance.GetReliefShading();
    //            m.SetTexture("_MainTex", heightmapGradientTexture);
    //            m.SetTexture("_HeightMap", heightMapTexture);
    //            m.SetFloat("_HeightMultiplier", 10f); // Set your height multiplier
    //        }
    //        AIGeneratedModelContainerMeshRenderer.material = m;
    //        //await Task.WhenAll(AsyncGenerateTerrainTask(rescaled_predicted_heightmap, AIGeneratedModelContainer));
    //        //await Task.WhenAll(AsyncGenerateTerrainTask(mesh, AIGeneratedModelContainer, rescaled_predicted_heightmap.GetLength(0), rescaled_predicted_heightmap.GetLength(1)));
    //        await Task.WhenAll(AsyncGenerateTerrainTask(ReshapeArray(input_public), StrokeModelContainer));
    //    }
    //    public async Task AsyncVisualizeMeshFromPNG(GameObject AIGeneratedModelContainer, GameObject StrokeModelContainer, string pathTerrain, string pathSketch)
    //    {
    //        if (ApplicationSettings.Instance.ExperimentCondition != ExperimentCondition._3DMappingAI)
    //            return;


    //        await GenerateTestMesh(pathTerrain, AIGeneratedModelContainer);
    //        await GenerateTestMesh(pathSketch, StrokeModelContainer);

    //    }
    //    private async Task GenerateTestMesh(string path, GameObject gameObject)
    //    {

    //        //input_public = ConvertPNGToGrayscaleArray1D(path);
    //        var input_public = LoadPNGTo1DArray(path);
    //        await Task.WhenAll(AsyncGenerateTerrainTask(ReshapeArray(input_public), gameObject));
    //    }
    //    public static float[] LoadPNGTo1DArray(string path)
    //    {
    //        if (!File.Exists(path))
    //        {
    //            return null;
    //        }

    //        // Load the image using Emgu CV
    //        Mat image = CvInvoke.Imread(path, ImreadModes.Grayscale);

    //        if (image == null || image.IsEmpty)
    //        {
    //            return null;
    //        }

    //        // Convert Mat to 1D float array
    //        return MatTo1DArray(image);
    //    }

    //    public static float[] MatTo1DArray(Mat mat)
    //    {
    //        int width = mat.Width;
    //        int height = mat.Height;
    //        int size = width * height;

    //        // Create a 1D array to hold the pixel values
    //        float[] result = new float[size];

    //        // Get the image data
    //        byte[] imageData = new byte[size];
    //        mat.CopyTo(imageData);

    //        // Convert the byte values to float values and store them in the array
    //        for (int i = 0; i < size; i++)
    //        {
    //            result[i] = imageData[i] / 255f; // Normalize the pixel values to [0, 1]
    //        }

    //        return result;
    //    }
    //    public static float[] ConvertPNGToGrayscaleArray1D(string path)
    //    {
    //        // Load the PNG file as a Texture2D
    //        //byte[] fileData = File.ReadAllBytes(path);
    //        byte[] fileData = File.ReadAllBytes(path);
    //        Texture2D texture = new Texture2D(2, 2); // Create a temporary texture
    //        texture.LoadImage(fileData); // Load the image data into the texture

    //        int width = texture.width;
    //        int height = texture.height;

    //        if (width != 512 || height != 512)
    //        {
    //            UnityEngine.Debug.LogError("The PNG file must be 512x512 pixels.");
    //            return null;
    //        }

    //        // Create a 512*512 float array to store grayscale values
    //        float[] grayscaleArray = new float[512 * 512];

    //        // Loop through each pixel and convert to grayscale
    //        for (int y = 0; y < height; y++)
    //        {
    //            for (int x = 0; x < width; x++)
    //            {
    //                Color pixelColor = texture.GetPixel(x, y);
    //                float grayscaleValue = pixelColor.grayscale; // Unity provides a grayscale property
    //                grayscaleArray[y * height + x] = grayscaleValue;
    //            }
    //        }

    //        return grayscaleArray;
    //    }
    //    public static float[] ReadTxtData(string filePath, int inputChunkSize)
    //    {
    //        float[] data1D = new float[inputChunkSize * inputChunkSize];
    //        string[] lines = File.ReadAllLines(filePath);

    //        if (lines.Length != inputChunkSize)
    //        {
    //            throw new System.Exception($"Expected {inputChunkSize} lines in the input file, but got {lines.Length}");
    //        }

    //        for (int i = 0; i < inputChunkSize; i++)
    //        {
    //            string[] values = lines[i].Split(' ');

    //            if (values.Length != inputChunkSize)
    //            {
    //                throw new System.Exception($"Expected {inputChunkSize} values in line {i}, but got {values.Length}");
    //            }

    //            for (int j = 0; j < inputChunkSize; j++)
    //            {
    //                data1D[i * inputChunkSize + j] = float.Parse(values[j]);
    //            }
    //        }
    //        return data1D;
    //    }
    //    public static float[] GetTxtData(float[,] heightMap, int inputChunkSize)
    //    {
    //        float[] data1D = new float[inputChunkSize * inputChunkSize];

    //        if (heightMap.Length != inputChunkSize)
    //        {
    //            throw new System.Exception($"Expected {inputChunkSize} lines in the input file, but got {heightMap.Length}");
    //        }

    //        for (int i = 0; i < inputChunkSize; i++)
    //        {
    //            for (int j = 0; j < inputChunkSize; j++)
    //            {
    //                data1D[i * inputChunkSize + j] = heightMap[i, j];
    //            }
    //        }
    //        return data1D;
    //    }
    //}

}
