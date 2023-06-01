using UnityEngine;

namespace Assets.BulletDecals.Scripts.Decals
{
    /// <summary>
    /// Class for defining Decal intersector box side
    /// </summary>
    public class RectanglePlane
    {
        public bool InverseInside;
                               
        private readonly Vector3 _vertex1;
        private readonly Vector3 _edgesNormal;

        private float _prevAlpha;

        /// <summary>
        /// Constructor that takes 3 vertices from the box side
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        public RectanglePlane(Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
        {
            _vertex1 = vertex1;

            var edge1 = vertex2 - _vertex1;
            var edge2 = vertex3 - _vertex1;

            _edgesNormal = Vector3.Cross(edge1, edge2);
        }        

        /// <summary>
        /// Get proportion of the distance from points to box side
        /// </summary>
        /// <param name="worldPoint1"></param>
        /// <param name="worldPoint2"></param>
        /// <returns></returns>
        public float GetAlpha(Vector3 worldPoint1, Vector3 worldPoint2)
        {            
            var distance1 = Vector3.Dot(_edgesNormal, _vertex1 - worldPoint1);
            var distance2 = Vector3.Dot(_edgesNormal, _vertex1 - worldPoint2);
            var alpha = distance1 / (distance1 - distance2);
            return alpha;
        }

        /// <summary>
        /// Get intersection point on the box side
        /// </summary>
        /// <param name="worldPoint1"></param>
        /// <param name="worldPoint2"></param>
        /// <returns></returns>
        public Vector3 GetIntersection(Vector3 worldPoint1, Vector3 worldPoint2)
        {
            _prevAlpha = GetAlpha(worldPoint1, worldPoint2);            
            var intersectionPoint = Vector3.Lerp(worldPoint1, worldPoint2, _prevAlpha);                        
            return intersectionPoint;
        }

        /// <summary>
        /// Get intersection normal
        /// </summary>
        /// <param name="worldPoint1"></param>
        /// <param name="worldPoint2"></param>
        /// <param name="normal1"></param>
        /// <param name="normal2"></param>
        /// <param name="usePrevAlpha"></param>
        /// <returns></returns>
        public Vector3 GetIntersectionNormal(Vector3 worldPoint1, Vector3 worldPoint2, Vector3 normal1, Vector3 normal2, bool usePrevAlpha = true)
        {
            var alpha = usePrevAlpha ? _prevAlpha : GetAlpha(worldPoint1, worldPoint2);
            return Vector3.Lerp(normal1, normal2, alpha);            
        }

        /// <summary>
        /// Check is point inside box
        /// </summary>
        /// <param name="worldPoint"></param>
        /// <returns></returns>
        public bool IsInside(Vector3 worldPoint)
        {                        
            var distance = Vector3.Dot(_edgesNormal, _vertex1 - worldPoint);

            return InverseInside ? distance > 0f : distance < 0f;            
        }
    }
}
