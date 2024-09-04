using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Linq;
using mattatz.MeshSmoothingSystem;

namespace MappingAI
{
    public static class MeshGenerator
    {
        private static readonly System.Random random = new System.Random();
        public static int GetRandomInt(int minValue, int maxValue, int scale)
        {
            return random.Next(minValue, maxValue) / scale;
        }

        public static Mesh GenerateMeshFlip(float[,] heightMap, out int _width, out int _height)
        {
            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            _width = width;
            _height = height;

            Vector3[] vertices = new Vector3[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            Vector2[] uv = new Vector2[width * height];

            // Create vertices
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Remapping the coordinate system to Unity's coordinate system
                    float xCoord = x;
                    float zCoord = y;
                    float yCoord = heightMap[x, y];

                    yCoord = yCoord * 255;
                    //yCoord = yCoord / 255 * 512;
                    //yCoord = yCoord / 255 * 512 * 1.2f;
                    vertices[y * width + x] = new Vector3(zCoord, yCoord, xCoord); // Map back to Unity coordinates and flip the Z axis by changing the sign
                    uv[y * width + x] = new Vector2((float)x / width, (float)y / height);
                }
            }

            // Create triangles
            int triangleIndex = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int bottomLeft = y * width + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + width;
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
            //Mesh mesh = new Mesh
            //{
            //    vertices = vertices,
            //    triangles = triangles,
            //    uv = uv
            //};

            // indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }
        public static Mesh Smooth(Mesh mesh, FilterType smoothType, int times = 3, float intensity = 0.5f, float hcAlpha = 0.5f, float hcBeta = 0.5f)
        {
            switch (smoothType)
            {
                case FilterType.Laplacian:
                    mesh = MeshSmoothing.LaplacianFilter(mesh, times);
                    break;
                case FilterType.HC:
                    mesh = MeshSmoothing.HCFilter(mesh, times, hcAlpha, hcBeta);
                    break;
            }
            return mesh;
        }
        public static Mesh GenerateMeshFlip(float[,] heightMap, Gradient gradient, out int _width, out int _height)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            _width = width;
            _height = height;

            Vector3[] vertices = new Vector3[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            Vector2[] uv = new Vector2[width * height];
            Color[] colors = new Color[width * height];

            float minHeight, maxHeight;
            GetMinMax(heightMap, out minHeight, out maxHeight);

            // Create vertices
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Remapping the coordinate system to Unity's coordinate system
                    float xCoord = x;
                    float zCoord = y;
                    float yCoord = heightMap[x, y];

                    yCoord = yCoord / 255f * 512f;
                    vertices[y * width + x] = new Vector3(zCoord, yCoord, xCoord); // Map back to Unity coordinates and flip the Z axis by changing the sign
                    uv[y * width + x] = new Vector2((float)x / width, (float)y / height);
                    float normalizedHeight = (yCoord - minHeight) / (maxHeight - minHeight);
                    int seaLevel = 5;
                    if (normalizedHeight < seaLevel)
                    {
                        colors[y * width + x] = new Color(0 / 256f, 42 / 256f, 73 / 256f);
                    }
                    else
                    {
                        colors[y * width + x] = gradient.Evaluate(normalizedHeight * (1 + GetRandomInt(-100, 100,300)));
                    }

                }
            }

