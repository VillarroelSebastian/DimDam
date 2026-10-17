"""Animated scenery for the Reino de Hielo. Every function returns loop-seamless frames."""
import math, random
from px import *

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')

def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

SNOWP = pal('#f4fbff', '#ffffff', '#c6def4', '#96b4e0', '#34408a', '#b4cdee')
ICEP = pal('#9be0ff', '#e4faff', '#52a6e8', '#2c64b8', '#14306e', '#3a86d0')
PINE = pal('#2a5c7a', '#4a8aa8', '#1c4460', '#123048', '#0a1c30')
WOODP = pal('#a8703c', '#d89c60', '#7a4a2c', '#4e2c24', '#2a1418')

# ------------------------------------------------------------------ T5 lake
def aurora(n=8):
    """Two curtains of light. Streak positions advance exactly one streak spacing over the loop."""
    out = []
    for i in range(n):
        cv = Canvas(640, 150); t = i / n
        for band, (col_a, col_b, y0, amp) in enumerate(((hexc('#6af0b0'), hexc('#3ac8e8'), 50, 22), (hexc('#b074f0'), hexc('#ff79b8'), 80, 16))):
            for x in range(640):
                yy = y0 + amp * S(x * .018 + t * TAU + band * 1.7) + 8 * S(x * .047 - t * TAU * 2 + band)
                streak = (x + int(t * 16)) % 16 < 9
                top = int(yy - 26 - 10 * S(x * .06 + t * TAU))
                for y in range(max(0, top), int(yy) + 12):
                    f = (y - top) / max(1.0, (yy + 12 - top))
                    if not streak and (x + y) % 2: continue
                    a = int(150 * (1 - abs(f - .45) * 1.3)) if f < .45 else int(150 * (1 - (f - .45) * 1.7))
                    if a > 25:
                        c = col_a if f < .5 else col_b
                        if dither(x, y, 1 - min(1, a / 150.0)) and a < 90: continue
                        cv.px[x, y] = (c[0], c[1], c[2], min(170, a + 30))
        out.append(cv)
    return out

def pine_tree(n=8, seed=0):
    out = []
    for i in range(n):
        cv = Canvas(64, 120); sway = 2.6 * S(i / n * TAU + seed)
        def tx(y): return 32 + sway * ((112 - y) / 100.0) ** 2
        cv.put(rect(30, 106, 35, 116), hexc('#4e2c24')); cv.put(outline_of(rect(30, 106, 35, 116)), hexc('#2a1418'))
        for r in range(6):
            y = 24 + r * 15; wd = 8 + r * 4.6
            x = tx(y) + sway * 0.15 * (6 - r) * .5
            bough = poly([(x - wd, y + 16), (x, y - 6), (x + wd, y + 16)])
            part(cv, bough, PINE, hi=False, k=2)
            cv.put({(int(x - wd + k), y + 16) for k in range(0, int(wd * 2), 3)}, hexc('#e4f2ff'))
            cv.put(poly([(x - wd * .7, y + 9), (x - wd * .1, y - 2), (x + wd * .2, y + 3), (x - wd * .2, y + 10)]), hexc('#f4fbff'))     # snow on the left boughs
        part(cv, tri((tx(24) - 4, 30), (tx(24), 12), (tx(24) + 4, 30)), PINE, hi=False, k=1)
        out.append(cv)
    return out

def wisp(n=6):
    out = []
    for i in range(n):
        cv = Canvas(28, 36); t = i / n * TAU
        by = 14 + round(2 * S(t))
        for k in range(5):
            cv.put(ell(14 - k * 1.2 + S(t - k * .7) * 1.6, by + 7 + k * 3, 3.0 - k * .5, 2.6 - k * .4), hexc('#bfe8ff') if k < 3 else hexc('#7ab8ff'))
        cv.put(outline_of(ell(14, by, 6, 6)), hexc('#5a8ae0')); cv.put(ell(14, by, 6, 6), hexc('#e8f8ff')); cv.put(ell(14, by, 3.4, 3.4), WHITE)
        cv.put({(11, by - 1), (16, by - 1)}, hexc('#3a5ac0')); cv.put({(13, by + 2), (14, by + 2)}, hexc('#3a5ac0'))
        out.append(cv)
    return out

