using System;
using System.Collections.Generic;
using System.Linq;

namespace WordBoogie;

public static class MathExt
{

	private static readonly Random random = new();

	/// <summary>
	/// I got this method from
	/// https://softwareengineering.stackexchange.com/questions/150616/get-weighted-random-item
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="enumerable">The list of items to pick from</param>
	/// <param name="weightFunc">A function to determine weight.</param>
	/// <returns></returns>
	public static int WeightedRandomIndex<T>(this IEnumerable<T> enumerable, Func<T, int> weightFunc)
	{
		int totalWeight = 0; // this stores sum of weights of all elements before current
		int selected = -1; // currently selected element
		int i = 0;
		foreach (var data in enumerable)
		{
			int weight = weightFunc(data); // weight of current element
			int r = random.Next(totalWeight + weight); // random value
			if (r >= totalWeight) // probability of this is weight/(totalWeight+weight)
				selected = i; // it is the probability of discarding last selected element and selecting current one instead
			totalWeight += weight; // increase weight sum
			i++;
		}

		return selected; // when iterations end, selected is some element of sequence.
	}

	public static int WeightedRandomIndex<T>(this IEnumerable<T> enumerable, Func<T, int> weightFunc, int totalWeight)
	{
		int r = random.Next(totalWeight);
		int i = 0;
		foreach (var data in enumerable)
		{
			int weight = weightFunc(data);
			r -= weight;
			if (r < 0) return i;
			i++;
		}

		throw new InvalidOperationException("No valid items to select."); // This should theoretically never happen if weights are correct
	}

	public static int WeightedRandomIndex<T>(this IEnumerable<T> enumerable, int[] weights, int totalWeight)
	{
		int r = random.Next(totalWeight);
		int i = 0;
		foreach (var _ in enumerable)
		{
			int weight = weights.ElementAt(i);
			r -= weight;
			if (r < 0) return i;
			i++;
		}

		throw new InvalidOperationException("No valid items to select."); // This should theoretically never happen if weights are correct
	}

	/// <summary>
	/// I got this method from
	/// https://softwareengineering.stackexchange.com/questions/150616/get-weighted-random-item
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="enumerable">The list of items to pick from</param>
	/// <param name="weightFunc">A function to determine weight.</param>
	/// <returns></returns>
	public static T WeightedRandom<T>(this IEnumerable<T> enumerable, Func<T, int> weightFunc)
	{
		int totalWeight = 0; // this stores sum of weights of all elements before current
		T selected = default(T); // currently selected element
		foreach (var data in enumerable)
		{
			int weight = weightFunc(data); // weight of current element
			int r = random.Next(totalWeight + weight); // random value
			if (r >= totalWeight) // probability of this is weight/(totalWeight+weight)
				selected = data; // it is the probability of discarding last selected element and selecting current one instead
			totalWeight += weight; // increase weight sum
		}

		return selected; // when iterations end, selected is some element of sequence.
	}

	public static T WeightedRandom<T>(this IEnumerable<T> enumerable, Func<T, int> weightFunc, int totalWeight)
	{
		int r = random.Next(totalWeight);
		foreach (var data in enumerable)
		{
			int weight = weightFunc(data);
			r -= weight;
			if (r < 0) return data;
		}

		throw new InvalidOperationException("No valid items to select."); // This should theoretically never happen if weights are correct
	}

	public static T WeightedRandom<T>(this IEnumerable<T> enumerable, int[] weights, int totalWeight)
	{
		int r = random.Next(totalWeight);
		int i = 0;
		foreach (var data in enumerable)
		{
			int weight = weights.ElementAt(i);
			r -= weight;
			if (r < 0) return data;
			i++;
		}

		throw new InvalidOperationException("No valid items to select."); // This should theoretically never happen if weights are correct
	}

}
