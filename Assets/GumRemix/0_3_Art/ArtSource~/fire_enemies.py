"""Creatures of the Fire Kingdom: forged automatons and living embers. They reuse the animation sets (names, frame counts, timings)
of the existing creature families by swapping the draw functions, so the enemy AI needs no new animation logic:
  bear -> Fundidor (forge golem)   bean -> Magma (lava blob)   seed -> Chispa (living spark)   owl -> Cenizo (ash bat)
  jelly -> Llama (wandering flame) cannon -> Brasero (furnace cannon)   crab -> Escarabajo (forged scarab)
plus the cooled-ash balls that hold a captured enemy."""
import math, random
from px import *
import enemies as E
import ice_enemies as IE
from fire_env import pal, halo, IRON, RUST, BRASS, SOOT, LAVA

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')
THREAD = hexc('#0e0608')
GLOWC = hexc('#ff8a24')
DARKEYE = hexc('#06070e')
CRUSTP = pal('#3a2430', '#7a5060', '#26161e', '#160a10', '#08040a', '#52303e')


def ember_glow(cv, cx, cy, r, strength=.4):
    halo(cv, cx, cy, r, hexc('#a0401a'), strength)


def visor(cv, x, y, w, col=None, hurt=False, closed=False):
    if hurt:
        cv.put({(x + 1 + i, y - 1 + i) for i in range(3)} | {(x + 3 - i, y - 1 + i) for i in range(3)}, hexc('#ffd060'))
    elif closed:
        cv.put(rect(x, y + 1, x + w, y + 2), hexc('#a04a1c'))
    else:
        cv.put(rect(x, y, x + w, y + 2), col or LAVA[1]); cv.put(rect(x, y, x + w, y + 1), LAVA[3])


# =============================================================================================== FUNDIDOR (bear)
def draw_fundidor(pal_=None, dy=0, sq=0, legs=((-5, 0), (5, 0)), arms=((-9, 4), (9, 4)), eyes='open', mouth=0, lean=0, hd=0, ear=0, sheen=0, **_):
    cv = Canvas(E.BW, E.BH)
    bx, by = 22 + lean, 32 + dy
    hx, hy = 22 + lean + (1 if lean else 0), 19 + dy + hd
    for i, (ox, lift) in enumerate(legs):
        lx = bx + ox
        part(cv, line(lx, by + 5, lx, E.BG - lift - 1, 2.8) | ell(lx + 1, E.BG - lift - 1, 4.2, 3.0), IRON, hi=False)
        cv.put({(int(lx) - 2 + k, E.BG - lift - 3) for k in range(5)}, BRASS['base'])
    body = ell(bx, by, 9.4 + sq * .8, 8.2 - sq * .8)
    # far arm (a piston with a fist)
    ax, ay = bx - 8, by - 3
    part(cv, line(ax, ay, ax + arms[0][0] * .6, ay + arms[0][1] * .7, 2.2) | ell(ax + arms[0][0] * .7, ay + arms[0][1] * .9, 3.4, 3.4), IRON, hi=False)
    part(cv, body, IRON)
    cv.put({(x, int(by) - 3) for x in range(int(bx - 9), int(bx + 10)) if (x, int(by) - 3) in body}, RUST['base'])
    cv.put({(x, int(by) + 4) for x in range(int(bx - 9), int(bx + 10)) if (x, int(by) + 4) in body}, RUST['base'])
    for rx_ in (-7, 7): cv.put({(int(bx + rx_), int(by) - 3), (int(bx + rx_), int(by) + 4)}, BRASS['hi'])
    # furnace window on the belly
    win = ell(bx + 1, by + 1, 4.4, 3.8)
    cv.put(outline_of(win), hexc('#06070e')); cv.put(win, hexc('#3a0e08'))
    fl = 2 + (sheen % 2)
    cv.put({(int(bx) - 2 + k, int(by) + 3 - (1 if k % 2 else 0)) for k in range(5)}, LAVA[0])
    cv.put({(int(bx) - 1 + k, int(by) + 2 - fl + (k % 2)) for k in range(3)}, LAVA[1]); cv.put({(int(bx), int(by) + 2 - fl)}, LAVA[3])
    ember_glow(cv, bx + 1, by + 1, 7, .35)
    # head: a riveted iron helmet with a smokestack
    stack_puff = [(hx + ear, hy - 15), (hx + ear * 2 + 2, hy - 18)]
    part(cv, rect(int(hx) - 2, int(hy) - 11, int(hx) + 3, int(hy) - 5), IRON, hi=False)
    for (px_, py_), r_ in zip(stack_puff, (2.6, 2.0)):
        cv.put({p for p in ell(px_, py_, r_, r_) if dither(p[0], p[1], .55)}, hexc('#4a3a40'))
    head = ell(hx, hy, 9.0, 7.6) | rect(int(hx) - 9, int(hy), int(hx) + 9, int(hy) + 7)
    part(cv, head, IRON)
    cv.put({(int(hx) - 7 + k, int(hy) - 5) for k in range(3)}, IRON['hi'])
    visor(cv, int(hx) - 6, int(hy) - 2, 12, hurt=(eyes == 'hurt'), closed=(eyes == 'closed'))
    if eyes == 'hurt': cv.put(rect(int(hx) - 6, int(hy) - 2, int(hx) + 6, int(hy)), hexc('#2a1a1e')); visor(cv, int(hx) - 1, int(hy) - 2, 3, hurt=True)
    cv.put({(int(hx) - 4 + k * 2, int(hy) + 3) for k in range(5)}, hexc('#06070e'))                   # grille mouth
    if mouth: cv.put({(int(hx) - 4 + k * 2, int(hy) + 4) for k in range(5)}, LAVA[0] if mouth == 1 else LAVA[2])
    # near arm
    ax, ay = bx + 8, by - 3
    part(cv, line(ax, ay, ax + arms[1][0] * .6, ay + arms[1][1] * .7, 2.2) | ell(ax + arms[1][0] * .7, ay + arms[1][1] * .9, 3.6, 3.6), IRON, hi=False)
    cv.put({(int(ax + arms[1][0] * .7) + k - 1, int(ay + arms[1][1] * .9) - 2) for k in range(3)}, IRON['hi'])
    return cv


