using UnityEngine;
using VRSketch;
/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */

namespace MappingAI
{
    public class EraseController : MonoBehaviour
    {
        private DrawingCanvas canvas;
        private void Start()
        {
            canvas = ComponentManager.Instance.GetDrawingCanvas();
        }
        public void Clear()
        {
            canvas.Clear();
        }

        public bool TryDelete(out InteractionType interactionType, out int elementID, bool mirror = false)
        {
            return canvas.DeleteSelected(out interactionType, out elementID, mirror);
        }
    }

}
