## Installation 🚀

### 1. Create and activate conda environment

```
conda env create -f environment.yml
conda activate sketchmapping
```

### 2. Download sketch2terrain dataset (training dataset)

The training dataset could be download from:

* [Sketch2terrain_RAW_obj]() (Will release soon.)
* [Sketch2terrain_dataset]() (Will release soon.)

🚨: And it should be placed within the <dataroot> directory. We will also provide the RAW .obj file of the sketch collected from VR. To generate the sketch image from the .obj file, please refer to [prepare_dataset.py](prepare_terrain_training_pair/prepare_dataset.py).

### 3. Dataset Structure:

```
|
├── dataset                       <- Dataset for training
│   ├── train_A                   <- Sketch image for training
│   ├── train_B                   <- Terrain image for training
|   ├── test_A                    <- Sketch image for testing
│   ├── test_B                    <- Terrain image for testing
```

## How to use 🔨

### 1. Train models [Pix2pix](https://arxiv.org/pdf/1611.07004) 

64 model:
```
python terrain_generation/train.py --dataroot <dataroot> --name 64_unet_model --model pix2pix --ngf 64 --no_flip
```

32 model:
```
python terrain_generation/train.py --dataroot <dataroot> --name 32_unet_model --model pix2pix --ngf 32 --no_flip
```

16 model:
```
python terrain_generation/train.py --dataroot <dataroot> --name 16_unet_model --model pix2pix --ngf 16 --no_flip
```

### 2. Generate ONNX file for VR interaction
```
python build_onnx_binary/prepare_onnx.py --dataroot ./ --name <name_of_the_project> --model pix2pix
```

### 2. Acknowledgement 🤗
We appreciate the help from: 
* Public code [Pix2pix](https://github.com/junyanz/pytorch-CycleGAN-and-pix2pix)
