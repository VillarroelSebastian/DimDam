import math
from px import *

W, H = 48, 60
G = 55                      # boot centre line; boot bottom lands on y=53 (pivot sits 2px above cell bottom)
PIVOT_Y = 2 / H

CREAM, CREAM_S, CREAM_D = hexc('#fff0d6'), hexc('#f4cfa6'), hexc('#d9a47e')
WHITE = hexc('#ffffff')

CELESTE = dict(hi=hexc('#b6f0ff'), base=hexc('#4aa9f4'), shade=hexc('#3566d8'), deep=hexc('#2b3fa6'),
               out=hexc('#161650'), out_l=hexc('#2a4fa8'), eye=hexc('#3a0f3c'), eye2=hexc('#c23a6e'),
               hornA=hexc('#ff8a72'), hornB=hexc('#ffdcb8'), horn_sh=hexc('#d9564a'),
               gem=hexc('#ff5468'), gem_hi=hexc('#ffc8c4'), gem_dk=hexc('#a01c46'),
               mouth=hexc('#3a0f3c'), tongue=hexc('#ff7d92'), cheek=hexc('#ff9fb4'), fin=hexc('#7fd0ff'))
CORAL = dict(hi=hexc('#ffb09a'), base=hexc('#f04e50'), shade=hexc('#c42c4a'), deep=hexc('#862048'),
             out=hexc('#3a0e34'), out_l=hexc('#a02848'), eye=hexc('#3a0a2a'), eye2=hexc('#ef4a68'),
             hornA=hexc('#fff3dc'), hornB=hexc('#f04e50'), horn_sh=hexc('#d9b490'),
             gem=hexc('#38d2ff'), gem_hi=hexc('#d8f8ff'), gem_dk=hexc('#1a78c4'),
             mouth=hexc('#3a0a2a'), tongue=hexc('#ff7d92'), cheek=hexc('#ff9fb4'), fin=hexc('#ff8a7a'))

def pose(**k):
    d = dict(dx=0, dy=0, sq=0, hdx=0, hdy=0, eyes='open', mouth='smile', fn=(4, 0), ff=(-4, 0),
             an=(4, 7), af=(-4, 7), sway=0, tail=0, near_arm=True, gem=0)
    d.update(k); return d

