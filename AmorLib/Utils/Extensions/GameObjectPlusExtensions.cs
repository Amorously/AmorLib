using LevelGeneration;
using System.Text;
using UnityEngine;
using AkEventCallback = AkCallbackManager.EventCallback;

namespace AmorLib.Utils.Extensions;

public static class GameObjectPlusExtensions
{
    /// <summary>
    /// Attempts to retrieve a component of type <typeparamref name="T"/> from the <see cref="GameObject"/>.
    /// </summary>
    /// <remarks>The normal TryGetComponent method is broken in Il2Cpp!</remarks>
    /// <param name="component">Contains the component of type <typeparamref name="T"/>, if found.</param>
    /// <returns>
    /// <see langword="true"/> if a component of type <typeparamref name="T"/> is found; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool TryAndGetComponent<T>(this GameObject go, out T component)
    {
        component = go.GetComponent<T>();
        return component != null;
    }

    /// <summary>
    /// Creates a new component of type <typeparamref name="T"/> if it does not exist and returns it.
    /// </summary>
    /// If the component does not exist, it is added to the <see cref="GameObject"/>.
    /// <returns>The component of type <typeparamref name="T"/>.</returns>
    public static T AddOrGetComponent<T>(this GameObject go) where T : Component
    {
        if (!go.TryAndGetComponent(out T comp))
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

    /// <summary>
    /// Returns the full hierarchical path of a <see cref="GameObject"/> in the scene.
    /// </summary>
    public static string GetFullPath(this GameObject go)
    {
        StringBuilder sb = new(go.name);
        Transform current = go.transform.parent;
        while (current != null)
        {
            sb.Insert(0, current.name + "/");
            current = current.parent;
        }
        return sb.ToString();
    }

    /// <summary>
    /// Creates a clone of a <see cref="GameObject"/> and instantiates any child <see cref="LG_PrefabSpawner"/>.
    /// </summary>
    public static GameObject ClonePrefabSpawners(this GameObject original, Vector3 position, Quaternion rotation, Transform parent)
    {
        var clone = UnityEngine.Object.Instantiate(original, position, rotation, parent);

        foreach (var spawner in clone.GetComponentsInChildren<LG_PrefabSpawner>())
        {
            try
            {
                GameObject prefab = UnityEngine.Object.Instantiate(spawner.m_prefab, spawner.transform.position, spawner.transform.rotation, spawner.transform.parent);
                if (spawner.m_disableCollision)
                {
                    foreach (Collider collider in prefab.GetComponentsInChildren<Collider>())
                    {
                        collider.enabled = false;
                    }
                }
                if (spawner.m_applyScale)
                {
                    prefab.transform.localScale = spawner.transform.localScale;
                }
                prefab.transform.SetParent(spawner.transform);
            }
            catch
            {
                continue;
            }
        }

        return clone;
    }

    /// <summary>
    /// Posts a sound event and registers a cleanup callback upon completion.
    /// </summary>
    public static uint PostWithCleanup(this CellSoundPlayer soundPlayer, uint eventID, Vector3 pos, uint in_uFlags = 1u)
    {
        return soundPlayer.Post(eventID, pos, in_uFlags, (AkEventCallback)SoundDoneCallback, soundPlayer);
    }

    private static void SoundDoneCallback(Il2CppSystem.Object in_pCookie, AkCallbackType in_type, AkCallbackInfo callbackInfo)
    {
        var callbackPlayer = in_pCookie.Cast<CellSoundPlayer>();
        callbackPlayer?.Recycle();
    }

    /// <summary>
    /// Compares the square magnitude of two <see cref="Vector3"/> values to the square of <paramref name="sqrThreshold"/>.
    /// </summary>
    public static bool IsWithinSqrDistance(this Vector3 a, Vector3 b, float sqrThreshold, out float sqrDistance)
    {
        sqrDistance = (a - b).sqrMagnitude;
        return sqrDistance <= sqrThreshold;
    }

    /// <summary>
    /// Compares the square magnitude of two <see cref="Vector3"/> values to the square of <paramref name="sqrThreshold"/>.
    /// </summary>
    public static bool IsWithinSqrDistance(this Vector3 a, Vector3 b, float sqrThreshold)
    {
        float sqrDistance = (a - b).sqrMagnitude;
        return sqrDistance <= sqrThreshold;
    }
}