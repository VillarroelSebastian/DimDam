"""Animation sets for the Mantis Kings (see fire_boss.py for the skeleton). Every animation is a list of poses fed to draw_king;
loops use whole sine periods so they close seamlessly, and the attack animations are keyframed."""
import math, random
from px import *
from fire_boss import (draw_king, make_pal, polar, S, C, TAU, CW, CH, GROUND, halo, LAVA, part, ell, line, poly, rect, mix, hexc)


def _swoosh(cv, P, c, r, a0, a1, w=3.0):
    """A glowing crescent left by a sweeping scythe."""
    steps = 18
    for k in range(steps + 1):
        a = a0 + (a1 - a0) * k / steps
        t = k / float(steps)
        ww = w * (.25 + .75 * math.sin(t * math.pi))
        for q in (0, 1):
            x, y = polar(c, a, r - q * ww * .5)
            cv.put(ell(x, y, ww * (1 - q * .5) / 2 + .4, ww * (1 - q * .5) / 2 + .4), P['flame'][3] if q else P['flame'][1])


def frame_king(P, pose, extra=None):
    cv = draw_king(P, **pose)
    if extra: extra(cv, P)
    return cv


def seq(P):
    A = {}
    N = 8

    def idle(i):
        t = i / N * TAU
        return dict(dy=round(1.2 * S(t)), bend=.25 + .15 * S(t), armF=(.9 + .15 * S(t), -.9 + .1 * S(t + 1), 1.0), armB=(1.2 + .15 * S(t + .8), -.8, 1.0),
                    cape=t, flick=.5 + .5 * S(2 * t), abd=S(t), hot=.15 + .1 * S(t))
    A['idle'] = [frame_king(P, idle(i)) for i in range(N)]

    def dance(i):
        t = i / N * TAU
        return dict(dy=round(-abs(2 * S(t))), bend=.3 + .3 * S(2 * t), lean=3 * S(t), tilt=2 * S(t), armF=(.5 + 1.0 * S(t), -.6 + .3 * C(t), 1.0),
                    armB=(1.2 - 1.0 * S(t), -.7, 1.0), step=(max(0, S(t)), max(0, -S(t))), cape=2 * t, flick=1.0 + S(2 * t), wing=.3 + .3 * S(t),
                    abd=S(t + .5), hot=.35 + .25 * S(2 * t))
    A['dance'] = [frame_king(P, dance(i)) for i in range(N)]

    def stepf(i):
        t = i / 6 * TAU
        return dict(dy=-round(abs(S(t)) * 1.5), bend=.2, lean=2, armF=(1.0, -.9, 1.0), armB=(1.3, -.8, 1.0), step=(max(0, S(t)), max(0, -S(t))),
                    cape=t, abd=S(t), flick=.5 + .5 * S(2 * t))
    A['step'] = [frame_king(P, stepf(i)) for i in range(6)]
    A['windup'] = [frame_king(P, dict(bend=.7 + .15 * i, lean=-2 - i, dy=1 + i, armF=(2.7, -.3, 1.1), armB=(2.3, -.3, 1.05), cape=1.5 + i, hot=.4 + .3 * i, flick=1 + i, wing=.2)) for i in range(3)]

    def streaks(cv, P_):
        for k, yy in enumerate((40, 50, 60, 70)):
            cv.put(line(10 + k * 3, yy, 34, yy, .5), P_['flame'][1 + k % 2])
    A['dash'] = [frame_king(P, dict(lean=8, dy=4 + (i % 2), bend=1.0, armF=(.15, .25, 1.25), armB=(.35, .15, 1.15), wing=1, cape=2.2 + i * .5, hot=1,
                                    flick=2 + i, abd=-.6, tilt=3, step=(.5, .3 * (i % 2))), streaks) for i in range(3)]

    def slash(i):
        ang_f = [2.8, 2.0, .9, .2, -.4, .6][i]
        return dict(bend=[.8, .6, .3, .5, .3, .3][i], lean=[-3, -1, 3, 5, 4, 1][i], dy=[2, 0, -1, 1, 0, 0][i], armF=(ang_f, -.3 if i < 3 else .4, 1.2),
                    armB=(1.4, -.8, 1.0), cape=i * 1.0, hot=[.5, .8, 1, 1, .8, .4][i], flick=2.0)

    def swoosh_fx(i):
        def f(cv, P_):
            if i in (2, 3, 4):
                a0, a1 = [(-1.5, .5), (-.8, 1.4), (-.1, 2.0)][i - 2]
                _swoosh(cv, P_, (52, 50), 30, a0, a1, 4.0)
        return f
    A['slash'] = [frame_king(P, slash(i), swoosh_fx(i)) for i in range(6)]
    A['leap'] = [frame_king(P, dict(bend=1.0, dy=2, lean=-2, armF=(2.5, -.4, 1.0), armB=(2.2, -.4, 1.0), hot=.4)),
                 frame_king(P, dict(bend=-.2, dy=-10, lean=1, armF=(-1.0, .5, 1.1), armB=(-.6, .6, 1.05), wing=.7, step=(.8, .6), hot=.6, cape=3)),
                 frame_king(P, dict(bend=-.3, dy=-15, armF=(-.3, .8, 1.15), armB=(-1.4, .4, 1.1), wing=1, step=(.6, .8), hot=.8, cape=4.2)),
                 frame_king(P, dict(bend=.1, dy=-9, lean=2, armF=(.9, .7, 1.1), armB=(1.2, .5, 1.0), wing=.6, step=(.5, .4), hot=.6, cape=5))]
    A['land'] = [frame_king(P, dict(bend=1.1, dy=3, lean=1, squash=.92, armF=(1.5, -.2, 1.0), armB=(1.7, -.3, 1.0), cape=6)),
                 frame_king(P, dict(bend=.5, dy=1, armF=(1.2, -.6, 1.0), armB=(1.4, -.7, 1.0), cape=6.4))]

    def divestreaks(cv, P_):
        for k, xx in enumerate((38, 46, 54)):
            cv.put(line(xx, 4 + k * 2, xx, 30, .5), P_['flame'][1 + k % 2])
    A['dive'] = [frame_king(P, dict(bend=-.4, dy=-6 - i * 2, lean=4, armF=(1.25, .35, 1.2), armB=(1.0, .5, 1.1), wing=1, step=(.7, .6), hot=1, cape=3 + i, flick=2.5), divestreaks) for i in range(3)]
    A['throw'] = [frame_king(P, dict(bend=.6, lean=-3, armF=(2.8, -.2, 1.2), armB=(1.4, -.8, 1.0), hot=.6, cape=1)),
                  frame_king(P, dict(bend=.8, lean=-5, armF=(3.0, -.1, 1.2), armB=(1.6, -.8, 1.0), hot=.9, cape=2)),
                  frame_king(P, dict(bend=.3, lean=5, armF=(.3, .1, 0.0), armB=(1.4, -.8, 1.0), hot=1.0, cape=3)),
                  frame_king(P, dict(bend=.3, lean=4, armF=(.1, .1, 0.0), armB=(1.3, -.8, 1.0), hot=.7, cape=4)),
                  frame_king(P, dict(bend=.2, lean=2, armF=(.6, .1, 0.0), armB=(1.2, -.8, 1.0), hot=.4, cape=5)),
                  frame_king(P, dict(bend=.2, lean=1, armF=(.8, .1, 0.0), armB=(1.2, -.8, 1.0), hot=.3, cape=6))]
    A['hurt'] = [frame_king(P, dict(lean=-4, dy=2, bend=.7, armF=(1.7, .2, 1.0), armB=(2.0, .3, 1.0), eyes='hurt', hurt=True, cape=2, hot=.6)),
                 frame_king(P, dict(lean=-6, dy=3, bend=.9, armF=(1.9, .3, 1.0), armB=(2.2, .4, 1.0), eyes='hurt', hurt=True, cape=3, hot=.3))]

    def rage_fx(i):
        def f(cv, P_):
            rng = random.Random(i)
            for k in range(10 + i * 4):
                a = rng.random() * TAU; d = 22 + rng.random() * (10 + i * 4)
                x, y = 48 + C(a) * d, 48 + S(a) * d * .8
                cv.put([(int(x), int(y))], P_['flame'][rng.randrange(1, 4)])
        return f
    A['rage'] = [frame_king(P, dict(bend=.3 + .1 * S(i), lean=-2, dy=-1, armF=(-.5 + .1 * i, .6, 1.2), armB=(-.9, .5, 1.15), wing=1, hot=1, flick=2 + i * .8, cape=i * 1.3), rage_fx(i)) for i in range(6)]
    A['death'] = death_frames(P)
    return A


