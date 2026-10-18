"""Animated scenery: everything here moves. Each function returns a list of Canvas frames (loops seamlessly)."""
import math, random
from px import *

S, C, TAU = math.sin, math.cos, 2 * math.pi
WHITE = hexc('#ffffff')

def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

CREAM = pal('#fff0d6', '#ffffff', '#f4cfa6', '#d9a47e', '#4a2038', '#e8c8a0')
PINK = pal('#ff79b8', '#ffd0ec', '#e2489a', '#a62e7c', '#43103f', '#c0407e')
MINT = pal('#6fe8b8', '#c8ffe8', '#3cb890', '#2a7a78', '#103a48', '#2a9a80')
GREEN = pal('#58d070', '#b8ffb8', '#2f9a58', '#20686a', '#0e2c40')
YELLOW = pal('#ffd23a', '#fff4a0', '#e89a2a', '#b05a30', '#4a1a28')
PURPLE = pal('#b070f0', '#e0c0ff', '#7a48d0', '#4a2a98', '#1e1050')
ORANGE = pal('#ff9a3a', '#ffd890', '#e0662a', '#a03c30', '#401428')
BLUE = pal('#4aa9f4', '#b6f0ff', '#3566d8', '#2b3fa6', '#161650')
RED = pal('#ee4650', '#ffa090', '#c02a4a', '#862050', '#3a0e34')

def spiral_mask(cv, cx, cy, r, phase, ca, cb, arms=2, turns=4.2):
    m = ell(cx, cy, r, r)
    for (x, y) in m:
        dx, dy = x + .5 - cx, y + .5 - cy
        s = (math.atan2(dy, dx) / TAU * arms + math.hypot(dx, dy) / turns + phase) % 1.0
        cv.put([(x, y)], ca if s < .5 else cb)
    return m

