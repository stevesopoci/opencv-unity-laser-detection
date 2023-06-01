using System.Linq;
using Assets.BulletDecals.Scripts.Decals;
using Assets.BulletDecals.Scripts.Raycast;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.BulletDecals.Scripts.Bullets
{
    /// <summary>
    /// Creates decal on object with collider that in front of the bullet
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [Tooltip("Allow Random Rotation of bullet marks")]
        public bool RandomRotation = true; 

        [Tooltip("LayerMask for Raycast")]
        public LayerMask HitLayerMask;

        [Tooltip("Distance for Raycast")]
        public float MaxDistance = Mathf.Infinity;

        [Tooltip("Trigger interaction for Raycast")]
        public QueryTriggerInteraction TriggerInteraction = QueryTriggerInteraction.UseGlobal;

        [Tooltip("Force applied to hit object")]
        public float Force = 0.0f;

        [Tooltip("Scale of the bullet mark")]
        public float Scale = 1.0f;

        /// <summary>
        /// Settings for bullet marks
        /// </summary>
        public BulletMarksSettings BulletMarksSettings { get; set; }

        private bool _isShooting;

        private Vector3 laserCoordinates;

        [SerializeField]
        private Camera cameraToShootFrom;

        [SerializeField]
        private Text coordinateX;

        [SerializeField]
        private Text coordinateY;

        public void Shoot()
        {
            _isShooting = true;
        }        

        private void FixedUpdate()
        {
            //do shooting inside FixedUpdate to make sure that all transforms are up to date
            if (_isShooting)
            {
                _isShooting = false;

#if UNITY_IOS
                PluginManager.resetDetectedLaser();

                laserCoordinates = new Vector3(PluginManager.getCoordinateX(), PluginManager.getCoordinateY(), 0);
#elif UNITY_EDITOR

                laserCoordinates = Input.mousePosition;
#endif

                Ray ray = cameraToShootFrom.ScreenPointToRay(laserCoordinates);

                coordinateX.text = "X: " + laserCoordinates.x.ToString();
                coordinateY.text = "Y: " + laserCoordinates.y.ToString();

                RaycastHit hitInfo;     
                //find intersection with any colliders in front of the bullet
                if (Physics.Raycast(ray, out hitInfo, MaxDistance, HitLayerMask.value, TriggerInteraction))
                {
                    if (hitInfo.collider is TerrainCollider)
                    {                        
                        SearchForIntersectionWithTerrain(hitInfo);
                    }
                    else
                    {
                        SearchForIntersectionWithMesh(hitInfo, ray, true);    
                    }                    
                }                               
            }
        }

        /// <summary>
        /// Searches for intersection with terrain and creates decal on it
        /// </summary>
        /// <param name="hitInfo"></param>
        private void SearchForIntersectionWithTerrain(RaycastHit hitInfo)
        {
            var terrain = hitInfo.transform.GetComponent<Terrain>();
            if (terrain != null)
            {
                CreateDecalOnTerrain(terrain, hitInfo.point, hitInfo.normal);
            }
        }

        /// <summary>
        /// Searches for intersection with object that has mesh on it
        /// </summary>
        /// <param name="hitInfo"></param>
        /// <param name="ray"></param>
        /// <param name="recursiveIntersection"></param>
        private void SearchForIntersectionWithMesh(RaycastHit hitInfo, Ray ray, bool recursiveIntersection)
        {
            var targetTransform = hitInfo.transform;

            var meshFilter = targetTransform.GetComponentInParent<MeshFilter>() ??
                             targetTransform.GetComponentInChildren<MeshFilter>();
            
            ApplyForce(targetTransform, ray.direction, hitInfo.point);

            CreateDecal(meshFilter, ray, recursiveIntersection);
        }

        /// <summary>
        /// Applies Force to target if it has rigidbody, so it will be moved by bullet
        /// </summary>
        /// <param name="target"></param>
        /// <param name="forceDirection"></param>
        /// <param name="forcePosition"></param>
        private void ApplyForce(Transform target, Vector3 forceDirection, Vector3 forcePosition)
        {
            if (Force > 0)
            {
                var targetRigidbody = target.GetComponent<Rigidbody>();
                if (targetRigidbody != null)
                {
                    var force = forceDirection * Force;
                    targetRigidbody.AddForceAtPosition(force, forcePosition);
                }
            }
        }

        /// <summary>
        /// Creates bullet hole decal on terrain
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="intersectionPoint"></param>
        /// <param name="intersectionNormal"></param>
        private void CreateDecalOnTerrain(Terrain terrain, Vector3 intersectionPoint, Vector3 intersectionNormal)
        {
            var targetTransform = terrain.transform;

            var bulletMark = PrepareBulletMark(intersectionPoint, intersectionNormal, targetTransform);
            if (bulletMark == null)
            {
                return;
            }

            //create geometry
            var meshDecal = bulletMark.GetComponent<MeshDecal>();
            meshDecal.CreateDecalFromTerrain(terrain, intersectionPoint);

            //attach to target object
            bulletMark.parent = targetTransform;  
        }

        /// <summary>
        /// Initialize bullet mark to create decal on target surface
        /// </summary>
        /// <param name="intersectionPoint"></param>
        /// <param name="intersectionNormal"></param>
        /// <param name="targetTransform"></param>
        /// <returns></returns>
        private Transform PrepareBulletMark(Vector3 intersectionPoint, Vector3 intersectionNormal, Transform targetTransform)
        {
            var objectTag = targetTransform.gameObject.tag;

            var bulletMark = BulletMarksSettings.GetBulletMarkByTag(objectTag, intersectionPoint,
                Quaternion.FromToRotation(Vector3.up, intersectionNormal));
            if (bulletMark == null)
            {
                return null;
            }

            //do random rotation on target surface
            if (RandomRotation)
            {
                bulletMark.Rotate(Vector3.up, Random.Range(0f, 360f));
            }

            bulletMark.localScale *= Scale;

            //create impact effect if available
            BulletMarksSettings.GetImpactEffectByTag(objectTag, intersectionPoint, bulletMark.rotation);

            return bulletMark;
        }        

        /// <summary>
        /// Creates decal on gameobject with MeshFilter
        /// </summary>
        /// <param name="meshFilter"></param>
        /// <param name="ray"></param>
        /// <param name="recursiveIntersection"></param>
        private void CreateDecal(MeshFilter meshFilter, Ray ray, bool recursiveIntersection)
        {
            if (meshFilter != null)
            {
                var targetTransform = meshFilter.transform;

                Vector3 intersectionPoint;
                Vector3 intersectionNormal;

                //try to find intersection point of the ray on mesh
                if (RaycastMesh.FindIntersectionPoint(ray, meshFilter, out intersectionPoint,
                    out intersectionNormal))
                {
                    var bulletMark = PrepareBulletMark(intersectionPoint, intersectionNormal, targetTransform);
                    if (bulletMark == null)
                    {
                        return;
                    }
                    //create geometry
                    var meshDecal = bulletMark.GetComponent<MeshDecal>();
                    meshDecal.CreateDecal(targetTransform);

                    //attach to target object
                    bulletMark.parent = targetTransform;                    
                }
                else if (recursiveIntersection)
                {                                        
                    //find other objects behind current collider to find intersection with one of them
                    RaycastHit[] hits = Physics.RaycastAll(ray, MaxDistance, HitLayerMask.value, TriggerInteraction);
                    if (hits.Length > 1)
                    {
                        var hitInfo = hits.OrderBy(p => p.distance).ElementAt(1);

                        if (hitInfo.collider is TerrainCollider)
                        {
                            SearchForIntersectionWithTerrain(hitInfo);
                        }
                        else
                        {
                            SearchForIntersectionWithMesh(hitInfo, ray, false);
                        }                                              
                    }
                }                
            }
        }
       

    }
}