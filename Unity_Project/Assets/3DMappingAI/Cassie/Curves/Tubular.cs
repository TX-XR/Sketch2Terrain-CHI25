using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Curve;

/*
    This code was adapted from https://github.com/mattatz/unity-tubular and somewhat broken down to keep only what was necessary for this project
    Check out mattatz original repository for better generation of tubular meshes, with regular sampling along the curve.
 */

namespace Tubular {

    public class Tubular {

        const float PI2 = Mathf.PI * 2f;

        public static Mesh Build(Curve.Curve curve, int tubularSegments, float radius, int radialSegments = 8, Color? baseColor = null, bool closed = false, bool variableWidth = false, bool variableOpacity = false)
        {
            // Use the provided base color or default to white if none is provided.
            Color color = baseColor ?? Color.white;

            // Initialize lists to store vertices, normals, tangents, colors, UVs, and mesh indices.
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var tangents = new List<Vector4>();
            List<Color> colors = new List<Color>();
            var uvs = new List<Vector2>();
            var indices = new List<int>();

            // Compute Frenet frames for the curve to provide orientation at each point along the curve.
            var frames = curve.ComputeFrenetFrames(tubularSegments, Vector3.zero, closed);

            // Generate the mesh segments along the curve.
            for (int i = 0; i < tubularSegments; i++)
            {
                GenerateSegment(curve, frames, tubularSegments, radius, radialSegments, color, variableWidth, variableOpacity, vertices, normals, tangents, colors, i);
            }
            // Special case for the last segment if the tube is not closed.
            GenerateSegment(curve, frames, tubularSegments, radius, radialSegments, color, variableWidth, variableOpacity, vertices, normals, tangents, colors, (!closed) ? tubularSegments : 0);

            // Generate UV mapping for the tube's surface.
            for (int i = 0; i <= tubularSegments; i++)
            {
                for (int j = 0; j <= radialSegments; j++)
                {
                    float u = 1f * j / radialSegments;
                    float v = 1f * i / tubularSegments;
                    uvs.Add(new Vector2(u, v));
                }
            }

            // Define faces of the tube by connecting vertices with triangles.
            for (int j = 1; j <= tubularSegments; j++)
            {
                for (int i = 1; i <= radialSegments; i++)
                {
                    int a = (radialSegments + 1) * (j - 1) + (i - 1);
                    int b = (radialSegments + 1) * j + (i - 1);
                    int c = (radialSegments + 1) * j + i;
                    int d = (radialSegments + 1) * (j - 1) + i;

                    // faces
                    indices.Add(a); indices.Add(d); indices.Add(b);
                    indices.Add(b); indices.Add(d); indices.Add(c);
                }
            }

            var mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.tangents = tangents.ToArray();
            mesh.colors = colors.ToArray();
            mesh.uv = uvs.ToArray();
            mesh.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
            return mesh;
        }

        static void GenerateSegment(
            Curve.Curve curve,
            List<FrenetFrame> frames,
            int tubularSegments,
            float maxRadius,
            int radialSegments,
            Color baseColor,
            bool variableWidth,
            bool variableOpacity,
            List<Vector3> vertices,
            List<Vector3> normals,
            List<Vector4> tangents,
            List<Color> colors,
            int i
        )
        {
            // TODO: we could probably do better than uniform sampling here,
            // but this works ok
            // Calculate the parameter 'u' by normalizing 'i' over the total number of segments.
            // This is used to accurately sample points along the curve.
            var u = 1f * i / tubularSegments;

            // Retrieve the point on the curve corresponding to 'u'.
            var p = curve.GetPoint(u);
            var fr = frames[i];

            var N = fr.Normal;
            var B = fr.Binormal;

            // Initialize radius and opacity to their maximum values.
            float radius = maxRadius;
            float opacity = 1.0f;

            // If variable width is enabled, scale the radius by the weight (thickness) at point 'u'.
            if (variableWidth)
                radius *= curve.GetWeight(u);
            if (variableOpacity)
                opacity *= curve.GetWeight(u);
            // Iterate over each radial segment to generate vertices, normals, tangents, and colors.
            for (int j = 0; j <= radialSegments; j++)
            {
                // Calculate the angle 'v' for the current radial segment.
                float v = 1f * j / radialSegments * PI2;
                // Compute the sine and cosine of 'v' for positioning around the circle.
                var sin = Mathf.Sin(v);
                var cos = Mathf.Cos(v);
                // Calculate the normal vector for the current vertex by combining N and B.
                Vector3 normal = (cos * N + sin * B).normalized;
                // Add the vertex positioned along the normal direction from the curve point 'p'.
                vertices.Add(p + radius * normal);
                normals.Add(normal);

                var tangent = fr.Tangent;
                tangents.Add(new Vector4(tangent.x, tangent.y, tangent.z, 0f));

                colors.Add(new Color(baseColor.r, baseColor.g, baseColor.b, opacity));
            }
        }

    }

}

