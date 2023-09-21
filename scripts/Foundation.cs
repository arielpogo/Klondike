using Godot;
using System.Collections.Generic;

public partial class Foundation : Node2D {
    Stack<Card> _stack = new();
    private Color _highlightColor = Color.Color8(44, 109, 119);
    private Color _noHighlightColor = Color.Color8(255, 255, 255);

    public void AddCard(Card c) { //gets cards from game or a cardHolder
        if (_stack.Count > 0) _stack.Peek().SelfModulate = _noHighlightColor;
        this.SelfModulate = _noHighlightColor;
        _stack.Push(c);
        c.Reparent(this);
        c.GlobalPosition = this.GlobalPosition;
        if(c.Value >= 49)GetNode<Game>("/root/game").CompletedFoundation(); //king placed
    }

    public bool AskIfCardHolderIsPlaceable(CardHolder holder) {
        if (holder.GetNumberOfCards() == 1) {
            int holderValue = holder.GetBaseCardValue();

            if (_stack.Count <= 0){
                if (holderValue <= 4) { //allow aces to be placed on empty piles
                    this.SelfModulate = _highlightColor;
                    return true;
                }
                else return false;
            }
            else {
                int myTopCardValue = _stack.Peek().Value;
                GD.Print($"(holderValue - 1) / 4: {(holderValue - 1) / 4}");
                GD.Print($"myTopCardValue / 4 - 1 {myTopCardValue / 4 - 1}");
                GD.Print($"holderValue % 4 {holderValue % 4}");
                GD.Print($"myTopCardValue % 4: {myTopCardValue % 4}");
                if ((holderValue - 1) / 4 == (myTopCardValue -1) / 4 + 1  && holderValue % 4 == myTopCardValue % 4) {//if the base card of the held stack is 1 greater than our value and the same suit
                    _stack.Peek().SelfModulate = _highlightColor;
                    return true;
                }
            }
        }
        return false;
    }
    public void NotifyCardHolderLeft() {
        if (_stack.Count > 0) _stack.Peek().SelfModulate = _noHighlightColor;
        this.SelfModulate = _noHighlightColor;
    }

    public void SeparateCard(Card targetCard) {
        if (targetCard == _stack.Peek()) {
            _stack.Pop();
            if (targetCard.Value >= 49) GetNode<Game>("/root/game").UncompletedFoundation(); //king placed
        }
        else {
            GD.PrintErr("Attempt to remove card from foundation out of order");
            return;
        }

        Game g = GetNode<Game>("/root/game"); //get Game
        CardHolder cardHolder = g.CardHolderPrefab.Instantiate() as CardHolder; //new CardHolder
        g.AddChild(cardHolder); //put the cardHolder in the sceneTree
        cardHolder.GlobalPosition = targetCard.GlobalPosition; //move it to the clicked card, before it calculates its mousegraboffset in SetCards
        cardHolder.SetCards(new Card[] { targetCard }, this); //Give it the card
    }
}
