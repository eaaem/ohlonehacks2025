using Godot;
using System;
using System.Runtime.CompilerServices;

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

	[Signal]
	public delegate void OpenedUIEventHandler();
	[Signal]
	public delegate void ClosedUIEventHandler();

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

		if (IsMoving)
		{
			return;
		}

		if (Input.IsActionJustPressed("pause"))
		{
			GlobalPauseState.Instance.IsPaused = false;
		}

		if (Input.IsActionJustReleased("pause"))
		{
			GlobalPauseState.Instance.IsPaused = true;
		}
	}

	public override void _InputEvent(Camera3D camera, InputEvent @event, Vector3 eventPosition, Vector3 normal, int shapeIdx)
	{
		if (@event is InputEventMouseButton mouseButton)
		{
			if (mouseButton.ButtonIndex == MouseButton.Left && mouseButton.Pressed)
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

		if (@event is InputEventKey key)
		{
			if (key.Keycode == Key.Escape && cameraIsDetached)
			{
				cameraIsDetached = false;
				detachedCameraTarget.RemoveChild(camera);
				cameraTarget.AddChild(camera);
				cameraTarget.Rotation = detachedCameraTarget.Rotation;
			}
			else if (key.Keycode == Key.E)
			{
				if (!GetNode<Control>("/root/BaseNode/UI/PlayerScreen").Visible)
				{
					OpenPlayerScreen();
				}
			}
			else if (key.Keycode == Key.T)
			{
				if (!GetNode<Control>("/root/BaseNode/UI/TrainerScreen").Visible)
				{
					OpenTrainingScreen();
				}
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

	void OpenPlayerScreen()
	{
		GlobalPauseState.Instance.IsPaused = true;

		Panel playerScreen = GetNode<Panel>("/root/BaseNode/UI/PlayerScreen/Background");

		playerScreen.GetNode<RichTextLabel>("Labels/Name").Text = "[b]" + playerData.name;
		playerScreen.GetNode<RichTextLabel>("Labels/Race").Text = "Race: " + playerData.race.ToString();
		playerScreen.GetNode<RichTextLabel>("Labels/Gold").Text = "Gold: " + playerData.gold;
		playerScreen.GetNode<RichTextLabel>("Labels/Level").Text = "Level: " + playerData.level;
		playerScreen.GetNode<RichTextLabel>("Labels/Strength").Text = "Strength: " + playerData.strength;
		playerScreen.GetNode<RichTextLabel>("Labels/Intelligence").Text = "Intelligence: " + playerData.intelligence;
		playerScreen.GetNode<RichTextLabel>("Labels/Charisma").Text = "Charisma: " + playerData.charisma;

		playerScreen.GetParent<Control>().Visible = true;

		IsMovementDisabled = true;
	}

	void ClosePlayerScreen()
	{
		GlobalPauseState.Instance.IsPaused = false;
		GetNode<Control>("/root/BaseNode/UI/PlayerScreen").Visible = false;
		IsMovementDisabled = false;
	}

	void OpenTrainingScreen()
	{
		PackedScene troopPanel = GD.Load<PackedScene>("res://Combat/troop_panel.tscn");

		VBoxContainer troopContainer = GetNode<VBoxContainer>("/root/BaseNode/UI/TrainerScreen/Background/Labels/VBoxContainer");

		foreach (Control child in troopContainer.GetChildren())
		{
			troopContainer.RemoveChild(child);
			child.QueueFree();
		}

		foreach (Troop troop in playerData.troops)
		{
			troopContainer.AddChild(CreateTroopPanel(troop, troopPanel));
		}

		GetNode<RichTextLabel>("/root/BaseNode/UI/TrainerScreen/Background/Labels/Gold").Text = "Gold: " + playerData.gold;

		GetNode<Control>("/root/BaseNode/UI/TrainerScreen").Visible = true;

		IsMovementDisabled = true;
	}

	Control CreateTroopPanel(Troop troop, PackedScene troopPanel)
	{
		Control panel = troopPanel.Instantiate<Control>();

		UpdateTroopText(panel.GetNode<RichTextLabel>("Title"), troop);

		if (troop.tier == 4)
		{
			Button button = panel.GetNode<Button>("Button");
			panel.RemoveChild(button);
			button.QueueFree();
		}
		else
		{	
			panel.GetNode<Button>("Button").Text = "Upgrade 1 for " + GetCostForUpgrade(troop) + " gold";
		}

		panel.GetNode<TroopInfoHolder>("TroopInfo").troopType = troop.troopType;
		panel.GetNode<TroopInfoHolder>("TroopInfo").tier = troop.tier;

		return panel;
	}

	void UpdateTroopText(RichTextLabel label, Troop troop)
	{
		string unitType = "";

		switch (troop.troopType)
		{
			case TroopType.Infantry:
				unitType = ((InfantryTroopTier)troop.tier).ToString();
				break;
			case TroopType.Archer:
				unitType = ((ArcherTroopTier)troop.tier).ToString();
				break;
			case TroopType.Cavalry:
				unitType = ((CavalryTroopTier)troop.tier).ToString();
				break;
			case TroopType.Mage:
				unitType = ((MageTroopTier)troop.tier).ToString();
				break;
		}

		unitType = unitType.Replace("_", " ");

		label.Text = troop.quantity + " " + unitType + " (" + troop.troopType.ToString() + ")";
	}

	public void OnUpgradeButtonDown(TroopType type, int tier)
	{
		int cost = GetCostForUpgrade(new Troop(0, type, tier));

		if (playerData.gold < cost)
		{
			return;
		}

		playerData.gold -= cost;

		GetNode<RichTextLabel>("/root/BaseNode/UI/TrainerScreen/Background/Labels/Gold").Text = "Gold: " + playerData.gold;

		int toUpgradeIndex = 0;
		int toUpgradeIntoIndex = -1;

		for (int i = 0; i < playerData.troops.Count; i++)
		{
			if (playerData.troops[i].troopType == type)
			{
				if (playerData.troops[i].tier == tier)
				{
					toUpgradeIndex = i;
				}
				else if (playerData.troops[i].tier == tier + 1)
				{
					toUpgradeIntoIndex = i;
				}
			}
		}

		if (toUpgradeIntoIndex == -1)
		{
			Troop troop = new Troop(1, type, tier + 1);
			playerData.troops.Add(troop);
			PackedScene troopPanel = GD.Load<PackedScene>("res://Combat/troop_panel.tscn");
			GetNode<VBoxContainer>("/root/BaseNode/UI/TrainerScreen/Background/Labels/VBoxContainer").AddChild(CreateTroopPanel(troop, troopPanel));
		}
		else
		{
			playerData.troops[toUpgradeIntoIndex].quantity++;
		}

		playerData.troops[toUpgradeIndex].quantity--;

		foreach (Control troopPanel in GetNode<VBoxContainer>("/root/BaseNode/UI/TrainerScreen/Background/Labels/VBoxContainer").GetChildren())
		{
			TroopInfoHolder infoHolder = troopPanel.GetNode<TroopInfoHolder>("TroopInfo");

			if (infoHolder.troopType == type && infoHolder.tier == tier)
			{
				UpdateTroopText(troopPanel.GetNode<RichTextLabel>("Title"), playerData.troops[toUpgradeIndex]);

				if (playerData.troops[toUpgradeIndex].quantity <= 0)
				{
					GetNode<VBoxContainer>("/root/BaseNode/UI/TrainerScreen/Background/Labels/VBoxContainer").RemoveChild(troopPanel);
					troopPanel.QueueFree();
				}
			}
			else if (infoHolder.troopType == type && infoHolder.tier == tier + 1 && toUpgradeIntoIndex != -1)
			{
				UpdateTroopText(troopPanel.GetNode<RichTextLabel>("Title"), playerData.troops[toUpgradeIntoIndex]);
			}
		}

		if (playerData.troops[toUpgradeIndex].quantity <= 0)
		{
			playerData.troops.Remove(playerData.troops[toUpgradeIndex]);
		}
	}

	void CloseTrainerScreen()
	{
		GetNode<Control>("/root/BaseNode/UI/TrainerScreen").Visible = false;
		IsMovementDisabled = false;
		GlobalPauseState.Instance.IsPaused = false;
	}

	int GetCostForUpgrade(Troop troop)
	{
		float modifier = 1f;

		if (troop.troopType == TroopType.Mage)
		{
			modifier = 1.5f;
		}

		return (int)(troop.tier * 3 * modifier);
	}
}
