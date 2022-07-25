using DuckGame;

namespace KzDuckMods
{
    public class BoxingAmmo : Dart
    {
        public SpriteMap sprite;

        public BoxingAmmo(float xpos, float ypos, Duck owner, float fireAngle)
     : base(xpos, ypos, owner, fireAngle)
        {
            sprite = new SpriteMap(GetPath("BoxingGlove"), 16, 16);
            graphic = sprite;
            _editorName = "Ragdoll Ammo";
            collisionSize = new Vec2(14, 12);
            collisionOffset = new Vec2(-9f, -9f);
            center = new Vec2(8f, 8f);
            this.weight = 9f;
        }

        public override void OnImpact(MaterialThing with, ImpactedFrom from)
        {
            if (this._stuck || with is Gun || (with is FeatherVolume || with is Teleporter || this.removeFromLevel))
                return;

            if (with is RagdollPart)
            {
                var ragdollPart = with as RagdollPart;
                Duck duck = ragdollPart.doll._duck;
                duck.Swear();
                ragdollPart.hSpeed = this.hSpeed * 3.1f;
                ragdollPart.vSpeed -= 3.3f;
            };

            if (with is Duck)
            {
                Duck duck = with as Duck;
                duck.hSpeed += this.hSpeed * 1.1f;
                duck.vSpeed -= 3.3f;
                (with as Duck).GoRagdoll();
                Event.Log((Event)new DartHitEvent(this.responsibleProfile, duck.profile));
                if (duck.holdObject is Grenade)
                    duck.forceFire = true;
                duck.Swear();
                duck.ThrowItem(true);
            };
            this._stuck = true;
            Level.Remove((Thing)this);
        }
    }
}