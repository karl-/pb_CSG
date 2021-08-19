using UnityEngine;
using System.Collections.Generic;

namespace Parabox.CSG
{
    /// <summary>
    /// Represents a plane in 3d space.
    /// <remarks>Does not include position.</remarks>
    /// </summary>
    sealed class Plane
    {
        public Vector3 normal;
        public float w;

        [System.Flags]
        enum EPolygonType
        {
            Coplanar    = 0,
            Front       = 1,
            Back        = 2,
            Spanning    = 3         /// 3 is Front | Back - not a separate entry
        };

        public Plane()
        {
            normal = Vector3.zero;
            w = 0f;
        }

        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            normal = Vector3.Cross(b - a, c - a);//.normalized;
            w = Vector3.Dot(normal, a);
        }

        public override string ToString() => $"{normal} {w}";

        public bool Valid()
        {
            return normal.magnitude > 0f;
        }

        public void Flip()
        {
            normal *= -1f;
            w *= -1f;
        }

        // Split `polygon` by this plane if needed, then put the polygon or polygon
        // fragments in the appropriate lists. Coplanar polygons go into either
        // `coplanarFront` or `coplanarBack` depending on their orientation with
        // respect to this plane. Polygons in front or in back of this plane go into
        // either `front` or `back`.
        public void SplitPolygon(Polygon polygon, List<Polygon> coplanarFront, List<Polygon> coplanarBack, List<Polygon> front, List<Polygon> back)
        {
            // Classify each point as well as the entire polygon into one of the above
            // four classes.
            EPolygonType polygonType = 0;
            List<EPolygonType> types = new List<EPolygonType>();

            for (int i = 0; i < polygon.vertices.Count; i++)
            {
                float t = Vector3.Dot(this.normal, polygon.vertices[i].position) - this.w;
                EPolygonType type = (t < -CSG.epsilon) ? EPolygonType.Back : ((t > CSG.epsilon) ? EPolygonType.Front : EPolygonType.Coplanar);
                polygonType |= type;
                types.Add(type);
            }

            // Put the polygon in the correct list, splitting it when necessary.
            switch (polygonType)
            {
                case EPolygonType.Coplanar:
                {
                    if (Vector3.Dot(this.normal, polygon.plane.normal) > 0)
                        coplanarFront.Add(polygon);
                    else
                        coplanarBack.Add(polygon);
                }
                break;

                case EPolygonType.Front:
                {
                    front.Add(polygon);
                }
                break;

                case EPolygonType.Back:
                {
                    back.Add(polygon);
                }
                break;

                case EPolygonType.Spanning:
                {
                    List<Vertex> f = new List<Vertex>();
                    List<Vertex> b = new List<Vertex>();

                    for (int i = 0; i < polygon.vertices.Count; i++)
                    {
                        int j = (i + 1) % polygon.vertices.Count;

                        EPolygonType ti = types[i], tj = types[j];

                        Vertex vi = polygon.vertices[i], vj = polygon.vertices[j];

                        if (ti != EPolygonType.Back)
                        {
                            f.Add(vi);
                        }

                        if (ti != EPolygonType.Front)
                        {
                            b.Add(vi);
                        }

                        if ((ti | tj) == EPolygonType.Spanning)
                        {
                            float t = (this.w - Vector3.Dot(this.normal, vi.position)) / Vector3.Dot(this.normal, vj.position - vi.position);

                            Vertex v = VertexUtility.Mix(vi, vj, t);

                            f.Add(v);
                            b.Add(v);
                        }
                    }

                    if (f.Count >= 3)
                    {
                        front.Add(new Polygon(f, polygon.material));
                    }

                    if (b.Count >= 3)
                    {
                        back.Add(new Polygon(b, polygon.material));
                    }
                }
                break;
            }   // End switch(polygonType)
        }
    }
}