def frozen_fall(n=8):
    out = []
    for i in range(n):
        cv = Canvas(40, 124)
        for y in range(0, 112):
            x0, x1 = 6 + int(1.2 * S(y * .2)), 34 + int(1.2 * S(y * .2))
            for x in range(x0, x1):
                c = hexc('#9be0ff')
                ph = (y + x * 2 + i * 2) % 16
                if ph < 2: c = WHITE
                elif ph < 5: c = hexc('#cdf1ff')
                elif ph > 12: c = hexc('#52a6e8')
                if x >= x1 - 3: c = hexc('#2c64b8')
                if x < x0 + 2: c = hexc('#e4faff')
                cv.put([(x, y)], c)
            cv.put([(x0 - 1, y), (x1, y)], hexc('#14306e'))
        for k in range(5):
            bx = 8 + k * 6; cv.put(ell(bx, 112 - int(abs(S(i / n * TAU * 2 + k * 1.3)) * 4), 3, 2.6), hexc('#e4faff'))
        out.append(cv)
    return out

# ------------------------------------------------------------------ T6 village
def igloo(n=8):
    out = []
    for i in range(n):
        cv = Canvas(88, 96); t = i / n
        d = {p for p in ell(40, 74, 32, 26) if p[1] <= 74} | rect(8, 74, 73, 80)
        part(cv, d, SNOWP)
        for r in range(4):
            y = 70 - r * 8
            cv.put({(x, y) for x in range(8, 73) if (x, y) in d}, SNOWP['shade'])
            off = (r % 2) * 7
            cv.put({(x, yy) for x in range(8 + off, 73, 14) for yy in range(y - 7, y) if (x, yy) in d}, SNOWP['shade'])
        door = ell(40, 74, 9, 12) | rect(31, 74, 50, 80)
        cv.put(door & d, hexc('#2a3a7a')); cv.put(outline_of(door) & d, SNOWP['out'])
        cv.put(ell(40, 78, 4, 3), hexc('#ffd070'))                                         # warm light inside
        ch = rect(56, 36, 66, 52)
        part(cv, ch, dict(SNOWP, base=hexc('#a8703c'), shade=hexc('#7a4a2c'), deep=hexc('#4e2c24'), hi=hexc('#d89c60'), out=hexc('#2a1418')), hi=False, k=1)
        cv.put(rect(54, 34, 68, 38), hexc('#f4fbff'))
        for k in range(3):                                                                  # smoke curls
            ph = (t + k / 3.0) % 1.0
            r = 3 + ph * 5; x = 61 + 6 * S(ph * TAU * 1.2 + k * 2) ; y = 33 - ph * 32
            if ph < .85: cv.put(ell(x, y, r, r * .85), hexc('#eef4ff') if ph < .5 else hexc('#c6d4f0')); cv.put({(int(x) - 1, int(y) - 1)}, WHITE)
        out.append(cv)
    return out

def lanterns(n=8):
    out = []
    for i in range(n):
        cv = Canvas(160, 56); t = i / n * TAU
        pts = [(x, 6 + 7 * S(math.pi * x / 160.0) + 0) for x in range(0, 160)]
        for x, y in pts: cv.put([(x, int(y))], hexc('#4e2c24'))
        for k, x0 in enumerate((20, 52, 84, 116, 144)):
            sw = 2.2 * S(t + k * .9)
            y0 = int(6 + 7 * S(math.pi * x0 / 160.0))
            cv.put(line(x0, y0, x0 + sw, y0 + 8, .5), hexc('#4e2c24'))
            lx, ly = x0 + sw, y0 + 8
            col = [hexc('#ff8a5a'), hexc('#ffd070'), hexc('#7af0ff'), hexc('#ff79b8'), hexc('#a8ff9a')][k]
            body = ell(lx, ly + 7, 6, 8)
            cv.put(outline_of(body), hexc('#3a2a44')); cv.put(body, col)
            flick = .6 + .4 * S(t * 2 + k)
            cv.put(ell(lx, ly + 7, 3 * flick + 1, 4 * flick + 1), hexc('#fff6c8')); cv.put({(int(lx) - 3, int(ly) + 3)}, WHITE)
            cv.put(rect(int(lx) - 3, int(ly) - 1, int(lx) + 4, int(ly) + 1), hexc('#6a4a3a'))
            for gy in range(int(ly) + 14, int(ly) + 24):         # glow dither under the lantern
                for gx in range(int(lx) - 6, int(lx) + 7):
                    if dither(gx, gy, .33 * flick * (1 - (gy - ly - 14) / 10.0)): cv.put([(gx, gy)], col)
        out.append(cv)
    return out

