using UnityEngine;
public class UsableDiagonal
{
	public UsableDiagonal(int c, Vector2Int e)
	{
		UsedCount = c;
		Diagonal = e;
	}
	public int UsedCount { get; set; }
	public Vector2Int Diagonal { get; set; }
	//確かに何も考えずに使えるけどここまでやるのはバカらしいかも
	public int GetMinIndex() => Mathf.Min(Diagonal.x, Diagonal.y);
	public int GetMaxIndex() => Mathf.Max(Diagonal.x, Diagonal.y);
	public Vector2Int GetInverse() => new Vector2Int(Diagonal.y, Diagonal.x);
}