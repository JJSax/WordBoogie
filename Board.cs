using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.ECS;

namespace Boggle;

public class Board
{

	const int width = 4;
	const int height = 4;

    readonly Dice[,] board;
	readonly Dice shuffleDie;
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;
	private static GameWindow gameWindow;
	private SpriteFont largeFont;
	private SpriteFont smallFont;
	private readonly BoggleSolver solver;
	private readonly List<string> wordList = [];
	private string currentWord = "";
    HashSet<string> allWords;

	private int score = 0;
	private readonly int[] wordLengthScores = [0, 0, 1, 1, 2, 3, 4, 5, 6, 7];

	public Board(GameWindow window)
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
		sandRemaining = TimeSpan.FromMinutes(3);
		UpdateSand();

		MouseExt.LeftMousePressed += AttemptShuffle;

		gameWindow = window;
		gameWindow.TextInput += TextInput;

		string filePath = "words_alpha_jj.txt";
		solver = new(File.ReadLines(filePath));
		FindWords();
	}

	private void FindWords()
	{
		allWords = solver.FindWords(board);

		foreach (string word in allWords)
		{
			Debug.WriteLine($"Board contains word: \"{word}\"");
		}
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

	private void TextInput(object sender, TextInputEventArgs args)
	{
		Keys key = args.Key;
		KeyboardState keys = Keyboard.GetState();

		if (key == Keys.Enter && currentWord.Length > 0)
		{
			if (allWords.Contains(currentWord) && !wordList.Contains(currentWord) && sandRemaining > TimeSpan.Zero)
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
	}

	private void UpdateSand()
	{
		TimeSpan min3 = TimeSpan.FromMinutes(3);
		int topOffset = (int)((min3 - sandRemaining) / min3 * sandTimer.Height);
		sand = new Rectangle(sandTimer.X + 5, sandTimer.Y + topOffset, sandTimer.Width - 10, sandTimer.Height - topOffset);
	}

	public void Update(GameTime gameTime)
	{
		sandRemaining -= gameTime.ElapsedGameTime;
		UpdateSand();
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
		int diceSize = Dice.GetImageSize;
		Vector2 offset = new(diceSize/2, diceSize/2);
		string word = solver.FoundWords.First();
		if (word != null){
			Stack<Vector2> copyVector = new(solver.Paths[word]);
			Vector2 startPoint = copyVector.Pop() * diceSize;
			for (int i = 1; i < word.Length; i++)
			{
				Vector2 endPoint = copyVector.Pop() * diceSize;
				spriteBatch.DrawLine(startPoint + offset, endPoint + offset, Color.Green, 5);
				startPoint = endPoint;
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
