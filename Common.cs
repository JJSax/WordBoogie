using System;
using System.Numerics;

namespace Boggle;

public class Common
{
	public static IEquatable<T> Wrap<T>(T n, T mn, T mx, out T o)
		where T : INumber<T>
	{
        T range = mx - mn;
        T mod = (n - mn) % range;

        if (mod < T.Zero)
        {
            mod += range;
        }

        o = mod + mn;
        return o;
	}
}
