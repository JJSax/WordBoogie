using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace WordBoogie.Scenes;

public class BoogieScene : Scene
{
	readonly Board board;

	private readonly Dictionary<int, SoundEffect> Sounds = [];
	private Texture2D diceTexture;
	private Rectangle arrowTexture;
	private Texture2D sandTexture;
	private SpriteFont hugeFont;
	private SpriteFont largeFont;
	private SpriteFont smallFont;
	private SoundEffect NewWord;
	private SoundEffect RareLetterUsed;
	private SoundEffect Start;

	readonly Dice shuffleDie;
	readonly Dice blankDie;

	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;

	public delegate void DrawDiceGrid(SpriteBatch spriteBatch);
	public DrawDiceGrid cDraw;
	public delegate void UpdateBoard(GameTime gameTime);
	public UpdateBoard cUpdate;

	private int glintLetterIndex = 0;
	private const float GLINTTIME = 0.4f;
	private float glintTime = GLINTTIME;
	private const float GLINTHOLD = 1.5f;
	private float glintHold = 0f;

	private float timeInTimer = 1;
	private int timeIn = 2;

	BoogieState gameState;
	private string currentWord = "";
	private int aiWordIndex = 0;

	private int score = 0;
	private int aiScore = 0;
	private readonly int[] wordLengthScores = [0, 0, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9];

	public BoogieScene()
	{
		board = new Board();

		shuffleDie = new Dice();
		shuffleDie.MakeShuffleDice();

		blankDie = new Dice();
		blankDie.MakeBlankDice();

		Rectangle drawPos = Dice.GetDrawPosition(0, Board.height);
		sandTimer = new Rectangle(drawPos.X + 100, drawPos.Y, 40, Dice.ImageSize);
		arrowTexture = new(Dice.ImageSize * 3, Dice.ImageSize, Dice.ImageSize, Dice.ImageSize);

		cDraw += DrawTimeIn;
		cUpdate += UpdateTimeIn;
	}

	public override void Enter(Scene from)
	{
		Globals.Window.TextInput += TextInput;
		Globals.Window.KeyDown += KeyboardInput;
		MouseExt.LeftMousePressed += AttemptShuffle;

		ResetTimer();
		UpdateSand();
		FindWords();

		gameState = BoogieState.timeIn;

		Start.Play();

		base.Enter(from);
	}

	public override void Exit()
	{
		Globals.Window.TextInput -= TextInput;
		Globals.Window.KeyDown -= KeyboardInput;
		MouseExt.LeftMousePressed -= AttemptShuffle;

		base.Exit();
	}

	public override void LoadContent()
	{
		ContentManager content = Globals.Content;

		largeFont = content.Load<SpriteFont>("RobotoMono-Medium-large");
		smallFont = content.Load<SpriteFont>("RobotoMono-Medium-small");
		hugeFont = content.Load<SpriteFont>("Arial");

		Texture2D primitiveTexture = new(Globals.GraphicsDevice, 1, 1);
		primitiveTexture.SetData([Color.White]);
		sandTexture = primitiveTexture;

		diceTexture = content.Load<Texture2D>("dice");

		Sounds.Add(3, content.Load<SoundEffect>("3"));
		Sounds.Add(4, content.Load<SoundEffect>("4"));
		Sounds.Add(5, content.Load<SoundEffect>("5"));
		Sounds.Add(6, content.Load<SoundEffect>("6"));
		Sounds.Add(7, content.Load<SoundEffect>("7"));
		Sounds.Add(8, content.Load<SoundEffect>("8p"));

		NewWord = content.Load<SoundEffect>("chime");
		RareLetterUsed = content.Load<SoundEffect>("rareLetter");
		Start = content.Load<SoundEffect>("start");

		base.LoadContent();
	}

	public override void UnloadContent()
	{
		base.UnloadContent();
	}

	public override void Update(GameTime gameTime)
	{
		cUpdate(gameTime);
	}

	private void UpdateScores()
	{
		foreach (string word in board.AIBoardWords)
		{
			if (board.WordList.Contains(word))
			{
				int wordScore = wordLengthScores[word.Length];
				score -= wordScore;
				aiScore -= wordScore;
			}
		}
	}

	private void UpdateSand()
	{
		TimeSpan min3 = TimeSpan.FromMinutes(3);
		int topOffset = (int)((min3 - sandRemaining) / min3 * sandTimer.Height);
		sand = new Rectangle(sandTimer.X + 5, sandTimer.Y + topOffset, sandTimer.Width - 10, sandTimer.Height - topOffset);
	}

