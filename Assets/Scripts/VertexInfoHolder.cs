using UnityEngine;
using System.Collections.Generic;

public class VertexInfoHolder
{
	public List<Vector3> vertices = new List<Vector3>();
	public List<Vector3> normals = new List<Vector3>();
	public List<Vector2> uvs = new List<Vector2>();
	public List<int> triangles = new List<int>();
	public List<List<int>> subIndices = new List<List<int>>();

	public void ClearAll()
	{
		vertices.Clear();
		normals.Clear();
		uvs.Clear();
		triangles.Clear();
		subIndices.Clear();
	}

	/// <summary>
	/// トライアングルとして3頂点を追加
	/// ※ 頂点情報は元のメッシュからコピーする
	/// </summary>
	/// <param name="p1">頂点1</param>
	/// <param name="p2">頂点2</param>
	/// <param name="p3">頂点3</param>
	/// <param name="submesh">対象のサブメシュ</param>
	public void AddTriangle(Mesh victim_mesh, int p1, int p2, int p3, int submesh)
	{
		// triangle index order goes 1,2,3,4....

		// 頂点配列のカウント。随時追加されていくため、ベースとなるindexを定義する。
		// ※ AddTriangleが呼ばれるたびに頂点数は増えていく。
		int base_index = vertices.Count;

		// 対象サブメッシュのインデックスに追加していく
		subIndices[submesh].Add(base_index + 0);
		subIndices[submesh].Add(base_index + 1);
		subIndices[submesh].Add(base_index + 2);

		// 三角形郡の頂点を設定
		triangles.Add(base_index + 0);
		triangles.Add(base_index + 1);
		triangles.Add(base_index + 2);

		// 対象オブジェクトの頂点配列から頂点情報を取得し設定する
		vertices.Add(victim_mesh.vertices[p1]);
		vertices.Add(victim_mesh.vertices[p2]);
		vertices.Add(victim_mesh.vertices[p3]);

		// 同様に、対象オブジェクトの法線配列から法線を取得し設定する
		normals.Add(victim_mesh.normals[p1]);
		normals.Add(victim_mesh.normals[p2]);
		normals.Add(victim_mesh.normals[p3]);

		// 同様に、UVも。
		uvs.Add(victim_mesh.uv[p1]);
		uvs.Add(victim_mesh.uv[p2]);
		uvs.Add(victim_mesh.uv[p3]);
	}

	/// <summary>
	/// トライアングルを追加する
	/// ※ オーバーロードしている他メソッドとは異なり、引数の値で頂点（ポリゴン）を追加する
	/// </summary>
	/// <param name="points3">トライアングルを形成する3頂点</param>
	/// <param name="normals3">3頂点の法線</param>
	/// <param name="uvs3">3頂点のUV</param>
	/// <param name="faceNormal">ポリゴンの法線</param>
	/// <param name="submesh">サブメッシュID</param>
	public void AddTriangle(Vector3[] points3, Vector3[] normals3, Vector2[] uvs3, Vector3 faceNormal, int submesh)
	{
		// 引数の3頂点から法線を計算
		Vector3 calculated_normal = Vector3.Cross((points3[1] - points3[0]).normalized, (points3[2] - points3[0]).normalized);

		int p1 = 0;
		int p2 = 1;
		int p3 = 2;

		// 引数で指定された法線と逆だった場合はインデックスの順番を逆順にする（つまり面を裏返す）
		if (Vector3.Dot(calculated_normal, faceNormal) < 0)
		{
			p1 = 2;
			p2 = 1;
			p3 = 0;
		}

		int base_index = vertices.Count;

		subIndices[submesh].Add(base_index + 0);
		subIndices[submesh].Add(base_index + 1);
		subIndices[submesh].Add(base_index + 2);

		triangles.Add(base_index + 0);
		triangles.Add(base_index + 1);
		triangles.Add(base_index + 2);

		vertices.Add(points3[p1]);
		vertices.Add(points3[p2]);
		vertices.Add(points3[p3]);

		normals.Add(normals3[p1]);
		normals.Add(normals3[p2]);
		normals.Add(normals3[p3]);

		uvs.Add(uvs3[p1]);
		uvs.Add(uvs3[p2]);
		uvs.Add(uvs3[p3]);
	}

}