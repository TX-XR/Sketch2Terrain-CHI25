<h1 align="center">Sketch2Terrain: AI-Driven Real-Time Terrain Sketch Mapping in AR</h1>

<!-- Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments. The system also serves as a comprehensive tool for researchers, providing a unified interface that supports the entire research workflow, including data collection, material preparation, and experimentation. -->

![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Teasor.png?raw=true "The apparatus and workflow of the interface.")
<h1 align="center">Abstract</h1>

Sketch mapping is an established research tool to study human spatial decision-making and information processing by externalizing one's spatial knowledge through sketches. We present Sketch2Terrain, an advanced generative 3D sketch mapping system that radically changes how we create sketch maps using Augmented Reality (AR). Sketch2Terrain introduces a pipeline that combines freehand mid-air sketching to create connected curve networks with generative Artificial Intelligence (AI), enabling the real-time generation of realistic and high-fidelity 3D terrain in under 100 milliseconds. Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments, and serves as a unified interface for researchers to set up a complete research flow including data collection, material preparation, and experimentation effortlessly. A between-subject study (N=36) on terrain sketch mapping revealed that generative 3D sketch mapping improved efficiency by 38.4%, terrain topology accuracy by 12.5%, and landmark accuracy by up to 12.1%, with only a 4.7% trade-off in terrain elevation accuracy compared to freehand 3D sketch mapping. Additionally, generative 3D sketch mapping reduced 60.53% perceived strain and 39.46% stress over 2D sketch mapping. These findings underscore the potential of generative 3D sketch mapping for applications requiring a deep understanding of vertical dimensions. 

<h1 align="center">Interface design</h1>

### Sketch2Terrain offers a homogeneous system for different modes with customizable functions tailored to specific needs.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Interface_while.png?raw=true "The interface design.")
(a) Observation Task Interface: This interface includes only the height adjustment function. An instruction panel displays task descriptions and timing information in front of the workspace. (b) Drawing Task Interface: This interface offers height adjustment, color change, redo, undo, and eraser functions. An instruction panel presents task descriptions and timing in front of the workspace. (c) Data Collection Mode Interface: By default, this interface uses a blue color scheme and includes functions for changing the terrain model and saving data.

<!-- ### Sketch2Terrain provides real-time terrain generation, allowing users to iteratively sketch and visualise for more accurate results.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Workflow.png?raw=true "The workflow.")
The typical workflow for creating a 3D terrain with Sketch2Terrain is illustrated in the sequence. User-sketched strokes are highlighted in blue. The system automatically generates a well-connected curve network to facilitate terrain creation. As more strokes are added (progressing from (a) to (f)), the generated terrain becomes increasingly accurate. The second row from (g) to (l) shows the height map of the corresponding terrain model (altitude as color gradient). -->

### Sketch2Terrain utilizes the widely recognized pix2pix model to facilitate the transformation between sketches and height map of terrain.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Architecture.png?raw=true "The detail of the pipeline of the Pix2pix model.")
(a) The architecture of the AI model. (b) Performance versus model size and inference time. (c) Comparison between the inference results for the Pix2Pix and diffusion-based models.

<h1 align="center">User study</h1>

### Experiments were conducted in a controlled laboratory environment.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Setting.png?raw=true "Experimental setting.")
We recruited 36 participants (17 females, 19 males), aged between 18 and 41 years (M=25.00, SD=4.51), for a comprehensive user study. We designed a between-subjects experiment with three conditions corresponding to 2D, 3D, and Generative 3D sketch mapping concepts, respectively: (1) 2D condition, (2) 3D condition, and (3) AI condition. 

### The experimental design consisted of three main phases.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Design.png?raw=true "Experimental design.")
Experimental procedure: The experiment for 2D and 3D conditions was composed of tutorial and sketch mapping phases followed by the survey, while the AI condition had a voluntarily free creation phase (which is not shown in the picture). A different sketching interface was provided for each condition, as shown.

### Sample sketches produced by participants in 2D, 3D and AI conditions for eight scenes.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Example_Sketch.png?raw=true "Example Sketch.")

