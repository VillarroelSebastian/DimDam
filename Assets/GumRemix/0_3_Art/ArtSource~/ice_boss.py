"""Monarca del Deshielo: a snow creature with a captive sun in his chest.
Form 1  host on a rolling armchair  ->  Form 2  cracked giant (chair destroyed)  ->  Form 3  hollow storm with a crown.
Every pose returns the sun position, so the in-game weak point follows the real drawing."""
import math, random
from px import *

S, C, TAU = math.sin, math.cos, 2 * math.pi
BW = BH = 128          # drawing canvas
PADX, PADY = 24, 6       # extra room so outstretched arms / shake never clip
CW, CHH = BW + 2 * PADX, BH + 2 * PADY
WHITE = hexc('#ffffff')

def pal(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

SNOW = pal('#f2faff', '#ffffff', '#bcd6f2', '#8aa8d8', '#2c3a80', '#a8c4ea')
SNOW_D = pal('#dceaf8', '#f6fcff', '#a6c0e6', '#7a98cc', '#2c3a80', '#98b4e0')
VELVET = pal('#8e2c7c', '#c4549e', '#5c1a58', '#3a1038', '#1c0820', '#7a2468')
GOLD = pal('#ffd23a', '#fff0a0', '#e8982a', '#a8602a', '#4a2410')
CROWN = pal('#b8ecff', '#ffffff', '#68b8f0', '#3a78d0', '#14306e')
FUR = pal('#fbfeff', '#ffffff', '#cfe2f6', '#a0bce4', '#2c3a80', '#b8d0f0')
DARK = hexc('#1a1646')
CRACK = hexc('#2a3a90'); CRACK_L = hexc('#7ab4f0')

def sun(cv, cx, cy, r, phase, rays=8, ray_len=5, bright=1.0):
    """The captive sun: layered discs plus rays that rotate with `phase` (period = one ray gap, so the loop is seamless)."""
    for k in range(rays):
        a = (k + phase) * TAU / rays
        cv.put(line(cx + C(a) * (r + 1), cy + S(a) * (r + 1), cx + C(a) * (r + ray_len), cy + S(a) * (r + ray_len), .9), hexc('#ffe36a'))
        cv.put({(int(cx + C(a) * (r + ray_len + 1)), int(cy + S(a) * (r + ray_len + 1)))}, WHITE)
    cv.put(outline_of(ell(cx, cy, r, r)), hexc('#a8402c'))
    cv.put(ell(cx, cy, r, r), hexc('#ff9a3a'))
    cv.put(ell(cx - .5, cy - .5, r - 2, r - 2), hexc('#ffd23a'))
    cv.put(ell(cx - 1, cy - 1, max(1, (r - 4) * bright), max(1, (r - 4) * bright)), hexc('#fff6b0'))
    cv.put({(int(cx) - 2, int(cy) - 3), (int(cx) - 3, int(cy) - 2)}, WHITE)

def cracks(cv, mask, amount, seed, cx=64, cy=70, length=34):
    """Jagged cracks radiating from the chest, clipped to the body mask."""
    rng = random.Random(seed)
    n = 7
    for k in range(n):
        a = rng.random() * TAU
        L = length * min(1.0, amount * (0.55 + rng.random() * 0.6))
        x, y = cx, cy
        pts = [(x, y)]
        steps = max(2, int(L / 4))
        for _ in range(steps):
            a += rng.uniform(-.7, .7)
            x += C(a) * 4; y += S(a) * 4
            pts.append((x, y))
        for i in range(len(pts) - 1):
            seg = line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], .55) & mask
            cv.put(seg, CRACK)
            cv.put({(p[0] - 1, p[1] - 1) for p in seg if (p[0] - 1, p[1] - 1) in mask and (p[0] - 1, p[1] - 1) not in seg}, CRACK_L)

