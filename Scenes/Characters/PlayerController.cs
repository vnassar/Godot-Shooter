using Godot;

public partial class PlayerController : CharacterBody3D
{
	[ExportGroup("Movement")]
	[Export] public float RunSpeed { get; set; } = 8;
	[Export] public float WalkSpeed { get; set; } = 5.0f;
	[Export] public float JumpVelocity { get; set; } = 4.5f;
	[Export] public float Gravity { get; set; } = 18.0f;

	[ExportGroup("Mouse Look")]
	[Export] public float MouseSensitivity { get; set; } = 0.0025f;

	private Node3D _head = null;
	private float _cameraPitch;

	// public const float Speed = 5.0f;

	public override void _Ready()
	{
		_head = GetNode<Node3D>("Head");

		Input.MouseMode = Input.MouseModeEnum.Captured;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventMouseMotion mouseMotion)
		{
			_cameraPitch -= mouseMotion.Relative.Y * MouseSensitivity;
			_cameraPitch = Mathf.Clamp(_cameraPitch, Mathf.DegToRad(-89), Mathf.DegToRad(89));

			_head.Rotation = new Godot.Vector3(_cameraPitch, 0, 0);
		}

		if (Input.IsActionJustPressed("ui_cancel"))
		{
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		Godot.Vector3 velocity = Velocity;

		ApplyGravity(ref velocity, delta);
		HandleJump(ref velocity);
		HandleMovement(ref velocity, delta);

		Velocity = velocity;
		MoveAndSlide();

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

		float currentSpeed = Input.IsActionPressed("walk") ? RunSpeed : WalkSpeed;

		if (direction != Vector3.Zero)
		{
			velocity.X = direction.X * currentSpeed;
			velocity.Z = direction.Z * currentSpeed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(velocity.X, 0, WalkSpeed);
			velocity.Z = Mathf.MoveToward(velocity.Z, 0, WalkSpeed);
		}
	}
}
