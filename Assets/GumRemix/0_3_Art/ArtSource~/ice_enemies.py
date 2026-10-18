"""Creatures of the Reino de Hielo (Penguin Squire, Patchwork Snowman, Frost Owl, Boreal Jellyfish, Igloo Crab, Snow Seed)
plus their snowball (captured) forms. Same method as the candy world: native resolution, fixed light, flat ramps, parametric cycles."""
import math
from px import *

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')
EYE = hexc('#1a1230')

def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

NAVY = pal('#2a3868', '#6a82bc', '#1c2650', '#121836', '#080c20', '#222f5c')
SNOW = pal('#f6fcff', '#ffffff', '#c6def4', '#96b4e0', '#34408a', '#b4cdee')
ICE = pal('#9be0ff', '#e4faff', '#52a6e8', '#2c64b8', '#14306e', '#3a86d0')
ORANGE = pal('#c88a3c', '#ecc080', '#a05a2c', '#6c301c', '#2a1020')
FROST = pal('#e2f2ff', '#ffffff', '#a8c8ee', '#7898d0', '#283c80', '#98b8e4')
CORALR = pal('#b8503e', '#e09284', '#82343c', '#521e30', '#200c1e', '#923c40')
LEAF = pal('#6aa090', '#b4d8c4', '#46705e', '#2e4a4a', '#0e1c28')
WOOD = pal('#a8703c', '#d89c60', '#7a4a2c', '#4e2c24', '#2a1418')

# ====================================================================== PENGUIN SQUIRE
PW, PH = 64, 56
PG = 51          # feet baseline

def draw_penguin(dy=0, tilt=0, feet=((-4, 0), (4, 0)), flipper=0, shield=(0, 0), eyes='open', slide=0.0, glow=0, beak=0):
    cv = Canvas(PW, PH)
    cx = PW // 2
    if slide > 0:    # belly slide: body lies flat, shield leads
        k = slide
        by = 44 - 2 * k
        for i, fx in enumerate((cx - 15 - 3 * k, cx - 11 - 2 * k)):
            part(cv, ell(fx, by + 3 + i, 5.5, 2.4), ORANGE, hi=False)
        body = ell(cx - 1, by, 14 + 2 * k, 8.5 - 1.2 * k)
        part(cv, body, NAVY)
        belly = ell(cx, by + 3, 11 + 2 * k, 4.5)
        cv.put(belly & body, SNOW['base']); cv.put({p for p in belly & body if (p[0], p[1] + 1) not in body or (p[0], p[1] + 1) not in belly}, SNOW['shade'])
        head = ell(cx + 14 + 2 * k, by - 3, 7.5, 7)
        part(cv, head, NAVY)
        cv.put(ell(cx + 17 + 2 * k, by - 2, 4.2, 4), SNOW['base'])
        cv.put(rect(cx + 15 + 2 * k, by - 5, cx + 19 + 2 * k, by - 2), EYE); cv.put({(cx + 15 + 2 * k, by - 5)}, WHITE)
        cv.put(poly([(cx + 21 + 2 * k, by - 2), (cx + 27 + 2 * k, by), (cx + 21 + 2 * k, by + 2)]), ORANGE['base'])
        cv.put({(cx + 14 + 2 * k, by - 7), (cx + 15 + 2 * k, by - 7), (cx + 16 + 2 * k, by - 6), (cx + 19 + 2 * k, by - 7), (cx + 20 + 2 * k, by - 7)}, hexc('#0a1028'))
        sh = ell(cx + 26 + 2 * k, by - 1, 4.8, 10)
        part(cv, sh, ICE)
        sparkle(cv, cx + 26 + 2 * k, by - 1)
        part(cv, ell(cx - 6, by - 6, 4, 2.6), NAVY, hi=False)   # trailing flipper
        return cv
    ox = tilt
    for i, (fx, lift) in enumerate(feet):
        part(cv, ell(cx + fx + (1 if i else -1) * 0 + ox * .3, PG - lift, 4.8, 2.3), ORANGE, hi=False)
    body = ell(cx + ox * .5, 33 + dy, 10.5, 14)
    part(cv, body, NAVY)
    belly = ell(cx + 2 + ox * .5, 36 + dy, 6.6, 10)
    cv.put(belly, SNOW['base']); cv.put({p for p in belly if (p[0], p[1] + 1) not in belly or (p[0] - 1, p[1]) not in belly}, SNOW['shade'])
    # far flipper (behind)
    fa = flipper
    part(cv, line(cx - 8 + ox * .5, 29 + dy, cx - 13 + ox * .5 - fa * .4, 38 + dy - fa, 2.1), NAVY, hi=False)
    head = ell(cx + 1 + ox, 15 + dy, 9.6, 8.8)
    part(cv, head, NAVY)
    face = ell(cx + 4 + ox, 17 + dy, 6.4, 5.8)
    cv.put(face, SNOW['base']); cv.put({p for p in face if (p[0], p[1] + 1) not in face}, SNOW['shade'])
    ex = cx + 2 + ox
    if eyes == 'open':
        for e in (ex - 1, ex + 6):
            cv.put(rect(e, 13 + dy, e + 3, 18 + dy), EYE); cv.put({(e, 13 + dy), (e, 14 + dy)}, WHITE)
        cv.put({(ex - 3, 11 + dy), (ex - 2, 11 + dy), (ex - 1, 12 + dy), (ex + 6, 12 + dy), (ex + 7, 11 + dy), (ex + 8, 11 + dy)}, hexc('#0a1028'))
    elif eyes == 'blink':
        for e in (ex - 1, ex + 6): cv.put({(e, 16 + dy), (e + 1, 16 + dy), (e + 2, 16 + dy)}, EYE)
    else:
        for e in (ex - 1, ex + 6): cv.put({(e, 13 + dy), (e + 2, 13 + dy), (e + 1, 15 + dy), (e, 17 + dy), (e + 2, 17 + dy)}, EYE)
    bk = poly([(ex + 8, 17 + dy), (ex + 15 + beak, 19 + dy), (ex + 8, 22 + dy)])
    part(cv, bk, ORANGE, hi=False, k=1)
    # shield (held in the near flipper, on the facing side): this is what blocks gum from the front
    sx, sy = cx + 12 + shield[0] + ox * .5, 33 + dy + shield[1]
    part(cv, line(cx + 8 + ox * .5, 30 + dy, sx - 3, sy - 2, 2.2), NAVY, hi=False)
    sh = ell(sx + 1, sy, 6.8, 10)
    part(cv, sh, ICE)
    cv.put(ring(ell(sx + 1, sy, 4.6, 7.8)) & sh, ICE['shade'])
    sparkle(cv, sx + 1, sy)
    if glow:
        cv.put(ring(sh, 1), WHITE)
        cv.put({(int(sx + 1), int(sy) - 11 - glow), (int(sx + 1), int(sy) + 11 + glow)}, WHITE)
    return cv

