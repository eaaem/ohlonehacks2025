using Godot;
using System;

public enum CivilizationType
{
	GreatExpanse,
	Foresthome,
	Athelia,
	Thaelanar,
	Korokdaron,
	Grokaria,
	Player,
	None
}

public partial class CivilizationHolder : Node
{
	public static CivilizationHolder Instance { get; set; }

	[Export]
	public Civilization[] civilizations = new Civilization[0];

    public override void _Ready()
    {
        Instance = this;

		for (int i = 0; i < civilizations.Length; i++)
		{
			Node3D settlementParent = GetNode<Node3D>("/root/BaseNode/" + (CivilizationType)i);
			civilizations[i].settlements = new SettlementData[settlementParent.GetChildCount()];

			for (int j = 0; j < civilizations[i].settlements.Length; j++)
			{
				civilizations[i].settlements[j] = settlementParent.GetChild<SettlementData>(j);
			}
		}
    }
}
