using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// A pile on the tableu.
/// </summary>
public partial class Pile : Sprite2D {
	private Stack<Card> _stack = new Stack<Card>(); //Keeps track of its cards
	private Color _highlightColor =  Color.Color8(44, 109, 119); //The color tint when a cardholder hovers over
	private Color _noHighlightColor = Color.Color8(255, 255, 255); //The default color tint (aka none)

	Pile() {
		CardHolder.CardHolderPlaced += CardHolderPlaced; //subscribe to the delegate which notifies when a card holder finishes placing cards
	}

	~Pile() {
		CardHolder.CardHolderPlaced -= CardHolderPlaced; //prevent memory leak by unsubscribing
	}

    public void AddCards(Card[] cards) { //gets cards from game or a cardHolder
		//cards placed: detint
		if (_stack.Count > 0) _stack.Peek().SelfModulate = _noHighlightColor;
		this.SelfModulate = _noHighlightColor;

		//process cards
        for (int i = 0; i < cards.Length; i++) {
			//GD.Print(cards[i].Name);
			_stack.Push(cards[i]);
			if (cards[i].GetParent() == null) this.AddChild(cards[i]); //for orphaned cards from Game
			else cards[i].Reparent(this); //default moving cards to being a parent
		}
		
		_stack.Peek().SetFacing(true); //top card should always be facing up
		AdjustCardPositions();
	}

	/// <summary>
	/// Take cards up to and including the target card and creates a cardHolder.
	/// </summary>
	/// <param name="targetCard">The card to take and all above it.</param>
	public void SeparateCard(Card targetCard) {
		//we need to maintain an odd number of stacks to maintain the order of the cards: this, the cardHolder, and the destination (an even # flips the order)
		//if we used a stack here, then we would need to use a queue in the cardHolder. Either is possible but I like instant access
		//to the last card in a cardHolder that a queue cannot provide. Thus queue here, stack in cardHolder (but must add its children to the scene
		//backwards to maintain their z indexes on the screen)
		Queue<Card> pickedUpCards = new Queue<Card>();

		//Get all cards up to the target card (we cannot see the last element of a queue and can't see cards we already moved)
		while(_stack.Peek() != targetCard) pickedUpCards.Enqueue(_stack.Pop());
		pickedUpCards.Enqueue(_stack.Pop()); //get the target card

		Game g = GetNode<Game>("/root/game"); //get game

        CardHolder cardHolder = g.CardHolderPrefab.Instantiate() as CardHolder; //new CardHolder
        g.AddChild(cardHolder); //put the cardHolder in the sceneTree
		cardHolder.GlobalPosition = targetCard.GlobalPosition; //move it to the clicked card, before it calculates its mousegraboffset in cardHolder.SetCards()
		cardHolder.SetCards(pickedUpCards.ToArray(), this); //Give it the cards removed
		//GD.Print($"Pile: {_stack.Peek().Name}");
	}

	/// <summary>
	/// Tells the cardHolder if its card(s) can go here.
	/// </summary>
	/// <param name="holder">The requesting holder.</param>
	/// <returns></returns>
	public bool AskIfCardHolderIsPlaceable(CardHolder holder) {
        int holderValue = holder.GetBaseCardValue();
		//GD.Print("Cardholder found!");

		if (_stack.Count <= 0){
			if (holderValue >= 49) { //allow kings to be placed on empty piles
				this.SelfModulate = _highlightColor; //tint the pile sprite
				return true;
			}
            else return false; //empty piles can't take a cardHolder that doesn't have a king at the base
        }
		else if (_stack.Peek().GetFacing()) {//not sure why we check this actually
			int myTopCardValue = _stack.Peek().Value;

			if ((holderValue - 1) / 4 + 1 == (myTopCardValue - 1) / 4 && holderValue % 2 != myTopCardValue % 2) {//if the base card of the held stack is 1 lower than our value and the opposite color
				_stack.Peek().SelfModulate = _highlightColor; //tint the top card
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Let us know the cardHolder isn't in our range anymore
	/// </summary>
	public void NotifyCardHolderLeft() {
		//detint
		if(_stack.Count > 0) _stack.Peek().SelfModulate = _noHighlightColor;
		this.SelfModulate = _noHighlightColor;
    }

	/// <summary>
	/// The card holder that came from this pile placed, I may flip my top card now.
	/// </summary>
	public void CardHolderPlaced() {
		if (_stack.Count > 0 && _stack.Peek().GetFacing() == false) {
			_stack.Peek().SetFacing(true);
            GetNode<Game>("/root/game").AddScore(5);
            AdjustCardPositions();
        }
    }

	/// <summary>
	/// Readjust a stack of cards to fit them on the screen.
	/// </summary>
	private void AdjustCardPositions() {
		float distFromBottom = Game.SCREENHEIGHT - (this.GlobalPosition.Y - Game.CARDHEIGHT/2); //top of the pile obj
		int numCards = _stack.Count - 1; //we dont care about the 0th card which is exactly aligned with pile obj
		float spacing = Game.MAXCARDSPACING;

		if (numCards * spacing + Game.CARDHEIGHT > distFromBottom) {
			spacing = (float) Math.Floor((distFromBottom - Game.CARDHEIGHT) / numCards);
		}

		int i = _stack.Count - 1;
		foreach (Card c in _stack) {
			c.GlobalPosition = new(this.GlobalPosition.X, this.GlobalPosition.Y + spacing * i);
			i--;
		}
	}
}
