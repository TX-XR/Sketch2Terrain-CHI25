using System;
using UnityEngine;
using static Oculus.Interaction.OptionalAttribute;

namespace MappingAI
{
    /// <summary>Toggles the primary hand for the stylus attachment</summary>
    public class PrimaryHandManager : MonoBehaviour
    {
        public GameObject secondaryHandTip;
        private Transform _previousController;

        //private void OnEnable()
        //{
        //    EventManagerVR.StartListening(EventManagerVR.StylusHandChanged, UpdateController);
        //    UpdateController();
        //}

        //private void OnDisable()
        //{
        //    EventManagerVR.StopListening(EventManagerVR.StylusHandChanged, UpdateController);
        //}

        private void Start()
        {
            UpdateController();
        }

        private void UpdateController()
        {
            ResetStylus();
            UpdateVisibility();
        }

        /// <summary>Reset the stylus position</summary>
        private void ResetStylus()
        {
            // set the stylus as the primiary hand's child
            ApplicationSettings.Instance.GetStylus().parent = ApplicationSettings.Instance.PrimaryStylusTip;
            ApplicationSettings.Instance.GetStylus().localPosition = Vector3.zero;
            ApplicationSettings.Instance.GetStylus().localRotation = Quaternion.identity;

            secondaryHandTip.transform.parent = ApplicationSettings.Instance.SecondaryStylusTip;
            secondaryHandTip.transform.localPosition = Vector3.zero;
            secondaryHandTip.transform.localRotation = Quaternion.identity;
        }

        /// <summary>Update the visibility of the controller</summary>
        private void UpdateVisibility()
        {
            if (_previousController)
            {
                //Enable the rendering of a previous controller
                SkinnedMeshRenderer[] previousControllerHand = _previousController.GetComponentsInChildren<SkinnedMeshRenderer>();
                Array.ForEach(previousControllerHand, rend => rend.enabled = true);

                //_previousController.gameObject.SetActive(true);
            }

            // Disable the visualization of the controller on the stylus hand
            SkinnedMeshRenderer[] stylusHand =
                ApplicationSettings.Instance.ControllerVisualization.GetComponentsInChildren<SkinnedMeshRenderer>();
            Array.ForEach(stylusHand, rend => rend.enabled = false);
            ApplicationSettings.Instance.ControllerVisualization.gameObject.SetActive(false);

            _previousController = ApplicationSettings.Instance.ControllerVisualization;
            changeVisibility(true);

            //ApplicationSettings.Instance.GetStylus().gameObject.SetActive(true);
        }

        public static void changeVisibility(bool flag)
        {
            Renderer stylus = ApplicationSettings.Instance.GetStylus().GetChild(0).GetComponent<Renderer>();
            stylus.enabled = flag;
            //Array.ForEach(stylus, rend => rend.enabled = flag);
        }

        public static bool getVisibilityState()
        {
            Renderer stylus = ApplicationSettings.Instance.GetStylus().GetChild(0).GetComponent<Renderer>();
            bool state = stylus.enabled? true:false;
            return state;
        }
    }
}