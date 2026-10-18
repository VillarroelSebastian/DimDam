"""Builds every PNG, its .meta (Point filter, uncompressed) and manifest.json into ../Resources/GumRemix/<category>/.
Run:  python build_all.py"""
import os, json, hashlib, sys, shutil
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from px import *
import heroes, enemies, fx, boss, env_bg, env_props
import ice_enemies as ie, ice_fx, ice_boss, ice_env, ice_props
import armor
import env_gothic
import ice_king
from grade import grade

HERE = os.path.dirname(os.path.abspath(__file__))
OUT = os.path.normpath(os.path.join(HERE, '..', 'Resources', 'GumRemix'))
os.makedirs(OUT, exist_ok=True)

META = open(os.path.join(HERE, 'texture_meta.template'), encoding='utf-8').read()

# start clean: stale PNGs from older layouts must not survive (folder .meta files are left to Unity)
for root, dirs, files in os.walk(OUT):
    for f in files:
        if f.endswith('.png') or f.endswith('.png.meta') or f.startswith('manifest.json'):
            os.remove(os.path.join(root, f))

manifest = []
FOLDER = ''
GRADE = None          # per-folder colour grade (dict for grade.grade) and the sheets that keep their own colours
GRADE_SKIP = ()

def folder(name):
    global FOLDER
    FOLDER = name

def guid_for(rel): return hashlib.md5(('gumremix/' + rel).encode()).hexdigest()

def save_png(name, im):
    d = os.path.join(OUT, FOLDER) if FOLDER else OUT
    os.makedirs(d, exist_ok=True)
    path = os.path.join(d, name + '.png')
    im.save(path)
    open(path + '.meta', 'w', newline='\n').write(META.format(guid=guid_for(FOLDER + '/' + name + '.png')))

def register(name, frames, cw, ch, cols, pivot=(0.5, 0.0), anims=None, extra=None, white=False, ppu=40):
    ims = [f.im if isinstance(f, Canvas) else f for f in frames]
    if GRADE is not None and not name.startswith(GRADE_SKIP):
        ims = [grade(im, **GRADE) for im in ims]
    sh = sheet(ims, cols, cw, ch)
    save_png(name, sh)
    rel = ('GumRemix/' + FOLDER + '/' if FOLDER else 'GumRemix/') + name
    entry = dict(name=name, file=rel, cw=cw, ch=ch, cols=cols, count=len(ims), ppu=ppu, px=pivot[0], py=pivot[1],
                 anims=[dict(name=a[0], start=a[1], count=a[2], fps=float(a[3]), loop=bool(a[4])) for a in (anims or [])])
    if extra: entry.update(extra)
    manifest.append(entry)
    if white:
        wsh = silhouette(sh)
        save_png(name + '_w', wsh)
        e2 = dict(entry); e2['name'] = name + '_w'; e2['file'] = rel + '_w'
        manifest.append(e2)
    print(f'{FOLDER:14s}{name:22s} {len(ims):3d} frames  {sh.width}x{sh.height}  colours={ncolors(sh)}')

def anims_from(order, table):
    return [(n, s, c, table[n][0], table[n][1]) for (n, s, c) in order]

def seq_anims(d, table):
    out = []; idx = 0; frames = []
    for k, v in d.items():
        out.append((k, idx, len(v), table[k][0], table[k][1])); idx += len(v); frames += v
    return frames, out

def reg_loop(name, frames, fps, pivot=(0.5, 0.0), pad=None):
    if pad: frames = [pad_canvas(f, *pad) for f in frames]
    register(name, frames, frames[0].w, frames[0].h, len(frames), pivot, [('loop', 0, len(frames), fps, True)])

