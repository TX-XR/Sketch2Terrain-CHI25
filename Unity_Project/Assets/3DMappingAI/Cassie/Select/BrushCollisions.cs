using UnityEngine;
using System.Collections.Generic;
using System.Linq;


// Responsible for sending constraint events to DrawController
// - position constraints (on collision with strokes and grid points)
// - on surface constraints

// Responsible for updating selected stroke and selected patch of Drawing Canvas
// those correspond to the candidates to be deleted, in case of delete button click

namespace MappingAI
{
    public class BrushCollisions : MonoBehaviour
    {
        //[SerializeField]
        public ParametersManager parameters = null;

        public SelectController selectController;
        public DrawController drawController;

        private Collider Collided = null;

        private List<Collider> collidedQueue = new List<Collider>();

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SpatialAnchorloaded, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SurfaceCalibrationInProgress, surfaceCalibrationInProgress);
        }
        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationCompleted, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SpatialAnchorloaded, surfaceCalibrationCompleted);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SurfaceCalibrationInProgress, surfaceCalibrationInProgress);
        }

        private void surfaceCalibrationInProgress()
        {
            parameters = null;
            drawController = null;
        }

        public void surfaceCalibrationCompleted()
        {
            if (parameters == null)
            {
                parameters = FindAnyObjectByType<ParametersManager>();
                drawController = FindAnyObjectByType<DrawController>();
            }

            //SphereCollider sphereCollider = GetComponent<SphereCollider>();
            //// true radius = transform.lossyScale * sphereCollider.radius
            //// => sphereCollider.radius = desiredRadius / transform.lossyScale;
            //sphereCollider.radius = desiredRadius / transform.localScale.x;
        }

        public void QueryCollision()
        {
            SendConstraint();
        }

        private void LateUpdate()
        {
            // Make sure everything in queue is still existing in scene
            // This sucks but it works
            collidedQueue = collidedQueue.Where(obj => obj != null).ToList();


            //if (drawController == null)
            //{
            //    var tryToFind = FindAnyObjectByType<DrawController>();
            //    if (tryToFind != null)
            //    {
            //        drawController = tryToFind;
            //        parameters = FindAnyObjectByType<CASSIEParametersProvider>();
            //    }
            //}
        }

        private void OnTriggerEnter(Collider other)
        {
            // If the brush is already colliding something
            // Add that to the queue

            // Ignore inner collider
            if (other.CompareTag("BrushCollider"))
                return;

            if (other.CompareTag("SketchRangeCollider"))
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SketchRangeIn);

            if (Collided)
                collidedQueue.Add(Collided);
            // Trigger change of main collided object
            OnCollidedChange(other);

            //Debug.Log("entering " + other.tag);
        }

        private void OnTriggerExit(Collider other)
        {

            // If brush exited the current main collided object
            // Attempt to fetch the next collided object from queue

            // Ignore inner collider
            if (other.CompareTag("BrushCollider"))
                return;

            if (other.CompareTag("SketchRangeCollider"))
                Sketch2TerrainEventManager.TriggerEvent(Sketch2TerrainEventManager.SketchRangeOut);
            //Debug.Log("leaving " + other.tag);

            // Notify of brush leaving patch
            //if (other.GetComponent<SurfacePatch>())
            //    drawController.OnPatchDeselect(gameObject.transform.position);

            if (other == Collided)
            {
                Collided = null;

                int i = 0;
                while (collidedQueue.Count > i)
                {
                    // Check if this object still exists
                    if (collidedQueue[i])
                    {
                        OnCollidedChange(collidedQueue[i]);
                        collidedQueue.RemoveAt(i);
                        break;
                    }
                    collidedQueue.RemoveAt(i);
                    i++;
                }

            }
            // Remove this object from queue as we are not colliding it anymore
            else
                collidedQueue.Remove(other);


        }

        private void OnCollidedChange(Collider newCollided)
        {
            //Debug.Log("main collided is now " + newCollided.tag);
            Collided = newCollided;
            SendConstraint();


            // Try to add a constraint for this collision point


            // Notify canvas of patch collision, to add patch constraint if needed
            //SurfacePatch patch = newCollided.GetComponent<SurfacePatch>();
            //if (patch != null)
            //{
            //    drawController.OnPatchCollide(patch.GetID(), gameObject.transform.position);
            //    return;
            //}

        }

        private void SendConstraint()
        {
            Collider objectCollided = GetPrioritaryCollider();

            // Try to add a constraint for this collision point
            if (objectCollided != null && drawController != null)
            {
                Vector3 position = transform.position;
                drawController.AddConstraint(position, objectCollided.gameObject);
            }
                
        }

        private Collider GetPrioritaryCollider()
        {
            if (Collided == null)
                return null;

            return Collided;
        }

    }
}
