using DuckGame;
using KzDuckMods.Things;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class ShufflerGun : Gun
    {
        private SpriteMap _sprite;

        public ShufflerGun(float xval, float yval) : base(xval, yval)
        {
            this.ammo = 1;
            this.physicsMaterial = PhysicsMaterial.Metal;
            this._type = "gun";
            this._sprite = new SpriteMap(Mod.GetPath<KzMod>("ShufflerGun"), 32, 32, false);
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
                SFX.Play(Mod.GetPath<KzMod>("sounds/shuffle-wave.wav"), 0.5f, 0.0f, 0.0f, false);
                this.ammo--;
                List<Duck> ducks = Level.CheckCircleAll<Duck>(this.position, 2200f).ToList();
                List<Ragdoll> ragdolls = Level.CheckCircleAll<Ragdoll>(this.position, 2200f).ToList();

                List<Tuple<Vec2, Vec2>> positions = new List<Tuple<Vec2, Vec2>>();
                foreach (Duck duck in ducks)
                {
                    positions.Add(new Tuple<Vec2, Vec2>(duck.position, duck.velocity));
                    SmokeStuff(duck.position);

                }
                foreach (Ragdoll ragdoll in ragdolls)
                {
                    if (!ragdoll._duck.dead)
                    {
                        Vec2 livePosition = new Vec2(ragdoll.position.x, ragdoll.position.y - (float)10.7401733);

                        positions.Add(new Tuple<Vec2, Vec2>(livePosition, ragdoll.velocity));
                        SmokeStuff(ragdoll.position);
                    }
                }

                var shuffledPositions = positions.OrderBy(x => Guid.NewGuid()).ToList();
                int i = 0;
                foreach (Duck duck in ducks)
                {
                    duck.position = shuffledPositions[i].Item1;
                    duck.velocity = shuffledPositions[i].Item2;
                    i++;
                }
                foreach (Ragdoll ragdoll in ragdolls)
                {
                    if (!ragdoll._duck.dead)
                    {
                        Vec2 livePosition = new Vec2(ragdoll.position.x, ragdoll.position.y + (float)10.7401733);

                        ragdoll.position = shuffledPositions[i].Item1;
                        ragdoll.velocity = shuffledPositions[i].Item2;

                        ragdoll.part1.position = shuffledPositions[i].Item1;
                        ragdoll.part1.position.x -= 6.0584f;
                        ragdoll.part1.doll.position = shuffledPositions[i].Item1;
                        ragdoll.part2.position = shuffledPositions[i].Item1;
                        ragdoll.part2.doll.position = shuffledPositions[i].Item1;
                        ragdoll.part3.position = shuffledPositions[i].Item1;
                        ragdoll.part3.position.x += 6.0584f;
                        ragdoll.part3.doll.position = shuffledPositions[i].Item1;

                        i++;
                    }

                }

            }
        }

        private void SmokeStuff(Vec2 position)
        {
            for (int index = 0; index < 6; ++index)
                Level.Add((Thing)new ElectricalChargeSafe(position.x, position.y, 0.2f, (Thing)this));

            int smokeAmount = 15;
            for (int i = 0; i < smokeAmount; ++i)
                Level.Add((Thing)SmallSmoke.New(position.x + Rando.Float(-8f, 8f), position.y + Rando.Float(-8f, 8f)));

        }
    }
}