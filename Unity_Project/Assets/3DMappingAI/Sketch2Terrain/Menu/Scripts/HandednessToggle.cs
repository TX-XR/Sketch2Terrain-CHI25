using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;

namespace MappingAI
{
    public class HandednessToggle : MonoBehaviour
    {
        [SerializeField] private Button previous;
        [SerializeField] private Button next;
        [SerializeField] private Text value;

        void Start()
        {
            previous.onClick.AddListener(UpdateHandedness);
            next.onClick.AddListener(UpdateHandedness);
            UpdateText();
        }

        void UpdateHandedness()
        {
            switch (ApplicationSettings.Instance.primaryHand)
            {
                case XRNode.RightHand:
                    ApplicationSettings.Instance.primaryHand = XRNode.LeftHand;
                    break;
                case XRNode.LeftHand:
                    ApplicationSettings.Instance.primaryHand = XRNode.RightHand;
                    break;
                default:
                    ApplicationSettings.Instance.primaryHand = XRNode.RightHand;
                    break;
            }

            if (EventSystem.current)
                EventSystem.current.SetSelectedGameObject(null);

            UpdateText();
        }

        void UpdateText()
        {
            switch (ApplicationSettings.Instance.primaryHand)
            {
                case XRNode.RightHand:
                    value.text = "Right Handed";
                    break;
                case XRNode.LeftHand:
                    value.text = "Left Handed";
                    break;
            }
        }
    }
}