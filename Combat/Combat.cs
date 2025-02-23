using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public enum Terrain
{
	Plains,
	River,
	Hills,
	Mountain,
	Settlement
}

public partial class Combat : Node
{
	private OverworldWarband currentEnemy;
	private Terrain currentTerrain;
	private int playerPower;
	private int enemyPower;
	private bool isInCombat = false;

	private int getTroopPower(Troop troop)
	{
		if (troop.tier == 0)
		{
			return 10;
		}
		else if (troop.tier == 1)
		{
			return 20;
		}
		else if (troop.tier == 2)
		{
			return 40;
		}
		else if (troop.tier == 3)
		{
			return 80;
		}
		else
		{
			return 160;
		}
	}

	private double determineModifier(Troop troop, Terrain terrain)
	{
		double troopModifier = 1;
		switch (terrain)
		{
			case Terrain.Plains:
				if (troop.troopType == TroopType.Infantry)
				{
					return 0.7;
				}
				else if (troop.troopType == TroopType.Archer)
				{
					return 1;
				}
				else if (troop.troopType == TroopType.Cavalry)
				{
					return 1.3;
				}
				break;
			case Terrain.Hills:
				if (troop.troopType == TroopType.Infantry)
				{
					return 1.1;
				}
				else if (troop.troopType == TroopType.Archer)
				{
					return 1.3;
				}
				else if (troop.troopType == TroopType.Cavalry)
				{
					return 0.8;
				}
				break;
			case Terrain.River:
				if (troop.troopType == TroopType.Infantry)
				{
					return 0.9;
				}
				else if (troop.troopType == TroopType.Archer)
				{
					return 1.1;
				}
				else if (troop.troopType == TroopType.Cavalry)
				{
					return 1.1;
				}
				break;
			case Terrain.Mountain:
				if (troop.troopType == TroopType.Infantry)
				{
					return 1.3;
				}
				else if (troop.troopType == TroopType.Archer)
				{
					return 0.7;
				}
				else if (troop.troopType == TroopType.Cavalry)
				{
					return 0.5;
				}
				break;
			case Terrain.Settlement:
				if (troop.troopType == TroopType.Infantry)
				{
					return 1;
				}
				else if (troop.troopType == TroopType.Archer)
				{
					return 0.7;
				}
				else if (troop.troopType == TroopType.Cavalry)
				{
					return 1;
				}
				break;
		}
		return troopModifier;
	}

	private int calculateTroopPower(Troop troop, Terrain terrain, Random random)
	{
		double modifier = determineModifier(troop, terrain);
		int power = getTroopPower(troop);
		double randomness = random.NextDouble() * (1.2 - 0.8) + 0.8;
		double totalPower = modifier * power * troop.quantity * randomness;
		return (int)Math.Floor(totalPower);
	}

	// tier 1 units: 10 base power
	// tier 2 units: 20 base power
	// tier 3 units: 40 base power
	// tier 4 units: 80 base power
	// tier 5 units: 160 base power

	// mages have double the power

	private double getTroopLossPercent(Troop troop)
	{
		if (troop.tier == 0)
		{
			return 1 / 2;
		}
		else if (troop.tier == 1)
		{
			return 1 / 4;
		}
		else if (troop.tier == 2)
		{
			return 1 / 8;
		}
		else if (troop.tier == 3)
		{
			return 1 / 16;
		}
		else
		{
			return 1 / 32;
		}
	}

	private Troop[] calculateTroopLoss(Troop[] playerTroops, int playerPower, int enemyPower)
	{
		List<Troop> lostTroops = new List<Troop>();
		int totalTroopsLost = 0;
		if (playerPower > enemyPower)
		{
			int totalTroopQuantity = 0;
			foreach (Troop troop in  playerTroops) {
				totalTroopQuantity += troop.quantity;
			}
			// lose 10% of your troops at equal power, and 0% at double power
			float powerMultiplier = playerPower / enemyPower;

			totalTroopsLost = (int)Math.Floor(totalTroopQuantity * 0.1 / powerMultiplier);
		}
		else
		{
			int powerDiff = enemyPower - playerPower;
			totalTroopsLost = (int)Math.Floor(Math.Pow(powerDiff / 10, 1 / 5));
		}

		Dictionary<int, List<Troop>> troopsByLevel = new Dictionary<int, List<Troop>>();
		foreach (Troop troop in playerTroops)
		{
			if (!troopsByLevel.ContainsKey(troop.tier))
			{
				troopsByLevel.Add(troop.tier, new List<Troop>());
			}
			troopsByLevel[troop.tier].Add(troop);
		}
		foreach (List<Troop> troops in troopsByLevel.Values.ToList())
		{
			foreach (Troop troop in troops)
			{
				int lostAmount = (int)Math.Floor(totalTroopsLost * getTroopLossPercent(troop) / 3);
				lostTroops.Add(new Troop(troop.quantity - lostAmount, troop.troopType, troop.tier));
			}
		}
		return lostTroops.ToArray();
	}

