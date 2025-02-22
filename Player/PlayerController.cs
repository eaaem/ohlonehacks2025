using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
	// Input map actions need to be replaced
	public const float Speed = 5.0f;
	public const float JumpVelocity = 4.5f;

	[Export]
	private Node3D cameraTarget;

	private Vector3 moveTarget;
	public bool IsMoving { get; set; }
	public bool IsMovementDisabled { get; set; }
	private bool isHoldingMiddleMouse = false;

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