def crown(cv, cx, cy, spikes=5, broken=False, glow=0.0, tilt=0):
    base = rect(int(cx - 14), int(cy), int(cx + 14), int(cy + 6))
    part(cv, base, CROWN, hi=False, k=1)
    for i in range(spikes):
        t = i / max(1, spikes - 1)
        sx = cx - 13 + t * 26
        h = 9 + (6 if i == spikes // 2 else 0) - (5 if broken and i in (1, 3) else 0)
        sp = tri((sx - 3.2, cy + 1), (sx + tilt * (1 - abs(t - .5)) + 0, cy - h), (sx + 3.2, cy + 1))
        part(cv, sp, CROWN, hi=False, k=1)
        cv.put({(int(sx), int(cy) - h + 2)}, WHITE)
    for gx, col in ((cx - 7, '#ff79b8'), (cx, '#ffe36a'), (cx + 7, '#7af0ff')):
        cv.put(ell(gx, cy + 3, 1.7, 1.7), hexc(col)); cv.put({(int(gx) - 1, int(cy) + 2)}, WHITE)
    if glow: cv.put(ring(ell(cx, cy - 4, 17 + glow * 3, 11 + glow * 2)), hexc('#e8fbff'))

def face(cv, hx, hy, eyes, mouth, scale=1.0, angry=False):
    ex = 6.5 * scale
    for sgn in (-1, 1):
        x = hx + sgn * ex
        if eyes == 'open':
            cv.put(ell(x, hy, 3.4 * scale, 3.9 * scale), DARK); cv.put({(int(x) - 1, int(hy) - 2)}, WHITE)
        elif eyes == 'closed':
            cv.put({(int(x) - 2 + i, int(hy) + (1 if i in (0, 4) else 0)) for i in range(5)}, DARK)
        elif eyes == 'hurt':
            cv.put({(int(x) - 2 + i, int(hy) - 2 + i) for i in range(5)} | {(int(x) + 2 - i, int(hy) - 2 + i) for i in range(5)}, DARK)
        elif eyes == 'glow':
            cv.put(rect(int(x) - 3, int(hy) - 1, int(x) + 4, int(hy) + 2), hexc('#7af0ff')); cv.put(rect(int(x) - 1, int(hy), int(x) + 2, int(hy) + 1), WHITE)
        # brows: gentle for the host, furious for the others
        if eyes != 'hurt':
            by = hy - 5 * scale
            if angry: cv.put(line(x + sgn * 4.5 * scale, by - 2, x - sgn * 4.5 * scale, by + 3, 1.3), CRACK)
            else: cv.put(line(x - 4 * scale, by + 1, x + 4 * scale, by, 1.0), SNOW['deep'])
    cv.put({(int(hx) - 11, int(hy) + 3), (int(hx) - 10, int(hy) + 3), (int(hx) + 10, int(hy) + 3), (int(hx) + 11, int(hy) + 3)}, hexc('#ff9fb4'))
    nose = poly([(hx - 1, hy + 1), (hx + 9 * scale, hy + 4), (hx - 1, hy + 5)])
    part(cv, nose, pal('#ff8a30', '#ffc890', '#e0602a', '#a03c2c', '#40142a'), hi=False, k=1)
    my = hy + 9 * scale
    if mouth == 'smile': cv.put({(int(hx) - 6 + i, int(my) + (0 if 1 < i < 10 else -1)) for i in range(12)}, DARK)
    elif mouth == 'open':
        m = ell(hx, my + 1, 6.5 * scale, 4.5 * scale); cv.put(outline_of(m), DARK); cv.put(m, hexc('#3a1038')); cv.put(ell(hx, my + 3, 3.2, 1.6), hexc('#ff6a8a'))
    elif mouth == 'fangs':
        m = ell(hx, my + 1, 7.5 * scale, 4.5 * scale); cv.put(outline_of(m), DARK); cv.put(m, hexc('#3a1038'))
        cv.put({(int(hx) - 6 + 3 * i, int(my) - 2 + j) for i in range(5) for j in range(2)}, WHITE)
    elif mouth == 'o':
        m = ell(hx + 1, my + 1, 3.2, 3.6); cv.put(outline_of(m), DARK); cv.put(m, hexc('#3a1038'))

def arm(cv, shoulder, hand, thick=6.5, fur=True, spikes=False, pal_=None):
    sx, sy = shoulder; hx, hy = hand
    part(cv, line(sx, sy, hx, hy, thick), pal_ or SNOW, hi=False, k=2)
    if fur:
        part(cv, ell(hx, hy, thick + 3.5, thick + 3), FUR, hi=True, k=2)
        cv.put({(int(hx) - 3 + 2 * i, int(hy) + 4) for i in range(4)}, FUR['deep'])
    if spikes:
        for dx in (-4, 0, 4):
            part(cv, tri((sx + dx - 2, sy - 3), (sx + dx, sy - 12 - abs(dx)), (sx + dx + 2, sy - 3)), CROWN, hi=False, k=1)

# ------------------------------------------------------------------------------------------ form 1: the host
def wheel(cv, x, y, r, phase):
    cv.put(ell(x, y, r, r), hexc('#3a1a28')); cv.put(outline_of(ell(x, y, r, r)), hexc('#1c0820'))
    cv.put(ell(x, y, r - 2, r - 2), hexc('#7a4a2c'))
    for k in range(6):
        a = (k + phase) * TAU / 6
        cv.put(line(x, y, x + C(a) * (r - 2), y + S(a) * (r - 2), .55), hexc('#d8a060'))
    cv.put(ell(x, y, 2, 2), hexc('#ffd23a'))

def draw_host(dy=0, sq=0.0, armL=(-12, 12), armR=(12, 12), eyes='open', mouth='smile', crown_dy=0, sunpulse=0.0, sunph=0.0, wheelph=0.0, lean=0, cracks_=0.0, hair=0, **_):
    cv = Canvas(BW, BH)
    cx = 64 + lean
    # chair: tall back with tufted buttons, wheels
    back = poly([(36, 112), (36, 28), (44, 14), (84, 14), (92, 28), (92, 112)])
    part(cv, back, VELVET)
    cv.put(outline_of(back), GOLD['deep'])
    cv.put({(x, 16) for x in range(46, 82)} | {(x, 17) for x in range(46, 82)}, GOLD['base'])
    for r_ in range(3):
        for c_ in range(3):
            cv.put(ell(46 + c_ * 18 + (9 if r_ % 2 else 0), 30 + r_ * 18, 1.5, 1.5), GOLD['base'])
    wheel(cv, 42, 114, 9, wheelph); wheel(cv, 86, 114, 9, wheelph + .3)
    seat = rect(34, 98, 94, 110)
    part(cv, seat, VELVET, hi=True, k=2)
    cv.put({(x, 98) for x in range(34, 94)}, GOLD['base']); cv.put({(x, 109) for x in range(34, 94)}, GOLD['shade'])
    # body (in front of the back, sitting on the seat)
    ty = 80 + dy
    torso = ell(cx, ty, 25 - sq, 25 + sq)
    part(cv, torso, SNOW)
    # chest window with the captive sun
    win = ell(cx, ty + 1, 12.5, 12.5)
    cv.put(outline_of(win), CROWN['out']); cv.put(win, hexc('#2a2a70'))
    sun(cv, cx, ty + 1, 8 + sunpulse * 1.2, sunph, ray_len=3 + sunpulse * 2, bright=.7 + .3 * sunpulse)
    cv.put(ring(ell(cx, ty + 1, 12.5, 12.5)), CROWN['base']); cv.put({(cx - 9, ty - 8 + k) for k in range(5)}, WHITE)
    cracks(cv, set(), 0, 1)
    if cracks_ > 0: cracks(cv, torso, cracks_, 11, cx, ty + 1, 26)
    # armrests in front of the body, arms on top
    for sx_ in (-1, 1):
        part(cv, rect(int(cx + sx_ * 33 - 5), 88, int(cx + sx_ * 33 + 6), 104), VELVET, hi=False, k=1)
        part(cv, rect(int(cx + sx_ * 33 - 7), 85, int(cx + sx_ * 33 + 8), 89), GOLD, hi=False, k=1)
    arm(cv, (cx - 22, ty - 8), (cx - 22 + armL[0], ty - 8 + armL[1]))
    arm(cv, (cx + 22, ty - 8), (cx + 22 + armR[0], ty - 8 + armR[1]))
    # head + crown
    hy = 44 + dy
    head = ell(cx, hy, 19.5, 17)
    part(cv, head, SNOW)
    face(cv, cx, hy, eyes, mouth)
    crown(cv, cx, hy - 14 + crown_dy, 5)
    return cv, (cx, ty + 1)

# ------------------------------------------------------------------------------------------ form 2: the cracked giant
def draw_giant(dy=0, sq=0.0, armL=(-18, 16), armR=(18, 16), eyes='open', mouth='fangs', crown_dy=0, sunpulse=0.0, sunph=0.0, lean=0, cracks_=1.0, stomp=0.0, shake=0, **_):
    cv = Canvas(BW, BH)
    cx = 64 + lean + shake
    # rubble from the destroyed chair
    for (x, y, w, h) in ((12, 118, 14, 6), (100, 119, 12, 5), (24, 121, 8, 4)):
        part(cv, rect(x, y, x + w, y + h), VELVET, hi=False, k=1)
    cv.put({(14 + i, 118) for i in range(0, 12, 2)}, GOLD['base'])
    # legs
    for sx_ in (-1, 1):
        lift = stomp if sx_ > 0 else 0
        part(cv, ell(cx + sx_ * 15, 112 - lift, 14, 11), SNOW_D, hi=False)
    ty = 74 + dy
    torso = ell(cx, ty, 31 - sq, 33 + sq)
    part(cv, torso, SNOW)
    # shoulder spikes
    for sx_ in (-1, 1):
        for k in range(3):
            part(cv, tri((cx + sx_ * (22 + k * 5) - 3, ty - 22 + k * 5), (cx + sx_ * (26 + k * 6), ty - 36 + k * 6), (cx + sx_ * (22 + k * 5) + 3, ty - 20 + k * 5)), CROWN, hi=False, k=1)
    # shattered window: shards around the sun
    win = ell(cx, ty + 4, 15, 15)
    cv.put(outline_of(win), CRACK); cv.put(win, hexc('#2a2a70'))
    for k in range(8):
        a = k * TAU / 8 + .2
        cv.put(poly([(cx + C(a) * 15, ty + 4 + S(a) * 15), (cx + C(a + .25) * 21, ty + 4 + S(a + .25) * 21), (cx + C(a + .5) * 15, ty + 4 + S(a + .5) * 15)]), CROWN['base'])
    sun(cv, cx, ty + 4, 10 + sunpulse * 1.5, sunph, ray_len=4 + sunpulse * 3, bright=.8 + .2 * sunpulse)
    cracks(cv, torso, cracks_, 7, cx, ty + 4, 40)
    arm(cv, (cx - 28, ty - 8), (cx - 28 + armL[0], ty - 8 + armL[1]), thick=8.5)
    arm(cv, (cx + 28, ty - 8), (cx + 28 + armR[0], ty - 8 + armR[1]), thick=8.5)
    hy = 36 + dy
    head = ell(cx, hy, 20.5, 17.5)
    part(cv, head, SNOW)
    cracks(cv, head, cracks_ * .8, 3, cx - 6, hy + 4, 20)
    face(cv, cx, hy, eyes, mouth, angry=True)
    crown(cv, cx, hy - 14 + crown_dy, 5, broken=True)
    return cv, (cx, ty + 4)

# ------------------------------------------------------------------------------------------ form 3: the hollow storm
def funnel_w(s_, expand):
    return (31 + expand * 5) * (1 - s_) ** 0.72 + 4

def draw_storm(phase=0.0, dy=0, sunpulse=0.0, sunph=0.0, eyes='glow', crown_dy=0, spread=0.0, armL=(-26, 6), armR=(26, 6), expand=0.0, hurt=False, wobble=0.0, **_):
    """Blizzard funnel. Helical streaks are continuous curves; one phase step = one streak spacing, so the loop is seamless."""
    cv = Canvas(BW, BH)
    cx = 64
    top, bot = 28, 122
    def centre(s_): return cx + S(s_ * 5.0 + phase * TAU) * 3.2 * s_ + wobble * s_
    body = set()
    for y in range(top, bot):
        s_ = (y - top) / float(bot - top)
        w = funnel_w(s_, expand) + S(y * .9 + phase * TAU * 2) * 1.3          # ragged, flickering edge
        for x in range(int(centre(s_) - w), int(centre(s_) + w) + 1): body.add((x, y + int(dy)))
    cv.put(outline_of(body), hexc('#2a3e96'))
    for (x, y) in body:
        t = (y - top - dy) / float(bot - top)
        cv.put([(x, y)], hexc('#3a5cc0') if dither(x, y, min(1.0, t * 1.4)) else hexc('#4a74d8'))
    cv.put({(x, y) for (x, y) in body if (x + 2, y + 2) not in body}, hexc('#26388a'))
    # hollow interior: a dark cavity shaped like the missing body (head + chest)
    cav_head = ell(cx + wobble * .1, 55 + dy, 14, 11)
    cav_chest = ell(cx + wobble * .2, 82 + dy, 17, 19)
    cavity = (cav_head | cav_chest) & body
    cv.put(cavity, hexc('#141c58'))
    cv.put({p for p in cavity if (p[0] - 1, p[1] - 1) not in cavity}, hexc('#2a3a90'))
    # helical streaks, back ones first (darker), front ones on top (white)
    for layer in (0, 1):
        for k in range(10):
            prev = None
            for j in range(0, 29):
                s_ = j / 28.0
                th = phase * TAU / 10.0 * 10 / 1.0 * 0 + (phase * TAU) + s_ * 6.4 + k * TAU / 10.0
                depth = S(th)
                if (depth > 0) != (layer == 1):
                    prev = None; continue
                w = funnel_w(s_, expand) * 0.93
                x = centre(s_) + C(th) * w
                y = top + s_ * (bot - top) + dy + depth * 2.0
                if prev is not None:
                    col = (WHITE if depth > .55 else hexc('#d6ebff')) if layer == 1 else hexc('#6a94e4')
                    cv.put(line(prev[0], prev[1], x, y, 1.1 if layer == 1 and depth > .4 else .7), col)
                prev = (x, y)
    # loose snowflakes orbiting the funnel
    rng = random.Random(5)
    for k in range(14):
        a_ = rng.random() * TAU + phase * TAU * (1 if k % 2 else -1)
        r_ = 22 + rng.random() * 22 + expand * 4
        y_ = 30 + rng.random() * 80
        cv.put({(int(cx + C(a_) * r_ * (0.4 + y_ / 200.0)), int(y_ + S(a_) * 3 + dy))}, WHITE)
    face_y = 52 + dy
    if not hurt:
        for sgn in (-1, 1):
            cv.put(rect(int(cx + sgn * 8) - 4, int(face_y), int(cx + sgn * 8) + 5, int(face_y) + 3), hexc('#7af0ff'))
            cv.put(rect(int(cx + sgn * 8) - 1, int(face_y), int(cx + sgn * 8) + 2, int(face_y) + 1), WHITE)
        cv.put({(int(cx) + k, int(face_y) + 11) for k in range(-4, 5)}, hexc('#7af0ff')) if eyes == 'glow' and False else None
    else:
        for sgn in (-1, 1):
            x = int(cx + sgn * 8); cv.put({(x - 2 + i, int(face_y) - 1 + i) for i in range(5)} | {(x + 2 - i, int(face_y) - 1 + i) for i in range(5)}, hexc('#bff0ff'))
    sun(cv, cx + wobble * .2, 82 + dy, 11 + sunpulse * 1.6, sunph, rays=10, ray_len=5 + sunpulse * 3, bright=.85 + .15 * sunpulse)
    for sgn, hand in ((-1, armL), (1, armR)):
        sx, sy = cx + sgn * 22, 66 + dy
        pts = [(sx + hand[0] * t, sy + hand[1] * t + S(t * 3 + phase * TAU * 2) * 2.5) for t in [i / 7.0 for i in range(8)]]
        m = set()
        for i in range(7): m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], 3.6 - i * .2)
        part(cv, m, dict(SNOW, base=hexc('#e6f4ff'), shade=hexc('#9ab8e8'), deep=hexc('#6a88c8')), hi=False, k=2)
        fx, fy = pts[-1]
        part(cv, ell(fx, fy, 6.8, 6.2), FUR, hi=True, k=2)
    crown(cv, cx, 20 + crown_dy + dy, 5, glow=1.0 if sunpulse > .5 else 0.0)
    return cv, (cx + wobble * .2, 82 + dy)

