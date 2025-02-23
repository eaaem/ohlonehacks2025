using Godot;
using System;

public partial class ShopButton : Button
{
	public ItemListing itemListing;

	// Called when the node enters the scene tree for the first time.
	public void Initialize()
	{
		ItemListing listing = itemListing;
		ButtonDown += () => GetNode<TradeUI>("/root/BaseNode/UI/TradeScreen").BuyItem(listing);
		ButtonDown += () => GetNode<TradeUI>("/root/BaseNode/UI/TradeScreen").SellItem(listing);
	}
}
