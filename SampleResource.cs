using Godot;
using System;

[GlobalClass]
public partial class SampleResource : Resource
{
    [Export]
    public string MyName { get; set; }
    [Export]
    public int id;
    [Export]
    public float modifier;
}
