using Meta.WitAi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace MappingAI
{

    public class HeightMapProcessor
    {
        /// <summary>
        /// Generate the scaled heightmap based on ratio and generate the mesh accordingly
        /// </summary>
        /// <param name="inputData"></param>
        /// <param name="heightMap"></param>
        /// <param name="inputChunkSize"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static (float[,], Mesh) ScaleHeightMap_GenerateMesh(float[] inputData, float[] heightMap, int inputChunkSize, float ratio)
        {
            int originalWidth = inputChunkSize;
            int originalHeight = inputChunkSize;
            int newWidth = Mathf.RoundToInt(originalWidth * ratio);
            int newHeight = Mathf.RoundToInt(originalHeight * ratio);

            // Generate the mesh accordingly
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            Vector3[] vertices = new Vector3[newWidth * newHeight];
            int[] triangles = new int[(newWidth - 1) * (newHeight - 1) * 6];
            Vector2[] uv = new Vector2[newWidth * newHeight];

            float[,] newHeightMap = new float[newWidth, newHeight];

            //float amplifyRatio = inputData.Max() / heightMap.Max();
            //Debug.Log("amplifyRatio:" + amplifyRatio);
            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float gx = x / (float)(newWidth - 1) * (originalWidth - 1);
                    float gy = y / (float)(newHeight - 1) * (originalHeight - 1);

                    int x0 = Mathf.FloorToInt(gx);
                    int x1 = Mathf.CeilToInt(gx);
                    int y0 = Mathf.FloorToInt(gy);
                    int y1 = Mathf.CeilToInt(gy);

                    float dx = gx - x0;
                    float dy = gy - y0;

                    float heightValue = Mathf.Lerp(
                        Mathf.Lerp(heightMap[x0 + y0 * originalWidth], heightMap[x1 + y0 * originalWidth], dx),
                        Mathf.Lerp(heightMap[x0 + y1 * originalWidth], heightMap[x1 + y1 * originalWidth], dx),
                        dy
                    );

                    newHeightMap[x, y] = Mathf.Clamp(heightValue, 0f, 1f);


                    // Remapping the coordinate system to Unity's coordinate system
                    float xCoord = x;
                    float zCoord = y;
                    float yCoord = newHeightMap[x, y];

                    //yCoord = yCoord * 255;
                    //yCoord = yCoord * amplifyRatio + 0.00001f; //avoid the terrain mesh conflit with the Sketching Canvas Bottom Plane
                    //yCoord = yCoord * amplifyRatio; //avoid the terrain mesh conflit with the Sketching Canvas Bottom Plane
                    vertices[y * newWidth + x] = new Vector3(zCoord, yCoord, xCoord); // Map back to Unity coordinates and flip the Z axis by changing the sign
                    uv[y * newWidth + x] = new Vector2((float)x / newWidth, (float)y / newHeight);
                }
            }
            // Create triangles
            int triangleIndex = 0;
            for (int y = 0; y < newHeight - 1; y++)
            {
                for (int x = 0; x < newWidth - 1; x++)
                {
                    int bottomLeft = y * newWidth + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + newWidth;
                    int topRight = topLeft + 1;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topRight;
                }
            }

            mesh.vertices = vertices;
            mesh.uv = uv;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return (newHeightMap, mesh);
        }
        public static float[,] ScaleHeightMap(float[,] originalHeightMap, float ratio)
        {
            int originalWidth = originalHeightMap.GetLength(0);
            int originalHeight = originalHeightMap.GetLength(1);
            int newWidth = Mathf.RoundToInt(originalWidth * ratio);
            int newHeight = Mathf.RoundToInt(originalHeight * ratio);

            float[,] newHeightMap = new float[newWidth, newHeight];

            for (int y = 0; y < newHeight; y++)
            {
                for (int x = 0; x < newWidth; x++)
                {
                    float gx = x / (float)(newWidth - 1) * (originalWidth - 1);
                    float gy = y / (float)(newHeight - 1) * (originalHeight - 1);

                    int x0 = Mathf.FloorToInt(gx);
                    int x1 = Mathf.CeilToInt(gx);
                    int y0 = Mathf.FloorToInt(gy);
                    int y1 = Mathf.CeilToInt(gy);

                    float dx = gx - x0;
                    float dy = gy - y0;

                    float heightValue = Mathf.Lerp(
                        Mathf.Lerp(originalHeightMap[x0, y0], originalHeightMap[x1, y0], dx),
                        Mathf.Lerp(originalHeightMap[x0, y1], originalHeightMap[x1, y1], dx),
                        dy
                    );

                    newHeightMap[x, y] = Mathf.Clamp(heightValue, 0f, 1f);
                }
            }

            return newHeightMap;
        }
       
        public static void PostProcessingPredictedResult(float[] array, float threshold)
        {
            // Apply the threshold using LINQ
            var processedArray = array.Select(value => value < threshold ? 0.000f : value).ToArray();

            // Copy the processed values back to the original array
            Array.Copy(processedArray, array, array.Length);
        }
    }

}