<h1 align="center">Results</h1>

![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Statistics_results.png?raw=true "Statistics_results.")
Bar plots of  (a) Terrain accuracy analysis measured by Intersection over Union (IoU) and Terrain Topology Score (TTS). (b) Landmark accuracy analysis measured by Landmark Topology Score (LTS) and Landmark Position Score (LPS). (c) Stroke number analysis. (d) Post-study questionnaire analysis. (e) User experience and task load analysis measured by SIM Task Load Index (SIM-TLX), the System Usability Scale (SUS), and the User Experience Questionnaire (UEQ).

### Design of post-study questionnaire.
In Q1 to Q3 and Q8 to Q17, they rated seven Likert-scale items from 1 (strongly disagree) to 7 (strongly agree). Q4 to Q7 were open-ended, and Q18 was a multi-choice question, allowing selection of "3D shape," "2D map," or "image" of the terrain. Q8 to Q17 was only applicable to the AI condition. 

- **Q1**: I felt like I could sketch fast.
- **Q2**: I felt like I was limited by the interface.
- **Q3**: I am satisfied with the resulting sketches.
- **Q4**: What are the most significant challenges you have encountered in the task?
- **Q5**: Which aspects did you find particularly satisfying?
- **Q6**: Are there any features you feel can improve the current method?
- **Q7**: Is there anything else about your experience that you would like to share?
- **Q8**: The accuracy of 3D shapes generated by AI is high.
- **Q9**: The fidelity of 3D shapes generated by AI is high.
- **Q10**: The generation speed of the AI model is high.
- **Q11**: The 3D shapes generated by AI looked as I intended.
- **Q12**: The 3D shapes generated by AI helped me recall terrain structure in the sketching task.
- **Q13**: The 3D shapes generated by AI helped me recall landmarks in the sketching task.
- **Q14**: The 3D shapes generated by AI reduced my cognitive workload in the sketching task.
- **Q15**: The 3D shapes generated by AI were distracting in the sketching task.
- **Q16**: The 3D shapes generated by AI are close to the cognitive model in my brain.
- **Q17**: How do you remember the terrain in your brain?

<h1 align="center">Installation</h1>

