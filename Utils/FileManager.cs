using System;
using System.IO;
using System.Collections.Generic;

namespace WordBoogie;

public class FileManager
{
	private static readonly string filePath = Path.Combine(
		Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
		"Word_Boogie",
		"words.txt"
	);

	// Ensure the directory exists
	static FileManager()
	{
		Directory.CreateDirectory(Path.GetDirectoryName(filePath));
	}

	public static void SaveWords(List<string> words, bool append)
	{
		using StreamWriter writer = new(filePath, append); // true to append
		foreach (var word in words)
		{
			writer.WriteLine(word);
		}
	}

	public static void SaveWord(string word, bool append)
	{
		using StreamWriter writer = new(filePath, append); // true to append
		writer.WriteLine(word);
	}

	public static void WriteWords(List<string> words) => SaveWords(words, false);
	public static void AppendWords(List<string> words) => SaveWords(words, true);
	public static void AppendWord(string word) => SaveWord(word, true);
	public static bool Exists() => File.Exists(filePath);

	// Read all words from the file
	public static List<string> ReadWords()
	{
		List<string> words = [];

		if (File.Exists(filePath))
		{
			using StreamReader reader = new(filePath);
			string line;
			while ((line = reader.ReadLine()) != null)
			{
				words.Add(line);
			}
		}

		return words;
	}
}
