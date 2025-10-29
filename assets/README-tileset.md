# Sample Tileset (tiles.png)

This is a placeholder. To test Kitty graphics rendering, you need to create a PNG tileset:

## Requirements

- 16x16 pixel tiles
- Tile 0: Floor sprite (gray/dark)
- Tile 1: Wall sprite (brown/brick)
- Format: RGBA PNG

## Quick Creation

You can create this using any image editor or run the following Python script:

```python
from PIL import Image
img = Image.new('RGBA', (32, 16), (0, 0, 0, 255))
# Tile 0: Floor (dark gray)
for y in range(16):
    for x in range(16):
        img.putpixel((x, y), (80, 80, 80, 255))
# Tile 1: Wall (brown)
for y in range(16):
    for x in range(16, 32):
        img.putpixel((x, y), (139, 69, 19, 255))
img.save('assets/tiles.png')
```

The tileset will be loaded automatically if present at ./assets/tiles.png
