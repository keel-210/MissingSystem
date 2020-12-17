using UnityEngine;

public class Vec3IntUtilTest : MonoBehaviour
{
	void Start()
	{
		var v = VecIntUtil.Sorted(new int[] { 43341, 351, 857641 });
		Debug.Log(v[0] + " " + v[1] + " " + v[2]);
	}
}