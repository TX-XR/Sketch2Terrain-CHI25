using System.Linq;
using UnityEngine;

public abstract class Stroke : MonoBehaviour
{
    public bool CanvasSpaceConstantScale = true;
    [HideInInspector]
    public int SubdivisionsPerUnit = 150;
    [HideInInspector]
    public float BaseCurveWidth = 0.0025f;
    ParametersManager parameters;
    private StrokeAppearance strokeAppearance;
    private LineRenderer lineRenderer;


    protected virtual void Awake()
    {
        strokeAppearance = gameObject.GetComponent<StrokeAppearance>();
        lineRenderer = GetComponent<LineRenderer>();
        if (parameters == null)
        {
            var p = FindAnyObjectByType<ParametersManager>();
            if (p != null)
                parameters = p;
            else
                return;
        }
        BaseCurveWidth = parameters.Current.StrokeWidth;
        SubdivisionsPerUnit = parameters.Current.SubdivisionsPerUnit;
    }

    public abstract void RenderAsLine(float scale);
    public abstract void RenderAsLineBySamples(float scale);
    protected void RenderPoints(Vector3[] points, float scale)
    {
        lineRenderer.enabled = true;
        lineRenderer.widthMultiplier = CanvasSpaceConstantScale ? BaseCurveWidth * scale : BaseCurveWidth;
        lineRenderer.positionCount = points.Length;
        lineRenderer.SetPositions(points);
        lineRenderer.alignment = LineAlignment.View;
    }

    public void UpdateWidth(float newScale)
    {
        if (CanvasSpaceConstantScale)
            lineRenderer.widthMultiplier = BaseCurveWidth * newScale;
    }

    public void UpdateCollider(Mesh colliderMesh)
    {
        // Generate collider
        MeshCollider collider = gameObject.GetComponent<MeshCollider>();
        //collider.sharedMesh = null;
        if (colliderMesh.vertices.Distinct().Count() >= 3)
        {
            collider.sharedMesh = null;
            collider.sharedMesh = colliderMesh;
        }
        
    }

    public void OnDeleteSelect()
    {
        strokeAppearance.OnDeleteSelect();
    }

    public void OnDeleteDeselect()
    {
        strokeAppearance.OnDeleteDeselect();
    }


    public virtual void Destroy()
    {
        try
        {
        Destroy(gameObject);
        }
        catch (System.Exception)
        {
            Debug.Log("Stroke Destroy fail");
        }
            
    }
}
