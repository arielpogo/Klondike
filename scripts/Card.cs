using Godot;

public partial class Card : Sprite2D {
	public void SetFacing(bool state){
		if(_face == state) return;
		else{
			_face = state;
			RefreshTexture();
		}
	}

	public bool GetFacing() { return _face; }
	
	private void RefreshTexture(){
		Game g = GetNode<Game>("/root/game");
		if(!_face) Texture = g.TextureArray[g.CardBackIndex];
		else Texture = g.TextureArray[Value];
	}
	
	public override void _Ready(){
		this.Name = "card" + Value.ToString();
		RefreshTexture();
		GD.Print("Card Added to Tree");
	}

	public void OnArea2DInputEvent(Viewport viewport, InputEvent input, int shape_idx) {
		if (input is InputEventMouseButton iemb && iemb.ButtonIndex == MouseButton.Left) {
			GD.Print("button and left");
			if (iemb.Pressed && _face &&!CardHolder.CardHolderExists) {
                GD.Print("pressed facing and card holder DNE");
                Vector2 locationClicked = GetGlobalMousePosition();
				//scan through the area2ds in our collision layer (cards only) we're overlapping
				foreach(Area2D a in GetNode<Area2D>("Area2D").GetOverlappingAreas()) {
					//ignore cards that weren't clicked
					if((a.GlobalPosition.Y-(Game.CARDHEIGHT/2)) > locationClicked.Y){
						GD.Print($"I am {this.Name}, I am comparing {a.GetParent().Name} and they weren't clicked!");
						continue;
					}
					//if a card is above us, then let them handle it
					if (this.GetIndex() < a.GetParent().GetIndex()){
						GD.Print($"I am {this.Name}, I am returning because {a.GetParent().Name} is above me");
					 return; //area's parent is a card	
					}
				}
				
				if(this.GetParent() is Pile parentPile) {
					GD.Print($"{this.Name} is calling the pile");
					parentPile.SeparateCard(this);
				}
				else if (this.GetParent() is Waste parentWaste){
					parentWaste.SeparateCard(this);
				}
				else if (this.GetParent() is Foundation parentFoundation) {
					parentFoundation.SeparateCard(this);
				}
			}
		}
	}

	public int Value = 1;
	private bool _face = false;
	//private bool _dragging = false;
	//private Vector2 _mouseGrabOffset {get; private set;} = new(0,0);
}
