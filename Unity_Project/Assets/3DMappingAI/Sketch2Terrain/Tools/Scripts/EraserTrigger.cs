
using UnityEngine;

namespace MappingAI
{
    public class EraserTrigger : MonoBehaviour
    {
        public Material highlightMaterial;
        public bool CanDelete = false;
        public EraseController eraseController;
        public SelectController selectController;
        private float colliderRadius;
        public float m_MaxSpinSpeed = 300;
        private float m_SpinSpeed;
        private float m_SpinSpeedVel;
        private float m_SpinAmount;
        private bool isHot;
        private MeshRenderer meshRenderer;
        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolHot, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isHot = true; meshRenderer.enabled = true; }  } ));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolCold, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isHot = false; meshRenderer.enabled = false; } }));

        }
        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolHot, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isHot = true; meshRenderer.enabled = true; } }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolCold, (() => { if (ApplicationSettings.Instance.ModeType == ModeType.Sketch) { isHot = false; meshRenderer.enabled = false; } }));
        }

        void Start()
        {
            if (selectController == null)
            {
                selectController = FindAnyObjectByType<SelectController>();
            }

            colliderRadius = GetComponent<SphereCollider>().radius * transform.lossyScale.x;
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
            isHot = false;
        }

        void Update()
        {
            if (isHot)
            {
                UpdatePosition();
            }
        }
        private void UpdatePosition()
        {

            // Calculate rotation amount based on time
            float rotationAmount = m_MaxSpinSpeed * Time.deltaTime;

            // Rotate the ball around its forward axis
            transform.Rotate(transform.forward, rotationAmount, Space.Self);
            //transform.localPosition = Vector3.zero;

        }

        private void OnTriggerEnter(Collider other)
        {
            selectController.OnDeleteCollision(other);
        }

        private void OnTriggerExit(Collider other)
        {
            selectController.OnDeleteCollisionExit(other);
            if (isHot)
            {
                Collider[] collided = Physics.OverlapSphere(transform.position, colliderRadius);
                if (collided.Length > 0)
                {
                    foreach (Collider col in collided)
                    {
                        if (selectController.OnDeleteCollision(col))
                            break;
                    }
                }
                    
            }

        }
    }
}