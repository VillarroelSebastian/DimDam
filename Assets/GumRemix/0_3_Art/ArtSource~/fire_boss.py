"""Los Reyes Mantis: two small robotic mantis kings of the Fire Kingdom (inspired by the Mantis Lords of Hollow Knight, as forged automatons).
Rey Brasa (rust, gold and ember orange) and Rey Ceniza (ash steel, silver and pale gold) fight as a pair and dance in step.
Each king is drawn from a small skeleton (legs, abdomen, chest, head, cape, two scythe arms, wings) so every pose and animation shares
the same construction and the loops stay clean. Frames face right; the game flips them for the left side."""
import math, random
from px import *
from fire_env import pal, halo, LAVA

S, C, TAU = math.sin, math.cos, 2 * math.pi
CW = CH = 96
GROUND = 90                      # feet baseline inside the cell
WHITE = hexc('#ffffff')


def make_pal(name):
    if name == 'brasa':
        return dict(
            armor=pal('#5a2c24', '#a8553a', '#3a1812', '#220c08', '#0e0404', '#7a3a2a'),
            trim=pal('#e0a030', '#fff0a0', '#a8681c', '#6a3c10', '#1c0e06', '#c88820'),
            cape=pal('#8a1c24', '#d04048', '#5a1018', '#34080e', '#14040a', '#a82830'),
            eye=hexc('#ffb040'), eye_hot=hexc('#fff0b0'), blade=pal('#c8c8d8', '#fff4e8', '#8a8aa0', '#585870', '#14141e', '#a8a8c0'),
            edge=hexc('#ff7a24'), glow=hexc('#ff8a24'), flame=[hexc('#ff4a10'), hexc('#ff8a24'), hexc('#ffc850'), hexc('#fff0b0')])
    return dict(
        armor=pal('#4a5068', '#8a94b4', '#32384e', '#1e2234', '#0a0c16', '#68728e'),
        trim=pal('#c8d0e0', '#ffffff', '#8890a8', '#586078', '#14182a', '#a8b0c8'),
        cape=pal('#443a5a', '#7a6a98', '#2e2640', '#1c1628', '#0a0810', '#5a4e78'),
        eye=hexc('#ffe890'), eye_hot=hexc('#ffffff'), blade=pal('#d8dce8', '#ffffff', '#9a9fb4', '#626880', '#14182a', '#b4bace'),
        edge=hexc('#ffd060'), glow=hexc('#ffc850'), flame=[hexc('#c85a1c'), hexc('#ffa030'), hexc('#ffe070'), hexc('#ffffff')])


# ------------------------------------------------------------------------------------------ construction helpers
def polar(p, ang, r):
    return (p[0] + C(ang) * r, p[1] + S(ang) * r)


def chain(cv, pts, thick, pl, hi=False):
    m = set()
    for i in range(len(pts) - 1):
        m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], thick[i] if isinstance(thick, (list, tuple)) else thick)
    part(cv, m, pl, hi=hi, k=1)
    return m


def blade(cv, p0, ang, length, curve, width, P, hot=0.0):
    """A curved scythe blade: a tapering crescent that bends by `curve` radians along its length, with a serrated inner edge and a glowing edge."""
    pts = []; widths = []
    p = p0; a = ang
    steps = 14
    for k in range(steps + 1):
        t = k / float(steps)
        pts.append(p); widths.append(width * (1 - t * .86) / 2.0 + .35)
        a += curve / steps
        p = polar(p, a, length / steps)
    m = set()
    for (x, y), w in zip(pts, widths): m |= ell(x, y, w, w)
    part(cv, m, P['blade'], hi=True, k=1)
    # the edge (outer side of the bend) glows, hotter during an attack
    side = 1 if curve > 0 else -1
    for k in range(2, steps, 1):
        x, y = pts[k]; a2 = ang + curve * k / steps
        nx, ny = -S(a2) * side, C(a2) * side
        ex, ey = x + nx * (widths[k] + .2), y + ny * (widths[k] + .2)
        cv.put([(int(round(ex)), int(round(ey)))], P['edge'] if hot < .5 or k % 2 else P['eye_hot'])
        if k % 3 == 0:                                            # serrations
            sx_, sy_ = x - nx * (widths[k] - .2), y - ny * (widths[k] - .2)
            cv.put([(int(round(sx_)), int(round(sy_)))], P['blade']['deep'])
    return pts


def foot(cv, ankle, toe_dir, P):
    ax, ay = ankle
    m = poly([(ax - 1.5, ay - 2), (ax + toe_dir * 8, ay + 1), (ax + toe_dir * 8, ay + 3), (ax - 3, ay + 3)])
    part(cv, m, P['armor'], hi=False, k=1)
    cv.put({(int(ax + toe_dir * 7), int(ay + 2))}, P['trim']['hi'])


