using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Diagnostics;
using UnityEngine.Rendering;

public class CameraController : MonoBehaviour
{
    [SerializeField]
        private float controlsEnableDelay = 1f;

        private float enabledTime;
        
        [SerializeField]
        private CinemachineVirtualCamera virtualCamera;

        [SerializeField]
        private Collider bounds;

        private CinemachineTransposer virtualCameraTransposer;

        [SerializeField]
        private LayerMask groundLayers = 1 << 11;

        [SerializeField]
        private GameObject target;

        public GameObject Target => target;

        [SerializeField]
        private float targetMovementSpeed = 0.5f;

        [SerializeField, Range(0, 10)]
        private float targetPositionDamping = 4f;
        
        [SerializeField, Range(0, 20)]
        private float targetRotationDamping = 4f;


        [SerializeField]
        private float targetRotationStep = 30f;

        [SerializeField]
        private Vector3 targetPosition;
        
        public Vector3 TargetPosition
        {
            get => targetPosition;
            set => targetPosition = value;
        }
        
        [SerializeField]
        private Vector3 targetOffset;
        
        [SerializeField]
        private Vector3 targetRotation;
        public Vector3 TargetRotation
        {
            get => targetRotation;
            set => targetRotation = value;
        }

        [Header("Limits")]
        [SerializeField]
        private float offsetSpeed = 0.3f;

        [SerializeField]
        private AnimationCurve yOffsetCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private Vector2 xOffsetLimits = new Vector2(3, 10);

        [SerializeField]
        private Vector2 yOffsetLimits = new Vector2(1.5f, 10);

        private bool mousePanning;
        private bool mousePanningPrevious;
        private bool keyboardPanning;
        private bool panningForward;
        private bool panningBackward;
        private bool panningLeft;
        private bool panningRight;
        private float xChange;
        private float yChange;

        private Transform stickyTransform;

        private bool wasStarted = false;

        public bool WasStarted => wasStarted;

        private bool isInputLocked;
        
        private void OnEnable()
        {
            enabledTime = Time.time;
            
            if (!target)
            {
                target = new GameObject();
                target.name = "Combat Camera target";
                target.transform.parent = transform;
            }

            //pawn = MissionManager.Combat.ActivePlayer.CurrentPawn.GetComponent<Pawn>();
            //pawn = MissionManager.AvailablePlayers[0].CurrentPawn.GetComponent<Pawn>();

            //if (MissionManager.CurrentPawn != null) 
            //    stickyTransform = MissionManager.CurrentPawn?.transform;

            if (stickyTransform)
            {
                target.transform.position = stickyTransform.position;
            }    
        
            targetRotation = target.transform.rotation.eulerAngles;
            targetPosition = target.transform.position;
            
            virtualCameraTransposer = virtualCamera.GetCinemachineComponent<CinemachineTransposer>();
            targetOffset = virtualCameraTransposer.m_FollowOffset;
        }

        private void Start()
        {
            wasStarted = true;
        }

        private void Update()
        {
            if (Time.time > enabledTime + controlsEnableDelay)
            {
                ProcessInputs();    
            }
            
            ConstrainPosition();
            SnapToGround();
            UpdateTarget();
        }

        private void ProcessInputs()
        {
            if (isInputLocked)
             return;

            mousePanning = Input.GetMouseButton(2);
            keyboardPanning = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) ||
                              Input.GetKey(KeyCode.D);
            yChange = 0;
            xChange = 0;

            if (Input.GetKeyDown(KeyCode.E))
            {
                targetRotation.y -= Mathf.Abs(targetRotationStep);
            }
            else if (Input.GetKeyDown(KeyCode.Q))
            {
                targetRotation.y += Mathf.Abs(targetRotationStep);
            }

            if (mousePanning)
            {
                stickyTransform = null;
                xChange = Input.GetAxis("Mouse X");
                yChange = Input.GetAxis("Mouse Y");
            }

            if (keyboardPanning)
            {
                stickyTransform = null;
                xChange = GetKeyboardMovementVector().x;
                yChange = GetKeyboardMovementVector().z;
            }

            if (xChange != 0 || yChange != 0)
            {
                targetPosition -= yChange *
                                  targetMovementSpeed *
                                  Vector3.Scale(this.target.transform.forward, new Vector3(1, 0, 1)).normalized;

                targetPosition -= xChange *
                                  targetMovementSpeed * Vector3.Scale(this.target.transform.right, new Vector3(1, 0, 1))
                                      .normalized;
            }

