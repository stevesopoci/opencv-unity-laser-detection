using UnityEngine;

namespace Assets.BulletDecals.Scripts.Raycast
{
    /// <summary>
    /// Allows to find intersection point with ray and Mesh
    /// </summary>
    public class RaycastMesh
    {

        /// <summary>
        /// Find intersection point and normal with ray and MeshFilter
        /// </summary>
        /// <param name="ray">Ray in world space</param>
        /// <param name="meshFilter">MeshFilter</param>
        /// <param name="intersectionPoint">returns intersection point</param>
        /// <param name="intersectionNormal">returns intersection point normal</param>
        /// <returns>true if there is intersection</returns>
        public static bool FindIntersectionPoint(Ray ray, MeshFilter meshFilter, out Vector3 intersectionPoint,
            out Vector3 intersectionNormal)
        {
            var mesh = meshFilter.sharedMesh;
            var worldToLocal = meshFilter.transform.worldToLocalMatrix;
            var localToWorld = meshFilter.transform.localToWorldMatrix;

            if (meshFilter.gameObject.isStatic)
            {
                var meshData = meshFilter.GetComponent<MeshData>();
                if (meshData != null)
                {
                    return FindIntersectionPoint(ray, meshData.Vertices, meshData.Triangles, meshData.Normals,
                        worldToLocal, localToWorld, out intersectionPoint, out intersectionNormal);
                }
            }
            
            return FindIntersectionPoint(ray, mesh.vertices, mesh.triangles, mesh.normals, worldToLocal,
                localToWorld, out intersectionPoint, out intersectionNormal);                        
        }

        /// <summary>
        /// Find intersection point and normal with ray and Mesh
        /// </summary>
        /// <param name="ray">Ray in world space</param>        
        /// <param name="vertices">mesh vertices</param>
        /// <param name="triangles">mesh triangles</param>
        /// <param name="normals">mesh normals</param>
        /// <param name="worldToLocal">worldToLocal matrix of the Mesh</param>
        /// <param name="localToWorld">localToWorld matrix of the Mesh</param>
        /// <param name="intersectionPoint">returns intersection point</param>
        /// <param name="intersectionNormal">returns intersection point normal</param>
        /// <param name="useBaricentric">flag for using baricentric method for normals calculation</param>       
        /// <returns>true if there is intersection</returns>
        public static bool FindIntersectionPoint(Ray ray, Vector3[] vertices, int[] triangles, Vector3[] normals, Matrix4x4 worldToLocal, Matrix4x4 localToWorld, out Vector3 intersectionPoint, out Vector3 intersectionNormal, bool useBaricentric = false)
        {        
            intersectionPoint = Vector3.zero;
            intersectionNormal = Vector3.zero;

            var barycentric = Vector3.zero;
            var intersectionPointBuffer = Vector3.zero;

            var minDistance = float.MaxValue;
            var hasIntersection = false;

            //matrix for ray in world space
            var rayMatrix = Matrix4x4.TRS(ray.origin, Quaternion.LookRotation(ray.direction), Vector3.one).inverse;            

            //convert local space of vertices to local space of ray
            var vertexlocalToRayLocalMatrix = rayMatrix * localToWorld;

            var localRayOrigin = rayMatrix.MultiplyPoint3x4(ray.origin);

            var trianglesCount = triangles.Length / 3;
            for (var i = 0; i < trianglesCount; i++)
            {
                var vertexIndex1 = triangles[i * 3];
                var vertexIndex2 = triangles[i * 3 + 1];
                var vertexIndex3 = triangles[i * 3 + 2];

                var vertex1 = vertices[vertexIndex1];
                var vertex2 = vertices[vertexIndex2];
                var vertex3 = vertices[vertexIndex3];

                //get vertex in local space of the ray
                var vertexRayLoc1 = vertexlocalToRayLocalMatrix.MultiplyPoint3x4(vertex1);
                var vertexRayLoc2 = vertexlocalToRayLocalMatrix.MultiplyPoint3x4(vertex2);
                var vertexRayLoc3 = vertexlocalToRayLocalMatrix.MultiplyPoint3x4(vertex3);                

                var isOutside = IsFullyOutside(vertexRayLoc1, vertexRayLoc2, vertexRayLoc3, localRayOrigin);
                if (isOutside)
                {
                    //skip triangles outside the ray
                    continue;                    
                }
                
                vertex1 = localToWorld.MultiplyPoint3x4(vertex1);
                vertex2 = localToWorld.MultiplyPoint3x4(vertex2);
                vertex3 = localToWorld.MultiplyPoint3x4(vertex3);

                var intersectionFlag = FindIntersectionWithTriangle(vertex1, vertex2, vertex3, ray,
                    ref intersectionPointBuffer, ref barycentric);
                if (intersectionFlag == 1)
                {                    
                    var distance = (ray.origin - intersectionPointBuffer).sqrMagnitude;
                    if (distance < minDistance)
                    {
                        minDistance = distance;

                        if (useBaricentric)
                        {
                            var normal1 = localToWorld.MultiplyVector(normals[vertexIndex1]);
                            var normal2 = localToWorld.MultiplyVector(normals[vertexIndex2]);
                            var normal3 = localToWorld.MultiplyVector(normals[vertexIndex3]);

                            intersectionNormal = barycentric.x * normal1 + barycentric.y * normal2 + barycentric.z * normal3;                        
                        }
                        else
                        {
                            var edge1 = vertex2 - vertex1;
                            var edge2 = vertex3 - vertex1;

                            intersectionNormal = Vector3.Cross(edge1, edge2);
                        }

                        //invert normal direction if transform is negative scaled
                        if (localToWorld.determinant < 0)
                        {                            
                            intersectionNormal *= -1;
                        }
                                                
                        intersectionPoint = intersectionPointBuffer;                        
                        
                        hasIntersection = true;
                    }
                }
            }            
            
            return hasIntersection;
        }

