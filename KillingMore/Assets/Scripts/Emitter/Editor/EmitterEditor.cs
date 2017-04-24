using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Graphs;

namespace Emitter
{
    public class EmitterEditor : Editor
    {
        protected virtual void DrawBullet()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12><b>Bullet Settings</b></size>", mEnableRichTextStyle);
            EditorGUILayout.Space();
            var ProjectilePrefabTooltip = new GUIContent("Bullet Prefab", "子弹prefab.");
            var ProjectilePrefabProp = serializedObject.FindProperty("ProjectilePrefab");
            if (ProjectilePrefabProp.objectReferenceValue == null)
            {
                GUI.color = Color.white;
                EditorGUILayout.HelpBox("Choose a suitable prefab to use for bullet fired.", MessageType.Info,
                    true);
                GUI.color = Color.white;
            }
            EditorGUILayout.PropertyField(ProjectilePrefabProp, ProjectilePrefabTooltip);

            var emitterPointTooltip = new GUIContent("EmitterPoint", "子弹发射点，默认为发射器位置.");
            var emitterPointProp = serializedObject.FindProperty("EmitterPoint");
            EditorGUILayout.PropertyField(emitterPointProp, emitterPointTooltip);
            if (emitterPointProp.serializedObject == null && emitterPointProp.serializedObject.targetObject == null) { }
            var initialVelocTooltip = new GUIContent("Initial Velocity", "子弹初始速度.");
            var accTooltip = new GUIContent("Accelerate Velocity", "子弹运行的加速度.");
            var accDurationTooltip = new GUIContent("Accelerate Duration", "加速持续时间.");
            var initialVelocProp = serializedObject.FindProperty("InitialVeloc");
            var accProp = serializedObject.FindProperty("Acc");
            var accDurationProp = serializedObject.FindProperty("AccDuration");
            EditorGUILayout.PropertyField(initialVelocProp, initialVelocTooltip);
            EditorGUILayout.PropertyField(accProp, accTooltip);
            EditorGUILayout.PropertyField(accDurationProp, accDurationTooltip);

            var penetrateTooltip = new GUIContent("Penetrate", "更新序列间隔，如果大于0则按顺序更新子发射器，否则则为同步.");
            var positionRandomnessTooltip = new GUIContent("Position Randomness", "子弹随机位置.");
            var PenetrateProp = serializedObject.FindProperty("Penetrate");
            var positionRandomnessProp = serializedObject.FindProperty("PositionRandomness");
            EditorGUILayout.PropertyField(PenetrateProp, penetrateTooltip);
            EditorGUILayout.Slider(positionRandomnessProp, 0f, 5f, positionRandomnessTooltip);
        }

        protected virtual void DrawEmitter()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12><b>Emitter Settings</b></size>", mEnableRichTextStyle);
            EditorGUILayout.Space();
            var emitDelayTooltip = new GUIContent("Emit Delay", "每个运行周期的延迟时间.");
            var cooldownTooltip = new GUIContent("Cooldown", "每个运行周期之间的冷却间隔，大于0时有效，否则将只有一个运行周期.");
            var emitDelayProp = serializedObject.FindProperty("Delay");
            var cooldownProp = serializedObject.FindProperty("Cooldown");
            EditorGUILayout.PropertyField(emitDelayProp, emitDelayTooltip);
            EditorGUILayout.PropertyField(cooldownProp, cooldownTooltip);

            var rotatingSpeedTooltip = new GUIContent("Rotating Speed", "发射器自身的旋转速度（角度）.");
            var revertAngleTooltip = new GUIContent("Revert Angle", "逆向阀值（角度）.");
            var linearRotatingTooltip = new GUIContent("Linear Rotating", "是否使用恒定速度旋转.");
            var orientateTooltip = new GUIContent("Orientate", "是否使用发射器定位瞄准.");
            var rotatingSpeedProp = serializedObject.FindProperty("RotatingSpeed");
            var revertAngleProp = serializedObject.FindProperty("RevertAngle");
            var linearRotatingProp = serializedObject.FindProperty("LinearRotating");
            var orientateProp = serializedObject.FindProperty("Orientate");
            EditorGUI.BeginDisabledGroup(orientateProp.boolValue);
            EditorGUILayout.PropertyField(rotatingSpeedProp, rotatingSpeedTooltip);
            EditorGUILayout.PropertyField(revertAngleProp, revertAngleTooltip);
            EditorGUILayout.PropertyField(linearRotatingProp, linearRotatingTooltip);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.PropertyField(orientateProp, orientateTooltip);
            //---Path
            var PathNameTooltip = new GUIContent("Path Name", "发射器使用的路径名.");
            var PathNameProp = serializedObject.FindProperty("PathName");
            EditorGUILayout.PropertyField(PathNameProp, PathNameTooltip);
        }

        protected virtual void DrawEffect()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("<size=12><b>Effect Settings</b></size>", mEnableRichTextStyle);
            EditorGUILayout.Space();
            var shotEffectTooltip = new GUIContent("Shot Effect", "射击特效.");
            var shotEffectProp = serializedObject.FindProperty("ShotEffect");
            EditorGUILayout.PropertyField(shotEffectProp, shotEffectTooltip);
            var shootClipTooltip = new GUIContent("ShootClip", "射击音效.");
            var shootClipProp = serializedObject.FindProperty("ShootClip");
            EditorGUILayout.PropertyField(shootClipProp, shootClipTooltip);
        }

        protected GUIStyle mEnableRichTextStyle;
    }
}