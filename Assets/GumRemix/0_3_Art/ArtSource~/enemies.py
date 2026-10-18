import math
from px import *

S = math.sin
WHITE = hexc('#ffffff')

def pal_of(base, hi, shade, deep, out, out_l=None):
    return dict(base=hexc(base), hi=hexc(hi), shade=hexc(shade), deep=hexc(deep), out=hexc(out), out_l=hexc(out_l or shade))

BEAR_RED = pal_of('#cf2c48', '#ff7a88', '#98203e', '#601438', '#2a0a24', '#7a1a40')
BEAR_GREEN = pal_of('#2fb070', '#9ef0b4', '#1a7a5a', '#124a54', '#08202c', '#146a58')
BEAR_ORANGE = pal_of('#e0802a', '#ffd080', '#b45222', '#783024', '#341020', '#964a24')
BEAN_YELLOW = pal_of('#e8b02c', '#ffe890', '#b87822', '#7a4228', '#34121c', '#98601c')
BEAN_PURPLE = pal_of('#9058d8', '#d0b0ff', '#5c36b0', '#38207a', '#160c3c', '#5030a0')
LOLLI_PINK = pal_of('#f0509c', '#ffb0d8', '#bc2c72', '#7c1c58', '#32082e', '#a02c68')

EYE = hexc('#2a0e2e'); GOUT = hexc('#3a0e34')

# ======================================================================= GUMMY BEAR
BW, BH = 44, 48
BG = 45

def draw_bear(pal, dy=0, sq=0, legs=((-5, 0), (5, 0)), arms=((-9, 4), (9, 4)), eyes='open', mouth=0, lean=0, hd=0, ear=0, sheen=0):
    cv = Canvas(BW, BH)
    bx, by = 22 + lean, 32 + dy
    hx, hy = 22 + lean + (1 if lean else 0), 19 + dy + hd
    for i, (ox, lift) in enumerate(legs):
        lx = bx + ox
        part(cv, line(lx, by + 5, lx + (1 if i else -1) * 0, BG - lift - 1, 3.2) | ell(lx + 1, BG - lift - 1, 3.8, 3.0), pal, hi=False)
    body = ell(bx, by, 9.2 + sq * .8, 8 - sq * .8)
    part(cv, body, pal)
    # far arm
    ax, ay = bx - 8, by - 3
    part(cv, ell(ax + arms[0][0] * .5, ay + arms[0][1] * .5, 3.2, 3.2) | ell(ax + arms[0][0] * .35, ay + arms[0][1] * .8, 2.6, 2.6), pal, hi=False)
    # head + ears
    ears = ell(hx - 7.5, hy - 8 + ear, 3.4, 3.4) | ell(hx + 7.5, hy - 8 + ear, 3.4, 3.4)
    part(cv, ears, pal, hi=False)
    part(cv, ell(hx - 7.5, hy - 8 + ear, 1.6, 1.6) | ell(hx + 7.5, hy - 8 + ear, 1.6, 1.6), dict(pal, base=pal['shade'], out=pal['shade']), outline=False, shade=False)
    part(cv, ell(hx, hy, 9.4, 8.2), pal)
    # glossy gummy sheen: short bright streak + dot, flickers with the sheen param
    cv.put({(hx - 5, hy - 4 + i) for i in range(4)}, hexc('#ffd8d0') if pal is BEAR_RED else pal['hi'])
    cv.put({(hx - 5, hy - 5)}, WHITE)
    cv.put({(bx - 5, by - 3 + i) for i in range(5 + sheen)}, pal['hi'])
    # snout
    sn = ell(hx + 3, hy + 3.5, 4.6, 3.2)
    cv.put(sn, hexc('#ffd0b0')); cv.put({p for p in sn if (p[0], p[1]+1) not in sn}, hexc('#e8a888'))
    cv.put({(hx + 5, hy + 2), (hx + 6, hy + 2)}, hexc('#4a1a2e'))
    # eyes + angry brows
    ey = hy - 3
    for ex, ew in ((hx - 5, 3), (hx + 2, 3)):
        if eyes == 'open':
            cv.put(rect(ex, ey, ex + ew, ey + 3), EYE); cv.put({(ex, ey)}, WHITE)
        elif eyes == 'hurt':
            cv.put({(ex, ey), (ex + 1, ey + 1), (ex + 2, ey + 2), (ex + 2, ey), (ex, ey + 2)}, EYE)
        else:
            cv.put(rect(ex, ey + 1, ex + ew, ey + 2), EYE)
    cv.put({(hx - 6, hy - 6), (hx - 5, hy - 5), (hx - 4, hy - 5), (hx - 3, hy - 4), (hx + 4, hy - 6), (hx + 3, hy - 5), (hx + 2, hy - 5), (hx + 1, hy - 4)} if eyes == 'open' else set(), GOUT)
    if mouth == 1: cv.put({(hx + 3, hy + 5), (hx + 4, hy + 5), (hx + 5, hy + 5)}, hexc('#b02848'))
    if mouth == 2:
        cv.put(rect(hx + 1, hy + 4, hx + 7, hy + 7), hexc('#4a0e2e')); cv.put({(hx + 2, hy + 4), (hx + 4, hy + 4), (hx + 6, hy + 4)}, WHITE)
    # near arm
    ax, ay = bx + 8, by - 3
    part(cv, ell(ax + arms[1][0] * .35, ay + arms[1][1] * .6, 3.4, 3.4), pal, hi=False)
    return cv