# =============================================================================================== MAGMA (bean)
def draw_magma(pal_=None, sx=1.0, sy=1.0, dy=0, tilt=0, eyes='open', mouth='smile', speck=0, **_):
    cv = Canvas(E.JW, E.JH)
    cx, cy = 20, E.JG - 9 * sy + dy
    ln = 5.5 * sy
    body = line(cx - tilt - 1, cy + ln * .5, cx + tilt + 1, cy - ln * .5, 8.4 * sx) | ell(cx, cy, 8.4 * sx, 9.5 * sy)
    cv.put(outline_of(body), hexc('#1c0a08'))
    for (x, y) in body:                                           # hot at the bottom, a darker skin on top
        t = (y - (cy - 9.5 * sy)) / (19.0 * sy)
        if t < .28: c = hexc('#4a1a10')
        elif t < .5: c = LAVA[0] if dither(x, y, (t - .28) / .22) else hexc('#4a1a10')
        elif t < .8: c = LAVA[1] if dither(x, y, (t - .5) / .3) else LAVA[0]
        else: c = LAVA[2] if dither(x, y, (t - .8) / .2 * .8) else LAVA[1]
        cv.put([(x, y)], c)
    cv.put({p for p in body if (p[0] - 1, p[1] - 1) not in body and p[1] < cy}, hexc('#6a2a1c'))        # cooled crust highlight
    for (px_, py_) in ((-3, -6), (3, -4), (0, -7)): cv.put({(int(cx + px_ * sx), int(cy + py_ * sy)), (int(cx + px_ * sx) + 1, int(cy + py_ * sy) + 1)}, hexc('#241008'))   # crust plates
    for (sx_, sy_) in ((-4, 5), (4, 3), (-5, 0), (2, 7)):
        if (sx_ + sy_ + speck) % 3 != 0: cv.put({(int(cx + sx_ * sx), int(cy + sy_ * sy))}, LAVA[3])               # rising bubbles
    ey = int(cy - 2 * sy)
    for ex in (int(cx - 4), int(cx + 2)):
        if eyes == 'open': cv.put(rect(ex, ey, ex + 3, ey + 4), hexc('#1c0a08')); cv.put({(ex + 1, ey + 2)}, LAVA[3])
        elif eyes == 'closed': cv.put(rect(ex, ey + 2, ex + 3, ey + 3), hexc('#1c0a08'))
        elif eyes == 'hurt': cv.put({(ex, ey), (ex + 2, ey), (ex + 1, ey + 1), (ex, ey + 2), (ex + 2, ey + 2)}, hexc('#1c0a08'))
    my = ey + 6
    if mouth == 'smile': cv.put({(cx - 3 + k, my + (1 if k % 2 else 0)) for k in range(7)}, hexc('#1c0a08'))
    elif mouth == 'open': cv.put(rect(cx - 2, my - 1, cx + 4, my + 3), hexc('#1c0a08')); cv.put({(cx - 2 + k, my - 1) for k in range(0, 6, 2)}, LAVA[3])
    elif mouth == 'o': cv.put(rect(cx, my, cx + 3, my + 3), hexc('#1c0a08'))
    for k in range(2):                                            # drips
        dx = int(cx - 3 + k * 7); cv.put({(dx, int(cy + 9.5 * sy) + j) for j in range(2 + (speck + k) % 2)}, LAVA[1])
    ember_glow(cv, cx, cy + 2, 14, .2)
    return cv


