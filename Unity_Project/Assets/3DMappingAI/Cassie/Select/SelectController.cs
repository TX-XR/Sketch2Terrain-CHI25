using UnityEngine;
/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */

namespace MappingAI
{
    public class SelectController : MonoBehaviour
    {
        private DrawingCanvas canvas;
        [SerializeField] AudioSource collideAudio;
        bool isEarserHot = false;
        private void Start()
        {
            if (canvas == null)
                canvas = ComponentManager.Instance.GetDrawingCanvas();
        }

        private void OnEnable()
        {
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolHot, (() => { isEarserHot = true; }));
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolCold, (() => { isEarserHot = false; }));
        }

        private void OnDisable()
        {
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolHot, (() => { isEarserHot = true; }));
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolCold, (() => { isEarserHot = false; }));
        }
        public bool OnDeleteCollision(Collider collided)
        {
            bool flag = true;
            if (collided.GetComponent<FinalStroke>() != null)
            {
                canvas.UpdateToDelete(collided.GetComponent<FinalStroke>());
                if (!isEarserHot)
                    collideAudio?.Play();
                return flag;
            }


            if (collided.GetComponent<TextStroke>() != null)
            {
                canvas.UpdateToDelete(collided.GetComponent<TextStroke>());
                return flag;
            }

            if (collided.GetComponent<_2DFinalStroke>() != null)
            {
                canvas.UpdateToDelete(collided.GetComponent<_2DFinalStroke>());
                return flag;
            }
            return false;
        }

        public void OnDeleteCollisionExit(Collider collided)
        {
            if (collided.GetComponent<FinalStroke>() != null)
                canvas.ClearToDelete(collided.GetComponent<FinalStroke>());
            if (collided.GetComponent<TextStroke>() != null)
                canvas.ClearToDelete(collided.GetComponent<TextStroke>());
            if (collided.GetComponent<_2DFinalStroke>() != null)
                canvas.ClearToDelete(collided.GetComponent<_2DFinalStroke>());
        }
    }
}

