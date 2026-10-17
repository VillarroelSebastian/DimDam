"""Projectiles, hazards and effects for the Reino de Hielo."""
import math
from px import *

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')
NAVY_OUT = hexc('#0e1840')

def draw_shard(i, n=6):
    """Harmful frost crystal (spins; freezes on contact)."""
    base = Canvas(24, 24)
    cx, cy = 12, 12
    d = poly([(cx, cy - 9), (cx + 4.5, cy), (cx, cy + 9), (cx - 4.5, cy)])
    base.put(outline_of(d), NAVY_OUT)
    base.put(d, hexc('#3a78e8'))
    base.put({p for p in d if p[0] >= cx}, hexc('#2a52c0'))
    base.put({(cx - 1, cy - 5 + k) for k in range(5)}, hexc('#bff0ff'))
    base.put({(cx, cy - 8), (cx, cy - 7)}, WHITE)
    return rotate(base, i / n * math.pi)

def draw_flake(i, n=6):
    """Parryable snowflake: six arms, PINK core (same colour language as the candy parry bubbles)."""
    cv = Canvas(28, 28)
    cx, cy = 14, 14
    a0 = i / n * (TAU / 6)
    for k in range(6):
        a = a0 + k * TAU / 6
        ex, ey = cx + C(a) * 11, cy + S(a) * 11
        cv.put(line(cx, cy, ex, ey, .75), hexc('#dff4ff'))
        mx, my = cx + C(a) * 6.5, cy + S(a) * 6.5
        for s in (-1, 1):
            cv.put(line(mx, my, mx + C(a + s * .9) * 3.6, my + S(a + s * .9) * 3.6, .6), hexc('#bfe6ff'))
        cv.put({(int(ex), int(ey))}, WHITE)
    cv.put(outline_of(ell(cx, cy, 3.6, 3.6)), hexc('#7a2c70'))
    cv.put(ell(cx, cy, 3.6, 3.6), hexc('#ff79b8')); cv.put({(cx - 1, cy - 1), (cx, cy - 1)}, hexc('#ffe0f0'))
    return cv

def draw_icicle(i, n=3, broken=False):
    cv = Canvas(20, 44)
    cx = 10
    sh = (-1, 0, 1)[i % 3]
    d = poly([(cx - 5 + sh, 2), (cx + 5 + sh, 2), (cx + sh, 40)])
    cv.put(outline_of(d), NAVY_OUT)
    cv.put(d, hexc('#a8e4ff'))
    cv.put({p for p in d if p[0] >= cx + sh}, hexc('#5ab0f0'))
    cv.put({p for p in d if p[0] >= cx + sh + 3}, hexc('#3a78d0'))
    cv.put({(cx - 3 + sh, 5 + k) for k in range(12)}, hexc('#e8fbff'))
    cv.put({(cx - 3 + sh, 4)}, WHITE)
    return cv

def draw_icicle_shatter(i, n=5):
    cv = Canvas(40, 32); t = i / (n - 1)
    cx, cy = 20, 26
    for k in range(9):
        a = -math.pi * (0.1 + 0.8 * k / 8.0)
        d = 4 + t * (15 + (k % 3) * 4)
        x, y = cx + C(a) * d, cy + S(a) * d * (1.0 - 0.35 * t) + t * t * 10
        sz = max(1.0, 4.4 - t * 3.0)
        cv.put(poly([(x, y - sz * 1.6), (x + sz, y), (x, y + sz * 1.6), (x - sz, y)]), hexc('#bfeaff') if k % 2 else hexc('#5ab0f0'))
    if t < .5: cv.put(ring(ell(cx, cy, 5 + t * 18, 3 + t * 8)), WHITE)
    return cv

