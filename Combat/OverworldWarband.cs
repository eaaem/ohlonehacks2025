using Godot;
using System;
using System.Collections.Generic;

public partial class OverworldWarband : CharacterBody3D
{
	public string warbandName;
	public CivilizationType civilizationAffiliation;
	private List<Troop> troops = new List<Troop>();
	public bool isHostileToPlayer;

	private bool hasGoal = false;
	private bool isMoving = false;
	private bool goingToSettlement = false;
	private Vector3 movementTarget = Vector3.Zero;

	PlayerController player;

	private float speed = 5f;

    public override void _Ready()
    {
		player = GetNode<PlayerController>("/root/BaseNode/Player");

		MouseEntered += OnMouseEntered;
		MouseExited += OnMouseExited;
    }

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

					movementTarget = CivilizationHolder.Instance.civilizations[(int)civilizationAffiliation].settlements[targetSettlementID].Position;
					isMoving = true;
					hasGoal = true;
					goingToSettlement = true;
				}
			}
		}	
	}

    public void OnMouseEntered()
	{
		Control tooltip = GD.Load<PackedScene>("res://Combat/warband_tooltip.tscn").Instantiate<Control>();

		tooltip.GetNode<RichTextLabel>("Title").Text = "[center]" + warbandName;

		PackedScene unitLabelScene = GD.Load<PackedScene>("res://Combat/unit_label.tscn");

		for (int i = 0; i < troops.Count; i++)
		{
			RichTextLabel label = unitLabelScene.Instantiate<RichTextLabel>();
			string unitType = "";

			switch (troops[i].troopType)
			{
				case TroopType.Infantry:
					unitType = ((InfantryTroopTier)troops[i].tier).ToString();
					break;
				case TroopType.Archer:
					unitType = ((ArcherTroopTier)troops[i].tier).ToString();
					break;
				case TroopType.Cavalry:
					unitType = ((CavalryTroopTier)troops[i].tier).ToString();
					break;
				case TroopType.Mage:
					unitType = ((MageTroopTier)troops[i].tier).ToString();
					break;
			}

			unitType = unitType.Replace("_", " ");

			label.Text = troops[i].quantity + " " + unitType + " (" + troops[i].troopType.ToString() + ")";
			tooltip.GetNode<VBoxContainer>("VBoxContainer").AddChild(label);
		}

		tooltip.GetNode<Panel>("Panel").Size = new Vector2(250, 25 + (tooltip.GetNode<VBoxContainer>("VBoxContainer").GetChildCount() * 20));

		tooltip.Name = "WarbandTooltip";
		tooltip.Position = GetViewport().GetMousePosition();
		GetNode<Node3D>("/root/BaseNode").AddChild(tooltip);
	}

	public void OnMouseExited()
	{
		Control tooltip = GetNode<Control>("/root/BaseNode/WarbandTooltip");
		GetNode<Node3D>("/root/BaseNode").RemoveChild(tooltip);
		tooltip.QueueFree();
	}

	public void OnPlayerEntered(Node3D body)
	{
		if (isHostileToPlayer)
		{
			GetNode<Combat>("/root/BaseNode/Combat").beginCombat(player.GetNode<Player>("PlayerData").troops.ToArray(), troops.ToArray(), Terrain.Plains);
		}
	}

    public override void _Process(double delta)
	{
		if (isHostileToPlayer)
		{
			float distance = Position.DistanceSquaredTo(player.Position);

			if (distance < 15f)
			{
				movementTarget = player.Position;
				isMoving = true;
			}
			else
			{
				isMoving = false;
			}
		}

		if (isMoving)
		{
			Vector3 velocity = Velocity;

			Vector3 direction = GlobalPosition.DirectionTo(movementTarget);
			
			velocity.X = direction.X * speed;
			velocity.Z = direction.Z * speed;

			Velocity = velocity;
			MoveAndSlide();

			if (GlobalPosition.DistanceSquaredTo(movementTarget) < 0.01f)
			{
				isMoving = false;

				if (goingToSettlement)
				{
					SetPhysicsProcess(false);
					GetParent().RemoveChild(this);
					QueueFree();
				}

				return;
			}
		}
	}
}
