using System.Linq;
using UnityEngine;

namespace Assets.Code.Tools
{
    public static class MeshExtensions  
    {
        public static Mesh ProjectWithRefMesh(this Mesh mesh, Mesh referenceMesh, Matrix4x4 trs, float offset = 0, float yForUnprojected = 0, float projectDepth = 5, bool debug = false)
        {
            var vertices = mesh.vertices;
            var referenceVertices = referenceMesh.vertices;
            var inverseTRS = trs.inverse;
            for (int i = 0; i < vertices.Length; i++)
            {
                Ray r = new Ray(trs.MultiplyPoint3x4(referenceVertices[i]), Vector3.down);
                RaycastHit hit;

                if (Physics.Raycast(r, out hit, projectDepth, Layers.Ground))
                {
                    if(debug)
                        Debug.DrawLine(r.origin, hit.point, Color.green);
                    vertices[i] = inverseTRS.MultiplyPoint3x4(hit.point + new Vector3(0, offset, 0));
                    if (debug)
                        Debug.DrawLine(trs.MultiplyPoint3x4(referenceVertices[i]), trs.MultiplyPoint3x4(vertices[i]), Color.blue);
                }
                else
                {
                    if(yForUnprojected != 0)
                    { 
                        var yPos = inverseTRS.MultiplyPoint3x4(new Vector3(0, yForUnprojected + offset, 0)).y;
                        vertices[i] = new Vector3(referenceVertices[i].x, 0, referenceVertices[i].z) + new Vector3(0, yPos, 0);
                    }
                    else
                    {
                        vertices[i] = referenceVertices[i] + new Vector3(0, offset, 0);
                    }
                    if (debug)
                        Debug.DrawRay(trs.MultiplyPoint3x4(referenceVertices[i]), Vector3.down * projectDepth, Color.red);
                }
            }
            mesh.vertices = vertices;
            return mesh;
        }


        public static Vector3[] GetColliderVertices(Collider collider)
        {
            if (collider is BoxCollider)
            {
                var vertices = new Vector3[4];
                var thisMatrix = collider.transform.localToWorldMatrix;
                var storedRotation = collider.transform.rotation;
                collider.transform.rotation = Quaternion.identity;

                var extents = collider.bounds.extents;
                vertices[0] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, extents.z));
                vertices[1] = thisMatrix.MultiplyPoint3x4(new Vector3(-extents.x, -extents.y, extents.z));
                vertices[2] = thisMatrix.MultiplyPoint3x4(new Vector3(extents.x, -extents.y, -extents.z));
                vertices[3] = thisMatrix.MultiplyPoint3x4(-extents);

                collider.transform.rotation = storedRotation;
                return vertices;
            }

            if (collider is MeshCollider)
            {
                
                var meshCreator = collider.GetComponent<MeshCreator>();
                return meshCreator.Handles.Select(v => meshCreator.transform.TransformPoint(v)).ToArray();
            }

            return null;
        }
    }
}
