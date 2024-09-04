using UnityEngine;
using Curve;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */
namespace MappingAI
{


    public class Beautifier : MonoBehaviour
    {

        public int MaxBeziersForSolver = 15;

        public bool DebugVisualization = false;

        [SerializeField]
        private ParametersManager parameters = null;

        // 3D GRID
        public Grid3D grid;
        // CANVAS
        public DrawingCanvas canvas;

        // Visualizer
        private ConstraintsVisualizer visualizer;

        private static System.Diagnostics.Stopwatch performanceTimer = new System.Diagnostics.Stopwatch();

        private void Awake()
        {
            MathNet.Numerics.Providers.Common.Mkl.MklProvider.Load(Application.productName + "_Data/Plugins/x86_64");
            var usingNativeMKL = MathNet.Numerics.Control.TryUseNativeMKL();
            Debug.Log("Using native MKL: " + usingNativeMKL);

            // Call Math.Net here so that it won't lag on first call at runtime
            double[,] m_array = {{ 1.0, 2.0 },
               { 3.0, 4.0 }};
            var m = Matrix<double>.Build.DenseOfArray(m_array);
            Vector<double> v = Vector<double>.Build.Dense(2);

            Vector<double> x = m.Solve(v);

            if (DebugVisualization)
                visualizer = GetComponent<ConstraintsVisualizer>();
            else
            {
                GetComponent<ConstraintsVisualizer>().enabled = false;
            }

            if (ConstraintSolver.LogPerformance)
                ConstraintSolver.LogStream = new System.IO.StreamWriter("solver_log.txt");
        }

        private void OnDestroy()
        {
            if (ConstraintSolver.LogPerformance)
                ConstraintSolver.LogStream.Close();
        }

