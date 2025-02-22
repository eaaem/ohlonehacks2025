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
    }
}
