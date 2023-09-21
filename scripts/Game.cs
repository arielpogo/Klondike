using Godot;
using System;

/*
TODO:
(DONE) Have CardHolders detect when they collide with a pile's interact area
	(DONE) -Set all interactareas to their own layer
	(DONE) Enlarge the pile's interactAreas
	-Set some sort of system so no two piles are selected at the same time
	(DONE) -Don't forget foundations
(DONE) Find a way to make the stock's cards moveable
(DONE) Have cardholders (or piles) move cards to piles with checks and such
(DONE) Have cardholders (or foundations) move cards to foundations
(DONE)Win condition
---- Later additions
(DONE) Score
Selectable card backs
*/

/// <summary>
/// The general manager of the game, initializes other objects, the deck, and keeps references/constants.
/// </summary>
public partial class Game : Node2D {
	//constants
	public const int NUMCARDS = 52;
	public const int NUMTEXTURES = 56;
	//in pixels:
	public const int CARDWIDTH = 71;
	public const int CARDHEIGHT = 96;
	public const int MAXCARDSPACING = 32;
	public const int SCREENHEIGHT = 480;
	public const int SCREENWIDTH = 640;
	
	//arrays
	private Card[][] _cardArray = new Card[8][]; //stock, piles
	public AtlasTexture[] TextureArray {get; private set;} = new AtlasTexture[NUMTEXTURES+1]; //no Texture0.Tres
	
	//variables
	public int CardBackIndex {get; private set;} = 56;
	public int EmptyStockIndex{get; private set;} = 53;
	private double _seconds = 0.0;
	private int _minutes = 0;
	private int _score = 0;
	private int _completedFoundations = 0;
	private bool _winMode = false; //stop timer
	
	//references
	private Label _timer_label;
	private Label _score_label;
	public PackedScene CardHolderPrefab {get; private set;}

	Game(){
		PackedScene cardPrefab = GD.Load<PackedScene>("res://prefabs/card.tscn");
		CardHolderPrefab = GD.Load<PackedScene>("res://prefabs/card_holder.tscn");
		
		//initializing _cardArray
		_cardArray[0] = new Card[24]; //stock
		for(int i = 1; i < 8; i++) _cardArray[i] = new Card[7];
		
		//initializing deck as an array of ints 1-52
		int[] _deck = new int[52];
		for(int i = 0; i < 52; i++) _deck[i] = i+1;
		
		int j;
		Random r = new Random();
		//fisher and yates shuffle
		for(int i = 0; i < _deck.Length; i++){
			j = r.Next(_deck.Length); //random number b/w 0 and 51
			(_deck[j], _deck[i]) = (_deck[i], _deck[j]); //swap via tuples
		}
		
		//initialize stock cards
		j = 0;
		GD.Print(_cardArray[0].Length);
		for(int i = 0; i < _cardArray[0].Length; i++, j++) {
			_cardArray[0][i] = cardPrefab.Instantiate() as Card; //1st 24 cards into stock
			_cardArray[0][i].Value = _deck[j];
		}
		
		//initialize and place table cards
		for(int k = 1; k < 8; k++){ //for each pile
			_cardArray[k] = new Card[k]; //make an array
			for (int i = 0; i < k; i++, j++) { //1 face down to pile 2, 2 to 3, etc.
				_cardArray[k][i] = cardPrefab.Instantiate() as Card;
				_cardArray[k][i].Value = _deck[j];
			}
		}
		
		//load all card texture atlases
		for(int i = 1; i <= NUMTEXTURES; i++) TextureArray[i] = (AtlasTexture) GD.Load("res://textures/" + i.ToString() + ".tres");
		GD.Print("End of Game constructor");
	}
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready() { //Entered scene tree and all children are ready: give them their cards
		GD.Print("Game entered tree");
		_timer_label = GetNode<Label>("stats/timer");
		_score_label = GetNode<Label>("stats/score");

        Stock _stock = GetNode<Stock>("stock");
		_stock.SetCards(_cardArray[0]);

        Foundation[] _foundations = new Foundation[4];
		for(int i = 0; i < 4; i++) _foundations[i] = GetNode<Foundation>("foundation" + i.ToString());
        
		Pile[] _piles = new Pile[7];
        for (int i = 0; i < 7; i++){
			_piles[i] = GetNode<Pile>("pile" + i.ToString());
			_piles[i].AddCards(_cardArray[i+1]);
		}
		
		GD.Print("End of Game initialization; Game is Ready");
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame. Physics process runs consistenly unlike _Ready which runs as soon as possible.
	public override void _PhysicsProcess(double delta) { //Basically just run the timer.
		if (_winMode) return;

		_seconds += delta;
		if(_seconds >= 60.0) {
			_seconds -= 60.0;
			_minutes++;
		}
		//the ternary if checks if it's a single digit, if so add a leading zero
		_timer_label.Text = _minutes.ToString() + ":" + ((_seconds < 10.0) ? "0" : String.Empty) + ((int) _seconds).ToString();
	}


	/// <summary>
	/// Called by a foundation when a king card is placed.
	/// </summary>
    public void CompletedFoundation() {
        GD.Print($"completed: {++_completedFoundations}");

		//Finish the game
        if (_completedFoundations == 4) {
            _winMode = true;
            Winscreen winscreen = GD.Load<PackedScene>("res://prefabs/winscreen.tscn").Instantiate() as Winscreen;
            this.AddChild(winscreen);
            _timer_label.Reparent(winscreen);
            _score_label.Reparent(winscreen);
        }
    }

    /// <summary>
    /// Called by a foundation that was complete, but isn't anymore.
    /// </summary>
    public void UncompletedFoundation() {
        GD.Print($"completed: {--_completedFoundations}");
    }

	/// <summary>
	/// Modify the score of the game.
	/// </summary>
	/// <param name="i">The amount to modify the score by.</param>
    public void AddScore(int i) {
        _score += i;
        if (_score < 0) _score = 0;
        _score_label.Text = _score.ToString();
    }
}
