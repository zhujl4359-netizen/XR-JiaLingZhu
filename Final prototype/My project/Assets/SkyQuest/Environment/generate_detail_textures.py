"""Generate small, seamless architectural surface maps for the Quest scene."""
from pathlib import Path
import numpy as np
from PIL import Image, ImageFilter

OUT = Path(__file__).parent / 'Textures'
OUT.mkdir(exist_ok=True)
N = 512
rng = np.random.default_rng(72130)

def noise(grid, radius=1):
    small = rng.random((grid, grid)) * 255
    arr = Image.fromarray(small.astype('uint8'), 'L').resize((N, N), Image.Resampling.BICUBIC)
    if radius:
        arr = arr.filter(ImageFilter.GaussianBlur(radius))
    return np.asarray(arr, dtype=np.float32) / 255 - .5

coarse = noise(12, 7)
medium = noise(44, 2)
fine = noise(192, 0)
y, x = np.mgrid[:N, :N]
vein = np.sin(x * .039 + y * .014 + coarse * 8) ** 18
joint = ((x % 128 < 2) | (y % 128 < 2)).astype(float)
stone_height = .53 + coarse * .22 + medium * .065 + fine * .018 - vein * .055 - joint * .12
stone = np.clip((.77 + (stone_height - .53) * .67) * 255, 0, 255).astype('uint8')
Image.fromarray(stone, 'L').convert('RGB').save(OUT / 'Limestone_Detail.png', optimize=True)

def normal(height, strength, file):
    dx = np.roll(height, 1, 1) - np.roll(height, -1, 1)
    dy = np.roll(height, 1, 0) - np.roll(height, -1, 0)
    xyz = np.dstack((dx * strength, dy * strength, np.ones_like(height)))
    xyz /= np.linalg.norm(xyz, axis=2)[..., None]
    Image.fromarray(np.clip((xyz * .5 + .5) * 255, 0, 255).astype('uint8'), 'RGB').save(OUT / file, optimize=True)

normal(stone_height, 2.8, 'Limestone_Normal.png')

# A quiet woven surface keeps hanging banners readable without looking flat.
thread = np.sin(x * np.pi / 4) * .026 + np.sin(y * np.pi / 4) * .026
cloth_height = .5 + thread + fine * .035 + coarse * .018
cloth = np.clip((.86 + (cloth_height - .5) * 1.2) * 255, 0, 255).astype('uint8')
Image.fromarray(cloth, 'L').convert('RGB').save(OUT / 'Cloth_Detail.png', optimize=True)
normal(cloth_height, 4.0, 'Cloth_Normal.png')

# Preserve the source metal's fine grain while leaving gold hue to the material.
metal = Image.open(OUT / 'Metal_Color.jpg').convert('L')
metal.save(OUT / 'Metal_Detail.png', optimize=True)

# Large, soft bands for distant cloud banks; no weave or hard normal is used.
cloud_height = .75 + noise(8, 18) * .12 + noise(24, 9) * .045
Image.fromarray(np.clip(cloud_height * 255, 0, 255).astype('uint8'), 'L').convert('RGB').save(
    OUT / 'Cloud_Detail.png', optimize=True)

# Stylized foliage and fine vertical bark grain share the architecture's palette.
leaf_height = .72 + noise(26, 4) * .22 + noise(96, 1) * .08
Image.fromarray(np.clip(leaf_height * 255, 0, 255).astype('uint8'), 'L').convert('RGB').save(
    OUT / 'Leaf_Detail.png', optimize=True)
normal(leaf_height, 2.0, 'Leaf_Normal.png')
bark_height = .55 + np.sin(x * .13 + noise(12, 12) * 7) * .035 + fine * .04
Image.fromarray(np.clip((.78 + (bark_height - .55)) * 255, 0, 255).astype('uint8'), 'L').convert('RGB').save(
    OUT / 'Bark_Detail.png', optimize=True)
normal(bark_height, 2.0, 'Bark_Normal.png')
