﻿// TSS - Unity visual tweener plugin
// © 2018 ObelardO aka Vladislav Trubitsyn
// obelardos@gmail.com
// https://obeldev.ru
// MIT License

using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEditor;

namespace TSS.Editor
{
    public static class TSSEditorUtils
    {
        #region Properties

        public static GUIContent addKeyButtonContent = new GUIContent("+", "Add a new element to list"),
                                  delKeyButtonContent = new GUIContent("-", "Delete this element from list");

        public static GUILayoutOption max18pxWidth = GUILayout.MaxWidth(18),
                                      max80pxWidth = GUILayout.MaxWidth(80),
                                      fixedLineHeight = GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight),
                                      max120pxWidth = GUILayout.MaxWidth(120),
                                      fixed35pxWidth = GUILayout.Width(35),
                                      fixed25pxHeight = GUILayout.Height(25),
                                      fixed120pxWidth = GUILayout.Width(120);

        public static Color redColor = new Color(1, 0.65f, 0.65f, 1);
        public static Color greenColor = new Color(0.65f, 1, 0.65f, 1);
        public static Color cyanColor = new Color(0.65f, 1, 1, 1);
        public static Color halfBlack = new Color(0f, 0f, 0f, 0.5f);

        #endregion

        #region Inspector tools

        public static Vector3 GetInspectorRotation(this Transform transform)
        {
            Vector3 result = Vector3.zero;
            MethodInfo mth = typeof(Transform).GetMethod("GetLocalEulerAngles", BindingFlags.Instance | BindingFlags.NonPublic);
            PropertyInfo pi = typeof(Transform).GetProperty("rotationOrder", BindingFlags.Instance | BindingFlags.NonPublic);
            object rotationOrder = null;
            if (pi != null)
            {
                rotationOrder = pi.GetValue(transform, null);
            }
            if (mth != null)
            {
                object retVector3 = mth.Invoke(transform, new object[] { rotationOrder });
                result = (Vector3)retVector3;
            }
            return result;
        }

        #endregion

        #region Assets

