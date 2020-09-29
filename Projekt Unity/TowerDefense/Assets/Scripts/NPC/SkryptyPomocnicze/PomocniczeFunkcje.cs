using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public static class PomocniczeFunkcje
{
    public static StrukturaDrzewa korzeńDrzewaPozycji = null;
    public static NPCClass celWrogów = null;
    private static string test = "Assets/Resources/Debug.txt";
    public static Dictionary<string, StrukturaDoPoolowania> stosTrupów = null;
    public static SpawnerHord spawnerHord = null;
    public static ManagerGryScript managerGryScript = null;
    public static SpawnBudynki spawnBudynki = null;
    public static List<InformacjeDlaPolWież>[,] tablicaWież = null;
    public static float distXZ = 5;
    public static ushort aktualneGranicaTab = 0;
    #region Obsługa położenia myszy względem ekranu
    public static Vector3 OkreślPozycjęŚwiataKursora(Vector3 lastPos)
    {
        Ray ray;
#if UNITY_STANDALONE
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
#if UNITY_ANDROID
        ray = Camera.main.ScreenPointToRay(Input.GetTouch(0));
#endif
        RaycastHit[] hit = new RaycastHit[1];
        int layerMask = ~(0 << 8);
        int hits = Physics.RaycastNonAlloc(ray, hit, 50f, layerMask, QueryTriggerInteraction.Collide);
        if (hits > 0)
        {
            if (hit[hits - 1].collider.GetType() == typeof(TerrainCollider))
            {
                return hit[hits - 1].point;
            }
        }
        return lastPos;
    }
    public static NPCClass OkreślKlikniętyNPC(ref NPCClass lastNPCCLass)
    {
        Ray ray;
#if UNITY_STANDALONE
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
#endif
#if UNITY_ANDROID
        ray = Camera.main.ScreenPointToRay(Input.GetTouch(0));
#endif
        int layerMask = (1 << 8) | (1 << 0);
        RaycastHit[] hit = new RaycastHit[1];
        int hits = Physics.RaycastNonAlloc(ray, hit, 50f, layerMask, QueryTriggerInteraction.Collide);
        if (hits > 0)
        {
            if (hit[hits - 1].collider.CompareTag("Budynek") || hit[hits - 1].collider.CompareTag("NPC"))
            {
                return hit[hits - 1].collider.GetComponent<NPCClass>();
            }
            else if (hit[hits - 1].collider.GetType() == typeof(TerrainCollider))
            {
                return null;
            }
        }
        return lastNPCCLass;
    }
    #endregion
    #region Drzewo pozycji
    //Wyszukuje najbliższy budynek w drzewie dla punktu wysłanego w parametrze pozycjaWyszukiwujacego
    public static Component WyszukajWDrzewie(ref StrukturaDrzewa korzeń, Vector3 pozycjaWyszukiwującego)
    {
        if (korzeń == null)
        {
            Debug.Log("Korzeń jest null");
            return null;
        }
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
        float minDistance = 10000f;
        Component compDoZwrotu = korzeń.komponentGałęzi;
        while (true)
        {
            float deltaX = aktualnieSprawdzanyNode.pozycjaGałęzi.x - pozycjaWyszukiwującego.x;
            float deltaZ = aktualnieSprawdzanyNode.pozycjaGałęzi.z - pozycjaWyszukiwującego.z;
            float d = Vector3.Distance(aktualnieSprawdzanyNode.pozycjaGałęzi, pozycjaWyszukiwującego);
            if (d < minDistance)
            {
                minDistance = d;
                compDoZwrotu = aktualnieSprawdzanyNode.komponentGałęzi;
            }
            if (deltaX > 0)  //Lewa strona drzewa
            {
                if (deltaZ > 0)  //Dolna strona drzewa
                {
                    if (aktualnieSprawdzanyNode.MxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else    //Górna strona drzewa
                {
                    if (aktualnieSprawdzanyNode.MxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else    //Prawa strona drzewa
            {
                if (deltaZ > 0)
                {
                    if (aktualnieSprawdzanyNode.PxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanyNode.PxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return compDoZwrotu;
    }

    public static void SkasujElementDrzewa(ref StrukturaDrzewa korzeń, Component _komponentDoSkasowania)
    {
        if (korzeń == null)
        {
            Debug.Log("Korzeń jest null");
            return;
        }
        StrukturaDrzewa[,] ponownieUstaw = ZnajdźElementPoKomponencie(ref korzeń, _komponentDoSkasowania);
        if (ponownieUstaw == null)
        {
            //Debug.Log("Nie odnalazłem elemenu drzewa do skasowania");
            return;
        }
        for (byte i = 0; i < 2; i++)
        {
            for (byte j = 0; j < 2; j++)
            {
                if (ponownieUstaw[i, j] != null)
                {
                    DodajDoDrzewaPozycji(ref korzeń, ponownieUstaw[i, j]);
                }
            }
        }
    }
    private static StrukturaDrzewa[,] ZnajdźElementPoKomponencie(ref StrukturaDrzewa korzeń, Component _com)
    {
        StrukturaDrzewa aktualnyElement = korzeń;
        Vector3 pos = _com.transform.position;
        StrukturaDrzewa prev = null;
        sbyte idx = -1; //0 +X+Z, 1 - +X-Z, 2 - -X+Z, 3 - -X-Z
        while (true)
        {
            if (_com == aktualnyElement.komponentGałęzi)
            {
                //Znalazłem :)
                StrukturaDrzewa[,] tabToReturn = { { null, null }, { null, null } };
                if (aktualnyElement.PxPz != null)
                {
                    tabToReturn[0, 0] = aktualnyElement.PxPz;
                }
                if (aktualnyElement.PxMz != null)
                {
                    tabToReturn[0, 1] = aktualnyElement.PxMz;
                }
                if (aktualnyElement.MxPz != null)
                {
                    tabToReturn[1, 0] = aktualnyElement.MxPz;
                }
                if (aktualnyElement.MxMz != null)
                {
                    tabToReturn[1, 1] = aktualnyElement.MxMz;
                }
                if (prev != null && idx > -1)
                {
                    switch (idx)
                    {
                        case 0:
                            prev.PxPz = null;
                            break;
                        case 1:
                            prev.PxMz = null;
                            break;
                        case 2:
                            prev.MxPz = null;
                            break;
                        case 3:
                            prev.MxMz = null;
                            break;

                    }
                }
                return tabToReturn;
            }
            if (pos.x < aktualnyElement.pozycjaGałęzi.x)
            {
                if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                {
                    if (aktualnyElement.MxMz != null)
                    {
                        idx = 3;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.MxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnyElement.MxPz != null)
                    {
                        idx = 2;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.MxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            else
            {
                if (pos.z < aktualnyElement.pozycjaGałęzi.z)
                {
                    if (aktualnyElement.PxMz != null)
                    {
                        idx = 1;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.PxMz;
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    if (aktualnyElement.PxPz != null)
                    {
                        idx = 0;
                        prev = aktualnyElement;
                        aktualnyElement = aktualnyElement.PxPz;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
        return null;
    }
    private static void DodajDoDrzewaPozycji(ref StrukturaDrzewa korzeń, StrukturaDrzewa elementDodawany)
    {
        StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
        Vector3 posKom = elementDodawany.pozycjaGałęzi;
        while (true)
        {
            if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
                //SaveInformationInDebug("If1 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.MxMz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                }
                else
                {
                    aktualnieSprawdzanyNode.MxMz = elementDodawany;
                    break;
                }
            }
            else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
                //SaveInformationInDebug("If2 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.PxMz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                }
                else
                {
                    aktualnieSprawdzanyNode.PxMz = elementDodawany;
                    break;
                }
            }
            else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z >= aktualnieSprawdzanyNode.pozycjaGałęzi.z)
            {
                //SaveInformationInDebug("If3 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.PxPz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                }
                else
                {
                    aktualnieSprawdzanyNode.PxPz = elementDodawany;
                    break;
                }
            }
            else
            {
                //SaveInformationInDebug("If4 dla obiektu o pozycji " + aktualnieSprawdzanyNode.pozycjaGałęzi.ToString() + " gdzie dodawany obiekt ma pozycję " + elementDodawany.pozycjaGałęzi);
                if (aktualnieSprawdzanyNode.MxPz != null)
                {
                    aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                }
                else    //
                {
                    aktualnieSprawdzanyNode.MxPz = elementDodawany;
                    break;
                }
            }
        }
    }
    public static void DodajDoDrzewaPozycji(Component _component, ref StrukturaDrzewa korzeń)
    {
        if (korzeń == null)
        {
            korzeń = new StrukturaDrzewa(_component);
        }
        else
        {
            StrukturaDrzewa aktualnieSprawdzanyNode = korzeń;
            Vector3 posKom = _component.transform.position;
            while (true)
            {
                if (posKom.x < aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.MxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxMz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.MxMz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z < aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.PxMz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxMz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.PxMz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else if (posKom.x >= aktualnieSprawdzanyNode.pozycjaGałęzi.x && posKom.z >= aktualnieSprawdzanyNode.pozycjaGałęzi.z)
                {
                    if (aktualnieSprawdzanyNode.PxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.PxPz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.PxPz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
                else
                {
                    if (aktualnieSprawdzanyNode.MxPz != null)
                    {
                        aktualnieSprawdzanyNode = aktualnieSprawdzanyNode.MxPz;
                    }
                    else
                    {
                        aktualnieSprawdzanyNode.MxPz = new StrukturaDrzewa(_component);
                        break;
                    }
                }
            }
        }
    }
    #endregion
    #region AI
    public static void ZwykłeAI(NPCClass pObiekt)
    {
        if (pObiekt == null)
            return;
        if (pObiekt.cel == null)
        {
            //Wyszukaj cel
            pObiekt.cel = WyszukajWDrzewie(ref korzeńDrzewaPozycji, pObiekt.transform.position) as NPCClass;
        }
        else
        {
            NPCClass cObiekt = pObiekt.cel.GetComponent<NPCClass>();
            if (cObiekt == null)
                return;
            if (cObiekt.AktualneŻycie <= 0)
            {
                if (pObiekt.cel != celWrogów)
                {
                    pObiekt.cel = WyszukajWDrzewie(ref korzeńDrzewaPozycji, cObiekt.transform.position) as NPCClass;
                    pObiekt.ResetujŚciezkę();
                }
            }
            else
            {
                float d = Vector3.Distance(pObiekt.transform.position, cObiekt.transform.position) - 0.25f;
                if (d <= pObiekt.zasięgAtaku)
                {
                    //Atakuj
                    pObiekt.Atakuj((d <= 3f + cObiekt.PobierzGranice()) ? true : false);
                }
            }
        }
    }
    public static void DodajDoStosuTrupów(KonkretnyNPCDynamiczny dodajDoTrupów)
    {
        if (stosTrupów == null)
            stosTrupów = new Dictionary<string, StrukturaDoPoolowania>();

        if (!stosTrupów.ContainsKey(dodajDoTrupów.nazwa))
        {
            StrukturaDoPoolowania sdp = new StrukturaDoPoolowania();
            sdp.nazwa = dodajDoTrupów.nazwa;
            sdp.listaObiektówPoolingu = new List<KonkretnyNPCDynamiczny>();
            sdp.listaObiektówPoolingu.Add(dodajDoTrupów);
            stosTrupów.Add(dodajDoTrupów.nazwa, sdp);
        }
        else
        {
            stosTrupów[dodajDoTrupów.nazwa].listaObiektówPoolingu.Add(dodajDoTrupów);
        }
        dodajDoTrupów.transform.position = new Vector3(0, -20f, 0);
    }
    public static GameObject ZwróćOBiektPoolowany(string nazwaZpoolera)
    {
        try
        {
            if (stosTrupów[nazwaZpoolera].listaObiektówPoolingu != null && stosTrupów[nazwaZpoolera].listaObiektówPoolingu.Count > 0)
            {
                int t = stosTrupów[nazwaZpoolera].listaObiektówPoolingu.Count - 1;
                GameObject objToReturn = stosTrupów[nazwaZpoolera].listaObiektówPoolingu[t].gameObject;
                stosTrupów[nazwaZpoolera].listaObiektówPoolingu.RemoveAt(t);
                return objToReturn;
            }
        }
        catch
        {
            //Debug.Log(nazwaZpoolera);
        }

        return null;
    }
    #endregion
    public static void SaveInformationInDebug(string s)
    {
        StreamWriter writer = new StreamWriter(test, true);
        writer.WriteLine(s);
        writer.Close();
    }
    public static void ResetujWszystko()
    {
        korzeńDrzewaPozycji = null;
        celWrogów = null;
        stosTrupów.Clear();
        stosTrupów = null;
        tablicaWież = null;
    }
    public static byte[] ZwrócIndeksyWTablicy(Vector3 pozycja)
    {
        byte x = (byte)(Mathf.FloorToInt((pozycja.x-aktualneGranicaTab)/distXZ));
        byte z = (byte)(Mathf.FloorToInt((pozycja.z-aktualneGranicaTab)/distXZ));
        return new byte[] {x, z};
    }
}
