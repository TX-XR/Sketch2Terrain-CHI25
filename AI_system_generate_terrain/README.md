## Installation 🚀

### 1. Create and activate conda environment

```
conda env create -f environment.yml
conda activate sketchmapping
```

### 2. Download sketch2terrain dataset (training dataset)

The training dataset could be download from:

* [Sketch2terrain]() (Release soon.)

🚨: And it should be placed within the <dataroot> directory.

### 3. Project Structure:

Structure of this repository:

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
python train.py --dataroot <dataroot> --name 64_unet_model --model pix2pix --ngf 64 --no_flip
```

32 model:
```
python train.py --dataroot <dataroot> --name 32_unet_model --model pix2pix --ngf 32 --no_flip
```

16 model:
```
python train.py --dataroot <dataroot> --name 16_unet_model --model pix2pix --ngf 16 --no_flip
```

### 2. Acknowledgement 🤗
We appreciate the help from: 
* Public code [Pix2pix](https://github.com/junyanz/pytorch-CycleGAN-and-pix2pix)
