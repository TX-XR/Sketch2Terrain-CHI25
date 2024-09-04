using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MappingAI
{
    public class ComponentManager : MonoBehaviour
    {
        
        private DrawingCanvas drawingCanvas;
        private ExportController exportController;
        private ParametersManager parameters = null;
        private InputController inputController;
        private StudyScenario studyScenario;        
        private TerrainManagerAsync terrainManagerAsync;
        private EraseController eraseController;        
        private GrabController grabController;        
        private ZoomController zoomController;
        private Grid3D grid3D;
        private TextTool textTool;
        private EraserTool eraserTool;         
        private CalibrationManagement calibrationManagement;
        private DrawController drawController;
        private AIModelManager aIModelManager;
        private UIManager uIManager;
        public static ComponentManager Instance;
        public ComponentManager() { Instance = this; }
        // Start is called before the first frame update
        private void Awake()
        {
            drawingCanvas = FindAnyObjectByType<DrawingCanvas>();
            exportController = FindAnyObjectByType<ExportController>();
            parameters = FindAnyObjectByType<ParametersManager>();
            inputController = FindAnyObjectByType<InputController>();
            studyScenario = FindAnyObjectByType<StudyScenario>();
            terrainManagerAsync = FindAnyObjectByType<TerrainManagerAsync>();
            eraseController = FindAnyObjectByType<EraseController>();
            grabController = FindAnyObjectByType<GrabController>();
            zoomController = FindAnyObjectByType<ZoomController>();
            grid3D = FindAnyObjectByType<Grid3D>();
            textTool = FindAnyObjectByType<TextTool>();
            eraserTool = FindAnyObjectByType<EraserTool>();
            calibrationManagement = FindAnyObjectByType<CalibrationManagement>();
            drawController = FindAnyObjectByType<DrawController>();
            aIModelManager = FindAnyObjectByType<AIModelManager>();
            uIManager = FindAnyObjectByType<UIManager>();
        }
        public AIModelManager GetAIModelManager()
        {
            return aIModelManager;
        }        
        public UIManager GetUIManager()
        {
            return uIManager;
        }
        public CalibrationManagement GetCalibrationManagement()
        {
            return calibrationManagement;
        }        
        public DrawController GetDrawController()
        {
            return drawController;
        }

        public DrawingCanvas GetDrawingCanvas()
        {
            return drawingCanvas;
        }           
        public ZoomController GetZoomController()
        {
            return zoomController;
        }             
        
        public GrabController GetGrabController()
        {
            return grabController;
        }              
        public EraseController GetEraseController()
        {
            return eraseController;
        }          
        
        public EraserTool GetEraserTool()
        {
            if (eraserTool == null)
                eraserTool = FindAnyObjectByType<EraserTool>();
            return eraserTool;
        }          
        
        public TextTool GetTextTool()
        {
            if (textTool == null)
                textTool = FindAnyObjectByType<TextTool>();
            return textTool;
        }        
                
        public Grid3D GetGrid3D()
        {
            return grid3D;
        }        
        
        public TerrainManagerAsync GetTerrainManagerAsync()
        {
            return terrainManagerAsync;
        }

        public ParametersManager GetCASSIEParametersProvider()
        {
            return parameters;
        }

        public ExportController GetExportController()
        {
            return exportController;
        }

        public InputController GetInputController()
        {
            return inputController;
        }

        public StudyScenario GetStudyScenario()
        {
            return studyScenario;
        }
    }
}

