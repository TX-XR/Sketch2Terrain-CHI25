using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MappingAI
{
    public class MeshRenderManager : MonoBehaviour
    {
        public static void DisableChildMeshRenderers(Transform parent)
        {
            // Disable MeshRenderer on the current GameObject if it has one
            MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            // Iterate through all child GameObjects and call the same function recursively
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                DisableChildMeshRenderers(child);
            }
        }

        public static void EnableChildMeshRenderers(Transform parent)
        {
            // Disable MeshRenderer on the current GameObject if it has one
            MeshRenderer meshRenderer = parent.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            // Iterate through all child GameObjects and call the same function recursively
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                EnableChildMeshRenderers(child);
            }
        }

        public static void EnableChildLineRenderers(Transform parent)
        {
            // Disable MeshRenderer on the current GameObject if it has one
            LineRenderer meshRenderer = parent.GetComponent<LineRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = true;
            }

            // Iterate through all child GameObjects and call the same function recursively
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                EnableChildLineRenderers(child);
            }
        }

        public static void DisableChildLineRenderers(Transform parent)
        {
            // Disable MeshRenderer on the current GameObject if it has one
            LineRenderer meshRenderer = parent.GetComponent<LineRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.enabled = false;
            }

            // Iterate through all child GameObjects and call the same function recursively
            for (int i = 0; i < parent.childCount; i++)
            {
                Transform child = parent.GetChild(i);
                DisableChildLineRenderers(child);
            }
        }
    }
}
