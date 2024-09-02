using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Boggle;

public class BoggleSolver
{
	private readonly Trie trie;
	private HashSet<string> foundWords;
	private readonly int[,] directions = new int[,] { {-1, -1}, {-1, 0}, {-1, 1}, {0, -1}, {0, 1}, {1, -1}, {1, 0}, {1, 1} };

	public BoggleSolver(IEnumerable<string> dictionary)
	{
		trie = new Trie();
		Debug.WriteLine("Writing {0} lines to the Trie.", dictionary.Count());
		foreach (var word in dictionary)
		{
			trie.Insert(word.ToUpper());
		}
	}

	public HashSet<string> FindWords(Dice[,] board)
	{
		foundWords = new HashSet<string>();
		int rows = board.GetLength(0);
		int cols = board.GetLength(1);
		bool[,] visited = new bool[rows, cols];

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				DFS(board, visited, i, j, "", trie.GetRoot);
			}
		}

		return foundWords;
	}

	private void DFS(Dice[,] board, bool[,] visited, int row, int col, string currentWord, TrieNode node)
	{
		if (row < 0 || col < 0 || row >= board.GetLength(0) || col >= board.GetLength(1) || visited[row, col])
			return;

		char letter = board[row, col].GetLetter;
		if (!node.Children.ContainsKey(letter))
		{
			return;
		}

		visited[row, col] = true;
		currentWord += letter;
		node = node.Children[letter];

		if (node.IsEndOfWord)
		{
			foundWords.Add(currentWord);
		}

		for (int i = 0; i < directions.GetLength(0); i++)
		{
			int newRow = row + directions[i, 0];
			int newCol = col + directions[i, 1];
			DFS(board, visited, newRow, newCol, currentWord, node);
		}

		visited[row, col] = false;
	}
}