# ------------------------------------------------------------------------------------------ animation sets
def lerp(a, b, t): return a + (b - a) * t
def ease(t): return t * t * (3 - 2 * t)

def seq_host():
    A = {}
    A['p1_idle'] = [dict(form=1, dy=round(S(i / 8 * TAU)), sq=.6 * S(i / 8 * TAU), crown_dy=round(S(i / 8 * TAU - .8)), sunph=i / 8.0, sunpulse=.5 + .5 * S(i / 8 * TAU),
                         wheelph=0.0, armL=(-12, 12 + round(S(i / 8 * TAU - 1.0))), armR=(12, 12 + round(S(i / 8 * TAU - 1.4))), eyes='closed' if i == 6 else 'open') for i in range(8)]
    A['p1_roll'] = [dict(form=1, lean=-2 - i, dy=1, sq=1, wheelph=i * .5, armL=(-14, 8), armR=(14, 8), mouth='open', eyes='open', sunph=i / 6.0, sunpulse=1 if i % 2 else .3) for i in range(6)]
    A['p1_hail'] = [dict(form=1, armL=(-14 + [0, -4, -8, -8, -4, 0][i], [12, -2, -14, -20, -10, 8][i]), armR=(14 + [0, 4, 8, 8, 4, 0][i], [12, -2, -14, -20, -10, 8][i]),
                         mouth='open' if i > 0 else 'smile', crown_dy=-i % 3, dy=-[0, 1, 2, 2, 1, 0][i], sunpulse=[.2, .5, .9, 1, .8, .4][i], sunph=i / 6.0) for i in range(6)]
    A['p1_hurt'] = [dict(form=1, dy=2, sq=2, eyes='hurt', mouth='o', armL=(-8, 8), armR=(8, 8), lean=-1, sunpulse=1, cracks_=.15),
                    dict(form=1, dy=3, sq=3, eyes='hurt', mouth='o', armL=(-6, 6), armR=(6, 6), lean=-2, sunpulse=.6, cracks_=.3)]
    A['to_p2'] = []
    for i in range(8):
        t = i / 7.0
        A['to_p2'].append(dict(form=1 if t < .55 else 2, dy=-round(3 * S(t * math.pi)), sq=2 * S(t * math.pi * 3), eyes='hurt' if t < .5 else 'open', mouth='o' if t < .55 else 'fangs',
                               armL=(-12 - 6 * t, 12 - 6 * t), armR=(12 + 6 * t, 12 - 6 * t), cracks_=min(1.0, t * 1.8), sunpulse=1, sunph=t, shake=round(S(i * 5) * 2), lean=0))
    A['p2_idle'] = [dict(form=2, dy=round(S(i / 8 * TAU)), sq=.8 * S(i / 8 * TAU), crown_dy=round(S(i / 8 * TAU - .8)), sunph=i / 8.0, sunpulse=.5 + .5 * S(i / 8 * TAU),
                         armL=(-18, 16 + round(1.5 * S(i / 8 * TAU - 1.0))), armR=(18, 16 + round(1.5 * S(i / 8 * TAU - 1.4)))) for i in range(8)]
    A['p2_stomp'] = [dict(form=2, dy=[1, 0, -3, -4, 3, 1][i], sq=[1, 0, -1, -1, 2, 1][i], stomp=[0, 4, 9, 9, 0, 0][i], armL=(-18, [16, 10, -8, -8, 22, 18][i]), armR=(18, [16, 10, -8, -8, 22, 18][i]), mouth='open', sunpulse=[.3, .5, .9, 1, 1, .5][i], sunph=i / 6.0) for i in range(6)]
    A['p2_pillars'] = [dict(form=2, dy=[0, 1, 2, 3, 2, 0][i], sq=[0, 1, 1.5, 2, 1, 0][i], armL=(-14 + [0, 0, 0, -2, -2, 0][i], [10, -6, -22, 28, 26, 16][i]), armR=(14 + [0, 0, 0, 2, 2, 0][i], [10, -6, -22, 28, 26, 16][i]), mouth='fangs', sunpulse=[.3, .6, 1, 1, .6, .4][i], sunph=i / 6.0, eyes='open') for i in range(6)]
    A['p2_summon'] = [dict(form=2, dy=-[0, 1, 2, 3, 2, 0][i], armL=(-24 + [0, -2, -4, -4, -2, 0][i], [10, -4, -12, -16, -8, 12][i]), armR=(24 + [0, 2, 4, 4, 2, 0][i], [10, -4, -12, -16, -8, 12][i]), mouth='open', sunpulse=[.3, .6, 1, 1, .8, .4][i], sunph=i / 6.0) for i in range(6)]
    A['p2_hurt'] = [dict(form=2, dy=2, sq=2, eyes='hurt', mouth='o', armL=(-14, 6), armR=(14, 6), lean=-1, sunpulse=1, cracks_=1.0),
                    dict(form=2, dy=3, sq=3, eyes='hurt', mouth='o', armL=(-12, 4), armR=(12, 4), lean=-2, sunpulse=.6, cracks_=1.0)]
    A['to_p3'] = []
    for i in range(8):
        t = i / 7.0
        A['to_p3'].append(dict(form=2 if t < .5 else 3, shake=round(S(i * 6) * 3), dy=-round(5 * S(t * math.pi)), cracks_=1.0, sq=-2 * S(t * math.pi), eyes='hurt' if t < .5 else 'glow', mouth='o',
                               armL=(-18 - 10 * t, 16 - 14 * t), armR=(18 + 10 * t, 16 - 14 * t), sunpulse=1, sunph=t, phase=t * .5, expand=t * 1.4))
    A['p3_idle'] = [dict(form=3, phase=i / 8.0, dy=round(2 * S(i / 8 * TAU)), sunph=i / 10.0, sunpulse=.5 + .5 * S(i / 8 * TAU), crown_dy=round(S(i / 8 * TAU - .8))) for i in range(8)]
    A['p3_ring'] = [dict(form=3, phase=i / 6.0 * .6, expand=[0, .6, 1.4, 2.0, 1.0, .2][i], armL=(-26 - [0, 4, 8, 10, 6, 0][i], [6, -2, -10, -12, -4, 4][i]), armR=(26 + [0, 4, 8, 10, 6, 0][i], [6, -2, -10, -12, -4, 4][i]), sunpulse=[.3, .6, 1, 1, .7, .4][i], sunph=i / 6.0, dy=-[0, 1, 2, 3, 2, 0][i]) for i in range(6)]
    A['p3_fan'] = [dict(form=3, phase=i / 6.0 * .6, expand=.6, armL=(-22 + [0, 2, 6, 8, 4, 0][i], [6, -8, -16, -4, 2, 6][i]), armR=(22 - [0, 2, 6, 8, 4, 0][i], [6, -8, -16, -4, 2, 6][i]), sunpulse=[.3, .6, 1, 1, .7, .4][i], sunph=i / 6.0, wobble=[0, 3, 6, 6, 3, 0][i]) for i in range(6)]
    A['p3_hurt'] = [dict(form=3, hurt=True, wobble=-4, dy=2, sunpulse=1, phase=.1, eyes='hurt'), dict(form=3, hurt=True, wobble=-6, dy=3, sunpulse=.6, phase=.2, eyes='hurt')]
    A['death'] = []
    n = 12
    for i in range(n):
        t = i / (n - 1.0)
        A['death'].append(dict(form=3, hurt=True, phase=t * .8, wobble=S(t * 14) * 6 * (1 - t), expand=-t * 3, dy=round(t * 18), sunpulse=1 if i % 2 else .5, sunph=t * 2, crown_dy=round(t * 26),
                               armL=(-26 + 18 * t, 6 + 24 * t), armR=(26 - 18 * t, 6 + 24 * t), fade=t))
    return A

