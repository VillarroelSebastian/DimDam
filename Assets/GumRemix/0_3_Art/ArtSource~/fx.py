import math
from px import *

S = math.sin
WHITE = hexc('#ffffff')
PINK, PINK_L, PINK_D, PINK_X = hexc('#ff79b8'), hexc('#ffd0ec'), hexc('#d8408e'), hexc('#43103f')

# ----------------------------------------------------------------------------- player gum shots
def draw_shot(d, i, n=4):
    cv = Canvas(24, 24)
    cx, cy = 12, 12
    vx, vy = {'fwd': (1, 0), 'diag': (.707, -.707), 'up': (0, -1)}[d]
    wob = S(i / n * 2 * math.pi)
    # stretched trail behind the blob (gets smaller away from head)
    for k in range(1, 6):
        r = max(.8, 3.6 - k * .6 + .3 * wob)
        cv.put(ell(cx - vx * k * 2.4, cy - vy * k * 2.4, r, r), PINK if k < 3 else PINK_D)
    head = ell(cx + vx * 2, cy + vy * 2, 4.6 + .4 * wob, 4.6 - .4 * wob)
    cv.put(outline_of(head), PINK_X); cv.put(head, PINK)
    cv.put({p for p in head if (p[0] + 1, p[1] + 1) not in head}, PINK_D)
    cv.put({(int(cx + vx * 2 - 2), int(cy + vy * 2 - 2)), (int(cx + vx * 2 - 1), int(cy + vy * 2 - 2))}, WHITE)
    cv.put({(int(cx + vx * 2 - 2), int(cy + vy * 2 - 1))}, PINK_L)
    return cv

def draw_splat(i, n=5):
    cv = Canvas(24, 24); t = i / (n - 1)
    cx, cy = 12, 12
    if t < .6:
        r = 4 + t * 9
        m = ell(cx, cy, r, r * .9) - ell(cx, cy, max(0, r - 4 + t * 4), max(0, (r - 4 + t * 4) * .9))
        cv.put(outline_of(ell(cx, cy, r, r * .9)), PINK_X); cv.put(ell(cx, cy, r, r * .9), PINK); cv.put(m, PINK_D)
        cv.put({(cx - 3, cy - 3), (cx - 2, cy - 3)}, WHITE)
    for j in range(7):
        a = j / 7 * 2 * math.pi + 1
        d = 4 + t * 9
        cv.put(ell(cx + math.cos(a) * d, cy + math.sin(a) * d, max(.9, 2.4 - t * 2), max(.9, 2.4 - t * 2)), PINK if j % 2 else PINK_L)
    return cv

# ----------------------------------------------------------------------------- enemy bubbles
def draw_bubble(kind, i, n=4):
    cv = Canvas(24, 24); t = i / n * 2 * math.pi
    cx, cy = 12, 12
    r = 8 + .6 * S(t)
    m = ell(cx, cy, r, r - .6 * S(t))
    if kind == 'pink':
        out, base, hi, sh = PINK_X, hexc('#ff9ad0'), WHITE, PINK_D
        cv.put(outline_of(m), out); cv.put(m, base)
        cv.put({p for p in m if (p[0] + 2, p[1] + 2) not in m}, sh)
        cv.put(ell(cx, cy, r - 3, r - 3), hexc('#ffc0e2'))
        cv.put({(cx - 4, cy - 4), (cx - 3, cy - 5), (cx - 3, cy - 4)}, hi)
        # star mark = "parry me"
        cv.put({(cx, cy - 2), (cx - 1, cy - 1), (cx, cy - 1), (cx + 1, cy - 1), (cx - 2, cy), (cx - 1, cy), (cx, cy), (cx + 1, cy), (cx + 2, cy), (cx - 1, cy + 1), (cx + 1, cy + 1)}, WHITE if i % 2 else PINK_L)
    else:
        out, base, sh = hexc('#1c0a38'), hexc('#6a3ac8'), hexc('#43208a')
        cv.put(outline_of(m), out); cv.put(m, base)
        cv.put({p for p in m if (p[0] + 2, p[1] + 2) not in m}, sh)
        cv.put({(cx - 4, cy - 4), (cx - 3, cy - 5), (cx - 3, cy - 4)}, hexc('#b8a0ff'))
        cv.put({(cx - 2, cy), (cx - 1, cy), (cx + 1, cy), (cx + 2, cy), (cx - 1, cy + 1), (cx, cy + 1), (cx + 1, cy + 1)}, hexc('#ff4a6a'))  # angry slit
    return cv

