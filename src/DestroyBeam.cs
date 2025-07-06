
using DuckGame;
using KzDuckMods.Things;
using System;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class DestroyBeam : MaterialThing
    {
        protected SpriteMap _beam;
        protected Vec2 _prev = Vec2.Zero;
        protected Vec2 _endPoint = Vec2.Zero;

        public enum DestroyerType
        {
            HoldablesAndDucks,
            Holdables,
            Weapons,
            EmptyWeapons,
        }
        public EditorProperty<DestroyerType> mode = new EditorProperty<DestroyerType>(DestroyerType.EmptyWeapons);

        public DestroyBeam(float xpos, float ypos) : base(xpos, ypos)
        {
            this._editorName = "Destroy Beam";
            this.editorTooltip = "Place 2 generators near each other to create a beam that destroy things passing through.";

            this._beam = new SpriteMap(Mod.GetPath<KzMod>("destroyBeam"), 16, 16);
            this._beam.ClearAnimations();
            this._beam.AddAnimation("idle", 1f, true, 0, 1, 2, 3, 4, 5, 6, 7);
            this._beam.SetAnimation("idle");
            this._beam.speed = 0.2f;
            this._beam.alpha = 0.3f;
            this._beam.center = new Vec2(0.0f, 8f);

            this.graphic = new Sprite(Mod.GetPath<KzMod>("destroyBeamer"));
            this.center = new Vec2(9f, 8f);
            this.collisionOffset = new Vec2(-2f, -5f);
            this.collisionSize = new Vec2(4f, 10f);
            this.depth = (Depth)(-0.5f);
            this.hugWalls = WallHug.Left;
        }


        public override void Update()
        {
            base.Update();

            switch (mode.value)
            {
                case DestroyerType.HoldablesAndDucks:
                    SetSpeedAndAlpha(0.6f, 0.7f);
                    return;
                case DestroyerType.Holdables:
                    SetSpeedAndAlpha(0.4f, 0.5f);
                    return;
                case DestroyerType.Weapons:
                    SetSpeedAndAlpha(0.2f, 0.35f);
                    return;
                case DestroyerType.EmptyWeapons:
                    SetSpeedAndAlpha(0.1f, 0.1f);
                    return;
            }


            if (Editor.editorDraw)
                return;
            if (this.GetType() == typeof(DestroyBeam))
            {
                //if (this._prev != this.position)
                //{
                //    this._endPoint = Vec2.Zero;
                //    for (int index = 0; index < 32; ++index)
                //    {
                //        Thing thing = (Thing)Level.CheckLine<Block>(this.position + new Vec2((float)(4 + index * 16), 0.0f), this.position + new Vec2((float)((index + 1) * 16 - 6), 0.0f));
                //        if (thing != null)
                //        {
                //            this._endPoint = new Vec2(thing.left - 2f, this.y);
                //            break;
                //        }
                //    }
                //    this._prev = this.position;
                //}
                //if (this._endPoint != Vec2.Zero)
                //{
                    //this.graphic.flipH = true;
                    //this.graphic.depth = this.depth;
                    //Graphics.Draw(this.graphic, this._endPoint.x, this._endPoint.y);
                    //this.graphic.flipH = false;
                    //this._beam.depth = this.depth - 2;
                    //float x = this._endPoint.x - this.x;
                    float x = this.collisionSize.x + this.x;
                    int num = (int)Math.Ceiling((double)x / 16.0);
                    for (int index = 0; index < num; ++index)
                    {
                        Level.Add((Thing)new ElectricalChargeSafe(this.x + (float)(index * 16), this.y, 1f));
                        Level.Add((Thing)new ElectricalChargeSafe(this.x + (float)(index * 16), this.y, 0.4f));
                    }
                //}
            }

        }

        private void SetSpeedAndAlpha(float speed, float alpha)
        {
            this._beam.speed = speed;
            this._beam.alpha = alpha;
        }

        public override void OnSoftImpact(MaterialThing with, ImpactedFrom from)
        {
            switch (mode.value)
            {
                case DestroyerType.HoldablesAndDucks:
                    HoldablesPlusDucks(with);
                    return;
                case DestroyerType.Holdables:
                    DestroyHoldableNoDucks(with);
                    return;
                case DestroyerType.Weapons:
                    DestroyWeapons(with, false);
                    return;
                case DestroyerType.EmptyWeapons:
                    DestroyWeapons(with, true);
                    return;
            }
        }

        private static void HoldablesPlusDucks(MaterialThing with)
        {
            if (with is Duck duck)
            {
                if (duck != null)
                {
                    DesintegrateDuck(duck);
                    return;
                }
            }
            else if (with is TrappedDuck trappedDuck)
            {
                if (trappedDuck != null
                    && trappedDuck._duckOwner != null
                    )
                {
                    DesintegrateDuck(trappedDuck._duckOwner);
                    return;
                }

            }
            else if (with is RagdollPart ragdoll)
            {
                if (ragdoll != null
                    && ragdoll.doll != null
                    && ragdoll.doll._duck != null
                    )
                {
                    DesintegrateDuck(ragdoll.doll._duck);
                    return;
                }
            }

            DestroyHoldableNoDucks(with);
        }

        private static void DesintegrateDuck(Duck duck)
        {
            if (!duck.dead)
            {
                duck.Scream();
                duck.Kill((DestroyType)new DTIncinerate((Thing)duck));
                if (duck != null &&
                    duck._cooked != null)
                {
                    Level.Remove(duck._cooked);
                }
            }
            SmokeOnDestroy(duck);
            Level.Remove(duck);

        }

        private static void DestroyWeapons(MaterialThing with, bool EmptyWeaponsOnly)
        {
            if (!(with is Gun gun))
                return;
            switch (gun)
            {
                case Sword _:
                    break;
                case SledgeHammer _:
                    break;
                default:
                    if (!EmptyWeaponsOnly || gun.ammo == 0)
                    {
                        Level.Remove((Thing)gun);
                    }
                    break;
            }
        }

        private static void DestroyHoldableNoDucks(MaterialThing with)
        {
            if (!(with is Holdable holdable))
                return;
            switch (holdable)
            {
                case TrappedDuck _:
                    break;
                case RagdollPart _:
                    break;
                default:
                    Level.Remove((Thing)holdable);
                    SmokeOnDestroy(holdable);
                    break;
            }
            return;
        }


        private static void SmokeOnDestroy(Thing stuff)
        {
            Level.Add((Thing)SmallSmoke.New(stuff.x, stuff.y));
            Level.Add((Thing)SmallSmoke.New(stuff.x + 4f, stuff.y));
            Level.Add((Thing)SmallSmoke.New(stuff.x - 4f, stuff.y));
            Level.Add((Thing)SmallSmoke.New(stuff.x, stuff.y + 4f));
            Level.Add((Thing)SmallSmoke.New(stuff.x, stuff.y - 4f));
        }

        public override void Draw()
        {
            if (Editor.editorDraw)
                return;
            if (this.GetType() == typeof(DestroyBeam))
            {
                if (this._prev != this.position)
                {
                    this._endPoint = Vec2.Zero;
                    for (int index = 0; index < 32; ++index)
                    {
                        Thing thing = (Thing)Level.CheckLine<Block>(this.position + new Vec2((float)(4 + index * 16), 0.0f), this.position + new Vec2((float)((index + 1) * 16 - 6), 0.0f));
                        if (thing != null)
                        {
                            this._endPoint = new Vec2(thing.left - 2f, this.y);
                            break;
                        }
                    }
                    this._prev = this.position;
                }
                if (this._endPoint != Vec2.Zero)
                {
                    this.graphic.flipH = true;
                    this.graphic.depth = this.depth;
                    Graphics.Draw(this.graphic, this._endPoint.x, this._endPoint.y);
                    this.graphic.flipH = false;
                    this._beam.depth = this.depth - 2;
                    float x = this._endPoint.x - this.x;
                    int num = (int)Math.Ceiling((double)x / 16.0);
                    for (int index = 0; index < num; ++index)
                    {
                        this._beam.cutWidth = index != num - 1 ? 0 : 16 - (int)((double)x % 16.0);
                        Graphics.Draw((Sprite)this._beam, this.x + (float)(index * 16), this.y);
                    }
                    this.collisionOffset = new Vec2(-1f, -4f);
                    this.collisionSize = new Vec2(x, 8f);
                }
                else
                {
                    this.collisionOffset = new Vec2(-1f, -5f);
                    this.collisionSize = new Vec2(4f, 10f);
                }
            }
            base.Draw();
        }
    }
}
