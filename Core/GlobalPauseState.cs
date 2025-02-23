using Godot;
using System;

public partial class GlobalPauseState : Node
{
	public static GlobalPauseState Instance { get; private set; }

	[Signal]
	public delegate void PauseEventHandler();
	[Signal]
	public delegate void UnpauseEventHandler();

	private bool isPaused;
	public bool IsPaused { 
		get
		{
			return isPaused;
		}
		set 
		{
			isPaused = value;
			if (value)
			{ 
				EmitSignal(SignalName.Pause);
				//pauseLabel.Visible = true; 
			} 
			else 
			{ 
				EmitSignal(SignalName.Unpause);
				pauseLabel.Visible = false;
			}
		}
	}
	RichTextLabel pauseLabel;

	public override void _Ready()
	{
		pauseLabel = GetNode<RichTextLabel>("/root/BaseNode/UI/PauseLabel");
		Instance = this;
	}
}
