using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class AngleSortMethod
{
	//xy平面上の点群が前提(z=0)
	static List<Vector3> CEL = new List<Vector3>();
	public static List<Vector3> MakeSingleConnectedEdgeList(List<Vector3> points)
	{
		Init();
		//重心を求める
		Vector3 ave = Vector3.zero;
		foreach (var v in points)
			ave += v;
		ave = ave / points.Count;

		var l = points.OrderBy(x => GetAngle(x, Vector3.up, ave)).ToList();
		for (int i = 0; i < l.Count; i += 2)
			CEL.Add(l[i]);
		return CEL;
	}
	static void Init()
	{
		CEL.Clear();
	}
	static float GetAngle(Vector3 a, Vector3 b, Vector3 o)
	{
		return Vector3.Cross(a - o, b - o).z > 0 ? Vector3.Angle(a - o, b - o) : 360 - Vector3.Angle(a - o, b - o);
	}
}