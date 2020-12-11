using UnityEngine;
using System.Collections.Generic;
public class DelaunayTriangulationTest : MonoBehaviour
{
	public int PointsCount;
	public List<Vector3> points = new List<Vector3>();
	List<Vector3> triangles = new List<Vector3>();
	public void ReplacePoints()
	{
		points = new List<Vector3>();
		for (int i = 0; i < PointsCount; i++)
			points.Add(new Vector3((Random.value - 0.5f) * 2, (Random.value - 0.5f) * 2, 0));
	}
	public void Triangulation()
	{
		triangles.Clear();
		if (points.Count >= 3)
			triangles = DelaunayTriangulation.Triangulate(points);
		Debug.Log(triangles.Count);
	}
	public void DeformationTriangulation()
	{
		triangles.Clear();
		if (points.Count >= 3)
			triangles = DeformationDelaunayTriangulation.Triangulate(points);
		Debug.Log(triangles.Count);
	}
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
			for (int i = 0; i < points.Count; i++)
			{
				Gizmos.DrawSphere(points[i], 0.05f);
				drawString(i + "", points[i] + Vector3.up * 0.05f, Color.white);
			}

		if (triangles.Count > 0)
			for (int i = 0; i < triangles.Count; i += 3)
			{
				Gizmos.DrawLine(triangles[i], triangles[i + 1]);
				Gizmos.DrawLine(triangles[i + 1], triangles[i + 2]);
				Gizmos.DrawLine(triangles[i + 2], triangles[i]);
			}
	}
	void drawString(string text, Vector3 worldPos, Color? colour = null)
	{
		UnityEditor.Handles.BeginGUI();
		if (colour.HasValue) GUI.color = colour.Value;
		var view = UnityEditor.SceneView.currentDrawingSceneView;
		Vector3 screenPos = view.camera.WorldToScreenPoint(worldPos);
		Vector2 size = GUI.skin.label.CalcSize(new GUIContent(text));
		GUI.Label(new Rect(screenPos.x - (size.x / 2), -screenPos.y + view.position.height + 4, size.x, size.y), text);
		UnityEditor.Handles.EndGUI();
	}
}