def horn(cv, pts, pal):
    m = set(); seg = []
    for i, (x, y, r) in enumerate(pts): m |= ell(x, y, r, r); seg.append((x, y, i))
    cv.put(outline_of(m), pal['out'])
    for p in m:
        j = min(seg, key=lambda s: (s[0]-p[0]-.5)**2 + (s[1]-p[1]-.5)**2)[2]
        cv.px[p[0], p[1]] = pal['hornA'] if (j // 2) % 2 == 0 else pal['hornB']
    cv.put({p for p in m if (p[0]+1, p[1]+1) not in m}, pal['horn_sh'])

def ribbon(cv, samples, ca, cb, shade, outline):
    """Continuous striped band through (x, y, radius) samples."""
    m = set(); owner = {}
    for i, (x, y, r) in enumerate(samples):
        c = ell(x, y, r, r * 0.92)
        for p in c: owner.setdefault(p, i)
        m |= c
    cv.put(outline_of(m), outline)
    for p in m:
        i = owner[p]
        cv.put([p], ca if (i // 4) % 2 == 0 else cb)
    cv.put({p for p in m if (p[0] + 1, p[1] + 1) not in m}, shade)

def draw_hero(pal, kind, P, winter=False):
    cv = Canvas(W, H)
    sq = P['sq']; sway = P['sway']
    bx, by = 23 + P['dx'], 43 + P['dy']
    hx, hy = 23 + P['dx'] + P['hdx'], 30 + P['dy'] + P['hdy']
    shoulder_n = (bx + 5, by - 3)
    mit = dict(pal, base=hexc('#f4fbff'), hi=hexc('#ffffff'), shade=hexc('#bcd8f0'), deep=hexc('#8aa8d0')) if winter else None
    scarf_a, scarf_b = (hexc('#ffb347'), hexc('#fff0c8')) if kind == 'celeste' else (hexc('#6fe0ff'), hexc('#ffffff'))
    scarf_s = hexc('#d9772e') if kind == 'celeste' else hexc('#3a9ad0')

    # far arm
    sx, sy = bx - 5, by - 3
    part(cv, line(sx, sy, sx + P['af'][0], sy + P['af'][1], 2.0), pal, hi=False)
    part(cv, ell(sx + P['af'][0], sy + P['af'][1], 2.9 if winter else 2.7, 2.9 if winter else 2.7), mit or pal, hi=False)
    if winter:
        cv.put({(int(sx + P['af'][0]) - 2 + k, int(sy + P['af'][1]) - 2) for k in range(5)}, scarf_a)
    # legs / boots
    for i, (ox, lift) in enumerate((P['ff'], P['fn'])):
        hipx, hipy = bx + (-3 if i == 0 else 3), by + 5
        ax, ay = bx + ox, G - lift - 2
        part(cv, line(hipx, hipy, ax, ay, 2.2), pal, hi=False)
        cuff = dict(pal, base=CREAM, shade=CREAM_S, deep=CREAM_D, hi=CREAM, out=pal['out'], out_l=pal['out'])
        part(cv, rect(ax - 2, ay - 2, ax + 3, ay), cuff, hi=False)
        part(cv, ell(ax + 1, G - lift, 4.4, 2.6 - max(0, sq) * .3), pal, hi=False)
    # tail
    t = P['tail']
    if kind == 'coral':
        pts = [(bx - 7, by + 3), (bx - 10, by - 0 + t), (bx - 12, by - 5 + t * 1.5), (bx - 11, by - 9 + t * 2)]
        m = set()
        for i, (x, y) in enumerate(pts): m |= ell(x, y, 2.6 - i * .35, 2.6 - i * .35)
        part(cv, m, dict(pal, base=pal['hornA'], shade=CREAM_S, deep=CREAM_D, hi=WHITE))
    else:
        part(cv, ell(bx - 8, by + 3 + t, 2.8, 2.8), pal)
    # horns, fins/ears
    for ox, big in ((-1, .85), (1, 1.0)):
        if kind == 'celeste': ctl = [(6, -7, 3.6), (9, -11, 3.2), (11, -15, 2.6), (10.5, -18.5, 1.8), (8.5, -20.5, 1.1)]
        else: ctl = [(4.5, -8, 3.4), (6.5, -12, 2.9), (8, -16, 1.9), (8.5, -18.5, 1.1)]
        pts = []
        for i in range(len(ctl) - 1):
            for tt in (0, .5):
                (x0_, y0_, r0), (x1_, y1_, r1) = ctl[i], ctl[i+1]
                pts.append((hx + ox * (x0_ + (x1_ - x0_) * tt) + (sway * ox * .35 if i > 1 else 0), hy + y0_ + (y1_ - y0_) * tt, (r0 + (r1 - r0) * tt) * big))
        horn(cv, pts, pal)
    for ox in (-1, 1):
        if kind == 'celeste':
            part(cv, tri((hx + ox*9, hy - 2), (hx + ox*15, hy - 4 + sway), (hx + ox*9, hy + 5)) | ell(hx + ox*10, hy + 1, 3, 3.5), dict(pal, base=pal['fin']), hi=False)
        else:
            part(cv, tri((hx + ox*7, hy - 5), (hx + ox*14, hy - 9 + sway), (hx + ox*11, hy + 1)), pal, hi=False)
    if winter:
        # trailing scarf: one continuous ribbon, striped by arc length. It follows the same sine curves as horns and tail,
        # with more lag toward the tip, so it whips instead of snapping.
        run = 1.0 if P['dx'] > 0 else 0.0
        L = 8.0 + run * 3.0 - (3.0 if P['dx'] < 0 else 0.0)
        samples = []
        n = 16
        for k in range(n):
            t = k / (n - 1.0)
            x = hx - 5.5 - t * L
            y = hy + 9.0 + t * 2.4 + (P['tail'] * 0.8 + P['sway'] * 0.3) * t * t * 1.4 + math.sin(t * 4.5 + P['tail'] * 0.5) * 0.9 * t
            samples.append((x, y, 2.7 - 0.9 * t))
        ribbon(cv, samples, scarf_a, scarf_b, scarf_s, pal['out'])
    # body, belly, gem
    part(cv, ell(bx, by, 6.8 + sq * .7, 6 - sq * .7), pal)
    belly = ell(bx + 1, by + 1, 3.8, 4.6)
    cv.put(belly, CREAM); cv.put({p for p in belly if (p[0], p[1]+1) not in belly}, CREAM_S)
    gx, gy = bx + 1, by
    cv.put({(gx, gy-2), (gx-1, gy-1), (gx, gy-1), (gx+1, gy-1), (gx-2, gy), (gx-1, gy), (gx, gy), (gx+1, gy), (gx+2, gy), (gx-1, gy+1), (gx, gy+1), (gx+1, gy+1), (gx, gy+2)}, pal['gem'])
    cv.put({(gx-1, gy-1), (gx, gy-2)}, pal['gem_hi']); cv.put({(gx+1, gy+1), (gx+2, gy), (gx, gy+2)}, pal['gem_dk'])
    if P['gem'] == 1: cv.put({(gx - 2, gy), (gx - 1, gy - 1)}, WHITE)
    if P['gem'] == 2: cv.put({(gx, gy), (gx - 1, gy), (gx, gy - 1)}, WHITE)
    if winter:
        ring = ell(hx + 0.5, hy + 8.0, 9.4, 3.1)
        part(cv, ring, dict(pal, base=scarf_a, shade=scarf_s, deep=scarf_s, hi=scarf_b, out=pal['out'], out_l=pal['out']), hi=False)
        cv.put({(hx - 7 + 3 * k + (0), hy + 7 + (k % 2)) for k in range(6)} & ring, scarf_b)
        end = poly([(hx + 3, hy + 9), (hx + 8, hy + 9), (hx + 7, hy + 15), (hx + 3, hy + 14)])
        part(cv, end, dict(pal, base=scarf_b, shade=scarf_a, deep=scarf_s, hi=WHITE, out=pal['out'], out_l=pal['out']), hi=False)
        cv.put({(hx + 4 + k, hy + 12) for k in range(3)}, scarf_a)
    # head
    head = ell(hx, hy, 10.2 + sq * .6, 8.8 - sq * .6)
    if kind == 'coral': head |= tri((hx - 2, hy - 8), (hx + 1, hy - 12), (hx + 3, hy - 8))
    part(cv, head, pal)
    if winter:
        for ox in (-1, 1):
            pad = ell(hx + ox * 10.2, hy + 1.5, 3.7, 4.0)
            part(cv, pad, dict(pal, base=hexc('#fff4fa'), hi=WHITE, shade=hexc('#f0c0d8'), deep=hexc('#c890b8')), hi=True)
            cv.put({(hx + ox * 9 + (0 if ox > 0 else -1), hy - 3 + k) for k in range(4)}, hexc('#6a5a8a'))   # band going up to the horns
    muz = ell(hx + 5.5, hy + 4.5, 4.8, 3.3)
    cv.put(muz, CREAM); cv.put({p for p in muz if (p[0], p[1]+1) not in muz}, CREAM_S)
    cv.put({(hx + 9, hy + 3)}, pal['eye'])               # nose
    # eyes: far eye narrower than near eye (3/4 view)
    ey = hy - 4
    for (ex, ew) in ((hx - 4, 3), (hx + 4, 4)):
        e = P['eyes']
        if e == 'open':
            cv.put(rect(ex, ey, ex + ew, ey + 6), pal['eye'])
            cv.put({(ex, ey), (ex + ew - 1, ey)}, pal['out'])
            cv.put(rect(ex + 1, ey + 4, ex + ew, ey + 6), pal['eye2'])
            cv.put({(ex, ey + 1), (ex, ey + 2), (ex + 1, ey + 1)}, WHITE)
            cv.put({(ex + ew - 1, ey + 4)}, WHITE)
        elif e == 'half':
            cv.put(rect(ex, ey + 2, ex + ew, ey + 6), pal['eye']); cv.put(rect(ex, ey + 2, ex + ew, ey + 3), pal['out'])
            cv.put({(ex, ey + 3)}, WHITE)
        elif e == 'closed':
            cv.put(rect(ex, ey + 4, ex + ew, ey + 5), pal['eye'])
        elif e == 'happy':
            cv.put({(ex, ey + 4), (ex + 1, ey + 3), (ex + 2, ey + 3), (ex + ew - 1, ey + 4)}, pal['eye'])
        elif e == 'hurt':
            cv.put({(ex, ey + 1), (ex + 1, ey + 2), (ex + 2, ey + 3), (ex, ey + 5), (ex + 1, ey + 4), (ex + 2, ey + 3), (ex + ew - 1, ey + 1), (ex + ew - 1, ey + 5)}, pal['eye'])
        elif e == 'focus':
            cv.put(rect(ex, ey + 1, ex + ew, ey + 6), pal['eye']); cv.put(rect(ex, ey + 1, ex + ew, ey + 2), pal['out'])
            cv.put(rect(ex + 1, ey + 4, ex + ew, ey + 6), pal['eye2']); cv.put({(ex, ey + 2), (ex + 1, ey + 2)}, WHITE)
    if kind == 'coral' or P['eyes'] == 'focus':
        cv.put({(hx - 5, hy - 6), (hx - 4, hy - 6), (hx - 3, hy - 5), (hx + 4, hy - 5), (hx + 5, hy - 6), (hx + 6, hy - 6), (hx + 7, hy - 6)}, pal['deep'])
    cv.put({(hx - 7, hy + 2), (hx - 6, hy + 2), (hx - 7, hy + 3), (hx - 6, hy + 3)}, pal['cheek'])
    mx, my = hx + 4, hy + 4
    m = P['mouth']
    if m == 'smile': cv.put({(mx, my), (mx + 1, my + 1), (mx + 2, my + 1), (mx + 3, my)}, pal['mouth'])
    elif m == 'open':
        cv.put(rect(mx, my, mx + 5, my + 3), pal['mouth']); cv.put({(mx + 1, my + 2), (mx + 2, my + 2), (mx + 3, my + 2)}, pal['tongue'])
    elif m == 'o': cv.put(rect(mx + 1, my, mx + 4, my + 3), pal['mouth'])
    elif m == 'grin': cv.put({(mx, my), (mx + 1, my + 1), (mx + 2, my + 1), (mx + 3, my + 1), (mx + 4, my)}, pal['mouth']); cv.put({(mx + 1, my + 2), (mx + 2, my + 2)}, CREAM)
    elif m == 'sad': cv.put({(mx, my + 1), (mx + 1, my), (mx + 2, my), (mx + 3, my + 1)}, pal['mouth'])
    # near arm
    if P['near_arm']:
        sx, sy = shoulder_n
        part(cv, line(sx, sy, sx + P['an'][0], sy + P['an'][1], 2.0), pal, hi=False)
        part(cv, ell(sx + P['an'][0], sy + P['an'][1], 2.9 if winter else 2.7, 2.9 if winter else 2.7), mit or pal, hi=False)
        if winter:
            cv.put({(int(sx + P['an'][0]) - 2 + k, int(sy + P['an'][1]) - 2) for k in range(5)}, scarf_a)
    return cv, shoulder_n

# ---------------------------------------------------------------- animation curves
def S(x): return math.sin(x)

def idle(i, n=8):
    t = i / n * 2 * math.pi
    breath = S(t)
    eyes = 'half' if i == 5 else ('closed' if i == 6 else 'open')
    return pose(dy=round(0.5 + 0.5 * breath), sq=0.5 * (1 - breath) * .0 + (0.6 if breath < -.3 else 0),
                hdy=round(0.8 * S(t - .6)), sway=round(1.4 * S(t - 1.0)), tail=round(1.2 * S(t - 1.4)),
                an=(4, 7 + round(0.8 * breath)), af=(-4, 7 + round(0.8 * breath)), eyes=eyes, gem=2 if i == 3 else (1 if i in (2, 4) else 0))

def run(i, n=8):
    p = i / n * 2 * math.pi
    bob = abs(math.cos(p))
    fl = lambda ph: max(0, round(4 * math.sin(ph + 1.57)))
    return pose(dx=2, dy=-round(1.6 * bob) + 1, hdy=round(.9 * S(p - .8)), hdx=1, sway=round(-2 + 1.6 * math.cos(p - .9)),
                tail=round(2 * S(p - 1.2)), fn=(round(6 * S(p)) + 2, fl(p)), ff=(round(6 * S(p + math.pi)) - 2, fl(p + math.pi)),
                an=(round(5 * S(p + math.pi)) + 1, 5 - round(2 * abs(S(p + math.pi)))),
                af=(round(5 * S(p)) - 1, 5 - round(2 * abs(S(p)))), mouth='open' if i in (2, 6) else 'smile', gem=1 if i % 4 == 0 else 0)

def jump_anims():
    a = {}
    a['crouch'] = [pose(dy=3, sq=2.2, hdy=1, fn=(6, 0), ff=(-6, 0), an=(5, 3), af=(-5, 3), eyes='half'),
                   pose(dy=4, sq=3.2, hdy=2, fn=(7, 0), ff=(-7, 0), an=(6, 1), af=(-6, 1), eyes='closed', mouth='grin')]
    a['rise'] = [pose(dy=-3, sq=-2.4, hdy=-2, fn=(3, 6), ff=(-3, 4), an=(5, -5), af=(-5, -5), mouth='open', sway=-3, tail=-2),
                 pose(dy=-5, sq=-1.4, hdy=-1, fn=(3, 7), ff=(-4, 5), an=(5, -7), af=(-5, -7), mouth='open', sway=-2, tail=-3)]
    a['apex'] = [pose(dy=-5, sq=0, hdy=0, fn=(4, 6), ff=(-4, 6), an=(7, -3), af=(-7, -3), eyes='happy', mouth='open', sway=0, tail=-1),
                 pose(dy=-4, sq=.4, hdy=1, fn=(4, 5), ff=(-4, 5), an=(7, -1), af=(-7, -1), eyes='happy', mouth='open', sway=1, tail=0)]
    a['fall'] = [pose(dy=-4, sq=-1.6, hdy=-1, fn=(4, 5), ff=(-4, 6), an=(6, -6), af=(-6, -6), mouth='o', sway=3, tail=2),
                 pose(dy=-5, sq=-2.0, hdy=-2, fn=(5, 6), ff=(-3, 5), an=(6, -7), af=(-6, -7), mouth='o', sway=4, tail=3)]
    a['land'] = [pose(dy=3, sq=3, hdy=2, fn=(7, 0), ff=(-7, 0), an=(7, 2), af=(-7, 2), eyes='closed', sway=3),
                 pose(dy=2, sq=1.6, hdy=1, fn=(6, 0), ff=(-6, 0), an=(6, 4), af=(-6, 4), eyes='half', sway=1),
                 pose(dy=1, sq=.5, fn=(5, 0), ff=(-5, 0), an=(5, 6), af=(-5, 6), sway=-1)]
    return a

def dash():
    return [pose(dx=3, dy=2, sq=1.5, hdx=2, fn=(9, 2), ff=(-9, 0), an=(-6, 3), af=(-8, 2), eyes='focus', mouth='grin', sway=-5, tail=-3),
            pose(dx=5, dy=3, sq=2.0, hdx=3, hdy=1, fn=(10, 3), ff=(-10, 1), an=(-7, 2), af=(-9, 1), eyes='focus', mouth='grin', sway=-6, tail=-4),
            pose(dx=5, dy=3, sq=2.2, hdx=3, hdy=1, fn=(9, 1), ff=(-10, 2), an=(-7, 3), af=(-9, 2), eyes='focus', mouth='grin', sway=-6, tail=-4)]

def hurt():
    return [pose(dx=-2, dy=1, sq=1, hdx=-2, hdy=1, eyes='hurt', mouth='o', an=(5, -4), af=(-5, -4), sway=4, tail=3),
            pose(dx=-3, dy=2, sq=2, hdx=-3, hdy=2, eyes='hurt', mouth='o', an=(7, -2), af=(-7, -2), sway=5, tail=4),
            pose(dx=-2, dy=1, sq=.5, hdx=-1, eyes='half', mouth='sad', an=(5, 3), af=(-5, 3), sway=2, tail=1)]

def cheer(i, n=8):
    p = i / n * 2 * math.pi
    up = max(0, S(p))
    return pose(dy=-round(2.6 * up), sq=-0.6 * up, hdy=round(S(p - .7)), eyes='happy', mouth='open',
                an=(3 + round(S(p)), -10 + round(S(p + 1.5))), af=(-3 - round(S(p)), -10 + round(S(p + 3.1))),
                fn=(4, round(3 * up)), ff=(-4, round(3 * up)), sway=round(2 * S(p - 1)), tail=round(2 * S(p - 1.5)), gem=2 if i % 4 == 0 else (1 if i % 2 else 0))

def all_poses():
    A = {}
    A['idle'] = [idle(i) for i in range(8)]
    A['run'] = [run(i) for i in range(8)]
    A.update(jump_anims())
    A['dash'] = dash(); A['hurt'] = hurt()
    A['cheer'] = [cheer(i) for i in range(8)]
    return A

# anim name -> (fps, loop)
ANIMS = dict(idle=(10, True), run=(16, True), crouch=(20, False), rise=(12, False), apex=(10, True), fall=(12, True),
             land=(18, False), dash=(18, True), hurt=(10, False), cheer=(12, True))

# ---------------------------------------------------------------- shooting arm sprites
AW = AH = 24
def draw_arm(pal, d, recoil, winter=False):
    cv = Canvas(AW, AH)
    ax, ay = 8, 14                     # shoulder anchor inside the arm cell
    L = 9 - recoil
    dirs = {'fwd': (1, 0), 'diag': (.72, -.72), 'up': (0, -1)}
    vx, vy = dirs[d]
    hx, hy = ax + vx * L, ay + vy * L
    part(cv, line(ax, ay, hx, hy, 2.0), pal, hi=False)
    part(cv, ell(hx, hy, 3.1 if winter else 2.9, 3.1 if winter else 2.9), dict(pal, base=hexc('#f4fbff'), hi=hexc('#ffffff'), shade=hexc('#bcd8f0'), deep=hexc('#8aa8d0')) if winter else pal, hi=False)
    # little gum glob at the fingertip, charged look
    gx, gy = hx + vx * 3.5, hy + vy * 3.5
    cv.put(ell(gx, gy, 1.8, 1.8), hexc('#ff9fcf')); cv.put({(int(gx), int(gy))}, hexc('#ffe0f0'))
    return cv, (ax, ay), (hx + vx * 5, hy + vy * 5)

def build(pal, kind, winter=False):
    poses = all_poses()
    frames, frames_na, meta, order = [], [], [], []
    for name, plist in poses.items():
        start = len(frames)
        for P in plist:
            cv, sh = draw_hero(pal, kind, P, winter); frames.append(cv); meta.append(sh)
            cv2, _ = draw_hero(pal, kind, dict(P, near_arm=False), winter); frames_na.append(cv2)
        order.append((name, start, len(plist)))
    return frames, frames_na, meta, order

def build_arms(pal, winter=False):
    arms, info = [], []
    for d in ('fwd', 'diag', 'up'):
        for r in (3, 1, 0):
            cv, anchor, muzzle = draw_arm(pal, d, r, winter); arms.append(cv)
            info.append(dict(dir=d, recoil=r, anchor=list(anchor), muzzle=[float(muzzle[0]), float(muzzle[1])]))
    return arms, info

# ---------------------------------------------------------------- ghost (downed player) in a bubble
def draw_ghost(pal, t):
    cv = Canvas(40, 44)
    bob = round(1.5 * S(t))
    cx, cy = 20, 22 + bob
    b = ell(cx, cy, 16, 16)
    cv.put(outline_of(b), hexc('#c9b3ff'))
    cv.put({p for p in b if (p[0]-1, p[1]-1) not in b}, hexc('#e9ddff'))
    cv.put({(cx - 9 + i, cy - 12 + int(i * .5)) for i in range(4)}, WHITE)
    # sleepy head
    hd = ell(cx, cy + 1, 8.5, 7.5)
    part(cv, hd, pal, hi=False)
    cv.put(rect(cx - 5, cy - 1, cx - 2, cy), pal['eye']); cv.put(rect(cx + 2, cy - 1, cx + 5, cy), pal['eye'])
    cv.put({(cx, cy + 3), (cx + 1, cy + 3)}, pal['mouth'])
    cv.put({(cx - 3, cy - 9), (cx - 2, cy - 10), (cx + 2, cy - 10), (cx + 3, cy - 9)}, pal['hornA'])
    cv.put(ell(cx + 1 + round(2 * S(t)), cy - 14, 1.5, 1.5), hexc('#ffffff'))
    return cv
