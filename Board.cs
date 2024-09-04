using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace Boggle;

public class Board
{

	const int width = 4;
	const int height = 4;

    readonly Dice[,] board;
	readonly Dice shuffleDie;
	private Texture2D diceTexture;
	private Rectangle arrowTexture;
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;
	private static GameWindow gameWindow;
	private SpriteFont largeFont;
	private SpriteFont smallFont;
	private readonly BoggleSolver solver;

	/// <summary>
	/// The list of words in this game that the player found.
	/// </summary>
	private readonly List<string> wordList = [];
	private string currentWord = "";

	/// <summary>
	/// All the words contained on this particular board.
	/// </summary>
    List<string> allWords;

	private int score = 0;
	private readonly int[] wordLengthScores = [0, 0, 1, 1, 2, 3, 4, 5, 6, 7];

	BoggleState gameState;
	private int aiWordIndex = 0;

	public Board(GameWindow window, Texture2D dice)
	{
		board = new Dice[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j] = new Dice();
			}
		}

		shuffleDie = new Dice();
		shuffleDie.MakeShuffleDice();

		Rectangle drawPos = shuffleDie.getDrawPosition(0, height);
		sandTimer = new Rectangle(drawPos.X + 100, drawPos.Y, 40, 80);
		sandRemaining = TimeSpan.FromMinutes(1);
		UpdateSand();

		MouseExt.LeftMousePressed += AttemptShuffle;

		gameWindow = window;
		gameWindow.TextInput += TextInput;
		gameWindow.KeyDown += KeyboardInput;

		string filePath = "words_alpha_jj.txt";
		solver = new(File.ReadLines(filePath));
		FindWords();

		gameState = BoggleState.playing;

		diceTexture = dice;
		arrowTexture = new(240, 80, Dice.GetImageSize, Dice.GetImageSize);
	}

	private void FindWords()
	{
		allWords = solver.FindWords(board).ToList();
	}

	public void AddContent(SpriteFont inFont, bool isLarge) { largeFont = inFont; }
	public void AddContent(SpriteFont inFont) { smallFont = inFont; }
	public void AddContent(Texture2D timerTexture) { sandTexture = timerTexture; }
	public void AddContent(Texture2D timerTexture, SpriteFont LargeFont, SpriteFont SmallFont)
	{
		AddContent(LargeFont, true);
		AddContent(SmallFont);
		AddContent(timerTexture);
	}

	private void KeyboardInput(object sender, InputKeyEventArgs args)
	{
		if (gameState != BoggleState.scoreNegotiation) return;

		Keys key = args.Key;
		int wordCount = allWords.Count;

		switch (key)
		{
			case Keys.Up:
				Common.Wrap(++aiWordIndex, 0, wordCount, out aiWordIndex);
				break;
			case Keys.Down:
				Common.Wrap(--aiWordIndex, 0, wordCount, out aiWordIndex);
				break;
		}

		currentWord = allWords[aiWordIndex];
	}

	private void TextInput(object sender, TextInputEventArgs args)
	{
		if (gameState != BoggleState.playing) return;

		Keys key = args.Key;
		KeyboardState keys = Keyboard.GetState();

		if (key == Keys.Enter && currentWord.Length > 0)
		{
			if (allWords.Contains(currentWord) && !wordList.Contains(currentWord))
			{
				wordList.Add(currentWord);
				score += wordLengthScores.ElementAt(currentWord.Length);
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
		if (shuffleDie.getDrawPosition(0, height).Contains(position))
			Shuffle();
	}

	public void Shuffle()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j].ChooseLetter();
			}
		}
		sandRemaining = TimeSpan.FromMinutes(3);
		wordList.Clear();

		FindWords();
		gameState = BoggleState.playing;
	}

	private void UpdateSand()
	{
		TimeSpan min3 = TimeSpan.FromMinutes(3);
		int topOffset = (int)((min3 - sandRemaining) / min3 * sandTimer.Height);
		sand = new Rectangle(sandTimer.X + 5, sandTimer.Y + topOffset, sandTimer.Width - 10, sandTimer.Height - topOffset);
	}

	public void Update(GameTime gameTime)
	{

		if (gameState != BoggleState.playing) return;
		sandRemaining -= gameTime.ElapsedGameTime;
		UpdateSand();

		if (sandRemaining <= TimeSpan.Zero) gameState = BoggleState.scoreNegotiation;
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j]?.Draw(i, j);
			}
		}

		spriteBatch.Draw(sandTexture, sandTimer, Color.Gray);
		spriteBatch.Draw(sandTexture, sand, Color.Yellow);
		shuffleDie.Draw(0, height);

		Vector2 position = new(5, gameWindow.ClientBounds.Height - 56);
		spriteBatch.DrawString(largeFont, currentWord, position, Color.Black);

		Vector2 scorePosition = new(sandTimer.Right + 10, Dice.GetImageSize * height);
		spriteBatch.DrawString(smallFont, $"Score: {score}", scorePosition, Color.Black);

		//! TEST LINE
		if (gameState == BoggleState.scoreNegotiation)
		{
			int diceSize = Dice.GetImageSize;
			Vector2 offset = new(diceSize/2, diceSize/2);
			string word = allWords.ElementAt(aiWordIndex);
			if (word != null){
				Stack<Vector2> copyVector = new(solver.Paths[word]);
				Vector2 startPoint = copyVector.Pop();
				for (int i = 1; i < word.Length; i++)
				{
					Vector2 endPoint = copyVector.Pop();
					Vector2 midPoint = (startPoint + endPoint) / 2 * diceSize + offset;
					spriteBatch.DrawCircle(midPoint, 4, 20, Color.Magenta, 1f, 0f);
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
					startPoint = endPoint;
				}
			}
		}

		int ind = 0;
		const int leftX = width * 80 + 20;
		const int columnCapacity = 20;
		const int textHeight = 20;
		foreach (string entry in wordList)
		{
			Vector2 pos = new(
				leftX + (ind / columnCapacity * 150),
				ind % columnCapacity * textHeight
			);
			spriteBatch.DrawString(smallFont, entry, pos, Color.Black);
			ind++;
		}
	}


}
