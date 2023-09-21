using Godot;
using System;

public partial class MainMenu : Control {
	PackedScene instructions_control;
	PackedScene credits_control;
	
	public override void _Ready() {
		instructions_control = GD.Load<PackedScene>("res://prefabs/instructions_control.tscn");
		credits_control = GD.Load<PackedScene>("res://prefabs/credits_control.tscn");
	}
	
	private void _on_play_pressed() {
		GetTree().ChangeSceneToFile("res://scenes/game.tscn");	
	}
	
	private void _on_instructions_pressed() {
		Node instructions = instructions_control.Instantiate();
		GetTree().Root.GetNode<Node2D>("mainMenu").AddChild(instructions);
	}
	private void _on_credits_pressed() {
		Node credits = credits_control.Instantiate();
		GetTree().Root.GetNode<Node2D>("mainMenu").AddChild(credits);
	}
}
