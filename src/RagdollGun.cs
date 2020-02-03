using DuckGame;

namespace KzDuckMods
{
    [EditorGroup("KzMod|BoxingGun")]
    internal class RagdollGun : DartGun
    {

        public SpriteMap sprite;
        public RagdollGun(float xval, float yval) : base(xval, yval)
        {
            sprite = new SpriteMap(GetPath("BoxingGun"), 32, 32, false);
            graphic = sprite;
            _editorName = "Boxing Gun";
            this.ammo = 1;
            this._barrelAngleOffset = 3f;
            this._kickForce = 6f;
        }
        public override void OnPressAction()
        {
            if (this.ammo > 0)
            {
                if ((double)this._burnLife <= 0.0)
                {
                    SFX.Play("dartStick", 0.5f, Rando.Float(0.2f) - 0.1f, 0.0f, false);
                }
                else
                {
                    --this.ammo;
                    SFX.Play("dartGunFire", 0.5f, Rando.Float(0.2f) - 0.1f, 0.0f, false);
                    this.kick = 1f;
                    if (this.receivingPress || !this.isServerForObject)
                        return;
                    Vec2 vec2 = this.Offset(this.barrelOffset);
                    float radians = this.barrelAngle + Rando.Float(-0.1f, 0.1f);
                    Dart dart = new MyMod.src.RagdollAmmo(vec2.x, vec2.y, this.owner as Duck, -radians);
                    this.Fondle((Thing)dart);
                    if (this.onFire)
                    {
                        Level.Add((Thing)SmallFire.New(0.0f, 0.0f, 0.0f, 0.0f, false, (MaterialThing)dart, true, (Thing)this, false));
                        dart.burning = true;
                        dart.onFire = true;
                    }
                    Vec2 vec = Maths.AngleToVec(radians);
                    dart.hSpeed = vec.x * 7.32f;
                    dart.vSpeed = vec.y * 7.32f;
                    Level.Add((Thing)dart);
                }
            }
            else
                this.DoAmmoClick();
        }

        public override void Update()
        {
            sprite.frame = this.ammo > 0 ? 0 : 1;
            sprite.frame = this.burntOut ? 2 : sprite.frame;
            base.Update();
        }

    }

   
}