	public void UpdateGame(GameTime gameTime)
	{
		float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
		if (currentWord != null && gameState == BoogieState.scoreNegotiation)
		{
			glintTime -= dt;
			glintHold -= dt;
			if (glintHold <= 0 && glintHold + dt > 0)
			{
				glintLetterIndex = 0;
				glintTime = GLINTTIME;
			}
			if (glintTime <= 0 && glintHold <= 0)
			{
				glintLetterIndex++;
				glintTime = GLINTTIME;
				if (glintLetterIndex >= currentWord.Length)
				{
					glintLetterIndex = 0;
					glintHold = GLINTHOLD;
				}
			}
		}

		if (gameState != BoogieState.playing) return;
		sandRemaining -= gameTime.ElapsedGameTime;
		UpdateSand();

		if (sandRemaining <= TimeSpan.Zero)
		{
			gameState = BoogieState.scoreNegotiation;
			currentWord = board.AIBoardWords.ElementAtOrDefault(aiWordIndex);

			UpdateScores();
		}
	}

	public void UpdateTimeIn(GameTime gt)
	{
		timeInTimer -= (float)gt.ElapsedGameTime.TotalSeconds;
		if (timeInTimer <= 0)
		{
			timeIn--;
			timeInTimer += 1;

			if (timeIn < 1)
			{
				cUpdate -= UpdateTimeIn;
				cDraw -= DrawTimeIn;

				cUpdate += UpdateGame;
				cDraw += DrawPlayingBoard;

				gameState = BoogieState.playing;
			}
		}
	}

	private void DrawArrows(SpriteBatch spriteBatch, string word)
	{
		int diceSize = Dice.ImageSize;
		Vector2 offset = new(diceSize / 2, diceSize / 2);
		List<Vector2> currentPath = board.Solver.Paths[word];
		for (int i = 0; i < currentPath.Count - 1; i++)
		{
			Vector2 startPoint = currentPath[i];
			Vector2 endPoint = currentPath[i + 1];
			Vector2 midPoint = (startPoint + endPoint) / 2 * diceSize + offset;
			Rectangle midRect = new(
				(int)midPoint.X,
				(int)midPoint.Y,
				diceSize, diceSize
			);
			float angle = (endPoint - startPoint).ToAngle();
			spriteBatch.Draw(
				diceTexture, midRect, arrowTexture,
				Color.White, angle, offset, SpriteEffects.None, 0f
			);
		}
	}

