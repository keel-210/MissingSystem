using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DelaunayTriangulationTest))]
public class DelaunayTriangulationTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		DelaunayTriangulationTest checker = target as DelaunayTriangulationTest;

		if (GUILayout.Button("Triangulation"))
		{
			checker.Triangulation();
			SceneView.RepaintAll();
		}
		if (GUILayout.Button("Replace Points"))
			checker.ReplacePoints();
		if (GUILayout.Button("Rect Points"))
			checker.RectPoints();
	}
}