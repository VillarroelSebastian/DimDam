using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>Loops a prop's frames on real time (keeps moving during hit-stop), with its own phase offset.</summary>
    public class PropAnim : MonoBehaviour
    {
        public Clip clip; public SpriteRenderer sr; public float speed = 1f; float offset;
        void Start() { offset = Random.value * 10f; }
        void Update()
        {
            if (clip == null) return;
            int n = clip.frames.Length;
            sr.sprite = clip.frames[((int)((Time.time + offset) * clip.fps * speed)) % n];
        }
    }

    /// <summary>Horizontal drift + a little parallax that follows the players.</summary>
    public class Drifter : MonoBehaviour
    {
        public float speed, parallax, baseX, wrap = 16f; public bool followPlayers = true; float x;
        void Update()
        {
            x += speed * Time.deltaTime;
            float px = 0f;
            var g = GameCtl.I;
            if (g != null && followPlayers) px = g.AveragePlayerX() * parallax;
            float v = Mathf.Repeat(x + baseX + 8f, wrap) - 8f;
            var p = transform.localPosition; p.x = v - px; transform.localPosition = p;
        }
    }

    public class Bobber : MonoBehaviour
    {
        public float amp = 0.15f, freq = 1.5f, phase; float y0; bool init;
        void Update()
        {
            if (!init) { y0 = transform.localPosition.y; init = true; phase = Random.value * 6f; }
            var p = transform.localPosition; p.y = y0 + Mathf.Sin(Time.time * freq + phase) * amp; transform.localPosition = p;
        }
    }

    /// <summary>Ambient particles drawn from the art library: bubbles, sprinkles, embers, falling snow, sparkles...</summary>
    public class Ambient : MonoBehaviour
    {
        public int theme; public float y0;
        class P { public Transform t; public SpriteRenderer sr; public Vector2 v; public float life, age, sway, ph; public int kind; public Sprite[] frames; public float fps; }
        readonly List<P> ps = new List<P>(); float spawnT; Sheet sprinkles, bubble, ember, drip, star, snow, sparkIce;

        void Start()
        {
            sprinkles = Art.Get("sprinkles"); bubble = Art.Get("bubble_small"); ember = Art.Get("ember"); drip = Art.Get("drip"); star = Art.Get("star");
            snow = Art.Get("snowflakes"); sparkIce = Art.Get("spark_ice");
        }

        float Rate()
        {
            switch (theme) { case 10: return 0.2f; case 11: return 0.12f; case 12: return 0.3f; case 13: return 0.16f; case 14: return 0.22f; case 3: return 0.5f; case 4: return 0.28f; case 5: return 0.11f; case 6: return 0.13f; case 7: return 0.22f; case 8: return 0.055f; case 9: return 0.2f; default: return 0.35f; }
        }

        void Update()
        {
            float dt = Time.deltaTime;
            spawnT -= dt;
            if (spawnT <= 0f) { spawnT = Rate(); Spawn(); }
            for (int i = ps.Count - 1; i >= 0; i--)
            {
                var p = ps[i]; p.age += dt;
                if (p.age >= p.life || p.t == null) { if (p.t != null) Destroy(p.t.gameObject); ps.RemoveAt(i); continue; }
                var pos = p.t.localPosition; pos.x += (p.v.x + Mathf.Sin(p.age * 2f + p.ph) * p.sway) * dt; pos.y += p.v.y * dt; p.t.localPosition = pos;
                if (p.frames != null) p.sr.sprite = p.frames[((int)(p.age * p.fps + p.ph * 3)) % p.frames.Length];
                float left = p.life - p.age; float a = Mathf.Min(1f, left / 0.6f, p.age / 0.4f);
                p.sr.color = new Color(1, 1, 1, Mathf.Clamp01(a));
            }
        }

        P New(Sprite s, Vector2 pos, Vector2 v, float life, int order, Sprite[] frames = null, float fps = 0f)
        {
            var sr = Pix.Make("amb", transform, s, new Vector3(pos.x, pos.y, 0), order);
            var p = new P { t = sr.transform, sr = sr, v = v, life = life, sway = Random.Range(0.1f, 0.5f), ph = Random.value * 6f, frames = frames, fps = fps };
            ps.Add(p); return p;
        }

        void Flake(float vxMin, float vxMax, float vyMin, float vyMax, int order)
        {
            float x = Random.Range(-8.5f, 8.5f);
            var p = New(snow[Random.Range(0, 8)], new Vector2(x, y0 + Levels.Height + 0.2f), new Vector2(Random.Range(vxMin, vxMax), Random.Range(vyMin, vyMax)), 9f, order);
            p.sway = theme == 8 ? 0.15f : 0.35f;
        }

        void Spawn()
        {
            float x = Random.Range(-8f, 8f);
            switch (theme)
            {
                case 0:   // night garden: glinting spores drift up, now and then a bubble rises
                    if (Random.value < .7f) New(sparkIce[0], new Vector2(x, y0 + Random.Range(1.2f, 7f)), new Vector2(Random.Range(-0.15f, 0.25f), Random.Range(0.15f, 0.5f)), 3.6f, -58, sparkIce.clips["spark"].frames, 4f);
                    else New(bubble[0], new Vector2(x, y0 + 1.1f), new Vector2(0f, Random.Range(0.5f, 1f)), 6f, -58);
                    break;
                case 1:   // embers & drips
                    if (Random.value < .5f) New(ember[0], new Vector2(x, y0 + Random.Range(1.5f, 6f)), new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.1f, 0.5f)), 5f, -60, ember.clips["pulse"].frames, 8f);
                    else New(drip[0], new Vector2(Random.Range(-7f, 7f), y0 + Levels.Height - 0.2f), new Vector2(0f, -3f), 2.6f, -60);
                    break;
                case 2:   // twinkling stars and lazy bubbles
                    if (Random.value < .55f) New(star[0], new Vector2(x, y0 + Random.Range(4.5f, 8.7f)), Vector2.zero, 3.2f, -85, star.clips["loop"].frames, 6f);
                    else New(bubble[0], new Vector2(x, y0 + 1.1f), new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.4f, 0.8f)), 7f, -58);
                    break;
                case 3:   // sparks from the factory
                    New(ember[0], new Vector2(Random.Range(-7f, 7f), y0 + 1.2f), new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(0.8f, 1.8f)), 3.2f, -60, ember.clips["pulse"].frames, 10f);
                    break;
                case 4:   // ballroom: embers from the candles rise slowly, a bubble of gum now and then
                    if (Random.value < .75f) New(ember[0], new Vector2(Random.Range(-7.5f, 7.5f), y0 + Random.Range(1.2f, 3f)), new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(0.25f, 0.7f)), 5f, -60, ember.clips["pulse"].frames, 8f);
                    else New(bubble[0], new Vector2(x, y0 + 1.1f), new Vector2(0f, Random.Range(0.4f, 0.8f)), 6f, -58);
                    break;
                case 5: Flake(-0.4f, 0.4f, -1.5f, -0.7f, -58); break;                         // calm snowfall
                case 6: Flake(0.2f, 0.7f, -1.3f, -0.6f, -58); break;                          // gentle flurry
                case 7:   // sparkling cave air + a stray flake
                    New(sparkIce[0], new Vector2(x, y0 + Random.Range(1.4f, 8.4f)), Vector2.zero, 1.6f, -55, sparkIce.clips["spark"].frames, 5f);
                    break;
                case 8: Flake(1.2f, 3.2f, -1.9f, -0.9f, -50); break;                         // wind-blown blizzard
                case 10: case 11: case 12: case 13: case 14:   // foundry: embers rise from the furnaces and the lava
                    New(ember[0], new Vector2(Random.Range(-7.5f, 7.5f), y0 + Random.Range(0.8f, 3.2f)), new Vector2(Random.Range(-0.3f, 0.3f), Random.Range(0.45f, 1.2f)), 4.2f, -58, ember.clips["pulse"].frames, 8f);
                    break;
                default:  // palace: glittering dust drifting down
                    if (Random.value < .6f) New(sparkIce[0], new Vector2(x, y0 + Random.Range(1.4f, 8.4f)), new Vector2(0, -0.2f), 1.8f, -55, sparkIce.clips["spark"].frames, 5f);
                    else Flake(-0.2f, 0.2f, -0.9f, -0.4f, -58);
                    break;
            }
        }
    }

    /// <summary>Keeps the tiles of one ledge glued to its runtime state: moving, shaking/cracking (brittle), squashing (elastic), blinking (bridge).</summary>
    public class PlatformView : MonoBehaviour
    {
        public Plat plat; public List<SpriteRenderer> tiles = new List<SpriteRenderer>(); public Sprite[] stateFrames; public bool tileRepeat;
        public float yOffset; int lastState = -1;

        void LateUpdate()
        {
            if (plat == null) return;
            bool vis = plat.active;
            if (plat.kind == PlatKind.Bridge && vis && plat.bridgeT < 2.5f && plat.bridgeT > 0f) vis = ((int)(Time.time * 12f)) % 2 == 0;      // about to fade
            for (int i = 0; i < tiles.Count; i++) tiles[i].enabled = vis;
            float jx = 0f;
            if (plat.kind == PlatKind.Brittle && plat.shake > 0f) jx = Mathf.Sign(Mathf.Sin(plat.shake * 90f)) * 0.03f;
            transform.localPosition = new Vector3(plat.x0 - plat.baseX0 + jx, plat.top - plat.baseTop, 0);
            if (stateFrames != null && stateFrames.Length >= 3)
            {
                int st = 0;
                if (plat.kind == PlatKind.Brittle) st = plat.shake <= 0f ? 0 : (plat.shake < 0.3f ? 1 : 2);
                else if (plat.kind == PlatKind.Elastic) st = plat.squash > 0.55f ? 1 : (plat.squash > 0.05f ? 2 : 0);
                if (st != lastState) { lastState = st; for (int i = 0; i < tiles.Count; i++) tiles[i].sprite = stateFrames[st]; }
            }
        }
    }

    /// <summary>Builds the visible scenery for one stage: sky, scrolling layers, animated props, tiles.</summary>
    public static class EnvBuilder
    {
        struct PropDef { public string sheet; public float x, y; public float scale; public int order; public float speed; public bool flip; public PropDef(string s, float px, float py, int o = -50, float sp = 1f, bool f = false) { sheet = s; x = px; y = py; scale = 1f; order = o; speed = sp; flip = f; } }

        public static GameObject Build(World w)
        {
            var root = new GameObject("env_" + w.def.title);
            if (GameCtl.I != null) root.transform.SetParent(GameCtl.I.transform, false);
            root.transform.localPosition = new Vector3(0, w.y0, 0);
            int th = w.def.theme; float c = 4.5f; bool ice = w.def.ice; bool fire = w.def.fire;

            // ---- sky (static) + layers that never stop moving
            Pix.Make("sky", root.transform, Art.Get("sky_" + th)[0], new Vector3(0, c, 0), -100);
            Layer(root, "far_" + th, c, -90, ice ? 0.3f : 0.35f, ice ? 0.04f : 0.06f);
            Layer(root, "mid_" + th, c, -80, ice ? 0.6f : 0.7f, ice ? 0.07f : 0.1f);
            ThemeProps(root.transform, th, w);

            // ---- ground, built from 1-unit tiles per sector (ice sectors are glossy, snow sectors are soft)
            var gs = Art.Get("ground_" + th);
            foreach (var p in w.def.plats)
            {
                if (!p.solid) continue;
                int n = Mathf.RoundToInt(p.x1 - p.x0); int frame = (ice && !p.ice) ? 1 : 0;
                for (int i = 0; i < n; i++) Pix.Make("ground", root.transform, gs[frame], new Vector3(p.x0 + 0.5f + i, 1f, 0), -20);
            }
            var ws = Art.Get("wall_" + th);
            for (int y = 1; y < 9; y++) { Pix.Make("wallL", root.transform, ws[0], new Vector3(-7.75f, y + 0.5f, 0), -15); var r = Pix.Make("wallR", root.transform, ws[0], new Vector3(7.75f, y + 0.5f, 0), -15); r.flipX = true; }

            // ---- ledges, assembled from 1-unit tiles. Dynamic ones get a PlatformView that follows their runtime state.
            var ps = Art.Get("plat_" + th);
            Sheet sp = (ice || fire) ? Art.Get("special_" + th) : null;
            foreach (var p in w.plats)
            {
                if (p.solid) continue;
                int n = Mathf.Max(1, Mathf.RoundToInt(p.x1 - p.x0));
                var holder = new GameObject("plat_" + p.kind); holder.transform.SetParent(root.transform, false);
                var view = holder.AddComponent<PlatformView>(); view.plat = p;
                for (int i = 0; i < n; i++)
                {
                    Sprite spr;
                    if (sp != null && p.kind == PlatKind.Brittle) spr = sp[0];
                    else if (sp != null && p.kind == PlatKind.Elastic) spr = sp[3];
                    else if (sp != null && p.kind == PlatKind.Moving) spr = sp[6];
                    else if (sp != null && p.kind == PlatKind.Bridge) spr = sp[7];
                    else spr = ps[i == 0 ? 0 : (i == n - 1 ? 4 : 1 + ((i * 7 + (int)(p.baseTop * 3)) % 3))];
                    var sr = Pix.Make("tile", holder.transform, spr, new Vector3(p.baseX0 + 0.5f + i, p.baseTop - w.y0, 0), -10);
                    view.tiles.Add(sr);
                }
                if (sp != null && p.kind == PlatKind.Brittle) view.stateFrames = new[] { sp[0], sp[1], sp[2] };
                if (sp != null && p.kind == PlatKind.Elastic) view.stateFrames = new[] { sp[3], sp[4], sp[5] };
                if (p.kind == PlatKind.Normal || p.kind == PlatKind.Brittle)
                {
                    var gl = new GameObject("glints"); gl.transform.SetParent(holder.transform, false);
                    var gc = gl.AddComponent<GlintSpawner>(); gc.x0 = p.baseX0 + 0.3f; gc.x1 = p.x1 - 0.3f; gc.y = p.baseTop - w.y0 - 0.05f; gc.ice = ice; gc.fire = fire;
                }
            }

            var amb = new GameObject("ambient"); amb.transform.SetParent(root.transform, false);
            var a = amb.AddComponent<Ambient>(); a.theme = th; a.y0 = 0f;
            return root;
        }

        static void Layer(GameObject root, string sheet, float cy, int order, float parallax, float drift)
        {
            var sh = Art.Get(sheet);
            var g = new GameObject(sheet); g.transform.SetParent(root.transform, false); g.transform.localPosition = new Vector3(0, cy, 0);
            for (int i = -1; i <= 1; i++) Pix.Make("t" + i, g.transform, sh[0], new Vector3(i * 16f, 0, 0), order);
            var d = g.AddComponent<Drifter>(); d.speed = drift; d.parallax = parallax; d.wrap = 16f; d.baseX = 0f;
        }

        static void Prop(Transform parent, PropDef d, bool bob = false)
        {
            var sh = Art.Get(d.sheet);
            string clipName = sh.clips.ContainsKey("loop") ? "loop" : null;
            var sr = Pix.Make(d.sheet, parent, sh[0], new Vector3(d.x, d.y, 0), d.order, d.scale);
            sr.flipX = d.flip;
            if (clipName != null) { var pa = sr.gameObject.AddComponent<PropAnim>(); pa.clip = sh.clips[clipName]; pa.sr = sr; pa.speed = d.speed; }
            if (bob) { var b = sr.gameObject.AddComponent<Bobber>(); b.amp = 0.12f; b.freq = Random.Range(1.2f, 1.9f); }
        }

        static void Clouds(Transform parent, string sheet, int count, float yMin, float yMax, float speed, int order)
        {
            var sh = Art.Get(sheet);
            for (int i = 0; i < count; i++)
            {
                var g = new GameObject("cloud"); g.transform.SetParent(parent, false);
                Pix.Make("c", g.transform, sh[i % 3], Vector3.zero, order);
                g.transform.localPosition = new Vector3(0, Random.Range(yMin, yMax), 0);
                var d = g.AddComponent<Drifter>(); d.speed = speed * Random.Range(0.7f, 1.3f); d.baseX = i * (16f / count) + Random.Range(-1f, 1f); d.parallax = 0.15f; d.wrap = 18f;
            }
        }

        static void ThemeProps(Transform r, int th, World w)
        {
            switch (th)
            {
                case 0:
                    Clouds(r, "clouds_0", 4, 6.2f, 8.4f, 0.12f, -95);
                    Prop(r, new PropDef("spire", -5.6f, 1.0f, -70, 1.0f));
                    Prop(r, new PropDef("licorice_0", 5.8f, 1.0f, -72, 1f));
                    Prop(r, new PropDef("licorice_1", 2.2f, 1.0f, -73, 0.8f)); Prop(r, new PropDef("licorice_2", -2.4f, 1.0f, -73, 0.9f));
                    for (int i = 0; i < 8; i++) Prop(r, new PropDef("mushroom_" + (i % 3), -7f + i * 2.0f + Random.Range(-0.3f, 0.3f), 1.0f, -19, 1f + Random.Range(-.2f, .2f), i % 2 == 0));
                    break;
                case 1:
                    Prop(r, new PropDef("waterfall", -6.2f, 8.5f, -75, 1f)); Prop(r, new PropDef("waterfall", 6.0f, 8.6f, -75, 1.1f, true));
                    Prop(r, new PropDef("crystal_0", -3.4f, 1.0f, -70)); Prop(r, new PropDef("crystal_1", 3.2f, 1.0f, -70)); Prop(r, new PropDef("crystal_0", 0.2f, 1.0f, -72, 1.2f, true));
                    Prop(r, new PropDef("pot", -6.3f, 1.0f, -19, 1f)); Prop(r, new PropDef("pot", 6.0f, 1.0f, -19, 1.1f, true));
                    break;
                case 2:
                    Clouds(r, "clouds_2", 6, 1.6f, 7.5f, 0.2f, -88);
                    for (int i = 0; i < 4; i++) Prop(r, new PropDef("lantern_" + i, -6f + i * 4.1f, 4.2f + (i % 2) * 2.2f, -60, 1f), true);
                    break;
                case 3:
                    Prop(r, new PropDef("gear_l", -5.5f, 6.4f, -70, 1f)); Prop(r, new PropDef("gear_s", -3.9f, 5.2f, -71, 1f, true));
                    Prop(r, new PropDef("gear_l", 5.2f, 5.6f, -70, 0.8f, true)); Prop(r, new PropDef("gear_s", 3.7f, 6.9f, -71, 1.2f));
                    Prop(r, new PropDef("steam", -7.0f, 1.0f, -19, 1f)); Prop(r, new PropDef("steam", 7.0f, 1.0f, -19, 1.2f));
                    Prop(r, new PropDef("vat", -2.0f, 1.0f, -19, 1f)); Prop(r, new PropDef("vat", 2.5f, 1.0f, -19, 1f));
                    Prop(r, new PropDef("lamp", -4.5f, 9.0f, -60, 1f)); Prop(r, new PropDef("lamp", 0f, 9.0f, -60, 1f)); Prop(r, new PropDef("lamp", 4.5f, 9.0f, -60, 1f));
                    break;
                case 4:
                    Prop(r, new PropDef("hall_candelabra", -7.0f, 1.0f, -69, 1.1f)); Prop(r, new PropDef("hall_candelabra", 7.0f, 1.0f, -69, 1.1f, true));
                    Prop(r, new PropDef("hall_candelabra", -3.9f, 1.0f, -69, 0.8f, true)); Prop(r, new PropDef("hall_candelabra", 3.9f, 1.0f, -69, 0.8f));
                    Prop(r, new PropDef("hall_chandelier", 0f, 9.0f, -60, 1.1f)); Prop(r, new PropDef("hall_chandelier", -4.8f, 9.0f, -61, 0.9f, true)); Prop(r, new PropDef("hall_chandelier", 4.8f, 9.0f, -61, 0.9f));
                    Prop(r, new PropDef("hall_curtain", -7.5f, 9.0f, -58, 1f)); Prop(r, new PropDef("hall_curtain", 7.5f, 9.0f, -58, 1f, true));
                    for (int i = 0; i < 2; i++)
                    {
                        var sh = Art.Get("beam");
                        var sr = Pix.Make("beam", r, sh[0], new Vector3(i == 0 ? -4f : 4f, 9.2f, 0), -65);
                        sr.transform.localScale = new Vector3(1.6f, 1.2f, 1f);
                        var sw = sr.gameObject.AddComponent<Sweeper>(); sw.baseAngle = i == 0 ? -10f : 10f; sw.amp = 5f; sw.speed = i == 0 ? 0.3f : 0.22f;
                    }
                    break;

                // ------------------------------------------------------------------ ICE KINGDOM
                case 5:   // Lago de los Susurros
                    Prop(r, new PropDef("aurora", 0f, 9.1f, -97, 1f));
                    Prop(r, new PropDef("dead_tree", -6.5f, 1.0f, -73, 1f)); Prop(r, new PropDef("pine_b", 6.0f, 1.0f, -73, 0.9f)); Prop(r, new PropDef("dead_tree_b", 1.8f, 1.0f, -74, 0.8f, true)); Prop(r, new PropDef("ice_lamp", -2.8f, 1.0f, -72, 1f));
                    Prop(r, new PropDef("wisp", -3.0f, 3.4f, -60, 1f), true); Prop(r, new PropDef("wisp", 3.6f, 4.8f, -60, 1.2f), true); Prop(r, new PropDef("wisp", 0.4f, 6.6f, -60, 0.9f), true);
                    break;
                case 6:   // Aldea de Iglus Torcidos
                    Prop(r, new PropDef("igloo", -5.2f, 1.0f, -70, 1f)); Prop(r, new PropDef("igloo", 5.4f, 1.0f, -70, 0.85f, true));
                    Prop(r, new PropDef("lanterns", -4.2f, 9.0f, -62, 1f)); Prop(r, new PropDef("lanterns", 4.2f, 9.0f, -62, 0.9f, true));
                    Prop(r, new PropDef("washflags", 0f, 8.7f, -63, 1.1f));
                    break;
                case 7:   // Cristales Cantores
                    Prop(r, new PropDef("frozenfall", -5.4f, 8.8f, -76, 1f)); Prop(r, new PropDef("frozenfall", 5.4f, 8.8f, -76, 1.1f, true));
                    Prop(r, new PropDef("glowcrystal_0", -6.4f, 1.0f, -71, 1f)); Prop(r, new PropDef("glowcrystal_1", -3.6f, 1.0f, -72, 0.9f));
                    Prop(r, new PropDef("glowcrystal_2", 3.8f, 1.0f, -72, 1.1f)); Prop(r, new PropDef("glowcrystal_0", 6.4f, 1.0f, -71, 0.8f, true));
                    break;
                case 8:   // Jardines Congelados
                    Prop(r, new PropDef("owlstatue", -6.5f, 1.0f, -71, 1f)); Prop(r, new PropDef("owlstatue", 6.5f, 1.0f, -71, 1f, true));
                    Prop(r, new PropDef("gardenlamp", -3.7f, 1.0f, -70, 1f)); Prop(r, new PropDef("gardenlamp", 3.7f, 1.0f, -70, 1.3f));
                    Prop(r, new PropDef("dead_tree", -1.6f, 1.0f, -74, 0.7f)); Prop(r, new PropDef("dead_tree_b", 1.7f, 1.0f, -74, 0.7f, true)); Prop(r, new PropDef("ice_lamp", 5.2f, 1.0f, -72, 1f));
                    break;
                case 10:   // Forja Abandonada
                    Prop(r, new PropDef("furnace", -5.8f, 1.0f, -72, 1f)); Prop(r, new PropDef("furnace", 5.6f, 1.0f, -72, 0.9f, true)); Prop(r, new PropDef("anvil", 0.4f, 1.0f, -71, 1f));
                    Prop(r, new PropDef("chain_hang", -3.0f, 9.0f, -64, 1f)); Prop(r, new PropDef("chain_hang", 3.4f, 9.0f, -64, 1f, true)); Prop(r, new PropDef("chain_hang", 0.2f, 9.0f, -65, 0.8f));
                    break;
                case 11:   // Rios de Magma
                    Prop(r, new PropDef("lavafall", -6.2f, 9.0f, -76, 1f)); Prop(r, new PropDef("lavafall", 6.0f, 9.0f, -76, 1.1f, true));
                    Prop(r, new PropDef("basalt_spike", -3.4f, 1.0f, -71, 1f)); Prop(r, new PropDef("basalt_spike", 3.2f, 1.0f, -71, 1.1f, true)); Prop(r, new PropDef("basalt_spike", 0.2f, 1.0f, -73, 0.8f));
                    break;
                case 12:   // Camara de Engranajes
                    Prop(r, new PropDef("fgear_l", -5.5f, 6.4f, -70, 1f)); Prop(r, new PropDef("fgear_s", -3.9f, 5.2f, -71, 1f, true));
                    Prop(r, new PropDef("fgear_l", 5.2f, 5.6f, -70, 0.8f, true)); Prop(r, new PropDef("fgear_s", 3.7f, 6.9f, -71, 1.2f));
                    Prop(r, new PropDef("piston", -7.0f, 1.0f, -69, 1f)); Prop(r, new PropDef("piston", 7.0f, 1.0f, -69, 1f, true));
                    Prop(r, new PropDef("steam_pipe", -2.4f, 1.0f, -69, 1f)); Prop(r, new PropDef("steam_pipe", 2.6f, 1.0f, -69, 1f, true));
                    break;
                case 13:   // Fundicion del Rey
                    Prop(r, new PropDef("crucible", -4.2f, 9.0f, -63, 1f)); Prop(r, new PropDef("crucible", 4.4f, 9.0f, -63, 1f, true)); Prop(r, new PropDef("chain_hang", 0f, 9.0f, -64, 0.9f));
                    Prop(r, new PropDef("furnace", -6.4f, 1.0f, -72, 1f)); Prop(r, new PropDef("furnace", 6.4f, 1.0f, -72, 0.9f, true)); Prop(r, new PropDef("anvil", 0.2f, 1.0f, -71, 1f));
                    break;
                case 14:   // Salon de las Hoces (boss arena)
                    Prop(r, new PropDef("mantis_statue", -2.7f, 1.0f, -73, 1.15f)); Prop(r, new PropDef("mantis_statue", 2.7f, 1.0f, -73, 1.15f, true));
                    Prop(r, new PropDef("brazier", -7.0f, 1.0f, -69, 1f)); Prop(r, new PropDef("brazier", 7.0f, 1.0f, -69, 1f, true)); Prop(r, new PropDef("brazier", -4.3f, 1.0f, -69, 0.8f)); Prop(r, new PropDef("brazier", 4.3f, 1.0f, -69, 0.8f, true));
                    Prop(r, new PropDef("mantis_banner", -5.4f, 9.0f, -61, 1f)); Prop(r, new PropDef("mantis_banner", 5.4f, 9.0f, -61, 1f, true));
                    Prop(r, new PropDef("hall_chandelier", 0f, 9.0f, -60, 1.1f));
                    break;
                default:  // Palacio de la Ultima Nevada
                    Prop(r, new PropDef("royalbanner", -6.9f, 9.0f, -68, 1f)); Prop(r, new PropDef("royalbanner", 0f, 9.0f, -68, 0.9f)); Prop(r, new PropDef("royalbanner", 6.9f, 9.0f, -68, 1.1f, true));
                    Prop(r, new PropDef("chandelier", -4.2f, 9.0f, -66, 1f)); Prop(r, new PropDef("chandelier", 4.2f, 9.0f, -66, 0.9f));
                    Prop(r, new PropDef("throne", -1.2f, 1.0f, -72, 1f));
                    break;
            }
        }
    }

    public class Sweeper : MonoBehaviour
    {
        public float baseAngle, amp, speed;
        void Update() { transform.localRotation = Quaternion.Euler(0, 0, baseAngle + Mathf.Sin(Time.time * speed) * amp); }
    }

    public class GlintSpawner : MonoBehaviour
    {
        public float x0, x1, y; public bool ice, fire; float t; Sheet gl; string clipName;
        void Start() { gl = Art.Get(ice ? "spark_ice" : (fire ? "spark" : "glint")); clipName = (ice || fire) ? "spark" : "glint"; t = Random.Range(0f, 2f); }
        void Update()
        {
            t -= Time.deltaTime;
            if (t > 0f) return;
            t = Random.Range(1.2f, 3.5f);
            var sr = Pix.Make("glint", transform, gl[0], new Vector3(Random.Range(x0, x1), y + Random.Range(-0.02f, 0.1f), 0), -9);
            var a = sr.gameObject.AddComponent<OneShot>(); a.clip = gl.clips[clipName]; a.sr = sr;
        }
    }

    public class OneShot : MonoBehaviour
    {
        public Clip clip; public SpriteRenderer sr; float t;
        void Update() { t += Time.deltaTime * clip.fps; int i = (int)t; if (i >= clip.frames.Length) { Destroy(gameObject); return; } sr.sprite = clip.frames[i]; }
    }
}
