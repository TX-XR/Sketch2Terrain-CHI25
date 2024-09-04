using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace MappingAI
{
    public enum ColorProperty
    {
        Terrain = 0,
        //Route = 1,
        Landmark = 1,
        Other = 2,
    }
    public class MaterialManager : MonoBehaviour
    {
        
        public List<Material> SelectedMaterialList = new List<Material> { };
        public List<Material> UnselectedMaterialList = new List<Material> { };
        public Material StrokesOn3DStairsSelected;
        public Color ColorOnText = Color.white;
        public Material StrokesOn3DStairsUnselected;
        public Material AITerrainMaterialDuringDraw;
        public Material ReliefShadingMaterial;
        public Material StrokesOn3DShapesSelected;
        public Material StrokesOn3DShapesUnselected;
        public Material UnselectedSurface;
        public Material SelectedSurface;
        public Material Display;
        public Material materialForTerrainTexture;
        [HideInInspector]
        public Material CurrentSelectedMaterial;
        [HideInInspector]
        public Material CurrentUnselectedMaterial;
        public RenderTexture renderTexture;
        //public Material SnapedSurface;

        public static MaterialManager Instance;
        public MaterialManager() { Instance = this; }

        private int currentMaterialIndex = 0;
        private bool colorChanged = false;
        private void Awake()
        {
            currentMaterialIndex = 0;
            if (SelectedMaterialList.Count > 0)
            {
                CurrentSelectedMaterial = SelectedMaterialList[currentMaterialIndex];
            }

        }
        // The color refers to different elements
        public ColorProperty GetColorProterty()
        {
            return (ColorProperty)currentMaterialIndex;
        }
        public ColorProperty GetColorProterty(Material material)
        {
            return (ColorProperty)SelectedMaterialList.IndexOf(material);
        }
        public void ChangeMaterial()
        {
            // Increment the current material index
            currentMaterialIndex = (currentMaterialIndex + 1) % SelectedMaterialList.Count;
            if (SelectedMaterialList.Count > 0)
            {
                CurrentSelectedMaterial = SelectedMaterialList[currentMaterialIndex];
            }
        }
        public void ChangeMaterialByIndex(int index)
        {
            // Increment the current material index
            currentMaterialIndex = index;
            if (SelectedMaterialList.Count > 0)
            {
                CurrentSelectedMaterial = SelectedMaterialList[currentMaterialIndex];
            }
        }

        public Material GetMaterialByIndex(int index)
        {
            // Increment the current material index
            if (index < SelectedMaterialList.Count)
                return SelectedMaterialList[index];
            else
                return SelectedMaterialList[0];
        }

        public Material GetAITerrainMaterialDuringDraw()
        {
            return AITerrainMaterialDuringDraw;
        }

        public Material GetReliefShading()
        {
            return ReliefShadingMaterial;
        }
        public void ResetMaterial()
        {
            // Increment the current material index
            currentMaterialIndex = 0;
            if (SelectedMaterialList.Count > 0)
            {
                CurrentSelectedMaterial = SelectedMaterialList[currentMaterialIndex];
            }
        }

        public Material GetUnselectedMaterial(Color desiredColor)
        {
            int index1 = UnselectedMaterialList.FindIndex(m => m.color == desiredColor);
            if (index1>=0)
            {
                return MaterialManager.Instance.UnselectedMaterialList[index1];
            }
            int index = SelectedMaterialList.FindIndex(m => m.color == desiredColor);
            if (index >= 0)
                return MaterialManager.Instance.UnselectedMaterialList[index];
            else return null;
        }

        public Material GetSelectedMaterial(Color desiredColor)
        {
            int index1 = SelectedMaterialList.FindIndex(m => m.color == desiredColor);
            if (index1 >= 0)
            {
                return MaterialManager.Instance.SelectedMaterialList[index1];
            }
            int index = UnselectedMaterialList.FindIndex(m => m.color == desiredColor);
            if (index >= 0)
                return MaterialManager.Instance.SelectedMaterialList[index];
            else return null;
        }
    }
}