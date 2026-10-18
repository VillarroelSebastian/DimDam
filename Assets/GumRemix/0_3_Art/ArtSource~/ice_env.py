"""Reino de Hielo scenery: five arenas (themes 5..9) with sky, far/mid layers, ledge/ground/wall tiles and the special ledge kinds."""
import math, random
from px import *
from env_bg import banded_sky, periodic_height, hills, SW, SH, full

def _hs(s): return sum(ord(c) * (i + 1) for i, c in enumerate(s))

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')

def peaks(w, h, base, comps, col, rim, deep, dither_h=60, sharp=1.0):
    """Seamless mountain range: sharp ridges (|sin|) instead of rolling hills."""
    cv = Canvas(w, h)
    for x in range(w):
        v = 0.0
        for a, k, ph in comps:
            v += a * (1.0 - abs(S(TAU * k * x / w + ph))) ** sharp
        y0 = int(base - v)
        for y in range(max(0, y0), h):
            c = col
            if y - y0 < 2: c = rim
            elif y - y0 < 14 and dither(x, y, 1.0 - (y - y0) / 14.0) and (x // 3 + y // 3) % 3: c = rim      # snow cap that thins out
            elif dither_h and y > y0 + 16 and dither(x, y, min(1.0, (y - y0 - 16) / dither_h)): c = deep
            cv.px[x, y] = c
    return cv

def snow_dots(cv, rng, n, y0, y1, col=WHITE):
    for _ in range(n):
        x = rng.randrange(0, cv.w); y = rng.randrange(y0, y1)
        if cv.px[x, y][3]: cv.put([(x, y)], col)

# ===================================================================================== skies / layers
def theme_lake():
    """T5 Lago de los Susurros: blue dusk with a faint aurora wash, snowy peaks, dark pines."""
    rng = random.Random(51)
    sky = banded_sky([(50, hexc('#070a1c')), (100, hexc('#0e1632')), (150, hexc('#182848')), (200, hexc('#2a4468')), (245, hexc('#4a6888')), (360, hexc('#7c94ac'))], zone=14)
    for _ in range(70):   # stars in the dark top
        x, y = rng.randrange(SW), rng.randrange(0, 110)
        sky.put([(x, y)], WHITE if rng.random() < .5 else hexc('#a8c4ec'))
    far = peaks(SW, 230, 120, [(70, 2, .4), (38, 5, 1.7), (16, 11, .2)], hexc('#9cb8e4'), hexc('#ffffff'), hexc('#7a98d0'), 60, sharp=1.1)
    mid = Canvas(SW, 232)
    # pine forest silhouette (seamless: trees sit on a grid that repeats at SW). Base of the canvas = ground line.
    for x in range(10, SW, 26):
        h = 84 + int(18 * S(x * .21)) + (x * 7) % 13
        for r in range(7):
            y = 232 - h + r * (h // 7)
            wd = 3 + r * 3.2
            mid.put(poly([(x - wd, y + 13), (x, y - 3), (x + wd, y + 13)]), hexc('#1a2e62') if r % 2 else hexc('#223a78'))
            mid.put({(int(x - wd + k), y + 13) for k in range(0, int(wd * 2), 2)}, hexc('#e4f2ff') if r % 2 == 0 else hexc('#a8c4ec'))
            mid.put({(int(x - wd * .6 + k), y + 5 + (k % 3)) for k in range(0, int(wd * 1.2))}, hexc('#2e4a8c'))
        mid.put(rect(x - 2, 232 - 8, x + 3, 232), hexc('#3a2a44'))
    return sky, full(far, 100), full(mid, 88)

def theme_village():
    """T6 Aldea de Iglus Torcidos: pale violet dawn, crooked igloos on snowy hills."""
    rng = random.Random(52)
    sky = banded_sky([(55, hexc('#161430')), (105, hexc('#241e48')), (155, hexc('#3c3060')), (205, hexc('#5a4a78')), (250, hexc('#7c6a88')), (360, hexc('#9a8a98'))], zone=12)
    far = peaks(SW, 220, 118, [(44, 2, 1.1), (26, 6, .6), (10, 13, 2.2)], hexc('#b8aee0'), hexc('#ffffff'), hexc('#9a90d0'), 55, sharp=1.0)
    mid = hills(SW, 230, 100, [(16, 3, .3), (8, 8, 1.4)], hexc('#e4e0fa'), hexc('#ffffff'), hexc('#c0b8ec'), 80)
    # little crooked igloos on the mid hills (static silhouettes; animated ones are props)
    for x0 in (60, 250, 420, 560):
        yb = int(100 - periodic_height(x0, [(16, 3, .3), (8, 8, 1.4)])) + 6
        d = ell(x0, yb + 4, 20 + (x0 % 7), 15)
        d = {p for p in d if p[1] <= yb + 2}
        mid.put(outline_of(d), hexc('#6a6ab0')); mid.put(d, hexc('#fbfdff'))
        mid.put({p for p in d if (p[0] + 2, p[1] + 2) not in d}, hexc('#c0c4ec'))
        mid.put(ell(x0 + 4, yb + 2, 5, 5) & d, hexc('#4a4a90'))
        for r in range(2): mid.put({(x, yb - 6 - r * 7) for x in range(x0 - 15, x0 + 16) if (x, yb - 6 - r * 7) in d}, hexc('#c0c4ec'))
    return sky, full(far, 120), full(mid, 130)

def theme_crystal():
    """T7 Cristales Cantores: dark teal cave full of crystal spires."""
    rng = random.Random(53)
    sky = banded_sky([(60, hexc('#050c20')), (120, hexc('#0a1a3a')), (180, hexc('#0e2a50')), (240, hexc('#143a62')), (360, hexc('#1a4870'))], zone=16)
    far = Canvas(SW, 260)
    for x in range(0, SW, 1):
        v = 0
        for a, k, ph in ((36, 3, .5), (20, 7, 1.4), (10, 17, .2)): v += a * (1 - abs(S(TAU * k * x / SW + ph)))
        for y in range(int(150 - v), 260):
            c = hexc('#1a3c78') if y - int(150 - v) > 2 else hexc('#4a86d0')
            if y - int(150 - v) > 20 and dither(x, y, min(1, (y - int(150 - v) - 20) / 60.0)): c = hexc('#122a58')
            far.px[x, y] = c
    # big faceted crystals in the far layer (periodic placement)
    for x0 in (40, 170, 330, 470, 590):
        h = 70 + (x0 * 3) % 40
        sp = poly([(x0 - 14, 260), (x0 - 12, 260 - h * .55), (x0, 260 - h), (x0 + 12, 260 - h * .55), (x0 + 14, 260)])
        far.put(outline_of(sp), hexc('#0a1a3a')); far.put(sp, hexc('#3a78c8'))
        far.put({p for p in sp if p[0] > x0}, hexc('#2a58a8')); far.put({(x0 - 4, y) for y in range(260 - int(h * .8), 260 - int(h * .3))}, hexc('#8ad0ff'))
    mid = Canvas(SW, 260)
    for x in range(SW):
        ln = 34 + periodic_height(x, [(24, 2, 2.1), (14, 5, .2), (8, 11, 1.0)])
        for y in range(0, int(max(8, ln))):
            mid.px[x, y] = hexc('#0e2450') if y < ln - 3 else hexc('#3a76c4') if y >= ln - 1 else hexc('#24488c')
    for x0 in (50, 160, 270, 400, 520, 610):
        h = 40 + (x0 * 5) % 34
        sp = poly([(x0 - 9, 0), (x0 + 9, 0), (x0 + 2, h), (x0 - 1, h + 8)])
        mid.put(outline_of(sp), hexc('#0a1a3a')); mid.put(sp, hexc('#3a8ae0'))
        mid.put({p for p in sp if p[0] > x0 - 2}, hexc('#2a62b8')); mid.put({(x0 - 5, y) for y in range(4, int(h * .7))}, hexc('#bfeaff'))
    return sky, full(far, 100), full(mid, 0)

def theme_garden():
    """T8 Jardines Congelados: green-grey twilight, frozen hedges and statues."""
    rng = random.Random(54)
    sky = banded_sky([(60, hexc('#1c2432')), (110, hexc('#2c3a4c')), (160, hexc('#40566a')), (210, hexc('#5a7084')), (260, hexc('#7488a0')), (360, hexc('#94a4b4'))], zone=12)
    far = peaks(SW, 220, 122, [(34, 2, .9), (20, 6, .1), (8, 14, 1.9)], hexc('#8cb0b0'), hexc('#f0fbff'), hexc('#70989c'), 50, sharp=.9)
    mid = Canvas(SW, 230)
    # frozen hedge wall with arches (periodic)
    for x0 in range(0, SW, 80):
        hed = rect(x0 + 2, 100, x0 + 78, 230)
        mid.put(outline_of(hed), hexc('#244844')); mid.put(hed, hexc('#3a6a64'))
        for y in range(104, 230, 8): mid.put({(x, y) for x in range(x0 + 4, x0 + 76) if (x // 2 + y // 4) % 3 == 0}, hexc('#4a8078'))
        mid.put({(x, y) for x in range(x0 + 2, x0 + 78) for y in range(100, 106) if (x * 3 + y) % 4 < 3}, hexc('#f0fbff'))
        arch = ell(x0 + 40, 170, 17, 24) | rect(x0 + 23, 170, x0 + 57, 230)
        mid.put(arch, hexc('#8cb8c0')); mid.put(outline_of(arch), hexc('#244844'))
        mid.put({p for p in arch if (p[0] + 2, p[1] + 2) not in arch}, hexc('#6a98a4'))
    return sky, full(far, 110), full(mid, 130)

def theme_palace():
    """T9 Palacio de la Ultima Nevada: royal-blue throne hall with ice pillars."""
    rng = random.Random(55)
    sky = banded_sky([(70, hexc('#04061a')), (140, hexc('#080c28')), (210, hexc('#0e1438')), (360, hexc('#161e48'))], zone=14)
    for _ in range(40):
        x, y = rng.randrange(SW), rng.randrange(0, 160); sky.put([(x, y)], hexc('#8ab4ff') if rng.random() < .5 else WHITE)
    # tall arched windows in the back wall
    for wx in range(20, SW, 96):
        arch = ell(wx + 28, 80, 28, 36) | rect(wx, 80, wx + 56, 220)
        sky.put(outline_of(arch), hexc('#0a0e2c')); sky.put(arch, hexc('#1a2866'))
        sky.put({p for p in arch if dither(p[0], p[1], (p[1] - 44) / 180.0)}, hexc('#2a3a84'))
        sky.put(rect(wx + 26, 44, wx + 30, 220), hexc('#0a0e2c')); sky.put(rect(wx, 130, wx + 56, 133), hexc('#0a0e2c'))
    far = Canvas(SW, SH)
    for px_ in range(48, SW, 160):      # pillars
        p = rect(px_, 0, px_ + 26, SH)
        part(far, p, dict(base=hexc('#8ab0f0'), hi=hexc('#e4f2ff'), shade=hexc('#5a82d0'), deep=hexc('#3a58a8'), out=hexc('#0a0e2c'), out_l=hexc('#5a82d0')))
        for yy in (30, 120, 220): far.put(rect(px_ - 3, yy, px_ + 29, yy + 8), hexc('#bcd6ff')); far.put(rect(px_ - 3, yy + 7, px_ + 29, yy + 9), hexc('#3a58a8'))
        far.put({(px_ + 6, y) for y in range(40, 330, 3)}, hexc('#e4f2ff'))
    mid = Canvas(SW, SH)
    return sky, far, mid

THEMES = [theme_lake, theme_village, theme_crystal, theme_garden, theme_palace]

# ===================================================================================== tiles
PW, PH = 40, 24
OUT = [hexc('#14306e'), hexc('#3a2a5a'), hexc('#10206a'), hexc('#1c3a44'), hexc('#0a0e2c')]

def snow_cap(cv, w, depth=7, seed=0):
    cap = rect(0, 0, w, depth) | {(x, y) for x in range(w) for y in range(depth, depth + [2, 4, 3, 5, 2, 3, 4, 2][(x // 5 + seed) % 8])}
    cv.put(cap, hexc('#f4fbff')); cv.put({(x, y) for (x, y) in cap if (x, y + 1) not in cap}, hexc('#bcd6f2')); cv.put(rect(0, 0, w, 1), WHITE)
    cv.put({(x, y) for x in range(0, w, 7) for y in (2, 3)}, hexc('#dbeaff'))
    return cap

def ice_plat(theme, kind):
    cv = Canvas(PW, PH)
    rng = random.Random(500 + theme * 13 + _hs(kind) % 31)
    L, R = kind == 'L', kind == 'R'
    if theme == 0:        # floating icebergs: chunky ice blocks capped with snow
        body = rect(0, 5, PW, PH - 1) | ell(8, 16, 9, 7) | ell(31, 16, 9, 7)
        body = {(x, y) for (x, y) in body if 0 <= x < PW and 0 <= y < PH}
        cv.put(body, hexc('#8fd2ff'))
        cv.put({(x, y) for (x, y) in body if (x + 3 * y) % 11 == 0}, hexc('#bfeaff'))
        cv.put({p for p in body if (p[0] + 2, p[1] + 2) not in body}, hexc('#4aa0e8')); cv.put({p for p in body if (p[0] + 1, p[1] + 3) not in body and p[1] > 14}, hexc('#2a62b8'))
        cv.put(outline_of(body), OUT[0]); snow_cap(cv, PW, 6, int(_hs(kind) % 5))
        cv.put(outline_of(rect(0, 0, PW, 6)) & {(x, y) for x in range(PW) for y in range(PH)}, OUT[0])
    elif theme == 1:      # village rooftops: plank edge with a thick snow blanket
        cv.put(rect(0, 8, PW, PH - 1), hexc('#8a5a3a'))
        for x in range(0, PW, 8): cv.put({(x, y) for y in range(9, PH - 1)}, hexc('#5a3624'))
        cv.put(rect(0, 8, PW, 10), hexc('#b0784a')); cv.put(rect(0, PH - 3, PW, PH - 1), hexc('#5a3624'))
        cv.put(rect(0, 5, PW, 8), hexc('#f4fbff'))
        cap = snow_cap(cv, PW, 7, 2)
        cv.put({(x, y) for x in range(2, PW, 9) for y in range(PH - 7, PH - 1)}, hexc('#dbeaff'))      # hanging icicles
        cv.put({(x, PH - 1) for x in range(2, PW, 9)}, hexc('#bcd6f2'))
        cv.put(outline_of(cap | rect(0, 8, PW, PH - 1)) & {(x, y) for x in range(PW) for y in range(PH)}, OUT[1])
    elif theme == 2:      # crystal slabs
        slab = rect(0, 3, PW, PH - 3) | ell(6, 12, 8, 9) | ell(34, 12, 8, 9)
        slab = {(x, y) for (x, y) in slab if 0 <= x < PW and 0 <= y < PH}
        cv.put(slab, hexc('#4aa0e8'))
        cv.put({(x, y) for (x, y) in slab if (x * 2 + y) % 13 < 2}, hexc('#7ad0ff'))
        cv.put({p for p in slab if (p[0] + 2, p[1] + 2) not in slab}, hexc('#2a62b8'))
        cv.put({(x, 3 + (x % 4 == 0)) for x in range(PW)}, hexc('#e4fbff')); cv.put(rect(0, 3, PW, 4), hexc('#bff0ff'))
        cv.put(outline_of(slab), OUT[2])
        for gx in (8, 22, 33): cv.put({(gx, y) for y in range(8, 14)}, hexc('#bff0ff'))
    elif theme == 3:      # garden stone ledge with snow and moss
        cv.put(rect(0, 6, PW, PH - 1), hexc('#8a9ca8'))
        for r_, y in enumerate(range(7, PH - 1, 6)):
            for bx in range(-(r_ % 2) * 10, PW, 20):
                cv.put(rect(max(0, bx), y, min(PW, bx + 19), y + 5), hexc('#a8bcc8')); cv.put(rect(max(0, bx), y + 4, min(PW, bx + 19), y + 5), hexc('#6a7e8a'))
        cv.put({(x, y) for x in range(PW) for y in range(PH - 6, PH - 1) if (x * 5 + y * 3) % 7 == 0}, hexc('#4a8a70'))     # moss
        cap = snow_cap(cv, PW, 6, 3)
        cv.put(outline_of(cap | rect(0, 6, PW, PH - 1)) & {(x, y) for x in range(PW) for y in range(PH)}, OUT[3])
    else:                 # palace ice marble with gold trim
        cv.put(rect(0, 4, PW, PH - 1), hexc('#a8c8f4'))
        for cx_ in range(0, PW, 20):
            cv.put(rect(cx_ + 1, 6, cx_ + 19, PH - 3), hexc('#c4dcff')); cv.put(rect(cx_ + 1, 6, cx_ + 19, 8), WHITE)
            cv.put({(cx_ + 5 + k, 12 + k // 2) for k in range(9)}, hexc('#8ab0e8'))
        cv.put(rect(0, 0, PW, 4), hexc('#ffe9a0')); cv.put(rect(0, 3, PW, 4), hexc('#d8a850')); cv.put(rect(0, 0, PW, 1), WHITE)
        cv.put({(x, 2) for x in range(2, PW, 8)}, hexc('#e8982a'))
        cv.put(outline_of(rect(0, 0, PW, PH - 1)), OUT[4])
    return cv

def special_tile(theme, kind, state=0):
    """kind: brittle (state 0 whole, 1 cracked, 2 very cracked) | elastic (state 0 rest, 1 squashed, 2 stretched) | raft | bridge."""
    cv = Canvas(PW, PH)
    if kind == 'brittle':
        t = ice_plat(theme, 'M1')
        cv.im.alpha_composite(t.im)
        # thin, pale, glassy: recolour towards white and add cracks
        for y in range(PH):
            for x in range(PW):
                p = cv.px[x, y]
                if p[3] and p[1] < 235: cv.px[x, y] = tuple(int(p[i] + (235 - p[i]) * .35) for i in range(3)) + (255,)
        rng = random.Random(7)
        n = [0, 3, 7][state]
        for k in range(n):
            x, y = 6 + k * 5, 6 + (k * 3) % 5
            for _ in range(4 + k % 3):
                cv.put([(x, y)], hexc('#2a52a8')); x += rng.choice((-1, 0, 1)); y += 1
                if y >= PH - 1: break
        cv.put({(x, 5) for x in range(2, PW - 2, 6)}, WHITE)
        return cv
    if kind == 'elastic':
        sq = [0, 3, -2][state]
        top = 7 + sq
        # springy crystal-jelly pad on a tiny pedestal
        cv.put(rect(8, PH - 7, PW - 8, PH - 1), hexc('#3a4a8a')); cv.put(rect(8, PH - 7, PW - 8, PH - 6), hexc('#6a7ec8')); cv.put(outline_of(rect(8, PH - 7, PW - 8, PH - 1)), OUT[2])
        pad = rect(2, top, PW - 2, PH - 7) | ell(PW / 2, top + 2, PW / 2 - 1, 5)
        pad = {(x, y) for (x, y) in pad if 0 <= x < PW and 0 <= y < PH}
        cv.put(pad, hexc('#7af0e0')); cv.put({p for p in pad if (p[0] + 2, p[1] + 2) not in pad}, hexc('#2ab8b0'))
        cv.put({(x, y) for x in range(5, PW - 5, 7) for y in range(top + 3, PH - 9)}, hexc('#bffcf4')); cv.put(outline_of(pad), hexc('#0e5a64'))
        cv.put({(x, top + 1) for x in range(5, PW - 5)}, hexc('#e4fffa'))
        return cv
    if kind == 'raft':
        t = ice_plat(theme, 'M0'); cv.im.alpha_composite(t.im)
        cv.put({(x, PH - 2) for x in range(PW)}, hexc('#2a62b8'))
        for x in (4, 20, 35): cv.put(ell(x, 12, 1.5, 1.5), hexc('#ffd23a')); cv.put({(x - 1, 11)}, WHITE)         # rivets
        cv.put({(x, y) for x in (1, PW - 2) for y in range(8, PH - 2)}, hexc('#ffd23a'))
        return cv
    # bridge: glowing crystal beams
    cv.put(rect(0, 3, PW, 9), hexc('#7ad8ff')); cv.put(rect(0, 3, PW, 4), WHITE); cv.put(rect(0, 8, PW, 9), hexc('#2a8ae0'))
    cv.put(outline_of(rect(0, 3, PW, 9)), OUT[2])
    for x in range(3, PW, 10): cv.put({(x, y) for y in range(10, 16)}, hexc('#7ad8ff')); cv.put({(x + 1, y) for y in range(10, 14)}, hexc('#2a8ae0'))
    cv.put({(x, 5) for x in range(2, PW, 5)}, hexc('#e4fbff'))
    return cv

GW_, GH_ = 40, 40
def ground_tile(theme, kind):
    """kind: 'ice' (slick, glossy) or 'snow' (soft). Row 0 is the walkable surface."""
    cv = Canvas(GW_, GH_)
    rng = random.Random(900 + theme)
    fill = [(hexc('#5ab0f0'), hexc('#3a82d8'), hexc('#8fd0ff')), (hexc('#b8a8d8'), hexc('#9888c0'), hexc('#d8c8f0')), (hexc('#2a62b8'), hexc('#1c4a98'), hexc('#4a86d8')),
            (hexc('#7a9ca8'), hexc('#5a808c'), hexc('#a0c0c8')), (hexc('#6a8ce0'), hexc('#4a68c0'), hexc('#9ab8f8'))][theme]
    cv.put(rect(0, 0, GW_, GH_), fill[0])
    for _ in range(26):
        x, y = rng.randrange(0, GW_ - 3), rng.randrange(6, GH_ - 2)
        cv.put([(x, y), (x + 1, y), (x + 2, y)], fill[2]); cv.put([(x, y + 1), (x + 1, y + 1)], fill[1])
    if kind == 'ice':
        cv.put(rect(0, 0, GW_, 3), hexc('#dff4ff')); cv.put(rect(0, 3, GW_, 6), hexc('#a8e0ff')); cv.put(rect(0, 6, GW_, 8), hexc('#6ab8f0'))
        cv.put({(x, 1) for x in range(3, GW_, 11)} | {(x + 1, 1) for x in range(3, GW_, 11)}, WHITE)      # glossy streaks
        cv.put({(x, 4) for x in range(8, GW_, 13)} | {(x + 1, 4) for x in range(8, GW_, 13)}, hexc('#ffffff'))
        for k, x0 in enumerate((6, 27)):
            cv.put({(x0 + i, 10 + (i * 2) // 3) for i in range(7)}, hexc('#2a62b8'))         # frozen cracks
    else:
        cap = rect(0, 0, GW_, 8) | {(x, y) for x in range(GW_) for y in range(8, 8 + [3, 5, 2, 6, 3, 4, 2, 5][x // 5])}
        cv.put(cap, hexc('#f4fbff')); cv.put({p for p in cap if (p[0], p[1] + 1) not in cap}, hexc('#bcd6f2'))
        cv.put({(x, y) for x in range(2, GW_, 6) for y in (1, 3)}, hexc('#dbeaff')); cv.put(rect(0, 0, GW_, 1), WHITE)
    return cv

def wall_tile(theme):
    cv = Canvas(20, 40)
    pal = [(hexc('#8fd2ff'), hexc('#dff4ff'), hexc('#4aa0e8')), (hexc('#8a5a3a'), hexc('#b0784a'), hexc('#5a3624')), (hexc('#4aa0e8'), hexc('#bff0ff'), hexc('#2a62b8')),
           (hexc('#8a9ca8'), hexc('#c4d4de'), hexc('#5a6e7a')), (hexc('#a8c8f4'), hexc('#ffffff'), hexc('#6a8ce0'))][theme]
    cv.put(rect(0, 0, 20, 40), pal[0]); cv.put(rect(0, 0, 3, 40), pal[1]); cv.put(rect(15, 0, 20, 40), pal[2])
    for y in range(0, 40, 10): cv.put(rect(0, y, 20, y + 1), pal[2]); cv.put({(5, y + 4), (6, y + 4), (5, y + 5), (6, y + 5)}, pal[1])
    cv.put({(0, y) for y in range(40)} | {(19, y) for y in range(40)}, OUT[theme])
    cv.put({(x, 0) for x in range(20)}, pal[1])
    return cv
