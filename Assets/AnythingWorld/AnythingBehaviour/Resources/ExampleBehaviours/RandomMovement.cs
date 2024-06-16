using AnythingWorld.Animation;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Behaviour
{
    public enum MovementType
    {
        SpawnAroundPoint,
        ManualGoal,
        SpawnAroundModel
    }
    public class RandomMovement : MonoBehaviour
    {
        private const int maxDistance = 100;
        public int JumpDetectorSize = 3;
        [Tooltip("Readonly, position of goal that object is moving towards. Can be regenerated.")]
        public Vector3 goalPosition = Vector3.zero;
        [Tooltip("When model enters this radius they have entered the goal.")]
        public float goalRadius = 0.25f;
        public float speedScalar = 1f;
        public float timeOutRadiusScalar = 0.5f;
        public float timeSinceGoalSet = 0f;

        public float ScaledMaxSpeed => maxSpeed * speedScalar;
        public float ScaledTurnSpeed => turnSpeed * speedScalar;
        //input
        [Header("Speed")]
        public bool scaleMovementToDatabaseSpeed = true;
        public float maxSpeed = 2;
        public float turnSpeed = 2;
        [Header("Thresholds")]
        [Tooltip("Speed above which walk animation is called.")]
        public float walkThreshold = 0.1f;
        [Tooltip("Speed % of max above which run animation is called.")]
        public float runThreshold = 0.7f;
        [Tooltip("Limit movement speed to the maximum threshold active.")]
        public bool clampToActivatedThresholds = false;
        [Header("Animation States Active")]
        public bool walk = true;
        public bool run = true;
        public bool jump = true;

        [Header("Braking")]
        public float brakeStrength = 1;

        public float brakeDist = 2;
        public bool brakeAtDestination = true;
        [Tooltip("Stop translational movement and rotation while continuing animation.")]
        public bool pauseMovement = false;
        public float stopThreshold = 0.1f;

        [Header("Goal Spawning")]
        public MovementType goalGenerationType = MovementType.SpawnAroundModel;

        [Header("Set Manual Goal")]
        public Transform manualGoalTransform;


        [Header("Goal Randomization")]
        [Tooltip("If false, spawn radius will around the model position.")]
        public bool spawnAroundModel = false;


        public float Speed01 => (run ? 1f : walk ? 0.5f : 0f) * variableSpeed / ScaledMaxSpeed;

        public Vector3 spawnAnchor = Vector3.zero;
        public float minimumPositionSpawnRadius = 2;
        public float positionSpawnRadius = 10;
        public bool generateNewPoints = true;

        [Header("Gizmo Settings")]
        public bool showGizmos = true;
        public Color handleColor = Color.white;
        public Color labelColor = Color.white;
        public float gizmoScale = 1f;

       // [HideInInspector]
        public RunWalkIdleController legacyAnimationController;
        public Animator animationController;
        public AnimatorOverrideController animationOverrideController;

        //Debug
        private Vector3 directionToGoal;
        private float variableSpeed;
        private float distanceToGoal;
        private float currentSpeed;
        private bool isJumping;

        [Header("Jump Settings")]
        public float maxJumpHeight = .6f;
        public float maxJumpDistance = 1f;
        public float timeToJumpApex = 0.5f;
        public LayerMask jumpableSurfaces;

        private float gravity;
        private float initialVelocityY;
        private Vector3 targetPosition;
        private Vector3 lastPositionMesured;

        private new Renderer renderer;

        public void Start()
        {
            renderer = GetComponentInChildren<Renderer>();

            if(TryGetComponent<Rigidbody>(out var rb))
            {
                rb.freezeRotation = true;              
            }
            if (scaleMovementToDatabaseSpeed && TryGetComponent<AnythingWorld.Utilities.ModelDataInspector>(out var inspector))
            {

                if (inspector.movement != null && inspector.movement.Count > 0)
                {
                    var averageScale = 0f;
                    foreach (var measurement in inspector.movement)
                    {
                        averageScale += measurement.value;
                    }
                    averageScale /= inspector.scales.Count;
                    speedScalar = averageScale / 50;
                }
                // Calculate gravity and initial Y velocity to reach max jump height
                gravity = (2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
                initialVelocityY = Mathf.Sqrt(2 * gravity * maxJumpHeight);
            }
            if (GetComponentInChildren<LegacyAnimationController>())
            {
                legacyAnimationController = GetComponentInChildren<RunWalkIdleController>();
                legacyAnimationController.crossfadeTime = 0.5f;
            }
            else if (GetComponentInChildren<Animator>())
            {
                animationController = GetComponentInChildren<Animator>();

                run = animationController.runtimeAnimatorController.animationClips.Any(x => x.name == "run") && !animationController.runtimeAnimatorController.animationClips.First(x => x.name == "run").empty;
                walk = animationController.runtimeAnimatorController.animationClips.Any(x => x.name == "walk") && !animationController.runtimeAnimatorController.animationClips.First(x => x.name == "walk").empty;
                jump = animationController.runtimeAnimatorController.animationClips.Any(x => x.name == "jump") && !animationController.runtimeAnimatorController.animationClips.First(x => x.name == "jump").empty;
            }

        }

        public void OnValidate()
        {
            UpdateSpawnAnchor();
        }

        private void FixedUpdate()
        {
            currentSpeed = Vector3.Distance(transform.position,lastPositionMesured)* maxDistance;
            lastPositionMesured = transform.position;
        }


        public void Update()
        {
            variableSpeed = Mathf.Lerp(ScaledMaxSpeed * brakeStrength, variableSpeed, Time.deltaTime);
            if (variableSpeed < stopThreshold) variableSpeed = 0;
            distanceToGoal = Vector3.Distance(goalPosition, transform.position);

            UpdateSpawnAnchor();

            switch (goalGenerationType)
            {
                case MovementType.ManualGoal:
                    //If manual goal
                    if (manualGoalTransform == null)
                    {
                        Debug.LogWarning("Goal transform not set, default to spawning around point.", gameObject);
                        goalGenerationType = MovementType.SpawnAroundPoint;
                        return;
                    }
                    break;
                case MovementType.SpawnAroundPoint:
                    //Spawn around centroid
                    if (distanceToGoal <= goalRadius + (timeSinceGoalSet * timeOutRadiusScalar) && generateNewPoints)
                    {
                        goalPosition = GetRandomPositionInsideSphere(spawnAnchor);
                        timeSinceGoalSet = 0;
                    }
                    break;
                case MovementType.SpawnAroundModel:
                    //If not spawning around point, spawn around model.
                    if (distanceToGoal <= goalRadius + (timeSinceGoalSet * timeOutRadiusScalar) && generateNewPoints)
                    {
                        goalPosition = GetRandomPositionInsideSphere(transform.position);
                        timeSinceGoalSet = 0;
                    }
                    break;
            }

            //Brake when close to target
            if (brakeAtDestination) { brakeStrength = Mathf.Clamp(distanceToGoal - brakeStrength, 0, 1); } else { brakeStrength = 1; };
            //Calculate vector to goal
            directionToGoal = new Vector3(goalPosition.x, transform.position.y, goalPosition.z) - transform.position;

            UpdateAnimationController();
            if (jump)
            {
                CheckIfNeedsToJump();
                FallDistance();
            }
            if (pauseMovement) return;
            TurnTowardsTarget(directionToGoal);
            MoveTowardsTarget();
          
            timeSinceGoalSet += Time.deltaTime;
        }
        //Check if we need to jump over an obstacle in front of us
        private void CheckIfNeedsToJump()
        {
            if (!isJumping && Physics.Raycast(transform.position - new Vector3(0, renderer.bounds.extents.y, 0), transform.forward, out var hit, renderer.bounds.extents.z * JumpDetectorSize))
            {
                Jump();
            }
        }
        //Call the jump animation
        private void Jump()
        {
            if (legacyAnimationController != null)
            {
                legacyAnimationController?.JumpStart();
            }
            else if (animationController != null)
            {
                animationController.SetTrigger("Jump");
            }
            if (DetectSurface(out targetPosition))
            {
                StartCoroutine(JumpArcRoutine(targetPosition));
            }
        }
        //Detect if we are falling
        private void FallDistance()
        {
            float distance = 0;
            bool isFalling = false;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, Vector3.down, out hit, maxDistance))
            {
                distance = hit.distance;
            }
            if (distance > renderer.bounds.extents.y*2)
            {
                isFalling = true;
            }
            else
            {
                isFalling = false;
            }
            if (legacyAnimationController != null)
            {
               legacyAnimationController?.JumpFall(isFalling);

            }
            else if (animationController != null)
            {
                animationController.SetBool("Falling", isFalling);
            }

        }
        //Detect the surface we are jumping to
        private bool DetectSurface(out Vector3 hitPoint)
        {
            Vector3 frontup = transform.position + transform.forward * 2 + transform.up * maxJumpHeight;
            RaycastHit hit;
            if (Physics.Raycast(frontup, Vector3.down, out hit, maxDistance))
            {
                Debug.DrawLine(frontup, hit.point,Color.red,10);
                hitPoint = hit.point;
                return true;
            }
            hitPoint = Vector3.zero;
            return false;
        }
        //Jump over the obstacle in front of us and land on the detected surface
        IEnumerator JumpArcRoutine(Vector3 targetPosition)
        {
            targetPosition += Vector3.up * renderer.bounds.extents.y*2;
            isJumping = true;
            pauseMovement = true;
            float elapsedTime = 0;
            Vector3 startPosition = transform.position;
           
            while (elapsedTime < timeToJumpApex * 2)
            {
                elapsedTime += Time.deltaTime;
                float height = initialVelocityY * elapsedTime - 0.5f * gravity * Mathf.Pow(elapsedTime, 2);
                Vector3 currentPosition = Vector3.Lerp(startPosition, targetPosition, (elapsedTime / (timeToJumpApex * 2)));
                currentPosition.y += height;
                
                transform.position = currentPosition;

                yield return new WaitForFixedUpdate();
            }
            isJumping = false;
            pauseMovement = false;
        }
        //Update the animation controller based on the speed
        private void UpdateAnimationController()
        {
            if (legacyAnimationController != null)
            {
                legacyAnimationController?.BlendAnimationOnSpeed(variableSpeed/ScaledMaxSpeed);

            }
            else if (animationController != null)
            {
                animationController.SetFloat("Speed", Speed01);
            }
        }

        public void UpdateSpawnAnchor()
        {
            switch (goalGenerationType)
            {
                case MovementType.ManualGoal:
                    if (manualGoalTransform == null)
                    {
                        Debug.LogWarning("Goal transform not set, default to spawning around point.", gameObject);
                        goalGenerationType = MovementType.SpawnAroundPoint;
                        return;
                    }
                    spawnAnchor = manualGoalTransform.position;
                    break;
                case MovementType.SpawnAroundPoint:
                    break;
                case MovementType.SpawnAroundModel:
                    spawnAnchor = transform.position;
                    break;
            }
        }
        public void MoveTowardsTarget()
        {
            transform.position = Vector3.Lerp(transform.position, transform.position + (transform.forward), variableSpeed * Time.deltaTime);
        }

        public void TurnTowardsTarget(Vector3 directionToTarget)
        {
            // Turn towards the target
            var normalizedLookDirection = directionToTarget.normalized;
            var m_LookRotation = Quaternion.LookRotation(normalizedLookDirection);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, m_LookRotation, Time.deltaTime * ScaledTurnSpeed);
        }

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!showGizmos) return;
            try
            {
                GUI.color = Color.white;
                Gizmos.color = Color.white;
                var midpoint = (transform.position + goalPosition) * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, directionToGoal);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, transform.forward * distanceToGoal / 2);


                var angle = Vector3.SignedAngle(transform.forward, directionToGoal, Vector3.up);
                var re = Vector3.Cross(transform.forward, directionToGoal);
                UnityEditor.Handles.Label(Vector3.Lerp(transform.position, goalPosition, 0.5f), angle.ToString("F2") + "°");
                UnityEditor.Handles.DrawWireArc(transform.position, transform.up, transform.forward, angle, distanceToGoal * 0.5f);
                GUI.color = Color.white;
            }
            catch
            {

            }
#endif
        }

        private Vector3 GetRandomPositionInsideSphere(Vector3 spawnCentroid)
        {
            var randomPosition = Random.insideUnitSphere.normalized * Random.Range(minimumPositionSpawnRadius, positionSpawnRadius);
            randomPosition = new Vector3(randomPosition.x, 0, randomPosition.z);
            randomPosition = spawnCentroid + randomPosition;
            return randomPosition;

        }
    }
}
