using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.BulletDecals.Scripts.Decals
{
    /// <summary>
    /// Main class for creating Decals 
    /// </summary>
    [RequireComponent(typeof(MeshRenderer))]
    [RequireComponent(typeof(MeshFilter))]
    public class MeshDecal : MonoBehaviour
    {        
        [Tooltip("Skip faces that have inverted normals")]
        public bool SkipOppositeDirectionFaces = true;        

        [Tooltip("Should be set to true to support Normal maps")]
        public bool CalculateTangents = true;

        [Tooltip("Decal texture scale")]
        public float TextureScale = 1f;

        [Tooltip("Decal mesh offset from target object")]
        public float MeshOffset = 0.0f;

        private Transform _targetTransform;        
        private MeshFilter _meshFilterProvider;                                
        private Transform _transform;
        private RectanglePlane[] _planes;
        
        private void Start()
        {
                        
        }

        /// <summary>
        /// Creates decal from terrain
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="position">position on terrain</param>
        public void CreateDecalFromTerrain(Terrain terrain, Vector3 position)
        {
            _targetTransform = terrain.transform;            
            _transform = transform;
            _meshFilterProvider = GetComponent<MeshFilter>();
            CreatePlanesIfNeeded();

            List<int> indices;
            List<Vector3> vertices;
            List<Vector3> normals;
            ExtractMeshInfoFromTerrain(terrain, position, out indices, out vertices, out normals);

            UpdateDecal(vertices.ToArray(), indices.ToArray(), normals.ToArray());
        }

        private void CreatePlanesIfNeeded()
        {
            if (_planes == null)
            {
                CreatePlanes();
            }
        }

        /// <summary>
        /// Creates box planes, required to proper geometry intersection detection
        /// </summary>
        private void CreatePlanes()
        {
            Vector3[] vertices =
            {
                new Vector3(0.5f, -0.5f, 0.5f),
                new Vector3(-0.5f, -0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, 0.5f),
                new Vector3(-0.5f, 0.5f, 0.5f),
                new Vector3(0.5f, 0.5f, -0.5f),
                new Vector3(-0.5f, 0.5f, -0.5f),
                new Vector3(0.5f, -0.5f, -0.5f)
            };

            //box planes
            var plane1 = new RectanglePlane(vertices[0], vertices[1], vertices[2]);
            var plane2 = new RectanglePlane(vertices[0], vertices[2], vertices[4]);
            var plane3 = new RectanglePlane(vertices[4], vertices[5], vertices[6]);
            var plane4 = new RectanglePlane(vertices[5], vertices[3], vertices[1]);
            var plane5 = new RectanglePlane(vertices[2], vertices[3], vertices[5]);
            var plane6 = new RectanglePlane(vertices[0], vertices[1], vertices[6]);
            plane6.InverseInside = true;

            _planes = new[] { plane1, plane2, plane3, plane4, plane5, plane6 };
        }

        /// <summary>
        /// Get vertices, triangles and normals from Terrain
        /// </summary>
        /// <param name="terrain">Terrain</param>
        /// <param name="position">position on terrain</param>
        /// <param name="indices">triangle indices</param>
        /// <param name="vertices">vertices</param>
        /// <param name="normals">normals</param>
        private void ExtractMeshInfoFromTerrain(Terrain terrain, Vector3 position, out List<int> indices, out List<Vector3> vertices, out List<Vector3> normals)
        {
            TerrainData terrainData = terrain.terrainData;

            // Create all lists needed
            indices = new List<int>();
            vertices = new List<Vector3>();
            normals = new List<Vector3>();

            Vector3 heightmapScale = terrainData.heightmapScale;

            // Compute fractions in x and z direction
            float dx = 1f / (terrainData.heightmapResolution - 1);
            float dz = 1f / (terrainData.heightmapResolution - 1);

            var intersectorSize = Mathf.Max(_transform.localScale.x, _transform.localScale.z);
            var positionOnTerrain = position - terrain.transform.position;

            var positionRatioX = positionOnTerrain.x / terrainData.size.x;
            var positionRatioZ = positionOnTerrain.z / terrainData.size.z;

            var sizeRatioX = intersectorSize / terrainData.size.x;
            var sizeRatioZ = intersectorSize / terrainData.size.z;

            var recalculatedSizeX = Mathf.CeilToInt(sizeRatioX * 0.5f * terrainData.heightmapResolution);
            var recalculatedSizeZ = Mathf.CeilToInt(sizeRatioZ * 0.5f * terrainData.heightmapResolution);

            var fromX = (int) (positionRatioX * (terrainData.heightmapResolution - 1)) - recalculatedSizeX;
            var toX = (int) (positionRatioX * (terrainData.heightmapResolution - 1)) + recalculatedSizeX + 2;

            var fromZ = (int) (positionRatioZ * (terrainData.heightmapResolution - 1)) - recalculatedSizeZ;
            var toZ = (int) (positionRatioZ * (terrainData.heightmapResolution - 1)) + recalculatedSizeZ + 2;

            //get data only for target position on terrain
            for (int ix = fromX; ix < terrainData.heightmapResolution && ix < toX; ix++)
            {
                float x = ix * heightmapScale.x;
                float ddx = ix * dx;

                for (int iz = fromZ; iz < terrainData.heightmapResolution && iz < toZ; iz++)
                {
                    float z = iz * heightmapScale.z;
                    float ddz = iz * dz;

                    // Sample height and normal at dx, dz
                    Vector3 point = new Vector3(x, terrainData.GetInterpolatedHeight(ddx, ddz), z);
                    Vector3 normal = terrainData.GetInterpolatedNormal(ddx, ddz);

                    // Add vertex and normal to the lists
                    vertices.Add(point);
                    normals.Add(normal);
                }
            }

            int w = toX - fromX;
            int h = toZ - fromZ;

            // Add triangle pairs (quad)
            for (int xx = 0; xx < w - 1; xx++)
            {
                for (int zz = 0; zz < h - 1; zz++)
                {
                    int a = zz + xx * w;
                    int b = a + w + 1;
                    int c = a + w;
                    int d = a + 1;

                    // Add indices in clockwise order (winding order)
                    indices.Add(a);
                    indices.Add(b);
                    indices.Add(c);

                    indices.Add(a);
                    indices.Add(d);
                    indices.Add(b);
                }
            }
        }

        /// <summary>
        /// Creates Decal on target with MeshFilter
        /// </summary>
        /// <param name="target"></param>
        public void CreateDecal(Transform target)
        {
            _targetTransform = target;
            
            _transform = transform;
            _meshFilterProvider = GetComponent<MeshFilter>();
            CreatePlanesIfNeeded();

            if (_targetTransform.gameObject.isStatic)
            {
                var meshData = _targetTransform.GetComponent<MeshData>();
                if (meshData != null)
                {
                    UpdateDecal(meshData.Vertices, meshData.Triangles, meshData.Normals);
                    return;
                }
            }
            
            var targetMesh = _targetTransform.GetComponent<MeshFilter>().sharedMesh;
            if (targetMesh != null)
            {
                UpdateDecal(targetMesh.vertices, targetMesh.triangles, targetMesh.normals);
            }                                    
        }        

        /// <summary>
        /// Core method to calculate and create Mesh decal
        /// </summary>
        /// <param name="targetVertices">source vertices</param>
        /// <param name="triangles">source triangle indices</param>
        /// <param name="normals">source normals</param>
        private void UpdateDecal(Vector3[] targetVertices, int[] triangles, Vector3[] normals)
        {            
            var localScale = _transform.localScale;
            var localUpDirection = _transform.InverseTransformDirection(_transform.up);

            float minX = -0.5f * localScale.x * TextureScale;
            float maxX = 0.5f * localScale.x * TextureScale;
            float minY = -0.5f * localScale.z * TextureScale;
            float maxY = 0.5f * localScale.z * TextureScale;
            var deltaX = maxX - minX;
            var deltaY = maxY - minY;
                                    
            //matrix for converting target local space to current transform local space
            var matrix = _transform.worldToLocalMatrix * _targetTransform.localToWorldMatrix;            
            //matrix for triangulation and UV mapping
            var matrix2D = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, localScale);
            var offset = localUpDirection * MeshOffset;
            var insideIndicies = new[] { 1, 2, 0 };
            var triangulator = new Triangulator();
            
            var meshList = new List<Mesh>();
            var trianglesCount = triangles.Length / 3;
            for (int i = 0; i < trianglesCount; i++)
            {
                var vertexIndex1 = triangles[i * 3];
                var vertexIndex2 = triangles[i * 3 + 1];
                var vertexIndex3 = triangles[i * 3 + 2];
                //vertex in local space of transform
                var vertex1 = matrix.MultiplyPoint3x4(targetVertices[vertexIndex1]);
                var vertex2 = matrix.MultiplyPoint3x4(targetVertices[vertexIndex2]);
                var vertex3 = matrix.MultiplyPoint3x4(targetVertices[vertexIndex3]);

                bool isTotallyInside = false;
                var isOutside = IsFullyOutside(vertex1, vertex2, vertex3, ref isTotallyInside);
                if (!isOutside)
                {
                    var normal1 = matrix.MultiplyVector(normals[vertexIndex1]);
                    var normal2 = matrix.MultiplyVector(normals[vertexIndex2]);
                    var normal3 = matrix.MultiplyVector(normals[vertexIndex3]);

                    var points = new List<Vector3> {vertex1, vertex2, vertex3};
                    var refNormals = new List<Vector3> {normal1, normal2, normal3};

                    var outputList = isTotallyInside ? points : FindIntersectionPoints(points, ref refNormals, _planes);
                    var outputCount = outputList.Count;

                    if (outputCount == 0)
                    {
                        continue;
                    }

                    if (SkipOppositeDirectionFaces)
                    {
                        var averageNormal = (normal1 + normal2 + normal3) / 3;
                        var dotProduct = Vector3.Dot(averageNormal, localUpDirection);
                        if (dotProduct <= 0.05f)
                        {
                            continue;
                        }    
                    }                                        

                    var vertices2DArray = new Vector2[outputCount];

                    //convert face to identity position and rotation for triangulator and uv-s
                    for (var n = 0; n < outputCount; n++)
                    {
                        var localPoint = outputList[n];
                        //make offset from target surface
                        outputList[n] = offset + localPoint;
                        localPoint = matrix2D * new Vector3(localPoint.x, 0, localPoint.z);
                        vertices2DArray[n] = new Vector2(localPoint.x, localPoint.z);
                    }

                    var indices = isTotallyInside ? insideIndicies : triangulator.Triangulate(vertices2DArray);                    
                    
                    var mesh = new Mesh
                    {
                        vertices = outputList.ToArray(),
                        triangles = indices,                        
                        normals = refNormals.ToArray(),
                        uv = vertices2DArray.Select(o =>
                        {
                            o.x = (o.x - minX) / deltaX;
                            o.y = (o.y - minY) / deltaY;
                            return o;
                        }).ToArray()
                    };

                    meshList.Add(mesh);
                }
            }
            if (meshList.Count > 0)
            {
                var combine = new CombineInstance[meshList.Count];
                for (int k = 0; k < meshList.Count; k++)
                {
                    combine[k].mesh = meshList[k];
                }
                
                var finalMesh = new Mesh();
                finalMesh.CombineMeshes(combine, true, false);
                if (CalculateTangents)
                {
                    CalculateMeshTangents(finalMesh);    
                }                
                    
                _meshFilterProvider.sharedMesh = finalMesh;                
            }
        }        

        /// <summary>
        /// Chech if triangle defined by 3 points is outside of the object bounds
        /// </summary>
        /// <param name="point1">1st point of triangle</param>
        /// <param name="point2">2nd point of triangle</param>
        /// <param name="point3">3rd point of triangle</param>
        /// <param name="isTotallyInside">became true if triangle is inside of the object bounds</param>
        /// <returns>true if triangle is outside of the object bounds</returns>
        private bool IsFullyOutside(Vector3 point1, Vector3 point2, Vector3 point3, ref bool isTotallyInside)
        {
            float halfX = 0.5f;
            float halfY = 0.5f;
            float halfZ = 0.5f;

            var onTheLeftSide = point1.x < -halfX && point2.x < -halfX && point3.x < -halfX;
            var onTheRightSide = point1.x > halfX && point2.x > halfX && point3.x > halfX;
            var onTheTopSide = point1.y > halfY && point2.y > halfY && point3.y > halfY;
            var onTheBottomSide = point1.y < -halfY && point2.y < -halfY && point3.y < -halfY;
            var onTheFrontSide = point1.z < -halfZ && point2.z < -halfZ && point3.z < -halfZ;
            var onTheRearSide = point1.z > halfZ && point2.z > halfZ && point3.z > halfZ;
                                    
            var isOutside = onTheLeftSide ||
                onTheRightSide ||
                onTheTopSide ||
                onTheBottomSide ||
                onTheFrontSide ||
                onTheRearSide;

            if (!isOutside)
            {
                var pointInside1 = (point1.x < halfX && point1.x > -halfX &&
                        point1.y < halfY && point1.y > -halfY &&
                        point1.z < halfZ && point1.z > -halfZ);

                var pointInside2 = (point2.x < halfX && point2.x > -halfX &&
                        point2.y < halfY && point2.y > -halfY &&
                        point2.z < halfZ && point2.z > -halfZ);

                var pointInside3 = (point3.x < halfX && point3.x > -halfX &&
                        point3.y < halfY && point3.y > -halfY &&
                        point3.z < halfZ && point3.z > -halfZ);

                isTotallyInside = pointInside1 && pointInside2 && pointInside3;    
            }                        

            return isOutside;
        }
        
        /// <summary>
        /// Calculates Mesh Tangents for Normal mapping
        /// </summary>
        /// <param name="mesh"></param>
        private void CalculateMeshTangents(Mesh mesh)
        {
            //speed up math by copying the mesh arrays
            var triangles = mesh.triangles;
            var vertices = mesh.vertices;
            var uv = mesh.uv;
            var normals = mesh.normals;

            //variable definitions
            var triangleCount = triangles.Length;
            var vertexCount = vertices.Length;

            var tan1 = new Vector3[vertexCount];
            var tan2 = new Vector3[vertexCount];

            var tangents = new Vector4[vertexCount];

            for (long a = 0; a < triangleCount; a += 3)
            {
                long i1 = triangles[a + 0];
                long i2 = triangles[a + 1];
                long i3 = triangles[a + 2];

                var v1 = vertices[i1];
                var v2 = vertices[i2];
                var v3 = vertices[i3];

                var w1 = uv[i1];
                var w2 = uv[i2];
                var w3 = uv[i3];

                float x1 = v2.x - v1.x;
                float x2 = v3.x - v1.x;
                float y1 = v2.y - v1.y;
                float y2 = v3.y - v1.y;
                float z1 = v2.z - v1.z;
                float z2 = v3.z - v1.z;

                float s1 = w2.x - w1.x;
                float s2 = w3.x - w1.x;
                float t1 = w2.y - w1.y;
                float t2 = w3.y - w1.y;
                
                float div = s1 * t2 - s2 * t1;
                float r = div == 0.0f ? 0.0f : 1.0f / div;

                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                tan1[i1] += sdir;
                tan1[i2] += sdir;
                tan1[i3] += sdir;

                tan2[i1] += tdir;
                tan2[i2] += tdir;
                tan2[i3] += tdir;
            }


            for (long a = 0; a < vertexCount; ++a)
            {
                var n = normals[a];
                var t = tan1[a];

                Vector3.OrthoNormalize(ref n, ref t);
                tangents[a].x = t.x;
                tangents[a].y = t.y;
                tangents[a].z = t.z;

                tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
            }

            mesh.tangents = tangents;
        }

        /// <summary>
        /// Implementation of the Sutherland-Hodgman clipping for intersection points detection
        /// </summary>
        /// <param name="points"></param>
        /// <param name="normals"></param>
        /// <param name="planes"></param>
        /// <returns></returns>
        private List<Vector3> FindIntersectionPoints(List<Vector3> points, ref List<Vector3> normals, RectanglePlane[] planes)
        {
            var outputList = points;

            var outputNormals = normals;

            var planesCount = planes.Length;
            Vector3 startPoint;
            Vector3 endPoint;

            Vector3 startNormal;
            Vector3 endNormal;

            for (var k = 0; k < planesCount; k++)
            {
                var plane = planes[k];
                var inputList = outputList;
                var inputNormals = outputNormals;

                outputList = new List<Vector3>();
                outputNormals = new List<Vector3>();

                var pointsCount = inputList.Count;
                if (pointsCount == 0)
                {
                    continue;
                }

                startPoint = inputList[pointsCount - 1];
                startNormal = inputNormals[pointsCount - 1];

                for (var m = 0; m < pointsCount; m++)
                {
                    endPoint = inputList[m];
                    endNormal = inputNormals[m];

                    if (plane.IsInside(endPoint))
                    {
                        if (!plane.IsInside(startPoint))
                        {
                            outputList.Add(plane.GetIntersection(startPoint, endPoint));
                            outputNormals.Add(plane.GetIntersectionNormal(startPoint, endPoint, startNormal, endNormal));
                        }
                        outputList.Add(endPoint);
                        outputNormals.Add(endNormal);
                    }
                    else if (plane.IsInside(startPoint))
                    {
                        outputList.Add(plane.GetIntersection(startPoint, endPoint));
                        outputNormals.Add(plane.GetIntersectionNormal(startPoint, endPoint, startNormal, endNormal));
                    }

                    startPoint = endPoint;
                    startNormal = endNormal;
                }
            }
            normals = outputNormals;

            return outputList;
        }
        
        /// <summary>
        /// Draw Gizmos in Editor if object is selected
        /// </summary>
        public void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.white;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
        }        

    }
}