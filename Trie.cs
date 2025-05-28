using System.Collections.Generic;

namespace WordBoogie;

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
		TrieNode node = root;
		foreach (char ch in word)
		{
			if (!node.Children.TryGetValue(ch, out TrieNode value))
			{
				value = new TrieNode();
				node.Children[ch] = value;
			}
			node = value;
		}
		node.IsEndOfWord = true;
	}

	public bool Search(string word)
	{
		TrieNode node = root;
		foreach (char ch in word)
		{
			if (!node.Children.TryGetValue(ch, out TrieNode value))
			{
				return false;
			}
			node = value;
		}
		return node.IsEndOfWord;
	}
}
