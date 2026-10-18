"""El Rey de Hielo: a gaunt, sorrowful king in dark frozen armour who keeps a captive sun in his chest.
Form 1  the king on his iron throne  ->  Form 2  the broken giant (throne shattered, crown cracked, beard of icicles)
->  Form 3  the hollow storm (empty armour and a spectral crown swirling in a blizzard).
Re-skins the old Monarca: the animation sets, frame counts, canvas and the per-frame sun anchors are imported from ice_boss,
so the fight code (weak point, shell, attacks) keeps working unchanged."""
import math, random
from px import *
import ice_boss as B

S, C, TAU = math.sin, math.cos, 2 * math.pi
BW, BH, PADX, PADY, CW, CHH = B.BW, B.BH, B.PADX, B.PADY, B.CW, B.CHH
WHITE = hexc('#ffffff')
pal = B.pal

ARMOR = pal('#27325c', '#6a86c4', '#1a2244', '#10162e', '#080c1c', '#4a62a0')
SILVER = pal('#b8c8e8', '#f0f6ff', '#7a90bc', '#4a5c8c', '#141c3c', '#98acd4')
ICE = pal('#8ec8f0', '#e8f8ff', '#4a90d0', '#2a58a0', '#0c1c48', '#6aa8e0')
SKIN = pal('#c8d8f0', '#f0f8ff', '#8fa4cc', '#5a6e9c', '#141c40', '#a8bce0')
CLOAK = pal('#3a2a5a', '#6a58a0', '#2a1c44', '#1a1030', '#0a0618', '#5a4888')
FURG = pal('#dde6f4', '#ffffff', '#a8b8d4', '#7888ac', '#141c40', '#c0cce4')
HAIR = pal('#8ea4cc', '#c8d8f0', '#5a6e9c', '#3a4a78', '#141c40', '#6a80b0')
IRON = pal('#2a2c44', '#5a5e84', '#1c1e30', '#10121e', '#06070e', '#44486c')
GLOW = hexc('#7af0ff'); GLOW_D = hexc('#2a8ad0'); SOCKET = hexc('#0c1230')
CRK = hexc('#10246a'); CRK_L = hexc('#6ac8ff')


def sun(cv, cx, cy, r, phase, rays=8, ray_len=5, bright=1.0):
    B.sun(cv, cx, cy, r, phase, rays, ray_len, bright)


def glow_cracks(cv, mask, amount, seed, cx=64, cy=70, length=34):
    """Jagged cracks that leak cold light, clipped to the body."""
    rng = random.Random(seed)
    for k in range(7):
        a = rng.random() * TAU
        L = length * min(1.0, amount * (0.55 + rng.random() * 0.6))
        x, y = cx, cy
        pts = [(x, y)]
        for _ in range(max(2, int(L / 4))):
            a += rng.uniform(-.7, .7); x += C(a) * 4; y += S(a) * 4; pts.append((x, y))
        for i in range(len(pts) - 1):
            seg = line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], .55) & mask
            cv.put(seg, CRK_L)
            cv.put({(p[0] + 1, p[1] + 1) for p in seg if (p[0] + 1, p[1] + 1) in mask and (p[0] + 1, p[1] + 1) not in seg}, CRK)


