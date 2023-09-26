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
	//TIndexは(edgeIndex,helperIndex)で構成されておりedgeIndexにより表されるedgeは(edgeIndex,edgeIndex+1)である
	static List<Vector2Int> TIndex = new List<Vector2Int>();
	static List<Vector2Int> DiagonalIndex = new List<Vector2Int>();
	static int MaxIndex, MinIndex;
	static bool LeftChain;
	static List<int> usedPoint = new List<int>();
	public static List<List<Vector3>> GetMonotones(List<Vector3> points)
	{
		Init();
		Points = new List<Vector3>(points);
		DrawDiagonal();
		monotonesIndex = MakeMonotonesIndex();
		monotones = MakeMonotones();
		return monotones;
	}
	public static List<Vector2Int> GetDiagonal(List<Vector3> points)
	{
		Init();
		Points = new List<Vector3>(points);
		DrawDiagonal();
		return DiagonalIndex;
	}
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		Init();
		Points = new List<Vector3>(points);
		DrawDiagonal();
		monotonesIndex = MakeMonotonesIndex();
		monotones = MakeMonotones();
		foreach (List<Vector3> l in monotones)
			triangles.AddRange(MonotoneTriangulation.Triangulate(l));
		return triangles;
	}
	public static void DrawDiagonal()
	{
		sortedList = Points.OrderBy(v => v.y).ToList();
		MaxIndex = Points.IndexOf(sortedList.Last());
		MinIndex = Points.IndexOf(sortedList.First());
		int MostLeftPointIndex = Points.IndexOf(Points.OrderBy(v => v.x).First());
		LeftChain = (MaxIndex < MostLeftPointIndex && MostLeftPointIndex < MinIndex + Points.Count - 1);

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
	}
	static List<List<int>> MakeMonotonesIndex()
	{
		Debug.Log("-------------------Start Making Monotones-------------------");
		//引いた対角線とNonMonotone連結辺からMonotone連結辺リストを生成する
		//妙に複雑だし難しい絶対なんか簡単な方法があるような気もしないでもない
		int startPoint = 0, nowPoint = 0;
		List<int> targetMonotoneIndex = new List<int>();
		List<int> StartPointIndexCache = new List<int>();
		List<UsableDiagonal> usedDiagonal = new List<UsableDiagonal>();
		List<List<int>> Indexs = new List<List<int>>();
		UsableDiagonal minD = new UsableDiagonal(0, Vector2Int.one * 100000);
		foreach (Vector2Int d in DiagonalIndex)
			usedDiagonal.Add(new UsableDiagonal(0, d));
		foreach (UsableDiagonal d in usedDiagonal)
			if (d.GetMinIndex() < minD.GetMinIndex())
				minD = d;
		Debug.Log("minD" + minD.Diagonal);
		//対角線+1個の単調多角形が生成される
		for (int i = 0; i < DiagonalIndex.Count + 1; i++)
		{
			//開始点の選定
			//対角線は必ず2回使用されるため既に使い切った対角線の頂点を開始点にすることはできない
			//一度開始点として使用するともう開始点として使用できない
			//開始点はメモしておくか
			startPoint = 100000;
			foreach (UsableDiagonal d in usedDiagonal)
			{
				if (d.UsedCount < 2 && d.Diagonal.x < startPoint && !StartPointIndexCache.Contains(d.Diagonal.x))
					startPoint = d.Diagonal.x;
				if (d.UsedCount < 2 && d.Diagonal.y < startPoint && !StartPointIndexCache.Contains(d.Diagonal.y))
					startPoint = d.Diagonal.y;
			}
			StartPointIndexCache.Add(startPoint);
			Debug.Log("Loop:" + i + ", Start Index:" + startPoint);
			nowPoint = startPoint;
			do
			{
				UsableDiagonal targetD = new UsableDiagonal(0, Vector2Int.one * 100000);
				//nowPointより大きい頂点の中で最小のものを探す
				//たぶんないと思うが一応記載しておくと対角線のみで構成される単純多角形には対応できない
				foreach (UsableDiagonal e in usedDiagonal)
				{
					if (nowPoint < e.Diagonal.x && e.Diagonal.x < targetD.Diagonal.x)
					{
						targetD = e;
					}
					else if (nowPoint < e.Diagonal.y && e.Diagonal.y < targetD.Diagonal.x)
					{
						targetD = e;
						targetD.Diagonal = new Vector2Int(e.Diagonal.y, e.Diagonal.x);
					}
					if (targetMonotoneIndex.Count > 0 && (nowPoint == e.Diagonal.x && e.Diagonal.y != targetMonotoneIndex.Last()))
					{
						targetD = e;
						Debug.Log("!!!Self Diagonal Edge!!!");
						break;
					}
					else if (targetMonotoneIndex.Count > 0 && (nowPoint == e.Diagonal.y && e.Diagonal.x != targetMonotoneIndex.Last()))
					{
						targetD = e;
						targetD.Diagonal = new Vector2Int(e.Diagonal.y, e.Diagonal.x);
						Debug.Log("!!!Self Diagonal Edge!!!");
						break;
					}
				}
				//接続している対角線が見つかった場合
				//他の対角線に接続している
				//自分自身に接続している 例えば開始点が3で3->5の対角線で3->4->5等
				if (targetD.Diagonal != Vector2Int.one * 100000)
				{
					targetD.UsedCount++;
					for (int j = nowPoint; j <= targetD.Diagonal.x; j++)
						targetMonotoneIndex.Add(j);
					nowPoint = targetD.Diagonal.y;
					Debug.Log("Got Diagonal Edge" + targetD.Diagonal + ", nowPoint:" + nowPoint + ", startPoint" + startPoint);
				}
				//見つからない場合は
				//対角線は必ずほかの対角線に接続しているため一番小さい要素の対角線に接続している
				else
				{
					Debug.Log("Min Diagonal Edge" + targetD.Diagonal + ", nowPoint:" + nowPoint + ", startPoint" + startPoint);
					for (int j = nowPoint; j < Points.Count; j++)
						targetMonotoneIndex.Add(j);
					for (int j = 0; j <= minD.GetMinIndex(); j++)
						targetMonotoneIndex.Add(j);
					minD.UsedCount++;
					nowPoint = minD.GetMaxIndex();
					Debug.Log("Min Diagonal Edge" + targetD.Diagonal + ", nowPoint:" + nowPoint + ", startPoint" + startPoint);
				}
			} while (nowPoint != startPoint);
			Indexs.Add(new List<int>(targetMonotoneIndex));
			Debug.Log("Target Monotone Count" + targetMonotoneIndex.Count);
			targetMonotoneIndex.Clear();
		}
		//長すぎ処理が複雑すぎ
		string s = "";
		foreach (var i in Indexs)
		{
			foreach (var l in i)
				s += l.ToString() + " , ";
			Debug.Log("Monotone Index " + s);
			s = "";
		}
		Debug.Log("-------------------End Making Monotones-------------------");
		return Indexs;
	}
	static List<List<Vector3>> MakeMonotones()
	{
		List<List<Vector3>> ms = new List<List<Vector3>>();
		var temp = new List<Vector3>();
		foreach (var l in monotonesIndex)
		{
			foreach (var i in l)
				temp.Add(Points[i]);
			ms.Add(new List<Vector3>(temp));
			temp.Clear();
		}
		return ms;
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
		AddHelper(index, index);
	}
	static void HandleMergeVertex(int index)
	{
		var targetIndex = index - 1 > 0 ? index - 1 : index - 1 + Points.Count;
		Vector2Int v = GetHelperIndex(targetIndex);
		if (GetHelperVertexType(targetIndex) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		TIndex.Remove(v);
		int j = GetNearestLeftEdgeIndex(index);
		if (GetHelperVertexType(j) == VertexType.MergeVertex)
			AddDiagonal(index, GetHelperIndex(j).y);
		ChangeHelper(j, index);
	}
	static void HandleRegularVertex(int index)
	{
		// ここの判定おかしい凹型ポリゴンで機能しない
		// 通常点であれば周り方向とひとつ前と後を見て上下どちらに流れているか見るだけでいいのでは
		if (IsVertexOnLeftChain(Points[index]))
		{
			Vector2Int v = GetHelperIndex(index - 1);
			if (GetHelperVertexType(index - 1) == VertexType.MergeVertex)
				AddDiagonal(index, v.y);
			TIndex.Remove(v);
			AddHelper(index, index);
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
		AddHelper(index, index);
	}
	static void HandleEndVertex(int index)
	{
		var targetIndex = index - 1 > 0 ? index - 1 : index - 1 + Points.Count;
		Vector2Int v = GetHelperIndex(targetIndex);
		if (GetHelperVertexType(targetIndex) == VertexType.MergeVertex)
			AddDiagonal(index, v.y);
		TIndex.Remove(v);
	}
	static void AddDiagonal(int index0, int index1)
	{
		//揃えて追加する
		if (index0 < index1)
			DiagonalIndex.Add(new Vector2Int(index0, index1));
		else
			DiagonalIndex.Add(new Vector2Int(index1, index0));
	}
	static void AddHelper(int index, int helperIndex)
	{
		Debug.Log("Add Helper" + new Vector2Int(index, helperIndex));
		TIndex.Add(new Vector2Int(index, helperIndex));
	}
	static void ChangeHelper(int index, int helperIndex)
	{
		Debug.Log("Change Helper" + new Vector2Int(index, helperIndex));
		for (int i = 0; i < TIndex.Count; i++)
			if (TIndex[i].x == index)
				TIndex[i] = new Vector2Int(index, helperIndex);
	}
	static Vector2Int GetHelperIndex(int index)
	{
		Debug.Log("Request Index : " + index + ", Count" + TIndex.Count + ", TIndex0" + TIndex[0]);
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
			if (Vector3.Cross(v - e1, edge).z >= 0)
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
		bool IsNextPointUnderTargetPoint = o.y < a.y;
		bool IsPrevPointUnderTargetPoint = o.y < b.y;
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
		Debug.Log("LeftChain " + (MaxIndex < t && t < MinIndex + Points.Count - 1) + " , target" + t + "Max:" + MaxIndex);
		return LeftChain & (MaxIndex < t && t < MinIndex + Points.Count - 1);
	}
}
public enum VertexType
{ StartVertex, SplitVertex, MergeVertex, RegularVertex, EndVertex, }