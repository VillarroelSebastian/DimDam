using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GumRemix
{
    public enum Mode { Title, Help, Banner, Play, Clear, Lift, KO, WorldDone, Results, GameOver }

    public class GameCtl : MonoBehaviour
    {
        public static GameCtl I;
        public const int RW = 640, RH = 360;
        static bool seenHelp;

        public Transform entRoot, hudRoot;
        public World world; public Player[] players = new Player[2];
        public List<Enemy> enemies = new List<Enemy>(); public List<PlayerShot> shots = new List<PlayerShot>();
        public List<EnemyShot> eshots = new List<EnemyShot>(); public List<Shockwave> waves = new List<Shockwave>();
        public List<SuperBeam> beams = new List<SuperBeam>(); public List<Fx> fxs = new List<Fx>(); public List<Particle> parts = new List<Particle>();
        public List<Crystal> crystals = new List<Crystal>(); public List<Icicle> icicles = new List<Icicle>(); public List<IcePillar> pillars = new List<IcePillar>();
        public WindCtl wind = new WindCtl();
        public BossBase boss;
        public float dt, time; public Mode mode; public int levelIndex; public int playerCount = 1;
        public Sprite white;

        Camera cam, displayCam; RenderTexture rt;
        float camY, hitStop, shakeAmp, shakeT, modeT; bool paused;
        GameObject envCur; World nextWorld; GameObject nextEnv;
        float liftFromY, liftToY; Vector2[] liftStart = new Vector2[2], liftEnd = new Vector2[2]; SpriteRenderer[] liftBubbles = new SpriteRenderer[2]; Anim[] liftAnims = new Anim[2];
        readonly List<KeyValuePair<float, EnemySpawn>> pending = new List<KeyValuePair<float, EnemySpawn>>();
        PText banner, banner2; float bannerLife, bannerElapsed; int bannerScale; Mode afterBanner;
        float levelTime, totalTime, icicleT, hintT, hintLife; int kills; int lastBossPhase = 1;
        // title / help
        int menuRow, menuWorld; PText[] titleTexts; GameObject titleRoot; SpriteRenderer[] titleHeroes = new SpriteRenderer[2]; Anim[] titleAnims = new Anim[2];
        GameObject helpRoot; int helpPage; int pendingStart;
        PText hintText; SpriteRenderer hintPanel; GameObject pauseRoot;
        // hud
        SpriteRenderer[][] hearts = new SpriteRenderer[2][]; SpriteRenderer[][] cardsBase = new SpriteRenderer[2][]; SpriteRenderer[][] cardsFill = new SpriteRenderer[2][];
        PText[] pLabels = new PText[2]; SpriteRenderer bossBarBack, bossBarFill; PText bossName; SpriteRenderer[] bossTicks = new SpriteRenderer[2];
        GameObject resultsRoot; float gameOverT; bool bossDownFlag;
        public bool inputLocked, god, holdClear; public int statStuck, statKicks, statBallKills, statParries;
        public bool solo = true, autoFire = true; public int statCoopBounces, statHugs, statWraps; bool armorHinted; BossBase lazoBoss; bool[] lazoDone = new bool[4]; float hugT, hugCd, spawnCd; Bond bond;
        public static void ResetSession() { seenHelp = false; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { I = null; seenHelp = false; }

        void Awake()
        {
            I = this; Art.Init(); Sfx.Init(gameObject);
            var e = new GameObject("entities"); e.transform.SetParent(transform, false); entRoot = e.transform;
            var h = new GameObject("hud"); h.transform.SetParent(transform, false); hudRoot = h.transform;
            SetupCameras();
            var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false); tex.SetPixel(0, 0, Color.white); tex.Apply(); tex.filterMode = FilterMode.Point;
            white = Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0, 0.5f), 40f);
            players[0] = new Player(0, "celeste"); players[1] = new Player(1, "coral");
            BuildHud(); BuildHint(); bond = new Bond(entRoot);
            EnterTitle();
        }

        void SetupCameras()
        {
            var c = new GameObject("GumCamera"); c.transform.SetParent(transform, false);
            cam = c.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = RH / 80f; cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = Color.black;
            cam.nearClipPlane = 0.1f; cam.farClipPlane = 50f; cam.depth = 0; cam.allowHDR = false; cam.allowMSAA = false;
            rt = new RenderTexture(RW, RH, 24, RenderTextureFormat.ARGB32) { filterMode = FilterMode.Point, antiAliasing = 1 };
            rt.Create(); cam.targetTexture = rt;
            displayCam = Camera.main;
            if (displayCam == null || displayCam == cam)
            {
                var d = new GameObject("GumDisplay"); d.transform.SetParent(transform, false);
                displayCam = d.AddComponent<Camera>();
            }
            if (displayCam.GetComponent<AudioListener>() == null) displayCam.gameObject.AddComponent<AudioListener>();
            foreach (var l in FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) l.enabled = l.gameObject == displayCam.gameObject;
            displayCam.clearFlags = CameraClearFlags.SolidColor; displayCam.backgroundColor = Color.black; displayCam.cullingMask = 0; displayCam.depth = -10;
            displayCam.orthographic = true;
        }

        void OnGUI()
        {
            if (Event.current.type != EventType.Repaint || rt == null) return;
            float sx = Screen.width / (float)RW, sy = Screen.height / (float)RH;
            float s = Mathf.Min(sx, sy); if (s >= 2f) s = Mathf.Floor(s);
            float w = RW * s, h = RH * s;
            GUI.DrawTexture(new Rect((Screen.width - w) * 0.5f, (Screen.height - h) * 0.5f, w, h), rt, ScaleMode.StretchToFill, false);
        }

        void OnDestroy() { if (rt != null) rt.Release(); if (I == this) I = null; }

        // ------------------------------------------------------------------------------------------ queries used by entities
        public Player NearestPlayer(Vector2 p)
        {
            Player best = null; float bd = float.MaxValue;
            foreach (var pl in players) if (pl != null && pl.Alive) { float d = (pl.Center - p).sqrMagnitude; if (d < bd) { bd = d; best = pl; } }
            return best;
        }
        public Player RandomAlivePlayer()
        {
            var l = new List<Player>(); foreach (var pl in players) if (pl != null && pl.Alive) l.Add(pl);
            return l.Count == 0 ? null : l[Random.Range(0, l.Count)];
        }
        public bool PlayersAllDown() { foreach (var pl in players) if (pl != null && pl.joined && pl.Alive) return false; return true; }
        public float AveragePlayerX() { float s = 0; int n = 0; foreach (var pl in players) if (pl != null && pl.joined) { s += pl.body.pos.x; n++; } return n == 0 ? 0 : s / n; }
        public int AliveEnemies() { int n = 0; foreach (var e in enemies) if (e.state != EState.Dead) n++; return n + pending.Count; }

        public Fx SpawnFx(string sheet, string clip, Vector2 pos, float scale = 1f, bool flip = false, int order = 30)
        {
            var f = Fx.Make(entRoot, sheet, clip, pos, scale, flip, order); fxs.Add(f); return f;
        }

        public void Burst(Vector2 pos, int n, float speed)
        {
            for (int i = 0; i < n; i++)
            {
                float a = Random.value * 6.283f; float sp = Random.Range(0.4f, 1f) * speed;
                parts.Add(Particle.Make(entRoot, pos, new Vector2(Mathf.Cos(a), Mathf.Sin(a) + .6f) * sp, 14f, Random.Range(0.6f, 1.1f), Random.Range(0, 6)));
            }
        }

        public void HitStop(float s) { hitStop = Mathf.Max(hitStop, s); }
        public void Shake(float amp, float dur) { shakeAmp = Mathf.Max(shakeAmp, amp); shakeT = Mathf.Max(shakeT, dur); }

        public Enemy SpawnEnemy(string kind, string color, Vector2 pos) { var e = new Enemy(kind, color, pos); enemies.Add(e); return e; }

        public void ClearHazards()
        {
            foreach (var s in eshots) s.Pop(); eshots.Clear();
            foreach (var w in waves) { w.dead = true; }
            foreach (var p in pillars) p.Dispose(); pillars.Clear();
            foreach (var i in icicles) i.Dispose(); icicles.Clear();
        }

        public void ThrowHat(Vector2 pos)
        {
            var f = SpawnFx("boss_hat", "spin", pos, 2f, false, 25);
            f.vel = new Vector2(-3.5f, 11f); f.life = 2.4f; f.grav = 22f;
        }

        // ---- ice kingdom events
        public void OnBrittleBreak(Plat p)
        {
            SpawnFx("icicle_shatter", "shatter", new Vector2(p.CenterX, p.top - 0.2f), 1.3f, false, 26);
            Sfx.Play("pop", .5f, 1.4f); Shake(.04f, .12f);
        }
        public void OnElasticBounce(Plat p, Vector2 at)
        {
            Sfx.Play("jump", .8f, 1.7f); SpawnFx("spark_ice", "spark", at + new Vector2(0, 0.3f), 1.6f, false, 31); Burst(at, 5, 3f);
        }
        Plat BridgeAt(int idx) { int k = 0; foreach (var p in world.plats) if (p.kind == PlatKind.Bridge) { if (k == idx) return p; k++; } return null; }
        public void ActivateBridge(int idx, float t)
        {
            var b = BridgeAt(idx); if (b == null) return;
            b.active = true; b.bridgeT = t; SpawnFx("spark_ice", "spark", new Vector2(b.CenterX, b.top), 2.4f, false, 31); Sfx.Play("lift", .6f, 1.6f);
        }
        public void ExtendBridge(int idx, float t) { var b = BridgeAt(idx); if (b != null && b.active) b.bridgeT = Mathf.Max(b.bridgeT, t); }
        public void TriggerGust(int dir) { wind.dir = dir; wind.state = 1; wind.timer = 1.0f; ShowHint(dir > 0 ? "VENTISCA >>>" : "<<< VENTISCA", 2f); Sfx.Play("warn", .6f, .6f); }

        public void OnEnemyDefeated(Enemy e) { kills++; }
        public void OnPlayerDown(Player p) { if (PlayersAllDown() && mode == Mode.Play) { gameOverT = 1.4f; } }
        public void OnBossDown() { bossDownFlag = true; }
        public void OnBossDead() { mode = Mode.KO; modeT = 0f; ShowBannerRaw("K.O.!", "", 99f, 8, new Color(1f, .93f, .4f)); Sfx.Play("ko"); Sfx.StopMusic(0.4f); }

        // ------------------------------------------------------------------------------------------ HUD
        void BuildHud()
        {
            var hs = Art.Get("hearts"); var cs = Art.Get("cards");
            for (int p = 0; p < 2; p++)
            {
                float bx = p == 0 ? -7.3f : 2.9f; float by = -4.05f;
                pLabels[p] = PText.Create(hudRoot, new Vector3(bx, by + 0.52f, 0), p == 0 ? "P1 CELESTE" : "P2 CORAL", 1, p == 0 ? new Color(.7f, .9f, 1f) : new Color(1f, .75f, .72f), false, 200);
                hearts[p] = new SpriteRenderer[4]; cardsBase[p] = new SpriteRenderer[5]; cardsFill[p] = new SpriteRenderer[5];
                for (int i = 0; i < 4; i++) hearts[p][i] = Pix.Make("heart", hudRoot, hs[0], new Vector3(bx + 0.3f + i * 0.52f, by + 0.05f, 0), 200);
                for (int i = 0; i < 5; i++)
                {
                    cardsBase[p][i] = Pix.Make("cardb", hudRoot, cs[0], new Vector3(bx + 2.65f + i * 0.44f, by + 0.05f, 0), 200);
                    cardsFill[p][i] = Pix.Make("cardf", hudRoot, cs[1], new Vector3(bx + 2.65f + i * 0.44f, by + 0.05f, 0), 201);
                }
            }
            bossBarBack = Pix.Make("bossbar", hudRoot, white, new Vector3(-4.1f, 4.0f, 0), 200); bossBarBack.transform.localScale = new Vector3(332, 10, 1); bossBarBack.color = new Color(.16f, .04f, .2f);
            bossBarFill = Pix.Make("bossfill", hudRoot, white, new Vector3(-4.0f, 4.0f, 0), 201); bossBarFill.color = new Color(1f, .45f, .7f);
            for (int i = 0; i < 2; i++) { bossTicks[i] = Pix.Make("tick", hudRoot, white, Vector3.zero, 202); bossTicks[i].transform.localScale = new Vector3(2, 8, 1); bossTicks[i].color = new Color(.16f, .04f, .2f); }
            bossName = PText.Create(hudRoot, new Vector3(0, 3.55f, 0), "EL GRAN CHICLON", 2, new Color(1f, .85f, .95f), true, 200);
            SetHudVisible(false);
        }

        void SetHudVisible(bool v, bool bossBar = false)
        {
            for (int p = 0; p < 2; p++)
            {
                bool on = v && (p == 0 || players[p].joined);
                pLabels[p].gameObject.SetActive(on);
                foreach (var h in hearts[p]) h.gameObject.SetActive(on);
                foreach (var c in cardsBase[p]) c.gameObject.SetActive(on);
                foreach (var c in cardsFill[p]) c.gameObject.SetActive(on);
            }
            bool b = v && bossBar;
            bossBarBack.gameObject.SetActive(b); bossBarFill.gameObject.SetActive(b); bossName.gameObject.SetActive(b);
            foreach (var t in bossTicks) t.gameObject.SetActive(b);
            if (b && boss != null)
            {
                bossName.Set(boss.DisplayName);
                bossBarFill.color = boss is IceBoss ? new Color(.55f, .85f, 1f) : new Color(1f, .45f, .7f);
            }
        }

        void UpdateHud()
        {
            var hs = Art.Get("hearts"); var cs = Art.Get("cards");
            for (int p = 0; p < 2; p++)
            {
                var pl = players[p]; if (!pl.joined) continue;
                for (int i = 0; i < 4; i++) hearts[p][i].sprite = i < pl.hp ? hs[((int)(time * 3f) % 2 == 0 || pl.hp > 1) ? 0 : 1] : hs[2];
                for (int i = 0; i < 5; i++)
                {
                    float fill = Mathf.Clamp01(pl.meter - i);
                    bool full = pl.meter >= 5f;
                    cardsFill[p][i].sprite = full ? cs[((int)(time * 6f)) % 2 == 0 ? 2 : 1] : cs[1];
                    cardsFill[p][i].color = new Color(1, 1, 1, full ? 1f : (fill >= 1f ? 1f : fill * 0.7f));
                    cardsFill[p][i].enabled = fill > 0.02f;
                }
                // a frozen player gets a visible cue on their label
                pLabels[p].color = pl.frozenT > 0f ? new Color(.6f, .9f, 1f) : (p == 0 ? new Color(.7f, .9f, 1f) : new Color(1f, .75f, .72f));
            }
            if (boss != null)
            {
                float frac = Mathf.Clamp01(boss.hp / (float)boss.TotalHp);
                bossBarFill.transform.localScale = new Vector3(Mathf.Round(frac * 328f), 6, 1);
                var floors = boss.PhaseFloors;
                for (int i = 0; i < 2 && i < floors.Length; i++)
                    bossTicks[i].transform.localPosition = new Vector3(-4.0f + 328f / 40f * floors[i] / boss.TotalHp, 4.0f, 0);
            }
        }

        // ------------------------------------------------------------------------------------------ hints & banners
        void BuildHint()
        {
            hintPanel = Pix.Make("hintpanel", hudRoot, white, new Vector3(-8f, 3.85f, 0), 230); hintPanel.transform.localScale = new Vector3(640, 24, 1); hintPanel.color = new Color(.1f, .04f, .22f, .72f);
            hintText = PText.Create(hudRoot, new Vector3(0, 3.7f, 0), "", 1, new Color(1f, .93f, .6f), true, 232);
            hintPanel.gameObject.SetActive(false); hintText.gameObject.SetActive(false);
        }

        public void ShowHint(string s, float life = 6f)
        {
            if (string.IsNullOrEmpty(s)) return;
            hintText.Set(s); hintT = 0f; hintLife = life;
            hintPanel.gameObject.SetActive(true); hintText.gameObject.SetActive(true);
            hintPanel.transform.localPosition = new Vector3(-8f, boss != null ? 3.15f : 3.9f, 0); hintText.transform.localPosition = new Vector3(0, boss != null ? 3.0f : 3.75f, 0);
        }

        void TickHint(float raw)
        {
            if (!hintText.gameObject.activeSelf) return;
            hintT += raw;
            float a = Mathf.Clamp01(Mathf.Min(hintT / 0.25f, (hintLife - hintT) / 0.4f));
            hintPanel.color = new Color(.1f, .04f, .22f, .72f * a); hintText.color = new Color(1f, .93f, .6f, a);
            if (hintT >= hintLife) { hintPanel.gameObject.SetActive(false); hintText.gameObject.SetActive(false); }
        }

        void ShowBanner(string a, string b, float life, Mode next, int scale = 4)
        {
            afterBanner = next; mode = Mode.Banner; modeT = 0f;
            ShowBannerRaw(a, b, life, scale, Color.white);
        }

        void ShowBannerRaw(string a, string b, float life, int scale, Color col)
        {
            if (banner != null) Destroy(banner.gameObject); if (banner2 != null) Destroy(banner2.gameObject);
            banner = PText.Create(hudRoot, new Vector3(0, 0.6f, 0), a, scale, col, true, 300); bannerScale = scale;
            banner2 = string.IsNullOrEmpty(b) ? null : PText.Create(hudRoot, new Vector3(0, -0.5f, 0), b, 2, new Color(1f, .85f, .95f), true, 300);
            bannerLife = life; bannerElapsed = 0f;
        }

        void HideBanner() { if (banner != null) Destroy(banner.gameObject); if (banner2 != null) Destroy(banner2.gameObject); banner = banner2 = null; }

        void TickBanner(float raw)
        {
            if (banner == null) return;
            bannerElapsed += raw;
            float k = Mathf.Clamp01(bannerElapsed / 0.22f);
            banner.scale = Mathf.Max(bannerScale, Mathf.RoundToInt(Mathf.Lerp(bannerScale * 2.2f, bannerScale, k)));
            float shakeY = bannerElapsed < 0.4f ? Mathf.Round(Mathf.Sin(bannerElapsed * 60f) * 2f) / 40f : 0f;
            banner.transform.localPosition = new Vector3(0, 0.6f + shakeY, 0);
            if (bannerElapsed > bannerLife - 0.25f) { float a = Mathf.Clamp01((bannerLife - bannerElapsed) / 0.25f); banner.color = new Color(banner.color.r, banner.color.g, banner.color.b, a); if (banner2 != null) banner2.color = new Color(1f, .85f, .95f, a); }
        }

        string LevelBannerTitle(int idx)
        {
            var d = Levels.All[idx];
            return d.boss ? "READY? WALLOP!" : "NIVEL " + (d.world + 1) + "-" + (idx % Levels.PerWorld + 1);
        }

        // ------------------------------------------------------------------------------------------ title menu
        void EnterTitle()
        {
            ClearAll(); HideBanner(); paused = false; SetPause(false); Sfx.PlayTrack(Track.Title); Sfx.Prefetch((Track)(menuWorld * Levels.PerWorld));
            levelIndex = menuWorld * Levels.PerWorld; world = new World(Levels.All[levelIndex], 0f); envCur = EnvBuilder.Build(world); camY = 4.5f;
            titleRoot = new GameObject("title"); titleRoot.transform.SetParent(hudRoot, false);
            titleTexts = new PText[8];
            titleTexts[0] = PText.Create(titleRoot.transform, new Vector3(0, 2.35f, 0), "CHICLE", 8, new Color(1f, .62f, .85f), true, 250);
            titleTexts[1] = PText.Create(titleRoot.transform, new Vector3(0, 0.75f, 0), "RUMBLE", 8, new Color(.55f, .85f, 1f), true, 250);
            for (int r = 0; r < 5; r++) titleTexts[2 + r] = PText.Create(titleRoot.transform, new Vector3(0, -0.4f - r * 0.46f, 0), "", 2, Color.white, true, 250);
            titleTexts[7] = PText.Create(titleRoot.transform, new Vector3(0, -3.25f, 0), "W S ELEGIR   A D CAMBIAR   ENTER ACEPTAR   JUNTOS ES MAS DIVERTIDO", 1, new Color(1f, .85f, .5f), true, 250);
            var panel = Pix.Make("panel", titleRoot.transform, white, new Vector3(-8f, -1.3f, 0), 245); panel.transform.localScale = new Vector3(640, 150, 1); panel.color = new Color(.16f, .04f, .22f, .66f);
            var b0 = titleTexts[0].gameObject.AddComponent<Bobber>(); b0.amp = 0.08f; b0.freq = 2f;
            var b1 = titleTexts[1].gameObject.AddComponent<Bobber>(); b1.amp = 0.08f; b1.freq = 2f; b1.phase = 1.5f;
            string[] hn = { "celeste", "coral" };
            for (int i = 0; i < 2; i++)
            {
                var sh = Art.Get("hero_" + hn[i] + (menuWorld == 1 ? "_ice" : ""));
                titleHeroes[i] = Pix.Make("th" + i, titleRoot.transform, sh[0], new Vector3(i == 0 ? -5.5f : 5.5f, -1.2f, 0), 240, 2f);
                titleHeroes[i].flipX = i == 1; titleAnims[i] = new Anim(titleHeroes[i], sh, "cheer");
            }
            SetHudVisible(false); menuRow = 0; mode = Mode.Title; modeT = 0;
            foreach (var p in players) { p.joined = false; p.ghost = false; p.SetVisible(false); }
            RefreshMenu();
        }

        void RefreshMenu()
        {
            var on = new Color(1f, .93f, .4f); var off = new Color(1, 1, 1, .62f);
            string[] rows = { "SOLO: CONTROLAS A LOS DOS", "2 JUGADORES", "DISPARO: < " + (autoFire ? "AUTOMATICO" : "MANUAL") + " >", "MUNDO: < " + (menuWorld == 0 ? "CHICLE" : "HIELO") + " >", "COMO JUGAR" };
            for (int i = 0; i < 5; i++) { titleTexts[2 + i].Set((menuRow == i ? "> " : "  ") + rows[i]); titleTexts[2 + i].color = menuRow == i ? on : off; }
        }

        void SwitchWorldMenu(int w)
        {
            if (w == menuWorld) return;
            menuWorld = w; Sfx.Play("select", .5f);
            // rebuild the backdrop and heroes so the menu shows the chosen kingdom
            int keep = menuRow; EnterTitle(); menuRow = keep; RefreshMenu();
        }

        void TickTitle(float raw)
        {
            for (int i = 0; i < 2; i++) titleAnims[i].Tick(raw);
            bool up = GumInput.Key(k => k.wKey.wasPressedThisFrame || k.upArrowKey.wasPressedThisFrame);
            bool down = GumInput.Key(k => k.sKey.wasPressedThisFrame || k.downArrowKey.wasPressedThisFrame);
            bool left = GumInput.Key(k => k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame);
            bool right = GumInput.Key(k => k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame);
            if (GumInput.Key(k => k.digit1Key.wasPressedThisFrame)) { menuRow = 0; RefreshMenu(); }
            if (GumInput.Key(k => k.digit2Key.wasPressedThisFrame)) { menuRow = 1; RefreshMenu(); }
            if (up) { menuRow = (menuRow + 4) % 5; Sfx.Play("select", .5f); RefreshMenu(); }
            if (down) { menuRow = (menuRow + 1) % 5; Sfx.Play("select", .5f); RefreshMenu(); }
            if ((left || right) && menuRow == 2) { autoFire = !autoFire; Sfx.Play("select", .5f); RefreshMenu(); }
            if ((left || right) && menuRow == 3) { SwitchWorldMenu(1 - menuWorld); return; }
            if (modeT > 0.3f && GumInput.Confirm())
            {
                Sfx.Play("select");
                if (menuRow == 2) { autoFire = !autoFire; RefreshMenu(); return; }
                if (menuRow == 3) { SwitchWorldMenu(1 - menuWorld); return; }
                if (menuRow == 4) { pendingStart = 0; OpenHelp(0); return; }
                int count = menuRow == 0 ? 1 : 2;
                if (!seenHelp) { pendingStart = count; OpenHelp(0); return; }
                StartGame(count);
            }
        }

        // ------------------------------------------------------------------------------------------ help screens
        void OpenHelp(int page)
        {
            seenHelp = true; mode = Mode.Help; modeT = 0f; helpPage = page;
            if (titleRoot != null) titleRoot.SetActive(false);
            BuildHelp();
        }

        void CloseHelp()
        {
            if (helpRoot != null) Destroy(helpRoot); helpRoot = null;
            if (pendingStart > 0) { int c = pendingStart; pendingStart = 0; StartGame(c); return; }
            if (titleRoot != null) titleRoot.SetActive(true);
            mode = Mode.Title; modeT = 0.2f;
        }

        void Icon(Transform parent, string sheet, int frame, Vector3 pos, float scale = 1f, string clip = null)
        {
            var sh = Art.Get(sheet); Sprite s = frame >= 0 ? sh[frame] : sh.clips[clip].frames[0];
            Pix.Make("icon", parent, s, pos, 262, scale);
        }

        void Line(Transform parent, float y, string text, string sheet, int frame, string clip = null, float iconScale = 1f)
        {
            if (!string.IsNullOrEmpty(sheet)) Icon(parent, sheet, frame, new Vector3(-7.2f, y + 0.2f, 0), iconScale, clip);
            PText.Create(parent, new Vector3(-6.3f, y, 0), text, 2, Color.white, false, 265);
        }

        void BuildHelp()
        {
            if (helpRoot != null) Destroy(helpRoot);
            helpRoot = new GameObject("help"); helpRoot.transform.SetParent(hudRoot, false);
            var t = helpRoot.transform;
            var bg = Pix.Make("bg", t, white, new Vector3(-8f, 0f, 0), 255); bg.transform.localScale = new Vector3(640, 360, 1); bg.color = new Color(.09f, .04f, .22f, .94f);
            string[] titles = { "CONTROLES", "COMO SE JUEGA", "JUNTOS ES MEJOR", "REINO DE HIELO" };
            PText.Create(t, new Vector3(0, 3.4f, 0), titles[helpPage], 4, new Color(1f, .93f, .4f), true, 265);
            if (helpPage == 0)
            {
                PText.Create(t, new Vector3(-7.4f, 2.5f, 0), "P1 CELESTE", 2, new Color(.7f, .9f, 1f), false, 265);
                PText.Create(t, new Vector3(0.4f, 2.5f, 0), "P2 CORAL", 2, new Color(1f, .75f, .72f), false, 265);
                string[] a = { "MOVER      A D", "SALTAR     ESPACIO", "DISPARAR   F", "DASH       G", "SUPER      H", "APUNTAR    W", "BAJAR      S+SALTO" };
                string[] b = { "MOVER      FLECHAS", "SALTAR     SHIFT DER", "DISPARAR   CTRL DER", "DASH       .", "SUPER      /", "APUNTAR    FLECHA ARR", "BAJAR      ABAJO+SALTO" };
                for (int i = 0; i < a.Length; i++)
                {
                    PText.Create(t, new Vector3(-7.4f, 1.8f - i * 0.52f, 0), a[i], 2, Color.white, false, 265);
                    PText.Create(t, new Vector3(0.4f, 1.8f - i * 0.52f, 0), b[i], 2, Color.white, false, 265);
                }
                PText.Create(t, new Vector3(0, -2.2f, 0), "MANDO: STICK MUEVE  A SALTA  X DISPARA  B DASH  Y SUPER", 1, new Color(.85f, .95f, 1f), true, 265);
                PText.Create(t, new Vector3(0, -2.6f, 0), "SOLO? UNA MANO CADA HEROE: EL DISPARO ES AUTOMATICO Y APUNTA SOLO", 1, new Color(1f, .85f, .5f), true, 265);
            }
            else if (helpPage == 2)
            {
                float y = 2.3f, d = 0.72f;
                Line(t, y, "VINCULO: EL CHICLE ENTRE LOS DOS DAñO", "cards", 1); y -= d;
                Line(t, y, "PONTE A CADA LADO DE UN ENEMIGO O DEL JEFE", "ball_bear_red", -1, "shake"); y -= d;
                Line(t, y, "CAE SOBRE TU COMPANERO Y REBOTAS ALTO", "hero_celeste", 18); y -= d;
                Line(t, y, "JUNTOS CERCA UN RATO: ABRAZO, +1 CORAZON", "hearts", 0); y -= d;
                Line(t, y, "ACORAZADOS: EL DISPARO REBOTA, USA EL LAZO", "bear_steel", -1, "idle", 0.5f); y -= d;
                Line(t, y, "SI UNO CAE, EL OTRO LO REVIVE TOCANDOLO", "ghost_coral", -1, "float"); y -= d;
                PText.Create(t, new Vector3(0, -2.9f, 0), "JEFES: EN SU GUARDIA SOLO EL LAZO LOS DANA. RODEENLOS", 1, new Color(1f, .85f, .5f), true, 265);
            }
            else if (helpPage == 1)
            {
                float y = 2.3f, d = 0.72f;
                Line(t, y, "DISPARA: EL ENEMIGO QUEDA EN UNA BOLA", "ball_bear_red", -1, "wobble"); y -= d;
                Line(t, y, "TOCA LA BOLA PARA PATEARLA Y ELIMINAR", "ball_bear_red", -1, "roll"); y -= d;
                Line(t, y, "SALTA SOBRE LO ROSA: PARRY Y CARTA", "bubbles", -1, "pink"); y -= d;
                Line(t, y, "5 CARTAS = SUPER, UN RAYO ENORME", "cards", 2); y -= d;
                Line(t, y, "LIMPIA EL PISO Y UNA BURBUJA TE SUBE", "bubble_lift", -1, "lift", 0.5f); y -= d;
                Line(t, y, "SI CAES, TU AMIGO TE REVIVE AL TOCARTE", "ghost_celeste", -1, "float"); y -= d;
                PText.Create(t, new Vector3(0, -2.9f, 0), "JEFE: SOLO RECIBE DANO EN SU PUNTO DEBIL O CON BOLAS", 1, new Color(1f, .85f, .5f), true, 265);
            }
            else
            {
                float y = 2.3f, d = 0.72f;
                Line(t, y, "EL HIELO RESBALA, LA NIEVE FRENA", "ground_5", 0, null, 0.6f); y -= d;
                Line(t, y, "LA ESCARCHA CONGELA: MARTILLEA BOTONES", "freeze", -1, "freeze", 0.38f); y -= d;
                Line(t, y, "DISPARA A LOS CRISTALES: SALE UN PUENTE", "crystal_on", -1, "loop", 0.42f); y -= d;
                Line(t, y, "TEJAS QUEBRADIZAS Y CAMAS ELASTICAS", "special_5", 3); y -= d;
                Line(t, y, "VENTISCA: BUSCA UN TECHO PARA RESGUARDARTE", "gust", -1, "loop"); y -= d;
                Line(t, y, "EL PINGUINO BLOQUEA: DISPARALE POR DETRAS", "penguin", -1, "idle", 0.42f); y -= d;
                PText.Create(t, new Vector3(0, -2.9f, 0), "TRAS CADA ATAQUE SU CORAZA BRILLA: DISPARA AL SOL", 1, new Color(1f, .85f, .5f), true, 265);
            }
            PText.Create(t, new Vector3(0, -3.6f, 0), "A D PAGINA " + (helpPage + 1) + "/4     ENTER SIGUIENTE     ESC VOLVER", 1, new Color(.9f, .9f, 1f), true, 265);
        }

        void TickHelp()
        {
            bool left = GumInput.Key(k => k.aKey.wasPressedThisFrame || k.leftArrowKey.wasPressedThisFrame);
            bool right = GumInput.Key(k => k.dKey.wasPressedThisFrame || k.rightArrowKey.wasPressedThisFrame);
            bool esc = GumInput.Key(k => k.escapeKey.wasPressedThisFrame);
            if (esc) { Sfx.Play("select", .5f); CloseHelp(); return; }
            if (left && helpPage > 0) { helpPage--; Sfx.Play("select", .4f); BuildHelp(); return; }
            if (right && helpPage < 3) { helpPage++; Sfx.Play("select", .4f); BuildHelp(); return; }
            if (modeT > 0.25f && GumInput.Confirm())
            {
                Sfx.Play("select");
                if (helpPage < 3) { helpPage++; BuildHelp(); } else CloseHelp();
            }
        }

        // ------------------------------------------------------------------------------------------ pause
        void SetPause(bool on)
        {
            paused = on;
            if (pauseRoot != null) { Destroy(pauseRoot); pauseRoot = null; }
            if (!on) return;
            pauseRoot = new GameObject("pause"); pauseRoot.transform.SetParent(hudRoot, false);
            var bg = Pix.Make("dim", pauseRoot.transform, white, new Vector3(-8f, 0f, 0), 270); bg.transform.localScale = new Vector3(640, 360, 1); bg.color = new Color(.09f, .04f, .22f, .78f);
            PText.Create(pauseRoot.transform, new Vector3(0, 1.8f, 0), "PAUSA", 6, new Color(1f, .93f, .4f), true, 280);
            string[] l = { "P1  A D MOVER  ESPACIO SALTAR  F DISPARAR  G DASH  H SUPER", "P2  FLECHAS  SHIFT SALTAR  CTRL DISPARAR  . DASH  / SUPER", "SALTA SOBRE LO ROSA (PARRY)  PATEA LAS BOLAS", "ESC SEGUIR   BACKSPACE MENU PRINCIPAL" };
            for (int i = 0; i < l.Length; i++) PText.Create(pauseRoot.transform, new Vector3(0, 0.5f - i * 0.55f, 0), l[i], 1, Color.white, true, 280);
        }

        // ------------------------------------------------------------------------------------------ flow
        void ClearAll()
        {
            if (bond != null) bond.Hide();
            foreach (var e in enemies) e.Dispose(); enemies.Clear();
            foreach (var s in eshots) s.Pop(); eshots.Clear();
            foreach (var w in waves) w.dead = true; waves.Clear();
            foreach (var b in beams) b.dead = true; beams.Clear();
            foreach (var f in fxs) f.Kill(); fxs.Clear();
            foreach (var p in parts) { p.dead = true; if (p.go) Destroy(p.go); } parts.Clear();
            foreach (var s in shots) s.dead = true; shots.Clear();
            foreach (var c in crystals) c.Dispose(); crystals.Clear();
            foreach (var i in icicles) i.Dispose(); icicles.Clear();
            foreach (var p in pillars) p.Dispose(); pillars.Clear();
            pending.Clear();
            if (boss != null) { boss.Dispose(); boss = null; }
            if (envCur != null) Destroy(envCur); if (nextEnv != null) Destroy(nextEnv);
            if (titleRoot != null) Destroy(titleRoot);
            if (resultsRoot != null) Destroy(resultsRoot);
            if (helpRoot != null) Destroy(helpRoot);
            for (int i = 0; i < 2; i++) if (liftBubbles[i] != null) { Destroy(liftBubbles[i].gameObject); liftBubbles[i] = null; }
            hintText.gameObject.SetActive(false); hintPanel.gameObject.SetActive(false);
            wind.Reset(false);
            bossDownFlag = false;
        }

        void StartGame(int count)
        {
            armorHinted = false; solo = count == 1; playerCount = 2; int startLevel = menuWorld * Levels.PerWorld; ClearAll(); totalTime = 0; kills = 0;
            foreach (var p in players) { p.parries = 0; p.hitsTaken = 0; p.meter = 0; }
            StartLevel(startLevel);
        }

        public void StartLevel(int idx)
        {
            levelIndex = idx; var def = Levels.All[idx];
            foreach (var e in enemies) e.Dispose(); enemies.Clear();
            foreach (var s in eshots) s.Pop(); eshots.Clear();
            foreach (var w in waves) w.dead = true; waves.Clear();
            foreach (var b in beams) b.dead = true; beams.Clear();
            foreach (var c in crystals) c.Dispose(); crystals.Clear();
            foreach (var i in icicles) i.Dispose(); icicles.Clear();
            foreach (var p in pillars) p.Dispose(); pillars.Clear();
            if (boss != null) { boss.Dispose(); boss = null; }
            if (envCur != null) Destroy(envCur);
            if (titleRoot != null) Destroy(titleRoot);
            if (resultsRoot != null) Destroy(resultsRoot);
            world = new World(def, idx * Levels.Height);
            envCur = EnvBuilder.Build(world);
            camY = world.Center.y;
            Sfx.PlayMusic(def.theme);
            SetupPlayersAndEnemies(def);
            levelTime = 0f; gameOverT = 0f; bossDownFlag = false;
            SetHudVisible(true, def.boss);
            string sub = def.boss ? def.title : def.title;
            ShowBanner(LevelBannerTitle(idx), sub, def.boss ? 2.3f : 2.4f, Mode.Play, def.boss ? 5 : 4);
            if (def.boss) Sfx.Play("bell", .8f);
        }

        void SetupHazards(LevelDef def)
        {
            foreach (var c in def.crystals) crystals.Add(new Crystal(new Vector2(c.x, c.y + world.y0), c.bridgeIndex));
            icicleT = 3.5f; wind.Reset(def.wind && !def.boss);
        }

        void SetupPlayersAndEnemies(LevelDef def)
        {
            for (int i = 0; i < 2; i++)
            {
                var p = players[i];
                bool want = i < playerCount || p.joined;
                if (!want) { p.joined = false; p.SetVisible(false); continue; }
                p.Join(new Vector2(def.playerX[i], world.GroundTop + 0.02f));
                p.meter = Mathf.Min(p.meter, 5f);
            }
            pending.Clear();
            float t = 0.3f;
            foreach (var s in def.spawns) { pending.Add(new KeyValuePair<float, EnemySpawn>(t, s)); t += 0.22f; }
            if (def.boss) { boss = def.world == 1 ? (BossBase)new IceBoss(world.GroundTop) : new Boss(world.GroundTop); lastBossPhase = 1; Sfx.Prefetch(BossTrack(def.world, 2)); Sfx.Prefetch(BossTrack(def.world, 3)); }
            SetupHazards(def);
        }

        void RestartLevel()
        {
            HideBanner();
            foreach (var p in players) { if (p.joined) p.meter = 0; }
            StartLevel(levelIndex);
        }

        // ------------------------------------------------------------------------------------------ update
        void Update()
        {
            float raw = Mathf.Min(Time.unscaledDeltaTime, 0.05f);
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame && mode == Mode.Play) SetPause(!paused);
            if (paused && Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame) { SetPause(false); EnterTitle(); return; }
            if (hitStop > 0f) { hitStop -= raw; dt = 0f; } else dt = paused ? 0f : raw;
            time += dt; modeT += raw;

            switch (mode)
            {
                case Mode.Title: TickTitle(raw); break;
                case Mode.Help: TickHelp(); break;
                case Mode.Banner:
                    TickWorldIdle();
                    if (modeT > bannerLife) { HideBanner(); mode = afterBanner; modeT = 0; if (afterBanner == Mode.Play && boss != null) boss.Activate(); Sfx.Play("wallop", .5f); ShowHint(world.def.hint, 6.5f); }
                    break;
                case Mode.Play: TickPlay(); break;
                case Mode.Clear: TickWorld(false); if (modeT > 1.5f) BeginLift(); break;
                case Mode.Lift: TickLift(raw); break;
                case Mode.KO: TickWorld(false); if (modeT > 2.6f) AfterBoss(); break;
                case Mode.WorldDone: TickWorldDone(); break;
                case Mode.Results: if (modeT > 0.8f && GumInput.Confirm()) { Sfx.Play("select"); EnterTitle(); } break;
                case Mode.GameOver: TickWorld(false); if (modeT > 0.8f && GumInput.Confirm()) { Sfx.Play("select"); RestartLevel(); } break;
            }
            TickBossMusic();
            TickBanner(raw); TickHint(raw);
            TickCamera(raw);
            if (mode != Mode.Title && mode != Mode.Results && mode != Mode.Help && mode != Mode.WorldDone) UpdateHud();
        }

        static Track BossTrack(int world, int phase)
        {
            if (world == 0) return phase == 1 ? Track.CircoBoss1 : (phase == 2 ? Track.CircoBoss2 : Track.CircoBoss3);
            return phase == 1 ? Track.PalacioBoss1 : (phase == 2 ? Track.PalacioBoss2 : Track.PalacioBoss3);
        }

        void TickBossMusic()
        {
            if (boss == null || (mode != Mode.Play && mode != Mode.Banner)) return;
            if (boss.phase != lastBossPhase)
            {
                lastBossPhase = boss.phase; Sfx.PlayTrack(BossTrack(world.def.world, boss.phase), 3.2f);
                // a breather between forms: everybody gets a heart back and fallen heroes return
                foreach (var p in players) if (p.joined) { if (p.ghost) p.Revive(2); else p.Heal(1); }
                ShowHint("NUEVA FORMA! +1 CORAZON PARA LOS DOS", 2.5f);
            }
        }

        void LateUpdate()
        {
            hudRoot.position = new Vector3(0f, Mathf.Round(camY * 40f) / 40f, 0f);
        }

        public bool IsBondActive { get { return bond != null && bond.Active; } }
        public void HintArmor()
        {
            if (armorHinted) return; armorHinted = true;
            ShowHint("ACORAZADO: EL DISPARO REBOTA. ATRAPALO CON EL LAZO!", 4f);
        }

        /// <summary>Guard phase: once per form, when the boss has lost ~40% of that form's life, gum bounces off until the string has done its share.</summary>
        void TickLazo()
        {
            if (boss == null) { lazoBoss = null; return; }
            if (lazoBoss != boss) { lazoBoss = boss; lazoDone = new bool[4]; boss.shotProof = false; boss.glow = false; }
            if (boss.shotProof)
            {
                boss.lazoT -= dt;
                bool broke = boss.lazoTaken >= boss.lazoTarget;
                if (broke || boss.lazoT <= 0f || boss.trans || boss.dying || boss.dead)
                {
                    boss.shotProof = false; boss.glow = false;
                    if (broke && !boss.dead) { Sfx.Play("parry"); Burst(boss.Hurtboxes()[0].center, 18, 6f); ShowHint("GUARDIA ROTA!", 2f); }
                }
                else if (((int)(time * 6f)) != ((int)((time - dt) * 6f)))
                {
                    var c = boss.Hurtboxes()[0]; Burst(new Vector2(Random.Range(c.xMin, c.xMax), Random.Range(c.yMin, c.yMax)), 1, 2f);
                }
            }
            else if (boss.Hittable && boss.phase >= 1 && boss.phase <= 3 && !lazoDone[boss.phase])
            {
                int p = boss.phase; var fl = boss.PhaseFloors;
                int top = p == 1 ? boss.TotalHp : fl[p - 2], bot = p == 3 ? 0 : fl[p - 1];
                if (boss.hp <= bot + (int)(0.6f * (top - bot)))
                {
                    lazoDone[p] = true; boss.shotProof = true; boss.glow = true; boss.lazoT = 20f; boss.lazoTaken = 0;
                    boss.lazoTarget = Mathf.Max(8, (int)(0.22f * (top - bot)));
                    ShowHint("GUARDIA! SOLO EL LAZO LO DANA: RODEALO", 4.5f); Sfx.Play("warn", .7f);
                }
            }
        }

        void TickHug()
        {
            var a = players[0]; var b = players[1]; hugCd -= dt;
            if (!a.Alive || !b.Alive) { hugT = 0f; return; }
            bool close = Mathf.Abs(a.Center.x - b.Center.x) < 1.5f && Mathf.Abs(a.Center.y - b.Center.y) < 1.3f;
            bool hurt = a.hp < a.maxHp || b.hp < b.maxHp;
            if (close && hurt && hugCd <= 0f)
            {
                hugT += dt;
                if (((int)(hugT * 4f)) != ((int)((hugT - dt) * 4f))) Burst((a.Center + b.Center) * 0.5f + new Vector2(0f, 0.9f), 2, 2f);
                if (hugT >= 1.6f)
                {
                    var low = a.hp <= b.hp ? a : b; low.Heal(1); hugT = 0f; hugCd = 14f; statHugs++;
                    Sfx.Play("revive"); Burst(low.Center, 12, 5f); SpawnFx("parry", "parry", low.Center, 1f, false, 30);
                    ShowHint("ABRAZO! +1 CORAZON", 2f);
                }
            }
            else hugT = Mathf.Max(0f, hugT - dt * 2f);
        }

        void TickWorldIdle()
        {
            world.Tick(dt, time);
            foreach (var p in players) p.Tick(dt, true);              // free to move while the banner shows; the enemies wait for it to finish
            foreach (var s in shots) s.Tick(dt);
            if (boss != null) boss.Tick(dt);
            foreach (var c in crystals) c.Tick(dt);
            bond.Tick(dt);
            TickFxAndParts();
            shots.RemoveAll(s => s.dead);
        }

        void TickPlay()
        {
            levelTime += dt; totalTime += dt;
            TickWorld(true);
            TickHug();
            if (!world.def.boss && !holdClear && AliveEnemies() == 0 && !PlayersAllDown()) { mode = Mode.Clear; modeT = 0; ShowBannerRaw("CLEAR!", "", 99f, 8, new Color(1f, .93f, .4f)); Sfx.Play("clear"); foreach (var p in players) if (p.joined) p.AddMeter(0.5f); }
            if (gameOverT > 0f) { gameOverT -= dt; if (gameOverT <= 0f && PlayersAllDown()) { mode = Mode.GameOver; modeT = 0; ShowBannerRaw("GAME OVER", "ENTER PARA REINTENTAR", 99f, 6, new Color(1f, .5f, .6f)); Sfx.Play("hurt"); Sfx.PlayTrack(Track.GameOver, 0.2f); } }
        }

        void TickWorld(bool input)
        {
            world.Tick(dt, time);
            SpawnPending(dt);
            wind.Tick(dt);
            foreach (var p in players) p.Tick(dt, input);
            bond.Tick(dt);
            foreach (var e in enemies) e.Tick(dt);
            foreach (var s in shots) s.Tick(dt);
            foreach (var s in eshots) s.Tick(dt);
            foreach (var w in waves) w.Tick(dt);
            foreach (var b in beams) b.Tick(dt);
            foreach (var c in crystals) c.Tick(dt);
            foreach (var p in pillars) p.Tick(dt);
            foreach (var i in icicles) i.Tick(dt);
            if (boss != null) boss.Tick(dt);
            TickLazo();
            if (world.def.icicles && mode == Mode.Play)
            {
                icicleT -= dt;
                if (icicleT <= 0f)
                {
                    icicleT = Random.Range(2.3f, 3.6f);
                    var t = RandomAlivePlayer(); float px = t != null ? t.body.pos.x + Random.Range(-3f, 3f) : Random.Range(-6f, 6f);
                    if (icicles.Count < 4) icicles.Add(new Icicle(Mathf.Clamp(px, -7f, 7f)));
                }
            }
            TickFxAndParts();
            enemies.RemoveAll(e => e.remove); shots.RemoveAll(s => s.dead); eshots.RemoveAll(s => s.dead);
            waves.RemoveAll(w => w.dead); beams.RemoveAll(b => b.dead); icicles.RemoveAll(i => i.dead); pillars.RemoveAll(p => p.dead);
        }

        void SpawnPending(float d)
        {
            if (pending.Count == 0) return;
            int alive = 0; foreach (var e in enemies) if (e.state != EState.Dead) alive++;
            if (alive >= world.def.maxAlive) { spawnCd = Mathf.Max(spawnCd, 0.6f); return; }       // wait until someone is cleared
            spawnCd -= d;
            if (spawnCd <= 0f)
            {
                var s = pending[0].Value; pending.RemoveAt(0);
                SpawnEnemy(s.kind, s.color, new Vector2(s.x, s.y + world.y0)); spawnCd = 0.9f;
            }
        }

        void TickFxAndParts()
        {
            foreach (var f in fxs) f.Tick(dt); fxs.RemoveAll(f => f.dead);
            foreach (var p in parts) p.Tick(dt); parts.RemoveAll(p => p.dead);
        }

        // ------------------------------------------------------------------------------------------ the climb to the next level
        void BeginLift()
        {
            HideBanner();
            int next = levelIndex + 1;
            nextWorld = new World(Levels.All[next], next * Levels.Height);
            nextEnv = EnvBuilder.Build(nextWorld);
            liftFromY = world.Center.y; liftToY = nextWorld.Center.y;
            var bs = Art.Get("bubble_lift");
            for (int i = 0; i < 2; i++)
            {
                var p = players[i]; if (!p.joined) continue;
                p.SetVisible(true);
                liftStart[i] = p.body.pos;
                liftEnd[i] = new Vector2(Levels.All[next].playerX[i], nextWorld.GroundTop + 0.02f);
                liftBubbles[i] = Pix.Make("liftbubble", entRoot, bs[0], p.Center, 35); liftAnims[i] = new Anim(liftBubbles[i], bs, "lift");
            }
            Sfx.Prefetch((Track)Levels.All[next].theme);
            bond.Hide(); mode = Mode.Lift; modeT = 0; Sfx.Play("lift");
            foreach (var e in eshots) e.Pop(); eshots.Clear();
            foreach (var i2 in icicles) i2.Dispose(); icicles.Clear();
            wind.Reset(false);
        }

        void TickLift(float raw)
        {
            const float dur = 2.5f;
            float u = Mathf.Clamp01(modeT / dur); float e = u * u * (3f - 2f * u);
            camY = Mathf.Lerp(liftFromY, liftToY, e);
            for (int i = 0; i < 2; i++)
            {
                var p = players[i]; if (!p.joined) continue;
                var pos = Vector2.Lerp(liftStart[i], liftEnd[i], e);
                pos.x += Mathf.Sin(modeT * 3f + i) * 0.35f * Mathf.Sin(u * Mathf.PI);
                p.Carry(pos, raw);
                liftAnims[i].Tick(raw);
                liftBubbles[i].transform.position = p.Center + new Vector2(0, 0.05f);
                liftBubbles[i].color = new Color(1, 1, 1, 0.85f);
            }
            TickFxAndParts();
            if (u >= 1f) FinishLift();
        }

        void FinishLift()
        {
            int next = levelIndex + 1;
            for (int i = 0; i < 2; i++) if (liftBubbles[i] != null) { SpawnFx("bubble_pops", "pink", liftBubbles[i].transform.position, 2f, false, 36); Destroy(liftBubbles[i].gameObject); liftBubbles[i] = null; }
            Sfx.Play("pop");
            foreach (var c in crystals) c.Dispose(); crystals.Clear();
            if (envCur != null) Destroy(envCur);
            envCur = nextEnv; nextEnv = null; world = nextWorld; nextWorld = null;
            levelIndex = next; var def = Levels.All[next];
            foreach (var f in fxs) f.Kill(); fxs.Clear();
            foreach (var pl in parts) { pl.dead = true; if (pl.go) Destroy(pl.go); } parts.Clear();
            Sfx.PlayMusic(def.theme);
            for (int i = 0; i < 2; i++)
            {
                var p = players[i]; if (!p.joined) continue;
                if (def.boss) { if (p.ghost) p.Revive(p.maxHp); else p.Heal(p.maxHp); }      // a full tank of hearts for the boss
                else if (p.ghost) p.Revive(2); else p.Heal(1);
                p.Place(liftEnd[i]); p.body.vel = Vector2.zero; p.invuln = 1f;
            }
            camY = world.Center.y;
            if (boss != null) { boss.Dispose(); boss = null; }
            pending.Clear(); float t = 0.3f;
            foreach (var s in def.spawns) { pending.Add(new KeyValuePair<float, EnemySpawn>(t, s)); t += 0.22f; }
            if (def.boss) { boss = def.world == 1 ? (BossBase)new IceBoss(world.GroundTop) : new Boss(world.GroundTop); lastBossPhase = 1; Sfx.Prefetch(BossTrack(def.world, 2)); Sfx.Prefetch(BossTrack(def.world, 3)); }
            SetupHazards(def);
            levelTime = 0; SetHudVisible(true, def.boss);
            ShowBanner(LevelBannerTitle(next), def.title, def.boss ? 2.3f : 2.4f, Mode.Play, def.boss ? 5 : 4);
            if (def.boss) Sfx.Play("bell", .8f);
        }

        // ------------------------------------------------------------------------------------------ boss defeated: next kingdom or the end
        void AfterBoss()
        {
            HideBanner();
            if (levelIndex + 1 < Levels.All.Length) ShowWorldDone(); else ShowResults();
        }

        void ShowWorldDone()
        {
            mode = Mode.WorldDone; modeT = 0f; SetHudVisible(false); Sfx.PlayTrack(Track.WorldDone, 0.1f);
            int w = world.def.world;
            resultsRoot = new GameObject("worlddone"); resultsRoot.transform.SetParent(hudRoot, false);
            var bg = Pix.Make("dim", resultsRoot.transform, white, new Vector3(-8f, 0f, 0), 280); bg.transform.localScale = new Vector3(640, 360, 1); bg.color = new Color(.16f, .04f, .22f, .82f);
            PText.Create(resultsRoot.transform, new Vector3(0, 2.6f, 0), "MUNDO " + (w + 1) + " COMPLETO", 6, new Color(1f, .93f, .4f), true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, 1.4f, 0), Levels.WorldNames[w] + " SUPERADO", 3, Color.white, true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, 0.2f, 0), "SIGUIENTE: " + Levels.WorldNames[w + 1], 3, new Color(.6f, .9f, 1f), true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, -0.7f, 0), "NUEVA ROPA DE INVIERNO Y NUEVOS PELIGROS", 2, new Color(1f, .85f, .95f), true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, -2.6f, 0), "PRESIONA ENTER", 2, new Color(.9f, .9f, 1f), true, 290);
        }

        void TickWorldDone()
        {
            if (modeT > 0.8f && GumInput.Confirm())
            {
                Sfx.Play("select"); if (resultsRoot != null) Destroy(resultsRoot);
                foreach (var p in players) if (p.joined) { p.ghost = false; p.hp = p.maxHp; }
                StartLevel(levelIndex + 1);
            }
        }

        // ------------------------------------------------------------------------------------------ results
        void ShowResults()
        {
            HideBanner();
            mode = Mode.Results; modeT = 0f; SetHudVisible(false); Sfx.PlayTrack(Track.Victory, 0.1f);
            int hits = 0, parries = 0; foreach (var p in players) if (p.joined) { hits += p.hitsTaken; parries += p.parries; }
            float par = 0; foreach (var l in Levels.All) par += l.parTime;
            bool both = levelIndex >= Levels.PerWorld;
            if (!both) { par = 0; for (int i = 0; i < Levels.PerWorld; i++) par += Levels.All[i].parTime; }
            float score = 100f - hits * 9f - Mathf.Max(0f, totalTime - par) * 0.15f + Mathf.Min(20f, parries * 4f);
            string grade = score >= 96 ? "S" : score >= 82 ? "A" : score >= 66 ? "B" : score >= 50 ? "C" : "D";
            resultsRoot = new GameObject("results"); resultsRoot.transform.SetParent(hudRoot, false);
            var bg = Pix.Make("dim", resultsRoot.transform, white, new Vector3(-8f, 0f, 0), 280); bg.transform.localScale = new Vector3(640, 360, 1); bg.color = new Color(.16f, .04f, .22f, .8f);
            PText.Create(resultsRoot.transform, new Vector3(0, 2.4f, 0), "VICTORIA!", 7, new Color(1f, .93f, .4f), true, 290);
            int mm = (int)(totalTime / 60f), ss = (int)(totalTime % 60f);
            PText.Create(resultsRoot.transform, new Vector3(0, 1.3f, 0), "TIEMPO " + mm + ":" + ss.ToString("00"), 3, Color.white, true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, 0.6f, 0), "GOLPES RECIBIDOS " + hits, 3, Color.white, true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, -0.1f, 0), "PARRYS " + parries, 3, Color.white, true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, -1.75f, 0), "NOTA " + grade, 5, new Color(1f, .62f, .85f), true, 290);
            PText.Create(resultsRoot.transform, new Vector3(0, -3.1f, 0), "PRESIONA ENTER", 2, new Color(.9f, .9f, 1f), true, 290);
        }

        // ------------------------------------------------------------------------------------------ camera
        void TickCamera(float raw)
        {
            if (mode != Mode.Lift) camY = Mathf.Lerp(camY, world != null ? world.Center.y : 4.5f, 0.2f);
            float ox = 0, oy = 0;
            if (shakeT > 0f)
            {
                shakeT -= raw; float a = shakeAmp * Mathf.Clamp01(shakeT / 0.3f);
                ox = Mathf.Round(Random.Range(-a, a) * 40f) / 40f; oy = Mathf.Round(Random.Range(-a, a) * 40f) / 40f;
                if (shakeT <= 0f) shakeAmp = 0f;
            }
            float cy = Mathf.Round(camY * 40f) / 40f;
            cam.transform.position = new Vector3(ox, cy + oy, -10f);
        }

        // ------------------------------------------------------------------------------------------ debug / test hooks
        public void DebugMenuWorld(int w) { SwitchWorldMenu(w); }
        public void DebugClearEnemies() { foreach (var e in enemies) e.Dispose(); enemies.Clear(); pending.Clear(); }
        public void DebugKillAll() { foreach (var e in enemies) if (e.state != EState.Dead) e.PopKill(); pending.Clear(); }
        public void DebugDamageBoss(int n) { if (boss != null) boss.Damage(n, null, false); }
        public void DebugStart(int n) { menuWorld = 0; StartGame(n); }
        public void DebugStartAt(int level, int n) { playerCount = 2; solo = n == 1; menuWorld = level >= Levels.PerWorld ? 1 : 0; ClearAll(); totalTime = 0; StartLevel(level); }
        public void DebugGoto(int idx) { StartGame(Mathf.Max(1, playerCount)); if (idx > 0) { StartLevel(idx); } }
        public void DebugKillPlayers() { foreach (var p in players) if (p.joined && p.Alive) { p.invuln = 0; p.superT = 0; p.Hit(99, p.body.pos.x - 1f); } }
        public void DebugSkipBanner() { if (mode == Mode.Banner) modeT = bannerLife + 0.01f; }
        public int WorldOf(int level) { return Levels.All[level].world; }

        public void SaveShot(string path)
        {
            cam.Render();
            var prev = RenderTexture.active; RenderTexture.active = rt;
            var t = new Texture2D(RW, RH, TextureFormat.RGB24, false); t.ReadPixels(new Rect(0, 0, RW, RH), 0, 0); t.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(path, t.EncodeToPNG()); Destroy(t);
        }
    }
}
