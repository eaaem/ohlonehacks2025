using Godot;
using System;

public enum Race {
	Human,
	Dwarf,
	Elf
}

public enum StatType {
	Strength,
	Charisma,
	Intelligence
}

public class Skill {
	public string name;
	public StatType stat;
	public int level;
}

public partial class Player : Node
{

	public string name;
	public Race race;
	public int level;
	public int gold;
	public int strength;
	public int charisma;
	public int intelligence;
	public Troop[] troops;
	public Skill[] skills;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
