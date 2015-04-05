using UnityEngine;
using System.Collections;
using Parabox.CSG;

/**
 * Simple demo of CSG operations.
 */
public class Demo : MonoBehaviour
{
	GameObject left, right, composite;
	bool wireframe = false;

	public Material wireframeMaterial = null;

	public GameObject[] fodder;	// prefabs containing two mesh children
	int index = 0;				// the index of example mesh prefabs

	enum BoolOp
	{
		Union,
		SubtractLR,
		SubtractRL,
		Intersect
	};

	void Awake()
	{
		Reset();

		wireframeMaterial.SetFloat("_Opacity", 0);
		cur_alpha = 0f;
		dest_alpha = 0f;
		
		ToggleWireframe();
	}

	/**
	 * Reset the scene to it's original state.
	 */
	public void Reset()
	{
		if(composite) GameObject.Destroy(composite);
		if(left) GameObject.Destroy(left);
		if(right) GameObject.Destroy(right);

		GameObject go = GameObject.Instantiate(fodder[index]);

		left = GameObject.Instantiate( go.transform.GetChild(0).gameObject );
		right = GameObject.Instantiate( go.transform.GetChild(1).gameObject );

		GameObject.Destroy(go);

		wireframeMaterial = left.GetComponent<MeshRenderer>().sharedMaterial;

		GenerateBarycentric(left);
		GenerateBarycentric(right);
	}

	public void Union()
	{
		Reset();
		Boolean( BoolOp.Union );
	}

	public void SubtractionLR()
	{
		Reset();
		Boolean( BoolOp.SubtractLR );
	}

	public void SubtractionRL()
	{
		Reset();
		Boolean( BoolOp.SubtractRL );
	}

	public void Intersection()
	{
		Reset();
		Boolean( BoolOp.Intersect );
	}

	void Boolean(BoolOp operation)
	{
		Mesh m;

		/**
		 * All boolean operations accept two gameobjects and return a new mesh.
		 * Order matters - left, right vs. right, left will yield different
		 * results in some cases.
		 */
		switch(operation)
		{
			case BoolOp.Union:
				m = CSG.Union(left, right);
				break;

			case BoolOp.SubtractLR:
				m = CSG.Subtract(left, right);
				break;

			case BoolOp.SubtractRL:
				m = CSG.Subtract(right, left);
				break;

			case BoolOp.Intersect:
			default:
				m = CSG.Intersect(right,left);
				break;
		}

		composite = new GameObject();
		composite.AddComponent<MeshFilter>().sharedMesh = m;
		composite.AddComponent<MeshRenderer>().sharedMaterial = left.GetComponent<MeshRenderer>().sharedMaterial;

		GenerateBarycentric( composite );

		GameObject.Destroy(left);
		GameObject.Destroy(right);
	}

	/**
	 * Turn the wireframe overlay on or off.
	 */
	public void ToggleWireframe()
	{
		wireframe = !wireframe;

		cur_alpha = wireframe ? 0f : 1f;
		dest_alpha = wireframe ? 1f : 0f;
		start_time = Time.time;
	}

	/**
	 * Swap the current example meshes
	 */
	public void ToggleExampleMeshes()
	{
		index++;
		if(index > fodder.Length-1)	index = 0;

		Reset();
	}

	float wireframe_alpha = 0f, cur_alpha = 0f, dest_alpha = 1f, start_time = 0f;

	void Update()
	{
		wireframe_alpha = Mathf.Lerp(cur_alpha, dest_alpha, Time.time - start_time);
		wireframeMaterial.SetFloat("_Opacity", wireframe_alpha);
	}

	/**
	 * Rebuild mesh with individual triangles, adding barycentric coordinates
	 * in the colors channel.  Not the most ideal wireframe implementation,
	 * but it works and didn't take an inordinate amount of time :)
	 */
	void GenerateBarycentric(GameObject go)
	{
		Mesh m = go.GetComponent<MeshFilter>().sharedMesh;

		if(m == null) return;

		int[] tris = m.triangles;
		int triangleCount = tris.Length;

		Vector3[] mesh_vertices		= m.vertices;
		Vector3[] mesh_normals		= m.normals;
		Vector2[] mesh_uv			= m.uv;

		Vector3[] vertices 	= new Vector3[triangleCount];
		Vector3[] normals 	= new Vector3[triangleCount];
		Vector2[] uv 		= new Vector2[triangleCount];
		Color[] colors 		= new Color[triangleCount];

		for(int i = 0; i < triangleCount; i++)
		{
			vertices[i] = mesh_vertices[tris[i]];
			normals[i] 	= mesh_normals[tris[i]];
			uv[i] 		= mesh_uv[tris[i]];

			colors[i] = i % 3 == 0 ? new Color(1, 0, 0, 0) : (i % 3) == 1 ? new Color(0, 1, 0, 0) : new Color(0, 0, 1, 0);

			tris[i] = i;
		}

		Mesh wireframeMesh = new Mesh();

		wireframeMesh.Clear();
		wireframeMesh.vertices = vertices;
		wireframeMesh.triangles = tris;
		wireframeMesh.normals = normals;
		wireframeMesh.colors = colors;
		wireframeMesh.uv = uv;

		go.GetComponent<MeshFilter>().sharedMesh = wireframeMesh;
	}
}