def wash_flags(n=8):
    out = []
    cols = [hexc('#7ab4ff'), hexc('#ffffff'), hexc('#ff9fb4'), hexc('#ffd070')]
    for i in range(n):
        cv = Canvas(160, 44); t = i / n * TAU
        for x in range(160): cv.put([(x, int(7 + 4 * S(TAU * x / 80.0)))], hexc('#4e2c24'))
        for k in range(7):
            x0 = 14 + k * 20; y0 = int(7 + 4 * S(TAU * x0 / 80.0)); fl = 2.0 * S(t + k * .9)
            tri_ = poly([(x0 - 8, y0 + 1), (x0 + 8, y0 + 1), (x0 + 6 + fl, y0 + 18), (x0 - 6 + fl, y0 + 19)])
            cv.put(outline_of(tri_), hexc('#3a2a5a')); cv.put(tri_, cols[k % 4])
            cv.put({p for p in tri_ if (p[0] + 1, p[1] + 1) not in tri_}, mix(cols[k % 4][:3], hexc('#5a4a9a'), .4))
            cv.put({(x0 - 4 + a, y0 + 5) for a in range(5)}, WHITE if k % 4 != 1 else hexc('#7ab4ff'))
        out.append(cv)
    return out

# ------------------------------------------------------------------ T7 crystals
def glow_crystal(n=6, scheme=0):
    out = []
    P = [pal('#52e8f0', '#d4ffff', '#2aa8d0', '#1c6aa8', '#10204a'), pal('#ff6ad0', '#ffd0f4', '#d03aa0', '#8a2478', '#2a0a40'), pal('#8af08a', '#e4ffd0', '#3ab86a', '#1c7a58', '#0a2a30')][scheme]
    for i in range(n):
        cv = Canvas(56, 80); t = i / n * TAU; pulse = .5 + .5 * S(t)
        for y in range(80):
            for x in range(56):
                d = math.hypot((x - 28) / 28.0, (y - 56) / 30.0)
                if .3 < d < .55 + .45 * pulse and dither(x, y, (1 - d) * .5): cv.put([(x, y)], P['hi'])
        for (cx, h, wd) in ((28, 64, 9), (14, 40, 7), (43, 46, 7), (22, 28, 5)):
            top = 78 - h
            sh = poly([(cx - wd, 78), (cx - wd, top + 10), (cx, top), (cx + wd, top + 10), (cx + wd, 78)])
            part(cv, sh, P)
            cv.put({(cx, y) for y in range(top + 3, 74) if (cx, y) in sh}, P['hi'])
            if pulse > .6: cv.put({(cx - wd + 2, top + 12 + k) for k in range(5)}, WHITE)
        out.append(cv)
    return out