def draw_king(P, dy=0, lean=0.0, bend=0.0, armF=(0.9, -.9, 1.0), armB=(1.2, -.8, 1.0), step=(0.0, 0.0), cape=0.0, flick=0.0, wing=0.0, eyes='open',
              hot=0.0, abd=0.0, squash=1.0, face=1, hurt=False, tilt=0.0, **_):
    """armF/armB = (upper-arm angle, blade angle relative to it, blade length factor); angles in radians (0 = right, +pi/2 = down).
    step = (front foot lift, back foot lift); abd = abdomen sway; wing = wing opening 0..1; hot = how brightly the core and blades burn."""
    cv = Canvas(CW, CH)
    gy = GROUND
    hipx, hipy = 46.0 + lean * .3, gy - 24 + dy + bend * 8
    chest = (hipx + lean * .5 + tilt * .5, hipy - 16 - (1 - squash) * -6)
    neck = (chest[0] + lean * .35, chest[1] - 12)
    head = (neck[0] + 2 + lean * .2, neck[1] - 7)

    # ---- cape (behind everything): two panels trailing back from the shoulders, waving with `cape`
    cp = poly([(chest[0] - 5, chest[1] - 6), (chest[0] + 3, chest[1] - 6),
               (chest[0] - 16 - 5 * S(cape), chest[1] + 26 + 2 * C(cape)), (chest[0] - 22 - 6 * S(cape + 1), chest[1] + 20 - 3 * S(cape)), (chest[0] - 24 - 4 * S(cape + 2), chest[1] + 8)])
    part(cv, cp, P['cape'], hi=False, k=1)
    for k in range(3): cv.put(line(chest[0] - 6 - k * 3, chest[1] - 2, chest[0] - 12 - k * 4 - 3 * S(cape + k), chest[1] + 20 + 2 * C(cape + k), .5), P['cape']['deep'])

    # ---- wings (folded plates on the back that open for jumps and dashes)
    for k, (wa, wl) in enumerate(((-2.4 + wing * 1.1, 24), (-2.0 + wing * .8, 20))):
        tip = polar((chest[0] - 4, chest[1] - 4), wa, wl)
        wm = poly([(chest[0] - 4, chest[1] - 4), tip, polar(tip, wa + 1.4, 5), polar((chest[0] - 4, chest[1] - 4), wa + .5, 8)])
        part(cv, wm, P['armor'], hi=False, k=1)
        cv.put(line(chest[0] - 4, chest[1] - 4, tip[0], tip[1], .5), P['trim']['base'])
        if wing > .5: cv.put(polar(tip, wa, 2) and {(int(polar(tip, wa, 2)[0]), int(polar(tip, wa, 2)[1]))}, P['flame'][1])

    # ---- abdomen: a segmented tail trailing behind the hips with a vent of flame at its tip
    ap = (hipx - 3, hipy - 2)
    seg_pts = [ap]
    a = 3.5 - .5 + abd * .3
    for k in range(4):
        a += -.28 + abd * .25 * S(k * 1.4 + 1)
        ap = polar(ap, a, 7.5 - k * .6); seg_pts.append(ap)
    for k in range(4):
        r = 5.6 - k * 1.1
        part(cv, ell(seg_pts[k + 1][0], seg_pts[k + 1][1], r, r * .92), P['armor'], hi=False, k=1)
        cv.put({(int(seg_pts[k + 1][0]) - 1, int(seg_pts[k + 1][1]) - 1)}, P['trim']['base'])
    tail = seg_pts[-1]
    fl = P['flame']
    cv.put(poly([(tail[0] - 2, tail[1] - 1), (tail[0] + 2, tail[1] + 2), (tail[0] - 8 - 2 * flick, tail[1] + 3 + S(flick) * 2)]), fl[0])
    cv.put(poly([(tail[0] - 1, tail[1]), (tail[0] + 1, tail[1] + 1), (tail[0] - 5 - 1.5 * flick, tail[1] + 2)]), fl[2])

    # ---- legs: digitigrade (thigh forward, shin back, foot forward); `step` lifts the feet
    for k, (ox, lift) in enumerate(((-3.5, step[1]), (3.5, step[0]))):
        hip_j = (hipx + ox, hipy + 3)
        knee = (hip_j[0] + 6 - bend * 2 + (lift * 3 if k else -lift * 2), hip_j[1] + 11 - lift * 3)
        ankle = (hip_j[0] - 2 + ox * .3 + (lift * 5 if k else -lift * 4), min(gy - 3 - lift * 9, knee[1] + 12 - bend * 3))
        chain(cv, [hip_j, knee], 2.7, P['armor'])
        chain(cv, [knee, ankle], 2.0, P['armor'])
        part(cv, ell(knee[0], knee[1], 2.5, 2.5), P['trim'], hi=False, k=1)
        foot(cv, ankle, 1, P)

    # ---- torso: waist, chest plate with a furnace core behind a grille
    part(cv, ell(hipx, hipy, 7.5, 6.5), P['armor'], hi=False)
    chest_m = ell(chest[0], chest[1], 10.2 * (2 - squash), 12.5 * squash)
    part(cv, chest_m, P['armor'])
    cv.put({(x, int(chest[1] - 8)) for x in range(int(chest[0] - 8), int(chest[0] + 9)) if (x, int(chest[1] - 8)) in chest_m}, P['trim']['base'])
    core = ell(chest[0] + 1, chest[1] + 1, 4.4, 4.4)
    cv.put(outline_of(core), hexc('#0a0408')); cv.put(core, hexc('#3a0e08'))
    cv.put(ell(chest[0] + 1, chest[1] + 1, 3.2 + hot, 3.2 + hot), fl[1]); cv.put(ell(chest[0] + .5, chest[1], 1.8 + hot * .6, 1.8 + hot * .6), fl[3])
    for gx in (-3, 0, 3): cv.put({(int(chest[0] + 1 + gx), int(chest[1] - 3 + k)) for k in range(7) if (int(chest[0] + 1 + gx), int(chest[1] - 3 + k)) in core}, hexc('#0a0408'))
    halo(cv, chest[0] + 1, chest[1] + 1, 12, hexc('#a0401a'), .22 + hot * .2)
    for sx_ in (-1, 1): part(cv, ell(chest[0] + sx_ * 9, chest[1] - 8, 4.6, 3.8), P['trim'], hi=True, k=1)              # shoulder plates

    # ---- head: a mantis head (inverted triangle) with glowing slit eyes, antennae and a small crown
    for sx_ in (-1, 1):
        ant = [(head[0] + sx_ * 3, head[1] - 7), (head[0] + sx_ * 8 - 2, head[1] - 17 - flick), (head[0] + sx_ * 11 - 6, head[1] - 22 + S(flick) * 2)]
        chain(cv, ant, .55, P['trim'], hi=False)
        cv.put({(int(ant[-1][0]), int(ant[-1][1]))}, P['eye_hot'])
    hm = poly([(head[0] - 9, head[1] - 7), (head[0] + 9, head[1] - 7), (head[0] + 7, head[1] + 1), (head[0] + 1, head[1] + 10), (head[0] - 5, head[1] + 3)])
    part(cv, hm, P['armor'])
    cv.put({(int(head[0] - 8 + k), int(head[1] - 7)) for k in range(17)}, P['trim']['base'])
    for sx_ in (-1, 1):
        ex = head[0] + sx_ * 4.4 + 1.5
        if eyes == 'hurt' or hurt:
            cv.put({(int(ex) - 2 + i, int(head[1]) - 4 + i) for i in range(4)} | {(int(ex) + 1 - i, int(head[1]) - 4 + i) for i in range(4)}, P['eye_hot'])
        else:
            eh = 3 if eyes == 'open' else 2
            slit = poly([(ex - 3.2, head[1] - 3), (ex + 3.4, head[1] - 4 - (1 if sx_ > 0 else 0)), (ex + 1.8, head[1] - 4 + eh), (ex - 2.6, head[1] - 3 + eh)])
            cv.put(slit, P['eye']); cv.put({(int(ex), int(head[1]) - 3), (int(ex) + 1, int(head[1]) - 3)}, P['eye_hot'])
            halo(cv, ex, head[1] - 2, 6, hexc('#a0401a'), .25)
    cv.put({(int(head[0]) - 1, int(head[1]) + 6), (int(head[0]), int(head[1]) + 7), (int(head[0]) + 1, int(head[1]) + 6)}, hexc('#0a0408'))
    cx_, cy_ = head[0] + 1, head[1] - 9
    cv.put(rect(int(cx_) - 6, int(cy_), int(cx_) + 7, int(cy_) + 3), P['trim']['base']); cv.put(rect(int(cx_) - 6, int(cy_), int(cx_) + 7, int(cy_) + 1), P['trim']['hi'])
    for k, off in enumerate((-5, 0, 5)):
        h_ = 6 if off == 0 else 4
        part(cv, poly([(cx_ + off - 2, cy_ + 1), (cx_ + off, cy_ - h_), (cx_ + off + 2, cy_ + 1)]), P['trim'], hi=False, k=1)
    cv.put(poly([(cx_ - 2, cy_ - 6), (cx_ + 1 + S(flick) * 1.5, cy_ - 13 - flick), (cx_ + 3, cy_ - 6)]), fl[1]); cv.put(poly([(cx_ - 1, cy_ - 6), (cx_ + 1, cy_ - 10 - flick * .6), (cx_ + 2, cy_ - 6)]), fl[3])

    # ---- the two scythe arms (drawn last: the back one first)
    sh = (chest[0] + 6, chest[1] - 7)
    for (ua, ba, bl), s_x, big in ((armB, -2, .9), (armF, 2, 1.0)):
        shoulder = (sh[0] - s_x, sh[1] + (1 if big < 1 else 0))
        elbow = polar(shoulder, ua, 11)
        chain(cv, [shoulder, elbow], 2.8, P['armor'])
        part(cv, ell(elbow[0], elbow[1], 3.2, 3.2), P['trim'], hi=False, k=1)
        blade(cv, elbow, ua + ba, 24 * bl * big, -1.5 if ba < 0 else 1.5, 6.4, P, hot)
    return cv


