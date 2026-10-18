"""Small effects of the Fire Kingdom: enemy fire bolts, geyser crack + flame column segments, falling cinders and their burst."""
import math, random
from px import *
from fire_env import pal, halo, IRON, LAVA

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')


def fire_shot(i, n=6):
    """A spinning bolt of fire: a hot core, a ragged flame body and a short streak behind it (rotates with i)."""
    cv = Canvas(24, 24); cx = cy = 12
    a = i / n * TAU
    body = ell(cx, cy, 6.4, 6.4)
    for k in range(5):                                                    # flame tongues rotating around the core
        b = a + k * TAU / 5
        body |= poly([(cx + C(b - .5) * 5, cy + S(b - .5) * 5), (cx + C(b) * (10 + 1.5 * S(a * 2 + k)), cy + S(b) * (10 + 1.5 * S(a * 2 + k))), (cx + C(b + .5) * 5, cy + S(b + .5) * 5)])
    cv.put(outline_of(body), hexc('#3a1008'))
    cv.put(body, LAVA[0])
    cv.put({p for p in body if math.hypot(p[0] - cx, p[1] - cy) < 8}, LAVA[1])
    cv.put(ell(cx, cy, 4.4, 4.4), LAVA[2]); cv.put(ell(cx - .5, cy - .5, 2.4, 2.4), LAVA[3])
    return cv


def fire_crack(i, n=3):
    """The glowing crack that warns where a geyser will erupt."""
    cv = Canvas(40, 8)
    rng = random.Random(4)
    x = 2; y = 4
    while x < 38:
        cv.put([(x, y)], LAVA[1] if (x + i) % 3 else LAVA[2]); cv.put([(x, y + 1)], LAVA[0])
        x += 1; y += rng.choice((-1, 0, 0, 1)); y = max(1, min(6, y))
    halo(cv, 20, 4, 18, hexc('#a0401a'), .3 + .1 * i, ry=4)
    for sx in (8, 20, 31): cv.put({(sx, 3 - (i % 2)), (sx, 2 - (i % 2))}, LAVA[3])
    return cv


def geyser_seg(part_, flick):
    """32x22 segment of the flame column (base / mid / tip); `flick` alternates the flame edge so it dances."""
    cv = Canvas(32, 22)
    w = 12 if part_ == 'mid' else (14 if part_ == 'base' else 9)
    for y in range(22):
        ww = w - (1 if (y + flick) % 5 == 0 else 0) - (int((y / 22.0) * 6) if part_ == 'tip' else 0)
        for x in range(16 - ww, 16 + ww):
            e = abs(x - 16) / float(max(1, ww))
            c = LAVA[0] if e > .62 else (LAVA[1] if e > .3 else LAVA[2])
            if e < .12 and (y + flick) % 6 < 3: c = LAVA[3]
            cv.put([(x, y)], c)
    if part_ == 'tip':
        cv.put(poly([(10, 4), (16, -2 + flick), (22, 4)]), LAVA[1])
    if part_ == 'base':
        cv.put(rect(2, 18, 30, 22), hexc('#3a1008')); cv.put({(x, 18) for x in range(2, 30, 2)}, LAVA[2])
    return cv


def cinder(i, n=3):
    """A falling lump of burning rock with a flame trail above it (20x44, falling downward)."""
    cv = Canvas(20, 44)
    rock = ell(10, 32, 7.5 + (i % 2) * .4, 9) | poly([(5, 28), (15, 28), (10, 40)])
    part(cv, rock, pal('#3a2430', '#8a5a6a', '#26161e', '#160a10', '#08040a'), hi=False)
    cv.put({(7 + k, 29 + (k % 2) * 3) for k in range(0, 7, 2)}, LAVA[1]); cv.put({(9, 33), (10, 34), (11, 35)}, LAVA[2])
    for k in range(4):
        h = 6 + k * 5 + (i * 3) % 4
        cv.put(poly([(10 - 4 + k, 24 - h * .2), (10 + (k % 2) * 2 - 1, 24 - h), (10 + 4 - k, 24 - h * .2)]), LAVA[0] if k < 2 else LAVA[1])
    cv.put(ell(10, 22, 2.2, 3.4), LAVA[3])
    return cv


def cinder_burst(i, n=5):
    cv = Canvas(40, 32); t = i / (n - 1); cx, cy = 20, 26
    if t < .3:
        r = 6 + t * 30
        cv.put(ell(cx, cy - r * .3, r, r * .6), LAVA[2]); cv.put(ell(cx, cy - r * .3, r * .6, r * .36), LAVA[3])
    else:
        k = (t - .3) / .7
        for j in range(10):
            a = -math.pi * (j + .5) / 10; d = 4 + k * 17 + (j % 3) * 2
            sz = max(1, 3.4 - k * 3 + (j % 2) * .6)
            cv.put(ell(cx + C(a) * d, cy + S(a) * d * 1.1 + k * 6 * k, sz, sz), LAVA[j % 3])
        cv.put({(x, cy - 1 + (x % 2)) for x in range(cx - int(8 + k * 12), cx + int(8 + k * 12))}, LAVA[0] if k < .6 else hexc('#5a1c10'))
    return cv
