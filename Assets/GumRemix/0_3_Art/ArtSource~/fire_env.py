"""Reino de Fuego: an abandoned, ember-lit foundry kingdom. Dark basalt, rust and brass lit from below by furnaces and lava;
the same sorrowful tone as the ice world, but warm and heavy. Five themes (10..14):
  10 Forja Abandonada   11 Rios de Magma   12 Camara de Engranajes   13 Fundicion del Rey   14 Salon de las Hoces (boss arena)
Same layer layout as the other worlds: sky (static), far and mid (seamless horizontal tiles), tiles (plat/special/ground/wall) and props."""
import math, random
from px import *
from env_bg import banded_sky, hills, periodic_height, sprinkle_dots, full, SW, SH

S = math.sin
C = math.cos
TAU = 2 * math.pi
WHITE = hexc('#ffffff')


def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))


BASALT = pal('#2e1e2a', '#6a4458', '#1e1219', '#130a10', '#0a0508', '#4a2c3e')
RUST = pal('#6a2a1a', '#b05a30', '#451a10', '#2a0e08', '#100404', '#8a3c22')
BRASS = pal('#c08a3a', '#f4d488', '#8a5a24', '#583414', '#1c0e08', '#a8742e')
IRON = pal('#3c3c52', '#80809c', '#282838', '#16161f', '#08080e', '#5a5a78')
SOOT = pal('#241a22', '#52384a', '#180f16', '#0e080c', '#060304', '#3a2434')
LAVA = [hexc('#ff4a10'), hexc('#ff8a24'), hexc('#ffc850'), hexc('#fff0b0')]
CRUST = hexc('#5a1c10')


def halo(cv, cx, cy, r, col, strength=.5, ry=None):
    ry = ry or r
    for y in range(int(cy - ry), int(cy + ry) + 1):
        for x in range(int(cx - r), int(cx + r) + 1):
            if not (0 <= x < cv.w and 0 <= y < cv.h): continue
            d = math.hypot((x - cx) / r, (y - cy) / ry)
            if d < 1 and dither(x, y, (1 - d) * strength): cv.px[x, y] = col


def wrapx(fn, x, w=SW):
    """Call fn(x + k*w) for k in (-1, 0, 1): everything drawn on a seamless tile must be repeated across the seam."""
    for k in (-1, 0, 1): fn(x + k * w)


def sparks(cv, rng, n, y0, y1, cols=None):
    cols = cols or [hexc('#ffd060'), hexc('#ff8a24'), hexc('#fff0b0')]
    for _ in range(n):
        x, y = rng.randrange(cv.w), rng.randrange(y0, y1)
        cv.put([(x, y)], rng.choice(cols))


# =============================================================================================== skies, far and mid layers
def sky10():
    rng = random.Random(10)
    sky = banded_sky([(40, hexc('#0a0608')), (100, hexc('#160a0e')), (160, hexc('#2a1010')), (210, hexc('#481a12')), (260, hexc('#7a2c14')), (310, hexc('#b8501c')), (360, hexc('#e88a30'))], zone=14)
    for x in range(0, SW, 4):                                 # drifting smoke: dithered dark bands across the glow
        for y in range(150, 300):
            if dither(x, y, max(0, S(x * .02 + y * .05) * .5 + .12)) and sky.px[x, y] != hexc('#0a0608'):
                sky.px[x, y] = mix(sky.px[x, y][:3], (10, 6, 8), .55)
    sparks(sky, rng, 50, 80, 300)
    return sky


