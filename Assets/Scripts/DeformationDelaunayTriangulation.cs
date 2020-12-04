using System.Collections.Generic;
using UnityEngine;

public static class DeformationDelaunayTriangulation
{
	//前提条件として入力は平面上の点集合
	//最初に適当な平面投影でもしてやればいいじゃろ
	//ドロネーグラフと辺に接する2つの三角形を保管するデータがほしい
	static List<Vector3> triangles = new List<Vector3>();
	static List<Vector3> Points = new List<Vector3>();
	static Dictionary<Vector2Int, List<int>> EdgeAndTrianglesIndex = new Dictionary<Vector2Int, List<int>>();
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		Points = points;
		for (int r = 0; r < Points.Count; r++)
		{
			int[] includeTriangle = PointIncludeTriangle();
			int i = includeTriangle[0], j = includeTriangle[1], k = includeTriangle[2];
			if (IsPointInTriangle(Points[r], Points[i], Points[j], Points[k]))
			{
				LegalizeEdge(r, i, j);
				LegalizeEdge(r, j, k);
				LegalizeEdge(r, k, i);
			}
			//辺に頂点が重なった場合何もしない実質的にその頂点は除外される 
			//平面投影した後はタブーだが同一平面状の点の領域を埋めるだけならOK
		}
		//最後に一番でけえ三角形の仮想頂点とそれにつながる辺を削除する

		return triangles;
	}
	static void LegalizeEdge(int r, int i, int j)
	{
		//点rを内包していない三角形を探す
		List<int> t = EdgeAndTrianglesIndex[new Vector2Int(i, j)];
		int startIndex = 0;
		if (IsPointInTriangle(Points[r], Points[t[0]], Points[t[1]], Points[t[2]]))
			startIndex = 3;

		int k = 0;
		for (int z = startIndex; z < 3; z++)
			if (t[z] != i && t[z] != j)
				k = t[z];

		if (!IsLegalEdge(r, i, j, k))
		{
			FlipEdge(r, i, j, k);
			LegalizeEdge(r, i, k);
			LegalizeEdge(r, k, j);
		}
	}
	static void AddTriangle()
	{

	}
	static void FlipEdge(int r, int i, int j, int k)
	{

	}
	static int[] PointIncludeTriangle()
	{
		return new int[] { 0, 0, 0 };
	}
	static bool IsLegalEdge(int r, int i, int j, int k)
	{
		Vector3 ij = (Points[i] - Points[j]).normalized, jk = (Points[k] - Points[j]);
		Vector3 ir = (Points[i] - Points[r]).normalized, rk = (Points[k] - Points[r]);
		return (Vector3.Dot(ij, jk)) < (Vector3.Dot(ir, rk));
	}
	static bool IsPointInTriangle(Vector3 p, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		Vector3 c0 = Vector3.Cross(t1 - t0, p - t1), c1 = Vector3.Cross(t2 - t1, p - t2), c2 = Vector3.Cross(t0 - t2, p - t0);
		return (Vector3.Dot(c0, c1) > 0 && Vector3.Dot(c0, c2) > 0);
	}
}