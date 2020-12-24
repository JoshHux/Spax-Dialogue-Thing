using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace TMPro.EditorUtilities
{
    [CustomEditor(typeof(TMP_Animated), true)]
    [CanEditMultipleObjects]
    public class TMP_AnimatedEditor : TMP_BaseEditorPanel
    {
        SerializedProperty speedProp;
        protected override void OnEnable()
        {
            base.OnEnable();
            speedProp = serializedObject.FindProperty("speed");

        }
        protected override void OnUndoRedo()
        {
        }
        protected override void DrawExtraSettings()
        {
            EditorGUILayout.LabelField("Animation Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(speedProp, new GUIContent("     Default Speed"));

        }
        protected override bool IsMixSelectionTypes()
        {
            return false;
        }

    }
}