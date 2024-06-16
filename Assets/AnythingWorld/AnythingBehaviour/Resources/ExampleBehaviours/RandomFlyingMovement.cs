using AnythingWorld.Animation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AnythingWorld.Behaviour
{
    public class RandomFlyingMovement : MonoBehaviour
    {
        //Position variables
        private Vector3 goalPosition;
        private Vector3 directionToGoal;
        //Debug
        public float brakingVariable = 1;
        private float variableSpeed;
        public Vector3 rotationDiff;
        private float distanceToGoal;
        //input
        [Header("Speed")]
        public float speed = 20;
        public float turnSpeed = 2;
        [Header("Braking")]
        public bool generateNewPoints = true;
        public bool brakeAtDestination = true;
        public float brakeDist = 2;
        [Header("Goal Randomization")]
        public float positionSpawnRadius = 100;
        public float goalRadius = 10;

        public FlyingVehicleAnimator animationController;
        public void Start()
        {
            if (GetComponentInChildren<FlyingVehicleAnimator>())
            {
                animationController = GetComponentInChildren<FlyingVehicleAnimator>();
            }
        }

        public void Update()
        {
            variableSpeed = speed * brakingVariable;
            goalPosition = new Vector3(goalPosition.x, transform.position.y, goalPosition.z);
            distanceToGoal = Vector3.Distance(goalPosition, transform.position);
            if (distanceToGoal < goalRadius)
            {
                if (generateNewPoints) goalPosition = GetRandomPositionInsideSphere();
            }
            //Brake when close to target
            if (brakeAtDestination) { brakingVariable = Mathf.Clamp(distanceToGoal - brakingVariable, 0, 1); } else { brakingVariable = 1; };
            //Calculate vector to goal
            directionToGoal = goalPosition - transform.position;


            if (variableSpeed > 0.1)
            {
                animationController?.Accelerate();
            }
            else
            {
                animationController?.Deceleration();
            }

            //Blend animation
            MoveTowardsTarget();
            TurnTowardsTarget(directionToGoal);
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
            transform.rotation = Quaternion.Slerp(transform.rotation, m_LookRotation, Time.deltaTime * turnSpeed);
        }
        private Vector3 direction;
        public bool showGizmos;

        private void OnDrawGizmosSelected()
        {
#if UNITY_EDITOR
            if (!showGizmos) return;
            try
            {
                GUI.color = Color.white;
                UnityEditor.Handles.Label(goalPosition + (Vector3.left * goalRadius), new GUIContent("Goal"));
                Gizmos.color = Color.white;
                var midpoint = (transform.position + goalPosition) * 0.5f;
                Gizmos.color = Color.green;
                Gizmos.DrawRay(transform.position, directionToGoal);
                Gizmos.color = Color.red;
                Gizmos.DrawRay(transform.position, transform.forward * distanceToGoal / 2);

                UnityEditor.Handles.DrawWireDisc(goalPosition, Vector3.up, goalRadius);
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.right, positionSpawnRadius);
                UnityEditor.Handles.DrawWireDisc(transform.position, Vector3.up, positionSpawnRadius);
                UnityEditor.Handles.Label(transform.position + (Vector3.left * (positionSpawnRadius) + Vector3.left), "Spawn Radius");
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

        private Vector3 GetRandomPositionInsideSphere()
        {
            var randomPosition = Random.insideUnitSphere * positionSpawnRadius;
            randomPosition = new Vector3(randomPosition.x,  0, randomPosition.z);
            randomPosition = transform.position + randomPosition;
            return randomPosition;

        }
    }
}
