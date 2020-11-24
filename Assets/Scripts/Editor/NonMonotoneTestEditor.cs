using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NonMonotoneTest))]
public class NonMonotoneTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		NonMonotoneTest checker = target as NonMonotoneTest;

		if (GUILayout.Button("Triangulation"))
		{
			Debug.ClearDeveloperConsole();
			checker.Triangulation();
			SceneView.RepaintAll();
		}
	}
}