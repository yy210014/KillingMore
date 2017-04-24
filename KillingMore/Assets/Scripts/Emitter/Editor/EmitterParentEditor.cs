using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.Graphs;

namespace Emitter
{
    [CustomEditor(typeof(EmitterParent)), CanEditMultipleObjects]
    public class EmitterParentEditor : EmitterEditor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            mEnableRichTextStyle = new GUIStyle { richText = true };
            var sequentialIntervalTooltip = new GUIContent("Sequential Interval", "更新序列间隔，如果大于0则按顺序更新子发射器，否则则为同步.");
            var sequentialIntervalProp = serializedObject.FindProperty("SequentialInterval");
            EditorGUILayout.PropertyField(sequentialIntervalProp, sequentialIntervalTooltip);
            serializedObject.ApplyModifiedProperties();
        }
    }
}