# =============================================================================================== HEROES
folder('Heroes')
for hname, pal, kind in (('celeste', heroes.CELESTE, 'celeste'), ('coral', heroes.CORAL, 'coral')):
    for suffix, winter in (('', False), ('_ice', True)):
        fr, fr_na, shoulders, order = heroes.build(pal, kind, winter)
        an = anims_from(order, heroes.ANIMS)
        px_ = (0.5, heroes.PIVOT_Y)
        sh_flat = [float(v) for s in shoulders for v in s]
        register('hero_' + hname + suffix, fr, heroes.W, heroes.H, 10, px_, an, dict(shoulders=sh_flat))
        register('hero_' + hname + suffix + '_noarm', fr_na, heroes.W, heroes.H, 10, px_, an, dict(shoulders=sh_flat))
        arms, info = heroes.build_arms(pal, winter)
        flat = []
        for a in info: flat += [a['anchor'][0], a['anchor'][1], a['muzzle'][0], a['muzzle'][1]]
        register('arm_' + hname + suffix, arms, heroes.AW, heroes.AH, 9, (8 / heroes.AW, 1 - 14 / heroes.AH), None, dict(armdata=flat))
    ghosts = [heroes.draw_ghost(pal, i / 6 * 2 * 3.14159265) for i in range(6)]
    register('ghost_' + hname, ghosts, 40, 44, 6, (0.5, 0.5), [('float', 0, 6, 8, True)])
register('freeze', [ice_fx.draw_freeze(i) for i in range(4)], 52, 64, 4, (0.5, 0.5), [('freeze', 0, 4, 8, True)])
register('freeze_break', [ice_fx.draw_freeze_break(i) for i in range(5)], 52, 64, 5, (0.5, 0.5), [('break', 0, 5, 16, False)])

# =============================================================================================== CANDY ENEMIES
folder('Enemies/Candy')
for nm, pal in (('red', enemies.BEAR_RED), ('green', enemies.BEAR_GREEN), ('orange', enemies.BEAR_ORANGE)):
    fr, an = seq_anims(enemies.bear_frames(pal), enemies.BEAR_ANIMS)
    register('bear_' + nm, fr, enemies.BW, enemies.BH, 10, (0.5, 1 / enemies.BH), an, white=True)
    bf, ban = seq_anims(enemies.ball_frames(pal), enemies.BALL_ANIMS)
    register('ball_bear_' + nm, bf, enemies.GW, enemies.GH, 9, (0.5, 7 / enemies.GH), ban, white=True)
for nm, pal in (('yellow', enemies.BEAN_YELLOW), ('purple', enemies.BEAN_PURPLE)):
    fr, an = seq_anims(enemies.bean_frames(pal), enemies.BEAN_ANIMS)
    register('bean_' + nm, fr, enemies.JW, enemies.JH, 10, (0.5, 1.5 / enemies.JH), an, white=True)
    bf, ban = seq_anims(enemies.ball_frames(pal), enemies.BALL_ANIMS)
    register('ball_bean_' + nm, bf, enemies.GW, enemies.GH, 9, (0.5, 7 / enemies.GH), ban, white=True)
# steel bear: only the rope can capture it (gum bounces off)
STEEL = enemies.pal_of('#7e8cae', '#dce6f6', '#56628a', '#343e5a', '#161c2c', '#46527a')
fr, an = seq_anims(enemies.bear_frames(enemies.BEAR_RED), enemies.BEAR_ANIMS)
fr = [armor.armorize(f.im if isinstance(f, Canvas) else f) for f in fr]
register('bear_steel', fr, enemies.BW, enemies.BH, 10, (0.5, 1 / enemies.BH), an, white=True)
bf, ban = seq_anims(enemies.ball_frames(STEEL), enemies.BALL_ANIMS)
register('ball_bear_steel', bf, enemies.GW, enemies.GH, 9, (0.5, 7 / enemies.GH), ban, white=True)
fr, an = seq_anims(enemies.lolli_frames(enemies.LOLLI_PINK), enemies.LOLLI_ANIMS)
register('lolli', fr, enemies.LW, enemies.LH, 10, (0.5, 1 - 24 / enemies.LH), an, white=True)
bf, ban = seq_anims(enemies.ball_frames(enemies.LOLLI_PINK), enemies.BALL_ANIMS)
register('ball_lolli', bf, enemies.GW, enemies.GH, 9, (0.5, 7 / enemies.GH), ban, white=True)
fr, an = seq_anims(enemies.cannon_frames(), enemies.CANNON_ANIMS)
register('cannon', fr, enemies.CW, enemies.CH, 7, (0.5, 1 / enemies.CH), an, white=True)
bf, ban = seq_anims(enemies.ball_frames(enemies.SUGAR), enemies.BALL_ANIMS)
register('ball_cannon', bf, enemies.GW, enemies.GH, 9, (0.5, 7 / enemies.GH), ban, white=True)

