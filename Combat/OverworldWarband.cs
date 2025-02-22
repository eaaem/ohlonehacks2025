using Godot;
using System;

public partial class OverworldWarband : Node
{
	public string warbandName;
	public CivilizationType civilizationAffiliation;
	public Troop[] troops;

	public void GenerateWarband(int numberOfTroops, TroopType favoredTroopType, int averageStrength)
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

			//troops[troops.Length] = new Troop()
		}
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
