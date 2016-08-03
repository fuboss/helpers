using System.Collections.Generic;
using UnityEngine;

namespace Assets.Code.Tools
{
    public class MeshCreator : MonoBehaviour
    {
        public Material DebugMaterial;
        public float MeshHeight = 5;
        public List<Vector3> Handles = new List<Vector3>();

        public bool IsValidHandles()
        {
            return Handles.Count >= 3 && CheckConvexity();
        }

        public void ClearMesh()
        {
            //gameObject.RemoveComponent(typeof(MeshCollider));
            //gameObject.RemoveComponent(typeof(MeshFilter));
            //gameObject.RemoveComponent(typeof(MeshRenderer));
        }

        public void CreateMeshCollider()
        {
            MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();

            if (!meshCollider)
                meshCollider = gameObject.AddComponent<MeshCollider>();
            
            if (!meshFilter)
                meshFilter = gameObject.AddComponent<MeshFilter>();
            
            if (!meshRenderer)
                meshRenderer = gameObject.AddComponent<MeshRenderer>();

            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = GenerateMesh();
            meshCollider.convex = true;
            meshFilter.sharedMesh = null;
            meshFilter.sharedMesh = meshCollider.sharedMesh;
            meshRenderer.material = DebugMaterial;
        }

        //http://www.sunshine2k.de/coding/java/Polygon/Convex/polygon.htm
        private bool CheckConvexity()
        {
            float res = 0;
            for (var i = 0; i < Handles.Count; i++)
            {
                var p = Handles[i];
                var tmp = Handles[(i + 1) % Handles.Count];
                var v = new Vector3 {x = tmp.x - p.x, z = tmp.z - p.z};
                var u = Handles[(i + 2) % Handles.Count];

                if (i == 0) // in first loop direction is unknown, so save it in res
                {
                    res = u.x * v.z - u.z * v.x + v.x * p.z - v.z * p.x;
                }
                else
                {
                    float newres = u.x * v.z - u.z * v.x + v.x * p.z - v.z * p.x;
                    if ((newres > 0 && res < 0) || (newres < 0 && res > 0))
                        return false;
                }
            }
            return true;
        }

        private int[] GetTriangles()
        {
            var triangles = new int[Handles.Count * 2 * 3];
            var halfCount = Handles.Count;
            for (var v = 0; v < halfCount; v++)
            {
                var t = v * 6;
                triangles[t] = triangles[t + 3] = v;

                var next = v + 1;
                if (next > halfCount - 1)
                    triangles[t + 2] = 0; 
                else
                    triangles[t + 2] = v + 1;

                next = halfCount + v + 1;
                if (next > halfCount * 2 - 1)
                    triangles[t + 1] = triangles[t + 5] = v + 1;
                else
                    triangles[t + 1] = triangles[t + 5] = halfCount + v + 1;

                triangles[t + 4] = halfCount + v;
          }
            return triangles;
        }

        private Mesh GenerateMesh()
        {
            var mesh = new Mesh();

            var vertices = new List<Vector3>();
            Handles.ForEach(p => vertices.Add(p + new Vector3(0, MeshHeight, 0)));
            vertices.AddRange(Handles);

            mesh.vertices = vertices.ToArray();
            mesh.triangles = GetTriangles();

            //set uv
            Vector2[] uvs = new Vector2[vertices.Count];
            int i = 0;
            while (i < uvs.Length)
            {
                uvs[i] = new Vector2(vertices[i].x, vertices[i].z);
                i++;
            }
            mesh.uv = uvs;

            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        void Awake()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.enabled = false;
        }
    }
}