# =============================================================================================== CHISPA (seed)
def draw_chispa(dy=0, sq=0, leaf=0, eyes='open', **_):
    cv = Canvas(IE.SDW, IE.SDH)
    cx, cy = 16, 21 + dy
    tip = poly([(cx - 5, cy - 4 - sq), (cx + leaf * 1.5, cy - 17 - sq + abs(leaf)), (cx + 5, cy - 4 - sq)])
    b = ell(cx, cy, 8.4 + sq * .6, 8.0 - sq * .6) | tip
    cv.put(outline_of(b), hexc('#2a0e08'))
    for (x, y) in b:
        t = (y - (cy - 17)) / 25.0
        cv.put([(x, y)], LAVA[3] if t > .55 and dither(x, y, (t - .55) * 1.6) else (LAVA[2] if t > .35 else (LAVA[1] if t > .15 else LAVA[0])))
    if eyes == 'open':
        for ex in (cx - 4, cx + 2): cv.put(rect(ex, cy - 1, ex + 2, cy + 2), hexc('#2a0e08')); cv.put({(ex, cy - 1)}, WHITE)
    else:
        for ex in (cx - 4, cx + 2): cv.put({(ex, cy), (ex + 1, cy)}, hexc('#2a0e08'))
    cv.put({(cx - 1, cy + 4), (cx, cy + 5), (cx + 1, cy + 4)}, hexc('#2a0e08'))
    for (dx, dyy) in ((-11, -8), (11, -2), (9, -12), (-10, 2)):
        cv.put({(cx + dx, cy + dyy + (1 if dy else 0))}, LAVA[2])
    return cv