def bear_frames(pal):
    F = {}
    F['idle'] = []
    for i in range(6):
        t = i / 6 * 2 * math.pi
        F['idle'].append(draw_bear(pal, dy=round(.6 + .6 * S(t)), hd=round(.7 * S(t - .7)), arms=((-9, 4 + round(S(t))), (9, 4 + round(S(t)))), ear=round(.8 * S(t - 1.2)), eyes='blink' if i == 4 else 'open', sheen=i % 2))
    F['walk'] = []
    for i in range(8):
        p = i / 8 * 2 * math.pi
        lift = lambda ph: max(0, round(3.2 * math.sin(ph + 1.57)))
        F['walk'].append(draw_bear(pal, dy=-round(1.5 * abs(math.cos(p))) + 1, legs=((round(4 * S(p + math.pi)) - 5, lift(p + math.pi)), (round(4 * S(p)) + 5, lift(p))),
                                   arms=((round(4 * S(p)) - 9, 4), (round(4 * S(p + math.pi)) + 9, 4)), lean=1, hd=round(.8 * S(p - .8)), ear=round(1.0 * S(p - 1.4)), mouth=1 if i in (2, 6) else 0))
    F['lunge'] = [draw_bear(pal, dy=3, sq=2.4, legs=((-7, 0), (7, 0)), arms=((-9, 6), (9, 6)), hd=2, eyes='open', lean=-2),
                  draw_bear(pal, dy=-1, sq=-1.5, legs=((-2, 0), (9, 2)), arms=((-8, -2), (12, -1)), lean=4, hd=0, mouth=2),
                  draw_bear(pal, dy=0, sq=-.5, legs=((0, 0), (8, 0)), arms=((-8, 0), (12, 0)), lean=5, hd=0, mouth=2),
                  draw_bear(pal, dy=2, sq=1.2, legs=((-6, 0), (6, 0)), arms=((-9, 5), (9, 5)), lean=1, hd=1)]
    F['hurt'] = [draw_bear(pal, dy=1, sq=1, eyes='hurt', mouth=2, lean=-2, hd=1, legs=((-6, 0), (6, 0))),
                 draw_bear(pal, dy=2, sq=2, eyes='hurt', mouth=2, lean=-3, hd=2, legs=((-7, 0), (7, 0)))]
    return F

BEAR_ANIMS = dict(idle=(8, True), walk=(14, True), lunge=(12, False), hurt=(10, False))

# ======================================================================= JELLY BEAN
JW, JH = 40, 40
JG = 38

