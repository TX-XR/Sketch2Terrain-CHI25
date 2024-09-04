using UnityEngine;
using Curve;
using System.Text;

namespace MappingAI {
    public static class CurvesExport
    {
        // Export curve data based on the sketch space range
        public static string CurveToPolylineLerp(Curve.Curve c, int subdivisionsPerUnit, float lowerBound = 0f, float UpperBound = 1f, bool showHeader = true)
        {
            StringBuilder sb = new StringBuilder();
            if (c is LineCurve)
            {
                // Line
                if (showHeader)
                    sb.Append("v 2\r\n"); // v {nb of points}
                Vector3 A = Utils.ChangeHandedness(c.GetPoint(0f));
                Vector3 B = Utils.ChangeHandedness(c.GetPoint(1f));

                A.y = Mathf.InverseLerp(lowerBound, UpperBound, A.y);
                B.y = Mathf.InverseLerp(lowerBound, UpperBound, B.y);

                sb.Append(Format(A));
                sb.Append(Format(B));

                return sb.ToString();
            }
            else
            {
                int nPts = Mathf.CeilToInt(c.GetLength() * subdivisionsPerUnit);

                // Curve header
                if (showHeader)
                    sb.Append("v " + nPts.ToString() + "\r\n"); // v {nb of points}
                float step = 1f / (nPts - 1);
                for (int i = 0; i < nPts; i++)
                {
                    Vector3 sample = Utils.ChangeHandedness(c.GetPoint(i * step));
                    float newY = Mathf.InverseLerp(lowerBound, UpperBound, sample.y);
                    sample.y = newY;

                    sb.Append(Format(sample));
                }
                return sb.ToString();
            }
        }

        public static string CurveToPolyline(Curve.Curve c, int subdivisionsPerUnit)
        {
            StringBuilder sb = new StringBuilder();
            if (c is LineCurve)
            {
                // Line
                sb.Append("v 2\r\n"); // v {nb of points}
                Vector3 A = Utils.ChangeHandedness(c.GetPoint(0f));
                Vector3 B = Utils.ChangeHandedness(c.GetPoint(1f));

                sb.Append(Format(A));
                sb.Append(Format(B));

                return sb.ToString();
            }
            else
            {
                int nPts = Mathf.CeilToInt(c.GetLength() * subdivisionsPerUnit);

                // Curve header
                sb.Append("v " + nPts.ToString() + "\r\n"); // v {nb of points}
                float step = 1f / (nPts - 1);
                for (int i = 0; i < nPts; i++)
                {
                    Vector3 sample = Utils.ChangeHandedness(c.GetPoint(i * step));
                    sb.Append(Format(sample));
                }
                return sb.ToString();
            }
        }
        public static string SamplesToPolylineLerp(Vector3[] inputSamples, float lowerBound = 0f, float UpperBound = 1f)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("v " + inputSamples.Length.ToString() + "\r\n"); // v {nb of points}

            foreach (var p in inputSamples)
            {
                Vector3 A = Utils.ChangeHandedness(p);
                A.y = Mathf.InverseLerp(lowerBound, UpperBound, A.y);
                sb.Append(Format(A));
            }

            return sb.ToString();
        }

        public static string SamplesToPolyline(Vector3[] inputSamples)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("v " + inputSamples.Length.ToString() + "\r\n"); // v {nb of points}

            foreach (var p in inputSamples)
            {
                sb.Append(Format(Utils.ChangeHandedness(p)));
            }

            return sb.ToString();
        }

        private static string Format(Vector3 v)
        {
            //return string.Format("{0:+0.000000;-0.000000} {1:+0.000000;-0.000000} {2:+0.000000;-0.000000}\r\n", v.x, v.y, v.z);
            return string.Format("{0:+0.00000;-0.00000} {1:+0.00000;-0.00000} {2:+0.00000;-0.00000}\r\n", v.z, -v.x, v.y);
        }
    }
}