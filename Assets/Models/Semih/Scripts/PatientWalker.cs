using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Hastayi isinlanmak yerine hedefe yurutur. Programcinin koduna dokunmaz:
/// PatientSequenceController hastayi SetPositionAndRotation ile isinlar; bu bilesen ayni karenin
/// LateUpdate'inde buyuk sicramayi (teleportThreshold'dan fazla) yakalar, hastayi eski yerine geri
/// koyar ve yeni hedefe saniyede ~speed metre hizla yurutur.
///
/// Rota: once dogrudan cizgi denenir. Duvara carpiyorsa sahnedeki "PatientWaypoints" nesnesinin
/// cocuklari (kapi ve koridor noktalari) ara nokta olarak kullanilir; en kisa engelsiz rota secilir.
/// Yururken hastanin collider'lari kapali, varinca eski hallerine doner.
/// </summary>
[DisallowMultipleComponent]
public class PatientWalker : MonoBehaviour
{
    [SerializeField] private float speed = 1.5f;
    [SerializeField] private float teleportThreshold = 1.5f;
    [SerializeField] private float clearanceRadius = 0.3f;
    [SerializeField] private float floorY = -0.092f;
    [SerializeField] private string waypointRootName = "PatientWaypoints";

    private readonly List<Vector3> _route = new List<Vector3>();
    private Collider[] _colliders;
    private bool[] _colliderStates;
    private Vector3 _lastPosition;
    private Quaternion _finalRotation;
    private int _routeIndex;
    private bool _walking;

    public bool IsWalking => _walking;

    private void Awake()
    {
        _lastPosition = transform.position;
        _colliders = GetComponentsInChildren<Collider>(true);
        _colliderStates = new bool[_colliders.Length];
    }

    private void LateUpdate()
    {
        Vector3 current = transform.position;

        // Isinlanma yakalandi: geri al, rotayi kur, yurumeye basla.
        if ((current - _lastPosition).magnitude > teleportThreshold)
        {
            Vector3 target = current;
            _finalRotation = transform.rotation;
            transform.position = _lastPosition;

            _route.Clear();
            _route.AddRange(PlanRoute(_lastPosition, target, CollectWaypoints(), clearanceRadius, floorY, transform));
            _routeIndex = 0;

            if (!_walking)
            {
                for (int i = 0; i < _colliders.Length; i++)
                {
                    if (_colliders[i] == null) continue;
                    _colliderStates[i] = _colliders[i].enabled;
                    _colliders[i].enabled = false;
                }
            }
            _walking = true;
        }

        if (_walking) Step(Time.deltaTime);

        _lastPosition = transform.position;
    }

    private void Step(float dt)
    {
        if (_routeIndex >= _route.Count)
        {
            Arrive();
            return;
        }

        Vector3 waypoint = _route[_routeIndex];
        Vector3 position = transform.position;
        Vector3 next = Vector3.MoveTowards(position, waypoint, speed * dt);

        Vector3 flat = waypoint - position;
        flat.y = 0f;
        if (flat.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(flat.normalized, Vector3.up);

        transform.position = next;

        if ((waypoint - next).sqrMagnitude < 0.0001f)
        {
            _routeIndex++;
            if (_routeIndex >= _route.Count) Arrive();
        }
    }

    private void Arrive()
    {
        transform.rotation = _finalRotation;
        for (int i = 0; i < _colliders.Length; i++)
        {
            if (_colliders[i] != null) _colliders[i].enabled = _colliderStates[i];
        }
        _walking = false;
    }

    private List<Vector3> CollectWaypoints()
    {
        var list = new List<Vector3>();
        GameObject root = GameObject.Find(waypointRootName);
        if (root == null) return list;
        foreach (Transform child in root.transform) list.Add(child.position);
        return list;
    }

    /// <summary>
    /// Baslangic, ara noktalar ve hedef arasinda engelsiz kenarlarla en kisa yolu bulur (Dijkstra).
    /// Donen liste baslangici icermez, hedefle biter. Yol yoksa yalnizca hedefi dondurur.
    /// </summary>
    public static List<Vector3> PlanRoute(Vector3 from, Vector3 to, IList<Vector3> waypoints,
        float radius, float floorY, Transform ignoreRoot)
    {
        var nodes = new List<Vector3> { from };
        nodes.AddRange(waypoints);
        nodes.Add(to);
        int n = nodes.Count, goal = n - 1;

        var dist = new float[n];
        var prev = new int[n];
        var done = new bool[n];
        for (int i = 0; i < n; i++) { dist[i] = float.MaxValue; prev[i] = -1; }
        dist[0] = 0f;

        for (int iter = 0; iter < n; iter++)
        {
            int u = -1;
            for (int i = 0; i < n; i++)
                if (!done[i] && dist[i] < float.MaxValue && (u < 0 || dist[i] < dist[u])) u = i;
            if (u < 0 || u == goal) break;
            done[u] = true;

            for (int v = 0; v < n; v++)
            {
                if (done[v] || v == u) continue;
                float d = Flat(nodes[u], nodes[v]);
                if (dist[u] + d >= dist[v]) continue;
                if (!IsClear(nodes[u], nodes[v], radius, floorY, ignoreRoot)) continue;
                dist[v] = dist[u] + d;
                prev[v] = u;
            }
        }

        var route = new List<Vector3>();
        if (prev[goal] < 0 && goal != 0)
        {
            Debug.LogWarning("[PatientWalker] Engelsiz rota bulunamadi, dogrudan yuruyor: " + from + " -> " + to);
            route.Add(to);
            return route;
        }

        for (int v = goal; v != 0; v = prev[v]) route.Insert(0, nodes[v]);
        return route;
    }

    private static float Flat(Vector3 a, Vector3 b)
    {
        a.y = 0f; b.y = 0f;
        return Vector3.Distance(a, b);
    }

    /// <summary>
    /// Iki nokta arasinda diz ve gogus hizasinda kure taramasi; statik engel varsa false.
    /// Ayrica yol boyunca her 0,5 m'de altta zemin olmali (haritada zeminsiz bosluklar var).
    /// </summary>
    public static bool IsClear(Vector3 a, Vector3 b, float radius, float floorY, Transform ignoreRoot)
    {
        Vector3 flatA = new Vector3(a.x, 0f, a.z), flatB = new Vector3(b.x, 0f, b.z);
        int floorSamples = Mathf.Max(1, Mathf.CeilToInt(Vector3.Distance(flatA, flatB) / 0.5f));
        for (int i = 0; i <= floorSamples; i++)
        {
            Vector3 p = Vector3.Lerp(flatA, flatB, i / (float)floorSamples);
            if (!Physics.Raycast(new Vector3(p.x, floorY + 0.3f, p.z), Vector3.down, 1f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                return false;
        }

        foreach (float h in new[] { 0.45f, 1.45f })
        {
            Vector3 p0 = new Vector3(a.x, floorY + h, a.z);
            Vector3 p1 = new Vector3(b.x, floorY + h, b.z);
            Vector3 delta = p1 - p0;
            float length = delta.magnitude;
            if (length < 0.001f) continue;

            RaycastHit[] hits = Physics.SphereCastAll(p0, radius, delta / length, length,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
            foreach (RaycastHit hit in hits)
            {
                Collider c = hit.collider;
                if (ignoreRoot != null && c.transform.IsChildOf(ignoreRoot)) continue;
                if (c.GetComponentInParent<PatientNPC>() != null) continue;            // diger hastalar
                if (c.attachedRigidbody != null && !c.attachedRigidbody.isKinematic) continue; // oyuncu
                return false;
            }
        }
        return true;
    }
}
