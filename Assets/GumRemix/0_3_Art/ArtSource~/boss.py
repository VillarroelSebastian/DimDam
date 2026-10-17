import math
from px import *

S = math.sin
C = math.cos
BW = BH = 128
WHITE = hexc('#ffffff')

GUM = dict(base=hexc('#ff79b8'), hi=hexc('#ffd0ec'), shade=hexc('#e2489a'), deep=hexc('#a62e7c'), out=hexc('#43103f'), out_l=hexc('#c0407e'))
GLOVE = dict(base=hexc('#fff8ec'), hi=WHITE, shade=hexc('#eed4c4'), deep=hexc('#c8a4a4'), out=hexc('#43103f'), out_l=hexc('#c0a0a0'))
LICORICE = dict(base=hexc('#2a1838'), hi=hexc('#5a4070'), shade=hexc('#1a0e28'), deep=hexc('#100818'), out=hexc('#100818'))
HATRED = dict(base=hexc('#ee3d4e'), hi=hexc('#ff8a8a'), shade=hexc('#b82a4a'), deep=hexc('#7a1c40'), out=hexc('#2a0a28'), out_l=hexc('#a02848'))
BELLY = hexc('#ffb4d8'); BELLY_S = hexc('#f08cc0')
EYEW = hexc('#fffaf0'); EYEW_S = hexc('#f0d0d8'); PUP = hexc('#2a0e2e')
YEL = hexc('#ffd23a'); YEL_S = hexc('#e8982a')

def lerp(a, b, t): return a + (b - a) * t

