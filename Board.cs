using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
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
	private MouseState _currentState = new MouseState();
	private MouseState _previousState = new MouseState();
	private Texture2D sandTexture;
	private Rectangle sandTimer;
	private Rectangle sand;
	private System.TimeSpan sandRemaining;

	public MouseState CurrentState => _currentState;
	public MouseState PreviousState => _previousState;
	// public bool MousePressed => _currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released;

	private delegate void MouseDownEvent();
	private static event MouseDownEvent MousePressed;

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
		sand = new Rectangle(drawPos.X + 100, drawPos.Y, 40, 80);
		sandRemaining = System.TimeSpan.FromMinutes(3);

		_currentState = new MouseState();
		_previousState = new MouseState();

		MousePressed += mousePressed;
	}

	public void mousePressed()
	{
		Debug.WriteLine(shuffleDie.GetQuad);
		Debug.WriteLine(_currentState.Position);
		if (shuffleDie.getDrawPosition(0, height).Contains(_currentState.Position))
		{
			Shuffle();
		}
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
		sandRemaining = System.TimeSpan.FromMinutes(3);
	}

	public void Update(GameTime gameTime)
	{
		sandRemaining -= gameTime.ElapsedGameTime;
		System.TimeSpan min3 = System.TimeSpan.FromMinutes(3);
		int topOffset = (int)((min3 - sandRemaining) / min3 * sandTimer.Height);
		// Rectangle oldSand = sand;
		sand = new Rectangle(sandTimer.X, sandTimer.Y + topOffset, sandTimer.Width, sandTimer.Height - topOffset);


		_currentState = Mouse.GetState();
		if (_currentState.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released)
		{
			// && shuffleDie.GetQuad.Contains(_currentState.Position)
			MousePressed?.Invoke();
		}
		_previousState = _currentState;
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