        // Replaces s by best matching candidate
        public (Curve.Curve, IntersectionConstraint[], MirrorPlaneConstraint[]) Beautify(
            InputStroke inputStroke,
            bool FitToConstraints,
            bool mirror,
            out List<SerializableConstraint> appliedConstraints, out List<SerializableConstraint> rejectedConstraints, out bool planar, out bool onSurface, out bool onMirror, out bool isClosed)
        {

            planar = false;
            onSurface = false;
            onMirror = false;
            isClosed = false;
            appliedConstraints = new List<SerializableConstraint>(0);
            rejectedConstraints = new List<SerializableConstraint>(0);

            Curve.Curve curve;
            IntersectionConstraint[] intersections = new IntersectionConstraint[0];
            MirrorPlaneConstraint[] mirrorPlaneConstraints = new MirrorPlaneConstraint[0];

            // Snap to line?
            // Only if we're not constrained to a surface

            List<Vector3> inputSamples = inputStroke.GetSafePoints(ablationDuration: parameters.Current.SamplesAblationDuration);

            float curveLength = inputStroke.Length;

            // Strokes that should become lines:
            //    - very short strokes
            //    - strokes that are close to linear and drawn fast

            float lineLength = Vector3.Distance(inputSamples[0], inputSamples[inputSamples.Count - 1]);
            //float lineDrawingSpeed = canvas.Small / 0.05f;
            float lineDrawingSpeed = parameters.Current.SmallDistance / 0.05f;
            if (curveLength < parameters.Current.SmallDistance || (Mathf.Abs(curveLength - lineLength) / lineLength < parameters.Current.SmallDistance && inputStroke.AverageDrawingSpeed() > lineDrawingSpeed))
            {
                curve = new LineCurve(inputSamples[0], inputSamples[inputSamples.Count - 1], 1f, 1f); // Last 2 parameters are unused pressure values
            }
            else
            {
                // Replace by a cubic bezier curve
                List<float> weights = inputStroke.GetWeights();
                // First remove hooks and break at corners
                List<List<Vector3>> G1sections = inputStroke.GetG1sections(
                    discontinuityAngularThreshold: parameters.Current.MaxAngularVariationInG1Section,
                    hookDiscontinuityAngularThreshold: parameters.Current.SmallAngle,
                    ablationDuration: parameters.Current.SamplesAblationDuration,
                    minSectionLength: parameters.Current.MinG1SectionLength,
                    maxHookLength: parameters.Current.MaxHookSectionLength,
                    maxHookStrokeRatio: parameters.Current.MaxHookSectionStrokeRatio);
                // Fit one polybezier curve per section
                List<CubicBezier> allBeziers = new List<CubicBezier>();
                foreach (var section in G1sections)
                {
                    CubicBezier[] beziers = BezierCurve.FitCurve(
                        points: section,
                        error: parameters.Current.BezierFittingError,
                        rdpError: parameters.Current.SmallDistance * 0.1f);
                    allBeziers.AddRange(beziers);
                }
                // Form back one complete bezier curve object made of all the G1 pieces
                curve = new BezierCurve(allBeziers.ToArray(), weights);
            }

            if (FitToConstraints)
            {
                // Correct intersections
                // This corrects intersection constraints positions by looking in a small neighborhood on the intersected curve
                List<Constraint> correctedConstraints = CorrectIntersections(
                    constraints: inputStroke.Constraints,
                    curve: curve,
                    search_distance: parameters.Current.SmallDistance,
                    N_steps: 5);

                // Self intersection at endpoints?
                if (curve as BezierCurve != null)
                {
                    PointOnCurve start = curve.GetPointOnCurve(0f);
                    PointOnCurve end = curve.GetPointOnCurve(1f);
                    if (Vector3.Distance(start.Position, end.Position) < parameters.Current.ClosedThreshold
                        && ((BezierCurve)curve).beziers.Length > 1
                        )
                    {
                        isClosed = true;
                    }
                }
                // Deal with overlaps with other strokes by cutting the new stroke if needed
                // Look at first and last constraints, check if the new stroke aligns well with the intersected stroke at those points
                if (!isClosed && correctedConstraints.Count > 0 && correctedConstraints[0] as IntersectionConstraint != null)
                {
                    IntersectionConstraint firstIntersection = (IntersectionConstraint)correctedConstraints[0];
                    PointOnCurve newStrokeStart = curve.GetPointOnCurve(0f);

                    // Check if tangents are aligned and if the intersection is close to beginning of the new stroke
                    if (Vector3.Distance(firstIntersection.Position, newStrokeStart.Position) < parameters.Current.ProximityThreshold
                        && Mathf.Abs(Vector3.Dot(firstIntersection.OldCurveData.Tangent, newStrokeStart.Tangent)) > Mathf.Cos(parameters.Current.SmallAngle)
                        )
                    {
                        // Modify constraint
                        correctedConstraints.RemoveAt(0);
                        IntersectionConstraint newConstraint = firstIntersection.IntersectedStroke.GetConstraint(firstIntersection.Position, parameters.Current.SnapToExistingNodeThreshold);
                        correctedConstraints.Insert(0, newConstraint);

                        // Cut input stroke
                        PointOnCurve newStrokeCorrectedStart = curve.Project(newConstraint.Position);
                        curve.CutAt(newStrokeCorrectedStart.t, throwBefore: true, snapToExistingAnchorThreshold: parameters.Current.SmallDistance * 0.1f);
                    }
                }

                if (!isClosed && correctedConstraints.Count > 0 && correctedConstraints[correctedConstraints.Count - 1] as IntersectionConstraint != null)
                {
                    IntersectionConstraint lastIntersection = (IntersectionConstraint)correctedConstraints[correctedConstraints.Count - 1];
                    PointOnCurve newStrokeEnd = curve.GetPointOnCurve(1f);

                    // Check if tangents are aligned and if the intersection is close to beginning of the new stroke
                    if (Vector3.Distance(lastIntersection.Position, newStrokeEnd.Position) < parameters.Current.ProximityThreshold
                        && Mathf.Abs(Vector3.Dot(lastIntersection.OldCurveData.Tangent, newStrokeEnd.Tangent)) > Mathf.Cos(parameters.Current.SmallAngle)
                        )
                    {
                        correctedConstraints.RemoveAt(correctedConstraints.Count - 1);
                        IntersectionConstraint newConstraint = lastIntersection.IntersectedStroke.GetConstraint(lastIntersection.Position, parameters.Current.SnapToExistingNodeThreshold);
                        correctedConstraints.Add(newConstraint);

                        // Cut input stroke
                        PointOnCurve newStrokeCorrectedEnd = curve.Project(newConstraint.Position);
                        curve.CutAt(newStrokeCorrectedEnd.t, throwBefore: false, snapToExistingAnchorThreshold: parameters.Current.SmallDistance * 0.1f);
                    }
                }



                if (curve as BezierCurve != null)
                {
                    // Avoid beautifying overly long curves
                    int N_bez = ((BezierCurve)curve).GetBezierCountBetween(0f, 1f);
                    if (N_bez > MaxBeziersForSolver)
                    {
                        return (curve, intersections, mirrorPlaneConstraints);
                    }
                    // Only treat non degenerate curve (ie, a curve that has non collapsed ctrl polygon edges)
                    if (((BezierCurve)curve).IsNonDegenerate())
                    {
                        ConstraintSolver solver = new ConstraintSolver(
                            (BezierCurve)curve,
                            //inputStroke.Constraints.ToArray(),
                            correctedConstraints.ToArray(),
                            canvas.OrthoDirections,
                            mu_fidelity: parameters.Current.MuFidelity,
                            proximityThreshold: parameters.Current.ProximityThreshold,
                            minDistanceBetweenAnchors: parameters.Current.MinDistanceBetweenAnchors,
                            angularProximityThreshold: parameters.Current.SmallAngle,
                            is_closed: isClosed,
                            planarity_allowed: true // do not allow planarity if the stroke is constrained to a surface
                        );

                        (curve, intersections, mirrorPlaneConstraints) = solver.GetBestFit(
                            out appliedConstraints,
                            out rejectedConstraints,
                            out List<int> constrainedAnchorIdx,
                            out planar,
                            out isClosed);

                    }
                }
                else if (curve as LineCurve != null)
                {

                    // A line can only find 2 intersections for now...
                    (intersections, mirrorPlaneConstraints) = ((LineCurve)curve).Constrain(
                        inputStroke.Constraints.ToArray(),
                        canvas.OrthoDirections,
                        parameters.Current.SmallAngle,
                        parameters.Current.ProximityThreshold,
                        out appliedConstraints,
                        out rejectedConstraints);
                }

                // Visualize
                if (visualizer != null)
                {
                    Debug.Log("visualizing constraints");
                    visualizer.Display(appliedConstraints, rejectedConstraints);
                }

            }

            return (curve, intersections, mirrorPlaneConstraints);
        }

