using Godot;
using System;

[GlobalClass]
public partial class Civilization : Node
{
	[Export]
	public string civilizationName;
	[Export]
	public string leaderName;
	[Export]
	public string race;
	[Export]
	public SettlementData[] settlements = new SettlementData[0];
}
