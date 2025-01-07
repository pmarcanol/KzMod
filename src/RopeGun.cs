using DuckGame;

[EditorGroup("KzMod")]
public class RopeGun : Gun
{
    private Vec2 _ropeTargetPosition;
    private bool _isRopeExtending = false;
    private bool _isRopeRetracting = false;
    private Sprite _hookSprite;

    private float _ropeSpeed = 80f;
    private float _ropeMaxDistance = 170f;

    private Thing _heldObject;
    private Duck _heldDuck;
    private float _detectRadius = 6f;

    public RopeGun(float xval, float yval) : base(xval, yval)
    {
        graphic = new Sprite("magnetGun");
        this.center = new Vec2(16f, 16f);
        this.collisionOffset = new Vec2(-8f, -4f);
        this.collisionSize = new Vec2(14f, 9f);
        this._barrelOffsetTL = new Vec2(24f, 14f);

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
            DoHeatUp(0.009f, position);
        }
        else if (_isRopeRetracting)
        {
            RetractRope();
            DoHeatUp(0.009f, position);
        }
        else
        {
            DoHeatUp(-0.003f, position);
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
        _hookSprite.position = new Vec2(this.barrelPosition.x, this.barrelPosition.y);
        Vec2 aimDirection = this.barrelVector;
        _ropeTargetPosition = _hookSprite.position + (aimDirection * _ropeMaxDistance);
        SetExpandRope();
    }

    private void ExtendRope()
    {
        Vec2 aimDirection = (_ropeTargetPosition - _hookSprite.position).normalized;
        _hookSprite.position += aimDirection * _ropeSpeed * 0.1f;

        if (_heldObject != null || !_isRopeExtending || _isRopeRetracting)
        {
            SetRetractRope();
            return;
        }

        if (ExtendCheckForWalls()) return;
        if (ExtendCheckForHoldable()) return;
        if (ExtendCheckForDuck()) return;
        if (ExtendCheckForRagdoll()) return;

        if ((_hookSprite.position - _ropeTargetPosition).length <= 5f)
        {
            SetRetractRope();
        }
    }

    private bool ExtendCheckForWalls()
    {
        bool isThereAWall = false;
        Block ragdoll = Level.CheckCircle<Block>(_hookSprite.position, _detectRadius);
        if (ragdoll != null)
        {
            _heldObject = null;
            SetRetractRope();
            isThereAWall = true;
        }
        return isThereAWall;
    }

    private bool ExtendCheckForRagdoll()
    {
        bool isThereAHoldable = false;
        Ragdoll ragdoll = Level.CheckCircle<Ragdoll>(_hookSprite.position, _detectRadius);
        if (ragdoll != null)
        {
            _heldObject = ragdoll;
            SetRetractRope();
            isThereAHoldable = true;
        }
        return isThereAHoldable;
    }

    private bool ExtendCheckForHoldable()
    {
        bool isThereAHoldable = false;
        Holdable weapon = Level.CheckCircle<Holdable>(_hookSprite.position, _detectRadius);
        if (weapon != null)
        {
            if (weapon.owner != null && weapon.owner is Duck currentOwner)
            {
                currentOwner.holdObject = null;
                weapon.owner = null;
            }

            _heldObject = weapon;
            SetRetractRope();

            isThereAHoldable = true;
        }
        return isThereAHoldable;
    }

    private bool ExtendCheckForDuck()
    {
        bool isThereADuck = false;

        Duck duck = Level.CheckCircle<Duck>(_hookSprite.position, _detectRadius);
        if (duck != null)
        {
            if (duck.holdObject != null)
            {
                //ToDo steal equipment?
                _heldObject = duck.holdObject;
                duck.holdObject = null;
            }
            else
            {
                this._heldDuck = duck;
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


    private void RetractRope()
    {
        Vec2 retractDirection = (this.barrelPosition - _hookSprite.position).normalized;
        _hookSprite.position += retractDirection * _ropeSpeed * 0.2f;

        RetractHeldObject();

        if ((_hookSprite.position - this.barrelPosition).length < 15f)
        {
            GrabObjectComingBack();

            _isRopeRetracting = false;
            _isRopeExtending = false;
        }
    }

    private void GrabObjectComingBack()
    {
        if (_heldObject != null && owner != null && owner is Duck)
        {
            //ToDo make sure if the owner is not a duck (maybe a magnet gun IDK) to drop it or something no idea
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
                ((Duck)owner).GiveHoldable((Holdable)holdable);
                _heldObject = null;
            }

            if (_heldObject is PhysicsObject physicsObject)
            {
                physicsObject.vSpeed = 0;
                physicsObject.hSpeed = 0;
            }

            TossWeapon();
        }



        _heldObject = null;
        _heldDuck = null;
    }

    private void RetractHeldObject()
    {
        if (_heldObject == null && _heldDuck != null)
        {
            //I think this would mean the duck in ragdoll got outside of the ragdoll , maybe netted, maybe just stood up
        }

        if (_heldObject != null)
        {
            if (_heldObject is PhysicsObject physicsObject)
            {
                physicsObject.vSpeed = 0;
                physicsObject.hSpeed = 0;
            }

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
            Graphics.DrawLine(this.barrelPosition, _hookSprite.position, Color.White, 1f);
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