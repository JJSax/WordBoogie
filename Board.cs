using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Boggle;

public class Board
{

	const int width = 4;
	const int height = 4;

	Dice[,] board;
	readonly Dice shuffleDie;
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;
	private static GameWindow gameWindow;
	private List<string> wordList = [];
	private string currentWord = "";
	private SpriteFont largeFont;
	private SpriteFont smallFont;

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
		// Get the pressed key
		Keys key = args.Key;
		KeyboardState keys = Keyboard.GetState();

		if (key == Keys.Enter && currentWord.Length > 0)
		{
			wordList.Add(currentWord);
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
		Debug.WriteLine($"Current Word: {currentWord}");
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

		Vector2 position = new Vector2(5, gameWindow.ClientBounds.Height - 56);
		spriteBatch.DrawString(largeFont, currentWord, position, Color.Black);

		int ind = 0;
		int leftX = 340;
		int columnCapacity = 20;
		int textHeight = 20;
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