# =============================================================================================== ICE ENEMIES
folder('Enemies/Ice')
GRADE = dict(dark=.72, curve=.30, sat=.62, tint=(-3, 2, 10)); GRADE_SKIP = ('ball_', 'snowhead')
def reg_creature(name, fam, anims, pivot=None, cols=10, inner=None):
    fr, an = seq_anims(fam, anims)
    cw, ch = fr[0].w, fr[0].h
    pv = pivot if pivot is not None else (0.5, feet_pivot(fr[0]))
    register(name, fr, cw, ch, cols, pv, an, white=True)
    bf, ban = seq_anims(ie.snowball_frames(inner), ie.BALL_ANIMS)
    register('ball_' + name, bf, ie.GW, ie.GH, 9, (0.5, 7 / ie.GH), ban, white=True)
reg_creature('penguin', ie.penguin_frames(), ie.PENGUIN_ANIMS, inner=ie.BALL_INNER['penguin'])
reg_creature('snowman', ie.snowman_frames(), ie.SNOWMAN_ANIMS, inner=ie.BALL_INNER['snowman'])
# iron snowman: only the rope can capture it
fr, an = seq_anims(ie.snowman_frames(), ie.SNOWMAN_ANIMS)
fp0 = feet_pivot(fr[0])
fr = [armor.armorize(f.im if isinstance(f, Canvas) else f, helm=0.34, chest=(0.50, 0.97), chest_inset=0.04, seams=False, gamma=1.9) for f in fr]
register('snowman_iron', fr, fr[0].width, fr[0].height, 10, (0.5, fp0), an, white=True)
bf, ban = seq_anims(ie.snowball_frames(ie.BALL_INNER['snowman']), ie.BALL_ANIMS)
register('ball_snowman_iron', bf, ie.GW, ie.GH, 9, (0.5, 7 / ie.GH), ban, white=True)
reg_creature('owl', ie.owl_frames(), ie.OWL_ANIMS, pivot=(0.5, 0.5), inner=ie.BALL_INNER['owl'])
reg_creature('jelly', ie.jelly_frames(), ie.JELLY_ANIMS, pivot=(0.5, 1 - 24 / 72.0), inner=ie.BALL_INNER['jelly'])
reg_creature('crab', ie.crab_frames(), ie.CRAB_ANIMS, inner=ie.BALL_INNER['crab'])
reg_creature('seed', ie.seed_frames(), ie.SEED_ANIMS, inner=ie.BALL_INNER['seed'])
register('snowhead', ie.snow_head_frames(8), 28, 28, 8, (0.5, 0.5), [('spin', 0, 8, 16, True)])

# =============================================================================================== BOSSES
folder('Bosses')
GRADE = None
bfr, border = boss.build_boss()
register('boss', bfr, boss.BW, boss.BH, 10, (0.5, boss.PIVOT_Y), anims_from(border, boss.BOSS_ANIMS), white=True)
register('boss_hat', boss.hat_frames(8), 48, 48, 8, (0.5, 0.5), [('spin', 0, 8, 14, True)])
ifr, iorder, ianch = ice_king.build_boss()
anch = []
for a in ianch: anch += [float(a[0]), float(a[1])]
register('icemonarch', ifr, ice_boss.CW, ice_boss.CHH, 10, (0.5, feet_pivot(ifr[0])), anims_from(iorder, ice_boss.ANIMS), dict(anchors=anch), white=True)

