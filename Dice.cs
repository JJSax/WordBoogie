using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Boggle;

public class Dice
{
	const int squareImgDiceSize = 80;
	static readonly char[] letters = ['A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z'];
	static readonly int[] letter_weights = [78,20,40,38,110,14,30,23,86,2,10,53,27,72,61,28,2,73,87,67,33,10,9,3,16,4];
	static readonly int weightSum = letter_weights.Sum();
	//todo letter score values
	static Texture2D diceTexture;
	private static Rectangle[] letter_rects = new Rectangle[26];
	private static Rectangle shuffleRect = new(160, 80, 80, 80);
	static SpriteBatch spriteBatch;

	private Rectangle quad;

	private int letterIndex;

	public static void Init(SpriteBatch sprite_batch, Texture2D dice_texture) {
		spriteBatch = sprite_batch;
		diceTexture = dice_texture;

		letter_rects = new Rectangle[letters.Length];
		for (int i = 0; i < letters.Length; i++)
		{
			letter_rects[i] = new Rectangle(i * 80, 0, 80, 80);
		}
	}

	public Dice() => ChooseLetter();

	public void ChooseLetter()
	{
		letterIndex = MathExt.WeightedRandomIndex(letters, letter_weights, weightSum);
		quad = letter_rects[letterIndex];
	}

	public void Draw(int x, int y)
	{
		spriteBatch.Draw(diceTexture, new Vector2(x * squareImgDiceSize, y * squareImgDiceSize), quad, Color.White);
	}



	public Rectangle getDrawPosition(int x, int y)
	{
		return new Rectangle(x * squareImgDiceSize, y * squareImgDiceSize, squareImgDiceSize, squareImgDiceSize);
	}

	public void MakeShuffleDice() => quad = shuffleRect;
	public int GetLetterIndex => letterIndex;
	public char GetLetter => letters[letterIndex];
	public static int GetImageSize => squareImgDiceSize;
	public Rectangle GetQuad => quad;
}
