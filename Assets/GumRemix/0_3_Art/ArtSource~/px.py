"""Tiny pixel-art toolkit: masks (sets of (x,y)), selective outlines, fixed light, dithering.
Everything is drawn at native resolution. No antialiasing, no gradients except ordered dithering."""
import math
from PIL import Image

def hexc(h, a=255):
    h = h.lstrip('#')
    return (int(h[0:2], 16), int(h[2:4], 16), int(h[4:6], 16), a)

CLEAR = (0, 0, 0, 0)

class Canvas:
    def __init__(self, w, h):
        self.w, self.h = w, h
        self.im = Image.new('RGBA', (w, h), CLEAR)
        self.px = self.im.load()
    def inb(self, p): return 0 <= p[0] < self.w and 0 <= p[1] < self.h
    def put(self, pts, col):
        for p in pts:
            if 0 <= p[0] < self.w and 0 <= p[1] < self.h: self.px[p[0], p[1]] = col
    def get(self, x, y): return self.px[x, y] if 0 <= x < self.w and 0 <= y < self.h else CLEAR

# ---------------- masks ----------------
def ell(cx, cy, rx, ry, W=512, H=512):
    m = set()
    if rx <= 0 or ry <= 0: return m
    for y in range(max(0, int(cy - ry - 1)), int(cy + ry + 2)):
        for x in range(max(0, int(cx - rx - 1)), int(cx + rx + 2)):
            dx, dy = (x + .5 - cx) / rx, (y + .5 - cy) / ry
            if dx*dx + dy*dy <= 1: m.add((x, y))
    return m

def line(x0, y0, x1, y1, r):
    m = set(); n = max(1, int(max(abs(x1-x0), abs(y1-y0)) * 2))
    for i in range(n + 1):
        t = i / n; m |= ell(x0 + (x1-x0)*t, y0 + (y1-y0)*t, r, r)
    return m

def rect(x0, y0, x1, y1): return {(x, y) for x in range(int(x0), int(x1)) for y in range(int(y0), int(y1))}

def poly(pts):
    xs = [p[0] for p in pts]; ys = [p[1] for p in pts]
    m = set(); n = len(pts)
    for y in range(int(min(ys)) - 1, int(max(ys)) + 2):
        for x in range(int(min(xs)) - 1, int(max(xs)) + 2):
            px_, py_ = x + .5, y + .5; inside = False; j = n - 1
            for i in range(n):
                xi, yi = pts[i]; xj, yj = pts[j]
                if (yi > py_) != (yj > py_) and px_ < (xj - xi) * (py_ - yi) / (yj - yi + 1e-9) + xi: inside = not inside
                j = i
            if inside: m.add((x, y))
    return m

def tri(a, b, c): return poly([a, b, c])
def shift(m, dx, dy): return {(x + dx, y + dy) for x, y in m}
def outline_of(m): return {(x+d[0], y+d[1]) for x, y in m for d in ((1,0),(-1,0),(0,1),(0,-1))} - m
def ring(m, n=1):
    cur = set(m)
    for _ in range(n): cur = cur | outline_of(cur)
    return cur - m
def bbox(m):
    xs = [p[0] for p in m]; ys = [p[1] for p in m]
    return min(xs), min(ys), max(xs), max(ys)

# ---------------- shading ----------------
def part(cv, mask, pal, hi=True, outline=True, shade=True, k=None):
    """Flat base + 1px rim light (top-left) + crescent shadow (bottom-right) + deeper corner.
    Selective outline: darker on the shadow side, softer on the lit side."""
    if not mask: return
    x0, y0, x1, y1 = bbox(mask); cx, cy = (x0 + x1) / 2, (y0 + y1) / 2
    if k is None: k = 2 if len(mask) > 36 else 1
    if outline:
        for o in outline_of(mask):
            lit = ((o[0]+1, o[1]) in mask or (o[0], o[1]+1) in mask) and not ((o[0]-1, o[1]) in mask or (o[0], o[1]-1) in mask)
            cv.put([o], pal.get('out_l', pal['out']) if lit else pal['out'])
    cv.put(mask, pal['base'])
    if shade:
        sh = {p for p in mask if (p[0]+k, p[1]+k) not in mask}
        cv.put(sh, pal['shade'])
        cv.put({p for p in sh if (p[0]+1, p[1]+1) not in mask and p[0] >= cx and p[1] >= cy}, pal['deep'])
        if hi and len(mask) > 30:
            cv.put({p for p in mask if (p[0]-1, p[1]-1) not in mask and p[0] <= cx + 1 and p[1] <= cy and p not in sh}, pal['hi'])

# ordered 4x4 Bayer dithering
BAYER = [[0, 8, 2, 10], [12, 4, 14, 6], [3, 11, 1, 9], [15, 7, 13, 5]]
def dither(x, y, t):
    """True where the 'upper' colour should be used for blend factor t in [0,1]."""
    return (BAYER[y % 4][x % 4] + .5) / 16 < t

def mix(a, b, t): return tuple(int(a[i] + (b[i]-a[i]) * t) for i in range(3)) + (255,)

def silhouette(im, col=(255, 255, 255, 255)):
    out = im.copy(); p = out.load()
    for y in range(out.height):
        for x in range(out.width):
            if p[x, y][3]: p[x, y] = col
    return out

def sheet(frames, cols, cw, ch):
    rows = (len(frames) + cols - 1) // cols
    sh = Image.new('RGBA', (cols*cw, rows*ch), CLEAR)
    for i, f in enumerate(frames):
        sh.paste(f.im if isinstance(f, Canvas) else f, ((i % cols)*cw, (i//cols)*ch))
    return sh

def ncolors(im): return len({p for p in im.getdata() if p[3]})


def rotate(cv, angle):
    """Nearest-neighbour rotation about the centre (used for spinning props: no smoothing, stays crisp)."""
    import math
    out = Canvas(cv.w, cv.h); cx, cy = cv.w / 2.0, cv.h / 2.0
    c, s = math.cos(angle), math.sin(angle)
    for y in range(cv.h):
        for x in range(cv.w):
            dx, dy = x + .5 - cx, y + .5 - cy
            sx, sy = int(cx + dx * c + dy * s), int(cy - dx * s + dy * c)
            if 0 <= sx < cv.w and 0 <= sy < cv.h:
                p = cv.px[sx, sy]
                if p[3]: out.px[x, y] = p
    return out

def edge_touch(frames):
    """Indices of frames that have opaque pixels on the cell border (would be clipped)."""
    bad = []
    for i, f in enumerate(frames):
        w, h = f.w, f.h
        if any(f.px[x, y][3] for x in (0, w - 1) for y in range(h)) or any(f.px[x, y][3] for y in (0, h - 1) for x in range(w)):
            bad.append(i)
    return bad


def pad_canvas(cv, l=0, t=0, r=0, b=0):
    """Enlarge a cell without touching the drawing (so poses can never be clipped)."""
    out = Canvas(cv.w + l + r, cv.h + t + b)
    out.im.alpha_composite(cv.im, (l, t))
    return out

def pad_family(F, l=0, t=0, r=0, b=0):
    return {k: [pad_canvas(c, l, t, r, b) for c in v] for k, v in F.items()}

def feet_pivot(frame):
    """Pivot y (fraction from the bottom) that puts the lowest opaque row of `frame` on the ground."""
    for y in range(frame.h - 1, -1, -1):
        if any(frame.px[x, y][3] for x in range(frame.w)):
            return (frame.h - 1 - y) / float(frame.h)
    return 0.0
