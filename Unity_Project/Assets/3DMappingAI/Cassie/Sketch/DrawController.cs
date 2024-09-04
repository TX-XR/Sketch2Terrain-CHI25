using UnityEngine;
using Curve;

using System.Collections.Generic;
using Mapbox.Unity.MeshGeneration.Data;
using System.Linq;
using System;

namespace MappingAI
{
    public class DrawController : MonoBehaviour
    {
        [Header("Stroke prefabs")]
        // STROKE
        public GameObject inputStrokePrefab;
        public GameObject textStrokePrefab;
        public GameObject finalStrokePrefab;
        public GameObject _2DFinalStrokePrefab;

        [Header("References")]

        [SerializeField]
        public ParametersManager parameters = null;

        // BEAUTIFIER
        public Beautifier beautifier;

        // SCENE DATA
        public DrawingCanvas canvas;
        //public SurfaceManager surfaceManager;
        public Grid3D grid;
        //public MirrorPlane mirrorPlane;

        // Reference to check for collisions on stroke start
        public BrushCollisions collisionDetector;

        [Header("Parameters")]
        // PARAMETERS (systems)
        public bool Beautification;
        //public bool SnapToTerrainSurface;

        private int subdivisionsPerUnit;
        private float colliderRadius;


        private int finalStrokeID = 0;
        private InputStroke currentStroke = null;
        private InputStroke current2DStroke = null;
        private InputController inputController;
        private bool createText = false;
        //private int currentSelectedPatchID = -1;
        //private void Awake()
        //{
        //    canvas = FindAnyObjectByType<DrawingCanvas>();
        //    collisionDetector = FindAnyObjectByType<BrushCollisions>();
        //}
        private void Start()
        {
            inputController = FindAnyObjectByType<InputController>();
            FinalStroke s = finalStrokePrefab.GetComponent<FinalStroke>();
            subdivisionsPerUnit = Mathf.CeilToInt(s.SubdivisionsPerUnit * 0.5f); // Reduce resolution by half
            colliderRadius = s.BaseCurveWidth * 0.5f; // The collider around the stroke is exactly the same radius as the stroke (BaseCurveWidth gives the diameter)
        }

        public void Init(bool surfacing)
        {
            finalStrokeID = 0;
            canvas.Init(surfacing);
        }

        public void SwitchSystem(bool surfacing)
        {
            // Set parameter
            canvas.SwitchSystem(surfacing);
        }
        public void SetCreateTextFlag(bool flag)
        {
            createText = flag;
        }
        public void NewStroke(Vector3 position)
        {
            GameObject strokeObject;
            if (createText)
            {
                strokeObject = canvas.Create(inputStrokePrefab, Primitive.Stroke);
            }
            else
            {
                strokeObject = canvas.Create(inputStrokePrefab, Primitive.Text);
            }

            currentStroke = strokeObject.GetComponent<InputStroke>();
            strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.CurrentSelectedMaterial.color;
            // Check for stroke start point constraint (other FinalStroke or Grid)
            TryAddGridConstraint(canvas.transform.InverseTransformPoint(position));
            collisionDetector.QueryCollision();
        }

        public void UpdateStroke(Vector3 position, Quaternion rotation, Vector3 velocity, float pressure)
        {
            if (!currentStroke.ShouldUpdate(canvas.transform.InverseTransformPoint(position), parameters.Current.MinSamplingDistance, parameters.Current.MaxSamplingDistance))
                return;

            // Check if current selected patch is still nearby
            //UpdateSelectedPatch(position);
            Vector3 brushNormal = canvas.transform.InverseTransformDirection(rotation * new Vector3(0, 0, 1)); // seems fine
            Vector3 relativePos = canvas.transform.InverseTransformPoint(position);

            Sample s = new Sample(relativePos, brushNormal, pressure, velocity);


            currentStroke.AddSample(s);

            TryAddGridConstraint(relativePos);

            if (currentStroke.Samples.Count > 1)
                RenderStroke(currentStroke); // Draw current stroke
        }

