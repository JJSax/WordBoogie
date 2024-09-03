using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Boggle;

public class BoggleSolver
{
	private readonly Trie trie;
	public HashSet<string> FoundWords {get; private set; }
	public Dictionary<string, Stack<Vector2>> Paths {get; private set; }
	private readonly int[,] directions = new int[,] { {-1, -1}, {-1, 0}, {-1, 1}, {0, -1}, {0, 1}, {1, -1}, {1, 0}, {1, 1} };

	public BoggleSolver(IEnumerable<string> dictionary)
	{
		trie = new Trie();
		Debug.WriteLine("Writing {0} lines to the Trie.", dictionary.Count());
		foreach (var word in dictionary)
		{
			trie.Insert(word.ToUpper());
		}
		Paths = [];
	}

	public HashSet<string> FindWords(Dice[,] board)
	{
		FoundWords = new HashSet<string>();
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

	private void DFS(Dice[,] board, bool[,] visited, int row, int col, string currentWord, TrieNode node, Stack<Vector2> currentPath)
	{
		if (row < 0 || col < 0 || row >= board.GetLength(0) || col >= board.GetLength(1) || visited[row, col])
			return;

		char letter = board[row, col].GetLetter;
		if (!node.Children.ContainsKey(letter))
			return;

		visited[row, col] = true;
		currentWord += letter;
		currentPath.Push(new Vector2(row, col));
		node = node.Children[letter];

		if (node.IsEndOfWord)
		{
			FoundWords.Add(currentWord);
			Paths[currentWord] = new Stack<Vector2>(currentPath);
			// Paths.Add(currentWord, new Stack<Vector2>(currentPath));
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