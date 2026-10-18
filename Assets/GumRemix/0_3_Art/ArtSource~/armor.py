"""Steel plating for the "can only be roped" enemies. Works on finished frames, so every animation keeps its own motion:
a helmet over the top of the silhouette and a breastplate band across the middle, both remapped to a steel ramp by the
luminance of the original pixels (the sprite's own shading survives), with a dark seam under each plate and a few rivets.
Eyes (near-white / near-black pixels) are left alone so the face still reads."""
import numpy as np
from PIL import Image

RAMP = [(0x2c, 0x34, 0x4a), (0x4b, 0x57, 0x76), (0x76, 0x86, 0xa8), (0xa8, 0xb8, 0xd4), (0xdc, 0xe6, 0xf6)]
SEAM = (0x1c, 0x22, 0x32)
RIVET = (0xf4, 0xf8, 0xff)


def _lum(p):
    return 0.30 * p[0] + 0.59 * p[1] + 0.11 * p[2]


DARK_RAMP = [(0x0c, 0x10, 0x20), (0x1c, 0x24, 0x40), (0x34, 0x40, 0x68), (0x60, 0x72, 0x9c), (0xa0, 0xb4, 0xd8)]


def armorize(im, helm=0.30, chest=(0.56, 0.74), chest_inset=0.12, seams=True, gamma=1.0, ramp=None, rivet=None, seam_col=None):
    ramp = ramp or RAMP; rivet = rivet or RIVET; seam_col = seam_col or SEAM
    a = np.array(im.convert('RGBA'))
    alpha = a[:, :, 3] > 0
    ys = np.where(alpha.any(axis=1))[0]
    if len(ys) == 0:
        return im.copy()
    top, bot = int(ys[0]), int(ys[-1])
    H = bot - top + 1
    out = a.copy()

    def plate(y0, y1, xinset):
        ls = []
        for y in range(y0, y1):
            for x in np.where(alpha[y])[0]:
                p = a[y, x]; l = _lum(p)
                if not (l > 232 and False) and max(p[0], p[1], p[2]) >= 64: ls.append(l)
        lo, hi = (min(ls), max(ls)) if ls else (0, 255)
        span = max(1.0, hi - lo)
        for y in range(y0, y1):
            xs = np.where(alpha[y])[0]
            if len(xs) == 0:
                continue
            x0, x1 = int(xs[0]), int(xs[-1])
            inset = int((x1 - x0 + 1) * xinset)
            for x in range(x0 + inset, x1 - inset + 1):
                if not alpha[y, x]:
                    continue
                p = a[y, x]
                l = _lum(p)
                if max(p[0], p[1], p[2]) < 64:       # eyes, mouth and the outline stay
                    continue
                k = min(len(ramp) - 1, max(0, int(((l - lo) / span) ** gamma * (len(ramp) - 0.01))))
                out[y, x, :3] = ramp[k]

    def seam(y, xinset):
        xs = np.where(alpha[y])[0]
        if len(xs) == 0:
            return
        x0, x1 = int(xs[0]), int(xs[-1])
        inset = int((x1 - x0 + 1) * xinset)
        for x in range(x0 + inset, x1 - inset + 1):
            if alpha[y, x]:
                out[y, x, :3] = seam_col

    h_end = top + max(3, int(H * helm))
    plate(top, h_end, 0.0)
    seam(h_end, 0.0)
    c0, c1 = top + int(H * chest[0]), top + int(H * chest[1])
    plate(c0, c1, chest_inset)
    if seams:
        seam(c0, chest_inset)
        seam(c1, chest_inset)
    # rivets: two on the helmet rim, two on the breastplate
    for (y, fx) in ((h_end - 1, 0.28), (h_end - 1, 0.72), (c0 + 2, 0.30), (c0 + 2, 0.70)):
        xs = np.where(alpha[y])[0]
        if len(xs) == 0:
            continue
        x = int(xs[0] + (xs[-1] - xs[0]) * fx)
        if alpha[y, x] and max(a[y, x][:3]) >= 64:
            out[y, x, :3] = rivet
    return Image.fromarray(out, 'RGBA')
