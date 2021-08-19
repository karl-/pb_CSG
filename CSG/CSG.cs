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
using System.Collections.Generic;

namespace Parabox.CSG
{
    /// <summary>
    /// Base class for CSG operations. Contains GameObject level methods for Subtraction, Intersection, and Union
    /// operations. The GameObjects passed to these functions will not be modified.
    /// </summary>
    public static class CSG
    {
        public enum BooleanOp
        {
            Intersection,
            Union,
            Subtraction
        }

        const float k_DefaultEpsilon = 0.00001f;
        static float s_Epsilon = k_DefaultEpsilon;

        /// <summary>
        /// Tolerance used by <see cref="Plane.SplitPolygon"/> determine whether planes are coincident.
        /// </summary>
        public static float epsilon
        {
            get => s_Epsilon;
            set => s_Epsilon = value;
        }

        /// <summary>
        /// Performs a boolean operation on two GameObjects.
        /// </summary>
        /// <returns>A new mesh.</returns>
        public static Model Perform(BooleanOp op, GameObject lhs, GameObject rhs)
        {
            switch (op)
            {
                case BooleanOp.Intersection:
                    return Intersect(lhs, rhs);
                case BooleanOp.Union:
                    return Union(lhs, rhs);
                case BooleanOp.Subtraction:
                    return Subtract(lhs, rhs);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns a new mesh by merging @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Union(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);
        
            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());
        
            List<Polygon> polygons = Node.Union(a, b).AllPolygons();
        
            return new Model(polygons);
        }
        
        /// <summary>
        /// Returns a new mesh by subtracting @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Subtract(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);
        
            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());
        
            List<Polygon> polygons = Node.Subtract(a, b).AllPolygons();
        
            return new Model(polygons);
        }

        /// <summary>
        /// Returns a new mesh by intersecting @lhs with @rhs.
        /// </summary>
        /// <param name="lhs">The base mesh of the boolean operation.</param>
        /// <param name="rhs">The input mesh of the boolean operation.</param>
        /// <returns>A new mesh if the operation succeeds, or null if an error occurs.</returns>
        public static Model Intersect(GameObject lhs, GameObject rhs)
        {
            Model csg_model_a = new Model(lhs);
            Model csg_model_b = new Model(rhs);

            Node a = new Node(csg_model_a.ToPolygons());
            Node b = new Node(csg_model_b.ToPolygons());

            List<Polygon> polygons = Node.Intersect(a, b).AllPolygons();

            return new Model(polygons);
        }
    }
}
