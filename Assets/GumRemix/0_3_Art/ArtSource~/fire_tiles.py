"""Tiles and animated props for the Fire Kingdom (themes 10..14); the layers (sky/far/mid) live in fire_env.py."""
import math, random
from px import *
from fire_env import pal, halo, BASALT, RUST, BRASS, IRON, SOOT, LAVA, S, C, TAU, WHITE

PW, PH = 40, 24
# per theme: (plate, plate_hi, body, body_dark, glow)
TILE_PAL = {
    10: (hexc('#4a4a62'), hexc('#8a8aa8'), hexc('#5a2a1c'), hexc('#341610'), hexc('#ff7a24')),
    11: (hexc('#3a2430'), hexc('#7a4a5a'), hexc('#2a1a24'), hexc('#180e14'), hexc('#ff5a14')),
    12: (hexc('#a8742e'), hexc('#f4d488'), hexc('#5a3a1c'), hexc('#341f0e'), hexc('#ffa030')),
    13: (hexc('#4a4a62'), hexc('#8a8aa8'), hexc('#3a1e1e'), hexc('#220e0e'), hexc('#ff6a1c')),
    14: (hexc('#c8943c'), hexc('#fff0b0'), hexc('#2a1218'), hexc('#160a0e'), hexc('#ff6a28')),
}


# =============================================================================================== tiles
def plat_tile(theme, kind):
    plate, plate_hi, body, body_d, glow = TILE_PAL[theme]
    cv = Canvas(PW, PH); rng = random.Random(300 + theme * 7 + sum(map(ord, kind)))
    cv.put(rect(0, 5, PW, PH - 1), body)
    for gx in range(0, PW, 10): cv.put(rect(gx, 6, gx + 1, PH - 2), body_d)                     # block seams
    for gy in range(11, PH - 2, 6): cv.put(rect(0, gy, PW, gy + 1), body_d)
    cv.put(rect(0, PH - 4, PW, PH - 1), body_d)
    for _ in range(3):                                                                         # glowing cracks leaking from below
        x = rng.randrange(3, PW - 3); y = PH - 2
        for k in range(rng.randrange(4, 8)):
            cv.put([(x, y)], glow); x += rng.choice((-1, 0, 1)); y -= 1
    cv.put({(x, PH - 2) for x in range(0, PW, 3)}, glow)
    cv.put({(x, PH - 1) for x in range(PW) if x % 2 == 0}, mix(glow[:3], (255, 220, 120), .4))
    cv.put(rect(0, 0, PW, 5), plate); cv.put(rect(0, 0, PW, 1), plate_hi); cv.put(rect(0, 4, PW, 5), body_d)   # the walkable plate
    for x in range(3, PW, 10): cv.put({(x, 2), (x + 1, 2), (x, 3), (x + 1, 3)}, BRASS['base'] if theme in (12, 14) else IRON['shade'])
    if theme == 14: cv.put({(x, 2) for x in range(0, PW, 4)}, hexc('#7a2a1c'))
    cv.put(outline_of(rect(0, 0, PW, PH - 1)), hexc('#0a0508'))
    if kind == 'L': cv.put({(0, y) for y in range(PH)}, cv.px[1, 10])
    return cv


