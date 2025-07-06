using DuckGame;
using System;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class BananaGrenade : Grenade
    {
        public StateBinding _timerBinding = new StateBinding(nameof(_timer));
        public StateBinding _pinBinding = new StateBinding(nameof(_pin));
        private SpriteMap _sprite;
        public bool _pin = true;
        public float _timer = 1.2f;
        private Duck _cookThrower;
        private float _cookTimeOnThrow;
        public bool pullOnImpact;
        private bool _explosionCreated;
        private bool _localDidExplode;
        private bool _didBonus;
        private static int grenade;
        public int gr;
        public int _explodeFrames = -1;

        private int _bananaMinCount = 4;
        private int _bananaMaxCount = 12;
        private float _bananaMinSpeed = 5f;
        private float _bananaMaxSpeed = 12f;
        private float _explosionPushForce = 60f;


        public Duck cookThrower => this._cookThrower;

        public float cookTimeOnThrow => this._cookTimeOnThrow;

        public BananaGrenade(float xval, float yval)
          : base(xval, yval)
        {
            this.ammo = 1;
            this._type = "gun";
            this._sprite = new SpriteMap(Mod.GetPath<KzMod>("bananaGrenade"), 16, 16);
            this.graphic = (Sprite)this._sprite;
            this.center = new Vec2(7f, 8f);
            this.collisionOffset = new Vec2(-4f, -5f);
            this.collisionSize = new Vec2(8f, 10f);
            this.bouncy = 0.8f;
            this.friction = 0.05f;
            this._fireRumble = RumbleIntensity.Kick;
            this._editorName = "Banana Grenade";
            this.editorTooltip = "Explodes into harmless bananas.";
            this._bio = "Splits into multiple bananas to cause chaos, not harm.";
        }

        public override void Initialize()
        {
            this.gr = BananaGrenade.grenade;
            ++BananaGrenade.grenade;
        }

        public override void OnNetworkBulletsFired(Vec2 pos)
        {
            this._pin = false;
            this._localDidExplode = true;
            if (!this._explosionCreated)
                Graphics.FlashScreen();
            this.CreateBananaExplosion(pos);
        }

        public void CreateBananaExplosion(Vec2 pos)
        {
            if (this._explosionCreated)
                return;
            float x = pos.x;
            float ypos = pos.y - 2f;
            Level.Add((Thing)new ExplosionPart(x, ypos));

            PushThings(pos);

            SpawnBananas(x, ypos);

            this._explosionCreated = true;
            SFX.Play("explode");
            RumbleManager.AddRumbleEvent(pos, new RumbleEvent(RumbleIntensity.Heavy, RumbleDuration.Short, RumbleFalloff.Medium));
        }

        private void SpawnBananas(float x, float ypos)
        {
            int bananaCount = Rando.Int(_bananaMinCount, _bananaMaxCount);
            for (int i = 0; i < bananaCount; ++i)
            {
                float angle = Rando.Float(0f, 360f);
                float speed = Rando.Float(_bananaMinSpeed, _bananaMaxSpeed);
                Vec2 velocity = new Vec2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;

                // Create a banana and call its EatBanana method
                Banana banana = new Banana(x, ypos);
                banana.hSpeed = velocity.x;
                banana.vSpeed = velocity.y;
                Level.Add(banana);
                banana.EatBanana();
            }
        }

        public override void Update()
        {
            base.Update();
            if (!this._pin)
            {
                this._timer -= 0.01f;
                this.holsterable = false;
            }
            if (!this._localDidExplode && (double)this._timer < 0.0)
            {
                this.CreateBananaExplosion(this.position);
                Level.Remove((Thing)this);
                this._destroyed = true;
            }
            this._sprite.frame = this._pin ? 0 : 1;
        }

        public override void OnSolidImpact(MaterialThing with, ImpactedFrom from)
        {
            if (this.pullOnImpact)
                this.OnPressAction();
            base.OnSolidImpact(with, from);
        }

        public override void OnPressAction()
        {
            if (!this._pin)
                return;
            this._pin = false;
            GrenadePin grenadePin = new GrenadePin(this.x, this.y);
            grenadePin.hSpeed = (float)-this.offDir * (1.5f + Rando.Float(0.5f));
            grenadePin.vSpeed = -2f;
            Level.Add((Thing)grenadePin);
            if (this.duck != null)
                RumbleManager.AddRumbleEvent(this.duck.profile, new RumbleEvent(this._fireRumble, RumbleDuration.Pulse, RumbleFalloff.None));
            SFX.Play("pullPin");
        }
        private void PushThings(Vec2 pos)
        {
            foreach (MaterialThing thing in Level.CheckCircleAll<MaterialThing>(pos, 60f))
            {
                if (thing != this)
                {
                    Vec2 direction = thing.position - pos;
                    float distance = direction.length;
                    float force = Math.Max(0, _explosionPushForce - distance) / _explosionPushForce * 10f; // Force decreases with distance
                    direction = direction.normalized * force;
                    thing.hSpeed += direction.x;
                    thing.vSpeed += direction.y;
                }
            }
        }
    }
}
