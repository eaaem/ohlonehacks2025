using Godot;
using System;

public partial class SettlementUI : Control
{
	public static SettlementUI Instance { get; set; }

	public SettlementData selfSettlementData;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	public void OpenUI(SettlementData settlementData)
	{
		selfSettlementData = settlementData;
		GetNode<PlayerController>("/root/BaseNode/Player").IsMovementDisabled = true;
		GetNode<PlayerController>("/root/BaseNode/Player").IsMoving = false;

		Civilization civilization = CivilizationHolder.Instance.civilizations[(int)settlementData.civilizationType];

		if (settlementData.civilizationType == CivilizationType.GreatExpanse)
		{
			GetNode<RichTextLabel>("Background/Labels/BasicInfo").Text = 
			"This [b]" + settlementData.settlementType.ToString() + "[/b] belongs to [b]" + civilization.civilizationName + "[/b]"
			+ " civilization, and is therefore owned by [b]" + civilization.leaderName + "[/b].";
		}
		else
		{
			GetNode<RichTextLabel>("Background/Labels/BasicInfo").Text = 
			"This [b]" + settlementData.settlementType.ToString() + "[/b] belongs to the [b]" + civilization.civilizationName + "[/b]"
			+ " civilization, and is therefore owned by [b]" + civilization.leaderName + "[/b].";
		}
		GetNode<RichTextLabel>("Background/Labels/SettlementName").Text = "[b]" + settlementData.settlementName + "[/b]";
		
		GetNode<RichTextLabel>("Background/Labels/Prosper").Text = "Its prosperity is [b]" + settlementData.prosperityScore.ToString() + "[/b].";
		GetNode<RichTextLabel>("Background/Labels/Military").Text = "Its military strength is [b]" 
																	+ settlementData.militaryStrength.ToString() + "[/b].";
		GetNode<RichTextLabel>("Background/Labels/Population").Text = "Its population is [b]" + settlementData.population + "[/b].";

		Visible = true;
	}

	public void OnTradeDown()
	{
		Visible = false;
		GetNode<TradeUI>("/root/BaseNode/UI/TradeScreen").OpenUI(selfSettlementData);
	}

	public void OnLeaveDown()
	{
		Visible = false;
		GetNode<PlayerController>("/root/BaseNode/Player").IsMovementDisabled = false;
	}
}