def draw_bubble_pop(kind, i, n=4):
    cv = Canvas(24, 24); t = i / (n - 1); cx, cy = 12, 12
    col = [hexc('#ffc0e2'), WHITE, PINK] if kind == 'pink' else [hexc('#b8a0ff'), hexc('#6a3ac8'), hexc('#ff4a6a')]
    for j in range(8):
        a = j / 8 * 2 * math.pi
        d = 4 + t * 8
        cv.put(ell(cx + math.cos(a) * d, cy + math.sin(a) * d, max(.8, 2.4 - t * 2), max(.8, 2.4 - t * 2)), col[j % 3])
    if t < .5: cv.put(ring(ell(cx, cy, 6 + t * 10, 6 + t * 10)), WHITE)
    return cv

# ----------------------------------------------------------------------------- small effects
def draw_dust(i, n=6):
    cv = Canvas(32, 24); t = i / (n - 1)
    for side in (-1, 1):
        for j in range(3):
            d = 3 + t * (9 + j * 3)
            r = max(.9, (3.2 - j * .6) * (1 - t * .7) + .4)
            cv.put(ell(16 + side * d, 20 - t * (3 + j * 2) - j, r, r), hexc('#fff0f6') if j != 1 else hexc('#ffc0dc'))
    return cv

def draw_spark(i, n=5):
    cv = Canvas(32, 32); t = i / (n - 1); cx, cy = 16, 16
    L = 5 + t * 9 if t < .8 else 14
    th = max(1, 3 - t * 3)
    for a in (0, math.pi / 4, math.pi / 2, 3 * math.pi / 4):
        cv.put(line(cx - math.cos(a) * L, cy - math.sin(a) * L, cx + math.cos(a) * L, cy + math.sin(a) * L, th * .5), WHITE if a in (0, math.pi / 2) else hexc('#ffe36a'))
    cv.put(ell(cx, cy, max(0.5, 4 - t * 4), max(0.5, 4 - t * 4)), WHITE)
    return cv

def draw_parry(i, n=6):
    cv = Canvas(48, 48); t = i / (n - 1); cx, cy = 24, 24
    r = 6 + t * 17
    cv.put(ring(ell(cx, cy, r, r)), hexc('#ff79b8')); cv.put(ring(ell(cx, cy, max(1, r - 3), max(1, r - 3))), WHITE)
    for k in range(8):
        a = k / 8 * 2 * math.pi + t
        d = r + 2
        cv.put(ell(cx + math.cos(a) * d, cy + math.sin(a) * d, max(.8, 2.6 - t * 2), max(.8, 2.6 - t * 2)), hexc('#ffe36a') if k % 2 else WHITE)
    return cv

def draw_flash(i, n=3):
    cv = Canvas(16, 16); r = [5, 3.4, 1.6][i]
    cv.put(ell(8, 8, r + 1.5, r + 1.5), hexc('#ff9ad0')); cv.put(ell(8, 8, r, r), WHITE)
    return cv

def draw_bubble_lift(i, n=4):
    cv = Canvas(64, 72); t = i / n * 2 * math.pi
    cx, cy = 32, 36
    rx, ry = 26 + 1.2 * S(t), 29 - 1.2 * S(t)
    m = ell(cx, cy, rx, ry)
    cv.put(outline_of(m), hexc('#9a80e8'))
    cv.put({p for p in m if (p[0] + 1, p[1] + 1) not in m}, hexc('#c8b4ff'))
    cv.put({(p[0], p[1]) for p in m if (p[0] - 1, p[1] - 1) not in m and p[0] < cx and p[1] < cy}, hexc('#ffffff'))
    cv.put({(cx - 15 + k, cy - 20 + int(k * .6)) for k in range(6)}, WHITE)
    cv.put({(cx + 17, cy + 10), (cx + 17, cy + 11), (cx + 16, cy + 12)}, hexc('#ffffff'))
    return cv

