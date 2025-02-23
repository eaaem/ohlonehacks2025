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

public partial class SettlementData : Node3D
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
    [Export]
    public bool atWar;
    public InventoryItem[] boughtItems = new InventoryItem[0];
    public Troop[] recruitedTroops = new Troop[0];

    public override void _Ready()
    {
        Initialize();
    }

    void Initialize()
    {
        Label3D nameLabel = GetNode<Label3D>("Name");
        nameLabel.Text = settlementName;
        nameLabel.Modulate = CivilizationHolder.Instance.civilizations[(int)civilizationType].color;
        GetNode<Node3D>(settlementType.ToString() + "Appearance").Visible = true;
    }

    public void OnPlayerEntered(Node3D body)
    {
        GetNode<SettlementUI>("/root/BaseNode/UI/SettlementScreen").OpenUI(this);
    }
}
