using MappingAI;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using VRSketch;
/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */
public enum Primitive
{
    Stroke,
    Surface,
    Text,
    _2DStroke
}

public class DrawingCanvas : MonoBehaviour
{
    [SerializeField]
    private ParametersManager parameters = null;

    // Display nodes
    [Header("Nodes display parameters")]
    public float NodesOpacity = 0.9f;
    [Tooltip("Value in pixels (so it may need to be changed depending on your resolution).")]
    public float NodesRadius = 10;

    [SerializeField] AudioSource selectedSource = null;

    // The strokes for 3D sketching condition
    public List<FinalStroke> TerrainStrokes { get; private set; }
    public List<FinalStroke> LandmarkStrokes { get; private set; }
    // The strokes for 2D sketching condition
    public List<_2DFinalStroke> _2DStrokes { get; private set; }
    public List<GameObject> AIGeneratedMesh { get; private set; }
    // The strokes for text
    public List<Stroke> TextStrokes { get; private set; }
    public Graph Graph { get; private set; } = new Graph();

    private Stroke selectedStroke = null;

    private GameObject StrokeContainer;
    private GameObject _2DStrokeContainer;
    private GameObject TextContainer;
    private Grid3D grid;

    // Display nodes stuff
    [Header("Node Display Shader")]
    public Shader shader;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Material material;

    // Shader uniforms indices
    private int _PointRadiusIdx;
    private int _ColorIdx;
    private int _OpacityIdx;
    private float pointRadius;

    private DrawController drawController;
    public Vector3[] OrthoDirections { get; private set; }

    private void Start()
    {
        TerrainStrokes = new List<FinalStroke>();
        LandmarkStrokes = new List<FinalStroke>();
        _2DStrokes = new List<_2DFinalStroke>();
        AIGeneratedMesh = new List<GameObject>();
        TextStrokes = new List<Stroke>();
        OrthoDirections = new Vector3[] { Vector3.right, Vector3.up, Vector3.forward };
        grid = GetComponentInChildren<Grid3D>();
        StrokeContainer = transform.Find("StrokeContainer").gameObject;
        TextContainer = transform.Find("TextContainer").gameObject;
        _2DStrokeContainer = transform.Find("2DStrokeContainer").gameObject;
        StrokeContainer.layer = LayerMask.NameToLayer("StrokeLayer");

        transform.localScale = Vector3.one;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        // Display nodes stuff
        material = new Material(shader);
        meshRenderer = GetComponent<MeshRenderer>();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = new Mesh();
        meshRenderer.material = material;

        pointRadius = NodesRadius;


        // Shader uniform indices
        _PointRadiusIdx = Shader.PropertyToID("_PointRadius");
        _ColorIdx = Shader.PropertyToID("_Color");
        _OpacityIdx = Shader.PropertyToID("_BaseOpacity");

        material.SetFloat(_PointRadiusIdx, pointRadius);

        material.SetColor(_ColorIdx, Color.red);
        material.SetFloat(_OpacityIdx, NodesOpacity);

        drawController = FindAnyObjectByType<DrawController>();
        if (parameters == null)
        {
            var p = FindAnyObjectByType<ParametersManager>();
            if (p != null)
                parameters = p;
            else
                return;
        }
    }


    public void ResetPosition_Rotation_Scale()
    {
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        this.Scale(1f, Vector3.zero);
        drawController.Init(true);
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log(Graph.Count() + " nodes currently in scene");
            foreach(var stroke in TerrainStrokes)
            {
                if (stroke as FinalStroke != null)
                {
                    ((FinalStroke)stroke).PrintSegments();
                }
            }
        }
#endif

        int N = Graph.Count();

        Mesh nodesMesh = new Mesh();

