using UnityEngine;
using System.Collections.Generic;
public class InternalAngleCheck : MonoBehaviour
{
	public List<Vector3> points = new List<Vector3>();
	float InternalAngle;
	void OnDrawGizmos()
	{
		Gizmos.color = Color.white;
		if (points.Count > 0)
			foreach (Vector3 p in points)
				Gizmos.DrawSphere(p, 0.05f);
		if (points.Count == 3)
		{
			InternalAngle = Vector3.Cross(points[0] - points[1], points[2] - points[1]).x > 0 ? Vector3.Angle(points[0] - points[1], points[2] - points[1]) : 360 - Vector3.Angle(points[0] - points[1], points[2] - points[1]);
			drawString("Angle : " + InternalAngle, points[1] - Vector3.up * 0.05f, Color.white);
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