Due to the use of the [Mapbox](https://www.mapbox.com/unity), the project could only run by `Play Mode` in Unity and then stream in the Meta Quest series of headsets. 

**🔴IMPORTANT🔴** We highly recommend using the [Meta Quest Link cable](https://www.meta.com/ch/en/quest/accessories/link-cable/) to reduce the latency of the streaming. 

### Steps for setting up
1. Follow [Meta Developer Instructions](https://developer.oculus.com/documentation/unity/unity-before-you-begin/) to set up Unity for XR development and import Meta XR packages.
2. The application has been tested on Unity `2022.2.19f1` but has not been tested on others.
3. Clone this repository or download the source code from GitHub.
4. Open the Unity Project with Unity `2022.2.19f1`. Find the correct scene in **Assets > 3DMappingAI > Scenes > Sketch2Terrain_demo_scene** and double-click it.
5. From the top menu of the Unity Editor, navigate to **Edit > Build Settings** to switch the Platform into Android.
6. You are ready to play the project.
### Customize experiment and mode settings
1. From the top menu of the Unity Editor, navigate to **Sketch2TerrainSetting > ExperimentSetting** to open the `Application Settings` in Inspector.
2. **🔴IMPORTANT🔴**: Under the **Mapbox > Mapbox Access Token** in Inspector, paste your [Mapbox access token](https://www.mapbox.com/install/unity/permission).
3. Under the **Handness > Primary Hand** in Inspector, select Right Hand or Left Hand.
4. Under the **Interface > Development Mode** in Inspector, select **DataCollection** for data collection mode, where researchers prepare training data for AI algorithms by tracing random terrain models; select **MaterialPreparation** for Material preparation mode, where researchers prepare example sketches for experiment; select **Experimentation** for Experimentation mode, where participants follow the experimental procedure to complete the experiment session.
5. Under the **Interface > Experiment Condition** in Inspector, select `2D` for 2D Sketch Mapping, where participants can only sketch on a flat canvas in a 2D workspace; select `3D` for 3D Sketch Mapping, where participants can sketch 3D strokes in a 3D workspace; select `AI` for Generative 3D Sketch Mapping, where AI generates 3D terrain shapes based on sketches.
6. Under the **Interface > Participant ID** in Inspector, insert a number for the identator.

#### Extended Reading: The difference between 2D, 3D, and AI conditions
2D, 3D, and Generative 3D Sketch Mapping methods are represented by 2D, 3D, and AI conditions. The system can seamlessly switch between interfaces for 2D, 3D, and AI conditions depending on the experiment setting. In the 3D and AI conditions, a 3D workspace is displayed, with the terrain model and sketches visible within it. Both conditions include basic sketching functions, but the AI condition additionally employs the algorithm and features a 'Deactivate AI' button on the non-dominant controller. When this button is pressed, the 3D models generated by AI will be hidden and the sketching function will remain unchanged. In the 2D condition, the 3D workspace is compressed into a 2D view, with the bottom surface remaining intact and strokes projected onto it.

### Customize sketching system settings
You can try out different system settings by creating your own ScriptableObject:

In your project (for example in Assets/Parameters) **right-click > Create > Parameters**. Customize the values.

Change the parameters currently used in the Unity app: in the scene, find the `Manager` GameObject. Under the script **Parameters Manager** drag your new Parameters ScriptableObject under Current Parameters.

The default values are the ones we used in the paper and user study. A detailed description of each parameter (units and effect) is provided, hover over the name to display the description. You can always come back to default parameters by dragging the ScriptableObject Default Parameters to the Parameters GameObject.

<h1 align="center">How to use the application</h1>

### Align the workspace to table

When you first get into the `Play Mode` in Unity, you are in the `Surface Calibration` phase. Your dominant hand's controller is visualized as a stylus, while your non-dominant hand is visualized as a controller. You should first put the stylus on the left corner of a table and press the `X: Calibrate` button on the non-dominant controller. Then you repeat this process for the right corner of a table. After you calibrate the workspace, the UI will be loaded and you can start sketching. 

After the first time calibrating the workspace, the location of the table is stored in the spatial anchor. You can directly load the workspace by pressing the `trigger` button on the non-dominant controller. 
### Interaction with the vitual button on the table
As shown in the interface design, there are some virtual buttons on the table. The researcher can customize these buttons in Unity. To trigger the functions of the button, you can simply use the stylus to touch those buttons during `Play Mode` of Unity.

### Experimentation mode
Assuming you are in `Experimentation mode`, you will first enter a tutorial phase. In the tutorial phase, you will see an example terrain model in the workspace and an example sketch on the left. You can press the `Grip` on the dominant hand to sketch. When you sketch, the example terrain model will disappear, which imitates the real `sketch mapping task`. In the `sketch mapping task`, participants need to externalize their cognitive map of terrain from memory, so the terrain model will disappear. Because it is the tutorial phase, so the terrain will appear again after you release the `Grip` button to end a stroke. There are some virtual buttons on the table. You can view the interface design images to see what these buttons do.

After you feel confident about the interface, press the `Thumbstick` button on the non-dominant controller to move to the next phase: The sketch mapping task. The sketch mapping task comprises two phases: observation and drawing. In the observation phase, users view a 3D model to memorize the spatial environment. During the drawing phase, they use one of the AR interfaces to create a sketch map from memory, incorporating both the terrain structure and the locations and shapes of landmarks. Users are free to adjust their seats, stand, or move around the table while sketching. 

The observation phase will last 2 minutes and the drawing phase will last 5 minutes. The controller can not stop the observation phase. If you want to skip the observation phase, you should press the `E` button on the keyboard. The drawing phase can be skipped by pressing the `Thumbstick` button on the non-dominant controller.

For a detailed tutorial, you can watch the instructions video.

### Data collection mode
In this mode, researchers can generate a random terrain model by pressing the "Next Terrain" button or revert to the previous terrain by pressing the "Previous Terrain" button on the table. After generating the terrain, researchers trace the main structure by drawing. Once the drawing is complete, the sketch and terrain information—including the height map, mesh model of the terrain, control points, and mesh model of the strokes, are saved as training data for the AI model by press the `SAVE` virtual button on the table. Strokes can be snapped to the terrain model within a set threshold to enhance data quality.

### Material preparation mode
The material preparation mode allows researchers to create experimental materials for different conditions. In this mode, 3D sketches are also projected in 2D and exported separately to maintain consistency between 2D and 3D example sketches. Similar to the data collection mode, strokes can be snapped to the terrain model. 

### Keyboard control
The design of the interface applies a redundant control system. The experimenter can also control the interface by pressing the keyboard.
- `X`:Export the sketch
- `Q`:Change to the next phase/scene (after pressing the `Q` button, the system will ask for confirmation, you can press `Q` again for confirmation or `W` to go back)
- `W`:Back to the current study
- `E`:Skip the Observation phase in the sketch mapping task.
- `L`:Load the workspace from spatial anchor.

### Saving the sketches
All exported files will be available in the **Assets > SketchData~** folder if playing from the editor.

In the `Experimentation mode,` the sketches will be saved automatically when changed to the next phase/scene. In the `Data collection mode` and `Material preparation mode`, the sketches will be saved by pressing the `SAVE` virtual button on the table.

In the Data collection mode under 3D condition, the sketch and terrain model will be saved as `Strokes.obj` and `Terrain.obj` files. The raw data for the control points of each stroke will be saved in a `Strokes.curves` file. The height map and texture of the terrain will also be saved as `Terrain.png` and `Terrain_heightmap.png` files.

Under AI condition, height map and texture of the AI-generated shape will also be saved as `AI_heightmap.png` and `AI_Texture.png` while the mesh of the AI-generated shape will be saved in `Strokes.obj`, too.

To sum up:
1. `Strokes.obj`: an OBJ file with a mesh corresponding to the strokes in the current sketch, rendered as tubular meshes.
2. `Strokes.curves`: a file that stores all strokes in the current sketch as polylines. This is designed as a super easy format to import in other systems that treat 3D polylines.
3. `LerpStrokes.curves`: a file that normalizes the raw data of the polylines into the size of the workspace with x and y coordinate ranges from -0.2 ~ 0.2 and z coordinate range from 0 ~ 0.4, aligned with the workspace. This is designed to align the polylines to the terrain model in the training data of the AI model.
4. `graph.json`: a file that stores the graph data structure of the current sketch.
5. `Terrain.obj`: an OBJ file with a mesh corresponding to the terrain model.
6. `Terrain.png`: a PNG file for the terrain model's texture.
7. `Terrain_heightmap.png`: a PNG file for the terrain model's heightmap normalized ranges from 0 ~ 1, with 1 means the height of the workspace (0.4m).
8. `AI_Texture.png`: a PNG file for the AI-generated shape's texture.
7. `AI_heightmap.png`: a PNG file for the AI-generated shape's heightmap normalized ranges from 0 ~ 1, with 1 means the height of the workspace (0.4m).
9. `[Study]_[timestamp].json`: a log of the entire sketching session.

### Dependencies/external code
- [Mapbox](https://www.mapbox.com/unity): for the terrain model generation.
- [CASSIE](https://gitlab.inria.fr/D3/cassie): for the curve network creation and smoothing post-processing.
- [Math.Net](https://numerics.mathdotnet.com/): included in the project.
- [Meta XR All-in-One SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657): for spatial anchor, pass through, and interaction.
- [Barracuda](https://github.com/Unity-Technologies/barracuda-release): for the coupling between the AR system and the AI mode.
- [otss-off-the-shelf-stylus](https://gitlab2.informatik.uni-wuerzburg.de/hci-development/otss-off-the-shelf-stylus): for aligning the virtual surface to the physical table.

### License
The code in this repository except for the external dependencies is provided under the MIT License. The external dependencies are provided under their respective licenses.
