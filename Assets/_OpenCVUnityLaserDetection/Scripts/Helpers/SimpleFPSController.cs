using UnityEngine;

namespace Assets.BulletDecals.Scripts.Helpers
{
    /// <summary>
    /// Simple First Person Controller
    /// </summary>
    public class SimpleFPSController : MonoBehaviour
    {
        public float GravityMultiplier = 2f;
        public float JumpSpeed = 10;
        public float MouseSensivity = 3;
        public float RunSpeed = 10;
        public float StickToGroundForce = 10;
        public float WalkSpeed = 5;

        private CharacterController _characterController;
        private CollisionFlags _collisionFlags;
        private bool _cursorIsLocked = true;
        private Vector2 _input;
        private bool _isJumping;
        private bool _isWalking;
        private bool _jump;
        private Vector3 _moveDir = Vector3.zero;
        private bool _previouslyGrounded;
        
        private void Start()
        {
            _characterController = GetComponent<CharacterController>();
            _isJumping = false;
        }

        private void Update()
        {
            UpdateMouseLook();
            // the jump state needs to read here to make sure it is not missed
            if (!_jump)
            {
                _jump = Input.GetButtonDown("Jump");
            }

            if (!_previouslyGrounded && _characterController.isGrounded)
            {
                _moveDir.y = 0f;
                _isJumping = false;
            }
            if (!_characterController.isGrounded && !_isJumping && _previouslyGrounded)
            {
                _moveDir.y = 0f;
            }

            _previouslyGrounded = _characterController.isGrounded;
        }

        private void UpdateMouseLook()
        {
            transform.Rotate(new Vector3(0, Input.GetAxis("Mouse X") * MouseSensivity, 0), Space.World);
            Camera.main.transform.Rotate(new Vector3(-Input.GetAxis("Mouse Y") * MouseSensivity, 0, 0));

            UpdateCursorLock();
        }

        private void UpdateCursorLock()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                _cursorIsLocked = false;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                _cursorIsLocked = true;
            }

            if (_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else if (!_cursorIsLocked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private void FixedUpdate()
        {
            float speed;
            GetInput(out speed);
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = transform.forward * _input.y + transform.right * _input.x;

            // get a normal for the surface that is being touched to move along it
            RaycastHit hitInfo;
            Physics.SphereCast(transform.position, _characterController.radius, Vector3.down, out hitInfo,
                _characterController.height / 2f, ~0, QueryTriggerInteraction.Ignore);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

            _moveDir.x = desiredMove.x * speed;
            _moveDir.z = desiredMove.z * speed;

            if (_characterController.isGrounded)
            {
                _moveDir.y = -StickToGroundForce;

                if (_jump)
                {
                    _moveDir.y = JumpSpeed;
                    _jump = false;
                    _isJumping = true;
                }
            }
            else
            {
                _moveDir += Physics.gravity * GravityMultiplier * Time.fixedDeltaTime;
            }
            _collisionFlags = _characterController.Move(_moveDir * Time.fixedDeltaTime);
        }

        private void GetInput(out float speed)
        {
            // Read input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            _isWalking = !Input.GetKey(KeyCode.LeftShift);

            // set the desired speed to be walking or running
            speed = _isWalking ? WalkSpeed : RunSpeed;
            _input = new Vector2(horizontal, vertical);

            // normalize input if it exceeds 1 in combined length:
            if (_input.sqrMagnitude > 1)
            {
                _input.Normalize();
            }
        }


        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            Rigidbody body = hit.collider.attachedRigidbody;
            //dont move the rigidbody if the character is on top of it
            if (_collisionFlags == CollisionFlags.Below)
            {
                return;
            }

            if (body == null || body.isKinematic)
            {
                return;
            }
            body.AddForceAtPosition(_characterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
        }
    }
}