        public void NewStrokeLocal(Vector3 relativePos)
        {
            GameObject strokeObject;
            if (createText)
            {
                strokeObject = canvas.Create(inputStrokePrefab, Primitive.Text);
                currentStroke = strokeObject.GetComponent<InputStroke>();
                currentStroke.UpdateWidth(parameters.Current.TextWidthRatio);
                strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.ColorOnText;
                inputController.ChangeHandAppearance(MaterialManager.Instance.ColorOnText);
            }
            else
            {
                strokeObject = canvas.Create(inputStrokePrefab, Primitive.Stroke);
                currentStroke = strokeObject.GetComponent<InputStroke>();
                strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.CurrentSelectedMaterial.color;
            }
            // Check for stroke start point constraint (other FinalStroke or Grid)
            TryAddGridConstraint(relativePos);
            collisionDetector.QueryCollision();
        }

        public void New2DStrokeLocal()
        {
            GameObject strokeObject;
            if (!createText)
            {
                strokeObject = canvas.Create(inputStrokePrefab, Primitive.Stroke);
                current2DStroke = strokeObject.GetComponent<InputStroke>();
                strokeObject.GetComponent<LineRenderer>().material.color = MaterialManager.Instance.CurrentSelectedMaterial.color;
            }
        }

        public void UpdateStrokeLocal(Vector3 relativePos, Vector3 primaryHandPos, Vector3 brushNormal, Vector3 velocity, float pressure, float tileSize)
        {
            if (!currentStroke.ShouldUpdate(relativePos, parameters.Current.MinSamplingDistance, parameters.Current.MaxSamplingDistance))
                return;
            SnapToTerrain(ref relativePos, primaryHandPos, tileSize);
            Sample s = new Sample(relativePos, brushNormal, pressure, velocity);

            currentStroke.AddSample(s);

            TryAddGridConstraint(relativePos);

            if (currentStroke.Samples.Count > 1)
            {
                if (createText)
                    RenderStroke(currentStroke, parameters.Current.TextWidthRatio);
                else
                    RenderStroke(currentStroke); // Draw current stroke
            }
        }

        public void Update2DStrokeLocal(Vector3 relativePos,Vector3 brushNormal, Vector3 velocity, float pressure)
        {

            if (current2DStroke != null)
                if (!current2DStroke.ShouldUpdate(relativePos, parameters.Current.MinSamplingDistance, parameters.Current.MaxSamplingDistance))
                    return;
            Sample s = new Sample(relativePos, brushNormal, pressure, velocity);

            current2DStroke.AddSample(s);
            TryAddGridConstraint(relativePos);
            if (current2DStroke.Samples.Count > 1)
            {
                RenderStroke(current2DStroke); // Draw current stroke
            }
        }

        private void SnapToTerrain(ref Vector3 drawingPosLocal, Vector3 primaryHandPos, float tileSize)
        {
            //if (!inputController.FreeCreationMode())
            //    return;
            // Only the terrain can snap
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.Experimentation|| MaterialManager.Instance.GetColorProterty() != ColorProperty.Terrain)
                return;
            // only terrain and route color can snap to terrain

