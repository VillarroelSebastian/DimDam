using System.Collections.Generic;
using UnityEngine;

namespace GumRemix
{
    /// <summary>What the game needs from any boss, so Gran Chiclon and the Monarca del Deshielo plug into the same flow.</summary>
    public abstract class BossBase
    {
        public bool shotProof, glow; public float lazoT, lazoTaken; public int lazoTarget;      // guard phase: only the string hurts
        public int hp; public int phase = 1; public float x, y; public bool active, trans, dying, dead;
        static readonly Rect[] None = new Rect[0];

        public abstract int TotalHp { get; }
        public abstract string DisplayName { get; }
        /// <summary>hp values at which the next form starts (descending), used for the ticks on the health bar.</summary>
        public abstract int[] PhaseFloors { get; }

        public virtual bool Hittable { get { return active && !trans && !dying && !dead; } }
        public virtual bool Dangerous { get { return active && !trans && !dead; } }
        public abstract Rect[] Hurtboxes();
        /// <summary>Parts that stop gum without being hurt (the Monarca's snowy shell).</summary>
        public virtual Rect[] BlockBoxes() { return None; }
        /// <summary>Where a rolling snowball counts as a hit.</summary>
        public virtual Rect[] BallBoxes() { return Hurtboxes(); }
        public abstract List<Rect> Dangerboxes();
        public abstract void Damage(int dmg, Player by, bool addMeter = true);
        public abstract void Tick(float dt);
        public abstract void Activate();
        public abstract void Dispose();
    }
}
