using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class NonMonotoneTriangulation
{
	static List<Vector3> Points = new List<Vector3>();
	static List<Vector3> sortedList = new List<Vector3>();
	static List<List<Vector3>> monotones = new List<List<Vector3>>();
	static List<Vector3> triangles = new List<Vector3>();
	//TIndexは(edgeIndex,helperIndex)で構成されておりedgeIndexにより表されるedgeは(edgeIndex,edgeIndex+1)である
	static List<Vector2Int> TIndex = new List<Vector2Int>();
	static List<Vector2Int> DiagonalIndex = new List<Vector2Int>();
	static int MaxIndex, MinIndex;
	static bool LeftChain;
	public static List<List<Vector3>> GetMonotones(List<Vector3> points)
	{
		Init();
		Points = points;
		MakeMonotone();
		return monotones;
	}
	public static List<Vector2Int> GetDiagonal(List<Vector3> points)
	{
		Init();
		Points = points;
		MakeMonotone();
		return DiagonalIndex;
	}
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		Init();
		Points = points;
		MakeMonotone();
		foreach (List<Vector3> l in monotones)
			triangles.AddRange(MonotoneTriangulation.Triangulate(l));
		return triangles;
	}
	public static void MakeMonotone()
	{
		sortedList = Points.OrderByDescending(v => v.y).ToList();
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
			Debug.Log("Target Vertex : " + TargetIndex + t.ToString() + ", o : " + o + ", a : " + a + ", b : " + b);
			switch (t)
			{
				case VertexType.StartVertex: HandleStartVertex(TargetIndex); break;
				case VertexType.SplitVertex: HandleSplitVertex(TargetIndex); break;
				case VertexType.MergeVertex: HandleMergeVertex(TargetIndex); break;
				case VertexType.RegularVertex: HandleRegularVertex(TargetIndex); break;
				case VertexType.EndVertex: HandleEndVertex(TargetIndex); break;
			}
		}
		//引いた対角線とNonMonotone連結辺からMonotone連結辺リストを生成する
	}
	static Vector3 GetListElementWithLoop(List<Vector3> l, int ind)
	{
		return l[0 <= ind ? ind % l.Count : ind % l.Count + l.Count];
	}
	static void Init()
	{
		//Init
		Points.Clear();
		monotones.Clear();
		triangles.Clear();
		sortedList.Clear();
		TIndex.Clear();
		DiagonalIndex.Clear();
	}
	//ここから先の5種類の頂点ごとの処理は割と共通部分が多いから後で整理
	static void HandleStartVertex(int index)
	{
		AddEdge(index, index);
	}
	static void HandleMergeVertex(int index)
	{
		Vector2Int v = GetHelperIndex(index - 1);
		if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		TIndex.Remove(v);
		int j = GetNearestLeftEdgeIndex(index);
		if (GetHelperVertexType(j) == VertexType.MergeVertex)
			AddDiagonal(index, GetHelperIndex(j).y);
		ChangeHelper(j, index);
	}
	static void HandleRegularVertex(int index)
	{
		if (IsVertexOnLeftChain(Points[index]))
		{
			Vector2Int v = GetHelperIndex(index - 1);
			if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
				AddDiagonal(index, v.y);
			TIndex.Remove(v);
			AddEdge(index, index);
		}
		else
		{
			int j = GetNearestLeftEdgeIndex(index);
			if (GetHelperVertexType(j) == VertexType.MergeVertex)
				AddDiagonal(index, GetHelperIndex(j).y);
			ChangeHelper(j, index);
		}
	}
	static void HandleSplitVertex(int index)
	{
		int j = GetNearestLeftEdgeIndex(index);
		AddDiagonal(index, GetHelperIndex(j).y);
		ChangeHelper(j, index);
		AddEdge(index, index);
	}
	static void HandleEndVertex(int index)
	{
		Vector2Int v = GetHelperIndex(index - 1);
		if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		TIndex.Remove(v);
	}
	static void AddDiagonal(int index0, int index1)
	{
		DiagonalIndex.Add(new Vector2Int(index0, index1));
	}
	static void AddEdge(int index, int helperIndex)
	{
		Debug.Log("Add Helper" + new Vector2Int(index, helperIndex));
		TIndex.Add(new Vector2Int(index, helperIndex));
	}
	static void ChangeHelper(int index, int helperIndex)
	{
		Debug.Log("Chenge Helper" + new Vector2Int(index, helperIndex));
		for (int i = 0; i < TIndex.Count; i++)
			if (TIndex[i].x == index)
				TIndex[i] = new Vector2Int(index, helperIndex);
	}
	static Vector2Int GetHelperIndex(int index)
	{
		Debug.Log("Request Index : " + index);
		foreach (Vector2Int e in TIndex)
			if (e.x == index)
				return e;
		return Vector2Int.one * -1;
	}
	static int GetNearestLeftEdgeIndex(int vertexIndex)
	{
		int edgeIndex = 0;
		Vector3 v = Points[vertexIndex];
		float distance = 1000000f;
		foreach (Vector2Int e in TIndex)
		{
			Vector3 e0 = GetListElementWithLoop(Points, e.x);
			Vector3 e1 = GetListElementWithLoop(Points, e.x + 1);
			Vector3 edge = Vector3.zero;
			if (e0.y < e1.y)
			{
				edge = e0;
				e0 = e1;
				e1 = edge;
			}
			edge = e0 - e1;
			Debug.Log("T search loop : e0" + e0 + " ,e1:" + e1 + " ,v" + v + " ," + TIndex.Count);
			//注目頂点が辺の左側にある
			if (Vector3.Cross(v - e1, edge).z > 0)
			{
				if (e1 == v || e0 == v)
				{
					edgeIndex = e.x;
					break;
				}
				float d = ((e0 - v) + edge.normalized * (-Vector3.Dot(e0 - v, edge.normalized))).magnitude;
				if (d < distance)
				{
					edgeIndex = e.x;
					distance = d;
				}
			}
			else
			{
				Debug.Log("Edge is right side");
				continue;
			}
		}
		Debug.Log("Nearest Edge : " + edgeIndex + " : " + distance);
		return edgeIndex;
	}
	static VertexType GetHelperVertexType(int index)
	{
		Vector2Int v = GetHelperIndex(index);
		Debug.Log("Helper : " + v);
		Vector3 o = Points[v.y];
		Vector3 a = GetListElementWithLoop(Points, v.y - 1);
		Vector3 b = GetListElementWithLoop(Points, v.y + 1);

		return GetVertexType(a, o, b);
	}
	static VertexType GetVertexType(Vector3 a, Vector3 o, Vector3 b)
	{
		bool IsNextPointUnderTargetPoint = o.y > a.y;
		bool IsPrevPointUnderTargetPoint = o.y > b.y;
		//ここあとで調整 辺連結頂点リストの回り方によって内角の最小方向は変化する ここでは反時計回りなら内角
		float InternalAngle = Vector3.Cross(a - o, b - o).z < 0 ? Vector3.Angle(a - o, b - o) : 360 - Vector3.Angle(a - o, b - o);
		Debug.Log("o : " + o + ", a : " + a + ", b : " + b + "InternalAngle :" + InternalAngle);

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