# =============================================================================================== FX & UI
folder('Fx')
shots = [fx.draw_shot(d, i) for d in ('fwd', 'diag', 'up') for i in range(4)]
register('shots', shots, 24, 24, 4, (0.5, 0.5), [('fwd', 0, 4, 20, True), ('diag', 4, 4, 20, True), ('up', 8, 4, 20, True)])
register('splat', [fx.draw_splat(i) for i in range(5)], 24, 24, 5, (0.5, 0.5), [('splat', 0, 5, 24, False)])
bub = [fx.draw_bubble('pink', i) for i in range(4)] + [fx.draw_bubble('dark', i) for i in range(4)]
register('bubbles', bub, 24, 24, 4, (0.5, 0.5), [('pink', 0, 4, 10, True), ('dark', 4, 4, 10, True)])
pops = [fx.draw_bubble_pop('pink', i) for i in range(4)] + [fx.draw_bubble_pop('dark', i) for i in range(4)]
register('bubble_pops', pops, 24, 24, 4, (0.5, 0.5), [('pink', 0, 4, 20, False), ('dark', 4, 4, 20, False)])
register('dust', [fx.draw_dust(i) for i in range(6)], 32, 24, 6, (0.5, 0.0), [('dust', 0, 6, 20, False)])
register('spark', [fx.draw_spark(i) for i in range(5)], 32, 32, 5, (0.5, 0.5), [('spark', 0, 5, 26, False)])
register('parry', [fx.draw_parry(i) for i in range(6)], 48, 48, 6, (0.5, 0.5), [('parry', 0, 6, 24, False)])
register('flash', [fx.draw_flash(i) for i in range(3)], 16, 16, 3, (0.5, 0.5), [('flash', 0, 3, 30, False)])
register('bubble_lift', [fx.draw_bubble_lift(i) for i in range(4)], 64, 72, 4, (0.5, 0.5), [('lift', 0, 4, 8, True)])
register('shock', [fx.draw_shock(i) for i in range(6)], 64, 20, 6, (0.5, 0.0), [('shock', 0, 6, 16, False)])
register('super', [fx.draw_super(i) for i in range(6)], 96, 40, 6, (0.0, 0.5), [('super', 0, 6, 18, True)])
register('trail', [fx.draw_trail(i) for i in range(4)], 16, 16, 4, (0.5, 0.5), [('trail', 0, 4, 16, False)])
# ice
register('ice_shard', [ice_fx.draw_shard(i) for i in range(6)], 24, 24, 6, (0.5, 0.5), [('spin', 0, 6, 18, True)])
register('ice_flake', [ice_fx.draw_flake(i) for i in range(6)], 28, 28, 6, (0.5, 0.5), [('spin', 0, 6, 12, True)])
register('icicle', [ice_fx.draw_icicle(i) for i in range(3)], 20, 44, 3, (0.5, 0.5), [('shake', 0, 3, 24, True)])
register('icicle_shatter', [ice_fx.draw_icicle_shatter(i) for i in range(5)], 40, 32, 5, (0.5, 0.0), [('shatter', 0, 5, 20, False)])
register('crystal_off', [ice_fx.draw_crystal(i, False) for i in range(6)], 36, 60, 6, (0.5, 2 / 60.0), [('loop', 0, 6, 4, True)])
register('crystal_on', [ice_fx.draw_crystal(i, True) for i in range(6)], 36, 60, 6, (0.5, 2 / 60.0), [('loop', 0, 6, 9, True)])
register('ice_crack', [ice_fx.draw_crack(i) for i in range(3)], 40, 8, 3, (0.5, 0.0), [('loop', 0, 3, 10, True)])
register('ice_pillar', [ice_fx.draw_pillar_part('base'), ice_fx.draw_pillar_part('mid'), ice_fx.draw_pillar_part('tip')], 32, 22, 3, (0.5, 0.0))
register('shock_ice', [ice_fx.recolor_shock(fx.draw_shock(i)) for i in range(6)], 64, 20, 6, (0.5, 0.0), [('shock', 0, 6, 16, False)])
register('note', [ice_fx.draw_note(i) for i in range(3)], 14, 16, 3, (0.5, 0.5), [('loop', 0, 3, 6, True)])
register('gust', [ice_fx.draw_gust(i) for i in range(3)], 40, 6, 3, (0.5, 0.5), [('loop', 0, 3, 8, True)])

folder('Ui')
register('hearts', [fx.draw_heart(True, 0), fx.draw_heart(True, 1), fx.draw_heart(False)], 24, 24, 3, (0.5, 0.5))
register('cards', [fx.draw_card(0), fx.draw_card(1), fx.draw_card(2)], 16, 24, 3, (0.5, 0.5))
fcv, fcw, fch, fcols = fx.draw_font()
register('font', [fcv], fcv.w, fcv.h, 1, (0.0, 0.0), None, dict(chars=''.join(fx.FONT_ORDER), glyphW=fcw, glyphH=fch, glyphCols=fcols))

