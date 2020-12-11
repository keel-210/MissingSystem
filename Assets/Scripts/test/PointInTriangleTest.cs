using UnityEngine;
using System.Collections.Generic;
public class PointInTriangleTest : MonoBehaviour
{
	public Vector3 point = Vector3.zero;
	public List<Vector3> triangles = new List<Vector3>();
	public void Check()
	{
		Debug.Log(IsPointInTriangle(point, triangles[0], triangles[1], triangles[2]));
	}
	bool IsPointInTriangle(Vector3 p, Vector3 t0, Vector3 t1, Vector3 t2)
	{
		//同一平面状にある点と三角形の内外判定(サマリーで書いたら？)←めんどい
		Vector3 c0 = Vector3.Cross(t1 - t0, p - t1), c1 = Vector3.Cross(t2 - t1, p - t2), c2 = Vector3.Cross(t0 - t2, p - t0);
		return (Vector3.Dot(c0, c1) > 0) && (Vector3.Dot(c0, c2) > 0);
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		Gizmos.DrawSphere(point, 0.05f);

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