# ------------------------------------------------------------------ T8 garden
def owl_statue(n=6):
    out = []
    for i in range(n):
        cv = Canvas(52, 88); sweep = i / n
        ped = rect(8, 66, 44, 86)
        part(cv, ped, pal('#a8bcc8', '#e4f0f8', '#7a909e', '#5a6e7a', '#2a3a44'), hi=True, k=2)
        cv.put(rect(6, 62, 46, 68), hexc('#c4d4de')); cv.put(outline_of(rect(6, 62, 46, 68)), hexc('#2a3a44'))
        body = ell(26, 42, 13, 20) | ell(26, 22, 11, 10)
        part(cv, body, pal('#cfe8fb', '#ffffff', '#92bce4', '#6a94cc', '#1c3a78'), hi=True, k=2)
        for sx in (-1, 1): cv.put(tri((26 + sx * 8, 14), (26 + sx * 11, 4), (26 + sx * 3, 12)), hexc('#cfe8fb'));
        cv.put({(19 + a, 22) for a in range(4)} | {(30 + a, 22) for a in range(4)}, hexc('#1c3a78')); cv.put(tri((24, 26), (28, 26), (26, 31)), hexc('#92bce4'))
        for k in range(4): cv.put({(17 + a + (k % 2) * 3, 40 + k * 5) for a in (0, 1, 2, 3)}, hexc('#92bce4'))
        # a glint travels diagonally across the statue and loops
        gx = int(-10 + sweep * 80)
        for k in range(26):
            x, y = gx + k // 2, 6 + k * 3
            if 0 <= x < 52 and 0 <= y < 86 and cv.px[x, y][3] and cv.px[x, y][:3] != hexc('#1c3a78')[:3]: cv.put([(x, y), (x + 1, y)], WHITE)
        out.append(cv)
    return out

def garden_lamp(n=4):
    out = []
    for i in range(n):
        cv = Canvas(32, 72); fl = [1.0, 1.3, .8, 1.15][i]
        part(cv, rect(14, 18, 18, 70), pal('#5a6e7a', '#a0b4c0', '#3a4a56', '#28343e', '#10181e'), hi=False, k=1)
        part(cv, rect(8, 66, 24, 72), pal('#5a6e7a', '#a0b4c0', '#3a4a56', '#28343e', '#10181e'), hi=False, k=1)
        cage = rect(9, 4, 23, 20); cv.put(cage, hexc('#ffe9b0')); cv.put(outline_of(cage), hexc('#28343e'))
        cv.put(ell(16, 14, 3.4 * fl, 5 * fl), hexc('#ff9a3a')); cv.put(ell(16, 15, 2 * fl, 3.4 * fl), hexc('#fff6b0'))
        cv.put(tri((7, 5), (16, -2), (25, 5)) if False else poly([(7, 5), (16, 0), (25, 5)]), hexc('#5a6e7a'))
        out.append(cv)
    return out

# ------------------------------------------------------------------ T9 palace
def chandelier(n=8):
    out = []
    for i in range(n):
        cv = Canvas(96, 100); t = i / n * TAU; sw = 3.0 * S(t)
        ax = 48
        cv.put(line(ax, 0, ax + sw * .3, 16, .5), hexc('#8a6a2a'))
        cx = ax + sw
        for k in range(3):
            r = 26 - k * 7; y = 26 + k * 18
            arc = ell(cx, y, r, 6)
            cv.put(outline_of(arc), hexc('#8a6a2a')); cv.put({p for p in arc if p[1] <= y}, hexc('#ffe9a0')); cv.put({p for p in arc if p[1] > y}, hexc('#e8982a'))
            for j in range(6 + (2 - k)):
                x = cx - r + 3 + j * (2 * r - 6) / max(1, 5 + (2 - k))
                ln = 8 + (j * 5 + k * 3) % 7 + (3 if (i + j + k) % 8 == 0 else 0)
                cv.put(poly([(x - 2, y + 5), (x + 2, y + 5), (x, y + 5 + ln)]), hexc('#bfeaff')); cv.put({(int(x) - 1, int(y) + 7)}, WHITE)
                if (i + j * 3 + k) % 8 == 0: cv.put({(int(x), int(y) + 6 + ln), (int(x) - 1, int(y) + 6 + ln), (int(x) + 1, int(y) + 6 + ln), (int(x), int(y) + 5 + ln), (int(x), int(y) + 7 + ln)}, WHITE)
            cv.put(ell(cx, y - 6, 1.6, 3), hexc('#fff6b0'))
        out.append(cv)
    return out

