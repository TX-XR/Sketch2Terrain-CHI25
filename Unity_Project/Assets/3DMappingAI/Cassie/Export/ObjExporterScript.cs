using System.Collections.Generic;
using System;
using System.IO;
using System.Text;

using UnityEngine;
using System.Linq;
using Mapbox.Unity.MeshGeneration.Data;
using System.Threading.Tasks;

namespace MappingAI {

    public class ObjExporterScript
    {
        private static int StartIndex = 0;

        public static void Start()
        {
            StartIndex = 0;
        }
        public static void End()
        {
            StartIndex = 0;
        }
        public static void ExportMeshAndTexture(Mesh combinedMesh, Transform transform, Texture2D combinedTexture, string exportDirectory, string fileName)
        {
            Directory.CreateDirectory(exportDirectory);

            string objFilePath = Path.Combine(exportDirectory, fileName + ".obj");
            string mtlFilePath = Path.Combine(exportDirectory, fileName + ".mtl");
            string textureFilePath = Path.Combine(exportDirectory, fileName + ".png");

            // Write the OBJ file
            using (StreamWriter sw = new StreamWriter(objFilePath))
            {
                sw.WriteLine("mtllib " + Path.GetFileName(mtlFilePath));

                // Export the combined mesh
                sw.Write(MeshToStringFromWorld2LocalTexture(combinedMesh, transform, fileName));

            }
            // Write the MTL file
            using (StreamWriter sw = new StreamWriter(mtlFilePath))
            {
                sw.WriteLine("newmtl " + fileName);
                sw.WriteLine("Ka 1.000 1.000 1.000");
                sw.WriteLine("Kd 1.000 1.000 1.000");
                sw.WriteLine("Ks 0.000 0.000 0.000");
                sw.WriteLine("d 1.0");
                sw.WriteLine("illum 2");
                sw.WriteLine("map_Kd " + Path.GetFileName(textureFilePath));
            }

            // Save the combined texture as a PNG file
            File.WriteAllBytes(textureFilePath, combinedTexture.EncodeToPNG());
        }
        public static string MeshToStringFromWorld2LocalMirrorFlip(Mesh m, Material material, Transform t)
        {
            Vector3 s = t.localScale;
            Vector3 p = t.localPosition;
            Quaternion r = t.localRotation;

            // Rotate 180 degrees around Y-axis
            //Quaternion rotate180 = Quaternion.Euler(0, 180, 0);

            int numVertices = 0;
            if (!m)
            {
                return "\n";
            }

            StringBuilder sb = new StringBuilder();

            // Export vertices
            foreach (Vector3 vv in m.vertices)
            {
                // Apply scaling, rotation, and translation to vertices
                Vector3 v = t.TransformPoint(Vector3.Scale(vv, s));
                v = t.InverseTransformPoint(v) + p;
                // Apply rotation
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");

            // Export normals
            foreach (Vector3 nn in m.normals)
            {
                // Apply rotation to normals
                Vector3 v = r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");

            // Export UVs
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            sb.Append("\n");

            string materialName = GenerateMaterialName(material);
            sb.Append("\n");
            sb.Append("usemtl ").Append(materialName).Append("\n");
            sb.Append("usemap ").Append(materialName).Append("\n");

            // Export faces
            for (int submesh = 0; submesh < m.subMeshCount; submesh++)
            {
                sb.Append("\n");

                int[] triangles = m.GetTriangles(submesh);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                            triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
                }
            }

            StartIndex += numVertices;
            return sb.ToString();
        }
        public static string MeshToStringFromWorld2Local(Mesh m, Transform t)
        {
            Vector3 s = t.localScale;
            Vector3 p = t.localPosition;
            Quaternion r = t.localRotation;


            int numVertices = 0;
            //Mesh m = mf.sharedMesh;
            if (!m)
            {
                return "\n";
            }

            StringBuilder sb = new StringBuilder();

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = t.InverseTransformPoint(vv);
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");

            // Export normals
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
            }
            sb.Append("\n");

            // Export UVs
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            // Export faces
            for (int material = 0; material < m.subMeshCount; material++)
            {
                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                            triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
                }
            }

            StartIndex += numVertices;
            return sb.ToString();
        }