	public bool BeginCombat()
	{
		// determine the power of both sides, and whoever has more wins
		// the power differential will determine how many of each side dies after the battle
		GD.Print("Fighting");
		Player player = GetNode<Player>("/root/BaseNode/Player/PlayerData");

		Troop[] lostTroops = calculateTroopLoss(player.troops.ToArray(), playerPower, enemyPower);
		foreach (Troop troop in lostTroops)
		{
			Troop existingElement = Array.Find(player.troops.ToArray(), playerTroop => playerTroop.troopType == troop.troopType && playerTroop.tier == troop.tier);
			Troop[] newTroops = player.troops.ConvertAll(playerTroop =>
			playerTroop.troopType == troop.troopType && playerTroop.tier == troop.tier ?
				new Troop(playerTroop.quantity - troop.quantity, playerTroop.troopType, playerTroop.tier) :
				playerTroop)
			.ToArray();
			player.troops = newTroops.ToList();
		}

		/*foreach (Troop troop in player.troops)
		{
			if (troop.quantity <= 0)
			{
				player.troops.Remove(troop);
			}
		}*/

		for (int i = 0; i < player.troops.Count; i++)
		{
			if (player.troops[i].quantity <= 0)
			{
				player.troops.Remove(player.troops[i]);
				i--;
			}
		}

		ShowCombatResults(playerPower > enemyPower, lostTroops);

		return playerPower > enemyPower;
	}

	void ShowCombatResults(bool won, Troop[] lostTroops)
	{
		GetNode<Control>("/root/BaseNode/UI/BattleUI/EncounterBackground").Visible = false;
		VBoxContainer container;

		if (won)
		{
			container = GetNode<VBoxContainer>("/root/BaseNode/UI/BattleUI/VictoryBackground/VBoxContainer");

			currentEnemy.GetParent().RemoveChild(currentEnemy);
			currentEnemy.QueueFree();

			GetNode<Control>("/root/BaseNode/UI/BattleUI/VictoryBackground").Visible = true;
		}
		else
		{
			container = GetNode<VBoxContainer>("/root/BaseNode/UI/BattleUI/DefeatBackground/VBoxContainer");

			int civilization = GD.RandRange(0, CivilizationHolder.Instance.civilizations.Length - 1);

			Node3D settlementParent = GetNode<Node3D>("/root/BaseNode/" + (CivilizationType)civilization);
			SettlementData targetSettlement = settlementParent.GetChild<SettlementData>(GD.RandRange(0, settlementParent.GetChildCount() - 1));

			GetNode<RichTextLabel>("/root/BaseNode/UI/BattleUI/DefeatBackground/Info").Text = "[center]You have been defeated on the field of combat.\n"
						 	+ "You have been routed to " + targetSettlement.settlementName + ", and have lost the following troops:";

			GetNode<PlayerController>("/root/BaseNode/Player").Position = targetSettlement.Position;

			GetNode<Control>("/root/BaseNode/UI/BattleUI/DefeatBackground").Visible = true;
		}

		foreach (RichTextLabel label in container.GetChildren())
		{
			container.RemoveChild(label);
			label.QueueFree();
		}

		for (int i = 0; i < lostTroops.Length; i++)
		{
			RichTextLabel label = new RichTextLabel();

			label.CustomMinimumSize = new Vector2(0, 55f);
			label.AddThemeFontSizeOverride("normal_font_size", 30);

			string unitType = "";

			switch (lostTroops[i].troopType)
			{
				case TroopType.Infantry:
					unitType = ((InfantryTroopTier)lostTroops[i].tier).ToString();
					break;
				case TroopType.Archer:
					unitType = ((ArcherTroopTier)lostTroops[i].tier).ToString();
					break;
				case TroopType.Cavalry:
					unitType = ((CavalryTroopTier)lostTroops[i].tier).ToString();
					break;
				case TroopType.Mage:
					unitType = ((MageTroopTier)lostTroops[i].tier).ToString();
					break;
			}

			unitType = unitType.Replace("_", " ");

			label.Text = lostTroops[i].quantity + " " + unitType + " (" + lostTroops[i].troopType.ToString() + ")";

			container.AddChild(label);
		}
	}

	void ExitCombat()
	{
		GetNode<Control>("/root/BaseNode/UI/BattleUI").Visible = false;
		GetNode<Control>("/root/BaseNode/UI/BattleUI/DefeatBackground").Visible = false;
		GetNode<Control>("/root/BaseNode/UI/BattleUI/VictoryBackground").Visible = false;
		SetProcess(true);
		isInCombat = false;
		GetNode<PlayerController>("/root/BaseNode/Player").IsMovementDisabled = false;
	}

	public void OpenCombatUI(Player player, OverworldWarband warband, Terrain terrain)
	{
		if (isInCombat)
		{
			return;
		}

		isInCombat = true;

		GlobalPauseState.Instance.IsPaused = true;

		SetProcess(false);

		GetNode<Control>("/root/BaseNode/UI/BattleUI").Visible = true;

		GetNode<PlayerController>("/root/BaseNode/Player").IsMovementDisabled = true;
		GetNode<PlayerController>("/root/BaseNode/Player").IsMoving = false;

		Control battleUI = GetNode<Control>("/root/BaseNode/UI/BattleUI/EncounterBackground");

		int playerPower = 0;
		int enemyPower = 0;
		Random random = new();
		foreach (Troop troop in player.troops)
		{
			playerPower += calculateTroopPower(troop, terrain, random);
		}

		foreach (Troop troop in warband.troops)
		{
			enemyPower += calculateTroopPower(troop, terrain, random);
		}

		battleUI.GetNode<RichTextLabel>("Info").Text = "[center]You have been challenged by [b]" + warband.warbandName + "[/b].\nTheir strength "
			+ "is " + enemyPower + ".\nYour strength is " + playerPower + ".";
		battleUI.Visible = true;

		this.playerPower = playerPower;
		this.enemyPower = enemyPower;
		currentEnemy = warband;
		currentTerrain = terrain;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