            Collider[] overlapped = Physics.OverlapSphere(primaryHandPos, parameters.Current.SnapToTerrainSurfaceThreshold);
            Collider[] terrainObjects = overlapped.Where(obj => obj.gameObject.layer == LayerMask.NameToLayer("TerrainLayer")).ToArray();
            float height = 0;
            if (terrainObjects.Length > 0)
            {
                float distance = float.MaxValue;
                Transform nearestTransform = terrainObjects.First().transform;

                for (int i = 0; i < terrainObjects.Length; i++)
                {
                    Transform transform = terrainObjects[i].transform;
                    if (Vector3.Distance(primaryHandPos, transform.position) < distance)
                    {
                        distance = Vector3.Distance(primaryHandPos, transform.position);
                        nearestTransform = transform;
                    }
                }
                height = GetHeightAtPositionFromTile(drawingPosLocal, nearestTransform.localPosition, nearestTransform.GetComponent<UnityTile>(), tileSize);
                drawingPosLocal = new Vector3(drawingPosLocal.x, height, drawingPosLocal.z);
            }
        }
        // Method to get the height at a specific x and z position
        public float GetHeightAtPositionFromTile(Vector3 localPosition, Vector3 tileLocalPosition, UnityTile unityTile, float tileSize)
        {
            // Normalize the local position to tile coordinates (0 to 1 range)\
            Vector3 locationInOrign = (localPosition - tileLocalPosition);
            float normalizedX = (locationInOrign.x + 0.5f * tileSize) / tileSize;
            float normalizedZ = (locationInOrign.z + 0.5f * tileSize) / tileSize;
            normalizedX = Math.Clamp(normalizedX, 0, 1);
            normalizedZ = Math.Clamp(normalizedZ, 0, 1);
            // Query the height data using normalized coordinates
            float height = unityTile.QueryHeightDataNonclamped(normalizedX, normalizedZ);

            return height + tileLocalPosition.y;
        }
        public bool CommitStroke(out SerializableStroke strokeData, bool mirror = false)
        {
            // Guard against invalid input
            if (!currentStroke.IsValid(parameters.Current.MinStrokeActionTime, parameters.Current.MinStrokeSize))
            {
                // Destroy input stroke
                currentStroke.Destroy();
                strokeData = new SerializableStroke(-1);
                return false;
            }

            // if the color is terrain color, then fit to constraints, otherwise, not.

            // Only the terrain will be beautified
            bool FitToConstraints = MaterialManager.Instance.GetColorProterty() == ColorProperty.Terrain ? Beautification : false;
            (Curve.Curve snappedCurve,
                IntersectionConstraint[] intersections,
                MirrorPlaneConstraint[] mirrorIntersections) =
                beautifier.Beautify(
                currentStroke,
                FitToConstraints,
                mirror,
                out List<SerializableConstraint> appliedConstraints,
                out List<SerializableConstraint> rejectedConstraints,
                out bool planar,
                out bool onSurface,
                out bool onMirror,
                out bool closedLoop);



            if (!snappedCurve.IsValid(parameters.Current.MinStrokeSize))
            {
                // Destroy input stroke
                currentStroke.Destroy();
                strokeData = new SerializableStroke(-1);
                return false;
            }

            if (createText)
            {
                ICommandWithResult<TextStroke> textCommandWithResult = new CommitTextCommand(textStrokePrefab, canvas, snappedCurve, closedLoop, currentStroke.Samples, this, parameters.Current.TextWidthRatio);
                VRCommandInvoker.Instance.ExecuteCommand(textCommandWithResult);
                currentStroke.Destroy();
                currentStroke = null;
                strokeData = new SerializableStroke(-1);
                return false;
            }

            // Trim dangling endpoint bits
            // Consider all intersections to find the first and last one
            // If these intersections are near the stroke endpoint, cut the stroke there
            // And correct each on curve parameter t for other intersections 

            if (intersections.Length > 0 || (mirror && mirrorIntersections.Length > 0))
            {
                TrimDanglingEndpoints(snappedCurve, intersections, mirrorIntersections);
            }

            ICommandWithResult<FinalStroke> commandWithResult = new CommitStrokeCommand(finalStrokePrefab, canvas, finalStrokeID, snappedCurve, closedLoop, currentStroke.GetPoints().ToArray(), intersections, parameters.Current.SnapToExistingNodeThreshold, parameters.Current.MergeConstraintsThreshold, this);
            VRCommandInvoker.Instance.ExecuteCommand(commandWithResult);
            FinalStroke finalStroke = commandWithResult.Result;

            if (current2DStroke != null)
            {
                if (current2DStroke.Samples.Count > 1)
                {
                    ICommandWithResult<_2DFinalStroke> commandWithResult_2D = new Commit2DStrokeCommand(_2DFinalStrokePrefab, canvas, current2DStroke, finalStrokeID, this, inputController.transform.position.y);
                    VRCommandInvoker.Instance.ExecuteCommand(commandWithResult_2D);
                }
                current2DStroke.Destroy();
                current2DStroke = null;
            }

            // Create final stroke game object and render the stroke
            //GameObject strokeObject = canvas.Create(finalStrokePrefab, Primitive.Stroke);

            //FinalStroke finalStroke = strokeObject.GetComponent<FinalStroke>();

            //finalStroke.SetID(finalStrokeID);
            //finalStroke.SetCurve(snappedCurve, closedLoop: closedLoop);
            //finalStroke.SaveInputSamples(currentStroke.GetPoints().ToArray());




            //foreach (var intersection in intersections)
            //{
            //    // OLD STROKE: Create or fetch node and create new segments if needed
            //    //Debug.Log("intersection param = " + intersection.NewCurveData.t.ToString("F6"));
            //    INode node = intersection.IntersectedStroke.AddIntersectionOldStroke(intersection.OldCurveData, parameters.Current.SnapToExistingNodeThreshold);

            //    // NEW STROKE: Insert node and create new segments if needed
            //    finalStroke.AddIntersectionNewStroke(node, intersection.NewCurveData, parameters.Current.MergeConstraintsThreshold);

            //    Debug.Log("[GRAPH UPDATE] added node with " + node.IncidentCount + " neighbors");
            //}


            //// Mirroring
            //if (mirror)
            //{

            //    // Create mirrored final stroke game object
            //    GameObject mirrorStrokeObject = canvas.Create(finalStrokePrefab, Primitive.Stroke);

            //    FinalStroke mirrorFinalStroke = mirrorStrokeObject.GetComponent<FinalStroke>();
            //    mirrorFinalStroke.SetID(finalStrokeID);

            //    // Set up mirror stroke curve and intersections
            //    //bool mirrorSuccess = mirrorPlane.Mirror(
            //    //    finalStroke, ref mirrorFinalStroke,
            //    //    onMirror,
            //    //    intersections, mirrorIntersections,
            //    //    closedLoop,
            //    //    parameters.Current.ProximityThreshold,
            //    //    parameters.Current.SnapToExistingNodeThreshold,
            //    //    parameters.Current.MergeConstraintsThreshold,
            //    //    prevent_extra_mirroring: !Beautification);

            //    //if (mirrorSuccess)
            //    //{
            //    //    canvas.Add(mirrorFinalStroke);

            //    //    RenderStroke(mirrorFinalStroke);

            //    //    // Generate collider mesh
            //    //    SolidifyStroke(mirrorFinalStroke);

            //    //    finalStrokeID++;
            //    //}
            //    //else
            //    //{
            //    //    // Abort
            //    //    mirrorFinalStroke.Destroy();
            //    //}
            //}

            //finalStroke.TrimDanglingSegments();

            //canvas.Add(finalStroke);
            // Generate collider mesh
            //SolidifyStroke(finalStroke);

            

            // Record stroke data
            strokeData = new SerializableStroke(
                finalStroke.ID,
                finalStroke.GetControlPoints(),
                currentStroke.GetPoints(parameters.Current.ExportRDPError),
            appliedConstraints,
                rejectedConstraints,
                onSurface,
                planar,
                closedLoop
                );
            // Destroy input stroke
            currentStroke.Destroy();

            currentStroke = null;
            finalStrokeID++;
            // Stop holding on to patch
            //DeselectPatch(position);

            return true; // success in creating final stroke
        }