        public static T CreateAsset<T>(string assetName) where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Selection.activeObject == null || string.IsNullOrEmpty(path) || !Directory.Exists(path)) path = "Assets";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(path, assetName + ".asset"));

            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

            return asset;
        }

        public static T LoadAssetFromUniqueAssetPath<T>(string aAssetPath) where T : Object
        {
            if (aAssetPath.Contains("::"))
            {
                string[] parts = aAssetPath.Split(new string[] { "::" }, System.StringSplitOptions.RemoveEmptyEntries);
                aAssetPath = parts[0];
                if (parts.Length > 1)
                {
                    string assetName = parts[1];
                    System.Type t = typeof(T);
                    var assets = AssetDatabase.LoadAllAssetsAtPath(aAssetPath)
                        .Where(i => t.IsAssignableFrom(i.GetType())).Cast<T>();
                    var obj = assets.Where(i => i.name == assetName).FirstOrDefault();
                    if (obj == null)
                    {
                        int id;
                        if (int.TryParse(parts[1], out id))
                            obj = assets.Where(i => i.GetInstanceID() == id).FirstOrDefault();
                    }
                    if (obj != null)
                        return obj;
                }
            }
            return AssetDatabase.LoadAssetAtPath<T>(aAssetPath);
        }

        public static string GetUniqueAssetPath(Object aObj)
        {
            string path = AssetDatabase.GetAssetPath(aObj);
            if (!string.IsNullOrEmpty(aObj.name))
                path += "::" + aObj.name;
            else
                path += "::" + aObj.GetInstanceID();
            return path;
        }

        #endregion

        #region Generic property drawing

        public static void DrawGenericProperty<T>(ref T propertyValue)
        {
            DrawGenericProperty(ref propertyValue, Color.white, null, null);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, Object recordingObject = null)
        {
            DrawGenericProperty(ref propertyValue, Color.white, null, recordingObject);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, Color propertyColor, Object recordingObject = null)
        {
            DrawGenericProperty(ref propertyValue, propertyColor, null, recordingObject);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, string propertyName)
        {
            DrawGenericProperty(ref propertyValue, Color.white, new GUIContent(TSSText.GetHumanReadableString(propertyName)), null);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, string propertyName, Object recordingObject = null)
        {
            DrawGenericProperty(ref propertyValue, Color.white, new GUIContent(TSSText.GetHumanReadableString(propertyName)), recordingObject);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, GUIContent propertyName = null, Object recordingObject = null)
        {
            DrawGenericProperty(ref propertyValue, Color.white, propertyName, recordingObject);
        }

        public static void DrawGenericProperty<T>(ref T propertyValue, Color propertyColor, GUIContent propertyName = null, Object recordingObject = null)
        {
            System.Type propertyType = typeof(T);

            EditorGUI.BeginChangeCheck();
            GUI.changed = false;
            if (propertyName != null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel(propertyName);
            }
            GUI.color = propertyColor;

            object value = propertyValue;
            object enteredValue = propertyValue;

            T displayedValue = (T)value;

            if (propertyType == typeof(float)) enteredValue = EditorGUILayout.FloatField((float)(object)displayedValue);
            else if (propertyType == typeof(bool)) enteredValue = EditorGUILayout.Toggle((bool)(object)displayedValue, propertyName == null ? max18pxWidth : max120pxWidth);
            else if (propertyType == typeof(int)) enteredValue = EditorGUILayout.IntField((int)(object)displayedValue);
            else if (propertyType == typeof(Color)) enteredValue = EditorGUILayout.ColorField((Color)(object)displayedValue);
            else if (propertyType == typeof(string)) enteredValue = EditorGUILayout.TextArea((string)(object)displayedValue);
            else if (propertyType == typeof(char)) enteredValue = DrawCharProperty(((object)displayedValue).ToString());
            else if (propertyType == typeof(Vector2)) enteredValue = EditorGUILayout.Vector2Field(string.Empty, (Vector2)(object)displayedValue);
            else if (propertyType == typeof(Vector3)) enteredValue = EditorGUILayout.Vector3Field(string.Empty, (Vector3)(object)displayedValue);
            else if (propertyType == typeof(Vector4)) enteredValue = EditorGUILayout.Vector4Field(string.Empty, (Vector4)(object)displayedValue);
            else if (propertyType == typeof(MaterialPropertyType)) enteredValue = EditorGUILayout.EnumPopup((MaterialPropertyType)(object)displayedValue);
            else if (propertyType == typeof(AnimationCurve)) enteredValue = EditorGUILayout.CurveField((AnimationCurve)(object)displayedValue);
            else if (propertyType == typeof(Gradient)) enteredValue = DrawGradientProperty((Gradient)(object)displayedValue);
            else if (propertyType == typeof(TweenType)) enteredValue = DrawTweenTypeProperty((TweenType)(object)displayedValue);
            else if (propertyType == typeof(TweenMode)) enteredValue = EditorGUILayout.EnumPopup((TweenMode)(object)displayedValue);
            else if (propertyType == typeof(ItemEffect)) enteredValue = EditorGUILayout.EnumPopup((ItemEffect)(object)displayedValue);
            else if (propertyType == typeof(ItemUpdateType)) enteredValue = EditorGUILayout.EnumPopup((ItemUpdateType)(object)displayedValue);
            else if (propertyType == typeof(TweenDirection)) enteredValue = EditorGUILayout.EnumPopup((TweenDirection)(object)displayedValue);
            else if (propertyType == typeof(IncorrectStateAction)) enteredValue = EditorGUILayout.EnumPopup((IncorrectStateAction)(object)displayedValue);
            else if (propertyType == typeof(ActivationMode)) enteredValue = EditorGUILayout.EnumPopup((ActivationMode)(object)displayedValue);
            else if (propertyType == typeof(PathLerpMode)) enteredValue = EditorGUILayout.EnumPopup((PathLerpMode)(object)displayedValue);
            else if (propertyType == typeof(TSSItem)) enteredValue = EditorGUILayout.ObjectField(string.Empty, (TSSItem)(object)propertyValue, typeof(TSSItem));
            else if (propertyType == typeof(Transform)) enteredValue = EditorGUILayout.ObjectField(string.Empty, (Transform)(object)propertyValue, typeof(Transform));

            if (EditorGUI.EndChangeCheck() || enteredValue != (object)displayedValue)
            {
                if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Editor] property");
                propertyValue = (T)enteredValue;
            }

            if (propertyName != null) EditorGUILayout.EndHorizontal();
            GUI.color = Color.white;
        }

        #endregion

        /*
        private static bool GenericPropertiesIsIdentical<T>(T[] values)
        {
            if (values.Length == 1) return true;
            for (int i = 0; i < values.Length; i++) if (!EqualityComparer<T>.Default.Equals(values[i], values[0])) return false;
            return true;
        }
        */

        #region Specific propery drawers 

        private static Gradient DrawGradientProperty(Gradient gradient)
        {
            var method = typeof(EditorGUI).GetMethods(BindingFlags.NonPublic | BindingFlags.Static).First(t => t.Name == "GradientField");
            return (Gradient)method.Invoke(null, new object[] { EditorGUILayout.GetControlRect(), gradient });
        }

        private static char DrawCharProperty(string srcStr)
        {
            string result = EditorGUILayout.TextField(srcStr);
            if (string.IsNullOrEmpty(result)) return '0';
            return result[0];
        }

        private static TweenType DrawTweenTypeProperty(TweenType tweenType)
        {
            TweenTypeBase tweenTypeBase = (TweenTypeBase)((int)tweenType < 4 ? (int)tweenType : (int)tweenType / 4 + 3);
            TweenTypeMode tweenTypeMode = (TweenTypeMode)((int)tweenType % 4);

            TweenTypeBase newTweenTypeBase = (TweenTypeBase)EditorGUILayout.EnumPopup(tweenTypeBase);
            TweenTypeMode newTweenTypeMode = tweenTypeMode;

            if ((int)tweenTypeBase > 3) newTweenTypeMode = (TweenTypeMode)EditorGUILayout.EnumPopup(tweenTypeMode);

            return (TweenType)((int)newTweenTypeBase < 4 ? (int)newTweenTypeBase : ((int)newTweenTypeBase - 3) * 4 + (int)newTweenTypeMode);
        }

        public static void DrawEventProperty(SerializedProperty eventHolder, string eventName, int eventListenersCount)
        {
            Rect drawZone = GUILayoutUtility.GetRect(0f, eventListenersCount == 0 ? 80f : eventListenersCount * 43f + 37f);
            SerializedProperty eventProperty = eventHolder.FindPropertyRelative(eventName);
            if (eventProperty == null) return;
            EditorGUI.PropertyField(drawZone, eventProperty);
            GUILayout.Space(2);
        }

        public static void DrawEventProperty(SerializedProperty eventProperty, int eventListenersCount)
        {
            Rect drawZone = GUILayoutUtility.GetRect(0f, eventListenersCount == 0 ? 80f : eventListenersCount * 43f + 37f);
            if (eventProperty == null) return;
            EditorGUI.PropertyField(drawZone, eventProperty);
            GUILayout.Space(2);
        }

        public static void DrawKeyCodeListProperty(List<KeyCode> keys, UnityEngine.Object recordingObject, SerializedProperty list, bool drawBack = true)
        {
            if (drawBack)
            {
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            }
            else EditorGUILayout.BeginVertical();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On keyboard", max120pxWidth);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();

            if (keys.Count == 0) EditorGUILayout.LabelField("List is empty", EditorStyles.centeredGreyMiniLabel);

            for (int i = 0; i < list.arraySize; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), new GUIContent(string.Empty));

                if (GUILayout.Button(delKeyButtonContent, max18pxWidth, fixedLineHeight))
                {
                    if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Item] remove button key");
                    keys.Remove(keys[i]);

                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();
                    return;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            if (GUILayout.Button(addKeyButtonContent, max18pxWidth, fixedLineHeight))
            {
                if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Item] add button key");
                keys.Add(KeyCode.None);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            if (keys.Count == 0) EditorGUILayout.Space();

            EditorGUILayout.EndVertical();

        }
        
        public static void DrawKeyCodeListProperty(List<KeyCode> keys, UnityEngine.Object recordingObject, bool drawBack = true)
        {
            if (drawBack)
            {
                GUI.backgroundColor = new Color(0f, 0f, 0f, 0.5f);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            }
            else EditorGUILayout.BeginVertical();

            GUI.backgroundColor = Color.white;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("On keyboard", max120pxWidth);
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();

            if (keys.Count == 0) EditorGUILayout.LabelField("List is empty", EditorStyles.centeredGreyMiniLabel);

            for (int i = 0; i < keys.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                EditorGUI.BeginChangeCheck();

                KeyCode newKey = (KeyCode)EditorGUILayout.EnumPopup(keys[i], fixedLineHeight);

                if (EditorGUI.EndChangeCheck())
                {
                    if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Item] edit button key");
                    keys[i] = newKey;
                }

                if (GUILayout.Button(delKeyButtonContent, max18pxWidth, fixedLineHeight))
                {
                    if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Item] remove button key");
                    keys.Remove(keys[i]);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            if (GUILayout.Button(addKeyButtonContent, max18pxWidth, fixedLineHeight))
            {
                if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Item] add button key");
                keys.Add(KeyCode.None);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        
        public static void DrawSeparator()
        {
            EditorGUILayout.LabelField(string.Empty, GUI.skin.horizontalSlider);
        }

        public static void DrawMinMaxSliderProperty(ref float minValue, ref float maxValue, UnityEngine.Object recordingObject, float clampMin = 0, float clampMax = 1)
        {
            float newMinValue = minValue;
            float newMaxValue = maxValue;

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.MinMaxSlider(ref newMinValue, ref newMaxValue, clampMin, clampMax);
            if (EditorGUI.EndChangeCheck())
            {
                if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS] some values");
                minValue = newMinValue;
                maxValue = newMaxValue;
            }
        }

        public static void DrawSliderProperty(ref float propertyValue, UnityEngine.Object recordingObject, GUIContent propertyName = null, float clampMin = 0.01f, float clampMax = 5)
        {
            EditorGUILayout.BeginHorizontal();

            if (propertyName != null) EditorGUILayout.PrefixLabel(propertyName);

            EditorGUI.BeginChangeCheck();

            float tweenBlendFactor = EditorGUILayout.Slider(propertyValue, clampMin, clampMax);
            if (EditorGUI.EndChangeCheck())
            {
                if (recordingObject != null) Undo.RecordObject(recordingObject, "[TSS Tween] blend factor");
                propertyValue = tweenBlendFactor;
            }

            EditorGUILayout.EndHorizontal();
        }

        public static void BeginBlackVertical()
        {
            GUI.backgroundColor = TSSEditorUtils.halfBlack;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUI.backgroundColor = Color.white;
        }

        #endregion
    }

    [CustomPropertyDrawer(typeof(TSSKeyCodeAttribute))]
    public class TSSKeyCodeDrawer : PropertyDrawer
    {
        #region Properties

        const float clearButtonWidth = 50;

        #endregion

        #region GUI

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            int id = GUIUtility.GetControlID((int)position.x, FocusType.Keyboard, position);

            string propertyLabel = ((KeyCode)property.intValue).ToString();

            if (GUIUtility.keyboardControl == id)
            {
                propertyLabel += " (press any key)";
                GUI.color = new Color(0.4f, 0.65f, 1f, 1f);
                position.width -= clearButtonWidth;
            }

            if (GUI.Button(position, propertyLabel, EditorStyles.popup))
            {
                GUIUtility.keyboardControl = id;
                Event.current.Use();
            }

            GUI.color = Color.white;

            position.x += position.width;
            position.width = clearButtonWidth;

            if (GUIUtility.keyboardControl == id && GUI.Button(position, "None"))
            {
                property.enumValueIndex = (int)KeyCode.None;
                Event.current.Use();
                GUIUtility.keyboardControl = -1;

            }

            if (GUIUtility.keyboardControl == id && Event.current.type == EventType.KeyUp)
            {
                if (Event.current.keyCode != KeyCode.None) property.enumValueIndex = KeyCodeToEnumIndex(property, Event.current.keyCode);
                Event.current.Use();
                GUIUtility.keyboardControl = -1;
            }
            else if (GUIUtility.keyboardControl == id && Event.current.isKey)
                Event.current.Use();

            GUI.color = Color.white;
        }

        public int KeyCodeToEnumIndex(SerializedProperty keyCodeProperty, KeyCode keyCode)
        {
            string[] keyCodeNames = keyCodeProperty.enumNames;
            for (int i = 0; i < keyCodeNames.Length; i++)
            {
                if (keyCodeNames[i].CompareTo(keyCode.ToString()) == 0)
                    return i;
            }
            return 0;
        }

        #endregion
    }
}