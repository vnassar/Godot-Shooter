using Godot;
using System;

public partial class TargetDummy : StaticBody3D, IDamageable
{
	[Export] public int MaxHealth {get;set;} = 100;

	private int _currentHealth;

    public override void _Ready()
    {
        _currentHealth = MaxHealth;
		GD.Print($"Target spawned with {_currentHealth} health.");
    }

	public void TakeDamage(int amount)
	{
		_currentHealth -= amount;

		GD.Print($"Target took {amount} damage. Health remaining: {_currentHealth}");

		if (_currentHealth <= 0)
		{
			Die();
		}
	}

	private void Die()
	{
		GD.Print("Target destroyed. It died...");
		QueueFree();
	}

}
