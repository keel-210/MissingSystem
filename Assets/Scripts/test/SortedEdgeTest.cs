using UnityEngine;
using System.Collections.Generic;
using System.Linq;
public class SortedEdgeTest : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	List<List<Vector3>> Edges = new List<List<Vector3>>();

	[SerializeField, ReadOnlyInInspector] List<Vector3> Verts = new List<Vector3>();
	[SerializeField, ReadOnlyInInspector] public List<Vector3> LinkedList = new List<Vector3>();
	public void Check()
	{
		Edges.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			Edges.Add(new List<Vector3> { points[i], points[i == points.Count - 1 ? 0 : i + 1] });
		}
		System.Random rnd = new System.Random();
		Edges = Edges.OrderBy(i => rnd.Next()).ToList();
		Verts.Clear();
		foreach (var e in Edges)
		{
			Verts.Add(e[0]);
			Verts.Add(e[1]);
		}
		LinkedList.Clear();
		LinkedList = SortedEdgeLinkedList.MakeLinkedList(Verts);
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
			foreach (Vector3 p in points)
				Gizmos.DrawSphere(p, 0.05f);

		if (LinkedList.Count > 0)
			for (int i = 0; i < LinkedList.Count; i++)
				Gizmos.DrawLine(LinkedList[i], LinkedList[i == LinkedList.Count - 1 ? 0 : i + 1]);
	}
}