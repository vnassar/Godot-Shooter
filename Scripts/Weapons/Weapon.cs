using Godot;
using System;

public partial class Weapon : Node3D
{

	[ExportGroup("Weapon Stats")]
	[Export] public string WeaponName {get;set;} = "Rifle";
	[Export] public int Damage {get;set;} = 25;
	[Export] public float Range {get;set;} = 100.0f;

	private RayCast3D _shootRay = null!;


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_shootRay = GetNode<RayCast3D>("ShootRay");

		_shootRay.TargetPosition = new Vector3(0, 0, -Range);
	}

	public void Fire()
	{
		_shootRay.ForceRaycastUpdate();

		if(!_shootRay.IsColliding())
		{
			GD.Print($"{WeaponName} fired and hit nothing");
			return;
		}

		GodotObject collider = _shootRay.GetCollider();

		GD.Print($"{WeaponName} hit: {collider}");

		if (collider is IDamageable damageable)
		{
			damageable.TakeDamage(Damage);
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
