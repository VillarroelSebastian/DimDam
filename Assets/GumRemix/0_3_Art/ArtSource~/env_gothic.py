"""Reino de Chicle, reimagined as sweet gothic: a moonlit garden of licorice trees and glowing gum mushrooms under a far-off castle,
and the princess's ballroom (arched windows, velvet curtains, candelabras, chandeliers) for the boss.
Same layer layout as env_bg (sky / far / mid / tiles) so the environment builder needs no new concepts, only new names."""
import math, random
from px import *
from env_bg import banded_sky, hills, periodic_height, sprinkle_dots, full, SW, SH

S = math.sin
TAU = 2 * math.pi
WHITE = hexc('#ffffff')

def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

LIC = pal('#2a1838', '#7a5a98', '#1a0e28', '#100818', '#0a0612', '#5a4078')
GOLD = pal('#f2c14e', '#fff2b0', '#c8892e', '#8a5a22', '#3a1a1a', '#c8892e')
IRON = pal('#2a2c44', '#5a5e84', '#1c1e30', '#10121e', '#06070e', '#44486c')
STONE = pal('#4a3a6a', '#7a68a0', '#32264e', '#221a38', '#120c22', '#5a4a80')
WINE = pal('#7a1838', '#c04068', '#52102c', '#380a20', '#1e0614', '#9a2848')


# =============================================================================================== sky / hills
def night_sky():
    rng = random.Random(7)
    sky = banded_sky([(40, hexc('#0d0828')), (95, hexc('#1c0c40')), (150, hexc('#3a1160')), (200, hexc('#6a1a78')), (240, hexc('#a82a82')), (285, hexc('#e0508c')), (360, hexc('#ff8aa0'))], zone=14)
    # stars (the upper sky only), a few of them four-pointed
    for _ in range(140):
        x, y = rng.randrange(SW), rng.randrange(0, 190)
        c = rng.choice([hexc('#ffffff'), hexc('#c8e8ff'), hexc('#ffd8f0'), hexc('#8ff0ff')])
        sky.put([(x, y)], c)
        if rng.random() < .12: sky.put([(x - 1, y), (x + 1, y), (x, y - 1), (x, y + 1)], c)
    # the gum moon with a soft dithered halo
    mx, my = 505, 84
    for y in range(my - 60, my + 60):
        for x in range(mx - 60, mx + 60):
            d = math.hypot(x - mx, y - my)
            if 30 < d < 58 and dither(x, y, (1 - (d - 30) / 28) * .5): sky.px[x, y] = hexc('#8a3aa0')
    moon = ell(mx, my, 29, 29)
    part(sky, moon, dict(base=hexc('#ffd8ee'), hi=hexc('#fff4fa'), shade=hexc('#f0a8d0'), deep=hexc('#c880b0'), out=hexc('#8a3aa0'), out_l=hexc('#ffc0e0')))
    for (cx, cy, r) in ((497, 78, 6), (516, 92, 4), (508, 66, 3), (489, 96, 3)):
        c = ell(cx, cy, r, r); sky.put(c, hexc('#f0b0d4')); sky.put({p for p in c if (p[0] - 1, p[1] - 1) not in c}, hexc('#c880b0'))
    return sky


