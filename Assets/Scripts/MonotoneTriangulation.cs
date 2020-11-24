using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class MonotoneTriangulation
{
	static List<Vector3> sortedPoints = new List<Vector3>();
	static List<Vector3> triangles = new List<Vector3>();
	static List<Vector3> tempVert = new List<Vector3>();
	static Stack<Vector3> untriangledVert = new Stack<Vector3>();
	static int MaxIndex;
	static int MinIndex;
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		//y単調なポリゴンの三角化 計算量はO(n)
		//だけどどっちにしろソートでO(nlogn)かかるからO(nlogn)
		sortedPoints.Clear();
		triangles.Clear();
		tempVert.Clear();
		untriangledVert.Clear();
		//まずy基準にソート
		//ソートをnormalに垂直な成分を基準に変更する
		sortedPoints = points.OrderBy(v => v.y).ToList();
		MaxIndex = points.IndexOf(sortedPoints.Last());
		MinIndex = points.IndexOf(sortedPoints.First());
		//ソート順最小の2つをスタックに入れる
		untriangledVert.Push(sortedPoints[0]);
		untriangledVert.Push(sortedPoints[1]);

		for (int i = 2; i < sortedPoints.Count - 1; i++)
		{
			Vector3 target = untriangledVert.Peek();
			if (GetTargetChain(points, sortedPoints[i]) != GetTargetChain(points, target))
			{
				//スタックの頂点とこのループで注目してる頂点が異なるチェインにある場合
				//このループで注目してる頂点とスタックの頂点全部でトライアングル形成
				while (untriangledVert.Count > 1)
				{
					if (untriangledVert.Count == 2)
						AddTriangle(untriangledVert.Pop(), untriangledVert.Pop(), sortedPoints[i]);
					else
						AddTriangle(untriangledVert.Pop(), untriangledVert.Peek(), sortedPoints[i]);
				}
				untriangledVert.Push(sortedPoints[i - 1]);
				untriangledVert.Push(sortedPoints[i]);
			}
			else
			{
				//スタックの頂点とこのループで注目してる頂点が同じチェインにある場合
				target = untriangledVert.Pop();
				//作れるだけトライアングル形成
				while (ShouldGenerateTriangle(target, untriangledVert.Peek(), sortedPoints[i]) && untriangledVert.Count > 1)
				{
					AddTriangle(target, untriangledVert.Pop(), sortedPoints[i]);
					if (untriangledVert.Count != 0)
						target = untriangledVert.Pop();
				}
				untriangledVert.Push(target);
				untriangledVert.Push(sortedPoints[i]);
			}
		}
		//ソート順最大の頂点と余った頂点全部でトライアングルを形成
		while (untriangledVert.Count > 1)
			if (untriangledVert.Count == 2)
				AddTriangle(untriangledVert.Pop(), untriangledVert.Pop(), sortedPoints.Last());
			else
				AddTriangle(untriangledVert.Pop(), untriangledVert.Peek(), sortedPoints.Last());
		return triangles;
	}
	static void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//ここの順番で時計回り反時計回りが決まるのであとで調整
		//Unityでは多分時計回り
		triangles.Add(p0);
		triangles.Add(p1);
		triangles.Add(p2);
	}
	static bool GetTargetChain(List<Vector3> points, Vector3 p)
	{
		int t = points.IndexOf(p);
		return (MinIndex < t && t < MaxIndex) || (MaxIndex < t && t < MinIndex);
	}
	static bool ShouldGenerateTriangle(Vector3 o, Vector3 a, Vector3 b)
	{
		//ここの条件は頂点の外積のMeshが展開している平面に垂直な成分を見る。yz平面に展開してるならx
		return Vector3.Cross(a - o, b - o).x > 0;
	}
}