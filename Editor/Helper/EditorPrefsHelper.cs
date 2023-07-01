using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;

namespace NCore.Editor
{
    public interface IEditorPrefs
    {
        /// <summary>
        /// 释放EditorPrefs
        /// </summary>
        void ReleaseEditorPrefs();
    }

    public class EditorPrefsHelper
    {
        private EditorPrefsHelper(){}

        [MenuItem("Tools/Prefs/Clear All EditorPrefs")]
        static void ClearAllEditorPrefs()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int i = 0, iMax = assemblies.Length; i < iMax; i++)
            {
                ClearAssemblyEditorPrefs(assemblies[i]);
            }

            // 自身的
            for (int i = 0, iMax = key_obj_list.Count; i < iMax; i++)
                EditorPrefs.DeleteKey(key_obj_list[i]);

            Debug.Log("清理EditorPrefs完成");
        }

        /// <summary>
        /// 删除一个程序集中的EditorPrefs
        /// </summary>
        /// <param name="assembly"></param>
        static void ClearAssemblyEditorPrefs(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            for (int i = 0, iMax = types.Length; i < iMax; i++)
            {
                //如果是接口
                if (types[i].IsInterface) continue;

                Type[] ins = types[i].GetInterfaces();
                foreach (var item in ins)
                {
                    if (item == typeof(IEditorPrefs))
                    {
                        object o = Activator.CreateInstance(types[i]);
                        MethodInfo method = item.GetMethod("ReleaseEditorPrefs");
                        method?.Invoke(o, null);
                        break;
                    }
                }
            }
        }

        #region Key

        static List<string> key_obj_list = new List<string>();

        #endregion


        #region Generic Get and Set methods
        private static string getPrefsKey(string key, bool onlyProject)
        {
            return onlyProject ? $"{Application.dataPath}_{key}" : key;
        }

        // bool
        public static bool GetBool(string prefsKey, bool defaultValue, bool onlyProject = false)
        {
            return EditorPrefs.GetBool(getPrefsKey(prefsKey,onlyProject), defaultValue);
        }

        public static void SetBool(string prefsKey, bool val, bool onlyProject = false)
        {
            EditorPrefs.SetBool(getPrefsKey(prefsKey, onlyProject), val);
        }

        // int
        public static int GetInt(string prefsKey, int defaultValue, bool onlyProject = false)
        {
            return EditorPrefs.GetInt(getPrefsKey(prefsKey, onlyProject), defaultValue);
        }

        public static void SetInt(string prefsKey, int val, bool onlyProject = false)
        {
            EditorPrefs.SetInt(getPrefsKey(prefsKey, onlyProject), val);
        }

        // float
        public static float GetFloat(string prefsKey, float defaultValue, bool onlyProject = false)
        {
            return EditorPrefs.GetFloat(getPrefsKey(prefsKey, onlyProject), defaultValue);
        }

        public static void SetFloat(string prefsKey, float val, bool onlyProject = false)
        {
            EditorPrefs.SetFloat(getPrefsKey(prefsKey, onlyProject), val);
        }

        // color
        public static Color GetColor(string prefsKey, Color c, bool onlyProject = false)
        {
            string strVal = GetString(getPrefsKey(prefsKey, onlyProject), c.r + " " + c.g + " " + c.b + " " + c.a);
            string[] parts = strVal.Split(' ');

            if (parts.Length != 4) return c;
            float.TryParse(parts[0], out c.r);
            float.TryParse(parts[1], out c.g);
            float.TryParse(parts[2], out c.b);
            float.TryParse(parts[3], out c.a);

            return c;
        }

        public static void SetColor(string prefsKey, Color c, bool onlyProject = false)
        {
            SetString(getPrefsKey(prefsKey, onlyProject), c.r + " " + c.g + " " + c.b + " " + c.a);
        }

        // enum
        public static T GetEnum<T>(string prefsKey, T defaultValue, bool onlyProject = false)
        {
            string val = GetString(getPrefsKey(prefsKey, onlyProject), defaultValue.ToString());
            string[] names = System.Enum.GetNames(typeof(T));
            System.Array values = System.Enum.GetValues(typeof(T));

            for (int i = 0; i < names.Length; ++i)
            {
                if (names[i] == val)
                    return (T) values.GetValue(i);
            }

            return defaultValue;
        }

        public static void SetEnum(string prefsKey, System.Enum val, bool onlyProject = false)
        {
            SetString(getPrefsKey(prefsKey, onlyProject), val.ToString());
        }

        // string
        public static string GetString(string prefsKey, string defaultValue, bool onlyProject = false)
        {
            return EditorPrefs.GetString(getPrefsKey(prefsKey, onlyProject), defaultValue);
        }

        public static void SetString(string prefsKey, string val, bool onlyProject = false)
        {
            EditorPrefs.SetString(getPrefsKey(prefsKey, onlyProject), val);
        }

        //Object
        public static T Get<T>(string prefsKey, T defaultValue) where T : Object
        {
            if (!key_obj_list.Contains(prefsKey))
                key_obj_list.Add(prefsKey);

            string path = EditorPrefs.GetString(prefsKey);
            if (string.IsNullOrEmpty(path)) return null;

            T retVal = LoadAsset<T>(path);

            if (retVal == null)
            {
                int id;
                if (int.TryParse(path, out id))
                    return EditorUtility.InstanceIDToObject(id) as T;
            }

            return retVal;
        }

        /// <summary>
        /// 加载Asset
        /// </summary>
        static T LoadAsset<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path)) return null;

            Object obj = AssetDatabase.LoadMainAssetAtPath(path);
            if (obj == null) return null;

            T val = obj as T;
            if (val != null) return val;

            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                if (obj is GameObject)
                {
                    GameObject go = obj as GameObject;
                    return go.GetComponent(typeof(T)) as T;
                }
            }

            return null;
        }

        public static void SetObject(string prefsKey, Object obj)
        {
            if (!key_obj_list.Contains(prefsKey))
                key_obj_list.Add(prefsKey);

            if (obj == null)
            {
                EditorPrefs.DeleteKey(prefsKey);
            }
            else
            {
                if (obj != null)
                {
                    string path = AssetDatabase.GetAssetPath(obj);
                    EditorPrefs.SetString(prefsKey,
                        string.IsNullOrEmpty(path) ? obj.GetInstanceID().ToString() : path);
                }
                else EditorPrefs.DeleteKey(prefsKey);
            }
        }

        #endregion


        public static bool HasKey(string key, bool onlyProject = false)
        {
            return EditorPrefs.HasKey(key);
        }

        public static void DeleteKey(string prefsKey, bool onlyProject = false)
        {
            EditorPrefs.DeleteKey(prefsKey);
        }
    }
}