using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public static class Utility
{
    /// <summary>
    /// Destroys all the children of the given transform
    /// </summary>
    /// <param name="transform"></param>
    public static void KillAllChildren(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Object.DestroyImmediate(child.gameObject);
            }
            else
            {
                Object.Destroy(child.gameObject);
            }
        }
    }

    /// <summary>
    /// Prints the entire hierarchy of a Transform
    /// </summary>
    /// <param name="transform"></param>
    public static void PrintHierarchy(this Transform transform)
    {
        string hierarchy = transform.name;
        Transform current = transform.parent;
        while (current != null)
        {
            hierarchy = $"{current.name} > {hierarchy}";
            current = current.parent;
        }
        Debug.Log(hierarchy);
    }

    /// <summary>
    /// Prints all of the GameObjects under the mouse
    /// </summary>
    public static void PrintUIElementUnderMouse() // à utiliser avec "if (Input.GetMouseButtonDown(0))"
    {
        List<GameObject> objects = DetectUIElementUnderMouse();
        foreach (GameObject obj in objects)
        {
            Debug.Log(obj.name);
        }
    }

    /// <summary>
    /// Returns all of the GameObjects under the mouse
    /// </summary>
    /// <returns></returns>
    public static List<GameObject> DetectUIElementUnderMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1 };
        pointerData.position = Mouse.current.position.ReadValue();

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);
        return results.Select(x => x.gameObject).ToList();
    }

    /// <summary>
    /// Returns the values of a given enum
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static IEnumerable<T> GetValues<T>()
    {
        return System.Enum.GetValues(typeof(T)).Cast<T>();
    }

    /// <summary>
    /// Shuffles a list randomly
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="random">Optional randomizer; if left null, will create a new one</param>
    public static void Shuffle<T>(this IList<T> list, System.Random random = null)
    {
        if (random == null)
            random = new System.Random();

        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Starts a particle system and its children
    /// </summary>
    /// <param name="component"></param>
    public static void StartParticles(this Component component)
    {
        List<ParticleSystem> particles = component.GetComponentsInChildren<ParticleSystem>().ToList();
        foreach (ParticleSystem particle in particles)
            particle.Play(false);
    }

    /// <summary>
    /// Stops a particle system and its children
    /// </summary>
    /// <param name="component"></param>
    public static void StopParticles(this Component component)
    {
        List<ParticleSystem> particles = component.GetComponentsInChildren<ParticleSystem>().ToList();
        foreach (ParticleSystem particle in particles)
        {
            particle.Stop(false, ParticleSystemStopBehavior.StopEmitting);
            ParticleSystem.MainModule main = particle.main;
            main.ringBufferMode = ParticleSystemRingBufferMode.Disabled;
        }
    }

    /// <summary>
    /// Starts a particle system and its children
    /// </summary>
    /// <param name="obj"></param>
    public static void StartParticles(this GameObject obj)
    {
        StartParticles(obj.transform);
    }

    /// <summary>
    /// Stops a particle system and its children
    /// </summary>
    /// <param name="obj"></param>
    public static void StopParticles(this GameObject obj)
    {
        StopParticles(obj.transform);
    }
}