        public static Mesh CombineMeshes(List<Mesh> meshes, List<Transform> transforms, float ratio)
        {
            Mesh combinedMesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int vertexOffset = 0;

            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = meshes[i];
                Transform transform = transforms[i];
                Vector3[] meshVertices = mesh.vertices;
                int[] meshTriangles = mesh.triangles;
                Vector2[] meshUVs = mesh.uv;

                Vector3 s = transform.localScale;
                Vector3 p = transform.localPosition;

                // Apply transformation to vertices
                for (int j = 0; j < meshVertices.Length; j++)
                {
                    Vector3 v = transform.TransformPoint(Vector3.Scale(meshVertices[j], s));
                    v = transform.InverseTransformPoint(v) - p;
                    meshVertices[j] = v;
                }

                // Simplify the mesh according to the ratio
                if (ratio < 1.0f)
                {
                    Mesh simplifiedMesh = SimplifyMesh(mesh, ratio);
                    meshVertices = simplifiedMesh.vertices;
                    meshTriangles = simplifiedMesh.triangles;
                    meshUVs = simplifiedMesh.uv;
                }

                // Add vertices, triangles, and uvs to the combined lists
                vertices.AddRange(meshVertices);

                // Add triangles to the combined list, with the correct offset
                for (int j = 0; j < meshTriangles.Length; j++)
                {
                    triangles.Add(meshTriangles[j] + vertexOffset);
                }

                // Add UVs to the combined list
                uvs.AddRange(meshUVs);

                vertexOffset += meshVertices.Length;
            }

            combinedMesh.SetVertices(vertices);
            combinedMesh.SetTriangles(triangles, 0);
            combinedMesh.SetUVs(0, uvs);
            combinedMesh.RecalculateNormals();
            combinedMesh.RecalculateBounds();

