using UnityEngine;


/*
    This code was adapted from https://gitlab.inria.fr/D3/cassie and kept only what was necessary for this project
    Check out their original repository for better explanations of the parameters.
 */


[CreateAssetMenu]
public class Parameters : ScriptableObject
{
    [Header("Absolute value thresholds")]
    [SerializeField]
    [Tooltip("Distance in centimeter in the VR app, at default zoom level. This corresponds to delta_1 * 0.5, with delta_1 the notation from the paper.")]
    private float defaultSmallDistance = 0.02f;
    [SerializeField]
    [Tooltip("Angle in radians")]
    private float smallAngle = Mathf.PI / 6;
    [SerializeField]
    [Tooltip("Default stroke width")]
    private float strokeWidth = 0.002f;
    [SerializeField]
    [Tooltip("Default text annotation width")]
    private float textWidthRatio = 0.5f;
    [SerializeField]
    [Tooltip("Subdivisions number for each stroke for beautification")]
    private int subdivisionsPerUnit = 150;
    // ALL DISTANCES BELOW ARE DEFINED RELATIVE TO SMALL DISTANCE (dist = relDist * smallDistance)
    // That is because we define small distance as a function of the zoom level, enabling some user control of the thresholds
    // smallDistance = defaultSmallDistance / scale
    [Header("Input sketching data")]
    [SerializeField]
    [Tooltip("The distance above which we take new samples from the user motion into account (expressed as a relative distance to SmallDistance)")]
    private float minSamplingDistance = 0.1f;
    [SerializeField]
    [Tooltip("The distance below which we take new samples from the user motion into account (expressed as a relative distance to SmallDistance)")]
    private float maxSamplingDistance = 5f;

    [Header("Stroke pre-processing")]
    [SerializeField]
    [Tooltip("The minimum length of a stroke, below which we ignore the input stroke (expressed as a relative distance to SmallDistance)")]
    private float minStrokeSize = 0.5f;

    [SerializeField]
    [Tooltip("The minimum sketching action duration for a stroke, below which we ignore the input stroke (in seconds)")]
    private float minStrokeActionTime = 0.2f;

    [SerializeField]
    [Tooltip("The time in seconds during which we ignore a few samples at the start and end of each stroke")]
    private float samplesAblationDuration = 0.02f;

    [SerializeField]
    [Tooltip("The minimum length that a G1 section in a poly-Bezier curve can have (expressed as a relative distance to SmallDistance)")]
    private float minG1SectionLength = 1f;

    [SerializeField]
    [Tooltip("The maximum length of a portion of stroke classified as endpoint hook and removed during pre-processing (expressed as a relative distance to SmallDistance). If the last or first segment is smaller than this paramater, then it will be removed.")]
    private float maxHookSectionLength = 3f;

    [SerializeField]
    [Tooltip("The maximum ratio of the total length of a stroke that can be classified as endpoint hook and removed during pre-processing")]
    private float maxHookSectionStrokeRatio = 0.15f;

    [SerializeField]
    [Tooltip("The maximum error allowed in poly-Bezier curve fitting (expressed as a relative distance to SmallDistance)")]
    private float bezierFittingError = 0.5f;

    [SerializeField]
    [Tooltip("The maximum angular difference between successive stroke tangents above which we detect a G1 discontinuity (in radians)")]
    private float maxAngularVariationInG1Section = Mathf.PI / 4;


    [Header("Intersection constraints detection")]
    [SerializeField]
    [Tooltip("The distance threshold to detect intersection constraints, r_proximity of the paper (expressed as a relative distance to SmallDistance)")]
    private float proximityThreshold = 2;


    [SerializeField]
    [Tooltip("Distance threshold for detecting curve closure")]
    private float closedThreshold = 1;

    [SerializeField]
    [Tooltip("The minimum distance between 2 intersection constraints on the stroke (comparing the position of constraints on new stroke) (expressed as a relative distance to SmallDistance)")]
    private float mergeConstraintsThreshold = 0.5f;

