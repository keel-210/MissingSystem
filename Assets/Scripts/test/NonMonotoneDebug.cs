using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NonMonotoneDebug : MonoBehaviour
{
	public NonMonotoneTest test;
	List<Vector3> points = new List<Vector3>();
	List<List<Vector3>> monotones = new List<List<Vector3>>();
	void Start()
	{
		points = new List<Vector3>(test.points);
		monotones.Clear();
		if (points.Count >= 3)
			monotones = NonMonotoneTriangulation.GetMonotones(points);
		else
			Debug.Log("Polygon Vertex is less than 3! Check it!");
		Debug.Log(monotones[0].Count);
	}
}