            // Create triangles
            int triangleIndex = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int bottomLeft = y * width + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + width;
                    int topRight = topLeft + 1;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topRight;
                }
            }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                colors = colors
            };

            // indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }

        public static void GetMinMax(float[,] heightMap, out float minHeight, out float maxHeight)
        {
            minHeight = heightMap.Cast<float>().Min();
            maxHeight = heightMap.Cast<float>().Max();
        }
        public static void GetMinMax(float[] heightMap, out float minHeight, out float maxHeight)
        {
            minHeight = heightMap.Cast<float>().Min();
            maxHeight = heightMap.Cast<float>().Max();
        }
        public static Mesh GenerateMeshFlip(float[,] heightMap, Gradient gradient)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);

            Vector3[] vertices = new Vector3[width * height];
            int[] triangles = new int[(width - 1) * (height - 1) * 6];
            Vector2[] uv = new Vector2[width * height];
            Color[] colors = new Color[width * height];

            // Find min and max height for normalization
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            foreach (float h in heightMap)
            {
                if (h < minHeight) minHeight = h;
                if (h > maxHeight) maxHeight = h;
            }

            // Create vertices
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Remapping the coordinate system to Unity's coordinate system
                    float xCoord = x;
                    float zCoord = y;
                    float yCoord = heightMap[x, y];

                    yCoord = yCoord / 255f * 512f;
                    vertices[y * width + x] = new Vector3(zCoord, yCoord, xCoord); // Map back to Unity coordinates and flip the Z axis by changing the sign
                    uv[y * width + x] = new Vector2((float)x / width, (float)y / height);
                    float normalizedHeight = (yCoord - minHeight) / (maxHeight - minHeight);
                    float seaLevel = 0.1f;
                    if (normalizedHeight < seaLevel)
                    {
                        colors[y * width + x] = new Color(0 / 256f, 42 / 256f, 73 / 256f);
                    }
                    else
                    {
                        colors[y * width + x] = gradient.Evaluate(normalizedHeight);
                    }

                }
            }

            // Create triangles
            int triangleIndex = 0;
            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width - 1; x++)
                {
                    int bottomLeft = y * width + x;
                    int bottomRight = bottomLeft + 1;
                    int topLeft = bottomLeft + width;
                    int topRight = topLeft + 1;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = topLeft;

                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                    triangles[triangleIndex++] = topRight;
                }
            }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uv,
                colors = colors
            };

            // indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }


        public static List<float[,]> SplitHeightMap(float[,] heightMap, int tiles)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            int tileWidth = width / tiles;
            int tileHeight = height / tiles;

            List<float[,]> heightMaps = new List<float[,]>();

            for (int tileY = 0; tileY < tiles; tileY++)
            {
                for (int tileX = 0; tileX < tiles; tileX++)
                {
                    float[,] tileHeightMap = new float[tileWidth, tileHeight];

                    for (int y = 0; y < tileHeight; y++)
                    {
                        for (int x = 0; x < tileWidth; x++)
                        {
                            tileHeightMap[x, y] = heightMap[x + tileX * tileWidth, y + tileY * tileHeight];
                        }
                    }

                    heightMaps.Add(tileHeightMap);
                }
            }

            return heightMaps;
        }



        //public static Mesh GenerateTerrain(float[,] heightMap, float xOffset, float zOffset, int xSize, int zSize, int heightMapSize)
        //{
        //    Mesh mesh = new Mesh();
        //    Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        //    float heightMapXScale = (float)heightMapSize / xSize;
        //    float heightMapZScale = (float)heightMapSize / zSize;

        //    for (int i = 0, z = 0; z <= zSize; z++)
        //    {
        //        for (int x = 0; x <= xSize; x++, i++)
        //        {
        //            float xCoord = (x) * heightMapXScale;
        //            float zCoord = (z) * heightMapZScale;

        //            // Clamp coordinates to be within valid range
        //            xCoord = Mathf.Clamp(xCoord, 0, heightMapSize - 1);
        //            zCoord = Mathf.Clamp(zCoord, 0, heightMapSize - 1);

        //            // Get interpolated height
        //            float height = GetInterpolatedHeight(heightMap, xCoord, zCoord);

        //            //height = (height / 255) * 512;
        //            vertices[i] = new Vector3(x + xOffset, height, z + zOffset);
        //        }
        //    }

        //    int[] triangles = new int[xSize * zSize * 6];
        //    for (int z = 0, vert = 0, tris = 0; z < zSize; z++, vert++)
        //    {
        //        for (int x = 0; x < xSize; x++, vert++, tris += 6)
        //        {
        //            triangles[tris] = vert;
        //            triangles[tris + 1] = vert + xSize + 1;
        //            triangles[tris + 2] = vert + 1;
        //            triangles[tris + 3] = vert + 1;
        //            triangles[tris + 4] = vert + xSize + 1;
        //            triangles[tris + 5] = vert + xSize + 2;
        //        }
        //    }

        //    mesh.Clear();
        //    mesh.vertices = vertices;
        //    mesh.triangles = triangles;
        //    mesh.RecalculateNormals();
        //    return mesh;
        //}

        //static float GetInterpolatedHeight(float[,] heightMap, float x, float z)
        //{
        //    int x0 = Mathf.FloorToInt(x);
        //    int x1 = Mathf.Clamp(x0 + 1, 0, heightMap.GetLength(0) - 1);
        //    int z0 = Mathf.FloorToInt(z);
        //    int z1 = Mathf.Clamp(z0 + 1, 0, heightMap.GetLength(1) - 1);

        //    float tx = x - x0;
        //    float tz = z - z0;
        //    float h0 = Mathf.Lerp(heightMap[x0, z0], heightMap[x1, z0], tx);
        //    float h1 = Mathf.Lerp(heightMap[x0, z1], heightMap[x1, z1], tx);
        //    return Mathf.Lerp(h0, h1, tz);
        //}

    }
}

