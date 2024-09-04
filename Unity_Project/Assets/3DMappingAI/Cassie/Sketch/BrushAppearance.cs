using MappingAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRSketch;

public class BrushAppearance : MonoBehaviour
{
    public GameObject DotGizmoPrefab;
    public Color GrabColor = Color.yellow;
    public Color EraseColor = Color.yellow;
    public Color ZoomColor = Color.yellow;
    public Color NoOpColor = Color.red;
    public Color FreehandColor = Color.green;
    public Color CASSIEColor = Color.blue;

    private TrailRenderer trail;
    private Material material;
    private Color currentDrawingColor;

    IEnumerator FadeNoOp()
    {
        for (float ft = 0f; ft <= 1f; ft += 0.01f)
        {
            Color c = Color.Lerp(NoOpColor, currentDrawingColor, ft);
            material.color = c;

            if (ft == 1f)
                trail.enabled = true;
            yield return null;
        }
    }


    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        GetComponent<MeshRenderer>().material = MaterialManager.Instance.CurrentSelectedMaterial;
        material = GetComponent<MeshRenderer>().material;   
        currentDrawingColor = MaterialManager.Instance.CurrentSelectedMaterial.color;
        material.color = currentDrawingColor;
        //trail.material = MaterialManager.Instance.CurrentSelectedMaterial;
        trail.startColor = currentDrawingColor;
        trail.endColor = Color.white;
    }

    public void OnDrawStart()
    {
        //material.color = currentDrawingColor;
        currentDrawingColor = MaterialManager.Instance.CurrentSelectedMaterial.color;
        material.color = currentDrawingColor;

        trail.enabled = false;
    }

    public void OnDrawEnd()
    {
        trail.startColor = currentDrawingColor;
        trail.enabled = true;
    }

    public void OnGrabStart()
    {
        trail.enabled = false;
        material.color = GrabColor;
    }

    public void OnGrabEnd()
    {
        trail.enabled = true;
        material.color = currentDrawingColor;
    }
    public void OnEraseStart()
    {
        trail.enabled = false;
        material.color = EraseColor;
    }

    public void OnEraseEnd()
    {
        trail.enabled = true;
        material.color = currentDrawingColor;
    }
    public void OnZoomStart()
    {
        trail.enabled = false;
        material.color = ZoomColor;
    }
    public void OnZoomEnd()
    {
        trail.enabled = true;
        material.color = currentDrawingColor;
    }

    public void OnNoOp()
    {
        trail.enabled = false;
        StartCoroutine("FadeNoOp");
    }

    public void SetColor(Color color)
    {
        material.color = color;
        //trail.material = MaterialManager.Instance.CurrentSelectedMaterial;
        trail.startColor = color;
        trail.endColor = Color.white;
    }
    public void OnModeChange(SketchSystem mode)
    {
        if (mode.Equals(SketchSystem.Baseline))
        {
            material.color = FreehandColor;
            trail.material.color = FreehandColor;
            currentDrawingColor = FreehandColor;
        }
        else
        {
            material.color = CASSIEColor;
            trail.material.color = CASSIEColor;
            currentDrawingColor = CASSIEColor;
        }
    }
}
