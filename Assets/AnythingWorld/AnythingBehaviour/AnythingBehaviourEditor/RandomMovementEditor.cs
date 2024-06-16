using UnityEditor;
using UnityEngine;

namespace AnythingWorld.Behaviour.Editor
{

    [CustomEditor(typeof(RandomMovement))]
    public class RandomMovementEditor : UnityEditor.Editor
    {
        public void OnSceneGUI()
        {
            
            var controller = (RandomMovement)target;
            controller.UpdateSpawnAnchor();

            if (controller.showGizmos)
            {
                if (controller.goalGenerationType == MovementType.ManualGoal && controller.manualGoalTransform != null)
                {
                    Handles.color = controller.handleColor;
                    Handles.DrawWireDisc(controller.manualGoalTransform.position, Vector3.up, controller.goalRadius);
                    GUI.color = controller.labelColor;
                    Handles.color = controller.labelColor;
                    Handles.Label(controller.manualGoalTransform.position + (Vector3.left * controller.goalRadius), new GUIContent("Goal"));
                }
                else
                {
                    //Allow user to position goal radius using position handle
                    if (controller.goalGenerationType == MovementType.SpawnAroundPoint)
                    {
                        Handles.color = controller.handleColor;
                        Handles.SphereHandleCap(0, controller.spawnAnchor, Quaternion.identity, 0.2f*controller.gizmoScale, EventType.Repaint);
                        controller.spawnAnchor = Handles.PositionHandle(controller.spawnAnchor, Quaternion.identity);
                    }
                   


                    // Position of radius label.
                    Vector3 radiusLabelPos = new Vector3(controller.spawnAnchor.x + controller.positionSpawnRadius, controller.spawnAnchor.y, controller.spawnAnchor.z);
                    Vector3 radiusLabelVec = (controller.spawnAnchor - radiusLabelPos).normalized;
                    EditorGUI.BeginChangeCheck();
                    Vector3 radiusAdjustment = Handles.Slider(radiusLabelPos, radiusLabelVec, 0.2f*controller.gizmoScale, Handles.SphereHandleCap, 0.01f);
                    if (EditorGUI.EndChangeCheck())
                    {
                        controller.positionSpawnRadius = radiusAdjustment.x - controller.spawnAnchor.x;
                    }
               


                    //Draw spawn radius
                    Handles.color = controller.handleColor;
                    Handles.DrawWireDisc(controller.spawnAnchor, Vector3.up, controller.positionSpawnRadius);
                    //Draw goal position
                    Handles.DrawWireDisc(controller.goalPosition, Vector3.up, controller.goalRadius + (controller.timeSinceGoalSet * controller.timeOutRadiusScalar));

                    GUI.color = controller.labelColor;
                    Handles.color = controller.labelColor;
                    //Spawn radius label
                    Handles.Label(controller.spawnAnchor + (Vector3.left * (controller.positionSpawnRadius) + Vector3.left), "Spawn Radius");
                    //Goal radius label
                    Handles.Label(controller.goalPosition+ (Vector3.left * (controller.goalRadius + (controller.timeSinceGoalSet * controller.timeOutRadiusScalar))), new GUIContent("Goal"));
                }
              

            }

        }

 
    }
}

