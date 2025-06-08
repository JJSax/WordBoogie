using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WordBoogie._Managers;
using WordBoogie.Utils;

namespace WordBoogie.Scenes;

public class MainMenuScene : Scene
{

	TextButton Start;
	ImageButton Settings;

	public MainMenuScene()
	{
		Point c = Globals.CenterWindow;
		int StartWidth = 450;
		Start = new(
			Globals.Content.Load<SpriteFont>("Arial"),
			"Start Game",
			new(c.X - StartWidth / 2, c.Y - 100, StartWidth, 70)
		);

		Point ws = Globals.WindowSize;
		Settings = new(
			Globals.Content.Load<Texture2D>("UI"),
			new(50, 0, 80, 80),
			new(ws.X - 80, ws.Y - 80, 60, 60)
		);

	}

	public override void Enter()
	{
		InputManager.OnLeftMousePressed += AttemptStart;
		InputManager.OnLeftMousePressed += AttemptSettings;
	}

	public override void Exit()
	{
		InputManager.OnLeftMousePressed -= AttemptStart;
		InputManager.OnLeftMousePressed -= AttemptSettings;
	}

	private void AttemptStart(Point position)
	{
		if (Start.Contains(position))
		{
			SceneManager.Push(new BoogieScene());
		}
	}

	private void AttemptSettings(Point position)
	{
		if (Settings.Contains(position))
		{
			Debug.WriteLine("OPEN SETTINGS");
			// SceneManager.Push(Settings)
		}
	}

	public override void Update(GameTime gameTime)
	{
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Start.Draw(spriteBatch);
		Settings.Draw(spriteBatch);
	}
}
