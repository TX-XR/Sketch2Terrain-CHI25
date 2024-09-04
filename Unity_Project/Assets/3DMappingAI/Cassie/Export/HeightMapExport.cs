using Mapbox.Map;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;

public static class HeightMapExport
{

    public static float heightDrift = 0.00f;
    public static(int minX, int minY, int maxX, int maxY) CalculateExtent(HashSet<UnwrappedTileId> CurrentExtent)
    {
        // Check if the set is empty
        if (CurrentExtent == null || CurrentExtent.Count == 0)
        {
            return (0, 0, 0, 0); // Return default values if empty
        }

        // Use LINQ to find the min and max X and Y values
        int minX = CurrentExtent.Min(id => id.X);
        int minY = CurrentExtent.Min(id => id.Y);
        int maxX = CurrentExtent.Max(id => id.X);
        int maxY = CurrentExtent.Max(id => id.Y);

        return (minX, minY, maxX, maxY);
    }

    public static void ExportHeightmaps(string outputPath, List<UnityTile> unityTiles, AbstractMap TerrainAbstractMap, int textureSize,float heightDiff, float lowerBound = 0f, float UpperBound = 1f)
    {
        // Size of the combined heightmap texture
        HashSet<UnwrappedTileId> CurrentExtent = TerrainAbstractMap.CurrentExtent;
        (int minX, int minY, int maxX, int maxY) = CalculateExtent(CurrentExtent);
        int sqrtUnityTilesCount = (int)Math.Sqrt(unityTiles.Count);

        Texture2D heightmapTexture = new Texture2D(textureSize * sqrtUnityTilesCount, textureSize * sqrtUnityTilesCount, TextureFormat.RGB24, false);

        for (int i = 0; i < unityTiles.Count; i++)
        {
            UnityTile tile = unityTiles[i];
            Vector2 vector2 = new Vector2(maxX - tile.CanonicalTileId.X + 1, maxY - tile.CanonicalTileId.Y + 1); // x-min, y-min
                                                                                                                 // Generate heightmap texture for the current UnityTile
            heightmapTexture = GenerateHeightmapTexture(tile, heightmapTexture, vector2, textureSize, heightDiff, lowerBound, UpperBound);
        }

        byte[] bytes = heightmapTexture.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
    }


    public static void ExportHeightmaps(string outputPath, float[,] heightMap, int textureSize)
    {
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        Texture2D heightmapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Calculate the corresponding position in the height map
                float srcX = (float)x / (textureSize - 1) * (width - 1);
                float srcY = (float)y / (textureSize - 1) * (height - 1);

                // Bilinear interpolation
                int x0 = (int)srcX;
                int x1 = Mathf.Min(x0 + 1, width - 1);
                int y0 = (int)srcY;
                int y1 = Mathf.Min(y0 + 1, height - 1);

                float xLerp = srcX - x0;
                float yLerp = srcY - y0;

                float top = Mathf.Lerp(heightMap[x0, y0], heightMap[x1, y0], xLerp);
                float bottom = Mathf.Lerp(heightMap[x0, y1], heightMap[x1, y1], xLerp);
                float interpolatedHeight = Mathf.Lerp(top, bottom, yLerp);
                
                float normalizedHeight = interpolatedHeight / 255f * 512f;
                normalizedHeight = Math.Clamp(normalizedHeight, 0f, 1f);
                Color color = new Color(normalizedHeight, normalizedHeight, normalizedHeight);

                heightmapTexture.SetPixel(x, y, color);
            }
        }

        heightmapTexture.Apply();
        byte[] bytes = heightmapTexture.EncodeToPNG();
        File.WriteAllBytes(outputPath, bytes);
        Debug.Log($"Heightmap exported to {outputPath}");
    }

    private static Texture2D GenerateHeightmapTexture(UnityTile tile, Texture2D heightmapTexture, Vector2 vector2, int textureSize, float heightdiff, float lowerBound = 0f, float UpperBound = 1f)
    {
        // Size of the texture
        int xOffset = textureSize * (int)vector2.x;
        int yOffset = textureSize * (int)vector2.y;
        // Iterate over each pixel in the texture
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                // Query height data for the current pixel's position
                float height = tile.QueryHeightDataNonclamped((float)x / textureSize, 1 - (float)y / textureSize) + heightdiff;
                //height += heightDrift;
                // Convert height to grayscale color (0-1)
                float grayValue = InverseLerp(lowerBound, UpperBound, height);

                // Set pixel color in the heightmap texture
                Color color = new Color(grayValue, grayValue, grayValue);
                heightmapTexture.SetPixel(x - xOffset, yOffset - y, color);
            }
        }
        // Apply changes and return the heightmap texture
        heightmapTexture.Apply();
        return heightmapTexture;
    }

    public static float InverseLerp(float a, float b, float value)
    {
        return Mathf.InverseLerp(a, b, value);
    }
}