def castle_layer(w, h, base_y_at):
    """A far crooked castle with a few lit windows, drawn BEHIND the hills so they bury its feet."""
    cv = Canvas(w, h)
    wall = pal('#241450', '#4a2c88', '#180e38', '#100828', '#0a0620', '#3a2468')
    def tower(cx, top, width, spire_h, windows):
        base = base_y_at(cx)
        body = rect(cx - width // 2, top, cx + width // 2, base + 4)
        part(cv, body, wall, hi=False)
        roof = poly([(cx - width // 2 - 3, top), (cx + width // 2 + 3, top), (cx + 1, top - spire_h)])
        part(cv, roof, pal('#3a1c78', '#7a4ac0', '#26105a', '#180a40', '#0a0620'), hi=False)
        cv.put([(cx + 1, top - spire_h - 1), (cx + 1, top - spire_h - 2), (cx + 2, top - spire_h - 3), (cx + 3, top - spire_h - 3)], hexc('#ff6ad0'))   # pennant
        for k in range(windows):
            wy = top + 10 + k * 16
            lit = (k + cx) % 3 != 0
            cv.put(rect(cx - 2, wy, cx + 2, wy + 7), hexc('#ffd060') if lit else hexc('#10081e'))
            if lit: cv.put(rect(cx - 2, wy + 4, cx + 2, wy + 7), hexc('#ff9a40'))
    tower(230, 40, 26, 36, 3); tower(268, 70, 20, 24, 2); tower(305, 22, 34, 58, 4); tower(345, 62, 22, 28, 2); tower(384, 48, 28, 40, 3)
    wallp = rect(244, 96, 372, 160)                                        # curtain wall joining the towers
    part(cv, wallp, wall, hi=False)
    for x in range(246, 372, 8): cv.put(rect(x, 92, x + 5, 97), wall['base'])   # battlements
    return cv


def night_far():
    base = lambda x: int(120 - periodic_height(x, [(18, 2, 0.4), (10, 5, 1.7), (5, 9, 0.2)]))
    far = Canvas(SW, 220)
    far.im.alpha_composite(castle_layer(SW, 220, base).im)
    h = hills(SW, 220, 120, [(18, 2, 0.4), (10, 5, 1.7), (5, 9, 0.2)], hexc('#22124e'), hexc('#4a2a88'), hexc('#160a38'), 70)
    far.im.alpha_composite(h.im)
    return full(far, 90)


def night_mid():
    rng = random.Random(22)
    mid = hills(SW, 260, 110, [(22, 3, 1.1), (12, 7, 0.3), (6, 11, 2.2)], hexc('#3a1450'), hexc('#a03a9a'), hexc('#240c3a'), 110)
    # spores: cyan and magenta glints instead of sprinkles
    sprinkle_dots(mid, rng, 120, [hexc('#58f0ff'), hexc('#ff6ad0'), hexc('#e8c8ff')], 130, 260)
    return full(mid, 135)


def theme0():
    return night_sky(), night_far(), night_mid()


# =============================================================================================== ballroom (boss arena)
def theme4():
    rng = random.Random(44)
    cv = Canvas(SW, SH)
    wall = hexc('#2a0a26'); wall_l = hexc('#3e1236'); wall_d = hexc('#1c0618')
    cv.put(rect(0, 0, SW, SH), wall)
    for x in range(0, SW, 40):                                              # vertical panel moulding
        cv.put(rect(x + 2, 30, x + 4, 290), wall_l); cv.put(rect(x + 38, 30, x + 40, 290), wall_d)
    for y in range(SH):                                                     # dithered darkening toward the ceiling
        for x in range(SW):
            if y < 90 and dither(x, y, (1 - y / 90.0) * .8): cv.px[x, y] = wall_d
    # arched stained-glass windows
    def window(cx, top, w, h):
        arch = rect(cx - w // 2, top + w // 2, cx + w // 2, top + h) | ell(cx, top + w // 2, w / 2.0, w / 2.0)
        cv.put(ring(arch, 3), GOLD['deep']); cv.put(ring(arch, 1), GOLD['base']); cv.put(ring(arch, 2), GOLD['shade'])
        for (x, y) in arch:
            t = (y - top) / float(h)
            base = mix(hexc('#1a1450'), hexc('#6a2a98'), t)
            cv.px[x, y] = base
        # diamond lattice with a few tinted panes
        lead = hexc('#150a22')
        for (x, y) in arch:
            u = (x - (cx - w // 2) + (y - top)) % 14; v = (x - (cx - w // 2) - (y - top)) % 14
            if u in (0, 1) or v in (0, 1): cv.px[x, y] = lead
            else:
                ia, ib = (x - (cx - w // 2) + (y - top)) // 14, (x - (cx - w // 2) - (y - top)) // 14
                pane = ((ia * 73856093) ^ (ib * 19349663) ^ (cx * 83492791)) % 9
                if pane == 0: cv.px[x, y] = hexc('#2a8ac0')
                elif pane == 3: cv.px[x, y] = hexc('#b02a88')
        cv.put(rect(cx - 1, top, cx + 1, top + h), lead)
        cv.put(rect(cx - w // 2 - 4, top + h, cx + w // 2 + 4, top + h + 6), GOLD['shade']); cv.put(rect(cx - w // 2 - 4, top + h, cx + w // 2 + 4, top + h + 2), GOLD['hi'])
        # a pale shaft of moonlight falling from each window
        for y in range(top + h + 6, top + h + 70):
            for x in range(cx - w // 2 - 6, cx + w // 2 + 20):
                if dither(x, y, .14 * (1 - (y - top - h) / 80.0)) and cv.px[x, y] == wall: cv.px[x, y] = hexc('#5a3a78')
    for cx in (84, 244, 396, 556): window(cx, 36, 70, 170)
    # columns with gold capitals
    for cx in (164, 476):
        part(cv, rect(cx - 10, 40, cx + 10, 300), pal('#3a1a40', '#6a3a70', '#26102c', '#180a1e', '#0e0612'), hi=False)
        for x in range(cx - 7, cx + 8, 5): cv.put(rect(x, 46, x + 1, 296), hexc('#4a2250'))
        part(cv, rect(cx - 15, 32, cx + 15, 46), GOLD, hi=True)
        part(cv, rect(cx - 15, 292, cx + 15, 306), GOLD, hi=True)
    # cornice and a tiled band behind the floor
    cv.put(rect(0, 0, SW, 22), hexc('#12040e')); cv.put(rect(0, 22, SW, 26), GOLD['deep']); cv.put(rect(0, 22, SW, 24), GOLD['base'])
    for x in range(0, SW, 16): cv.put(poly([(x, 26), (x + 16, 26), (x + 8, 34)]), GOLD['shade'])
    for y in range(296, SH):
        for x in range(SW):
            k = ((x // 20) + ((y - 296) // 10)) % 2
            cv.px[x, y] = hexc('#3a1432') if k else hexc('#240a20')
    return cv, Canvas(SW, SH), Canvas(SW, SH)


# =============================================================================================== props
def _glow(cv, cx, cy, r, col, strength=.5):
    for y in range(int(cy - r), int(cy + r) + 1):
        for x in range(int(cx - r), int(cx + r) + 1):
            d = math.hypot(x - cx, y - cy)
            if d < r and not cv.get(x, y)[3] and dither(x, y, (1 - d / r) * strength): cv.put([(x, y)], col)


def licorice_tree(n=8, scheme=0):
    """Twisted black licorice tree whose bulbs of gum pulse like lanterns."""
    glow = [(hexc('#ff4aa8'), hexc('#ffc0e8'), hexc('#7a1a6a')), (hexc('#4de8e0'), hexc('#d8ffff'), hexc('#1a6a8a')), (hexc('#b070f0'), hexc('#e8d0ff'), hexc('#4a2a98'))][scheme]
    out = []
    for i in range(n):
        cv = Canvas(64, 120)
        t = i / n * TAU
        sway = 3.0 * S(t)
        def tx(y): return 32 + sway * ((112 - y) / 80.0) ** 2 + 3 * S(y * .09)
        pts = [(tx(y), y) for y in range(114, 40, -2)]
        trunk = set()
        for (x, y) in pts: trunk |= ell(x, y, 3.6 - (114 - y) * .022, 3.4 - (114 - y) * .02)
        part(cv, trunk, LIC, hi=True)
        # three curled branches with a glowing bulb at each tip (the bulbs lag the sway)
        for k, (by, side, ln) in enumerate(((84, -1, 22), (68, 1, 26), (52, -1, 18))):
            bx0 = tx(by); lag = sway * .4 * side
            br = set(); px_, py_ = bx0, by
            for s_ in range(1, 9):
                u = s_ / 8.0
                nx = bx0 + side * ln * u + lag * u; ny = by - 10 * math.sin(u * 2.4) - 4 * u
                br |= line(px_, py_, nx, ny, 1.9 - u * .6); px_, py_ = nx, ny
            part(cv, br, LIC, hi=False)
            pulse = .5 + .5 * S(t * 1 + k * 2.1)
            r = 4.2 + 1.2 * pulse
            _glow(cv, px_, py_, 11, glow[2], .45 + .25 * pulse)
            bulb = ell(px_, py_, r, r)
            part(cv, bulb, dict(base=glow[0], hi=glow[1], shade=glow[2], deep=glow[2], out=hexc('#1a0a28'), out_l=glow[2]), hi=True)
            cv.put([(int(px_ - r * .4), int(py_ - r * .4))], WHITE)
        # crown of the tree: a cluster of three bigger bulbs
        top = (tx(42) + sway * .3, 36)
        for (dx, dy, rr, ph) in ((-8, 6, 6.5, 0), (8, 4, 7.0, 1.7), (0, -6, 8.0, 3.1)):
            pulse = .5 + .5 * S(t + ph)
            cx, cy = top[0] + dx + sway * .25, top[1] + dy
            _glow(cv, cx, cy, rr + 8, glow[2], .4 + .3 * pulse)
            b = ell(cx, cy, rr + pulse, rr + pulse)
            part(cv, b, dict(base=glow[0], hi=glow[1], shade=glow[2], deep=glow[2], out=hexc('#1a0a28'), out_l=glow[2]), hi=True)
            cv.put({(int(cx - rr * .4), int(cy - rr * .45)), (int(cx - rr * .4) + 1, int(cy - rr * .45))}, WHITE)
        out.append(cv)
    return out


def glow_mushroom(n=6, scheme=0):
    cols = [(hexc('#ff5ab8'), hexc('#ffc8ec'), hexc('#8a1a78')), (hexc('#4de8e0'), hexc('#d8ffff'), hexc('#1a6a8a')), (hexc('#b070f0'), hexc('#ecd8ff'), hexc('#4a2a98'))][scheme]
    out = []
    for i in range(n):
        cv = Canvas(32, 40)
        t = i / n * TAU
        pulse = .5 + .5 * S(t)
        for (x, h, r) in ((9, 14, 7), (20, 20, 9), (27, 10, 5)):
            y = 38
            stem = rect(x - 2, y - h, x + 2, y)
            part(cv, stem, dict(base=hexc('#e8d8f0'), hi=WHITE, shade=hexc('#b8a0c8'), deep=hexc('#8a70a0'), out=hexc('#2a1838'), out_l=hexc('#b8a0c8')), hi=False)
            cap = ell(x, y - h, r, r * .7) & {(px_, py_) for px_ in range(32) for py_ in range(40) if py_ <= y - h + 1}
            _glow(cv, x, y - h, r + 5, cols[2], .35 + .3 * pulse)
            part(cv, cap, dict(base=cols[0], hi=cols[1], shade=cols[2], deep=cols[2], out=hexc('#2a1838'), out_l=cols[2]), hi=True)
            for (dx, dy) in ((-3, -1), (0, -3), (3, -1)):
                cv.put([(x + dx, y - h + dy)], WHITE if pulse > .5 else cols[1])
        # a rising spore
        sy = 30 - int(((i / n) * 26))
        cv.put([(16 + int(2 * S(t * 2)), sy)], cols[1])
        out.append(cv)
    return out


def spire(n=8):
    """A crooked gothic spire: stone body, scalloped roof, a window that flickers and a pennant in the wind."""
    out = []
    for i in range(n):
        cv = Canvas(96, 120)
        t = i / n * TAU
        body = poly([(30, 118), (64, 118), (60, 46), (34, 46)])
        part(cv, body, STONE, hi=False)
        for y in range(52, 118, 9):                                         # brick courses
            cv.put({(x, y) for x in range(34 - (118 - y) // 20, 62 + (118 - y) // 20)}, STONE['shade'])
        roof = poly([(26, 48), (68, 48), (49, 4)])
        part(cv, roof, pal('#5a2a98', '#9a5ad8', '#3a1a70', '#26104e', '#120a2a'), hi=False)
        for k in range(5):                                                  # roof tiles
            y = 14 + k * 8
            cv.put({(x, y) for x in range(49 - (y - 4) // 2 + 2, 49 + (y - 4) // 2 - 1) if (x + k) % 3 == 0}, hexc('#7a4ac0'))
        flick = .6 + .4 * S(t * 3 + 1)
        win = rect(44, 70, 54, 86) | ell(49, 70, 5, 5)
        cv.put(outline_of(win), hexc('#120c22')); cv.put(win, hexc('#ffd060') if flick > .75 else hexc('#ff9a40'))
        cv.put(rect(48, 70, 51, 86), hexc('#120c22'))
        _glow(cv, 49, 78, 16, hexc('#a04a40'), .25 * flick)
        # pennant on a pole
        cv.put(line(49, 4, 49, -0, 1.0), hexc('#120a2a'))
        flag = poly([(50, 2), (50 + 15 + 3 * S(t), 4 + 2 * S(t - 1)), (50, 9)])
        cv.put(flag, hexc('#ff4aa8'))
        out.append(cv)
    return out


def candelabra(n=6):
    """Three-armed gold candelabra with flickering flames (pivot at its foot)."""
    out = []
    for i in range(n):
        cv = Canvas(48, 64)
        t = i / n * TAU
        part(cv, rect(22, 24, 26, 60), GOLD, hi=False)
        part(cv, ell(24, 60, 10, 3.4), GOLD, hi=False)
        part(cv, ell(24, 44, 4.4, 3), GOLD, hi=False)
        arms = set(line(24, 28, 10, 26, 1.1) | line(24, 28, 38, 26, 1.1) | line(10, 26, 10, 22, 1.1) | line(38, 26, 38, 22, 1.1) | line(24, 28, 24, 20, 1.1))
        part(cv, arms, GOLD, hi=False, shade=False)
        for k, (x, y) in enumerate(((10, 22), (24, 18), (38, 22))):
            part(cv, rect(x - 1, y - 6, x + 2, y), dict(base=hexc('#fff0e0'), hi=WHITE, shade=hexc('#e0c8b8'), deep=hexc('#b0988c'), out=hexc('#3a1a1a')), hi=False)
            f = S(t * 2 + k * 2.2)
            fh = 6 + 1.6 * f; fx = x + .5 + .8 * S(t * 3 + k)
            _glow(cv, fx, y - 10, 11, hexc('#a0502a'), .3)
            flame = ell(fx, y - 7 - fh * .5, 2.2, fh * .6)
            cv.put(flame, hexc('#ffb040')); cv.put(ell(fx, y - 6.5 - fh * .3, 1.2, fh * .35), hexc('#fff4a0'))
        out.append(cv)
    return out


def chandelier(n=8):
    """Hanging iron-and-gold chandelier (pivot at the top of the chain); it sways a little and the candles flicker."""
    out = []
    for i in range(n):
        cv = Canvas(112, 96)
        t = i / n * TAU
        sw = 2.2 * S(t)
        cv.put(rect(55, 0, 57, 24), hexc('#1a0e22'))                            # chain
        for y in range(2, 24, 4): cv.put(rect(54, y, 58, y + 2), GOLD['shade'])
        ox = int(round(sw))
        ring = ell(56 + ox, 44, 44, 9)
        part(cv, ring & {(x, y) for x in range(112) for y in range(96) if y >= 44}, GOLD, hi=False)
        part(cv, ell(56 + ox, 44, 44, 5) - ell(56 + ox, 44, 41, 3), GOLD, hi=False, shade=False)
        part(cv, poly([(52 + ox, 24), (60 + ox, 24), (64 + ox, 40), (48 + ox, 40)]), GOLD, hi=False)
        part(cv, ell(56 + ox, 62, 5, 7), GOLD, hi=True)
        cv.put(rect(55 + ox, 68, 57 + ox, 80), GOLD['base'])
        for k, dx in enumerate((-40, -24, -8, 8, 24, 40)):
            x = 56 + ox + dx; y = 44 + int(round(8 * math.sqrt(max(0, 1 - (dx / 44.0) ** 2))))
            part(cv, rect(x - 1, y - 9, x + 2, y), dict(base=hexc('#fff0e0'), hi=WHITE, shade=hexc('#e0c8b8'), deep=hexc('#b0988c'), out=hexc('#3a1a1a')), hi=False)
            f = S(t * 2 + k * 1.3)
            fh = 6 + 1.6 * f
            _glow(cv, x + .5, y - 13, 12, hexc('#a0502a'), .28)
            cv.put(ell(x + .5, y - 10 - fh * .5, 2.2, fh * .6), hexc('#ffb040')); cv.put(ell(x + .5, y - 9.5 - fh * .3, 1.2, fh * .35), hexc('#fff4a0'))
            cv.put([(x, y + 3), (x, y + 4), (x, y + 5)], hexc('#e8f0ff') if k % 2 else hexc('#ffc0e8'))      # crystal drops
        out.append(cv)
    return out


def curtain(n=8):
    """Velvet drape that hangs from the top of the screen and breathes in a draught (pivot at the top)."""
    out = []
    for i in range(n):
        cv = Canvas(64, 200)
        t = i / n * TAU
        for y in range(0, 200):
            tail = (y / 200.0) ** 1.6
            for x in range(0, 64):
                edge = 4 * tail * S(t + y * .03)
                if x < 3 - edge * 0 or x > 61 - 2 * tail * 4 + edge: continue
                fold = S((x + 3 * tail * S(t)) * .55)
                c = WINE['base']
                if fold > .55: c = WINE['hi']
                elif fold < -.45: c = WINE['shade']
                elif fold < -.8: c = WINE['deep']
                cv.px[x, y] = c
        # scalloped hem that sways
        for x in range(0, 61, 2):
            hy = 190 + int(5 * S(x * .4 + t)) + int(3 * S(t + x * .1))
            cv.put(rect(x, hy, x + 2, 200), CLEAR)
        # gold rope tie and tassel
        ty = 118
        cv.put(rect(2, ty, 62, ty + 3), GOLD['base']); cv.put(rect(2, ty, 62, ty + 1), GOLD['hi']); cv.put(rect(2, ty + 2, 62, ty + 3), GOLD['shade'])
        part(cv, ell(58, ty + 11 + int(S(t) * 1.2), 3, 7), GOLD, hi=False)
        # gold rod and rings at the top
        cv.put(rect(0, 0, 64, 5), GOLD['base']); cv.put(rect(0, 0, 64, 2), GOLD['hi']); cv.put(rect(0, 4, 64, 6), GOLD['deep'])
        out.append(cv)
    return out


def lantern(n=6, scheme=0):
    """A paper lantern with a candle inside, swinging on its string (replaces the balloons of the sky level)."""
    P = [pal('#c0304a', '#ff8a8a', '#8a1c3c', '#5a1030', '#240814'), pal('#7a4ad0', '#c0a0ff', '#52309a', '#321a6a', '#14082e'),
         pal('#2a9aa8', '#9af0f0', '#1a6a82', '#10425a', '#08182a'), pal('#d08a2a', '#ffd88a', '#a05a22', '#6a3418', '#2a1408')][scheme]
    out = []
    for i in range(n):
        cv = Canvas(32, 56); t = i / n * TAU
        sw = 2.2 * S(t + scheme)
        cx = 16 + sw * .5
        for y in range(0, 12):
            cv.put([(int(16 + sw * (y / 56.0) ** 1.4), y)], hexc('#d8c8a8'))
        _glow(cv, cx, 32, 16, P['shade'], .35)
        body = ell(cx, 32, 9.5, 12)
        part(cv, body, P)
        for rx in (-6, -3, 0, 3, 6): cv.put({p for p in body if abs(p[0] - cx - rx * .0 - rx) < .6}, P['shade'])      # ribs
        flick = .5 + .5 * S(t * 2 + scheme)
        cv.put(ell(cx, 32, 3.2 + flick, 5 + flick), mix(P['hi'][:3], hexc('#fff4c0'), .6))
        part(cv, rect(cx - 6, 19, cx + 6, 23), GOLD, hi=False)
        part(cv, rect(cx - 6, 43, cx + 6, 47), GOLD, hi=False)
        for k in range(3): cv.put(line(cx - 3 + k * 3, 47, cx - 3 + k * 3 + sw * .3, 54, .5), GOLD['base'])        # tassel
        out.append(cv)
    return out


# =============================================================================================== tiles (night garden + ballroom)
PW, PH = 40, 24

def plat_tile_night(kind):
    """Dark cocoa wafer under hot-magenta frosting with a glowing lip."""
    cv = Canvas(PW, PH); rng = random.Random(100 + sum(map(ord, kind)) % 997)
    body, body_d, body_l, out = hexc('#5a3040'), hexc('#3a1c2c'), hexc('#7a4458'), hexc('#10061c')
    cv.put(rect(0, 5, PW, PH - 1), body)
    for gx in range(0, PW, 8): cv.put(rect(gx, 6, gx + 1, PH - 2), body_d)
    for gy in range(9, PH - 2, 6): cv.put(rect(0, gy, PW, gy + 1), body_d)
    cv.put(rect(0, PH - 4, PW, PH - 1), body_d); cv.put(rect(0, 5, PW, 6), body_l)
    frosting = rect(0, 0, PW, 7) | {(x, y) for x in range(PW) for y in range(7, 7 + [2, 4, 3, 5, 2, 3, 4, 2][x // 5 % 8])}
    if kind == 'L': frosting |= ell(3, 4, 4, 4)
    cv.put(frosting, hexc('#e03a98')); cv.put(rect(0, 0, PW, 2), hexc('#ff9ad8')); cv.put({(x, y) for (x, y) in frosting if (x, y + 1) not in frosting}, hexc('#8a1a6a'))
    for _ in range(7):                                                  # tiny glints, not sprinkles
        x, y = rng.randrange(1, PW - 1), rng.randrange(1, 5)
        cv.put([(x, y)], rng.choice([hexc('#ffffff'), hexc('#8ff0ff'), hexc('#ffe9a0')]))
    cv.put(outline_of(frosting | rect(0, 5, PW, PH - 1)), out)
    if kind == 'L': cv.put({(0, y) for y in range(PH)}, cv.px[1, 10])
    return cv


def plat_tile_hall(kind):
    """Ebony stage boards with a gold edge."""
    cv = Canvas(PW, PH)
    cv.put(rect(0, 4, PW, PH - 1), hexc('#2a1424'))
    cv.put(rect(0, 4, PW, 6), hexc('#f2c14e')); cv.put(rect(0, PH - 4, PW, PH - 2), hexc('#c8892e'))
    for x in range(0, PW, 10): cv.put({(x + 4, y) for y in range(7, PH - 4)}, hexc('#12060e'))
    for x in range(2, PW, 10): cv.put({(x, 13), (x + 1, 13), (x, 12), (x - 1, 14), (x + 2, 14)}, hexc('#7a2a58'))     # inlaid diamonds
    cv.put(rect(0, 0, PW, 4), hexc('#fff2b0')); cv.put(rect(0, 0, PW, 1), WHITE); cv.put(rect(0, 3, PW, 4), hexc('#c8892e'))
    cv.put(outline_of(rect(0, 0, PW, PH - 1)), hexc('#10040c'))
    if kind == 'L': cv.put({(0, y) for y in range(PH)}, cv.px[1, 10])
    return cv


GROUND_PALS = {
    0: (hexc('#4a2438'), hexc('#68384e'), hexc('#341a2c'), hexc('#e03a98'), hexc('#ff9ad8'), hexc('#8a1a6a')),
    4: (hexc('#2a1424'), hexc('#4a2438'), hexc('#1c0c18'), hexc('#f2c14e'), hexc('#fff2b0'), hexc('#c8892e')),
}
WALL_PALS = {
    0: (hexc('#7a2a68'), hexc('#c04aa0'), hexc('#4a1a48')),
    4: (hexc('#3a1a40'), hexc('#7a4a80'), hexc('#241028')),
}


# =============================================================================================== ice world: sorrowful props
def dead_tree(n=8, seed=0):
    """A leafless frozen tree: twisted bare branches carrying thin snow, with icicles dripping from the twigs."""
    out = []
    TRUNK = pal('#2a2e48', '#5a6288', '#1c2038', '#10132a', '#080a18', '#444a70')
    for i in range(n):
        cv = Canvas(64, 120)
        t = i / n * TAU
        sway = 1.6 * S(t + seed)
        def tx(y): return 32 + sway * ((112 - y) / 80.0) ** 2 + 2.5 * S(y * .08 + seed)
        trunk = set()
        for y in range(114, 36, -2): trunk |= ell(tx(y), y, 3.8 - (114 - y) * .03, 3.4 - (114 - y) * .027)
        part(cv, trunk, TRUNK, hi=True)
        rng = random.Random(40 + seed)
        for k, (by, side, ln) in enumerate(((90, -1, 24), (74, 1, 26), (60, -1, 20), (48, 1, 16), (40, -1, 12))):
            bx0 = tx(by); lag = sway * .5 * side
            br = set(); px_, py_ = bx0, by
            for s_ in range(1, 8):
                u = s_ / 7.0
                nx = bx0 + side * ln * u + lag * u; ny = by - 12 * math.sin(u * 2.0) - 5 * u
                br |= line(px_, py_, nx, ny, 1.9 - u * 1.0)
                if s_ in (4, 6):                                           # a twig forking off
                    br |= line(nx, ny, nx + side * 5, ny - 6 - rng.random() * 3, .7)
                px_, py_ = nx, ny
            part(cv, br, TRUNK, hi=False)
            cv.put({(int(x), int(y) - 2) for (x, y) in br if rng.random() < .16 and (x, y - 1) not in br}, hexc('#c8d8f0'))       # snow on top
            for dx_ in (-2, 3):                                            # icicles hanging from the branch tips
                cv.put(line(px_ + dx_, py_ + 1, px_ + dx_, py_ + 4 + (i + k + dx_) % 3, .6), hexc('#a8c4e8'))
        out.append(cv)
    return out


def ice_lamp(n=6):
    """A cold iron lamp post whose pale flame gutters (pivot at its foot)."""
    out = []
    for i in range(n):
        cv = Canvas(24, 64); t = i / n * TAU
        part(cv, rect(11, 20, 14, 62), IRON, hi=False)
        part(cv, ell(12, 62, 7, 2.6), IRON, hi=False)
        part(cv, rect(7, 12, 18, 21), IRON, hi=False)
        flick = .5 + .5 * S(t * 2 + 1)
        _glow(cv, 12, 12, 14, hexc('#2a5a88'), .35 + .15 * flick)
        part(cv, poly([(8, 12), (17, 12), (15, 3), (10, 3)]), dict(base=hexc('#8ac0e8'), hi=hexc('#e8f8ff'), shade=hexc('#4a88c0'), deep=hexc('#2a5a90'), out=hexc('#0a1230'), out_l=hexc('#4a88c0')), hi=False)
        cv.put(ell(12.5, 8.5, 2.0 + flick * .6, 3.2 + flick), hexc('#e8fbff'))
        cv.put(rect(8, 2, 17, 4), IRON['base'])
        out.append(cv)
    return out