# =============================================================================================== CENIZO (owl): a robotic ash bat
def draw_cenizo(wing=0.0, dy=0, tilt=0, eyes='open', dive=0.0, hurt=False, **_):
    cv = Canvas(IE.OW, IE.OH)
    cx, cy = 30, 30 + dy
    for side, near in ((-1, False), (1, True)):
        ang = -1.15 + (wing + 1) * 0.5 * 1.8
        if dive: ang = -1.25 - dive * 0.1
        rx_, ry_ = cx + side * 8, cy - 3 + dive * 3
        L = 21 - dive * 7
        tipx, tipy = rx_ + side * math.cos(ang) * L, ry_ + math.sin(ang) * L
        low = (rx_ + side * 4, cy + 9)
        mem = poly([(rx_, ry_), (tipx, tipy), (tipx - side * 4, tipy + 8 + dive * 4), ((tipx + low[0]) / 2, (tipy + low[1]) / 2 + 6), low])
        part(cv, mem, pal('#5a1c14' if not near else '#7a2a1a', '#d8602a', '#3a1008', '#220806', '#0c0404'), hi=False, k=2)
        for k in range(3):                                            # ember veins running along the wing
            f = (k + 1) / 4.0
            cv.put(line(rx_, ry_, tipx * f + low[0] * (1 - f), tipy * f + low[1] * (1 - f), .5), LAVA[1] if near else LAVA[0])
        cv.put(line(rx_, ry_, tipx, tipy, 1.1), IRON['base'])           # the iron spar
    body = ell(cx + tilt * .3, cy + 3, 8.6, 11.4)
    part(cv, body, IRON)
    cv.put(ell(cx + tilt * .3, cy + 4, 4.6, 5.4), hexc('#3a0e08')); cv.put(ell(cx + tilt * .3, cy + 5, 3, 3.4), LAVA[1]); cv.put({(cx + int(tilt * .3), cy + 4)}, LAVA[3])
    for sx in (-1, 1): part(cv, tri((cx + sx * 6, cy - 10), (cx + sx * 9, cy - 20 + dive * 3), (cx + sx * 2, cy - 12)), IRON, hi=False, k=1)
    head = ell(cx + tilt * .5, cy - 7, 9.4, 8.2)
    part(cv, head, IRON)
    for ex in (cx - 4, cx + 4):
        if hurt: cv.put({(int(ex) - 2 + i + tilt // 2, cy - 9 + i) for i in range(4)} | {(int(ex) + 1 - i + tilt // 2, cy - 9 + i) for i in range(4)}, LAVA[2])
        else:
            cv.put(rect(int(ex) - 1 + tilt // 2, cy - 9, int(ex) + 3 + tilt // 2, cy - 6), hexc('#06070e')); cv.put(rect(int(ex) + tilt // 2, cy - 8, int(ex) + 2 + tilt // 2, cy - 7), LAVA[3] if eyes == 'open' else LAVA[1])
    cv.put({(cx - 1, cy - 3), (cx, cy - 2), (cx + 1, cy - 3)}, hexc('#06070e'))
    ember_glow(cv, cx, cy + 4, 14, .22)
    return cv


# =============================================================================================== LLAMA (jelly): a wandering flame
def draw_llama(pulse=0.0, wave=0.0, flare=0.0, hurt=False, dy=0, **_):
    cv = Canvas(IE.JW, IE.JH)
    cx = 26
    r_x = 15 + pulse * 2.2 + flare * 3
    r_y = 14 - pulse * 2.0 + flare
    cy = 24 + dy
    for k in range(5):                                                # flame tendrils instead of tentacles
        x0 = cx - 11 + k * 5.5
        pts = []
        for j in range(14):
            t = j / 13.0
            pts.append((x0 + math.sin(wave * TAU + k * 1.1 + t * 3.0) * (1.2 + 3.4 * t) * (1.0 + flare * .5), cy + 8 + t * (28 + 6 * (k % 2))))
        m = set()
        for i in range(len(pts) - 1):
            m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], max(.7, 2.0 - 1.1 * i / 13.0))
        cv.put(outline_of(m), hexc('#2a0e08'))
        for (x, y) in m:
            t = (y - cy - 8) / 36.0
            cv.put([(x, y)], LAVA[1] if t < .35 else (LAVA[0] if dither(x, y, (t - .35) * 1.4) else LAVA[1]))
    bell = ell(cx, cy, r_x, r_y)
    for k_, kx in enumerate((-8, 0, 8)):                                  # three licking tongues of flame crown the head
        fl = 3.0 * S(wave * TAU + k_ * 2.1)
        tip_y = cy - r_y - (7 + (6 if k_ == 1 else 0)) - pulse * 2 - flare * 3 - abs(fl) * .6
        bell |= poly([(cx + kx - 5, cy - r_y * .55), (cx + kx + fl, tip_y), (cx + kx + 5, cy - r_y * .55)])
    cv.put(outline_of(bell), hexc('#2a0e08'))
    y0, y1 = cy - r_y - 14, cy + 6
    for (x, y) in bell:
        t = (y - y0) / (y1 - y0)
        if hurt: cv.put([(x, y)], WHITE if (x + y) % 3 else hexc('#ffd0a0'))
        else: cv.put([(x, y)], LAVA[0] if t < .35 else (LAVA[1] if t < .6 else (LAVA[2] if dither(x, y, (t - .6) * 2) else LAVA[1])))
    # hollow face
    for ex in (cx - 6, cx + 3):
        if hurt: cv.put({(ex + i, cy - 2 + i) for i in range(4)} | {(ex + 3 - i, cy - 2 + i) for i in range(4)}, hexc('#2a0e08'))
        else: cv.put(rect(ex, cy - 2, ex + 4, cy + 3), hexc('#2a0e08')); cv.put(rect(ex + 1, cy - 1, ex + 3, cy), LAVA[3])
    cv.put({(cx - 3 + k, cy + 6 + (1 if k % 2 else 0)) for k in range(7)}, hexc('#2a0e08'))
    ember_glow(cv, cx, cy, r_x + 6, .22)
    return cv


# =============================================================================================== BRASERO (cannon)
def draw_brasero(recoil=0, bob=0, flash=0, hurt=False, wheel=0, **_):
    cv = Canvas(E.CW, E.CH)
    for wx in (14, 36):                                                # cast wheels
        w = ell(wx, 41, 6, 6); part(cv, w, IRON, hi=False)
        for k in range(4):
            a = wheel * .5 + k * math.pi / 2
            cv.put(line(wx, 41, wx + math.cos(a) * 4, 41 + math.sin(a) * 4, .5), IRON['hi'])
        cv.put(ell(wx, 41, 1.5, 1.5), BRASS['base'])
    ang = math.radians(-48)
    bx0, by0 = 24, 24 + bob
    L = 20 - recoil
    ex, ey = bx0 + math.cos(ang) * L, by0 + math.sin(ang) * L
    tube = line(bx0, by0, ex, ey, 6.2)
    cv.put(outline_of(tube), IRON['out'])
    for (x, y) in tube:
        d = ((x - bx0) * math.cos(ang) + (y - by0) * math.sin(ang))
        cv.put([(x, y)], IRON['base'] if int((d + 40) // 5) % 2 == 0 else IRON['shade'])
    cv.put({p for p in tube if (p[0] + 2, p[1] + 2) not in tube}, IRON['deep'])
    for k in range(2):                                                 # brass bands
        d = 6 + k * 9
        bxk, byk = bx0 + math.cos(ang) * d, by0 + math.sin(ang) * d
        cv.put({p for p in tube if abs((p[0] - bxk) * math.cos(ang) + (p[1] - byk) * math.sin(ang)) < 1.1}, BRASS['base'])
    rim = ell(ex, ey, 6.6, 6.6)
    cv.put(outline_of(rim), IRON['out']); cv.put(rim, IRON['deep']); cv.put(ell(ex + 1, ey + 1, 4, 4), hexc('#1c0a08')); cv.put(ell(ex + 1, ey + 1, 2.4, 2.4), LAVA[0])
    if flash:
        cv.put(ell(ex + 3, ey - 3, 4 + flash, 4 + flash), LAVA[2]); cv.put(ell(ex + 3, ey - 3, 2 + flash, 2 + flash), WHITE)
    base = rect(8, 24 + bob, 42, 40) | rect(10, 22 + bob, 40, 24 + bob)
    part(cv, base, IRON)
    for x in range(12, 40, 6): cv.put({(x, 25 + bob), (x, 26 + bob)}, BRASS['base'])
    for ex_ in (15, 27):
        if hurt: cv.put({(ex_ + 1, 28 + bob), (ex_ + 5, 28 + bob), (ex_ + 3, 30 + bob), (ex_ + 1, 32 + bob), (ex_ + 5, 32 + bob)}, hexc('#ffd060'))
        else: cv.put(rect(ex_, 28 + bob, ex_ + 7, 33 + bob), hexc('#06070e')); cv.put(rect(ex_ + 1, 30 + bob, ex_ + 6, 32 + bob), LAVA[1]); cv.put(rect(ex_ + 1, 30 + bob, ex_ + 6, 31 + bob), LAVA[3])
    cv.put(rect(15, 36 + bob, 36, 38 + bob), hexc('#06070e'))
    cv.put({(x, 36 + bob) for x in range(16, 36, 3)} | {(x, 37 + bob) for x in range(16, 36, 3)}, LAVA[0])            # a furnace grille mouth
    ember_glow(cv, 26, 30 + bob, 20, .18)
    return cv


# =============================================================================================== ESCARABAJO (crab): a forged scarab with a shell it tucks under
def draw_escarabajo(walk=0.0, tuck=0.0, dy=0, claw=0.0, eyes='open', hurt=False, shake=0, **_):
    cv = Canvas(IE.CW, IE.CH)
    cx = 32 + shake
    base_y = IE.CG - 4 - dy
    for side in (-1, 1):                                               # six legs
        for k in range(3):
            ph = walk * TAU + k * 1.2 + (0 if side < 0 else math.pi)
            lift = max(0.0, S(ph)) * 3.2 * (1 - tuck)
            hx = cx + side * (12 + k * 3.2); hy = base_y - 6
            fx = cx + side * (17 + k * 4.4 + (C(ph) * 1.6) * (1 - tuck)); fy = IE.CG - 1 - lift
            if tuck > 0.6: fx = cx + side * (13 + k * 2); fy = IE.CG - 1
            part(cv, line(hx, hy, hx + side * 2, hy + 4, 1.3), IRON, hi=False, k=1)
            part(cv, line(hx + side * 2, hy + 4, fx, fy, 1.3), IRON, hi=False, k=1)
    for side in (-1, 1):                                               # pincers
        ax = cx + side * (21 - tuck * 6); ay = base_y - 14 - claw * 5 * (1 - tuck) + tuck * 5
        part(cv, line(cx + side * 14, base_y - 8, ax, ay + 4, 2.1), IRON, hi=False, k=1)
        pin = ell(ax + side * 3, ay, 5.4, 4.8)
        part(cv, pin, IRON, hi=False)
        claw_poly = poly([(ax + side * 3, ay - 1), (ax + side * 11, ay - 5 - claw * 2), (ax + side * 8, ay + 1)])
        cv.put(outline_of(claw_poly), IRON['out']); cv.put(claw_poly, BRASS['base'])
    rx, ry = 19 + tuck * 2, 17 - tuck * 4
    dome = {p for p in ell(cx, base_y, rx, ry) if p[1] <= base_y}
    dome |= rect(int(cx - rx), int(base_y), int(cx + rx) + 1, int(base_y) + 4)
    part(cv, dome, IRON)
    cv.put({(int(cx), y) for y in range(int(base_y - ry), int(base_y) + 4) if (int(cx), y) in dome}, hexc('#06070e'))        # split between the two wing-cases
    for y in range(int(base_y - ry) + 2, int(base_y) + 3, 2):
        if (int(cx) + 1, y) in dome: cv.put({(int(cx) + 1, y)}, LAVA[1])                                                     # ember glowing through the seam
    for r in range(3):
        y = int(base_y - 5 - r * 5)
        cv.put({(x, y) for x in range(int(cx - rx), int(cx + rx) + 1) if (x, y) in dome and abs(x - cx) > 2}, IRON['shade'])
    cv.put({(x, y) for (x, y) in dome if (x - 1, y - 1) not in dome and y < base_y - 6 and x < cx}, IRON['hi'])
    for rv in (-10, 10): cv.put({(int(cx + rv), int(base_y) - 4)}, BRASS['hi'])
    door = ell(cx + 5, base_y + 0, 6.2, 6.5) | rect(int(cx + 5 - 6), int(base_y), int(cx + 5 + 7), int(base_y) + 4)
    cv.put(door & dome, hexc('#1c0a08'))
    if tuck < .6:
        cv.put({(int(cx) + 2 + k, int(base_y) - 2) for k in range(3)} | {(int(cx) + 8 + k, int(base_y) - 2) for k in range(3)}, LAVA[3])
        cv.put({(int(cx) + 1, int(base_y) - 4), (int(cx) + 2, int(base_y) - 3), (int(cx) + 10, int(base_y) - 3), (int(cx) + 11, int(base_y) - 4)}, LAVA[1])
    return cv


# =============================================================================================== frames: swap the draw functions in, call the original frame builders
def _with(patches, fn):
    saved = [(mod, name, getattr(mod, name)) for mod, name, _ in patches]
    for mod, name, new in patches: setattr(mod, name, new)
    try: return fn()
    finally:
        for mod, name, old in saved: setattr(mod, name, old)


def fundidor_frames(): return _with([(E, 'draw_bear', draw_fundidor)], lambda: E.bear_frames(None))
def magma_frames(): return _with([(E, 'draw_bean', draw_magma)], lambda: E.bean_frames(None))
def chispa_frames(): return _with([(IE, 'draw_seed', draw_chispa)], IE.seed_frames)
def cenizo_frames(): return _with([(IE, 'draw_owl', draw_cenizo)], IE.owl_frames)
def llama_frames(): return _with([(IE, 'draw_jelly', draw_llama)], IE.jelly_frames)
def brasero_frames(): return _with([(E, 'draw_cannon', draw_brasero)], E.cannon_frames)
def escarabajo_frames(): return _with([(IE, 'draw_crab', draw_escarabajo)], IE.crab_frames)


# =============================================================================================== cooled-ash balls that hold a captured enemy
GW = GH = 48


def draw_ashball(inner_col, i=0, mode='wobble', n=8):
    cv = Canvas(GW, GH)
    cx, cy = 24, 26
    t = i / n * TAU
    sx, sy, dy = 1.0, 1.0, 0
    if mode == 'wobble': sx, sy = 1 + .05 * S(t * 2), 1 - .05 * S(t * 2)
    if mode == 'shake': sx = 1 + .07 * S(t * 4); dy = round(S(t * 3))
    r = 15
    m = ell(cx + (round(S(t * 6)) if mode == 'shake' else 0), cy + dy + (15 - r * sy), r * sx, r * sy)
    part(cv, m, CRUSTP)
    for (x, y) in m:
        if (x * 7 + y * 13) % 11 == 0 and x > cx - 6: cv.put([(x, y)], CRUSTP['deep'])
    glow = .5 + .5 * S(t * (3 if mode == 'shake' else 1.5))
    rng = random.Random(3)
    for k in range(5):                                       # glowing cracks in the ash crust (the heat of whoever is inside)
        a = rng.random() * TAU + (i / n * TAU if mode == 'roll' else 0)
        x, y = cx + C(a) * 6, cy + dy + S(a) * 6
        for s_ in range(5):
            a += rng.uniform(-.6, .6); x += C(a) * 2; y += S(a) * 2
            if (int(x), int(y)) in m: cv.put([(int(x), int(y))], LAVA[1] if s_ % 2 or glow > .6 else LAVA[0])
    inner = ell(cx, cy + dy + 1, 8, 7.5)
    cv.put(inner, mix(inner_col[:3], (255, 160, 60), .35 + .25 * glow))
    cv.put({p for p in inner if (p[0] + 2, p[1] + 2) not in inner}, mix(inner_col[:3], (80, 20, 10), .55))
    ang = (i / n * TAU) if mode == 'roll' else 0
    for off in (-3.2, 3.2):
        px_ = cx + C(ang) * off - S(ang) * -1; py_ = cy + dy + S(ang) * off + C(ang) * -1
        ex, ey = int(px_), int(py_)
        cv.put({(ex, ey - 1), (ex + 1, ey - 1), (ex, ey), (ex + 1, ey), (ex, ey + 1), (ex + 1, ey + 1)}, hexc('#1c0a08')); cv.put({(ex, ey - 1)}, LAVA[3])
    if mode == 'roll':
        for k in range(5):
            a = ang + k * .22
            cv.put({(int(cx + C(a) * 12.5), int(cy + S(a) * 12.5))}, LAVA[2])
    else:
        cv.put({(cx - 9 + k, cy - 11 + (k // 2)) for k in range(4)}, hexc('#a07080'))
    ember_glow(cv, cx, cy, 20, .12 * glow)
    return cv


def draw_ashpop(i, n=7):
    cv = Canvas(GW, GH); cx, cy = 24, 26; t = i / (n - 1)
    cols = [LAVA[3], LAVA[2], LAVA[1], hexc('#5a3a44')]
    if t < .25:
        r = 15 + t * 24; m = ell(cx, cy, r, r * .9)
        cv.put(outline_of(m), CRUSTP['out']); cv.put(m, LAVA[2]); cv.put(ell(cx, cy, r - 5, (r - 5) * .9), LAVA[3])
    else:
        k = (t - .25) / .75
        for j in range(12):
            a = j / 12 * TAU + .3; d = 8 + k * 20 + (j % 3) * 2; size = max(1, 4 - k * 4 + (j % 2))
            cv.put(ell(cx + C(a) * d, cy + S(a) * d * .9 + k * 5, size, size), cols[j % 4])
        if k < .5: cv.put(ring(ell(cx, cy, 8 + k * 24, 7 + k * 22)), LAVA[2])
    return cv


def ashball_frames(inner_col):
    F = {}
    F['wobble'] = [draw_ashball(inner_col, i, 'wobble', 6) for i in range(6)]
    F['shake'] = [draw_ashball(inner_col, i, 'shake', 6) for i in range(6)]
    F['roll'] = [draw_ashball(inner_col, i, 'roll', 8) for i in range(8)]
    F['pop'] = [draw_ashpop(i, 7) for i in range(7)]
    return F


BALL_INNER = dict(bear=hexc('#7a8aa8'), bean=hexc('#ff6a2a'), seed=hexc('#ffc850'), owl=hexc('#8a3a2a'), jelly=hexc('#ff9a3a'), cannon=hexc('#6a6a80'), crab=hexc('#5a5a70'))
