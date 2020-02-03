using DuckGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyMod.src
{
    [EditorGroup("KzMod|Boomerang")]
    public class Boomerang : Gun
    {
        private SpriteMap sprite;
        public Boomerang(float x, float y) : base(x, y)
        {
            this.sprite = new SpriteMap(GetPath("ImpactGrenade"), 11, 11);
            graphic = sprite;
            physicsMaterial = PhysicsMaterial.Wood;
        }

        public override void Update()
        {
            base.Update();

            if (this.lastHSpeed < 1f && _framesSinceThrown < 120)
            {
                this.hSpeed -= 3f;

            }
            if (_framesSinceThrown < 120)
            {
                this.hSpeed = this.lastHSpeed;
            }
        }
    }
}