    [SerializeField]
    [Tooltip("The distance below which we snap a new constraint to an existing node (expressed as a relative distance to SmallDistance)")]
    private float snapToExistingNodeThreshold = 1f;

    [SerializeField]
    [Tooltip("The distance threshold to snap to the surface of the terrain model when collect training data")]
    private float snapToTerrainSurfaceThreshold = 2;

    [Header("Curve neatening optimization")]
    [SerializeField]
    [Tooltip("Parameter controlling the balance between fidelity and intersection constraint satisfaction (connectivity). This is referred to as lambda in the paper.")]
    private float muFidelity = 0.6f;

    [SerializeField]
    [Tooltip("The minimum distance between 2 Bezier curve anchor points (the control points that lie on the curve, eg P_0, P_3)." +
        "This defines how finely we will allow the curve to be split and will directly impact how close successive constraints on the stroke can be, since every constraint is applied at an anchor point" +
        "(expressed as a relative distance to SmallDistance)")]
    private float minDistanceBetweenAnchors = 1f;

    [Header("Exporting data")]
    [SerializeField]
    [Tooltip("The maximum error allowed in RDP simplification of input samples upon export (expressed as a relative distance to SmallDistance)")]
    private float exportRDPError = 0.1f;

    public float SmallDistance
    {
        get { return defaultSmallDistance / scale; }
    }

    public float StrokeWidth
    {
        get { return strokeWidth; }
    }    
    public float TextWidthRatio
    {
        get { return textWidthRatio; }
    }
     public int SubdivisionsPerUnit
    {
        get { return subdivisionsPerUnit; }
    }

    public float SmallAngle
    {
        get { return smallAngle; }
    }

    public float MinSamplingDistance
    {
        get { return SmallDistance * minSamplingDistance; }
    }
    public float MaxSamplingDistance
    {
        get { return SmallDistance * maxSamplingDistance; }
    }
    public float MinStrokeSize
    {
        get { return SmallDistance * minStrokeSize; }
    }

    public float MinStrokeActionTime
    {
        get { return minStrokeActionTime; }
    }

    public float SamplesAblationDuration
    {
        get { return samplesAblationDuration; }
    }

    public float MinG1SectionLength
    {
        get { return SmallDistance * minG1SectionLength; }
    }

    public float MaxHookSectionLength
    {
        get { return SmallDistance * maxHookSectionLength; }
    }

    public float MaxHookSectionStrokeRatio
    {
        get { return maxHookSectionStrokeRatio; }
    }

    public float MaxAngularVariationInG1Section
    {
        get { return maxAngularVariationInG1Section; }
    }

    public float BezierFittingError
    {
        get { return SmallDistance * bezierFittingError; }
    }

    public float ProximityThreshold
    {
        get { return SmallDistance * proximityThreshold; }
    }    
    
    public float SnapToTerrainSurfaceThreshold
    {
        get { return SmallDistance * snapToTerrainSurfaceThreshold; }
    }

    public float ClosedThreshold
    {
        get { return SmallDistance * closedThreshold; }
    }


    public float MergeConstraintsThreshold
    {
        get { return SmallDistance * mergeConstraintsThreshold; }
    }

    public float SnapToExistingNodeThreshold
    {
        get { return SmallDistance * snapToExistingNodeThreshold; }
    }

    public float MuFidelity
    {
        get { return muFidelity; }
    }

    public float MinDistanceBetweenAnchors
    {
        get { return SmallDistance * minDistanceBetweenAnchors; }
    }

    public float ExportRDPError
    {
        get { return SmallDistance * exportRDPError; }
    }

    private float scale = 1f;
    public void UpdateScale(float newScale)
    {
        scale = newScale;
    }

    private void OnEnable()
    {
        scale = 1f;
    }

}