# =============================================================================================== CANDY WORLD
folder('World/Candy')
for t, fn in enumerate(env_bg.THEMES):
    sky, far, mid = fn()
    register(f'sky_{t}', [sky], env_bg.SW, env_bg.SH, 1, (0.5, 0.5))
    register(f'far_{t}', [far], far.w, far.h, 1, (0.5, 0.0))
    register(f'mid_{t}', [mid], mid.w, mid.h, 1, (0.5, 0.0))
    kinds = ['L', 'M0', 'M1', 'M2', 'R']
    register(f'plat_{t}', [env_bg.plat_tile(t, k) for k in kinds], env_bg.PW, env_bg.PH, 5, (0.5, 1.0))
    top, fill = env_bg.ground_tiles(t)
    register(f'ground_{t}', [top, fill], 40, 40, 2, (0.5, 1.0))
    register(f'wall_{t}', [env_bg.wall_tile(t)], 20, 40, 1, (0.5, 0.5))

P = env_props
for s in range(3): reg_loop(f'licorice_{s}', env_gothic.licorice_tree(8, s), 8)
reg_loop('spire', env_gothic.spire(8), 8)
for s in range(3): reg_loop(f'mushroom_{s}', env_gothic.glow_mushroom(6, s), 6)
pk = lambda *c: P.pal(*c)
register('clouds_0', [P.cloud(96, 40, 1, pk('#3a2068', '#7a4ab0', '#26104e', '#180a38', '#0e0624', '#5a3a98')), P.cloud(120, 44, 2, pk('#3a2068', '#7a4ab0', '#26104e', '#180a38', '#0e0624', '#5a3a98')), P.cloud(80, 34, 3, pk('#4a2a7a', '#8a5ac0', '#2e1658', '#1c0c40', '#0e0624', '#6a46a8'))], 120, 44, 3, (0.5, 0.5))
register('clouds_2', [P.cloud(128, 56, 4, pk('#5a3a88', '#e890c8', '#3e2868', '#2a1a4e', '#140a2e', '#c070b0')), P.cloud(144, 60, 5, pk('#4a3078', '#d880c0', '#321e5a', '#221444', '#120a28', '#b868a8')), P.cloud(100, 44, 6, pk('#68468e', '#f0a0d0', '#46306e', '#30205a', '#160c30', '#d078b8'))], 144, 60, 3, (0.5, 0.5))
for s in range(2): reg_loop(f'crystal_{s}', P.crystal(6, s), 6)
reg_loop('waterfall', P.waterfall(8), 10)
reg_loop('pot', P.caramel_pot(8), 10)
for s in range(4): reg_loop(f'lantern_{s}', env_gothic.lantern(6, s), 6)
reg_loop('star', P.star_twinkle(4), 5, (0.5, 0.5))
reg_loop('gear_l', P.gear(64, 10, 8, 0), 6, (0.5, 0.5))
reg_loop('gear_s', P.gear(40, 8, 8, 1), 9, (0.5, 0.5))
reg_loop('steam', P.steam_vent(8), 8)
reg_loop('vat', P.choc_vat(8), 8)
reg_loop('lamp', P.lamp(8), 6, (0.5, 1.0))
reg_loop('hall_chandelier', env_gothic.chandelier(8), 7, (0.5, 1.0))
reg_loop('hall_candelabra', env_gothic.candelabra(6), 8)
reg_loop('hall_curtain', env_gothic.curtain(8), 5, (0.5, 1.0))
register('beam', P.beam(), 64, 240, 1, (0.5, 1.0))
sp = P.sprinkle_sheet()
register('sprinkles', sp, 8, 8, 4, (0.5, 0.5))
bubf, bpops = P.bubble_frames()
register('bubble_small', bubf, 16, 16, 1, (0.5, 0.5))
register('bubble_small_pop', bpops, 16, 16, 4, (0.5, 0.5), [('pop', 0, 4, 18, False)])
register('glint', P.glint_frames(5), 11, 11, 5, (0.5, 0.5), [('glint', 0, 5, 14, False)])
register('ember', P.ember_frames(4), 8, 8, 4, (0.5, 0.5), [('pulse', 0, 4, 8, True)])
dr, drs = P.drip_frames()
register('drip', dr, 8, 12, 2, (0.5, 0.0), [('drip', 0, 2, 8, True)])
register('drip_splash', drs, 16, 8, 3, (0.5, 0.0), [('splash', 0, 3, 14, False)])

