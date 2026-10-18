using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GumRemix
{
    // ------------------------------------------------------------------------------ input
    public struct Pad
    {
        public float x;
        public bool up, down, jumpDown, jumpHeld, shoot, dashDown;
    }

    public static class GumInput
    {
        /// <summary>Tests and bots can replace the real devices.</summary>
        public static Func<int, Pad> Override;
        public static bool ForceConfirm;          // lets automated tests press Enter

        public static Pad Read(int i)
        {
            if (Override != null) return Override(i);
            Pad p = default(Pad);
            var kb = Keyboard.current;
            if (kb != null)
            {
                if (i == 0)
                {
                    p.x = (kb.dKey.isPressed ? 1 : 0) - (kb.aKey.isPressed ? 1 : 0);
                    p.up = kb.wKey.isPressed; p.down = kb.sKey.isPressed;
                    p.jumpDown = kb.spaceKey.wasPressedThisFrame; p.jumpHeld = kb.spaceKey.isPressed;
                    p.shoot = kb.fKey.isPressed; p.dashDown = kb.gKey.wasPressedThisFrame;
                }
                else
                {
                    p.x = (kb.rightArrowKey.isPressed ? 1 : 0) - (kb.leftArrowKey.isPressed ? 1 : 0);
                    p.up = kb.upArrowKey.isPressed; p.down = kb.downArrowKey.isPressed;
                    p.jumpDown = kb.rightShiftKey.wasPressedThisFrame; p.jumpHeld = kb.rightShiftKey.isPressed;
                    p.shoot = kb.rightCtrlKey.isPressed; p.dashDown = kb.periodKey.wasPressedThisFrame;
                }
            }
            var pads = Gamepad.all;
            if (i < pads.Count)
            {
                var g = pads[i];
                float sx = g.leftStick.x.ReadValue();
                if (Mathf.Abs(sx) > 0.3f) p.x = Mathf.Sign(sx);
                if (g.dpad.right.isPressed) p.x = 1;
                if (g.dpad.left.isPressed) p.x = -1;
                p.up |= g.leftStick.y.ReadValue() > 0.5f || g.dpad.up.isPressed;
                p.down |= g.leftStick.y.ReadValue() < -0.5f || g.dpad.down.isPressed;
                p.jumpDown |= g.buttonSouth.wasPressedThisFrame; p.jumpHeld |= g.buttonSouth.isPressed;
                p.shoot |= g.buttonWest.isPressed || g.rightTrigger.isPressed;
                p.dashDown |= g.buttonEast.wasPressedThisFrame || g.rightShoulder.wasPressedThisFrame;
            }
            return p;
        }

        public static bool Confirm()
        {
            if (ForceConfirm) { ForceConfirm = false; return true; }
            var kb = Keyboard.current;
            if (kb != null && (kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame || kb.spaceKey.wasPressedThisFrame)) return true;
            foreach (var g in Gamepad.all) if (g.startButton.wasPressedThisFrame || g.buttonSouth.wasPressedThisFrame) return true;
            return false;
        }

        public static bool Key(Func<Keyboard, bool> f) { var kb = Keyboard.current; return kb != null && f(kb); }
    }

    // ------------------------------------------------------------------------------ audio (all synthesised in code)
    public static class Sfx
    {
        const int SR = 22050;
        static AudioSource[] pool; static int next;
        static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        public static float Volume = 0.55f;

        struct Seg { public float dur, f0, f1; public int wave; public float vol; public Seg(float d, float a, float b, int w, float v) { dur = d; f0 = a; f1 = b; wave = w; vol = v; } }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { pool = null; next = 0; host = null; clips.Clear(); GumInput.Override = null; GumInput.ForceConfirm = false; }

        public static void Init(GameObject hostObj)
        {
            if (pool != null && pool[0] != null && host != null) return;
            pool = new AudioSource[10];
            for (int i = 0; i < pool.Length; i++) { pool[i] = hostObj.AddComponent<AudioSource>(); pool[i].playOnAwake = false; }
            Sfx.host = hostObj.GetComponent<MusicHost>() ?? hostObj.AddComponent<MusicHost>();
            if (clips.Count > 0) return;
            Def("shoot", new Seg(.07f, 1500, 500, 1, .5f));
            Def("jump", new Seg(.14f, 300, 760, 0, .8f));
            Def("land", new Seg(.06f, 160, 90, 3, .5f));
            Def("dash", new Seg(.16f, 900, 200, 3, .45f));
            Def("hit", new Seg(.07f, 600, 300, 3, .7f));
            Def("stick", new Seg(.16f, 520, 140, 0, .8f), new Seg(.08f, 160, 300, 0, .6f));
            Def("kick", new Seg(.12f, 220, 70, 0, 1f), new Seg(.05f, 400, 200, 3, .5f));
            Def("pop", new Seg(.05f, 900, 1600, 1, .5f), new Seg(.12f, 1200, 200, 3, .6f));
            Def("hurt", new Seg(.28f, 520, 90, 2, .8f));
            Def("parry", new Seg(.07f, 1320, 1320, 1, .5f), new Seg(.12f, 1760, 1760, 1, .5f), new Seg(.16f, 2640, 2640, 0, .5f));
            Def("revive", new Seg(.08f, 520, 520, 0, .7f), new Seg(.08f, 660, 660, 0, .7f), new Seg(.16f, 880, 880, 0, .7f));
            Def("select", new Seg(.06f, 880, 880, 1, .4f), new Seg(.08f, 1320, 1320, 1, .4f));
            Def("clear", new Seg(.1f, 523, 523, 1, .5f), new Seg(.1f, 659, 659, 1, .5f), new Seg(.1f, 784, 784, 1, .5f), new Seg(.28f, 1046, 1046, 1, .5f));
            Def("lift", new Seg(.7f, 240, 700, 0, .6f));
            Def("ko", new Seg(.15f, 880, 880, 1, .5f), new Seg(.15f, 660, 660, 1, .5f), new Seg(.15f, 440, 440, 1, .5f), new Seg(.5f, 220, 110, 2, .6f));
            Def("boom", new Seg(.4f, 140, 40, 3, 1f));
            Def("spit", new Seg(.12f, 300, 900, 0, .7f), new Seg(.05f, 900, 500, 3, .3f));
            Def("warn", new Seg(.08f, 1000, 1000, 1, .4f), new Seg(.08f, 0, 0, 1, 0f), new Seg(.08f, 1000, 1000, 1, .4f));
            Def("bell", new Seg(.5f, 1568, 1568, 0, .5f));
            Def("wallop", new Seg(.12f, 400, 800, 2, .6f), new Seg(.3f, 800, 300, 2, .6f));
        }

        static void Def(string name, params Seg[] segs)
        {
            float total = 0; foreach (var s in segs) total += s.dur;
            int n = (int)(total * SR); var d = new float[n];
            int o = 0; float ph = 0; var rng = new System.Random(name.GetHashCode());
            foreach (var s in segs)
            {
                int m = (int)(s.dur * SR);
                for (int i = 0; i < m && o + i < n; i++)
                {
                    float t = i / (float)m;
                    float f = Mathf.Lerp(s.f0, s.f1, t);
                    ph += f / SR; ph -= Mathf.Floor(ph);
                    float v;
                    switch (s.wave)
                    {
                        case 0: v = Mathf.Sin(ph * 6.2831853f); break;
                        case 1: v = ph < .5f ? 1f : -1f; break;
                        case 2: v = ph * 2f - 1f; break;
                        default: v = (float)(rng.NextDouble() * 2 - 1); break;
                    }
                    float env = Mathf.Clamp01(i / 60f) * Mathf.Pow(1f - t, 1.4f);
                    d[o + i] = v * s.vol * env * 0.6f;
                }
                o += m;
            }
            var c = AudioClip.Create(name, n, 1, SR, false); c.SetData(d, 0); clips[name] = c;
        }

        public static void Play(string name, float vol = 1f, float pitch = 1f)
        {
            if (pool == null || !clips.ContainsKey(name)) return;
            var s = pool[next++ % pool.Length];
            s.pitch = pitch * UnityEngine.Random.Range(0.97f, 1.03f);
            s.PlayOneShot(clips[name], vol * Volume);
        }

        // ---- music lives in GumMusic.cs (composed songs rendered by MusicRenderer, played by MusicHost)
        static MusicHost host;
        public static MusicHost Host { get { return host; } }
        public static Track ThemeTrack(int theme) { return theme >= 10 ? (Track)(8 + theme) : (Track)Mathf.Clamp(theme, 0, 9); }
        public static void PlayMusic(int theme) { PlayTrack(ThemeTrack(theme)); }
        public static void PlayTrack(Track t, float fade = 1.8f) { if (host != null) host.Play(t, fade); }
        public static void Prefetch(Track t) { if (host != null) host.Prefetch(t); }
        public static void StopMusic(float fade = 0.8f) { if (host != null) host.Stop(fade); }
    }

    // ------------------------------------------------------------------------------ helpers
    public static class Pix
    {
        public static SpriteRenderer Make(string name, Transform parent, Sprite s, Vector3 pos, int order, float scale = 1f)
        {
            var go = new GameObject(name);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.localPosition = pos;
            if (scale != 1f) go.transform.localScale = new Vector3(scale, scale, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = s; sr.sortingOrder = order;
            return sr;
        }

        public static float Approach(float v, float target, float maxDelta)
        {
            return v < target ? Mathf.Min(v + maxDelta, target) : Mathf.Max(v - maxDelta, target);
        }
    }

    /// <summary>Text drawn with the 5x7 pixel font, as sprites, so it stays crisp inside the 640x360 render.</summary>
    public class PText : MonoBehaviour
    {
        public string text = ""; public Color color = Color.white; public bool shadow = true;
        public int order = 100; public int scale = 1; public bool centered;
        readonly List<SpriteRenderer> main = new List<SpriteRenderer>();
        readonly List<SpriteRenderer> back = new List<SpriteRenderer>();
        string shown; Color shownColor; int shownScale; bool shownCenter;
        public float Width { get { return text.Length * 6 * scale / 40f; } }

        public static PText Create(Transform parent, Vector3 pos, string s, int scale = 1, Color? col = null, bool center = false, int order = 100)
        {
            var go = new GameObject("text_" + s); go.transform.SetParent(parent, false); go.transform.localPosition = pos;
            var t = go.AddComponent<PText>(); t.scale = scale; t.centered = center; t.order = order; if (col.HasValue) t.color = col.Value;
            t.Set(s); return t;
        }

        public void Set(string s)
        {
            text = s ?? "";
            Rebuild();
        }

        void LateUpdate() { if (shown != text || shownColor != color || shownScale != scale || shownCenter != centered) Rebuild(); }

        void Rebuild()
        {
            shown = text; shownColor = color; shownScale = scale; shownCenter = centered;
            string up = text.ToUpperInvariant();
            while (main.Count < up.Length) { main.Add(NewGlyph("g")); back.Add(NewGlyph("s")); }
            float adv = 6 * scale / 40f;
            float x0 = centered ? -(up.Length * adv) * 0.5f : 0f;
            for (int i = 0; i < main.Count; i++)
            {
                bool on = i < up.Length && Art.Glyph(up[i]) != null;
                main[i].gameObject.SetActive(on); back[i].gameObject.SetActive(on && shadow);
                if (!on) continue;
                var spr = Art.Glyph(up[i]);
                main[i].sprite = spr; back[i].sprite = spr;
                main[i].color = color; back[i].color = new Color(0.16f, 0.04f, 0.2f, color.a);
                main[i].transform.localScale = new Vector3(scale, scale, 1); back[i].transform.localScale = new Vector3(scale, scale, 1);
                main[i].transform.localPosition = new Vector3(x0 + i * adv, 0, 0);
                float so = Mathf.Max(1, scale / 3) / 40f; back[i].transform.localPosition = new Vector3(x0 + i * adv + so, -so, 0);
                main[i].sortingOrder = order + 1; back[i].sortingOrder = order;
            }
        }

        SpriteRenderer NewGlyph(string n)
        {
            var go = new GameObject(n); go.transform.SetParent(transform, false);
            return go.AddComponent<SpriteRenderer>();
        }
    }
}