def draw_bean(pal, sx=1.0, sy=1.0, dy=0, tilt=0, eyes='open', mouth='smile', speck=0):
    cv = Canvas(JW, JH)
    cx, cy = 20, JG - 9 * sy + dy
    ln = 5.5 * sy
    # capsule tilted slightly (lean toward direction of travel)
    body = line(cx - tilt - 1, cy + ln * .5, cx + tilt + 1, cy - ln * .5, 8.4 * sx) | ell(cx, cy, 8.4 * sx, 9.5 * sy)
    part(cv, body, pal)
    cv.put({(cx - 5 + i, cy - 7 + (i // 2)) for i in range(3)}, pal['hi'])
    cv.put({(cx - 6, cy - 5), (cx - 6, cy - 4)}, WHITE)
    # sprinkles speckle
    for (sx_, sy_, c) in ((-3, 5, '#ffffff'), (4, 4, '#ff7ab8'), (-5, 1, '#ff7ab8'), (5, -4, '#ffffff'), (1, 8, '#3a0e5a')):
        if (sx_ + sy_ + speck) % 3 != 0: cv.put({(int(cx + sx_ * sx), int(cy + sy_ * sy)), (int(cx + sx_ * sx) + 1, int(cy + sy_ * sy))}, hexc(c))
    ey = int(cy - 2 * sy)
    for ex in (int(cx - 4), int(cx + 2)):
        if eyes == 'open':
            cv.put(rect(ex, ey, ex + 3, ey + 4), EYE); cv.put({(ex, ey), (ex, ey + 1)}, WHITE)
        elif eyes == 'closed': cv.put(rect(ex, ey + 2, ex + 3, ey + 3), EYE)
        elif eyes == 'hurt': cv.put({(ex, ey), (ex + 2, ey), (ex + 1, ey + 1), (ex, ey + 2), (ex + 2, ey + 2)}, EYE)
    my = ey + 6
    if mouth == 'smile': cv.put({(cx - 1, my), (cx, my + 1), (cx + 1, my + 1), (cx + 2, my)}, EYE)
    elif mouth == 'open': cv.put(rect(cx - 1, my, cx + 3, my + 3), EYE); cv.put({(cx, my + 2), (cx + 1, my + 2)}, hexc('#ff6a8a'))
    elif mouth == 'o': cv.put(rect(cx, my, cx + 2, my + 2), EYE)
    return cv

def bean_frames(pal):
    F = {}
    F['idle'] = []
    for i in range(6):
        t = i / 6 * 2 * math.pi
        F['idle'].append(draw_bean(pal, sx=1 + .05 * S(t), sy=1 - .06 * S(t), eyes='closed' if i == 4 else 'open', speck=i // 2))
    F['crouch'] = [draw_bean(pal, sx=1.12, sy=.82, mouth='smile'), draw_bean(pal, sx=1.22, sy=.7, eyes='closed', mouth='open')]
    F['air'] = [draw_bean(pal, sx=.86, sy=1.2, tilt=2, mouth='open', dy=-2), draw_bean(pal, sx=.94, sy=1.1, tilt=1, mouth='open', dy=-2),
                draw_bean(pal, sx=1.0, sy=1.0, tilt=-1, mouth='o', dy=-1)]
    F['land'] = [draw_bean(pal, sx=1.25, sy=.72, eyes='closed'), draw_bean(pal, sx=1.1, sy=.9)]
    F['hurt'] = [draw_bean(pal, sx=1.1, sy=.86, eyes='hurt', mouth='o', tilt=-2), draw_bean(pal, sx=1.18, sy=.78, eyes='hurt', mouth='o', tilt=-3)]
    return F
BEAN_ANIMS = dict(idle=(8, True), crouch=(12, False), air=(10, False), land=(14, False), hurt=(10, False))

# ======================================================================= LOLLIPOP FLYER
LW, LH = 64, 60

def spiral_disc(cv, cx, cy, r, phase, ca, cb):
    m = ell(cx, cy, r, r)
    for (x, y) in m:
        dx, dy = x + .5 - cx, y + .5 - cy
        th = math.atan2(dy, dx); rr = math.hypot(dx, dy)
        s = (th / (2 * math.pi) * 2 + rr / 4.2 + phase) % 1.0
        cv.put([(x, y)], ca if s < .5 else cb)
    return m

def draw_lolli(pal, i, n=8, hurt=False, mood='angry'):
    cv = Canvas(LW, LH)
    t = i / n * 2 * math.pi
    cx, cy = 32, 24 + round(1.2 * S(t))
    r = 13
    # wrapper-paper wings flap
    flap = S(t)
    for side in (-1, 1):
        wy = cy - 4 - flap * 6
        tip = (cx + side * (24 + abs(flap) * 1), wy - 6 + flap * 3)
        w = poly([(cx + side * 9, cy - 2), tip, (cx + side * 20, cy + 6 - flap * 3), (cx + side * 8, cy + 6)])
        cv.put(outline_of(w), hexc('#4a3a98'))
        for p in w:
            cv.px[p[0], p[1]] = hexc('#d8f0ff') if (p[0] + p[1]) % 5 else hexc('#ffffff')
        cv.put({p for p in w if (p[0] - side, p[1] + 1) not in w}, hexc('#98c8f0'))
    # stick + bow with wobble
    sway = round(2 * S(t - .8))
    stick = line(cx, cy + r - 2, cx + sway, cy + r + 18, 1.6)
    part(cv, stick, dict(base=hexc('#fff0d6'), shade=hexc('#e8c8a0'), deep=hexc('#c8a080'), hi=WHITE, out=hexc('#401040')), hi=False)
    bow = ell(cx - 3 + sway, cy + r + 4, 3, 2.2) | ell(cx + 3 + sway, cy + r + 4, 3, 2.2)
    part(cv, bow, dict(base=hexc('#8a6af0'), shade=hexc('#5a3ac0'), deep=hexc('#3a2a98'), hi=hexc('#c0b0ff'), out=hexc('#201060')), hi=False)
    # spiral head
    cv.put(outline_of(ell(cx, cy, r, r)), pal['out'])
    ca, cb = (hexc('#ff6aa8'), hexc('#fff0f6')) if not hurt else (hexc('#ffffff'), hexc('#ffc0dc'))
    m = spiral_disc(cv, cx, cy, r, -i / n * .5, ca, cb)
    cv.put({p for p in m if (p[0] + 2, p[1] + 2) not in m}, hexc('#c0407a'))
    cv.put({p for p in m if (p[0] + 1, p[1] + 1) not in m and p[0] > cx and p[1] > cy}, hexc('#802060'))
    cv.put({(cx - 8 + k, cy - 9 + (k // 2)) for k in range(4)}, WHITE)
    # face sticker (does not spin)
    face = ell(cx + 1, cy + 1, 7.4, 6.2)
    cv.put(outline_of(face), hexc('#401040')); cv.put(face, hexc('#fff0d6'))
    cv.put({p for p in face if (p[0], p[1] + 1) not in face}, hexc('#f4cfa6'))
    for ex in (cx - 4, cx + 2):
        if hurt:
            cv.put({(ex, cy - 3), (ex + 2, cy - 3), (ex + 1, cy - 2), (ex, cy - 1), (ex + 2, cy - 1)}, EYE)
        else:
            cv.put(rect(ex, cy - 3, ex + 3, cy + 1), EYE); cv.put({(ex, cy - 3)}, WHITE)
    if not hurt: cv.put({(cx - 5, cy - 5), (cx - 4, cy - 5), (cx - 3, cy - 4), (cx + 4, cy - 5), (cx + 3, cy - 5), (cx + 2, cy - 4)}, hexc('#401040'))
    cv.put(rect(cx - 2, cy + 3, cx + 4, cy + 5), hexc('#b02848')); cv.put({(cx - 1, cy + 3), (cx + 1, cy + 3), (cx + 3, cy + 3)}, WHITE)
    return cv

def lolli_frames(pal):
    return dict(fly=[draw_lolli(pal, i) for i in range(8)], hurt=[draw_lolli(pal, 0, hurt=True), draw_lolli(pal, 2, hurt=True)])
LOLLI_ANIMS = dict(fly=(14, True), hurt=(10, False))

# ======================================================================= SUGAR CANNON
CW, CH = 56, 48
SUGAR = pal_of('#fff6ea', '#ffffff', '#e8d0c0', '#c0a0a8', '#4a2038', '#c8b0b8')
BARREL = pal_of('#ff7ab0', '#ffc0dc', '#d84a8a', '#982870', '#401040', '#b8407a')

def draw_cannon(recoil=0, bob=0, flash=0, hurt=False, wheel=0):
    cv = Canvas(CW, CH)
    # wheels
    for wx in (14, 36):
        w = ell(wx, 41, 6, 6); part(cv, w, pal_of('#8a5a3a', '#c89868', '#6a3a2a', '#4a2a28', '#2a1018'), hi=False)
        for k in range(4):
            a = wheel * .5 + k * math.pi / 2
            cv.put(line(wx, 41, wx + math.cos(a) * 4, 41 + math.sin(a) * 4, .5), hexc('#e8c898'))
        cv.put(ell(wx, 41, 1.5, 1.5), hexc('#2a1018'))
    # barrel (striped candy cane tube), aimed up-right
    ang = math.radians(-48)
    bx0, by0 = 24, 24 + bob
    L = 20 - recoil
    ex, ey = bx0 + math.cos(ang) * L, by0 + math.sin(ang) * L
    tube = line(bx0, by0, ex, ey, 6.2)
    cv.put(outline_of(tube), BARREL['out'])
    for (x, y) in tube:
        d = ((x - bx0) * math.cos(ang) + (y - by0) * math.sin(ang))
        cv.put([(x, y)], BARREL['base'] if int((d + 40) // 4) % 2 == 0 else hexc('#fff0f6'))
    cv.put({p for p in tube if (p[0] + 2, p[1] + 2) not in tube}, BARREL['shade'])
    rim = ell(ex, ey, 6.6, 6.6)
    cv.put(outline_of(rim), BARREL['out']); cv.put(rim, BARREL['deep'])
    cv.put(ell(ex + 1, ey + 1, 4, 4), hexc('#2a0a28'))
    if flash:
        cv.put(ell(ex + 3, ey - 3, 4 + flash, 4 + flash), hexc('#ffe36a')); cv.put(ell(ex + 3, ey - 3, 2 + flash, 2 + flash), WHITE)
    # sugar-cube base with a grumpy face
    base = rect(8, 24 + bob, 42, 40) | rect(10, 22 + bob, 40, 24 + bob)
    part(cv, base, SUGAR)
    cv.put({(x, 28 + bob + (x % 3 == 0)) for x in range(13, 37, 3)}, hexc('#ffffff'))
    for ex_ in (17, 28):
        if hurt: cv.put({(ex_, 29 + bob), (ex_ + 2, 29 + bob), (ex_ + 1, 30 + bob), (ex_, 31 + bob), (ex_ + 2, 31 + bob)}, EYE)
        else: cv.put(rect(ex_, 29 + bob, ex_ + 3, 33 + bob), EYE); cv.put({(ex_, 29 + bob)}, WHITE)
    cv.put({(15, 27 + bob), (16, 27 + bob), (17, 28 + bob), (29, 28 + bob), (30, 27 + bob), (31, 27 + bob)}, hexc('#4a2038'))
    cv.put(rect(21, 36 + bob, 27, 38 + bob), hexc('#b02848')); cv.put({(22, 36 + bob), (24, 36 + bob), (26, 36 + bob)}, WHITE)
    return cv

def cannon_frames():
    F = {}
    F['idle'] = [draw_cannon(bob=round(.6 + .6 * S(i / 4 * 2 * math.pi)), wheel=i) for i in range(4)]
    F['fire'] = [draw_cannon(recoil=0, flash=0), draw_cannon(recoil=1, flash=0, bob=1), draw_cannon(recoil=0, flash=3), draw_cannon(recoil=4, flash=1, bob=1),
                 draw_cannon(recoil=3, bob=1), draw_cannon(recoil=1), draw_cannon(recoil=0, bob=1)]
    F['hurt'] = [draw_cannon(hurt=True, bob=1, recoil=2), draw_cannon(hurt=True, bob=2, recoil=3)]
    return F
CANNON_ANIMS = dict(idle=(6, True), fire=(16, False), hurt=(10, False))

# ======================================================================= GUM BALL (a captured enemy)
GW = GH = 48

def draw_ball(inner, i=0, mode='wobble', n=8):
    cv = Canvas(GW, GH)
    cx, cy = 24, 26
    t = i / n * 2 * math.pi
    sx, sy, dy = 1.0, 1.0, 0
    if mode == 'wobble': sx, sy = 1 + .05 * S(t * 2), 1 - .05 * S(t * 2)
    if mode == 'shake': sx = 1 + .07 * S(t * 4); dy = round(S(t * 3))
    r = 15
    m = ell(cx + (round(S(t * 6)) if mode == 'shake' else 0), cy + dy + (15 - r * sy), r * sx, r * sy)
    P = pal_of('#ff8ac8', '#ffd0ea', '#e0589e', '#a03a88', '#4a1050', '#c850a0')
    part(cv, m, P)
    ax, ay = cx, cy + dy
    # enemy trapped inside: darker translucent silhouette + eyes that rotate when rolling
    ang = (i / n * 2 * math.pi) if mode == 'roll' else 0
    inner_mask = ell(ax, ay + 1, 9, 8.5)
    cv.put(inner_mask, mix(inner['base'][:3], hexc('#ff8ac8'), .55))
    cv.put({p for p in inner_mask if (p[0] + 2, p[1] + 2) not in inner_mask}, mix(inner['shade'][:3], hexc('#e0589e'), .5))
    for k, off in enumerate((-3.2, 3.2)):
        px_ = ax + math.cos(ang) * off - math.sin(ang) * -1; py_ = ay + math.sin(ang) * off + math.cos(ang) * -1
        ex_, ey_ = int(px_), int(py_)
        cv.put({(ex_, ey_ - 1), (ex_ + 1, ey_ - 1), (ex_, ey_), (ex_ + 1, ey_), (ex_, ey_ + 1), (ex_ + 1, ey_ + 1)}, EYE)
        cv.put({(ex_, ey_ - 1)}, WHITE)
        if mode != 'roll': cv.put({(ex_ - k_, ey_ - 3 + (1 if k == 0 else 0) * 0 + (k_ if k == 0 else 1 - k_)) for k_ in (0, 1)}, EYE)
    # rolling streak that rotates around the ball (sells the rotation)
    if mode == 'roll':
        for k in range(5):
            a = ang * 1.0 + k * .22
            cv.put({(int(cx + math.cos(a) * 12.5), int(cy + math.sin(a) * 12.5))}, WHITE)
    else:
        cv.put({(cx - 9 + k, cy - 11 + (k // 2)) for k in range(4)}, WHITE)
    if mode == 'shake': cv.put({(cx - 15, cy - 11), (cx + 15, cy - 9), (cx - 16, cy + 3)}, WHITE)
    return cv

def draw_pop(i, n=7):
    cv = Canvas(GW, GH)
    cx, cy = 24, 26
    t = i / (n - 1)
    pcol = [hexc('#ff8ac8'), hexc('#ffd0ea'), hexc('#e0589e'), hexc('#ffffff')]
    if t < .25:
        r = 15 + t * 24
        m = ell(cx, cy, r, r * .9); cv.put(outline_of(m), hexc('#4a1050')); cv.put(m, hexc('#ff8ac8')); cv.put(ring(m, 0), hexc('#ffd0ea'))
        cv.put(ell(cx, cy, r - 5, (r - 5) * .9), hexc('#ffb8dc'))
    else:
        k = (t - .25) / .75
        for j in range(10):
            a = j / 10 * 2 * math.pi + .3
            d = 8 + k * 20 + (j % 3) * 2
            size = max(1, 4 - k * 4 + (j % 2))
            cv.put(ell(cx + math.cos(a) * d, cy + math.sin(a) * d * .9 + k * 5, size, size), pcol[j % 4])
        if k < .5:
            cv.put(ring(ell(cx, cy, 8 + k * 24, 7 + k * 22)), hexc('#ffd0ea'))
    return cv

def ball_frames(inner):
    F = {}
    F['wobble'] = [draw_ball(inner, i, 'wobble', 6) for i in range(6)]
    F['shake'] = [draw_ball(inner, i, 'shake', 6) for i in range(6)]
    F['roll'] = [draw_ball(inner, i, 'roll', 8) for i in range(8)]
    F['pop'] = [draw_pop(i, 7) for i in range(7)]
    return F
BALL_ANIMS = dict(wobble=(8, True), shake=(14, True), roll=(18, True), pop=(18, False))
