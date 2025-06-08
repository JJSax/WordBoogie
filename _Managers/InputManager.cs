using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace WordBoogie._Managers;

public class InputManager
{
	private static KeyboardState _currentKeyboard;
	private static KeyboardState _lastKeyboard;
	public static KeyboardState CurrentKeyboard => _currentKeyboard;
	// Keyboard Methods
	public static bool KeyPressed(Keys key) => _currentKeyboard.IsKeyDown(key) && _lastKeyboard.IsKeyUp(key);
	public static bool KeyDown(Keys key) => _currentKeyboard.IsKeyDown(key);




	private static MouseState _currentMouseState;
	private static MouseState _lastMouseState;
	public static MouseState CurrentMouse => _currentMouseState;
	// Mouse Methods
	/// <summary>
	/// Method to see if the wheel was scrolled up in this frame.
	/// </summary>
	/// <returns></returns>
	public static bool WheelUp => _currentMouseState.ScrollWheelValue > _lastMouseState.ScrollWheelValue;
	/// <summary>
	/// Method to see if the wheel was scrolled down in this frame.
	/// </summary>
	/// <returns></returns>
	public static bool WheelDown => _currentMouseState.ScrollWheelValue < _lastMouseState.ScrollWheelValue;

	public static bool LeftMousePressed => _lastMouseState.LeftButton == ButtonState.Released && _currentMouseState.LeftButton == ButtonState.Pressed;
	public static bool RightMousePressed => _lastMouseState.RightButton == ButtonState.Released && _currentMouseState.RightButton == ButtonState.Pressed;
	public delegate void MouseDownEvent(Point position);
	public static event MouseDownEvent OnLeftMousePressed;
	public static event MouseDownEvent OnRightMousePressed;

	public static bool LeftMouseDown => _currentMouseState.LeftButton == ButtonState.Pressed;
	public static bool RightMouseDown => _currentMouseState.RightButton == ButtonState.Pressed;

	public static Point MousePosition => _currentMouseState.Position;

	public static void Update()
	{
		_lastKeyboard = _currentKeyboard;
		_currentKeyboard = Keyboard.GetState();

		_lastMouseState = _currentMouseState;
		_currentMouseState = Mouse.GetState();

		if (LeftMousePressed) OnLeftMousePressed?.Invoke(MousePosition);
		if (RightMousePressed) OnRightMousePressed?.Invoke(MousePosition);
	}
}
