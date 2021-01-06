using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;
public class ConvertNaObjektyEditor : EditorWindow
{
    public static bool instance = false;
    private float odl = 20f;
    private bool _kasowanie = false;
    private bool _generujDrzewa = false;
    private bool _generujDetale = false;
    private bool _łączObiekty = false;
    [MenuItem("Window/Convert/ConvertFromTerrain")]
    public static void PokażOkno()
    {
        if (!instance)
        {
            GetWindow<ConvertNaObjektyEditor>("Convert");
            instance = true;
        }
    }
    void OnGUI()
    {
        GUILayout.Label("Te okno służy do konwersji obiektów z terenu na obiekty");
        GUILayout.Space(odl);
        _kasowanie = EditorGUILayout.Toggle("Kasuj obiekty", _kasowanie);
        _generujDrzewa = EditorGUILayout.Toggle("Generuj drzewa", _generujDrzewa);
        _generujDetale = EditorGUILayout.Toggle("Generuj detale", _generujDetale);
        _łączObiekty = EditorGUILayout.Toggle("Spróbuj połączyć obiekty", _łączObiekty);
        if (GUILayout.Button("Konwertuj"))
        {
            if (Selection.activeGameObject != null)
            {
                Transform s = Selection.activeGameObject.transform;

                int czy = Konwertuj(ref s, _generujDrzewa, _generujDetale, _kasowanie, _łączObiekty);
                if (czy == 1)
                {
                    Debug.Log("Nie udała się konwersja. Zaznaczony obiekt nie posiada komponentu TerrainData, lub nie mam co konwertować");
                }
                else if (czy == 0)
                {
                    Debug.Log("Konwersja się udała");
                }
            }
            else
            {
                Debug.LogError("Nie zaznaczono terenu!!!!");
            }
        }
    }
    public static int Konwertuj(ref Transform tr, bool _generujDrzewa, bool _generujDetale, bool czyKasowac = false, bool czyłączyćObiekty = false)
    {
        Terrain ter = null;
        ter = tr.GetComponent<Terrain>();
        if (ter == null)
        {
            return 1;
        }
        TerrainData td = null;
        td = ter.terrainData;
        if (td == null)
        {
            return 1;
        }
        if (td.treeInstanceCount > 0 && _generujDrzewa)
        {
            GameObject treeParent = null;
            treeParent = GameObject.Find("Drzewa");
            if (treeParent == null)
                treeParent = new GameObject("Drzewa");
            TreeInstance[] tIns = td.treeInstances;
            TreePrototype[] tP = td.treePrototypes;
            for (int i = 0; i < td.treeInstanceCount; i++)
            {
                Quaternion qt = Quaternion.Euler(0, tIns[i].rotation, 0);
                Vector3 pos = tIns[i].position;
                pos.x *= td.size.x;
                pos.y *= td.size.y;
                pos.z *= td.size.z;
                GameObject tree = GameObject.Instantiate(tP[tIns[i].prototypeIndex].prefab, pos, qt);
                float yScale = (Mathf.Abs(tIns[i].heightScale - tIns[i].widthScale) < 0.25f) ? tIns[i].heightScale : tIns[i].widthScale;
                tree.transform.localScale = new Vector3(tIns[i].widthScale, yScale, tIns[i].widthScale);
                tree.transform.SetParent(treeParent.transform);

            }
            if (czyKasowac)
            {
                TreeInstance[] ti = new TreeInstance[0];
                td.SetTreeInstances(ti, false);
            }
            if (czyłączyćObiekty)
            {
                ŁączDzieciWJedno(treeParent.transform);
            }
        }
        else
        {
            Debug.Log("Brak drzew");
        }
        if (td.detailPatchCount > 0 && _generujDetale)
        {
            GameObject detParent = null;
            detParent = GameObject.Find("Detale");
            if (detParent == null)
                detParent = new GameObject("Detale");
            DetailPrototype[] dp = td.detailPrototypes;
            if (tr.GetComponent<Terrain>().detailObjectDensity != 0)
            {
                int detailWidth = td.detailWidth;
                int detailHeight = td.detailHeight;
                float delatilWToTerrainW = td.size.x / detailWidth;
                float delatilHToTerrainW = td.size.z / detailHeight;

                Vector3 mapPosition = ter.transform.position;

                bool toDensity = false;
                float targetDensity = 0;
                if (ter.detailObjectDensity != 1)
                {
                    targetDensity = (1 / (1f - ter.detailObjectDensity));
                    toDensity = true;
                }
                float currentDensity = 0;

                for (byte i = 0; i < dp.Length; i++)
                {
                    GameObject go = dp[i].prototype;
                    Debug.Log("Protonyp d["+i+"] = "+dp[i].prototype.name);
                    float minWidth = dp[i].minWidth;
                    float maxWidth = dp[i].maxWidth;

                    float minH = dp[i].minHeight;
                    float maxH = dp[i].maxHeight;
                        int[,] map = td.GetDetailLayer(0, 0, td.detailWidth, td.detailHeight, i);

                        List<Vector3> trawa = new List<Vector3>();

                        for (int z = 0; z < td.detailHeight; z++)
                        {
                            for (int x = 0; x < td.detailWidth; x++)
                            {
                                if (map[x, z] > 0)
                                {
                                    currentDensity += 1f;

                                    bool pass = false;

                                    if (!toDensity)
                                        pass = true;
                                    else
                                        pass = currentDensity < targetDensity;

                                    if (pass)
                                    {
                                        float _z = (x * delatilWToTerrainW) + mapPosition.z;
                                        float _x = (z * delatilHToTerrainW) + mapPosition.x;
                                        float _y = ter.SampleHeight(new Vector3(_x, 0, _z));
                                        trawa.Add(new Vector3(
                                            _x,
                                            _y,
                                            _z
                                            ));
                                    }
                                    else
                                    {
                                        currentDensity -= targetDensity;
                                    }
                                }
                            }
                        }
                        for (int k = 0; k < trawa.Count; k++)
                        {
                            Quaternion q = Quaternion.Euler(0, UnityEngine.Random.Range(0, 360), 0);
                            GameObject gok = Instantiate(go, trawa[k], q);
                            gok.transform.localScale = new Vector3(UnityEngine.Random.Range(minWidth, maxWidth),
                            UnityEngine.Random.Range(minH, maxH),
                            UnityEngine.Random.Range(minWidth, maxWidth));
                            gok.transform.SetParent(detParent.transform);
                        }
                        if (czyKasowac)
                        {
                            for (int z = 0; z < td.detailHeight; z++)
                            {
                                for (int x = 0; x < td.detailWidth; x++)
                                {
                                    if (map[x, z] > 0)
                                    {
                                        map[x, z] = 0;
                                    }
                                }
                            }
                            td.SetDetailLayer(0, 0, i, map);
                        }
                }
                if (czyłączyćObiekty)
                {
                    ŁączDzieciWJedno(detParent.transform);
                }
            }
        }
        else
        {
            Debug.Log("Brak detali");
        }
        return 1;
    }
    private static void ŁączDzieciWJedno(Transform trRodzica)
    {
        List<GameObject> gos = new List<GameObject>();
        for (int i = 0; i < trRodzica.childCount; i++)
        {
            Transform rodzic = null;
            Transform actSO = trRodzica.GetChild(i);
            MeshFilter mf = actSO.GetComponent<MeshFilter>();
            MeshRenderer mr = actSO.GetComponent<MeshRenderer>();
            Vector3[] v = mf.sharedMesh.vertices;
            Vector3[] n = mf.sharedMesh.normals;
            Vector2[] uv1 = mf.sharedMesh.uv;
            Vector2[] uv2 = mf.sharedMesh.uv2;
            int[] triangle = mf.sharedMesh.triangles;
            Vector4[] tangents = mf.sharedMesh.tangents;
            Collider[] comps = actSO.GetComponents<Collider>();
            for (byte j = 0; j < gos.Count; j++)
            {
                if (gos[j].name == actSO.name)
                {
                    rodzic = gos[j].transform;
                    if (SprawdźCzyMamMniejNiż(ref rodzic))
                    {
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            if (rodzic == null)
            {
                rodzic = StwórzObiekt(actSO.name, gos.Count, mr.sharedMaterial).transform;
                gos.Add(rodzic.gameObject);
            }
            MeshFilter mff = rodzic.GetComponent<MeshFilter>();
            Mesh m = mff.sharedMesh;
            Vector3[] nv = DodajTablice(m.vertices, v, actSO.position, actSO.localScale, actSO.transform);
            int iddMax = nv.Length - v.Length;
            Vector3[] nm = DodajTablice(m.normals, n, Vector3.zero, Vector3.one);
            Vector2[] nuv1 = DodajTablice(m.uv, uv1);
            Vector2[] nuv2 = DodajTablice(m.uv2, uv2);
            int[] nT = DodajTablice(m.triangles, triangle, iddMax);
            Vector4[] nTange = DodajTablice(m.tangents, tangents);
            m.vertices = nv;
            m.normals = nm;
            m.uv = nuv1;
            m.uv2 = nuv2;
            m.triangles = nT;
            m.tangents = nTange;
            if (comps.Length > 0)
            {
                Bounds bounds = new Bounds(rodzic.position, Vector3.zero);
                bounds.Encapsulate(m.bounds);
                for (byte j = 0; j < comps.Length; j++)
                {
                    if (ReferenceEquals(comps[j].GetType(), typeof(BoxCollider)))
                    {
                        BoxCollider t = (BoxCollider)comps[j];
                        BoxCollider bc = rodzic.gameObject.AddComponent<BoxCollider>();

                        bc.size = Vector3.Scale(t.size, actSO.localScale);
                        bc.center = new Vector3(t.center.x + actSO.position.x, t.center.y * actSO.localScale.y, t.center.z + actSO.position.z);
                    }
                    else if (ReferenceEquals(comps[j].GetType(), typeof(SphereCollider)))
                    {
                        SphereCollider t = (SphereCollider)comps[j];
                        SphereCollider bc = rodzic.gameObject.AddComponent<SphereCollider>();

                        bc.radius = t.radius*actSO.localScale.x;
                        bc.center = new Vector3(t.center.x + actSO.position.x, t.center.y * actSO.localScale.y, t.center.z + actSO.position.z);
                    }
                    else if (ReferenceEquals(comps[j].GetType(), typeof(CapsuleCollider)))
                    {
                        CapsuleCollider t = (CapsuleCollider)comps[j];
                        CapsuleCollider bc = rodzic.gameObject.AddComponent<CapsuleCollider>();

                        bc.radius = t.radius * actSO.localScale.x;
                        bc.height = t.height * actSO.localScale.y;
                        bc.center = new Vector3(t.center.x + actSO.position.x, t.center.y * actSO.localScale.y, t.center.z + actSO.position.z);
                    }
                }
                m.RecalculateBounds();
                m.RecalculateNormals();
                m.RecalculateTangents();
                mff.sharedMesh = m;
            }
        }
    }
    private static GameObject StwórzObiekt(string nazwa, int idx, Material mat)
    {
        GameObject go = new GameObject(nazwa/* + "_" + idx.ToString()*/);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        Mesh m = new Mesh();
        go.GetComponent<MeshFilter>().sharedMesh = m;
        return go;
    }

    private static bool SprawdźCzyMamMniejNiż(ref Transform go)
    {
        return (go.GetComponent<MeshFilter>().sharedMesh.vertexCount < 55000) ? true : false;
    }
    #region Dodaj tablice
    private static Vector3[] DodajTablice(Vector3[] t1, Vector3[] t2, Vector3 offset, Vector3 skala, Transform tir = null)
    {
        Vector3[] tab = new Vector3[t1.Length + t2.Length];
        for (int i = 0; i < t1.Length; i++)
        {
            tab[i] = t1[i];
        }
        int idx = t1.Length;
        for (int i = 0; i < t2.Length; i++, idx++)
        {
            if(tir != null)
            {
                t2[i] = tir.TransformDirection(t2[i]);
            }
            tab[idx] = Vector3.Scale(t2[i], skala) + offset;
        }
        return tab;
    }
    private static Vector2[] DodajTablice(Vector2[] t1, Vector2[] t2)
    {
        Vector2[] tab = new Vector2[t1.Length + t2.Length];
        for (int i = 0; i < t1.Length; i++)
        {
            tab[i] = t1[i];
        }
        int idx = t1.Length;
        for (int i = 0; i < t2.Length; i++, idx++)
        {
            tab[idx] = t2[i];
        }
        return tab;
    }
    private static Vector4[] DodajTablice(Vector4[] t1, Vector4[] t2)
    {
        Vector4[] tab = new Vector4[t1.Length + t2.Length];
        for (int i = 0; i < t1.Length; i++)
        {
            tab[i] = t1[i];
        }
        int idx = t1.Length;
        for (int i = 0; i < t2.Length; i++, idx++)
        {
            tab[idx] = t2[i];
        }
        return tab;
    }
    private static int[] DodajTablice(int[] t1, int[] t2, int idvMax)  //idvMax - to zmienna przechowyjąca wartość ilości vertów
    {
        int[] tab = new int[t1.Length + t2.Length];
        for (int i = 0; i < t1.Length; i++)
        {
            tab[i] = t1[i];
        }
        int idx = t1.Length;
        for (int i = 0; i < t2.Length; i++, idx++)
        {
            tab[idx] = t2[i] + idvMax;
        }
        return tab;
    }
    #endregion
}