// Original CSG.JS library by Evan Wallace (http://madebyevan.com), under the MIT license.
// GitHub: https://github.com/evanw/csg.js/
// 
// C++ port by Tomasz Dabrowski (http://28byteslater.com), under the MIT license.
// GitHub: https://github.com/dabroz/csgjs-cpp/
//
// C# port by Karl Henkel (parabox.co), under MIT license.
//  
// Constructive Solid Geometry (CSG) is a modeling technique that uses Boolean
// operations like union and intersection to combine 3D solids. This library
// implements CSG operations on meshes elegantly and concisely using BSP trees,
// and is meant to serve as an easily understandable implementation of the
// algorithm. All edge cases involving overlapping coplanar polygons in both
// solids are correctly handled.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Parabox.CSG
{
	/**
	 * Base class for CSG operations.  Contains GameObject level methods for Subtraction, Intersection, and Union operations.
	 * The GameObjects passed to these functions will not be modified.
	 */
	public class CSG
	{
#region Const

		public const float EPSILON = 0.00001f; ///< Tolerance used by `splitPolygon()` to decide if a point is on the plane.
#endregion

		/**
		 * Returns a new mesh by merging @lhs with @rhs.
		 */
		public static Mesh Union(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Union(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}

		/**
		 * Returns a new mesh by subtracting @rhs from @lhs.
		 */
		public static Mesh Subtract(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Subtract(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}

		/**
		 * Return a new mesh by intersecting @lhs with @rhs.  This operation
		 * is non-commutative, so set @lhs and @rhs accordingly.
		 */
		public static Mesh Intersect(GameObject lhs, GameObject rhs)
		{
			CSG_Model csg_model_a = new CSG_Model(lhs);
			CSG_Model csg_model_b = new CSG_Model(rhs);

			CSG_Node a = new CSG_Node( csg_model_a.ToPolygons() );
			CSG_Node b = new CSG_Node( csg_model_b.ToPolygons() );

			List<CSG_Polygon> polygons = CSG_Node.Intersect(a, b).AllPolygons();

			CSG_Model result = new CSG_Model(polygons);

			return result.ToMesh();
		}
	}
}