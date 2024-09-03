import numpy as np
import matplotlib.pyplot as plt
import os
from tqdm import tqdm

import cv2


def line_normalization(lines):
    normalized_lines = []
    for line in lines:
        # filter value smaller than -0.2 and 0.2
        line = np.array(line)

        ignore_x_index = np.array(np.where(line[:, 0] <= -0.2)).tolist() + np.array(np.where(line[:, 0] >= 0.2)).tolist()
        ingore_y_index = np.array(np.where(line[:, 1] <= -0.2)).tolist() + np.array(np.where(line[:, 1] >= 0.2)).tolist()
        ignore_index = ignore_x_index + ingore_y_index
        ignore_index = [item for sublist in ignore_index for item in sublist]

        if len(ignore_index) != 0:
            # Delete the element in array through index
            line = np.delete(line, ignore_index, axis=0)

        if len(line) == 0:
            continue

        # Normalize the line to the range [0, 1]
        line[:,:2] += 0.2
        line[:,:2] /= 0.4
        line[:,:2] *= 512

        # Maximum value should be 256
        line[:, 2] *= 255
        if line[:, 0].max() >= 512 or line[:, 1].max() >= 512:
            continue
        line = np.floor(line).astype(int)
        normalized_lines.append(line)
    return normalized_lines

def visualize_lines(normalized_lines, terrain, new_voxels):
    # Visualize line_seg into 3d scatter plot
    fig = plt.figure()
    ax = fig.add_subplot(111, projection='3d')
    for line in normalized_lines:
        line = np.array(line)
        ax.scatter(line[:, 0], line[:, 1], line[:, 2])
        ax.set_xlabel('X')
        ax.set_ylabel('Y')
        ax.set_zlabel('Z')

    grid_size = 512
    x = np.arange(grid_size)
    y = np.arange(grid_size)
    X, Y = np.meshgrid(x, y)
    ax.plot_surface(X, Y, terrain, cmap='terrain')
    # show x, y, z axis
    ax.set_xlabel('X')
    ax.set_ylabel('Y')
    ax.set_zlabel('Z')

    fig, axs = plt.subplots(1, 2)
    axs[0].imshow(terrain)
    axs[1].imshow(new_voxels)
    plt.show()


def plot_line_low(voxels, x1, y1, z1, x2, y2, z2):
    dx = x2 - x1
    dy = y2 - y1
    dz = z2 - z1
    xi = 1 if dx > 0 else -1
    yi = 1 if dy > 0 else -1
    zi = 1 if dz > 0 else -1
    dx, dy, dz = abs(dx), abs(dy), abs(dz)
    if dx >= dy and dx >= dz:  # X is the driving axis
        p1 = 2*dy - dx
        p2 = 2*dz - dx
        while x1 != x2:
            # print(x1, y1, z1)
            voxels[x1, y1, z1] = True
            if p1 >= 0:
                y1 += yi
                p1 -= 2*dx
            if p2 >= 0:
                z1 += zi
                p2 -= 2*dx
            p1 += 2*dy
            p2 += 2*dz
            x1 += xi
    elif dy >= dx and dy >= dz:  # Y is the driving axis
        p1 = 2*dx - dy
        p2 = 2*dz - dy
        while y1 != y2:
            voxels[x1, y1, z1] = True
            if p1 >= 0:
                x1 += xi
                p1 -= 2*dy
            if p2 >= 0:
                z1 += zi
                p2 -= 2*dy
            p1 += 2*dx
            p2 += 2*dz
            y1 += yi
    else:  # Z is the driving axis
        p1 = 2*dy - dz
        p2 = 2*dx - dz
        while z1 != z2:
            voxels[x1, y1, z1] = True
            if p1 >= 0:
                y1 += yi
                p1 -= 2*dz
            if p2 >= 0:
                x1 += xi
                p2 -= 2*dz
            p1 += 2*dy
            p2 += 2*dx
            z1 += zi
    voxels[x2, y2, z2] = True

