using System.Collections.Generic;

namespace Boggle;

public class TrieNode
{
	public Dictionary<char, TrieNode> Children = [];
	public bool IsEndOfWord = false;
}

public class Trie
{
	private readonly TrieNode root;
	public TrieNode GetRoot => root;

	public Trie()
	{
		root = new TrieNode();
	}

	public void Insert(string word)
	{
		var node = root;
		foreach (var ch in word)
		{
			if (!node.Children.ContainsKey(ch))
			{
				node.Children[ch] = new TrieNode();
			}
			node = node.Children[ch];
		}
		node.IsEndOfWord = true;
	}

	public bool Search(string word)
	{
		var node = root;
		foreach (var ch in word)
		{
			if (!node.Children.ContainsKey(ch))
			{
				return false;
			}
			node = node.Children[ch];
		}
		return node.IsEndOfWord;
	}
}
