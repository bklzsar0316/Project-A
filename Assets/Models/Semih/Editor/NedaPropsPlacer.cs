#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Semih_Prototype sahnesine Neda1 modellerini yerlestirir.
/// - Hepsi tek bir "NedaProps" kok nesnesi altinda toplanir (sil = tek tik).
/// - Tekrar calistirilabilir: nesneleri adlariyla bulur.
/// - Sahneyi KAYDETMEZ; yalnizca "kaydedilmemis" olarak isaretler. Kaydetmek sana ait (Ctrl+S).
/// - Konumlar Map nesnesinin yerel uzayinda saklanir: Map tasinirsa nesneler onunla gider.
/// </summary>
public static class NedaPropsPlacer
{
    // ------------------------------------------------------------------ ayarlar
    const string ScenePath = "Assets/Scenes/Semih_Prototype.unity";
    const string RootName = "NedaProps";
    const string MapName = "Map";
    const string ModelDir = "Assets/Models/Neda1/";

    // Konumlarin tasarlandigi andaki Map konumu (dunya). Yalnizca farki onemli.
    static readonly Vector3 DesignMapPos = new Vector3(-1.12401f, 0f, -1.6772f);

    // Map'in zemin yuzeyi (sahnedeki HospitalChair'lerle ayni).
    const float FloorY = -0.0916f;

    // Yatagin bas ucu (yuksek baslik) mesh'in yerel -z ucundadir. Yaw 180 ile bas ucu dunya +z'ye
    // (kuzey yan duvara) bakar; yatak bu duvara yaslanir, arka duvar ekran/masa icin bos kalir.
    const float BedYaw = 180f;
    const float BedHeadWallGap = 0.03f;    // bas ucu ile kuzey yan duvar arasi

    // Yatak olcegi. Istenen deger; odaya sigmazsa otomatik olarak sigan en buyuk deger kullanilir.
    // HospitalBed.fbx (2026-11-19 yeniden export): uzun kenar 2.0 m, origin taban ortasinda.
    // 1.19 = eski sahnedeki gorunen boyut (yaklasik 2.38 x 1.28 x 1.28 m). Gercek 2.0 m yatak icin 1f yap.
    // Eski deger 3 idi: eski FBX'te yataktan 4.75 m uzakta duran bir kutu oldugu icin olculer yanlisti.
    const float BED_SCALE = 1.19f;

    // Yatak (olcek = 1) olculeri (m), Unity yerel ekseninde. Yatagin uzun ekseni Unity z'dir
    // (arka duvara paralel), eni x'tir.
    const float BedBaseLength = 2.0f;      // z (uzun eksen)
    const float BedBaseWidth = 1.079f;     // x
    const float BedCenterFromBackWall = 3.07f; // yatak merkezinin arka duvarin ic yuzune uzakligi (kapidan yataga ~2 m bosluk)
    const float BedWallGap = 0.05f;        // yatak ile arka duvar arasi en az pay
    const float RoomMargin = 0.30f;        // acik kenar / yan duvarlardan en az pay
    const float RoomDepthZ = 5.82f;        // tedavi odalarinin ic derinligi (z)

    // Eski surumlerde yerlestirilmis, artik yonetilmeyen nesneler: Update bunlari siler.
    static readonly string[] RemovedNamePrefixes = { "HospitalCurtain_" };

    // Tedavi odalari: backX = arka (kapali) duvarin ic yuzunun x'i, openX = acik kenarin x'i,
    // headDirX = bas ucunun yonu (+1 dogu, -1 bati)
    struct TreatmentRoom
    {
        public string Group, Tag;
        public float BackX, OpenX, HeadDirX, CenterZ;
        public TreatmentRoom(string group, string tag, float backX, float openX, float headDirX, float centerZ)
        { Group = group; Tag = tag; BackX = backX; OpenX = openX; HeadDirX = headDirX; CenterZ = centerZ; }
    }

