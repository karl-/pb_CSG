using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
	/**
	 * Represents a polygon face with an arbitrary number of vertices.
	 */
	class CSG_Polygon
	{
		public List<CSG_Vertex> vertices;
		public CSG_Plane plane;

		public CSG_Polygon(List<CSG_Vertex> list)
		{
			this.vertices = list;
			this.plane = new CSG_Plane(list[0].position, list[1].position, list[2].position);
		}

		public void Flip()
		{
			this.vertices.Reverse();

			for(int i = 0; i < vertices.Count; i++)
				vertices[i].Flip();

			plane.Flip();
		}

		public override string ToString()
		{
			// return System.String.Format("V: {0}, {1}, {2}\nN: ({3}, {4}, {5})", 
			// 	new object[] {
			// 		vertices[0].position.ToString(),
			// 		vertices[1].position.ToString(),
			// 		vertices[2].position.ToString(),
			// 		plane.normal.x,
			// 		plane.normal.y,
			// 		plane.normal.z 
			// 	});
			return "N: " + plane.normal;
		}
	}
}