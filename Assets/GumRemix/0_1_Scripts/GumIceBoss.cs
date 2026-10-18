using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>
    /// Monarca del Deshielo. A snow creature with a captive sun in his chest: only the sun can be hurt, the snowy shell stops gum.
    /// Three forms (host in a rolling armchair, cracked giant, hollow storm). Attacks are small coroutines.
    /// </summary>
    public class IceBoss : BossBase
    {
        public const int Phase1Hp = 100, Phase2Hp = 100, Phase3Hp = 90;
        public const int MaxHp = Phase1Hp + Phase2Hp + Phase3Hp;
        public override int TotalHp { get { return MaxHp; } }
        public override string DisplayName { get { return "MONARCA DEL DESHIELO"; } }
        public override int[] PhaseFloors { get { return new[] { Phase2Hp + Phase3Hp, Phase3Hp }; } }

        GameObject go; SpriteRenderer sr; Anim anim; Sheet sheet;
        IEnumerator<float> routine; float wait, idleTimer = 1.4f, flashT, flashCd, t; int lastAttack = -1;
        float homeX = 4.4f, groundY; bool chargeLow; float deathT, explodeT; bool hailGapActive;
        readonly Rect[] hb = new Rect[1]; readonly List<Rect> dg = new List<Rect>(); Rect[] blocks = new Rect[1]; Rect[] balls = new Rect[2];
        const float Frame = 0.001f, Scale = 2f;
        /// <summary>While > 0 the snowy shell is cracked open and gum reaches the sun (after every attack); tests can force it shut.</summary>
        public float openT; public bool forceClosed; static readonly Rect[] NoBlock = new Rect[0];
        GameCtl G { get { return GameCtl.I; } }

        public IceBoss(float groundTop)
        {
            hp = MaxHp; groundY = groundTop; x = homeX; y = groundTop;
            sheet = Art.Get("icemonarch");
            sr = Pix.Make("icemonarch", G.entRoot, sheet[0], new Vector3(x, y, 0), 5, Scale);
            go = sr.gameObject; anim = new Anim(sr, sheet, "p1_idle");
        }

        string Pfx { get { return "p" + phase + "_"; } }

        // ------------------------------------------------------------------------------------------ geometry
        int AbsFrame()
        {
            if (sr == null || sr.sprite == null) return 0;
            var n = sr.sprite.name; int u = n.LastIndexOf('_'); int i; return int.TryParse(n.Substring(u + 1), out i) ? i : 0;
        }

        /// <summary>World position of the captive sun in the frame that is on screen right now.</summary>
        public Vector2 SunPos()
        {
            var d = sheet.def; int f = AbsFrame();
            float ax = d.anchors[f * 2], ay = d.anchors[f * 2 + 1];
            float lx = (ax - d.cw * 0.5f) / d.ppu * Scale;
            float ly = ((d.ch - ay) - d.py * d.ch) / d.ppu * Scale;
            return new Vector2(x + lx, y + ly);
        }

        public override Rect[] Hurtboxes()
        {
            var s = SunPos(); float r = 0.85f;
            hb[0] = new Rect(s.x - r, s.y - r, r * 2f, r * 2f);
            return hb;
        }

        Rect ShellRect()
        {
            if (phase == 3) return new Rect(x - 1.5f, y + 0.4f, 3.0f, 4.7f);
            if (phase == 2) return new Rect(x - 1.7f, y, 3.4f, 5.3f);
            return new Rect(x - 1.5f, y, 3.0f, 4.7f);
        }

        public override Rect[] BlockBoxes()
        {
            if (openT > 0f && !forceClosed) return NoBlock;
            blocks[0] = ShellRect();
            return blocks;
        }

        public override Rect[] BallBoxes()
        {
            balls[0] = ShellRect(); balls[1] = Hurtboxes()[0];
            return balls;
        }

        public override List<Rect> Dangerboxes()
        {
            dg.Clear();
            if (chargeLow) { dg.Add(new Rect(x - 1.3f, y, 2.6f, 1.6f)); return dg; }
            var b = ShellRect(); b.xMin += 0.6f; b.xMax -= 0.6f; b.yMin += 0.3f; b.yMax -= 1.0f;
            dg.Add(b);
            return dg;
        }

        public override void Activate() { active = true; idleTimer = 2.4f; openT = 4.5f; G.ShowHint("SU CORAZA SE ABRE: DISPARA AL SOL!", 3f); }

        // ------------------------------------------------------------------------------------------ damage
        public override void Damage(int dmg, Player by, bool addMeter = true)
        {
            if (!Hittable) return;
            hp -= dmg; if (flashCd <= 0f) { flashT = 0.05f; flashCd = 0.26f; }
            if (by != null && addMeter) by.AddMeter(0.02f * dmg);
            Sfx.Play("hit", .4f, 1.4f + Random.value * .3f);
            G.SpawnFx("spark", "spark", SunPos(), dmg >= 6 ? 2.4f : 1.2f, false, 32);
            if (dmg >= 6) G.Shake(.1f, .2f);
            if (phase == 1 && hp <= Phase2Hp + Phase3Hp) { hp = Phase2Hp + Phase3Hp; routine = Transition(2); wait = 0; trans = true; }
            else if (phase == 2 && hp <= Phase3Hp) { hp = Phase3Hp; routine = Transition(3); wait = 0; trans = true; }
            else if (phase == 3 && hp <= 0) { hp = 0; dying = true; routine = null; DieStart(); }
        }

        // ------------------------------------------------------------------------------------------ tick
        public override void Tick(float dt)
        {
            if (dead) return;
            t += dt; flashT -= dt; flashCd -= dt; openT -= dt; anim.flash = flashT > 0f;
            if (dying) { DieTick(dt); Render(dt); return; }
            if (!active) { anim.Play(Pfx + "idle"); Render(dt); return; }
            if (routine != null)
            {
                wait -= dt;
                while (wait <= 0f)
                {
                    if (!routine.MoveNext()) { routine = null; chargeLow = false; idleTimer = phase == 1 ? 1.5f : (phase == 2 ? 1.15f : 1.0f); openT = phase == 3 ? 4.2f : 4.8f; break; }
                    wait = routine.Current;
                }
            }
            else
            {
                idleTimer -= dt; Idle();
                if (idleTimer <= 0f && !G.PlayersAllDown()) { routine = NextAttack(); wait = 0; }
            }
            Render(dt);
        }

        void Idle()
        {
            sr.flipX = false; anim.Play(Pfx + "idle");
            if (phase == 3)
            {
                x = Mathf.Lerp(x, homeX - 1.2f + Mathf.Sin(t * 0.8f) * 2.8f, 0.05f);
                y = Mathf.Lerp(y, groundY + 1.3f + Mathf.Sin(t * 1.5f) * 0.4f, 0.08f);
            }
            else { x = Mathf.Lerp(x, homeX, 0.06f); y = groundY; }
        }

        void Render(float dt)
        {
            go.transform.position = new Vector3(x, y, 0);
            sr.color = glow ? Color.Lerp(Color.white, new Color(.6f, 1f, 1f), 0.4f + 0.4f * Mathf.Sin(t * 10f)) : (openT > 0f && !forceClosed && !dying) ? Color.Lerp(Color.white, new Color(1f, .88f, .45f), 0.45f + 0.45f * Mathf.Sin(t * 9f)) : Color.white;      // the cracked shell glows
            anim.Tick(dt);
        }

        // ------------------------------------------------------------------------------------------ attacks
        Player Target() { return G.RandomAlivePlayer(); }

        IEnumerator<float> NextAttack()
        {
            int n = 3, pick; do { pick = Random.Range(0, n); } while (pick == lastAttack);
            lastAttack = pick;
            if (phase == 1) return pick == 0 ? Hail() : (pick == 1 ? Charge() : Volley());
            if (phase == 2) return pick == 0 ? Stomp() : (pick == 1 ? Pillars() : Summon());
            return pick == 0 ? Rings() : (pick == 1 ? FanSweep() : GustRain());
        }

        EnemyShot Shard(Vector2 at, Vector2 v, float grav, bool pink)
        {
            var s = new EnemyShot(at, v, grav, pink, 1.1f); G.eshots.Add(s); return s;
        }

        // ---- form 1: the host
        IEnumerator<float> Hail()
        {
            anim.Play("p1_hail", true); Sfx.Play("warn", .5f, .8f);
            yield return 0.7f;
            float gap = Random.Range(-5.5f, 3.5f);           // a free corridor 3.4 units wide, so there is always a safe lane
            int pinks = 0;
            for (int i = 0; i < 10; i++)
            {
                float px = Random.Range(-7.2f, 7.2f);
                if (Mathf.Abs(px - gap) < 2.3f) px = gap + (px < gap ? -1 : 1) * Random.Range(2.4f, 3.4f);
                px = Mathf.Clamp(px, -7.2f, 7.2f);
                bool pink = (i == 4 || i == 10) && pinks < 2; if (pink) pinks++;
                Shard(new Vector2(px, G.world.y0 + Levels.Height + 0.3f), new Vector2(0, -2.5f), 7f, pink);
                Sfx.Play("spit", .25f, 1.7f);
                yield return 0.18f;
            }
            yield return 0.7f;
        }

        IEnumerator<float> Charge()
        {
            anim.Play("p1_roll", true); Sfx.Play("warn", .6f, .7f);
            float shake = 0f;
            while (shake < 1.0f) { shake += G.dt; x = homeX + Mathf.Sin(shake * 70f) * 0.05f; yield return Frame; }
            chargeLow = true; Sfx.Play("dash", .8f, .5f); G.Shake(.08f, .5f);
            anim.speed = 1.8f;
            float target = -5.6f;
            while (x > target) { x -= 6.8f * G.dt; yield return Frame; }
            G.Shake(.2f, .3f); Sfx.Play("boom", .8f, 1.2f);
            G.waves.Add(new Shockwave(x + 1.6f, 1, groundY, "shock_ice"));
            chargeLow = false; anim.speed = 1f;
            yield return 0.45f;
            anim.speed = 1.0f; chargeLow = true;
            while (x < homeX) { x += 4.2f * G.dt; yield return Frame; }
            x = homeX; chargeLow = false; anim.speed = 1f;
            yield return 0.3f;
        }

        IEnumerator<float> Volley()
        {
            for (int v = 0; v < 2; v++)
            {
                anim.Play("p1_hail", true); yield return 0.32f;
                var mouth = new Vector2(x - 0.8f, y + 3.3f);
                for (int i = 0; i < 3; i++)
                {
                    var p = Target(); var tgt = p != null ? p.Center : new Vector2(-3f, groundY + 1f);
                    float dx = tgt.x + Random.Range(-1.4f, 1.4f) - mouth.x, dy = tgt.y - mouth.y; const float g = 10f;
                    float T = Mathf.Clamp(Mathf.Abs(dx) / 6f, 1.0f, 1.9f);
                    var vel = new Vector2(dx / T, (dy + 0.5f * g * T * T) / T);
                    Shard(mouth, vel, g, i == 1 && v == 0);
                    Sfx.Play("spit", .4f, 1.1f);
                    yield return 0.16f;
                }
                yield return 0.55f;
            }
        }

        // ---- form 2: the cracked giant
        IEnumerator<float> Stomp()
        {
            for (int hop = 0; hop < 2; hop++)
            {
                var tgt = Target();
                float tx = hop == 0 ? Mathf.Clamp(tgt != null ? tgt.body.pos.x : 0f, -3.5f, 5f) : homeX;
                float x0 = x;
                anim.Play("p2_stomp", true); Sfx.Play("warn", .5f, .7f);
                yield return 0.6f;
                float dur = 1.0f, u = 0f;
                while (u < 1f) { u += G.dt / dur; float k = Mathf.Min(u, 1f); x = Mathf.Lerp(x0, tx, k); y = groundY + 1.8f * 4f * k * (1f - k); yield return Frame; }
                x = tx; y = groundY;
                Sfx.Play("boom", 1f, .9f); G.Shake(.3f, .35f);
                G.SpawnFx("dust", "dust", new Vector2(x - 1.6f, y), 2f, false, 8); G.SpawnFx("dust", "dust", new Vector2(x + 1.6f, y), 2f, false, 8);
                G.waves.Add(new Shockwave(x - 1.8f, -1, groundY, "shock_ice")); G.waves.Add(new Shockwave(x + 1.8f, 1, groundY, "shock_ice"));
                yield return 0.7f;
            }
        }

        IEnumerator<float> Pillars()
        {
            anim.Play("p2_pillars", true); Sfx.Play("warn", .5f, .6f);
            yield return 0.45f;
            for (int i = 0; i < 3; i++)
            {
                var p = Target(); float px = p != null ? p.body.pos.x : -3f;
                px = Mathf.Clamp(px + Random.Range(-0.8f, 0.8f), -7f, 5.5f);
                G.pillars.Add(new IcePillar(px, groundY));
                yield return 0.8f;
            }
            yield return 1.8f;
        }

        IEnumerator<float> Summon()
        {
            anim.Play("p2_summon", true); yield return 0.5f;
            Sfx.Play("wallop", .6f);
            float[] xs = { -5.6f, -4.2f, -0.2f, 0.2f }; float[] ys = { 3.2f, 3.2f, 5.4f, 3.2f };
            for (int i = 0; i < 2 && G.AliveEnemies() < 4; i++)
            {
                int j = Random.Range(0, 4);
                G.SpawnEnemy(i == 0 ? "penguin" : "seed", "", new Vector2(xs[j] + Random.Range(-.3f, .3f), ys[j]));
                yield return 0.4f;
            }
            yield return 0.8f;
        }

        // ---- form 3: the hollow storm
        IEnumerator<float> Rings()
        {
            for (int r = 0; r < 2; r++)
            {
                anim.Play("p3_ring", true); Sfx.Play("warn", .5f, 1.1f);
                yield return 0.35f;
                var p = Target(); var origin = SunPos();
                float aim = p != null ? Mathf.Atan2(p.Center.y - origin.y, p.Center.x - origin.x) : Mathf.PI;
                float gapHalf = 0.85f;
                int n = 18;
                for (int i = 0; i < n; i++)
                {
                    float a = i * Mathf.PI * 2f / n;
                    float d = Mathf.Abs(Mathf.DeltaAngle(a * Mathf.Rad2Deg, (aim + (r == 1 ? 0.55f : 0f)) * Mathf.Rad2Deg)) * Mathf.Deg2Rad;
                    if (d < gapHalf) continue;
                    Shard(origin, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3.5f, 0f, i % 7 == 3);
                }
                Sfx.Play("boom", .5f, 1.6f); G.Shake(.06f, .2f);
                yield return 1.5f;
            }
        }

        IEnumerator<float> FanSweep()
        {
            anim.Play("p3_fan", true); Sfx.Play("warn", .5f, 1.0f);
            yield return 0.4f;
            var p = Target(); var origin = SunPos();
            float aim = p != null ? Mathf.Atan2(p.Center.y - origin.y, p.Center.x - origin.x) : Mathf.PI;
            for (int i = 0; i < 9; i++)
            {
                float a = aim + Mathf.Lerp(-0.9f, 0.9f, i / 8f);
                Shard(origin, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 4.6f, 0f, i == 4);
                Sfx.Play("spit", .3f, 1.5f);
                yield return 0.12f;
            }
            yield return 0.7f;
        }

        IEnumerator<float> GustRain()
        {
            anim.Play("p3_ring", true); G.TriggerGust(Random.value < .5f ? 1 : -1);
            yield return 0.5f;
            for (int i = 0; i < 6; i++)
            {
                float px = Random.Range(-7f, 7f);
                Shard(new Vector2(px, G.world.y0 + Levels.Height + 0.3f), new Vector2(0, -2f), 6f, i == 3 || i == 7);
                yield return 0.55f;
            }
            yield return 0.6f;
        }

        // ------------------------------------------------------------------------------------------ transitions & death
        IEnumerator<float> Transition(int to)
        {
            trans = true; chargeLow = false; sr.flipX = false; anim.speed = 1f;
            G.ClearHazards();
            Sfx.Play("wallop", 1f, .8f); G.Shake(.3f, .9f); G.HitStop(.1f);
            anim.Play(to == 2 ? "to_p2" : "to_p3", true);
            float t0 = 0, dur = 0.95f, startY = y;
            while (t0 < dur)
            {
                t0 += G.dt;
                if (to == 3) y = Mathf.Lerp(startY, groundY + 1.3f, Mathf.Clamp01(t0 / dur));
                yield return Frame;
            }
            phase = to; trans = false;
            G.Burst(SunPos(), 18, 6f); G.SpawnFx("spark", "spark", SunPos(), 3f, false, 32);
            Sfx.Play("bell", .6f, .8f);
            routine = null; idleTimer = 1.8f; openT = 4.5f;
            yield break;
        }

        void DieStart()
        {
            G.ClearHazards(); anim.speed = 1f; anim.Play("death", true); sr.flipX = false; deathT = 0f; explodeT = 0f;
            Sfx.Play("ko"); G.HitStop(.15f); G.Shake(.35f, 1.4f);
            G.OnBossDown();
        }

        void DieTick(float dt)
        {
            deathT += dt; explodeT -= dt;
            if (explodeT <= 0f)
            {
                explodeT = 0.13f;
                var p = new Vector2(x + Random.Range(-1.8f, 1.8f), y + Random.Range(0.8f, 4.5f));
                G.SpawnFx("spark_ice", "spark", p, 2.2f, false, 31);
                G.Burst(p, 6, 5f); Sfx.Play("pop", .5f, .8f + Random.value * .5f);
            }
            if (anim.done && deathT > 1.9f && !dead) { dead = true; go.SetActive(false); G.Burst(SunPos(), 40, 8f); Sfx.Play("boom", 1f, .8f); G.OnBossDead(); }
        }

        public override void Dispose() { if (go != null) Object.Destroy(go); }
    }
}
