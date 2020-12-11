using UnityEngine;

public class Vec3IntUtilTest : MonoBehaviour
{
	void Start()
	{
		Debug.Log(VecIntUtil.Sorted(new Vector3Int(0, 1, 4)));
		Debug.Log(VecIntUtil.Sorted(new Vector3Int(0, 0, 4)));
		Debug.Log(VecIntUtil.Sorted(new Vector3Int(0, 0, 0)));
		Debug.Log(VecIntUtil.Sorted(new Vector3Int(50, 654, 2)));
		Debug.Log(VecIntUtil.Sorted(new Vector3Int(84635, 15463, 35453)));
	}
}