        public void RenderStroke(Stroke s)
        {
            s.RenderAsLine(canvas.transform.localScale.x);
        }

        public void RenderStrokeBySamples(Stroke s)
        {
            s.RenderAsLineBySamples(canvas.transform.localScale.x);
        }
        public void RenderStroke(Stroke s, float scale)
        {
            s.RenderAsLine(scale * canvas.transform.localScale.x);
        }
        // Called only when stroke is done drawing, to generate its collider
        public Mesh SolidifyStroke(FinalStroke s)
        {
            //Mesh strokeMesh = brush.Solidify(s.Curve, true);
            int tubularSegments = Mathf.CeilToInt(s.Curve.GetLength() * subdivisionsPerUnit);

            Mesh strokeMesh = Tubular.Tubular.Build(s.Curve, tubularSegments, colliderRadius);

            s.UpdateCollider(strokeMesh);
            return strokeMesh;
        }
        public Mesh SolidifyStroke(_2DFinalStroke s)
        {
            //Mesh strokeMesh = brush.Solidify(s.Curve, true);
            int tubularSegments = Mathf.CeilToInt(s.Curve.GetLength() * subdivisionsPerUnit);

            Mesh strokeMesh = Tubular.Tubular.Build(s.Curve, tubularSegments, colliderRadius);

            s.UpdateCollider(strokeMesh);
            return strokeMesh;
        }
        public Mesh SolidifyStrokeBySample(FinalStroke s)
        {
            //Mesh strokeMesh = brush.Solidify(s.Curve, true);
            int tubularSegments = Mathf.CeilToInt(s.Curve.GetLength() * subdivisionsPerUnit);

            Mesh strokeMesh = Tubular.Tubular.Build(s.Curve, tubularSegments, colliderRadius);

            s.UpdateCollider(strokeMesh);
            return strokeMesh;
        }

