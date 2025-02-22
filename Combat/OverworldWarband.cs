using Godot;
using System;
using System.Collections.Generic;

public partial class OverworldWarband : CharacterBody3D
{
	public string warbandName;
	public CivilizationType civilizationAffiliation;
	public List<Troop> troops;
	public bool isHostileToPlayer;

	private bool hasGoal = false;
	private bool isMoving = false;
	private bool goingToSettlement = false;
	private Vector3 movementTarget = Vector3.Zero;

	private float speed = 5f;

	public void CreateWarband(int numberOfTroops, TroopType favoredTroopType, int averageStrength)
	{
		int currentMax = numberOfTroops;

		for (int i = 0; i < 3; i++)
		{
			int quantity = 0;
			if ((int)favoredTroopType == i)
			{
				quantity = GD.RandRange(1, currentMax - (currentMax / 4));
			}
			else
			{
				quantity = GD.RandRange(0, currentMax - (currentMax / 3));
			}

			for (int j = 0; j < 5; j++)
			{
				if (quantity <= 0)
				{
					break;
				}

				int denominator = 1;

				if ((j + 1) - averageStrength != 0)
				{
					denominator = (j + 1) - averageStrength;
				}

				int numberAtTier = GD.RandRange(0 + Mathf.Abs(j - 4), quantity / denominator);

				if (numberAtTier > 0)
				{
					troops.Add(new Troop(numberAtTier, (TroopType)i, j));
					quantity -= numberAtTier;
				}
			}

			currentMax -= quantity;
		}

		WarbandBehavior();
	}

	public async void WarbandBehavior()
	{
		while (true)
		{
			await ToSignal(GetTree().CreateTimer(5f), Timer.SignalName.Timeout);

			if (!hasGoal)
			{
				int decision = GD.RandRange(0, 2);

				if (decision == 0) // Decide to stand around
				{
					isMoving = false;
				}
				else if (decision == 1 || civilizationAffiliation == CivilizationType.None) // Move to a random nearby point
				{
					movementTarget = new Vector3((float)GD.RandRange(Position.X - 10f, Position.X + 10f), 0f,
												 (float)GD.RandRange(Position.Z - 10f, Position.Z + 10f));
					isMoving = true;
					hasGoal = true;
				}
				else // Go to a settlement
				{
					int targetSettlementID = GD.RandRange(0,
								CivilizationHolder.Instance.civilizations[(int)civilizationAffiliation].settlements.Length);

					movementTarget = GetNode<Node3D>(civilizationAffiliation.ToString()).GetChild<Node3D>(targetSettlementID).Position;
					isMoving = true;
					hasGoal = true;
					goingToSettlement = true;
				}
			}
		}	
	}

	public override void _Process(double delta)
	{
		if (isMoving)
		{
			Vector3 velocity = Velocity;

			Vector3 direction = GlobalPosition.DirectionTo(movementTarget);
			
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;

			if (GlobalPosition.DistanceSquaredTo(movementTarget) < 0.01f)
			{
				isMoving = false;

				if (goingToSettlement)
				{
					GetParent().RemoveChild(this);
					QueueFree();
				}

				return;
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}
}
