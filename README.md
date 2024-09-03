# Sketch2Terrain: AI-Driven Real-Time Universal-Style Terrain Sketch Mapping in AR
Sketch2Terrain empowers non-experts to create concise and unambiguous sketch maps of natural environments. The system also serves as a comprehensive tool for researchers, providing a unified interface that supports the entire research workflow, including data collection, material preparation, and experimentation.
### Sketch2Terrain is an advanced generative 3D sketch mapping system that revolutionizes how we create sketch maps using Augmented Reality (AR).
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Teasor.png?raw=true "The apparatus and workflow of the interface.")
### Sketch2Terrain offers a homogeneous system for different modes with customizable functions tailored to specific needs.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Interface_while.png?raw=true "The interface design.")
(a) Observation Task Interface: This interface includes only the height adjustment function. An instruction panel displays task descriptions and timing information in front of the workspace. (b) Drawing Task Interface: This interface offers height adjustment, color change, redo, undo, and eraser functions. An instruction panel presents task descriptions and timing in front of the workspace. (c) Data Collection Mode Interface: By default, this interface uses a blue color scheme and includes functions for changing the terrain model and saving data.
### Sketch2Terrain provides real-time terrain generation, allowing users to iteratively sketch and visualise for more accurate results.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Workflow.png?raw=true "The workflow.")
The typical workflow for creating a 3D terrain with Sketch2Terrain is illustrated in the sequence. User-sketched strokes are highlighted in blue. The system automatically generates a well-connected curve network to facilitate terrain creation. As more strokes are added (progressing from (a) to (f)), the generated terrain becomes increasingly accurate. The second row from (g) to (l) shows the height map of the corresponding terrain model (altitude as color gradient).
### Sketch2Terrain utilizes the widely recognized pix2pix model to facilitate the transformation between height map of sketches and height map of terrain shapes.
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Architecture.png?raw=true "The detail of the pipeline of the Pix2pix model.")

# User study
![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Setting.png?raw=true "Experimental setting.")
We recruited 36 participants (17 females, 19 males), aged between 18 and 41 years (M=25.00, SD=4.51), for a comprehensive user study in a controlled laboratory setting. We designed a between-subjects experiment with three conditions corresponding to 2D, 3D, and Generative 3D sketch mapping concepts, respectively: (1) 2D condition, (2) 3D condition, and (3) AI condition. 

![Alt text](https://github.com/ETH-IKG/Sketch2Terrain/blob/main/images/Experiment_Design.png?raw=true "Experimental design.")
Experimental procedure: The experiment for 2D and 3D conditions was composed of tutorial and sketch mapping phases followed by the survey, while the AI condition had a voluntarily free creation phase (which is not shown in the picture). A different sketching interface was provided for each condition, as shown.
