using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections;

//=============================================================================
//  TrafficLightManagerEditor
//  by Healthbar Games (http://healthbargames.pl)
//  author: Mariusz Skowroński
//
//  Custom editor for TrafficLightManager
//
//=============================================================================
namespace HealthbarGames
{
    [CustomEditor(typeof(TrafficLightManager))]
    [CanEditMultipleObjects]
    [System.Serializable]
    public class TrafficLightManagerEditor : Editor
    {
        // Serialized properties exposed in TrafficLightManager
        SerializedProperty mDefaultStartTime;
        SerializedProperty mDefaultActiveTime;
        SerializedProperty mDefaultEndTime;

        SerializedProperty mPhaseDelay;
        SerializedProperty mYellowBlinkFreq;

        SerializedProperty mInitialProgram;

        ReorderableList mPhaseList;
        static bool mHeaderFoldout;
        static bool mPhaseListFoldout;

        SerializedProperty mSelectedPhase;

        public void OnEnable()
        {
            // Find all properties exposed in TrafficLightManager
            mDefaultStartTime = serializedObject.FindProperty("DefaultPhaseStartTime");
            mDefaultActiveTime = serializedObject.FindProperty("DefaultPhaseActiveTime");
            mDefaultEndTime = serializedObject.FindProperty("DefaultPhaseEndTime");

            mPhaseDelay = serializedObject.FindProperty("PhaseDelay");
            mYellowBlinkFreq = serializedObject.FindProperty("YellowBlinkFreq");
            mInitialProgram = serializedObject.FindProperty("InitialProgram");

            // For phase list create reorderable list
            mPhaseList = new ReorderableList(serializedObject, serializedObject.FindProperty("PhaseList"), true, true, true, true);

            // Setup callbacks for reorderable list
            mPhaseList.drawHeaderCallback = this.OnPhaseListDrawHeader;
            mPhaseList.drawElementCallback = this.OnPhaseListDrawElement;
            mPhaseList.onAddCallback = this.OnPhaseListAddElement;
            mPhaseList.onSelectCallback = this.OnPhaseListSelectElement;
            mPhaseList.onChangedCallback = this.OnPhaseListChanged;

        }