def rasterize_line(voxels, x1, y1, z1, x2, y2, z2):
    plot_line_low(voxels, x1, y1, z1, x2, y2, z2)
    return voxels

def line_voxalization(normalized_lines, grid_size=512):
    voxels = np.zeros((grid_size, grid_size, grid_size), dtype=bool)
    # Rasterize the line
    for line in normalized_lines:
        for i in range(len(line)-1):
            x1, y1, z1 = line[i]
            x2, y2, z2 = line[i+1]
            voxels = rasterize_line(voxels, x1, y1, z1, x2, y2, z2)

    # Swape axis x and z
    voxels = voxels.astype(int)
    new_voxels = np.zeros((grid_size, grid_size))
    # Loop voxel elements
    for i in range(voxels.shape[0]):
        for j in range(voxels.shape[1]):
            if voxels[i, j, :].any():
                # print(i, j, voxels[i, j, :])
                new_voxels[i, j] = np.max(np.nonzero(voxels[i, j, :])[0])

    new_voxels = np.fliplr(new_voxels)
    new_voxels = np.rot90(new_voxels, 1)
    return new_voxels


def main():
    base_dir = '' # Enter the base directory

    train_A_dir = os.path.join(base_dir, 'train_A')
    train_B_dir = os.path.join(base_dir, 'train_B')
    test_A_dir = os.path.join(base_dir, 'test_A')
    test_B_dir = os.path.join(base_dir, 'test_B')

    os.makedirs(os.path.join(base_dir, 'train_A'), exist_ok=True)
    os.makedirs(os.path.join(base_dir, 'train_B'), exist_ok=True)
    os.makedirs(os.path.join(base_dir, 'test_A'), exist_ok=True)
    os.makedirs(os.path.join(base_dir, 'test_B'), exist_ok=True)

    obj_dir = '' # Enter the directory of the obj files

    list_files = os.listdir(obj_dir)
    for index, l in enumerate(tqdm(list_files)):
        lerp_strokes = os.path.join(obj_dir, l, 'LerpStrokes.curves')
        lines = [] # Declare an empty list to store the lines of the file
        # Open the file and read it line by line
        line_seg = []
        with open(lerp_strokes, 'r') as file:
            for line in file:
                if 'v' in line:
                    lines.append(np.array(line_seg))
                    line_seg = []
                else:
                    x, y, z = line.split(' ')
                    x, y, z = float(x), float(y), float(z)
                    line_seg.append([x, y, z])

        lines = lines[1:] # Remove first empty line
        normalized_lines = line_normalization(lines)

        # Catch if file is exist
        terrain_file = os.path.join(obj_dir, l, 'Terrain_heightmap.png')
        if not os.path.exists(terrain_file):
            terrain = np.zeros((512, 512))
        else:
            terrain = os.path.join(obj_dir, l, 'Terrain_heightmap.png')
            terrain = plt.imread(terrain)[:, :, 0]
            terrain -= terrain.min()

            # Resize the image to 512*512
            terrain = np.array(terrain)
            terrain = terrain[:512, :512]
            terrain *= 255
            terrain = np.rot90(terrain, 3)

            if len(np.unique(terrain)) == 1 or np.max(terrain) < 50:
                print('nearly empty terrain')
                continue

        new_voxels_proj= line_voxalization(normalized_lines)

        # save terrain, new_voxels_proj
        new_voxels_proj_save_path = os.path.join(train_A_dir, str(index)+'.png')
        # Save image into gray scale
        cv2.imwrite(new_voxels_proj_save_path, new_voxels_proj)
        terrain_save_path = os.path.join(train_B_dir, str(index)+'.png')
        cv2.imwrite(terrain_save_path, terrain)

        # save terrain, new_voxels_proj
        new_voxels_proj_save_path = os.path.join(test_A_dir, str(index)+'.png')
        # Save image into gray scale
        cv2.imwrite(new_voxels_proj_save_path, new_voxels_proj)
        terrain_save_path = os.path.join(test_B_dir, str(index)+'.png')
        cv2.imwrite(terrain_save_path, terrain)

if __name__ == '__main__':
    main()
