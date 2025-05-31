using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace WordBoogie;

public class Dice
{
	private const int squareImgDiceSize = 80;

	private static readonly char[] alphabet = ['A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'];
	private static readonly int[] letter_weights = [78, 20, 40, 38, 110, 14, 30, 23, 86, 2, 10, 53, 27, 72, 61, 28, 2, 73, 87, 67, 33, 10, 9, 3, 16, 4];
	private static readonly int weightSum = letter_weights.Sum();
	//todo letter score values
	private static Texture2D diceTexture;
	private static Rectangle[] letter_rects = new Rectangle[26];
	private static Rectangle shuffleRect = new(ImageSize * 2, ImageSize, ImageSize, ImageSize);
	private static Rectangle blankRect = new(ImageSize, ImageSize, ImageSize, ImageSize);
	private static SpriteBatch spriteBatch;

	private Rectangle quad;

	public int LetterIndex { get; private set; }

	public static void Init(SpriteBatch sprite_batch, Texture2D dice_texture)
	{
		spriteBatch = sprite_batch;
		diceTexture = dice_texture;

		letter_rects = new Rectangle[alphabet.Length];
		for (int i = 0; i < alphabet.Length; i++)
		{
			letter_rects[i] = new Rectangle(i * ImageSize, 0, ImageSize, ImageSize);
		}
	}

	public Dice() => ChooseLetter();

	public void ChooseLetter()
	{
		LetterIndex = MathExt.WeightedRandomIndex(alphabet, letter_weights, weightSum);
		quad = letter_rects[LetterIndex];
	}

	public void Draw(int x, int y, Color color)
	{
		spriteBatch.Draw(diceTexture, new Vector2(x * ImageSize, y * ImageSize), quad, color);
	}

	public static Rectangle GetDrawPosition(int x, int y)
	{
		return new Rectangle(x * ImageSize, y * ImageSize, ImageSize, ImageSize);
	}

	public void MakeShuffleDice() => quad = shuffleRect;
	public void MakeBlankDice() => quad = blankRect;
	public char GetLetter => alphabet[LetterIndex];
	public static int ImageSize => squareImgDiceSize;
	public Rectangle GetQuad => quad;
}
