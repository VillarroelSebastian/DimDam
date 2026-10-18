using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>A scythe thrown like a boomerang: it flies out to where the target stood, hangs there for a beat and comes back to its owner.</summary>
    public class ScytheShot
    {
        public bool dead; public Vector2 pos; readonly Vector2 start, target; readonly System.Func<Vector2> home;
        float t; readonly float outT; bool coming; GameObject go; SpriteRenderer sr; Anim anim; float spin;

        public ScytheShot(Vector2 from, Vector2 to, float outTime, System.Func<Vector2> owner, Color tint)
        {
            start = from; target = to; outT = outTime; home = owner; pos = from;
            var sh = Art.Get("mantis_scythe");
            sr = Pix.Make("scythe", GameCtl.I.entRoot, sh[0], from, 19, 1.3f); sr.color = tint;
            go = sr.gameObject; anim = new Anim(sr, sh, "spin");
        }

        public void Tick(float dt)
        {
            if (dead) return;
            var g = GameCtl.I; t += dt; anim.Tick(dt);
            if (!coming)
            {
                float u = Mathf.Clamp01(t / outT); float e = 1f - (1f - u) * (1f - u);
                pos = Vector2.Lerp(start, target, e);
                if (u >= 1f && t > outT + 0.28f) coming = true;       // hangs for a beat, glowing: the cue that it is about to return
            }
            else
            {
                var h = home(); var d = h - pos;
                float sp = 10f + (t - outT) * 3f;
                pos = Vector2.MoveTowards(pos, h, sp * dt);
                if (d.magnitude < 0.7f) { Kill(); return; }
            }
            go.transform.position = new Vector3(pos.x, pos.y, 0);
            foreach (var p in g.players)
                if (p != null && p.Alive && p.invuln <= 0f && (p.Center - pos).sqrMagnitude < 0.6f * 0.6f) { p.Hit(1, pos.x); }
            if (t > 6f) Kill();
        }

        void Kill() { dead = true; if (go) Object.Destroy(go); }
        public void Dispose() { Kill(); }
    }

    /// <summary>
    /// Los Reyes Mantis: two small robotic mantis kings that fight as a pair and dance in step (inspired by the Mantis Lords of Hollow Knight).
    /// They share one health pool and three forms of the fight: opening dance, scissors (leaps and dives) and frenzy (everything at once).
    /// </summary>
    public class FireBoss : BossBase
    {
        public const int Phase1Hp = 120, Phase2Hp = 120, Phase3Hp = 110;
        public const int MaxHp = Phase1Hp + Phase2Hp + Phase3Hp;
        public override int TotalHp { get { return MaxHp; } }
        public override string DisplayName { get { return "LOS REYES MANTIS"; } }
        public override int[] PhaseFloors { get { return new[] { Phase2Hp + Phase3Hp, Phase3Hp }; } }

        class King
        {
            public GameObject go; public SpriteRenderer sr; public Anim anim; public float x, y, face = -1f; public bool dash, slashing, hidden; public Rect slash; public float flashT;
            public Vector2 Pos { get { return new Vector2(x, y); } }
        }

        readonly King[] K = new King[2];
        IEnumerator<float> routine; float wait, idleTimer = 1.6f, flashCd, t; int lastAttack = -1;
        float groundY; float deathT, explodeT; bool dancing;
        readonly Rect[] hb = new Rect[2]; readonly List<Rect> dg = new List<Rect>(); readonly List<ScytheShot> scythes = new List<ScytheShot>();
        const float Frame = 0.001f, Scale = 1.4f;
        static readonly float[] HomeX = { 3.0f, 6.0f };
        GameCtl G { get { return GameCtl.I; } }

        public FireBoss(float groundTop)
        {
            hp = MaxHp; groundY = groundTop; y = groundTop; x = 4.5f;
            string[] sheets = { "mantis_brasa", "mantis_ceniza" };
            for (int i = 0; i < 2; i++)
            {
                var sh = Art.Get(sheets[i]); var k = new King { x = HomeX[i], y = groundTop, face = -1f };
                k.sr = Pix.Make("mantis" + i, G.entRoot, sh[0], new Vector3(k.x, k.y, 0), 5 + i, Scale);
                k.go = k.sr.gameObject; k.anim = new Anim(k.sr, sh, "idle"); K[i] = k;
            }
        }

        // ------------------------------------------------------------------------------------------ geometry
        public override Rect[] Hurtboxes()
        {
            for (int i = 0; i < 2; i++) { var k = K[i]; hb[i] = k.hidden ? new Rect(-99f, -99f, 0.1f, 0.1f) : new Rect(k.x - 0.65f, k.y + 0.05f, 1.3f, 2.4f); }
            return hb;
        }

        public override List<Rect> Dangerboxes()
        {
            dg.Clear();
            foreach (var k in K)
            {
                if (k.hidden) continue;
                dg.Add(new Rect(k.x - 0.4f, k.y + 0.2f, 0.8f, 1.9f));
                if (k.dash) dg.Add(new Rect(k.x + (k.face > 0 ? 0f : -1.5f), k.y + 0.1f, 1.5f, 2.1f));
                if (k.slashing) dg.Add(k.slash);
            }
            return dg;
        }

        public override void Activate() { active = true; idleTimer = 2.2f; }

        // ------------------------------------------------------------------------------------------ damage
        public override void Damage(int dmg, Player by)
        {
            if (!Hittable) return;
            hp -= dmg;
            if (flashCd <= 0f) { foreach (var k in K) k.flashT = 0.05f; flashCd = 0.26f; }
            Sfx.Play("hit", .35f, .8f + Random.value * .2f);
            if (dmg >= 6) { G.SpawnFx("spark", "spark", new Vector2(x, y + 1.5f), 2f, false, 30); G.Shake(.1f, .2f); }
            if (phase == 1 && hp <= Phase2Hp + Phase3Hp) { hp = Phase2Hp + Phase3Hp; BeginTransition(2); }
            else if (phase == 2 && hp <= Phase3Hp) { hp = Phase3Hp; BeginTransition(3); }
            else if (phase == 3 && hp <= 0) { hp = 0; dying = true; routine = null; DieStart(); }
        }

        void BeginTransition(int to) { routine = Transition(to); wait = 0; trans = true; }

        // ------------------------------------------------------------------------------------------ tick
        public override void Tick(float dt)
        {
            if (dead) return;
            t += dt; flashCd -= dt;
            foreach (var k in K) { k.flashT -= dt; k.anim.flash = k.flashT > 0f; }
            for (int i = scythes.Count - 1; i >= 0; i--) { scythes[i].Tick(dt); if (scythes[i].dead) scythes.RemoveAt(i); }
            x = (K[0].x + K[1].x) * 0.5f; y = Mathf.Max(K[0].y, K[1].y);
            if (dying) { DieTick(dt); Render(dt); return; }
            if (!active) { Idle(dt, false); Render(dt); return; }
            if (routine != null)
            {
                wait -= dt;
                while (wait <= 0f)
                {
                    if (!routine.MoveNext()) { routine = null; EndAttack(); break; }
                    wait = routine.Current;
                }
            }
            else
            {
                idleTimer -= dt; Idle(dt, true);
                if (idleTimer <= 0f && !G.PlayersAllDown()) { routine = NextAttack(); wait = 0; }
            }
            Render(dt);
        }

        void EndAttack()
        {
            foreach (var k in K) { k.dash = false; k.slashing = false; k.hidden = false; k.y = groundY; }
            dancing = false; idleTimer = phase == 1 ? 1.3f : (phase == 2 ? 1.0f : 0.8f);
        }

        /// <summary>Between attacks the kings dance: they sway in mirrored steps near their posts.</summary>
        void Idle(float dt, bool fighting)
        {
            for (int i = 0; i < 2; i++)
            {
                var k = K[i];
                float tx = HomeX[i] + (fighting ? Mathf.Sin(t * 1.3f + i * Mathf.PI) * 0.9f : 0f);
                k.x = Mathf.Lerp(k.x, tx, 0.06f); k.y = groundY;
                var tg = G.NearestPlayer(k.Pos); if (tg != null) k.face = tg.body.pos.x >= k.x ? 1f : -1f;
                k.anim.Play(fighting ? "dance" : "idle");
            }
        }

        void Render(float dt)
        {
            foreach (var k in K)
            {
                k.go.transform.position = new Vector3(k.x, k.y, 0); k.sr.flipX = k.face < 0f; k.go.SetActive(!dead);
                k.sr.color = glow ? Color.Lerp(Color.white, new Color(.6f, 1f, 1f), 0.4f + 0.4f * Mathf.Sin(Time.time * 10f)) : Color.white;      // guarded: only the string hurts
                k.anim.Tick(dt);
            }
        }

        // ------------------------------------------------------------------------------------------ helpers
        Player Target() { return G.RandomAlivePlayer(); }
        static float EaseOut(float u) { return 1f - (1f - u) * (1f - u); }
        void Face(King k, float tx) { k.face = tx >= k.x ? 1f : -1f; }
        float Clamp(float v) { return Mathf.Clamp(v, Levels.Left + 0.8f, Levels.Right - 0.8f); }
        Color Tint(int i) { return i == 0 ? new Color(1f, .8f, .6f) : new Color(1f, 1f, .85f); }

        /// <summary>Both kings step to their marks (quickly, in mirrored stride) while a dancing beat plays.</summary>
        IEnumerator<float> StepTo(float x0, float x1, float dur)
        {
            float a0 = K[0].x, a1 = K[1].x; float u = 0f;
            K[0].anim.Play("step", true); K[1].anim.Play("step", true);
            Face(K[0], x0); Face(K[1], x1);
            while (u < 1f)
            {
                u += G.dt / dur; float e = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f));
                K[0].x = Mathf.Lerp(a0, x0, e); K[1].x = Mathf.Lerp(a1, x1, e); K[0].y = K[1].y = groundY;
                yield return Frame;
            }
        }

        IEnumerator<float> Windup(params int[] who)
        {
            foreach (int i in who) K[i].anim.Play("windup", true);
            Sfx.Play("warn", .5f, 1.1f);
            yield return phase == 1 ? 0.55f : (phase == 2 ? 0.45f : 0.36f);
        }

        IEnumerator<float> NextAttack()
        {
            int n = phase == 1 ? 3 : (phase == 2 ? 4 : 5);
            int pick; do { pick = Random.Range(0, n); } while (pick == lastAttack && n > 1);
            lastAttack = pick;
            if (phase == 1) return pick == 0 ? CrossDash(false) : (pick == 1 ? SlashCombo(false) : ScytheToss(1));
            if (phase == 2) return pick == 0 ? SlashCombo(true) : (pick == 1 ? ScytheToss(2) : (pick == 2 ? LeapCross() : DanceDash()));
            return pick == 0 ? CrossDash(true) : (pick == 1 ? FlameRing() : (pick == 2 ? LeapCross() : (pick == 3 ? ScytheToss(3) : DanceDash())));
        }

        // ------------------------------------------------------------------------------------------ attacks
        /// <summary>The kings take the two ends of the hall, bow, and charge across in turn (together in the last form).</summary>
        IEnumerator<float> CrossDash(bool together)
        {
            int first = Random.value < .5f ? 0 : 1; int second = 1 - first;
            var s = StepTo(-6.6f, 6.6f, 0.65f); while (s.MoveNext()) yield return s.Current;
            Face(K[0], 6.6f); Face(K[1], -6.6f);
            var w = Windup(0, 1); while (w.MoveNext()) yield return w.Current;
            var d1 = DashAcross(K[first], first == 0 ? 6.6f : -6.6f, 15f, together ? K[second] : null, together ? (first == 0 ? -6.6f : 6.6f) : 0f);
            while (d1.MoveNext()) yield return d1.Current;
            if (!together)
            {
                yield return 0.2f;
                var d2 = DashAcross(K[second], second == 0 ? 6.6f : -6.6f, 15f, null, 0f);
                while (d2.MoveNext()) yield return d2.Current;
            }
            yield return 0.45f;
        }

        IEnumerator<float> DashAcross(King a, float tx, float speed, King b, float tbx)
        {
            Face(a, tx); a.anim.Play("dash", true); a.dash = true; Sfx.Play("dash", .7f, .7f);
            if (b != null) { Face(b, tbx); b.anim.Play("dash", true); b.dash = true; }
            float guard = 0f;
            while (guard < 2.2f)
            {
                guard += G.dt; bool moving = false;
                float step = speed * G.dt;
                if (Mathf.Abs(a.x - tx) > step) { a.x += Mathf.Sign(tx - a.x) * step; moving = true; } else a.x = tx;
                if (b != null) { if (Mathf.Abs(b.x - tbx) > step) { b.x += Mathf.Sign(tbx - b.x) * step; moving = true; } else b.x = tbx; }
                if (G.time % .09f < G.dt) G.SpawnFx("spark", "spark", new Vector2(a.x, a.y + 0.2f), 1f, false, 29);
                if (!moving) break;
                yield return Frame;
            }
            a.dash = false; a.anim.Play("land", true);
            if (b != null) { b.dash = false; b.anim.Play("land", true); }
            Sfx.Play("land", .8f, .6f); G.Shake(.08f, .15f);
            yield return 0.3f;
        }

        /// <summary>One king (then the other, or both in a pincer) closes in and slashes: a wide fan in front of it.</summary>
        IEnumerator<float> SlashCombo(bool pincer)
        {
            var tg = Target(); float tx = tg != null ? tg.body.pos.x : 0f;
            int near = Mathf.Abs(K[0].x - tx) <= Mathf.Abs(K[1].x - tx) ? 0 : 1;
            if (!pincer)
            {
                for (int n = 0; n < 2; n++)
                {
                    int i = n == 0 ? near : 1 - near; var k = K[i];
                    tg = Target(); tx = tg != null ? tg.body.pos.x : 0f;
                    float stand = Clamp(tx + (k.x > tx ? 2.3f : -2.3f));
                    var mv = Glide(k, stand, 0.4f); while (mv.MoveNext()) yield return mv.Current;
                    var sl = Slash(k, tx); while (sl.MoveNext()) yield return sl.Current;
                }
            }
            else
            {
                tg = Target(); tx = tg != null ? tg.body.pos.x : 0f;
                var m0 = GlideBoth(Clamp(tx - 2.3f), Clamp(tx + 2.3f), 0.5f); while (m0.MoveNext()) yield return m0.Current;
                Face(K[0], 1f); Face(K[1], -1f);
                var w = Windup(0, 1); while (w.MoveNext()) yield return w.Current;
                SlashStart(K[0]); SlashStart(K[1]);
                yield return 0.14f; SlashHit(K[0], 3.0f); SlashHit(K[1], 3.0f);
                yield return 0.30f; SlashEnd(K[0]); SlashEnd(K[1]);
            }
            yield return 0.4f;
        }

        IEnumerator<float> Glide(King k, float tx, float dur)
        {
            float x0 = k.x; float u = 0f; k.anim.Play("step", true); Face(k, tx);
            while (u < 1f) { u += G.dt / dur; k.x = Mathf.Lerp(x0, tx, Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f))); yield return Frame; }
        }

        IEnumerator<float> GlideBoth(float t0, float t1, float dur)
        {
            float a0 = K[0].x, a1 = K[1].x; float u = 0f; K[0].anim.Play("step", true); K[1].anim.Play("step", true);
            while (u < 1f) { u += G.dt / dur; float e = Mathf.SmoothStep(0f, 1f, Mathf.Min(u, 1f)); K[0].x = Mathf.Lerp(a0, t0, e); K[1].x = Mathf.Lerp(a1, t1, e); yield return Frame; }
        }

        void SlashStart(King k) { k.anim.Play("slash", true); k.slashing = false; Sfx.Play("warn", .3f, 1.6f); }
        void SlashHit(King k, float reach)
        {
            k.slashing = true; k.slash = new Rect(k.x + (k.face > 0 ? 0.1f : -reach - 0.1f), k.y + 0.1f, reach, 2.2f);
            Sfx.Play("wallop", .5f, 1.5f); G.Shake(.06f, .15f);
            G.SpawnFx("spark", "spark", new Vector2(k.x + k.face * 1.4f, k.y + 1.1f), 1.6f, false, 31);
        }
        void SlashEnd(King k) { k.slashing = false; }

        IEnumerator<float> Slash(King k, float tx)
        {
            Face(k, tx);
            var w = Windup(K[0] == k ? 0 : 1); while (w.MoveNext()) yield return w.Current;
            SlashStart(k); yield return 0.14f;
            SlashHit(k, 2.9f); yield return 0.3f;
            SlashEnd(k); k.anim.Play("idle", true); yield return 0.12f;
        }

        /// <summary>Both kings throw a boomerang scythe at where the players stand (more of them in later forms).</summary>
        IEnumerator<float> ScytheToss(int count)
        {
            for (int round = 0; round < (phase == 3 ? 2 : 1); round++)
            {
                var tg = Target(); float tx = tg != null ? tg.body.pos.x : 0f; float ty = tg != null ? tg.Center.y : groundY + 1f;
                Face(K[0], tx); Face(K[1], tx);
                foreach (var k in K) k.anim.Play("throw", true);
                Sfx.Play("warn", .5f, 1.3f);
                yield return 0.30f;
                for (int i = 0; i < 2; i++)
                {
                    var k = K[i]; var owner = k;
                    for (int c = 0; c < count; c++)
                    {
                        float spread = count == 1 ? 0f : (c - (count - 1) * .5f) * 1.9f;
                        var dest = new Vector2(Mathf.Clamp(tx + spread, Levels.Left + .5f, Levels.Right - .5f), Mathf.Max(groundY + .9f, ty + (c % 2 == 0 ? 0f : .5f)));
                        scythes.Add(new ScytheShot(new Vector2(k.x + k.face * .6f, k.y + 1.5f), dest, 0.75f, () => new Vector2(owner.x, owner.y + 1.3f), Tint(i)));
                    }
                }
                Sfx.Play("spit", .6f, .6f);
                yield return 0.6f;
                foreach (var k in K) k.anim.Play("idle", true);
                yield return 0.25f;
            }
            yield return 1.4f + (phase == 1 ? 0.6f : 0f);
        }

        /// <summary>Both kings leap to the two walls, hang there, then plunge across each other toward the players and land with flame waves.</summary>
        IEnumerator<float> LeapCross()
        {
            float[] sx = { K[0].x, K[1].x };
            float[] wall = { -6.7f, 6.7f };
            // leap up to the walls (a parabola each)
            for (int i = 0; i < 2; i++) { K[i].anim.Play("leap", true); Face(K[i], wall[i]); }
            Sfx.Play("jump", .7f, .6f);
            float u = 0f;
            while (u < 1f)
            {
                u += G.dt / 0.7f; float k = Mathf.Min(u, 1f);
                for (int i = 0; i < 2; i++) { K[i].x = Mathf.Lerp(sx[i], wall[i], k); K[i].y = groundY + 4.4f * 4f * k * (1f - k) + 3.6f * k; }
                yield return Frame;
            }
            for (int i = 0; i < 2; i++) { K[i].y = groundY + 3.6f; K[i].anim.Play("windup", true); }
            Sfx.Play("warn", .6f, .9f);
            yield return phase == 3 ? 0.5f : 0.7f;
            var tg = Target(); float tx = tg != null ? tg.body.pos.x : 0f;
            float[] land = { Clamp(tx + 1.2f), Clamp(tx - 1.2f) };
            for (int i = 0; i < 2; i++) { K[i].anim.Play("dive", true); Face(K[i], land[i]); }
            Sfx.Play("dash", .8f, .6f);
            float[] x0 = { K[0].x, K[1].x }; float[] y0 = { K[0].y, K[1].y };
            u = 0f;
            while (u < 1f)
            {
                u += G.dt / 0.42f; float e = u * u;
                for (int i = 0; i < 2; i++) { K[i].x = Mathf.Lerp(x0[i], land[i], Mathf.Min(e, 1f)); K[i].y = Mathf.Lerp(y0[i], groundY, Mathf.Min(e, 1f)); K[i].dash = true; }
                yield return Frame;
            }
            for (int i = 0; i < 2; i++) { K[i].y = groundY; K[i].dash = false; K[i].anim.Play("land", true); }
            Sfx.Play("boom", .9f); G.Shake(.28f, .3f);
            for (int i = 0; i < 2; i++)
            {
                G.SpawnFx("dust", "dust", new Vector2(K[i].x, groundY), 1.6f, false, 8);
                G.waves.Add(new Shockwave(K[i].x - 0.8f, -1, groundY, "shock_fire")); G.waves.Add(new Shockwave(K[i].x + 0.8f, 1, groundY, "shock_fire"));
            }
            yield return 0.9f;
        }

        /// <summary>The "baile": the kings swap sides in a mirrored dance and then charge in step, crossing in the middle.</summary>
        IEnumerator<float> DanceDash()
        {
            dancing = true;
            var s = StepTo(-2.0f, 2.0f, 0.5f); while (s.MoveNext()) yield return s.Current;
            float tdance = 0f; K[0].anim.Play("dance", true); K[1].anim.Play("dance", true);
            float dur = phase == 3 ? 1.0f : 1.4f;
            while (tdance < dur)
            {
                tdance += G.dt;
                K[0].x = -2.0f + Mathf.Sin(tdance * 4.4f) * 2.0f; K[1].x = 2.0f - Mathf.Sin(tdance * 4.4f) * 2.0f;
                K[0].face = Mathf.Cos(tdance * 4.4f) >= 0 ? 1f : -1f; K[1].face = -K[0].face;
                if (G.time % .12f < G.dt) { G.Burst(new Vector2(K[0].x, K[0].y + 1.4f), 1, 2f); G.Burst(new Vector2(K[1].x, K[1].y + 1.4f), 1, 2f); }
                yield return Frame;
            }
            var tg = Target(); float tx = tg != null ? tg.body.pos.x : 0f;
            var w = Windup(0, 1); while (w.MoveNext()) yield return w.Current;
            Face(K[0], tx >= K[0].x ? 1f : -1f); Face(K[1], tx >= K[1].x ? 1f : -1f);
            float a = Clamp(tx + (K[0].x < tx ? 2.6f : -2.6f)), b = Clamp(tx + (K[1].x < tx ? 2.6f : -2.6f));
            K[0].dash = K[1].dash = true; K[0].anim.Play("dash", true); K[1].anim.Play("dash", true); Sfx.Play("dash", .7f, .8f);
            float guard = 0f;
            while (guard < 1.4f)
            {
                guard += G.dt; bool moving = false; float step = 14f * G.dt;
                if (Mathf.Abs(K[0].x - a) > step) { K[0].x += Mathf.Sign(a - K[0].x) * step; moving = true; }
                if (Mathf.Abs(K[1].x - b) > step) { K[1].x += Mathf.Sign(b - K[1].x) * step; moving = true; }
                if (!moving) break;
                yield return Frame;
            }
            K[0].dash = K[1].dash = false;
            SlashStart(K[0]); SlashStart(K[1]); yield return 0.14f; SlashHit(K[0], 2.8f); SlashHit(K[1], 2.8f);
            yield return 0.3f; SlashEnd(K[0]); SlashEnd(K[1]);
            yield return 0.5f;
        }

        /// <summary>Final form: both leap up, release a ring of fire bolts and land with a wave each.</summary>
        IEnumerator<float> FlameRing()
        {
            var s = StepTo(-3.0f, 3.0f, 0.45f); while (s.MoveNext()) yield return s.Current;
            float[] sx = { K[0].x, K[1].x };
            for (int i = 0; i < 2; i++) K[i].anim.Play("leap", true);
            Sfx.Play("jump", .7f, .7f);
            float u = 0f;
            while (u < 1f) { u += G.dt / 0.55f; float k = Mathf.Min(u, 1f); for (int i = 0; i < 2; i++) { K[i].x = Mathf.Lerp(sx[i], 0f + (i == 0 ? -0.9f : 0.9f), k); K[i].y = groundY + 4.6f * Mathf.Sin(k * Mathf.PI * .5f); } yield return Frame; }
            for (int i = 0; i < 2; i++) K[i].anim.Play("rage", true);
            Sfx.Play("warn", .6f, .8f); yield return 0.35f;
            for (int ring = 0; ring < 2; ring++)
            {
                for (int i = 0; i < 2; i++)
                {
                    int n = 10; float off = ring * Mathf.PI / n + i * .2f;
                    for (int j = 0; j < n; j++)
                    {
                        float ang = off + j * Mathf.PI * 2f / n;
                        G.eshots.Add(new EnemyShot(new Vector2(K[i].x, K[i].y + 1.2f), new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * 4.2f, 0f, false, 1f));
                    }
                }
                Sfx.Play("boom", .5f, 1.4f); G.Shake(.1f, .2f);
                yield return 0.55f;
            }
            u = 0f; float[] y0 = { K[0].y, K[1].y }; float[] x0 = { K[0].x, K[1].x };
            for (int i = 0; i < 2; i++) K[i].anim.Play("dive", true);
            while (u < 1f) { u += G.dt / 0.4f; float e = u * u; for (int i = 0; i < 2; i++) { K[i].y = Mathf.Lerp(y0[i], groundY, Mathf.Min(e, 1f)); K[i].x = Mathf.Lerp(x0[i], i == 0 ? -2.2f : 2.2f, Mathf.Min(e, 1f)); K[i].dash = true; } yield return Frame; }
            for (int i = 0; i < 2; i++) { K[i].dash = false; K[i].anim.Play("land", true); K[i].y = groundY; }
            Sfx.Play("boom", .9f); G.Shake(.3f, .35f);
            for (int i = 0; i < 2; i++) { G.waves.Add(new Shockwave(K[i].x - 0.8f, -1, groundY, "shock_fire")); G.waves.Add(new Shockwave(K[i].x + 0.8f, 1, groundY, "shock_fire")); }
            yield return 1.0f;
        }

        // ------------------------------------------------------------------------------------------ transitions & death
        IEnumerator<float> Transition(int to)
        {
            trans = true; dancing = false;
            foreach (var k in K) { k.dash = false; k.slashing = false; k.hidden = false; k.y = groundY; }
            foreach (var sc in scythes) sc.Dispose(); scythes.Clear();
            G.ClearHazards();
            Sfx.Play("wallop", 1f); G.Shake(.3f, .9f); G.HitStop(.1f);
            var s = StepTo(-1.2f, 1.2f, 0.5f); while (s.MoveNext()) yield return s.Current;
            foreach (var k in K) k.anim.Play("rage", true);
            Sfx.Play("bell", .8f, .7f);
            float t0 = 0f;
            while (t0 < 1.1f)
            {
                t0 += G.dt;
                if (G.time % .1f < G.dt) { G.Burst(new Vector2(K[0].x + Random.Range(-1f, 1f), K[0].y + Random.Range(.4f, 2.4f)), 2, 5f); G.Burst(new Vector2(K[1].x + Random.Range(-1f, 1f), K[1].y + Random.Range(.4f, 2.4f)), 2, 5f); }
                yield return Frame;
            }
            phase = to; trans = false;
            G.SpawnFx("spark", "spark", new Vector2(x, y + 1.5f), 3f, false, 30);
            Sfx.Play("boom", .8f, .8f); G.Shake(.2f, .4f);
            routine = null; idleTimer = 1.6f;
            yield break;
        }

        void DieStart()
        {
            G.ClearHazards();
            foreach (var sc in scythes) sc.Dispose(); scythes.Clear();
            foreach (var k in K) { k.dash = false; k.slashing = false; k.hidden = false; k.y = groundY; k.anim.Play("death", true); }
            deathT = 0f; explodeT = 0f;
            Sfx.Play("ko"); G.HitStop(.15f); G.Shake(.35f, 1.4f);
            G.OnBossDown();
        }

        void DieTick(float dt)
        {
            deathT += dt; explodeT -= dt;
            if (explodeT <= 0f)
            {
                explodeT = 0.12f;
                var k = K[Random.Range(0, 2)]; var p = new Vector2(k.x + Random.Range(-1f, 1f), k.y + Random.Range(0.3f, 2.4f));
                string fxn = Random.value < .5f ? "spark" : "parry"; G.SpawnFx(fxn, fxn, p, 1.6f, false, 31);
                G.Burst(p, 6, 5f); Sfx.Play("pop", .5f, .7f + Random.value * .4f);
            }
            if (K[0].anim.done && K[1].anim.done && deathT > 1.8f && !dead)
            {
                dead = true; foreach (var k in K) k.go.SetActive(false);
                G.Burst(new Vector2(x, y + 1f), 40, 8f); Sfx.Play("boom", 1f); G.OnBossDead();
            }
        }

        public override void Dispose()
        {
            foreach (var sc in scythes) sc.Dispose(); scythes.Clear();
            foreach (var k in K) if (k != null && k.go != null) Object.Destroy(k.go);
        }
    }
}