            if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0)
            {
                targetOffset.z = Mathf.Clamp(
                    Mathf.Abs(targetOffset.z) + (Input.GetAxis("Mouse ScrollWheel") * offsetSpeed * -1),
                    xOffsetLimits.x, xOffsetLimits.y);

                targetOffset.y =
                    Mathf.Clamp(
                        yOffsetCurve.Evaluate((targetOffset.z - xOffsetLimits.x) /
                                              (xOffsetLimits.y - xOffsetLimits.x)) *
                        (yOffsetLimits.y - yOffsetLimits.x) + yOffsetLimits.x, yOffsetLimits.x, yOffsetLimits.y);
            }
        }

        private void ConstrainPosition()
        {
            if (!(bounds is BoxCollider boxCollider)) return;

            var origin = boxCollider.transform.position;
            
            targetPosition.x = Mathf.Max(targetPosition.x, origin.x + boxCollider.center.x - boxCollider.size.x * 0.5f);
            targetPosition.x = Mathf.Min(targetPosition.x, origin.x + boxCollider.center.x + boxCollider.size.x * 0.5f);
            targetPosition.y = Mathf.Max(targetPosition.y, origin.y + boxCollider.center.y - boxCollider.size.y * 0.5f);
            targetPosition.y = Mathf.Min(targetPosition.y, origin.y + boxCollider.center.y + boxCollider.size.y * 0.5f);
            targetPosition.z = Mathf.Max(targetPosition.z, origin.z + boxCollider.center.z - boxCollider.size.z * 0.5f);
            targetPosition.z = Mathf.Min(targetPosition.z, origin.z + boxCollider.center.z + boxCollider.size.z * 0.5f);
        }

        private void SnapToGround()
        {
            RaycastHit hit = RaycastFromTop(groundLayers, targetPosition + Vector3.up * 150);

            if (hit.collider)
            {
                targetPosition = hit.point;
            }
        }

        private void UpdateTarget()
        {
            if (stickyTransform)
                targetPosition = stickyTransform.transform.position;

            target.transform.position = Vector3.Lerp(target.transform.position, targetPosition,
                Time.deltaTime * targetPositionDamping);

            //target.transform.position = targetPosition;
            target.transform.rotation = Quaternion.Lerp(target.transform.rotation, Quaternion.Euler(targetRotation), Time.deltaTime * targetRotationDamping);

            targetOffset.z = Mathf.Abs(targetOffset.z) * -1;
            virtualCameraTransposer.m_FollowOffset = Vector3.Lerp(virtualCameraTransposer.m_FollowOffset, targetOffset,
                Time.deltaTime * targetPositionDamping);
        }

        public void SetTarget(Vector3 position, Quaternion rotation)
        {
            targetPosition = position;
            targetRotation = rotation.eulerAngles;
        }

        /*public void SnapToPawn(Pawn targetPawn)
        {
            stickyTransform = targetPawn.transform;
        }*/

        private Vector3 GetKeyboardMovementVector()
        {
            Vector3 output = new Vector3();

            if (Input.GetKey(KeyCode.W))
            {
                output.z -= 1;
            }
            if (Input.GetKey(KeyCode.S))
            {
                output.z += 1;
            }
            if (Input.GetKey(KeyCode.A))
            {
                output.x += 1;
            }
            if (Input.GetKey(KeyCode.D))
            {
                output.x -= 1;
            }

            return output;
        }

        /*public void OnPawnSelected(object value)
        {

            var sender = (value as GameObject)?.GetComponent<Pawn>();

            if (sender)
            {
                switch (MissionManager.GameplayMode)
                {
                    case GameplayMode.Combat when MissionManager.Combat.ActivePlayer == sender.Player:
                    case GameplayMode.Explorer when MissionManager.AvailablePlayers[0] == sender.Player:
                        SnapToPawn(sender);
                        break;
                }
            }
        }*/

        public void SetCameraArea(Collider collider)
        {
            bounds = collider;
            var confiner = virtualCamera.GetComponent<CinemachineConfiner>();
            
            if (confiner)
            {
                confiner.m_BoundingVolume = collider;
            }
            
            confiner.InvalidatePathCache();
        }

        public Collider GetCameraArea()
        {
            return bounds;
        }

        public void SetCameraTarget(Vector3 position, Transform stickTarget = null)
        {
            stickyTransform = stickTarget;
            SetTarget(position, Quaternion.Euler(targetRotation));
        }

        public void SetCameraTarget(Vector3 position, Quaternion rotation, Transform stickTo = null)
        {
            targetRotation = rotation.eulerAngles;
            SetCameraTarget(position, stickTo);
        }

        public void GetCameraTarget(out Vector3 position, out Quaternion rotation)
        {
            position = targetPosition;
            rotation = Quaternion.Euler(targetRotation);
        }

        public void DisableInput()
        {
            isInputLocked = true;
        }

        public void EnableInput()
        {
            isInputLocked = false;
        }

        public bool IsInputEnabled()
        {
            return isInputLocked;
        }

        /*public void UpdateCameraProperties(CameraUpdateObject updateObject)
        {
            if (updateObject.updateStickyTarget)
            {
                stickyTransform = updateObject.stickToTarget;
            }

            if (updateObject.updateTarget)
            {
                var targetPos = updateObject.targetPosition ?? updateObject.target.position;
                SetTarget(targetPos, Quaternion.Euler( updateObject.updateRotation ? updateObject.rotation : targetRotation));
            }

            if (updateObject.updateOffsets)
            {
                targetOffset.z = updateObject.offsets.z;
                targetOffset.y = updateObject.offsets.y;
            }
        }*/
        
        public static RaycastHit RaycastFromTop(LayerMask mask, Vector3 origin)
        {
            RaycastHit hit;

            Ray ray = new Ray(origin, Vector3.down);

            if (Physics.Raycast(ray, out hit, 10000, mask))
            {
                return hit;
            }

            return hit;
        }

        /*public CameraProperties GetCameraProperties()
        {
            var output = new CameraProperties
            {
                TargetPosition = targetPosition,
                TargetRotation = targetRotation,
                TargetOffset = targetOffset,
                StickyTransform = stickyTransform
            };

            return output;
        }*/
    }

