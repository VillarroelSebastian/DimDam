"""La Princesa Chicle: an elegant gum princess (form 1) whose body gives way as she is hurt: the crown cracks off, the face
stretches and the gown melts into drips (form 2), and finally she swells into a many-eyed gum horror (form 3).
Same animation names, frame counts and timings as the old ringmaster (animation sets are imported from boss_clown),
so the fight code keeps working. Everything is drawn at native resolution with the shared pixel toolkit."""
import math
from px import *
import boss_clown as old

S = math.sin
C = math.cos
OX, OY, BOT_PAD = 20, 24, 22   # the figure lives in a 128x128 space inside a bigger cell so crowns, claws and drips never clip
BW, BH = OX * 2 + 128, OY + 128 + BOT_PAD
PIVOT_Y = (BOT_PAD + 2) / float(BH)
WHITE = hexc('#ffffff')

SKIN0 = dict(base=hexc('#ffb0d6'), hi=hexc('#ffe6f3'), shade=hexc('#ee78b8'), deep=hexc('#b8428c'), out=hexc('#3a0e3a'), out_l=hexc('#c0407e'))
SKIN1 = dict(base=hexc('#c850b8'), hi=hexc('#f89ae8'), shade=hexc('#923094'), deep=hexc('#561a6c'), out=hexc('#200a2c'), out_l=hexc('#8a3498'))
DRESS0 = dict(base=hexc('#cf2a86'), hi=hexc('#ff72bc'), shade=hexc('#951f6c'), deep=hexc('#5e1450'), out=hexc('#2a0a2a'), out_l=hexc('#a02070'))
LIC = dict(base=hexc('#2a1838'), hi=hexc('#6a4a88'), shade=hexc('#1a0e28'), deep=hexc('#100818'), out=hexc('#100818'))
GOLD = dict(base=hexc('#f2c14e'), hi=hexc('#fff2b0'), shade=hexc('#c8892e'), deep=hexc('#8a5a22'), out=hexc('#43103f'), out_l=hexc('#c8892e'))
HAIR = dict(base=hexc('#8a1c78'), hi=hexc('#e04aa8'), shade=hexc('#62135a'), deep=hexc('#3e0c42'), out=hexc('#240a2a'), out_l=hexc('#7a2a78'))
LOLLI = dict(base=hexc('#ff7ab8'), hi=hexc('#ffd0ec'), shade=hexc('#e2489a'), deep=hexc('#a62e7c'), out=hexc('#43103f'), out_l=hexc('#c0407e'))
BONE = hexc('#fff0e0'); BONE_S = hexc('#e8c8c0')
LIP = hexc('#b81e66'); LIP_S = hexc('#7a1048'); LIP_H = hexc('#ff6aa8')
MAW = hexc('#4a0a2a'); TONGUE = hexc('#ff6a8a')
CYAN = hexc('#4de8e0'); CYAN_H = hexc('#d8ffff'); CYAN_S = hexc('#2aa0b8')
VIOLET = hexc('#8a30c8'); ACID = hexc('#e8ff58'); ACID_S = hexc('#b8d030')
EYEW = hexc('#fffaf0'); EYEW_S = hexc('#f0d0d8'); PUP = hexc('#2a0e2e')
BLUSH = hexc('#ff5aa0')


def lerp(a, b, t): return a + (b - a) * t
def lpal(a, b, t): return {k: mix(a[k], b[k], t) for k in a if k in b}


class Off:
    """Draws in the 128x128 figure space but writes into the bigger cell."""
    def __init__(self, cv, ox, oy): self.cv, self.ox, self.oy = cv, ox, oy
    def put(self, pts, col): self.cv.put([(x + self.ox, y + self.oy) for x, y in pts], col)


class Tf:
    """Squash/stretch about the hem, a global lift, and an optional shrink (the humanoid sinking into the swollen form)."""
    def __init__(s, sx, sy, dy, hs=1.0, hdy=0.0, cx=64.0, bot=124.0):
        s.sx, s.sy, s.dy, s.hs, s.hdy, s.cx, s.bot = sx, sy, dy, hs, hdy, cx, bot
    def X(s, x): return s.cx + (x - s.cx) * s.sx * s.hs
    def Y(s, y): return s.bot - (s.bot - y) * s.sy * s.hs + s.dy + s.hdy
    def R(s, v): return v * s.hs * (s.sx + s.sy) / 2
    def E(s, x, y, rx, ry): return ell(s.X(x), s.Y(y), max(.6, rx * s.sx * s.hs), max(.6, ry * s.sy * s.hs))
    def L(s, x0, y0, x1, y1, r): return line(s.X(x0), s.Y(y0), s.X(x1), s.Y(y1), max(.7, s.R(r)))
    def P(s, pts): return poly([(s.X(a), s.Y(b)) for a, b in pts])
    def pt(s, x, y): return (int(round(s.X(x))), int(round(s.Y(y))))