# ----------------------------------------------------------------------------------------- THEME 0
def lolli_tree(n=8, scheme=0):
    out = []
    colors = [(hexc('#ff6aa8'), hexc('#fff0f6'), '#c0407a'), (hexc('#58d0ff'), hexc('#f0fcff'), '#2a88c0'), (hexc('#ffb83a'), hexc('#fff6d8'), '#d0782a')][scheme]
    for i in range(n):
        cv = Canvas(64, 120)
        t = i / n * TAU
        sway = 3.4 * S(t)
        def tx(y): return 32 + sway * ((112 - y) / 70.0) ** 2
        # trunk (cane): stripes follow the bend
        pts = [(tx(y), y) for y in range(112, 44, -2)]
        m = set()
        for (x, y) in pts: m |= ell(x, y, 2.6, 2.6)
        cv.put(outline_of(m), hexc('#4a2038'))
        for (x, y) in m: cv.put([(x, y)], hexc('#fff0d6') if ((y + x // 2) // 5) % 2 == 0 else colors[0])
        cv.put({(x, y) for (x, y) in m if (x + 1, y + 1) not in m}, hexc('#c8a0a0'))
        # leaves (gumdrops) lagging behind the sway
        for ly, side in ((88, -1), (76, 1)):
            lx = tx(ly) + side * 7
            part(cv, ell(lx + sway * .3 * side, ly - 3, 5.5, 3.2), pal('#58d070', '#b8ffb8', '#2f9a58', '#20686a', '#0e2c40'), hi=False)
        # candy disc
        cx, cy = tx(44) + sway * .3, 28
        cv.put(outline_of(ell(cx, cy, 25, 25)), hexc('#4a2038'))
        msk = spiral_mask(cv, cx, cy, 25, 0, colors[0], colors[1])
        cv.put({p for p in msk if (p[0] + 2, p[1] + 2) not in msk}, hexc(colors[2]))
        cv.put({(int(cx) - 16 + k, int(cy) - 16 + k // 2) for k in range(6)}, WHITE)
        out.append(cv)
    return out

def windmill(n=8):
    out = []
    for i in range(n):
        cv = Canvas(96, 120)
        # tower
        tw = poly([(34, 118), (62, 118), (56, 46), (40, 46)])
        part(cv, tw, dict(base=hexc('#fff0d6'), hi=WHITE, shade=hexc('#f4cfa6'), deep=hexc('#d9a47e'), out=hexc('#4a2038'), out_l=hexc('#e8c8a0')))
        for y in range(52, 116, 12):
            cv.put({(x, yy) for yy in range(y, y + 6) for x in range(34, 63) if (x, yy) in tw}, hexc('#ff9ad0'))
        part(cv, rect(44, 96, 54, 118), pal('#a86840', '#c88850', '#6a3a28', '#4a2a28', '#2a1018'), hi=False)
        part(cv, poly([(36, 46), (60, 46), (48, 26)]), pal('#ff79b8', '#ffd0ec', '#e2489a', '#a62e7c', '#43103f'), hi=False)
        hub = (48, 40)
        ang = i / n * (math.pi / 2)
        for k in range(4):
            a = ang + k * math.pi / 2
            ex, ey = hub[0] + C(a) * 40, hub[1] + S(a) * 40
            blade = line(hub[0], hub[1], ex, ey, 3.2)
            cv.put(outline_of(blade), hexc('#4a2038'))
            for (x, y) in blade:
                d = math.hypot(x - hub[0], y - hub[1])
                cv.put([(x, y)], hexc('#ff6aa8') if int(d // 6) % 2 == 0 else hexc('#fff0f6'))
            # sail
            sl = poly([(hub[0] + C(a) * 10 - S(a) * 3, hub[1] + S(a) * 10 + C(a) * 3), (hub[0] + C(a) * 38 - S(a) * 3, hub[1] + S(a) * 38 + C(a) * 3),
                       (hub[0] + C(a) * 38 - S(a) * 13, hub[1] + S(a) * 38 + C(a) * 13), (hub[0] + C(a) * 12 - S(a) * 11, hub[1] + S(a) * 12 + C(a) * 11)])
            cv.put(sl - blade, hexc('#ffe0f0')); cv.put(outline_of(sl) - blade, hexc('#c0407a'))
        part(cv, ell(hub[0], hub[1], 5, 5), pal('#ffd23a', '#fff4a0', '#e89a2a', '#b05a30', '#4a1a28'), hi=False)
        out.append(cv)
    return out

def gumdrop_flower(n=6, scheme=0):
    out = []
    P = [PINK, PURPLE, ORANGE][scheme]
    for i in range(n):
        cv = Canvas(32, 40); t = i / n * TAU; sw = 2.2 * S(t + scheme)
        for y in range(38, 14, -1):
            x = 16 + sw * ((38 - y) / 24) ** 2
            cv.put([(int(x), y), (int(x) + 1, y)], hexc('#2f9a58'))
        hx, hy = 16 + sw, 13
        part(cv, ell(hx - 7, hy + 9, 5, 2.4), GREEN, hi=False); part(cv, ell(hx + 8 + sw * .3, hy + 14, 5, 2.4), GREEN, hi=False)
        head = ell(hx, hy, 8, 8) | rect(int(hx - 8), int(hy), int(hx + 8), int(hy + 5))
        part(cv, head, P)
        cv.put({(int(hx) - 4, int(hy) - 4), (int(hx) - 3, int(hy) - 5), (int(hx) - 3, int(hy) - 4)}, WHITE)
        for sx_, sy_ in ((-3, 2), (3, 0), (1, 4)): cv.put({(int(hx) + sx_, int(hy) + sy_), (int(hx) + sx_ + 1, int(hy) + sy_)}, hexc('#fff0f6'))
        out.append(cv)
    return out

def cloud(w, h, rng_seed, P):
    rng = random.Random(rng_seed)
    cv = Canvas(w, h)
    m = rect(8, h // 2, w - 8, h - 6)
    for _ in range(7):
        r = rng.randrange(h // 5, h // 2 - 2)
        m |= ell(rng.randrange(r + 4, w - r - 4), h - 6 - r + 2 - rng.randrange(0, h // 4), r, r)
    m = {p for p in m if p[1] < h - 5 and 0 <= p[0] < w}
    part(cv, m, P, k=3)
    return cv

# ----------------------------------------------------------------------------------------- THEME 1
def crystal(n=6, scheme=0):
    out = []
    base = [pal('#52e8f0', '#d4ffff', '#2aa8d0', '#1c6aa8', '#10204a'), pal('#ff6ad0', '#ffd0f4', '#d03aa0', '#8a2478', '#2a0a40')][scheme]
    for i in range(n):
        cv = Canvas(48, 64); t = i / n * TAU
        pulse = .5 + .5 * S(t)
        # glow halo (dithered, grows and shrinks)
        for y in range(64):
            for x in range(48):
                d = math.hypot((x - 24) / 24, (y - 42) / 24)
                if d < .55 + .45 * pulse and dither(x, y, (1 - d) * .55) and d > .25: cv.put([(x, y)], mix(base['hi'][:3], hexc('#ffffff'), .0) if False else hexc('#ffb8e8') if scheme else hexc('#a0f0ff'))
        for (cx, h, wd) in ((24, 48, 8), (13, 32, 6), (35, 36, 6)):
            top = 62 - h
            sh = poly([(cx - wd, 62), (cx - wd, top + 8), (cx, top), (cx + wd, top + 8), (cx + wd, 62)])
            part(cv, sh, base)
            cv.put({(x, y) for (x, y) in sh if x == cx or x == cx - 1 and y < top + 14}, base['hi'])
            cv.put({(cx - wd + 2, top + 12 + k) for k in range(5)}, WHITE)
        if pulse > .75: cv.put({(30, 20), (29, 20), (31, 20), (30, 19), (30, 21)}, WHITE)
        out.append(cv)
    return out

def waterfall(n=8):
    out = []
    for i in range(n):
        cv = Canvas(40, 120)
        for y in range(0, 108):
            wob = int(round(1.2 * S(y * .22 + i / n * TAU)))
            x0, x1 = 6 + wob, 34 + wob
            for x in range(x0, x1):
                c = hexc('#ffb040')
                phase = (y + x * 2 + i * 2) % 16     # streaks slide down 2px/frame -> period 16 loops in 8 frames
                if phase < 3: c = hexc('#ffe090')
                elif phase < 5: c = hexc('#e88030')
                if x < x0 + 2: c = hexc('#ffe090') if (y + i * 2) % 16 < 8 else hexc('#ffb040')
                if x >= x1 - 3: c = hexc('#c86020')
                cv.put([(x, y)], c)
            cv.put([(x0 - 1, y), (x1, y)], hexc('#2a1030'))
        # splash foam at the base
        for k in range(5):
            bx = 8 + k * 6 + int(2 * S(i / n * TAU + k)); by = 106 - int(abs(S(i / n * TAU * 2 + k * 1.3)) * 5)
            cv.put(ell(bx, by, 3, 2.6), hexc('#ffe090')); cv.put({(bx - 1, by - 1)}, WHITE)
        cv.put(ell(20, 112, 18, 4), hexc('#ffb040')); cv.put(ring(ell(20, 112, 18, 4)), hexc('#c86020'))
        out.append(cv)
    return out

def caramel_pot(n=8):
    out = []
    rng = random.Random(5)
    bubbles = [(rng.randrange(14, 40), rng.random()) for _ in range(5)]
    for i in range(n):
        cv = Canvas(64, 48)
        t = i / n
        pot = ell(32, 32, 26, 14) | rect(8, 22, 56, 36)
        part(cv, pot, pal('#4a3a50', '#7a6a88', '#342a3c', '#241c2c', '#120a18'), hi=False)
        cv.put(rect(6, 18, 58, 22), hexc('#7a6a88')); cv.put(outline_of(rect(6, 18, 58, 22)), hexc('#120a18'))
        surf = ell(32, 20, 25, 4.4)
        cv.put(surf, hexc('#ffb040')); cv.put({p for p in surf if (p[0], p[1] + 1) not in surf}, hexc('#c86020'))
        cv.put({(14 + (k + i * 2) % 36, 19) for k in range(0, 36, 6)}, hexc('#ffe090'))
        for (bx, ph) in bubbles:
            k = (t + ph) % 1.0
            r = 1.5 + 3 * k
            by = 20 - k * 8
            if k < .85:
                b = ell(bx, by, r, r * .8); cv.put(outline_of(b), hexc('#c86020')); cv.put(b, hexc('#ffd070')); cv.put({(int(bx) - 1, int(by) - 1)}, WHITE)
            else:
                for a in range(5): cv.put({(int(bx + C(a * 1.26) * 5 * (k - .8) * 10), int(by - 4 + S(a * 1.26) * 3))}, hexc('#ffe090'))
        # steam curls
        for s_ in range(2):
            ph = (t + s_ * .5) % 1.0
            cv.put(ell(26 + s_ * 14 + 3 * S(ph * TAU), 12 - ph * 12, 3 - ph * 2, 3 - ph * 2) if ph < .9 else set(), hexc('#ffe9d0') if ph < .5 else hexc('#d8a898'))
        out.append(cv)
    return out

# ----------------------------------------------------------------------------------------- THEME 2 & 4
def balloon(n=6, scheme=0):
    out = []
    P = [RED, BLUE, YELLOW, MINT][scheme]
    for i in range(n):
        cv = Canvas(32, 56); t = i / n * TAU
        sw = 2.4 * S(t + scheme)
        for y in range(28, 54):
            x = 16 + sw * ((y - 28) / 26) ** 1.6 * S(y * .3 + t)
            cv.put([(int(x), y)], hexc('#fff0d6'))
        part(cv, ell(16, 14, 11, 13) | poly([(16, 26), (13, 30), (19, 30)]), P)
        cv.put({(10, 8), (9, 9), (9, 10), (10, 7)}, WHITE)
        out.append(cv)
    return out

def star_twinkle(n=4):
    out = []
    for i in range(n):
        cv = Canvas(11, 11); L = [1, 3, 5, 3][i]
        for k in range(-L, L + 1):
            cv.put([(5 + k, 5)], WHITE if abs(k) < 2 else hexc('#ffe0f0')); cv.put([(5, 5 + k)], WHITE if abs(k) < 2 else hexc('#ffe0f0'))
        cv.put([(5, 5)], WHITE)
        out.append(cv)
    return out

# ----------------------------------------------------------------------------------------- THEME 3
def gear(size, teeth, n=8, scheme=0):
    out = []
    P = [pal('#8a6068', '#c09a98', '#5a3a48', '#3a2434', '#1a0e1c'), pal('#d8a058', '#fff0b0', '#a86a30', '#6a3a28', '#2a1218')][scheme]
    r0 = size / 2 - 5
    for i in range(n):
        cv = Canvas(size, size); c = size / 2
        phi = i / n * (TAU / teeth)
        m = set()
        for y in range(size):
            for x in range(size):
                dx, dy = x + .5 - c, y + .5 - c
                th = math.atan2(dy, dx); rr = math.hypot(dx, dy)
                tooth = 1 if C(teeth * (th - phi)) > -.15 else 0
                if rr <= r0 + 4.2 * tooth: m.add((x, y))
        hole = ell(c, c, r0 * .62, r0 * .62)
        spokes = {(x, y) for (x, y) in hole if C(teeth * (math.atan2(y + .5 - c, x + .5 - c) - phi)) > .35 and math.hypot(x + .5 - c, y + .5 - c) > r0 * .22}
        part(cv, m, P, k=3)
        cv.put(spokes, hexc('#000000') if False else CLEAR)
        cv.put(outline_of(spokes) & m, P['out'])
        cv.put(ell(c, c, r0 * .24, r0 * .24), P['deep']); cv.put(ell(c - 1, c - 1, r0 * .14, r0 * .14), P['hi'])
        out.append(cv)
    return out

def steam_vent(n=8):
    out = []
    for i in range(n):
        cv = Canvas(40, 72); t = i / n
        part(cv, rect(10, 54, 30, 70), pal('#8a6068', '#c09a98', '#5a3a48', '#3a2434', '#1a0e1c'), hi=False)
        part(cv, rect(7, 50, 33, 56), pal('#a07880', '#d0b0a8', '#6a4a58', '#44283a', '#1a0e1c'), hi=False)
        for k in range(3):
            ph = (t + k / 3) % 1.0
            r = 4 + ph * 8
            x = 20 + 3 * S(ph * TAU + k * 2); y = 46 - ph * 44
            col = hexc('#fff2ea') if ph < .45 else hexc('#e0c8c8') if ph < .75 else hexc('#b89aa0')
            m = ell(x, y, r, r * .85)
            if ph < .8: cv.put(m, col); cv.put({(p[0], p[1]) for p in m if (p[0] + 1, p[1] + 1) not in m}, hexc('#c0a0a8') if ph < .45 else hexc('#a08890'))
            else: cv.put({p for p in m if dither(p[0], p[1], 1 - (ph - .8) / .2)}, col)
        out.append(cv)
    return out

def choc_vat(n=8):
    out = []
    rng = random.Random(9)
    bubbles = [(rng.randrange(12, 52), rng.random()) for _ in range(6)]
    for i in range(n):
        cv = Canvas(72, 56); t = i / n
        body = rect(6, 20, 66, 54)
        part(cv, body, pal('#7a8aa0', '#c0d0e0', '#4a5a78', '#34405a', '#141a2a'), hi=False)
        cv.put(rect(6, 20, 66, 23), hexc('#c0d0e0'))
        for rx in range(12, 64, 14): cv.put({(rx, ry) for ry in range(30, 52)}, hexc('#5a6a88')); cv.put({(rx + 1, ry) for ry in range(30, 52)}, hexc('#8a9ab8'))
        cv.put(rect(4, 14, 68, 22), hexc('#9aaac8')); cv.put(outline_of(rect(4, 14, 68, 22)), hexc('#141a2a'))
        surf = ell(36, 17, 31, 4)
        cv.put(surf, hexc('#6a3a28')); cv.put(ell(36, 16, 28, 2.6), hexc('#8a5438'))
        cv.put({(14 + (k + i * 3) % 46, 15) for k in range(0, 46, 9)}, hexc('#b07858'))
        for (bx, ph) in bubbles:
            k = (t + ph) % 1.0
            if k < .8:
                r = 1.5 + 3.2 * k; by = 16 - k * 6
                b = ell(bx, by, r, r * .75); cv.put(outline_of(b), hexc('#3a1c14')); cv.put(b, hexc('#8a5438')); cv.put({(int(bx) - 1, int(by) - 1)}, hexc('#c89878'))
            else: cv.put({(int(bx) + a, 12 - (a % 2)) for a in (-3, 0, 3)}, hexc('#8a5438'))
        out.append(cv)
    return out

def lamp(n=8):
    out = []
    for i in range(n):
        cv = Canvas(48, 72); t = i / n * TAU
        sw = 5 * S(t)
        ax = 24
        lx, ly = ax + sw, 44
        cv.put(line(ax, 0, lx, ly - 8, .6), hexc('#8a6068'))
        sh = poly([(lx - 11, ly + 6), (lx - 4, ly - 8), (lx + 4, ly - 8), (lx + 11, ly + 6)])
        part(cv, sh, pal('#5a6a88', '#9aaac8', '#3a4668', '#262e48', '#141a2a'), hi=False)
        cv.put(rect(int(lx - 8), ly + 4, int(lx + 8), ly + 8), hexc('#ffe090'))
        # soft glow cone
        for y in range(ly + 8, 70):
            for x in range(int(lx - 8 - (y - ly) * .35), int(lx + 8 + (y - ly) * .35)):
                if dither(x, y, .32 * (1 - (y - ly) / 40)): cv.put([(x, y)], hexc('#ffd070'))
        out.append(cv)
    return out

# ----------------------------------------------------------------------------------------- THEME 4
def bunting(n=8):
    out = []
    cols = [hexc('#ee3d4e'), hexc('#ffd23a'), hexc('#fff2e8'), hexc('#4aa9f4')]
    for i in range(n):
        cv = Canvas(160, 40); t = i / n * TAU
        ys = lambda x: 8 + 5 * math.sin(TAU * x / 80.0)
        for x in range(160): cv.put([(x, int(ys(x)))], hexc('#2a0a28'))
        for k in range(8):
            x0 = 10 + k * 20
            y0 = int(ys(x0))
            flap = 2 * S(t + k * .8)
            tri_ = poly([(x0 - 7, y0 + 1), (x0 + 7, y0 + 1), (x0 + flap, y0 + 16 + abs(flap))])
            cv.put(outline_of(tri_), hexc('#2a0a28')); cv.put(tri_, cols[k % 4])
            cv.put({p for p in tri_ if (p[0] + 1, p[1] + 1) not in tri_}, mix(cols[k % 4][:3], hexc('#701838'), .45))
            cv.put({(x0 - 4, y0 + 2), (x0 - 3, y0 + 2)}, WHITE)
        out.append(cv)
    return out

def audience(n=8):
    out = []
    rng = random.Random(21)
    hats = [hexc('#ffd23a'), hexc('#4aa9f4'), hexc('#ee3d4e'), None, hexc('#58d070'), None, hexc('#ff79b8'), hexc('#b070f0')]
    for i in range(n):
        cv = Canvas(192, 64); t = i / n * TAU
        for k in range(10):
            ph = k * 1.9
            bob = int(round(2.2 * abs(S(t + ph))))
            x = 10 + k * 19; y = 40 - bob
            body = ell(x, 58, 11, 14)
            head = ell(x, y, 8.5, 8.5)
            for m, c in ((body, hexc('#2a1030')), (head, hexc('#2a1030'))): cv.put(m, c)
            cv.put({p for p in head if (p[0] - 1, p[1] - 1) not in head}, hexc('#6a3048'))
            if hats[k % 8] is not None:
                cv.put(poly([(x - 6, y - 5), (x + 6, y - 5), (x, y - 15)]), hats[k % 8])
                cv.put({(x, y - 16)}, WHITE)
            if k % 3 == 0 and S(t + ph) > .3:                    # raised arms cheering
                cv.put(line(x - 8, y + 10, x - 13, y - 6 - bob, 2) | line(x + 8, y + 10, x + 13, y - 6 - bob, 2), hexc('#2a1030'))
        out.append(cv)
    return out

def beam(n=1):
    cv = Canvas(64, 240)
    for y in range(240):
        w = 4 + y * .12
        for x in range(int(32 - w), int(32 + w)):
            edge = abs(x - 32) / w
            a = 64 if edge < .75 else 34
            cv.put([(x, y)], (210, 200, 255, int(a * (1 - y / 300))))
    return [cv]

# ----------------------------------------------------------------------------------------- PARTICLES
def sprinkle_sheet():
    cols = ['#ff79b8', '#ffe36a', '#8ff0ff', '#c8a0ff', '#fff0f6', '#7af0a0']
    frames = []
    for c in cols:
        for r in range(4):
            cv = Canvas(8, 8); a = r * math.pi / 4
            cv.put(line(4 - C(a) * 2, 4 - S(a) * 2, 4 + C(a) * 2, 4 + S(a) * 2, .5), hexc(c))
            frames.append(cv)
    return frames

def bubble_frames():
    cv = Canvas(16, 16)
    m = ell(8, 8, 6, 6)
    cv.put(outline_of(m), hexc('#c9b3ff')); cv.put({(x, y) for (x, y) in m if (x + 1, y + 1) not in m}, hexc('#e9ddff'))
    cv.put({(5, 5), (5, 6), (6, 5)}, WHITE)
    pops = []
    for i in range(4):
        c2 = Canvas(16, 16); t = i / 3
        for j in range(6):
            a = j / 6 * TAU
            c2.put(ell(8 + C(a) * (3 + t * 5), 8 + S(a) * (3 + t * 5), max(.7, 1.8 - t * 1.4), max(.7, 1.8 - t * 1.4)), hexc('#e9ddff') if j % 2 else WHITE)
        pops.append(c2)
    return [cv], pops

def glint_frames(n=5):
    out = []
    for i in range(n):
        cv = Canvas(11, 11); L = [0, 2, 4, 2, 0][i]
        for k in range(-L, L + 1): cv.put([(5 + k, 5), (5, 5 + k)], WHITE)
        cv.put([(5, 5)], WHITE)
        out.append(cv)
    return out

def ember_frames(n=4):
    out = []
    for i in range(n):
        cv = Canvas(8, 8); r = [1.2, 2, 2.6, 2][i]
        cv.put(ell(4, 4, r + 1.2, r + 1.2), hexc('#ff9a3a')); cv.put(ell(4, 4, r, r), hexc('#ffe090'))
        out.append(cv)
    return out

def drip_frames():
    d = []
    for i in range(2):
        cv = Canvas(8, 12); h = 6 + i * 2
        cv.put(ell(4, 4, 2.6, 2.6) | rect(3, 1, 5, 2 + h // 2), hexc('#ffb040')); cv.put({(3, 3)}, hexc('#ffe090'))
        d.append(cv)
    sp = []
    for i in range(3):
        cv = Canvas(16, 8)
        for j in range(3): cv.put(ell(8 + (j - 1) * (2 + i * 2), 6 - (1 if j == 1 else 0) * (2 - i), 1.4, 1.4), hexc('#ffb040'))
        sp.append(cv)
    return d, sp
