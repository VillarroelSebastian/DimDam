using System;
using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    [Serializable] public class AnimDef { public string name; public int start; public int count; public float fps; public bool loop; }

    [Serializable]
    public class SheetDef
    {
        public string name, file;
        public int cw, ch, cols, count;
        public float ppu, px, py;
        public AnimDef[] anims;
        public float[] shoulders;   // hero sheets: shoulder pixel (x,y from top-left) per frame
        public float[] armdata;     // arm sheets: anchor x,y + muzzle x,y per frame
        public float[] anchors;     // boss sheets: weak-point pixel (x,y from top-left) per frame
        public string chars;        // font
        public int glyphW, glyphH, glyphCols;
    }

    [Serializable] class Manifest { public SheetDef[] sheets; }

    public class Clip
    {
        public string name; public Sprite[] frames; public float fps; public bool loop;
    }

    public class Sheet
    {
        public SheetDef def; public Texture2D tex; public Sprite[] sprites;
        public Dictionary<string, Clip> clips = new Dictionary<string, Clip>();
        public Sheet white;      // white silhouette twin used for hit flashes
        public Sprite this[int i] { get { return sprites[Mathf.Clamp(i, 0, sprites.Length - 1)]; } }
        public Clip Clip(string n) { Clip c; if (!clips.TryGetValue(n, out c)) throw new Exception("No clip '" + n + "' in sheet " + def.name); return c; }
        public bool Has(string n) { return clips.ContainsKey(n); }
    }

    /// <summary>Loads the sprite-sheet manifest written by ArtSource~/build_all.py.</summary>
    public static class Art
    {
        static Dictionary<string, SheetDef> defs;
        static readonly Dictionary<string, Sheet> cache = new Dictionary<string, Sheet>();
        static readonly Dictionary<char, Sprite> glyphs = new Dictionary<char, Sprite>();

        // Enter Play Mode Options may leave the domain alive between runs: sprites/textures from the last run are destroyed, so forget them.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { defs = null; cache.Clear(); glyphs.Clear(); }

        public static void Init()
        {
            if (defs != null) return;
            var ta = Resources.Load<TextAsset>("GumRemix/manifest");
            if (ta == null) throw new Exception("GumRemix manifest not found in Resources/GumRemix");
            var m = JsonUtility.FromJson<Manifest>(ta.text);
            defs = new Dictionary<string, SheetDef>();
            foreach (var s in m.sheets) defs[s.name] = s;
        }

        public static bool Exists(string name) { Init(); return defs.ContainsKey(name); }

        public static Sheet Get(string name)
        {
            Init();
            Sheet sh;
            if (cache.TryGetValue(name, out sh)) return sh;
            SheetDef d;
            if (!defs.TryGetValue(name, out d)) throw new Exception("Unknown sheet " + name);
            sh = new Sheet { def = d };
            sh.tex = Resources.Load<Texture2D>(d.file);
            if (sh.tex == null) throw new Exception("Texture missing: " + d.file);
            sh.tex.filterMode = FilterMode.Point; sh.tex.wrapMode = TextureWrapMode.Clamp;
            int rows = (d.count + d.cols - 1) / d.cols;
            sh.sprites = new Sprite[d.count];
            for (int i = 0; i < d.count; i++)
            {
                int c = i % d.cols, r = i / d.cols;
                var rect = new Rect(c * d.cw, (rows - 1 - r) * d.ch, d.cw, d.ch);
                sh.sprites[i] = Sprite.Create(sh.tex, rect, new Vector2(d.px, d.py), d.ppu, 0, SpriteMeshType.FullRect);
                sh.sprites[i].name = d.name + "_" + i;
            }
            if (d.anims != null)
                foreach (var a in d.anims)
                {
                    var frames = new Sprite[a.count];
                    Array.Copy(sh.sprites, a.start, frames, 0, a.count);
                    sh.clips[a.name] = new Clip { name = a.name, frames = frames, fps = a.fps, loop = a.loop };
                }
            cache[name] = sh;
            if (!name.EndsWith("_w") && defs.ContainsKey(name + "_w")) sh.white = Get(name + "_w");
            return sh;
        }

        public static Sprite Glyph(char c)
        {
            Sprite s;
            if (glyphs.TryGetValue(c, out s)) return s;
            var f = Get("font");
            int idx = f.def.chars.IndexOf(c);
            if (idx < 0) { glyphs[c] = null; return null; }
            int rows = (f.def.chars.Length + f.def.glyphCols - 1) / f.def.glyphCols;
            int col = idx % f.def.glyphCols, row = idx / f.def.glyphCols;
            s = Sprite.Create(f.tex, new Rect(col * f.def.glyphW, (rows - 1 - row) * f.def.glyphH, f.def.glyphW, f.def.glyphH), Vector2.zero, f.def.ppu, 0, SpriteMeshType.FullRect);
            glyphs[c] = s;
            return s;
        }
    }

    /// <summary>Frame player. Owners tick it with their own (hit-stop aware) delta time.</summary>
    public class Anim
    {
        public SpriteRenderer sr; public Sheet sheet; public Clip clip;
        public float t; public bool done; public float speed = 1f; public bool flash;
        public int Frame { get; private set; }

        public Anim(SpriteRenderer r, Sheet s, string clipName = null) { sr = r; sheet = s; if (clipName != null) Play(clipName, true); }

        public void Play(string name, bool restart = false)
        {
            if (clip != null && clip.name == name && !restart) return;
            clip = sheet.Clip(name); t = 0; done = false; Apply(0);
        }

        /// <summary>Switch to another sheet that shares the clip name (keeps the playhead).</summary>
        public void SetSheet(Sheet s)
        {
            if (s == sheet) return;
            sheet = s;
            if (clip != null) { clip = s.Clip(clip.name); Apply(Frame); }
        }

        /// <summary>Switch sheet AND clip (restarts).</summary>
        public void Use(Sheet s, string clipName, bool restart = false)
        {
            if (sheet == s && clip != null && clip.name == clipName && !restart) return;
            sheet = s; clip = s.Clip(clipName); t = 0; done = false; Apply(0);
        }

        public void Tick(float dt)
        {
            if (clip == null) return;
            t += dt * clip.fps * speed;
            int n = clip.frames.Length, i;
            if (clip.loop) i = ((int)t) % n;
            else { i = Mathf.Min((int)t, n - 1); if (t >= n) done = true; }
            Apply(i);
        }

        public void Apply(int i)
        {
            Frame = i;
            var s = (flash && sheet.white != null) ? sheet.white : sheet;
            var c = s.clips.ContainsKey(clip.name) ? s.clips[clip.name] : clip;
            sr.sprite = c.frames[Mathf.Clamp(i, 0, c.frames.Length - 1)];
        }
    }
}
