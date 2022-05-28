using UnityEngine;
using System.Collections.Generic;
public class TriangulationCheckTest : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	List<Vector3> triangles = new List<Vector3>();
	public void Triangulation()
	{
		triangles.Clear();
		if (points.Count >= 3)
			triangles = MonotoneTriangulation.Triangulate(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");

	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
		{
			for (int i = 0; i < points.Count; i++)
			{
				Gizmos.DrawSphere(points[i], 0.05f);
				UnityEditor.Handles.Label(points[i] + new Vector3(0.05f, -0.05f, 0), "v" + i);
			}
		}

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
	}
}