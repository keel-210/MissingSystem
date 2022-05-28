using UnityEngine;
using System.Collections.Generic;
public class NonMonotoneTest : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	List<Vector3> triangles = new List<Vector3>();
	List<List<Vector3>> monotones = new List<List<Vector3>>();
	List<Vector2Int> DiagonalsIndex = new List<Vector2Int>();
	List<List<Vector3>> Diagonals = new List<List<Vector3>>();
	public void CheckDiagonal()
	{
		DiagonalsIndex.Clear();
		Diagonals.Clear();
		if (points.Count >= 3)
			DiagonalsIndex = NonMonotoneTriangulation.GetDiagonal(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");
		//対角線インデックスリストを対角線リストに変換する
		foreach (Vector2Int d in DiagonalsIndex)
			Diagonals.Add(new List<Vector3>() { points[d.x], points[d.y] });
	}
	public void Monotone()
	{
		DiagonalsIndex.Clear();
		Diagonals.Clear();
		triangles.Clear();
		monotones.Clear();
		if (points.Count >= 3)
			monotones = NonMonotoneTriangulation.GetMonotones(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");
		foreach (var v in monotones[0])
			Debug.Log("m : " + v);
	}
	public void Triangulation()
	{
		DiagonalsIndex.Clear();
		Diagonals.Clear();
		triangles.Clear();
		monotones.Clear();
		if (points.Count >= 3)
			triangles = NonMonotoneTriangulation.Triangulate(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");

	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
		{
			foreach (Vector3 p in points)
				Gizmos.DrawSphere(p, 0.1f);
			for (int i = 0; i < points.Count; i++)
			{
				if (i == points.Count - 1)
					Gizmos.DrawLine(points[i], points[0]);
				else
					Gizmos.DrawLine(points[i], points[i + 1]);
				UnityEditor.Handles.Label(points[i] + new Vector3(0.05f, -0.05f, 0), "v" + i);
			}
		}

		if (triangles.Count > 0)
			for (int i = 0; i < triangles.Count; i += 3)
			{
				Gizmos.DrawLine(triangles[i], triangles[i + 1]);
				Gizmos.DrawLine(triangles[i + 1], triangles[i + 2]);
				Gizmos.DrawLine(triangles[i + 2], triangles[i]);
			}

		Gizmos.color = Color.red;
		if (Diagonals.Count > 0)
			foreach (List<Vector3> d in Diagonals)
				Gizmos.DrawLine(d[0], d[1]);

		Gizmos.color = Color.blue;
		if (monotones.Count > 0)
			foreach (List<Vector3> ls in monotones)
				for (int i = 0; i < ls.Count; i++)
					if (i == ls.Count - 1)
						Gizmos.DrawLine(ls[i] + Vector3.one * 0.01f, ls[0] + Vector3.one * 0.01f);
					else
						Gizmos.DrawLine(ls[i] + Vector3.one * 0.01f, ls[i + 1] + Vector3.one * 0.01f);


	}
}