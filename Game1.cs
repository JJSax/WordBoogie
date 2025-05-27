using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace WordBoogie;

public class Game1 : Game
{
	private GraphicsDeviceManager _graphics;
	private SpriteBatch _spriteBatch;

	Board board;
	MouseExt mouseExt = MouseExt.Instance;

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

		Texture2D dice = Content.Load<Texture2D>("Dice");
		Dice.Init(_spriteBatch, dice);

		SpriteFont largeFont = Content.Load<SpriteFont>("RobotoMono-Medium-large");
		SpriteFont smallFont = Content.Load<SpriteFont>("RobotoMono-Medium-small");
		Texture2D primitiveTexture = new(GraphicsDevice, 1, 1);
		primitiveTexture.SetData([Color.White]);

		board = new Board(Window, dice);
		board.AddContent(primitiveTexture, largeFont, smallFont);
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