def teeth(cv, x0, x1, y, n, down, h, col=BONE, shade=BONE_S):
    """A row of pointed teeth between x0 and x1 hanging from (down) or rising to (up) the line y."""
    w = (x1 - x0) / float(n)
    for i in range(n):
        a = x0 + i * w
        pts = [(a, y), (a + w, y), (a + w / 2, y + (h if down else -h))]
        m = poly(pts)
        cv.put(m, col)
        cv.put({p for p in m if (p[0] + 1, p[1]) not in m}, shade)


def draw_boss(phase=1, morph=0.0, dy=0, sx=1.0, sy=1.0, eyes='smug', mouth='smirk', armL=(-8, 14), armR=(8, 14), curl=0.0,
              hat=True, hatdy=0, hatx=0, drip=0.0, bow=0, look=0, feet_dy=0, blink=False, balloon=0.0, rag=0.0, arms=True):
    big = Canvas(BW, BH); cv = Off(big, OX, OY)
    m = max(0.0, min(1.0, morph)); t = max(0.0, min(1.0, balloon))
    cx = 64.0
    skin = lpal(SKIN0, SKIN1, m)
    dress = lpal(DRESS0, SKIN1, m * .75)
    hair = lpal(HAIR, SKIN1, m * .35)
    hs = 1.0 - 0.8 * t ** 0.8                                  # the humanoid shrinks as the swelling takes over
    mass_cy = lerp(92, 58, t)
    hdy = (mass_cy - 78) * min(1.0, t * 1.6) if t > 0 else 0.0   # ... and sinks toward the middle of the mass
    T = Tf(sx, sy, dy, hs, hdy)

    # ================================================================ the swollen form (form 3 and its transition)
    mrx = lerp(24, 46, t) * sx; mry = lerp(26, 44, t) * sy
    mcy = lerp(96, 58, t) + dy
    if rag > 0:
        mrx *= lerp(1, .55, rag); mry *= lerp(1, .5, rag); mcy = lerp(mcy, 108, rag)
    mass = set()
    if t > 0.03:
        mass = ell(cx, mcy, mrx, mry) | ell(cx - mrx * .5, mcy - mry * .5, mrx * .52, mry * .42) | ell(cx + mrx * .52, mcy - mry * .42, mrx * .46, mry * .46) \
            | ell(cx - mrx * .8, mcy + mry * .15, mrx * .3, mry * .36) | ell(cx + mrx * .78, mcy + mry * .2, mrx * .3, mry * .4)
        for k, (off, hh) in enumerate(((-.62, 11), (-.3, 15), (.05, 9), (.4, 16), (.72, 10))):     # gum spikes along the back
            mass |= poly([(cx + off * mrx - 4, mcy - mry * .78), (cx + off * mrx + S(k * 3 + drip * 6.283) * 1.5, mcy - mry * .78 - hh * t), (cx + off * mrx + 4, mcy - mry * .78)])
    if t > 0.03:
        # tendrils (they replace the legs) and strand arms hang behind the mass
        for k, off in enumerate((-24, -12, 0, 12, 24)):
            if rag > .85: break
            bx = cx + off * (mrx / 46.0); by = mcy + mry - 6
            ex = bx + 4 * S(drip * 6.283 + k * 1.3) * t; ey = by + (10 + 12 * abs(S(k * 2.1 + drip * 6.283 * 0.5))) * t * (1 - rag)
            part(cv, line(bx, by, ex, ey, 2.7), skin, hi=False)
            cv.put(ell(ex, ey + 1.5, 3.2, 3.6), skin['base']); cv.put({p for p in ell(ex, ey + 1.5, 3.2, 3.6) if (p[0] + 1, p[1] + 1) not in ell(ex, ey + 1.5, 3.2, 3.6)}, skin['shade'])
        part(cv, mass, skin)
        bl = ell(cx - mrx * .1, mcy + mry * .55, mrx * .62, mry * .3)
        cv.put({p for p in bl if dither(p[0], p[1], .35)}, skin['hi'])           # glossy dithered belly sheen
        for k, (a0, a1) in enumerate(((-2.4, .5), (-.9, 1.1), (0.4, 1.6), (2.2, 2.8), (-2.9, 3.4))):         # dark veins
            x0_, y0_ = cx + C(a0) * mrx * .25, mcy + S(a0) * mry * .2
            for i in range(1, 7):
                x1_ = x0_ + C(a0 + .35 * S(i + k)) * mrx * .13; y1_ = y0_ + S(a0 + .35 * S(i + k)) * mry * .13
                cv.put(line(x0_, y0_, x1_, y1_, .5), skin['deep']); x0_, y0_ = x1_, y1_
        cv.put({p for p in mass if p[0] > cx + mrx * .15 and p[1] > mcy - mry * .1 and dither(p[0], p[1], .14)}, skin['shade'])
        sh = ell(cx - mrx * .55, mcy - mry * .5, 3.0, 4.6); cv.put(sh, WHITE)
        cv.put(line(cx - mrx * .55 + 4, mcy - mry * .5 + 9, cx - mrx * .55 + 4, mcy - mry * .5 + 12, 1.0), skin['hi'])

    # ================================================================ form 1 / 2 humanoid
    if hs > 0.22:
        # --- far arm and hair behind the body
        def arm_geom(sgn, d):
            k = 1.1 + .45 * m
            shx, shy = cx + sgn * 17, 51
            hx, hy = shx + d[0] * k, shy + d[1] * .95 - 4
            hx = max(-4.0, min(132.0, hx))
            return shx, shy, hx, hy

        def draw_arm(sgn, d, claws):
            shx, shy, hx, hy = arm_geom(sgn, d)
            ex_, ey_ = (shx + hx) / 2 + sgn * 1.5, (shy + hy) / 2 + 3
            part(cv, T.L(shx, shy, ex_, ey_, 3.0 + 1.3 * m), skin, hi=False)
            glove = lpal(LIC, skin, m * .85)
            part(cv, T.L(ex_, ey_, hx, hy, 3.5 + 1.0 * m), glove, hi=False)
            if m > .3: part(cv, T.E(ex_, ey_, 3.6 + 1.6 * m, 3.6 + 1.6 * m), skin, hi=False)       # knobbly elbow
            mx_, my_ = lerp(ex_, hx, .55), lerp(ey_, hy, .55)
            if m < .6: part(cv, T.E(mx_, my_, 3.6, 3.6), GOLD, hi=False)          # gold cuff
            if claws:
                n = 3
                for i in range(n):
                    a = math.atan2(hy - ey_, hx - ex_) + (i - 1) * .55
                    L_ = 7 + 3 * m
                    part(cv, T.L(hx, hy, hx + C(a) * L_, hy + S(a) * L_, 1.1), dict(base=BONE, hi=WHITE, shade=BONE_S, deep=BONE_S, out=skin['out']), hi=False, shade=False)
                part(cv, T.E(hx, hy, 4.4, 4.2), skin, hi=False)
            else:
                part(cv, T.E(hx, hy, 4.6, 4.4), glove, hi=True)

        part(cv, T.E(cx - 17, 34, 7, 16) | T.E(cx + 17, 34, 7, 16), hair, hi=False)
        for sgn in (-1, 1):                                       # ringlets (behind the arms)
            for j in range(4):
                x = cx + sgn * (19 + curl * 1.8 * (j + 1) * .5 + j * .6 + 2 * m * S(drip * 6.283 + j)); y = 30 + j * 7 + m * j * 2
                part(cv, T.E(x, y, 3.5 + m * .6, 3.6 + m * 1.2), hair, hi=False)
        if m > .05:                                              # melting hair: strands that drip down behind the shoulders
            for k in range(7):
                x = cx - 18 + k * 6
                ln = (6 + 24 * m) * (0.6 + 0.4 * S(k * 1.7 + drip * 6.283))
                part(cv, T.L(x, 26, x + S(k) * 1.5, 26 + ln, 1.9), hair, hi=False)
                cv.put(T.E(x + S(k) * 1.5, 26 + ln + 1, 2.4, 2.8), hair['base'])
        if arms: draw_arm(-1, armL, m > .5)

        # --- gown
        hem = 118 - 7 * m + feet_dy * 0.5
        left = []; right = []
        for i in range(7):
            u = i / 6.0; y = 72 + (hem - 72) * u; w = 11 + 27 * (u ** 0.72) + (3 * S(u * 6 + drip * 6.283) * m)
            left.append((cx - w, y)); right.append((cx + w, y))
        skirt = T.P(left + right[::-1])
        for k in range(-5, 6):
            skirt |= T.E(cx + k * 7.4, hem, 4.4, 4.6)
            if m > .02:                                           # melting: uneven drips that thin out and end in a fat drop
                ln = m * (3 + 9 * abs(S(k * 2.3 + 1.1))) * (0.75 + .25 * S(drip * 6.283 + k))
                xo = cx + k * 7.4 + 2.2 * S(k * 1.7 + drip * 6.283)
                skirt |= T.L(cx + k * 7.4, hem, xo, hem + ln, 2.6 * (1 - .35 * m)) | T.E(xo, hem + ln + 1.2, 3.1 + m * .8, 3.6)
        if m > .3:
            part(cv, T.E(cx, 123, 38 + 8 * m, 3.4), dict(skin, base=skin['shade'], shade=skin['deep']), hi=False)
        part(cv, skirt, dress)
        for ty_, amp in ((93, 1.6), (106, 1.9)):                    # ruffle tiers
            for x in range(-34, 35):
                yy = ty_ + round(amp * S(x * .62))
                if abs(x) < 11 + 27 * ((ty_ - 72) / (hem - 72)) ** .72 - 3:
                    cv.put([T.pt(cx + x, yy)], dress['hi']); cv.put([T.pt(cx + x, yy + 1)], dress['shade'])
        for bx_, by_, br_ in ((cx - 14, 112, 3.2), (cx + 9, 98, 2.6), (cx + 22, 114, 3.6), (cx - 26, 101, 2.2)):      # gum bubbles caught in the gown
            if m < .8:
                bb = T.E(bx_, by_, br_, br_); cv.put(outline_of(bb), dress['deep']); cv.put(bb, dress['shade'])
                cv.put([T.pt(bx_ - br_ * .35, by_ - br_ * .4)], WHITE)
        cv.put(T.L(cx - 24, 104, cx - 19, 91, 1.0), WHITE)                # gloss streak
        for gx, gy in ((cx + 15, 100), (cx - 8, 112), (cx + 24, 112)):    # gold sparkles
            cv.put([T.pt(gx, gy), T.pt(gx - 1, gy), T.pt(gx + 1, gy), T.pt(gx, gy - 1), T.pt(gx, gy + 1)], GOLD['hi'])

        # --- corset, shoulders, neck
        part(cv, T.P([(cx - 10, 72), (cx + 10, 72), (cx + 14, 53), (cx - 14, 53)]), LIC, hi=False)
        for yy in (53, 54, 70, 71):
            cv.put([T.pt(cx + x, yy) for x in range(-11, 12) if abs(x) <= 10 + (yy - 53) * 0 + 2], GOLD['base'] if yy in (53, 71) else GOLD['shade'])
        for k in range(4):
            yy = 57 + k * 4
            cv.put(T.L(cx - 2, yy, cx + 2, yy + 3, .55), GOLD['base']); cv.put(T.L(cx + 2, yy, cx - 2, yy + 3, .55), GOLD['base'])
        if m > .25:                                            # the bodice cracks open
            for xx, sg in ((cx - 7, 1), (cx + 6, -1)):
                pts = [(xx, 54)]
                for k in range(1, 5): pts.append((xx + sg * (k % 2) * 3, 54 + k * 4))
                for i in range(len(pts) - 1): cv.put(T.L(pts[i][0], pts[i][1], pts[i + 1][0], pts[i + 1][1], .7 + .4 * m), skin['deep'])
        part(cv, T.E(cx, 51, 16, 5), skin, hi=False)
        part(cv, T.E(cx - 17, 51, 5.6, 5.6) | T.E(cx + 17, 51, 5.6, 5.6), dress)
        hr = 1 + .5 * m
        top = 17.5
        fry = 13.5 * hr; fcy = top + fry
        part(cv, T.P([(cx - 3.5, 38), (cx + 3.5, 38), (cx + 4.5, 52), (cx - 4.5, 52)]), skin, hi=False)
        for xx in range(-6, 7):                                  # necklace
            cv.put([T.pt(cx + xx, 48 + (xx * xx) // 14)], GOLD['base'])
        cv.put([T.pt(cx, 51), T.pt(cx - 1, 51), T.pt(cx, 52), T.pt(cx - 1, 52)], CYAN)
        cv.put([T.pt(cx - 1, 51)], CYAN_H)

        # --- head
        face = T.E(cx, fcy, 12.5, fry) | T.P([(cx - 9, fcy + fry * .45), (cx + 9, fcy + fry * .45), (cx + 1.5, fcy + fry + 4 + 8 * m), (cx - 1.5, fcy + fry + 4 + 8 * m)])
        part(cv, face, skin)
        eyc = fcy + (-1 if m < .2 else 0)
        for sgn in (-1, 1):
            cheek = T.E(cx + sgn * 8.2, eyc + 7, 3.2, 2.0)
            cv.put({p for p in cheek if dither(p[0], p[1], .55)}, BLUSH)

        # eyes (the left one swells and turns acid yellow as she breaks)
        for k, sgn in enumerate((-1, 1)):
            es = (1 + .5 * m) if sgn == -1 else (1 - .22 * m)
            ex = cx + sgn * 6.4 * (1 + .12 * m)
            ey = eyc
            rx_, ry_ = 4.5 * es, 5.2 * es
            if eyes == 'wide': ry_ *= 1.15; rx_ *= 1.08
            if eyes == 'angry': ry_ *= .78
            if eyes == 'x':
                for d_ in range(-4, 5):
                    cv.put([T.pt(ex + d_ * .9, ey + d_ * .9), T.pt(ex + d_ * .9, ey - d_ * .9)], PUP)
                continue
            if eyes == 'hurt':
                cv.put(T.L(ex - 4.5 * es, ey + 2, ex, ey - 2.5, .9), PUP); cv.put(T.L(ex, ey - 2.5, ex + 4.5 * es, ey + 2, .9), PUP)
                continue
            if blink:
                cv.put(T.L(ex - 4.6, ey, ex + 4.6, ey + 1, .9), PUP)
                cv.put(T.L(ex + sgn * 4.2, ey, ex + sgn * 6.5, ey - 2, .7), PUP)
                continue
            w = T.E(ex, ey, rx_, ry_)
            cv.put(outline_of(w), skin['out']); cv.put(w, EYEW)
            cv.put({p for p in w if (p[0] + 1, p[1] + 1) not in w}, EYEW_S)
            iris = mix(VIOLET, ACID, max(0, (m - .3) / .7)) if sgn == -1 else VIOLET
            ir = (2.9 if eyes != 'wide' else 2.2) * es
            iw = T.E(ex + look * 1.4, ey + .8, ir, ir * 1.28)
            cv.put(iw, iris)
            if m > .45 and sgn == -1:
                cv.put(T.E(ex + look * 1.4, ey + .8, .9, ir * 1.28), PUP)             # slit pupil
            else:
                cv.put(T.E(ex + look * 1.4, ey + 1.2, 1.4, 1.9 * es), PUP)
            cv.put([T.pt(ex + look * 1.4 - 1.2, ey - .8)], WHITE)
            lid_y = ey - ry_ * (.0 if eyes == 'smug' else -1)
            if eyes == 'smug':                                  # heavy lid
                lid = {p for p in w if p[1] < T.Y(ey - ry_ * .1)}
                cv.put(lid, skin['base']); cv.put(T.L(ex - rx_, ey - ry_ * .1, ex + rx_, ey - ry_ * .1, .5), skin['out'])
            cv.put(T.L(ex - sgn * 4.6, ey - ry_ + .6, ex + sgn * 4.8, ey - ry_ - (1.2 if eyes != 'angry' else -.6), .8), skin['out'])   # upper lash line
            cv.put(T.L(ex + sgn * 4.4, ey - ry_ + .4, ex + sgn * 7.4, ey - ry_ - 2.0, .6), skin['out'])                                    # flick
            by_ = ey - ry_ - 3.2
            if eyes == 'angry': cv.put(T.L(ex - sgn * 5.5, by_ + 2.6, ex + sgn * 5.5, by_ - 1.8, 1.0), hair['deep'])
            elif eyes == 'wide': cv.put(T.L(ex - 5, by_ - 1.5, ex + 5, by_ - 1.5, .8), hair['deep'])
            else: cv.put(T.L(ex - 5, by_ + 1.0, ex + 5, by_ - .6 * sgn, .8), hair['deep'])

        if m > .55:                                               # a third eye opens in her forehead
            e3 = T.E(cx, fcy - fry * .62, 3.6 * (m - .4) * 1.6, 3.2 * (m - .4) * 1.6)
            cv.put(outline_of(e3), skin['out']); cv.put(e3, hexc('#fff8d8'))
            cv.put(T.E(cx + look, fcy - fry * .62 + .4, 1.5, 1.9), ACID); cv.put(T.E(cx + look, fcy - fry * .62 + .4, .6, 1.8), PUP)
        # mouth
        my = fcy + fry * .52 + 3.5 * m
        mw = 5.0 * (1 + .75 * m)
        if mouth == 'smirk':
            for i in range(int(-mw), int(mw) + 1):
                yy = my + (1 if abs(i) > mw * .62 else 0) - (1 if i > mw * .6 else 0) * 2
                cv.put([T.pt(cx + i, yy), T.pt(cx + i, yy + 1)], LIP)
                cv.put([T.pt(cx + i, yy - 1)], LIP_H) if abs(i) < 2 else None
            cv.put([T.pt(cx - mw, my + 1), T.pt(cx + mw + 1, my - 2)], LIP_S)
        elif mouth in ('open', 'spit'):
            big_ = 1.0 if mouth == 'open' else 1.45
            mo = T.E(cx, my + 1.5, mw * .9 * big_, (3.4 if mouth == 'open' else 5.6) * (1 + .2 * m))
            cv.put(outline_of(mo), LIP_S); cv.put(mo, MAW)
            x0_, x1_ = bbox(mo)[0], bbox(mo)[2]
            teeth(cv, x0_ + 1, x1_, bbox(mo)[1], max(3, int((x1_ - x0_) / 3)), True, 2 + int(m * 2))
            cv.put(T.E(cx, my + 4.5, mw * .45, 1.8), TONGUE)
        elif mouth == 'o':
            mo = T.E(cx, my + 1.5, 2.6 + m * 1.2, 3.2 + m)
            cv.put(outline_of(mo), LIP_S); cv.put(mo, MAW); cv.put(T.E(cx, my + 3, 1.4, 1.1), TONGUE)
        elif mouth == 'frown':
            for i in range(int(-mw), int(mw) + 1):
                yy = my + 2 - (1 if abs(i) < mw * .55 else 0) - (1 if abs(i) < mw * .25 else 0)
                cv.put([T.pt(cx + i, yy), T.pt(cx + i, yy + 1)], LIP)
        if m > .4 and mouth in ('smirk', 'frown'):               # the smile has too many teeth
            teeth(cv, cx - mw * .8, cx + mw * .8, my + 1, 6, True, 2)

        # --- hair on top (drawn after the face so it frames it) and crown
        part(cv, T.E(cx, 19, 16.5, 9) | T.E(cx - 12, 25, 6, 8) | T.E(cx + 12, 25, 6, 8), hair)
        for a in range(0, 14):
            ang = a / 13.0 * 3.4 + .4
            cv.put([T.pt(cx - 6 + 5 * C(ang), 17 - 3.4 * S(ang))], hair['hi'])
        if hat and t < .3 and m < .4:
            hx = cx + hatx; y0 = 6 + hatdy
            part(cv, T.P([(hx - 13, y0 + 8), (hx + 13, y0 + 8), (hx + 12, y0 + 14), (hx - 12, y0 + 14)]), GOLD, hi=True)
            for i, off in enumerate((-12, -6, 0, 6, 12)):
                h_ = 10 if off == 0 else 7
                part(cv, T.P([(hx + off - 3.5, y0 + 9), (hx + off, y0 + 9 - h_), (hx + off + 3.5, y0 + 9)]), GOLD, hi=False)
                cv.put(T.E(hx + off, y0 + 9 - h_ - 0.6, 1.7, 1.7), hexc('#ff3aa0')); cv.put([T.pt(hx + off - .6, y0 + 9 - h_ - 1.2)], WHITE)
            part(cv, T.E(hx, y0 + 11, 2.7, 2.2), dict(base=CYAN, hi=CYAN_H, shade=CYAN_S, deep=CYAN_S, out=GOLD['out']), hi=False)
        elif m >= .4 and t < .7:                                  # broken crown: gold shards stuck in her hair
            for off, h_, lean in ((-9, 11, -3), (-2, 8, 1), (8, 12, 4)):
                part(cv, T.P([(cx + off - 3, 19), (cx + off + lean, 19 - h_), (cx + off + 3, 19)]), GOLD, hi=False)

        # --- near arm and the scepter
        if arms:
            draw_arm(1, armR, m > .5)
            if m < .5 and t < .3:
                shx, shy, hx, hy = arm_geom(1, armR)
                tipx, tipy = hx + 5, hy - 24
                part(cv, T.L(hx, hy + 2, tipx, tipy, 1.2), GOLD, hi=False)
                disc = T.E(tipx, tipy - 4, 7.5, 7.5)
                part(cv, disc, LOLLI)
                for (px_, py_) in disc:
                    ddx, ddy = px_ + .5 - T.X(tipx), py_ + .5 - T.Y(tipy - 4)
                    r_ = math.hypot(ddx, ddy); ang_ = math.atan2(ddy, ddx)
                    if r_ < 6.2 and int((r_ * .62 + ang_ / (2 * math.pi) * 2.0) % 2.0) == 0: cv.put([(px_, py_)], WHITE)

    # ================================================================ features of the swollen form
    if t > 0.03:
        # rags of the gown that have not melted yet
        for sgn in (-1, 1):
            k = t * (1 - rag * .6)
            bx = cx + sgn * (mrx - 3); by = mcy + mry * .35
            part(cv, poly([(bx, by - 6), (bx + sgn * 7 * k, by + 4 * k), (bx + sgn * 3, by + 14 * k)]), dress, hi=False)
            part(cv, poly([(bx - sgn * 4, by + 8), (bx + sgn * 5 * k, by + 18 * k), (bx - sgn * 6, by + 16 * k)]), LIC, hi=False)
        # crown shards stuck in the top of the mass
        for off, h_, lean in ((-22, 12, -5), (-8, 9, 1), (10, 13, 5), (24, 8, 6)):
            part(cv, poly([(cx + off - 3.5, mcy - mry + 5), (cx + off + lean, mcy - mry + 5 - h_ * t), (cx + off + 3.5, mcy - mry + 5)]), GOLD, hi=False)
        cv.put(ell(cx + 10 + 5, mcy - mry - 8 * t + 5, 1.8, 1.8), hexc('#ff3aa0'))
        # eyes: more and more of them
        eye_def = [(-16, -14, 6.2), (9, -20, 7.2), (25, -6, 4.8), (-30, 0, 4.2), (-2, -4, 4.0), (16, 4, 3.4)]
        n_eyes = 2 + int(round(4 * max(0, t - .35) / .65))
        for i, (ox_, oy_, r_) in enumerate(eye_def[:n_eyes]):
            ex = cx + ox_ * (mrx / 46.0); ey = mcy + oy_ * (mry / 44.0)
            r_ *= min(1.0, .5 + t * .6) * (.8 if i > 1 else 1.0)
            if eyes == 'x':
                for d_ in range(-int(r_), int(r_) + 1):
                    cv.put([(int(ex + d_), int(ey + d_)), (int(ex + d_), int(ey - d_))], PUP)
                continue
            if eyes == 'hurt':
                cv.put(line(ex - r_, ey + 1.5, ex, ey - r_ * .5, .9), PUP); cv.put(line(ex, ey - r_ * .5, ex + r_, ey + 1.5, .9), PUP)
                continue
            w = ell(ex, ey, r_, r_ * (1.0 if eyes != 'angry' else .75))
            cv.put(outline_of(w), skin['out']); cv.put(w, hexc('#fff8d8'))
            cv.put({p for p in w if (p[0] + 1, p[1] + 1) not in w}, hexc('#e8d8a8'))
            ir = r_ * (.62 if eyes != 'wide' else .4)
            iw = ell(ex + look * 1.5, ey + .5, ir, ir)
            cv.put(iw, ACID); cv.put({p for p in iw if (p[0] + 1, p[1] + 1) not in iw}, ACID_S)
            cv.put(ell(ex + look * 1.5, ey + .5, max(.7, ir * .22), ir * .98), PUP)                       # slit pupil
            for vx_, vy_ in ((-1, -1), (1, -1), (-1, 1)):
                cv.put([(int(ex + vx_ * r_ * .8), int(ey + vy_ * r_ * .55)), (int(ex + vx_ * r_ * .55), int(ey + vy_ * r_ * .4))], hexc('#e0405a'))   # bloodshot
            if eyes == 'angry': cv.put(line(ex - r_, ey - r_ * .55 - 1 - (i % 2) * 1, ex + r_, ey - r_ - 2, 1.0), skin['deep'])
        # maw
        mf = {'frown': .8, 'o': .6, 'open': 1.05, 'spit': 1.25}.get(mouth, .9)
        mwx = mrx * .74 * (.35 + .65 * t); mwy = mry * .30 * mf * (.35 + .65 * t)
        mcx_, mcy_ = cx, mcy + mry * .4
        mo = ell(mcx_, mcy_, mwx, mwy) & mass
        cv.put(outline_of(mo) & mass, LIP_S); cv.put(mo, MAW)
        if mo:
            x0_, y0_, x1_, y1_ = bbox(mo)
            teeth(cv, x0_ + 1, x1_ - 1, y0_ + 1, max(4, int((x1_ - x0_) / 5.5)), True, 3 + int(t * 3))
            teeth(cv, x0_ + 3, x1_ - 3, y1_, max(3, int((x1_ - x0_) / 7)), False, 2 + int(t * 2))
            cv.put(ell(mcx_, mcy_ + mwy * .45, mwx * .4, mwy * .38) & mo, TONGUE)
            if mouth in ('spit', 'open') and t > .5:
                for k in (-1, 1):                                  # drool
                    cv.put(line(mcx_ + k * mwx * .5, y1_, mcx_ + k * mwx * .5, y1_ + 5 + 2 * S(drip * 6.283 + k), .8), skin['hi'])
        # strand arms with claws
        if arms:
            for sgn, d in ((-1, armL), (1, armR)):
                bx = cx + sgn * (mrx - 5); by = mcy + 2
                hx = max(-4.0, min(132.0, bx + d[0] * 1.5 * t + sgn * 3)); hy = by + d[1] * 1.2 * t
                part(cv, line(bx, by, (bx + hx) / 2, (by + hy) / 2 + 3, 3.4), skin, hi=False)
                part(cv, line((bx + hx) / 2, (by + hy) / 2 + 3, hx, hy, 3.0), skin, hi=False)
                part(cv, ell(hx, hy, 4.6, 4.4), skin, hi=False)
                for i in range(3):
                    a = (math.atan2(hy - by, hx - bx) if (hx != bx or hy != by) else 0) + (i - 1) * .6
                    part(cv, line(hx, hy, hx + C(a) * 8, hy + S(a) * 8, 1.1), dict(base=BONE, hi=WHITE, shade=BONE_S, deep=BONE_S, out=skin['out']), hi=False, shade=False)
    return big


# ---------------------------------------------------------------- animation sets (shared with the old boss)
all_frames = old.all_frames
BOSS_ANIMS = old.BOSS_ANIMS


def build_boss():
    A = all_frames(); frames = []; order = []
    for name, plist in A.items():
        order.append((name, len(frames), len(plist)))
        for P in plist: frames.append(draw_boss(**P))
    return frames, order


# --------------------------------------------------------------- the crown she throws (spins in the air)
def hat_frames(n=8):
    out = []
    base = Canvas(48, 48)
    cx, cy = 24, 26
    part(base, poly([(cx - 12, cy + 4), (cx + 12, cy + 4), (cx + 11, cy - 3), (cx - 11, cy - 3)]), GOLD, hi=True)
    for off in (-10, -5, 0, 5, 10):
        h_ = 12 if off == 0 else 8
        part(base, poly([(cx + off - 3.4, cy - 2), (cx + off, cy - 2 - h_), (cx + off + 3.4, cy - 2)]), GOLD, hi=False)
        base.put(ell(cx + off, cy - 3 - h_, 1.7, 1.7), hexc('#ff3aa0'))
    part(base, ell(cx, cy + 1, 2.8, 2.2), dict(base=CYAN, hi=CYAN_H, shade=CYAN_S, deep=CYAN_S, out=GOLD['out']), hi=False)
    for i in range(n):
        a = i / n * 2 * math.pi
        cv = Canvas(48, 48)
        for y in range(48):
            for x in range(48):
                dx, dy = x + .5 - 24, y + .5 - 24
                sx_ = int(24 + dx * C(a) + dy * S(a)); sy_ = int(24 - dx * S(a) + dy * C(a))
                if 0 <= sx_ < 48 and 0 <= sy_ < 48:
                    p = base.px[sx_, sy_]
                    if p[3]: cv.px[x, y] = p
        out.append(cv)
    return out
