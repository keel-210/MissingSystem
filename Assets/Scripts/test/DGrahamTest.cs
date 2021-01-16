using UnityEngine;
using System.Collections.Generic;
public class DGrahamTest : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	List<Vector3> cutPoint = new List<Vector3>();
	List<Vector3> triangles = new List<Vector3>();
	public void Check()
	{
		cutPoint.Clear();
		for (int i = 0; i < points.Count; i++)
		{
			cutPoint.Add(points[i]);
			if (i != 0)
				cutPoint.Add(points[i]);
			if (i == points.Count - 1)
				cutPoint.Add(points[0]);
		}
		triangles.Clear();
		triangles = AngleSortMethod.MakeSingleConnectedEdgeList(cutPoint);
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
			foreach (Vector3 p in points)
				Gizmos.DrawSphere(p, 0.05f);

		if (triangles.Count > 0)
			for (int i = 0; i < triangles.Count; i++)
				Gizmos.DrawLine(triangles[i], triangles[i == triangles.Count - 1 ? 0 : i + 1]);
	}
}