using System;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Net.Quic;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Boggle;

public class Dice
{
	const int squareImgDiceSize = 80;
    static readonly string[] letters = ["A","B","C","D","E","F","G","H","I","J","K","L","M","N","O","P","Qu","R","S","T","U","V","W","X","Y","Z"];
	static readonly int[] letter_weights = [132,24,44,68,276,36,32,98,112,2,12,64,38,107,120,30,2,96,101,144,45,16,38,2,30,1];
	static readonly int weightSum = letter_weights.Sum();
	//todo letter score values
	static Texture2D diceTexture;
	private static Rectangle[] letter_rects = new Rectangle[26];
	private static Rectangle shuffleRect = new Rectangle(160, 80, 80, 80);
	static readonly Random rand = new();
	static SpriteBatch spriteBatch;

	private Rectangle quad;

	int letterIndex;

	public static void Init(SpriteBatch sprite_batch, Texture2D dice_texture) {
		spriteBatch = sprite_batch;
		diceTexture = dice_texture;

		letter_rects = new Rectangle[letters.Length];
		for (int i = 0; i < letters.Length - 1; i++)
		{
			letter_rects[i] = new Rectangle(i * 80, 0, 80, 80);
		}
	}

	public Dice()
	{
		ChooseLetter();
	}

	public void ChooseLetter()
	{
		letterIndex = MathExt.WeightedRandomIndex(letters, letter_weights, weightSum);
		quad = letter_rects[letterIndex];
	}

	public void Draw(int x, int y)
	{
		spriteBatch.Draw(diceTexture, new Vector2(x * squareImgDiceSize, y * squareImgDiceSize), quad, Color.White);
	}


	public void MakeShuffleDice()
	{
		quad = shuffleRect;
	}

	public Rectangle getDrawPosition(int x, int y)
	{
		return new Rectangle(x * squareImgDiceSize, y * squareImgDiceSize, squareImgDiceSize, squareImgDiceSize);
	}

	public int GetImageSize => squareImgDiceSize;
	public Rectangle GetQuad => quad;
}
