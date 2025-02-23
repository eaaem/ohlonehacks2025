using Godot;
using System;

public partial class ShopButton : Button
{
	public ItemListing itemListing;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		ButtonDown += () => GetNode<TradeUI>("/root/BaseNode/UI/TradeScreen").BuyItem(itemListing);
		ButtonDown += () => GetNode<TradeUI>("/root/BaseNode/UI/TradeScreen").SellItem(itemListing);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
