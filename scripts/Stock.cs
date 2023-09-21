using Godot;
using System.Collections.Generic;

public partial class Stock : TextureButton {
	private Stack<Card> _stack = new Stack<Card>();
	private Waste _waste;
	private Game _game;

	//Gets starting cards from Game
    public void SetCards(Card[] cards){ 
		for(int i = cards.Length - 1; i >= 0; i--){
			_stack.Push(cards[i]);
			this.AddChild(cards[i]);
			cards[i].GlobalPosition = new(-100, 0);
		}
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() { 
		_game = GetNode<Game>("/root/game");
		_waste = GetNode<Waste>("/root/game/waste");
		this.TextureNormal = _game.TextureArray[_game.CardBackIndex];
	}
	
	//called when pressed by the mouse
	public override void _Pressed(){
		if (_stack.Count <= 0) { //empty, get all cards from the waste
            Card c;
			while ((c = _waste.RemoveTopCard()) != null) {
				_stack.Push(c);
				c.Reparent(this);
				c.GlobalPosition = new(-100, 0); //move it offscreen
			}
			this.TextureNormal = _game.TextureArray[_game.CardBackIndex];
			_game.AddScore(-100);
		}
		else _waste.RecieveCard(_stack.Pop()); //otherwise push the next card to the waste
		
		//if empty now, then change the texture to the empty stack texture
		if(_stack.Count <= 0) this.TextureNormal = _game.TextureArray[_game.EmptyStockIndex];
	}
}