        /// <summary>
        /// Check is triangle points are outside of the ray
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="point3"></param>
        /// <param name="rayOrigin"></param>
        /// <returns></returns>
        private static bool IsFullyOutside(Vector3 point1, Vector3 point2, Vector3 point3, Vector3 rayOrigin)
        {
            float rayX = rayOrigin.x;
            float rayY = rayOrigin.y;

            var onTheLeftSide = point1.x < rayX && point2.x < rayX && point3.x < rayX;
            if (onTheLeftSide)
            {
                return true;
            }
            var onTheRightSide = point1.x > rayX && point2.x > rayX && point3.x > rayX;
            if (onTheRightSide)
            {
                return true;
            }
            var onTheTopSide = point1.y > rayY && point2.y > rayY && point3.y > rayY;
            if (onTheTopSide)
            {
                return true;
            }
            var onTheBottomSide = point1.y < rayY && point2.y < rayY && point3.y < rayY;
            if (onTheBottomSide)
            {
                return true;
            }
            return false;
        }        

        /// <summary>
        /// Find intersection with triangle
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        /// <param name="ray"></param>
        /// <param name="intersectionPoint"></param>
        /// <param name="barycentric"></param>
        /// <returns>1 when there is intersection, -1 when triangle is broken, 0 - no intersection</returns>
        private static int FindIntersectionWithTriangle(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3, Ray ray, ref Vector3 intersectionPoint, ref Vector3 barycentric)
        {
            var u = vertex2 - vertex1;
            var v = vertex3 - vertex1;

            var edgesNormal = Vector3.Cross(u, v);

            if (edgesNormal == Vector3.zero)
            {
                return -1;//broken triangle
            }

            var a = Vector3.Dot(edgesNormal, vertex1 - ray.origin);
            var b = Vector3.Dot(edgesNormal, ray.direction);
            
            var alpha = a / b;

            if (alpha < 0)
            {
                return 0;//ray goes away from triangle, no intersect
            }
            intersectionPoint = ray.origin + ray.direction * alpha;

            var uu = Vector3.Dot(u, u);
            var uv = Vector3.Dot(u, v);
            var vv = Vector3.Dot(v, v);
            var w = intersectionPoint - vertex1;
            var wu = Vector3.Dot(w, u);
            var wv = Vector3.Dot(w, v);
            var d = uv * uv - uu * vv;
            var s = (uv * wv - vv * wu) / d;
            if (s < 0 || s > 1.0f)
            {
                return 0;
            }
            var t = (uv * wu - uu * wv) / d;
            if (t < 0 || (s + t) > 1.0f)
            {
                return 0;
            }

            barycentric.y = s;
            barycentric.z = t;
            barycentric.x = 1.0f - s - t;

            return 1;            
        }       

    }
}