# =============================================================================================== ICE WORLD
folder('World/Ice')
GRADE = dict(dark=.46, curve=.28, sat=.60, tint=(-4, 2, 14)); GRADE_SKIP = ('sky_', 'wisp', 'glowcrystal_', 'spark_ice', 'snowflakes', 'gardenlamp', 'chandelier', 'dead_tree', 'ice_lamp')
for k, fn in enumerate(ice_env.THEMES):
    t = 5 + k
    sky, far, mid = fn()
    register(f'sky_{t}', [sky], ice_env.SW, ice_env.SH, 1, (0.5, 0.5))
    register(f'far_{t}', [far], far.w, far.h, 1, (0.5, 0.0))
    register(f'mid_{t}', [mid], mid.w, mid.h, 1, (0.5, 0.0))
    register(f'plat_{t}', [ice_env.ice_plat(k, kd) for kd in ('L', 'M0', 'M1', 'M2', 'R')], ice_env.PW, ice_env.PH, 5, (0.5, 1.0))
    sp_frames = [ice_env.special_tile(k, 'brittle', s) for s in range(3)] + [ice_env.special_tile(k, 'elastic', s) for s in range(3)] + [ice_env.special_tile(k, 'raft'), ice_env.special_tile(k, 'bridge')]
    register(f'special_{t}', sp_frames, ice_env.PW, ice_env.PH, 8, (0.5, 1.0))
    register(f'ground_{t}', [ice_env.ground_tile(k, 'ice'), ice_env.ground_tile(k, 'snow')], 40, 40, 2, (0.5, 1.0))
    register(f'wall_{t}', [ice_env.wall_tile(k)], 20, 40, 1, (0.5, 0.5))

IP = ice_props
aur = IP.aurora(8)
register('aurora', aur, 640, 150, 2, (0.5, 1.0), [('loop', 0, 8, 3, True)])
reg_loop('dead_tree', env_gothic.dead_tree(8, 0), 6)
reg_loop('dead_tree_b', env_gothic.dead_tree(8, 3), 6)
reg_loop('ice_lamp', env_gothic.ice_lamp(6), 7)
reg_loop('pine', IP.pine_tree(8, 0), 6, pad=(6, 4, 6, 0))
reg_loop('pine_b', IP.pine_tree(8, 2), 6, pad=(6, 4, 6, 0))
reg_loop('wisp', IP.wisp(6), 6, (0.5, 0.5), pad=(4, 2, 4, 2))
reg_loop('frozenfall', IP.frozen_fall(8), 10, (0.5, 1.0))
reg_loop('igloo', IP.igloo(8), 7, pad=(0, 10, 0, 0))
reg_loop('lanterns', IP.lanterns(8), 6, (0.5, 1.0))
reg_loop('washflags', IP.wash_flags(8), 7, (0.5, 1.0))
for s in range(3): reg_loop(f'glowcrystal_{s}', IP.glow_crystal(6, s), 6)
reg_loop('owlstatue', IP.owl_statue(6), 6)
reg_loop('gardenlamp', IP.garden_lamp(4), 7, pad=(0, 4, 0, 0))
reg_loop('chandelier', IP.chandelier(8), 6, (0.5, 1.0))
reg_loop('royalbanner', IP.royal_banner(8), 7, (0.5, 1.0))
reg_loop('throne', IP.throne(4), 3)
register('snowflakes', IP.snow_sheet(), 8, 8, 8, (0.5, 0.5))
register('spark_ice', IP.spark_ice(4), 12, 12, 4, (0.5, 0.5), [('spark', 0, 4, 12, False)])

folder('')
GRADE = None
json.dump(dict(sheets=manifest), open(os.path.join(OUT, 'manifest.json'), 'w'), indent=1)
open(os.path.join(OUT, 'manifest.json.meta'), 'w', newline='\n').write(
    'fileFormatVersion: 2\nguid: %s\nTextScriptImporter:\n  externalObjects: {}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n' % guid_for('manifest.json'))
print('sheets:', len(manifest))
