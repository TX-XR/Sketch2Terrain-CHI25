using UnityEngine;

/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */

namespace MappingAI
{
    public class GrabController : MonoBehaviour
    {
        private Vector3 startPosition;
        private Quaternion startRotation;
        private Vector3 positionOffset;
        private Quaternion q0;
        private Quaternion qObj;
        public GameObject rotationArrowLeft2Right;
        public GameObject rotationArrowRight2Left;
        public float rotationSpeed = 2f;
        private DrawingCanvas canvas;
        private bool CanRotate = false;
        private InputController inputController;



        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.RotationToolHot, () => { CanRotate = true; });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.RotationToolCold, () => { CanRotate = false; });
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.RotationToolHot, () => { CanRotate = true; });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.RotationToolCold, () => { CanRotate = false; });
        }

        private void Awake()
        {
            canvas = FindAnyObjectByType<DrawingCanvas>();
            CanRotate = false;
            inputController = FindAnyObjectByType<InputController>();
            OnGrabEnd();
        }

        public void GrabStart(Vector3 handPos, Quaternion handRot)
        {
            startPosition = handPos;
            startRotation = handRot;

            positionOffset = canvas.transform.position - startPosition;
            q0 = Quaternion.Inverse(startRotation);
            qObj = canvas.transform.rotation;
        }
        public void GrabUpdate(Vector3 handPos, Quaternion handRot)
        {
            if (Vector3.Distance(handPos, startPosition) > 0.01f || Quaternion.Angle(startRotation, handRot) > 0.1f)
            {
                Vector3 endPosition = handPos;
                Quaternion q1 = handRot;

                canvas.transform.position = endPosition;
                canvas.transform.rotation = q1 * q0 * qObj;
                Vector3 correctedOffset = q1 * q0 * positionOffset;
                canvas.transform.position += correctedOffset;
                //canvas.transform.position += Vector3.ProjectOnPlane(correctedOffset, Vector3.up);
            }
        }

        public void OnGrabEnd()
        {
            MeshRenderManager.DisableChildMeshRenderers(rotationArrowRight2Left.transform);
            MeshRenderManager.DisableChildMeshRenderers(rotationArrowLeft2Right.transform);
        }
        public void GrabUpdateConstraint(Vector3 handPos, Quaternion handRot)
        {
            if (Vector3.Distance(handPos, startPosition) > 0.01f || Quaternion.Angle(startRotation, handRot) > 0.1f)
            {
                // Calculate rotation angle based on the change in y-coordinate
                if (CanRotate)
                {
                    float deltaY = handPos.x - startPosition.x;

                    Mathf.Clamp(deltaY, -0.2f, 0.2f);
                    if (deltaY < -0.1)
                    {
                        MeshRenderManager.EnableChildMeshRenderers(rotationArrowRight2Left.transform);
                        MeshRenderManager.DisableChildMeshRenderers(rotationArrowLeft2Right.transform);
                    }
                    else if (deltaY > 0.1)
                    {
                        MeshRenderManager.DisableChildMeshRenderers(rotationArrowRight2Left.transform);
                        MeshRenderManager.EnableChildMeshRenderers(rotationArrowLeft2Right.transform);
                    }
                    else
                    {
                        MeshRenderManager.DisableChildMeshRenderers(rotationArrowRight2Left.transform);
                        MeshRenderManager.DisableChildMeshRenderers(rotationArrowLeft2Right.transform);
                    }


                    float rotationAngle = deltaY * rotationSpeed; // Adjust rotation speed if necessary

                    // Apply rotation around the model's center
                    canvas.transform.RotateAround(canvas.transform.position, Vector3.up, -rotationAngle);
                }
                else
                {
                    // Calculate position difference and project onto x-z plane
                    Vector3 positionDifference = handPos - startPosition;
                    //Vector3 projectedPositionDifference = Vector3.ProjectOnPlane(positionDifference, Vector3.up);

                    // Update object's position on the x-z plane
                    Vector3 newPosition = startPosition + positionOffset;
                    newPosition.y += positionDifference.y; // only adjust y

                    if (Vector3.Distance(newPosition, inputController.transform.position) < 0.05)
                    {
                        newPosition.y = inputController.transform.position.y;
                    }
                    canvas.transform.position = newPosition;
                }
            }
        }
    }


}
