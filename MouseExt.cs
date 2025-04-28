using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WordBoogie;

public class MouseExt
{

	private static readonly MouseExt _instance = new();

	private MouseState state = new();
	private MouseState _previousState = new();

	public MouseState CurrentState => state;
	public MouseState PreviousState => _previousState;

	public delegate void MouseDownEvent(Point position);
	public static event MouseDownEvent LeftMousePressed;
	public static event MouseDownEvent RightMousePressed;

	private MouseExt() {}
	public static MouseExt Instance { get{ return _instance; } }
	public MouseState State => state;

	public void Update()
	{
		state = Mouse.GetState();
		if (state.LeftButton == ButtonState.Pressed && _previousState.LeftButton == ButtonState.Released)
			LeftMousePressed?.Invoke(state.Position);

		if (state.RightButton == ButtonState.Pressed && _previousState.RightButton == ButtonState.Released)
			RightMousePressed?.Invoke(state.Position);

		_previousState = state;
	}
}
