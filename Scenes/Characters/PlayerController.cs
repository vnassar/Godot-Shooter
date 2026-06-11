using System;
using Godot;

public partial class PlayerController : CharacterBody3D
{
	[ExportGroup("Movement")]
	[Export] public float RunSpeed { get; set; } = 8;
	[Export] public float WalkSpeed { get; set; } = 5.0f;
	[Export] public float JumpVelocity { get; set; } = 4.5f;
	[Export] public float Gravity { get; set; } = 18.0f;
	[Export] public float GroundAcceleration { get; set; } = 18.0f;
	[Export] public float GroundDeceleration { get; set; } = 14.0f;
	[Export] public float AirAcceleration { get; set; } = 4.0f;

	[ExportGroup("Crouch")]
	[Export] public float StandingHeight { get; set; } = 1.6f;
	[Export] public float CrouchingHeight { get; set; } = 0.9f;
	[Export] public float StandingEyeHeight { get; set; } = 1.6f;
	[Export] public float CrouchingEyeHeight { get; set; } = 1.0f;
	[Export] public float CrouchSpeed { get; set; } = 2.8f;
	[Export] public float CrouchTransitionSpeed { get; set; } = 10.0f;


	[ExportGroup("Mouse Look")]
	[Export] public float MouseSensitivity { get; set; } = 0.0025f;

	private Node3D _head = null;
	private float _cameraPitch;

	// public const float Speed = 5.0f;
	private CollisionShape3D _collisionShape = null;
	private CapsuleShape3D _capsuleShape = null;
	private bool _isCrouching;

	private Weapon _activeWeapon = null!;


	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");

		_collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
		
		_activeWeapon = GetNode<Weapon>("Head/Camera3D/WeaponHolder/Rifle");
		_capsuleShape = _collisionShape.Shape as CapsuleShape3D ?? throw new InvalidOperationException("CollisionShape3D must use a CapsuleShape3D.");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			RotateY(-mouseMotion.Relative.X * MouseSensitivity);

			_cameraPitch -= mouseMotion.Relative.Y * MouseSensitivity;
			_cameraPitch = Mathf.Clamp(_cameraPitch, Mathf.DegToRad(-89), Mathf.DegToRad(89));

			_head.Rotation = new Godot.Vector3(_cameraPitch, 0, 0);
		}

		if (Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		
		if (Input.IsActionPressed("shoot")) _activeWeapon.Fire();
	}

	

	public override void _PhysicsProcess(double delta)
	{
		Godot.Vector3 velocity = Velocity;

		HandleCrouch(delta);
		ApplyGravity(ref velocity, delta);
		HandleJump(ref velocity);
		HandleMovement(ref velocity, delta);

		Velocity = velocity;
		MoveAndSlide();

		float horizontalSpeed = new Vector2(Velocity.X, Velocity.Z).Length();

	}

	private void ApplyGravity(ref Godot.Vector3 velocity, double delta)
	{
		if (!IsOnFloor())
		{
			velocity.Y -= Gravity * (float)delta;
		}
	}

	private void HandleJump(ref Godot.Vector3 velocity)
	{
		if (Input.IsActionJustPressed("jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
		}
	}

	private void HandleMovement(ref Godot.Vector3 velocity, double delta)
	{
		Godot.Vector2 inputDirection = Input.GetVector(
			"move_left",
			"move_right",
			"move_forward",
			"move_backward"
		);

		Godot.Vector3 direction = Transform.Basis * new Vector3(inputDirection.X, 0, inputDirection.Y);
		direction = direction.Normalized();

		float targetSpeed = GetTargetSpeed();

		Vector3 targetvelocity = direction * targetSpeed;

		float acceleration = IsOnFloor() ? GroundAcceleration : AirAcceleration;
		float deceleration = IsOnFloor() ? GroundDeceleration : AirAcceleration;

		if (direction != Vector3.Zero)
		{
			velocity.X = Mathf.MoveToward(velocity.X, targetvelocity.X, acceleration * (float)delta);
			velocity.Z = Mathf.MoveToward(velocity.Z, targetvelocity.Z, acceleration * (float)delta);
		}
		else
		{
			velocity.X = Mathf.MoveToward(
            velocity.X,
            0,
            deceleration * (float)delta
        );

        velocity.Z = Mathf.MoveToward(
            velocity.Z,
            0,
            deceleration * (float)delta
        );
		}
	}

	private float GetTargetSpeed()
	{
		if (_isCrouching) return CrouchSpeed;

		if (Input.IsActionPressed("walk")) return WalkSpeed;

		return RunSpeed;
	}

	private void HandleCrouch(double delta)
	{
		_isCrouching = Input.IsActionPressed("crouch");

		float targetCapsuleHeight = _isCrouching ? CrouchingHeight : StandingHeight;

		float targetEyeHeight = _isCrouching ? CrouchingEyeHeight : StandingEyeHeight;

		_capsuleShape.Height = Mathf.MoveToward(_capsuleShape.Height, targetCapsuleHeight, CrouchTransitionSpeed * (float)delta);

		_head.Position = new Vector3(_head.Position.X, Mathf.MoveToward(_head.Position.Y, targetEyeHeight, CrouchTransitionSpeed * (float)delta), _head.Position.Z);

		_collisionShape.Position = new Vector3(_collisionShape.Position.X, _capsuleShape.Height / 2.0f, _collisionShape.Position.Z);
	}
}
