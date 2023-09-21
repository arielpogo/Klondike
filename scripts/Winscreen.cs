using Godot;

public partial class Winscreen : Control {
    private void OnReturnPressed() {
        GetTree().ChangeSceneToFile("res://scenes/main_menu.tscn");
    }
}

