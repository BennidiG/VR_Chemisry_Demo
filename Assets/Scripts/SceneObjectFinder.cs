using UnityEngine;

/// <summary>
/// Finds scene objects by name, including objects on an inactive UI panel.
/// Active lookups still use GameObject.Find; the more expensive fallback only
/// runs when the active object cannot be found.
/// </summary>
public static class SceneObjectFinder
{
    public static GameObject Find(string path)
    {
        GameObject activeObject = GameObject.Find(path);
        if (activeObject != null)
            return activeObject;

        string[] parts = path.Split('/');
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject candidate in allObjects)
        {
            if (!candidate.scene.IsValid() || candidate.name != parts[parts.Length - 1])
                continue;

            Transform current = candidate.transform;
            bool matchesPath = true;
            for (int i = parts.Length - 2; i >= 0; i--)
            {
                current = current.parent;
                if (current == null || current.name != parts[i])
                {
                    matchesPath = false;
                    break;
                }
            }

            if (matchesPath)
                return candidate;
        }

        return null;
    }
}
