using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DGrahamTest))]
public class DGrahamTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		DGrahamTest checker = target as DGrahamTest;

		if (GUILayout.Button("Check"))
			checker.Check();
	}
}