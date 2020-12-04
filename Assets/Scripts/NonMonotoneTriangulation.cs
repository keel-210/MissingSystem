using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class NonMonotoneTriangulation
{
	static List<Vector3> Points = new List<Vector3>();
	static List<Vector3> sortedList = new List<Vector3>();
	static List<List<Vector3>> monotones = new List<List<Vector3>>();
	static List<List<int>> monotonesIndex = new List<List<int>>();
	static List<Vector3> triangles = new List<Vector3>();
	static List<Vector2Int> EdgeAndHelperIndex = new List<Vector2Int>();
	static List<Vector2Int> DiagonalIndex = new List<Vector2Int>();
	static int MaxIndex;
	static int MinIndex;
	static bool LeftChain;
	public static List<List<Vector3>> Monotone(List<Vector3> points)
	{
		Points = points;
		MakeMonotone();
		return monotones;
	}
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		//Init
		monotones.Clear();
		triangles.Clear();
		Points = points;
		MakeMonotone();
		foreach (List<Vector3> l in monotones)
			triangles.AddRange(MonotoneTriangulation.Triangulate(l));
		return triangles;
	}
	public static void MakeMonotone()
	{
		sortedList = Points.OrderBy(v => v.y).ToList();
		MaxIndex = Points.IndexOf(sortedList.Last());
		MinIndex = Points.IndexOf(sortedList.First());
		int MostLeftPointIndex = Points.IndexOf(Points.OrderBy(v => v.x).First());
		LeftChain = ((MinIndex < MostLeftPointIndex && MostLeftPointIndex < MaxIndex) || (MaxIndex < MostLeftPointIndex && MostLeftPointIndex < MinIndex));

		for (int i = 0; i < sortedList.Count; i++)
		{
			int TargetIndex = Points.IndexOf(sortedList[i]);
			Vector3 o = sortedList[i];
			Vector3 a = GetListElementWithLoop(Points, TargetIndex - 1);
			Vector3 b = GetListElementWithLoop(Points, TargetIndex + 1);

			VertexType t = GetVertexType(a, o, b);
			switch (t)
			{
				case VertexType.StartVertex: HandleStartVertex(TargetIndex); break;
				case VertexType.SplitVertex: HandleSplitVertex(TargetIndex); break;
				case VertexType.MergeVertex: HandleMergeVertex(TargetIndex); break;
				case VertexType.RegularVertex: HandleRegularVertex(TargetIndex); break;
				case VertexType.EndVertex: HandleEndVertex(TargetIndex); break;
			}
		}
	}
	public static Vector3 GetListElementWithLoop(List<Vector3> l, int ind)
	{
		return l[0 < ind ? ind % l.Count : ind % l.Count + l.Count];
	}
	static void HandleStartVertex(int index)
	{
		AddEdge(index, index);
	}
	static void HandleSplitVertex(int index)
	{

	}
	static void HandleMergeVertex(int index)
	{
		Vector2Int v = GetHelperIndex(index - 1);
		if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		EdgeAndHelperIndex.Remove(v);

	}
	static void HandleRegularVertex(int index)
	{
		if (IsVertexOnLeftChain(Points[index]))
		{

		}
		else
		{

		}
	}
	static void HandleEndVertex(int index)
	{
		Vector2Int v = GetHelperIndex(index - 1);
		if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		EdgeAndHelperIndex.Remove(v);

	}
	static void DrawDiagonal()
	{
		List<Vector2Int> temp = new List<Vector2Int>();
		foreach (Vector2Int v in DiagonalIndex)
		{
			temp.Add(v);
			temp.Add(new Vector2Int(v.y, v.x));
		}

	}
	static void AddDiagonal(int index0, int index1)
	{
		DiagonalIndex.Add(new Vector2Int(index0, index1));
	}
	static void AddEdge(int index, int helperIndex)
	{
		EdgeAndHelperIndex.Add(new Vector2Int(index, helperIndex));
	}
	static void ChangeHelper(int index, int helperIndex)
	{
		for (int i = 0; i < EdgeAndHelperIndex.Count; i++)
			if (EdgeAndHelperIndex[i].x == index)
				EdgeAndHelperIndex[i] = new Vector2Int(index, helperIndex);
	}
	static Vector2Int GetHelperIndex(int index)
	{
		foreach (Vector2Int e in EdgeAndHelperIndex)
			if (e.x == index)
				return e;
		return Vector2Int.one * -1;
	}
	static bool IsNearestLeftEdge()
	{
		return true;
	}
	static VertexType GetHelperVertexType(int index)
	{
		Vector2Int v = GetHelperIndex(index);
		Vector3 o = Points[v.y];
		Vector3 a = GetListElementWithLoop(Points, v.y - 1);
		Vector3 b = GetListElementWithLoop(Points, v.y + 1);

		return GetVertexType(a, o, b);
	}
	static VertexType GetVertexType(Vector3 a, Vector3 o, Vector3 b)
	{
		bool IsNextPointUnderTargetPoint = o.y > a.y;
		bool IsPrevPointUnderTargetPoint = o.y > b.y;
		//ここあとで調整 辺連結頂点リストの回り方によって内角の最小方向は変化する 反時計回りなら内角
		float InternalAngle = Vector3.Cross(a - o, b - o).x > 0 ? Vector3.Angle(a - o, b - o) : 360 - Vector3.Angle(a - o, b - o);

		if (IsNextPointUnderTargetPoint && IsPrevPointUnderTargetPoint)
			if (InternalAngle < 180)
				return VertexType.StartVertex;
			else
				return VertexType.SplitVertex;
		else if (!IsNextPointUnderTargetPoint && !IsPrevPointUnderTargetPoint)
			if (InternalAngle < 180)
				return VertexType.EndVertex;
			else
				return VertexType.MergeVertex;
		else
			return VertexType.RegularVertex;
	}
	static bool IsVertexOnLeftChain(Vector3 p)
	{
		int t = Points.IndexOf(p);
		return LeftChain && ((MinIndex < t && t < MaxIndex) || (MaxIndex < t && t < MinIndex));
	}
}
public enum VertexType
{ StartVertex, SplitVertex, MergeVertex, RegularVertex, EndVertex, }