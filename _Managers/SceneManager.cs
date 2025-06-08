using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WordBoogie.Scenes;

namespace WordBoogie._Managers;

public static class SceneManager
{
	private readonly static Stack<Scene> _sceneStack = new();
	public static Scene Current => _sceneStack.Peek();

	public static void Push(Scene scene)
	{
		if (!scene.IsLoaded)
			scene.LoadContent();

		scene.Enter(_sceneStack.Peek());
		_sceneStack.Push(scene);
	}

	public static void Pop()
	{
		if (_sceneStack.Count <= 0)
			throw new InvalidOperationException("Attempt to pop from an empty stack");

		var scene = _sceneStack.Pop();

		scene.Exit();
		if (scene.IsLoaded)
			scene.UnloadContent();
	}

	public static void Switch(Scene scene)
	{
		_sceneStack.TryPeek(out Scene last);
		if (last != null)
		{
			last.Exit();
			last.UnloadContent();
		}

		_sceneStack.Clear();

		if (!scene.IsLoaded)
			scene.LoadContent();

		scene.Enter(null);
		_sceneStack.Push(scene);
	}

	public static void Update(GameTime gameTime)
	{
		if (_sceneStack.Count > 0)
			_sceneStack.Peek().Update(gameTime);
	}

	public static void Draw(SpriteBatch spriteBatch)
	{
		var scenesToDraw = _sceneStack.Reverse().ToList();
		bool drawUnder = true;

		foreach (var scene in scenesToDraw)
		{
			if (drawUnder || scene.ShouldDrawUnderneath)
			{
				scene.Draw(spriteBatch);
			}

			if (!scene.ShouldDrawUnderneath)
				drawUnder = false;
		}

		// Optionally: handle overlay passes separately
		foreach (var scene in scenesToDraw)
		{
			scene.DrawOverlay(spriteBatch);
		}
	}
}
