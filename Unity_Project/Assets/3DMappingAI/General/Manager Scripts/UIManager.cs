using UnityEngine;

namespace MappingAI
{
    public class UIManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject UI_DataCollection;
        public GameObject UI_Experiment;
        private GameObject CurrentUI;
        void Awake()
        {
            UI_DataCollection.SetActive(false);
            UI_Experiment.SetActive(false);
            if (ApplicationSettings.Instance.DevelopmentMode == DevelopmentMode.DataCollection)
                CurrentUI = UI_DataCollection;
            else
                CurrentUI = UI_Experiment;
        }

        public void updateCurrentUI_DataCollection()
        {
            if (ComponentManager.Instance.GetInputController().FreeCreationMode())
            {
                CurrentUI.SetActive(false);
                CurrentUI = UI_DataCollection;
                CurrentUI.SetActive(true);
            }
        }
        public GameObject getCurrentUI()
        {
            return CurrentUI;
        }
    }
}