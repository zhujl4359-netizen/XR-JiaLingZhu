using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

// One-time authored-scene update. Keep the flight path clear while making
// the visible load-bearing architecture solid for the CharacterController.
public static class SkyQuestWorldSolidifier
{
    private static readonly string[] Scenes =
    {
        "Assets/SkyQuest/SkyQuest3S.unity",
        "Assets/SkyQuest/SkyDesktop.unity"
    };
    private const string RouteMarker = "Lowered flame route v1";

    [MenuItem("SKY/Lower flight flames and seal buildings")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before updating the scenes.");
        foreach (string path in Scenes)
        {
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            int colliders = 0;
            foreach (GameObject root in scene.GetRootGameObjects())
            foreach (MeshFilter filter in root.GetComponentsInChildren<MeshFilter>(true))
            {
                if (!IsSolidArchitecture(filter.gameObject.name) || filter.sharedMesh == null ||
                    filter.GetComponent<Renderer>() == null || filter.GetComponent<Collider>() != null)
                    continue;
                MeshCollider solid = filter.gameObject.AddComponent<MeshCollider>();
                solid.sharedMesh = filter.sharedMesh;
                solid.convex = false;
                solid.isTrigger = false;
                EditorUtility.SetDirty(filter.gameObject);
                colliders++;
            }

            int lowered = 0;
            GameObject challenge = GameObject.Find("Flying flame challenge");
            if (challenge == null) throw new InvalidOperationException("Flight challenge missing in " + path);
            if (challenge.transform.Find(RouteMarker) == null)
            {
                Physics.SyncTransforms();
                foreach (SkyQuestFlightFlame flame in challenge.GetComponentsInChildren<SkyQuestFlightFlame>(true))
                {
                    Vector3 start = flame.transform.position;
                    Vector3 best = start;
                    float bestCost = float.MaxValue;
                    float targetY = start.y - .85f;
                    for (int ring = 0; ring <= 5; ring++)
                    {
                        float radius = ring * .35f;
                        int angles = ring == 0 ? 1 : 8;
                        for (int angle = 0; angle < angles; angle++)
                        {
                            float radians = angle * Mathf.PI * 2f / angles;
                            Vector3 horizontal = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * radius;
                            for (float y = targetY; y <= start.y + .01f; y += .1f)
                            {
                                Vector3 candidate = new Vector3(start.x + horizontal.x, y, start.z + horizontal.z);
                                if (IsBlocked(candidate, challenge.transform)) continue;
                                float cost = (y - targetY) + radius * .4f;
                                if (cost < bestCost) { bestCost = cost; best = candidate; }
                                break;
                            }
                        }
                    }
                    if (bestCost == float.MaxValue) throw new InvalidOperationException("No clear flight flame position: " + flame.name);
                    flame.transform.position = best;
                    EditorUtility.SetDirty(flame.transform);
                    if (best.y < start.y - .05f) lowered++;
                }
                new GameObject(RouteMarker).transform.SetParent(challenge.transform, false);
            }
            // Flame 11 used to be lifted above a nearby structure. Bring it back
            // into the playable flight route at a clear spot beside that structure.
            const string flame11Marker = "Lowered flame 11 v2";
            if (challenge.transform.Find(flame11Marker) == null)
            {
                foreach (SkyQuestFlightFlame flame in challenge.GetComponentsInChildren<SkyQuestFlightFlame>(true))
                {
                    if (!flame.name.EndsWith("11", StringComparison.Ordinal)) continue;
                    Vector3 start = flame.transform.position;
                    Vector3 best = start;
                    float bestCost = float.MaxValue;
                    for (int ring = 0; ring <= 12; ring++)
                    {
                        float radius = ring * .35f;
                        int angles = ring == 0 ? 1 : 16;
                        for (int angle = 0; angle < angles; angle++)
                        {
                            float radians = angle * Mathf.PI * 2f / angles;
                            Vector3 horizontal = new Vector3(Mathf.Cos(radians), 0f, Mathf.Sin(radians)) * radius;
                            for (float y = 4.2f; y <= start.y + .01f; y += .15f)
                            {
                                Vector3 candidate = new Vector3(start.x + horizontal.x, y, start.z + horizontal.z);
                                if (IsBlocked(candidate, challenge.transform)) continue;
                                float cost = (y - 4.2f) + radius * .25f;
                                if (cost < bestCost) { bestCost = cost; best = candidate; }
                                break;
                            }
                        }
                    }
                    if (bestCost == float.MaxValue) throw new InvalidOperationException("No clear spot for flame 11 in " + path);
                    flame.transform.position = best;
                    EditorUtility.SetDirty(flame.transform);
                    Debug.Log($"IP1 flight flame 11 moved to {best} in {path}");
                    break;
                }
                new GameObject(flame11Marker).transform.SetParent(challenge.transform, false);
            }
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene)) throw new InvalidOperationException("Could not save " + path);
            Debug.Log($"IP1 solid architecture and lower flames: {path}, new colliders={colliders}, lowered flames={lowered}");
        }
        AssetDatabase.SaveAssets();
    }

    private static bool IsSolidArchitecture(string name)
    {
        string n = name.ToLowerInvariant();
        return n.Contains("voussoir") || n == "vault rib" || n.Contains("cross vault") ||
            n.Contains("architrave") || n.Contains("ceiling") ||
            n.Contains("ceiling beam") || n.Contains("column capital") ||
            n.Contains("column gilded capital") || n.Contains("perimeter column capital");
    }

    private static bool IsBlocked(Vector3 position, Transform challenge)
    {
        foreach (Collider hit in Physics.OverlapSphere(position, .56f, ~0, QueryTriggerInteraction.Ignore))
        {
            if (hit == null || !hit.enabled || !hit.gameObject.activeInHierarchy ||
                hit.transform.IsChildOf(challenge)) continue;
            return true;
        }
        return false;
    }
}
