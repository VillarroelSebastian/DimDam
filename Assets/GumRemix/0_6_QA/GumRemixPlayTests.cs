using System;
using System.Collections;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GumRemix.Tests
{
    /// <summary>
    /// QA for Chicle Rumble. A bot plays both kingdoms end to end; targeted tests prove each ice mechanic with numbers.
    /// Screenshots land in the folder given by env var GUMREMIX_SHOTS (or the temp folder).
    /// </summary>
    public class GumRemixPlayTests
    {
        GameCtl G; string outDir; int frame; readonly StringBuilder report = new StringBuilder();

        // ------------------------------------------------------------------------------------------ helpers
        string Shot(string name) { var p = Path.Combine(outDir, name + ".png"); G.SaveShot(p); return p; }

        Pad Bot(int i)
        {
            var pad = new Pad(); var p = G.players[i];
            if (p == null || !p.joined) return pad;
            if (p.ghost) { pad.x = (G.players[1 - i].Center.x > p.body.pos.x) ? 1 : -1; pad.up = true; return pad; }
            if (p.frozenT > 0f) { pad.jumpDown = frame % 2 == 0; return pad; }          // mash to break the ice
            Vector2 tgt = Vector2.zero; bool has = false; float best = float.MaxValue;
            foreach (var e in G.enemies)
            {
                if (e.state == EState.Dead) continue;
                float d = (e.body.Center - p.Center).sqrMagnitude; if (d < best) { best = d; tgt = e.body.Center; has = true; }
            }
            if (G.boss != null && G.boss.Dangerous)
            {
                var sun = G.boss as IceBoss;
                tgt = sun != null ? sun.SunPos() : new Vector2(G.boss.x, G.boss.y + 2.5f); has = true;
            }
            if (!has) return pad;
            float dx = tgt.x - p.Center.x, dy = tgt.y - p.Center.y;
            pad.x = Mathf.Abs(dx) > 2.2f ? Mathf.Sign(dx) : 0f;
            pad.shoot = true; pad.up = dy > 1.2f && Mathf.Abs(dx) < 3f;
            pad.jumpHeld = dy > 1.0f;
            pad.jumpDown = (frame + i * 17) % 55 == 0 && (dy > 1.0f || (frame % 3 == 0));
            foreach (var e in G.enemies) if (e.state == EState.Stuck) { float ex = e.body.pos.x - p.body.pos.x; if (Mathf.Abs(e.body.pos.y - p.body.pos.y) < 1f) pad.x = Mathf.Sign(ex); }
            return pad;
        }

        IEnumerator Frames(int n) { for (int i = 0; i < n; i++) { frame++; yield return null; } }
        IEnumerator Seconds(float s) { float t = 0; while (t < s) { t += Mathf.Min(Time.unscaledDeltaTime, 0.05f); frame++; yield return null; } }
        /// <summary>The boss is down once the KO banner shows or the game has already moved on to the world-done screen.</summary>
        IEnumerator UntilBossDown(float timeout)
        {
            float t = 0; while (G.mode != Mode.KO && G.mode != Mode.WorldDone && G.mode != Mode.Results && t < timeout) { t += Time.unscaledDeltaTime; frame++; yield return null; }
            Assert.IsTrue(G.mode == Mode.KO || G.mode == Mode.WorldDone || G.mode == Mode.Results, "the boss never went down (stuck in " + G.mode + ")");
        }
        IEnumerator UntilMode(Mode m, float timeout)
        {
            float t = 0; while (G.mode != m && t < timeout) { t += Time.unscaledDeltaTime; frame++; yield return null; }
            Assert.AreEqual(m, G.mode, "Timed out waiting for mode " + m + " (stuck in " + G.mode + ")");
        }
        void Log(string s) { report.AppendLine(s); Debug.Log("[GumQA] " + s); }
        void PressEnter() { GumInput.ForceConfirm = true; }

        IEnumerator Boot()
        {
            outDir = Environment.GetEnvironmentVariable("GUMREMIX_SHOTS");
            if (string.IsNullOrEmpty(outDir)) outDir = Path.Combine(Application.temporaryCachePath, "gumshots");
            Directory.CreateDirectory(outDir);
            GameCtl.ResetSession(); GumInput.Override = null; GumInput.ForceConfirm = false;
            var go = new GameObject("boot"); go.AddComponent<GumRemixBootstrap>();
            yield return Frames(3);
            G = GameCtl.I; Assert.NotNull(G, "GameCtl missing");
            Assert.AreEqual(Mode.Title, G.mode);
        }

        IEnumerator Teardown()
        {
            GumInput.Override = null; GumInput.ForceConfirm = false;
            if (G != null) UnityEngine.Object.Destroy(G.gameObject);
            yield return Frames(3);
            File.AppendAllText(Path.Combine(outDir, "report.txt"), report.ToString()); report.Length = 0;
        }

        /// <summary>Start a level directly, skip the banner and park the level so it cannot end by itself.</summary>
        IEnumerator StartAt(int level, int players)
        {
            G.holdClear = true; G.god = false;
            G.DebugStartAt(level, players);
            yield return Frames(2); G.DebugSkipBanner();
            yield return UntilMode(Mode.Play, 4f);
            G.DebugClearEnemies();
            yield return Frames(2);
        }

        // ------------------------------------------------------------------------------------------ menu, help, world select
        [UnityTest, Timeout(300000)]
        public IEnumerator MenuAndHelp()
        {
            yield return Boot();
            yield return Seconds(0.6f); Shot("m00_title_candy");
            G.DebugMenuWorld(1); yield return Seconds(0.5f); Shot("m01_title_ice");
            G.DebugMenuWorld(0); yield return Seconds(0.4f);
            PressEnter(); yield return Seconds(0.2f);                     // first play: the help appears before the game
            Assert.AreEqual(Mode.Help, G.mode, "first play should open the help screens");
            yield return Seconds(0.4f); Shot("m02_help_controls");
            PressEnter(); yield return Seconds(0.4f); Shot("m03_help_howto");
            PressEnter(); yield return Seconds(0.4f); Shot("m04_help_together");
            PressEnter(); yield return Seconds(0.4f); Shot("m05_help_ice");
            PressEnter(); yield return Seconds(0.3f);
            Assert.AreEqual(Mode.Banner, G.mode, "closing the help should start the game");
            Log("menu and help ok");
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ full playthrough of both kingdoms
        [UnityTest, Timeout(1800000)]
        public IEnumerator FullPlaythrough()
        {
            yield return Boot();
            yield return Seconds(0.8f); Shot("00_title");
            GumInput.Override = Bot; G.god = true;
            G.DebugStart(2);
            yield return Seconds(0.6f); Shot("01_banner_level1");
            yield return UntilMode(Mode.Play, 6f);
            Assert.AreEqual(2, CountJoined(), "two players expected");
            yield return Seconds(4f); Shot("02_level1_play");
            float natural = 0f;
            while (G.mode == Mode.Play && natural < 70f) { natural += Time.unscaledDeltaTime; frame++; yield return null; }
            Log("candy 1 natural result after " + natural.ToString("0.0") + "s: mode=" + G.mode + " stuck=" + G.statStuck + " kicks=" + G.statKicks + " ballKills=" + G.statBallKills);
            Assert.Greater(G.statStuck, 0, "no enemy was ever stuck in gum");
            Assert.Greater(G.statKicks, 0, "no gum ball was ever kicked");
            if (G.mode == Mode.Play) G.DebugKillAll();

            // ---- candy levels 1..4 then the boss
            for (int lv = 0; lv < 4; lv++)
            {
                if (lv > 0) { yield return Seconds(4f); Shot("c" + (lv + 1) + "_play"); }
                if (G.mode == Mode.Play) G.DebugKillAll();
                yield return UntilMode(Mode.Clear, 6f); yield return UntilMode(Mode.Lift, 6f);
                yield return Seconds(1.2f); if (lv == 0) Shot("lift_c1_mid");
                yield return UntilMode(Mode.Banner, 8f);
                Assert.AreEqual(lv + 1, G.levelIndex, "level index after climb");
                yield return UntilMode(Mode.Play, 6f);
            }
            Assert.AreEqual(4, G.levelIndex); Assert.IsInstanceOf<Boss>(G.boss);
            yield return Seconds(5f); Shot("c5_boss_phase1");
            G.DebugDamageBoss(Boss.Phase1Hp + 5); yield return Seconds(2.4f); Assert.AreEqual(2, G.boss.phase);
            yield return Seconds(4f); Shot("c5_boss_phase2");
            G.DebugDamageBoss(Boss.Phase2Hp + 5); yield return Seconds(2.4f); Assert.AreEqual(3, G.boss.phase);
            yield return Seconds(4f); Shot("c5_boss_phase3");
            G.DebugDamageBoss(Boss.Phase3Hp + 5);
            yield return UntilBossDown(8f); yield return Seconds(0.4f); Shot("c5_boss_ko");
            yield return UntilMode(Mode.WorldDone, 8f); yield return Seconds(0.6f); Shot("w_world_complete");
            Log("candy world finished");

            // ---- into the ice kingdom
            PressEnter(); yield return Seconds(0.3f);
            Assert.AreEqual(5, G.levelIndex, "world 2 starts at level index 5");
            Assert.AreEqual(Mode.Banner, G.mode);
            yield return Seconds(0.8f); Shot("i1_banner");
            yield return UntilMode(Mode.Play, 6f);
            for (int lv = 5; lv < 9; lv++)
            {
                yield return Seconds(lv == 5 ? 6f : 5f); Shot("i" + (lv - 4) + "_play");
                int alive = G.AliveEnemies();
                if (G.mode == Mode.Play) G.DebugKillAll();
                yield return UntilMode(Mode.Clear, 6f); yield return UntilMode(Mode.Lift, 6f);
                yield return Seconds(1.2f); Shot("lift_i" + (lv - 4) + "_mid");
                yield return UntilMode(Mode.Banner, 8f);
                Assert.AreEqual(lv + 1, G.levelIndex, "ice level index after climb");
                Log("ice: climbed to level " + (G.levelIndex + 1) + " (left " + alive + " enemies)");
                yield return UntilMode(Mode.Play, 6f);
            }
            Assert.AreEqual(9, G.levelIndex); Assert.IsInstanceOf<IceBoss>(G.boss);
            yield return Seconds(5f); Shot("i5_boss_phase1");
            G.DebugDamageBoss(IceBoss.Phase1Hp + 5); yield return Seconds(2.4f); Assert.AreEqual(2, G.boss.phase);
            yield return Seconds(4f); Shot("i5_boss_phase2");
            G.DebugDamageBoss(IceBoss.Phase2Hp + 5); yield return Seconds(2.4f); Assert.AreEqual(3, G.boss.phase);
            yield return Seconds(4f); Shot("i5_boss_phase3");
            G.DebugDamageBoss(IceBoss.Phase3Hp + 5);
            yield return Seconds(1.0f); Shot("i5_boss_dying");
            yield return UntilBossDown(8f); yield return Seconds(0.4f); Shot("i5_boss_ko");
            yield return UntilMode(Mode.Results, 8f); yield return Seconds(0.6f); Shot("z_results");
            Log("ice world finished, results reached");

            // ---- game over & retry
            G.god = false; GumInput.Override = null;
            G.DebugGoto(0);
            GumInput.Override = i => new Pad();
            yield return UntilMode(Mode.Play, 8f); yield return Seconds(1f);
            G.DebugKillPlayers();
            yield return UntilMode(Mode.GameOver, 6f); yield return Seconds(0.3f); Shot("zz_gameover");
            Log("game over reached");
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ ice mechanics, proven with numbers
        [UnityTest, Timeout(900000)]
        public IEnumerator IceMechanics()
        {
            yield return Boot();
            GumInput.Override = i => new Pad();

            // ---- 1) ice keeps your momentum, snow brakes you
            yield return StartAt(5, 1);
            var p = G.players[0]; G.god = true;
            p.Place(new Vector2(-7.4f, G.world.GroundTop + 0.02f)); yield return Frames(6);       // let the floor type register first
            Assert.IsTrue(p.body.groundIce, "the first sector of the lake is ice");
            p.body.vel = new Vector2(6.4f, 0); float xi = p.body.pos.x; float t = 0;
            while (t < 3f && Mathf.Abs(p.body.vel.x) > 0.4f) { t += Time.unscaledDeltaTime; frame++; yield return null; }
            float dIce = p.body.pos.x - xi;
            yield return Frames(5);
            p.Place(new Vector2(-2.6f, G.world.GroundTop + 0.02f)); yield return Frames(6);
            Assert.IsFalse(p.body.groundIce, "the middle sector is snow");
            p.body.vel = new Vector2(6.4f, 0); float xs = p.body.pos.x; t = 0;
            while (t < 3f && Mathf.Abs(p.body.vel.x) > 0.4f) { t += Time.unscaledDeltaTime; frame++; yield return null; }
            float dSnow = p.body.pos.x - xs;
            Log("slide: ice " + dIce.ToString("0.00") + "u, snow " + dSnow.ToString("0.00") + "u");
            Assert.Greater(dIce, 2.2f, "ice should keep sliding");
            Assert.Less(dSnow, 1.2f, "snow should stop you quickly");
            Assert.Greater(dIce, dSnow * 2.5f, "ice must slide far more than snow");

            // ---- 2) frozen: lasts a while, mashing buttons thaws it, then you are immune for a moment
            G.god = false; p.invuln = 0f; p.Place(new Vector2(0f, G.world.GroundTop + 0.02f));
            yield return Frames(3);
            p.Hit(1, p.body.pos.x - 1f, true);
            Assert.Greater(p.frozenT, 2f, "a frost shard should freeze for ~2.4s"); Assert.AreEqual(3, p.hp);
            yield return Frames(2); Shot("x1_frozen");
            float tf = 0f; int f0 = frame;
            GumInput.Override = i => i == 0 ? new Pad { jumpDown = (frame % 2 == 0) } : new Pad();
            while (p.frozenT > 0f && tf < 4f) { tf += Time.unscaledDeltaTime; frame++; yield return null; }
            Log("thaw by mashing took " + tf.ToString("0.00") + "s");
            Assert.Less(tf, 1.4f, "mashing buttons must thaw much faster than waiting"); Assert.Greater(p.freezeImmune, 0f);
            GumInput.Override = i => new Pad();

            // ---- 3) brittle ledges break after ~0.85s and come back
            Plat brittle = null; foreach (var pl in G.world.plats) if (pl.kind == PlatKind.Brittle) { brittle = pl; break; }
            Assert.NotNull(brittle, "level 1 of ice has brittle icebergs");
            G.god = true; p.Place(new Vector2(brittle.CenterX, brittle.top + 0.02f));
            yield return Seconds(1.4f); Assert.IsFalse(brittle.active, "brittle ice should have broken");
            yield return Seconds(5f); Assert.IsTrue(brittle.active, "brittle ice should return");
            Log("brittle ledge broke and returned");

            // ---- 4) the penguin's shield blocks gum from the front only
            G.autoFire = false;                                   // the streaming gum would shoot the penguin we are measuring
            var peng = G.SpawnEnemy("penguin", "", new Vector2(4f, G.world.GroundTop + 0.02f)); peng.facing = 1;
            yield return Seconds(0.7f);
            var at = new Vector2(peng.body.pos.x + 1.5f, peng.body.pos.y + 0.6f);
            Assert.IsTrue(peng.Blocks(at, new Vector2(-14f, 0f)), "gum from the front must be blocked");
            Assert.IsFalse(peng.Blocks(new Vector2(peng.body.pos.x - 1.5f, peng.body.pos.y + 0.6f), new Vector2(14f, 0f)), "gum from behind must pass");
            Log("penguin shield ok");

            // ---- 5) the elevator carries its rider
            yield return StartAt(6, 1); p = G.players[0]; G.god = true;
            Plat lift = null; foreach (var pl in G.world.plats) if (pl.kind == PlatKind.Moving) { lift = pl; break; }
            Assert.NotNull(lift);
            p.Place(new Vector2(lift.CenterX, lift.top + 0.02f)); yield return Frames(3);
            float y0 = p.body.pos.y; float seen = 0f;
            for (int k = 0; k < 70; k++) { frame++; yield return null; seen = Mathf.Max(seen, Mathf.Abs(p.body.pos.y - y0)); Assert.Less(Mathf.Abs(p.body.pos.y - lift.top), 0.15f, "rider must stay on the elevator"); }
            Log("elevator carried the player " + seen.ToString("0.00") + "u");
            Assert.Greater(seen, 0.4f);
            Shot("x2_village");

            // ---- 6) elastic pad launches you; resonant crystal builds the bridge
            yield return StartAt(7, 1); p = G.players[0]; G.god = true;
            Plat spring = null, bridge = null;
            foreach (var pl in G.world.plats) { if (pl.kind == PlatKind.Elastic && spring == null) spring = pl; if (pl.kind == PlatKind.Bridge) bridge = pl; }
            Assert.NotNull(spring); Assert.NotNull(bridge); Assert.IsFalse(bridge.active, "bridge starts hidden");
            p.Place(new Vector2(spring.CenterX, spring.top + 2.8f)); float vmax = 0f;
            for (int k = 0; k < 90; k++) { frame++; yield return null; vmax = Mathf.Max(vmax, p.body.vel.y); }
            Log("elastic launch speed " + vmax.ToString("0.0"));
            Assert.Greater(vmax, 15f, "an elastic pad must bounce hard");
            Assert.AreEqual(2, G.crystals.Count);
            for (int k = 0; k < 3; k++) { G.crystals[0].Hit(1f); yield return Frames(2); }
            Assert.IsTrue(bridge.active, "three quick hits make the crystal sing and raise the bridge");
            yield return Seconds(0.3f); Shot("x3_crystals");
            Log("crystals raised the bridge");

            // ---- 7) blizzard: pushes open ground, spares sheltered players
            yield return StartAt(8, 2); G.god = true;
            var a = G.players[0]; var b = G.players[1];
            a.Place(new Vector2(-3.25f, G.world.GroundTop + 0.02f));     // the open gap between the ledges b.Place(new Vector2(0f, G.world.GroundTop + 0.02f));
            yield return Frames(3);
            G.wind.Reset(false);
            float ax0 = a.body.pos.x, bx0 = b.body.pos.x;
            G.TriggerGust(1); yield return Seconds(1.0f); Shot("x4_wind_warning");
            yield return Seconds(1.2f);
            float da = a.body.pos.x - ax0, db = b.body.pos.x - bx0;
            Log("wind pushed open player " + da.ToString("0.00") + "u, sheltered player " + db.ToString("0.00") + "u");
            Assert.Greater(da, 0.5f, "an exposed player must be blown along until a roof stops the wind"); Assert.Less(Mathf.Abs(db), 0.3f, "a sheltered player must not move");
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ ice boss: only the sun can be hurt
        [UnityTest, Timeout(600000)]
        public IEnumerator MonarchWeakPoint()
        {
            yield return Boot();
            GumInput.Override = i => new Pad();
            G.holdClear = true; G.god = true;
            G.DebugStartAt(9, 1); yield return Frames(2); G.DebugSkipBanner(); yield return UntilMode(Mode.Play, 4f);
            var boss = G.boss as IceBoss; Assert.NotNull(boss); boss.forceClosed = true;
            for (int ph = 1; ph <= 3; ph++)
            {
                yield return Seconds(1.2f);
                var sun = boss.Hurtboxes()[0]; var body = boss.BlockBoxes()[0];
                Assert.IsTrue(body.Overlaps(sun), "phase " + ph + ": the sun must be inside the shell");
                Assert.Less(sun.width * sun.height, body.width * body.height * 0.25f, "the weak point must be small");
                // gum on the shell: no damage
                int before = boss.hp;
                var shell = new PlayerShot(G.players[0], new Vector2(body.xMin + 0.2f, body.yMin + 0.3f), new Vector2(0, 0.01f), "fwd"); G.shots.Add(shell);
                yield return Frames(3); Assert.AreEqual(before, boss.hp, "gum on the shell must not hurt"); Assert.IsTrue(shell.dead, "gum is stopped by the shell");
                // gum on the sun: damage
                var sunPos = boss.SunPos();
                var hit = new PlayerShot(G.players[0], sunPos, new Vector2(0, 0.01f), "fwd"); G.shots.Add(hit);
                yield return Frames(3); Assert.Less(boss.hp, before, "gum on the sun must hurt");
                boss.forceClosed = false; boss.openT = 3f; Assert.AreEqual(0, boss.BlockBoxes().Length, "an open shell must not block gum"); boss.forceClosed = true;
                Shot("b" + ph + "_sun");
                Log("phase " + ph + ": sun at " + sunPos + " hurtbox " + sun.width.ToString("0.0") + "u inside shell " + body.width.ToString("0.0") + "x" + body.height.ToString("0.0"));
                if (ph == 1) { G.DebugDamageBoss(boss.hp - (IceBoss.Phase2Hp + IceBoss.Phase3Hp) + 1); yield return Seconds(2.4f); }
                if (ph == 2) { G.DebugDamageBoss(boss.hp - IceBoss.Phase3Hp + 1); yield return Seconds(2.4f); }
            }
            Assert.AreEqual(3, boss.phase);
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ music
        static void WriteWav(string path, float[] interleaved, int sampleRate)
        {
            using (var fs = new FileStream(path, FileMode.Create))
            using (var w = new BinaryWriter(fs))
            {
                int n = interleaved.Length;
                w.Write(Encoding.ASCII.GetBytes("RIFF")); w.Write(36 + n * 2); w.Write(Encoding.ASCII.GetBytes("WAVEfmt "));
                w.Write(16); w.Write((short)1); w.Write((short)2); w.Write(sampleRate); w.Write(sampleRate * 4); w.Write((short)4); w.Write((short)16);
                w.Write(Encoding.ASCII.GetBytes("data")); w.Write(n * 2);
                for (int i = 0; i < n; i++) w.Write((short)(Mathf.Clamp(interleaved[i], -1f, 1f) * 32767f));
            }
        }

        [Test, Timeout(600000)]
        public void EveryTrackRendersCleanly()
        {
            string dir = Environment.GetEnvironmentVariable("GUMREMIX_AUDIO");
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            var sb = new StringBuilder();
            foreach (Track t in Enum.GetValues(typeof(Track)))
            {
                var song = SongBook.Get(t); int frames;
                var sw = System.Diagnostics.Stopwatch.StartNew();
                var d = MusicRenderer.Render(song, out frames);
                sw.Stop();
                float peak = 0f; double acc = 0; bool bad = false;
                for (int i = 0; i < d.Length; i++) { float v = d[i]; if (float.IsNaN(v) || float.IsInfinity(v)) bad = true; float a = Mathf.Abs(v); if (a > peak) peak = a; acc += v * v; }
                float rms = Mathf.Sqrt((float)(acc / d.Length)); float seconds = frames / (float)MusicRenderer.SR;
                Assert.IsFalse(bad, t + ": NaN/Infinity in the audio");
                Assert.LessOrEqual(peak, 0.81f, t + ": the limiter must keep the peak under control");
                Assert.That(rms, Is.InRange(0.07f, 0.19f), t + ": loudness out of range");
                if (song.loop)
                {
                    Assert.That(seconds, Is.InRange(25f, 95f), t + ": loop length");
                    float seam = Mathf.Abs(d[0] - d[d.Length - 2]) + Mathf.Abs(d[1] - d[d.Length - 1]);
                    Assert.Less(seam, 0.35f, t + ": the loop must close without a click (" + seam.ToString("0.000") + ")");
                }
                else Assert.That(seconds, Is.InRange(4f, 25f), t + ": jingle length");
                sb.AppendLine(string.Format("{0,-13} {1,-30} {2,5:0} bpm  {3,5:0.0}s  rms {4:0.000}  peak {5:0.00}  render {6} ms", t, song.name, song.bpm, seconds, rms, peak, sw.ElapsedMilliseconds));
                if (!string.IsNullOrEmpty(dir)) WriteWav(Path.Combine(dir, ((int)t).ToString("00") + "_" + t + ".wav"), d, MusicRenderer.SR);
            }
            Log("music\n" + sb);
            var outFile = Environment.GetEnvironmentVariable("GUMREMIX_SHOTS");
            if (!string.IsNullOrEmpty(outFile)) { Directory.CreateDirectory(outFile); File.WriteAllText(Path.Combine(outFile, "music_report.txt"), sb.ToString()); }
        }

        [UnityTest, Timeout(120000)]
        public IEnumerator MusicPlaysAndSwitches()
        {
            yield return Boot();
            Assert.NotNull(Sfx.Host, "the music host must exist");
            float t = 0; while (!(Sfx.Host.Current.HasValue && Sfx.Host.Current.Value == Track.Title) && t < 20f) { t += Time.unscaledDeltaTime; yield return null; }
            Assert.AreEqual(Track.Title, Sfx.Host.Current, "the title music should start once rendered (took " + t.ToString("0.0") + "s)");
            Sfx.PlayTrack(Track.Lago, 0.2f); t = 0;
            while (!(Sfx.Host.Current.HasValue && Sfx.Host.Current.Value == Track.Lago) && t < 20f) { t += Time.unscaledDeltaTime; yield return null; }
            Assert.AreEqual(Track.Lago, Sfx.Host.Current, "switching tracks must work (took " + t.ToString("0.0") + "s)");
            Log("music host ok");
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ better together / solo
        [UnityTest, Timeout(300000)]
        public IEnumerator BetterTogetherAndSolo()
        {
            yield return Boot();
            G.holdClear = true; G.god = true; G.autoFire = true;
            G.DebugStartAt(0, 1); yield return Frames(2);
            Assert.AreEqual(Mode.Banner, G.mode);
            var p0 = G.players[0]; var p1 = G.players[1];
            Assert.IsTrue(p0.joined && p1.joined, "solo still puts both heroes on stage");
            float x0 = p0.body.pos.x;
            GumInput.Override = i => { var pd = new Pad(); if (i == 0) pd.x = 1f; return pd; };
            yield return Seconds(0.5f);
            Assert.Greater(p0.body.pos.x, x0 + 0.5f, "heroes must be able to move during the level banner");
            Assert.AreEqual(0, G.enemies.Count, "enemies wait until the banner ends");
            GumInput.Override = null;
            G.DebugSkipBanner(); yield return UntilMode(Mode.Play, 4f);
            // spawn throttle: never more than maxAlive at once
            int peak = 0; float t = 0; while (t < 8f) { t += Time.unscaledDeltaTime; frame++; int live = 0; foreach (var e in G.enemies) if (e.state != EState.Dead) live++; peak = Mathf.Max(peak, live); yield return null; }
            Assert.LessOrEqual(peak, G.world.def.maxAlive, "too many enemies on screen at once");
            // auto fire: an enemy in front gets shot without pressing anything
            G.DebugClearEnemies(); yield return Frames(2);
            p0.Place(new Vector2(-3f, G.world.GroundTop + 0.02f)); p1.Place(new Vector2(4f, G.world.GroundTop + 0.02f));
            G.SpawnEnemy("bean", "yellow", new Vector2(2f, G.world.GroundTop + 0.2f));
            int stuck0 = G.statStuck; yield return Seconds(2.5f);
            Assert.Greater(G.statStuck, stuck0, "auto-fire should have stuck the enemy without any button");
            // head bounce: falling on the partner springs you up
            G.DebugClearEnemies(); p1.Place(new Vector2(-6f, G.world.GroundTop + 0.02f)); p0.Place(new Vector2(-6f, G.world.GroundTop + 1.8f));
            yield return Seconds(1.2f);
            Assert.Greater(G.statCoopBounces, 0, "landing on the partner must bounce");
            int listeners = 0; foreach (var l in UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None)) if (l.enabled) listeners++;
            Assert.AreEqual(1, listeners, "exactly one AudioListener must be active or there is no sound");
            Assert.IsTrue(Camera.main != null || Camera.allCamerasCount > 0, "something must render to the screen");
            p0.Place(new Vector2(0f, G.world.GroundTop + 0.02f)); p1.Place(new Vector2(2f, G.world.GroundTop + 0.02f)); yield return Frames(2);
            Assert.IsTrue(p0.Together, "heroes close together charge each other's gum");
            Log("better together ok: bounces=" + G.statCoopBounces + " peakAlive=" + peak);
            yield return Teardown();
        }

        // ------------------------------------------------------------------------------------------ the scene asset itself
        [UnityTest, Timeout(120000)]
        public IEnumerator SceneAssetBoots()
        {
#if UNITY_EDITOR
            var sc = UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode("Assets/GumRemix/0_0_Scenes/GumRemix.unity", new UnityEngine.SceneManagement.LoadSceneParameters(UnityEngine.SceneManagement.LoadSceneMode.Single));
            float guard = 0; while (!sc.isLoaded && guard < 20f) { guard += Time.unscaledDeltaTime; yield return null; }
            yield return null; yield return null;
            Assert.NotNull(GameCtl.I, "the scene asset did not start the game (missing script reference?)");
            Assert.AreEqual(Mode.Title, GameCtl.I.mode);
            outDir = Environment.GetEnvironmentVariable("GUMREMIX_SHOTS");
            if (string.IsNullOrEmpty(outDir)) outDir = Path.Combine(Application.temporaryCachePath, "gumshots");
            Directory.CreateDirectory(outDir); G = GameCtl.I;
            yield return Seconds(0.8f); Shot("scene_title");
#else
            yield break;
#endif
        }

        int CountJoined() { int n = 0; foreach (var p in G.players) if (p.joined) n++; return n; }
    }
}