        private List<Constraint> CorrectIntersections(List<Constraint> constraints, Curve.Curve curve, float search_distance, int N_steps)
        {
            List<Constraint> correctedConstraints = new List<Constraint>(constraints.Count);

            foreach (var c in constraints)
            {
                if (c as IntersectionConstraint != null)
                {
                    PointOnCurve bestPoint = ((IntersectionConstraint)c).OldCurveData;
                    Curve.Curve intersectedCurve = ((IntersectionConstraint)c).IntersectedStroke.Curve;

                    if (bestPoint.t != 0f && bestPoint.t != 1f)
                    {
                        // Search in a small zone around the initial intersection position
                        Vector3 cPos = c.Position;
                        Vector3 dT = search_distance * 0.5f * ((IntersectionConstraint)c).OldCurveData.Tangent;

                        PointOnCurve zoneStart = intersectedCurve.Project(cPos + dT);
                        PointOnCurve zoneEnd = intersectedCurve.Project(cPos - dT);

                        // If the zone is big enough, search in it
                        if (Vector3.Distance(zoneStart.Position, zoneEnd.Position) > search_distance * 0.1f)
                        {
                            float step = (Mathf.Clamp(zoneEnd.t, 0f, 1f) - Mathf.Clamp(zoneStart.t, 0f, 1f)) / N_steps;
                            float minDist = Vector3.Distance(curve.Project(bestPoint.Position).Position, bestPoint.Position);

                            for (int i = 0; i <= N_steps; i++)
                            {
                                PointOnCurve onOldCurve = intersectedCurve.GetPointOnCurve(zoneStart.t + step * i);
                                Vector3 projOnNewCurve = curve.Project(onOldCurve.Position).Position;
                                float dist = Vector3.Distance(projOnNewCurve, onOldCurve.Position);
                                if (dist < minDist)
                                {
                                    minDist = dist;
                                    bestPoint = onOldCurve;
                                }
                            }
                        }
                    }

                    IntersectionConstraint correctedC = ((IntersectionConstraint)c).IntersectedStroke.GetConstraint(bestPoint.Position, parameters.Current.SnapToExistingNodeThreshold);
                    correctedConstraints.Add(correctedC);
                }
                else
                {
                    correctedConstraints.Add(c);
                }
            }

            return correctedConstraints;
        }

    }
}