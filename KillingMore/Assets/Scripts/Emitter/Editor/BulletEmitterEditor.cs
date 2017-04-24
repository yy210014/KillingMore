using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Graphs;

namespace Emitter
{
    [CustomEditor(typeof(Emitter_Bullet)), CanEditMultipleObjects]
    public class BulletEmitterEditor : EmitterParentEditor
    {
        Emitter_Bullet component;

        public override void OnInspectorGUI()
        {
            component = (Emitter_Bullet)target;
            serializedObject.Update();
            mEnableRichTextStyle = new GUIStyle { richText = true };
            DrawBullet();
            DrawEmitter();
            DrawEffect();
            if (component.EmitterPoint == null) component.EmitterPoint = component.transform;
            serializedObject.ApplyModifiedProperties();
        }

        protected override void DrawBullet()
        {
            base.DrawBullet();
            //可去掉参数
            var bulletRotatingSpeedTooltip = new GUIContent("Rotating Speed", "子弹自旋转速度(角度)");
            var bulletRotatingSpeedProp = serializedObject.FindProperty("BulletRotatingSpeed");
            EditorGUILayout.PropertyField(bulletRotatingSpeedProp, bulletRotatingSpeedTooltip);
            var maxBCSpeedTooltip = new GUIContent("Max BC Speed", "子弹的最大弹道修正速度，单位角度");
            var maxBCSpeedSpeedProp = serializedObject.FindProperty("MaxBCSpeed");
            EditorGUILayout.PropertyField(maxBCSpeedSpeedProp, maxBCSpeedTooltip);
            var BCDurationTooltip = new GUIContent("BC Duration", "子弹的弹道修正有效时间");
            var BCDurationSpeedProp = serializedObject.FindProperty("BCDuration");
            EditorGUILayout.PropertyField(BCDurationSpeedProp, BCDurationTooltip);
            var explodeDelayTooltip = new GUIContent("Explode Delay", "子弹爆炸的时间, 大于0时有效");
            var explodeDelayProp = serializedObject.FindProperty("ExplodeDelay");
            EditorGUILayout.PropertyField(explodeDelayProp, explodeDelayTooltip);
        }

        void ShowList(SerializedProperty list)
        {
            EditorGUILayout.PropertyField(list);
            SerializedProperty size = list.FindPropertyRelative("Array.size");
            EditorGUILayout.PropertyField(size);
            for (int i = 0; i < list.arraySize; i++)
            {
                var prop = list.GetArrayElementAtIndex(i);
                EditorGUILayout.PropertyField(prop, true);
            }
        }
        private static GUIContent
           moveButtonContent = new GUIContent("\u21b4", "move down"),
           duplicateButtonContent = new GUIContent("+", "duplicate"),
           deleteButtonContent = new GUIContent("X", "delete");
        private static void ShowButtons(SerializedProperty list, int index)
        {

            if (GUILayout.Button(moveButtonContent, EditorStyles.miniButtonLeft, GUILayout.Width(20f)))
            {
                list.MoveArrayElement(index, index + 1);
            }

            if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, GUILayout.Width(20f)))
            {
                list.InsertArrayElementAtIndex(index);
            }

            if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, GUILayout.Width(20f)))
            {
                int oldSize = list.arraySize;
                list.DeleteArrayElementAtIndex(index);

                if (list.arraySize == oldSize)
                {
                    list.DeleteArrayElementAtIndex(index);
                }
            }
        }

        protected override void DrawEmitter()
        {
            base.DrawEmitter();

            var emitTimesTooltip = new GUIContent("Emit Times", "每个运行周期发射的次数，-1 表示无限制.");
            var emitIntervalTooltip = new GUIContent("Emit Interval", "每次发射的间隔.");
            var emitNumPerShotTooltip = new GUIContent("Bullets Per Shot", "每次发射的子弹数量.");
            var distributionAngleTooltip = new GUIContent("Distribution Angle", "扇形发射的间隔角度.");
            var emitTimesProp = serializedObject.FindProperty("EmitTimes");
            var emitIntervalProp = serializedObject.FindProperty("EmitInterval");
            var emitNumPerShotProp = serializedObject.FindProperty("EmitNumPerShot");
            var distributionAngleProp = serializedObject.FindProperty("DistributionAngle");
            EditorGUILayout.PropertyField(emitTimesProp, emitTimesTooltip);
            EditorGUILayout.PropertyField(emitIntervalProp, emitIntervalTooltip);
            EditorGUILayout.PropertyField(emitNumPerShotProp, emitNumPerShotTooltip);
            EditorGUILayout.PropertyField(distributionAngleProp, distributionAngleTooltip);
        }

        public void RegisterUndo(string name, params Object[] objects)
        {
            if (objects != null && objects.Length > 0)
            {
                UnityEditor.Undo.RecordObjects(objects, name);

                foreach (Object obj in objects)
                {
                    if (obj == null) continue;
                    EditorUtility.SetDirty(obj);
                }
            }
        }
    }
}