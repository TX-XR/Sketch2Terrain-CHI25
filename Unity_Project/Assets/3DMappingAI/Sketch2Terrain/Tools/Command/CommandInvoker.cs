using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MappingAI;

namespace MappingAI
{

    public class VRCommandInvoker : MonoBehaviour
    {
        private Stack<ICommand> undoStack = new Stack<ICommand>();
        private Stack<ICommand> redoStack = new Stack<ICommand>();
        private ComponentManager componentManager;
        private AIModelManager aIModelManager;
        private bool canInference = true;

        public static VRCommandInvoker Instance;
        private void OnEnable()
        {
            // buttonXandA
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.Redo, Redo);
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolHot, () => { canInference = false; });
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.EraserToolCold, () => { canInference = true; });
            // buttonYandB
            Sketch2TerrainEventManager.StartListening(Sketch2TerrainEventManager.Undo, Undo);
        }
        private void OnDisable()
        {
            // buttonXandA
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.Redo, Redo);
            // buttonYandB
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.Undo, Undo);
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolHot, () => { canInference = false; });
            Sketch2TerrainEventManager.StopListening(Sketch2TerrainEventManager.EraserToolCold, () => { canInference = true; });
        }

        private void Awake()
        {
            Instance = this;
        }
        private void Start()
        {
            componentManager = FindAnyObjectByType<ComponentManager>();
            aIModelManager = componentManager.GetAIModelManager();
        }
        private void ResetEverything()
        {
            undoStack = new Stack<ICommand>();
            redoStack = new Stack<ICommand>();
        }
        /// <summary>
        /// Execute a command and add it to the undo redo system.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>False if the command was discarded because it was not executed successfully.</returns>
        public bool ExecuteCommand(ICommand command)
        {
            if (command.Execute())
            {
                undoStack.Push(command);
                redoStack.Clear();
                //RenderTextureManager.ResetRenderTexture();
                if (canInference)
                    aIModelManager.ExecuteInferenceAsync();
                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Reverse the command that was last executed.
        /// </summary>
        public void Undo()
        {
            if (undoStack.Count <= 0)
            {
                return;
            }
            ICommand executedCommand = undoStack.Pop();
            executedCommand.Undo();
            redoStack.Push(executedCommand);
            aIModelManager.ExecuteInferenceAsync();
        }
        /// <summary>
        /// Replay the last undone command.
        /// </summary>
        public void Redo()
        {
            if (redoStack.Count <= 0)
            {
                return;
            }
            ICommand undoneCommand = redoStack.Pop();
            undoneCommand.Redo();
            undoStack.Push(undoneCommand);
            aIModelManager.ExecuteInferenceAsync();
        }
    }
}
