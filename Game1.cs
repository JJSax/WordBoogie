using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using WordBoogie.Scenes;

namespace WordBoogie;

public class Game1 : Game
{
	private readonly GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

	BoogieScene board;
	readonly MouseExt mouseExt = MouseExt.Instance;

	public Game1()
	{
		_graphics = new GraphicsDeviceManager(this);
		Content.RootDirectory = "Content";
		IsMouseVisible = true;
	}

	protected override void Initialize()
	{
		base.Initialize();
	}

	protected override void LoadContent()
	{
		_spriteBatch = new SpriteBatch(GraphicsDevice);

		Globals.Initialize(Content, GraphicsDevice, _spriteBatch, Window);

		Texture2D dice = Content.Load<Texture2D>("Dice");
		Dice.Init(dice);

		board = new BoogieScene();
		board.Initialize();
		board.LoadContent();
		board.Enter(null);
	}

	protected override void Update(GameTime gameTime)
	{
		if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
			Exit();

		mouseExt.Update();
		board.Update(gameTime);

		base.Update(gameTime);
	}

	protected override void Draw(GameTime gameTime)
	{
		GraphicsDevice.Clear(Color.CornflowerBlue);

		_spriteBatch.Begin();
		board.Draw(_spriteBatch);
		_spriteBatch.End();

		base.Draw(gameTime);
	}
}