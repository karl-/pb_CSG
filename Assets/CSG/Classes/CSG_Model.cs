using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
	/**
	 * Representation of a mesh in CSG terms.  Contains methods for translating to and from UnityEngine.Mesh.
	 */
	class CSG_Model
	{
		public List<CSG_Vertex> vertices;
		public List<int> indices;

		public CSG_Model()
		{
			this.vertices = new List<CSG_Vertex>();
			this.indices = new List<int>();
		}

		/**
		 * Initialize a CSG_Model with the mesh of a gameObject.
		 */
		public CSG_Model(GameObject go)
		{
			vertices = new List<CSG_Vertex>();

			Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
			Transform trans = go.GetComponent<Transform>();

			int vertexCount = m.vertexCount;
			
			Vector3[] v = m.vertices;
			Vector3[] n = m.normals;
			Vector2[] u = m.uv;
			Color[] c = m.colors;

			for(int i = 0; i < v.Length; i++)
				vertices.Add( new CSG_Vertex(trans.TransformPoint(v[i]), trans.TransformDirection(n[i]), u == null ? Vector2.zero : u[i], c == null || c.Length != vertexCount ? Color.white : c[i]) );

			indices = new List<int>(m.triangles);
		}

		public CSG_Model(List<CSG_Polygon> list)
		{
			this.vertices = new List<CSG_Vertex>();
			this.indices = new List<int>();

			int p = 0;
			for (int i = 0; i < list.Count; i++)
			{
				CSG_Polygon poly = list[i];

				for (int j = 2; j < poly.vertices.Count; j++)
				{
					this.vertices.Add(poly.vertices[0]);		
					this.indices.Add(p++);

					this.vertices.Add(poly.vertices[j - 1]);	
					this.indices.Add(p++);

					this.vertices.Add(poly.vertices[j]);		
					this.indices.Add(p++);
				}
			}
		}

		public List<CSG_Polygon> ToPolygons()
		{
			List<CSG_Polygon> list = new List<CSG_Polygon>();

			for (int i = 0; i < indices.Count; i+= 3)
			{
				List<CSG_Vertex> triangle = new List<CSG_Vertex>()
				{
					vertices[indices[i+0]],
					vertices[indices[i+1]],
					vertices[indices[i+2]]
				};

				list.Add(new CSG_Polygon(triangle));
			}

			return list;
		}

		/**
		 * Converts a CSG_Model to a Unity mesh.
		 */
		public Mesh ToMesh()
		{
			Mesh m = new Mesh();

			int vc = vertices.Count;

			Vector3[] v = new Vector3[vc];
			Vector3[] n = new Vector3[vc];
			Vector2[] u = new Vector2[vc];
			Color[] c = new Color[vc];

			for(int i = 0; i < vc; i++)
			{
				v[i] = this.vertices[i].position;
				n[i] = this.vertices[i].normal;
				u[i] = this.vertices[i].uv;
				c[i] = this.vertices[i].color;
			}

			m.vertices = v;
			m.normals = n;
			m.colors = c;
			m.uv = u;
			m.triangles = this.indices.ToArray();

			return m;
		}
	}
}