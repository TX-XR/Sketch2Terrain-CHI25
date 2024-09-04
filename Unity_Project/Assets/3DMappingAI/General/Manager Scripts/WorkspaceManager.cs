
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MappingAI
{
    /// <summary>
    /// The worksapce manager is used for visualizing the boundary of the workspace and de-visualize the upper body in 2D condition.
    /// </summary>
    public class WorkspaceManager : MonoBehaviour
    {
        [SerializeField]
        private GameObject workspace;
        [SerializeField]
        private GameObject workspaceBottomPlane;
        [SerializeField]
        private GameObject workspaceUpperBody;
        [SerializeField]
        private GameObject workspaceLowerBody;

        private bool isUpperBodyActive = true;

        private static WorkspaceManager _instance;

        public static WorkspaceManager Instance
        {
            get
            {
                return _instance;
            }
            set { _instance = value; }
        }
        List<LineRenderer> lineRenderers;
        Color originalColor;

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchRangeIn, SketchRangeIn);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.SketchRangeOut, SketchRangeOut);
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchRangeIn, SketchRangeIn);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.SketchRangeOut, SketchRangeOut);
        }
        void Start()
        {
            _instance = this;
            isUpperBodyActive = false;
            lineRenderers = workspace.GetComponentsInChildren<LineRenderer>().ToList();
            originalColor = lineRenderers[0].material.color;
            if (ApplicationSettings.Instance.DevelopmentMode != DevelopmentMode.DataCollection)
            {
                workspaceBottomPlane.SetActive(false);
                workspaceUpperBody.SetActive(false);
                workspaceLowerBody.SetActive(false);
            }
        }

        public void EnableUpperBody(bool showBottomPlane = false)
        {
            if (!isUpperBodyActive)
            {
                isUpperBodyActive = true;
                workspaceUpperBody.SetActive(true);

            }
            ActivateBottomPlane(showBottomPlane);
        }
        public void EnableLowerBody(bool flag)
        {
            workspaceLowerBody.SetActive(flag);
        }
        public void DisableUpperBody(bool showBottomPlane = true)
        {
            if (isUpperBodyActive)
            {
                isUpperBodyActive = false;
                workspaceUpperBody.SetActive(false);
                ActivateBottomPlane(showBottomPlane);
            }
        }

        public void ActivateBottomPlane(bool showBottomPlane)
        {
            workspaceBottomPlane.SetActive(showBottomPlane);
        }

        public float GetBottomPlaneHeight()
        {
            return workspaceBottomPlane.transform.position.y;
        }
        void SketchRangeIn()
        {
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.material.color = originalColor;
            }
        }

        void SketchRangeOut()
        {
            foreach (LineRenderer lineRenderer in lineRenderers)
            {
                lineRenderer.material.color = new Color(169f / 255f, 141f / 255f, 168f / 255f);
            }
        }
    }

}
