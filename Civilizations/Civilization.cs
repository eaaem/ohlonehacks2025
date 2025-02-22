using Godot;
using System;


public enum CharacterRace
{
	Human,
	Elf,
	Dwarf
}

[GlobalClass]
public partial class Civilization : Resource
{
	[Export]
	public string civilizationName;
	[Export]
	public string leaderName;
	[Export(PropertyHint.Enum)]
	public CharacterRace race;
	[Export]
	public Color color;
	[Export]
	public SettlementData[] settlements = new SettlementData[0];
}
