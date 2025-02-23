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

	private int getTroopPower(Troop troop)
	{
		if (troop.tier == 1)
		{
			return 10;
		}
		else if (troop.tier == 2)
		{
			return 20;
		}
		else if (troop.tier == 3)
		{
			return 40;
		}
		else if (troop.tier == 4)
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
		if (troop.tier == 1)
		{
			return 1 / 2;
		}
		else if (troop.tier == 2)
		{
			return 1 / 4;
		}
		else if (troop.tier == 3)
		{
			return 1 / 8;
		}
		else if (troop.tier == 4)
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
			// less losses if you win
			int powerDiff = playerPower - enemyPower;
			totalTroopsLost = (int)Math.Floor(Math.Pow(powerDiff / 10, 1 / 3));
		}
		else
		{
			int powerDiff = playerPower - enemyPower;
			totalTroopsLost = (int)Math.Floor(Math.Sqrt(powerDiff / 10));
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

	public void beginCombat(Player player, OverworldWarband warband, Terrain terrain)
	{
		// determine the power of both sides, and whoever has more wins
		// the power differential will determine how many of each side dies after the battle
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

		Troop[] lostTroops = calculateTroopLoss(player.troops.ToArray(), playerPower, enemyPower);
		foreach (Troop troop in lostTroops)
		{
			Troop existingElement = Array.Find(player.troops.ToArray(), playerTroop => playerTroop.troopType == troop.troopType && playerTroop.tier == troop.tier);
			Troop[] newTroops = player.troops.ConvertAll(playerTroop =>
			playerTroop.troopType == troop.troopType && playerTroop.tier == troop.tier ?
				new Troop(playerTroop.quantity - troop.quantity, playerTroop.troopType, playerTroop.tier) :
				playerTroop)
			.ToArray();
			// player troops = new troops
		}
		GD.Print("Battle over");
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
