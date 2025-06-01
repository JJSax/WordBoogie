using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
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
	readonly Dice blankDie;
	private readonly Texture2D diceTexture;
	private Rectangle arrowTexture;
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;
	private static GameWindow gameWindow;
	private SpriteFont hugeFont;
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
	readonly HashSet<string> userWordList;
	/// <summary>
	/// All the words contained on this particular board.
	/// </summary>
	HashSet<string> allBoardWords;
	/// <summary>
	/// The list of words the ai found
	/// </summary>
	private List<string> aiBoardWords;
	private int aiWordIndex = 0;

	private int score = 0;
	private int aiScore = 0;
	private readonly int[] wordLengthScores = [0, 0, 1, 1, 1, 2, 3, 4, 5, 6, 7, 8, 9];

	private int glintLetterIndex = 0;
	private const float GLINTTIME = 0.4f;
	private float glintTime = GLINTTIME;
	private const float GLINTHOLD = 1.5f;
	private float glintHold = 0f;

	private float timeInTimer = 1;
	private int timeIn = 2;

	public delegate void DrawDiceGrid(SpriteBatch spriteBatch);
	public DrawDiceGrid Draw;
	public delegate void UpdateBoard(GameTime gameTime);
	public UpdateBoard Update;

	BoogieState gameState;
	readonly Random random = new();

	private void ResetTimer() => sandRemaining = TimeSpan.FromMinutes(3);

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

		blankDie = new Dice();
		blankDie.MakeBlankDice();

		Rectangle drawPos = Dice.GetDrawPosition(0, height);
		sandTimer = new Rectangle(drawPos.X + 100, drawPos.Y, 40, Dice.ImageSize);
		ResetTimer();
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

		gameState = BoogieState.timeIn;
		Draw += DrawTimeIn;
		Update += UpdateTimeIn;

		diceTexture = dice;
		arrowTexture = new(Dice.ImageSize * 3, Dice.ImageSize, Dice.ImageSize, Dice.ImageSize);
	}

	private void FindWords()
	{
		allBoardWords = solver.FindAllWords(board);

		aiBoardWords = [];

		int[] odds = [0, 0, 0, 2, 4, 6, 10, 12, 15, 20, 21, 22, 23, 25, 30];

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

	public void AddContent(ContentManager content, GraphicsDevice graphics)
	{
		largeFont = content.Load<SpriteFont>("RobotoMono-Medium-large");
		smallFont = content.Load<SpriteFont>("RobotoMono-Medium-small");
		hugeFont = content.Load<SpriteFont>("Arial");

		Texture2D primitiveTexture = new(graphics, 1, 1);
		primitiveTexture.SetData([Color.White]);
		sandTexture = primitiveTexture;
	}

	private void KeyboardInput(object sender, InputKeyEventArgs args)
	{
		if (gameState != BoogieState.scoreNegotiation || aiBoardWords.Count == 0) return;

		Keys key = args.Key;
		bool validKey = false;

		switch (key)
		{
			case Keys.Up:
				validKey = true;
				Common.Wrap(++aiWordIndex, 0, aiBoardWords.Count, out aiWordIndex);
				break;
			case Keys.Down:
				validKey = true;
				Common.Wrap(--aiWordIndex, 0, aiBoardWords.Count, out aiWordIndex);
				break;
		}

		if (validKey)
		{
			glintLetterIndex = 0;
			glintTime = GLINTTIME;
			glintHold = 0;
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
		if (Dice.GetDrawPosition(0, height).Contains(position) && (gameState == BoogieState.playing || gameState == BoogieState.scoreNegotiation))
		{
			Update += UpdateTimeIn;
			Draw += DrawTimeIn;

			Update -= UpdateGame;
			Draw -= DrawPlayingBoard;

			gameState = BoogieState.timeIn;
			timeInTimer = 1;
			timeIn = 2;

			Shuffle();
		}
	}

	private void Shuffle()
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
		ResetTimer();
	}

	private void UpdateScores()
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
			currentWord = aiBoardWords.ElementAtOrDefault(aiWordIndex);

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
				Update -= UpdateTimeIn;
				Draw -= DrawTimeIn;

				Update += UpdateGame;
				Draw += DrawPlayingBoard;

				gameState = BoogieState.playing;
			}
		}
	}

	private void DrawArrows(SpriteBatch spriteBatch, string word)
	{
		int diceSize = Dice.ImageSize;
		Vector2 offset = new(diceSize / 2, diceSize / 2);
		List<Vector2> currentPath = solver.Paths[word];
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
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				blankDie.Draw(i, j, Color.White);
			}
		}
		spriteBatch.DrawString(hugeFont, "Get Ready", new(400, 200), Color.White);
	}

	private void DrawLetterBoard(SpriteBatch spriteBatch)
	{
		string word = aiBoardWords.ElementAtOrDefault(aiWordIndex);
		bool shouldHighlight = word != null && gameState == BoogieState.scoreNegotiation;

		Color color = gameState == BoogieState.scoreNegotiation ? Color.LightSlateGray : Color.White;
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j]?.Draw(i, j, color);
			}
		}

		if (!shouldHighlight) return;

		List<Vector2> currentWordPath = solver.Paths[word];
		int total = currentWordPath.Count;
		int shineIndex = total - glintLetterIndex - 1; // 0-based from front

		for (int i = total - 1; i >= 0; i--)
		{
			Color c = glintHold < 0 && i != shineIndex ? Color.LightGray : Color.White;
			int ipx = (int)currentWordPath[i].X;
			int ipy = (int)currentWordPath[i].Y;
			board[ipx, ipy].Draw(ipx, ipy, c);
		}

		DrawArrows(spriteBatch, word);
	}

	public void DrawPlayingBoard(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(sandTexture, sandTimer, Color.Gray);
		spriteBatch.Draw(sandTexture, sand, Color.Yellow);
		shuffleDie.Draw(0, height, Color.White);

		DrawLetterBoard(spriteBatch);

		if (currentWord != null)
		{
			Vector2 position = new(5, gameWindow.ClientBounds.Height - 56);
			spriteBatch.DrawString(largeFont, currentWord, position, Color.Black);
		}


		Vector2 scorePosition = new(sandTimer.Right + 10, Dice.ImageSize * height);
		spriteBatch.DrawString(smallFont, $"Score: {score}", scorePosition, Color.Black);

		Rectangle aiScoreRect = Dice.GetDrawPosition(0, height);
		Vector2 aiScorePosition = new(5, aiScoreRect.Bottom);
		spriteBatch.DrawString(smallFont, $"AI Score: {aiScore}", aiScorePosition, Color.Black);


		int ind = 0;
		const int rightX = width * 80 + 20;
		const int columnCapacity = 16;
		const int textHeight = 20;

		foreach (string entry in wordList)
		{
			Color wordColor = Color.Black;
			if (currentWord == entry) wordColor = Color.Crimson;
			Vector2 pos = new(
				rightX + (ind / columnCapacity * 150),
				ind % columnCapacity * textHeight
			);
			if (gameState == BoogieState.scoreNegotiation && aiBoardWords.Contains(entry))
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


}