        if (N > 0)
        {
            Vector3[] nodes = Graph.GetNodes();
            int[] indices = new int[nodes.Length];

            for (int i = 0; i < nodes.Length; i++)
                indices[i] = i;

            nodesMesh.SetVertices(nodes);

            nodesMesh.SetIndices(indices, MeshTopology.Points, 0);

            material.SetFloat(_PointRadiusIdx, pointRadius * Mathf.Pow(transform.localScale.x, 1f));
        }
        else
        {
            nodesMesh.SetVertices(new Vector3[] { });
            nodesMesh.SetIndices(new int[] { }, MeshTopology.Points, 0);
        }
        meshFilter.mesh = nodesMesh;
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        //int N = Graph.Count();
        int[] nodeIDs = Graph.GetNodeIDs();
        // Draw all nodes and associated normals and tangents
        foreach (int id in nodeIDs)
        {
            Gizmos.color = Graph.IsSharp(id) ? Color.yellow : Color.gray;

            Vector3 nodePos = this.transform.TransformPoint(Graph.Get(id));
            Vector3[] segments = Graph.GetNeighbors(id);

            Gizmos.DrawSphere(nodePos, 0.005f);

            int j = 0;

            foreach(var tan in segments)
            {
                Handles.color = FewColors.Get(j);
                j++;
                Vector3 dir = this.transform.TransformDirection(tan);
                Vector3 to = nodePos + 0.02f * dir;
                Handles.DrawAAPolyLine(4f, new Vector3[] { nodePos, to });
            }

            // Draw normal if there is one
            Vector3 normal = Graph.GetNormal(id);
            if (normal.magnitude > 0f)
            {
                Vector3 dir = this.transform.TransformDirection(normal);
                Vector3 to = nodePos + 0.05f * dir;
                Handles.color = Color.black;
                Handles.DrawAAPolyLine(10f, new Vector3[] { nodePos, to });
            }
        }
#endif
    }

    public void Init(bool surfacing)
    {
        Clear();
        Graph.Init(surfacing);
    }

    public void SwitchSystem(bool surfacing)
    {
        // Set parameter
        Graph.SwitchSystem(surfacing);
    }


    public void Scale(float newScale, Vector3 zoomCenter)
    {
        Vector3 originalPos = transform.localPosition;
        Vector3 translation = originalPos - zoomCenter;

        float RS = newScale / transform.localScale.x; // relative scale factor

        // calc final position post-scale
        Vector3 FP = zoomCenter + translation * RS;

        // finally, actually perform the scale/translation
        transform.localScale = new Vector3(newScale, newScale, newScale);
        transform.localPosition = FP;

        // Scale Small value
        parameters.Current.UpdateScale(newScale);

        // Scale stroke width
        foreach (var stroke in TerrainStrokes)
        {
            stroke.UpdateWidth(newScale);
        }
        foreach (var stroke in TextStrokes)
        {
            stroke.UpdateWidth(newScale);
        }
        foreach (var stroke in LandmarkStrokes)
        {
            stroke.UpdateWidth(newScale);
        }
        // Scale grid
        grid.Scale(newScale);
    }

    public void Add2D(_2DFinalStroke s)
    {
        _2DStrokes.Add(s);
    }    
    

    public void Remove2D(_2DFinalStroke s)
    {

        _2DStrokes.Remove(s);
    }

    public void AddMesh(GameObject s)
    {
        AIGeneratedMesh.Add(s);
    }
    public void RemoveMesh(GameObject s)
    {

        AIGeneratedMesh.Remove(s);
    }
    public void AddTerrainStroke(FinalStroke s)
    {
        TerrainStrokes.Add(s);
        // Get graph update
        GraphUpdate();
    }

    public void RemoveTerrainStroke(FinalStroke s)
    {
        TerrainStrokes.Remove(s);
        // Get graph update
        GraphUpdate();
    }

    public void AddLandmarkStroke(FinalStroke s)
    {
        LandmarkStrokes.Add(s);
        // Get graph update
        GraphUpdate();
    }

    public void RemoveLandmarkStroke(FinalStroke s)
    {
        LandmarkStrokes.Remove(s);
        // Get graph update
        GraphUpdate();
    }
    public void AddTextStroke(TextStroke s)
    {
        TextStrokes.Add(s);
    }
    public void RemoveTextStroke(TextStroke s)
    {

        TextStrokes.Remove(s);
    }
    public void Delete(Stroke s, bool mirror = false)
    {
        ICommand command = new DeleteCommand(this, s.gameObject, drawController.finalStrokePrefab, drawController._2DFinalStrokePrefab, drawController.textStrokePrefab, parameters.Current.TextWidthRatio);
        VRCommandInvoker.Instance.ExecuteCommand(command);
    }

    public bool TryAddPatchAt(Vector3 worldPos, bool mirroring)
    {
        Vector3 pos = transform.InverseTransformPoint(worldPos);

        bool lookAtNonManifold = false;
        bool success = Graph.TryFindCycleAt(pos, lookAtNonManifold);
        if (!success)
        {
            lookAtNonManifold = true;
            success = Graph.TryFindCycleAt(pos, lookAtNonManifold);
        }

        if (success)
        {
            GraphUpdate();
        }

        return success;
    }

    public bool DeleteSelected(out InteractionType type, out int elementID, bool mirror)
    {
        type = InteractionType.Idle;
        elementID = -1;

        if (selectedStroke != null)
        {
            Delete(selectedStroke, mirror);

            // Log info
            if (selectedStroke.GetComponent<FinalStroke>() != null)
            {
                type = InteractionType.StrokeDelete;
                elementID = selectedStroke.GetComponent<FinalStroke>().ID;
            }

            // For Text
            if (selectedStroke.GetComponent<InputStroke>() != null)
            {
                type = InteractionType.StrokeDelete;
                elementID = -2; // For text
            }

            selectedStroke = null;
            return true;
        }
        else
            return false;
    }


    public void Clear()
    {
        foreach (Stroke s in TerrainStrokes)
            s.Destroy();
        TerrainStrokes = new List<FinalStroke>();

        foreach (Stroke s in LandmarkStrokes)
        {
            try
            {
                s.Destroy();
            }
            catch (System.Exception)
            {
                Debug.Log("LandmarkStrokes clear fail");
            }
            
        }    
            
        LandmarkStrokes = new List<FinalStroke>();

        foreach (Stroke s in _2DStrokes)
            s.Destroy();
        _2DStrokes = new List<_2DFinalStroke>();

        foreach (Stroke s in TextStrokes)
            s.Destroy();
        TextStrokes = new List<Stroke>();

        foreach (GameObject s in AIGeneratedMesh)
           Destroy(s);
        AIGeneratedMesh = new List<GameObject>();

        GraphUpdate();
        Graph.Clear();
    }

    public bool UpdateToDelete(Stroke stroke)
    {

        if (selectedStroke == null || !selectedStroke.Equals(stroke))
        {

            ClearToDelete(selectedStroke);

            selectedStroke = stroke;
            selectedStroke.OnDeleteSelect();

            return true;
        }
        return false;
    }

    public bool ClearToDelete(Stroke toClear)
    {
        if (selectedStroke != null && selectedStroke.Equals(toClear))
        {
            selectedStroke.OnDeleteDeselect();
            selectedStroke = null;
            return true;
        }
        return false;
    }

    // Create a game object as a child of the canvas,
    // reset its own transform so that only the canvas transform impacts world positions
    public GameObject Create(GameObject prefab, Primitive type)
    {
        Transform parent = type.Equals(Primitive.Stroke) ? StrokeContainer.transform
                         //: type.Equals(Primitive.Surface) ? SurfaceContainer.transform
                         : type.Equals(Primitive.Text) ? TextContainer.transform
                         : type.Equals(Primitive._2DStroke) ? _2DStrokeContainer.transform
                         : gameObject.transform;
        GameObject newObject = Instantiate(prefab, Vector3.zero, Quaternion.identity, parent);
        newObject.transform.localPosition = Vector3.zero;
        newObject.transform.localRotation = Quaternion.identity;
        newObject.transform.localScale = Vector3.one;
        LineRenderer goLineRenderer = newObject.GetComponent<LineRenderer>();
        goLineRenderer.material.EnableKeyword("_EMISSION");
        goLineRenderer.positionCount = 1;
        goLineRenderer.numCapVertices = 3;
        goLineRenderer.numCornerVertices = 3;
        goLineRenderer.shadowCastingMode = ShadowCastingMode.Off;
        goLineRenderer.alignment = LineAlignment.View;
        goLineRenderer.generateLightingData = true;
        goLineRenderer.sortingLayerName = "StrokeLayer";
        return newObject;
    }

    public void GraphUpdate()
    {
        // Update cycles
        Graph.TryFindAllCycles();
        Graph.Update(out List<ICycle> toAdd, out List<ICycle> toRemove);
    }
}