def sparkle(cv, x, y):
    x, y = int(x), int(y)
    cv.put({(x, y - 3), (x, y - 2), (x, y - 1), (x, y), (x, y + 1), (x, y + 2), (x, y + 3), (x - 3, y), (x - 2, y), (x - 1, y), (x + 1, y), (x + 2, y), (x + 3, y),
            (x - 2, y - 2), (x + 2, y + 2), (x - 2, y + 2), (x + 2, y - 2)}, WHITE)

def penguin_frames():
    F = {}
    F['idle'] = [draw_penguin(dy=round(.6 + .6 * S(i / 6 * TAU)), flipper=round(S(i / 6 * TAU - .8) * 1.5), shield=(0, round(S(i / 6 * TAU - .4))), eyes='blink' if i == 4 else 'open') for i in range(6)]
    wd = []
    for i in range(8):
        p = i / 8 * TAU
        lift = lambda ph: max(0, round(3.2 * math.sin(ph + 1.57)))
        wd.append(draw_penguin(dy=-round(1.2 * abs(C(p))), tilt=round(2.2 * S(p)), feet=((-4 + round(2 * S(p + math.pi)), lift(p + math.pi)), (4 + round(2 * S(p)), lift(p))),
                               flipper=round(3 * S(p + math.pi)), shield=(round(S(p) * .8), round(S(p - .6))), eyes='open'))
    F['waddle'] = wd
    F['slide'] = [draw_penguin(slide=.0 + .45), draw_penguin(slide=.8), draw_penguin(slide=1.0), draw_penguin(slide=.8)]
    F['hurt'] = [draw_penguin(dy=1, tilt=-3, flipper=4, shield=(-3, 3), eyes='hurt', beak=-1), draw_penguin(dy=2, tilt=-4, flipper=5, shield=(-4, 4), eyes='hurt', beak=-1)]
    F['block'] = [draw_penguin(shield=(2, 0), glow=1), draw_penguin(shield=(1, 0), glow=2)]
    return pad_family(F, 10, 2, 10, 4)
PENGUIN_ANIMS = dict(idle=(8, True), waddle=(14, True), slide=(14, False), hurt=(10, False), block=(18, False))

# ====================================================================== PATCHWORK SNOWMAN
NW, NH = 48, 64
NG = 61

def patch(cv, x, y, w, h, col):
    cv.put(rect(x, y, x + w, y + h), col)
    cv.put({(x + k, y) for k in range(0, w, 2)} | {(x + k, y + h - 1) for k in range(1, w, 2)} | {(x, y + k) for k in range(0, h, 2)} | {(x + w - 1, y + k) for k in range(1, h, 2)}, hexc('#3a2a44'))

