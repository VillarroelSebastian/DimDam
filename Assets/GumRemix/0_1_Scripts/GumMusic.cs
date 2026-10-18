using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace GumRemix
{
    /// <summary>Every piece of music in the game. The first ten values match the level themes (0..9).</summary>
    public enum Track
    {
        Prado = 0, Cueva = 1, Nubes = 2, Fabrica = 3, CircoBoss1 = 4,
        Lago = 5, Aldea = 6, Cristales = 7, Jardines = 8, PalacioBoss1 = 9,
        Title = 10, Victory = 11, GameOver = 12, WorldDone = 13,
        CircoBoss2 = 14, CircoBoss3 = 15, PalacioBoss2 = 16, PalacioBoss3 = 17
    }

    // ================================================================================================================
    //  SONG DATA. Pitches are written as note names ("C#5"), durations in sixteenths ("E5:4" = a quarter note).
    //  A bar is 16 units (4/4) or 12 units (3/4). Chords: roots A-G with optional #/b and a quality
    //  ("", m, 7, m7, maj7, 6, m6, dim, sus4, 5). A bar with two chords splits in half. Bars are separated by '|'.
    // ================================================================================================================
    public class Section { public string chords, melody; }

    public class Song
    {
        public string name; public float bpm = 120f, swing; public int beats = 4; public int transpose;
        public string lead = "pulse", drums = "swing", bass = "walk", comp = "none", compInst = "stab";
        public bool pad, harmony, loop = true, fills = true; public float echo, leadGain = 1f, targetRms = 0.13f;
        public Dictionary<string, Section> sec = new Dictionary<string, Section>(); public string form = "A B A B";
        public int BarUnits { get { return beats * 4; } }

        public Song Add(string id, string chords, string melody) { sec[id] = new Section { chords = chords, melody = melody }; return this; }
        public Song Clone() { var s = (Song)MemberwiseClone(); s.sec = new Dictionary<string, Section>(sec); return s; }
    }

    public static class SongBook
    {
        static Song S(string name, float bpm, float swing, string lead, string drums, string bass, string comp, bool pad, float echo, string form = "A B A B")
        {
            return new Song { name = name, bpm = bpm, swing = swing, lead = lead, drums = drums, bass = bass, comp = comp, pad = pad, echo = echo, form = form };
        }

        public static Song Get(Track t)
        {
            switch (t)
            {
                case Track.Title: return Title();
                case Track.Prado: return Prado();
                case Track.Cueva: return Cueva();
                case Track.Nubes: return Nubes();
                case Track.Fabrica: return Fabrica();
                case Track.CircoBoss1: return Circo();
                case Track.CircoBoss2: { var s = Circo(); s.name = "Salon II"; s.transpose = 2; s.bpm = 150f; s.harmony = true; s.echo = 0.1f; return s; }
                case Track.CircoBoss3: { var s = Circo(); s.name = "Salon III"; s.transpose = 4; s.bpm = 168f; s.harmony = true; s.echo = 0.22f; s.pad = true; return s; }
                case Track.Lago: return Lago();
                case Track.Aldea: return Aldea();
                case Track.Cristales: return Cristales();
                case Track.Jardines: return Jardines();
                case Track.PalacioBoss1: return Palacio();
                case Track.PalacioBoss2: { var s = Palacio(); s.name = "Palacio II"; s.transpose = 2; s.bpm = 138f; s.drums = "gallop"; s.harmony = true; return s; }
                case Track.PalacioBoss3: { var s = Palacio(); s.name = "Palacio III"; s.transpose = 3; s.bpm = 156f; s.drums = "gallop"; s.harmony = true; s.echo = 0.3f; return s; }
                case Track.Victory: return Victory();
                case Track.WorldDone: return WorldDone();
                default: return GameOver();
            }
        }

        // --------------------------------------------------------------------------------------------- menu: the Chicle motif
        static Song Title()
        {
            var s = S("Chicle", 104f, 0f, "musicbox", "waltz", "waltz", "waltz", true, 0.3f, "A B A B");
            s.beats = 3; s.harmony = false; s.targetRms = 0.12f;
            s.Add("A", "Am | Am | F | E | Am | Dm | E | Am",
                "E5:4 A5:4 C6:4 | B5:4 A5:4 E5:4 | F5:4 A5:4 C6:4 | B5:4 G#5:4 E5:4 | A5:4 C6:4 E6:4 | D6:4 F5:4 A5:4 | G#5:4 B5:4 D6:4 | A5:8 r:4");
            s.Add("B", "C | G | Am | E | F | Dm | E | Am",
                "E6:4 G6:4 E6:4 | D6:4 B5:4 G5:4 | C6:4 E6:4 A5:4 | G#5:4 B5:4 E6:4 | F6:4 A5:4 C6:4 | D6:4 A5:4 F5:4 | E5:4 G#5:4 B5:4 | A5:8 r:4");
            return s;
        }

        // --------------------------------------------------------------------------------------------- candy world
        static Song Prado()
        {
            var s = S("Jardin Nocturno", 98f, 0f, "musicbox", "half", "sub", "arp8", true, 0.3f);
            s.compInst = "pluck"; s.targetRms = 0.12f;
            s.Add("A", "Am | Am | F | E | Am | Dm | F E | Am",
                "A5:4 C6:2 E6:2 D6:4 C6:4 | B5:4 A5:4 E5:8 | F5:4 A5:2 C6:2 B5:4 A5:4 | G#5:4 B5:4 E6:8 | " +
                "A5:4 C6:2 E6:2 A6:4 G6:4 | F6:4 D6:4 A5:4 F5:4 | A5:2 C6:2 F6:4 E6:2 B5:2 G#5:4 | A5:12 r:4");
            s.Add("B", "C | G | Am | F | Dm | E | F E | Am",
                "E6:4 G6:4 E6:4 C6:4 | D6:4 B5:4 G5:8 | C6:4 E6:4 A6:4 E6:4 | C6:4 A5:4 F5:8 | " +
                "D6:4 F6:4 A6:4 F6:4 | G#5:4 B5:4 E6:4 D6:4 | C6:4 A5:4 B5:2 G#5:2 E5:4 | A5:8 E5:4 A5:4");
            return s;
        }

        static Song Cueva()
        {
            var s = S("Cueva de Caramelo", 100f, 0.35f, "pulse", "half", "pulse", "offbeat", true, 0.25f);
            s.Add("A", "Am | Am | F | E7 | Am | Dm | F E7 | Am",
                "A5:4 C6:2 B5:2 A5:4 E5:4 | E5:2 A5:2 C6:4 B5:2 A5:2 G#5:4 | F5:4 A5:2 C6:2 A5:4 F5:4 | E5:2 G#5:2 B5:4 D6:4 B5:4 | " +
                "A5:4 C6:2 E6:2 D6:4 C6:4 | D6:4 C6:2 A5:2 F5:4 A5:4 | F5:2 A5:2 C6:4 B5:2 G#5:2 E5:4 | A5:8 r:8");
            s.Add("B", "Dm | Am | Bb | E7 | Dm | Am | F | E7",
                "F5:4 A5:4 D6:4 A5:4 | E5:4 A5:4 C6:4 E6:4 | D6:4 Bb5:2 D6:2 F6:4 D6:4 | E6:4 D6:2 B5:2 G#5:4 B5:4 | " +
                "D6:2 F6:2 A6:4 F6:4 D6:4 | C6:2 E6:2 A6:4 E6:4 C6:4 | A5:4 C6:4 F6:4 C6:4 | B5:2 G#5:2 E5:4 G#5:2 B5:2 E6:4");
            return s;
        }

        static Song Nubes()
        {
            var s = S("Nubes de Tormenta", 88f, 0f, "bell", "half", "pulse", "arp8", true, 0.35f);
            s.compInst = "pluck";
            s.Add("A", "Em7 | Cmaj7 | Am7 | B7 | Em7 | Cmaj7 | Am7 B7 | Em7",
                "B5:4 E6:4 G6:4 E6:4 | C6:4 E6:4 G6:4 B6:4 | A5:4 C6:4 E6:4 C6:4 | D#6:4 F#6:4 B6:4 F#6:4 | " +
                "G6:6 E6:2 B5:8 | E6:6 C6:2 G5:8 | A5:2 C6:2 E6:4 D#6:2 F#6:2 B6:4 | E6:12 r:4");
            s.Add("B", "Am7 | Em7 | Cmaj7 | B7 | Am7 | Em7 | Cmaj7 B7 | Em7",
                "C6:4 E6:4 A6:4 E6:4 | B5:4 E6:4 G6:4 B6:4 | G6:4 E6:4 C6:4 G5:4 | F#6:4 D#6:4 B5:4 F#5:4 | " +
                "A5:4 C6:4 E6:8 | G5:4 B5:4 E6:8 | E6:2 G6:2 C7:4 B6:2 D#6:2 F#6:4 | E6:8 B5:4 E6:4");
            return s;
        }

        static Song Fabrica()
        {
            var s = S("Fabrica de Chocolate", 136f, 0f, "pulse", "factory", "pulse", "offbeat", false, 0f);
            s.Add("A", "Dm | Dm | Bb | C | Dm | Dm | Gm | A7",
                "D5:2 r:2 D5:2 F5:2 A5:2 r:2 F5:2 D5:2 | D5:2 r:2 D5:2 F5:2 A5:2 C6:2 A5:2 F5:2 | Bb4:2 r:2 Bb4:2 D5:2 F5:2 r:2 D5:2 Bb4:2 | C5:2 r:2 C5:2 E5:2 G5:2 r:2 E5:2 C5:2 | " +
                "D5:2 r:2 D5:2 F5:2 A5:2 r:2 F5:2 D5:2 | D5:2 F5:2 A5:2 D6:2 C6:2 A5:2 F5:2 D5:2 | G5:2 r:2 G5:2 Bb5:2 D6:2 r:2 Bb5:2 G5:2 | A5:2 C#6:2 E6:2 G6:2 E6:2 C#6:2 A5:4");
            s.Add("B", "Gm | Gm | Dm | Dm | Bb | C | A7 | A7",
                "D6:4 Bb5:2 G5:2 D6:4 Bb5:4 | G5:2 Bb5:2 D6:4 G6:4 D6:4 | F6:4 D6:2 A5:2 F6:4 D6:4 | A5:2 D6:2 F6:4 A6:4 F6:4 | " +
                "D6:4 F6:4 Bb6:4 F6:4 | E6:4 G6:4 C7:4 G6:4 | E6:2 A6:2 C#7:4 A6:2 E6:2 C#6:4 | A6:8 r:8");
            return s;
        }

        static Song Circo()
        {
            var s = S("Salon del Trono", 132f, 0f, "trumpet", "waltz", "waltz", "waltz", true, 0.05f);
            s.beats = 3; s.compInst = "stab"; s.targetRms = 0.13f;
            s.Add("A", "Dm | Dm | Gm | A7 | Dm | Bb | A7 | Dm",
                "D5:4 F5:4 A5:4 | D6:6 C6:2 A5:4 | Bb5:4 D6:4 G6:4 | E6:4 C#6:4 A5:4 | F5:4 A5:4 D6:4 | D6:4 F6:4 Bb6:4 | A6:4 E6:4 C#6:4 | D6:8 r:4");
            s.Add("B", "Bb | C | Dm | A7 | Gm | Dm | A7 | Dm",
                "F6:4 D6:4 Bb5:4 | G6:4 E6:4 C6:4 | A6:4 F6:4 D6:4 | C#6:4 E6:4 A6:4 | G6:4 Bb6:4 D7:4 | F6:4 A6:4 D7:4 | E7:4 C#7:4 A6:4 | D7:8 A6:4");
            return s;
        }

        // --------------------------------------------------------------------------------------------- ice world
        static Song Lago()
        {
            var s = S("Lago de los Susurros", 84f, 0f, "bell", "half", "sub", "arp8", true, 0.35f);
            s.compInst = "bell"; s.targetRms = 0.115f;
            s.Add("A", "Dm7 | Bbmaj7 | Gm7 | Asus4 A | Dm7 | Bbmaj7 | Gm7 | A",
                "A5:6 F5:2 D5:4 r:4 | D6:4 C6:4 Bb5:8 | G5:6 Bb5:2 D6:8 | E6:6 D6:2 C#6:4 E6:4 | " +
                "F6:6 D6:2 A5:4 r:4 | D6:4 F6:4 A6:8 | G6:4 F6:2 D6:2 Bb5:8 | A5:4 C#6:4 E6:8");
            s.Add("B", "Gm7 | Dm7 | Bbmaj7 | A | Gm7 | Dm7 | Bbmaj7 | A",
                "D6:6 Bb5:2 G5:4 Bb5:4 | A5:6 F5:2 D5:4 F5:4 | F5:4 Bb5:4 D6:4 F6:4 | E6:6 C#6:2 A5:8 | " +
                "G6:6 D6:2 Bb5:4 D6:4 | F6:6 D6:2 A5:4 D6:4 | D6:4 F6:4 Bb6:4 A6:4 | A6:12 r:4");
            return s;
        }

        static Song Aldea()
        {
            var s = S("Aldea de Iglus Torcidos", 112f, 0f, "musicbox", "waltz", "waltz", "waltz", false, 0.15f);
            s.beats = 3; s.harmony = false;
            s.Add("A", "G | Em | C | D | G | Em | Am D | G",
                "G5:4 B5:4 D6:4 | E6:4 D6:4 B5:4 | C6:4 E6:4 G6:4 | F#6:4 E6:2 D6:2 A5:4 | B5:2 D6:2 G6:4 D6:4 | E6:4 G6:4 B6:4 | A5:4 C6:2 D6:2 F#6:2 A6:2 | G6:8 r:4");
            s.Add("B", "C | G | Am | D | C | G | Am D | G",
                "E6:4 G6:4 C7:4 | D6:4 B5:4 G5:4 | A5:4 C6:4 E6:4 | D6:4 F#6:4 A6:4 | G6:4 E6:4 C6:4 | B5:4 D6:4 G6:4 | C6:4 A5:2 F#5:2 A5:2 D6:2 | B5:4 D6:4 G6:4");
            return s;
        }

        static Song Cristales()
        {
            var s = S("Cristales Cantores", 96f, 0f, "bell", "half", "pulse", "arp8", true, 0.4f);
            s.compInst = "pluck";
            s.Add("A", "Em | C | G | D | Em | C | Am B7 | Em",
                "B5:2 E6:2 G6:4 E6:2 B5:2 G5:4 | C6:2 E6:2 G6:4 E6:2 C6:2 G5:4 | D6:2 G6:2 B6:4 G6:2 D6:2 B5:4 | A5:2 D6:2 F#6:4 D6:2 A5:2 F#5:4 | " +
                "G6:2 B6:2 E7:4 B6:2 G6:2 E6:4 | E6:2 G6:2 C7:4 G6:2 E6:2 C6:4 | A5:2 C6:2 E6:4 D#6:2 F#6:2 B6:4 | E6:8 r:8");
            s.Add("B", "Am | B7 | Em | Em | C | D | G B7 | Em",
                "E6:4 C6:4 A5:4 C6:4 | D#6:4 F#6:4 B6:4 F#6:4 | G6:4 E6:2 B5:2 E6:8 | B5:2 E6:2 G6:4 B6:8 | " +
                "G6:4 E6:4 C6:4 E6:4 | F#6:4 D6:4 A5:4 D6:4 | G6:4 D6:4 F#6:4 D#6:4 | E6:12 r:4");
            return s;
        }

        static Song Jardines()
        {
            var s = S("Jardines Congelados", 108f, 0f, "pulse", "bounce", "pulse", "offbeat", true, 0.25f);
            s.Add("A", "Am | F | C | G | Am | F | G | Am",
                "A5:2 C6:2 E6:4 C6:2 A5:2 E5:4 | F5:2 A5:2 C6:4 A5:2 F5:2 C5:4 | E5:2 G5:2 C6:4 G5:2 E5:2 C5:4 | D5:2 G5:2 B5:4 D6:4 B5:4 | " +
                "C6:2 E6:2 A6:4 E6:2 C6:2 A5:4 | A5:2 C6:2 F6:4 C6:2 A5:2 F5:4 | B5:2 D6:2 G6:4 D6:2 B5:2 G5:4 | A5:8 r:4 E5:2 A5:2");
            s.Add("B", "Dm | Am | Bb | G | Dm | Am | F G | Am",
                "F5:4 A5:4 D6:4 A5:4 | E5:4 A5:4 C6:4 E6:4 | D6:4 Bb5:4 F5:4 D5:4 | B5:4 D6:4 G6:4 D6:4 | " +
                "A5:2 D6:2 F6:4 D6:2 A5:2 F5:4 | C6:2 E6:2 A6:4 E6:2 C6:2 A5:4 | F6:4 C6:4 D6:4 B5:4 | A5:12 r:4");
            return s;
        }

        static Song Palacio()
        {
            var s = S("Palacio de la Ultima Nevada", 120f, 0f, "trumpet", "march", "pulse", "none", true, 0f);
            s.Add("A", "Cm | Cm | Ab | Bb | Cm | Fm | G | Cm",
                "C5:6 Eb5:2 G5:4 C6:4 | Bb5:4 G5:4 Eb5:8 | Ab5:6 C6:2 Eb6:4 C6:4 | D6:4 Bb5:4 F5:8 | " +
                "C6:4 Eb6:4 G6:4 Eb6:4 | F6:4 Ab5:4 C6:4 Ab5:4 | B5:4 D6:4 G6:4 F6:4 | Eb6:8 C6:8");
            s.Add("B", "Ab | Eb | Bb | Cm | Ab | Fm | G | G",
                "Ab5:4 C6:4 Eb6:4 Ab6:4 | G5:4 Bb5:4 Eb6:4 G6:4 | F6:4 D6:4 Bb5:4 D6:4 | Eb6:4 C6:4 G5:4 C6:4 | " +
                "Eb6:4 Ab6:4 C7:4 Ab6:4 | C7:4 Ab6:4 F6:4 C6:4 | D6:4 G6:4 B6:4 D7:4 | G6:12 r:4");
            return s;
        }

        // --------------------------------------------------------------------------------------------- jingles (do not loop)
        static Song Victory()
        {
            var s = S("Victoria", 132f, 0.3f, "trumpet", "march", "oompah", "charleston", true, 0f, "A");
            s.loop = false; s.harmony = true; s.targetRms = 0.14f; s.fills = false;
            s.Add("A", "C | F | C G | C | F | Fm | C G | C",
                "C5:2 E5:2 G5:2 C6:4 G5:2 C6:4 | A5:2 C6:2 F6:4 E6:2 C6:2 A5:4 | G5:2 C6:2 E6:4 D6:2 B5:2 G5:4 | C6:8 E6:4 G6:4 | " +
                "A5:2 C6:2 F6:4 A6:4 F6:4 | Ab5:2 C6:2 F6:4 Ab6:4 F6:4 | G6:4 E6:4 D6:4 B5:4 | C7:8 r:8");
            return s;
        }

        static Song WorldDone()
        {
            var s = S("Mundo completo", 112f, 0.25f, "trumpet", "march", "oompah", "charleston", true, 0.1f, "A");
            s.loop = false; s.harmony = true; s.targetRms = 0.14f; s.fills = false;
            s.Add("A", "G | Em | C D | G",
                "B5:2 D6:2 G6:4 D6:2 B5:2 G5:4 | E6:2 G6:2 B6:4 G6:2 E6:2 B5:4 | C6:4 E6:4 D6:4 F#6:4 | G6:12 r:4");
            return s;
        }

        static Song GameOver()
        {
            var s = S("Fin de la partida", 76f, 0f, "bell", "none", "sub", "none", true, 0.3f, "A");
            s.loop = false; s.targetRms = 0.11f; s.fills = false;
            s.Add("A", "Am | F | Dm E | Am",
                "E6:4 C6:4 A5:4 E5:4 | C6:4 A5:4 F5:8 | D5:4 F5:4 E5:4 G#5:4 | A5:12 r:4");
            return s;
        }
    }

    // ================================================================================================================
    //  RENDERER: turns a Song into stereo float samples (offline, thread-safe, deterministic).
    // ================================================================================================================
    public static class MusicRenderer
    {
        public const int SR = 32000;
        const float TAU = 6.2831853f;

        struct Ev { public int start, len; public float freq, amp, pan; public string inst; }
        struct Chord { public int rootPc; public int[] iv; public string nm; }

        static readonly Dictionary<char, int> Pc = new Dictionary<char, int> { { 'C', 0 }, { 'D', 2 }, { 'E', 4 }, { 'F', 5 }, { 'G', 7 }, { 'A', 9 }, { 'B', 11 } };

        public static int NoteMidi(string n, int transpose)
        {
            int pc; if (!Pc.TryGetValue(n[0], out pc)) throw new Exception("Bad note '" + n + "'");
            int i = 1; if (i < n.Length && n[i] == '#') { pc++; i++; } else if (i < n.Length && n[i] == 'b') { pc--; i++; }
            int oct = int.Parse(n.Substring(i));
            return 12 * (oct + 1) + pc + transpose;
        }

        static float Hz(float midi) { return 440f * Mathf.Pow(2f, (midi - 69f) / 12f); }

        static Chord ParseChord(string c, int transpose)
        {
            int pc = Pc[c[0]]; int i = 1;
            if (i < c.Length && c[i] == '#') { pc++; i++; } else if (i < c.Length && c[i] == 'b') { pc--; i++; }
            string q = c.Substring(i); int[] iv;
            switch (q)
            {
                case "": iv = new[] { 0, 4, 7 }; break;
                case "m": iv = new[] { 0, 3, 7 }; break;
                case "7": iv = new[] { 0, 4, 7, 10 }; break;
                case "m7": iv = new[] { 0, 3, 7, 10 }; break;
                case "maj7": iv = new[] { 0, 4, 7, 11 }; break;
                case "6": iv = new[] { 0, 4, 7, 9 }; break;
                case "m6": iv = new[] { 0, 3, 7, 9 }; break;
                case "dim": iv = new[] { 0, 3, 6 }; break;
                case "sus4": iv = new[] { 0, 5, 7 }; break;
                case "5": iv = new[] { 0, 7, 12 }; break;
                default: throw new Exception("Bad chord '" + c + "'");
            }
            return new Chord { rootPc = ((pc + transpose) % 12 + 12) % 12, iv = iv, nm = c };
        }

        // ---------------------------------------------------------------------------------------------- scheduling
        public static float[] Render(Song s, out int loopSamples)
        {
            int bu = s.BarUnits; float step = 60f / s.bpm / 4f; int stepS = Mathf.RoundToInt(step * SR);
            var form = s.form.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int bars = 0; foreach (var f in form) bars += s.sec[f].chords.Split('|').Length;
            int barS = stepS * bu; loopSamples = bars * barS;
            int tail = SR * 2; int total = loopSamples + tail;
            var evLead = new List<Ev>(); var evBack = new List<Ev>(); var evDrum = new List<Ev>();
            var rng = new System.Random(s.name.GetHashCode() ^ 1234);

            int barIndex = 0;
            foreach (var secId in form)
            {
                var sec = s.sec[secId];
                var chordBars = sec.chords.Split('|'); var melBars = sec.melody.Split('|');
                if (chordBars.Length != melBars.Length) throw new Exception(s.name + "/" + secId + ": " + chordBars.Length + " chord bars vs " + melBars.Length + " melody bars");
                for (int b = 0; b < chordBars.Length; b++, barIndex++)
                {
                    int barStart = barIndex * barS;
                    var ct = chordBars[b].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    var chords = new Chord[ct.Length]; for (int k = 0; k < ct.Length; k++) chords[k] = ParseChord(ct[k], s.transpose);
                    Chord next = chords[chords.Length - 1];
                    // the chord of the following bar (for walking bass approach notes)
                    string nextTok = null;
                    if (b + 1 < chordBars.Length) nextTok = chordBars[b + 1].Trim().Split(' ')[0]; else nextTok = sec.chords.Split('|')[0].Trim().Split(' ')[0];
                    var nextChord = ParseChord(nextTok, s.transpose);

                    // ---- melody
                    int u = 0;
                    foreach (var tok in melBars[b].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        var p = tok.Split(':'); int dur = p.Length > 1 ? int.Parse(p[1]) : 2;
                        if (p[0] != "r")
                        {
                            int m = NoteMidi(p[0], s.transpose);
                            float off = (s.swing > 0f && u % 4 == 2) ? s.swing * 0.667f * stepS : 0f;
                            int st = barStart + u * stepS + Mathf.RoundToInt(off);
                            float len = dur * stepS * (dur >= 4 ? 0.96f : 0.9f);
                            evLead.Add(new Ev { start = st, len = Mathf.RoundToInt(len), freq = Hz(m), amp = 0.30f * s.leadGain, pan = -0.1f, inst = s.lead });
                            if (s.harmony) evBack.Add(new Ev { start = st, len = Mathf.RoundToInt(len), freq = Hz(m - 12), amp = 0.12f, pan = 0.25f, inst = s.lead == "trumpet" ? "trumpet" : "pulse" });
                        }
                        u += dur;
                    }
                    if (u != bu) throw new Exception(s.name + "/" + secId + " bar " + (b + 1) + ": melody has " + u + " units, expected " + bu);

                    // ---- harmony per chord segment
                    int segUnits = bu / chords.Length;
                    for (int k = 0; k < chords.Length; k++)
                    {
                        var c = chords[k]; int segStart = barStart + k * segUnits * stepS;
                        var nc = k + 1 < chords.Length ? chords[k + 1] : nextChord;
                        AddBass(evBack, s, c, nc, segStart, segUnits, stepS, k);
                        AddComp(evBack, s, c, segStart, segUnits, stepS);
                        if (s.pad) AddPad(evBack, c, segStart, segUnits * stepS);
                    }
                    AddDrums(evDrum, s, barStart, stepS, bu, barIndex, rng, (barIndex % 4 == 3) && s.fills);
                }
            }

            // ---- mix. Lead bus is separate so it can get an echo.
            var leadL = new float[total]; var leadR = new float[total]; var mixL = new float[total]; var mixR = new float[total];
            foreach (var e in evLead) Voice(e, leadL, leadR, total, rng);
            foreach (var e in evBack) Voice(e, mixL, mixR, total, rng);
            foreach (var e in evDrum) Voice(e, mixL, mixR, total, rng);
            if (s.echo > 0f) Echo(leadL, leadR, stepS * 6, 0.38f, s.echo, s.loop ? loopSamples : total);
            for (int i = 0; i < total; i++) { mixL[i] += leadL[i]; mixR[i] += leadR[i]; }

            // ---- loop: the tail wraps around onto the beginning, so the seam is seamless
            int outLen = s.loop ? loopSamples : total;
            var outL = new float[outLen]; var outR = new float[outLen];
            for (int i = 0; i < total; i++)
            {
                int j = s.loop ? i % loopSamples : i; if (j >= outLen) continue;
                outL[j] += mixL[i]; outR[j] += mixR[i];
            }
            // loudness: normalise by RMS, then soft-limit
            double acc = 0; for (int i = 0; i < outLen; i++) acc += outL[i] * outL[i] + outR[i] * outR[i];
            float rms = Mathf.Sqrt((float)(acc / (2.0 * outLen))); float g = rms > 1e-6f ? s.targetRms / rms : 1f;
            var data = new float[outLen * 2];
            for (int i = 0; i < outLen; i++)
            {
                data[2 * i] = Soft(outL[i] * g); data[2 * i + 1] = Soft(outR[i] * g);
            }
            if (!s.loop) { int fade = SR / 8; for (int i = 0; i < fade && i < outLen; i++) { float f = 1f - i / (float)fade; int j = outLen - 1 - i; data[2 * j] *= f; data[2 * j + 1] *= f; } }
            loopSamples = outLen;
            return data;
        }

        static float Soft(float x) { return (float)Math.Tanh(x * 1.25) * 0.8f; }

        // ---------------------------------------------------------------------------------------------- accompaniment
        static int BassNote(Chord c, int interval, int baseOct = 36)
        {
            int root = baseOct + c.rootPc; if (root > baseOct + 7) root -= 12;       // keep the bass between roughly E1 and G2
            return root + interval;
        }

        static void AddBass(List<Ev> ev, Song s, Chord c, Chord next, int st, int units, int stepS, int segIndex)
        {
            float amp = 0.40f; int root = BassNote(c, 0); int fifth = BassNote(c, 7);
            int third = BassNote(c, c.iv[1]);
            switch (s.bass)
            {
                case "walk":
                    if (units >= 16)
                    {
                        int approach = BassNote(next, 0) + (((c.rootPc + next.rootPc) % 2 == 0) ? -1 : 1);
                        int[] seq = { root, third, fifth, approach };
                        for (int i = 0; i < 4; i++) ev.Add(new Ev { start = st + i * 4 * stepS, len = (int)(3.6f * stepS), freq = Hz(seq[i]), amp = amp, pan = 0f, inst = "bass" });
                    }
                    else { ev.Add(new Ev { start = st, len = (int)(3.6f * stepS), freq = Hz(root), amp = amp, pan = 0f, inst = "bass" }); ev.Add(new Ev { start = st + 4 * stepS, len = (int)(3.6f * stepS), freq = Hz(fifth), amp = amp, pan = 0f, inst = "bass" }); }
                    break;
                case "oompah":
                    ev.Add(new Ev { start = st, len = (int)(3f * stepS), freq = Hz(root), amp = amp, pan = 0f, inst = "bass" });
                    if (units >= 8) ev.Add(new Ev { start = st + 8 * stepS, len = (int)(3f * stepS), freq = Hz(fifth), amp = amp, pan = 0f, inst = "bass" });
                    if (units >= 16) { ev.Add(new Ev { start = st + 4 * stepS, len = (int)(2.4f * stepS), freq = Hz(root + 12), amp = amp * 0.6f, pan = 0f, inst = "bass" }); ev.Add(new Ev { start = st + 12 * stepS, len = (int)(2.4f * stepS), freq = Hz(fifth), amp = amp * 0.6f, pan = 0f, inst = "bass" }); }
                    break;
                case "pulse":
                    {
                        int[] pat = { 0, 12, 0, 7, 0, 12, 0, 7 };
                        for (int i = 0; i * 2 < units; i++) ev.Add(new Ev { start = st + i * 2 * stepS, len = (int)(1.8f * stepS), freq = Hz(root + pat[i % 8]), amp = amp * (i % 4 == 0 ? 1f : 0.75f), pan = 0f, inst = "bass" });
                    }
                    break;
                case "sub":
                    ev.Add(new Ev { start = st, len = units * stepS, freq = Hz(root), amp = amp * 0.85f, pan = 0f, inst = "sub" });
                    break;
                case "waltz":
                    ev.Add(new Ev { start = st, len = (int)(3.4f * stepS), freq = Hz(root), amp = amp, pan = 0f, inst = "bass" });
                    break;
            }
        }

        static float[] ChordNotes(Chord c, int baseMidi)
        {
            int r = baseMidi + (((c.rootPc - baseMidi) % 12) + 12) % 12;
            var n = new float[c.iv.Length]; for (int i = 0; i < n.Length; i++) n[i] = r + c.iv[i]; return n;
        }

        static void AddComp(List<Ev> ev, Song s, Chord c, int st, int units, int stepS)
        {
            var notes = ChordNotes(c, 55);
            Action<int, float, float> hit = (u, len, amp) =>
            {
                for (int i = 0; i < notes.Length; i++)
                    ev.Add(new Ev { start = st + u * stepS, len = (int)(len * stepS), freq = Hz(notes[i]), amp = amp, pan = (i % 2 == 0 ? -0.35f : 0.35f), inst = s.compInst });
            };
            switch (s.comp)
            {
                case "charleston": hit(0, 3f, 0.085f); if (units >= 8) hit(6, 2f, 0.085f); break;
                case "offbeat": for (int u = 2; u < units; u += 4) hit(u, 1.6f, 0.08f); break;
                case "waltz": if (units >= 12) { hit(4, 3f, 0.075f); hit(8, 3f, 0.075f); } else hit(3, 2f, 0.075f); break;
                case "arp8":
                    {
                        var arp = new List<float>(); foreach (var n in ChordNotes(c, 60)) arp.Add(n); arp.Add(arp[0] + 12);
                        for (int i = 0; i * 2 < units; i++)
                        {
                            int idx = i % (arp.Count * 2 - 2); if (idx >= arp.Count) idx = arp.Count * 2 - 2 - idx;
                            ev.Add(new Ev { start = st + i * 2 * stepS, len = (int)(2.4f * stepS), freq = Hz(arp[idx]), amp = 0.11f, pan = (i % 2 == 0 ? -0.4f : 0.4f), inst = s.compInst });
                        }
                    }
                    break;
            }
        }

        static void AddPad(List<Ev> ev, Chord c, int st, int len)
        {
            var notes = ChordNotes(c, 52);
            for (int i = 0; i < notes.Length; i++) ev.Add(new Ev { start = st, len = len, freq = Hz(notes[i]), amp = 0.075f, pan = (i - notes.Length / 2f) * 0.35f, inst = "pad" });
        }

        // ---------------------------------------------------------------------------------------------- drums
        static void AddDrums(List<Ev> ev, Song s, int barStart, int stepS, int bu, int barIndex, System.Random rng, bool fill)
        {
            string kick, snare, hat, open = "";
            switch (s.drums)
            {
                case "swing": kick = "x.......x......."; snare = "....x.......x..."; hat = "x.x.x.x.x.x.x.x."; break;
                case "half": kick = "x.........x....."; snare = "........x......."; hat = "..x...x...x...x."; break;
                case "march": kick = "x...x...x...x..."; snare = ".x.x.x.x.x.x.x.x"; hat = "................"; break;
                case "gallop": kick = "x..x..x.x..x..x."; snare = "....x.......x..."; hat = "x.x.x.x.x.x.x.xx"; break;
                case "factory": kick = "x.x.x.x.x.x.x.x."; snare = "....x.......x..."; hat = "xxxxxxxxxxxxxxxx"; break;
                case "bounce": kick = "x...x...x...x..."; snare = "....x.......x..."; hat = "..x...x...x...x."; open = "..x...x...x...x."; break;
                case "waltz": kick = "x..........."; snare = "....x...x..."; hat = "x.x.x.x.x.x."; break;
                default: return;
            }
            for (int u = 0; u < bu; u++)
            {
                int st = barStart + u * stepS + ((s.swing > 0f && u % 4 == 2) ? Mathf.RoundToInt(s.swing * 0.667f * stepS) : 0);
                if (u < kick.Length && kick[u] == 'x') ev.Add(new Ev { start = st, len = (int)(0.16f * SR), amp = 0.55f, pan = 0f, inst = "kick" });
                if (u < snare.Length && snare[u] == 'x') ev.Add(new Ev { start = st, len = (int)(0.14f * SR), amp = 0.30f, pan = 0.05f, inst = "snare" });
                if (u < hat.Length && hat[u] == 'x') ev.Add(new Ev { start = st, len = (int)(0.05f * SR), amp = (u % 4 == 0 ? 0.12f : 0.08f), pan = 0.2f, inst = "hat" });
                if (u < open.Length && open[u] == 'x' && s.drums == "bounce") ev.Add(new Ev { start = st, len = (int)(0.14f * SR), amp = 0.07f, pan = -0.2f, inst = "ohat" });
            }
            if (fill && bu == 16)
                for (int u = 12; u < 16; u++) ev.Add(new Ev { start = barStart + u * stepS, len = (int)(0.1f * SR), amp = 0.22f + 0.02f * (u - 12), pan = -0.1f + 0.1f * (u - 12), inst = "snare" });
        }

        // ---------------------------------------------------------------------------------------------- instruments
        static void Voice(Ev e, float[] L, float[] R, int total, System.Random rng)
        {
            float gl = Mathf.Sqrt((1f - e.pan) * 0.5f), gr = Mathf.Sqrt((1f + e.pan) * 0.5f);
            int rel = SR / 14; int n = e.len + rel; double ph = 0, ph2 = 0, ph3 = 0; float f = e.freq; float prevNoise = 0f;
            for (int i = 0; i < n; i++)
            {
                int idx = e.start + i; if (idx >= total) break; if (idx < 0) continue;
                float t = i / (float)SR; float env; float v;
                switch (e.inst)
                {
                    case "pulse":      // chip lead with a late vibrato
                        {
                            float vib = 1f + 0.004f * Mathf.Sin(TAU * 5.4f * t) * Mathf.Clamp01((t - 0.12f) * 6f);
                            ph += f * vib / SR; ph -= Math.Floor(ph); ph2 += f * 1.004f / SR; ph2 -= Math.Floor(ph2);
                            v = ((ph < 0.3 ? 1f : -1f) + 0.5f * (ph2 < 0.5 ? 1f : -1f)) * 0.6f;
                            env = Adsr(i, e.len, rel, 0.004f, 0.09f, 0.62f);
                        }
                        break;
                    case "trumpet":     // brass: bright attack that settles
                        {
                            float vib = 1f + 0.005f * Mathf.Sin(TAU * 5.8f * t) * Mathf.Clamp01((t - 0.1f) * 5f);
                            ph += f * vib / SR; ph -= Math.Floor(ph);
                            float bright = 0.35f + 0.65f * Mathf.Exp(-t * 7f); v = 0f;
                            for (int h = 1; h <= 7; h++) v += Mathf.Sin(TAU * (float)ph * h) / h * (h == 1 ? 1f : bright);
                            v *= 0.55f; env = Adsr(i, e.len, rel, 0.012f, 0.07f, 0.82f);
                        }
                        break;
                    case "bell":        // FM bell: glassy attack that decays; long notes keep ringing
                        {
                            float dec = Mathf.Exp(-t * 2.2f); float idxm = 2.6f * Mathf.Exp(-t * 5f);
                            ph += f / SR; ph2 += f * 3.5 / SR;
                            v = Mathf.Sin(TAU * (float)(ph - Math.Floor(ph)) + idxm * Mathf.Sin(TAU * (float)(ph2 - Math.Floor(ph2)))) * dec;
                            env = Mathf.Clamp01(i / 40f) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)rel));
                        }
                        break;
                    case "musicbox":    // pluck with a bright, quickly fading third partial
                        {
                            ph += f / SR; ph -= Math.Floor(ph);
                            v = Mathf.Sin(TAU * (float)ph) + 0.45f * Mathf.Exp(-t * 14f) * Mathf.Sin(TAU * (float)ph * 4f) + 0.2f * Mathf.Exp(-t * 20f) * Mathf.Sin(TAU * (float)ph * 7f);
                            env = Mathf.Exp(-t * 4.2f) * Mathf.Clamp01(i / 30f) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)rel));
                        }
                        break;
                    case "pluck":       // soft triangle pluck for arpeggios
                        {
                            ph += f / SR; ph -= Math.Floor(ph);
                            v = Mathf.Abs((float)ph * 4f - 2f) - 1f; v += 0.25f * Mathf.Sin(TAU * (float)ph * 2f);
                            env = Mathf.Exp(-t * 7f) * Mathf.Clamp01(i / 24f) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)rel));
                        }
                        break;
                    case "stab":        // short saw chord hit
                        {
                            ph += f / SR; ph -= Math.Floor(ph);
                            v = (float)(ph * 2.0 - 1.0) * 0.6f + 0.4f * ((ph < 0.5) ? 1f : -1f) * 0.5f;
                            env = Mathf.Exp(-t * 11f) * Mathf.Clamp01(i / 20f) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)rel));
                        }
                        break;
                    case "bass":
                        {
                            ph += f / SR; ph -= Math.Floor(ph);
                            v = (Mathf.Abs((float)ph * 4f - 2f) - 1f) * 0.8f + 0.5f * Mathf.Sin(TAU * (float)ph) + 0.12f * Mathf.Exp(-t * 20f) * ((ph < 0.5) ? 1f : -1f);
                            env = (0.55f + 0.45f * Mathf.Exp(-t * 9f)) * Mathf.Clamp01(i / 30f) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)(rel / 2)));
                        }
                        break;
                    case "sub":
                        {
                            ph += f / SR; ph -= Math.Floor(ph);
                            v = Mathf.Sin(TAU * (float)ph) + 0.18f * Mathf.Sin(TAU * (float)ph * 2f);
                            env = Mathf.Clamp01(i / (SR * 0.06f)) * (i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)rel));
                        }
                        break;
                    case "pad":         // three slightly detuned triangles, slow swell
                        {
                            ph += f * 0.996 / SR; ph -= Math.Floor(ph); ph2 += f / SR; ph2 -= Math.Floor(ph2); ph3 += f * 1.004 / SR; ph3 -= Math.Floor(ph3);
                            v = ((Mathf.Abs((float)ph * 4f - 2f) - 1f) + (Mathf.Abs((float)ph2 * 4f - 2f) - 1f) + (Mathf.Abs((float)ph3 * 4f - 2f) - 1f)) * 0.33f;
                            float a = Mathf.Clamp01(t / 0.35f); float r = i < e.len ? 1f : Mathf.Clamp01(1f - (i - e.len) / (float)(SR / 3));
                            env = a * r;
                            if (i >= e.len + SR / 3) goto done;
                        }
                        break;
                    case "kick":
                        {
                            float fr = 46f + 110f * Mathf.Exp(-t * 38f); ph += fr / SR;
                            v = Mathf.Sin(TAU * (float)(ph - Math.Floor(ph))); env = Mathf.Exp(-t * 15f) * Mathf.Clamp01(i / 8f);
                        }
                        break;
                    case "snare":
                        {
                            float nz = (float)(rng.NextDouble() * 2 - 1); ph += 185f / SR;
                            v = nz * 0.8f * Mathf.Exp(-t * 24f) + Mathf.Sin(TAU * (float)(ph - Math.Floor(ph))) * 0.5f * Mathf.Exp(-t * 34f); env = 1f;
                        }
                        break;
                    case "hat":
                    case "ohat":
                        {
                            float nz = (float)(rng.NextDouble() * 2 - 1); float hp = nz - prevNoise; prevNoise = nz;
                            v = hp * 0.6f * Mathf.Exp(-t * (e.inst == "hat" ? 70f : 20f)); env = 1f;
                        }
                        break;
                    default: v = 0f; env = 0f; break;
                }
                float o = v * env * e.amp; L[idx] += o * gl; R[idx] += o * gr;
            }
        done: return;
        }

        static float Adsr(int i, int len, int rel, float a, float d, float sus)
        {
            float t = i / (float)SR; float e;
            if (t < a) e = t / a; else if (t < a + d) e = 1f - (1f - sus) * ((t - a) / d); else e = sus;
            if (i >= len) e *= Mathf.Clamp01(1f - (i - len) / (float)rel);
            return e;
        }

        /// <summary>Ping-pong feedback delay run twice over the buffer, so the repeats also wrap around the loop point.</summary>
        static void Echo(float[] L, float[] R, int delay, float fb, float wet, int wrapLen)
        {
            int n = Mathf.Min(L.Length, wrapLen > 0 ? wrapLen + SR * 2 : L.Length);
            var dl = new float[delay]; var dr = new float[delay]; int pos = 0;
            for (int pass = 0; pass < 2; pass++)
                for (int i = 0; i < n; i++)
                {
                    float inL = L[i], inR = R[i];
                    float oL = dr[pos], oR = dl[pos];                // cross-feed: ping-pong
                    dl[pos] = inL + oL * fb; dr[pos] = inR + oR * fb;
                    if (pass == 1) { L[i] = inL + oL * wet; R[i] = inR + oR * wet; }
                    pos = (pos + 1) % delay;
                }
        }
    }

    // ================================================================================================================
    //  PLAYBACK: renders on a worker thread, cross-fades between tracks, keeps a tiny cache.
    // ================================================================================================================
    public class MusicHost : MonoBehaviour
    {
        class Job { public Track track; public Task<float[]> task; public int loopSamples; }
        AudioSource[] src = new AudioSource[2]; float[] startVol = new float[2]; int cur; Track? current, wanted; bool wantedLoop = true; float fade = 0.5f, fadeT = 1f; float volume = 0.5f;
        readonly Dictionary<Track, AudioClip> clips = new Dictionary<Track, AudioClip>(); readonly Dictionary<Track, Job> jobs = new Dictionary<Track, Job>();
        readonly List<Track> order = new List<Track>();

        void Awake()
        {
            for (int i = 0; i < 2; i++) { src[i] = gameObject.AddComponent<AudioSource>(); src[i].playOnAwake = false; src[i].volume = 0f; }
        }

        public void Prefetch(Track t) { if (!clips.ContainsKey(t) && !jobs.ContainsKey(t)) StartJob(t); }

        void StartJob(Track t)
        {
            var job = new Job { track = t };
            job.task = Task.Run(() => { int n; var d = MusicRenderer.Render(SongBook.Get(t), out n); job.loopSamples = n; return d; });
            jobs[t] = job;
        }

        public void Play(Track t, float fadeSeconds = 1.8f)
        {
            if (wanted.HasValue && wanted.Value == t) return;
            wanted = t; fade = fadeSeconds; wantedLoop = SongBook.Get(t).loop;
            if (!clips.ContainsKey(t) && !jobs.ContainsKey(t)) StartJob(t);
        }

        public void Stop(float fadeSeconds = 0.8f) { wanted = null; fade = fadeSeconds; fadeT = 0f; for (int i = 0; i < 2; i++) startVol[i] = src[i].volume; }

        public bool Ready(Track t) { return clips.ContainsKey(t); }
        public Track? Current { get { return current; } }

        void Update()
        {
            // finished renders become clips (on the main thread)
            List<Track> done = null;
            foreach (var kv in jobs) if (kv.Value.task.IsCompleted) { if (done == null) done = new List<Track>(); done.Add(kv.Key); }
            if (done != null)
                foreach (var t in done)
                {
                    var job = jobs[t]; jobs.Remove(t);
                    if (job.task.IsFaulted) { Debug.LogError("Music render failed for " + t + ": " + job.task.Exception); continue; }
                    var c = AudioClip.Create("music_" + t, job.loopSamples, 2, MusicRenderer.SR, false); c.SetData(job.task.Result, 0);
                    clips[t] = c; order.Add(t);
                }
            // trim the cache (never the playing or wanted ones)
            while (order.Count > 4)
            {
                Track victim = default(Track); bool found = false;
                foreach (var t in order) if (!(current.HasValue && current.Value == t) && !(wanted.HasValue && wanted.Value == t)) { victim = t; found = true; break; }
                if (!found) break;
                order.Remove(victim); Destroy(clips[victim]); clips.Remove(victim);
            }
            // start the wanted track once it is ready
            if (wanted.HasValue && (!current.HasValue || current.Value != wanted.Value) && clips.ContainsKey(wanted.Value))
            {
                for (int i = 0; i < 2; i++) startVol[i] = src[i].volume;
                int next = 1 - cur; var s = src[next]; s.clip = clips[wanted.Value]; s.loop = wantedLoop; s.timeSamples = 0; s.volume = 0f; s.Play();
                current = wanted; cur = next; fadeT = 0f;
            }
            // cross-fade
            // equal-power cross-fade: the new track rises on a quarter sine while the old one falls on a quarter cosine, so the mix never dips or jumps
            fadeT += Time.unscaledDeltaTime / Mathf.Max(0.05f, fade);
            float k = Mathf.Clamp01(fadeT);
            for (int i = 0; i < 2; i++)
            {
                bool active = (i == cur) && wanted.HasValue;
                src[i].volume = active ? volume * Mathf.Sin(k * Mathf.PI * 0.5f) : startVol[i] * Mathf.Cos(k * Mathf.PI * 0.5f);
                if (!active && k >= 1f && src[i].isPlaying) src[i].Stop();
            }
        }

        void OnDestroy() { foreach (var c in clips.Values) if (c != null) Destroy(c); }
    }
}
