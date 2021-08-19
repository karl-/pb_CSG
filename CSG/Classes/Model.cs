using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Parabox.CSG
{
    /// <summary>
    /// Representation of a mesh in CSG terms. Contains methods for translating to and from UnityEngine.Mesh.
    /// </summary>
    public sealed class Model
    {
        List<Vertex> m_Vertices;
        List<Material> m_Materials;
        List<List<int>> m_Indices;

        public List<Material> materials
        {
            get { return m_Materials; }
            set { m_Materials = value; }
        }

        public List<Vertex> vertices
        {
            get { return m_Vertices; }
            set { m_Vertices = value; }
        }

        public List<List<int>> indices
        {
            get { return m_Indices; }
            set { m_Indices = value; }
        }

        public Mesh mesh
        {
            get { return (Mesh)this; }
        }

        public Model(GameObject gameObject) :
            this(gameObject.GetComponent<MeshFilter>()?.sharedMesh,
                gameObject.GetComponent<MeshRenderer>()?.sharedMaterials,
                gameObject.GetComponent<Transform>())
        {
        }

        /// <summary>
        /// Initialize a Model from a UnityEngine.Mesh and transform.
        /// </summary>
        public Model(Mesh mesh, Material[] materials, Transform transform)
        {
            if(mesh == null)
                throw new ArgumentNullException("mesh");

            if(transform == null)
                throw new ArgumentNullException("transform");

            m_Vertices = VertexUtility.GetVertices(mesh).Select(x => transform.TransformVertex(x)).ToList();
            m_Materials = new List<Material>(materials);
            m_Indices = new List<List<int>>();

            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
            {
                if (mesh.GetTopology(i) != MeshTopology.Triangles)
                    continue;
                var indices = new List<int>();
                mesh.GetIndices(indices, i);
                m_Indices.Add(indices);
            }
        }

        internal Model(List<Polygon> polygons)
        {
            m_Vertices = new List<Vertex>();
            Dictionary<Material, List<int>> submeshes = new Dictionary<Material, List<int>>();

            int p = 0;

            for (int i = 0; i < polygons.Count; i++)
            {
                Polygon poly = polygons[i];
                List<int> indices;

                if (!submeshes.TryGetValue(poly.material, out indices))
                    submeshes.Add(poly.material, indices = new List<int>());

                for (int j = 2; j < poly.vertices.Count; j++)
                {
                    m_Vertices.Add(poly.vertices[0]);
                    indices.Add(p++);

                    m_Vertices.Add(poly.vertices[j - 1]);
                    indices.Add(p++);

                    m_Vertices.Add(poly.vertices[j]);
                    indices.Add(p++);
                }
            }

            m_Materials = submeshes.Keys.ToList();
            m_Indices = submeshes.Values.ToList();
        }

        internal List<Polygon> ToPolygons()
        {
            List<Polygon> list = new List<Polygon>();

            for (int s = 0, c = m_Indices.Count; s < c; s++)
            {
                var indices = m_Indices[s];

                for (int i = 0, ic = indices.Count; i < indices.Count; i += 3)
                {
                    List<Vertex> triangle = new List<Vertex>()
                    {
                        m_Vertices[indices[i + 0]],
                        m_Vertices[indices[i + 1]],
                        m_Vertices[indices[i + 2]]
                    };

                    list.Add(new Polygon(triangle, m_Materials[s]));
                }
            }

            return list;
        }

        public static explicit operator Mesh(Model model)
        {
            var mesh = new Mesh();
            VertexUtility.SetMesh(mesh, model.m_Vertices);
            mesh.subMeshCount = model.m_Indices.Count;
            for (int i = 0, c = mesh.subMeshCount; i < c; i++)
            {
#if UNITY_2019_3_OR_NEWER
                mesh.SetIndices(model.m_Indices[i], MeshTopology.Triangles, i);
#else
                mesh.SetIndices(model.m_Indices[i].ToArray(), MeshTopology.Triangles, i);
#endif
            }

            return mesh;
        }
    }
}
