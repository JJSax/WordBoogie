using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace WordBoogie.Utils;

public class Button(Rectangle where)
{
	public Rectangle Where { get; } = where;

	public virtual bool Contains(Point position)
	{
		return Where.Contains(position);
	}
}

public class ImageButton(Texture2D texture, Rectangle sourcePosition, Rectangle where) : Button(where)
{
	private readonly Texture2D _texture = texture;
	private readonly Rectangle _source = sourcePosition;

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(_texture, Where, _source, Color.White);
	}
}

public class TextButton(SpriteFont font, string text, Rectangle where) : Button(where)
{
	private readonly SpriteFont _font = font;
	private readonly string _text = text;

	public void Draw(SpriteBatch spriteBatch)
	{
		var textSize = _font.MeasureString(_text);
		var pos = new Vector2(
			Where.X + (Where.Width - textSize.X) / 2,
			Where.Y + (Where.Height - textSize.Y) / 2
		);

		spriteBatch.DrawRectangle(Where, Color.White);
		spriteBatch.DrawString(_font, _text, pos, Color.White);
	}
}
