using DuckGame;
using System;

namespace KzDuckMods
{
    class ExplosiveVest: Equipment
    {

        private SpriteMap _sprite;
        private bool exploded = false;

        public YihadGun detonator;
        public float timer;
        public ExplosiveVest(float x, float y) : base(x, y)
        {

            this._sprite = new SpriteMap(GetPath("YihadHat"), 32, 32, false);
            graphic = _sprite;
            _sprite.frame = 1;
            timer = 2.2f;
            center = new Vec2(20f, 23f);
            this.physicsMaterial = PhysicsMaterial.Metal;
            this._wearOffset = new Vec2(1f, 1f);
        }

        public override void Update()
        {
            this.timer -= 0.01f;
            if (timer <= 0  && !exploded)
            {
                exploded = true;
                float num1 = this.y - 2f;
                for (int index = 0; index < 20; ++index)
                {
                    float num2 = (float)((double)index * 18.0 - 5.0) + Rando.Float(10f);
                    ATShrapnel atShrapnel = new ATShrapnel();
                    atShrapnel.range = 60f + Rando.Float(18f);
                    Bullet bullet = new Bullet(x + (float)(Math.Cos((double)Maths.DegToRad(num2)) * 6.0), num1 - (float)(Math.Sin((double)Maths.DegToRad(num2)) * 6.0), (AmmoType)atShrapnel, num2, (Thing)null, false, -1f, false, true);
                    bullet.firedFrom = (Thing)this;
                    Level.Add((Thing)bullet);
                }
                foreach (Window window in Level.CheckCircleAll<Window>(this.position, 40f))
                {
                    if (Level.CheckLine<Block>(this.position, window.position, (Thing)window) == null)
                        window.Destroy((DestroyType)new DTImpact((Thing)this));
                }
            }
            if (exploded)
            {
                Level.Remove(this);
                Level.Remove(detonator);
            }
            if (this.owner as Duck != null)
            {
                var duck = (Duck)this.owner;
            }
            base.Update();
        }

        public void AllahuAkhbar()
        {
            this.timer = 0.01f;
        }
    }
}