        override
        public void OnInspectorGUI()
        {
            serializedObject.Update();

            // DEFAULT TIMINGS section
            mHeaderFoldout = EditorGUILayout.Foldout(mHeaderFoldout, "Default Timings");
            if (mHeaderFoldout)
            {
                EditorGUI.indentLevel += 1;
                // Default Timings - Start Time
                EditorGUILayout.BeginHorizontal();
                mDefaultStartTime.floatValue = EditorGUILayout.Slider("Start Time", mDefaultStartTime.floatValue, 0.0f, 10.0f);
                if (GUILayout.Button("Set for all"))
                {
                    for (int i = 0; i < mPhaseList.serializedProperty.arraySize; i++)
                    {
                        SerializedProperty phase = mPhaseList.serializedProperty.GetArrayElementAtIndex(i);
                        phase.FindPropertyRelative("PhaseStartTime").floatValue = mDefaultStartTime.floatValue;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Default Timings - Phase Time
                EditorGUILayout.BeginHorizontal();
                mDefaultActiveTime.floatValue = EditorGUILayout.Slider("Phase Time", mDefaultActiveTime.floatValue, 1.0f, 300.0f);
                if (GUILayout.Button("Set for all"))
                {
                    for (int i = 0; i < mPhaseList.serializedProperty.arraySize; i++)
                    {
                        SerializedProperty phase = mPhaseList.serializedProperty.GetArrayElementAtIndex(i);
                        phase.FindPropertyRelative("PhaseActiveTime").floatValue = mDefaultActiveTime.floatValue;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Default Timings - End Time
                EditorGUILayout.BeginHorizontal();
                mDefaultEndTime.floatValue = EditorGUILayout.Slider("End Time", mDefaultEndTime.floatValue, 0.0f, 10.0f);
                if (GUILayout.Button("Set for all"))
                {
                    for (int i = 0; i < mPhaseList.serializedProperty.arraySize; i++)
                    {
                        SerializedProperty phase = mPhaseList.serializedProperty.GetArrayElementAtIndex(i);
                        phase.FindPropertyRelative("PhaseEndTime").floatValue = mDefaultEndTime.floatValue;
                    }
                }
                EditorGUILayout.EndHorizontal();

                // Default Timings - Button for setting default timings for all phases
                if (GUILayout.Button("Set default timings for all phases"))
                {
                    for (int i = 0; i < mPhaseList.serializedProperty.arraySize; i++)
                    {
                        SerializedProperty phase = mPhaseList.serializedProperty.GetArrayElementAtIndex(i);
                        phase.FindPropertyRelative("PhaseStartTime").floatValue = mDefaultStartTime.floatValue;
                        phase.FindPropertyRelative("PhaseActiveTime").floatValue = mDefaultActiveTime.floatValue;
                        phase.FindPropertyRelative("PhaseEndTime").floatValue = mDefaultEndTime.floatValue;
                    }
                }

                EditorGUI.indentLevel -= 1;
            } // End of DEFAULT TIMINGS section

            mPhaseDelay.floatValue = EditorGUILayout.Slider("Phase Delay", mPhaseDelay.floatValue, 0.0f, 10.0f);
            mYellowBlinkFreq.floatValue = EditorGUILayout.Slider("Yellow Blink Freq.", mYellowBlinkFreq.floatValue, 0.1f, 5.0f);
            mInitialProgram.enumValueIndex = EditorGUILayout.Popup("Initial Program", mInitialProgram.enumValueIndex, mInitialProgram.enumDisplayNames);

            // PHASE LIST section
            mPhaseListFoldout = EditorGUILayout.Foldout(mPhaseListFoldout, "Phases");
            if (mPhaseListFoldout)
            {
                EditorGUI.indentLevel += 1;
                mPhaseList.DoLayoutList();

                // Phase Info
                if (mSelectedPhase != null)
                {
                    SerializedProperty phase = mSelectedPhase;

                    SerializedProperty phaseStartTime = phase.FindPropertyRelative("PhaseStartTime");
                    SerializedProperty phaseActiveTime = phase.FindPropertyRelative("PhaseActiveTime");
                    SerializedProperty phaseEndTime = phase.FindPropertyRelative("PhaseEndTime");
                    SerializedProperty lightsList = phase.FindPropertyRelative("TrafficLights");

                    phaseStartTime.floatValue = EditorGUILayout.Slider("Start time", phaseStartTime.floatValue, 0.0f, 10.0f);
                    phaseActiveTime.floatValue = EditorGUILayout.Slider("Phase time", phaseActiveTime.floatValue, 1.0f, 300.0f);
                    phaseEndTime.floatValue = EditorGUILayout.Slider("End time", phaseEndTime.floatValue, 0.0f, 10.0f);

                    EditorGUILayout.PropertyField(lightsList, true);
                }
                else
                {
                    EditorGUILayout.LabelField("[No phase selected]");
                }
                EditorGUI.indentLevel -= 1;
            } // End of PHASE LIST section


            if (GUI.changed)
                EditorUtility.SetDirty(target);

            serializedObject.ApplyModifiedProperties();
        }

        // Phase list callback for drawing header
        private void OnPhaseListDrawHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Phase List Sequence");
        }

        // Phase list callback for drawing single row
        private void OnPhaseListDrawElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = mPhaseList.serializedProperty.GetArrayElementAtIndex(index);
            rect.y += 2;
            //EditorGUI.PropertyField(new Rect(rect.x, rect.y, 60, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Type"), GUIContent.none);

            // index
            EditorGUI.LabelField(new Rect(rect.x, rect.y, 35, EditorGUIUtility.singleLineHeight), string.Format("{0:00}", index + 1));
            // label
            EditorGUI.PropertyField(new Rect(rect.x + 35, rect.y, rect.width - 35, EditorGUIUtility.singleLineHeight), element.FindPropertyRelative("Name"), GUIContent.none);

        }

        private void OnPhaseListAddElement(ReorderableList list)
        {
            int index = list.serializedProperty.arraySize;
            list.serializedProperty.arraySize++;
            list.index = index;
            var element = list.serializedProperty.GetArrayElementAtIndex(index);

            element.FindPropertyRelative("Name").stringValue = string.Format("Phase {0:00}", index + 1);
            element.FindPropertyRelative("PhaseStartTime").floatValue = mDefaultStartTime.floatValue;
            element.FindPropertyRelative("PhaseActiveTime").floatValue = mDefaultActiveTime.floatValue;
            element.FindPropertyRelative("PhaseEndTime").floatValue = mDefaultEndTime.floatValue;
            element.FindPropertyRelative("TrafficLights").arraySize = 0;
        }


        private void OnPhaseListSelectElement(ReorderableList list)
        {
            //Debug.Log("PhaseList selection:" + list.index.ToString());
            mSelectedPhase = list.serializedProperty.GetArrayElementAtIndex(list.index);
        }

        private void OnPhaseListChanged(ReorderableList list)
        {
            if (list.index >= 0)
                mSelectedPhase = list.serializedProperty.GetArrayElementAtIndex(list.index);
            else
                mSelectedPhase = null;
        }
    }
}
