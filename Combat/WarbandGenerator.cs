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
			await ToSignal(GetTree().CreateTimer(3f), Timer.SignalName.Timeout);

			int chance = GD.RandRange(0, 1);

			if (chance == 0)
			{
				OverworldWarband warband = GD.Load<PackedScene>(warbandScene.ResourcePath).Instantiate<OverworldWarband>();
				GetNode<Node3D>("/root/BaseNode").AddChild(warband);

				int affiliation = GD.RandRange(0, CivilizationHolder.Instance.civilizations.Length + 3);

				if (affiliation < CivilizationHolder.Instance.civilizations.Length - 2)
				{
					// TEST: REMOVE THIS LINE LATER
					affiliation = 0;
					int settlementID = GD.RandRange(0, CivilizationHolder.Instance.civilizations[affiliation].settlements.Length - 1);
					SettlementData settlementToSpawnAt = CivilizationHolder.Instance.civilizations[affiliation].settlements[settlementID];

					float troopAmountModifier = ((int)settlementToSpawnAt.militaryStrength + 1) / 2f;

					warband.warbandName = "Troops of " + CivilizationHolder.Instance.civilizations[affiliation].civilizationName;
					warband.CreateWarband((int)(GD.RandRange(30, 45) * troopAmountModifier), TroopType.Archer, 
										  GD.RandRange(2, (int)settlementToSpawnAt.militaryStrength + 1));
					warband.Position = settlementToSpawnAt.Position + (Vector3.Up * 0.25f);
				}
				else // Bandit
				{
					warband.warbandName = "Bandits";
					warband.CreateWarband(GD.RandRange(2, 9), TroopType.Infantry, GD.RandRange(1, 2));
					warband.isHostileToPlayer = true;
					warband.civilizationAffiliation = CivilizationType.None;
					Vector3 playerPosition = GetNode<PlayerController>("/root/BaseNode/Player").Position;
					warband.Position = new Vector3(playerPosition.X + 5f, 0.25f, playerPosition.Z + 5f);
				}
			}
		}
	}
}
