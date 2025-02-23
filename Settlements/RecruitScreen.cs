using Godot;
using System;
using System.Collections.Generic;

public partial class RecruitScreen : Node
{

	private int determineProsperityModifier(TroopType troopType, SettlementData settlementData)
	{
		switch (settlementData.prosperityScore)
		{
			case Prosperity.Poor:
				if (troopType == TroopType.Mage) {
					return 0;
				}
				return 3;
			case Prosperity.Struggling:
				if (troopType == TroopType.Mage) {
					return 0;
				}
				return 3;
			case Prosperity.Average:
				if (troopType == TroopType.Mage) {
					return 1;
				}
				return 2;
			case Prosperity.Good:
				if (troopType == TroopType.Mage) {
					return 1;
				}
				return 2;
			case Prosperity.Prospering:
				if (troopType == TroopType.Mage) {
					return 2;
				}
				return 1;
		}
		return 0;
	}

	private int determineWarfareModifier(TroopType troopType, SettlementData settlementData)
	{
		if (settlementData.atWar)
		{
			return 0;
		}
		if (troopType == TroopType.Mage && settlementData.prosperityScore != Prosperity.Poor && settlementData.prosperityScore != Prosperity.Struggling) {
			return 1;
		}
		return 2;
	}

	private int determineSizeModifier(TroopType troopType, SettlementData settlementData)
	{
		switch (settlementData.settlementType)
		{
			case SettlementType.Village:
				if (troopType == TroopType.Mage) {
					return 0;
				}
				return 3;
			case SettlementType.Town:
				if (troopType == TroopType.Mage) {
					return 1;
				}
				return 2;
			case SettlementType.City:
				if (troopType == TroopType.Mage) {
					return 2;
				}
				return 2;
		}
		return 0;
	}

	private int getTroopAmount(TroopType troopType, SettlementData settlement)
	{
		int prosperityModifier = determineProsperityModifier(troopType, settlement);
		int warModifier = determineWarfareModifier(troopType, settlement);
		int sizeModifier = determineSizeModifier(troopType, settlement);

		return prosperityModifier + warModifier + sizeModifier;

	}

	private Troop[] getRecruitableTroops(SettlementData settlement)
	{
		// You can only recruit tier 1 troops
		int infantry = getTroopAmount(TroopType.Infantry, settlement);
		int archers = getTroopAmount(TroopType.Archer, settlement);
		int cavalry = getTroopAmount(TroopType.Cavalry, settlement);
		int mage = getTroopAmount(TroopType.Mage, settlement);

		List<Troop> troops = new()
        {
            new Troop(infantry, TroopType.Infantry, 0),
			new Troop(archers, TroopType.Archer, 0),
			new Troop(cavalry, TroopType.Cavalry, 0),
			new Troop(mage, TroopType.Mage, 0)
        };
		
		return troops.ToArray();
	}

	private void RecruitTroop(Troop troop)
	{
		int troopPrice = troop.troopType == TroopType.Mage ? 20 : 10;
		List<Troop> newTroops = new();
		foreach (Troop playerTroop in Player.Instance.troops) {
			if (playerTroop.troopType == troop.troopType && playerTroop.tier == troop.tier) {
				newTroops.Add(new Troop(playerTroop.quantity + 1, playerTroop.troopType, playerTroop.tier));
			} else {
				newTroops.Add(playerTroop);
			}
		}
		Player.Instance.troops = newTroops;
	}

	public void OpenRecruitScreen(SettlementData settlement)
	{

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
