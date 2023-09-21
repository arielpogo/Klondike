using Godot;
using System.Collections.Generic;

public partial class Waste : Sprite2D {
	private Stack<Card> _stack = new Stack<Card>();
		
	public void RecieveCard(Card c){
		_stack.Push(c);
		c.Reparent(this);
		c.SetFacing(true);

		c.GlobalPosition = this.GlobalPosition;
	}
	
	public Card RemoveTopCard(){
		if(_stack.Count > 0) return _stack.Pop();
		else return null;
	}

    public void SeparateCard(Card targetCard) {
		if(targetCard == _stack.Peek()) _stack.Pop();
		else{
			GD.PrintErr("Attempt to remove card from waste out of order");
			return;
		}
		
		Game g = GetNode<Game>("/root/game"); //get Game
		CardHolder cardHolder = g.CardHolderPrefab.Instantiate() as CardHolder; //new CardHolder
		g.AddChild(cardHolder); //put the cardHolder in the sceneTree
		cardHolder.GlobalPosition = targetCard.GlobalPosition; //move it to the clicked card, before it calculates its mousegraboffset in SetCards
		cardHolder.SetCards(new Card[]{targetCard}, this); //Give it the card
	}
}
