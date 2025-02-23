using Godot;
using System;

public enum ItemType
{
	Warfare,
	Essential,
	Luxury
}

[GlobalClass]
public partial class Item : Resource
{
	[Export]
	public string itemName;
	[Export]
	public int price;
	[Export]
	public ItemType itemType;
	// high rarity value = more common
	[Export]
	public int rarity;
}
