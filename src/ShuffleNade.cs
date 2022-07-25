using KzDuckMods;
using KzDuckMods.Things;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DuckGame.SwapperMod
{
    [EditorGroup("KzMod")]
    public class ShuffleNade : Grenade
    {

        private SpriteMap _sprite;

        public ShuffleNade(float xval, float yval) : base(xval, yval)
        {
            this.ammo = 1;
            this.physicsMaterial = PhysicsMaterial.Metal;
            this._type = "gun";
            this._sprite = new SpriteMap(Mod.GetPath<KzMod>("ShuffleNade"), 32, 32, false);
            this._editorName = "ShuffleNade";
            this.graphic = (Sprite)this._sprite;
            this.center = new Vec2(15f, 18f);
            this.scale = new Vec2(0.43f, 0.43f);
            this.position.Rotate(Maths.DegToRad(90f), Vec2.Zero);
            this.handAngle = 0.28f;
            this.collisionOffset = new Vec2(-6f, -4f);
            this.collisionSize = new Vec2(12f, 9f);
            this._barrelOffsetTL = new Vec2(26f, 14f);
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
            if (!receivingPress)
            {
                SFX.Play(Mod.GetPath<KzMod>("sounds/shufflenade-teleport.wav"), 1f, 0.0f, 0.0f, false);

                if (!this._pin)
                    return;
                this._sprite.frame = 1;

                List<Duck> ducks = Level.CheckCircleAll<Duck>(this.position, 2200f).ToList();
                List<Ragdoll> ragdolls = Level.CheckCircleAll<Ragdoll>(this.position, 2200f).ToList();

                List<Holdable> holdables = new List<Holdable>();
                foreach (Duck duck in ducks)
                {
                    holdables.Add(duck.holdObject);
                    SmokeStuff(duck.position);
                }

                var shuffledHoldables = holdables.OrderBy(x => Guid.NewGuid()).ToList();
                int i = 0;
                foreach (Duck duck in ducks)
                {
                    duck.holdObject = shuffledHoldables[i];
                    if (duck.holdObject != null)
                    {
                        duck.holdObject.owner = (Thing)duck;
                        duck.holdObject.enablePhysics = false;
                        duck.holdObject.solid = false;
                        duck.holdObject.hSpeed = 0.0f;
                        duck.holdObject.vSpeed = 0.0f;
                        duck.holdObject.enablePhysics = false;
                    }
                    i++;
                }
            }

            base.OnPressAction();
        }

        private void SmokeStuff(Vec2 position)
        {
            for (int index = 0; index < 4; ++index)
                Level.Add((Thing)new ElectricalChargeSafe(position.x, position.y, 0.3f, (Thing)this));

            int smokeAmount = 15;
            for (int i = 0; i < smokeAmount; ++i)
                Level.Add((Thing)SmallSmoke.New(position.x + Rando.Float(-8f, 8f), position.y + Rando.Float(-8f, 8f)));
        }
    }
}