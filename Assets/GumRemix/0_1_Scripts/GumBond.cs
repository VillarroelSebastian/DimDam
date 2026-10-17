using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>
    /// "Better Together": a string of gum stretches between the two heroes while they are close enough.
    /// Anything that crosses it is hurt (enemies, the boss's weak point, crystals) and enemy shots that touch it burst,
    /// so the two heroes make a trap just by standing on opposite sides of a threat.
    /// </summary>
    public class Bond
    {
        public bool Active; public const float Reach = 7.2f, Release = 8.0f;
        readonly List<SpriteRenderer> beads = new List<SpriteRenderer>(); readonly Transform root; float dmgT, popT, sfxT; int hits;
        public int Hits { get { return hits; } }
        public Vector2 A, B;

        public Bond(Transform parent)
        {
            var go = new GameObject("bond"); go.transform.SetParent(parent, false); root = go.transform;
        }

        SpriteRenderer Bead(int i)
        {
            while (beads.Count <= i)
            {
                var sr = Pix.Make("bead", root, GameCtl.I.white, Vector3.zero, 9, 3f);
                sr.color = new Color(1f, .62f, .86f); beads.Add(sr);
            }
            return beads[i];
        }

        public void Hide() { Active = false; foreach (var b in beads) if (b != null) b.enabled = false; }

        public void Tick(float dt)
        {
            if (beads.Count > 0 && beads[0] == null) beads.Clear();
            var g = GameCtl.I; var p0 = g.players[0]; var p1 = g.players[1];
            if (p0 == null || p1 == null || !p0.Alive || !p1.Alive) { Hide(); return; }
            A = p0.Center + new Vector2(0, 0.1f); B = p1.Center + new Vector2(0, 0.1f);
            float d = Vector2.Distance(A, B);
            if (!Active && d < Reach) Active = true; else if (Active && d > Release) Active = false;
            if (!Active) { Hide(); return; }

            // draw: gum beads along a sagging curve (quantised to pixels so it stays crisp); it tightens as the heroes move apart
            float sag = Mathf.Clamp((Release - d) * 0.14f, 0f, 1.1f);
            int n = Mathf.Max(2, Mathf.CeilToInt(d / 0.16f));
            float wob = Mathf.Sin(g.time * 7f) * 0.03f;
            for (int i = 0; i < n; i++)
            {
                float t = i / (float)(n - 1);
                var pos = Vector2.Lerp(A, B, t) + new Vector2(0, -sag * Mathf.Sin(Mathf.PI * t) + wob * Mathf.Sin(t * 12f + g.time * 5f));
                pos.x = Mathf.Round(pos.x * 40f) / 40f; pos.y = Mathf.Round(pos.y * 40f) / 40f;
                var sr = Bead(i); sr.enabled = true; sr.transform.position = new Vector3(pos.x - 0.0375f, pos.y, 0);
                sr.color = (i % 4 == 0) ? new Color(1f, .93f, .97f) : new Color(1f, .62f, .86f);
            }
            for (int i = n; i < beads.Count; i++) beads[i].enabled = false;

            dmgT -= dt; popT -= dt; sfxT -= dt;
            // the gum catches enemy shots
            if (popT <= 0f)
                foreach (var s in g.eshots)
                    if (!s.dead && !s.persistent && Near(s.pos, 0.35f + s.radius)) { s.Pop(); popT = 0.08f; hits++; g.Burst(s.pos, 3, 3f); p0.AddMeter(0.1f); }
            if (dmgT > 0f) return;
            bool did = false;
            foreach (var e in g.enemies)
                if (e.Hittable && Crosses(e.Hurtbox)) { e.Damage(1, p0); did = true; hits++; }
            foreach (var c in g.crystals) if (Crosses(c.Hurtbox)) { c.Hit(1f); did = true; }
            if (g.boss != null && g.boss.Hittable)
                foreach (var hb in g.boss.Hurtboxes()) if (Crosses(hb)) { g.boss.Damage(3, p0); did = true; hits++; }
            if (did)
            {
                dmgT = 0.4f; g.Shake(.03f, .08f); p0.AddMeter(0.08f);
                if (sfxT <= 0f) { Sfx.Play("stick", .4f, 1.6f); sfxT = 0.3f; }
            }
        }

        /// <summary>Does the (straight) segment between the heroes pass through this rectangle?</summary>
        bool Crosses(Rect r)
        {
            r.xMin -= 0.12f; r.xMax += 0.12f; r.yMin -= 0.2f; r.yMax += 0.2f;      // the string is a little forgiving
            int steps = Mathf.CeilToInt(Vector2.Distance(A, B) / 0.2f);
            for (int i = 0; i <= steps; i++) { var p = Vector2.Lerp(A, B, i / (float)steps); if (r.Contains(p)) return true; }
            return false;
        }

        bool Near(Vector2 p, float rad)
        {
            var ab = B - A; float len2 = ab.sqrMagnitude; if (len2 < 0.0001f) return false;
            float t = Mathf.Clamp01(Vector2.Dot(p - A, ab) / len2);
            return (A + ab * t - p).sqrMagnitude < rad * rad;
        }

        public void Dispose() { if (root != null) Object.Destroy(root.gameObject); }
    }
}
