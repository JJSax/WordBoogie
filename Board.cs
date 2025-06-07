using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WordBoogie;

public class Board
{
	public const int width = 4;
	public const int height = 4;

	public readonly Dice[,] board;

	public BoogieSolver Solver { get; private set; }

	/// <summary>
	/// The list of words in this game that the player found.
	/// </summary>
	public List<string> WordList { get; set; } = [];

	/// <summary>
	/// All the words that are either confirmed as real, or use has used and accepted to be real.
	/// </summary>
	public HashSet<string> UserWordList { get; private set; }
	/// <summary>
	/// All the words contained on this particular board.
	/// </summary>
	public HashSet<string> AllBoardWords { get; private set; }
	/// <summary>
	/// The list of words the ai found
	/// </summary>
	public List<string> AIBoardWords { get; private set; }
	readonly Random random = new();
	private static readonly HashSet<char> rareLetters = ['Q', 'V', 'W', 'X', 'Y', 'Z'];

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

		// Get paths to binary data folder
		string dictionaryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "words_alpha_jj.txt");
		string realWordPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "confirmed_words.txt");

		UserWordList = new(FileManager.ReadWords().Concat(File.ReadLines(realWordPath)));

		WordList = [];
		AIBoardWords = [];
		AllBoardWords = [];

		Solver = new(File.ReadLines(dictionaryPath), UserWordList);
		FindAllWords();
	}

	public void FindAllWords() => AllBoardWords = Solver.FindAllWords(board);

	public int SetAIWords(int[] odds, int[] wordLengthScores)
	{
		int score = 0;

		foreach (string word in AllBoardWords)
		{
			if (Solver.ConfirmedTrie.Search(word))
			{
				int choice = random.Next(odds[word.Length]);
				if (choice == 0)
				{
					AIBoardWords.Add(word);
					score += wordLengthScores[word.Length];
				}
			}
		}

		return score;
	}

	public void Shuffle()
	{
		for (int i = 0; i < width; i++)
		{
			for (int j = 0; j < height; j++)
			{
				board[i, j].ChooseLetter();
			}
		}

		WordList.Clear();
		FindAllWords();
	}

	public bool IsNewWord(string word)
	{
		if (!Solver.WordInConfirmed(word))
		{
			FileManager.AppendWord(word);
			UserWordList.Add(word);
			return true;
		}
		return false;
	}

	public bool EnterUserWord(string word, out bool isNew)
	{
		isNew = false;
		if (AllBoardWords.Contains(word) && !WordList.Contains(word))
		{
			if (!Solver.WordInConfirmed(word))
			{
				FileManager.AppendWord(word);
				UserWordList.Add(word);
				isNew = true;
			}

			WordList.Add(word);

			return true;
		}

		return false;
	}

	public static bool RareLetterUsed(string word)
	{
		foreach (char c in word)
		{
			if (rareLetters.Contains(c)) return true;
		}
		return false;
	}
}
