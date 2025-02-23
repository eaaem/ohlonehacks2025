using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class TradeUI : Control
{
	private bool isBuying = true;

	public static TradeUI Instance { get; set; }

	private SettlementData selfSettlementData;
	[Export]
	private VBoxContainer container;

	private float determineProsperityModifier(Item item, bool playerBuying)
	{
		switch (selfSettlementData.prosperityScore)
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

	float determineWarfareModifier(Item item)
	{
		if (selfSettlementData.atWar && item.itemType == ItemType.Warfare)
		{
			return 2f;
		}
		return 1f;
	}

	float determineSizeModifier(Item item)
	{
		switch (selfSettlementData.settlementType)
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
				if (item.itemType == ItemType.Essential)
				{
					return 1.1f;
				}
				else if (item.itemType == ItemType.Warfare)
				{
					return 1.1f;
				}
				else if (item.itemType == ItemType.Luxury)
				{
					return 0.9f;
				}
				break;
		}
		return 1f;
	}

	public int DetemineBuyPrice(Item item)
	{
		float prosperityModifier = determineProsperityModifier(item, true);
		float warModifier = determineWarfareModifier(item);
		float sizeModifier = determineSizeModifier(item);

		return (int)Math.Floor(item.price * prosperityModifier * warModifier * sizeModifier * 1.1);
	}

	public int DetermineSellPrice(Item item)
	{
		float prosperityModifier = determineProsperityModifier(item, false);
		float warModifier = determineWarfareModifier(item);
		float sizeModifier = determineSizeModifier(item);
		return Math.Min((int)Math.Floor(item.price * prosperityModifier * warModifier * sizeModifier * 0.9), (int) DetemineBuyPrice(item));
	}

	public int DetermineItemQuantity(Item item)
	{
		float prosperityModifier = determineProsperityModifier(item, false);
		float warModifier = determineWarfareModifier(item);
		float sizeModifier = determineSizeModifier(item);
		return (int)Math.Floor(item.rarity * prosperityModifier * warModifier * sizeModifier);
	}

	public List<ItemListing> GetItemListings()
	{
		List<ItemListing> itemListings = new();
		Item[] allItems = GetNode<ItemList>("/root/BaseNode/ItemReference").items;
		foreach (Item item in allItems)
		{
			int boughtQuantity = 0;

			if (selfSettlementData.boughtItems != null)
			{
				//boughtQuantity = selfSettlementData.boughtItems.ToList().Find(settlementItem => settlementItem.item.itemName == item.itemName).quantity;
				foreach (InventoryItem inventoryItem in selfSettlementData.boughtItems)
				{
					if (inventoryItem.item != null && inventoryItem.item.itemName == item.itemName)
					{
						boughtQuantity = inventoryItem.quantity;
					}
				}
			}
			int quantity = DetermineItemQuantity(item);
			if (quantity > 0)
			{
				itemListings.Add(new ItemListing(item, quantity - boughtQuantity, DetemineBuyPrice(item), DetermineSellPrice(item)));
			}
		}
		return itemListings;
	}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Instance = this;
	}

	public void BuyItem(ItemListing item)
	{
		if (!isBuying)
		{
			return;
		}

		if (item.buyPrice > Player.Instance.gold)
		{
			return;
		}

		GD.Print("Buying");

		List<InventoryItem> settlementItems = new();
		
		foreach (InventoryItem invItem in selfSettlementData.boughtItems)
		{
			if (invItem.item.itemName == item.item.itemName)
			{
				if (item.quantity <= invItem.quantity)
				{
					//return;
				}
				settlementItems.Add(new InventoryItem(invItem.item, invItem.quantity + 1));
			}
			else
			{
				settlementItems.Add(invItem);
			}
		}
		if (!settlementItems.Any(settlementItem => settlementItem.item.itemName == item.item.itemName)) {
			settlementItems.Add(new InventoryItem(item.item, 1));
		}
		selfSettlementData.boughtItems = settlementItems.ToArray();
		Player.Instance.gold -= item.buyPrice;

		foreach (Control control in container.GetChildren())
		{
			ShopButton shopButton = control.GetNode<ShopButton>("Name");

			if (shopButton.itemListing == item)
			{
				shopButton.itemListing.quantity--;

				if (shopButton.itemListing.quantity <= 0)
				{
					container.RemoveChild(control);
					control.QueueFree();
				}
				else
				{
					shopButton.Text = item.item.itemName + " (" + shopButton.itemListing.quantity + "; cost for 1: " + item.sellPrice + ")";
				}

				break;
			}
		}

		GetNode<RichTextLabel>("Background/Labels/Gold").Text = "Gold: " + Player.Instance.gold;

		for (int i = 0; i < Player.Instance.inventory.Count; i++)
		{
			if (Player.Instance.inventory[i].item.itemName == item.item.itemName)
			{
				Player.Instance.inventory[i].quantity++;
				return;
			}
		}

		Player.Instance.inventory.Add(new InventoryItem(item.item, 1));
	}

	public void SellItem(ItemListing item)
	{
		if (isBuying)
		{
			return;
		}

		Player.Instance.gold += item.sellPrice;
		List<InventoryItem> settlementItems = new();
		foreach (InventoryItem invItem in selfSettlementData.boughtItems)
		{
			if (invItem.Name == item.item.itemName)
			{
				settlementItems.Add(new InventoryItem(invItem.item, invItem.quantity - 1));
			}
		}
		selfSettlementData.boughtItems = settlementItems.ToArray();

		GetNode<RichTextLabel>("Background/Labels/Gold").Text = "Gold: " + Player.Instance.gold;

		foreach (Control control in container.GetChildren())
		{
			ShopButton shopButton = control.GetNode<ShopButton>("Name");

			if (shopButton.itemListing == item)
			{
				shopButton.itemListing.quantity--;

				if (shopButton.itemListing.quantity <= 0)
				{
					container.RemoveChild(control);
					control.QueueFree();
				}
				else
				{
					shopButton.Text = item.item.itemName + " (" + shopButton.itemListing.quantity + "; sell price for 1: " + item.sellPrice + ")";
				}

				break;
			}
		}

		for (int i = 0; i < Player.Instance.inventory.Count; i++)
		{
			if (Player.Instance.inventory[i].item.itemName == item.item.itemName)
			{
				Player.Instance.inventory[i].quantity--;

				if (Player.Instance.inventory[i].quantity <= 0)
				{
					Player.Instance.inventory.Remove(Player.Instance.inventory[i]);
				}

				return;
			}
		}
	}

	public void OpenUI(SettlementData settlementData)
	{
		selfSettlementData = settlementData;

		GetNode<RichTextLabel>("Background/Labels/Title").Text = "[b]Trading with " + selfSettlementData.settlementName + "[/b]";

		GetNode<RichTextLabel>("Background/Labels/Gold").Text = "Gold: " + Player.Instance.gold;

		OnBuyButtonDown();

		Visible = true;
	}

	public void OnBuyButtonDown()
	{
		isBuying = true;

		PackedScene shopItemsScene = GD.Load<PackedScene>("res://Settlements/trade.tscn");

		ClearContainer();

		foreach (ItemListing item in GetItemListings())
		{
			Control control = shopItemsScene.Instantiate<Control>();
			control.GetNode<ShopButton>("Name").Text = item.item.itemName + " (" + item.quantity + "; cost for 1: " + item.buyPrice + ")";
			control.GetNode<ShopButton>("Name").itemListing = item;
			control.GetNode<ShopButton>("Name").Initialize();

			container.AddChild(control);
		}
	}

	public void OnSellButtonDown()
	{
		isBuying = false;

		PackedScene shopItemsScene = GD.Load<PackedScene>("res://Settlements/trade.tscn");

		ClearContainer();

		foreach (InventoryItem item in Player.Instance.inventory)
		{
			ItemListing itemListing = new ItemListing(item.item, item.quantity, 0, DetermineSellPrice(item.item));

			Control control = shopItemsScene.Instantiate<Control>();
			control.GetNode<ShopButton>("Name").Text = item.item.itemName + " (" + item.quantity + "; sell price for 1: " + itemListing.sellPrice + ")";
			control.GetNode<ShopButton>("Name").itemListing = itemListing;
			control.GetNode<ShopButton>("Name").Initialize();

			container.AddChild(control);
		}
	}

	void ClearContainer()
	{
		foreach (Control child in container.GetChildren())
		{
			container.RemoveChild(child);
			child.QueueFree();
		}
	}

	public void OnBackDown()
	{
		Visible = false;
		GetNode<SettlementUI>("/root/BaseNode/UI/SettlementScreen").OpenUI(selfSettlementData);
	}
}


public partial class ItemListing
{

	public ItemListing(Item _item, int _quantity, int _buyPrice, int _sellPrice)
	{
		item = _item;
		quantity = _quantity;
		buyPrice = _buyPrice;
		sellPrice = _sellPrice;
	}

	public Item item;
	public int quantity;
	public int buyPrice;
	public int sellPrice;
}