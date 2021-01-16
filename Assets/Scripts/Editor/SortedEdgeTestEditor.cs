using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SortedEdgeTest))]
public class SortedEdgeTestEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		SortedEdgeTest checker = target as SortedEdgeTest;

		if (GUILayout.Button("Check"))
			checker.Check();
	}
}