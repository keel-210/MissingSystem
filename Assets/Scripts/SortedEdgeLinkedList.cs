using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public static class SortedEdgeLinkedList
{
	static List<Vector3> LinkedList = new List<Vector3>();
	static List<List<Vector3>> Edges = new List<List<Vector3>>();
	static List<Chains> chains = new List<Chains>();
	public static List<Vector3> MakeLinkedList(List<Vector3> points)
	{
		Initialize();
		//辺を保存 ついでに辺の流れを統一する
		for (int i = 0; i < points.Count - 1; i += 2)
		{
			if (points[i].y < points[i + 1].y)
				Edges.Add(new List<Vector3> { points[i], points[i + 1] });
			else
				Edges.Add(new List<Vector3> { points[i + 1], points[i] });
		}
		//xでソートした後yでソート ここでO(nlogn)
		Edges = Edges.OrderBy(x => x[0].x).OrderBy(x => x[0].y).ToList();
		//チェーンを構築する ここ以降の処理は全てチェーン数に依存することになる
		//凸があるとチェーンが一つ増える 多分最悪n/4個チェーンが構築される
		//x->yのソートで次の頂点が必ず同じ頂点が並ぶはず
		chains.Add(new Chains(Edges[0][0], Edges[0][1], Edges[1][1]));
		for (int i = 2; i < Edges.Count; i++)
		{
			bool NewChainFlg = false;
			for (int j = 0; j < chains.Count; j++)
				if (NewChainFlg = NewChainFlg | chains[j].Add(Edges[i][0], Edges[i][1]))
					break;
			if (!NewChainFlg)
				chains.Add(new Chains(Edges[i][0], Edges[i][1], Edges[i + 1][1]));
		}
		//最後にチェーンから連結辺リストを作る 計算量O(nlogn)
		chains = chains.OrderBy(x => x.leftChain[0].x).ToList();
		LinkedList.AddRange(chains[0].GetChain(true));
		for (int i = 1; i < chains.Count; i++)
		{
			if (ProximityCheck(LinkedList.Last(), chains[i].LeftChainLast()))
				LinkedList.AddRange(chains[i].GetChain(true));
			else if (ProximityCheck(LinkedList.First(), chains[i].LeftChainLast()))
				LinkedList.InsertRange(0, chains[i].GetChain(true));
		}
		return LinkedList;
	}
	static void Initialize()
	{
		LinkedList.Clear();
		Edges.Clear();
		chains.Clear();
	}
	static bool ProximityCheck(Vector3 a, Vector3 b)
	{
		return Vector3.Distance(a, b) < 0.001f;
	}
	class Chains
	{
		public bool IsChecked = false;
		public Chains(Vector3 startPoint, Vector3 a, Vector3 b)
		{
			leftChain.Add(startPoint);
			rightChain.Add(startPoint);
			leftChain.Add(a);
			rightChain.Add(b);
		}
		public List<Vector3> leftChain, rightChain = new List<Vector3>();
		public bool Add(Vector3 p0, Vector3 p1)
		{
			if (ProximityCheck(leftChain.Last(), p0))
			{
				leftChain.Add(p1);
				return true;
			}
			else if (ProximityCheck(rightChain.Last(), p0))
			{
				rightChain.Add(p1);
				return true;
			}
			else
				return false;
		}
		public Vector3 LeftChainLast() { return leftChain.Last(); }
		public Vector3 RightChainLast() { return rightChain.Last(); }
		public List<Vector3> GetChain(bool IsLeft)
		{
			List<Vector3> l = new List<Vector3>();
			IsChecked = true;
			if (IsLeft)
			{
				l.AddRange(leftChain);
				rightChain.Reverse(1, rightChain.Count - 1);
				l.AddRange(rightChain.GetRange(1, rightChain.Count - 1));
			}
			else
			{
				l.AddRange(rightChain);
				leftChain.Reverse(1, leftChain.Count - 1);
				l.AddRange(leftChain.GetRange(1, leftChain.Count - 1));
			}
			return l;
		}
	}
}