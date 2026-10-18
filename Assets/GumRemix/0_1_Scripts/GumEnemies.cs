using System;
using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    public enum EState { Normal, Stuck, Rolling, Dead }

    /// <summary>Enemy projectile. In the candy world: dark bubbles. In the ice world: frost shards (harmful, they freeze).</summary>
    public class EnemyShot
    {
        public Vector2 pos, vel; public float grav, radius = 0.28f, life = 7f; public bool dead, persistent, bounce, freezes; public int bounces;
        public Action<Vector2> onLand;
        GameObject go; SpriteRenderer sr; Anim anim; string kind, sheetName; float scale; bool ice;

        public EnemyShot(Vector2 p, Vector2 v, float g, bool pink, float sc = 1f, bool bouncy = false, string sheet = null, string clip = null, float rad = 0.28f)
        {
            pink = false;      // parry is gone: there are no pink shots any more
            pos = p; vel = v; grav = g; scale = sc; radius = rad * sc; bounce = bouncy;
            ice = GameCtl.I.world != null && GameCtl.I.world.def.ice;
            if (sheet == null) sheet = ice ? (pink ? "ice_flake" : "ice_shard") : "bubbles";
            if (clip == null) clip = ice ? "spin" : (pink ? "pink" : "dark");
            kind = pink ? "pink" : "dark"; sheetName = sheet;
            freezes = ice && !pink && sheet == "ice_shard";
            if (ice && sheet != "bubbles") radius = (sheet == "snowhead" ? 0.4f : 0.26f) * sc;
            var sh = Art.Get(sheet);
            sr = Pix.Make("eshot", GameCtl.I.entRoot, sh[0], p, 18, sc);
            go = sr.gameObject; anim = new Anim(sr, sh, clip);
        }

        public bool Hits(Rect r)
        {
            float cx = Mathf.Clamp(pos.x, r.xMin, r.xMax), cy = Mathf.Clamp(pos.y, r.yMin, r.yMax);
            return (pos.x - cx) * (pos.x - cx) + (pos.y - cy) * (pos.y - cy) < radius * radius;
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            vel.y -= grav * dt; pos += vel * dt; life -= dt; anim.Tick(dt);
            if (pos.y - radius < g.world.GroundTop)
            {
                if (bounce && bounces < 4) { pos.y = g.world.GroundTop + radius; vel.y = Mathf.Abs(vel.y) * 0.82f; bounces++; if (vel.y < 3f) vel.y = 5f; }
                else { Land(); return; }
            }
            if (bounce) { if (pos.x < Levels.Left + radius) { pos.x = Levels.Left + radius; vel.x = Mathf.Abs(vel.x); } if (pos.x > Levels.Right - radius) { pos.x = Levels.Right - radius; vel.x = -Mathf.Abs(vel.x); } }
            else if (pos.x < Levels.Left - 1f || pos.x > Levels.Right + 1f || pos.y > g.world.y0 + Levels.Height + 2f) { dead = true; UnityEngine.Object.Destroy(go); return; }
            if (life <= 0) { Land(); return; }
            go.transform.position = pos;
        }

        void Land() { var p = pos; var cb = onLand; Pop(); if (cb != null) cb(p); }

        public void Pop()
        {
            if (dead) return;
            dead = true; UnityEngine.Object.Destroy(go);
            if (ice) GameCtl.I.SpawnFx("spark_ice", "spark", pos, 1.6f, false, 26);
            else GameCtl.I.SpawnFx("bubble_pops", kind, pos, scale, false, 26);
            Sfx.Play("pop", .35f, ice ? 1.8f : 1.3f);
        }

    }

    /// <summary>Ground shock wave (boss slam). Jump over it.</summary>
    public class Shockwave
    {
        public bool dead; public float x; int dir; float life = 2.4f; GameObject go; SpriteRenderer sr; Anim anim; float y;
        public Shockwave(float px, int d, float groundY, string sheet = "shock")
        {
            x = px; dir = d; y = groundY;
            var sh = Art.Get(sheet);
            sr = Pix.Make("shock", GameCtl.I.entRoot, sh[0], new Vector3(px, groundY, 0), 17);
            go = sr.gameObject; sr.flipX = d < 0; anim = new Anim(sr, sh, "shock");
        }
        public bool Hits(Rect r) { return new Rect(x - 0.55f, y, 1.1f, 0.55f).Overlaps(r); }
        public void Tick(float dt)
        {
            x += dir * 5.0f * dt; life -= dt; anim.Tick(dt); if (anim.done) anim.Play("shock", true);
            go.transform.position = new Vector3(x, y, 0);
            if (life <= 0 || x < Levels.Left - 1f || x > Levels.Right + 1f) { dead = true; UnityEngine.Object.Destroy(go); }
        }
    }

    public class Enemy
    {
        public string kind, color; public int hp, maxHp; public EState state = EState.Normal; public Body body = new Body();
        public int facing = 1; public float kickCd; public bool remove;
        GameObject go; SpriteRenderer sr; Anim anim; Sheet sMain, sBall;
        float flashT, hurtT, stuckT, rollT, t, thinkT, actT, shootT, lungeCd, spawnT = 0.5f, blockT, vulnT, headlessT; int bounces, rollDir, mode, shots, shellHits;
        float baseY; bool manualFrame; Vector2 spriteOffset; float hw, hh, hhFull; Player stuckBy; float blinkT; bool thrown; Vector2 diveTarget;

        GameCtl G { get { return GameCtl.I; } }
        public bool IsFlyer { get { return kind == "lolli" || kind == "owl" || kind == "jelly"; } }

        public Rect Hurtbox
        {
            get
            {
                if (state == EState.Stuck || state == EState.Rolling) return new Rect(body.pos.x - 0.4f, body.pos.y, 0.8f, 0.8f);
                return new Rect(body.pos.x - hw, body.pos.y, hw * 2f, hh);
            }
        }
        public bool Hittable { get { return state == EState.Normal && spawnT <= 0f; } }
        /// <summary>Plated enemies shrug off gum: only the string between the heroes can wrap them into a ball.</summary>
        public bool Armored { get { return color == "steel" || color == "iron"; } }
        float wrapT; bool wrapTouched;

        float deflectT;
        public void Deflect(Vector2 at, Vector2 shotDir)
        {
            G.Ricochet(at, shotDir); flashT = 0.08f; deflectT = 0.14f;       // white flash + a little rattle
        }

        public void Wrap(float dt, Player by)
        {
            if (!Hittable) return;
            wrapT += dt; wrapTouched = true; flashT = Mathf.Max(flashT, 0.02f);
            if (((int)(wrapT * 12f)) != ((int)((wrapT - dt) * 12f))) G.Burst(body.Center, 1, 2.5f);
            if (wrapT >= 0.65f) { wrapT = 0f; Stick(by); G.statWraps++; }
        }

        public Enemy(string k, string c, Vector2 pos)
        {
            kind = k; color = c;
            switch (k)
            {
                case "bear": sMain = Art.Get("bear_" + c); sBall = Art.Get("ball_bear_" + c); hw = 0.4f; hh = 1.0f; hp = 3; break;
                case "bean": sMain = Art.Get("bean_" + c); sBall = Art.Get("ball_bean_" + c); hw = 0.36f; hh = 0.8f; hp = 2; break;
                case "lolli": sMain = Art.Get("lolli"); sBall = Art.Get("ball_lolli"); hw = 0.4f; hh = 0.9f; hp = 2; spriteOffset = new Vector2(0, 0.45f); break;
                case "cannon": sMain = Art.Get("cannon"); sBall = Art.Get("ball_cannon"); hw = 0.45f; hh = 0.9f; hp = 4; break;
                case "penguin": sMain = Art.Get(c == "iron" ? "penguin_iron" : "penguin"); sBall = Art.Get("ball_penguin"); hw = 0.42f; hh = 1.05f; hp = 3; break;
                case "snowman": sMain = Art.Get(c == "iron" ? "snowman_iron" : "snowman"); sBall = Art.Get(c == "iron" ? "ball_snowman_iron" : "ball_snowman"); hw = 0.4f; hh = 1.35f; hp = 3; break;
                case "owl": sMain = Art.Get(c == "iron" ? "owl_iron" : "owl"); sBall = Art.Get("ball_owl"); hw = 0.45f; hh = 0.95f; hp = 2; spriteOffset = new Vector2(0, 0.42f); break;
                case "jelly": sMain = Art.Get("jelly"); sBall = Art.Get("ball_jelly"); hw = 0.55f; hh = 1.25f; hp = 3; spriteOffset = new Vector2(0, 0.62f); break;
                case "crab": sMain = Art.Get("crab"); sBall = Art.Get("ball_crab"); hw = 0.7f; hh = 0.85f; hp = 6; break;
                default: sMain = Art.Get("seed"); sBall = Art.Get("ball_seed"); hw = 0.24f; hh = 0.55f; hp = 1; break;   // seed
            }
            hhFull = hh; maxHp = hp; body.w = hw * 2f; body.h = hh;
            facing = UnityEngine.Random.value < .5f ? 1 : -1;
            if (IsFlyer) { body.pos = new Vector2(pos.x, pos.y - hh * 0.5f); baseY = body.pos.y; } else body.pos = pos;
            sr = Pix.Make(k, G.entRoot, sMain[0], pos, 6);
            go = sr.gameObject;
            string first = IsFlyer ? (kind == "jelly" ? "float" : "fly") : (kind == "penguin" ? "idle" : "idle");
            anim = new Anim(sr, sMain, first);
            thinkT = UnityEngine.Random.Range(0.4f, 1.4f); shootT = UnityEngine.Random.Range(1.4f, 2.8f); lungeCd = UnityEngine.Random.Range(0.8f, 1.8f); t = UnityEngine.Random.value * 5f;
            G.SpawnFx("spark", "spark", body.Center, 1.2f, false, 30);
            if (Armored) G.HintArmor();
        }

        public void Tick(float dt)
        {
            t += dt; kickCd -= dt; flashT -= dt; hurtT -= dt; spawnT -= dt; blockT -= dt; vulnT -= dt;
            if (!wrapTouched) wrapT = Mathf.Max(0f, wrapT - dt * 1.2f);
            wrapTouched = false;
            sr.color = wrapT > 0f ? Color.Lerp(Color.white, new Color(1f, .62f, .86f), Mathf.Clamp01(wrapT / 0.65f) * 0.8f) : Color.white;
            anim.flash = flashT > 0f;
            switch (state)
            {
                case EState.Normal: TickNormal(dt); break;
                case EState.Stuck: TickStuck(dt); break;
                case EState.Rolling: TickRolling(dt); break;
                case EState.Dead: anim.Tick(dt); if (anim.done) { remove = true; UnityEngine.Object.Destroy(go); return; } break;
            }
            if (remove) return;
            deflectT -= dt;
            go.transform.position = (Vector3)(body.pos + (state == EState.Normal ? spriteOffset : Vector2.zero) + (deflectT > 0f ? new Vector2(Mathf.Sin(t * 90f) * 0.06f, 0f) : Vector2.zero));
            sr.flipX = state == EState.Rolling ? rollDir < 0 : facing < 0;
            float a = 1f;
            if (spawnT > 0) { blinkT += dt; a = ((int)(blinkT * 20)) % 2 == 0 ? 1f : 0.3f; }
            sr.color = new Color(1, 1, 1, a);
            if (!manualFrame) anim.Tick(dt);
        }

        void Gravity(float dt) { body.vel.y = Mathf.Max(-21f, body.vel.y - 38f * dt); }

        // ------------------------------------------------------------------------------------------ behaviours
        void TickNormal(float dt)
        {
            manualFrame = false;
            switch (kind)
            {
                case "bear": BearAI(dt); break;
                case "bean": BeanAI(dt); break;
                case "lolli": LolliAI(dt); break;
                case "cannon": CannonAI(dt); break;
                case "penguin": PenguinAI(dt); break;
                case "snowman": SnowmanAI(dt); break;
                case "owl": OwlAI(dt); break;
                case "jelly": JellyAI(dt); break;
                case "crab": CrabAI(dt); break;
                default: SeedAI(dt); break;
            }
        }

        bool EdgeAhead() { return float.IsNaN(G.world.SupportBelow(body.pos.x + facing * 0.5f, body.pos.y + 0.1f, 0.35f)); }

        void Patrol(float dt, float speed)
        {
            body.vel.x = facing * speed;
            body.Step(dt, G.world, G.time);
            if (body.grounded && (EdgeAhead() || (facing > 0 && body.hitR) || (facing < 0 && body.hitL))) { facing = -facing; body.vel.x = 0; }
        }

        void BearAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0) { body.vel.x = 0; anim.Play("hurt"); body.Step(dt, G.world, G.time); return; }
            if (mode == 1)
            {
                actT += dt; anim.Play("lunge");
                body.vel.x = (actT > 0.12f && actT < 0.3f) ? facing * 5.6f : 0f;
                if (actT > 0.36f) { mode = 0; lungeCd = UnityEngine.Random.Range(1.6f, 2.8f); anim.Play("walk", true); }
            }
            else
            {
                anim.Play("walk"); body.vel.x = facing * 1.5f; lungeCd -= dt;
                var p = G.NearestPlayer(body.Center);
                if (p != null && lungeCd <= 0 && Mathf.Abs(p.Center.y - body.Center.y) < 1.1f)
                {
                    float dx = p.Center.x - body.pos.x;
                    if (Mathf.Abs(dx) < 3f && Mathf.Sign(dx) == facing) { mode = 1; actT = 0; anim.Play("lunge", true); Sfx.Play("spit", .3f, 0.7f); }
                }
            }
            body.Step(dt, G.world, G.time);
            if (body.grounded && (EdgeAhead() || (facing > 0 && body.hitR) || (facing < 0 && body.hitL))) { facing = -facing; body.vel.x = 0; }
        }

        void BeanAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0) { anim.Play("hurt"); body.vel.x = 0; body.Step(dt, G.world, G.time); return; }
            if (body.grounded)
            {
                body.vel.x = 0;
                if (mode == 2) { anim.Play("land"); actT -= dt; if (actT <= 0) { mode = 0; thinkT = UnityEngine.Random.Range(0.5f, 1.2f); anim.Play("idle", true); } }
                else if (mode == 1)
                {
                    anim.Play("crouch"); actT -= dt;
                    if (actT <= 0)
                    {
                        var p = G.NearestPlayer(body.Center);
                        float dir = facing; float vy = 10f;
                        if (p != null) { dir = Mathf.Sign(p.Center.x - body.pos.x); if (Mathf.Abs(p.Center.x - body.pos.x) < 0.3f) dir = facing; if (p.body.pos.y > body.pos.y + 1.4f) vy = 14.4f; else if (p.body.pos.y < body.pos.y - 1.4f) vy = 6f; }
                        facing = (int)dir; body.vel = new Vector2(dir * 3.4f, vy); body.grounded = false; mode = 3; manualFrame = true;
                        Sfx.Play("jump", .25f, 1.6f);
                    }
                }
                else { anim.Play("idle"); thinkT -= dt; if (thinkT <= 0) { mode = 1; actT = 0.16f; anim.Play("crouch", true); } }
            }
            else { mode = 3; manualFrame = true; anim.Play("air"); anim.Apply(body.vel.y > 3f ? 0 : (body.vel.y > -3f ? 1 : 2)); }
            body.Step(dt, G.world, G.time);
            if (mode == 3 && body.grounded) { mode = 2; actT = 0.18f; anim.Play("land", true); manualFrame = false; }
            if (!body.grounded && body.hitL) { facing = 1; body.vel.x = Mathf.Abs(body.vel.x); }
            if (!body.grounded && body.hitR) { facing = -1; body.vel.x = -Mathf.Abs(body.vel.x); }
        }

        void LolliAI(float dt)
        {
            anim.Play(hurtT > 0 ? "hurt" : "fly");
            body.pos.x += facing * 2.1f * dt;
            if (body.pos.x > Levels.Right - 0.6f) facing = -1;
            if (body.pos.x < Levels.Left + 0.6f) facing = 1;
            body.pos.y = baseY + Mathf.Sin(t * 2.2f) * 0.5f;
            shootT -= dt;
            if (shootT < 0.25f && shootT > 0f) flashT = Mathf.Max(flashT, 0.05f);
            if (shootT <= 0)
            {
                shootT = UnityEngine.Random.Range(2.0f, 3.2f);
                G.eshots.Add(new EnemyShot(body.Center + new Vector2(0, -0.4f), new Vector2(facing * 0.8f, 0f), 9f, true));
                Sfx.Play("spit", .3f, 1.4f);
            }
        }

        void CannonAI(float dt)
        {
            Gravity(dt); body.vel.x = 0;
            body.Step(dt, G.world, G.time);
            var p = G.NearestPlayer(body.Center);
            if (p != null && mode == 0) facing = p.Center.x >= body.pos.x ? 1 : -1;
            if (hurtT > 0 && mode == 0) { anim.Play("hurt"); return; }
            if (mode == 0)
            {
                anim.Play("idle"); shootT -= dt;
                if (shootT <= 0 && p != null) { mode = 1; actT = 0; anim.Play("fire", true); Sfx.Play("warn", .4f); }
            }
            else
            {
                actT += dt;
                if (p == null) { mode = 0; shootT = 1f; anim.Play("idle", true); return; }
                if (actT > 0.12f && mode == 1)
                {
                    mode = 2; shots++;
                    var m = body.pos + new Vector2(0.235f * facing, 0.95f);
                    float dx = p.Center.x - m.x, dy = p.Center.y - m.y;
                    float T = Mathf.Clamp(Mathf.Abs(dx) / 6f, 0.7f, 1.5f); const float g = 11f;
                    var v = new Vector2(dx / T, (dy + 0.5f * g * T * T) / T);
                    G.eshots.Add(new EnemyShot(m, v, g, shots % 3 == 0, 1.1f));
                    Sfx.Play("boom", .35f, 1.7f); G.Shake(.04f, .1f);
                }
                if (anim.done) { mode = 0; shootT = UnityEngine.Random.Range(2.2f, 3.2f); anim.Play("idle", true); }
            }
        }

        // ---------------------------------------------------------------- ice creatures
        void PenguinAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0) { body.vel.x = 0; anim.Play("hurt"); body.Step(dt, G.world, G.time); return; }
            if (mode == 1)      // belly slide, shield first
            {
                actT += dt; anim.Play("slide");
                float sp = body.groundIce ? 8.6f : 7.2f;
                body.vel.x = facing * sp * Mathf.Clamp01(1.2f - actT / 0.9f);
                body.h = 0.7f; hh = 0.7f;
                body.Step(dt, G.world, G.time);
                if (actT > 0.85f || (body.grounded && EdgeAhead()) || (facing > 0 && body.hitR) || (facing < 0 && body.hitL))
                { mode = 0; body.h = hhFull; hh = hhFull; lungeCd = UnityEngine.Random.Range(2.0f, 3.4f); anim.Play("waddle", true); }
                return;
            }
            anim.Play(blockT > 0f ? "block" : "waddle");
            lungeCd -= dt;
            var p = G.NearestPlayer(body.Center);
            if (p != null && lungeCd <= 0 && Mathf.Abs(p.Center.y - body.Center.y) < 1.2f)
            {
                float dx = p.Center.x - body.pos.x;
                if (Mathf.Abs(dx) < 4.5f && Mathf.Sign(dx) == facing) { mode = 1; actT = 0; anim.Play("slide", true); Sfx.Play("dash", .35f, 0.8f); return; }
            }
            Patrol(dt, 1.4f);
        }

        /// <summary>The penguin's shield stops gum that arrives from its front.</summary>
        public bool Blocks(Vector2 shotPos, Vector2 shotVel)
        {
            if (kind != "penguin" || state != EState.Normal || spawnT > 0f || hurtT > 0f) return false;
            if (Mathf.Abs(shotVel.x) < 0.5f || Mathf.Sign(shotVel.x) == facing) return false;       // must be flying toward its face
            return shotPos.y > body.pos.y + 0.05f && shotPos.y < body.pos.y + 1.15f;
        }

        public void OnBlocked(Vector2 at)
        {
            blockT = 0.28f; anim.Play("block", true);
            Sfx.Play("hit", .5f, 1.9f); G.SpawnFx("spark_ice", "spark", at, 1.4f, false, 31);
        }

        void SnowmanAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0) { anim.Play("hurt"); body.vel.x = 0; body.Step(dt, G.world, G.time); return; }
            // 4 = headless rampage, 5 = regrow
            if (mode == 4)
            {
                headlessT -= dt; var p = G.NearestPlayer(body.Center);
                anim.Play("run_nohead");
                if (p != null) facing = p.Center.x >= body.pos.x ? 1 : -1;
                body.vel.x = facing * 3.6f;
                body.Step(dt, G.world, G.time);
                if (body.grounded && EdgeAhead()) body.vel.x = 0;
                if (headlessT <= 0) { mode = 5; actT = 0; anim.Play("regrow", true); body.vel.x = 0; }
                return;
            }
            if (mode == 5) { actT += dt; body.vel.x = 0; body.Step(dt, G.world, G.time); if (anim.done) { mode = 0; thrown = false; shootT = UnityEngine.Random.Range(3.5f, 5.0f); anim.Play("idle", true); } return; }
            if (mode == 3)      // throwing its head
            {
                actT += dt; body.vel.x = 0; body.Step(dt, G.world, G.time);
                if (!thrown && actT > 0.3f) { thrown = true; ThrowHead(); }
                if (anim.done) { mode = 4; headlessT = 3.4f; anim.Play("run_nohead", true); }
                return;
            }
            if (body.grounded)
            {
                body.vel.x = 0;
                if (mode == 2) { actT -= dt; anim.Play("idle"); if (actT <= 0) { mode = 0; thinkT = UnityEngine.Random.Range(0.6f, 1.3f); } }
                else if (mode == 1)
                {
                    actT -= dt;
                    if (actT <= 0)
                    {
                        var p = G.NearestPlayer(body.Center); float dir = facing; float vy = 9.6f;
                        if (p != null) { dir = Mathf.Sign(p.Center.x - body.pos.x); if (p.body.pos.y > body.pos.y + 1.4f) vy = 13.8f; }
                        facing = (int)dir; body.vel = new Vector2(dir * 2.4f, vy); body.grounded = false; mode = 6; Sfx.Play("jump", .22f, 1.2f);
                    }
                }
                else
                {
                    anim.Play("idle"); thinkT -= dt; shootT -= dt;
                    var p = G.NearestPlayer(body.Center);
                    if (shootT <= 0 && p != null && Mathf.Abs(p.Center.x - body.pos.x) < 8f) { mode = 3; actT = 0; thrown = false; anim.Play("throw", true); Sfx.Play("warn", .4f); return; }
                    if (thinkT <= 0) { mode = 1; actT = 0.18f; }
                }
            }
            else { anim.Play("hop"); }
            body.Step(dt, G.world, G.time);
            if (mode == 6 && body.grounded) { mode = 2; actT = 0.2f; }
            if (!body.grounded && body.hitL) { facing = 1; body.vel.x = Mathf.Abs(body.vel.x); }
            if (!body.grounded && body.hitR) { facing = -1; body.vel.x = -Mathf.Abs(body.vel.x); }
        }

        void ThrowHead()
        {
            var p = G.NearestPlayer(body.Center); if (p == null) return;
            var m = body.pos + new Vector2(0.3f * facing, hhFull - 0.1f);
            float dx = p.Center.x - m.x, dy = p.Center.y - m.y; const float g = 13f;
            float T = Mathf.Clamp(Mathf.Abs(dx) / 6.5f, 0.6f, 1.5f);
            var v = new Vector2(dx / T, (dy + 0.5f * g * T * T) / T);
            var s = new EnemyShot(m, v, g, false, 1f, false, "snowhead", "spin");
            s.freezes = false;
            s.onLand = at => { if (G.AliveEnemies() < 9) { G.SpawnEnemy("seed", "", new Vector2(Mathf.Clamp(at.x, -7f, 7f), at.y)); } };
            G.eshots.Add(s);
            Sfx.Play("spit", .5f, 0.8f);
        }

        void OwlAI(float dt)
        {
            anim.Play(hurtT > 0 ? "hurt" : (mode == 1 || mode == 2 ? "dive" : "fly"));
            if (mode == 0)
            {
                body.pos.x += facing * 2.5f * dt;
                if (body.pos.x > Levels.Right - 0.7f) facing = -1;
                if (body.pos.x < Levels.Left + 0.7f) facing = 1;
                body.pos.y = baseY + Mathf.Sin(t * 2.4f) * 0.4f;
                shootT -= dt;
                if (shootT <= 0) { var p = G.NearestPlayer(body.Center); if (p != null) { mode = 1; actT = 0; diveTarget = p.Center; } shootT = UnityEngine.Random.Range(3.0f, 4.4f); }
            }
            else if (mode == 1)      // hover, shiver and lock on
            {
                actT += dt; body.pos.x += Mathf.Sin(actT * 60f) * 0.02f;
                if (actT < 0.5f) { var p = G.NearestPlayer(body.Center); if (p != null) diveTarget = p.Center; }
                flashT = Mathf.Max(flashT, ((int)(actT * 14) % 2 == 0) ? 0.03f : 0f);
                if (actT > 0.55f) { mode = 2; actT = 0; Sfx.Play("dash", .5f, 1.4f); }
            }
            else if (mode == 2)
            {
                actT += dt;
                var dir = (diveTarget - body.Center).normalized;
                if (dir.sqrMagnitude < .01f) dir = Vector2.down;
                body.pos += dir * 10.5f * dt;
                facing = dir.x >= 0 ? 1 : -1;
                if (actT > 1.0f || body.pos.y <= G.world.GroundTop + 0.1f || (diveTarget - body.Center).sqrMagnitude < 0.2f) { mode = 3; actT = 0; }
            }
            else
            {
                body.pos.y = Mathf.MoveTowards(body.pos.y, baseY, 5f * dt);
                body.pos.x += facing * 1.5f * dt;
                if (Mathf.Abs(body.pos.y - baseY) < 0.1f) mode = 0;
            }
            body.pos.x = Mathf.Clamp(body.pos.x, Levels.Left + 0.5f, Levels.Right - 0.5f);
        }

        void JellyAI(float dt)
        {
            if (hurtT > 0f && mode == 0) anim.Play("hurt");
            if (mode == 0)
            {
                anim.Play("float");
                body.pos.x += facing * 1.0f * dt;
                if (body.pos.x > Levels.Right - 0.9f) facing = -1;
                if (body.pos.x < Levels.Left + 0.9f) facing = 1;
                body.pos.y = baseY + Mathf.Sin(t * 1.4f) * 0.45f;
                shootT -= dt;
                var p = G.NearestPlayer(body.Center);
                if (shootT <= 0 && p != null) { mode = 1; actT = 0; anim.Play("flare", true); Sfx.Play("warn", .4f, 1.3f); }
            }
            else
            {
                actT += dt;
                if (mode == 1 && actT > 0.14f)
                {
                    mode = 2; shots++;
                    var p = G.NearestPlayer(body.Center);
                    if (p != null)
                    {
                        var origin = body.Center + new Vector2(0, -0.2f);
                        float baseAng = Mathf.Atan2(p.Center.y - origin.y, p.Center.x - origin.x);
                        for (int k = -1; k <= 1; k++)
                        {
                            bool pink = k == 0 && shots % 2 == 0;
                            float a = baseAng + k * 0.3f;
                            G.eshots.Add(new EnemyShot(origin, new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 5.0f, 0f, pink, 1f));
                        }
                        Sfx.Play("spit", .5f, 1.7f);
                    }
                }
                if (anim.done) { mode = 0; shootT = UnityEngine.Random.Range(2.8f, 3.8f); anim.Play("float", true); }
            }
        }

        void CrabAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0f && mode == 0) { body.vel.x = 0; anim.Play("hurt"); body.Step(dt, G.world, G.time); return; }
            if (mode == 1)      // tuck into the shell
            {
                actT += dt; body.vel.x = 0; anim.Play("charge");
                if (actT > 0.4f) { mode = 2; actT = 0; Sfx.Play("dash", .5f, .6f); }
                body.Step(dt, G.world, G.time); return;
            }
            if (mode == 2)      // roll as an igloo
            {
                actT += dt; anim.Play("charge");
                body.vel.x = facing * 6.4f;
                body.Step(dt, G.world, G.time);
                if (actT > 0.9f || (body.grounded && EdgeAhead()) || (facing > 0 && body.hitR) || (facing < 0 && body.hitL)) { mode = 0; vulnT = 1.8f; body.vel.x = 0; lungeCd = UnityEngine.Random.Range(2.4f, 3.6f); anim.Play("idle", true); }
                return;
            }
            anim.Play(Mathf.Abs(body.vel.x) > .1f ? "scuttle" : "idle");
            lungeCd -= dt;
            var p = G.NearestPlayer(body.Center);
            if (p != null && lungeCd <= 0 && vulnT <= 0 && Mathf.Abs(p.Center.y - body.Center.y) < 1.3f)
            {
                float dx = p.Center.x - body.pos.x;
                if (Mathf.Abs(dx) < 5f) { facing = dx >= 0 ? 1 : -1; mode = 1; actT = 0; anim.Play("charge", true); Sfx.Play("warn", .35f, .7f); return; }
            }
            if (vulnT > 0f) { body.vel.x = 0; body.Step(dt, G.world, G.time); return; }
            Patrol(dt, 1.1f);
        }

        void SeedAI(float dt)
        {
            Gravity(dt);
            if (hurtT > 0) { body.vel.x = 0; anim.Play("hurt"); body.Step(dt, G.world, G.time); return; }
            if (body.grounded)
            {
                body.vel.x = 0; thinkT -= dt; anim.Play("idle");
                if (thinkT <= 0)
                {
                    var p = G.NearestPlayer(body.Center); float dir = facing; float vy = 8.6f;
                    if (p != null) { dir = Mathf.Sign(p.Center.x - body.pos.x); if (p.body.pos.y > body.pos.y + 1.4f) vy = 13.2f; }
                    facing = (int)dir; body.vel = new Vector2(dir * 2.8f, vy); body.grounded = false; anim.Play("hop", true); thinkT = UnityEngine.Random.Range(0.8f, 1.4f);
                }
            }
            body.Step(dt, G.world, G.time);
            if (!body.grounded && body.hitL) { facing = 1; body.vel.x = Mathf.Abs(body.vel.x); }
            if (!body.grounded && body.hitR) { facing = -1; body.vel.x = -Mathf.Abs(body.vel.x); }
        }

        // ------------------------------------------------------------------------------------------ damage & gum
        public void Damage(int dmg, Player by)
        {
            if (!Hittable) return;
            if (kind == "crab" && vulnT <= 0f && mode != 1)
            {
                shellHits++;
                if (shellHits % 2 == 1) { Sfx.Play("hit", .5f, 1.9f); G.SpawnFx("spark_ice", "spark", body.Center + new Vector2(0, 0.3f), 1.3f, false, 31); flashT = 0.04f; return; }   // the shell takes every other hit
            }
            hp -= dmg; flashT = 0.07f; hurtT = (kind == "crab" || kind == "penguin") ? 0.12f : 0.2f;
            Sfx.Play("hit", .5f, 1f + UnityEngine.Random.value * .2f);
            G.SpawnFx("spark", "spark", body.Center, 0.8f, false, 30);
            if (hp <= 0) Stick(by);
        }

        public void Stick(Player by)
        {
            if (state != EState.Normal) return;
            state = EState.Stuck; stuckT = 7.5f; stuckBy = by; kickCd = 0.3f; mode = 0; manualFrame = false;
            body.vel.x = 0; body.w = 0.8f; body.h = 0.8f;
            anim.Use(sBall, "wobble", true);
            G.statStuck++; Sfx.Play("stick", .8f);
            G.SpawnFx("splat", "splat", body.Center, 1.6f, false, 30);
            G.Burst(body.Center, 6, 3f);
        }

        void TickStuck(float dt)
        {
            Gravity(dt); body.vel.x = Pix.Approach(body.vel.x, 0, 12f * dt);
            body.Step(dt, G.world, G.time);
            stuckT -= dt;
            if (stuckT < 1.8f) anim.Play("shake");
            if (stuckT <= 0f)
            {
                state = EState.Normal; hp = Mathf.Max(1, maxHp / 2); spawnT = 0.6f; hurtT = 0;
                body.w = hw * 2f; body.h = hhFull; hh = hhFull; mode = 0; vulnT = 0;
                anim.Use(sMain, IsFlyer ? (kind == "jelly" ? "float" : "fly") : "idle", true);
                if (IsFlyer) baseY = body.pos.y;
                G.SpawnFx("bubble_pops", "pink", body.Center, 1.4f, false, 30); Sfx.Play("pop", .5f, .8f);
            }
        }

        public void Kick(int dir)
        {
            if (state != EState.Stuck) return;
            state = EState.Rolling; rollDir = dir; rollT = 0; bounces = 0; kickCd = 0.15f;
            anim.Use(sBall, "roll", true);
            G.statKicks++; body.vel.y = 3f; Sfx.Play("kick", .9f);
            G.SpawnFx("dust", "dust", body.pos, 1f, false, 8);
        }

        void TickRolling(float dt)
        {
            rollT += dt;
            Gravity(dt); body.vel.x = rollDir * 9.5f;
            body.Step(dt, G.world, G.time);
            if (body.hitL || body.hitR)
            {
                rollDir = -rollDir; bounces++; Sfx.Play("hit", .6f, .6f); G.Shake(.05f, .12f);
                G.SpawnFx("spark", "spark", body.Center + new Vector2(rollDir * -0.4f, 0), 1f, false, 30);
            }
            foreach (var e in G.enemies)
            {
                if (e == this || e.state == EState.Dead || e.state == EState.Rolling) continue;
                if (!e.Hurtbox.Overlaps(Hurtbox)) continue;
                if (e.state == EState.Stuck) { e.Kick(rollDir); G.HitStop(.05f); }
                else if (e.spawnT <= 0f) { e.PopKill(); G.statBallKills++; G.HitStop(.06f); }
            }
            foreach (var c in G.crystals) if (c.Hurtbox.Overlaps(Hurtbox)) c.Hit(4f);
            if (G.boss != null && G.boss.Hittable)
                foreach (var hb in G.boss.BallBoxes())
                    if (hb.Overlaps(Hurtbox)) { G.boss.Damage(14, stuckBy); G.Shake(.15f, .3f); Pop(); return; }
            if (rollT > 5.5f || bounces >= 3 || body.pos.y < G.world.y0 - 1f) Pop();
        }

        public void PopKill()
        {
            if (state == EState.Dead) return;
            state = EState.Stuck; body.w = 0.8f; body.h = 0.8f; anim.Use(sBall, "wobble", true);
            Pop();
        }

        public void Pop()
        {
            if (state == EState.Dead) return;
            state = EState.Dead; anim.Use(sBall, "pop", true); anim.flash = false; flashT = 0; manualFrame = false;
            sr.flipX = false;
            Sfx.Play("pop", .8f);
            G.Burst(body.Center, 14, 5.5f);
            G.OnEnemyDefeated(this);
        }

        public void Dispose() { if (go != null) UnityEngine.Object.Destroy(go); }
    }
}