        public Mesh SolidifyStroke(TextStroke s, float ratio)
        {
            //Mesh strokeMesh = brush.Solidify(s.Curve, true);
            int tubularSegments = Mathf.CeilToInt(s.Curve.GetLength() * subdivisionsPerUnit);

            Mesh strokeMesh = Tubular.Tubular.Build(s.Curve, tubularSegments, colliderRadius * ratio);

            s.UpdateCollider(strokeMesh);
            return strokeMesh;
        }

        public bool AddConstraint(Vector3 collisionPos, GameObject collided)
        {
            // If is drawing, create a new constraint and add it to the current stroke
            if (currentStroke)
            {
                // Find constraint type and exact position
                Constraint constraint;
                Vector3 relativePos = canvas.transform.InverseTransformPoint(collisionPos); // position of collision in canvas space

                //Vector3 relativePos = collisionPos; // position of collision in canvas space

                switch (collided.tag)
                {
                    case "FinalStroke":
                        FinalStroke stroke = collided.GetComponent<FinalStroke>();
                        // Check if the curve can serve to create constraints
                        if (stroke != null)
                        {
                            constraint = stroke.GetConstraint(relativePos, parameters.Current.SnapToExistingNodeThreshold);
                        }
                        // Otherwise give up this constraint
                        else
                        {
                            constraint = new Constraint(relativePos);
                        }
                        break;
                    //case "MirrorPlane":
                    //    //Debug.Log("[Mirror] collided with plane");
                    //    Vector3 onPlanePos = mirrorPlane.Project(relativePos);
                    //    constraint = new MirrorPlaneConstraint(onPlanePos, mirrorPlane.GetNormal());
                    //    break;
                    default:
                        //constraint = new Constraint(relativePos);
                        constraint = null;
                        break;
                }

                if (constraint != null)
                {
                    currentStroke.AddConstraint(constraint, parameters.Current.MergeConstraintsThreshold);

                    // Constraint was successfully added
                    return true;
                }
            }

            // Didn't add constraint
            return false;
        }

        //public void OnPatchCollide(int patchID, Vector3 pos)
        //{
        //    if (currentSelectedPatchID != patchID)
        //    {
        //        if (currentSelectedPatchID != -1)
        //        {
        //            // Check whether we should select new patch or keep old one
        //            if (
        //                !surfaceManager.ProjectOnPatch(patchID, pos, out Vector3 posOnPatch, canvasSpace: true)
        //                || Vector3.Distance(pos, posOnPatch) > parameters.Current.ProximityThreshold)
        //            {
        //                currentSelectedPatchID = patchID;

        //                // Add constraint of surface ingoing point, if currently drawing
        //                if (currentStroke)
        //                {
        //                    AddInSurfaceConstraint(currentSelectedPatchID, pos);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            currentSelectedPatchID = patchID;

        //            // Add constraint of surface ingoing point, if currently drawing
        //            if (currentStroke)
        //            {
        //                AddInSurfaceConstraint(currentSelectedPatchID, pos);
        //            }
        //        }

