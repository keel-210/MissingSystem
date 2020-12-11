using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PointInTriangleTest))]
public class TriangleTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		PointInTriangleTest checker = target as PointInTriangleTest;

		if (GUILayout.Button("Check"))
			checker.Check();
	}
}