def draw_snowman(dy=0, sq=0, lean=0, head=True, arms=((-9, 4), (9, 4)), eyes='open', hdx=0, hdy=0, mouth='smile', head_lift=0):
    cv = Canvas(NW, NH)
    cx = 24
    bx = cx + lean * .4
    # twig arms behind the body (thick enough to read at game size)
    for sgn, (dx_, dy_) in ((-1, arms[0]), (1, arms[1])):
        sx = bx + sgn * 9; sy = 36 + dy
        ex, ey = sx + dx_, sy + dy_
        tw = line(sx, sy, ex, ey, 1.25)
        cv.put(outline_of(tw), WOOD['out']); cv.put(tw, WOOD['base']); cv.put({p for p in tw if (p[0] + 1, p[1] + 1) not in tw}, WOOD['shade'])
        for fy in (-3.4, 0.8):
            fg = line(ex, ey, ex + sgn * 3.6, ey + fy, .7)
            cv.put(outline_of(fg) - tw, WOOD['out']); cv.put(fg, WOOD['base'])
    low = ell(bx, 50 + dy * .5 + sq, 13 + sq * .5, 11.5 - sq * .5)
    part(cv, low, SNOW)
    mid = ell(bx + lean * .2, 35 + dy, 10.4 - sq * .2, 9.4 + sq * .2)
    part(cv, mid, SNOW)
    patch(cv, int(bx) - 10, 46 + int(dy * .5) + sq, 8, 6, hexc('#6a4a6c')); patch(cv, int(bx) + 3, 52 + int(dy * .5) + sq, 7, 5, hexc('#4a5a7c'))
    patch(cv, int(bx) + 1, 30 + dy, 6, 6, hexc('#3a4a5c'))
    cv.put({(int(bx) - 6 + k, 40 + dy + (k % 2)) for k in range(0, 10, 2)}, hexc('#3a2a44'))
    for by_ in (34 + dy, 39 + dy): cv.put(ell(bx - 2, by_, 1.5, 1.5), hexc('#2a2a44'))
    if head:
        hx, hy = bx + hdx, 20 + dy + hdy - head_lift
        hd = ell(hx, hy, 9.6, 8.8)
        part(cv, hd, SNOW)
        patch(cv, int(hx) - 9, int(hy) - 6, 6, 5, hexc('#3a4a6c'))
        if eyes == 'open':
            cv.put(ell(hx - 3.2, hy - 1, 2.5, 2.5), hexc('#2a2a44')); cv.put(ell(hx + 4.4, hy - 1.6, 1.8, 1.8), hexc('#2a2a44'))
            cv.put({(int(hx) - 4, int(hy) - 3), (int(hx) - 3, int(hy) - 3), (int(hx) + 4, int(hy) - 3)}, WHITE)
            cv.put({(int(hx) - 6, int(hy) - 5), (int(hx) - 5, int(hy) - 4), (int(hx) - 4, int(hy) - 4)}, hexc('#2a2a44'))
        elif eyes == 'hurt':
            for ex in (hx - 3, hx + 4): cv.put({(int(ex) - 1, int(hy) - 3), (int(ex), int(hy) - 2), (int(ex) + 1, int(hy) - 1), (int(ex) + 1, int(hy) - 3), (int(ex) - 1, int(hy) - 1)}, hexc('#2a2a44'))
        else:
            for ex in (hx - 3, hx + 4): cv.put({(int(ex) - 1, int(hy) - 1), (int(ex), int(hy) - 1), (int(ex) + 1, int(hy) - 1)}, hexc('#2a2a44'))
        nose = poly([(hx + 1, hy + 0.5), (hx + 13, hy + 3.5), (hx + 1, hy + 6)])
        part(cv, nose, pal('#c8803a', '#e8b878', '#a05a2c', '#6c301c', '#2a1020'), hi=False, k=1)
        if mouth == 'smile':       # a flat, downturned line of coal: the mournful snowman
            cv.put({(int(hx) - 6 + k, int(hy) + 7 + (0 if 1 < k < 8 else 1)) for k in range(10)}, hexc('#2a2a44'))
        elif mouth == 'o':
            cv.put(ell(hx + 1, hy + 7, 2.2, 2.0), hexc('#2a2a44'))
        brim = ell(hx, hy - 8, 10.5, 2.4)                      # a battered black top hat, tilted and dented
        part(cv, brim, pal('#24243c', '#5a5a80', '#14142a', '#0a0a1a', '#04040c'), hi=False, k=1)
        crown_ = poly([(hx - 6, hy - 8), (hx + 6, hy - 8), (hx + 5, hy - 19), (hx + 1, hy - 20), (hx - 5, hy - 18)])
        part(cv, crown_, pal('#24243c', '#5a5a80', '#14142a', '#0a0a1a', '#04040c'), hi=False, k=1)
        cv.put({(int(hx) - 5 + k, int(hy) - 10) for k in range(10)}, hexc('#5a3a5c'))         # a frayed ribbon
        cv.put({(int(hx) + 3, int(hy) - 16), (int(hx) + 2, int(hy) - 15)}, hexc('#7a7aa0'))
    else:
        cv.put(ell(bx, 27 + dy, 6.5, 2.4), SNOW['shade'])
    return cv

