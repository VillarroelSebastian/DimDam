using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    public enum PlatKind { Normal, Brittle, Elastic, Moving, Bridge }

    /// <summary>One walkable ledge. Non-solid ledges can be jumped up through and dropped through.</summary>
    public class Plat
    {
        public float x0, x1, top; public bool solid, ice; public PlatKind kind = PlatKind.Normal;
        // moving
        public float baseX0, baseTop, ampX, ampY, speed, phase;
        // runtime
        public bool active = true; public float dx, dy;           // delta applied this frame (riders are carried by it)
        public float shake, goneT, squash, bridgeT;                 // brittle / elastic / bridge animation state
        public float triggered = -1f;                               // brittle: seconds since someone stepped on it
        public int viewId = -1;

        public Plat(float a, float b, float t, bool s = false) { x0 = a; x1 = b; top = t; solid = s; baseX0 = a; baseTop = t; }
        public float Width { get { return x1 - x0; } }
        public float CenterX { get { return (x0 + x1) * 0.5f; } }
        public Plat Clone(float yOffset)
        {
            var p = (Plat)MemberwiseClone(); p.top += yOffset; p.baseTop += yOffset; return p;
        }

        public void Update(float dt, float time)
        {
            dx = dy = 0f;
            if (kind == PlatKind.Moving)
            {
                float w = x1 - x0;
                float nx = baseX0 + Mathf.Sin(time * speed + phase) * ampX;
                float nt = baseTop + Mathf.Sin(time * speed + phase) * ampY;
                dx = nx - x0; dy = nt - top;
                x0 = nx; x1 = nx + w; top = nt;
            }
            else if (kind == PlatKind.Brittle)
            {
                if (goneT > 0f) { goneT -= dt; if (goneT <= 0f) { active = true; shake = 0; triggered = -1f; } }
                else if (triggered >= 0f)
                {
                    triggered += dt; shake = triggered;
                    if (triggered >= 0.85f) { active = false; goneT = 4.2f; shake = 0; triggered = -1f; GameCtl.I.OnBrittleBreak(this); }
                }
            }
            else if (kind == PlatKind.Elastic) squash = Mathf.Max(0f, squash - dt * 5f);
            else if (kind == PlatKind.Bridge)
            {
                if (bridgeT > 0f) { bridgeT -= dt; if (bridgeT <= 0f) { active = false; } }
            }
        }

        public void Touch() { if (kind == PlatKind.Brittle && triggered < 0f && goneT <= 0f) triggered = 0f; }
    }

    public class EnemySpawn
    {
        public string kind, color; public float x, y;
        public EnemySpawn(string k, string c, float px, float py) { kind = k; color = c; x = px; y = py; }
    }

    public class CrystalDef { public float x, y; public int bridgeIndex; public CrystalDef(float px, float py, int b) { x = px; y = py; bridgeIndex = b; } }

    public class LevelDef
    {
        public string title, subtitle, hint; public int theme; public int world; public bool boss;
        public List<Plat> plats = new List<Plat>();
        public List<EnemySpawn> spawns = new List<EnemySpawn>();
        public List<CrystalDef> crystals = new List<CrystalDef>();
        public float[] playerX = { -2f, 2f };
        public float parTime = 60f;
        public bool wind, icicles;                       // blizzard gusts / falling icicles
        public int maxAlive = 4;                         // enemies on screen at once; the rest arrive in waves
        public bool ice { get { return world == 1; } }
    }

    /// <summary>The ten stages: five candy (world 0) and five ice (world 1). Local coordinates: y=0 is the bottom of the screen, ground top at y=1.</summary>
    public static class Levels
    {
        public const float Height = 9f;       // one screen = 16 x 9 units (640x360 px at 40 ppu)
        public const float Left = -7.5f, Right = 7.5f;
        public const int PerWorld = 5;
        static LevelDef[] all;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { all = null; }          // platforms carry state (broken ice, lifts): rebuild them every run
        public static LevelDef[] All { get { if (all == null) all = Build(); return all; } }
        public static readonly string[] WorldNames = { "REINO DE CHICLE", "REINO DE HIELO" };

        static Plat P(float a, float b, float t) { return new Plat(a, b, t); }
        static Plat Brittle(float a, float b, float t) { return new Plat(a, b, t) { kind = PlatKind.Brittle }; }
        static Plat Elastic(float a, float b, float t) { return new Plat(a, b, t) { kind = PlatKind.Elastic }; }
        static Plat Moving(float a, float b, float t, float ax, float ay, float speed, float phase = 0f) { return new Plat(a, b, t) { kind = PlatKind.Moving, ampX = ax, ampY = ay, speed = speed, phase = phase }; }
        static Plat Bridge(float a, float b, float t) { return new Plat(a, b, t) { kind = PlatKind.Bridge, active = false }; }
        static Plat Ground(float a, float b, bool ice) { return new Plat(a, b, 1f, true) { ice = ice }; }

        static LevelDef L(string title, string sub, int theme, int world, params Plat[] p)
        {
            var d = new LevelDef { title = title, subtitle = sub, theme = theme, world = world };
            d.plats.Add(Ground(-8f, 8f, false));
            d.plats.AddRange(p);
            return d;
        }

        static LevelDef LG(string title, string sub, int theme, Plat[] ground, params Plat[] p)
        {
            var d = new LevelDef { title = title, subtitle = sub, theme = theme, world = 1 };
            d.plats.AddRange(ground); d.plats.AddRange(p);
            return d;
        }

        static void S(LevelDef d, string kind, float x, float y, string color = "") { d.spawns.Add(new EnemySpawn(kind, color, x, y)); }

        static LevelDef[] Build()
        {
            var l = new LevelDef[10];

            // ============================================================ WORLD 1: CHICLE
            l[0] = L("PRADO PIRULETA", "ATRAPA A LOS OSITOS", 0, 0,
                P(-7.5f, -3f, 3.1f), P(3f, 7.5f, 3.1f), P(-4.5f, 4.5f, 5.2f), P(-7.5f, -2.5f, 7.3f), P(2.5f, 7.5f, 7.3f));
            S(l[0], "bear", -5f, 3.1f, "red"); S(l[0], "bear", 5f, 3.1f, "red"); S(l[0], "bean", -5f, 7.3f, "yellow"); S(l[0], "bean", 0f, 5.2f, "yellow");
            l[0].parTime = 55; l[0].hint = "DISPARA AL ENEMIGO PARA ATRAPARLO EN UNA BOLA. LUEGO PATEALA!";

            l[1] = L("CUEVA DE CARAMELO", "CUIDADO CON LOS SALTARINES", 1, 0,
                P(-6f, -1.5f, 3.1f), P(1.5f, 6f, 3.1f), P(-7.5f, -4f, 5.2f), P(-1.5f, 1.5f, 5.2f), P(4f, 7.5f, 5.2f), P(-5f, -1f, 7.3f), P(1f, 5f, 7.3f));
            S(l[1], "bear", 0f, 3.1f, "steel"); S(l[1], "bear", -4f, 3.1f, "green"); S(l[1], "bear", 4f, 3.1f, "green"); S(l[1], "bean", 0f, 5.2f, "purple"); 
            S(l[1], "lolli", 0f, 6.2f, "pink");
            l[1].parTime = 70; l[1].hint = "SALTA SOBRE LAS BURBUJAS ROSAS: ES UN PARRY Y CARGA TU SUPER";

            l[2] = L("NUBES DE MALVAVISCO", "LLUVIA DE CARAMELOS", 2, 0,
                P(-7.5f, -4.5f, 3.1f), P(-2f, 2f, 3.1f), P(4.5f, 7.5f, 3.1f), P(-5.5f, -1.5f, 5.2f), P(1.5f, 5.5f, 5.2f), P(-3f, 3f, 7.3f));
            S(l[2], "bear", -6f, 3.1f, "orange"); S(l[2], "bear", 6f, 3.1f, "orange"); S(l[2], "lolli", -3f, 6.5f, "pink"); 
            S(l[2], "cannon", 0f, 7.3f, "sugar"); S(l[2], "bear", 6f, 5.2f, "steel"); S(l[2], "lolli", 3f, 6.5f, "pink"); S(l[2], "bean", 0f, 3.1f, "yellow");
            l[2].parTime = 80;

            l[3] = L("FABRICA DE CHOCOLATE", "LOS CANONES ESTAN CALIENTES", 3, 0,
                P(-7.5f, -2f, 3.1f), P(2f, 7.5f, 3.1f), P(-6f, -1f, 5.2f), P(1f, 6f, 5.2f), P(-7.5f, -4f, 7.3f), P(-1.5f, 1.5f, 7.3f), P(4f, 7.5f, 7.3f));
            S(l[3], "cannon", -6.5f, 7.3f, "sugar"); S(l[3], "cannon", 6.5f, 7.3f, "sugar"); S(l[3], "bear", -4f, 3.1f, "red"); S(l[3], "bear", 4f, 3.1f, "green");
            S(l[3], "bean", 3f, 5.2f, "purple"); S(l[3], "bear", -6f, 3.1f, "steel"); S(l[3], "bear", 5f, 5.2f, "steel"); S(l[3], "lolli", 0f, 6.9f, "pink");
            l[3].parTime = 95;

            l[4] = L("LA PRINCESA CHICLE", "JEFE FINAL", 4, 0,
                P(-7.2f, -4f, 3.2f), P(-2f, 1.2f, 3.2f), P(-6f, -2.5f, 5.4f), P(-0.8f, 2.2f, 5.4f), P(-4f, -1f, 7.5f));
            l[4].boss = true; l[4].playerX = new[] { -5.5f, -3.5f }; l[4].parTime = 150;

            // ============================================================ WORLD 2: HIELO (reimagined from the original Reino de Hielo)
            // 1) Lago de los Susurros: ice and snow sectors on the floor, small brittle icebergs, asymmetric steps
            l[5] = LG("LAGO DE LOS SUSURROS", "EL HIELO RESBALA", 5,
                new[] { Ground(-8f, -3f, true), Ground(-3f, 1f, false), Ground(1f, 4.5f, true), Ground(4.5f, 8f, false) },
                P(-7.5f, -4.5f, 3.2f), Brittle(-2.5f, 0.5f, 3.2f), P(3f, 7.5f, 3.2f),
                Brittle(-6f, -3f, 5.4f), P(-0.5f, 3.5f, 5.4f),
                P(-7.5f, -3.5f, 7.5f), P(2f, 6f, 7.5f));
            S(l[5], "penguin", -6f, 3.2f); S(l[5], "penguin", 5f, 3.2f); S(l[5], "seed", -1f, 3.2f); S(l[5], "seed", 1.5f, 5.4f);
            S(l[5], "snowman", -5f, 7.5f); S(l[5], "snowman", 5f, 3.2f, "iron");
            l[5].parTime = 75; l[5].hint = "EL HIELO ES RESBALADIZO. LOS PINGUINOS BLOQUEAN POR DELANTE: DISPARALES POR DETRAS";

            // 2) Aldea de Iglus Torcidos: rooftops, a vertical elevator, a brittle alley
            l[6] = LG("ALDEA DE IGLUS TORCIDOS", "SUBE POR LOS TEJADOS", 6,
                new[] { Ground(-8f, 8f, false) },
                P(-7.5f, -4f, 3.1f), P(-1.5f, 1.5f, 3.1f), P(4f, 7.5f, 3.1f),
                Brittle(-7.5f, -4.5f, 5.3f), Brittle(4.5f, 7.5f, 5.3f), Moving(-1f, 1f, 5.0f, 0f, 1.7f, 1.1f),
                P(-5f, -1.8f, 7.5f), P(1.8f, 5f, 7.5f));
            S(l[6], "snowman", -5.5f, 3.1f); S(l[6], "penguin", 0f, 3.1f); S(l[6], "crab", 0f, 1f);
            S(l[6], "owl", -3f, 6.8f); S(l[6], "snowman", 5.5f, 3.1f, "iron"); S(l[6], "seed", 3f, 7.5f);
            l[6].parTime = 85; l[6].hint = "EL ASCENSOR SUBE Y BAJA. LAS TEJAS QUEBRADIZAS TIEMBLAN: NO TE QUEDES";

            // 3) Cristales Cantores: elastic crystal pads, two towers, a high bridge charged by the crystals
            l[7] = LG("CRISTALES CANTORES", "DISPARA A LOS CRISTALES", 7,
                new[] { Ground(-8f, 8f, true) },
                Elastic(-7f, -6f, 2.4f), Elastic(6f, 7f, 2.4f),
                P(-4.5f, -1.5f, 4.6f), P(1.5f, 4.5f, 4.6f),
                Bridge(-3f, 3f, 7.4f), P(-7.5f, -5.5f, 6.9f), P(5.5f, 7.5f, 6.9f));
            l[7].crystals.Add(new CrystalDef(-0.8f, 1f, 0)); l[7].crystals.Add(new CrystalDef(0.8f, 1f, 0));
            S(l[7], "jelly", -3f, 6.0f); S(l[7], "snowman", 0f, 3.1f, "iron"); S(l[7], "owl", 0f, 8.0f); S(l[7], "penguin", -3f, 4.6f); S(l[7], "seed", 3f, 4.6f); S(l[7], "crab", 5f, 1f);
            l[7].parTime = 90; l[7].icicles = true; l[7].hint = "DISPARA A LOS CRISTALES PARA CONSTRUIR EL PUENTE. LAS ESCARCHAS TE CONGELAN: MARTILLEA BOTONES";

            // 4) Jardines Congelados: brittle routes, a central shelter, a ferry; blizzard gusts
            l[8] = LG("JARDINES CONGELADOS", "REFUGIATE DE LA VENTISCA", 8,
                new[] { Ground(-8f, -2f, false), Ground(-2f, 2f, true), Ground(2f, 8f, false) },
                Brittle(-7f, -4f, 3.2f), P(4f, 7.5f, 3.2f), P(-2.5f, 2.5f, 3.5f),
                Brittle(-7.5f, -4.5f, 5.6f), P(4.5f, 7.5f, 5.6f), Moving(-1.5f, 1.5f, 7.4f, 3.2f, 0f, 0.9f));
            S(l[8], "penguin", -5f, 1f); S(l[8], "snowman", -5.5f, 3.2f, "iron"); S(l[8], "snowman", 3f, 3.2f, "iron"); S(l[8], "snowman", 5.5f, 3.2f); S(l[8], "owl", 0f, 6.8f); S(l[8], "jelly", -4f, 6.4f); S(l[8], "crab", 5f, 5.6f); S(l[8], "seed", 0f, 3.5f);
            l[8].parTime = 100; l[8].wind = true; l[8].icicles = true; l[8].hint = "VIENE UNA VENTISCA: PROTEGETE BAJO UN TECHO";

            // 5) Palacio de la Ultima Nevada: boss arena (stairs, galleries, a ferry, a high bridge that breaks)
            l[9] = LG("PALACIO DE LA ULTIMA NEVADA", "EL REY DE HIELO", 9,
                new[] { Ground(-8f, -4f, false), Ground(-4f, 2f, true), Ground(2f, 8f, false) },
                P(-7.2f, -4.2f, 3.2f), P(-2f, 1.2f, 3.2f), P(-6f, -3f, 5.4f),
                Moving(-3.5f, -0.5f, 5.4f, 1.8f, 0f, 0.8f), Brittle(-4.5f, -1.5f, 7.5f));
            l[9].boss = true; l[9].playerX = new[] { -5.8f, -3.8f }; l[9].parTime = 170; l[9].wind = true;
            l[3].maxAlive = 5; l[8].maxAlive = 5;
            return l;
        }
    }

    /// <summary>A level instance placed at a vertical offset (levels are stacked: the camera climbs).</summary>
    public class World
    {
        public LevelDef def; public float y0; public List<Plat> plats = new List<Plat>();
        public float GroundTop { get { return y0 + 1f; } }
        public float Left { get { return Levels.Left; } }
        public float Right { get { return Levels.Right; } }
        public Vector2 Center { get { return new Vector2(0, y0 + 4.5f); } }

        public World(LevelDef d, float y)
        {
            def = d; y0 = y;
            foreach (var p in d.plats) plats.Add(p.Clone(y));
        }

        public void Tick(float dt, float time) { foreach (var p in plats) p.Update(dt, time); }

        public bool OneWayUnder(float x, float top)
        {
            foreach (var p in plats) if (p.active && !p.solid && Mathf.Abs(p.top - top) < 0.05f && x >= p.x0 && x <= p.x1) return true;
            return false;
        }

        /// <summary>Top of the nearest ledge directly below (x, y) within maxDrop, or NaN.</summary>
        public float SupportBelow(float x, float y, float maxDrop)
        {
            float best = float.NaN;
            foreach (var p in plats)
            {
                if (!p.active || x < p.x0 || x > p.x1) continue;
                if (p.top <= y + 0.05f && p.top >= y - maxDrop && (float.IsNaN(best) || p.top > best)) best = p.top;
            }
            return best;
        }

        /// <summary>True when a (non-moving or moving) ledge roofs this spot within `reach` above: used by the blizzard shelters.</summary>
        public bool Sheltered(float x, float y, float reach)
        {
            foreach (var p in plats)
            {
                if (!p.active || p.solid) continue;
                if (x >= p.x0 && x <= p.x1 && p.top > y + 0.6f && p.top < y + reach) return true;
            }
            return false;
        }

        public Plat PlatAt(float x, float y)
        {
            Plat best = null;
            foreach (var p in plats) if (p.active && x >= p.x0 - 0.02f && x <= p.x1 + 0.02f && Mathf.Abs(p.top - y) < 0.12f && (best == null || p.top > best.top)) best = p;
            return best;
        }
    }

    /// <summary>Kinematic body: axis-separated movement, one-way ledges, solid ground, side walls. Carries riders of moving ledges.</summary>
    public class Body
    {
        public Vector2 pos, vel; public float w = 0.6f, h = 1f;
        public bool grounded, hitL, hitR, landedThisStep; public float dropUntil;
        public float groundTop; public bool groundIce; public Plat ride; public bool bounced;

        public Rect Rect { get { return new Rect(pos.x - w * 0.5f, pos.y, w, h); } }
        public Vector2 Center { get { return new Vector2(pos.x, pos.y + h * 0.5f); } }

        public void Step(float dt, World wd, float time)
        {
            hitL = hitR = false; landedThisStep = false; bounced = false;
            if (ride != null && grounded && ride.active) { pos.x += ride.dx; pos.y += ride.dy; }
            pos.x += vel.x * dt;
            float minX = wd.Left + w * 0.5f, maxX = wd.Right - w * 0.5f;
            if (pos.x < minX) { pos.x = minX; hitL = true; }
            if (pos.x > maxX) { pos.x = maxX; hitR = true; }
            float prev = pos.y - (ride != null && grounded ? 0f : 0f);
            float preVy = vel.y;
            pos.y += vel.y * dt;
            bool wasGrounded = grounded; grounded = false; Plat landed = null;
            if (vel.y <= 0.0001f)
            {
                float best = float.NaN;
                foreach (var p in wd.plats)
                {
                    if (!p.active) continue;
                    if (pos.x + w * 0.35f < p.x0 || pos.x - w * 0.35f > p.x1) continue;
                    if (!p.solid && time < dropUntil) continue;
                    // a ledge that rose into us this frame (elevator) still counts as support
                    if (prev + 0.05f >= p.top - p.dy - 0.03f && pos.y <= p.top && (float.IsNaN(best) || p.top > best)) { best = p.top; landed = p; }
                }
                if (!float.IsNaN(best))
                {
                    pos.y = best; vel.y = 0f; grounded = true; groundTop = best; landedThisStep = !wasGrounded; ride = landed; groundIce = landed.ice;
                    if (landed.kind == PlatKind.Brittle) landed.Touch();
                    if (landed.kind == PlatKind.Elastic && (preVy < -4f || !wasGrounded && preVy < -2f))
                    {
                        vel.y = Mathf.Clamp(-preVy * 0.9f + 13f, 17f, 24f); grounded = false; landed.squash = 1f; bounced = true; ride = null;
                        GameCtl.I.OnElasticBounce(landed, pos);
                    }
                }
            }
            if (!grounded) { ride = null; groundIce = false; }
        }
    }
}
