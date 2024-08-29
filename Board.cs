using System;
using System.Data;
using Microsoft.Xna.Framework.Graphics;

namespace Boggle;

public class Board
{

	const int width = 4;
	const int height = 4;

	Dice[,] board;

	public Board()
	{
		board = new Dice[width, height];
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j] = new Dice();
			}
		}
	}

	public void Draw(SpriteBatch spriteBatch)
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j]?.Draw(i, j);
			}
		}
	}
}