def render(P):
    P = dict(P); form = P.pop('form'); fade = P.pop('fade', 0.0)
    if form == 1: cv, a = draw_host(**P)
    elif form == 2: cv, a = draw_giant(**P)
    else: cv, a = draw_storm(**P)
    if fade > 0:    # the storm dissolves with an ordered-dither wipe, leaving the sun and the falling crown
        for y in range(BH):
            for x in range(BW):
                if cv.px[x, y][3] and abs(x - a[0]) + abs(y - a[1]) > 14 and dither(x, y, fade * 1.15): cv.px[x, y] = (0, 0, 0, 0)
    return pad_canvas(cv, PADX, PADY, PADX, PADY), (a[0] + PADX, a[1] + PADY)

ANIMS = {'p1_idle': (10, True), 'p1_roll': (16, True), 'p1_hail': (11, False), 'p1_hurt': (10, False), 'to_p2': (9, False),
         'p2_idle': (10, True), 'p2_stomp': (11, False), 'p2_pillars': (11, False), 'p2_summon': (10, False), 'p2_hurt': (10, False), 'to_p3': (9, False),
         'p3_idle': (10, True), 'p3_ring': (11, False), 'p3_fan': (11, False), 'p3_hurt': (10, False), 'death': (9, False)}

def build_boss():
    A = seq_host(); frames = []; order = []; anchors = []
    for name, plist in A.items():
        order.append((name, len(frames), len(plist)))
        for P in plist:
            cv, a = render(P); frames.append(cv); anchors.append(a)
    return frames, order, anchors