            return combinedMesh;
        }

        private static Mesh SimplifyMesh(Mesh mesh, float ratio)
        {
            Mesh simplifiedMesh = new Mesh();

            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector2[] uvs = mesh.uv;

            List<Vector3> newVertices = new List<Vector3>();
            List<int> newTriangles = new List<int>();
            List<Vector2> newUVs = new List<Vector2>();

            int vertexStep = Mathf.CeilToInt(1 / ratio);
            int triangleStep = Mathf.CeilToInt(3 / ratio);

            // Reduce vertices
            for (int i = 0; i < vertices.Length; i += vertexStep)
            {
                newVertices.Add(vertices[i]);
                if (i < uvs.Length)
                {
                    newUVs.Add(uvs[i]);
                }
            }

            // Reduce triangles
            for (int i = 0; i < triangles.Length; i += triangleStep)
            {
                if (i + 2 < triangles.Length)
                {
                    int newIndex0 = triangles[i] / vertexStep;
                    int newIndex1 = triangles[i + 1] / vertexStep;
                    int newIndex2 = triangles[i + 2] / vertexStep;

                    if (newIndex0 < newVertices.Count && newIndex1 < newVertices.Count && newIndex2 < newVertices.Count)
                    {
                        newTriangles.Add(newIndex0);
                        newTriangles.Add(newIndex1);
                        newTriangles.Add(newIndex2);
                    }
                }
            }

            simplifiedMesh.SetVertices(newVertices);
            simplifiedMesh.SetTriangles(newTriangles, 0);
            simplifiedMesh.SetUVs(0, newUVs);
            simplifiedMesh.RecalculateNormals();
            simplifiedMesh.RecalculateBounds();

            return simplifiedMesh;
        }
     
        public static string MeshToStringFromWorld2LocalTexture(Mesh m, Transform t, string textureFileName)
        {
            Vector3 s = t.localScale;
            Vector3 p = t.localPosition;
            Quaternion r = t.localRotation;


            int numVertices = 0;
            //Mesh m = mf.sharedMesh;
            if (!m)
            {
                return "\n";
            }

            StringBuilder sb = new StringBuilder();

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = t.InverseTransformPoint(vv);
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");

            // Export normals
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
            }
            sb.Append("\n");

            // Export UVs
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }
            // Create a single material entry in the OBJ file
            sb.Append("usemtl ").Append(Path.GetFileNameWithoutExtension(textureFileName)).Append("\n");
            sb.Append("usemap ").Append(Path.GetFileNameWithoutExtension(textureFileName)).Append("\n");

            // Export faces
            for (int material = 0; material < m.subMeshCount; material++)
            {
                int[] triangles = m.GetTriangles(material);
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                            triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
                }
            }

            StartIndex += numVertices;
            return sb.ToString();
        }
        /// <summary>
        /// To achieve the desired behavior where the output is in a space relative to the parent (with the center at (0,0,0)), we need to transform the vertices and normals accordingly. 
        /// </summary>
        /// <param name="m"></param>
        /// <param name="t"></param>
        /// <param name="objectSpace"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static string MeshToString(Mesh m, Transform t, bool objectSpace = false, Transform parent = null)
        {
            Vector3 s = Vector3.zero;
            Vector3 p = Vector3.zero;
            Quaternion r = Quaternion.identity;

            if (objectSpace)
            {
                s = t.localScale;
                p = t.localPosition;
                r = t.localRotation;
            }
            else
            {
                s = t.lossyScale;
                if (parent != null)
                {
                    p = parent.InverseTransformPoint(t.position);
                    r = Quaternion.Inverse(parent.rotation) * t.rotation;
                }
                else
                {
                    p = t.position;
                    r = t.rotation;
                }
            }

            int numVertices = 0;
            if (!m)
            {
                return "\n";
            }

            StringBuilder sb = new StringBuilder();

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = objectSpace ? vv : t.TransformPoint(vv);
                if (parent != null && !objectSpace)
                {
                    v = parent.InverseTransformPoint(v);
                }
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = objectSpace ? nn : r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 uv in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", uv.x, uv.y));
            }

            int[] triangles = m.GetTriangles(0);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                        triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
            }

            StartIndex += numVertices;
            return sb.ToString();
        }
        public static string MeshToString(Mesh m, Material material, Transform t, bool objectSpace = false)
        {
            Vector3 s = Vector3.zero;
            Vector3 p = Vector3.zero;
            Quaternion r = Quaternion.identity;
            //if (objectSpace)
            //{
            //    s = t.localScale;
            //    p = t.localPosition;
            //    r = t.localRotation;
            //}
            //else
            //{
            //    s = t.lossyScale;
            //    p = t.position;
            //    r = t.rotation;
            //}


            s = t.localScale;
            p = t.localPosition;
            r = t.localRotation;
            //Vector3 s = t.lossyScale;
            //Vector3 p = t.position;
            //Quaternion r = t.rotation;


            int numVertices = 0;
            //Mesh m = mf.sharedMesh;
            if (!m)
            {
                return "\n";
            }
            

            StringBuilder sb = new StringBuilder();

            foreach (Vector3 vv in m.vertices)
            {
                Vector3 v = objectSpace ? vv : t.TransformPoint(vv);
                numVertices++;
                sb.Append(string.Format("v {0} {1} {2}\n", v.x, v.y, -v.z));
            }
            sb.Append("\n");
            foreach (Vector3 nn in m.normals)
            {
                Vector3 v = objectSpace ? nn : r * nn;
                sb.Append(string.Format("vn {0} {1} {2}\n", -v.x, -v.y, v.z));
            }
            sb.Append("\n");
            foreach (Vector3 v in m.uv)
            {
                sb.Append(string.Format("vt {0} {1}\n", v.x, v.y));
            }

            string materialName = GenerateMaterialName(material);
            sb.Append("\n");
            sb.Append("usemtl ").Append(materialName).Append("\n");
            sb.Append("usemap ").Append(materialName).Append("\n");

            int[] triangles = m.GetTriangles(0);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                sb.Append(string.Format("f {0}/{0}/{0} {1}/{1}/{1} {2}/{2}/{2}\n",
                                        triangles[i] + 1 + StartIndex, triangles[i + 1] + 1 + StartIndex, triangles[i + 2] + 1 + StartIndex));
            }

            StartIndex += numVertices;
            return sb.ToString();
        }
        public static string GenerateMaterialName(Material material)
        {
            if (material.HasProperty("_HeightMap"))
            {
                return material.name + "_HeightMap";
            }
            else if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                return material.name + "_" + ColorUtility.ToHtmlStringRGB(color);
            }
            return material.name;
        }
        public static string MaterialToString(Material material)
        {
            StringBuilder sb = new StringBuilder();

            string materialName = GenerateMaterialName(material);
            sb.Append("newmtl ").Append(materialName).Append("\n");
            if (material.HasProperty("_Color"))
            {
                Color color = material.color;
                sb.Append(string.Format("Kd {0} {1} {2}\n", color.r, color.g, color.b));
            }
            if (material.HasProperty("_SpecColor"))
            {
                Color specColor = material.GetColor("_SpecColor");
                sb.Append(string.Format("Ks {0} {1} {2}\n", specColor.r, specColor.g, specColor.b));
            }
            if (material.HasProperty("_MainTex"))
            {
                Texture texture = material.mainTexture;
                if (texture != null)
                {
                    sb.Append("map_Kd ").Append("AI_Texture" + texture.name).Append(".png").Append("\n");
                }
            }

            return sb.ToString();
        }


        public static async Task GenerateTerrainCombinedMeshAsync(UnityTile[] allTiles, Transform parent, int index, Action<GameObject, UnityTile[]> callback)
        {
            // Wait for 3 seconds
            List<Mesh> combinedMeshes = new List<Mesh>();
            List<Transform> combinedTransforms = new List<Transform>();
            List<Texture2D> textures = new List<Texture2D>();
            foreach (UnityTile s in allTiles)
            {
                if (s.GetComponent<MeshFilter>() != null)
                {
                    combinedMeshes.Add(s.GetComponent<MeshFilter>().sharedMesh);
                    combinedTransforms.Add(s.transform);
                }
                // Get the texture from the mesh's material
                if (s.GetComponent<Renderer>().material.mainTexture is Texture2D texture)
                {
                    textures.Add(texture);
                }
            }

            Mesh combinedMesh = CombineMeshes(combinedMeshes, combinedTransforms, textures, ExportController.GetTerrainTextureSize(), out Texture2D combinedTexture);

            // Create a new GameObject to display the combined mesh
            GameObject combinedObject = new GameObject("CombinedTerrainMesh" + (index + 1));
            combinedObject.transform.parent = parent;
            combinedObject.transform.localPosition = Vector3.zero;
            combinedObject.transform.localRotation = Quaternion.identity;
            MeshFilter mf = combinedObject.AddComponent<MeshFilter>();
            MeshRenderer mr = combinedObject.AddComponent<MeshRenderer>();
            mf.mesh = combinedMesh;
            // Apply the combined texture
            //Material mat = new Material(Shader.Find("Standard"))
            Material mat = new Material(MaterialManager.Instance.materialForTerrainTexture);
            mat.mainTexture = combinedTexture;
            mr.material = mat;

            // Invoke the callback with the combined mesh and texture
            callback?.Invoke(combinedObject, allTiles);
            await Task.Yield();
        }

        public static GameObject GenerateTerrainCombinedMesh(UnityTile[] allTiles, int index)
        {
            List<Mesh> combinedMeshes = new List<Mesh>();
            List<Transform> combinedTransforms = new List<Transform>();
            List<Texture2D> textures = new List<Texture2D>();

            foreach (UnityTile s in allTiles)
            {
                combinedMeshes.Add(s.GetComponent<MeshFilter>().sharedMesh);
                combinedTransforms.Add(s.transform);
                // Get the texture from the mesh's material
                if (s.GetComponent<Renderer>().material.mainTexture is Texture2D texture)
                {
                    textures.Add(texture);
                }
            }

            Mesh combinedMesh = CombineMeshes(combinedMeshes, combinedTransforms, textures, ExportController.GetTerrainTextureSize(), out Texture2D combinedTexture);

            // Create a new GameObject to display the combined mesh
            GameObject combinedObject = new GameObject("CombinedTerrainMesh" + (index + 1));
            //combinedObject.transform.parent = parent;
            combinedObject.transform.position = Vector3.zero;
            combinedObject.transform.rotation = Quaternion.identity;
            combinedObject.transform.localPosition = Vector3.zero + new Vector3(0, HeightMapExport.heightDrift, 0);
            combinedObject.transform.localRotation = Quaternion.identity;

            MeshFilter mf = combinedObject.AddComponent<MeshFilter>();
            MeshRenderer mr = combinedObject.AddComponent<MeshRenderer>();
            mf.mesh = combinedMesh;
            // Apply the combined texture
            //Material mat = new Material(Shader.Find("Standard"))
            Material mat = new Material(MaterialManager.Instance.materialForTerrainTexture);
            mat.mainTexture = combinedTexture;
            mr.material = mat;
            return combinedObject;
        }
        public static Mesh CombineMeshes(List<Mesh> meshes, List<Transform> transforms, List<Texture2D> textures, int textureSize, out Texture2D combinedTexture)
        {
            Mesh combinedMesh = new Mesh();

            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();

            int vertexOffset = 0;

            // Combine textures into a texture atlas
            combinedTexture = CombineTextures(textures, textureSize, out Rect[] textureRects);

            // Combine meshes and apply transformations
            for (int i = 0; i < meshes.Count; i++)
            {
                Mesh mesh = meshes[i];
                Transform transform = transforms[i];
                Vector3[] meshVertices = mesh.vertices;
                int[] meshTriangles = mesh.triangles;
                Vector2[] meshUVs = mesh.uv;

                // Apply transformation to vertices
                for (int j = 0; j < meshVertices.Length; j++)
                {
                    //meshVertices[j] = transform.TransformPoint(meshVertices[j]);
                    meshVertices[j] = transform.localPosition + meshVertices[j];
                }

                // Adjust UVs to fit into the atlas
                Rect rect = textureRects[i];
                for (int j = 0; j < meshUVs.Length; j++)
                {
                    meshUVs[j] = new Vector2(
                        Mathf.Lerp(rect.xMin, rect.xMax, meshUVs[j].x),
                        Mathf.Lerp(rect.yMin, rect.yMax, meshUVs[j].y)
                    );
                }

                // Add vertices, triangles, and uvs to the combined lists
                vertices.AddRange(meshVertices);
                uvs.AddRange(meshUVs);

                // Add triangles to the combined list, with the correct offset
                for (int j = 0; j < meshTriangles.Length; j++)
                {
                    triangles.Add(meshTriangles[j] + vertexOffset);
                }

                vertexOffset += mesh.vertexCount;
            }

            combinedMesh.SetVertices(vertices);
            combinedMesh.SetTriangles(triangles, 0);
            combinedMesh.SetUVs(0, uvs);

            //// Apply the average position and rotation to the combined mesh
            //Vector3[] combinedVertices = combinedMesh.vertices;
            //for (int i = 0; i < combinedVertices.Length; i++)
            //{
            //    combinedVertices[i] -= averagePosition;
            //    combinedVertices[i] = averageRotation * combinedVertices[i];
            //    combinedVertices[i] += averagePosition;
            //}
            //combinedMesh.SetVertices(combinedVertices);

            return combinedMesh;
        }

        private static Texture2D CombineTextures(List<Texture2D> textures, int textureSize, out Rect[] rects)
        {
            Texture2D textureAtlas = new Texture2D(textureSize, textureSize);
            rects = textureAtlas.PackTextures(textures.ToArray(), 0, textureSize);

            return textureAtlas;
        }
        
        public Texture2D GenerateHeightmapTexture(UnityTile tile)
        {
            // Size of the texture
            int textureSize = 1024;
            Texture2D heightmapTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGB24, false);

            // Iterate over each pixel in the texture
            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // Query height data for the current pixel's position
                    float height = tile.QueryHeightData((float)x / textureSize, 1 - (float)y / textureSize);
                    // Convert height to grayscale color (0-1)
                    float grayValue = Mathf.InverseLerp(0, 255, height);
                    // Set pixel color in the heightmap texture
                    Color color = new Color(grayValue, grayValue, grayValue);
                    heightmapTexture.SetPixel(x, y, color);
                }
            }

            // Apply changes and return the heightmap texture
            heightmapTexture.Apply();
            return heightmapTexture;
        }

        public static void MeshToHeightmapTexture(Mesh mesh, Transform transform, string outputPath, int textureSize = 1024)
        {
            // Size of the heightmap texture
            Texture2D heightmapTexture = new Texture2D(textureSize, textureSize);

            // Calculate the world space bounds of the mesh
            Bounds bounds = mesh.bounds;
            bounds.center = transform.TransformPoint(bounds.center);

            // Calculate the step size for sampling the mesh
            float stepSizeX = bounds.size.x / textureSize;
            float stepSizeZ = bounds.size.z / textureSize;

            // Sample the height of the mesh at each pixel's position
            for (int z = 0; z < textureSize; z++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    // Calculate the world space position of the pixel
                    Vector3 pixelWorldPos = bounds.min + new Vector3(x * stepSizeX, 0, z * stepSizeZ);

                    // Raycast to find the height of the mesh at the pixel's position
                    RaycastHit hit;
                    if (Physics.Raycast(pixelWorldPos + Vector3.up * bounds.size.y, Vector3.down, out hit, bounds.size.y * 2, LayerMask.GetMask("Default")))
                    {
                        // Convert the height to a color value (0-1)
                        float heightNormalized = Mathf.InverseLerp(bounds.min.y, bounds.max.y, hit.point.y);

                        // Set the pixel color in the heightmap texture
                        heightmapTexture.SetPixel(x, z, new Color(heightNormalized, heightNormalized, heightNormalized));
                    }
                    else
                    {
                        // If no mesh is hit, assign black color
                        heightmapTexture.SetPixel(x, z, Color.black);
                    }
                }
            }

            // Apply changes and return the heightmap texture
            heightmapTexture.Apply();
            byte[] bytes = heightmapTexture.EncodeToPNG();
            File.WriteAllBytes(outputPath, bytes);
        }

    }
}