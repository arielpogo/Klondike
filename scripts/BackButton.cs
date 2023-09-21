using Godot;

public partial class BackButton : Button {
	private void _on_pressed() {
		GetParent().QueueFree();
	}
}
