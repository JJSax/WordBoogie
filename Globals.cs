using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace WordBoogie;

public static class Globals
{
	public static SpriteBatch SpriteBatch { get; set; }
	public static GraphicsDevice GraphicsDevice { get; set; }
	public static Point WindowSize { get; private set; } = new(2000, 1200);
	public static Point CenterWindow { get; private set; } = WindowSize / new Point(2, 2);
	public static GameWindow Window { get; private set; }

	private static ContentManager _content;
	public static ContentManager Content => _content ?? throw new InvalidOperationException("ContentManager not initialized");

	public static void Initialize(ContentManager content, GraphicsDevice graphicsDevice, SpriteBatch spriteBatch, GameWindow gameWindow)
	{
		_content = content;
		GraphicsDevice = graphicsDevice;
		SpriteBatch = spriteBatch;
		Window = gameWindow;
		UpdateWindowSize();
	}

	public static void UpdateWindowSize()
	{
		WindowSize = new(
			GraphicsDevice.Viewport.Width,
			GraphicsDevice.Viewport.Height
		);
		CenterWindow = WindowSize / new Point(2, 2);
	}

	public static RenderTarget2D GetNewRenderTarget()
	{
		return new(GraphicsDevice, WindowSize.X, WindowSize.Y);
	}

}
