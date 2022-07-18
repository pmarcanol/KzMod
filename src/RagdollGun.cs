using DuckGame;
using KzDuckMods.Things;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class RagdollGun : Gun
    {
        private SpriteMap _sprite;

        public RagdollGun(float xval, float yval) : base(xval, yval)
        {
            this.ammo = 1;
            this.physicsMaterial = PhysicsMaterial.Metal;
            this._type = "gun";
            this._sprite = new SpriteMap(Mod.GetPath<KzMod>("RagdollGun"), 32, 32, false);
            this.graphic = (Sprite)this._sprite;
            this.center = new Vec2(15f, 18f);
            this.scale = new Vec2(0.43f, 0.43f);
            this.position.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            this.collisionOffset = new Vec2(-6f, -4f);
            this.collisionSize = new Vec2(12f, 9f);
            this._barrelOffsetTL = new Vec2(26f, 14f);
            this.handAngle = 0.28f;
            this._sprite.frame = 0;
        }

        public override void Fire() { }

        public override void Update()
        {
            if (this.owner != null)
            {
                int dir = owner.offDir < (sbyte)0 ? -1 : 1;
                this.handAngle = Math.Abs(this.handAngle) * dir;
            }
            base.Update();
        }

        public override void OnPressAction()
        {
            base.OnPressAction();
            if (this.ammo > 0

                )
            {
                this._sprite.frame = 1;

                Graphics.flashAdd = 1.3f;
                Layer.Game.darken = 1.3f;

                this.ammo--;
                List<Duck> ducks = Level.CheckCircleAll<Duck>(this.position, 2200f).ToList();
                foreach (Duck duck in ducks)
                {
                    duck.GoRagdoll();
                }
                SFX.Play(Mod.GetPath<KzMod>("sounds/ragdoll-ray.wav"), 0.8f, 0.0f, 0.0f, false);
                Level.Add(new Flash(0.4f));
            }
        }

    }
}