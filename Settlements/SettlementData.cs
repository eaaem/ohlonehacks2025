using Godot;
using System;

public enum Prosperity {
    Poor,
    Struggling,
    Average,
    Good,
    Prospering
}

public enum MilitaryStrength {
    Miserable,
    Bad,
    Average,
    Good,
    Extraordinary
}

[GlobalClass]
public partial class SettlementData : Node
{
    [Export]
    public string settlementName;
    [Export]
    public Prosperity prosperityScore;
    [Export]
    public int population;
    [Export]
    public MilitaryStrength militaryStrength;
    [Export]
    public Civilization civilization;   
}