	private void DrawTimeIn(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < Board.width; i++)
		{
			for (int j = 0; j < Board.height; j++)
			{
				blankDie.Draw(i, j, Color.White);
			}
		}
		spriteBatch.DrawString(hugeFont, "Get Ready", new(400, 200), Color.White);
	}

	private void DrawLetterBoard(SpriteBatch spriteBatch)
	{
		string word = board.AIBoardWords.ElementAtOrDefault(aiWordIndex);
		bool shouldHighlight = word != null && gameState == BoogieState.scoreNegotiation;

		Color color = gameState == BoogieState.scoreNegotiation ? Color.LightSlateGray : Color.White;
		for (int i = 0; i < Board.width; i++)
		{
			for (int j = 0; j < Board.height; j++)
			{
				board.board[i, j]?.Draw(i, j, color);
			}
		}

		if (!shouldHighlight) return;

		List<Vector2> currentWordPath = board.Solver.Paths[word];
		int total = currentWordPath.Count;
		int shineIndex = total - glintLetterIndex - 1; // 0-based from front

		for (int i = total - 1; i >= 0; i--)
		{
			Color c = glintHold < 0 && i != shineIndex ? Color.LightGray : Color.White;
			int ipx = (int)currentWordPath[i].X;
			int ipy = (int)currentWordPath[i].Y;
			board.board[ipx, ipy].Draw(ipx, ipy, c);
		}

		DrawArrows(spriteBatch, word);
	}

	public void DrawPlayingBoard(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(sandTexture, sandTimer, Color.Gray);
		spriteBatch.Draw(sandTexture, sand, Color.Yellow);
		shuffleDie.Draw(0, Board.height, Color.White);

		DrawLetterBoard(spriteBatch);

		if (currentWord != null)
		{
			Vector2 position = new(5, Globals.Window.ClientBounds.Height - 56);
			spriteBatch.DrawString(largeFont, currentWord, position, Color.Black);
		}

		Vector2 scorePosition = new(sandTimer.Right + 10, Dice.ImageSize * Board.height);
		spriteBatch.DrawString(smallFont, $"Score: {score}", scorePosition, Color.Black);

		Rectangle aiScoreRect = Dice.GetDrawPosition(0, Board.height);
		Vector2 aiScorePosition = new(5, aiScoreRect.Bottom);
		spriteBatch.DrawString(smallFont, $"AI Score: {aiScore}", aiScorePosition, Color.Black);

		int ind = 0;
		const int rightX = Board.width * 80 + 20;
		const int columnCapacity = 16;
		const int textHeight = 20;

		foreach (string entry in board.WordList)
		{
			Color wordColor = Color.Black;
			if (currentWord == entry) wordColor = Color.Crimson;
			Vector2 pos = new(
				rightX + (ind / columnCapacity * 150),
				ind % columnCapacity * textHeight
			);
			if (gameState == BoogieState.scoreNegotiation && board.AIBoardWords.Contains(entry))
			{
				Vector2 wordSize = smallFont.MeasureString(entry);
				Vector2 offset = new(0, 4);
				Vector2 startPos = pos + new Vector2(0, wordSize.Y / 2) - offset;
				Vector2 endPos = startPos + new Vector2(wordSize.X, 12);
				spriteBatch.DrawLine(startPos, endPos, Color.AntiqueWhite, 2f);
			}
			spriteBatch.DrawString(smallFont, entry, pos, wordColor);
			ind++;
		}
	}

	private void ResetTimer() => sandRemaining = TimeSpan.FromMinutes(3);
	private void KeyboardInput(object sender, InputKeyEventArgs args)
	{
		if (gameState != BoogieState.scoreNegotiation || board.AIBoardWords.Count == 0) return;

		Keys key = args.Key;
		bool validKey = false;

		switch (key)
		{
			case Keys.Up:
				validKey = true;
				Common.Wrap(++aiWordIndex, 0, board.AIBoardWords.Count, out aiWordIndex);
				break;
			case Keys.Down:
				validKey = true;
				Common.Wrap(--aiWordIndex, 0, board.AIBoardWords.Count, out aiWordIndex);
				break;
		}

		if (validKey)
		{
			glintLetterIndex = 0;
			glintTime = GLINTTIME;
			glintHold = 0;
		}

		currentWord = board.AIBoardWords[aiWordIndex];
	}

	private void TextInput(object sender, TextInputEventArgs args)
	{
		if (gameState != BoogieState.playing) return;

		Keys key = args.Key;
		KeyboardState keys = Keyboard.GetState();

		if (key == Keys.Enter && currentWord.Length > 0)
		{
			if (board.EnterUserWord(currentWord, out bool isNew))
			{
				if (isNew)
					NewWord.Play(0.25f, 0, 0);

				score += wordLengthScores.ElementAt(currentWord.Length);

				if (Board.RareLetterUsed(currentWord))
					RareLetterUsed.Play(0.25f, 0, 0);

				Sounds[(int)MathF.Min(8, currentWord.Length)].Play();
			}
			currentWord = "";
		}
		else if (key == Keys.Back && (keys.IsKeyDown(Keys.LeftControl) || keys.IsKeyDown(Keys.RightControl)) && currentWord.Length > 0)
			// Ctrl+Backspace
			currentWord = "";
		else if (key == Keys.Back && currentWord.Length > 0)
			currentWord = currentWord.Substring(0, currentWord.Length - 1);
		else if (char.IsLetter(args.Character))
			currentWord += char.ToUpper(args.Character);

		// Optional: Debug output
		// Debug.WriteLine($"Current Word: {currentWord}");
	}

	private void AttemptShuffle(Point position)
	{
		if (Dice.GetDrawPosition(0, Board.height).Contains(position) && (gameState == BoogieState.playing || gameState == BoogieState.scoreNegotiation))
		{
			cUpdate += UpdateTimeIn;
			cDraw += DrawTimeIn;

			cUpdate -= UpdateGame;
			cDraw -= DrawPlayingBoard;

			gameState = BoogieState.timeIn;
			timeInTimer = 1;
			timeIn = 2;

			Start.Play();
			ResetTimer();

			board.Shuffle();
			currentWord = "";
			aiWordIndex = 0;

			FindWords();
		}
	}

	private void FindWords()
	{
		board.FindAllWords();

		int[] odds = [0, 0, 0, 2, 4, 6, 10, 12, 15, 20, 21, 22, 23, 25, 30];

		if (aiScore >= aiScore + 20)
			odds[3] = 3;
		if (aiScore >= aiScore + 20)
			odds[3] = 1;

		aiScore += board.SetAIWords(odds, wordLengthScores);

	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		cDraw(spriteBatch);
	}
}
