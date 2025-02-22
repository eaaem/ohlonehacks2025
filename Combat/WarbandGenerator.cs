using Godot;
using System;

public partial class WarbandGenerator : Node
{
	[Export]
	private PackedScene warbandScene;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GenerateWarbandsOverTime();
	}

	async void GenerateWarbandsOverTime()
	{
		while (true)
		{
			await ToSignal(GetTree().CreateTimer(5f), Timer.SignalName.Timeout);

			int chance = GD.RandRange(0, 1);

			if (chance == 0)
			{
				OverworldWarband warband = GD.Load<PackedScene>(warbandScene.ResourcePath).Instantiate<OverworldWarband>();

				int affiliation = GD.RandRange(0, CivilizationHolder.Instance.civilizations.Length + 3);

				if (affiliation < CivilizationHolder.Instance.civilizations.Length - 2)
				{
					SettlementData settlementToSpawnAt = CivilizationHolder.Instance.civilizations[affiliation].settlements[GD.RandRange(0, 
															CivilizationHolder.Instance.civilizations[affiliation].settlements.Length)];

					float troopAmountModifier = ((int)settlementToSpawnAt.militaryStrength + 1) / 2f;

					warband.warbandName = "Troops of " + CivilizationHolder.Instance.civilizations[affiliation].civilizationName;
					//warband.CreateWarband(GD.RandRange(30, 45) * troopAmountModifier, TroopType.Archer, GD.RandRange(2, ))
				}
				else // Bandit
				{
					warband.warbandName = "Bandits";
					warband.CreateWarband(GD.RandRange(2, 30), TroopType.Infantry, GD.RandRange(1, 2));
				}
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
