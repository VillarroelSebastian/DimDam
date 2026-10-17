using UnityEngine;

namespace GumRemix
{
    /// <summary>One-shot animated sprite (dust puffs, sparks, pops...).</summary>
    public class Fx
    {
        public GameObject go; public SpriteRenderer sr; public Anim anim; public Vector2 vel; public bool dead; public float life = -1f; public float grav;

        public static Fx Make(Transform parent, string sheet, string clip, Vector2 pos, float scale, bool flip, int order)
        {
            var f = new Fx();
            var sh = Art.Get(sheet);
            f.sr = Pix.Make("fx_" + sheet, parent, sh[0], pos, order, scale);
            f.go = f.sr.gameObject; f.sr.flipX = flip;
            f.anim = new Anim(f.sr, sh, clip);
            return f;
        }

        public void Tick(float dt)
        {
            if (dead) return;
            vel.y -= grav * dt;
            go.transform.position += (Vector3)(vel * dt);
            anim.Tick(dt);
            if (life > 0f) { life -= dt; if (life <= 0f) Kill(); }
            if (anim.done && life < 0f) Kill();
        }

        public void Kill() { dead = true; if (go != null) Object.Destroy(go); }
    }

    /// <summary>Confetti-style sprinkle used for pops, landings and ambience.</summary>
    public class Particle
    {
        public GameObject go; public SpriteRenderer sr; public Vector2 vel; public float grav, life, age; public int color; public float spin, rot; public bool dead; public float drag;
        Sheet sheet;

        public static Particle Make(Transform parent, Vector2 pos, Vector2 vel, float grav, float life, int color, int order = 40)
        {
            var p = new Particle();
            p.sheet = Art.Get("sprinkles");
            p.sr = Pix.Make("sprinkle", parent, p.sheet[color * 4], pos, order);
            p.go = p.sr.gameObject; p.vel = vel; p.grav = grav; p.life = life; p.color = color;
            p.spin = Random.Range(6f, 16f) * (Random.value < .5f ? -1 : 1);
            return p;
        }

        public void Tick(float dt)
        {
            age += dt; if (age >= life) { dead = true; Object.Destroy(go); return; }
            vel.y -= grav * dt; vel -= vel * drag * dt;
            go.transform.position += (Vector3)(vel * dt);
            rot += spin * dt;
            sr.sprite = sheet[color * 4 + (((int)rot % 4) + 4) % 4];
            float left = life - age;
            sr.color = new Color(1, 1, 1, left < 0.3f ? left / 0.3f : 1f);
        }
    }
}
