using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TriangulationCheckTest))]
public class TriangulationCheckEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		TriangulationCheckTest checker = target as TriangulationCheckTest;

		if (GUILayout.Button("Triangulation"))
		{
			Debug.ClearDeveloperConsole();
			checker.Triangulation();
			SceneView.RepaintAll();
		}
	}
}