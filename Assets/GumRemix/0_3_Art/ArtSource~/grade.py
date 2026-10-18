"""Colour grading for the sad, dark ice world: pixels keep their relative contrast but lose saturation and brightness and
drift toward a cold blue-violet. Alpha and pixel structure are untouched, so animations and outlines stay exactly as drawn."""
import numpy as np
from PIL import Image


def grade(im, dark=0.55, curve=0.40, sat=0.65, tint=(-4, 2, 14)):
    a = np.array(im.convert('RGBA')).astype(np.float32)
    rgb = a[:, :, :3]
    lum = (0.299 * rgb[:, :, 0] + 0.587 * rgb[:, :, 1] + 0.114 * rgb[:, :, 2])[:, :, None]
    out = lum + (rgb - lum) * sat
    out = out * (dark + curve * lum / 255.0)
    out = out + np.array(tint, dtype=np.float32) * (1.0 - lum / 255.0 * 0.5)
    a[:, :, :3] = np.clip(out, 0, 255)
    return Image.fromarray(a.astype(np.uint8), 'RGBA')
