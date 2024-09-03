using System;
using System.Numerics;

namespace Boggle;

public class Common
{
	public static IEquatable<T> Wrap<T>(T n, T mn, T mx, out T o)
		where T : INumber<T>
	{
		o = ((n - mn) % (mx - mn) ) + mn;
		return o;
	}
}