    static readonly TreatmentRoom[] TreatmentRooms =
    {
        new TreatmentRoom("Room1_Treatment", "R1", -4.234f, -10.144f, +1f, 19.30f),
        new TreatmentRoom("Room2_Treatment", "R2", -4.234f, -10.144f, +1f, 34.32f),
        new TreatmentRoom("Room3_Treatment", "R3", -22.02f, -16.12f, -1f, 34.32f),
        new TreatmentRoom("Room4_Treatment", "R4", -22.02f, -16.12f, -1f, 19.30f),
    };

    // ------------------------------------------------------------------ menu
    [MenuItem("Tools/Neda/Place Props (Update)")]
    static void PlaceUpdate() { Run(false); }

    [MenuItem("Tools/Neda/Place Props (Missing Only)")]
    static void PlaceMissing() { Run(true); }

    [MenuItem("Tools/Neda/Remove NedaProps")]
    static void Remove()
    {
        if (!Preflight(out Scene scene, out _)) return;
        GameObject root = FindRoot(scene);
        if (root == null)
        {
            Debug.Log("[NedaProps] Silinecek 'NedaProps' bulunamadi.");
            return;
        }
        Undo.DestroyObjectImmediate(root);
        EditorSceneManager.MarkSceneDirty(scene);
        Debug.Log("[NedaProps] 'NedaProps' silindi (Ctrl+Z ile geri alinabilir). Sahne kaydedilmedi.");
    }

    // ------------------------------------------------------------------ ana akis
    static void Run(bool missingOnly)
    {
        if (!Preflight(out Scene scene, out Transform map)) return;

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName("Place Neda Props");
        int undoGroup = Undo.GetCurrentGroup();

        var ctx = new Ctx { Map = map, MissingOnly = missingOnly };

        GameObject root = FindRoot(scene);
        if (root == null)
        {
            root = new GameObject(RootName);
            SceneManager.MoveGameObjectToScene(root, scene);
            Undo.RegisterCreatedObjectUndo(root, "Create NedaProps");
        }
        root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        root.transform.localScale = Vector3.one;
        ctx.Root = root.transform;

        // ---- Eski surumlerden kalan (artik yonetilmeyen) nesneleri sil: yalnizca Update kipinde ----
        int removedLegacy = 0;
        if (!missingOnly) removedLegacy = RemoveLegacy(ctx.Root);

        // ---- Yatak olcegi: istenen deger odaya sigmazsa sigan en buyugu kullan ----
        float bedScale = ComputeBedScale(out string scaleReport);

        // ---- Tedavi odalari: Room1-4 ----
        foreach (TreatmentRoom room in TreatmentRooms)
            PlaceTreatmentRoom(ctx, room, bedScale);

        // ---- Room6: X-ray, BigScreen dogu duvarin ic yuzunde (x -4.24), alt kenar zeminden 1.0 m ----
        {
            Transform g = Group(ctx, "Room6_XRay");
            GameObject go = Spawn(ctx, g, "BigScreen_R6", "BigScreen");
            if (go != null)
            {
                const float wallInnerX = -4.24f, halfDepth = 0.045f, gap = 0.01f;
                Vector3 p = new Vector3(wallInnerX - halfDepth - gap, FloorY + 1.0f, -18.16f);
                SetPose(ctx, go.transform, p, 0f);
            }
        }

        // ---- Room7: ilac / depo, MedicineCabinet bati duvara yasli, on yuzu dogu (acik kenar) ----
        {
            Transform g = Group(ctx, "Room7_Storage");
            GameObject go = Spawn(ctx, g, "MedicineCabinet_R7", "MedicineCabinet");
            if (go != null)
            {
                const float wallInnerX = -22.013f, halfDepth = 0.227f, gap = 0.01f;
                Vector3 p = new Vector3(wallInnerX + halfDepth + gap, FloorY, -18.16f);
                SetPose(ctx, go.transform, p, 180f);
                // FBX origin'i tabanda degil: zemine ve bati duvarina mesh sinirlariyla oturt
                SnapToFloor(go, ctx.P(new Vector3(0f, FloorY, 0f)).y);
                go.transform.position += Vector3.right * (ctx.P(new Vector3(wallInnerX + gap, 0f, 0f)).x - WorldBounds(go).min.x);
                AddBoxCollider(go);
            }
        }

        // ---- Guvenlik kameralari ----
        {
            Transform g = Group(ctx, "Cameras");

            // Koridor: Room5'in guney duvarinin koridor yuzu (z 43.30), aciklik kenarindan 0.24 m iceride.
            GameObject c1 = Spawn(ctx, g, "SecurityCamera_Corridor", "SecurityCamera");
            if (c1 != null)
                MountCamera(ctx, c1.transform,
                    wallPoint: new Vector3(-9.90f, 3.00f, 43.303f),
                    outward: new Vector3(0f, 0f, -1f),
                    target: new Vector3(-13.1f, 0.5f, 25.0f));

            // Bekleme alani: Doctor Room'un dogu duvarinin dis yuzu (x 2.176), kuzey koseye 0.26 m.
            GameObject c2 = Spawn(ctx, g, "SecurityCamera_Waiting", "SecurityCamera");
            if (c2 != null)
                MountCamera(ctx, c2.transform,
                    wallPoint: new Vector3(2.176f, 3.00f, 1.35f),
                    outward: new Vector3(1f, 0f, 0f),
                    target: new Vector3(7.6f, 0.8f, 0.2f));
        }

        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene); // KAYDETMEZ, sadece isaretler

