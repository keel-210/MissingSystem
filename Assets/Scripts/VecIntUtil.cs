using UnityEngine;

public static class VecIntUtil
{
	public static Vector3Int Sorted(Vector3Int v)
	{
		//バブルソートの理屈
		int temp;
		if (v.y < v.x)
		{
			temp = v.y;
			v.y = v.x;
			v.x = temp;
		}
		if (v.z < v.y)
		{
			temp = v.z;
			v.z = v.y;
			v.y = temp;
		}
		if (v.y < v.x)
		{
			temp = v.y;
			v.y = v.x;
			v.x = temp;
		}
		return v;
	}
	public static Vector2Int Sorted(Vector2Int v)
	{
		//バブルソートの理屈
		int temp;
		if (v.y < v.x)
		{
			temp = v.y;
			v.y = v.x;
			v.x = temp;
		}
		return v;
	}
	public static bool Contain012(Vector3Int v)
	{
		return Is012(v.x) || Is012(v.y) || Is012(v.z);
	}
	public static bool Contain012(Vector2Int v)
	{
		return Is012(v.x) || Is012(v.y);
	}
	static bool Is012(int i)
	{
		return (i == 0 || i == 1 || i == 2);
	}
}