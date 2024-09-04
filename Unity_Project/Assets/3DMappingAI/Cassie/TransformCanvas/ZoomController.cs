using UnityEngine;


/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */
public class ZoomController : MonoBehaviour
{
    public float MaxScale = 3f;
    public float MinScale = 1f;
    private float startHandsDistance;
    private float startScale;
    public DrawingCanvas canvas;

    public void StartZoom(float handsDistance)
    {
        startHandsDistance = handsDistance;
        if (canvas == null)
        {
            canvas = GetComponent<DrawingCanvas>();
            if (!canvas)
                return;
        }
        startScale = canvas.transform.localScale.x;
    }

    public bool UpdateZoom(Vector3 zoomCenter, float currentHandsDistance, out float newScale)
    {
        newScale = startScale * currentHandsDistance / startHandsDistance;
        if (newScale <= MinScale)
        {
            newScale = MinScale;
            return false;
        }
        if (newScale >= MaxScale)
        {
            newScale = MaxScale;
            return false;
        }
        if (newScale > 0)
            canvas.Scale(newScale, zoomCenter);
        return true;

    }

    public void ResetScale()
    {
        canvas.Scale(1f, Vector3.zero);
    }
}
