using UnityEditor;
using UnityEngine;

namespace MappingAI
{
    public class MenuEntries : Editor
    {
        [MenuItem("Sketch2TerrainSetting/ExperimentSetting")]
        public static void ShowApplicationSettings()
        {
            Selection.activeObject = ApplicationSettings.Instance;
        }
    }
}