def snowman_frames():
    F = {}
    F['idle'] = [draw_snowman(dy=round(.5 + .5 * S(i / 6 * TAU)), hdx=round(S(i / 6 * TAU - .8)), hdy=round(S(i / 6 * TAU - .5) * .8), arms=((-9, 4 + round(S(i / 6 * TAU))), (9, 4 + round(S(i / 6 * TAU + 1)))), eyes='blink' if i == 4 else 'open') for i in range(6)]
    hp = []
    for i in range(8):
        p = i / 8 * TAU
        up = max(0, S(p)); down = max(0, -S(p))
        hp.append(draw_snowman(dy=-round(2.6 * up) + round(1.4 * down), sq=round(1.6 * down) - round(1.0 * up), lean=round(2 * C(p)), hdx=round(S(p - 1.0) * 1.6), hdy=round(S(p - .6) * 1.4),
                               arms=((-10, 2 - round(3 * up)), (10, 2 - round(3 * up))), mouth='o' if up > .6 else 'smile'))
    F['hop'] = hp
    F['throw'] = [draw_snowman(sq=1, lean=-2, arms=((-8, 6), (11, -2)), head_lift=0, hdx=-1),
                  draw_snowman(sq=-1, lean=-3, arms=((-8, 3), (10, -9)), head_lift=3, hdx=-1, mouth='o'),
                  draw_snowman(sq=-1, lean=-3, arms=((-8, 3), (11, -14)), head_lift=8, hdx=-1, mouth='o'),
                  draw_snowman(head=False, lean=3, arms=((-10, -6), (12, -10)), sq=-1),
                  draw_snowman(head=False, lean=1, arms=((-10, -4), (10, -6))),
                  draw_snowman(head=False, arms=((-9, 2), (9, 2)))]
    F['idle_nohead'] = [draw_snowman(head=False, dy=round(.5 + .5 * S(i / 4 * TAU)), arms=((-9, 4 + round(S(i / 4 * TAU) * 2)), (9, 4 - round(S(i / 4 * TAU) * 2)))) for i in range(4)]
    hn = []
    for i in range(8):
        p = i / 8 * TAU; up = max(0, S(p)); down = max(0, -S(p))
        hn.append(draw_snowman(head=False, dy=-round(3 * up) + round(1.4 * down), sq=round(1.4 * down), lean=round(2.6 * C(p)), arms=((-11, -3 - round(4 * S(p * 2))), (11, -3 + round(4 * S(p * 2))))))
    F['run_nohead'] = hn
    F['regrow'] = [draw_snowman(head=False, sq=1), draw_snowman(head=True, head_lift=-4, hdy=3, sq=1, eyes='closed'), draw_snowman(head=True, hdy=2, sq=1, eyes='closed'), draw_snowman(head=True, sq=0, mouth='o')]
    F['hurt'] = [draw_snowman(dy=1, sq=1, lean=-3, eyes='hurt', mouth='o', arms=((-11, -2), (11, -2))), draw_snowman(dy=2, sq=2, lean=-4, eyes='hurt', mouth='o', arms=((-12, -4), (12, -4)))]
    return pad_family(F, 6, 12, 6, 4)
SNOWMAN_ANIMS = dict(idle=(8, True), hop=(12, True), throw=(11, False), idle_nohead=(8, True), run_nohead=(16, True), regrow=(10, False), hurt=(10, False))

def snow_head_frames(n=8):
    base = Canvas(28, 28); cx, cy = 14, 14
    hd = ell(cx, cy, 10.2, 9.4); part(base, hd, SNOW)
    patch(base, 6, 9, 5, 4, hexc('#6a9aef'))
    base.put(ell(cx - 3, cy - 1, 1.9, 1.9), hexc('#2a2a44')); base.put(ell(cx + 4, cy - 1.4, 1.4, 1.4), hexc('#2a2a44'))
    nose = poly([(cx + 1, cy + 1), (cx + 11, cy + 3), (cx + 1, cy + 4)]); part(base, nose, pal('#ff8a30', '#ffc890', '#e0602a', '#a03c2c', '#40142a'), hi=False, k=1)
    base.put({(cx - 3 + k, cy + 6) for k in range(7)}, hexc('#2a2a44'))
    hat = poly([(cx - 5, cy - 7), (cx + 4, cy - 8), (cx + 1, cy - 13)]); part(base, hat, pal('#e8625c', '#ff9a8a', '#b8403c', '#7a2c3c', '#2a1030'), hi=False, k=1)
    return [rotate(base, i / n * TAU) for i in range(n)]

# ====================================================================== FROST OWL
OW, OH = 60, 56

