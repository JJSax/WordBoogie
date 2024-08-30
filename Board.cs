using System;
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
	private readonly Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private TimeSpan sandRemaining;

	public Board(Texture2D timerTexture)
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

		sandTexture = timerTexture;
		Rectangle drawPos = shuffleDie.getDrawPosition(0, height);
		sandTimer = new Rectangle(drawPos.X + 100, drawPos.Y, 40, 80);
		sandRemaining = TimeSpan.FromMinutes(3);
		UpdateSand();

		MouseExt.LeftMousePressed += AttemptShuffle;
	}

	public void AttemptShuffle(Point position)
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
	}
}
