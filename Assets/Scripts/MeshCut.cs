using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeshCut
{
	private static VertexInfoHolder left_side = new VertexInfoHolder();
	private static VertexInfoHolder right_side = new VertexInfoHolder();

	private static Plane blade;
	private static Mesh victim_mesh;
	private static List<Vector3> new_vertices = new List<Vector3>();

	static void CutInitialize(GameObject victim)
	{
		victim_mesh = victim.GetComponent<MeshFilter>().mesh;
		new_vertices.Clear();
		left_side.ClearAll();
		right_side.ClearAll();
	}
	/// <summary>
	/// （指定された「victim」をカットする。ブレード（平面）とマテリアルから切断を実行する）
	/// </summary>
	/// <param name="victim">Victim.</param>
	/// <param name="blade_plane">Blade plane.</param>
	/// <param name="capMaterial">Cap material.</param>
	public static GameObject[] Cut(GameObject victim, Vector3 anchorPoint, Vector3 normalDirection, Material capMaterial)
	{
		// victimから相対的な平面（ブレード）をセット
		// 具体的には、対象オブジェクトのローカル座標での平面の法線と位置から平面を生成する
		blade = new Plane(
			victim.transform.InverseTransformDirection(-normalDirection),
			victim.transform.InverseTransformPoint(anchorPoint)
		);

		CutInitialize(victim);
		// カット開始
		CutTriangles();
		Fill(new_vertices);
		// メッシュを生成
		Mesh left_HalfMesh = SetMesh("Split Mesh Left");
		Mesh right_HalfMesh = SetMesh("Split Mesh Right");

		// 返すGameObjectの用意
		GameObject leftSideObj = victim;
		GameObject rightSideObj = new GameObject("", typeof(MeshFilter), typeof(MeshRenderer));
		rightSideObj.transform.position = victim.transform.position;
		rightSideObj.transform.rotation = victim.transform.rotation;

		// 設定されているマテリアル配列を取得
		Material[] mats = AddCutFaceMaterial(victim.GetComponent<MeshRenderer>().sharedMaterials, capMaterial);

		leftSideObj.name = "left " + victim.name;
		leftSideObj.GetComponent<MeshFilter>().mesh = left_HalfMesh;
		leftSideObj.GetComponent<MeshRenderer>().materials = mats;

		rightSideObj.name = "right " + victim.name;
		rightSideObj.GetComponent<MeshFilter>().mesh = right_HalfMesh;
		rightSideObj.GetComponent<MeshRenderer>().materials = mats;

		return new GameObject[] { leftSideObj, rightSideObj };
	}
	static Material[] AddCutFaceMaterial(Material[] mats, Material capMaterial)
	{
		if (mats[mats.Length - 1].name != capMaterial.name)
		{
			left_side.subIndices.Add(new List<int>());
			right_side.subIndices.Add(new List<int>());

			// カット面分増やしたマテリアル配列を準備
			Material[] newMats = new Material[mats.Length + 1];
			mats.CopyTo(newMats, 0);
			newMats[mats.Length] = capMaterial;

			return newMats;
		}
		else
			return mats;

	}
	static Mesh SetMesh(string name)
	{
		Mesh HalfMesh = new Mesh();
		HalfMesh.name = name;
		HalfMesh.vertices = left_side.vertices.ToArray();
		HalfMesh.triangles = left_side.triangles.ToArray();
		HalfMesh.normals = left_side.normals.ToArray();
		HalfMesh.uv = left_side.uvs.ToArray();
		HalfMesh.subMeshCount = left_side.subIndices.Count;
		for (int i = 0; i < left_side.subIndices.Count; i++)
			HalfMesh.SetIndices(left_side.subIndices[i].ToArray(), MeshTopology.Triangles, i);

		return HalfMesh;
	}
	/// <summary>
	/// カットを実行する。ただし、実際のメッシュの操作ではなく、あくまで頂点の振り分け、事前準備
	/// </summary>
	/// <param name="submesh">サブメッシュのインデックス</param>
	/// <param name="sides">評価した3頂点の左右情報</param>
	/// <param name="indexes">頂点インデックス</param>
	static void CutTriangle(int submesh, bool[] sides, int[] indexes)
	{
		// 左右それぞれの情報を保持するための配列郡
		Vector3[] leftPoints = new Vector3[2];
		Vector3[] leftNormals = new Vector3[2];
		Vector2[] leftUvs = new Vector2[2];
		Vector3[] rightPoints = new Vector3[2];
		Vector3[] rightNormals = new Vector3[2];
		Vector2[] rightUvs = new Vector2[2];

		bool didsetLeft = false;
		bool didsetRight = false;

		// 3頂点分繰り返す
		// 処理内容としては、左右を判定して、左右の配列に3頂点を振り分ける処理を行っている
		//ここもゴミ
		int p;
		for (int side = 0; side < 3; side++)
		{
			p = indexes[side];
			// sides[side]がtrue、つまり左側の場合
			if (sides[side])
			{
				// すでに左側の頂点が設定されているか（3頂点が左右に振り分けられるため、必ず左右どちらかは2つの頂点を持つことになる）
				if (!didsetLeft)
				{
					didsetLeft = true;

					leftPoints[0] = leftPoints[1] = victim_mesh.vertices[p];
					leftUvs[0] = leftUvs[1] = victim_mesh.uv[p];
					leftNormals[0] = leftNormals[1] = victim_mesh.normals[p];
				}
				else
				{
					// 2頂点目の場合は2番目に直接頂点情報を設定する
					leftPoints[1] = victim_mesh.vertices[p];
					leftUvs[1] = victim_mesh.uv[p];
					leftNormals[1] = victim_mesh.normals[p];
				}
			}
			else
			{
				// 左と同様の操作を右にも行う
				if (!didsetRight)
				{
					didsetRight = true;

					rightPoints[0] = rightPoints[1] = victim_mesh.vertices[p];
					rightUvs[0] = rightUvs[1] = victim_mesh.uv[p];
					rightNormals[0] = rightNormals[1] = victim_mesh.normals[p];
				}
				else
				{
					rightPoints[1] = victim_mesh.vertices[p];
					rightUvs[1] = victim_mesh.uv[p];
					rightNormals[1] = victim_mesh.normals[p];
				}
			}
		}

		float normalizedDistance = 0f;
		float distance = 0f;
		Vector3[] newVertexs = new Vector3[2];
		Vector3[] newNormals = new Vector3[2];
		Vector2[] newUVs = new Vector2[2];
		// ---------------------------
		// 定義した面と交差する点を探す。
		// つまり、平面によって分割される点を探す。
		// 左の点を起点に、右の点に向けたレイを飛ばし、その分割点を探る。
		//左右分で二回回す
		for (int i = 0; i < 2; i++)
		{
			blade.Raycast(new Ray(leftPoints[i], (rightPoints[i] - leftPoints[i]).normalized), out distance);

			// 見つかった交差点を、頂点間の距離で割ることで、分割点の左右の割合を算出する
			normalizedDistance = distance / (rightPoints[i] - leftPoints[i]).magnitude;
			newVertexs[i] = Vector3.Lerp(leftPoints[i], rightPoints[i], normalizedDistance);
			newNormals[i] = Vector3.Lerp(leftNormals[i], rightNormals[i], normalizedDistance);
			newUVs[i] = Vector2.Lerp(leftUvs[i], rightUvs[i], normalizedDistance);
		}

		if (newVertexs[0] == newVertexs[1])
			return;
		ConnectLoop(newVertexs);

		AddNewTriangles(submesh, leftPoints, leftNormals, leftUvs, rightPoints, rightNormals, rightUvs, newVertexs, newNormals, newUVs);
	}
	static void ConnectLoop(Vector3[] newVertexs)
	{
		//一繋ぎのループを形成するため重複を削除しながら追加を行う
		int DuplicateIndex0 = new_vertices.IndexOf(newVertexs[0]);
		int DuplicateIndex1 = new_vertices.IndexOf(newVertexs[1]);
		//4パターン存在 
		//重複無し→Add
		//両方重複→何もしない
		//0番重複→重複したインデックスの後ろに1番追加
		//1番重複→重複したインデックスの前に0番追加
		if (DuplicateIndex0 != -1)
		{
			if (DuplicateIndex1 != -1)
			{
				new_vertices.Add(newVertexs[0]);
				new_vertices.Add(newVertexs[1]);
			}
			else
				new_vertices.Insert(DuplicateIndex0 + 1, newVertexs[1]);
		}
		else
		{
			if (DuplicateIndex1 != -1)
				new_vertices.Insert(DuplicateIndex1 - 1, newVertexs[0]);
		}
	}
	static void AddNewTriangles(int submesh, Vector3[] leftPoints, Vector3[] leftNormals, Vector2[] leftUvs, Vector3[] rightPoints, Vector3[] rightNormals, Vector2[] rightUvs, Vector3[] newVertexs, Vector3[] newNormals, Vector2[] newUVs)
	{
		// 計算された新しい頂点を使って、新トライアングルを左右ともに追加する
		// memo: どう分割されても、左右どちらかは1つの三角形になる気がするけど、縮退三角形的な感じでとにかく2つずつ追加している感じだろうか？
		//↑の懸念は正しい。重複頂点が発生するし意味がないため削除する。
		//ここカス長すぎるしスマートじゃない
		left_side.AddTriangle(
			new Vector3[] { leftPoints[0], newVertexs[0], newVertexs[1] },
			new Vector3[] { leftNormals[0], newNormals[0], newNormals[1] },
			new Vector2[] { leftUvs[0], newUVs[0], newUVs[1] },
			newNormals[0],
			submesh
		);

		if (leftPoints[0] != leftPoints[1])
			left_side.AddTriangle(
				new Vector3[] { leftPoints[0], leftPoints[1], newVertexs[1] },
				new Vector3[] { leftNormals[0], leftNormals[1], newNormals[1] },
				new Vector2[] { leftUvs[0], leftUvs[1], newUVs[1] },
				newNormals[1],
				submesh
			);

		right_side.AddTriangle(
			new Vector3[] { rightPoints[0], newVertexs[0], newVertexs[1] },
			new Vector3[] { rightNormals[0], newNormals[0], newNormals[1] },
			new Vector2[] { rightUvs[0], newUVs[0], newUVs[1] },
			newNormals[0],
			submesh
		);

		if (rightPoints[0] != rightPoints[1])
			right_side.AddTriangle(
				new Vector3[] { rightPoints[0], rightPoints[1], newVertexs[1] },
				new Vector3[] { rightNormals[0], rightNormals[1], newNormals[1] },
				new Vector2[] { rightUvs[0], rightUvs[1], newUVs[1] },
				newNormals[1],
				submesh
			);
	}
	private static List<int> capVertTracker = new List<int>();
	private static List<Vector3> capVertpolygon = new List<Vector3>();

	/// <summary>
	/// カットを実行
	/// </summary>
	static void CutTriangles()
	{
		bool[] sides = new bool[3];
		int[] indices;
		int p1, p2, p3;

		// サブメッシュの数だけループ
		for (int sub = 0; sub < victim_mesh.subMeshCount; sub++)
		{
			// サブメッシュのインデックス数を取得
			indices = victim_mesh.GetIndices(sub);

			// List<List<int>>型のリスト。サブメッシュ一つ分のインデックスリスト
			left_side.subIndices.Add(new List<int>());
			right_side.subIndices.Add(new List<int>());

			// サブメッシュのインデックス数/3分ループ
			for (int i = 0; i < indices.Length; i += 3)
			{
				// トライアングルのインデックスを取得。
				p1 = indices[i + 0];
				p2 = indices[i + 1];
				p3 = indices[i + 2];

				// それぞれ評価中のメッシュの頂点が、カット面の左右どちらにあるかを評価。
				sides[0] = blade.GetSide(victim_mesh.vertices[p1]);
				sides[1] = blade.GetSide(victim_mesh.vertices[p2]);
				sides[2] = blade.GetSide(victim_mesh.vertices[p3]);

				// 頂点0、頂点1、頂点2がどちらも同じ側にある場合はカットしない。そうでない場合カット
				if (sides[0] == sides[1] && sides[0] == sides[2])
					if (sides[0])
						left_side.AddTriangle(victim_mesh, p1, p2, p3, sub);
					else
						right_side.AddTriangle(victim_mesh, p1, p2, p3, sub);
				else
					CutTriangle(sub, sides, new int[] { p1, p2, p3 });
			}
		}
	}

	/// <summary>
	/// カット面を埋める
	/// </summary>
	/// <param name="vertices">ポリゴンを形成する頂点リスト</param>
	static void Fill(List<Vector3> vertices)
	{
		// カット平面の中心点を計算する
		Vector3 center = Vector3.zero;

		// 引数で渡された頂点位置をすべて合計する
		foreach (Vector3 point in vertices)
		{
			center += point;
		}

		// それを頂点数の合計で割り、中心とする
		center = center / vertices.Count;

		// カット平面をベースにしたupward
		Vector3 upward = new Vector3(blade.normal.y, -blade.normal.x, blade.normal.z);

		// 法線と「上方向」から、横軸を算出
		Vector3 left = Vector3.Cross(blade.normal, upward);

		Vector3 displacement = Vector3.zero;
		Vector3 newUV1 = Vector3.zero;
		Vector3 newUV2 = Vector3.zero;

		// 引数で与えられた頂点分ループを回す
		for (int i = 0; i < vertices.Count; i++)
		{
			// 計算で求めた中心点から、各頂点への方向ベクトル
			displacement = vertices[i] - center;

			// 新規生成するポリゴンのUV座標を求める。
			// displacementが中心からのベクトルのため、UV的な中心である0.5をベースに、内積を使ってUVの最終的な位置を得る
			newUV1 = Vector3.zero;
			newUV1.x = 0.5f + Vector3.Dot(displacement, left);
			newUV1.y = 0.5f + Vector3.Dot(displacement, upward);
			newUV1.z = 0.5f + Vector3.Dot(displacement, blade.normal);

			// 次の頂点。ただし、最後の頂点の次は最初の頂点を利用するため、若干トリッキーな指定方法をしている（% vertices.Count）
			displacement = vertices[(i + 1) % vertices.Count] - center;

			newUV2 = Vector3.zero;
			newUV2.x = 0.5f + Vector3.Dot(displacement, left);
			newUV2.y = 0.5f + Vector3.Dot(displacement, upward);
			newUV2.z = 0.5f + Vector3.Dot(displacement, blade.normal);

			// uvs.Add(new Vector2(relativePosition.x, relativePosition.y));
			// normals.Add(blade.normal);

			// 左側のポリゴンとして、求めたUVを利用してトライアングルを追加
			left_side.AddTriangle(
				new Vector3[] { vertices[i], vertices[(i + 1) % vertices.Count], center },
				new Vector3[] { -blade.normal, -blade.normal, -blade.normal },
				new Vector2[] { newUV1, newUV2, new Vector2(0.5f, 0.5f) },
				-blade.normal,
				left_side.subIndices.Count - 1 // カット面。最後のサブメッシュとしてトライアングルを追加
			);

			// 右側のトライアングル。基本は左側と同じだが、法線だけ逆向き。
			right_side.AddTriangle(
				new Vector3[] { vertices[i], vertices[(i + 1) % vertices.Count], center },
				new Vector3[] { blade.normal, blade.normal, blade.normal },
				new Vector2[] { newUV1, newUV2, new Vector2(0.5f, 0.5f) },
				blade.normal,
				right_side.subIndices.Count - 1 // カット面。最後のサブメッシュとしてトライアングルを追加
			);
		}
	}
}