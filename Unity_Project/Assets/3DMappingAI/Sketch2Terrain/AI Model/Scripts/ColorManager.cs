using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MappingAI
{
    public class ColorManager
    {
        // Convert a hex string to a Color object
        public static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte a = 255; // Assume fully opaque if alpha is not specified
            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            // If the string length is 8, it includes the alpha channel
            if (hex.Length == 8)
            {
                a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
            }

            return new Color32(r, g, b, a);
        }
        public static Gradient CreateSelfDefinedGradient()
        {
            Gradient gradient = new Gradient();

            // Set the color keys based on the provided values
            GradientColorKey[] colorKeys = new GradientColorKey[7];
            colorKeys[0] = new GradientColorKey(HexToColor("80aaa1"), 0f);
            colorKeys[1] = new GradientColorKey(HexToColor("2E6868"), 0.12f);
            colorKeys[2] = new GradientColorKey(HexToColor("f0f8ce"), 0.17f);
            colorKeys[3] = new GradientColorKey(HexToColor("d2af72"), 0.3f);
            colorKeys[4] = new GradientColorKey(HexToColor("be7b40"), 0.5f);
            colorKeys[5] = new GradientColorKey(HexToColor("a95e28"), 0.8f);
            colorKeys[6] = new GradientColorKey(HexToColor("FFFFFF"), 1f);

            // Set the alpha keys (you can keep it simple with full opacity if no specific alpha values are provided)
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1.0f, 0.0f); // Fully opaque at the start
            alphaKeys[1] = new GradientAlphaKey(1.0f, 1.0f); // Fully opaque at the end

            // Apply the color and alpha keys to the gradient
            gradient.SetKeys(colorKeys, alphaKeys);
            return gradient;
        }


        public static Gradient InitializeGradient(GradientTheme gradientTheme)
        {
            Gradient g= new Gradient();
            switch (gradientTheme)
            {
                case GradientTheme.SnowyMountains:
                    g = CreateSnowyGradient();
                    break;
                case GradientTheme.EarthyMountains:
                    g = CreateEarthyGradient();
                    break;
                case GradientTheme.ForestedMountains:
                    g = CreateForestedGradient();
                    break;
                case GradientTheme.SelfDefined:
                    g = CreateSelfDefinedGradient();
                    break;
            }
            return g;
        }

        private static Gradient CreateSnowyGradient()
        {
            Gradient gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[]
            {
            new GradientColorKey(new Color(0.36f, 0.25f, 0.20f), 0.0f),
            new GradientColorKey(new Color(0.75f, 0.75f, 0.75f), 0.5f),
            new GradientColorKey(Color.white, 1.0f)
            };
            gradient.alphaKeys = new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            };
            return gradient;
        }

        private static Gradient CreateEarthyGradient()
        {
            Gradient gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[]
            {
            new GradientColorKey(new Color(0.54f, 0.27f, 0.07f), 0.0f),
            new GradientColorKey(new Color(0.72f, 0.52f, 0.04f), 0.5f),
            new GradientColorKey(new Color(0.4f, 0.4f, 0.4f), 1.0f)
            };
            gradient.alphaKeys = new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            };
            return gradient;
        }

        private static Gradient CreateForestedGradient()
        {
            Gradient gradient = new Gradient();
            gradient.colorKeys = new GradientColorKey[]
            {
            new GradientColorKey(new Color(0.13f, 0.55f, 0.13f), 0.0f),
            new GradientColorKey(new Color(0.34f, 0.25f, 0.14f), 0.5f),
            new GradientColorKey(new Color(0.6f, 0.6f, 0.6f), 1.0f)
            };
            gradient.alphaKeys = new GradientAlphaKey[]
            {
            new GradientAlphaKey(1.0f, 0.0f),
            new GradientAlphaKey(1.0f, 1.0f)
            };
            return gradient;
        }
        /// <summary>
        /// Scale the original heightmap to a smaller heightmap for faster execution
        /// </summary>
        /// <param name="originalHeightMap"></param>
        /// <param name="ratio"></param>
        /// <returns></returns>
        public static (Texture2D, Texture2D, Texture2D) Create2DHeightMapTexture(float[,] heightMap, Gradient gradient, float[] inputData)
        {
            int originalWidth = heightMap.GetLength(0);
            int originalHeight = heightMap.GetLength(1);

            Texture2D heightMapTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            Texture2D heightMapGradientTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            Texture2D inputDataTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            float minHeight, maxHeight;
            MeshGenerator.GetMinMax(heightMap, out minHeight, out maxHeight);
            for (int y = 0; y < originalHeight; y++)
            {
                for (int x = 0; x < originalWidth; x++)
                {
                    float heightValue = heightMap[x, y];
                    heightMapTexture.SetPixel(x, y, new Color(heightValue, heightValue, heightValue, 1.0f));
                    float normalizedHeight = (heightValue - minHeight) / (maxHeight - minHeight);
                    heightMapGradientTexture.SetPixel(x, y, gradient.Evaluate(normalizedHeight * (1 + MeshGenerator.GetRandomInt(-100, 100, 300))));

                    int index = (int)x + (int)y * originalHeight;
                    float heightValue_input = inputData[index];
                    inputDataTexture.SetPixel(x, y, new Color(heightValue_input, heightValue_input, heightValue_input, 1.0f));

                }
            }
            heightMapTexture.Apply();
            heightMapGradientTexture.Apply();
            inputDataTexture.Apply();
            return (heightMapTexture, heightMapGradientTexture, inputDataTexture);
        }
        public static (Texture2D, Texture2D, Texture2D) Create1DHeightMapTexture(float[] heightMap, int inputChunkSize, Gradient gradient, float[] inputData)
        {
            int originalWidth = inputChunkSize;
            int originalHeight = inputChunkSize;

            Texture2D heightMapTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            Texture2D heightMapGradientTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            Texture2D inputDataTexture = new Texture2D(originalWidth, originalHeight, TextureFormat.RGBA32, false);
            float minHeight, maxHeight;
            MeshGenerator.GetMinMax(heightMap, out minHeight, out maxHeight);
            for (int x = 0; x < originalWidth; x++)
            {
                for (int y = 0; y <  originalHeight; y++)
                {
                    int index = (int)x + (int)y * originalHeight;

                    float heightValue = heightMap[index];
                    heightMapTexture.SetPixel(x, y, new Color(heightValue, heightValue, heightValue, 1.0f));

                    float normalizedHeight = (heightValue - minHeight) / (maxHeight - minHeight);
                    //heightMapGradientTexture.SetPixel(x, y, gradient.Evaluate(normalizedHeight * (1 + MeshGenerator.GetRandomInt(-100, 100, 300))));
                    heightMapGradientTexture.SetPixel(x, y, new Color(normalizedHeight, normalizedHeight, normalizedHeight, 1.0f));


                    float heightValue_input = inputData[index];
                    inputDataTexture.SetPixel(x, y, new Color(heightValue_input, heightValue_input, heightValue_input, 1.0f));
                }
            }
            heightMapTexture.Apply();
            heightMapGradientTexture.Apply();
            inputDataTexture.Apply();
            return (heightMapTexture, heightMapGradientTexture, inputDataTexture);
        }

        public static Texture2D Create2DHeightMapTexture(float[,] heightMap)
        {
            int width = heightMap.GetLength(0);
            int height = heightMap.GetLength(1);
            Texture2D heightMapTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float heightValue = heightMap[x, y];
                    heightMapTexture.SetPixel(x, y, new Color(heightValue, heightValue, heightValue, 1.0f));
                }
            }
            heightMapTexture.Apply();
            return heightMapTexture;
        }

        public static Texture2D Create1DHeightMapTexture(float[] heightMap, int inputChunkSize)
        {
            Texture2D heightMapTexture = new Texture2D(inputChunkSize, inputChunkSize, TextureFormat.RGBA32, false);
            for (int x = 0; x < inputChunkSize; x++)
            {
                for (int y = 0; y < inputChunkSize; y++)
                {
                    int index = (int)x * inputChunkSize + (int)y;
                    float heightValue = heightMap[index];
                    heightMapTexture.SetPixel(y, x, new Color(heightValue, heightValue, heightValue, 1.0f));
                }
            }
            heightMapTexture.Apply();
            return heightMapTexture;
        }

        public static void SaveTextureAsPNG(Texture2D texture, string path)
        {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
            Debug.Log("Texture saved to " + path);
        }

    }

}
