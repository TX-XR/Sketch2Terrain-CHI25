using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Curve;
using System.Text;


namespace MappingAI
{


    /// <summary>
    /// The final stroke after beautification and auto-modify based on the constraints
    /// </summary>
    public class _2DFinalStroke : Stroke
    {
        public bool DebugGizmos = false;

        public int ID { get; private set; }

        public MappingAI.ColorProperty colorProperty { get; private set; }
        public Curve.Curve Curve { get; private set; } = null;

        private LinkedList<ISegment> segments = new LinkedList<ISegment>();
        private Graph _graph;
        public bool _closedLoop;
        public Vector3[] inputSamples { get; private set; } = null;

        // FinalStroke should always be initialized as a child to a DrawingCanvas
        protected override void Awake()
        {
            base.Awake();
            // Fetch reference to scene graph
            DrawingCanvas parent = GetComponentInParent<DrawingCanvas>();
            if (parent != null)
            {
                _graph = parent.Graph;
            }

            else
                throw new System.Exception("FinalStroke should always be initialized as a child to a DrawingCanvas");
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Bezier ctrl points
            List<Vector3> pts = Curve.GetControlPoints();

            Gizmos.color = Color.blue;

            int idx = 0;
            foreach (var p in pts)
            {
                Gizmos.color = idx % 3 == 0 ? Color.blue : Color.yellow;
                Gizmos.DrawSphere(transform.TransformPoint(p), 0.002f);
                idx++;
            }

            foreach (var s in segments)
            {
                // Label: ID
                Vector3 midpoint = this.transform.TransformPoint(s.GetPointAt(0.5f));
                Handles.Label(midpoint, s.ID.ToString());
            }
        }
#endif

        public override void RenderAsLine(float scale)
        {
            int N = Mathf.Max(Mathf.CeilToInt(Curve.GetLength() * SubdivisionsPerUnit * scale), 4);
            Vector3[] points = new Vector3[N];

            float step = 1f / (N - 1);

            for (int i = 0; i < N; i++)
            {
                float t = i * step;
                points[i] = Curve.GetPoint(t);
            }

            RenderPoints(points, scale);
        }

        public override void RenderAsLineBySamples(float scale)
        {
            RenderPoints(inputSamples, scale);
        }

        public void SetID(int id)
        {
            this.ID = id;
        }

        public void SetColorProperty(MappingAI.ColorProperty colorProperty)
        {
            this.colorProperty = colorProperty;
        }
        public List<Vector3> GetControlPoints()
        {
            return Curve.GetControlPoints();
        }


        public void SetCurve(Curve.Curve c, bool closedLoop = false)
        {
            this.Curve = c;
            this._closedLoop = closedLoop;
            // Create a segment representing the whole stroke
        }

        public void SaveInputSamples(Vector3[] samples)
        {
            inputSamples = samples;
        }

        public override void Destroy()
        {
            // Remove each segment from graph
            foreach (var segment in this.segments)
            {
                _graph.Remove(segment);
            }

            // Destroy game object
            base.Destroy();
        }

        public Vector3 ClosestPoint(Vector3 position, bool canvasSpace = false)
        {
            // Position in world space
            Vector3 canvasSpacePos = canvasSpace ? position : gameObject.transform.InverseTransformPoint(position);
            // Get closest point on curve
            PointOnCurve onCurve = this.Curve.Project(canvasSpacePos);

            return onCurve.Position;
        }

        public bool MendSegments(INode mendAt, ISegment sA, ISegment sB)
        {
            LinkedListNode<ISegment> sALink = this.segments.Find(sA);

            LinkedListNode<ISegment> sLeft = null;
            LinkedListNode<ISegment> sRight = null;

            if (sALink.Next != null && sALink.Next.Value.Equals(sB)
                && sA.GetEndNode().Equals(mendAt) && sB.GetStartNode().Equals(mendAt)
                )
            {
                // sA is before sB
                sLeft = sALink;
                sRight = sALink.Next;
            }
            else if (sALink.Previous != null && sALink.Previous.Value.Equals(sB)
                && sA.GetStartNode().Equals(mendAt) && sB.GetEndNode().Equals(mendAt)
                )
            {
                // then sB is before sA
                sLeft = sALink.Previous;
                sRight = sALink;
            }
            else
            {
                // Node is closing the loop, don't remove it
                return false;
            }

            // Mend segments in graph:
            // - Delete sRight and edit end of sLeft
            // - Deal with cycles
            _graph.MendSegments(sLeft.Value, sRight.Value);

            // Actually remove it from stroke too
            this.segments.Remove(sRight);
            return true;
        }

        public Vector3 ParallelTransport(Vector3 v, float from, float to)
        {
            return Curve.ParallelTransport(v, from, to);
        }


        public void PrintSegments()
        {
            Debug.Log("Stroke with " + this.segments.Count + " segments");
            //if (this.segments.Count > 0)
            //    Debug.Log(segments.First.Value.GetStartNode().Position.ToString("F6"));
            foreach (var segment in this.segments)
            {
                Debug.Log(segment.GetStartNode().Position.ToString("F6"));
                Debug.Log(segment.GetStartParam());
                Debug.Log(segment.GetEndNode().Position.ToString("F6"));
                Debug.Log(segment.GetEndParam());
                Debug.Log("------------------");
            }
        }



        public ISegment GetSegmentContaining(float param)
        {
            return GetSegmentInListContaining(param).Value;
        }

        private LinkedListNode<ISegment> GetSegmentInListContaining(float param)
        {
            LinkedListNode<ISegment> segment = this.segments.First;
            while (segment.Next != null && segment.Value.GetEndParam() < param)
            {
                segment = segment.Next;
            }
            return segment;
        }

        private INode GetClosest(ISegment segment, Vector3 position)
        {
            INode oldLeft = segment.GetStartNode();
            INode oldRight = segment.GetEndNode();

            // Find closest among those 2
            INode closest = Vector3.Distance(oldLeft.Position, position) < Vector3.Distance(oldRight.Position, position) ? oldLeft : oldRight;

            return closest;
        }

    }
}