def draw_shock(i, n=6):
    cv = Canvas(64, 20); t = i / (n - 1)
    h = 4 + int((1 - abs(t - .4)) * 10)
    for side in (-1, 1):
        for k in range(6):
            x = 32 + side * (3 + k * 5 * (.4 + t))
            hh = max(2, int(h - k * 1.3))
            if x < 0 or x >= 64: continue
            cv.put(rect(x - 2, 19 - hh, x + 2, 19), hexc('#ff79b8')); cv.put(rect(x - 2, 19 - hh, x - 1, 19), hexc('#ffd0ec'))
            cv.put({(x - 2, 19 - hh - 1), (x - 1, 19 - hh - 1), (x, 19 - hh - 1), (x + 1, 19 - hh - 1)}, PINK_X)
    return cv

def draw_super(i, n=6):
    cv = Canvas(96, 40); t = i / n * 2 * math.pi
    for x in range(96):
        h = 14 + 3 * S(x * .25 + t * 2)
        for y in range(int(20 - h), int(20 + h)):
            edge = y < 20 - h + 2 or y >= 20 + h - 2
            cv.put([(x, y)], PINK_X if (y == int(20 - h) or y == int(20 + h) - 1) else (hexc('#ffd0ec') if edge else (WHITE if abs(y - 20) < 5 else PINK)))
    return cv

def draw_trail(i, n=4):
    cv = Canvas(16, 16); t = i / (n - 1)
    cv.put(ell(8, 8, max(1, 5 - t * 4), max(1, 5 - t * 4)), hexc('#ffd0ec')); return cv

# ----------------------------------------------------------------------------- UI icons
def draw_heart(full, i=0):
    cv = Canvas(24, 24)
    base = PINK if full else hexc('#3a2a58'); hi = PINK_L if full else hexc('#54406e'); sh = PINK_D if full else hexc('#2a1c44')
    m = ell(8.5, 10, 6, 6) | ell(15.5, 10, 6, 6) | poly([(3, 12), (21, 12), (12, 22)])
    cv.put(outline_of(m), PINK_X if full else hexc('#1a1030')); cv.put(m, base)
    cv.put({p for p in m if (p[0] + 2, p[1] + 2) not in m}, sh)
    cv.put({(6, 6), (7, 5), (6, 7)}, hi)
    if full and i == 1: cv.put({(6, 6), (7, 5), (8, 5)}, WHITE)
    return cv

def draw_card(state):
    cv = Canvas(16, 24)
    r = rect(1, 1, 15, 23)
    cv.put(outline_of(r), PINK_X)
    if state == 0: cv.put(r, hexc('#3a2a58')); cv.put(rect(2, 2, 14, 3), hexc('#54406e'))
    elif state == 1: cv.put(r, hexc('#fff0f6')); cv.put(rect(1, 14, 15, 23), PINK_L); cv.put(rect(3, 4, 13, 12), PINK)
    else:
        cv.put(r, WHITE); cv.put(rect(3, 4, 13, 12), hexc('#ffe36a'))
    cv.put({(2 + k, 2) for k in range(3)}, WHITE)
    return cv

