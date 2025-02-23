using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	// Input map actions need to be replaced
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export]
	private Node3D cameraTarget;
	[Export]
	private Player playerData;

	private Vector3 moveTarget;
	public bool IsMoving { get; set; }
	public bool IsMovementDisabled { get; set; }
	private bool isHoldingMiddleMouse = false;

	private Control tooltip = null;

	public override void _PhysicsProcess(double delta)
	{
		if (IsMovementDisabled)
		{
			return;
		}

		if (IsMoving)
		{
			Vector3 velocity = Velocity;

			Vector3 direction = GlobalPosition.DirectionTo(moveTarget);
			
			velocity.X = direction.X * Speed;
			velocity.Z = direction.Z * Speed;

			if (GlobalPosition.DistanceSquaredTo(moveTarget) < 0.01f)
			{
				IsMoving = false;
				return;
			}

			Velocity = velocity;
			MoveAndSlide();
		}
	}

	public override void _InputEvent(Camera3D camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				moveTarget = eventPosition;
				IsMoving = true;
			}
		}
	}

	public void OnMouseEntered()
	{
		// This is the exact same code as in the OverworldWarband tooltip generator and it's ugly
		CreateTooltip();
	}

	public void OnMouseExited()
	{
		Control tooltip = GetNode<Control>("/root/BaseNode/WarbandTooltip");
		GetNode<Node3D>("/root/BaseNode").RemoveChild(tooltip);
		tooltip.QueueFree();
	}

	public void CreateTooltip()
	{
		Control tooltip = GD.Load<PackedScene>("res://Combat/warband_tooltip.tscn").Instantiate<Control>();

		tooltip.GetNode<RichTextLabel>("Title").Text = "[center]" + playerData.name + "'s Party";

		PackedScene unitLabelScene = GD.Load<PackedScene>("res://Combat/unit_label.tscn");


		for (int i = 0; i < playerData.troops.Count; i++)
		{
			RichTextLabel label = unitLabelScene.Instantiate<RichTextLabel>();
			string unitType = "";

			switch (playerData.troops[i].troopType)
			{
				case TroopType.Infantry:
					unitType = ((InfantryTroopTier)playerData.troops[i].tier).ToString();
					break;
				case TroopType.Archer:
					unitType = ((ArcherTroopTier)playerData.troops[i].tier).ToString();
					break;
				case TroopType.Cavalry:
					unitType = ((CavalryTroopTier)playerData.troops[i].tier).ToString();
					break;
				case TroopType.Mage:
					unitType = ((MageTroopTier)playerData.troops[i].tier).ToString();
					break;
			}

			unitType = unitType.Replace("_", " ");

			label.Text = playerData.troops[i].quantity + " " + unitType + " (" + playerData.troops[i].troopType.ToString() + ")";
			tooltip.GetNode<VBoxContainer>("VBoxContainer").AddChild(label);
		}

		tooltip.GetNode<Panel>("Panel").Size = new Vector2(250, 25 + (tooltip.GetNode<VBoxContainer>("VBoxContainer").GetChildCount() * 20));

		tooltip.Name = "WarbandTooltip";
		tooltip.Position = GetViewport().GetMousePosition();
		GetNode<Node3D>("/root/BaseNode").AddChild(tooltip);
	}

	public override void _Input(InputEvent @event)
	{
		if (IsMovementDisabled)
		{
			return;
		}

		if (@event is InputEventMouseMotion mouseMotion && isHoldingMiddleMouse)
		{
			Vector3 rotation = cameraTarget.Rotation;

			rotation.X += mouseMotion.Relative.Y * 0.01f;
			rotation.Y += mouseMotion.Relative.X * 0.01f;

			rotation.X = Mathf.Clamp(rotation.X, Mathf.DegToRad(-90f), Mathf.DegToRad(15f));
			cameraTarget.Rotation = rotation;
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				if (cameraTarget.GetNode<Camera3D>("PlayerCamera").Position.Z > 3.5f)
				{
					cameraTarget.GetNode<Camera3D>("PlayerCamera").Position -= (Vector3.Back * 0.5f);
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				if (cameraTarget.GetNode<Camera3D>("PlayerCamera").Position.Z < 15f)
				{
					cameraTarget.GetNode<Camera3D>("PlayerCamera").Position += (Vector3.Back * 0.5f);
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.Middle)
			{
				if (mouseButton.IsReleased())
				{
					isHoldingMiddleMouse = false;
				}
				else
				{
					isHoldingMiddleMouse = true;
				}
			}
		}
	}
}
