"""Static environment layers + tiles for the 5 themes (sky, far, mid, platform, ground, wall)."""
import math, random
from px import *

SW, SH = 640, 360
def _hs(s): return sum(ord(c) * (i + 1) for i, c in enumerate(s))

S = math.sin
TAU = 2 * math.pi
WHITE = hexc("#ffffff")

def banded_sky(stops, w=SW, h=SH, zone=14):
    """stops: list of (y_end, colour). Hard bands blended by a Bayer dither zone: classic pixel-art sky."""
    cv = Canvas(w, h)
    cols = [c for _, c in stops]; ys = [y for y, _ in stops]
    for y in range(h):
        i = 0
        while i < len(ys) - 1 and y >= ys[i]: i += 1
        c0 = cols[i]
        if i < len(ys) - 1:
            d = ys[i] - y                     # distance to next band edge
            if d < zone:
                t = 1 - d / zone
                for x in range(w):
                    cv.px[x, y] = cols[i + 1] if dither(x, y, t) else c0
                continue
        for x in range(w): cv.px[x, y] = c0
    return cv

def periodic_height(x, comps, w=SW):
    return sum(a * S(TAU * k * x / w + ph) for a, k, ph in comps)

def hills(w, h, base, comps, col, rim, deep=None, dither_h=0, rim_px=2):
    """Seamless silhouette layer. Optional dithered darkening toward the bottom."""
    cv = Canvas(w, h)
    for x in range(w):
        y0 = int(base - periodic_height(x, comps, w))
        for y in range(max(0, y0), h):
            c = col
            if y - y0 < rim_px: c = rim
            elif deep is not None and dither_h and y > y0 + 6:
                t = min(1, (y - y0 - 6) / dither_h)
                if dither(x, y, t): c = deep
            cv.px[x, y] = c
    return cv

def sprinkle_dots(cv, rng, n, cols, y0, y1, region=None):
    for _ in range(n):
        x = rng.randrange(0, cv.w); y = rng.randrange(y0, y1)
        if cv.px[x, y][3] and (region is None or region(x, y)):
            c = rng.choice(cols)
            horiz = rng.random() < .5
            cv.put([(x, y), (x + 1, y)] if horiz else [(x, y), (x, y + 1)], c)

def full(cv, y):
    out = Canvas(SW, SH); out.im.alpha_composite(cv.im, (0, y)); return out