def death_frames(P):
    """Ten frames: the king staggers, cracks leak light, then comes apart and the pieces settle into a smouldering heap."""
    out = []
    rng = random.Random(9)
    for i in range(10):
        t = i / 9.0
        if t < .42:
            k = t / .42

            def fx(cv, P_, k=k, i=i):
                r2 = random.Random(i)
                for _ in range(int(6 + 14 * k)):
                    cv.put([(48 + r2.randint(-12, 12), 52 + r2.randint(-22, 14))], P_['flame'][r2.randrange(1, 4)])
            out.append(frame_king(P, dict(lean=-3 - 3 * k + 2 * S(i * 3), dy=2 + int(4 * k), bend=.5 + k * .8, armF=(1.6 + k, .4, 1.0), armB=(2.0, .3, 1.0), eyes='hurt',
                                          hurt=True, hot=1, cape=i, flick=2 + k * 2, squash=1 - .1 * k), fx))
        else:
            k = (t - .42) / .58
            cv = Canvas(CW, CH)
            gy = GROUND

            def piece(m, pal_, dx, dyy, hi=True):
                part(cv, {(x + int(dx), y + int(dyy)) for (x, y) in m}, pal_, hi=hi, k=1)
            fall = k * k
            cx, cy = 48, gy - 24
            head_m = poly([(-8, -7), (8, -7), (6, 1), (1, 9), (-5, 3)])
            piece(head_m, P['armor'], cx + 4 + 14 * k + 2 * S(k * 6), min(gy - 10, cy - 38 + 40 * fall))
            crown_m = rect(-6, 0, 7, 3) | poly([(-5, 0), (-3, -5), (-1, 0)]) | poly([(1, 0), (3, -5), (5, 0)])
            piece(crown_m, P['trim'], cx - 12 - 16 * k, min(gy - 4, cy - 44 + 46 * fall))
            chest_y = int(min(gy - 12, cy - 16 + 14 * fall))
            piece(ell(0, 0, 10, 12), P['armor'], cx - 2, chest_y)
            glow = max(0.0, 1 - k)
            cv.put({(x + cx - 1, y + chest_y + 1) for (x, y) in ell(0, 0, 4, 4)}, mix(P['glow'][:3], (60, 14, 8), 1 - glow))
            for sx_, sd in ((-1, -22), (1, 24)):
                piece(ell(0, 0, 11, 2.4), P['blade'], cx + sd * k * 1.1, min(gy - 3, cy - 18 + 22 * fall + (4 if sx_ > 0 else 0)), hi=False)
            for sx_ in (-1, 1):
                piece(rect(-2, -8, 3, 8), P['armor'], cx + sx_ * (6 + 10 * k), min(gy - 8, cy + 6 + 8 * fall), hi=False)
            for q in range(int(10 * (1 - k)) + 2):
                cv.put([(cx + rng.randint(-26, 26), gy - rng.randint(0, 18))], P['flame'][rng.randrange(1, 4)])
            cv.put({(x, gy + 1) for x in range(cx - 22, cx + 24, 3)}, hexc('#241018'))
            out.append(cv)
    return out


