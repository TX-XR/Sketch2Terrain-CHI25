using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace MappingAI{
    public class MenuController : MonoBehaviour
    {
        public GameObject keyboard;
        public GameObject keyboardPosition;
        public Text handednessText;
        public Text participantID;
        Regex regex = new Regex(@"\d+");
        private void Start()
        {
            if (ApplicationSettings.Instance.primaryHand == UnityEngine.XR.XRNode.RightHand)
            {
                handednessText.text = "Right Handed";
            }
            else
            {
                handednessText.text = "Left Handed";
            }
        }

        private void Update()
        {
            if (Vector3.Distance(keyboard.transform.position, keyboard.transform.position) > 0.1f)
            {
                keyboard.transform.position = keyboard.transform.position;
                keyboard.transform.rotation = keyboard.transform.rotation;
            }

        }
        public void SwitchScene(string sceneName)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
        }
        public void SwitchScene(int sceneIndex)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneIndex);
        }

        public void Load2DScene()
        {
            //SwitchScene("2DCondition");
            ApplicationSettings.Instance.ExperimentCondition = ExperimentCondition._2D;
            SwitchScene("3DCondition");
        }

        public void Load3DScene()
        {
            ApplicationSettings.Instance.ExperimentCondition = ExperimentCondition._3D;
            SwitchScene("3DCondition");
        }
        public void LoadAIScene()
        {
            ApplicationSettings.Instance.ExperimentCondition = ExperimentCondition._AI;
            SwitchScene("3DCondition");
        }

        public void ChangeHandness()
        {
            if (ApplicationSettings.Instance.primaryHand == UnityEngine.XR.XRNode.RightHand)
            {
                handednessText.text = "Left Handed";
                ApplicationSettings.Instance.primaryHand = UnityEngine.XR.XRNode.LeftHand;
            }
            else
            {
                handednessText.text = "Right Handed";
                ApplicationSettings.Instance.primaryHand = UnityEngine.XR.XRNode.RightHand;
            }
        }

        public void SetParticipantID()
        {
            ApplicationSettings.Instance.participantID = ExtractNumbers(participantID.text);
        }

        public static string ExtractNumbers(string input)
        {
            string s = "";
            Regex regex = new Regex(@"\d+");
            MatchCollection matches = regex.Matches(input);

            foreach (Match match in matches)
            {
                s += int.Parse(match.Value);
            }
            if (s.Length == 0)
            {
                return "0";
            }
            return s;
        }
    }
}

