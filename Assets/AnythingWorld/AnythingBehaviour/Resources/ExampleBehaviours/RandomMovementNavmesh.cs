using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using AnythingWorld.Animation;
namespace AnythingWorld.Behaviour
{
    public class RandomMovementNavmesh : MonoBehaviour
    {
        #region Fields
        public float turnSpeed;
        public float moveSpeed;
        public float m_Range = 25.0f;
        public float rest = 0;
        public NavMeshAgent m_Agent;
        public RunWalkIdleController controller;
        #endregion
        private float timeSet;
        void Start()
        {
            timeSet = Time.time;
        }

        void Update()
        {
            SolveMovement();
        }

        private void SolveMovement()
        {
            if (m_Agent != null)
            {
                var elapsed = Time.time - timeSet;
                controller.walkThreshold = 0.01f;
                controller.runThreshold = 1.5f;
                controller.BlendAnimationOnSpeed(m_Agent.velocity.magnitude);

                if (m_Agent.isActiveAndEnabled && m_Agent.isOnNavMesh)
                {
                    if (m_Agent.pathPending || m_Agent.remainingDistance > m_Agent.radius && elapsed < 10f)
                        return;
                    if (rest > Time.time)
                        return;

                    float num = Random.Range(2, 10);
                    rest = Time.time + num;
                }
                else
                {
                    return;
                }

                var randomDir = Random.insideUnitSphere * m_Range;
                randomDir += transform.position;
                NavMeshHit hit;
                NavMesh.SamplePosition(randomDir, out hit, m_Range, 1);
                var finalPos = hit.position;
                m_Agent.destination = finalPos;

                //m_Agent.destination =  m_Agent.gameObject.transform.position * (m_Range * Random.insideUnitCircle);
                timeSet = Time.time;
            }
        }
    }
}
