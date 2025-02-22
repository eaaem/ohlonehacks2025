using Godot;

public enum BuildingType {
    Tax_Building,
    Barracks,
    Archery_Range,
    Stable,
    Wizards_Tower
}

public partial class Building: Node {
    public BuildingType type;
    public int level;
}