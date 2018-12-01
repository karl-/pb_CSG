using UnityEngine;

namespace Parabox.CSG
{
    /// <summary>
    /// Represents a single mesh vertex. Contains fields for position, color, normal, and textures.
    /// </summary>
	struct CSG_Vertex
	{
		public Vector3 position;
		public Color color;
		public Vector3 normal;
		public Vector2 uv;

		public CSG_Vertex(Vector3 position, Vector3 normal, Vector2 uv, Color color)
		{
			this.position = position;
			this.normal = normal;
			this.uv = uv;
			this.color = color;
		}

        /// <summary>
        /// Flip the direction of this vertex's normal.
        /// </summary>
		public void Flip()
		{
			normal *= -1f;
		}

        /// <summary>
        /// Create a new vertex between 'a' and 'b' by linearly interpolating all properties by `t`.
        /// Subclasses should override this to interpolate additional properties.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static CSG_Vertex Interpolate(CSG_Vertex a, CSG_Vertex b, float t)
		{
			CSG_Vertex ret = new CSG_Vertex();

			ret.position = Vector3.Lerp(a.position, b.position, t);
			ret.normal = Vector3.Lerp(a.normal, b.normal, t);
			ret.uv = Vector2.Lerp(a.uv, b.uv, t);
			ret.color = (a.color + b.color) / 2f;

			return ret;
		}
	}
}