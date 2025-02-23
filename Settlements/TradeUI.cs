using Godot;
using System;
using System.Collections.Generic;

public partial class TradeUI : Control
{

    public class ItemListing {
        public Item item;
        public int quantity;
        public int buyPrice;
        public int sellPrice;
    }

    public static TradeUI Instance { get; set; }

    private SettlementData selfSettlementData;

    private float determineProsperityModifier(SettlementData settlementData, Item item, bool playerBuying)
	{
		switch (settlementData.prosperityScore)
		{
			case Prosperity.Poor:
				if (item.itemType == ItemType.Essential)
				{
					if (playerBuying)
					{
						return 1.5f;
					}
					else
					{
						return 1.3f;
					}
				}
				else if (item.itemType == ItemType.Warfare)
				{
					if (playerBuying)
					{
						return 0.9f;
					}
					else
					{
						return 0.9f;
					}
				}
				else if (item.itemType == ItemType.Luxury)
				{
					if (playerBuying)
					{
						return 0.7f;
					}
					else
					{
						return 0.3f;
					}
				}
				break;
			case Prosperity.Struggling:
				if (item.itemType == ItemType.Essential)
				{
					if (playerBuying)
					{
						return 1.25f;
					}
					else
					{
						return 1.25f;
					}
				}
				else if (item.itemType == ItemType.Warfare)
				{
					if (playerBuying)
					{
						return 1f;
					}
					else
					{
						return 1f;
					}
				}
				else if (item.itemType == ItemType.Luxury)
				{
					if (playerBuying)
					{
						return 0.8f;
					}
					else
					{
						return 0.5f;
					}
				}
				break;
			case Prosperity.Average:
				return 1f;
			case Prosperity.Good:
				if (item.itemType == ItemType.Essential)
				{
					if (playerBuying)
					{
						return 0.9f;
					}
					else
					{
						return 0.8f;
					}
				}
				else if (item.itemType == ItemType.Warfare)
				{
					if (playerBuying)
					{
						return 1.1f;
					}
					else
					{
						return 1.1f;
					}
				}
				else if (item.itemType == ItemType.Luxury)
				{
					if (playerBuying)
					{
						return 1.1f;
					}
					else
					{
						return 1.1f;
					}
				}
				break;
			case Prosperity.Prospering:
				if (item.itemType == ItemType.Essential)
				{
					if (playerBuying)
					{
						return 0.7f;
					}
					else
					{
						return 0.7f;
					}
				}
				else if (item.itemType == ItemType.Warfare)
				{
					if (playerBuying)
					{
						return 1.1f;
					}
					else
					{
						return 1.1f;
					}
				}
				else if (item.itemType == ItemType.Luxury)
				{
					if (playerBuying)
					{
						return 1f;
					}
					else
					{
						return 1.3f;
					}
				}
				break;
		}
		return 1f;
	}

	float determineWarfareModifier(SettlementData settlementData, Item item) {
		if (settlementData.atWar && item.itemType == ItemType.Warfare) {
			return 2f;
		}
		return 1f;
	}

	float determineSizeModifier(SettlementData settlementData, Item item)
	{
		switch (settlementData.settlementType)
		{
			case SettlementType.Village:
				if (item.itemType == ItemType.Essential)
				{
					return 0.8f;
				}
				else if (item.itemType == ItemType.Warfare)
				{
					return 0.9f;
				}
				else if (item.itemType == ItemType.Luxury)
				{
					return 1.1f;
				}
				break;
			case SettlementType.Town:
				return 1f;
			case SettlementType.City:
			if (item.itemType == ItemType.Essential) {
					return 1.1f;
				} else if (item.itemType == ItemType.Warfare) {
					return 1.1f;
				} else if (item.itemType == ItemType.Luxury) {
					return 0.9f;
				}
				break;
		}
		return 1f;
	}

	public int DetemineBuyPrice(SettlementData settlementData, Item item)
	{
		float prosperityModifier = determineProsperityModifier(settlementData, item, true);
		float warModifier = determineWarfareModifier(settlementData, item);
		float sizeModifier = determineSizeModifier(settlementData, item);

		return (int) Math.Floor(item.price * prosperityModifier * warModifier * sizeModifier * 1.1);
	}

	public int DetermineSellPrice(SettlementData settlementData, Item item)
	{
		float prosperityModifier = determineProsperityModifier(settlementData, item, false);
		float warModifier = determineWarfareModifier(settlementData, item);
		float sizeModifier = determineSizeModifier(settlementData, item);
		return (int) Math.Floor(item.price * prosperityModifier * warModifier * sizeModifier * 0.9);
	}

    public int DetermineItemQuantity(SettlementData settlementData, Item item) {
        float prosperityModifier = determineProsperityModifier(settlementData, item, false);
		float warModifier = determineWarfareModifier(settlementData, item);
		float sizeModifier = determineSizeModifier(settlementData, item);
		return (int) Math.Floor(item.rarity * prosperityModifier * warModifier * sizeModifier);
    }

    public List<ItemListing> GetItemListings() {
        List<ItemListing> itemListings = new();

        return itemListings;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        Instance = this;
    }

    public void OpenUI(SettlementData settlementData)
    {
        GetNode<RichTextLabel>("Background/Labels/SettlementName").Text = "[b]" + settlementData.settlementName + "[/b]";

        PackedScene shopItemsScene = GD.Load<PackedScene>("res://Settlements/trade.tscn");
        foreach (ItemListing item in GetItemListings()) {
            RichTextLabel itemName = shopItemsScene.Instantiate<RichTextLabel>();
            RichTextLabel itemQuantity = shopItemsScene.Instantiate<RichTextLabel>();
            Button sellItem = shopItemsScene.Instantiate<Button>();
            Button buyItem = shopItemsScene.Instantiate<Button>();

            itemName.Text = item.item.itemName;
            itemQuantity.Text = item.quantity.ToString();
            sellItem.Text = item.sellPrice.ToString();
            buyItem.Text = item.buyPrice.ToString();
        }
        GetNode<VBoxContainer>("VBoxContainer");

        Visible = true;
    }

    public void OnBackDown()
	{
		Visible = false;
		GetNode<SettlementUI>("/root/BaseNode/UI/SettlementScreen").OpenUI(selfSettlementData);
	}
}
