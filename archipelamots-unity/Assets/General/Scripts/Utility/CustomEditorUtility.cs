#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class CustomEditorUtility
{
    /// <summary>
    /// Returns all the assets that are instances of a specific ScriptableObject type
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FindAllInstances<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name);
        T[] instances = new T[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            instances[i] = AssetDatabase.LoadAssetAtPath(path, typeof(T)) as T;
        }

        return instances;

    }

    /// <summary>
    /// Returns all the components of the given type that exist in the Assets
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T[] FindAllScripts<T>() where T : MonoBehaviour
    {
        string[] guids = AssetDatabase.FindAssets("t:GameObject a:assets", new[] { "Assets/General/Objects" });
        List<T> instances = new List<T>();
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            GameObject obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            T[] components = obj.GetComponentsInChildren<T>();
            instances.AddRange(components);
        }

        return instances.ToArray();
    }
}
#endif