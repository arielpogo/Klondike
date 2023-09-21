using Godot;
using System;
using System.Collections.Generic;

public partial class CardHolder : Node2D {
	private Vector2 _mouseGrabOffset = new(0,0);
	private Stack<Card> _stack = new Stack<Card>();
	private Node _origin;
	public Node destination = null;
	public static bool CardHolderExists = false;

	public delegate void CardHolderPlacedDelgate();
	public static CardHolderPlacedDelgate CardHolderPlaced;

	public void SetCards(Card[] cards, Node origin) { //gets cards from a Pile, Waste, Foundation
		CardHolderExists = true;
		_origin = origin;
		for (int i = 0; i < cards.Length; i++) {
            _stack.Push(cards[i]);
        }
		for(int i = cards.Length - 1; i >= 0; i--) cards[i].Reparent(this);

        _mouseGrabOffset = this.GlobalPosition - GetGlobalMousePosition();
		//sometimes your mouse can move far enough in time to go beyond a cardHolder's area2d, meaning no input will be detected, thus the clamp
		_mouseGrabOffset = new(Math.Clamp(_mouseGrabOffset.X, Game.CARDWIDTH * -1, Game.CARDWIDTH), Math.Clamp(_mouseGrabOffset.Y, Game.CARDHEIGHT * -1, Game.CARDHEIGHT));
	}

	public int GetBaseCardValue() {
		return _stack.Peek().Value;
	}

	public int GetNumberOfCards() {
		return _stack.Count;
	}
	
	//Called when any input is given to its Area2D, we only care about left mouse buttons being unpressed
	private void OnArea2DInputEvent(Node viewport, InputEvent input, long shape_idx) {
		if (input is InputEventMouseButton iemb && iemb.ButtonIndex == MouseButton.Left && !iemb.Pressed) {
			CardHolderExists = false;
			if (destination != null) { //a new home was found
				Game g = GetNode<Game>("/root/game");

				if(destination is Pile p) { //in the future I should make a base class for these with shared interfaces
                    p.AddCards(_stack.ToArray());
					if (_origin is Waste) g.AddScore(5);
					else if (_origin is Foundation) g.AddScore(-15);
                    QueueFree();
                }
				else if (destination is Foundation f) {
					f.AddCard(_stack.Pop());
                    if (_origin is Waste || _origin is Pile) g.AddScore(10);
                    QueueFree();
				}
                CardHolderPlaced.Invoke();
            }
			else { //no home found
				GD.Print($"No home found for cardHolder with base card value {GetBaseCardValue()}");
				if (_origin is Pile p) {
					p.AddCards(_stack.ToArray());
					QueueFree();
				}
				else if (_origin is Waste w) {
					w.RecieveCard(_stack.Pop());
					QueueFree();
				}
				else if (_origin is Foundation f) {
                    f.AddCard(_stack.Pop());
                    QueueFree();
                }
			}
		}
	}

	private void OnArea2DEntered(Area2D area) {
		if (area.GetParent() is Pile p && p.AskIfCardHolderIsPlaceable(this)) {
			if (destination is Pile dp) dp.NotifyCardHolderLeft(); //this is stupid
			else if (destination is Foundation df) df.NotifyCardHolderLeft();
			destination = p;
            GD.Print($"Destination is {destination.Name}");
        }
		else if (area.GetParent() is Foundation f && f.AskIfCardHolderIsPlaceable(this)) {
            if (destination is Pile dp) dp.NotifyCardHolderLeft();
            else if (destination is Foundation df) df.NotifyCardHolderLeft();
            destination = f;
            GD.Print($"Destination is {destination.Name}");
        }
	}

	private void OnArea2DExited(Area2D area) {
		if (area == destination) {
			if (area.GetParent() is Pile p) {
				p.NotifyCardHolderLeft();
				destination = null;
			}
			else if (area.GetParent() is Foundation f) {
				f.NotifyCardHolderLeft();
				destination = null;
			}
		}
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) {
		this.GlobalPosition = GetGlobalMousePosition() + _mouseGrabOffset;
	}
}



