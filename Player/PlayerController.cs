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
	[Export]
	private Node3D detachedCameraTarget;
	[Export]
	private Camera3D camera;
	private Vector3 moveTarget;
	public bool IsMoving { get; set; }
	public bool IsMovementDisabled { get; set; }
	private bool isHoldingMiddleMouse = false;

	private bool cameraIsDetached = false;

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

			if (GlobalPosition.DistanceSquaredTo(moveTarget) < 0.1f)
			{
				GlobalPauseState.Instance.IsPaused = true;
				IsMoving = false;
				return;
			}

			Velocity = velocity;
			MoveAndSlide();
		}

		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_backward");

		if (inputDir != Vector2.Zero)
		{
			if (!cameraIsDetached)
			{
				cameraIsDetached = true;
				cameraTarget.RemoveChild(camera);
				detachedCameraTarget.AddChild(camera);
				detachedCameraTarget.Rotation = cameraTarget.Rotation;
			}

			Vector3 position = detachedCameraTarget.Position;

			//Vector3 relative = detachedCameraTarget.Transform.Basis * new Vector3(inputDir.X + po, 0, inputDir.Y);
			Vector3 cameraRightDir = camera.GlobalTransform.Basis.X;
			Vector3 cameraFrontDir = camera.GlobalTransform.Basis.Z;
			position += inputDir.X * cameraRightDir;
			position += inputDir.Y * cameraFrontDir;
			position.Y = detachedCameraTarget.Position.Y;

			detachedCameraTarget.Position = position;
		}
	}

	public override void _InputEvent(Camera3D camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left)
			{
				GlobalPauseState.Instance.IsPaused = false;
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

		if (@event is InputEventKey key && key.Keycode == Key.Escape)
		{
			if (cameraIsDetached)
			{
				cameraIsDetached = false;
				detachedCameraTarget.RemoveChild(camera);
				cameraTarget.AddChild(camera);
				cameraTarget.Rotation = detachedCameraTarget.Rotation;
			}
		}

		if (@event is InputEventMouseMotion mouseMotion && isHoldingMiddleMouse)
		{
			Vector3 rotation;
			if (!cameraIsDetached)
			{
				rotation = cameraTarget.Rotation;
			}
			else
			{
				rotation = detachedCameraTarget.Rotation;
			}

			rotation.X += mouseMotion.Relative.Y * 0.01f;
			rotation.Y += mouseMotion.Relative.X * 0.01f;

			rotation.X = Mathf.Clamp(rotation.X, Mathf.DegToRad(-90f), Mathf.DegToRad(15f));

			if (!cameraIsDetached)
			{
				cameraTarget.Rotation = rotation;
			}
			else
			{
				detachedCameraTarget.Rotation = rotation;
			}
		}

		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.WheelUp)
			{
				if (camera.Position.Z > 3.5f)
				{
					camera.Position -= (Vector3.Back * 0.5f);
				}
			}
			else if (mouseButton.ButtonIndex == MouseButton.WheelDown)
			{
				if (camera.Position.Z < 15f)
				{
					camera.Position += (Vector3.Back * 0.5f);
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
