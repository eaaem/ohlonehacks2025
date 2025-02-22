using Godot;
using System;

public enum TroopType {
	Infantry,
	Archer,
	Cavalry,
	Mage
}

public enum InfantryTroopTier {
	Peasant,
	Foot_Soldier,
	Man_At_Arms,
	Knight,
	Magic_Knight
}

public enum ArcherTroopTier {
	Slingshot,
	Short_Bow,
	Long_Bow,
	Crossbow,
	Sharp_Shooter
}

public enum CavalryTroopTier {
	Light_Cavalry,
	Medium_Cavalry,
	Heavy_Cavalry,
	Light_Unicorn,
	Heavy_Unicorn
}

public enum MageTroopTier {
	Apprentice,
	Journeyman,
	Magician,
	Wizard,
	Sorcerer
}

public partial class Troop : Node
{
	public int quantity;
	public TroopType troopType;
	public int tier;
}
