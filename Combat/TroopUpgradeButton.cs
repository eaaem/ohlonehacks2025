using Godot;
using System;

public partial class TroopUpgradeButton : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ButtonDown += () => GetNode<PlayerController>("/root/BaseNode/Player").OnUpgradeButtonDown(GetParent().GetChild<TroopInfoHolder>(2).troopType,
																GetParent().GetChild<TroopInfoHolder>(2).tier);
	}
}
