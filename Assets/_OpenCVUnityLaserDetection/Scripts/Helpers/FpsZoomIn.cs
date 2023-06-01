using UnityEngine;

namespace Assets.BulletDecals.Scripts.Helpers
{
    /// <summary>
    /// When attached to Camera adds ability to zoom in
    /// Click right mouse to Zoom in
    /// </summary>
    public class FpsZoomIn : MonoBehaviour
    {
        public float ZoomInFov = 15;
        private float _defaultFov;
        
        void Start ()
        {
            _defaultFov = Camera.main.fieldOfView;
        }
	        
        void Update () 
        {
            if (Input.GetMouseButtonDown(1))
            {
                Camera.main.fieldOfView = ZoomInFov;
            }
            else if (Input.GetMouseButtonUp(1))
            {
                Camera.main.fieldOfView = _defaultFov;
            }
        }
    }
}
