using Godot;
using System;

public partial class SampleResourceHolder : Button
{
	[Export]
	SampleResource myResource;
	
	public void OnButtonDown()
	{
		GD.Print(myResource.MyName + " " + myResource.id);
	}
}
