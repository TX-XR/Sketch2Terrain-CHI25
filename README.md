# Sketch2Terrain: AI-Driven Real-Time Universal-Style Terrain Sketch Mapping in AR
Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments. The system also serves as a comprehensive tool for researchers, providing a unified interface that supports the entire research workflow, including data collection, material preparation, and experimentation.
### Sketch2Terrain is an advanced generative 3D sketch mapping system that revolutionizes how we create sketch maps using Augmented Reality (AR).
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Teasor.png?raw=true "The apparatus and workflow of the interface.")
# Abstract
Sketch mapping is an established research tool to study human spatial decision-making and information processing by externalizing one's spatial knowledge through sketches. We present Sketch2Terrain, an advanced generative 3D sketch mapping system that radically changes how we create sketch maps using Augmented Reality (AR). Sketch2Terrain introduces a pipeline that combines freehand mid-air sketching to create connected curve networks with generative Artificial Intelligence (AI), enabling the real-time generation of realistic and high-fidelity 3D terrain in under 100 milliseconds. Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments, and serves as a unified interface for researchers to set up a complete research flow including data collection, material preparation, and experimentation effortlessly. A between-subject study (N=36) on terrain sketch mapping revealed that generative 3D sketch mapping improved efficiency by 38.4%, terrain topology accuracy by 12.5%, and landmark accuracy by up to 12.1%, with only a 4.7% trade-off in terrain elevation accuracy compared to freehand 3D sketch mapping. Additionally, generative 3D sketch mapping reduced 60.53% perceived strain and 39.46% stress over 2D sketch mapping. These findings underscore the potential of generative 3D sketch mapping for applications requiring a deep understanding of vertical dimensions. 

# Interface design

### Sketch2Terrain offers a homogeneous system for different modes with customizable functions tailored to specific needs.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Interface_while.png?raw=true "The interface design.")
(a) Observation Task Interface: This interface includes only the height adjustment function. An instruction panel displays task descriptions and timing information in front of the workspace. (b) Drawing Task Interface: This interface offers height adjustment, color change, redo, undo, and eraser functions. An instruction panel presents task descriptions and timing in front of the workspace. (c) Data Collection Mode Interface: By default, this interface uses a blue color scheme and includes functions for changing the terrain model and saving data.
### Sketch2Terrain provides real-time terrain generation, allowing users to iteratively sketch and visualise for more accurate results.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Workflow.png?raw=true "The workflow.")
The typical workflow for creating a 3D terrain with Sketch2Terrain is illustrated in the sequence. User-sketched strokes are highlighted in blue. The system automatically generates a well-connected curve network to facilitate terrain creation. As more strokes are added (progressing from (a) to (f)), the generated terrain becomes increasingly accurate. The second row from (g) to (l) shows the height map of the corresponding terrain model (altitude as color gradient).
### Sketch2Terrain utilizes the widely recognized pix2pix model to facilitate the transformation between sketches and height map of terrain.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Architecture.png?raw=true "The detail of the pipeline of the Pix2pix model.")
(a) The architecture of the AI model. (b) Performance versus model size and inference time. The area of the markers indicates the model size. (c) Comparison between the inference results for the Pix2Pix and diffusion-based models.
# User study
### Experiments were conducted in a controlled laboratory environment.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Setting.png?raw=true "Experimental setting.")
We recruited 36 participants (17 females, 19 males), aged between 18 and 41 years (M=25.00, SD=4.51), for a comprehensive user study. We designed a between-subjects experiment with three conditions corresponding to 2D, 3D, and Generative 3D sketch mapping concepts, respectively: (1) 2D condition, (2) 3D condition, and (3) AI condition. 

### The experimental design consisted of three main phases.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Design.png?raw=true "Experimental design.")
Experimental procedure: The experiment for 2D and 3D conditions was composed of tutorial and sketch mapping phases followed by the survey, while the AI condition had a voluntarily free creation phase (which is not shown in the picture). A different sketching interface was provided for each condition, as shown.

### Sample sketches produced by participants in 2D, 3D and AI conditions for eight scenes.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Example_Sketch.png?raw=true "Experimental design.")

# Results
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Example_Sketch.png?raw=true "Statistics_results.")