# ============================================================== theme palettes / layers
def theme0():
    """Prado Piruleta - bright candy meadow, morning."""
    rng = random.Random(10)
    sky = banded_sky([(60, hexc('#79c8ff')), (120, hexc('#a4dcff')), (170, hexc('#c8ecff')), (215, hexc('#ffe0f0')), (360, hexc('#ffc4e0'))])
    # tiny pixel sun with a ring
    for r, c in ((26, hexc('#fff3b0')), (20, hexc('#ffe56a')), (15, hexc('#fff3b0'))):
        sky.put(ell(520, 74, r, r), c) if r != 26 else None
    sky.put(ring(ell(520, 74, 24, 24)), hexc('#fff3b0'))
    far = hills(SW, 220, 120, [(18, 2, 0.4), (10, 5, 1.7), (5, 9, 0.2)], hexc('#9fe8c8'), hexc('#d4fbe8'), hexc('#7fd0b0'), 70)
    # stripy candy-cane hill details
    for cx in (90, 330, 560):
        for k in range(9):
            x0 = cx - 40 + k * 9
            for y in range(140, 220):
                for x in range(x0, x0 + 4):
                    p = far.px[x % SW, y]
                    if p[3] and (y + x // 2) % 12 < 6 and y > far.h - 70: pass
    mid = hills(SW, 260, 110, [(22, 3, 1.1), (12, 7, 0.3), (6, 11, 2.2)], hexc('#ffa8d4'), hexc('#ffd8ec'), hexc('#e880b8'), 110)
    sprinkle_dots(mid, rng, 220, [hexc('#fff0f6'), hexc('#ffe36a'), hexc('#8ff0ff'), hexc('#c8a0ff')], 130, 260)
    return sky, full(far, 90), full(mid, 135)

def theme1():
    """Cueva de Caramelo - warm amber cave."""
    rng = random.Random(11)
    sky = banded_sky([(50, hexc('#2a1030')), (110, hexc('#4a1c34')), (170, hexc('#7a2c34')), (230, hexc('#b0503a')), (360, hexc('#d88040'))], zone=16)
    # faint glow in the middle
    for y in range(120, 300):
        for x in range(SW):
            d = math.hypot((x - 320) / 320, (y - 210) / 110)
            if d < 1 and dither(x, y, (1 - d) * .55): sky.px[x, y] = hexc('#e8a050')
    # stalactites hang from ceiling (far)
    far = Canvas(SW, SH)
    for x in range(SW):
        ln = 20 + periodic_height(x, [(18, 3, .4), (12, 7, 1.3), (6, 13, 2.0)])
        for y in range(0, int(max(6, ln))): far.px[x, y] = hexc('#5a2a3a') if y < ln - 2 else hexc('#7a3c44')
    far.put({(x, y) for x in range(SW) for y in range(0, 6)}, hexc('#3a1a30'))
    # distant floor rocks
    base = hills(SW, 220, 150, [(10, 4, .2), (6, 9, 1.1)], hexc('#6a3040'), hexc('#8a4a4a'), hexc('#4a2438'), 28)
    far.im.alpha_composite(base.im, (0, 140))
    mid = Canvas(SW, SH)
    for x in range(SW):
        ln = 30 + periodic_height(x, [(22, 2, 2.1), (14, 5, .2), (8, 11, 1.0)])
        for y in range(0, int(max(8, ln))):
            mid.px[x, y] = hexc('#3a1a2c') if y < ln - 2 else hexc('#6a3444')
    mid.put({(x, y) for x in range(SW) for y in range(0, 10)}, hexc('#2a1224'))
    floor = hills(SW, 260, 205, [(12, 3, 1.3), (6, 8, .1)], hexc('#52202c'), hexc('#7a3a40'), hexc('#3a1824'), 30)
    mid.im.alpha_composite(floor.im, (0, 100))
    return sky, far, mid

def theme2():
    """Nubes de Malvavisco - dusk sky of marshmallow clouds."""
    rng = random.Random(12)
    sky = banded_sky([(55, hexc('#3a2a78')), (105, hexc('#5a3a98')), (150, hexc('#8a4ab0')), (195, hexc('#d060b0')), (240, hexc('#ff8aa0')), (360, hexc('#ffc890'))], zone=12)
    far = hills(SW, 220, 150, [(14, 3, .5), (9, 6, 2.0), (5, 12, 1.0)], hexc('#b070c8'), hexc('#d8a0e0'), hexc('#8a50b0'), 70)
    mid = hills(SW, 260, 120, [(18, 4, 1.4), (10, 9, .2), (6, 15, 2.5)], hexc('#ffb4d0'), hexc('#fff0f6'), hexc('#e890b8'), 110)
    return sky, full(far, 90), full(mid, 135)

def theme3():
    """Fabrica de Chocolate - factory interior."""
    rng = random.Random(13)
    sky = banded_sky([(70, hexc('#1c1020')), (140, hexc('#2c1a28')), (210, hexc('#3e2430')), (360, hexc('#5a3438'))], zone=14)
    # lit windows across the back wall
    for wy in (50, 120, 190):
        for wx in range(20, SW, 64):
            lit = rng.random() < .6
            w = rect(wx, wy, wx + 36, wy + 36)
            sky.put(outline_of(w), hexc('#16101a')); sky.put(w, hexc('#ffb040') if lit else hexc('#3a2430'))
            if lit:
                sky.put(rect(wx, wy + 24, wx + 36, wy + 36), hexc('#e88030')); sky.put({(wx + 4 + k, wy + 4) for k in range(8)}, hexc('#ffe090'))
            sky.put(rect(wx + 17, wy, wx + 19, wy + 36), hexc('#16101a')); sky.put(rect(wx, wy + 17, wx + 36, wy + 19), hexc('#16101a'))
    # brick-ish vertical pipes seamless
    far = Canvas(SW, 220)
    for px_ in range(40, SW, 160):
        p = rect(px_, 0, px_ + 18, 220)
        part(far, p, dict(base=hexc('#6a4448'), hi=hexc('#9a6a68'), shade=hexc('#4a2c38'), deep=hexc('#34202c'), out=hexc('#1c1020'), out_l=hexc('#4a2c38')))
        for yy in range(30, 220, 54): far.put(rect(px_ - 2, yy, px_ + 20, yy + 5), hexc('#8a5a58')); far.put(rect(px_ - 2, yy + 4, px_ + 20, yy + 6), hexc('#34202c'))
    mid = Canvas(SW, 260)
    for yy in (40, 150):  # horizontal girders
        g = rect(0, yy, SW, yy + 14)
        part(mid, g, dict(base=hexc('#5a3a40'), hi=hexc('#8a6460'), shade=hexc('#3e2530'), deep=hexc('#2a1826'), out=hexc('#1c1020'), out_l=hexc('#3e2530')))
        for x in range(8, SW, 24): mid.put({(x, yy + 6), (x + 1, yy + 6), (x, yy + 7), (x + 1, yy + 7)}, hexc('#c8a080'))
    return sky, full(far, 140), full(mid, 100)

def theme4():
    """Circo del Chicloso - big-top boss arena."""
    sky = Canvas(SW, SH)
    cols = (hexc('#a82c4c'), hexc('#e6c4aa'))
    for x in range(SW):
        # stripes converge toward a ridge pole at the top centre
        k = int((x - 320) / 40.0 + 100) % 2
        base = cols[k]
        for y in range(SH):
            c = base
            shade = 0
            if y < 120: shade = 1 - y / 120
            # darken toward the roof with ordered dithering
            if dither(x, y, shade * .9): c = hexc('#701838') if k == 0 else hexc('#a87888')
            sky.px[x, y] = c
    # fabric seam shadows
    for sx in range(0, SW, 40):
        for y in range(SH):
            sky.put([(sx + 320 % 40, y)], hexc('#8a2040') if y % 2 else hexc('#701838'))
    # warm stage glow near the floor
    for y in range(230, SH):
        for x in range(SW):
            if dither(x, y, (y - 230) / 130 * .5): sky.px[x, y] = hexc('#ffb080') if sky.px[x, y] == cols[1] else hexc('#e0506a')
    return sky, Canvas(SW, SH), Canvas(SW, SH)

THEMES = [theme0, theme1, theme2, theme3, theme4]

# ============================================================== tiles
PW, PH = 40, 24         # platform tile (1 x 0.6 units)

def plat_tile(theme, kind):
    """kind: 'L','M0','M1','M2','R'. Drawn so that the walkable surface is the top edge."""
    cv = Canvas(PW, PH)
    rng = random.Random(100 + _hs(kind) % 997)
    if theme == 0:      # wafer biscuit with pink frosting + gum drips
        body = hexc('#e8b070'); body_d = hexc('#c88850'); body_l = hexc('#f8d098'); out = hexc('#4a2038')
        cv.put(rect(0, 5, PW, PH - 1), body)
        for gx in range(0, PW, 8): cv.put(rect(gx, 6, gx + 1, PH - 2), body_d)
        for gy in range(9, PH - 2, 6): cv.put(rect(0, gy, PW, gy + 1), body_d)
        cv.put(rect(0, PH - 4, PW, PH - 1), body_d); cv.put(rect(0, 5, PW, 6), body_l)
        frosting = rect(0, 0, PW, 7) | {(x, y) for x in range(PW) for y in range(7, 7 + [2, 4, 3, 5, 2, 3, 4, 2][x // 5 % 8])}
        if kind == 'L': frosting |= ell(3, 4, 4, 4)
        cv.put(frosting, hexc('#ff8cc4')); cv.put(rect(0, 0, PW, 2), hexc('#ffd0ea')); cv.put({(x, y) for (x, y) in frosting if (x, y + 1) not in frosting}, hexc('#d8408e'))
        sprinkle_dots(cv, rng, 8, [hexc('#fff0f6'), hexc('#8ff0ff'), hexc('#ffe36a')], 1, 5)
        cv.put(outline_of(frosting | rect(0, 5, PW, PH - 1)), out)
    elif theme == 1:    # toffee brick, caramel top
        cv.put(rect(0, 4, PW, PH - 1), hexc('#8a4a30'))
        for r_, y in enumerate(range(5, PH - 1, 6)):
            for bx in range(-(r_ % 2) * 10, PW, 20):
                cv.put(rect(max(0, bx), y, min(PW, bx + 19), y + 5), hexc('#a8603a')); cv.put(rect(max(0, bx), y + 4, min(PW, bx + 19), y + 5), hexc('#6a3828'))
                cv.put({(max(0, bx) + 1, y), (max(0, bx) + 2, y)}, hexc('#c88050'))
        cap = rect(0, 0, PW, 6) | {(x, y) for x in range(PW) for y in range(6, 6 + [3, 5, 2, 4, 6, 3, 2, 4][x // 5 % 8])}
        cv.put(cap, hexc('#ffb040')); cv.put(rect(0, 0, PW, 2), hexc('#ffe090')); cv.put({(x, y) for (x, y) in cap if (x, y + 1) not in cap}, hexc('#d07820'))
        cv.put({(6 + k, 1) for k in range(3)} | {(26 + k, 2) for k in range(2)}, hexc('#ffffff'))
        cv.put(outline_of(cap | rect(0, 4, PW, PH - 1)), hexc('#2a1030'))
    elif theme == 2:    # puffy marshmallow
        m = rect(0, 3, PW, PH - 3) | ell(10, 8, 9, 7) | ell(30, 8, 9, 7) | ell(20, 6, 10, 6)
        m |= ell(8, PH - 7, 8, 5) | ell(32, PH - 7, 8, 5) | ell(20, PH - 6, 10, 5)
        m = {(x, y) for (x, y) in m if 0 <= x < PW and 0 <= y < PH}
        if kind == 'L': m = {p for p in m if p[0] > 1 or abs(p[1] - 12) < 7}
        part(cv, m, dict(base=hexc('#fff4fa'), hi=hexc('#ffffff'), shade=hexc('#f0c8e0'), deep=hexc('#d09ac0'), out=hexc('#7a3a78'), out_l=hexc('#e0a8d0')))
        cv.put({(6 + k, 5) for k in range(3)}, hexc('#ffffff'))
    elif theme == 3:    # chocolate bar + foil edge
        cv.put(rect(0, 4, PW, PH - 1), hexc('#6a3a28'))
        for cx_ in range(0, PW, 20):
            blk = rect(cx_ + 1, 6, cx_ + 19, PH - 3)
            cv.put(blk, hexc('#7a4430')); cv.put(rect(cx_ + 1, 6, cx_ + 19, 8), hexc('#9a5c40')); cv.put(rect(cx_ + 3, 10, cx_ + 17, 17), hexc('#6a3a28'))
            cv.put(rect(cx_ + 3, 10, cx_ + 17, 11), hexc('#5a3020')); cv.put(rect(cx_ + 3, 16, cx_ + 17, 17), hexc('#8a5438'))
        cv.put(rect(0, 0, PW, 5), hexc('#d8d8e8')); cv.put(rect(0, 0, PW, 1), WHITE); cv.put(rect(0, 4, PW, 5), hexc('#8a8ab0'))
        for x in range(2, PW, 8): cv.put({(x, 2), (x + 1, 2)}, hexc('#a0a0c8'))
        cv.put({(x, 3) for x in range(0, PW, 2)}, hexc('#b0b0d0'))
        cv.put(outline_of(rect(0, 0, PW, PH - 1)), hexc('#1c1020'))
        cv.put({(2, 2), (PW - 3, 2)}, hexc('#40405a'))
    else:               # circus stage boards
        cv.put(rect(0, 4, PW, PH - 1), hexc('#a02848'))
        cv.put(rect(0, 4, PW, 6), hexc('#e8c050')); cv.put(rect(0, PH - 4, PW, PH - 2), hexc('#e8c050'))
        for x in range(0, PW, 10): cv.put({(x + 4, y) for y in range(7, PH - 4)}, hexc('#701838'))
        for x in range(2, PW, 10): cv.put({(x, 14), (x + 1, 14), (x, 13)}, hexc('#ffe090'))
        cv.put(rect(0, 0, PW, 4), hexc('#ffe9b8')); cv.put(rect(0, 0, PW, 1), WHITE); cv.put(rect(0, 3, PW, 4), hexc('#d8a870'))
        cv.put(outline_of(rect(0, 0, PW, PH - 1)), hexc('#2a0a28'))
    if kind == 'L': cv.put({(0, y) for y in range(PH)}, CLEAR if theme in (0,) else cv.px[1, 10])
    return cv

GW_, GH_ = 40, 40
def ground_tiles(theme):
    """top tile (with decoration) and fill tile."""
    top = Canvas(GW_, GH_); fill = Canvas(GW_, GH_)
    rng = random.Random(200 + theme)
    pals = [
        (hexc('#8a5030'), hexc('#a86840'), hexc('#6a3a28'), hexc('#ff8cc4'), hexc('#ffd0ea'), hexc('#d8408e')),
        (hexc('#6a3438'), hexc('#8a4a44'), hexc('#4a2430'), hexc('#ffb040'), hexc('#ffe090'), hexc('#d07820')),
        (hexc('#e8b8d8'), hexc('#f8d8ec'), hexc('#c890b8'), hexc('#ffffff'), hexc('#ffffff'), hexc('#e0b0d0')),
        (hexc('#4a2c28'), hexc('#6a4038'), hexc('#34201c'), hexc('#d8d8e8'), hexc('#ffffff'), hexc('#8a8ab0')),
        (hexc('#7a2038'), hexc('#a02848'), hexc('#581430'), hexc('#e8c050'), hexc('#ffe9b8'), hexc('#b08030')),
    ][theme]
    body, body_l, body_d, cap, cap_l, cap_d = pals
    for cv in (top, fill):
        cv.put(rect(0, 0, GW_, GH_), body)
        # speckled texture, same pattern in both tiles for seamless stacking
    for cv in (top, fill):
        rr = random.Random(300 + theme)
        for _ in range(26):
            x, y = rr.randrange(0, GW_ - 2), rr.randrange(0, GH_ - 2)
            cv.put([(x, y), (x + 1, y)], body_l); cv.put([(x, y + 1), (x + 1, y + 1)], body_d)
        for _ in range(5):
            x, y = rr.randrange(2, GW_ - 3), rr.randrange(2, GH_ - 3)
            cv.put(ell(x, y, 2, 1.6), [hexc('#fff0f6'), hexc('#ffe36a'), hexc('#8ff0ff')][rr.randrange(3)] if theme in (0, 4) else body_d)
    topcap = rect(0, 0, GW_, 8) | {(x, y) for x in range(GW_) for y in range(8, 8 + [3, 5, 2, 6, 3, 4, 2, 5][x // 5])}
    top.put(topcap, cap); top.put(rect(0, 0, GW_, 2), cap_l); top.put({(x, y) for (x, y) in topcap if (x, y + 1) not in topcap}, cap_d)
    top.put({(x, 7) for x in range(GW_)}, cap_d) if theme == 2 else None
    top.put({(x, y) for (x, y) in outline_of(topcap)} & {(x, y) for x in range(GW_) for y in range(GH_)}, hexc('#2a1030'))
    return top, fill

def wall_tile(theme):
    cv = Canvas(20, 40)
    pal = [(hexc('#ff9ad0'), hexc('#ffd0ea'), hexc('#d8408e')), (hexc('#a8603a'), hexc('#c88050'), hexc('#6a3828')),
           (hexc('#fff4fa'), hexc('#ffffff'), hexc('#d09ac0')), (hexc('#7a4430'), hexc('#9a5c40'), hexc('#5a3020')),
           (hexc('#e8c050'), hexc('#ffe9b8'), hexc('#b08030'))][theme]
    cv.put(rect(0, 0, 20, 40), pal[0]); cv.put(rect(0, 0, 3, 40), pal[1]); cv.put(rect(15, 0, 20, 40), pal[2]); cv.put(rect(17, 0, 20, 40), pal[2])
    for y in range(0, 40, 10): cv.put(rect(0, y, 20, y + 1), pal[2]); cv.put({(5, y + 4), (6, y + 4), (5, y + 5), (6, y + 5)}, pal[1])
    cv.put({(x, y) for x in range(20) for y in range(40) if x in (0,)}, hexc('#2a1030')); cv.put({(19, y) for y in range(40)}, hexc('#2a1030'))
    return cv
