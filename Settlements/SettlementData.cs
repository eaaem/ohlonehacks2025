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

public enum SettlementType
{
    Village,
    Town,
    City
}

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
    public CivilizationType civilizationType;  
    [Export]
    public SettlementType settlementType;
    [Export]
    public Troop[] troops; 
    [Export]
    public Building[] buildings;

    public override void _Ready()
    {
        InitializeAppearance();
    }

    void InitializeAppearance()
    {
        Label3D nameLabel = GetNode<Label3D>("Name");
        nameLabel.Text = settlementName;
        nameLabel.Modulate = CivilizationHolder.Instance.civilizations[(int)civilizationType].color;
    }
}
