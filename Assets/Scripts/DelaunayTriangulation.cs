using System.Collections.Generic;
using UnityEngine;

public static class DelaunayTriangulation
{
	//前提条件として入力は平面上の点集合
	//最初に適当な平面投影でもしてやればいいじゃろ
	//ドロネーグラフと辺に接する2つの三角形を保管するデータがほしい
	static List<Vector3> triangles = new List<Vector3>();
	static List<Vector3> Points = new List<Vector3>();
	static Dictionary<(int, int), List<GraphNode<(int, int, int)>>> EdgeAndTrianglesIndex = new Dictionary<(int, int), List<GraphNode<(int, int, int)>>>();
	static Graph<(int, int, int)> DelaunayGraph = new Graph<(int, int, int)>();
	static HashSet<GraphNode<(int, int, int)>> aliveNodes = new HashSet<GraphNode<(int, int, int)>>();
	public static List<Vector3> Triangulate(List<Vector3> points)
	{
		Initialize();
		PrepareBigEnoughTriangle(points);

		for (int r = 3; r < Points.Count; r++)
		{
			//この三角形探索はDelaunayGraphを用いるので現在の三角形分割が実際に存在する必要はない
			//でも実質DelaunayGraphもEdgeAndTriangleも現在の三角形分割を表現してるじゃん←それはそう
			GraphNode<(int, int, int)> includeTriangleGraph = SearchTriangleIncludePoint(r);
			(int, int, int) includeTriangle = includeTriangleGraph.Value;
			int i = includeTriangle.Item1, j = includeTriangle.Item2, k = includeTriangle.Item3;
			if (IsPointInTriangleValue(Points[r], Points[i], Points[j], Points[k]) > 0)
			{
				//ここに新頂点rによる分割処理を入れる
				//ここのドロネーグラフは親が必ず一つで子が三つ
				//三角形ijkをijr,jkr,kirに分割する
				var ijr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(i, j, r));
				var jkr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(j, k, r));
				var kir = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(k, i, r));
				DelaunayGraph.AddDirectedEdge(includeTriangleGraph, ijr);
				DelaunayGraph.AddDirectedEdge(includeTriangleGraph, jkr);
				DelaunayGraph.AddDirectedEdge(includeTriangleGraph, kir);
				aliveNodes.Remove(includeTriangleGraph);
				aliveNodes.Add(ijr);
				aliveNodes.Add(jkr);
				aliveNodes.Add(kir);
				AddEdgeAndTriangleIndex(i, r, ijr);
				AddEdgeAndTriangleIndex(i, r, kir);
				AddEdgeAndTriangleIndex(j, r, jkr);
				AddEdgeAndTriangleIndex(j, r, ijr);
				AddEdgeAndTriangleIndex(k, r, kir);
				AddEdgeAndTriangleIndex(k, r, jkr);
				ChangeEdgeAndTriangleIndex(i, j, ijr, includeTriangle);
				ChangeEdgeAndTriangleIndex(j, k, jkr, includeTriangle);
				ChangeEdgeAndTriangleIndex(k, i, kir, includeTriangle);
				LegalizeEdge(r, i, j);
				LegalizeEdge(r, j, k);
				LegalizeEdge(r, k, i);
			}
			else
			{
				//ここの条件キッチリ1にならんかもしれん
				// if (Vector3.Dot((Points[j] - Points[i]).normalized, (Points[r] - Points[i]).normalized) == 1)//rは辺ij上 これは必要ない
				// { }				else 
				if (Vector3.Dot((Points[k] - Points[j]).normalized, (Points[r] - Points[j]).normalized) == 1)//rは辺jk上
				{
					VecIntUtil.Swap(ref i, ref j);
					VecIntUtil.Swap(ref j, ref k);
				}
				else if (Vector3.Dot((Points[i] - Points[k]).normalized, (Points[r] - Points[k]).normalized) == 1)//rは辺ki上
				{
					VecIntUtil.Swap(ref i, ref j);
					VecIntUtil.Swap(ref j, ref k);
				}

				int[] EdgeOnPoint = VecIntUtil.Sorted(new int[] { i, j });
				List<GraphNode<(int, int, int)>> t = EdgeAndTrianglesIndex[(EdgeOnPoint[0], EdgeOnPoint[1])];

				var jkr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(j, k, r));
				var kir = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(k, i, r));
				if (t.Count != 1)
				{

					GraphNode<(int, int, int)> ijlNode = k != VecIntUtil.NonContainValue(t[0].Value, i, j) ? t[0] : t[1];
					int l = VecIntUtil.NonContainValue(ijlNode.Value, i, j);

					//ここに新頂点rによる分割処理を入れる
					//三角形探索に使用しているのはEdgeAndTriangleなので三角形そのものを追加する必要はない
					//ここの処理は辺ij,jk,kiのどれに属してるかで順番が変わる
					//新しくできる三角形は辺ij上なら三角形ilr,jlr,jkr,kir
					//新しくできる三角形は辺jk上なら三角形ilr,jlr,jkr,kir
					//新しくできる三角形は辺ki上なら三角形ilr,jlr,jkr,kir
					var ilr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(i, l, r));
					var jlr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(j, l, r));

					AddEdgeAndTriangleIndex(i, r, kir);
					AddEdgeAndTriangleIndex(i, r, ilr);
					AddEdgeAndTriangleIndex(j, r, jkr);
					AddEdgeAndTriangleIndex(j, r, jlr);
					AddEdgeAndTriangleIndex(k, r, jkr);
					AddEdgeAndTriangleIndex(k, r, kir);
					AddEdgeAndTriangleIndex(l, r, ilr);
					AddEdgeAndTriangleIndex(l, r, jlr);

					ChangeEdgeAndTriangleIndex(i, l, ilr, ijlNode.Value);
					ChangeEdgeAndTriangleIndex(l, j, jlr, ijlNode.Value);
					ChangeEdgeAndTriangleIndex(j, k, jkr, includeTriangle);
					ChangeEdgeAndTriangleIndex(k, i, kir, includeTriangle);

					DelaunayGraph.AddDirectedEdge(ijlNode, ilr);
					DelaunayGraph.AddDirectedEdge(ijlNode, jlr);
					DelaunayGraph.AddDirectedEdge(includeTriangleGraph, jkr);
					DelaunayGraph.AddDirectedEdge(includeTriangleGraph, kir);

					aliveNodes.Remove(includeTriangleGraph);
					aliveNodes.Remove(ijlNode);
					aliveNodes.Add(ilr);
					aliveNodes.Add(jlr);
					aliveNodes.Add(jkr);
					aliveNodes.Add(kir);

					LegalizeEdge(r, i, l);
					LegalizeEdge(r, l, j);
					LegalizeEdge(r, j, k);
					LegalizeEdge(r, k, i);
				}
				else
				{
					AddEdgeAndTriangleIndex(i, r, kir);
					AddEdgeAndTriangleIndex(j, r, jkr);
					AddEdgeAndTriangleIndex(k, r, jkr);
					AddEdgeAndTriangleIndex(k, r, kir);

					ChangeEdgeAndTriangleIndex(j, k, jkr, includeTriangle);
					ChangeEdgeAndTriangleIndex(k, i, kir, includeTriangle);

					DelaunayGraph.AddDirectedEdge(includeTriangleGraph, jkr);
					DelaunayGraph.AddDirectedEdge(includeTriangleGraph, kir);

					aliveNodes.Remove(includeTriangleGraph);
					aliveNodes.Add(jkr);
					aliveNodes.Add(kir);

					LegalizeEdge(r, j, k);
					LegalizeEdge(r, k, i);
				}
			}
		}
		//"十分に大きい三角形"の仮想頂点とそれにつながる辺を削除する
		//最後に三角形分割をDelaunayGraphかEdgeAndTriangleから生成する
		//DelaunayGraphなら全て列挙しながら子要素が0の三角形を取り出す
		//EdgeAndTriangleなら適当に取り出して注目三角形に含まれる要素を消去しながら列挙する
		//上の処理とセットにして"十分に大きい三角形"の仮想頂点を含まないことも条件に入れれば楽じゃない？
		triangles = new List<Vector3>();
		MakeTriangles();
		return triangles;
	}
	static void Initialize()
	{
		Points.Clear();
		EdgeAndTrianglesIndex.Clear();
		DelaunayGraph = new Graph<(int, int, int)>();
		aliveNodes.Clear();
	}
	static void PrepareBigEnoughTriangle(List<Vector3> points)
	{
		//"十分に大きい三角形"はすべての点の中の最大値*2を辺とする正方形をピッタリ覆う三角形を定義すればよい
		float AbsMaxValue = 0;
		foreach (Vector3 v in points)
		{
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.x) ? Mathf.Abs(v.x) : AbsMaxValue;
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.y) ? Mathf.Abs(v.y) : AbsMaxValue;
			AbsMaxValue = AbsMaxValue < Mathf.Abs(v.z) ? Mathf.Abs(v.z) : AbsMaxValue;
		}
		Vector3 p0 = new Vector3(3f * AbsMaxValue, 0, 0);
		Vector3 p1 = new Vector3(0, 3f * AbsMaxValue, 0);
		Vector3 p2 = new Vector3(-3f * AbsMaxValue, -3f * AbsMaxValue, 0);
		Points.Add(p0);
		Points.Add(p1);
		Points.Add(p2);
		Points.AddRange(points);
		GraphNode<(int, int, int)> bet = new GraphNode<(int, int, int)>((0, 1, 2));
		DelaunayGraph.AddNode(bet);
		aliveNodes.Add(bet);
		AddEdgeAndTriangleIndex(0, 1, bet);
		AddEdgeAndTriangleIndex(1, 2, bet);
		AddEdgeAndTriangleIndex(2, 0, bet);
	}
	static GraphNode<(int, int, int)> SearchTriangleIncludePoint(int r)
	{
		GraphNode<(int, int, int)> temp = (GraphNode<(int, int, int)>)DelaunayGraph.NodeList[0];
		//ここのループはドロネーグラフの階層分だけ行われる
		while (temp != null && temp.Neighbors.Count > 0)
			temp = GetNodeIncludePoint(temp.Neighbors, Points[r]);
		return temp;
	}
	static GraphNode<(int, int, int)> GetNodeIncludePoint(NodeList<(int, int, int)> list, Vector3 point)
	{
		foreach (GraphNode<(int, int, int)> v in list)
			if (IsPointInTriangle(point, Points[v.Value.Item1], Points[v.Value.Item2], Points[v.Value.Item3]))
				return v;
		return null;
	}
	static void LegalizeEdge(int r, int i, int j)
	{
		int[] v = VecIntUtil.Sorted(new int[] { i, j });
		if (!EdgeAndTrianglesIndex.ContainsKey((v[0], v[1])))
			return;

		List<GraphNode<(int, int, int)>> t = EdgeAndTrianglesIndex[(v[0], v[1])];
		//最大の三角形の辺には2つめの三角形は存在しない
		//最大の三角形の辺はフリップできない
		if (t.Count < 2 || i == j)
			return;
		//追加された点rを内包していない三角形を探す
		//そもそものところ追加された三角形の一つは確実にrを含むためrを含まない三角形について判定すればよい
		i = v[0];
		j = v[1];
		int k = r != VecIntUtil.NonContainValue(t[0].Value, i, j) ? VecIntUtil.NonContainValue(t[0].Value, i, j) : VecIntUtil.NonContainValue(t[1].Value, i, j);
		//辺フリップ処理
		if (!IsLegalEdge(r, i, j, k))
		{
			FlipEdge(r, i, j, k);
			LegalizeEdge(r, i, k);
			LegalizeEdge(r, k, j);
		}
	}

	static void AddEdgeAndTriangleIndex(int i, int j, GraphNode<(int, int, int)> k)
	{
		//ここijkが小さい順に並ぶように調整する←調整しなくてもいいのでは
		//辺に対して三角形を表すのにVector3Int要る？辺+頂点でいいじゃん(いいじゃん)←ダメだった
		//辺だけ調整する
		var v = VecIntUtil.SortedPoint(i, j);
		if (!EdgeAndTrianglesIndex.ContainsKey(v))
			EdgeAndTrianglesIndex.Add(v, new List<GraphNode<(int, int, int)>>() { k });
		else if (!EdgeAndTrianglesIndex[v].Contains(k))
			EdgeAndTrianglesIndex[v].Add(k);
	}
	static void ChangeEdgeAndTriangleIndex(int i, int j, GraphNode<(int, int, int)> k, (int, int, int) oldIndex)
	{
		var v = VecIntUtil.SortedPoint(i, j);
		if (EdgeAndTrianglesIndex.ContainsKey(v))
		{
			var target = EdgeAndTrianglesIndex[v];
			if (target.Count > 1)
			{
				if (target[0].Value == oldIndex && target[1].Value != k.Value)
					target[0] = k;
				else if (target[1].Value == oldIndex && target[0].Value != k.Value)
					target[1] = k;
			}
			else if (target[0].Value == oldIndex)
				target[0] = k;
		}
	}
	static void FlipEdge(int r, int i, int j, int k)
	{
		//ゴチャゴチャしてきたな
		//辺ijを消して辺krにフリップする
		var ikr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(i, k, r));
		var jkr = new GraphNode<(int, int, int)>(VecIntUtil.SortedPoint(j, k, r));
		var nowEdge = VecIntUtil.SortedPoint(i, j);
		var newEdge = VecIntUtil.SortedPoint(r, k);
		//フリップした時の親は二つ(辺を横切る以上必ず二つの三角形を分割する)
		//この場合辺ijをフリップするので三角形ijr,ijkを分割することになる
		DelaunayGraph.AddDirectedEdge(EdgeAndTrianglesIndex[nowEdge][0], ikr);
		DelaunayGraph.AddDirectedEdge(EdgeAndTrianglesIndex[nowEdge][0], jkr);
		DelaunayGraph.AddDirectedEdge(EdgeAndTrianglesIndex[nowEdge][1], ikr);
		DelaunayGraph.AddDirectedEdge(EdgeAndTrianglesIndex[nowEdge][1], jkr);
		aliveNodes.Remove(EdgeAndTrianglesIndex[nowEdge][0]);
		aliveNodes.Remove(EdgeAndTrianglesIndex[nowEdge][1]);
		aliveNodes.Add(ikr);
		aliveNodes.Add(jkr);
		(int, int, int) ijk = VecIntUtil.SortedPoint(i, j, k), ijr = VecIntUtil.SortedPoint(i, j, r);
		ChangeEdgeAndTriangleIndex(i, k, ikr, ijk);
		ChangeEdgeAndTriangleIndex(i, r, ikr, ijr);
		ChangeEdgeAndTriangleIndex(j, k, jkr, ijk);
		ChangeEdgeAndTriangleIndex(j, r, jkr, ijr);
		EdgeAndTrianglesIndex[newEdge] = new List<GraphNode<(int, int, int)>>() { ikr, jkr };
		EdgeAndTrianglesIndex.Remove(nowEdge);
	}
	static bool IsLegalEdge(int r, int i, int j, int k)
	{
		//三角形のうち外接円の弦となる適当な辺と残りの頂点の角度＝適当な弦に対する円周角
		//(ここで判定する辺はijであるため、弦となる適当な辺はik, jkのどちらかである。今回はik)
		//点rと弦でできる角度が円周角より小さければ点rは外接円より外側にある
		//この時辺ijは正当な辺とする
		//弦となる適当な辺からみてkとrは同じ側にある必要がある
		//ikjとikrの外積方向の比較でどうすか
		Vector3 c0 = Vector3.Cross(Points[k] - Points[i], Points[j] - Points[k]), c1 = Vector3.Cross(Points[k] - Points[i], Points[r] - Points[k]);
		if (Vector3.Dot(c0, c1) < 0)
			VecIntUtil.Swap(ref i, ref j);
		Vector3 ij = (Points[i] - Points[j]).normalized, jk = (Points[k] - Points[j]).normalized;
		Vector3 ir = (Points[i] - Points[r]).normalized, rk = (Points[k] - Points[r]).normalized;
		return ((Vector3.Dot(ij, jk)) < (Vector3.Dot(ir, rk)));
	}
	static bool IsPointInTriangle(Vector3 p, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		//同一平面状にある点と三角形の内外判定(サマリーで書いたら？)←めんどい
		//微妙に-方向にはみ出すことがある-1E-06~09程度
		Vector3 c0 = Vector3.Cross(t1 - t0, p - t1), c1 = Vector3.Cross(t2 - t1, p - t2), c2 = Vector3.Cross(t0 - t2, p - t0);
		return (Vector3.Dot(c0, c1) > -0.00001f) && (Vector3.Dot(c0, c2) >= -0.00001f);
	}
	static float IsPointInTriangleValue(Vector3 p, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		//同一平面状にある点と三角形の内外判定数値版
		Vector3 c0 = Vector3.Cross(t1 - t0, p - t1), c1 = Vector3.Cross(t2 - t1, p - t2), c2 = Vector3.Cross(t0 - t2, p - t0);
		return (Vector3.Dot(c0, c1) * (Vector3.Dot(c0, c2)));
	}
	static void MakeTriangles()
	{
		foreach (var v in aliveNodes)
		{
			if (!VecIntUtil.Contain012(v.Value))
			{
				triangles.Add(Points[v.Value.Item1]);
				triangles.Add(Points[v.Value.Item2]);
				triangles.Add(Points[v.Value.Item3]);
			}
		}
	}
}
