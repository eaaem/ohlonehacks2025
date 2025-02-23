using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class RecruitScreen : Control
{
	private Troop[] recruitableTroops = new Troop[0];
	private SettlementData settlementData;

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

		foreach (Troop recruitedTroop in settlement.recruitedTroops)
		{
			if (recruitedTroop.troopType == TroopType.Infantry) {
				infantry -= recruitedTroop.quantity;
			} else if (recruitedTroop.troopType == TroopType.Archer) {
				archers -= recruitedTroop.quantity;
			} else if (recruitedTroop.troopType == TroopType.Cavalry) {
				cavalry -= recruitedTroop.quantity;
			} else {
				mage -= recruitedTroop.quantity;
			}
		}

		List<Troop> troops = new();

		if (infantry > 0) {
			troops.Add(new Troop(infantry, TroopType.Infantry, 0));
		}

		if (archers > 0) {
			troops.Add(new Troop(archers, TroopType.Archer, 0));
		}

		if (cavalry > 0) {
			troops.Add(new Troop(cavalry, TroopType.Cavalry, 0));
		}

		if (mage > 0) {
			troops.Add(new Troop(mage, TroopType.Mage, 0));
		}
		
		return troops.ToArray();
	}

	private bool RecruitTroop(Troop troop, SettlementData settlement)
	{
		int troopPrice = troop.troopType == TroopType.Mage ? 20 : 10;
		if (Player.Instance.gold < troopPrice) {
			return false;
		}

		Player.Instance.gold -= troopPrice;

		for (int i = 0; i < Player.Instance.troops.Count; i++)
		{
			if (Player.Instance.troops[i].troopType == troop.troopType && Player.Instance.troops[i].tier == troop.tier)
			{
				GD.Print(Player.Instance.troops[i].quantity);
				Player.Instance.troops[i].quantity++;
				return true;
			}
		}

		Player.Instance.troops.Add(new Troop(1, troop.troopType, troop.tier));
		return true;
	}

    public override void _Ready()
    {
        GetNode<Button>("Labels/VBoxContainer/Infantry/Button").ButtonDown += () => OnRecruitButtonDown(0);
        GetNode<Button>("Labels/VBoxContainer/Archer/Button").ButtonDown += () => OnRecruitButtonDown(1);
        GetNode<Button>("Labels/VBoxContainer/Cavalry/Button").ButtonDown += () => OnRecruitButtonDown(2);
        GetNode<Button>("Labels/VBoxContainer/Mage/Button").ButtonDown += () => OnRecruitButtonDown(3);
    }

    public void OpenRecruitScreen(SettlementData settlement)
	{
		recruitableTroops = getRecruitableTroops(settlement);

		settlementData = settlement;

		HideAll();

		GetNode<Button>("Labels/VBoxContainer/Infantry/Button").Disabled = false;
        GetNode<Button>("Labels/VBoxContainer/Archer/Button").Disabled = false;
        GetNode<Button>("Labels/VBoxContainer/Cavalry/Button").Disabled = false;
        GetNode<Button>("Labels/VBoxContainer/Mage/Button").Disabled = false;

        GetNode<RichTextLabel>("Labels/Gold").Text = "[center]Gold: " + Player.Instance.gold;

		for (int i = 0; i < recruitableTroops.Length; i++)
		{
			if (recruitableTroops[i].quantity > 0)
			{
				Control label = GetNode<Control>("Labels/VBoxContainer/" + recruitableTroops[i].troopType.ToString());
				label.GetNode<RichTextLabel>("RichTextLabel").Text = recruitableTroops[i].quantity + " " + recruitableTroops[i].troopType;
				label.Visible = true;
			}
		}

		Visible = true;
	}

	void OnRecruitButtonDown(int id)
	{
		for (int i = 0; i < recruitableTroops.Length; i++)
		{
			if (recruitableTroops[i].troopType == (TroopType)id)
			{
				if (RecruitTroop(recruitableTroops[i], settlementData))
				{
					recruitableTroops[i].quantity--;

					GetNode<RichTextLabel>("Labels/VBoxContainer/" + ((TroopType)(id)).ToString() + "/RichTextLabel").Text = recruitableTroops[i].quantity + " " + recruitableTroops[i].troopType;

					if (recruitableTroops[i].quantity <= 0)
					{
						GetNode<Button>("Labels/VBoxContainer/" + ((TroopType)(id)).ToString() + "/Button").Disabled = true;
					}

        			GetNode<RichTextLabel>("Labels/Gold").Text = "[center]Gold: " + Player.Instance.gold;
				}	
			
				return;
			}
		}
	}

	void HideAll()
	{
		foreach (Control control in GetNode<VBoxContainer>("Labels/VBoxContainer").GetChildren())
		{
			control.Visible = false;
		}
	}

	void OnBackButtonDown()
	{
		Visible = false;
		GetNode<SettlementUI>("/root/BaseNode/UI/SettlementScreen").OpenUI(settlementData);
	}
}