def crown(cv, cx, cy, spikes=7, broken=False, glow=0.0, tilt=0):
    """Tall jagged crown of ice on a silver band; broken ones lose spikes and crack."""
    band = rect(int(cx - 15), int(cy + 2), int(cx + 15), int(cy + 8))
    part(cv, band, SILVER, hi=True, k=1)
    for i in range(spikes):
        t = i / max(1, spikes - 1)
        sx = cx - 14 + t * 28
        h = 10 + (8 if i == spikes // 2 else 0) - 3 * abs(i - spikes // 2) * 0.5
        if broken and i in (1, 4, 5): h = 3
        lean = tilt * (1 - abs(t - .5)) + (t - .5) * 4
        part(cv, tri((sx - 2.6, cy + 3), (sx + lean, cy - h + 2), (sx + 2.6, cy + 3)), ICE, hi=False, k=1)
        cv.put({(int(sx + lean * .6), int(cy - h + 4))}, WHITE)
    cv.put(ell(cx, cy + 5, 2.4, 2.2), GLOW); cv.put({(int(cx) - 1, int(cy) + 4)}, WHITE)
    if glow:
        for dx_, dy_ in ((-16, -8), (15, -10), (0, -20), (-9, -16), (10, -17)): cv.put({(int(cx + dx_), int(cy + dy_)), (int(cx + dx_) + 1, int(cy + dy_))}, hexc('#bfeaff'))


def beard(cv, cx, top_y, length, spread=1.0, sway=0.0):
    """A long beard of icicles (strands taper to points); drawn after the head so it hangs over the collar."""
    for k, off in enumerate((-7, -3.5, 0, 3.5, 7)):
        ln = length * (0.62 + .38 * ((k * 3) % 5) / 4.0)
        x0 = cx + off * spread; x1 = x0 + sway * (ln / length) + (k - 2.5) * .6
        m = poly([(x0 - 2.2, top_y), (x0 + 2.2, top_y), (x1, top_y + ln)])
        part(cv, m, FURG, hi=False, k=1)
        cv.put({(int(x0) - 1, int(top_y) + 2), (int(x0) - 1, int(top_y) + 3)}, WHITE)


def face(cv, hx, hy, eyes, mouth, scale=1.0, angry=False, cracked=0.0):
    """Sunken, sorrowful face: dark sockets with a faint cold light, heavy brows and a drawn mouth."""
    ex = 6.2 * scale
    for sgn in (-1, 1):
        x = hx + sgn * ex
        if eyes in ('open', 'glow'):
            cv.put(ell(x, hy, 3.8 * scale, 4.3 * scale), SOCKET)
            cv.put(rect(int(x) - 2, int(hy) - 1, int(x) + 3, int(hy) + 2), GLOW if (eyes == 'glow' or angry) else GLOW_D)
            cv.put({(int(x), int(hy))}, WHITE)
        elif eyes == 'closed':
            cv.put({(int(x) - 3 + i, int(hy) + (1 if i in (0, 5) else 0)) for i in range(6)}, SOCKET)
        elif eyes == 'hurt':
            cv.put({(int(x) - 2 + i, int(hy) - 2 + i) for i in range(5)} | {(int(x) + 2 - i, int(hy) - 2 + i) for i in range(5)}, GLOW)
        if eyes != 'hurt':
            by = hy - 5.5 * scale
            if angry: cv.put(line(x + sgn * 4.8 * scale, by - 2, x - sgn * 4.8 * scale, by + 3, 1.4), SOCKET)
            else: cv.put(line(x - sgn * 4.6 * scale, by + 2.2, x + sgn * 4.6 * scale, by - .6, 1.2), SKIN['deep'])      # brows tilted up toward the middle: grief
        cv.put({(int(x) - 2 + i, int(hy) + 6) for i in range(5) if i % 2 == 0}, SKIN['shade'])                          # tear tracks / hollow cheeks
    cv.put({(int(hx), int(hy) + 2), (int(hx), int(hy) + 3), (int(hx) + 1, int(hy) + 4)}, SKIN['shade'])                 # nose
    my = hy + 9 * scale
    if mouth == 'smile':
        cv.put({(int(hx) - 4 + i, int(my) + (1 if i in (0, 1, 7, 8) else 0)) for i in range(9)}, SOCKET)               # a thin downturned line
    elif mouth == 'open':
        m = ell(hx, my + 1, 5.2 * scale, 3.8 * scale); cv.put(outline_of(m), SOCKET); cv.put(m, hexc('#0a0e24')); cv.put({(int(hx) - 3 + 2 * i, int(my) - 1) for i in range(4)}, SILVER['base'])
    elif mouth == 'fangs':
        m = ell(hx, my + 1, 6.5 * scale, 4.2 * scale); cv.put(outline_of(m), SOCKET); cv.put(m, hexc('#0a0e24'))
        cv.put({(int(hx) - 5 + 3 * i, int(my) - 2 + j) for i in range(4) for j in range(3)}, hexc('#e8f4ff'))
        cv.put({(int(hx) - 4 + 3 * i, int(my) + 4 - j) for i in range(3) for j in range(2)}, hexc('#c8d8f0'))
    elif mouth == 'o':
        m = ell(hx + 1, my + 1, 3.0, 3.6); cv.put(outline_of(m), SOCKET); cv.put(m, hexc('#0a0e24'))
    if cracked: glow_cracks(cv, ell(hx, hy, 15 * scale, 17 * scale), cracked, 3, hx - 4, hy + 4, 18)


def gauntlet(cv, shoulder, hand, thick=6.0, claw=False):
    sx, sy = shoulder; hx, hy = hand
    ex, ey = (sx + hx) / 2, (sy + hy) / 2 + 3
    part(cv, line(sx, sy, ex, ey, thick), CLOAK, hi=False, k=2)
    part(cv, line(ex, ey, hx, hy, thick - .8), ARMOR, hi=False, k=2)
    part(cv, ell(ex, ey, thick, thick), SILVER, hi=False, k=1)                          # elbow guard
    part(cv, ell(hx, hy, thick + 1.6, thick + 1.2), ARMOR, hi=True, k=2)
    cv.put({(int(hx) - 2, int(hy) - 3), (int(hx), int(hy) - 4), (int(hx) + 2, int(hy) - 3)}, SILVER['hi'])      # knuckle glints
    if claw:
        for i in range(3):
            a = math.atan2(hy - ey, hx - ex) + (i - 1) * .5
            part(cv, line(hx, hy, hx + C(a) * 9, hy + S(a) * 9, 1.2), ICE, hi=False, shade=False, k=1)


def wheel(cv, x, y, r, phase):
    cv.put(ell(x, y, r, r), hexc('#10121e')); cv.put(outline_of(ell(x, y, r, r)), hexc('#06070e'))
    cv.put(ell(x, y, r - 2, r - 2), hexc('#3a3c58'))
    for k in range(6):
        a = (k + phase) * TAU / 6
        cv.put(line(x, y, x + C(a) * (r - 2), y + S(a) * (r - 2), .55), SILVER['shade'])
    cv.put(ell(x, y, 2, 2), SILVER['base'])


# ------------------------------------------------------------------------------------------ form 1: the king on his throne
def draw_host(dy=0, sq=0.0, armL=(-12, 12), armR=(12, 12), eyes='open', mouth='smile', crown_dy=0, sunpulse=0.0, sunph=0.0, wheelph=0.0, lean=0, cracks_=0.0, hair=0, **_):
    cv = Canvas(BW, BH)
    cx = 64 + lean
    # throne: tall pointed back of dark iron with silver trim and ice growing out of the stone
    back = poly([(36, 112), (36, 34), (44, 16), (49, 4), (54, 18), (64, 8), (74, 18), (79, 4), (84, 16), (92, 34), (92, 112)])
    part(cv, back, IRON)
    cv.put(outline_of(back), SILVER['deep'])
    cv.put({(x, 22) for x in range(48, 81)}, SILVER['shade']); cv.put({(x, 21) for x in range(48, 81)}, SILVER['base'])
    for (ix, iy, ih) in ((38, 108, 14), (90, 106, 18), (36, 70, 10)):
        part(cv, tri((ix - 3, iy), (ix + 1, iy - ih), (ix + 4, iy)), ICE, hi=False, k=1)
    for r_ in range(3):
        for c_ in range(3):
            cv.put(ell(46 + c_ * 18 + (9 if r_ % 2 else 0), 34 + r_ * 18, 1.4, 1.4), SILVER['shade'])
    wheel(cv, 42, 114, 9, wheelph); wheel(cv, 86, 114, 9, wheelph + .3)
    seat = rect(34, 98, 94, 110)
    part(cv, seat, IRON, hi=True, k=2)
    cv.put({(x, 98) for x in range(34, 94)}, SILVER['base']); cv.put({(x, 109) for x in range(34, 94)}, SILVER['deep'])
    # cloak spilling behind the shoulders, then the armoured body
    ty = 80 + dy
    cloak = poly([(cx - 34, 108), (cx - 30, ty - 20), (cx - 18, ty - 30), (cx + 18, ty - 30), (cx + 30, ty - 20), (cx + 34, 108)])
    part(cv, cloak, CLOAK, hi=False)
    for k in range(5): cv.put({(int(cx - 30 + k * 13 + S(k * 2) * 2), y) for y in range(ty - 10, 106, 2)}, CLOAK['shade'])
    torso = ell(cx, ty, 23 - sq, 26 + sq)
    part(cv, torso, ARMOR)
    cv.put({(x, ty - 20) for x in range(int(cx - 10), int(cx + 11))}, SILVER['shade'])
    for sx_ in (-1, 1):                                                     # pauldrons with an ice thorn each
        part(cv, ell(cx + sx_ * 24, ty - 14, 8.5, 6.5), SILVER, hi=True, k=1)
        part(cv, tri((cx + sx_ * 24 - 3, ty - 18), (cx + sx_ * 27, ty - 31), (cx + sx_ * 24 + 3, ty - 18)), ICE, hi=False, k=1)
    for xx in (-16, 16): cv.put(line(cx + xx, ty - 12, cx + xx * .8, ty + 22, .55), SILVER['shade'])
    cv.put({(x, ty + 19) for x in range(int(cx - 15), int(cx + 16))}, SILVER['base']); cv.put({(x, ty + 20) for x in range(int(cx - 15), int(cx + 16))}, SILVER['deep'])
    for xx in range(-12, 13, 6): cv.put({(int(cx + xx), int(ty) + 19)}, WHITE)
    # chest: a cage of ice teeth around the captive sun
    win = ell(cx, ty + 1, 12.5, 12.5)
    cv.put(outline_of(win), SILVER['deep']); cv.put(win, SOCKET)
    sun(cv, cx, ty + 1, 8 + sunpulse * 1.2, sunph, ray_len=3 + sunpulse * 2, bright=.7 + .3 * sunpulse)
    for k in range(9):
        a = k * TAU / 9 + .3
        part(cv, tri((cx + C(a - .2) * 13.5, ty + 1 + S(a - .2) * 13.5), (cx + C(a) * 8.5, ty + 1 + S(a) * 8.5), (cx + C(a + .2) * 13.5, ty + 1 + S(a + .2) * 13.5)), ICE, hi=False, k=1)
    cv.put({(cx - 9, ty - 8 + k) for k in range(4)}, WHITE)
    if cracks_ > 0: glow_cracks(cv, torso, cracks_, 11, cx, ty + 1, 26)
    # armrests, arms
    for sx_ in (-1, 1):
        part(cv, rect(int(cx + sx_ * 33 - 5), 88, int(cx + sx_ * 33 + 6), 104), IRON, hi=False, k=1)
        part(cv, rect(int(cx + sx_ * 33 - 7), 85, int(cx + sx_ * 33 + 8), 89), SILVER, hi=False, k=1)
    gauntlet(cv, (cx - 22, ty - 8), (cx - 22 + armL[0], ty - 8 + armL[1]))
    gauntlet(cv, (cx + 22, ty - 8), (cx + 22 + armR[0], ty - 8 + armR[1]))
    # pale hair behind the head, fur collar, head, beard, crown
    hy = 44 + dy
    for k in range(7):
        x = cx - 17 + k * 5.6
        part(cv, line(x, hy - 6, x + (k - 3) * 1.2 + S(k + hair) * 1.5, hy + 24, 1.5), HAIR, hi=False, k=1)
    part(cv, ell(cx, hy + 15, 21, 6.5), FURG, hi=True, k=2)
    head = ell(cx, hy, 15.5, 17.5) | poly([(cx - 10, hy + 6), (cx + 10, hy + 6), (cx, hy + 22)])
    part(cv, head, SKIN)
    face(cv, cx, hy, eyes, mouth)
    beard(cv, cx, hy + 14, 15, 1.0, 0)
    crown(cv, cx, hy - 16 + crown_dy, 7)
    return cv, (cx, ty + 1)


# ------------------------------------------------------------------------------------------ form 2: the broken giant
def draw_giant(dy=0, sq=0.0, armL=(-18, 16), armR=(18, 16), eyes='open', mouth='fangs', crown_dy=0, sunpulse=0.0, sunph=0.0, lean=0, cracks_=1.0, stomp=0.0, shake=0, **_):
    cv = Canvas(BW, BH)
    cx = 64 + lean + shake
    # wreck of the throne
    for (x, y, w, h) in ((10, 116, 16, 8), (100, 117, 14, 7), (24, 120, 10, 5)):
        part(cv, rect(x, y, x + w, y + h), IRON, hi=False, k=1)
    cv.put({(12 + i, 116) for i in range(0, 14, 2)}, SILVER['base'])
    for (ix, ih) in ((18, 12), (104, 14)):
        part(cv, tri((ix - 4, 118), (ix, 118 - ih), (ix + 4, 118)), ICE, hi=False, k=1)
    for sx_ in (-1, 1):
        lift = stomp if sx_ > 0 else 0
        part(cv, ell(cx + sx_ * 15, 112 - lift, 14, 11), ARMOR, hi=False)
        cv.put({(int(cx + sx_ * 15) - 5 + i, 106 - int(lift)) for i in range(11)}, SILVER['shade'])
    ty = 74 + dy
    # tattered cloak behind
    cl = poly([(cx - 40, 112), (cx - 34, ty - 20), (cx - 20, ty - 34), (cx + 20, ty - 34), (cx + 34, ty - 20), (cx + 40, 112), (cx + 30, 104), (cx + 22, 114), (cx + 10, 104), (cx - 6, 114), (cx - 20, 104), (cx - 30, 114)])
    part(cv, cl, CLOAK, hi=False)
    torso = ell(cx, ty, 30 - sq, 33 + sq)
    part(cv, torso, ARMOR)
    cv.put({(x, ty - 26) for x in range(int(cx - 12), int(cx + 13))}, SILVER['shade'])
    for sx_ in (-1, 1):                                                     # pauldron crystals
        part(cv, ell(cx + sx_ * 30, ty - 14, 9, 7), SILVER, hi=True, k=1)
        for k in range(3):
            part(cv, tri((cx + sx_ * (22 + k * 5) - 3, ty - 22 + k * 5), (cx + sx_ * (27 + k * 6), ty - 40 + k * 6), (cx + sx_ * (22 + k * 5) + 3, ty - 20 + k * 5)), ICE, hi=False, k=1)
    win = ell(cx, ty + 4, 15, 15)
    cv.put(outline_of(win), CRK); cv.put(win, SOCKET)
    for k in range(9):
        a = k * TAU / 9 + .2
        cv.put(poly([(cx + C(a) * 15, ty + 4 + S(a) * 15), (cx + C(a + .25) * 22, ty + 4 + S(a + .25) * 22), (cx + C(a + .5) * 15, ty + 4 + S(a + .5) * 15)]), ICE['base'])
    sun(cv, cx, ty + 4, 10 + sunpulse * 1.5, sunph, ray_len=4 + sunpulse * 3, bright=.8 + .2 * sunpulse)
    glow_cracks(cv, torso, cracks_, 7, cx, ty + 4, 40)
    gauntlet(cv, (cx - 28, ty - 8), (cx - 28 + armL[0], ty - 8 + armL[1]), thick=8.0)
    gauntlet(cv, (cx + 28, ty - 8), (cx + 28 + armR[0], ty - 8 + armR[1]), thick=8.0, claw=True)
    hy = 36 + dy
    for k in range(8):
        x = cx - 20 + k * 5.6
        part(cv, line(x, hy - 6, x + (k - 3.5) * 1.6 + S(k) * 2, hy + 26, 1.6), HAIR, hi=False, k=1)
    part(cv, ell(cx, hy + 15, 23, 7), FURG, hi=True, k=2)
    head = ell(cx, hy, 16.5, 18.5) | poly([(cx - 11, hy + 6), (cx + 11, hy + 6), (cx, hy + 24)])
    part(cv, head, SKIN)
    face(cv, cx, hy, eyes, mouth, scale=1.05, angry=True, cracked=cracks_ * .8)
    beard(cv, cx, hy + 15, 21, 1.15, 2 * S(dy))
    crown(cv, cx, hy - 17 + crown_dy, 7, broken=True, tilt=2)
    return cv, (cx, ty + 4)


# ------------------------------------------------------------------------------------------ form 3: the hollow storm
def funnel_w(s_, expand):
    return (31 + expand * 5) * (1 - s_) ** 0.72 + 4


def draw_storm(phase=0.0, dy=0, sunpulse=0.0, sunph=0.0, eyes='glow', crown_dy=0, spread=0.0, armL=(-26, 6), armR=(26, 6), expand=0.0, hurt=False, wobble=0.0, **_):
    """Blizzard funnel around the empty armour. Helical streaks are continuous curves; one phase step = one streak spacing (seamless loop)."""
    cv = Canvas(BW, BH)
    cx = 64
    top, bot = 28, 122
    def centre(s_): return cx + S(s_ * 5.0 + phase * TAU) * 3.2 * s_ + wobble * s_
    body = set()
    for y in range(top, bot):
        s_ = (y - top) / float(bot - top)
        w = funnel_w(s_, expand) + S(y * .9 + phase * TAU * 2) * 1.3
        for x in range(int(centre(s_) - w), int(centre(s_) + w) + 1): body.add((x, y + int(dy)))
    cv.put(outline_of(body), hexc('#0a1030'))
    for (x, y) in body:
        t = (y - top - dy) / float(bot - top)
        cv.put([(x, y)], hexc('#243a78') if dither(x, y, min(1.0, t * 1.4)) else hexc('#34508e'))
    cv.put({(x, y) for (x, y) in body if (x + 2, y + 2) not in body}, hexc('#14224e'))
    # the hollow: a dark cavity in the shape of the missing king, with a faint empty breastplate and helm floating inside
    cav_head = ell(cx + wobble * .1, 55 + dy, 14, 12)
    cav_chest = ell(cx + wobble * .2, 82 + dy, 17, 19)
    cavity = (cav_head | cav_chest) & body
    cv.put(cavity, hexc('#080c24'))
    cv.put({p for p in cavity if (p[0] - 1, p[1] - 1) not in cavity}, hexc('#1c2c68'))
    helm = (ell(cx + wobble * .1, 50 + dy, 9.5, 9) | rect(int(cx - 9 + wobble * .1), 50 + dy, int(cx + 10 + wobble * .1), 62 + dy)) & body
    cv.put(helm, hexc('#161e44')); cv.put({p for p in helm if (p[0] - 1, p[1] - 1) not in helm}, hexc('#3a4e90'))
    # streaks: back ones dark, front ones pale
    for layer in (0, 1):
        for k in range(10):
            prev = None
            for j in range(0, 29):
                s_ = j / 28.0
                th = (phase * TAU) + s_ * 6.4 + k * TAU / 10.0
                depth = S(th)
                if (depth > 0) != (layer == 1):
                    prev = None; continue
                w = funnel_w(s_, expand) * 0.93
                x = centre(s_) + C(th) * w
                y = top + s_ * (bot - top) + dy + depth * 2.0
                if prev is not None:
                    col = (hexc('#e8f4ff') if depth > .55 else hexc('#a8c4ec')) if layer == 1 else hexc('#4a68b0')
                    cv.put(line(prev[0], prev[1], x, y, 1.1 if layer == 1 and depth > .4 else .7), col)
                prev = (x, y)
    # shreds of cloak and shards of armour caught in the wind
    rng = random.Random(5)
    for k in range(14):
        a_ = rng.random() * TAU + phase * TAU * (1 if k % 2 else -1)
        r_ = 22 + rng.random() * 22 + expand * 4
        y_ = 30 + rng.random() * 80
        px_, py_ = int(cx + C(a_) * r_ * (0.4 + y_ / 200.0)), int(y_ + S(a_) * 3 + dy)
        if k % 3 == 0: cv.put(poly([(px_, py_), (px_ + 4, py_ + 1), (px_ + 1, py_ + 4)]), SILVER['shade'])
        elif k % 3 == 1: cv.put({(px_, py_), (px_ + 1, py_), (px_, py_ + 1), (px_ + 1, py_ + 1)}, CLOAK['hi'])
        else: cv.put({(px_, py_)}, WHITE)
    face_y = 52 + dy
    if not hurt:
        for sgn in (-1, 1):
            cv.put(rect(int(cx + sgn * 6) - 3, int(face_y), int(cx + sgn * 6) + 4, int(face_y) + 3), GLOW)
            cv.put(rect(int(cx + sgn * 6) - 1, int(face_y), int(cx + sgn * 6) + 2, int(face_y) + 1), WHITE)
    else:
        for sgn in (-1, 1):
            x = int(cx + sgn * 6); cv.put({(x - 2 + i, int(face_y) - 1 + i) for i in range(5)} | {(x + 2 - i, int(face_y) - 1 + i) for i in range(5)}, hexc('#bff0ff'))
    sun(cv, cx + wobble * .2, 82 + dy, 11 + sunpulse * 1.6, sunph, rays=10, ray_len=5 + sunpulse * 3, bright=.85 + .15 * sunpulse)
    for sgn, hand in ((-1, armL), (1, armR)):                          # spectral arms ending in empty gauntlets
        sx, sy = cx + sgn * 22, 66 + dy
        pts = [(sx + hand[0] * t, sy + hand[1] * t + S(t * 3 + phase * TAU * 2) * 2.5) for t in [i / 7.0 for i in range(8)]]
        m = set()
        for i in range(7): m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], 3.6 - i * .2)
        part(cv, m, dict(ARMOR, base=hexc('#34508e'), shade=hexc('#1c2c68'), deep=hexc('#10183a')), hi=False, k=2)
        fx, fy = pts[-1]
        part(cv, ell(fx, fy, 6.8, 6.2), ARMOR, hi=True, k=2)
    crown(cv, cx, 20 + crown_dy + dy, 7, glow=1.0 if sunpulse > .5 else 0.0)
    return cv, (cx + wobble * .2, 82 + dy)


# ------------------------------------------------------------------------------------------ wiring: same animations, anchors and canvas
B.draw_host, B.draw_giant, B.draw_storm = draw_host, draw_giant, draw_storm
ANIMS = B.ANIMS
build_boss = B.build_boss
