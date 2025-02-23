using Godot;
using System;

public partial class GlobalPauseState : Node
{
	public static GlobalPauseState Instance { get; private set; }

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
				pauseLabel.Visible = true; 
			} 
			else 
			{ 
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
