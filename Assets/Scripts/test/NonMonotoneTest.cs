using UnityEngine;
using System.Collections.Generic;
public class NonMonotoneTest : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	List<Vector3> triangles = new List<Vector3>();
	List<List<Vector3>> monotones = new List<List<Vector3>>();
	public void Monotone()
	{
		triangles.Clear();
		monotones.Clear();
		if (points.Count >= 3)
			monotones = NonMonotoneTriangulation.Monotone(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");
	}
	public void Triangulation()
	{
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
			foreach (Vector3 p in points)
				Gizmos.DrawSphere(p, 0.1f);

		if (triangles.Count > 0)
			for (int i = 0; i < triangles.Count; i += 3)
			{
				Gizmos.color = Color.red;
				Gizmos.DrawLine(triangles[i], triangles[i + 1]);
				Gizmos.color = Color.green;
				Gizmos.DrawLine(triangles[i + 1], triangles[i + 2]);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine(triangles[i + 2], triangles[i]);
			}

		Gizmos.color = Color.white;
		if (monotones.Count > 0)
			foreach (List<Vector3> ls in monotones)
				for (int i = 0; i < triangles.Count - 1; i += 2)
					Gizmos.DrawLine(triangles[i], triangles[i + 1]);
	}
}