def royal_banner(n=8):
    out = []
    for i in range(n):
        cv = Canvas(40, 112); t = i / n * TAU
        cv.put(rect(2, 0, 38, 4), hexc('#ffd23a')); cv.put(outline_of(rect(2, 0, 38, 4)), hexc('#4a2410'))
        for y in range(4, 100):
            wave = 2.6 * S(y * .09 - t) * min(1.0, (y - 4) / 30.0)
            x0, x1 = int(5 + wave), int(35 + wave)
            for x in range(x0, x1): cv.put([(x, y)], hexc('#3a5ccc') if (y // 2 + x // 6) % 6 else hexc('#4a70e0'))
            cv.put([(x0, y), (x1 - 1, y)], hexc('#ffd23a'))
        # swallow-tail bottom
        for k in range(-6, 7):
            y = 100 + (6 - abs(k)); x = int(20 + k + 2.6 * S(100 * .09 - t))
            cv.put([(x, y)], hexc('#3a5ccc'))
        cx = int(20 + 2.6 * S(50 * .09 - t))
        sn = poly([(cx, 36), (cx + 9, 46), (cx, 56), (cx - 9, 46)]); cv.put(sn, hexc('#e4f2ff')); cv.put(outline_of(sn), hexc('#ffd23a'))
        cv.put({(cx, 44), (cx, 46), (cx, 48), (cx - 2, 46), (cx + 2, 46)}, hexc('#3a5ccc'))
        out.append(cv)
    return out

def throne(n=4):
    out = []
    for i in range(n):
        cv = Canvas(88, 120)
        back = poly([(18, 116), (18, 40), (30, 14), (44, 4), (58, 14), (70, 40), (70, 116)])
        part(cv, back, pal('#b8d8ff', '#ffffff', '#7aa8e8', '#4a78c8', '#14306e'), hi=True, k=2)
        for sx in (-1, 1):
            for k in range(3): cv.put(tri((44 + sx * (14 + k * 8) - 3, 34 + k * 10), (44 + sx * (18 + k * 8), 20 + k * 10), (44 + sx * (14 + k * 8) + 3, 36 + k * 10)), hexc('#e4f2ff'))
        seat = rect(14, 84, 74, 100); part(cv, seat, pal('#b8d8ff', '#ffffff', '#7aa8e8', '#4a78c8', '#14306e'), hi=True, k=2)
        cv.put(rect(18, 88, 70, 98), hexc('#8e2c7c')); cv.put(rect(18, 88, 70, 90), hexc('#c4549e'))
        cv.put(rect(10, 70, 18, 100), hexc('#a8c8f4')); cv.put(rect(70, 70, 78, 100), hexc('#a8c8f4'))
        for (gx, gy) in ((44, 30), (30, 52), (58, 52)):
            cv.put(ell(gx, gy, 3, 3), [hexc('#ff79b8'), hexc('#ffe36a'), hexc('#7af0ff')][(gx // 14) % 3]); cv.put({(gx - 1, gy - 1)}, WHITE)
        if i % 2: cv.put({(24, 24), (23, 24), (25, 24), (24, 23), (24, 25)}, WHITE)
        else: cv.put({(64, 38), (63, 38), (65, 38), (64, 37), (64, 39)}, WHITE)
        out.append(cv)
    return out

# ------------------------------------------------------------------ particles
def snow_sheet():
    """Eight flakes: three sizes with a little twinkle, used by the falling-snow ambience."""
    out = []
    for i in range(8):
        cv = Canvas(8, 8)
        if i % 4 == 0: cv.put({(3, 3), (4, 3), (3, 4), (4, 4)}, WHITE)
        elif i % 4 == 1: cv.put({(3, 3), (4, 3), (3, 4), (4, 4), (3, 2), (3, 5), (2, 3), (5, 3)} if i > 3 else {(3, 3), (4, 3), (3, 4), (4, 4)}, WHITE)
        elif i % 4 == 2: cv.put({(4, 4)}, WHITE)
        else: cv.put({(3, 3), (4, 3), (3, 4), (4, 4), (4, 1), (4, 6), (1, 4), (6, 4)} if i > 3 else {(3, 3), (4, 3)}, hexc('#dbeaff'))
        out.append(cv)
    return out

def spark_ice(n=4):
    out = []
    for i in range(n):
        cv = Canvas(12, 12); L = [1, 3, 4, 2][i]
        for k in range(-L, L + 1): cv.put([(6 + k, 6), (6, 6 + k)], hexc('#e4fbff'))
        cv.put([(6, 6)], WHITE); out.append(cv)
    return out
