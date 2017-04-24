using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Graphs;

namespace Emitter
{
 //   [CustomEditor(typeof(Emitter_Missile)), CanEditMultipleObjects]
    public class MissileEmitterEditor : EmitterParentEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            mEnableRichTextStyle = new GUIStyle { richText = true };
            DrawBullet();
            DrawEmitter();
            DrawEffect();
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawBullet()
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
            var positionRandomnessTooltip = new GUIContent("Position Randomness", "子弹随机位置.");
            var positionRandomnessProp = serializedObject.FindProperty("PositionRandomness");
            EditorGUILayout.Slider(positionRandomnessProp, 0f, 5f, positionRandomnessTooltip);

            var initialVelocTooltip = new GUIContent("Initial Velocity", "子弹初始速度.");
            var accTooltip = new GUIContent("Accelerate Velocity", "子弹运行的加速度.");
            var accDurationTooltip = new GUIContent("Accelerate Duration", "加速持续时间.");
            var initialVelocProp = serializedObject.FindProperty("InitialVeloc");
            var accProp = serializedObject.FindProperty("Acc");
            var accDurationProp = serializedObject.FindProperty("AccDuration");
            EditorGUILayout.PropertyField(initialVelocProp, initialVelocTooltip);
            EditorGUILayout.PropertyField(accProp, accTooltip);
            EditorGUILayout.PropertyField(accDurationProp, accDurationTooltip);

            var maTooltip = new GUIContent("Max Speed", "Max speed - tweak as needed.");
            var pcTooltip = new GUIContent("Proportional Const", "Proportional constant - 0f - 1f (set this to tweak performance of missile) lower is more 'swingy' higher is more 'direct/concise'.");
            var maProp = serializedObject.FindProperty("MaxSpeed");
            var pcProp = serializedObject.FindProperty("ProportionalConst");
            EditorGUILayout.PropertyField(maProp, maTooltip);
            EditorGUILayout.PropertyField(pcProp, pcTooltip);
        }

        protected override void DrawEmitter()
        {
            base.DrawEmitter();
            var smoTooltip = new GUIContent("Swarm Missiles Outward", "导弹向外发射.");
            var smoProp = serializedObject.FindProperty("SwarmMissilesOutward");
            EditorGUILayout.PropertyField(smoProp, smoTooltip);
            if (smoProp.boolValue)
            {
                EditorGUI.indentLevel++;
                var nsodTooltip = new GUIContent("Node Swarm Offset Distance", "向外发射距离.");
                var ndTooltip = new GUIContent("Node Direction", "导弹扩散方向.");
                var nsodProp = serializedObject.FindProperty("NodeSwarmOffsetDistance");
                var ndProp = serializedObject.FindProperty("NodeDirection");
                EditorGUILayout.PropertyField(nsodProp, nsodTooltip);
                EditorGUILayout.PropertyField(ndProp, ndTooltip);
                EditorGUI.indentLevel--;
            }
        }
    }
}