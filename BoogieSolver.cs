using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace WordBoogie;

public class BoogieSolver
{
	public Trie DictTrie { get; private set; }
	public Trie ConfirmedTrie { get; private set; }
	public HashSet<string> FoundWords { get; private set; }
	public Dictionary<string, List<Vector2>> Paths { get; private set; }
	private readonly int[,] directions = new int[,] { { -1, -1 }, { -1, 0 }, { -1, 1 }, { 0, -1 }, { 0, 1 }, { 1, -1 }, { 1, 0 }, { 1, 1 } };

	public BoogieSolver(IEnumerable<string> dictionary, IEnumerable<string> confirmedWords)
	{
		DictTrie = new Trie();
		Debug.WriteLine("Writing {0} lines to the dictionary Trie.", dictionary.Count());
		foreach (string word in dictionary)
		{
			DictTrie.Insert(word.ToUpper());
		}

		ConfirmedTrie = new Trie();
		foreach (string word in confirmedWords)
		{
			ConfirmedTrie.Insert(word.ToUpper());
		}
		Debug.WriteLine("Writing {0} lines to the confirmed Trie.", confirmedWords.Count());

		Paths = [];
	}

	private static bool SearchTrie(Trie trie, string word) => trie.Search(word);
	public bool WordInDictionary(string word) => SearchTrie(DictTrie, word);
	public bool WordInConfirmed(string word) => SearchTrie(ConfirmedTrie, word);

	private HashSet<string> FindWords(Dice[,] board, Trie trie)
	{
		FoundWords = [];
		int rows = board.GetLength(0);
		int cols = board.GetLength(1);
		bool[,] visited = new bool[rows, cols];
		Stack<Vector2> currentPath = new();

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				DFS(board, visited, i, j, "", trie.GetRoot, currentPath);
			}
		}

		return FoundWords;
	}

	public HashSet<string> FindAllWords(Dice[,] board)
	{
		return FindWords(board, DictTrie);
	}

	public HashSet<string> FindConfirmedWords(Dice[,] board)
	{
		return FindWords(board, ConfirmedTrie);
	}

	private void DFS(Dice[,] board, bool[,] visited, int row, int col, string currentWord, TrieNode node, Stack<Vector2> currentPath)
	{
		if (row < 0 || col < 0 || row >= board.GetLength(0) || col >= board.GetLength(1) || visited[row, col])
			return;

		TrieNode value;
		char letter = board[row, col].GetLetter;
		if (letter == 'Q')
		{
			if (!node.Children.TryGetValue('Q', out TrieNode qNode)) return;
			if (!qNode.Children.TryGetValue('U', out value)) return;
			currentWord += "QU";
			node = value; // Point node to the U node, not Q
		}
		else
		{
			if (!node.Children.TryGetValue(letter, out value)) return;
			currentWord += letter;
			node = value;
		}

		visited[row, col] = true;
		currentPath.Push(new Vector2(row, col));

		if (node.IsEndOfWord)
		{
			FoundWords.Add(currentWord);
			Paths[currentWord] = currentPath.ToList();
		}

		for (int i = 0; i < directions.GetLength(0); i++)
		{
			int newRow = row + directions[i, 0];
			int newCol = col + directions[i, 1];
			DFS(board, visited, newRow, newCol, currentWord, node, currentPath);
		}

		currentPath.Pop();
		visited[row, col] = false;
	}
}