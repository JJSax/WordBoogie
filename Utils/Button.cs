using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordBoogie.Utils;

public class Button(Texture2D texture, Rectangle sourcePosition, Rectangle where)
{
	readonly Texture2D texture = texture;
	Rectangle source = sourcePosition;
	Rectangle where = where;

	public bool Contains(Point position)
	{
		return where.Contains(position);
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		spriteBatch.Draw(texture, where, source, Color.White);
	}
}
