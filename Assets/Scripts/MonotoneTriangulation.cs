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
		sortedPoints = points.OrderByDescending(v => v.x).OrderByDescending(v => v.y).ToList();
		string s = "";
		foreach (var v in sortedPoints)
			s += v.ToString();
		Debug.Log(s);
		MaxIndex = points.IndexOf(sortedPoints.Last());
		MinIndex = points.IndexOf(sortedPoints.First());
		Debug.Log("Min: " + MinIndex + ", Max: " + MaxIndex);
		//ソート順最小の2つをスタックに入れる
		untriangledVert.Push(sortedPoints[0]);
		untriangledVert.Push(sortedPoints[1]);

		for (int i = 2; i < sortedPoints.Count - 1; i++)
		{
			Vector3 target = untriangledVert.Peek();
			if (GetTargetChain(points, sortedPoints[i]) != GetTargetChain(points, target))
			{
				Debug.Log("Diff");
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
				Debug.Log("Same");
				//スタックの頂点とこのループで注目してる頂点が同じチェインにある場合
				target = untriangledVert.Pop();
				//作れるだけトライアングル形成
				while (untriangledVert.Count > 0 && ShouldGenerateTriangle(target, untriangledVert.Peek(), points))
				{
					Debug.Log("Add Triangle o: " + target + ", a: " + untriangledVert.Peek() + ", b: " + sortedPoints[i]);
					AddTriangle(target, untriangledVert.Peek(), sortedPoints[i]);
					if (untriangledVert.Count > 0)
						target = untriangledVert.Pop();
				}
				Debug.Log("Last Target" + target);
				untriangledVert.Push(target);
				untriangledVert.Push(sortedPoints[i]);
			}
		}
		Debug.Log(untriangledVert.Count);
		//ソート順最大の頂点と余った頂点全部でトライアングルを形成
		// ここの形成次第では多角形外の三角形が生成される？
		// ここの形成でもトライアングルの向きを意識しなければならない
		while (untriangledVert.Count > 1)
		{
			if (untriangledVert.Count == 2)
			{
				AddTriangle(sortedPoints.Last(), untriangledVert.Pop(), untriangledVert.Pop());
			}
			else
			{
				AddTriangle(sortedPoints.Last(), untriangledVert.Pop(), untriangledVert.Peek());
			}
		}
		s = "";
		foreach (var (v, i) in triangles.Select((value, index) => (value, index)))
			s += v.ToString() + (i % 3 == 2 ? "\n" : "");
		Debug.Log(s + ", triangles" + triangles.Count);
		return triangles;
	}
	static void AddTriangle(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		//ここの順番で時計回り反時計回りが決まるのであとで調整
		//Unityでは多分時計回り
		// TODO:法線揃える処理を入れること
		triangles.Add(p0);
		triangles.Add(p1);
		triangles.Add(p2);
	}
	static bool GetTargetChain(List<Vector3> points, Vector3 p)
	{
		int t = points.IndexOf(p);
		return (MinIndex < t && t < MaxIndex) || (MaxIndex < t && t < MinIndex);
	}
	static bool ShouldGenerateTriangle(Vector3 o, Vector3 a, List<Vector3> points)
	{
		// これから伸ばそうとしている辺が隣接辺の間に収まっているかの判定
		var oIndex = points.IndexOf(o);
		var prevIndex = oIndex - 1 > 0 ? oIndex - 1 : points.Count - 1;
		var nextIndex = (oIndex + 1) % points.Count;
		var nearEdgeDot = Vector3.Dot(points[prevIndex] - o, points[nextIndex] - o) * (Vector3.Cross(points[prevIndex] - o, points[nextIndex] - o).z < 0 ? -1 : 1);
		var targetDot = Vector3.Dot(points[prevIndex] - o, a - o) * (Vector3.Cross(points[prevIndex] - o, a - o).z < 0 ? -1 : 1);
		return targetDot < nearEdgeDot;
	}
}