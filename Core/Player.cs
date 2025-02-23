using Godot;
using System;
using System.Collections.Generic;

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
	public List<Troop> troops = new List<Troop>();
	public Skill[] skills;

    public override void _Ready()
    {
        name = "Test";
		gold = 50;
		troops.Add(new Troop(5, TroopType.Infantry, 1));
    }
}
