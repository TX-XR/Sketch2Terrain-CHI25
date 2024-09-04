using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MappingAI
{
    //[RequireComponent(typeof(OVRSpatialAnchor))]
    public class SpatialAnchorSaver : MonoBehaviour
    {
        
        public OVRSpatialAnchor _spatialAnchor;
        public bool isCreated = false;
        public bool isSaved = false;
        // Start is called before the first frame update
        private void Awake()
        {
            _spatialAnchor = GetComponent<OVRSpatialAnchor>();
        }
        private IEnumerator Start()
        {
            while (_spatialAnchor && _spatialAnchor.PendingCreation)
            {
                yield return null;
            }

            if (_spatialAnchor)
            {
                Debug.Log(_spatialAnchor.Created
                    ? _spatialAnchor.Uuid.ToString()
                    : "Anchor creation failed");

                isCreated = true;
                OnSaveLocalButtonPressed();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        #region UI Event Listeners

        /// <summary>
        /// UI callback for the anchor menu's Save button
        /// </summary>
        public void OnSaveLocalButtonPressed()
        {
            if (!_spatialAnchor) return;

            _spatialAnchor.SaveAnchorAsync().ContinueWith((result, anchor) =>
            {
                if (result.Success)
                {
                    anchor.OnSave();
                    isSaved = true;
                }
                else
                {
                    Debug.LogError($"Failed to save anchor {anchor._spatialAnchor.Uuid} with error {result.Status}.");
                }
            }, this);
        }

        void OnSave()
        {
            AnchorUuidStore.Add(_spatialAnchor.Uuid);
        }

        /// <summary>
        /// UI callback for the anchor menu's Hide button
        /// </summary>
        public void OnHideButtonPressed()
        {
            Destroy(gameObject);
        }

        /// <summary>
        /// UI callback for the anchor menu's Erase button
        /// </summary>
        public void OnEraseButtonPressed()
        {
            if (!_spatialAnchor) return;

            EraseAnchor();
        }

        void EraseAnchor()
        {
            _spatialAnchor.EraseAnchorAsync().ContinueWith((result, anchor) =>
            {
                if (result.Success)
                {
                    anchor.OnErase();
                }
                else
                {
                    Debug.LogError($"Failed to erase anchor {anchor._spatialAnchor.Uuid} with result {result.Status}");
                }
            }, this);
        }

        void OnErase()
        {
            AnchorUuidStore.Remove(_spatialAnchor.Uuid);
        }
        #endregion // UI Event Listeners

        #region outdated
        //private Dictionary<ulong, GameObject> anchors;
        //private void UpdateAnchors()
        //{
        //    foreach (var anchor in anchors)
        //    {
        //        ulong anchorKey = anchor.Key;
        //        bool isAnchorGot = OVRPlugin.TryLocateSpace(anchorKey, OVRPlugin.GetTrackingOriginType(), out OVRPlugin.Posef pose);
        //        if (isAnchorGot)
        //        {
        //            anchor.Value.transform.position = pose.ToOVRPose().position;
        //            anchor.Value.transform.rotation = pose.ToOVRPose().orientation;
        //        }
        //    }
        //}

        //public void CreateSpatialAnchor(Transform anchorPoint, GameObject objectToAnchor)
        //{
        //    OVRPlugin.SpatialAnchorCreateInfo spatialAnchorCreateInfo = new OVRPlugin.SpatialAnchorCreateInfo();
        //    spatialAnchorCreateInfo.BaseTracking = OVRPlugin.GetTrackingOriginType();
        //    spatialAnchorCreateInfo.PoseInSpace = OVRExtensions.ToOVRPose(anchorPoint, false).ToPosef();
        //    spatialAnchorCreateInfo.Time = OVRPlugin.GetTimeInSeconds();

        //    bool isAnchorCreated = OVRPlugin.CreateSpatialAnchor(spatialAnchorCreateInfo, out ulong anchorKey);
        //    if (isAnchorCreated)
        //    {
        //        anchors.Add(anchorKey, objectToAnchor);
        //    }
        //}
        #endregion
    }
}