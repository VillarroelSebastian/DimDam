using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>Resonant crystal: gum it four times and it sings, raising the glowing bridge for a while.</summary>
    public class Crystal
    {
        public Vector2 pos; public int bridgeIndex; public float charge; public bool active; float sinceHit;
        GameObject go; SpriteRenderer sr; Anim anim; Sheet sOff, sOn; float activeT, noteT, flashT;
        public const float Needs = 3f, ActiveTime = 14f;

        public Crystal(Vector2 p, int bridge)
        {
            pos = p; bridgeIndex = bridge;
            sOff = Art.Get("crystal_off"); sOn = Art.Get("crystal_on");
            sr = Pix.Make("crystal", GameCtl.I.entRoot, sOff[0], p, 4);
            go = sr.gameObject; anim = new Anim(sr, sOff, "loop");
        }

        public Rect Hurtbox { get { return new Rect(pos.x - 0.55f, pos.y, 1.1f, 1.45f); } }

        public void Hit(float amount)
        {
            flashT = 0.08f; sinceHit = 0f;
            Sfx.Play("select", .5f, 1.0f + charge * 0.22f);
            GameCtl.I.SpawnFx("spark_ice", "spark", pos + new Vector2(Random.Range(-.3f, .3f), 1f), 1.4f, false, 31);
            if (active) { activeT = ActiveTime; GameCtl.I.ExtendBridge(bridgeIndex, ActiveTime); return; }
            charge += amount;
            if (charge >= Needs) Activate();
        }

        void Activate()
        {
            active = true; activeT = ActiveTime; charge = 0;
            anim.Use(sOn, "loop", true);
            GameCtl.I.ActivateBridge(bridgeIndex, ActiveTime);
            GameCtl.I.Burst(pos + new Vector2(0, 1f), 14, 5f);
            Sfx.Play("revive"); GameCtl.I.Shake(.06f, .3f);
        }

        public void Tick(float dt)
        {
            flashT -= dt; noteT -= dt;
            sinceHit += dt;
            if (!active && sinceHit > 0.6f) charge = Mathf.Max(0f, charge - 0.5f * dt);     // charge only leaks after a pause in the shooting
            else
            {
                activeT -= dt;
                if (noteT <= 0f) { noteT = 0.28f; var f = GameCtl.I.SpawnFx("note", "loop", pos + new Vector2(Random.Range(-.5f, .5f), 1.2f), 1f, false, 32); f.vel = new Vector2(Random.Range(-.3f, .3f), 1.4f); f.life = 1.6f; }
                if (activeT <= 0f) { active = false; anim.Use(sOff, "loop", true); }
            }
            anim.flash = false;
            sr.color = flashT > 0f ? new Color(1.6f, 1.6f, 1.6f, 1f) : Color.white;
            anim.Tick(dt);
            go.transform.position = new Vector3(pos.x + (flashT > 0 ? Mathf.Sin(flashT * 200f) * 0.03f : 0f), pos.y, 0);
        }

        public void Dispose() { if (go != null) Object.Destroy(go); }
    }

    /// <summary>An icicle that shivers where it will fall (with a ground marker), then drops.</summary>
    public class Icicle
    {
        public bool dead; float x, y, vy, warn = 0.85f; bool falling; GameObject go, shadow; SpriteRenderer sr, shr; Anim anim; float landY;

        public Icicle(float px)
        {
            var g = GameCtl.I; x = px; y = g.world.y0 + Levels.Height + 0.4f;
            var sh = Art.Get("icicle");
            sr = Pix.Make("icicle", g.entRoot, sh[0], new Vector3(x, y, 0), 19); go = sr.gameObject; anim = new Anim(sr, sh, "shake");
            landY = LandingY();
            shr = Pix.Make("marker", g.entRoot, g.white, new Vector3(x - 0.45f, landY + 0.04f, 0), 3);
            shadow = shr.gameObject; shadow.transform.localScale = new Vector3(18, 3, 1); shr.color = new Color(.1f, .15f, .45f, .55f);
            Sfx.Play("warn", .35f, 1.5f);
        }

        float LandingY()
        {
            var g = GameCtl.I; float best = g.world.GroundTop;
            foreach (var p in g.world.plats) if (p.active && !p.solid && x >= p.x0 && x <= p.x1 && p.top > best && p.top < y - 1f) best = p.top;
            return best;
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            anim.Tick(dt);
            if (!falling)
            {
                warn -= dt;
                shr.color = new Color(.1f, .15f, .45f, .35f + .25f * Mathf.Abs(Mathf.Sin(warn * 14f)));
                y = Mathf.Lerp(y, g.world.y0 + Levels.Height - 0.8f, 0.2f);
                go.transform.position = new Vector3(x + Mathf.Sin(warn * 80f) * 0.04f, y, 0);
                if (warn <= 0f) { falling = true; landY = LandingY(); }
                return;
            }
            vy -= 38f * dt; y += vy * dt;
            go.transform.position = new Vector3(x, y, 0);
            var r = new Rect(x - 0.2f, y - 0.5f, 0.4f, 1.0f);
            foreach (var p in g.players) if (p != null && p.Alive && p.invuln <= 0 && r.Overlaps(p.body.Rect)) { p.Hit(1, x); Shatter(); return; }
            if (y - 0.5f <= landY) Shatter();
        }

        void Shatter()
        {
            dead = true; Object.Destroy(go); Object.Destroy(shadow);
            GameCtl.I.SpawnFx("icicle_shatter", "shatter", new Vector2(x, landY), 1f, false, 26);
            Sfx.Play("pop", .5f, 1.6f); GameCtl.I.Shake(.04f, .1f);
        }

        public void Dispose() { dead = true; if (go) Object.Destroy(go); if (shadow) Object.Destroy(shadow); }
    }

    /// <summary>Ice pillar erupting from a telegraphed crack (boss attack). Built from stacked segments so it stays crisp.</summary>
    public class IcePillar
    {
        public bool dead; float x, y, t, hold; GameObject root; List<SpriteRenderer> segs = new List<SpriteRenderer>(); SpriteRenderer crack; Anim crackAnim; int n = 5;
        const float Warn = 1.2f, Rise = 0.22f;

        public IcePillar(float px, float groundY, float holdTime = 1.1f)
        {
            x = px; y = groundY; hold = holdTime;
            var g = GameCtl.I; var cs = Art.Get("ice_crack");
            crack = Pix.Make("crack", g.entRoot, cs[0], new Vector3(x, y + 0.02f, 0), 4); crackAnim = new Anim(crack, cs, "loop");
            root = new GameObject("pillar"); root.transform.SetParent(g.entRoot, false); root.transform.position = new Vector3(x, y, 0);
            var ps = Art.Get("ice_pillar");
            for (int i = 0; i < n; i++)
            {
                var s = Pix.Make("seg" + i, root.transform, ps[i == 0 ? 0 : (i == n - 1 ? 2 : 1)], new Vector3(0, 0.0f + SegY(i), 0), 16);
                s.enabled = false; segs.Add(s);
            }
            Sfx.Play("warn", .5f, .8f);
        }

        float SegY(int i) { return i == 0 ? 0f : 0.4f + (i - 1) * 0.4f; }
        float Height(float prog) { return (0.4f + (n - 2) * 0.4f + 0.5f) * prog; }

        public Rect Danger
        {
            get
            {
                if (t < Warn) return new Rect(0, 0, 0, 0);
                float prog = Mathf.Clamp01((t - Warn) / Rise);
                return new Rect(x - 0.42f, y, 0.84f, Height(prog) * 0.95f);
            }
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            t += dt;
            if (t < Warn)
            {
                crackAnim.Tick(dt); crack.color = new Color(1, 1, 1, .55f + .45f * Mathf.Abs(Mathf.Sin(t * 12f)));
                crack.transform.position = new Vector3(x + Mathf.Sin(t * 70f) * .02f, y + 0.02f, 0);
                return;
            }
            crack.enabled = false;
            if (!segs[0].enabled) { Sfx.Play("boom", .7f, 1.3f); g.Shake(.12f, .25f); g.SpawnFx("icicle_shatter", "shatter", new Vector2(x, y), 1.4f, false, 27); }
            float prog = Mathf.Clamp01((t - Warn) / Rise);
            float h = Height(prog);
            for (int i = 0; i < n; i++) segs[i].enabled = SegY(i) < h - 0.01f || i == 0;
            if (t > Warn + Rise + hold)
            {
                float sink = (t - Warn - Rise - hold) / 0.3f;
                if (sink >= 1f) { dead = true; Dispose(); return; }
                root.transform.position = new Vector3(x, y - sink * 1.2f, 0);
            }
            var r = Danger;
            foreach (var p in g.players) if (p != null && p.Alive && p.invuln <= 0 && r.width > 0 && r.Overlaps(p.body.Rect)) p.Hit(1, x);
        }

        public void Dispose() { if (root) Object.Destroy(root); if (crack) Object.Destroy(crack.gameObject); }
    }

    /// <summary>A fire geyser: a glowing crack warns where the ground will split, then a column of flame erupts and burns whoever stands in it.</summary>
    public class Geyser
    {
        public bool dead; float x, y, t, hold; GameObject root; readonly List<SpriteRenderer> segs = new List<SpriteRenderer>(); SpriteRenderer crack; Anim crackAnim; Sheet ps; const int n = 7;
        const float Warn = 1.15f, Rise = 0.18f;

        public Geyser(float px, float groundY, float holdTime = 0.9f)
        {
            x = px; y = groundY; hold = holdTime;
            var g = GameCtl.I; var cs = Art.Get("fire_crack");
            crack = Pix.Make("crack", g.entRoot, cs[0], new Vector3(x, y + 0.02f, 0), 4); crackAnim = new Anim(crack, cs, "loop");
            root = new GameObject("geyser"); root.transform.SetParent(g.entRoot, false); root.transform.position = new Vector3(x, y, 0);
            ps = Art.Get("geyser_seg");
            for (int i = 0; i < n; i++) { var s = Pix.Make("seg" + i, root.transform, ps[Idx(i, false)], new Vector3(0, i * 0.5f, 0), 16); s.enabled = false; segs.Add(s); }
            Sfx.Play("warn", .5f, .7f);
        }

        int Idx(int i, bool alt) { return (i == 0 ? 0 : (i == n - 1 ? 2 : 1)) + (alt ? 3 : 0); }
        float Height(float prog) { return (n * 0.5f + 0.1f) * prog; }

        public Rect Danger
        {
            get
            {
                if (t < Warn) return new Rect(0, 0, 0, 0);
                float prog = Mathf.Clamp01((t - Warn) / Rise);
                return new Rect(x - 0.45f, y, 0.9f, Height(prog) * 0.95f);
            }
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I; t += dt;
            if (t < Warn)
            {
                crackAnim.Tick(dt); crack.color = new Color(1, 1, 1, .6f + .4f * Mathf.Abs(Mathf.Sin(t * 12f)));
                crack.transform.position = new Vector3(x + Mathf.Sin(t * 70f) * .02f, y + 0.02f, 0);
                return;
            }
            crack.enabled = false;
            if (!segs[0].enabled) { Sfx.Play("boom", .7f, 1.2f); g.Shake(.1f, .25f); g.SpawnFx("cinder_burst", "shatter", new Vector2(x, y), 1.6f, false, 27); }
            float prog = Mathf.Clamp01((t - Warn) / Rise); float h = Height(prog);
            bool alt = ((int)(t * 14f)) % 2 == 0;
            for (int i = 0; i < n; i++) { segs[i].enabled = i * 0.5f < h - 0.01f || i == 0; segs[i].sprite = ps[Idx(i, alt)]; }
            if (t > Warn + Rise + hold)
            {
                float sink = (t - Warn - Rise - hold) / 0.3f;
                if (sink >= 1f) { dead = true; Dispose(); return; }
                root.transform.localScale = new Vector3(1f, 1f - sink * 0.9f, 1f);
            }
            var r = Danger; if (t > Warn + Rise + hold) r.height *= 0.3f;
            foreach (var p in g.players) if (p != null && p.Alive && p.invuln <= 0 && r.width > 0 && r.Overlaps(p.body.Rect)) p.Hit(1, x);
        }

        public void Dispose() { if (root) Object.Destroy(root); if (crack) Object.Destroy(crack.gameObject); }
    }

    /// <summary>A lump of burning rock that glows where it will fall (with a ground marker), then drops and bursts.</summary>
    public class Cinder
    {
        public bool dead; float x, y, vy, warn = 0.8f; bool falling; GameObject go, shadow; SpriteRenderer sr, shr; Anim anim; float landY;

        public Cinder(float px)
        {
            var g = GameCtl.I; x = px; y = g.world.y0 + Levels.Height + 0.4f;
            var sh = Art.Get("cinder");
            sr = Pix.Make("cinder", g.entRoot, sh[0], new Vector3(x, y, 0), 19); go = sr.gameObject; anim = new Anim(sr, sh, "shake");
            landY = LandingY();
            shr = Pix.Make("marker", g.entRoot, g.white, new Vector3(x - 0.45f, landY + 0.04f, 0), 3);
            shadow = shr.gameObject; shadow.transform.localScale = new Vector3(18, 3, 1); shr.color = new Color(1f, .45f, .1f, .55f);
            Sfx.Play("warn", .35f, 1.2f);
        }

        float LandingY()
        {
            var g = GameCtl.I; float best = g.world.GroundTop;
            foreach (var p in g.world.plats) if (p.active && !p.solid && x >= p.x0 && x <= p.x1 && p.top > best && p.top < y - 1f) best = p.top;
            return best;
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I; anim.Tick(dt);
            if (!falling)
            {
                warn -= dt; shr.color = new Color(1f, .45f, .1f, .35f + .3f * Mathf.Abs(Mathf.Sin(warn * 14f)));
                y = Mathf.Lerp(y, g.world.y0 + Levels.Height - 0.8f, 0.2f);
                go.transform.position = new Vector3(x + Mathf.Sin(warn * 80f) * 0.04f, y, 0);
                if (warn <= 0f) { falling = true; landY = LandingY(); }
                return;
            }
            vy -= 36f * dt; y += vy * dt;
            go.transform.position = new Vector3(x, y, 0);
            var r = new Rect(x - 0.22f, y - 0.5f, 0.44f, 1.0f);
            foreach (var p in g.players) if (p != null && p.Alive && p.invuln <= 0 && r.Overlaps(p.body.Rect)) { p.Hit(1, x); Burst(); return; }
            if (y - 0.5f <= landY) Burst();
        }

        void Burst()
        {
            dead = true; Object.Destroy(go); Object.Destroy(shadow);
            GameCtl.I.SpawnFx("cinder_burst", "shatter", new Vector2(x, landY), 1f, false, 26);
            Sfx.Play("pop", .5f, 1.1f); GameCtl.I.Shake(.04f, .1f);
        }

        public void Dispose() { dead = true; if (go) Object.Destroy(go); if (shadow) Object.Destroy(shadow); }
    }

    /// <summary>Blizzard gusts: warned, then push everybody who is not under a roof.</summary>
    public class WindCtl
    {
        public int state; public float timer = 5f; public int dir = 1; float streakT;
        public float Strength { get { return state == 2 ? 17f : 0f; } }

        public void Reset(bool enabled) { state = 0; timer = enabled ? 6f : 9999f; }

        public float PushFor(Player p)
        {
            if (state != 2) return 0f;
            var g = GameCtl.I;
            if (g.world.Sheltered(p.body.pos.x, p.body.pos.y, 2.6f)) return 0f;
            return dir * Strength;
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            timer -= dt;
            if (state == 0 && timer <= 0f) { state = 1; timer = 1.3f; dir = Random.value < .5f ? 1 : -1; g.ShowHint(dir > 0 ? "VENTISCA >>>" : "<<< VENTISCA", 2.2f); Sfx.Play("warn", .6f, .6f); }
            else if (state == 1 && timer <= 0f) { state = 2; timer = 3.4f; Sfx.Play("lift", .5f, .5f); g.Shake(.03f, 3f); }
            else if (state == 2 && timer <= 0f) { state = 0; timer = Random.Range(6f, 9f); }
            if (state > 0)
            {
                streakT -= dt;
                float rate = state == 1 ? 0.18f : 0.04f;
                if (streakT <= 0f)
                {
                    streakT = rate;
                    float startX = dir > 0 ? Levels.Left - 1f : Levels.Right + 1f;
                    var f = g.SpawnFx("gust", "loop", new Vector2(startX, g.world.y0 + Random.Range(1.4f, 8.6f)), 1f, dir < 0, 36);
                    f.vel = new Vector2(dir * (state == 1 ? 9f : 24f), 0); f.life = 1.4f;
                }
            }
        }
    }
}