# ----------------------------------------------------------------------------- 5x7 pixel font
GLYPHS = {
'A': ['01110','10001','10001','11111','10001','10001','10001'], 'B': ['11110','10001','10001','11110','10001','10001','11110'],
'C': ['01110','10001','10000','10000','10000','10001','01110'], 'D': ['11110','10001','10001','10001','10001','10001','11110'],
'E': ['11111','10000','10000','11110','10000','10000','11111'], 'F': ['11111','10000','10000','11110','10000','10000','10000'],
'G': ['01110','10001','10000','10111','10001','10001','01111'], 'H': ['10001','10001','10001','11111','10001','10001','10001'],
'I': ['01110','00100','00100','00100','00100','00100','01110'], 'J': ['00111','00010','00010','00010','00010','10010','01100'],
'K': ['10001','10010','10100','11000','10100','10010','10001'], 'L': ['10000','10000','10000','10000','10000','10000','11111'],
'M': ['10001','11011','10101','10101','10001','10001','10001'], 'N': ['10001','11001','10101','10011','10001','10001','10001'],
'O': ['01110','10001','10001','10001','10001','10001','01110'], 'P': ['11110','10001','10001','11110','10000','10000','10000'],
'Q': ['01110','10001','10001','10001','10101','10010','01101'], 'R': ['11110','10001','10001','11110','10100','10010','10001'],
'S': ['01111','10000','10000','01110','00001','00001','11110'], 'T': ['11111','00100','00100','00100','00100','00100','00100'],
'U': ['10001','10001','10001','10001','10001','10001','01110'], 'V': ['10001','10001','10001','10001','10001','01010','00100'],
'W': ['10001','10001','10001','10101','10101','11011','10001'], 'X': ['10001','10001','01010','00100','01010','10001','10001'],
'Y': ['10001','10001','01010','00100','00100','00100','00100'], 'Z': ['11111','00001','00010','00100','01000','10000','11111'],
'0': ['01110','10001','10011','10101','11001','10001','01110'], '1': ['00100','01100','00100','00100','00100','00100','01110'],
'2': ['01110','10001','00001','00010','00100','01000','11111'], '3': ['11110','00001','00001','01110','00001','00001','11110'],
'4': ['00010','00110','01010','10010','11111','00010','00010'], '5': ['11111','10000','11110','00001','00001','10001','01110'],
'6': ['00110','01000','10000','11110','10001','10001','01110'], '7': ['11111','00001','00010','00100','01000','01000','01000'],
'8': ['01110','10001','10001','01110','10001','10001','01110'], '9': ['01110','10001','10001','01111','00001','00010','01100'],
'!': ['00100','00100','00100','00100','00100','00000','00100'], '?': ['01110','10001','00001','00110','00100','00000','00100'],
'.': ['00000','00000','00000','00000','00000','01100','01100'], ':': ['00000','01100','01100','00000','01100','01100','00000'],
'-': ['00000','00000','00000','11111','00000','00000','00000'], "'": ['00100','00100','01000','00000','00000','00000','00000'],
'/': ['00001','00010','00010','00100','01000','01000','10000'], 'x': ['00000','00000','10001','01010','00100','01010','10001'],
',': ['00000','00000','00000','00000','01100','00100','01000'], ' ': ['00000']*7, '+': ['00000','00100','00100','11111','00100','00100','00000'],
'%': ['11001','11010','00010','00100','01000','01011','10011'],
'<': ['00010','00100','01000','10000','01000','00100','00010'], '>': ['01000','00100','00010','00001','00010','00100','01000'],
'=': ['00000','00000','11111','00000','11111','00000','00000'], '(': ['00010','00100','01000','01000','01000','00100','00010'],
')': ['01000','00100','00010','00010','00010','00100','01000'], '*': ['00000','10101','01110','11111','01110','10101','00000'],
'_': ['00000','00000','00000','00000','00000','00000','11111'],
}
FONT_ORDER = list(GLYPHS.keys())
def draw_font():
    cw, ch = 6, 8
    cols = 16
    rows = (len(FONT_ORDER) + cols - 1) // cols
    cv = Canvas(cols * cw, rows * ch)
    for idx, c in enumerate(FONT_ORDER):
        ox, oy = (idx % cols) * cw, (idx // cols) * ch
        for y, row in enumerate(GLYPHS[c]):
            for x, v in enumerate(row):
                if v == '1': cv.put([(ox + x, oy + y)], WHITE)
    return cv, cw, ch, cols