        //    }

        //}

        //public void OnPatchDeselect(Vector3 pos)
        //{
        //    if (currentStroke)
        //    {
        //        // If we're currently drawing, prevent patch deselect based on pure collision events
        //        // we want to keep drawing projected on the same patch as we started

        //    }
        //    else
        //    {
        //        DeselectPatch(pos);
        //    }
        //}

        //private void DeselectPatch(Vector3 pos)
        //{
        //    // Add constraint of surface outgoing point to draw controller
        //    if (currentStroke)
        //    {
        //        AddOutSurfaceConstraint(currentSelectedPatchID, pos);
        //    }
        //    else
        //        surfaceManager.OnDetailDrawStop(currentSelectedPatchID); // still send stop event to surface patch (to have correct appearance)
        //    currentSelectedPatchID = -1;
        //}

        //private bool AddInSurfaceConstraint(int patchID, Vector3 onStrokePos)
        //{
        //    if (currentStroke)
        //    {
        //        // Check whether we are actually still close to the patch
        //        if (
        //            !surfaceManager.ProjectOnPatch(patchID, onStrokePos, out Vector3 posOnPatch, canvasSpace: false)
        //            || Vector3.Distance(onStrokePos, posOnPatch) > parameters.Current.ProximityThreshold)
        //        {
        //            //Debug.Log("went too far from patch");
        //            currentSelectedPatchID = -1;
        //            return false;
        //        }
        //        //Debug.Log("add constraint to patch " + patchID);
        //        surfaceManager.OnDetailDrawStart(patchID);
        //        currentStroke.InConstrainToSurface(patchID, canvas.transform.InverseTransformPoint(onStrokePos));
        //        return true;
        //    }
        //    return false;
        //}

        //private bool AddOutSurfaceConstraint(int patchID, Vector3 onStrokePos)
        //{
        //    if (currentStroke)
        //    {
        //        surfaceManager.OnDetailDrawStop(patchID);
        //        currentStroke.OutConstrainToSurface(patchID, canvas.transform.InverseTransformPoint(onStrokePos));
        //        return true;
        //    }
        //    return false;
        //}

        //private bool GetProjectedPos(Vector3 brushPos, out Vector3 projectedPos, float maxDist)
        //{
        //    projectedPos = brushPos;

        //    if (currentSelectedPatchID == -1)
        //        return false;

        //    if (surfaceManager.ProjectOnPatch(currentSelectedPatchID, brushPos, out projectedPos))
        //    {
        //        if (Vector3.Distance(projectedPos, brushPos) < maxDist)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}

        //private void UpdateSelectedPatch(Vector3 brushPos)
        //{

        //    if (currentSelectedPatchID != -1)
        //    {
        //        // A patch is currently selected,
        //        // Check whether it should still be selected,
        //        // otherwise, explicitly deselect it

        //        Vector3 projectedPos;
        //        if (!GetProjectedPos(brushPos, out projectedPos, maxDist: parameters.Current.ProximityThreshold))
        //        {
        //            //if (currentStroke)
        //            //    Debug.Log("got too far from patch while drawing");
        //            DeselectPatch(brushPos);
        //        }
        //    }
        //}

        private bool TryAddGridConstraint(Vector3 pos)
        {
            if (grid.TryFindConstraint(pos, parameters.Current.ProximityThreshold, out Vector3 gridPointPos))
            {
                // Check for potential stroke/stroke intersection
                Collider[] overlapped = Physics.OverlapSphere(canvas.transform.TransformPoint(gridPointPos), parameters.Current.ProximityThreshold);
                Constraint constraint = null;
                Collider prioritary = null;

                // If overlap with other FinalStroke, add stroke constraint
                foreach (var obj in overlapped)
                {
                    if (obj.CompareTag("FinalStroke") && obj.GetComponent<FinalStroke>() != null)
                    {
                        // Add stroke/stroke constraint insteand
                        constraint = obj.GetComponent<FinalStroke>().GetConstraint(pos, parameters.Current.SnapToExistingNodeThreshold);
                        prioritary = obj;
                    }
                    // Second prioritary is mirror plane
                    //if (obj.CompareTag("MirrorPlane") && prioritary == null)
                    //{
                    //    prioritary = obj;
                    //    // Add stroke/mirror constraint insteand
                    //    constraint = new MirrorPlaneConstraint(gridPointPos, mirrorPlane.GetNormal());
                    //}
                }

                // Otherwise, add grid constraint
                if (constraint == null)
                    constraint = new Constraint(gridPointPos); // Grid point pos is in canvas space

                currentStroke.AddConstraint(constraint, parameters.Current.MergeConstraintsThreshold);
                //Debug.Log("found grid constraint at " + pos.ToString("F3"));
                return true;
            }
            else
                return false;
        }

