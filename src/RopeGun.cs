using DuckGame;

namespace KzDuckMods
{
    [EditorGroup("KzMod")]
    public class RopeGun : Gun
    {
        private Vec2 _ropeTargetPosition;
        private bool _isRopeExtending = false;
        private bool _isRopeRetracting = false;
        private Sprite _hookSprite;

        private float _ropeSpeed = 80f;
        private float _ropeExtendSpeedRate = 0.1f;
        private float _ropeRetractSpeedRate = 0.2f;
        private float _ropeMaxDistance = 170f;
        private float _detectRadius = 6.5f;
        private float _heatUpRate = 0.009f;
        private float _coolDownRate = -0.003f;
        private float _duckGrabHelditemRage = 15f;

        //kickback?
        //random direction rate , could depend on how hot it is
        
        private Thing _heldObject;

        private Sound soundEffect = null;
        private SpriteMap _sprite;

        public RopeGun(float xval, float yval) : base(xval, yval)
        {
            this._sprite = new SpriteMap(Mod.GetPath<KzMod>("RopeGun"), 32, 32, false);
            this._sprite.frame = 0;
            this.graphic = (Sprite)this._sprite;
            this.center = new Vec2(16f, 16f);
            this.collisionOffset = new Vec2(-8f, -4f);
            this.collisionSize = new Vec2(16f, 9f);
            this._barrelOffsetTL = new Vec2(27f, 14f);

            _hookSprite = new Sprite("hook");
            _hookSprite.center = new Vec2(4, 4);
        }

        public override void Update()
        {
            base.Update();

            ammo = 99;
            if (_isRopeExtending)
            {
                ExtendRope();
                DoHeatUp(_heatUpRate, position);
            }
            else if (_isRopeRetracting)
            {
                RetractRope();
                DoHeatUp(_heatUpRate, position);
            }
            else
            {
                DoHeatUp(_coolDownRate, position);
            }
        }

        public override void OnPressAction()
        {
            if (!_isRopeExtending && !_isRopeRetracting)
            {
                FireRope();
            }
        }

        private void FireRope()
        {
            this.soundEffect = SFX.Play(Mod.GetPath<KzMod>("sounds/sliding-rope-3.wav"), 1f, 0.0f, 0.0f, false);
            this._sprite.frame = 1;
            this.graphic = (Sprite)this._sprite;

            _hookSprite.position = new Vec2(this.barrelPosition.x, this.barrelPosition.y);
            Vec2 aimDirection = this.barrelVector;
            _ropeTargetPosition = _hookSprite.position + (aimDirection * _ropeMaxDistance);
            SetExpandRope();
        }

        private void ExtendRope()
        {
            Vec2 aimDirection = (_ropeTargetPosition - _hookSprite.position).normalized;
            _hookSprite.position += aimDirection * _ropeSpeed * _ropeExtendSpeedRate;

            if (_heldObject != null || !_isRopeExtending || _isRopeRetracting)
            {
                SetRetractRope();
                return;
            }

            if (ExtendCheckForWalls()) return;
            if (ExtendCheckForDuck()) return;
            if (ExtendCheckForHoldable()) return;
            if (ExtendCheckForRagdoll()) return;

            if ((_hookSprite.position - _ropeTargetPosition).length <= 5f)
            {
                SetRetractRope();
            }
        }
        private bool ExtendCheckForWalls()
        {
            bool isThereAWall = false;
            Block wall = Level.CheckCircle<Block>(_hookSprite.position, _detectRadius);
            if (wall != null)
            {
                _heldObject = null;
                SetRetractRope();
                isThereAWall = true;
            }

            return isThereAWall;
        }
        private bool ExtendCheckForDuck()
        {
            bool isThereADuck = false;

            Duck duck = Level.CheckCircle<Duck>(_hookSprite.position, _detectRadius);
            if (duck != null
                && duck.ragdoll == null
                && duck._trapped == null
                && duck != this.owner
                )
            {
                if (duck.holdObject != null && !(duck.holdObject is Hat) && !(duck.holdObject is Equipment))
                {
                    _heldObject = duck.holdObject;
                    duck.holdObject = null;
                }
                else
                {
                    duck.Scream();
                    duck.GoRagdoll();
                    if (duck.ragdoll != null)
                    {
                        _heldObject = duck.ragdoll;
                    }
                }

                SetRetractRope();
                isThereADuck = true;
            }
            return isThereADuck;
        }
        private bool ExtendCheckForHoldable()
        {
            bool isThereAHoldable = false;
            Holdable holdable = Level.CheckCircle<Holdable>(_hookSprite.position, _detectRadius);
            if (holdable != null
                && holdable.owner == null
                && holdable != this
                && !(holdable is Hat)
                && !(holdable is RagdollPart)
                && holdable.GetType() != typeof(Hat))
            {
                _heldObject = holdable;
                _heldObject.enablePhysics = false;
                _heldObject.hSpeed = 0;
                _heldObject.vSpeed = 0;

                SetRetractRope();
                isThereAHoldable = true;
            }
            return isThereAHoldable;
        }
        private bool ExtendCheckForRagdoll()
        {
            bool isThereARagdoll = false;
            Ragdoll ragdoll = Level.CheckCircle<Ragdoll>(_hookSprite.position, _detectRadius);
            if (ragdoll != null)
            {
                _heldObject = ragdoll;

                if (ragdoll._duck != null && !ragdoll._duck.dead)
                {
                    ragdoll._duck.Scream();
                }

                SetRetractRope();
                isThereARagdoll = true;
            }
            return isThereARagdoll;
        }