def draw_crystal(i, active, n=6):
    """Resonant crystal. Inactive: dim and slow pulse. Active: bright, rings of sound."""
    cv = Canvas(36, 60)
    t = i / n * TAU
    pulse = .5 + .5 * S(t)
    base = (dict(base=hexc('#6ad8ff'), hi=hexc('#e4fbff'), shade=hexc('#2a9ae0'), deep=hexc('#1c5ab8'), out=hexc('#10206a'), out_l=hexc('#2a7ad0'))
            if active else dict(base=hexc('#4a78b8'), hi=hexc('#8ab0e0'), shade=hexc('#34589a'), deep=hexc('#243c78'), out=hexc('#10206a'), out_l=hexc('#34589a')))
    if active:
        for k in range(2):
            ph = (i / n + k * .5) % 1.0
            cv.put(ring(ell(18, 36, 6 + ph * 14, 4 + ph * 9)), hexc('#e4fbff') if ph < .6 else hexc('#7ad0ff'))
    for (cx, h, wd, top_dx) in ((18, 46, 7, 0), (8, 30, 5, -1), (28, 34, 5, 1)):
        top = 58 - h
        sh = poly([(cx - wd, 58), (cx - wd, top + 9), (cx + top_dx, top), (cx + wd, top + 9), (cx + wd, 58)])
        part(cv, sh, base)
        cv.put({(cx + top_dx, y) for y in range(top + 2, 54, 1) if (cx + top_dx, y) in sh}, base['hi'] if (active or pulse > .5) else base['shade'])
        if active and pulse > .55: cv.put({(cx - wd + 2, top + 11 + k) for k in range(5)}, WHITE)
    return cv

def draw_note(i, n=3):
    cv = Canvas(14, 16); y = 0 if i == 0 else (1 if i == 1 else -1)
    col = hexc('#e4fbff')
    cv.put(ell(5, 12 + y, 2.6, 2.1), col); cv.put(line(7, 12 + y, 7, 3 + y, .6), col); cv.put({(8, 3 + y), (9, 4 + y), (9, 5 + y), (8, 6 + y)}, col)
    return cv

def draw_freeze(i, n=4, hero_w=48, hero_h=60):
    """Ice block that encases a frozen player (translucent, shimmering)."""
    cv = Canvas(52, 64)
    block = rect(8, 12, 44, 58) | ell(26, 14, 18, 8) | ell(26, 58, 18, 5)
    block = {p for p in block if 0 <= p[0] < 52 and 0 <= p[1] < 64}
    for (x, y) in block:
        a = 150 if (x + y) % 5 else 175
        cv.px[x, y] = (150, 215, 255, a)
    cv.put(outline_of(block), (60, 130, 220, 255))
    cv.put({p for p in block if (p[0] + 2, p[1] + 2) not in block}, (60, 130, 220, 200))
    cv.put({p for p in block if (p[0] - 1, p[1] - 1) not in block and p[0] < 26}, (255, 255, 255, 235))
    sx = (i * 9) % 36
    cv.put({(10 + sx + k, 20 + k * 2) for k in range(6) if (10 + sx + k, 20 + k * 2) in block}, (255, 255, 255, 230))
    cv.put({(11 + sx + k, 20 + k * 2) for k in range(6) if (11 + sx + k, 20 + k * 2) in block}, (255, 255, 255, 170))
    cv.put({(14, 40), (15, 41), (17, 38), (36, 30), (35, 31)}, (255, 255, 255, 255))
    return cv

def draw_freeze_break(i, n=5):
    cv = Canvas(52, 64); t = i / (n - 1)
    cx, cy = 26, 36
    for k in range(14):
        a = k / 14 * TAU + .4
        d = 4 + t * (20 + (k % 4) * 3)
        x, y = cx + C(a) * d, cy + S(a) * d * 1.1 + t * t * 14
        sz = max(.8, 4.0 - t * 3)
        cv.put(poly([(x - sz, y - sz), (x + sz * 1.2, y - sz * .4), (x + sz * .4, y + sz * 1.2)]), (190, 235, 255, 255) if k % 2 else (90, 160, 235, 255))
    if t < .4: cv.put(ring(ell(cx, cy, 14 + t * 30, 18 + t * 30)), (255, 255, 255, 255))
    return cv

