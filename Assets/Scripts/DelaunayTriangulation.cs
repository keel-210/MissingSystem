using System.Collections.Generic;
using UnityEngine;

public static class DelaunayTriangulation
{
	//前提条件として入力は平面上の点集合
	//最初に適当な平面投影でもしてやればいいじゃろ
	//ドロネーグラフと辺に接する2つの三角形を保管するデータがほしい
	static List<Vector3> triangles = new List<Vector3>();
	static List<Vector3> Points = new List<Vector3>();
	static Dictionary<Vector2Int, List<int>> EdgeAndTrianglesIndex = new Dictionary<Vector2Int, List<int>>();
	static Graph<Vector3Int> DelaunayGraph = new Graph<Vector3Int>();
	static Vector3 p0, p1, p2;
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		PrepareBigEnoughTriangle(points);

		for (int r = 3; r < Points.Count; r++)
		{
			Vector3Int includeTriangle = SearchTriangleIncludePoint(r);
			int i = includeTriangle[0], j = includeTriangle[1], k = includeTriangle[2];
			if (IsPointInTriangle(Points[r], Points[i], Points[j], Points[k]))
			{
				LegalizeEdge(r, i, j);
				LegalizeEdge(r, j, k);
				LegalizeEdge(r, k, i);
			}
			else
			{
				List<int> t = EdgeAndTrianglesIndex[new Vector2Int(i, j)];
				int startIndex = 0, l = 0;
				if (t[0] != k)
					l = t[0];
				else
					l = t[1];

				//ここの条件キッチリ1にならんかもしれん
				if (Vector3.Dot(Points[j] - Points[i], Points[r] - Points[i]) == 1)
					startIndex = i;
				else if (Vector3.Dot(Points[k] - Points[j], Points[r] - Points[j]) == 1)
					startIndex = j;
				else if (Vector3.Dot(Points[i] - Points[k], Points[r] - Points[k]) == 1)
					startIndex = k;

				LegalizeEdge(r, includeTriangle[startIndex], l);
				LegalizeEdge(r, l, includeTriangle[(startIndex + 1) % 3]);
				LegalizeEdge(r, includeTriangle[(startIndex + 1) % 3], includeTriangle[(startIndex + 2) % 3]);
				LegalizeEdge(r, includeTriangle[(startIndex + 2) % 3], includeTriangle[startIndex]);
			}
		}
		//最後に一番でけえ三角形の仮想頂点とそれにつながる辺を削除する

		return triangles;
	}
	static void PrepareBigEnoughTriangle(List<Vector3> points)
	{
		float AbsMaxValue = 0;
		foreach (Vector3 v in points)
		{
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.x) ? Mathf.Abs(v.x) : AbsMaxValue;
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.y) ? Mathf.Abs(v.y) : AbsMaxValue;
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.z) ? Mathf.Abs(v.z) : AbsMaxValue;
		}
		p0 = new Vector3(3 * AbsMaxValue, 0, 0);
		p1 = new Vector3(0, 3 * AbsMaxValue, 0);
		p2 = new Vector3(-3 * AbsMaxValue, -3 * AbsMaxValue, 0);
		Points.Add(p0);
		Points.Add(p1);
		Points.Add(p2);
		Points.AddRange(points);
		DelaunayGraph.AddNode(new Vector3Int(0, 1, 2));
		AddTriangle(null, 0, 1, 2);
	}
	static Vector3Int SearchTriangleIncludePoint(int r)
	{

		return Vector3Int.zero;
	}
	static void LegalizeEdge(int r, int i, int j)
	{
		//追加された点rを内包していない三角形を探す
		List<int> t = EdgeAndTrianglesIndex[new Vector2Int(i, j)];
		//最大の三角形の辺には2つめの三角形は存在しない
		if (t.Count < 2)
			return;
		//そもそものところ追加された三角形の一つは確実にrを含むためrを含まない三角形について判定すればよい
		int k = t[0];
		if (k == r)
			k = t[1];

		//探したら辺フリップするかどうか決める
		if (!IsLegalEdge(r, i, j, k))
		{
			// FlipEdge(r, i, j, k);
			LegalizeEdge(r, i, k);
			LegalizeEdge(r, k, j);
		}
	}
	static void AddTriangle(GraphNode<Vector3Int> parentNode, int i, int j, int k)
	{
		if (parentNode != null)
		{
			DelaunayGraph.AddDirectedEdge(parentNode, new GraphNode<Vector3Int>(new Vector3Int(i, j, k)));

			AddEdgeAndTriangleIndex(parentNode, i, j, k);
			AddEdgeAndTriangleIndex(parentNode, j, k, i);
			AddEdgeAndTriangleIndex(parentNode, k, i, j);
		}
		else
		{

		}
	}
	static void AddEdgeAndTriangleIndex(GraphNode<Vector3Int> parentNode, int i, int j, int k)
	{
		//ここijkが小さい順に並ぶように調整する←調整しなくてもいいのでは
		//辺に対して三角形を表すのにVector3Int要る？辺+頂点でいいじゃん(いいじゃん)
		if (!EdgeAndTrianglesIndex.ContainsKey(new Vector2Int(i, j)))
			EdgeAndTrianglesIndex.Add(new Vector2Int(i, j), new List<int>() { k });
		else
		{
			EdgeAndTrianglesIndex.Remove(new Vector2Int(i, j));
			EdgeAndTrianglesIndex[new Vector2Int(i, j)].Add(k);
		}
	}
	static void FlipEdge(GraphNode<Vector3Int> parentNode, int r, int i, int j, int k)
	{
		EdgeAndTrianglesIndex.Remove(new Vector2Int(i, j));
		EdgeAndTrianglesIndex.Add(new Vector2Int(r, k), new List<int>() { i, j });
	}
	static bool IsLegalEdge(int r, int i, int j, int k)
	{
		//三角形のうち外接円の弦となる適当な辺と残りの頂点の角度＝適当な弦に対する円周角
		//(ここで判定する辺はijであるため、弦となる適当な辺はik, jkのどちらかである。今回はik)
		//点rと弦でできる角度が円周角より小さければ点rは外接円より外側にある
		//よって辺ijは正当な辺
		Vector3 ij = (Points[i] - Points[j]).normalized, jk = (Points[k] - Points[j]).normalized;
		Vector3 ir = (Points[i] - Points[r]).normalized, rk = (Points[k] - Points[r]).normalized;
		return (Vector3.Dot(ij, jk)) < (Vector3.Dot(ir, rk));
	}
	static bool IsPointInTriangle(Vector3 p, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		Vector3 c0 = Vector3.Cross(t1 - t0, p - t1), c1 = Vector3.Cross(t2 - t1, p - t2), c2 = Vector3.Cross(t0 - t2, p - t0);
		return (Vector3.Dot(c0, c1) > 0 && Vector3.Dot(c0, c2) > 0);
	}
}