        private void RetractRope()
        {
            Vec2 retractDirection = (this.barrelPosition - _hookSprite.position).normalized;
            _hookSprite.position += retractDirection * _ropeSpeed * _ropeRetractSpeedRate;

            RetractHeldObject();

            if ((_hookSprite.position - this.barrelPosition).length < _duckGrabHelditemRage)
            {
                GrabObjectComingBack();

                if (this.soundEffect != null)
                {
                    this.soundEffect.Stop();
                }

                this._sprite.frame = 0;
                this.graphic = (Sprite)this._sprite;

                _isRopeRetracting = false;
                _isRopeExtending = false;
            }
        }

        private void RetractHeldObject()
        {
            if (_heldObject != null)
            {
                if (_heldObject is Ragdoll ragdoll)
                {
                    if (ragdoll.part1 != null) ragdoll.part1.position = Vec2.Lerp(ragdoll.part1.position, _hookSprite.position, 0.3f);
                    if (ragdoll.part2 != null) ragdoll.part2.position = Vec2.Lerp(ragdoll.part2.position, _hookSprite.position, 0.3f);
                    if (ragdoll.part3 != null) ragdoll.part3.position = Vec2.Lerp(ragdoll.part3.position, _hookSprite.position, 0.3f);
                }
                else
                {
                    _heldObject.position = Vec2.Lerp(_heldObject.position, _hookSprite.position, 0.3f);
                }
            }
        }

        private void GrabObjectComingBack()
        {
            if (_heldObject != null && owner != null && owner is Duck duck)
            {
                Holdable holdable = null;

                if (_heldObject is Ragdoll ragdoll && ragdoll.part1 != null)
                {
                    holdable = ragdoll.part1;
                }
                else if (_heldObject is Holdable h)
                {
                    holdable = h;
                }

                if (holdable != null)
                {
                    duck.GiveHoldable(holdable);
                    TossWeapon();
                    _heldObject = null;
                    return;
                }
            }

            if (_heldObject != null
                && _heldObject is PhysicsObject physicsObject)
            {
                physicsObject.vSpeed = 1;
                physicsObject.hSpeed = 1;
                physicsObject.enablePhysics = true;
                physicsObject.position = Vec2.Lerp(physicsObject.position, _hookSprite.position, 0.1f);
            }

            _heldObject = null;
        }

        private void TossWeapon()
        {
            this.hSpeed = this.offDir * Rando.Float(-4f, -2f);
            this.vSpeed = Rando.Float(-4f, -2f);
        }

        public override void Draw()
        {
            Graphics.material = null;
            base.Draw();
            if (_isRopeExtending || _isRopeRetracting)
            {
                Vec2 lineStart = this.barrelPosition + this.barrelVector * -8f;
                Graphics.DrawLine(
                    lineStart,
                    _hookSprite.position,
                    Color.AntiqueWhite,
                    1.5f);
                Graphics.Draw(_hookSprite, _hookSprite.position.x, _hookSprite.position.y);
            }
        }

        private void SetRetractRope()
        {
            _isRopeExtending = false;
            _isRopeRetracting = true;
        }
        private void SetExpandRope()
        {
            _isRopeExtending = true;
            _isRopeRetracting = false;
        }
    }
}
