using UnityEngine;

namespace Assets.BulletDecals.Scripts
{
    public class MeshData : MonoBehaviour
    {
        public Vector3[] Vertices { get; set; }
        public Vector3[] Normals { get; set; }
        public int[] Triangles { get; set; }

        public void Initialize()
        {
            var meshFilter = GetComponentInParent<MeshFilter>() ??
                             GetComponentInChildren<MeshFilter>();

            if (meshFilter != null)
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh != null)
                {
                    Vertices = mesh.vertices;
                    Normals = mesh.normals;
                    Triangles = mesh.triangles;
                }                
            }
        }
    }
}