def draw_boss(phase=1, morph=0.0, dy=0, sx=1.0, sy=1.0, eyes='smug', mouth='smirk', armL=(-8, 14), armR=(8, 14), curl=0.0,
              hat=True, hatdy=0, hatx=0, drip=0.0, bow=0, look=0, feet_dy=0, blink=False, balloon=0.0, rag=0.0, arms=True):
    """phase 1/2: pear-shaped gum ringmaster. morph 0..1 stretches P1 -> P2. balloon 0..1 inflates toward phase 3."""
    cv = Canvas(BW, BH)
    ccx = 64
    # ---- geometry (lerped between forms)
    ry0 = lerp(34, 42, morph); cy0 = lerp(80, 74, morph); rx0 = lerp(37, 35, morph)
    cx_, cy_ = ccx, lerp(cy0, 58, balloon) + dy
    rx = lerp(rx0, 46, balloon) * sx
    ry = lerp(ry0, 44, balloon) * sy
    if rag > 0:     # deflate: shrink and sag toward the floor
        rx *= lerp(1, .55, rag); ry *= lerp(1, .5, rag); cy_ = lerp(cy_, 108, rag)
    # ---- drips for phase 2 (hang from the sides)
    body = ell(cx_, cy_, rx, ry)
    if drip > 0 and balloon < .5:
        for k, off in enumerate((-26, -14, 13, 25)):
            ln = 6 + 8 * ((drip + k * .27) % 1.0) * (1 if k % 2 else .7)
            body |= ell(cx_ + off, cy_ + ry - 4 + ln * .5, 3.4, 3 + ln * .5)
            body |= ell(cx_ + off, cy_ + ry - 4 + ln, 3.4, 3.4)
    # ---- feet (fade into dangling legs when ballooning)
    if balloon < .65:
        fy = 120 + feet_dy
        for fx in (46, 82):
            part(cv, ell(fx, fy, 12, 6.5), dict(GUM, base=hexc('#e2489a'), shade=hexc('#b8337e'), deep=hexc('#862068')), hi=False)
    # ---- far arm
    def draw_arm(sgn, d):
        sx0, sy0 = cx_ + sgn * (rx - 4), cy_ - 6 + (4 if balloon > .5 else 0)
        ex, ey = sx0 + d[0], sy0 + d[1]
        part(cv, line(sx0, sy0, ex, ey, 6.5 - balloon * 2), GUM, hi=False)
        part(cv, ell(ex, ey, 9.5 - balloon * 3, 9 - balloon * 3), GLOVE, hi=True)
        cv.put({(int(ex - 2), int(ey + 1)), (int(ex + 2), int(ey + 1))}, GLOVE['deep'])
    if arms: draw_arm(-1, armL)
    # ---- balloon knot + dangling legs
    if balloon > 0.01:
        kx, ky = ccx, cy_ + ry
        t = balloon
        knot = poly([(kx - 4, ky - 3), (kx + 4, ky - 3), (kx + 6, ky + 6 * t), (kx - 6, ky + 6 * t)])
        part(cv, knot, GUM, hi=False)
        for sgn in (-1, 1):
            lx = kx + sgn * 8
            sw = round(2 * S(dy * .3 + sgn))
            part(cv, line(kx + sgn * 2, ky + 4, lx + sw, ky + 4 + 14 * t, 2.2), GUM, hi=False)
            part(cv, ell(lx + sw + 2, ky + 4 + 14 * t + 2, 6, 3.6), dict(GUM, base=hexc('#e2489a'), shade=hexc('#b8337e'), deep=hexc('#862068')), hi=False)
    # ---- body
    part(cv, body, GUM)
    # glossy belly + big shine
    if balloon < .5 and rag == 0:
        bl = ell(cx_, cy_ + ry * .28, rx * .58, ry * .52)
        cv.put(bl, BELLY); cv.put({p for p in bl if (p[0], p[1] + 1) not in bl}, BELLY_S)
    shine = [(cx_ - rx * .62, cy_ - ry * .45), (cx_ - rx * .5, cy_ - ry * .62)]
    cv.put(line(shine[0][0], shine[0][1], shine[1][0], shine[1][1], 2.2) | ell(shine[0][0] - 1, shine[0][1] + 5, 1.8, 1.8), WHITE)
    cv.put(line(shine[0][0] + 3, shine[0][1] + 7, shine[0][0] + 3, shine[0][1] + 11, 1.0), GUM['hi'])
    # ---- face
    fcx = ccx
    fy = cy_ - ry * lerp(.28, .1, balloon)
    es = lerp(1.0, .78, balloon)
    ex_off = 14 * es
    for k, sgn in enumerate((-1, 1)):
        ex = fcx + sgn * ex_off + look * 2
        er_x, er_y = 8.5 * es, 10 * es
        if eyes == 'x':
            c = int(ex); y0 = int(fy)
            cv.put({(c - 4 + i, y0 - 4 + i) for i in range(9)} | {(c - 4 + i, y0 + 4 - i) for i in range(9)}, PUP)
            continue
        if eyes == 'hurt':
            c = int(ex); y0 = int(fy)
            cv.put({(c - 5 + i, y0 - 3 + (i if i < 5 else 9 - i) // 1) for i in range(9)}, PUP) if sgn == -1 else cv.put({(c + 5 - i, y0 - 3 + (i if i < 5 else 9 - i)) for i in range(9)}, PUP)
            continue
        w = ell(ex, fy, er_x, er_y)
        cv.put(outline_of(w), GUM['out']); cv.put(w, EYEW)
        cv.put({p for p in w if (p[0] + 1, p[1] + 1) not in w}, EYEW_S)
        pr = 4.2 * es if eyes != 'wide' else 2.8 * es
        pw = ell(ex + look * 2.4, fy + (2 if eyes in ('smug', 'angry') else 0), pr, pr * 1.15)
        cv.put(pw, PUP); cv.put({(int(ex + look * 2.4 - 1), int(fy - 1))}, WHITE)
        if eyes == 'smug' or blink:   # heavy lid covers upper half
            lid_y = fy - er_y * (.02 if blink else .4)
            lid = {p for p in w if p[1] < lid_y} if not blink else {p for p in w if p[1] < fy + er_y * .5}
            cv.put(lid, GUM['base']); cv.put({(p[0], int(lid_y)) for p in w if p[1] == int(lid_y) + 0}, GUM['out'])
            cv.put({(x, int(lid_y)) for x in range(int(ex - er_x), int(ex + er_x + 1)) if (x, int(lid_y)) in w or (x, int(lid_y) + 1) in w}, GUM['out'])
        if eyes in ('angry', 'wide'):
            by_ = fy - er_y - (3 if eyes == 'angry' else 6)
            if eyes == 'angry':
                cv.put(line(ex - sgn * -7, by_ + 4 * -1 + 8, ex + sgn * -7, by_ + 4 + 1, 1.6) if False else line(ex + sgn * 8, by_ + 1, ex - sgn * 7, by_ + 7, 1.8), PUP)
            else:
                cv.put(line(ex - 7, by_ + 2 - sgn * 0, ex + 7, by_ + 2, 1.3), PUP)
    # ---- mustache (licorice), tips curl up/down with `curl`
    my = fy + ry * lerp(.38, .3, balloon) - 2 + (0)
    msc = lerp(1.0, .7, balloon)
    for sgn in (-1, 1):
        pts = [(fcx + sgn * 2, my), (fcx + sgn * 10 * msc, my + 3 * msc), (fcx + sgn * 20 * msc, my + 1 - curl * 4 * msc), (fcx + sgn * 25 * msc, my - 5 * msc - curl * 6 * msc)]
        m = set()
        for i in range(len(pts) - 1):
            r0 = (4.2 - i * 1.1) * msc; r1 = (4.2 - (i + 1) * 1.1) * msc
            m |= line(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], max(.9, (r0 + r1) / 2))
        part(cv, m, LICORICE, hi=False)
        cv.put({(int(pts[1][0]) - sgn, int(pts[1][1]) - 1)}, LICORICE['hi'])
    # ---- mouth
    mouthy = my + 8 * msc
    mw = 16 * msc
    if mouth == 'smirk':
        cv.put({(int(fcx - mw + i), int(mouthy + 2 - (1 if 5 < i < 22 else 0) - (1 if i > 24 else 0) * 2 + (2 if i < 3 else 0))) for i in range(int(mw * 2))}, PUP)
        cv.put({(int(fcx + mw - 1), int(mouthy - 1)), (int(fcx + mw), int(mouthy - 2))}, PUP)
    elif mouth in ('open', 'spit'):
        big = 1.0 if mouth == 'open' else 1.5
        mo = ell(fcx, mouthy + 4, mw * .8 * big * msc ** .3, (7 if mouth == 'open' else 11) * msc)
        cv.put(outline_of(mo), PUP); cv.put(mo, hexc('#5a1030'))
        top = {p for p in mo if p[1] < mouthy + 1}
        cv.put({(x, y) for (x, y) in top if y <= int(mouthy) - 1 + 0 and (x // 3) % 2 == 0 and y < mouthy - 1 + 3 - 0 and (x, y - 1) not in mo}, WHITE)
        cv.put({(x, int(mouthy - 2)) for x in range(int(fcx - mw * .6), int(fcx + mw * .6)) if (x // 3) % 2 == 0 and (x, int(mouthy - 2)) in mo}, WHITE)
        cv.put(ell(fcx, mouthy + 9 * msc, mw * .4, 3.2), hexc('#ff6a8a'))
    elif mouth == 'o':
        mo = ell(fcx + 2, mouthy + 3, 6 * msc, 6 * msc)
        cv.put(outline_of(mo), PUP); cv.put(mo, hexc('#5a1030'))
        cv.put(ell(fcx + 2, mouthy + 6, 3.4 * msc, 2.2), hexc('#ff6a8a'))
    elif mouth == 'frown':
        cv.put({(int(fcx - 8 + i), int(mouthy + 5 - (abs(i - 8) < 6 and 1 or 0) - (abs(i - 8) < 3 and 1 or 0))) for i in range(17)}, PUP)
    # ---- bow tie
    if balloon < .5 and rag == 0:
        by2 = cy_ + ry * .62 + bow
        l = poly([(fcx - 2, by2), (fcx - 16, by2 - 8), (fcx - 16, by2 + 8)]); r = poly([(fcx + 2, by2), (fcx + 16, by2 - 8), (fcx + 16, by2 + 8)])
        for m in (l, r):
            part(cv, m, dict(base=YEL, hi=hexc('#fff0a0'), shade=YEL_S, deep=hexc('#b0602a'), out=GUM['out']), hi=False)
        cv.put({(fcx - 11, int(by2) - 2), (fcx - 8, int(by2) + 2), (fcx + 10, int(by2) - 2), (fcx + 7, int(by2) + 2)}, hexc('#ee3d4e'))
        part(cv, ell(fcx, by2, 3.4, 4), HATRED, hi=False)
    # ---- near arm (in front of body)
    if arms: draw_arm(1, armR)
    # ---- hat
    if hat and balloon < .3 and morph < .4:
        hx0 = int(round(ccx + hatx)); hy0 = int(round(cy_ - ry + 2 + hatdy))
        crown = poly([(hx0 - 15, hy0), (hx0 - 13, hy0 - 22), (hx0 + 13, hy0 - 22), (hx0 + 15, hy0)])
        part(cv, crown, HATRED, hi=False)
        for sx_ in range(-12, 14, 6):
            cv.put({(hx0 + sx_ + a, hy0 - 22 + b) for b in range(22) for a in range(3) if (hx0 + sx_ + a, hy0 - 22 + b) in crown}, hexc('#fff2e8'))
        part(cv, rect(hx0 - 15, hy0 - 6, hx0 + 15, hy0 - 2), dict(base=YEL, hi=hexc('#fff0a0'), shade=YEL_S, deep=hexc('#b0602a'), out=GUM['out']), hi=False)
        part(cv, ell(hx0, hy0 + 1, 22, 4.4), HATRED, hi=False)
        part(cv, ell(hx0, hy0 - 22, 13, 3.4), dict(HATRED, base=hexc('#ff6a78')), hi=False)
    elif morph >= .4 and balloon < .5:   # a few defiant gum tufts replace the hat
        for off, h in ((-8, 9), (0, 12), (8, 8)):
            part(cv, tri((cx_ + off - 4, cy_ - ry + 3), (cx_ + off + (off * .2), cy_ - ry - h), (cx_ + off + 4, cy_ - ry + 3)), GUM, hi=False)
    return cv

# ---------------------------------------------------------------- animation sets
def idle1(i, n=8):
    t = i / n * 2 * math.pi
    return dict(phase=1, sy=1 + .035 * S(t), sx=1 - .02 * S(t), hatdy=round(1.2 * S(t - .9)), hatx=round(S(t - .5)), curl=.5 + .5 * S(t - .4),
                armL=(-8, 14 + round(S(t + 1))), armR=(8, 14 + round(S(t))), look=round(S(t * .5)), blink=(i == 6), bow=round(S(t - 1)))
def spit1(i, n=6):
    k = [0, 1, 2, 3, 4, 5][i]
    sx = [1.0, 1.06, 1.12, .96, .94, 1.0][k]; sy = [1.0, 1.04, 1.1, .92, .96, 1.0][k]
    mouth = ['smirk', 'smirk', 'open', 'spit', 'spit', 'open'][k]
    return dict(phase=1, sx=sx, sy=sy, mouth=mouth, eyes='smug' if k < 2 else 'angry', armL=(-12, 6 - 4 * (k > 1)), armR=(12, 6 - 4 * (k > 1)), hatdy=[0, -1, -3, 2, 1, 0][k], curl=.2)
def slam1():
    return [dict(phase=1, sy=.82, sx=1.12, dy=0, armL=(-10, 18), armR=(10, 18), eyes='angry', mouth='open', hatdy=3),
            dict(phase=1, sy=1.14, sx=.9, dy=-1, armL=(-6, -10), armR=(6, -10), eyes='angry', mouth='open', feet_dy=-6, hatdy=-4),
            dict(phase=1, sy=1.1, sx=.92, dy=0, armL=(-8, -12), armR=(8, -12), eyes='angry', mouth='open', feet_dy=-4, hatdy=-3),
            dict(phase=1, sy=1.12, sx=.92, dy=0, armL=(-14, 6), armR=(14, 6), eyes='wide', mouth='o', feet_dy=-2),
            dict(phase=1, sy=.74, sx=1.2, dy=2, armL=(-18, 14), armR=(18, 14), eyes='angry', mouth='open', hatdy=5),
            dict(phase=1, sy=.9, sx=1.08, dy=1, armL=(-12, 16), armR=(12, 16), eyes='smug', mouth='smirk', hatdy=2)]
def hurt1():
    return [dict(phase=1, sy=.94, sx=1.04, eyes='hurt', mouth='open', armL=(-14, 4), armR=(14, 4), hatdy=3, hatx=-2),
            dict(phase=1, sy=.9, sx=1.08, eyes='hurt', mouth='o', armL=(-16, 2), armR=(16, 2), hatdy=4, hatx=-3)]
def morph12():
    out = []
    for i in range(8):
        t = i / 7
        hat = i < 3
        out.append(dict(phase=2, morph=t, sy=1 + .1 * S(t * math.pi * 3), sx=1 - .06 * S(t * math.pi * 3), eyes='hurt' if i < 3 else 'wide' if i < 6 else 'angry', mouth='o' if i < 6 else 'open',
                        hat=hat, hatdy=-i * 5, hatx=i * 2, armL=(-10 - i * 2, 10 - i * 3), armR=(10 + i * 2, 10 - i * 3), drip=t, curl=t))
    return out
def idle2(i, n=8):
    t = i / n * 2 * math.pi
    return dict(phase=2, morph=1, hat=False, sy=1 + .05 * S(t), sx=1 - .035 * S(t), eyes='angry', mouth='smirk' if i % 4 else 'open', curl=-.3 + .8 * S(t),
                drip=i / n, armL=(-12, 12 + round(2 * S(t))), armR=(12, 12 + round(2 * S(t + 1.4))), look=round(S(t)), bow=round(S(t - 1)))
def swing2():
    return [dict(phase=2, morph=1, hat=False, sx=1.0, eyes='angry', mouth='open', armL=(-14, 8), armR=(-6, -22), drip=.2),
            dict(phase=2, morph=1, hat=False, sx=1.06, eyes='angry', mouth='open', armL=(-14, 8), armR=(-18, -26), drip=.4),
            dict(phase=2, morph=1, hat=False, sx=.96, eyes='angry', mouth='spit', armL=(-14, 8), armR=(36, 4), drip=.6),
            dict(phase=2, morph=1, hat=False, sx=.94, eyes='angry', mouth='spit', armL=(-14, 8), armR=(48, 8), drip=.8),
            dict(phase=2, morph=1, hat=False, sx=.98, eyes='angry', mouth='open', armL=(-14, 8), armR=(30, 14), drip=.0),
            dict(phase=2, morph=1, hat=False, sx=1.0, eyes='angry', mouth='smirk', armL=(-14, 8), armR=(14, 14), drip=.2)]
def summon2():
    return [dict(phase=2, morph=1, hat=False, sy=.92, eyes='angry', mouth='open', armL=(-12, 16), armR=(12, 16)),
            dict(phase=2, morph=1, hat=False, sy=1.04, eyes='angry', mouth='open', armL=(-14, -8), armR=(14, -8), drip=.3),
            dict(phase=2, morph=1, hat=False, sy=1.1, eyes='wide', mouth='spit', armL=(-12, -26), armR=(12, -26), drip=.5),
            dict(phase=2, morph=1, hat=False, sy=1.1, eyes='wide', mouth='spit', armL=(-12, -28), armR=(12, -28), drip=.7),
            dict(phase=2, morph=1, hat=False, sy=1.0, eyes='angry', mouth='open', armL=(-14, -6), armR=(14, -6), drip=.9),
            dict(phase=2, morph=1, hat=False, sy=.94, eyes='angry', mouth='smirk', armL=(-12, 12), armR=(12, 12), drip=.1)]
def hurt2():
    return [dict(phase=2, morph=1, hat=False, sy=.93, sx=1.05, eyes='hurt', mouth='open', armL=(-18, 0), armR=(18, 0), drip=.5),
            dict(phase=2, morph=1, hat=False, sy=.88, sx=1.1, eyes='hurt', mouth='o', armL=(-20, -2), armR=(20, -2), drip=.7)]
def morph23():
    out = []
    for i in range(8):
        t = i / 7
        out.append(dict(phase=3, morph=1, balloon=t, hat=False, sx=1 + .1 * S(t * math.pi * 4), sy=1 + .1 * S(t * math.pi * 4 + 1), eyes='wide', mouth='o' if i < 7 else 'frown',
                        armL=(-10, 6 + i), armR=(10, 6 + i), dy=-round(t * 16), drip=0))
    return out
def idle3(i, n=8):
    t = i / n * 2 * math.pi
    return dict(phase=3, morph=1, balloon=1, hat=False, dy=round(3 * S(t)), sy=1 + .03 * S(t), sx=1 - .02 * S(t), eyes='wide', mouth='frown' if i % 5 else 'o', curl=S(t),
                armL=(-14, 8 + round(3 * S(t + 1))), armR=(14, 8 + round(3 * S(t))), look=round(S(t)))
def blow3():
    return [dict(phase=3, morph=1, balloon=1, hat=False, sx=1.0, eyes='angry', mouth='frown', armL=(-14, 6), armR=(14, 6)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=1.06, sy=1.04, eyes='angry', mouth='o', armL=(-16, 2), armR=(16, 2)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=1.1, sy=1.08, eyes='angry', mouth='spit', armL=(-18, -2), armR=(18, -2)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=.94, sy=.96, eyes='angry', mouth='spit', armL=(-12, 6), armR=(12, 6)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=.96, sy=.98, eyes='angry', mouth='o', armL=(-12, 8), armR=(12, 8)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=1.0, eyes='wide', mouth='frown', armL=(-14, 8), armR=(14, 8))]
def hurt3():
    return [dict(phase=3, morph=1, balloon=1, hat=False, sx=.94, sy=1.06, eyes='hurt', mouth='o', armL=(-18, -6), armR=(18, -6)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=.9, sy=1.1, eyes='hurt', mouth='o', armL=(-20, -10), armR=(20, -10))]
def dive3():
    return [dict(phase=3, morph=1, balloon=1, hat=False, sx=.9, sy=1.14, eyes='angry', mouth='open', armL=(-10, -14), armR=(10, -14)),
            dict(phase=3, morph=1, balloon=1, hat=False, sx=1.16, sy=.84, eyes='hurt', mouth='o', armL=(-20, 4), armR=(20, 4))]
def death3():
    out = []
    n = 12
    for i in range(n):
        t = i / (n - 1)
        if t < .5:
            k = t / .5
            out.append(dict(phase=3, morph=1, balloon=1, hat=False, eyes='x', mouth='o', sx=1 + .12 * S(k * math.pi * 5), sy=1 - .1 * S(k * math.pi * 5), dy=round(-6 * k + 4 * S(k * 9)),
                            rag=k * .25, armL=(-18, -8 + round(6 * S(k * 8))), armR=(18, -8 + round(6 * S(k * 8 + 2)))))
        else:
            k = (t - .5) / .5
            out.append(dict(phase=3, morph=1, balloon=1 - k * .6, hat=False, eyes='x', mouth='o' if k < .6 else 'frown', sx=1 + .3 * k, sy=1 - .5 * k, dy=round(18 * k), rag=.25 + .75 * k,
                            armL=(-16, 6 + 6 * k), armR=(16, 6 + 6 * k), arms=k < .85))
    return out

def all_frames():
    A = {}
    A['p1_idle'] = [idle1(i) for i in range(8)]
    A['p1_spit'] = [spit1(i) for i in range(6)]
    A['p1_slam'] = slam1()
    A['p1_hurt'] = hurt1()
    A['to_p2'] = morph12()
    A['p2_idle'] = [idle2(i) for i in range(8)]
    A['p2_swing'] = swing2()
    A['p2_summon'] = summon2()
    A['p2_hurt'] = hurt2()
    A['to_p3'] = morph23()
    A['p3_idle'] = [idle3(i) for i in range(8)]
    A['p3_blow'] = blow3()
    A['p3_hurt'] = hurt3()
    A['p3_dive'] = dive3()
    A['death'] = death3()
    return A

BOSS_ANIMS = {'p1_idle': (10, True), 'p1_spit': (12, False), 'p1_slam': (10, False), 'p1_hurt': (10, False), 'to_p2': (9, False),
              'p2_idle': (10, True), 'p2_swing': (12, False), 'p2_summon': (10, False), 'p2_hurt': (10, False), 'to_p3': (9, False),
              'p3_idle': (10, True), 'p3_blow': (12, False), 'p3_hurt': (10, False), 'p3_dive': (8, False), 'death': (9, False)}

def build_boss():
    A = all_frames(); frames = []; order = []
    for name, plist in A.items():
        order.append((name, len(frames), len(plist)))
        for P in plist: frames.append(draw_boss(**P))
    return frames, order

# --------------------------------------------------------------- the knocked-off hat (spins in the air)
def hat_frames(n=8):
    out = []
    base = Canvas(48, 48)
    cx, cy = 24, 24
    crown = poly([(cx - 10, cy + 4), (cx - 9, cy - 12), (cx + 9, cy - 12), (cx + 10, cy + 4)])
    part(base, crown, HATRED, hi=False)
    for sx_ in range(-8, 10, 6):
        base.put({(cx + sx_ + a, cy - 12 + b) for b in range(16) for a in range(3) if (cx + sx_ + a, cy - 12 + b) in crown}, hexc('#fff2e8'))
    part(base, rect(cx - 10, cy - 2, cx + 10, cy + 1), dict(base=YEL, hi=hexc('#fff0a0'), shade=YEL_S, deep=hexc('#b0602a'), out=GUM['out']), hi=False)
    part(base, ell(cx, cy + 5, 15, 3.4), HATRED, hi=False)
    for i in range(n):
        a = i / n * 2 * math.pi
        cv = Canvas(48, 48)
        for y in range(48):
            for x in range(48):
                dx, dy = x + .5 - cx, y + .5 - cy
                sx_ = int(cx + dx * C(a) + dy * S(a)); sy_ = int(cy - dx * S(a) + dy * C(a))
                if 0 <= sx_ < 48 and 0 <= sy_ < 48:
                    p = base.px[sx_, sy_]
                    if p[3]: cv.px[x, y] = p
        out.append(cv)
    return out
