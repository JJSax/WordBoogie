using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

namespace WordBoogie;

public class Board
{

	const int width = 4;
	const int height = 4;

	readonly Dice[,] board;
	readonly Dice shuffleDie;
	private readonly Texture2D diceTexture;
	private Rectangle arrowTexture;
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;
	private static GameWindow gameWindow;
	private SpriteFont largeFont;
	private SpriteFont smallFont;
	private readonly BoogieSolver solver;

	/// <summary>
	/// The list of words in this game that the player found.
	/// </summary>
	private readonly List<string> wordList = [];
	private string currentWord = "";

	/// <summary>
	/// All the words that are either confirmed as real, or use has used and accepted to be real.
	/// </summary>
	List<string> userWordList;
	/// <summary>
	/// All the words contained on this particular board.
	/// </summary>
	List<string> allBoardWords;
	/// <summary>
	/// The list of words the ai found
	/// </summary>
	private List<string> aiBoardWords;
	private int aiWordIndex = 0;

	private int score = 0;
	private int aiScore = 0;
	private readonly int[] wordLengthScores = [0, 0, 1, 1, 1, 2, 3, 4, 5, 6, 7];

	BoogieState gameState;
	readonly Random random = new();

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
		sandRemaining = TimeSpan.FromMinutes(3);
		UpdateSand();

		MouseExt.LeftMousePressed += AttemptShuffle;

		gameWindow = window;
		gameWindow.TextInput += TextInput;
		gameWindow.KeyDown += KeyboardInput;

		// Get paths to binary data folder
		string dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words_alpha_jj.txt");
		string realWordPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "confirmed_words.txt");

		userWordList = new(FileManager.ReadWords().Concat(File.ReadLines(realWordPath)));

		solver = new(File.ReadLines(dictionaryPath), userWordList);
		FindWords();

		gameState = BoogieState.playing;

		diceTexture = dice;
		arrowTexture = new(240, 80, Dice.GetImageSize, Dice.GetImageSize);
	}

	private void FindWords()
	{
		allBoardWords = solver.FindAllWords(board).ToList();

		aiBoardWords = [];

		int[] odds = [0,0,0, 2, 4, 6, 10, 12, 15, 20, 21, 22, 23, 25, 30];

		if (aiScore >= score + 20)
			odds[3] = 3;
		if (score >= aiScore + 20)
			odds[3] = 1;

		foreach (string word in allBoardWords)
		{
			if (solver.ConfirmedTrie.Search(word))
			{
				int choice = random.Next(odds[word.Length]);
				if (choice == 0)
				{
					aiBoardWords.Add(word);
					aiScore += wordLengthScores[word.Length];
				}
			}
		}
	}
	public void AddContent(Texture2D timerTexture, SpriteFont LargeFont, SpriteFont SmallFont)
	{
		largeFont = LargeFont;
		smallFont = SmallFont;
		sandTexture = timerTexture;
	}

	private void KeyboardInput(object sender, InputKeyEventArgs args)
	{
		if (gameState != BoogieState.scoreNegotiation) return;

		Keys key = args.Key;
		int wordCount = aiBoardWords.Count;

		switch (key)
		{
			case Keys.Up:
				Common.Wrap(++aiWordIndex, 0, wordCount, out aiWordIndex);
				break;
			case Keys.Down:
				Common.Wrap(--aiWordIndex, 0, wordCount, out aiWordIndex);
				break;
		}

		currentWord = aiBoardWords[aiWordIndex];
	}

	private void TextInput(object sender, TextInputEventArgs args)
	{
		if (gameState != BoogieState.playing) return;

		Keys key = args.Key;
		KeyboardState keys = Keyboard.GetState();

		if (key == Keys.Enter && currentWord.Length > 0)
		{
			if (allBoardWords.Contains(currentWord) && !wordList.Contains(currentWord))
			{
				if (!solver.WordInConfirmed(currentWord))
				{
					FileManager.AppendWord(currentWord);
					userWordList.Add(currentWord);
				}

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
		wordList.Clear();

		FindWords();
		gameState = BoogieState.playing;

		currentWord = "";
		aiWordIndex = 0;
		sandRemaining = TimeSpan.FromMinutes(3);
	}

	private void updateScores()
	{
		foreach (string word in aiBoardWords)
		{
			if (wordList.Contains(word))
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

	public void Update(GameTime gameTime)
	{

		if (gameState != BoogieState.playing) return;
		sandRemaining -= gameTime.ElapsedGameTime;
		UpdateSand();

		if (sandRemaining <= TimeSpan.Zero)
		{
			gameState = BoogieState.scoreNegotiation;
			currentWord = aiBoardWords.ElementAtOrDefault(aiWordIndex);

			updateScores();
		}
	}

	private void DrawArrows(SpriteBatch spriteBatch)
	{
		if (gameState != BoogieState.scoreNegotiation) return;

		string word = aiBoardWords.ElementAt(aiWordIndex);
		if (word == null) return;

		int diceSize = Dice.GetImageSize;
		Vector2 offset = new(diceSize / 2, diceSize / 2);
		Stack<Vector2> copyVector = new(solver.Paths[word]);
		Vector2 startPoint = copyVector.Pop();
		for (int i = 1; i < word.Length; i++)
		{
			Vector2 endPoint = copyVector.Pop();
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
			startPoint = endPoint;
		}
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

		Rectangle aiScoreRect = shuffleDie.getDrawPosition(0, height);
		Vector2 aiScorePosition = new(5, aiScoreRect.Bottom);
		spriteBatch.DrawString(smallFont, $"AI Score: {aiScore}", aiScorePosition, Color.Black);

		DrawArrows(spriteBatch);

		int ind = 0;
		const int leftX = width * 80 + 20;
		const int columnCapacity = 16;
		const int textHeight = 20;

		foreach (string entry in wordList)
		{
			Color wordColor = Color.Black;
			if (currentWord == entry) wordColor = Color.Crimson;
			Vector2 pos = new(
				leftX + (ind / columnCapacity * 150),
				ind % columnCapacity * textHeight
			);
			if (gameState == BoogieState.scoreNegotiation && aiBoardWords.Contains(entry))
			{
				Vector2 wordSize = smallFont.MeasureString(entry);
				Vector2 offset = new(0, 4);
				Vector2 startPos = pos + new Vector2(0, wordSize.Y/2) - offset;
				Vector2 endPos = startPos + new Vector2(wordSize.X, 12);
				spriteBatch.DrawLine(startPos, endPos, Color.AntiqueWhite, 2f);
			}
			spriteBatch.DrawString(smallFont, entry, pos, wordColor);
			ind++;
		}
	}


}
