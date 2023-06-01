using UnityEngine;

namespace Assets.BulletDecals.Scripts.Bullets
{
    /// <summary>
    /// Base implementation of the Gun to allow player to shoot objects in front of it
    /// Can play shoot sounds if AudioSource is attached
    /// </summary>
    [RequireComponent(typeof(Bullet))]
    public class Gun : MonoBehaviour
    {
        /// <summary>
        /// Shooting reload time interval in Seconds
        /// </summary>
        public float ReloadIntervalSec = 0.1f;
        
        /// <summary>
        /// Bullet marks and impact effects settings
        /// </summary>
        public BulletMarksSettings BulletMarksSettings;

        private float _currentReloadTime;
        private AudioSource _audioSource;        
        private Bullet _bullet;

        private bool didDetectLaser;

        private void Start()
        {
            if (BulletMarksSettings == null)
            {
                Debug.LogError("BulletMarksSettings is not set for Gun game object");
            }

            _bullet = GetComponent<Bullet>();
            _bullet.BulletMarksSettings = BulletMarksSettings;
            
            _audioSource = GetComponent<AudioSource>();
        }
        
        private void Update()
        {

#if UNITY_IOS
            didDetectLaser = PluginManager.didDetectLaser();
#elif UNITY_EDITOR 
            if (Input.GetMouseButtonDown(0))
            {
                didDetectLaser = true;
            }
            else
            {
                didDetectLaser = false;
            }
#endif

            if (didDetectLaser)
            {
                _currentReloadTime = 0;
            }

            if (didDetectLaser)
            {
                if (_currentReloadTime <= 0)
                {
                    Shoot();
                    _currentReloadTime = ReloadIntervalSec;
                }

            }

            if (_currentReloadTime > 0)
            {
                _currentReloadTime -= Time.deltaTime;
            }
        }
        

        private void Shoot()
        {                                                    
            if (_bullet != null)
            {
                _bullet.Shoot();
                
                if (_audioSource != null)
                {
                    _audioSource.Play();
                }
            }
            
        }
    }
}