ANIMS = {'idle': (10, True), 'dance': (10, True), 'step': (14, True), 'windup': (8, False), 'dash': (16, True), 'slash': (14, False), 'leap': (10, False),
         'land': (12, False), 'dive': (12, True), 'throw': (12, False), 'hurt': (10, False), 'rage': (11, False), 'death': (9, False)}


def build(name):
    P = make_pal(name)
    A = seq(P)
    frames = []; order = []
    for k in ANIMS:
        order.append((k, len(frames), len(A[k])))
        frames += A[k]
    return frames, order


# ------------------------------------------------------------------------------------------ projectiles and fx
def scythe_frames(n=8):
    """The thrown scythe: a spinning crescent with a glowing edge (32x32)."""
    P = make_pal('brasa')
    out = []
    for i in range(n):
        a = i / n * TAU
        cv = Canvas(32, 32)
        c = (16, 16)
        pts = [polar(c, a + (k / 15.0 - .5) * 2.2, 10) for k in range(16)]
        m = set()
        for k, (x, y) in enumerate(pts):
            w = 1.2 + 2.6 * math.sin(k / 15.0 * math.pi)
            m |= ell(x, y, w, w)
        part(cv, m, P['blade'], hi=True, k=1)
        for k, (x, y) in enumerate(pts):
            if 2 < k < 13:
                ang = a + (k / 15.0 - .5) * 2.2
                cv.put([(int(x + C(ang) * 3.2), int(y + S(ang) * 3.2))], P['edge'] if k % 2 else P['eye_hot'])
        cv.put(ell(16, 16, 2.2, 2.2), hexc('#2a1c20')); cv.put({(16, 16)}, P['edge'])
        halo(cv, 16, 16, 15, hexc('#8a3418'), .18)
        out.append(cv)
    return out


def slash_arc(i, n=5):
    """A big fan of flame that marks a scythe sweep (64x64)."""
    P = make_pal('brasa')
    cv = Canvas(64, 64); t = i / (n - 1.0)
    a1 = -.2 - 1.3 * min(1.0, t * 1.6)
    r = 22 + t * 12
    _swoosh(cv, P, (14, 56), r, 0.1, a1, 6 * (1 - t * .6))
    _swoosh(cv, P, (14, 56), r - 9, 0.1, a1 * .9, 3 * (1 - t * .6))
    if t > .5:
        for k in range(8):
            a = .1 + (a1 - .1) * (k / 7.0)
            x, y = polar((14, 56), a, r + 5 + 8 * (t - .5))
            cv.put([(int(x), int(y))], P['flame'][1 + k % 2])
    return cv


def shock_fire(i, n=6):
    """A low wave of flame that races along the floor (64x20)."""
    cv = Canvas(64, 20)
    for x in range(64):
        h = 4 + 10 * abs(S((x + i * 11) * .21)) * (1 - abs(x - 32) / 40.0)
        for y in range(int(20 - h), 20):
            e = (20 - y) / max(1.0, h)
            cv.put([(x, y)], LAVA[3] if e < .3 else (LAVA[2] if e < .55 else (LAVA[1] if e < .8 else LAVA[0])))
    return cv
