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
	BoogieScene boogieScene;

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

	public override void Enter() { }

	public override void Exit() { }

	public override void Update(GameTime gameTime)
	{
		if (InputManager.LeftMousePressed)
		{
			if (Settings.Contains(InputManager.MousePosition))
			{
				Debug.WriteLine("OPEN SETTINGS");
			}

			if (Start.Contains(InputManager.MousePosition))
			{
				boogieScene ??= new BoogieScene();
				SceneManager.Push(boogieScene);
			}
		}
	}

	public override void Draw(SpriteBatch spriteBatch)
	{
		Start.Draw(spriteBatch);
		Settings.Draw(spriteBatch);
	}
}