def draw_owl(wing=0.0, dy=0, tilt=0, eyes='open', dive=0.0, hurt=False):
    """wing in [-1 (up) .. 1 (down)]."""
    cv = Canvas(OW, OH)
    cx, cy = 30, 30 + dy
    # wings: a tapered paddle driven by a real angle, with three feather fingers at the tip
    for side, near in ((-1, False), (1, True)):
        ang = -1.15 + (wing + 1) * 0.5 * 1.8          # wing=-1 -> raised, wing=+1 -> lowered
        if dive: ang = -1.25 - dive * 0.1
        rx_, ry_ = cx + side * 8, cy - 3 + dive * 3
        L = 21 - dive * 7
        tipx, tipy = rx_ + side * math.cos(ang) * L, ry_ + math.sin(ang) * L
        pad_ = line(rx_, ry_, tipx, tipy, 4.2) | line(rx_ + side * 2, ry_ + 3, tipx, tipy + 2, 2.6)
        for k in (-1, 0, 1):
            fa = ang + k * 0.38
            pad_ |= line(tipx - side * 3, tipy, tipx + side * math.cos(fa) * 8, tipy + math.sin(fa) * 8, 1.5)
        part(cv, pad_, dict(FROST, base=FROST['hi'] if near else FROST['base']), hi=False, k=2)
        cv.put({p for p in pad_ if (p[0] - side, p[1] - 1) not in pad_ and p[1] < ry_}, WHITE)
    # tail feathers
    part(cv, poly([(cx - 5, cy + 12), (cx + 5, cy + 12), (cx + 7, cy + 20), (cx, cy + 22), (cx - 7, cy + 20)]), dict(FROST, base=FROST['shade'], shade=FROST['deep']), hi=False, k=1)
    body = ell(cx + tilt * .3, cy + 3, 12.5, 14)
    part(cv, body, FROST)
    # chest feather scallops
    for r in range(3):
        for k in range(3 + r % 2):
            fx = cx - 5 + k * 5 + (2.5 if r % 2 == 0 else 0); fy = cy + 5 + r * 4.5
            cv.put({(int(fx) + a, int(fy) + (1 if abs(a) > 1 else 2)) for a in (-2, -1, 0, 1, 2)}, FROST['deep'])
    # head + ear tufts
    for sx in (-1, 1): part(cv, tri((cx + sx * 8, cy - 11), (cx + sx * 11, cy - 21 + dive * 3), (cx + sx * 3, cy - 14)), FROST, hi=False, k=1)
    head = ell(cx + tilt * .5, cy - 7, 12, 10)
    part(cv, head, FROST)
    # mask: darker facial disc, big yellow eyes
    disc = ell(cx + tilt * .5 + 1, cy - 6, 9.6, 7.6)
    cv.put(disc, hexc('#c4def6')); cv.put({p for p in disc if (p[0], p[1] + 1) not in disc}, FROST['shade'])
    for ex in (cx - 5, cx + 5):
        e = ell(ex + tilt * .5 + 1, cy - 7, 4.4, 4.6)
        cv.put(outline_of(e), hexc('#1a2050'))
        if hurt:
            cv.put({(int(ex) - 2 + tilt // 2 + i, int(cy) - 9 + i) for i in range(5)} | {(int(ex) - 2 + tilt // 2 + i, int(cy) - 5 - i) for i in range(5)}, hexc('#1a2050'))
        else:
            cv.put(e, hexc('#ffe36a'))
            cv.put({p for p in e if (p[0], p[1] + 1) not in e or (p[0] + 1, p[1]) not in e}, hexc('#f0a82a'))
            if eyes == 'open':
                cv.put(rect(int(ex) + 0 + tilt // 2, int(cy) - 9, int(ex) + 3 + tilt // 2, int(cy) - 4), hexc('#1a1230')); cv.put({(int(ex) + tilt // 2, int(cy) - 9)}, WHITE)
            else:
                cv.put(rect(int(ex) - 1 + tilt // 2, int(cy) - 9, int(ex) + 3 + tilt // 2, int(cy) - 8), hexc('#1a2050')) if eyes == 'angry' else None
                cv.put(rect(int(ex) + tilt // 2, int(cy) - 7, int(ex) + 2 + tilt // 2, int(cy) - 3), hexc('#1a1230'))
    cv.put({(cx - 9, cy - 13), (cx - 8, cy - 12), (cx - 7, cy - 11), (cx - 3, cy - 10), (cx + 3, cy - 10), (cx + 7, cy - 11), (cx + 8, cy - 12), (cx + 9, cy - 13)} if not hurt else set(), hexc('#1a2050'))
    part(cv, tri((cx + tilt * .5 - 2, cy - 4), (cx + tilt * .5 + 3, cy - 4), (cx + tilt * .5 + .5, cy + 1)), ORANGE, hi=False, k=1)
    # talons
    if not dive:
        for sx in (-4, 4): cv.put({(cx + sx + k, cy + 17 + (k == 0)) for k in range(-1, 2)}, ORANGE['base'])
    return cv

def owl_frames():
    F = {}
    F['fly'] = [draw_owl(wing=S(i / 8 * TAU), dy=-round(1.6 * S(i / 8 * TAU - 1.0)), tilt=round(S(i / 8 * TAU - 2.0) * 1.2)) for i in range(8)]
    F['dive'] = [draw_owl(dive=.4, eyes='angry', dy=-2), draw_owl(dive=.8, eyes='angry', dy=0), draw_owl(dive=1.0, eyes='angry', dy=2)]
    F['hurt'] = [draw_owl(wing=.8, hurt=True, tilt=-3, dy=1), draw_owl(wing=-.4, hurt=True, tilt=-4, dy=2)]
    return pad_family(F, 4, 2, 4, 6)
OWL_ANIMS = dict(fly=(14, True), dive=(12, False), hurt=(10, False))

# ====================================================================== BOREAL JELLYFISH
JW, JH = 52, 72

BANDS = [hexc('#3ab8a0'), hexc('#3a86c8'), hexc('#5a62c0'), hexc('#8a4ab8')]

def aurora_shade(x, y, y0, y1, a=None, b=None):
    """Four colour bands (teal > sky > periwinkle > violet), blended with a Bayer dither zone at each edge."""
    t = (y - y0) / max(1.0, (y1 - y0))
    pos = max(0.0, min(0.999, t)) * 3.0
    i = int(pos); f = pos - i
    return BANDS[i + 1] if dither(x, y, max(0.0, (f - 0.55) / 0.45)) else BANDS[i]

def draw_jelly(pulse=0.0, wave=0.0, flare=0.0, hurt=False, dy=0):
    cv = Canvas(JW, JH)
    cx = 26
    r_x = 17 + pulse * 2.4 + flare * 3
    r_y = 13 - pulse * 2.2 + flare * 1
    cy = 22 + dy
    for k in range(5):
        x0 = cx - 11 + k * 5.5
        pts = []
        for j in range(14):
            t = j / 13.0
            pts.append((x0 + math.sin(wave * TAU + k * 1.1 + t * 3.0) * (1.2 + 3.4 * t) * (1.0 + flare * .5), cy + 8 + t * (30 + 5 * (k % 2))))
        m = set()
        for i in range(len(pts) - 1):
            r = 1.6 - 0.7 * i / 13.0
            m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], max(.7, r))
        cv.put(outline_of(m), hexc('#2a2a8a'))
        for (x, y) in m: cv.put([(x, y)], aurora_shade(x, y, cy + 6, cy + 44))
    bell = ell(cx, cy, r_x, r_y)
    bell |= {(x, y) for x in range(int(cx - r_x), int(cx + r_x) + 1) for y in range(int(cy), int(cy + 4)) if abs(x + .5 - cx) < r_x - 1 + (1 if (x % 4 < 2) else 0)}
    cv.put(outline_of(bell), hexc('#2a2a8a'))
    y0, y1 = cy - r_y, cy + 4
    for (x, y) in bell:
        cv.put([(x, y)], aurora_shade(x, y, y0, y1) if not hurt else (hexc('#ffffff') if (x + y) % 3 else hexc('#ffd0f0')))
    cv.put({p for p in bell if (p[0] + 2, p[1] + 2) not in bell}, hexc('#7a50c8'))
    cv.put({p for p in bell if (p[0] - 1, p[1] - 1) not in bell and p[1] < cy - 3 and p[0] < cx}, hexc('#e4fff8'))
    core = ell(cx, cy - 1, 5 + flare * 2, 4.2 + flare)
    cv.put(core, hexc('#fff6b0')); cv.put(ell(cx, cy - 1, 2.6 + flare, 2.2), WHITE)
    if not hurt:
        for ex in (cx - 9, cx + 7): cv.put({(ex + i, int(cy) + 3 + (1 if i in (0, 3) else 0)) for i in range(4)}, hexc('#2a2a8a'))
    else:
        for ex in (cx - 9, cx + 7): cv.put({(ex, int(cy) + 2), (ex + 1, int(cy) + 3), (ex + 2, int(cy) + 4), (ex + 2, int(cy) + 2), (ex, int(cy) + 4)}, hexc('#2a2a8a'))
    return cv

def jelly_frames():
    F = {}
    F['float'] = [draw_jelly(pulse=S(i / 8 * TAU), wave=i / 8.0, dy=-round(S(i / 8 * TAU - .6))) for i in range(8)]
    F['flare'] = [draw_jelly(pulse=-.6, wave=.05, flare=0), draw_jelly(pulse=.6, wave=.2, flare=.6), draw_jelly(pulse=1, wave=.3, flare=1.0), draw_jelly(pulse=.8, wave=.4, flare=.8), draw_jelly(pulse=.2, wave=.5, flare=.3)]
    F['hurt'] = [draw_jelly(pulse=-.9, hurt=True, wave=.2, dy=1), draw_jelly(pulse=-1, hurt=True, wave=.35, dy=2)]
    return F
JELLY_ANIMS = {'float': (10, True), 'flare': (14, False), 'hurt': (10, False)}

# ====================================================================== IGLOO CRAB
CW, CH = 64, 48
CG = 45

def draw_crab(walk=0.0, tuck=0.0, dy=0, claw=0.0, eyes='open', hurt=False, shake=0):
    cv = Canvas(CW, CH)
    cx = 32 + shake
    base_y = CG - 4 - dy
    # legs (3 per side), scuttling with alternating phase
    for side in (-1, 1):
        for k in range(3):
            ph = walk * TAU + k * 1.2 + (0 if side < 0 else math.pi)
            lift = max(0.0, S(ph)) * 3.2 * (1 - tuck)
            hx = cx + side * (12 + k * 3.2); hy = base_y - 6
            fx = cx + side * (17 + k * 4.4 + (C(ph) * 1.6) * (1 - tuck)); fy = CG - 1 - lift
            if tuck > 0.6: fx = cx + side * (13 + k * 2); fy = CG - 1
            part(cv, line(hx, hy, hx + side * 2, hy + 4, 1.3), CORALR, hi=False, k=1)
            part(cv, line(hx + side * 2, hy + 4, fx, fy, 1.3), CORALR, hi=False, k=1)
    # eye stalks
    for sx in (-1, 1):
        ex, ey = cx + sx * 6 + 4, base_y - 20 - (1 - tuck) * 3
        cv.put(line(cx + sx * 5, base_y - 12, ex, ey + 4, .7), CORALR['base']); cv.put(outline_of(line(cx + sx * 5, base_y - 12, ex, ey + 4, .7)), CORALR['out'])
        if tuck < .7:
            e = ell(ex, ey, 3.2, 3.2); cv.put(outline_of(e), CORALR['out']); cv.put(e, SNOW['base'])
            if hurt: cv.put({(int(ex) - 1 + i, int(ey) - 1 + i) for i in range(3)} | {(int(ex) + 1 - i, int(ey) - 1 + i) for i in range(3)}, EYE)
            else: cv.put(rect(int(ex), int(ey) - 1, int(ex) + 2, int(ey) + 2), EYE); cv.put({(int(ex), int(ey) - 1)}, WHITE)
    # claws
    for side in (-1, 1):
        ax = cx + side * (21 - tuck * 6); ay = base_y - 14 - claw * 5 * (1 - tuck) + tuck * 5
        part(cv, line(cx + side * 14, base_y - 8, ax, ay + 4, 2.1), CORALR, hi=False, k=1)
        pin = ell(ax + side * 3, ay, 5.8, 5.0)
        part(cv, pin, CORALR, hi=False)
        cv.put(poly([(ax + side * 3, ay - 1), (ax + side * 11, ay - 5 - claw * 2), (ax + side * 8, ay + 1)]), CORALR['base']); cv.put(outline_of(poly([(ax + side * 3, ay - 1), (ax + side * 11, ay - 5 - claw * 2), (ax + side * 8, ay + 1)])), CORALR['out'])
    # igloo shell: dome of ice bricks (this is its armour)
    rx, ry = 19 + tuck * 2, 17 - tuck * 4
    dome = {p for p in ell(cx, base_y, rx, ry) if p[1] <= base_y}
    dome |= rect(int(cx - rx), int(base_y), int(cx + rx) + 1, int(base_y) + 4)
    part(cv, dome, ICE)
    # brick courses
    for r in range(4):
        y = int(base_y - 4 - r * 4)
        cv.put({(x, y) for x in range(int(cx - rx), int(cx + rx) + 1) if (x, y) in dome}, ICE['shade'])
        off = (r % 2) * 4
        cv.put({(x, yy) for x in range(int(cx - rx) + off, int(cx + rx) + 1, 8) for yy in range(y - 3, y) if (x, yy) in dome}, ICE['shade'])
    cv.put({(x, y) for (x, y) in dome if (x - 1, y - 1) not in dome and y < base_y - 6 and x < cx}, WHITE)
    # doorway face: it peeks out of the entrance
    door = ell(cx + 5, base_y + 0, 6.2, 6.5) | rect(int(cx + 5 - 6), int(base_y), int(cx + 5 + 7), int(base_y) + 4)
    cv.put(door & dome, hexc('#1c2a5a'))
    if tuck < .6:
        cv.put({(int(cx) + 2 + k, int(base_y) - 2) for k in range(3)} | {(int(cx) + 8 + k, int(base_y) - 2) for k in range(3)}, hexc('#ffe36a'))
        cv.put({(int(cx) + 1, int(base_y) - 4), (int(cx) + 2, int(base_y) - 3), (int(cx) + 10, int(base_y) - 3), (int(cx) + 11, int(base_y) - 4)}, hexc('#ffe36a'))
    return cv

def crab_frames():
    F = {}
    F['idle'] = [draw_crab(walk=0, dy=round(.5 + .5 * S(i / 4 * TAU)), claw=.5 + .5 * S(i / 4 * TAU - .8)) for i in range(4)]
    F['scuttle'] = [draw_crab(walk=i / 8.0, dy=round(abs(S(i / 8 * math.pi * 2)) * 1.4), claw=.5 + .5 * S(i / 8 * TAU * 2)) for i in range(8)]
    F['charge'] = [draw_crab(tuck=.3, claw=1), draw_crab(tuck=.7, dy=0), draw_crab(tuck=1.0, shake=0), draw_crab(tuck=1.0, shake=1), draw_crab(tuck=1.0, shake=-1)]
    F['hurt'] = [draw_crab(hurt=True, shake=-2, dy=-1, claw=1), draw_crab(hurt=True, shake=-3, dy=-2, claw=.2)]
    return pad_family(F, 10, 2, 10, 4)
CRAB_ANIMS = dict(idle=(8, True), scuttle=(16, True), charge=(12, False), hurt=(10, False))

# ====================================================================== SNOW SEED (tiny, fragile)
SDW = SDH = 32

def draw_seed(dy=0, sq=0, leaf=0, eyes='open'):
    cv = Canvas(SDW, SDH)
    cx, cy = 16, 21 + dy
    # sprout
    for k in range(2):
        sx = 1 if k else -1
        part(cv, tri((cx, cy - 8 - sq), (cx + sx * (8 + leaf), cy - 15 - sq + abs(leaf)), (cx + sx * 2, cy - 7 - sq)), LEAF, hi=False, k=1)
    b = ell(cx, cy, 9 + sq * .6, 8.4 - sq * .6)
    part(cv, b, SNOW)
    if eyes == 'open':
        for ex in (cx - 4, cx + 2): cv.put(rect(ex, cy - 2, ex + 2, cy + 1), EYE); cv.put({(ex, cy - 2)}, WHITE)
        cv.put({(cx - 6, cy - 4), (cx - 5, cy - 3), (cx + 4, cy - 4), (cx + 3, cy - 3)}, hexc('#34408a'))
    else:
        for ex in (cx - 4, cx + 2): cv.put({(ex, cy - 1), (ex + 1, cy - 1)}, EYE)
    cv.put({(cx - 1, cy + 3), (cx, cy + 4), (cx + 1, cy + 4), (cx + 2, cy + 3)}, hexc('#34408a'))
    return cv

def seed_frames():
    F = {}
    F['idle'] = [draw_seed(dy=round(.5 + .5 * S(i / 4 * TAU)), leaf=round(S(i / 4 * TAU)), eyes='closed' if i == 3 else 'open') for i in range(4)]
    F['hop'] = [draw_seed(sq=2, dy=1), draw_seed(sq=-1.5, dy=-4, leaf=2), draw_seed(sq=-.5, dy=-6, leaf=1), draw_seed(sq=-1, dy=-3, leaf=-1), draw_seed(sq=2.5, dy=1, eyes='closed'), draw_seed(sq=1, dy=0)]
    F['hurt'] = [draw_seed(sq=2, dy=1, eyes='closed')]
    return F
SEED_ANIMS = dict(idle=(8, True), hop=(12, False), hurt=(10, False))

# ====================================================================== SNOWBALLS (captured creatures)
GW = GH = 48
SNOWBALL = pal('#f6fcff', '#ffffff', '#c6def4', '#96b4e0', '#34408a', '#a8c4ec')

def draw_snowball(inner_col, i=0, mode='wobble', n=8):
    cv = Canvas(GW, GH)
    cx, cy = 24, 26
    t = i / n * TAU
    sx, sy, dy = 1.0, 1.0, 0
    if mode == 'wobble': sx, sy = 1 + .05 * S(t * 2), 1 - .05 * S(t * 2)
    if mode == 'shake': sx = 1 + .07 * S(t * 4); dy = round(S(t * 3))
    r = 15
    m = ell(cx + (round(S(t * 6)) if mode == 'shake' else 0), cy + dy + (15 - r * sy), r * sx, r * sy)
    part(cv, m, SNOWBALL)
    # packed-snow texture: dither speckles on the shadow side
    for (x, y) in m:
        if (x * 7 + y * 13) % 11 == 0 and x > cx - 6: cv.put([(x, y)], SNOWBALL['shade'])
    ang = (i / n * TAU) if mode == 'roll' else 0
    inner = ell(cx, cy + dy + 1, 9, 8.5)
    cv.put(inner, mix(inner_col[:3], hexc('#e4f0ff'), .45))
    cv.put({p for p in inner if (p[0] + 2, p[1] + 2) not in inner}, mix(inner_col[:3], hexc('#8aa8d8'), .55))
    for off in (-3.2, 3.2):
        px_ = cx + C(ang) * off - S(ang) * -1; py_ = cy + dy + S(ang) * off + C(ang) * -1
        ex, ey = int(px_), int(py_)
        cv.put({(ex, ey - 1), (ex + 1, ey - 1), (ex, ey), (ex + 1, ey), (ex, ey + 1), (ex + 1, ey + 1)}, EYE); cv.put({(ex, ey - 1)}, WHITE)
        if mode != 'roll': cv.put({(ex - 1 + (0 if off < 0 else 1), ey - 3 + (1 if off < 0 else 0)), (ex + (0 if off < 0 else 1), ey - 3 + (0 if off < 0 else 1))}, EYE)
    if mode == 'roll':
        for k in range(5):
            a = ang + k * .22
            cv.put({(int(cx + C(a) * 12.5), int(cy + S(a) * 12.5))}, WHITE)
    else:
        cv.put({(cx - 9 + k, cy - 11 + (k // 2)) for k in range(4)}, WHITE)
    if mode == 'shake': cv.put({(cx - 15, cy - 11), (cx + 15, cy - 9), (cx - 16, cy + 3)}, hexc('#ffffff'))
    return cv

def draw_snowpop(i, n=7):
    cv = Canvas(GW, GH); cx, cy = 24, 26; t = i / (n - 1)
    cols = [WHITE, hexc('#e4f2ff'), hexc('#a8c8ee'), hexc('#7ad0ff')]
    if t < .25:
        r = 15 + t * 24; m = ell(cx, cy, r, r * .9)
        cv.put(outline_of(m), SNOWBALL['out']); cv.put(m, WHITE); cv.put(ell(cx, cy, r - 5, (r - 5) * .9), hexc('#e4f2ff'))
    else:
        k = (t - .25) / .75
        for j in range(12):
            a = j / 12 * TAU + .3; d = 8 + k * 20 + (j % 3) * 2; size = max(1, 4 - k * 4 + (j % 2))
            cv.put(ell(cx + C(a) * d, cy + S(a) * d * .9 + k * 5, size, size), cols[j % 4])
        if k < .5: cv.put(ring(ell(cx, cy, 8 + k * 24, 7 + k * 22)), hexc('#e4f2ff'))
    return cv

def snowball_frames(inner_col):
    F = {}
    F['wobble'] = [draw_snowball(inner_col, i, 'wobble', 6) for i in range(6)]
    F['shake'] = [draw_snowball(inner_col, i, 'shake', 6) for i in range(6)]
    F['roll'] = [draw_snowball(inner_col, i, 'roll', 8) for i in range(8)]
    F['pop'] = [draw_snowpop(i, 7) for i in range(7)]
    return F
BALL_ANIMS = dict(wobble=(8, True), shake=(14, True), roll=(18, True), pop=(18, False))
BALL_INNER = dict(penguin=hexc('#34457c'), snowman=hexc('#e8625c'), owl=hexc('#9bc0e8'), jelly=hexc('#7af8d8'), crab=hexc('#ff7a68'), seed=hexc('#5ad08a'))