        Debug.Log("[NedaProps] " + scaleReport);
        Debug.Log("[NedaProps] Bitti (" + (missingOnly ? "yalnizca eksikler" : "guncelle") + "): "
                  + ctx.Created + " nesne olusturuldu, " + ctx.Skipped + " atlandi, "
                  + removedLegacy + " eski nesne silindi. "
                  + "Sahne KAYDEDILMEDI; kontrol edip Ctrl+S ile sen kaydet. Geri almak icin Ctrl+Z.");
    }

    // ------------------------------------------------------------------ yatak olcegi
    static float ComputeBedScale(out string report)
    {
        float fit = float.MaxValue;
        foreach (TreatmentRoom r in TreatmentRooms)
        {
            float roomLength = Mathf.Abs(r.OpenX - r.BackX);                 // arka duvardan acik kenara (x)
            float halfW = 0.5f * BedBaseWidth;
            float byBackWall = (BedCenterFromBackWall - BedWallGap) / halfW;
            float byOpenEdge = (roomLength - BedCenterFromBackWall - RoomMargin) / halfW;
            float byDepth = (RoomDepthZ - 2f * RoomMargin) / BedBaseLength;  // uzun eksen z boyunca
            fit = Mathf.Min(fit, Mathf.Min(byDepth, Mathf.Min(byBackWall, byOpenEdge)));
        }
        fit = Mathf.Floor(fit * 100f) / 100f;                                // 2 basamaga asagi yuvarla

        float used = Mathf.Min(BED_SCALE, fit);
        report = "Yatak olcegi: istenen " + BED_SCALE.ToString("0.###") + ", odaya sigan en buyuk " + fit.ToString("0.###")
                 + " -> kullanilan " + used.ToString("0.###")
                 + (used < BED_SCALE ? " (BED_SCALE odaya sigmadigi icin dusuruldu)" : "")
                 + ". Yatak " + (BedBaseLength * used).ToString("0.00") + " (z) x " + (BedBaseWidth * used).ToString("0.00")
                 + " (x) m; merkezi arka duvardan " + BedCenterFromBackWall.ToString("0.00") + " m.";
        return used;
    }

    // ------------------------------------------------------------------ tedavi odasi
    static void PlaceTreatmentRoom(Ctx ctx, TreatmentRoom room, float bedScale)
    {
        Transform g = Group(ctx, room.Group);

        // Yatak merkezi arka duvardan sabit uzaklikta (uzun ekseni z, duvara paralel)
        float bedX = room.BackX - room.HeadDirX * BedCenterFromBackWall;
        Vector3 bed = new Vector3(bedX, FloorY, room.CenterZ);

        GameObject bedGo = Spawn(ctx, g, "HospitalBed_" + room.Tag, "HospitalBed");
        if (bedGo != null)
        {
            SetPose(ctx, bedGo.transform, bed, BedYaw);
            bedGo.transform.localScale = Vector3.one * bedScale;
            SnapToFloor(bedGo, ctx.P(new Vector3(0f, FloorY, 0f)).y); // origin'e guvenme: mesh sinirlari ile olc
            // bas ucu kuzey yan duvara (ic yuz = CenterZ + RoomDepthZ/2) yaslanir
            float northInnerZ = ctx.P(new Vector3(0f, 0f, room.CenterZ + 0.5f * RoomDepthZ)).z;
            bedGo.transform.position += Vector3.forward * ((northInnerZ - BedHeadWallGap) - WorldBounds(bedGo).max.z);
            AddBoxCollider(bedGo);                                  // yerel uzayda hesaplanir, olcekle birlikte buyur
        }
    }

    // Mesh sinirlarindan dunya AABB'si. Renderer.bounds yeni olusturulan nesnelerde ayni karede
    // guncel olmayabildigi icin mesh kose noktalari kullanilir.
    static Bounds WorldBounds(GameObject go)
    {
        bool first = true;
        Bounds b = new Bounds(go.transform.position, Vector3.zero);
        foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;
            Bounds mb = mf.sharedMesh.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 c = mb.center + Vector3.Scale(mb.extents, new Vector3(
                    (i & 1) == 0 ? -1f : 1f, (i & 2) == 0 ? -1f : 1f, (i & 4) == 0 ? -1f : 1f));
                Vector3 w = mf.transform.TransformPoint(c);
                if (first) { b = new Bounds(w, Vector3.zero); first = false; } else b.Encapsulate(w);
            }
        }
        return b;
    }

    // Nesnenin en alt noktasini verilen dunya y'sine oturtur.
    static void SnapToFloor(GameObject go, float worldFloorY)
    {
        go.transform.position += Vector3.up * (worldFloorY - WorldBounds(go).min.y);
    }

    // ------------------------------------------------------------------ kamera
    // Taban plakasi duvara yaslanir; model +Y ekseni (mercek topu) odaya/hedefe bakar.
    static void MountCamera(Ctx ctx, Transform cam, Vector3 wallPoint, Vector3 outward, Vector3 target)
    {
        const float tiltDeg = 20f;     // asagi egim
        const float standOff = 0.10f;  // duvardan pivot mesafesi

        Vector3 n = outward.normalized;
        Vector3 h = target - wallPoint;
        h.y = 0f;
        h = h.sqrMagnitude < 1e-6f ? n : h.normalized;
        if (Vector3.Dot(h, n) < 0.3f) h = (h + n).normalized; // duvarin arkasina bakmasin

        float t = tiltDeg * Mathf.Deg2Rad;
        Vector3 view = (h * Mathf.Cos(t) + Vector3.down * Mathf.Sin(t)).normalized;

        Vector3 posDesign = wallPoint + n * standOff;
        Vector3 pos = ctx.P(posDesign);
        Vector3 viewWorld = ctx.Map.TransformDirection(view);

        cam.position = pos;
        cam.rotation = Quaternion.FromToRotation(Vector3.up, viewWorld);
    }

    // ------------------------------------------------------------------ yardimcilar
    class Ctx
    {
        public Transform Map;
        public Transform Root;
        public bool MissingOnly;
        public int Created, Skipped;

        // tasarim dunyasi -> gercek dunya (Map'in yerel uzayi uzerinden)
        public Vector3 P(Vector3 designWorld)
        {
            return Map.TransformPoint(designWorld - DesignMapPos);
        }
    }

    static int RemoveLegacy(Transform root)
    {
        var victims = new List<GameObject>();
        foreach (Transform group in root)
        {
            foreach (Transform child in group)
            {
                foreach (string prefix in RemovedNamePrefixes)
                {
                    if (child.name.StartsWith(prefix)) { victims.Add(child.gameObject); break; }
                }
            }
        }
        foreach (GameObject v in victims) Undo.DestroyObjectImmediate(v);
        return victims.Count;
    }

    static void SetPose(Ctx ctx, Transform t, Vector3 designPos, float yaw)
    {
        t.position = ctx.P(designPos);
        t.rotation = ctx.Map.rotation * Quaternion.Euler(0f, yaw, 0f);
    }

    static Transform Group(Ctx ctx, string name)
    {
        Transform g = ctx.Root.Find(name);
        if (g != null) return g;
        var go = new GameObject(name);
        Undo.RegisterCreatedObjectUndo(go, "Create group " + name);
        go.transform.SetParent(ctx.Root, false);
        return go.transform;
    }

    static GameObject Spawn(Ctx ctx, Transform parent, string objName, string fbxName)
    {
        Transform existing = parent.Find(objName);
        if (existing != null)
        {
            if (ctx.MissingOnly) { ctx.Skipped++; return null; }
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        string path = ModelDir + fbxName + ".fbx";
        GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null)
        {
            Debug.LogError("[NedaProps] FBX bulunamadi: " + path);
            return null;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
        Undo.RegisterCreatedObjectUndo(go, "Create " + objName);
        go.name = objName;
        go.transform.localScale = Vector3.one;
        ctx.Created++;
        return go;
    }

    static void AddBoxCollider(GameObject go)
    {
        if (go.GetComponent<Collider>() != null) return;

        Transform root = go.transform;
        bool first = true;
        Bounds b = new Bounds();
        foreach (MeshFilter mf in go.GetComponentsInChildren<MeshFilter>())
        {
            Mesh m = mf.sharedMesh;
            if (m == null) continue;
            Bounds mb = m.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = mb.center + new Vector3(
                    ((i & 1) == 0 ? -1f : 1f) * mb.extents.x,
                    ((i & 2) == 0 ? -1f : 1f) * mb.extents.y,
                    ((i & 4) == 0 ? -1f : 1f) * mb.extents.z);
                // yerel uzay (root'un kendi olcegi dahil edilmez): collider olcekle birlikte buyur
                Vector3 p = root.InverseTransformPoint(mf.transform.TransformPoint(corner));
                if (first) { b = new Bounds(p, Vector3.zero); first = false; }
                else b.Encapsulate(p);
            }
        }
        if (first) return;

        BoxCollider col = Undo.AddComponent<BoxCollider>(go);
        col.center = b.center;
        col.size = b.size;
    }

    static GameObject FindRoot(Scene scene)
    {
        foreach (GameObject g in scene.GetRootGameObjects())
            if (g.name == RootName) return g;
        return null;
    }

    static bool Preflight(out Scene scene, out Transform map)
    {
        scene = SceneManager.GetActiveScene();
        map = null;

        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogError("[NedaProps] Play modunda calismaz.");
            return false;
        }
        if (scene.path != ScenePath)
        {
            Debug.LogError("[NedaProps] Etkin sahne '" + ScenePath + "' olmali, su an: '" + scene.path + "'. Hicbir sey yapilmadi.");
            return false;
        }
        foreach (GameObject g in scene.GetRootGameObjects())
        {
            if (g.name == MapName) { map = g.transform; break; }
        }
        if (map == null)
        {
            Debug.LogError("[NedaProps] Sahnede kokte '" + MapName + "' adli nesne yok. Hicbir sey yapilmadi.");
            return false;
        }
        return true;
    }
}
#endif