def far10():
    far = Canvas(SW, SH)
    def tower(x, w, h, cool=False):
        base = 300
        body = poly([(x - w, base), (x - w * (.6 if cool else .9), base - h), (x + w * (.6 if cool else .9), base - h), (x + w, base)])
        part(far, body, SOOT, hi=False)
        far.put({(x - int(w * .6) + k, base - h) for k in range(int(w * 1.2))}, hexc('#7a3018'))            # lit rim
        if not cool:
            for k in range(3): far.put(rect(x - 2, base - h + 14 + k * 24, x + 3, base - h + 20 + k * 24), hexc('#e8742a') if (x // 40 + k) % 2 else hexc('#3a1a1c'))
        else:
            for yy in range(base - h + 10, base - 10, 12): far.put({(x - int(w * .8) + k, yy) for k in range(int(w * 1.6))}, SOOT['shade'])
    for x, w, h, cool in ((70, 20, 110, False), (170, 34, 90, True), (290, 16, 150, False), (410, 40, 100, True), (520, 18, 130, False), (600, 28, 80, True)):
        wrapx(lambda xx: tower(xx, w, h, cool), x)
    ridge = hills(SW, 160, 100, [(14, 3, .5), (8, 7, 1.4), (4, 13, 2.2)], hexc('#1c1016'), hexc('#6a2a18'), hexc('#120a0e'), 40)
    far.im.alpha_composite(ridge.im, (0, 200))
    return far


def mid10():
    mid = Canvas(SW, SH)
    rng = random.Random(110)
    for yy in (60, 150):                                      # girders
        g = rect(0, yy, SW, yy + 12)
        part(mid, g, RUST, hi=False)
        for x in range(8, SW, 22): mid.put(ell(x, yy + 6, 1.5, 1.5), BRASS['base'])
    for x in range(40, SW, 120):                              # posts and hanging chains
        part(mid, rect(x, 60, x + 12, 300), RUST, hi=False)
        for yy in range(72, 290, 18): mid.put(rect(x - 2, yy, x + 14, yy + 3), RUST['shade'])
        for k in range(14): mid.put(ell(x + 6 + 30 * (1 if x % 240 else -1), 72 + k * 5, 2, 2.6) - ell(x + 6 + 30 * (1 if x % 240 else -1), 72 + k * 5, 1, 1.6), SOOT['out'])
    floor = hills(SW, 160, 70, [(8, 3, 1.1), (5, 8, .2)], hexc('#2a1218'), hexc('#7a3a20'), hexc('#160a0e'), 60)
    mid.im.alpha_composite(floor.im, (0, 210))
    return mid


def sky11():
    rng = random.Random(11)
    sky = banded_sky([(60, hexc('#0a0406')), (120, hexc('#1c0a0c')), (180, hexc('#3a1410')), (240, hexc('#7a2a10')), (300, hexc('#c85a14')), (360, hexc('#ffa030'))], zone=14)
    halo(sky, 320, 340, 300, hexc('#ffc850'), .35, ry=110)
    sparks(sky, rng, 60, 100, 330)
    return sky


def far11():
    far = Canvas(SW, SH)
    for x in range(0, SW, 4):                                 # stalactites hanging from the cave ceiling
        ln = 24 + periodic_height(x, [(20, 3, .4), (12, 7, 1.3), (6, 13, 2.0)])
        for y in range(0, int(max(6, ln))): far.px[x, y] = hexc('#2a1218') if y < ln - 2 else hexc('#7a3018')
        for k in range(1, 4):
            if x + k < SW:
                for y in range(0, int(max(6, ln))): far.px[x + k, y] = far.px[x, y]
    far.put({(x, y) for x in range(SW) for y in range(0, 6)}, hexc('#120a0e'))
    cols = hills(SW, 200, 120, [(16, 3, .2), (10, 6, 1.1), (5, 12, 2.0)], hexc('#2a1218'), hexc('#8a3a1c'), hexc('#180c10'), 50)
    far.im.alpha_composite(cols.im, (0, 170))
    for x in range(30, SW, 70):                               # basalt columns
        h = 60 + (x * 7) % 50
        far.put(rect(x, 330 - h, x + 18, 360), hexc('#1c0e14')); far.put(rect(x + 12, 330 - h, x + 18, 360), hexc('#120a0e')); far.put(rect(x, 330 - h, x + 18, 332 - h), hexc('#5a2418'))
    return far


def mid11():
    mid = Canvas(SW, SH)
    rng = random.Random(111)
    # a river of lava with a dithered glowing edge and floating dark crust islands
    for x in range(SW):
        y0 = int(215 + 6 * S(x * .03) + 3 * S(x * .11))
        for y in range(y0, y0 + 46):
            t = (y - y0) / 46.0
            c = LAVA[0] if dither(x, y, .45 + .25 * S(x * .07 + y * .2)) else LAVA[1]
            if dither(x + 1, y, .12 + .3 * (1 - t)): c = LAVA[2]
            mid.px[x, y] = c
        mid.put([(x, y0 - 1)], hexc('#5a1c10'))
    for _ in range(9):
        x, y = rng.randrange(20, SW - 20), rng.randrange(228, 252)
        isl = ell(x, y, rng.randrange(10, 20), 3)
        mid.put(isl, hexc('#2a1218')); mid.put({p for p in isl if (p[0], p[1] - 1) not in isl}, hexc('#7a3018'))
    floor = hills(SW, 120, 40, [(6, 3, .7), (4, 9, 1.6)], hexc('#1c0e14'), hexc('#7a3a20'), hexc('#120a0e'), 50)
    mid.im.alpha_composite(floor.im, (0, 262))
    return mid


def sky12():
    rng = random.Random(12)
    sky = banded_sky([(70, hexc('#0e0a08')), (140, hexc('#1e1610')), (210, hexc('#322418')), (360, hexc('#4a3420'))], zone=14)
    def gear(cx, cy, r, teeth, col, hole):
        g = ell(cx, cy, r, r)
        for k in range(teeth):
            a = k * TAU / teeth
            g |= poly([(cx + C(a - .12) * (r - 2), cy + S(a - .12) * (r - 2)), (cx + C(a - .08) * (r + 7), cy + S(a - .08) * (r + 7)), (cx + C(a + .08) * (r + 7), cy + S(a + .08) * (r + 7)), (cx + C(a + .12) * (r - 2), cy + S(a + .12) * (r - 2))])
        sky.put(outline_of(g), hexc('#0a0604')); sky.put(g, col)
        sky.put(ell(cx, cy, r * .72, r * .72), hexc('#241a12')); sky.put(ell(cx, cy, r * .28, r * .28), hole)
        for k in range(5):
            a = k * TAU / 5
            sky.put(line(cx, cy, cx + C(a) * r * .7, cy + S(a) * r * .7, 2.4), col)
    gear(120, 120, 60, 14, hexc('#3a2a1a'), hexc('#0a0604')); gear(320, 80, 40, 10, hexc('#463220'), hexc('#0a0604')); gear(500, 150, 70, 16, hexc('#3a2a1a'), hexc('#0a0604')); gear(240, 220, 34, 9, hexc('#4a3622'), hexc('#0a0604'))
    for wy in (240, 296):
        for wx in range(16, SW, 52):
            if rng.random() < .55:
                sky.put(rect(wx, wy, wx + 20, wy + 10), hexc('#ff9a3a') if rng.random() < .7 else hexc('#c8501c')); sky.put(rect(wx, wy + 6, wx + 20, wy + 10), hexc('#c8501c'))
                halo(sky, wx + 10, wy + 5, 16, hexc('#7a3418'), .3)
    return sky


def far12():
    far = Canvas(SW, SH)
    for x in range(30, SW, 96):                               # tall brass pistons with a lit ring
        part(far, rect(x, 40, x + 22, 360), BRASS, hi=True)
        for yy in range(60, 340, 56):
            far.put(rect(x - 3, yy, x + 25, yy + 8), BRASS['hi']); far.put(rect(x - 3, yy + 6, x + 25, yy + 9), BRASS['deep'])
        far.put({(x + 7, y) for y in range(48, 358, 3)}, hexc('#f4d488'))
    # tone it down: this layer sits behind everything
    for y in range(SH):
        for x in range(SW):
            p = far.px[x, y]
            if p[3]: far.px[x, y] = mix(p[:3], (20, 12, 10), .55)
    return far


def mid12():
    mid = Canvas(SW, SH)
    for yy in (50, 170):                                      # girders with rivets
        part(mid, rect(0, yy, SW, yy + 14), pal('#5a3a20', '#8a5a34', '#3a2412', '#241408', '#100804'), hi=False)
        for x in range(8, SW, 24): mid.put(ell(x, yy + 7, 1.6, 1.6), BRASS['base'])
    for x in range(60, SW, 160):
        r = 26
        g = ell(x, 118, r, r)
        for k in range(10):
            a = k * TAU / 10
            g |= poly([(x + C(a - .15) * (r - 1), 118 + S(a - .15) * (r - 1)), (x + C(a - .09) * (r + 6), 118 + S(a - .09) * (r + 6)), (x + C(a + .09) * (r + 6), 118 + S(a + .09) * (r + 6)), (x + C(a + .15) * (r - 1), 118 + S(a + .15) * (r - 1))])
        part(mid, g, pal('#6a4424', '#a87438', '#442a14', '#2a1a0c', '#100804'), hi=False)
        mid.put(ell(x, 118, 8, 8), hexc('#140c06'))
    return mid


def sky13():
    rng = random.Random(13)
    sky = banded_sky([(60, hexc('#120808')), (130, hexc('#241010')), (200, hexc('#401a10')), (280, hexc('#6a2a10')), (360, hexc('#a04a18'))], zone=14)
    halo(sky, 320, 360, 330, hexc('#d8741c'), .3, ry=130)
    for x, w, h in ((60, 30, 230), (200, 22, 200), (420, 34, 240), (570, 24, 190)):      # chimney stacks with a lit rim
        sky.put(outline_of(rect(x - w // 2, 360 - h, x + w // 2, 360)), hexc('#0a0404'))
        sky.put(rect(x - w // 2, 360 - h, x + w // 2, 360), hexc('#1e0e10'))
        sky.put(rect(x + w // 2 - 4, 360 - h, x + w // 2, 360), hexc('#5a2414'))
        sky.put(rect(x - w // 2 - 3, 360 - h, x + w // 2 + 3, 366 - h), hexc('#2e1618'))
        for k in range(8): halo(sky, x + 2 * S(k), 340 - h - k * 10, 10 + k * 2, hexc('#3a1610'), .35)       # smoke trail
    sparks(sky, rng, 50, 120, 340)
    return sky


def far13():
    far = Canvas(SW, SH)
    for x in range(0, SW, 160):                               # gantry cranes
        part(far, rect(x + 20, 60, x + 30, 300), SOOT, hi=False)
        part(far, rect(x + 20, 60, x + 130, 70), SOOT, hi=False)
        for k in range(10): far.put(line(x + 24 + k * 6, 66, x + 24 + k * 6 + 6, 116, .5), SOOT['shade'])
        for k in range(8): far.put(ell(x + 120, 72 + k * 6, 2, 2.6) - ell(x + 120, 72 + k * 6, 1, 1.6), hexc('#0a0508'))
        part(far, poly([(x + 108, 130), (x + 132, 130), (x + 128, 156), (x + 112, 156)]), pal('#4a2218', '#8a4a2a', '#2e140e', '#1c0a06', '#0a0404'), hi=False)
        halo(far, x + 120, 150, 14, hexc('#7a3018'), .35)
    return far


def mid13():
    mid = Canvas(SW, SH)
    for x in range(40, SW, 200):                              # molten vats with a glowing surface
        vat = poly([(x, 262), (x + 90, 262), (x + 80, 330), (x + 10, 330)])
        part(mid, vat, IRON, hi=False)
        mid.put(ell(x + 45, 262, 45, 7), hexc('#1c0e10'))
        mid.put(ell(x + 45, 262, 41, 5), LAVA[0]); mid.put(ell(x + 40, 261, 24, 2.4), LAVA[1]); mid.put(ell(x + 38, 260, 9, 1.4), LAVA[2])
        halo(mid, x + 45, 250, 50, hexc('#a03c14'), .3, ry=30)
        mid.put(rect(x + 20, 290, x + 70, 292), IRON['shade'])
    for yy in (80, 150): part(mid, rect(0, yy, SW, yy + 10), RUST, hi=False)
    return mid


def sky14():
    rng = random.Random(14)
    cv = Canvas(SW, SH)
    cv.put(rect(0, 0, SW, SH), hexc('#1a0a10'))
    for y in range(SH):                                        # hall glow rising from the floor
        for x in range(SW):
            if dither(x, y, max(0.0, (y - 150) / 210.0) * .8): cv.px[x, y] = hexc('#3a1414')
            if dither(x, y, max(0.0, (y - 250) / 110.0) * .5): cv.px[x, y] = hexc('#5a2012')
    def arch_window(cx, top, w, h):
        arch = rect(cx - w // 2, top + w // 2, cx + w // 2, top + h) | ell(cx, top + w // 2, w / 2.0, w / 2.0)
        cv.put(ring(arch, 3), BRASS['deep']); cv.put(ring(arch, 1), BRASS['base'])
        for (x, y) in arch:
            t = (y - top) / float(h)
            cv.px[x, y] = mix((60, 10, 14), (255, 120, 40), t * t)
        for (x, y) in arch:
            u = (x - cx + (y - top)) % 14; v = (x - cx - (y - top)) % 14
            if u in (0, 1) or v in (0, 1): cv.px[x, y] = hexc('#1a0a08')
        cv.put(rect(cx - 1, top, cx + 1, top + h), hexc('#1a0a08'))
        halo(cv, cx, top + h - 10, w, hexc('#6a2a14'), .25, ry=40)
    for cx in (84, 244, 396, 556): arch_window(cx, 30, 64, 170)
    for cx in (164, 476):                                        # columns with mantis-crest capitals
        part(cv, rect(cx - 10, 40, cx + 10, 300), pal('#3a1c1c', '#6a3a34', '#26100e', '#180806', '#0c0404'), hi=False)
        for x in range(cx - 7, cx + 8, 5): cv.put(rect(x, 46, x + 1, 296), hexc('#4a2424'))
        part(cv, rect(cx - 15, 32, cx + 15, 46), BRASS, hi=True); part(cv, rect(cx - 15, 292, cx + 15, 306), BRASS, hi=True)
    cv.put(rect(0, 0, SW, 24), hexc('#0c0406')); cv.put(rect(0, 24, SW, 28), BRASS['deep']); cv.put(rect(0, 24, SW, 26), BRASS['base'])
    for y in range(296, SH):
        for x in range(SW):
            cv.px[x, y] = hexc('#2e1216') if ((x // 20) + ((y - 296) // 10)) % 2 else hexc('#1a0a0e')
    sparks(cv, rng, 40, 60, 290)
    return cv


def sky14_far():
    far = Canvas(SW, SH)
    return far


THEMES = [lambda: (sky10(), full(far10(), 0), full(mid10(), 0)),
          lambda: (sky11(), full(far11(), 0), full(mid11(), 0)),
          lambda: (sky12(), full(far12(), 0), full(mid12(), 0)),
          lambda: (sky13(), full(far13(), 0), full(mid13(), 0)),
          lambda: (sky14(), Canvas(SW, SH), Canvas(SW, SH))]
