using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    public class PlayerShot
    {
        public Vector2 pos, vel; public float life = 0.62f; public int dmg = 1; public bool dead; public Player owner;
        GameObject go; SpriteRenderer sr; Anim anim;

        public PlayerShot(Player o, Vector2 p, Vector2 v, string dir)
        {
            owner = o; pos = p; vel = v; if (o.Together) dmg = 2;
            var sh = Art.Get("shots");
            sr = Pix.Make("shot", GameCtl.I.entRoot, sh[0], p, 20);
            go = sr.gameObject; sr.flipX = v.x < 0;
            if (dmg > 1) { sr.color = new Color(1f, .95f, .6f); go.transform.localScale = new Vector3(1.3f, 1.3f, 1f); }       // charged by your partner
            anim = new Anim(sr, sh, dir);
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            pos += vel * dt; life -= dt; anim.Tick(dt);
            go.transform.position = pos;
            var r = new Rect(pos.x - 0.22f, pos.y - 0.22f, 0.44f, 0.44f);
            foreach (var e in g.enemies)
                if (e.Hittable && e.Hurtbox.Overlaps(r))
                {
                    if (e.Armored) { e.Deflect(pos, vel); Splat(); return; }
                    if (e.Blocks(pos, vel)) { e.OnBlocked(pos); Splat(); return; }       // the penguin's shield
                    e.Damage(dmg, owner); Splat(); return;
                }
            foreach (var c in g.crystals) if (c.Hurtbox.Overlaps(r)) { c.Hit(1f); Splat(); return; }
            if (g.boss != null && g.boss.Hittable)
            {
                foreach (var hb in g.boss.Hurtboxes()) if (hb.Overlaps(r))
                {
                    if (g.boss.shotProof) { g.Ricochet(pos, vel); Splat(); return; }      // guarded
                    g.boss.Damage(dmg, owner); Splat(); return;
                }
                foreach (var bb in g.boss.BlockBoxes())
                    if (bb.Overlaps(r)) { Sfx.Play("hit", .3f, 1.9f); g.SpawnFx("spark_ice", "spark", pos, 1.2f, false, 31); Splat(); return; }
            }
            if (life <= 0f || pos.x < Levels.Left - 0.2f || pos.x > Levels.Right + 0.2f || pos.y < g.world.GroundTop || pos.y > g.world.y0 + Levels.Height) Splat();
        }

        void Splat()
        {
            dead = true; Object.Destroy(go);
            if (GameCtl.I.world.def.ice) GameCtl.I.SpawnFx("splat", "splat", pos, 1f, false, 22);
            else GameCtl.I.SpawnFx("splat", "splat", pos, 1f, false, 22);
        }
    }

    /// <summary>The "EX" move: a wide gum beam that sticks every enemy in front of the player.</summary>
    public class SuperBeam
    {
        public bool dead; Player owner; float t; float tick; GameObject root; SpriteRenderer[] parts = new SpriteRenderer[3]; Anim[] anims = new Anim[3];
        public const float Len = 7.2f, Life = 0.85f;

        public SuperBeam(Player o)
        {
            owner = o;
            root = new GameObject("superbeam"); root.transform.SetParent(GameCtl.I.entRoot, false);
            var sh = Art.Get("super");
            for (int i = 0; i < 3; i++)
            {
                parts[i] = Pix.Make("seg" + i, root.transform, sh[0], Vector3.zero, 24);
                anims[i] = new Anim(parts[i], sh, "super"); anims[i].t = i * 1.7f;
            }
        }

        public Rect Area()
        {
            var m = owner.Muzzle(); float x0 = owner.facing > 0 ? m.x : m.x - Len;
            return new Rect(x0, m.y - 0.55f, Len, 1.1f);
        }

        public void Tick(float dt)
        {
            var g = GameCtl.I;
            t += dt; tick -= dt;
            var m = owner.Muzzle();
            float grow = Mathf.Clamp01(t / 0.12f), shrink = Mathf.Clamp01((Life - t) / 0.15f);
            for (int i = 0; i < 3; i++)
            {
                anims[i].Tick(dt);
                float off = i * 2.4f * owner.facing;
                parts[i].transform.position = new Vector3(m.x + off, m.y, 0);
                parts[i].flipX = owner.facing < 0;
                parts[i].transform.localScale = new Vector3(1f, Mathf.Min(grow, shrink), 1f);
                parts[i].enabled = (i * 2.4f) < Len * grow + 0.1f;
            }
            if (tick <= 0f)
            {
                tick = 0.1f;
                var a = Area();
                foreach (var e in g.enemies) if (e.state == EState.Normal && e.Hurtbox.Overlaps(a)) e.Stick(owner);
                if (g.boss != null && g.boss.Hittable)
                    foreach (var hb in g.boss.Hurtboxes()) if (hb.Overlaps(a)) { g.boss.Damage(2, owner, false); break; }
                foreach (var s in g.eshots) if (!s.dead && a.Contains(s.pos)) s.Pop();
                foreach (var c in g.crystals) if (c.Hurtbox.Overlaps(a)) c.Hit(2f);
            }
            if (t >= Life) { dead = true; Object.Destroy(root); }
        }
    }

    public class Player
    {
        public int index; public string hero; public bool joined, ghost;
        public int hp = 4, maxHp = 4; public float meter;           // meter 0..5 cards
        public int facing = 1; public Body body = new Body { w = 0.56f, h = 1.1f };
        public float invuln, superT;
        public int parries, hitsTaken;

        GameObject go; SpriteRenderer sr; Anim anim; Sheet sArm, sNoArm;
        SpriteRenderer armSr; Sheet armSheet;
        GameObject ghostGo; SpriteRenderer ghostSr; Anim ghostAnim;
        float shootCd, shootT, recoilT, dashT, dashCd, hurtT, coyote, jumpBuf, landT, trailT, blink, reviveGrace;
        public float frozenT, freezeImmune; SpriteRenderer freezeSr; Anim freezeAnim; bool prevShoot; float prevAxis; bool iceWorld;
        bool jumpCut; int aimDir; Vector2 lastMuzzle; float lastFallSpeed;
        Pad pad;

        public Player(int idx, string heroName)
        {
            index = idx; hero = heroName;
            sArm = Art.Get("hero_" + hero); sNoArm = Art.Get("hero_" + hero + "_noarm"); armSheet = Art.Get("arm_" + hero);
            var root = GameCtl.I.entRoot;
            sr = Pix.Make("player" + idx, root, sArm[0], Vector3.zero, 10 + idx);
            go = sr.gameObject;
            anim = new Anim(sr, sArm, "idle");
            armSr = Pix.Make("arm", root, armSheet[0], Vector3.zero, 12 + idx); armSr.enabled = false;
            var gs = Art.Get("ghost_" + hero);
            ghostSr = Pix.Make("ghost" + idx, root, gs[0], Vector3.zero, 14); ghostGo = ghostSr.gameObject; ghostGo.SetActive(false);
            ghostAnim = new Anim(ghostSr, gs, "float");
            var fs = Art.Get("freeze");
            freezeSr = Pix.Make("freeze" + idx, root, fs[0], Vector3.zero, 13); freezeAnim = new Anim(freezeSr, fs, "freeze"); freezeSr.gameObject.SetActive(false);
            go.SetActive(false); armSr.gameObject.SetActive(false);
        }

        /// <summary>Winter clothes (scarf, earmuffs, mittens) in the ice kingdom.</summary>
        public void SetWorld(bool ice)
        {
            iceWorld = ice; string sfx = ice ? "_ice" : "";
            sArm = Art.Get("hero_" + hero + sfx); sNoArm = Art.Get("hero_" + hero + sfx + "_noarm"); armSheet = Art.Get("arm_" + hero + sfx);
        }

        GameCtl G { get { return GameCtl.I; } }
        public Vector2 Center { get { return body.Center; } }
        public bool Alive { get { return joined && !ghost; } }

        public void Join(Vector2 at)
        {
            joined = true; ghost = false; hp = maxHp; meter = Mathf.Min(meter, 5f);
            body.pos = at; body.vel = Vector2.zero; invuln = 1.2f; hurtT = 0; dashT = 0; superT = 0;
            frozenT = 0; freezeImmune = 0; SetWorld(G.world != null && G.world.def.ice);
            go.SetActive(true); armSr.gameObject.SetActive(true); ghostGo.SetActive(false);
            facing = index == 0 ? 1 : -1;
            anim.Play("idle", true);
        }

        public void Place(Vector2 at) { body.pos = at; body.vel = Vector2.zero; go.transform.position = at; }
        public void SetVisible(bool v) { if (!joined) return; go.SetActive(v && !ghost); armSr.gameObject.SetActive(v && !ghost); ghostGo.SetActive(v && ghost); if (!v) freezeSr.gameObject.SetActive(false); }

        public void Heal(int n) { if (Alive) hp = Mathf.Min(maxHp, hp + n); }
        public void AddMeter(float v, bool share = true)
        {
            meter = Mathf.Min(5f, meter + v);
            if (share) foreach (var o in G.players) if (o != this && o.Alive) o.AddMeter(v * 0.5f, false);      // together: your partner charges too
        }

        public void Revive(int hpAmount)
        {
            ghost = false; hp = hpAmount; invuln = 2f; reviveGrace = 0.5f;
            go.SetActive(true); armSr.gameObject.SetActive(true); ghostGo.SetActive(false);
            body.pos = ghostGo.transform.position - new Vector3(0, 0.6f, 0); body.vel = new Vector2(0, 8f);
            Sfx.Play("revive");
            G.SpawnFx("parry", "parry", Center, 1f, false, 30);
        }

        /// <summary>Used while a bubble lifts the player to the next level.</summary>
        public void Carry(Vector2 pos, float dt)
        {
            body.pos = pos; body.vel = new Vector2(0, 0.01f); body.grounded = false;
            shootT = 0; hurtT = 0; dashT = 0; superT = 0; recoilT = 0; invuln = 0.1f; frozenT = 0; freezeSr.gameObject.SetActive(false);
            if (ghost) { ghostGo.transform.position = pos + new Vector2(0, 0.6f); ghostAnim.Tick(dt); return; }
            Visuals(dt);
        }

        public Vector2 Muzzle()
        {
            // anchor of the arm sprite plus the muzzle offset from the arm data
            return lastMuzzle;
        }

        // ------------------------------------------------------------------------------------------------ tick
        public void Tick(float dt, bool allowInput)
        {
            if (!joined) return;
            pad = allowInput ? GumInput.Read(index) : default(Pad);
            if (ghost) { GhostTick(dt); return; }

            float time = G.time;
            invuln = Mathf.Max(0, invuln - dt); superT = Mathf.Max(0, superT - dt); hurtT = Mathf.Max(0, hurtT - dt);
            shootCd -= dt; shootT = Mathf.Max(0, shootT - dt); recoilT = Mathf.Max(0, recoilT - dt);
            dashT = Mathf.Max(0, dashT - dt); dashCd -= dt; landT = Mathf.Max(0, landT - dt); coyote -= dt; jumpBuf -= dt; reviveGrace -= dt;

            freezeImmune = Mathf.Max(0f, freezeImmune - dt);
            bool frozen = frozenT > 0f; if (frozen) FreezeTick(dt);
            bool control = hurtT <= 0 && superT <= 0 && !frozen;
            float ix = control ? pad.x : 0f;
            if (control && Mathf.Abs(ix) > 0.1f) facing = ix > 0 ? 1 : -1;

            // ---- horizontal
            if (frozen) body.vel.x = 0f;
            else if (dashT > 0) { body.vel.x = facing * 13f; body.vel.y = 0; }
            else if (hurtT > 0) body.vel.x = Pix.Approach(body.vel.x, 0, 14f * dt);
            else if (superT > 0) body.vel.x = 0;
            else
            {
                float target = ix * 6.4f;
                bool onIce = body.grounded && body.groundIce;              // ice keeps your momentum; snow lets you stop
                float accel = body.grounded ? (onIce ? 12f : 70f) : 42f;
                if (Mathf.Abs(ix) < 0.1f) accel = onIce ? 5.5f : accel * 1.2f;
                body.vel.x = Pix.Approach(body.vel.x, target, accel * dt);
            }
            float windDrift = 0f;
            if (!frozen && superT <= 0f) { float push = G.wind != null ? G.wind.PushFor(this) : 0f; if (push != 0f) windDrift = Mathf.Sign(push) * (body.grounded ? 3.6f : 4.6f); }

            // ---- jumping / parry / drop-through
            if (control && pad.jumpDown)
            {
                if (pad.down && body.grounded && G.world.OneWayUnder(body.pos.x, body.groundTop))
                {
                    body.dropUntil = time + 0.24f; body.pos.y -= 0.06f; body.grounded = false; body.vel.y = -2f; Sfx.Play("land", .5f, 1.4f);
                }
                else if (body.grounded || coyote > 0) jumpBuf = 0.12f;
                else if (!TryParry()) jumpBuf = 0.12f;
            }
            if (control && jumpBuf > 0 && (body.grounded || coyote > 0))
            {
                body.vel.y = 14.6f; jumpBuf = 0; coyote = 0; body.grounded = false; jumpCut = false; landT = 0;
                Sfx.Play("jump", .7f);
                G.SpawnFx("dust", "dust", new Vector2(body.pos.x, body.pos.y), 1f, false, 8);
            }
            if (!pad.jumpHeld && body.vel.y > 5.5f && !jumpCut && dashT <= 0) { body.vel.y = 5.5f; jumpCut = true; }

            // ---- dash
            if (control && pad.dashDown && dashCd <= 0 && dashT <= 0)
            {
                dashT = 0.2f; dashCd = 0.7f; invuln = Mathf.Max(invuln, 0.24f);
                Sfx.Play("dash", .8f); G.SpawnFx("dust", "dust", new Vector2(body.pos.x, body.pos.y), 1f, false, 8);
            }

            // ---- super
            if (control && pad.superDown && meter >= 5f && dashT <= 0)
            {
                meter = 0; superT = SuperBeam.Life; invuln = Mathf.Max(invuln, SuperBeam.Life + .1f); shootT = SuperBeam.Life; recoilT = .1f;
                G.beams.Add(new SuperBeam(this)); G.Shake(.12f, .5f); G.HitStop(.08f); Sfx.Play("super");
                body.vel = Vector2.zero;
            }

            // ---- gravity
            if (dashT <= 0) body.vel.y = Mathf.Max(-21f, body.vel.y - 38f * dt);
            bool wasGround = body.grounded;
            float preVy = body.vel.y; float prevY = body.pos.y;
            body.Step(dt, G.world, time);
            if (body.bounced) jumpCut = true;
            TryHeadBounce(prevY);
            if (windDrift != 0f) body.pos.x = Mathf.Clamp(body.pos.x + windDrift * dt, Levels.Left + body.w * 0.5f, Levels.Right - body.w * 0.5f);
            if (wasGround && !body.grounded && body.vel.y <= 0) coyote = 0.09f;
            if (body.grounded) coyote = 0.09f;
            if (body.landedThisStep)
            {
                landT = 0.12f; jumpCut = false;
                if (preVy < -7f) { Sfx.Play("land", .8f); G.SpawnFx("dust", "dust", new Vector2(body.pos.x, body.pos.y), 1f, false, 8); }
            }

            // ---- shooting
            // auto mode only pulls the trigger for you: you still aim (W = up, W + a side = diagonal, otherwise straight ahead)
            bool streaming = G.autoFire && control;
            aimDir = pad.up ? (Mathf.Abs(pad.x) > 0.1f ? 1 : 2) : 0;
            bool fire = pad.shoot || streaming;
            if (control && fire && shootCd <= 0 && dashT <= 0)
            {
                shootCd = 0.125f; shootT = 0.2f; recoilT = 0.09f; Fire();
            }

            // ---- contacts
            Contacts();

            // ---- trail while dashing
            if (dashT > 0) { trailT -= dt; if (trailT <= 0) { trailT = 0.045f; var f = G.SpawnFx("trail", "trail", Center, 1.6f, false, 9); f.sr.color = new Color(1f, .8f, .93f, .8f); } }

            Visuals(dt);
        }

        // ---- together: land on your partner's head and you spring up (reach ledges alone you could not)
        void TryHeadBounce(float prevY)
        {
            if (body.grounded || body.vel.y > -1f || dashT > 0f) return;
            foreach (var o in G.players)
            {
                if (o == this || !o.Alive || o.frozenT > 0f) continue;
                float top = o.body.pos.y + o.body.h;
                if (Mathf.Abs(body.pos.x - o.body.pos.x) > 0.55f) continue;
                if (prevY >= top - 0.05f && body.pos.y <= top + 0.1f)
                {
                    body.pos.y = top; body.vel.y = pad.jumpHeld ? 19f : 15.5f; jumpCut = false; jumpBuf = 0f; coyote = 0f; G.statCoopBounces++;
                    Sfx.Play("jump", .7f, 1.5f); G.SpawnFx("spark", "spark", new Vector2(body.pos.x, top), 1.3f, false, 30); AddMeter(0.05f);
                    return;
                }
            }
        }

        bool TryParry()
        {
            foreach (var s in G.eshots)
            {
                if (s.dead || !s.parryable) continue;
                if ((s.pos - Center).sqrMagnitude < 1.0f)
                {
                    s.Parried(); parries++; G.statParries++;
                    body.vel.y = 13f; jumpCut = false; invuln = Mathf.Max(invuln, .35f);
                    AddMeter(1f); Sfx.Play("parry"); G.HitStop(.07f);
                    G.SpawnFx("parry", "parry", s.pos, 1f, false, 30);
                    G.Burst(s.pos, 8, 4f);
                    return true;
                }
            }
            return false;
        }

        /// <summary>Close to the partner: the gum is charged (double damage).</summary>
        public bool Together { get { foreach (var o in G.players) if (o != this && o.Alive && Alive && (o.Center - Center).sqrMagnitude < 4.5f * 4.5f) return true; return false; } }

        void Fire()
        {
            Vector2 dir; string d;
            if (aimDir == 0) { dir = new Vector2(facing, 0); d = "fwd"; }
            else if (aimDir == 1) { dir = new Vector2(facing * 0.7071f, 0.7071f); d = "diag"; }
            else { dir = Vector2.up; d = "up"; }
            G.shots.Add(new PlayerShot(this, lastMuzzle, dir * 14.5f, d));
            G.SpawnFx("flash", "flash", lastMuzzle, 1f, false, 21);
            Sfx.Play("shoot", .45f, 1f + 0.08f * (index));
        }

        void Contacts()
        {
            var r = body.Rect;
            r.xMin += .05f; r.xMax -= .05f; r.yMax -= .1f;
            foreach (var e in G.enemies)
            {
                if (e.state == EState.Dead) continue;
                if (e.state == EState.Stuck)
                {
                    if (e.Hurtbox.Overlaps(r) && e.kickCd <= 0f)
                    {
                        float dx = e.body.pos.x - body.pos.x;
                        e.Kick(Mathf.Abs(dx) > 0.08f ? (int)Mathf.Sign(dx) : facing);
                        G.Burst(e.body.Center, 5, 3f);
                        G.HitStop(.04f);
                    }
                    continue;
                }
                if (e.state == EState.Rolling) continue;
                if (invuln <= 0 && e.Hurtbox.Overlaps(r)) { Hit(1, e.body.pos.x); return; }
            }
            if (G.boss != null && G.boss.Dangerous && invuln <= 0)
                foreach (var hb in G.boss.Dangerboxes()) if (hb.Overlaps(r)) { Hit(1, hb.center.x); return; }
            if (invuln <= 0)
                foreach (var s in G.eshots)
                    if (!s.dead && s.Hits(r)) { Hit(1, s.pos.x, s.freezes); if (!s.persistent) s.Pop(); return; }
            if (invuln <= 0 && body.grounded)
                foreach (var w in G.waves) if (!w.dead && w.Hits(r)) { Hit(1, w.x); return; }
        }

        public void Hit(int dmg, float srcX, bool freeze = false)
        {
            if (invuln > 0 || ghost || !joined || superT > 0 || G.god) return;
            hp -= dmg; hitsTaken++; invuln = 1.7f; hurtT = 0.36f; dashT = 0;
            float dir = body.pos.x >= srcX ? 1f : -1f;
            body.vel = new Vector2(dir * 6f, 9f); body.grounded = false;
            Sfx.Play("hurt"); G.Shake(.18f, .25f); G.HitStop(.09f);
            G.SpawnFx("spark", "spark", Center, 1.4f, false, 30);
            if (hp <= 0) Die();
            else if (freeze && freezeImmune <= 0f) Freeze(2.4f);
        }

        /// <summary>Encased in ice: mash any button to crack out, or get a friend close to speed it up.</summary>
        public void Freeze(float t)
        {
            if (frozenT > 0f || ghost) return;
            frozenT = t; hurtT = 0; dashT = 0; shootT = 0; body.vel = new Vector2(0, Mathf.Min(0f, body.vel.y)); invuln = Mathf.Max(invuln, t + 0.3f);
            Sfx.Play("stick", .9f, 2f); G.SpawnFx("spark_ice", "spark", Center, 2f, false, 33);
        }

        void FreezeTick(float dt)
        {
            bool press = pad.jumpDown || pad.dashDown || pad.superDown || (pad.shoot && !prevShoot) || (Mathf.Abs(pad.x) > 0.1f && Mathf.Sign(pad.x) != Mathf.Sign(prevAxis));
            if (press) { frozenT -= 0.28f; Sfx.Play("hit", .35f, 2.2f); G.SpawnFx("spark_ice", "spark", Center + new Vector2(Random.Range(-.4f, .4f), Random.Range(0f, 1f)), 1f, false, 33); }
            prevShoot = pad.shoot; if (Mathf.Abs(pad.x) > 0.1f) prevAxis = pad.x;
            foreach (var o in G.players) if (o != this && o.Alive && (o.Center - Center).sqrMagnitude < 2.6f) frozenT -= dt * 1.4f;
            frozenT -= dt;
            if (frozenT <= 0f) Unfreeze();
        }

        void Unfreeze()
        {
            frozenT = 0f; freezeImmune = 3f; invuln = Mathf.Max(invuln, 1.0f);
            G.SpawnFx("freeze_break", "break", Center, 1f, false, 34); Sfx.Play("pop", .7f, 1.5f); G.Burst(Center, 8, 4f);
            freezeSr.gameObject.SetActive(false);
        }

        void Die()
        {
            hp = 0; ghost = true; meter = 0; frozenT = 0; freezeSr.gameObject.SetActive(false);
            go.SetActive(false); armSr.gameObject.SetActive(false);
            ghostGo.SetActive(true); ghostGo.transform.position = Center;
            G.Burst(Center, 12, 5f);
            G.OnPlayerDown(this);
        }

        void GhostTick(float dt)
        {
            Vector3 p = ghostGo.transform.position;
            float sp = 4.2f;
            p.x += pad.x * sp * dt; p.y += ((pad.up ? 1 : 0) - (pad.down ? 1 : 0)) * sp * dt;
            if (pad.jumpHeld) p.y += sp * 0.6f * dt;
            if (G.solo && Mathf.Abs(pad.x) < 0.1f && !pad.up && !pad.down && !pad.jumpHeld)
            {
                Player buddy = null; foreach (var o in G.players) if (o != this && o.Alive) buddy = o;
                if (buddy != null) { var to = (buddy.Center + new Vector2(0f, 0.4f)) - (Vector2)p; if (to.magnitude > 0.2f) { var stp = to.normalized * 3.6f * dt; p.x += stp.x; p.y += stp.y; } }
            }
            p.x = Mathf.Clamp(p.x, Levels.Left + .5f, Levels.Right - .5f);
            p.y = Mathf.Clamp(p.y, G.world.y0 + 1.6f, G.world.y0 + Levels.Height - 1f);
            p.y += Mathf.Sin(G.time * 3f + index) * 0.4f * dt;
            ghostGo.transform.position = p;
            ghostAnim.Tick(dt);
            ghostSr.flipX = pad.x < 0 || (Mathf.Abs(pad.x) < .1f && facing < 0);
            body.pos = new Vector2(p.x, p.y - 0.5f);
            foreach (var o in G.players)
                if (o != this && o.Alive && ((Vector2)p - o.Center).sqrMagnitude < 0.8f) { Revive(1); return; }
        }

        // ------------------------------------------------------------------------------------------------ visuals
        void Visuals(float dt)
        {
            string a;
            if (frozenT > 0f) a = "hurt";
            else if (hurtT > 0) a = "hurt";
            else if (dashT > 0) a = "dash";
            else if (!body.grounded) a = body.vel.y > 3.5f ? "rise" : (body.vel.y > -3f ? "apex" : "fall");
            else if (landT > 0) a = "land";
            else if (Mathf.Abs(body.vel.x) > 0.6f) a = "run";
            else a = "idle";
            anim.Play(a);
            anim.speed = a == "run" ? Mathf.Clamp(Mathf.Abs(body.vel.x) / 6.4f, 0.6f, 1.2f) : 1f;
            bool armed = shootT > 0 && hurtT <= 0 && dashT <= 0;
            anim.SetSheet(armed ? sNoArm : sArm);
            anim.Tick(dt);
            sr.flipX = facing < 0;
            go.transform.position = new Vector3(body.pos.x, body.pos.y, 0);
            // blink when invulnerable (not while dashing: that is a deliberate i-frame)
            float alpha = 1f;
            if (invuln > 0 && dashT <= 0 && superT <= 0) { blink += dt; alpha = ((int)(blink * 18f)) % 2 == 0 ? 1f : 0.35f; }
            sr.color = frozenT > 0f ? new Color(.72f, .88f, 1f, 1f) : new Color(1, 1, 1, alpha); armSr.color = sr.color;
            if (frozenT > 0f)
            {
                freezeSr.gameObject.SetActive(true); freezeAnim.Tick(dt);
                freezeSr.transform.position = new Vector3(body.pos.x, body.pos.y + 0.58f, 0);
                float pulse = 0.85f + 0.15f * Mathf.Sin(G.time * 30f * Mathf.Min(1f, 0.4f + (3f - frozenT) * 0.5f));
                freezeSr.color = new Color(1, 1, 1, pulse);
            }
            else freezeSr.gameObject.SetActive(false);

            // arm sprite: attaches to this frame's shoulder
            var def = sr.sprite != null ? (armed ? sNoArm : sArm).def : null;
            int fi = anim.Frame + anim.clip.frames.Length * 0;
            int absFrame = FrameIndexInSheet();
            float sx = def.shoulders[absFrame * 2], sy = def.shoulders[absFrame * 2 + 1];
            float ox = (sx - def.px * def.cw) / def.ppu * facing;
            float oy = ((def.ch - sy) - def.py * def.ch) / def.ppu;
            var shoulder = new Vector3(body.pos.x + ox, body.pos.y + oy, 0);
            int ri = recoilT > 0.05f ? 0 : (recoilT > 0f ? 1 : 2);
            int ai = aimDir * 3 + ri;
            var ad = armSheet.def.armdata;
            float mx = ad[ai * 4 + 2] - ad[ai * 4], my = ad[ai * 4 + 3] - ad[ai * 4 + 1];
            lastMuzzle = new Vector2(shoulder.x + mx / armSheet.def.ppu * facing, shoulder.y - my / armSheet.def.ppu);
            armSr.enabled = armed;
            if (armed)
            {
                armSr.sprite = armSheet[ai]; armSr.flipX = facing < 0;
                armSr.transform.position = shoulder;
            }
            else
            {
                // keep a sensible muzzle position for the first shot of a burst
                armSr.transform.position = shoulder;
            }
        }

        int FrameIndexInSheet()
        {
            // clip.frames are a slice of the sheet; recover the absolute index from the sprite name
            var n = sr.sprite.name; int u = n.LastIndexOf('_');
            int idx; return int.TryParse(n.Substring(u + 1), out idx) ? idx : 0;
        }

        public void Dispose()
        {
            if (go != null) Object.Destroy(go);
            if (armSr != null) Object.Destroy(armSr.gameObject);
            if (ghostGo != null) Object.Destroy(ghostGo);
            if (freezeSr != null) Object.Destroy(freezeSr.gameObject);
        }
    }
}
