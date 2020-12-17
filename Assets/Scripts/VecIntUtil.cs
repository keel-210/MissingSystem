using UnityEngine;

public static class VecIntUtil
{
	static int temp;
	public static void Swap<T>(ref T i, ref T j)
	{
		T t = j;
		j = i;
		i = t;
	}
	public static int[] Sorted(int[] v)
	{
		if (v.Length == 3)
		{
			//バブルソートの理屈
			if (v[1] < v[0])
				Swap(ref v[0], ref v[1]);
			if (v[2] < v[1])
				Swap(ref v[1], ref v[2]);
			if (v[1] < v[0])
				Swap(ref v[0], ref v[1]);
		}
		else
			if (v[1] < v[0])
			Swap(ref v[0], ref v[1]);
		return v;
	}
	public static (int, int) SortedPoint(int i, int j)
	{
		if (j < i)
			Swap(ref i, ref j);
		return (i, j);
	}
	public static (int, int, int) SortedPoint(int i, int j, int k)
	{
		if (j < i)
			Swap(ref i, ref j);
		if (k < j)
			Swap(ref j, ref k);
		if (j < i)
			Swap(ref i, ref j);
		return (i, j, k);
	}
	public static bool Contain012((int, int, int) v)
	{
		return Is012(v.Item1) || Is012(v.Item2) || Is012(v.Item3);
	}
	static bool Is012(int i)
	{
		return (i == 0 || i == 1 || i == 2);
	}
	public static int NonContainValue((int, int, int) v, int i, int j)
	{
		return (v.Item1 != i && v.Item1 != j) ? v.Item1 : (v.Item2 != i && v.Item2 != j) ? v.Item2 : v.Item3;
	}
}