def special_tile(theme, kind, state=0):
    plate, plate_hi, body, body_d, glow = TILE_PAL[theme]
    cv = Canvas(PW, PH)
    if kind == 'brittle':                      # cooling crust: 0 intact, 1 shaking, 2 cracked
        cv.put(rect(0, 3, PW, PH - 3), hexc('#2a1612')); cv.put(rect(0, 3, PW, 5), hexc('#5a2c20'))
        rng = random.Random(7)
        for _ in range(2 + state * 3):
            x = rng.randrange(4, PW - 4); y = 6
            for k in range(8 + state * 3):
                cv.put([(x, y)], LAVA[1] if k % 3 else LAVA[2]); x += rng.choice((-1, 0, 1)); y += 1 if y < PH - 5 else 0
        cv.put({(x, PH - 4) for x in range(0, PW, 2)}, glow)
        if state == 2: cv.put({(x, 3) for x in range(6, 34, 3)}, hexc('#ff9a30'))
        cv.put(outline_of(rect(0, 3, PW, PH - 3)), hexc('#0a0508'))
        return cv
    if kind == 'elastic':                      # steam-vent pad: 0 rest, 1 compressed, 2 released with a puff
        top = 6 + (3 if state == 1 else 0)
        cv.put(rect(6, top, PW - 6, PH - 4), BRASS['base']); cv.put(rect(6, top, PW - 6, top + 2), BRASS['hi']); cv.put(rect(6, PH - 6, PW - 6, PH - 4), BRASS['deep'])
        for x in range(10, PW - 8, 7): cv.put(rect(x, top + 3, x + 3, top + 5), hexc('#10080a'))
        cv.put(rect(4, PH - 4, PW - 4, PH - 1), IRON['base']); cv.put(rect(4, PH - 4, PW - 4, PH - 3), IRON['hi'])
        cv.put({(5, y) for y in range(top, PH - 4)} | {(PW - 6, y) for y in range(top, PH - 4)}, BRASS['deep'])
        if state == 2:
            for k, (dx, dy, r) in enumerate(((-6, -2, 4), (0, -6, 5), (7, -3, 4))):
                cv.put(ell(PW // 2 + dx, top + dy, r, r * .8), hexc('#d8c8c0')); cv.put(ell(PW // 2 + dx - 1, top + dy - 1, r - 2, (r - 2) * .8), hexc('#fff0e8'))
        cv.put(outline_of(rect(6, top, PW - 6, PH - 4)), hexc('#1c0e08'))
        return cv
    if kind == 'raft':                         # lift platform: brass deck on a bracket with a lamp underneath
        cv.put(rect(0, 3, PW, 9), BRASS['base']); cv.put(rect(0, 3, PW, 4), BRASS['hi']); cv.put(rect(0, 8, PW, 9), BRASS['deep'])
        for x in range(4, PW, 8): cv.put({(x, 5), (x + 1, 5)}, hexc('#1c0e08'))
        br = poly([(4, 9), (PW - 4, 9), (PW - 10, 17), (10, 17)])
        cv.put(br, IRON['base']); cv.put(outline_of(br), hexc('#08080e'))
        halo(cv, PW // 2, 19, 12, glow, .6, ry=5); cv.put(ell(PW // 2, 18, 3, 2), hexc('#ffd060'))
        cv.put(outline_of(rect(0, 3, PW, 9)), hexc('#1c0e08'))
        return cv
    cv.put(rect(0, 4, PW, 10), hexc('#ff8a24')); cv.put(rect(0, 4, PW, 6), hexc('#ffd060')); cv.put(rect(0, 8, PW, 10), hexc('#c8501c'))      # bridge: a bar of ember light
    for x in range(2, PW, 6): cv.put({(x, 6)}, hexc('#fff0b0'))
    return cv


def ground_tile(theme, kind='top'):
    plate, plate_hi, body, body_d, glow = TILE_PAL[theme]
    cv = Canvas(40, 40); rng = random.Random(500 + theme * 3 + (1 if kind == 'alt' else 0))
    cv.put(rect(0, 0, 40, 40), body)
    for _ in range(30):
        x, y = rng.randrange(0, 38), rng.randrange(8, 38)
        cv.put([(x, y), (x + 1, y)], body_d)
    for _ in range(3 if kind == 'top' else 5):                                                  # lava veins
        x = rng.randrange(3, 37); y = rng.randrange(14, 38)
        for k in range(rng.randrange(6, 14)):
            cv.put([(x, y)], glow); cv.put([(x + 1, y)], mix(glow[:3], (255, 220, 120), .5))
            x += rng.choice((-1, 0, 1)); y -= rng.choice((0, 1, 1))
    cv.put(rect(0, 0, 40, 8), plate); cv.put(rect(0, 0, 40, 2), plate_hi); cv.put(rect(0, 7, 40, 8), body_d)
    for x in range(4, 40, 10): cv.put({(x, 4), (x + 1, 4), (x, 5), (x + 1, 5)}, BRASS['base'] if theme in (12, 14) else IRON['shade'])
    cv.put({(x, 3) for x in range(0, 40, 5)}, plate_hi)
    return cv


def wall_tile(theme):
    plate, plate_hi, body, body_d, glow = TILE_PAL[theme]
    cv = Canvas(20, 40)
    cv.put(rect(0, 0, 20, 40), body); cv.put(rect(0, 0, 3, 40), plate_hi); cv.put(rect(14, 0, 20, 40), body_d)
    for y in range(0, 40, 10):
        cv.put(rect(0, y, 20, y + 1), body_d); cv.put({(5, y + 4), (6, y + 4), (5, y + 5), (6, y + 5)}, glow)
    cv.put({(0, y) for y in range(40)} | {(19, y) for y in range(40)}, hexc('#0a0508'))
    return cv


# =============================================================================================== props (each returns a list of Canvas frames)
def furnace(n=8):
    """Squat furnace with a glowing mouth; the flames and the light on the floor flicker."""
    out = []
    for i in range(n):
        cv = Canvas(72, 72); t = i / n * TAU
        part(cv, rect(8, 22, 64, 70), IRON, hi=False)
        part(cv, poly([(14, 22), (58, 22), (50, 6), (22, 6)]), IRON, hi=False)
        for y in range(26, 70, 12): cv.put(rect(8, y, 64, y + 2), IRON['shade'])
        for x in (12, 60): cv.put(ell(x, 50, 1.6, 1.6), BRASS['base'])
        mouth = rect(22, 34, 50, 62) | ell(36, 34, 14, 7)
        cv.put(outline_of(mouth), hexc('#06070e')); cv.put(mouth, hexc('#3a0e08'))
        for k in range(5):
            fx = 26 + k * 5 + 1.5 * S(t * 2 + k * 1.7); fh = 14 + 6 * S(t * 3 + k * 2.1) + (4 if k == 2 else 0)
            cv.put(poly([(fx - 4, 62), (fx + 4, 62), (fx + S(t + k) * 2, 62 - fh)]) & mouth, LAVA[0])
            cv.put(poly([(fx - 2, 62), (fx + 2, 62), (fx + S(t + k) * 1.2, 62 - fh * .65)]) & mouth, LAVA[1])
            cv.put(poly([(fx - 1, 62), (fx + 1, 62), (fx, 62 - fh * .35)]) & mouth, LAVA[3])
        halo(cv, 36, 66, 34, hexc('#a0401a'), .22 + .08 * S(t), ry=10)
        part(cv, rect(38, 0, 46, 8), IRON, hi=False)
        for k in range(3): halo(cv, 42 + 2 * S(t + k), 0, 5, hexc('#2a1c20'), .5)
        out.append(cv)
    return out


def anvil(n=6):
    out = []
    for i in range(n):
        cv = Canvas(48, 40); t = i / n * TAU
        part(cv, poly([(8, 12), (40, 12), (36, 20), (28, 22), (28, 30), (34, 36), (14, 36), (20, 30), (20, 22), (12, 20)]), IRON, hi=True)
        cv.put(rect(8, 12, 40, 14), IRON['hi'])
        part(cv, rect(2, 12, 10, 16), IRON, hi=False)
        hot = .5 + .5 * S(t)
        cv.put(rect(18, 8, 32, 12), mix((200, 70, 20), (255, 200, 90), hot)); cv.put(rect(18, 8, 32, 9), LAVA[3] if hot > .5 else LAVA[2])
        halo(cv, 25, 8, 16, hexc('#7a2c14'), .3 * hot + .1, ry=8)
        for k in range(3):                                             # sparks jumping off
            a = (i / n + k / 3.0) % 1.0
            cv.put([(int(25 + (k - 1) * 8 * a), int(8 - 12 * a + 14 * a * a))], LAVA[2] if a < .7 else LAVA[0])
        out.append(cv)
    return out


def chain_hang(n=8):
    """A heavy chain with a hook, swaying (pivot at the top)."""
    out = []
    for i in range(n):
        cv = Canvas(40, 130); t = i / n * TAU
        sw = 3 * S(t)
        pts = [(20 + sw * (y / 100.0) ** 1.5, y) for y in range(0, 100, 5)]
        for k, (x, y) in enumerate(pts):
            l = (ell(x, y + 2.5, 3.2, 4.2) - ell(x, y + 2.5, 1.6, 2.6)) if k % 2 == 0 else rect(int(x) - 1, y, int(x) + 2, y + 5)
            cv.put(l, IRON['base'] if k % 4 < 2 else IRON['shade'])
        x, y = pts[-1]
        hook = line(x, y + 4, x, y + 14, 2.4) | ell(x + 3, y + 20, 7, 7)
        part(cv, hook - ell(x + 3, y + 20, 3.6, 3.6), IRON, hi=False)
        out.append(cv)
    return out


def lavafall(n=8):
    """A fall of molten rock from the ceiling (pivot at the top)."""
    out = []
    for i in range(n):
        cv = Canvas(64, 200); t = i / n * TAU
        for y in range(0, 200):
            w = 11 + 3 * S(y * .08 + t)
            for x in range(int(32 - w), int(32 + w) + 1):
                e = abs(x - 32) / w
                flow = (y * .4 - i * 5.0) % 12
                c = LAVA[0]
                if e < .55: c = LAVA[1]
                if e < .25 and flow < 5: c = LAVA[2]
                if e < .1 and flow < 2: c = LAVA[3]
                cv.px[x, y] = c
        halo(cv, 32, 100, 30, hexc('#a0401a'), .22, ry=100)
        for k in range(6):
            y = (i * 7 + k * 33) % 190
            cv.put([(int(32 + 15 + 4 * S(k * 2 + t)), y), (int(32 - 16 - 3 * S(k + t)), (y + 40) % 190)], LAVA[2])
        out.append(cv)
    return out


def basalt_spike(n=6, scheme=0):
    """Obsidian crystal cluster with veins of ember light."""
    out = []
    for i in range(n):
        cv = Canvas(48, 64); t = i / n * TAU
        glow = .5 + .5 * S(t + scheme)
        for (x, h, w) in ((12, 38, 6), (24, 56, 8), (36, 30, 6)):
            sp = poly([(x - w, 62), (x - w + 2, 62 - h + 6), (x, 62 - h), (x + w - 2, 62 - h + 6), (x + w, 62)])
            part(cv, sp, pal('#241a2c', '#6a4a7a', '#160e1e', '#0e0814', '#060308', '#44305a'), hi=True)
            cv.put({(x - 1, y) for y in range(62 - h + 8, 60, 3)}, mix((255, 90, 20), (255, 200, 90), glow))
        halo(cv, 24, 60, 22, hexc('#7a2c14'), .3 * glow + .1, ry=10)
        out.append(cv)
    return out


def fgear(size=40, teeth=9, n=8, scheme=0):
    """Rotating dark-brass gear; one rotation step = one tooth so the loop is seamless."""
    out = []
    r = size // 2 - 6
    for i in range(n):
        cv = Canvas(size, size); c = size / 2.0
        rot = (i / float(n)) * TAU / teeth
        g = ell(c, c, r, r)
        for k in range(teeth):
            a = k * TAU / teeth + rot
            g |= poly([(c + C(a - .22) * (r - 2), c + S(a - .22) * (r - 2)), (c + C(a - .13) * (r + 5), c + S(a - .13) * (r + 5)), (c + C(a + .13) * (r + 5), c + S(a + .13) * (r + 5)), (c + C(a + .22) * (r - 2), c + S(a + .22) * (r - 2))])
        part(cv, g, pal('#6a4424', '#a87438', '#442a14', '#2a1a0c', '#100804'), hi=True)
        cv.put(ell(c, c, r * .66, r * .66), hexc('#3a2410'))
        for k in range(5):
            a = k * TAU / 5 + rot
            cv.put(line(c, c, c + C(a) * r * .62, c + S(a) * r * .62, 1.8), hexc('#7a5230'))
        cv.put(ell(c, c, r * .24, r * .24), hexc('#140c06')); cv.put(ell(c, c, r * .1, r * .1), hexc('#ff9a3a'))
        out.append(cv)
    return out


def piston(n=8):
    """A tall piston that pumps with a puff of steam at the top of each stroke."""
    out = []
    for i in range(n):
        cv = Canvas(40, 140); t = i / n * TAU
        stroke = 18 * (.5 + .5 * S(t))
        part(cv, rect(8, 50, 32, 138), IRON, hi=True)
        for y in range(60, 130, 14): cv.put(rect(6, y, 34, y + 3), IRON['hi']); cv.put(rect(6, y + 2, 34, y + 4), IRON['deep'])
        part(cv, rect(15, 22 + int(stroke), 25, 52), BRASS, hi=True)
        part(cv, rect(10, 14 + int(stroke), 30, 24 + int(stroke)), BRASS, hi=True)
        if S(t) > .85:
            for k in range(3):
                cv.put(ell(20 + (k - 1) * 6, 10 - k * 3, 4 + k, 3.4 + k * .6), hexc('#a89890')); cv.put(ell(19 + (k - 1) * 6, 9 - k * 3, 2.4 + k, 2 + k * .6), hexc('#e8dcd4'))
        out.append(cv)
    return out


def steam_pipe(n=8):
    out = []
    for i in range(n):
        cv = Canvas(40, 80); t = i / n * TAU
        part(cv, rect(14, 44, 26, 78), IRON, hi=True)
        part(cv, poly([(8, 44), (32, 44), (28, 38), (12, 38)]), BRASS, hi=False)
        for k in range(4):
            ph = (i / n + k / 4.0) % 1.0
            r = 4 + ph * 8; y = 36 - ph * 32
            a = 1 - ph
            cv.put({p for p in ell(20 + 4 * S(k * 2 + t), y, r, r * .8) if dither(p[0], p[1], .35 + .6 * a)}, hexc('#c8bcb4'))
        out.append(cv)
    return out


def crucible(n=8):
    """A hanging crucible that tips and pours a thin stream of metal (pivot at the top)."""
    out = []
    for i in range(n):
        cv = Canvas(64, 150); t = i / n * TAU
        tilt = 5 * S(t)
        cv.put(line(32, 0, 32 + tilt * .4, 50, 1.4), IRON['base']); cv.put(line(32, 0, 20 + tilt * .4, 50, 1.0), IRON['shade']); cv.put(line(32, 0, 44 + tilt * .4, 50, 1.0), IRON['shade'])
        pot = poly([(18 + tilt, 50), (46 + tilt, 50), (42 + tilt, 76), (22 + tilt, 76)])
        part(cv, pot, IRON, hi=False)
        cv.put(ell(32 + tilt, 50, 14, 3), hexc('#1c0e10')); cv.put(ell(32 + tilt, 50, 12, 2), LAVA[0]); cv.put(ell(30 + tilt, 49.6, 6, 1), LAVA[2])
        if S(t) > -.2:
            x = 46 + tilt
            cv.put(line(x, 54, x + 1, 130, 1.3), LAVA[1]); cv.put(line(x, 54, x + 1, 130, .5), LAVA[3])
            cv.put(ell(x + 1, 132, 4, 1.6), LAVA[0])
        halo(cv, 32 + tilt, 62, 24, hexc('#8a3418'), .2, ry=30)
        out.append(cv)
    return out


def brazier(n=8):
    out = []
    for i in range(n):
        cv = Canvas(48, 80); t = i / n * TAU
        part(cv, poly([(18, 78), (30, 78), (27, 54), (21, 54)]), BRASS, hi=False)
        part(cv, poly([(8, 54), (40, 54), (36, 38), (12, 38)]), BRASS, hi=True)
        cv.put(ell(24, 38, 12, 3), hexc('#1c0e08'))
        for k in range(4):
            fx = 15 + k * 6 + 1.5 * S(t * 2 + k); fh = 22 + 8 * S(t * 3 + k * 1.9) + (6 if k in (1, 2) else 0)
            cv.put(poly([(fx - 5, 38), (fx + 5, 38), (fx + S(t + k) * 3, 38 - fh)]), LAVA[0])
            cv.put(poly([(fx - 3, 38), (fx + 3, 38), (fx + S(t + k) * 2, 38 - fh * .68)]), LAVA[1])
            cv.put(poly([(fx - 1.5, 38), (fx + 1.5, 38), (fx, 38 - fh * .36)]), LAVA[3])
        halo(cv, 24, 30, 30, hexc('#a0401a'), .25 + .06 * S(t), ry=34)
        out.append(cv)
    return out


def mantis_banner(n=8):
    """A tall banner with the crest of the Mantis Kings (crossed scythes under a crown), swaying (pivot at the top)."""
    out = []
    for i in range(n):
        cv = Canvas(56, 170); t = i / n * TAU
        cv.put(rect(0, 0, 56, 5), BRASS['base']); cv.put(rect(0, 0, 56, 2), BRASS['hi']); cv.put(rect(0, 4, 56, 6), BRASS['deep'])
        for y in range(6, 160):
            tail = ((y - 6) / 150.0) ** 1.5
            x0 = int(4 + 3 * tail * S(t + y * .04)); x1 = int(52 - 3 * tail * S(t + y * .04 + 1))
            if y > 150: x1 -= (y - 150) // 2; x0 += (y - 150) // 2
            for x in range(x0, x1):
                fold = S(x * .5 + tail * 2 * S(t))
                cv.px[x, y] = hexc('#8a1c24') if fold > .4 else (hexc('#5a1018') if fold < -.5 else hexc('#701822'))
        sw = int(3 * S(t))
        for (x, y) in line(14 + sw, 52, 42 + sw, 96, 1.6) | line(42 + sw, 52, 14 + sw, 96, 1.6):
            cv.px[x, y] = BRASS['hi']
        cv.put(poly([(20 + sw, 40), (36 + sw, 40), (34 + sw, 30), (28 + sw, 36), (22 + sw, 30)]), BRASS['hi'])
        cv.put({(x, 160 + (x % 6 < 3)) for x in range(12, 44, 3)}, BRASS['base'])
        out.append(cv)
    return out


def mantis_statue(n=4):
    """A towering statue of a Mantis King with glowing eyes that flicker (pivot at its feet)."""
    out = []
    for i in range(n):
        cv = Canvas(80, 140); t = i / n * TAU
        part(cv, rect(20, 120, 60, 138), SOOT, hi=False)
        part(cv, poly([(32, 120), (48, 120), (46, 70), (34, 70)]), IRON, hi=False)
        part(cv, poly([(30, 70), (50, 70), (46, 48), (34, 48)]), IRON, hi=False)
        part(cv, ell(40, 38, 11, 10), IRON, hi=True)
        part(cv, poly([(30, 30), (26, 14), (36, 28)]), IRON, hi=False); part(cv, poly([(50, 30), (54, 14), (44, 28)]), IRON, hi=False)
        for sgn in (-1, 1):
            pts = [(40 + sgn * 6, 54), (40 + sgn * 24, 44), (40 + sgn * 34, 70), (40 + sgn * 30, 104)]
            m = set()
            for k in range(3): m |= line(pts[k][0], pts[k][1], pts[k + 1][0], pts[k + 1][1], 2.6 - k * .5)
            part(cv, m, IRON, hi=False)
            cv.put(line(pts[2][0], pts[2][1], pts[3][0], pts[3][1], .6), IRON['hi'])
        fl = .6 + .4 * S(t * 2 + 1)
        for sgn in (-1, 1):
            cv.put(rect(int(40 + sgn * 5) - 2, 36, int(40 + sgn * 5) + 3, 39), mix((255, 90, 30), (255, 220, 120), fl)); halo(cv, 40 + sgn * 5, 37, 8, hexc('#7a2c14'), .3 * fl)
        out.append(cv)
    return out