        private void TrimDanglingEndpoints(Curve.Curve curve, IntersectionConstraint[] intersections, MirrorPlaneConstraint[] mirrorIntersections)
        {
            Constraint firstIntersection;
            Constraint lastIntersection;

            // Initialize intersections
            if (intersections.Length > 0)
            {
                firstIntersection = intersections[0];
                lastIntersection = intersections[0];
            }
            else if (mirrorIntersections.Length > 0)
            {
                firstIntersection = mirrorIntersections[0];
                lastIntersection = mirrorIntersections[0];
            }
            else
            {
                // No intersections => won't trim endpoints
                return;
            }

            foreach (var intersection in intersections)
            {
                if (intersection.NewCurveData.t < firstIntersection.NewCurveData.t)
                {
                    firstIntersection = intersection;
                }

                else if (intersection.NewCurveData.t > lastIntersection.NewCurveData.t)
                {
                    lastIntersection = intersection;
                }

            }

            foreach (var intersection in mirrorIntersections)
            {
                if (intersection.NewCurveData.t < firstIntersection.NewCurveData.t)
                {
                    firstIntersection = intersection;
                }

                else if (intersection.NewCurveData.t > lastIntersection.NewCurveData.t)
                {
                    lastIntersection = intersection;
                }
            }

            float firstIntersectionParam = firstIntersection.NewCurveData.t;

            // Total length
            float length = curve.GetLength();

            // First segment length
            float firstSegmentLength = curve.LengthBetween(0f, firstIntersectionParam);

            // Cut if first segment is sufficiently small (yet still exists)
            if (firstSegmentLength > Constants.eps && firstSegmentLength < parameters.Current.MaxHookSectionStrokeRatio * length && firstSegmentLength < parameters.Current.MaxHookSectionLength)
            {
                //Debug.Log("cutting stroke at t = " + firstIntersectionParam);
                Reparameterization? r = curve.CutAt(firstIntersectionParam, throwBefore: true, snapToExistingAnchorThreshold: parameters.Current.SmallDistance * 0.1f);

                foreach (var intersection in intersections)
                {
                    intersection.ReparameterizeNewCurve(r, curve);
                }

                foreach (var intersection in mirrorIntersections)
                {
                    intersection.ReparameterizeNewCurve(r, curve);
                }

            }

            float lastIntersectionParam = lastIntersection.NewCurveData.t;

            length = curve.GetLength();
            float lastSegmentLength = curve.LengthBetween(lastIntersectionParam, 1f);

            // Cut if last segment is sufficiently small (yet still exists)
            if (lastSegmentLength > Constants.eps && lastSegmentLength < parameters.Current.MaxHookSectionStrokeRatio * length && lastSegmentLength < parameters.Current.MaxHookSectionLength)
            {
                //Debug.Log("cutting stroke at t = " + lastIntersectionParam);
                Reparameterization? r = curve.CutAt(lastIntersectionParam, throwBefore: false, snapToExistingAnchorThreshold: parameters.Current.SmallDistance * 0.1f);

                foreach (var intersection in intersections)
                {
                    intersection.ReparameterizeNewCurve(r, curve);
                }


                foreach (var intersection in mirrorIntersections)
                {
                    intersection.ReparameterizeNewCurve(r, curve);
                }
            }
        }

    }


}
