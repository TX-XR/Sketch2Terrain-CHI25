<h1 align="center">Sketch2Terrain: AI-Driven Real-Time Universal-Style Terrain Sketch Mapping in AR</h1>

Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments. The system also serves as a comprehensive tool for researchers, providing a unified interface that supports the entire research workflow, including data collection, material preparation, and experimentation.

### Sketch2Terrain is an advanced generative 3D sketch mapping system that revolutionizes how we create sketch maps using Augmented Reality (AR).
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Teasor.png?raw=true "The apparatus and workflow of the interface.")

<h1 align="center">Abstract</h1>

Sketch mapping is an established research tool to study human spatial decision-making and information processing by externalizing one's spatial knowledge through sketches. We present Sketch2Terrain, an advanced generative 3D sketch mapping system that radically changes how we create sketch maps using Augmented Reality (AR). Sketch2Terrain introduces a pipeline that combines freehand mid-air sketching to create connected curve networks with generative Artificial Intelligence (AI), enabling the real-time generation of realistic and high-fidelity 3D terrain in under 100 milliseconds. Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments, and serves as a unified interface for researchers to set up a complete research flow including data collection, material preparation, and experimentation effortlessly. A between-subject study (N=36) on terrain sketch mapping revealed that generative 3D sketch mapping improved efficiency by 38.4%, terrain topology accuracy by 12.5%, and landmark accuracy by up to 12.1%, with only a 4.7% trade-off in terrain elevation accuracy compared to freehand 3D sketch mapping. Additionally, generative 3D sketch mapping reduced 60.53% perceived strain and 39.46% stress over 2D sketch mapping. These findings underscore the potential of generative 3D sketch mapping for applications requiring a deep understanding of vertical dimensions. 

<h1 align="center">Interface design</h1>

### Sketch2Terrain offers a homogeneous system for different modes with customizable functions tailored to specific needs.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Interface_while.png?raw=true "The interface design.")
(a) Observation Task Interface: This interface includes only the height adjustment function. An instruction panel displays task descriptions and timing information in front of the workspace. (b) Drawing Task Interface: This interface offers height adjustment, color change, redo, undo, and eraser functions. An instruction panel presents task descriptions and timing in front of the workspace. (c) Data Collection Mode Interface: By default, this interface uses a blue color scheme and includes functions for changing the terrain model and saving data.
### Sketch2Terrain provides real-time terrain generation, allowing users to iteratively sketch and visualise for more accurate results.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Workflow.png?raw=true "The workflow.")
The typical workflow for creating a 3D terrain with Sketch2Terrain is illustrated in the sequence. User-sketched strokes are highlighted in blue. The system automatically generates a well-connected curve network to facilitate terrain creation. As more strokes are added (progressing from (a) to (f)), the generated terrain becomes increasingly accurate. The second row from (g) to (l) shows the height map of the corresponding terrain model (altitude as color gradient).
### Sketch2Terrain utilizes the widely recognized pix2pix model to facilitate the transformation between sketches and height map of terrain.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Architecture.png?raw=true "The detail of the pipeline of the Pix2pix model.")
(a) The architecture of the AI model. (b) Performance versus model size and inference time. The area of the markers indicates the model size. (c) Comparison between the inference results for the Pix2Pix and diffusion-based models.

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
Bar plots of  (a) Terrain accuracy analysis measured by Intersection over Union (IoU) and Terrain Topology Score (TTS). (b) Landmark accuracy analysis measured by Landmark Topology Score (LTS) and Landmark Position Score (LPS). (c) Stroke number analysis. (d) Post-study questionnaire analysis. (e) User experience and task load analysis measured by SIM Task Load Index (SIM-TLX), the System Usability Scale (SUS), and the User Experience Questionnaire (UEQ). The error bars indicate the 95\% confidence intervals. $\ast = p < .05$, $\ast\ast = p < .01$, $\ast\ast\ast = p < .001$.

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

Due to the use of the Mapbox SDK, the project could only run by Play Mode in Unity and then stream in the Meta Quest series of headsets.
1. You can follow [Meta Developer Instructions](https://developer.oculus.com/documentation/unity/unity-before-you-begin/) to set up. The application has been tested on Unity 2022.2.19f1 but has not been tested on others.
2. Clone this repository or download the source code from GitHub.
3. Open the Unity project with 2022.2.19f1. Find the correct scene in Assets > 3DMappingAI > Scenes > Sketch2Terrain_demo_scene and double-click it.
4. You are ready to play the project.
### Settings
1. From the top menu of the Unity Editor, navigate to **Sketch2TerrainSetting > ExperimentSetting** to open the **Application Settings** in Inspector.
2. **🔴IMPORTANT🔴**: Under the **Interface > Mapbox Access Token** in Inspector, paste your [Mapbox access token](https://www.mapbox.com/install/unity/permission).
3. Under the **Handness > Primary Hand** in Inspector, select Right Hand or Right Hand.
4. Under the **Interface > Development Mode** in Inspector, select **Data Collection** for data collection mode, where researchers prepare training data for AI algorithms by tracing random terrain models; select **MaterialPreparation** for Material preparation mode, where researchers prepare example sketches for experiment; select **Experimentation** for Experimentation mode, where participants follow the experimental procedure to complete the experiment session.
5. Under the **Interface > Experiment Condition** in Inspector, select **2D** for 2D Sketch Mapping, where participants can only sketch on a flat canvas in a 2D workspace; select **3D** for 3D Sketch Mapping, where participants can sketch 3D strokes in a 3D workspace; select **AI** for Generative 3D Sketch Mapping, where AI generates 3D terrain shapes based on sketches.
6. Under the **Interface > Participant ID** in Inspector, insert a number for the identator.

### Customize system settings
You can try out different system settings by creating your own ScriptableObject:
In your project (for example in Assets/Parameters) right-click > Create > CASSIE Parameters. Customize the values.
Change the parameters currently used in the Unity app: in the scene, find the Parameters GameObject. Under the script CASSIE Parameters Provider drag your new Parameters ScriptableObject under Current Parameters.
The default values are the ones we used in the paper and user study. A detailed description of each parameter (units and effect) is provided, hover over the name to display the description.
You can always come back to default parameters by dragging the ScriptableObject Default CASSIE Parameters to the Parameters GameObject.
### Dependencies/external code
- [Mapbox](https://www.mapbox.com/unity): for the terrain model generation.
- [CASSIE](https://gitlab.inria.fr/D3/cassie): for the curve network creation and smoothing post-processing.
- [Math.Net](https://numerics.mathdotnet.com/): included in the project.
- [Meta XR All-in-One SDK](https://assetstore.unity.com/packages/tools/integration/meta-xr-all-in-one-sdk-269657): for spatial anchor, pass through, and interaction.
- [Barracuda](https://github.com/Unity-Technologies/barracuda-release): for the coupling between the AR system and the AI mode.
- [otss-off-the-shelf-stylus](https://gitlab2.informatik.uni-wuerzburg.de/hci-development/otss-off-the-shelf-stylus): for aligning the virtual surface to the physical table.

### License
The code in this repository except for the external dependencies is provided under the MIT License. The external dependencies are provided under their respective licenses.
