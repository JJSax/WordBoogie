using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordBoogie.Scenes;

public abstract class Scene
{
	private bool _loaded;
	public bool IsLoaded => _loaded;
	public bool ForceLoad = false;

	public virtual void LoadContent()
	{
		_loaded = true;
	}
	public virtual void UnloadContent()
	{
		_loaded = false;
	}

	public virtual void Enter() { }
	public virtual void Enter(Scene from) { }
	public virtual void Exit() { }

	public abstract void Update(GameTime gameTime);
	public abstract void Draw(SpriteBatch spriteBatch);

	// Optional: overlay-specific draw pass
	public virtual void DrawOverlay(SpriteBatch spriteBatch) { }

	// For handling blur/background effects
	public virtual bool ShouldDrawUnderneath => false;
}