def draw_gust(i, n=3):
    """Wind streak used by the blizzard gusts."""
    cv = Canvas(40, 6); L = [22, 30, 36][i % 3]
    cv.put({(x, 2) for x in range(6, 6 + L)}, (230, 245, 255, 220)); cv.put({(x, 3) for x in range(10, 6 + L - 6)}, (180, 215, 245, 200))
    cv.put({(x, 1) for x in range(12, 6 + L - 10)}, (255, 255, 255, 160))
    return cv


def draw_crack(i, n=3):
    """Warning crack on the floor where a pillar will erupt."""
    cv = Canvas(40, 8)
    pts = [(2, 4), (8, 3), (12, 5), (18, 3), (22, 5), (28, 3), (33, 5), (38, 4)]
    for k in range(len(pts) - 1):
        cv.put(line(pts[k][0], pts[k][1] + (i % 2), pts[k + 1][0], pts[k + 1][1] + (i % 2), .6), hexc('#ffffff') if (k + i) % 2 else hexc('#7ad8ff'))
    cv.put({(x, 6) for x in range(4 + i * 3, 36, 9)}, hexc('#2a62b8'))
    return cv

def draw_pillar_part(kind):
    """kind: base | mid | tip   (16px tall slices of a faceted ice pillar)."""
    if kind == 'base':
        cv = Canvas(32, 16)
        b = rect(2, 3, 30, 16) | rect(0, 10, 32, 16)
        cv.put(outline_of(b), NAVY_OUT); cv.put(b, hexc('#8fd8ff'))
        cv.put({p for p in b if p[0] > 18}, hexc('#4aa0e8')); cv.put({p for p in b if p[0] > 26}, hexc('#2a62b8'))
        cv.put(rect(3, 3, 29, 6), hexc('#f4fbff')); cv.put({(x, 3) for x in range(3, 29)}, WHITE)
        cv.put({(9, y) for y in range(7, 15)}, hexc('#e4faff'))
        return cv
    if kind == 'mid':
        cv = Canvas(32, 16)
        b = rect(4, 0, 28, 16)
        cv.put(outline_of(b) & {(x, y) for x in range(32) for y in range(16)}, NAVY_OUT); cv.put(b, hexc('#9be0ff'))
        cv.put({p for p in b if p[0] > 17}, hexc('#52a6e8')); cv.put({p for p in b if p[0] > 24}, hexc('#2c64b8'))
        cv.put({(9, y) for y in range(1, 15)}, hexc('#e4faff')); cv.put({(10, y) for y in range(3, 11)}, WHITE)
        return cv
    cv = Canvas(32, 22)
    t = poly([(4, 22), (4, 12), (16, 0), (28, 12), (28, 22)])
    cv.put(outline_of(t), NAVY_OUT); cv.put(t, hexc('#9be0ff'))
    cv.put({p for p in t if p[0] > 16}, hexc('#52a6e8')); cv.put({p for p in t if p[0] > 24}, hexc('#2c64b8'))
    cv.put({(9, y) for y in range(10, 21)}, hexc('#e4faff')); cv.put({(15, 3), (16, 3), (15, 5)}, WHITE)
    return cv

def recolor_shock(canvas):
    """Candy shock wave -> frost shock wave (same silhouette, blue palette)."""
    mp = {hexc('#ff79b8'): hexc('#7ad8ff'), hexc('#ffd0ec'): hexc('#e4fbff'), hexc('#43103f'): hexc('#14306e')}
    out = Canvas(canvas.w, canvas.h)
    for y in range(canvas.h):
        for x in range(canvas.w):
            p = canvas.px[x, y]
            if p[3]: out.px[x, y] = mp.get(p, p)
    return out
