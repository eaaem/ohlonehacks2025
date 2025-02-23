using Godot;
using System;

public partial class InventoryItem : Node
{
	public InventoryItem(Item _item, int _quantity) {
		item = _item;
		quantity = _quantity;
	}
	public Item item;
	public int quantity;
}
