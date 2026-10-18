using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>La Princesa Chicle: a three-form gum princess (elegant, then melting, then a many-eyed horror). Attacks are written as small coroutines.</summary>
    public class Boss : BossBase
    {
        public const int Phase1Hp = 170, Phase2Hp = 150, Phase3Hp = 125;
        public const int MaxHp = Phase1Hp + Phase2Hp + Phase3Hp;
        public override int TotalHp { get { return MaxHp; } }
        public override string DisplayName { get { return "LA PRINCESA CHICLE"; } }
        public override int[] PhaseFloors { get { return new[] { Phase2Hp + Phase3Hp, Phase3Hp }; } }

        GameObject go; SpriteRenderer sr; Anim anim; Sheet sheet;
        IEnumerator<float> routine; float wait, idleTimer = 1.2f, flashT, flashCd, t; int lastAttack = -1;
        bool manual; bool sweepActive; Rect sweepRect; bool diveActive; float homeX = 4.6f; float groundY;
        float deathT; int deathStage; float explodeT;
        readonly Rect[] hb = new Rect[1]; readonly List<Rect> dg = new List<Rect>();
        const float Frame = 0.001f;
        GameCtl G { get { return GameCtl.I; } }

        public Boss(float groundTop)
        {
            hp = MaxHp; groundY = groundTop; x = homeX; y = groundTop;
            sheet = Art.Get("boss");
            sr = Pix.Make("boss", G.entRoot, sheet[0], new Vector3(x, y, 0), 5, 2f);
            go = sr.gameObject; anim = new Anim(sr, sheet, "p1_idle");
        }

        public override Rect[] Hurtboxes()
        {
            if (phase == 3) hb[0] = new Rect(x - 1.9f, y + 1.5f, 3.8f, 3.8f);
            else hb[0] = new Rect(x - (phase == 1 ? 1.5f : 1.6f), y + 0.3f, phase == 1 ? 3.0f : 3.2f, phase == 1 ? 5.2f : 5.3f);
            return hb;
        }

        public override List<Rect> Dangerboxes()
        {
            dg.Clear();
            var b = Hurtboxes()[0]; b.xMin += 0.6f; b.xMax -= 0.6f; b.yMin += 0.3f; b.yMax -= 0.9f;
            dg.Add(b);
            if (sweepActive) dg.Add(sweepRect);
            return dg;
        }

        public override void Activate() { active = true; idleTimer = 2.4f; }

        string Pfx { get { return "p" + phase + "_"; } }

        // ------------------------------------------------------------------------------------------ damage
        public override void Damage(int dmg, Player by, bool addMeter = true)
        {
            if (!Hittable) return;
            hp -= dmg; if (flashCd <= 0f) { flashT = 0.045f; flashCd = 0.28f; }
            if (by != null && addMeter) by.AddMeter(0.012f * dmg);
            Sfx.Play("hit", .35f, .7f + Random.value * .2f);
            if (dmg >= 6) { G.SpawnFx("spark", "spark", new Vector2(x, y + 3.2f), 2f, false, 30); G.Shake(.1f, .2f); }
            if (phase == 1 && hp <= Phase2Hp + Phase3Hp) { hp = Phase2Hp + Phase3Hp; routine = Transition(2); wait = 0; trans = true; }
            else if (phase == 2 && hp <= Phase3Hp) { hp = Phase3Hp; routine = Transition(3); wait = 0; trans = true; }
            else if (phase == 3 && hp <= 0) { hp = 0; dying = true; routine = null; DieStart(); }
        }

        // ------------------------------------------------------------------------------------------ tick
        public override void Tick(float dt)
        {
            if (dead) return;
            t += dt; flashT -= dt; flashCd -= dt; anim.flash = flashT > 0f;
            if (dying) { DieTick(dt); Render(dt); return; }
            if (!active) { anim.Play(Pfx + "idle"); Render(dt); return; }
            if (routine != null)
            {
                wait -= dt;
                while (wait <= 0f)
                {
                    if (!routine.MoveNext()) { routine = null; manual = false; sweepActive = false; diveActive = false; idleTimer = phase == 1 ? 1.5f : (phase == 2 ? 1.15f : 1.0f); break; }
                    wait = routine.Current;
                }
            }
            else
            {
                idleTimer -= dt;
                Idle();
                if (idleTimer <= 0f && !G.PlayersAllDown()) { routine = NextAttack(); wait = 0; }
            }
            Render(dt);
        }

        void Idle()
        {
            manual = false; sr.flipX = false;
            anim.Play(Pfx + "idle");
            if (phase == 3)
            {
                // the balloon hovers in a lazy figure eight
                x = Mathf.Lerp(x, homeX - 0.6f + Mathf.Sin(t * 0.9f) * 3.2f, 0.05f);
                y = Mathf.Lerp(y, groundY + 1.9f + Mathf.Sin(t * 1.7f) * 0.45f, 0.08f);
            }
            else { x = Mathf.Lerp(x, homeX, 0.06f); y = groundY; }
        }

        void Render(float dt)
        {
            go.transform.position = new Vector3(x, y, 0);
            sr.color = glow ? Color.Lerp(Color.white, new Color(.6f, 1f, 1f), 0.4f + 0.4f * Mathf.Sin(Time.time * 10f)) : Color.white;      // guarded: the string is the only thing that hurts
            if (!manual) anim.Tick(dt);
        }

        // ------------------------------------------------------------------------------------------ helpers
        static IEnumerator<float> W(float s) { yield return s; }

        Player Target() { return G.RandomAlivePlayer(); }
        Vector2 Mouth() { return new Vector2(x - 0.4f, y + (phase == 3 ? 2.7f : 4.1f)); }

        void Spit(Vector2 vel, float grav, bool pink, float scale = 1.2f)
        {
            G.eshots.Add(new EnemyShot(Mouth(), vel, grav, pink, scale));
            Sfx.Play("spit", .5f, .9f);
        }

        IEnumerator<float> NextAttack()
        {
            int n = phase == 1 ? 3 : (phase == 2 ? 4 : 3);
            int pick; do { pick = Random.Range(0, n); } while (pick == lastAttack && n > 1);
            lastAttack = pick;
            if (phase == 1) return pick == 0 ? Spit1() : (pick == 1 ? Slam1() : Summon(1));
            if (phase == 2) return pick == 0 ? Sweep2() : (pick == 1 ? Rolling2() : (pick == 2 ? Rain2() : Summon(2)));
            return pick == 0 ? Spiral3() : (pick == 1 ? Dive3() : Drop3());
        }

        // ------------------------------------------------------------------------------------------ phase 1
        IEnumerator<float> Spit1()
        {
            for (int v = 0; v < 2; v++)
            {
                anim.Play("p1_spit", true); manual = false;
                yield return 0.26f;
                for (int i = 0; i < 3; i++) Spit(new Vector2(-5.2f - i * 0.9f, 3.8f + i * 1.6f), 9.5f, i == 1 && v == 0);
                yield return 0.55f;
            }
            yield return 0.3f;
        }

        IEnumerator<float> Slam1()
        {
            manual = true; anim.Play("p1_slam", true);
            for (int hop = 0; hop < 2; hop++)
            {
                var tgt = Target();
                float tx = hop == 0 ? Mathf.Clamp(tgt != null ? tgt.body.pos.x : 0f, -3.2f, 5.0f) : homeX;
                float x0 = x;
                anim.Apply(0); sr.flipX = false; Sfx.Play("warn", .4f);
                yield return 0.6f;
                float dur = 1.05f, u = 0f;
                Sfx.Play("jump", .6f, .5f);
                while (u < 1f)
                {
                    u += G.dt / dur; float k = Mathf.Min(u, 1f);
                    x = Mathf.Lerp(x0, tx, k); y = groundY + 3.4f * 4f * k * (1f - k);
                    anim.Apply(k < .12f ? 1 : (k < .55f ? 2 : 3));
                    yield return Frame;
                }
                x = tx; y = groundY; anim.Apply(4);
                Sfx.Play("boom", 1f); G.Shake(.3f, .35f);
                G.SpawnFx("dust", "dust", new Vector2(x - 1.5f, y), 2f, false, 8); G.SpawnFx("dust", "dust", new Vector2(x + 1.5f, y), 2f, false, 8);
                G.waves.Add(new Shockwave(x - 1.6f, -1, groundY)); G.waves.Add(new Shockwave(x + 1.6f, 1, groundY));
                yield return 0.16f; anim.Apply(5); yield return 0.5f;
            }
            manual = false;
        }

        IEnumerator<float> Summon(int ph)
        {
            anim.Play(ph == 1 ? "p1_spit" : "p2_summon", true); manual = false;
            yield return 0.4f;
            Sfx.Play("wallop", .6f);
            string kind = ph == 1 ? "bean" : "bear", col = ph == 1 ? "purple" : "orange";
            int cap = 3;
            var picks = new List<float> { -5.4f, -1.4f, 3.4f };
            // minions drop onto the ledges so the players can stick them and kick them at the boss
            if (G.AliveEnemies() < cap)
            {
                float[] ys = { 3.2f, 5.4f, 5.4f };
                float[] xs = { -5.6f, -4.0f, 0.2f };
                for (int i = 0; i < (ph == 1 ? 1 : 2) && G.AliveEnemies() < cap; i++)
                {
                    int j = Random.Range(0, 3);
                    G.SpawnEnemy(kind, col, new Vector2(xs[j] + Random.Range(-.4f, .4f), ys[j]));
                    yield return 0.35f;
                }
            }
            yield return 0.7f;
        }

        // ------------------------------------------------------------------------------------------ phase 2
        IEnumerator<float> Sweep2()
        {
            manual = false; sr.flipX = true;
            anim.Play("p2_swing", true);
            Sfx.Play("warn", .6f);
            yield return 0.65f;       // wind-up (frames 0..3)
            Sfx.Play("dash", .8f, .5f); G.Shake(.12f, .25f);
            sweepActive = true; float t0 = 0;
            while (t0 < 0.25f)
            {
                t0 += G.dt;
                sweepRect = new Rect(x - 7.2f, groundY + 2.45f, 6.0f, 1.0f);
                yield return Frame;
            }
            sweepActive = false;
            yield return 0.5f;
            sr.flipX = false;
        }

        IEnumerator<float> Rolling2()
        {
            anim.Play("p2_summon", true); manual = false;
            yield return 0.45f;
            for (int i = 0; i < 2; i++)
            {
                Sfx.Play("boom", .6f, 1.3f);
                var s = new EnemyShot(new Vector2(x - 2.0f, groundY + 0.6f), new Vector2(-4.0f, 0f), 0f, i == 1, 2f);
                s.persistent = true; s.life = 5f; G.eshots.Add(s);
                yield return 1.7f;
            }
            yield return 0.4f;
        }

        IEnumerator<float> Rain2()
        {
            anim.Play("p2_summon", true); manual = false;
            yield return 0.5f;
            for (int i = 0; i < 6; i++)
            {
                float rx = Random.Range(Levels.Left + 0.8f, 2.0f);
                bool pink = i == 2 || i == 6;
                G.eshots.Add(new EnemyShot(new Vector2(rx, G.world.y0 + Levels.Height + 0.3f), new Vector2(0f, -2f), 6f, pink, 1.2f));
                Sfx.Play("spit", .3f, 1.5f);
                yield return 0.55f;
            }
            yield return 0.4f;
        }

        // ------------------------------------------------------------------------------------------ phase 3 (balloon)
        IEnumerator<float> Spiral3()
        {
            anim.Play("p3_blow", true); manual = false;
            yield return 0.3f;
            float a = Random.value * 6.28f;
            for (int i = 0; i < 9; i++)
            {
                a += 0.7f;
                var v = new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 3.4f;
                G.eshots.Add(new EnemyShot(new Vector2(x, y + 3.5f), v, 0f, i % 5 == 4, 1f));
                Sfx.Play("spit", .3f, 1.2f);
                yield return 0.2f;
            }
            yield return 0.4f;
        }

        IEnumerator<float> Dive3()
        {
            manual = true; anim.Play("p3_dive", true); anim.Apply(0);
            var tgt = Target();
            float tx = Mathf.Clamp(tgt != null ? tgt.body.pos.x : 0f, -5f, 5.5f);
            Sfx.Play("warn", .6f);
            float shake = 0f;
            while (shake < 1.0f) { shake += G.dt; x = Mathf.Lerp(x, tx, 0.1f); y = Mathf.Lerp(y, groundY + 3.3f, 0.1f) + Mathf.Sin(shake * 60f) * 0.06f; yield return Frame; }
            diveActive = true;
            while (y > groundY + 0.02f) { y = Mathf.Max(groundY, y - 12f * G.dt); yield return Frame; }
            anim.Apply(1); Sfx.Play("boom", 1f); G.Shake(.3f, .35f);
            G.SpawnFx("dust", "dust", new Vector2(x, y), 2f, false, 8);
            G.waves.Add(new Shockwave(x - 1.5f, -1, groundY)); G.waves.Add(new Shockwave(x + 1.5f, 1, groundY));
            yield return 0.5f;
            diveActive = false;
            anim.Apply(0);
            float u = 0f; float y0 = y;
            while (u < 1f) { u += G.dt / 0.9f; y = Mathf.Lerp(y0, groundY + 1.9f, 1f - (1f - u) * (1f - u)); yield return Frame; }
            manual = false;
        }

        IEnumerator<float> Drop3()
        {
            anim.Play("p3_blow", true); manual = false;
            yield return 0.4f;
            float start = Random.Range(-6.5f, -3.5f);
            for (int i = 0; i < 5; i++)
            {
                G.eshots.Add(new EnemyShot(new Vector2(start + i * 1.6f, G.world.y0 + Levels.Height + 0.3f), new Vector2(0, -2.5f), 4f, i % 3 == 1, 1.2f));
                Sfx.Play("spit", .3f, 1.6f);
                yield return 0.34f;
            }
            yield return 0.6f;
        }

        // ------------------------------------------------------------------------------------------ transitions & death
        IEnumerator<float> Transition(int to)
        {
            trans = true; manual = false; sweepActive = false; diveActive = false; sr.flipX = false;
            G.ClearHazards();
            Sfx.Play("wallop", 1f); G.Shake(.3f, .9f); G.HitStop(.1f);
            anim.Play(to == 2 ? "to_p2" : "to_p3", true);
            if (to == 2) G.ThrowHat(new Vector2(x, y + 5.4f));
            float start = to == 2 ? y : y;
            float t0 = 0; float dur = 0.95f;
            while (t0 < dur)
            {
                t0 += G.dt;
                if (to == 3) { y = Mathf.Lerp(start, groundY + 1.9f, Mathf.Clamp01(t0 / dur)); }
                yield return Frame;
            }
            phase = to; trans = false;
            G.SpawnFx("spark", "spark", new Vector2(x, y + 2.5f), 3f, false, 30);
            Sfx.Play("bell", .6f);
            routine = null; idleTimer = 1.8f;
            yield break;
        }

        void DieStart()
        {
            G.ClearHazards();
            manual = false; anim.Play("death", true); sr.flipX = false; deathT = 0f; deathStage = 0; explodeT = 0f;
            Sfx.Play("ko"); G.HitStop(.15f); G.Shake(.35f, 1.4f);
            G.OnBossDown();
        }

        void DieTick(float dt)
        {
            deathT += dt; explodeT -= dt;
            if (explodeT <= 0f)
            {
                explodeT = 0.13f;
                var p = new Vector2(x + Random.Range(-1.8f, 1.8f), y + Random.Range(0.8f, 5.2f));
                string fxn = Random.value < .5f ? "spark" : "parry"; G.SpawnFx(fxn, fxn, p, 1.6f, false, 31);
                G.Burst(p, 6, 5f); Sfx.Play("pop", .5f, .7f + Random.value * .4f);
            }
            if (anim.done && deathT > 1.9f && !dead) { dead = true; go.SetActive(false); G.Burst(new Vector2(x, y + 1.5f), 40, 8f); Sfx.Play("boom", 1f); G.OnBossDead(); }
        }

        public override void Dispose() { if (go != null) Object.Destroy(go); }
    }
}
