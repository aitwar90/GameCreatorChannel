using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class PomocniczeFunkcje
{
    public static StrukturaDrzewa korzeńDrzewaPozycji = null;
    public static NPCClass celWrogów = null;
    private static string test = "Assets/Resources/Debug.txt";
    public static Dictionary<string, StrukturaDoPoolowania> stosTrupów = null;
    public static SpawnerHord spawnerHord = null;
    public static ManagerGryScript managerGryScript = null;
    public static MainMenu mainMenu = null;
    public static SpawnBudynki spawnBudynki = null;
    public static MuzykaScript muzyka = null;
    public static List<InformacjeDlaPolWież>[,] tablicaWież = null;
    public static float distXZ = 5;
    public static ushort odblokowanyPoziomEpoki = 1;
    public static byte odblokowaneEpoki = 1;
    public static ushort aktualneGranicaTab = 0;
    public static Camera oCam = null;
    public static EventSystem eSystem = null;
    public static GraphicRaycaster gr = null;
    public static sbyte poHerbacie = -1;
    private static RaycastHit[] tabHit = null;
    #region Obsługa położenia myszy względem ekranu
    public static Vector3 OkreślPozycjęŚwiataKursora(Vector3 lastPos, ref bool hitUI)
    {
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        Vector2 posK = Vector2.zero;

#if UNITY_STANDALONE
        posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                posK = Input.GetTouch(0).position;
            }
            else
            {
                return lastPos;
            }
        }
#endif
        if (CzyKliknalemUI())
        {
            hitUI = true;
            return lastPos;
        }
        else
        {
            hitUI = false;
        }
        RaycastHit[] rh = ZwrócHity(ref oCam, posK);
        if(rh == null)
            return lastPos;
        if (poHerbacie > 0)
        {
            if (rh[poHerbacie - 1].collider.GetType() == typeof(TerrainCollider))
            {
                return rh[poHerbacie - 1].point;
            }
        }
        return lastPos;
    }
    public static NPCClass OkreślKlikniętyNPC(ref NPCClass lastNPCCLass)
    {
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        Vector2 posK = Vector2.zero;
        #if UNITY_STANDALONE
        posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                posK = Input.GetTouch(0).position;
            }
            else
            {
                return lastNPCCLass;
            }
        }
#endif
        RaycastHit[] rh = ZwrócHity(ref oCam, posK);
        if(rh == null)
            return lastNPCCLass;
        if (poHerbacie > 0)
        {
            return ZwrócNaPodstawieHit(ref rh, ref lastNPCCLass);
        }
        return lastNPCCLass;
    }
    private static NPCClass ZwrócNaPodstawieHit(ref RaycastHit[] rays, ref NPCClass lastClass)
    {
        int tempIBudynek = -1, tempITeren = -1;
        if (poHerbacie > 0)
        {
            for (int i = poHerbacie - 1; i >= 0; i--)
            {
                if (rays[i].collider.CompareTag("Budynek") || rays[i].collider.CompareTag("NPC"))
                {
                    tempIBudynek = i;
                    break;
                }
                else if (rays[i].collider.GetType() == typeof(TerrainCollider))
                {
                    tempITeren = i;
                }
            }
        }
        if (tempIBudynek > -1)
        {
            return rays[tempIBudynek].collider.GetComponent<NPCClass>();
        }
        else if (tempITeren > -1)
        {
            return null;
        }
        else
            return lastClass;
    }
     private static RaycastHit[] ZwrócHity(ref Camera camera, Vector2 pozycjaK)
    {
        if (poHerbacie < 0) //Już raz wyszukałem w tej turze colidery
        {
            Ray ray = camera.ScreenPointToRay(pozycjaK);
            ray.direction = ray.direction*40f;
            int layerMask = ~((0 << 8) | (0 << 0));
            if(tabHit == null)
                tabHit = new RaycastHit[2];
            poHerbacie = (sbyte)Physics.RaycastNonAlloc(ray, tabHit, 40f, layerMask, QueryTriggerInteraction.Collide);
        }
        if(poHerbacie > 0)
            return tabHit;
        else
            return null;
    }
        public static bool CzyKliknalemUI()
    {
        int fingerID = -1;
        if (!Input.mousePresent && Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled)
            {
                return spawnBudynki.KlikUI;
            }
            fingerID = t.fingerId;
        }
        if (EventSystem.current.IsPointerOverGameObject(fingerID))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void ResetujDaneRaycast()
    {
        poHerbacie = -1;
        tabHit = null;
    }
    #endregion
    #region Drzewo pozycji
    //Wyszukuje najbliższy budynek w drzewie dla punktu wysłanego w parametrze pozycjaWyszukiwujacego
    public static Component WyszukajWDrzewie(ref StrukturaDrzewa korzeń, Vector3 pozycjaWyszukiwującego)
    {
        if (korzeń == null)
        {
#if UNITY_STANDALONE
            Debug.Log("Korzeń jest null");
#endif
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
#if UNITY_STANDALONE
            Debug.Log("Korzeń jest null");
#endif
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
    public static bool ZwykłeAI(NPCClass pObiekt)
    {
        if (pObiekt == null)
        {
            return false;
        }
        if (pObiekt.cel == null)
        {
            //Wyszukaj cel
            pObiekt.cel = PomocniczeFunkcje.WyszukajWDrzewie(ref korzeńDrzewaPozycji, pObiekt.transform.position) as NPCClass;
            if (pObiekt.cel != null)
            {
                pObiekt.ObsluzAnimacje("haveTarget", true);
            }
            else
            {
                if (!pObiekt.odgłosyNPC.isPlaying)
                {
                    PomocniczeFunkcje.muzyka.WłączWyłączClip(ref pObiekt.odgłosyNPC, true, TagZEpoka("Idle", pObiekt.epokaNPC, pObiekt.tagRodzajDoDźwięków), true);
                }
            }
        }
        else
        {
            NPCClass cObiekt = pObiekt.cel.GetComponent<NPCClass>();
            if (cObiekt == null)
            {
                return false;
            }
            if (cObiekt.AktualneŻycie <= 0)
            {
                if (pObiekt.cel != celWrogów)
                {
                    pObiekt.cel = PomocniczeFunkcje.WyszukajWDrzewie(ref korzeńDrzewaPozycji, cObiekt.transform.position) as NPCClass;
                    pObiekt.ResetujŚciezkę();
                    pObiekt.ObsluzAnimacje("haveTarget", (pObiekt.cel == null) ? false : true);
                    return false;
                }
                else
                {
                    pObiekt.ObsluzAnimacje("haveTarget", false);
                }
            }
            else
            {
                float d = Vector3.Distance(pObiekt.transform.position, cObiekt.transform.position) - 0.25f;
                if (d <= pObiekt.zasięgAtaku)
                {
                    //Atakuj
                    pObiekt.Atakuj((d <= 3f + cObiekt.PobierzGranice()) ? true : false);
                    pObiekt.ObsluzAnimacje("inRange", true);
                }
                else
                {
                    if (!pObiekt.odgłosyNPC.isPlaying)
                    {
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref pObiekt.odgłosyNPC, true, TagZEpoka("Poruszanie", pObiekt.epokaNPC, pObiekt.tagRodzajDoDźwięków), true);
                    }
                    pObiekt.ObsluzAnimacje("inRange", false);
                }
            }
        }
        return true;
    }
    public static void DodajDoStosuTrupów(KonkretnyNPCDynamiczny dodajDoTrupów)
    {
        if (stosTrupów == null)
            stosTrupów = new Dictionary<string, StrukturaDoPoolowania>();

        if (stosTrupów.ContainsKey(dodajDoTrupów.NID))
        {
            stosTrupów[dodajDoTrupów.NID].listaObiektówPoolingu.Add(dodajDoTrupów);
        }
        else
        {
            StrukturaDoPoolowania sdp = new StrukturaDoPoolowania();
            sdp.nazwa = dodajDoTrupów.NID;
            sdp.listaObiektówPoolingu = new List<KonkretnyNPCDynamiczny>();
            sdp.listaObiektówPoolingu.Add(dodajDoTrupów);
            stosTrupów.Add(dodajDoTrupów.NID, sdp);
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
    public static void UstawTimeScale(float tScale)
    {
        Time.timeScale = tScale;
    }
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
        stosTrupów = null;
        tablicaWież = null;
        tabHit = null;
        poHerbacie = -1;
        managerGryScript.ResetManagerGryScript();
        spawnBudynki.DestroyBuildings();
        if (spawnerHord != null)
        {
            spawnerHord.UsunWszystkieJednostki();
            spawnerHord = null;
        }
    }
    public static short[] ZwrócIndeksyWTablicy(Vector3 pozycja)
    {
        short x = (short)(Mathf.FloorToInt((pozycja.x - aktualneGranicaTab) / distXZ));
        short z = (short)(Mathf.FloorToInt((pozycja.z - aktualneGranicaTab) / distXZ));
        return new short[] { x, z };
    }
    public static bool SprawdźCzyWykraczaPozaZakresTablicy(short x, short z)
    {
        if (x < 0 || x > 19 || z < 0 || z > 19)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public static void ZapiszDane()
    {
        DataSave ds = new DataSave();

        ds.ilośćMonet = ManagerGryScript.iloscCoinów;
        ds._odblokowanieEpoki = odblokowaneEpoki;
        ds._odblokowanyPoziomEpoki = odblokowanyPoziomEpoki;
        ds.poziomHP = PomocniczeFunkcje.managerGryScript.hpIdx;
        ds.poziomAtak = PomocniczeFunkcje.managerGryScript.atkIdx;
        ds.poziomDef = PomocniczeFunkcje.managerGryScript.defIdx;
        List<ZapisSkrzynek> zsl = new List<ZapisSkrzynek>();
        for (byte i = 0; i < 4; i++)
        {
            Skrzynka s = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
            if (s.ReuseTimer || s.button.enabled)
            {
                ZapisSkrzynek t = new ZapisSkrzynek();
                t.czyAktywna = s.button.interactable;
                t.dzień = s.pozostałyCzas.Day;
                t.godzina = (byte)s.pozostałyCzas.Hour;
                t.minuta = (byte)s.pozostałyCzas.Minute;
                t.sekunda = (byte)s.pozostałyCzas.Second;
                t.miesiąc = (byte)s.pozostałyCzas.Month;
                t.rok = s.pozostałyCzas.Year;
                if (t.czyAktywna || s.ReuseTimer)
                {
                    t.czyIstniejeSkrzynka = true;
                }
                else
                {
                    t.czyIstniejeSkrzynka = false;
                }
                zsl.Add(t);
            }
        }
        ds._skrzynki = zsl.ToArray();
        List<EnOrDisBudynki> tmp = new List<EnOrDisBudynki>();
        for (ushort i = 0; i < spawnBudynki.wszystkieBudynki.Length; i++)
        {
            KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
            EnOrDisBudynki eodb = new EnOrDisBudynki();
            eodb.nazwa = knpcs.name;
            eodb.zablokowanie = knpcs.Zablokowany;
            tmp.Add(eodb);
        }
        ds._zablokowaneBudynki = tmp.ToArray();
        if (managerGryScript.ekwipunekGracza.Length > 0)
        {
            List<EkwiStruct> ekwi = new List<EkwiStruct>();
            for (byte i = 0; i < managerGryScript.ekwipunekGracza.Length; i++)
            {
                EkwiStruct es = new EkwiStruct();
                es.iloscPrzedmiotu = managerGryScript.ekwipunekGracza[i].ilośćDanejNagrody;
                ekwi.Add(es);
            }
            ds._ekwipunek = ekwi.ToArray();
        }
        string ścieżka = ZwróćŚcieżkęZapisu("dataBaseTDv1.asc");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs;
        if (!File.Exists(ścieżka))
        {
            fs = File.Create(ścieżka);
        }
        else
        {
            File.Delete(ścieżka);
            fs = File.Create(ścieżka);
        }
        bf.Serialize(fs, ds);
        fs.Close();

    }
    public static void ŁadujDane()
    {
        string ścieżka = ZwróćŚcieżkęZapisu("dataBaseTDv1.asc");
        if (File.Exists(ścieżka))
        {
            Debug.Log(ścieżka);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(ścieżka, FileMode.Open);
            DataSave ds = (DataSave)bf.Deserialize(fs);

            fs.Close();

            ManagerGryScript.iloscCoinów = (ushort)ds.ilośćMonet;
            odblokowaneEpoki = ds._odblokowanieEpoki;
            odblokowanyPoziomEpoki = ds._odblokowanyPoziomEpoki;
            managerGryScript.hpIdx = (ushort)ds.poziomHP;
            managerGryScript.atkIdx = (ushort)ds.poziomAtak;
            managerGryScript.defIdx = (ushort)ds.poziomDef;
            for (ushort i = 0; i < ds._skrzynki.Length; i++)
            {
                if (ds._skrzynki[i].czyIstniejeSkrzynka || ds._skrzynki[i].czyAktywna)
                {
                    int offsetD = ds._skrzynki[i].dzień - System.DateTime.Now.Day;
                    int offsetG = ds._skrzynki[i].godzina - System.DateTime.Now.Hour;
                    int offsetM = ds._skrzynki[i].minuta - System.DateTime.Now.Minute;
                    int offsetS = ds._skrzynki[i].sekunda - System.DateTime.Now.Second;
                    int offsetMSC = ds._skrzynki[i].miesiąc - System.DateTime.Now.Month;
                    int offsetR = ds._skrzynki[i].rok - System.DateTime.Now.Year;
                    Skrzynka s = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
                    System.DateTime dT = System.DateTime.Now;
                    dT = dT.AddDays(offsetD);
                    //Debug.Log("dt godzina przed dodaniem "+dT.Hour);
                    dT = dT.AddHours(offsetG);
                    //Debug.Log("dt godzina po dodaniu "+dT.Hour);
                    dT = dT.AddMinutes(offsetM);
                    dT = dT.AddSeconds(offsetS);
                    dT = dT.AddYears(offsetR);
                    dT = dT.AddMonths(offsetMSC);
                    s.pozostałyCzas = dT;
                    //Debug.Log("dT.czas = "+dT.ToShortTimeString());
                    //Debug.Log("Dzień = "+offsetD+" godzina = "+offsetG+" minuta = "+offsetM+" sekunda = "+offsetS+" miesiąc = "+offsetMSC+" rok = "+offsetR);
                    s.button.interactable = ds._skrzynki[i].czyAktywna;
                    s.ReuseTimer = ds._skrzynki[i].czyIstniejeSkrzynka;
                }
            }
            for (ushort i = 0; i < ds._zablokowaneBudynki.Length; i++)
            {
                for (byte j = 0; j < spawnBudynki.wszystkieBudynki.Length; j++)
                {
                    KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>();
                    if (knpcs.name == ds._zablokowaneBudynki[i].nazwa)
                    {
                        knpcs.Zablokowany = knpcs.blokowany;
                        knpcs.Zablokowany = ds._zablokowaneBudynki[i].zablokowanie;
                    }
                }
            }
            for (ushort i = 0; i < ds._ekwipunek.Length; i++)
            {
                //Istnieją przedmioty w ekwipunku
                managerGryScript.ekwipunekGracza[i].ilośćDanejNagrody = ds._ekwipunek[i].iloscPrzedmiotu;
            }
        }
        else
        {
            Debug.Log("Nie odnalazłem zapisu");
            for (byte j = 0; j < spawnBudynki.wszystkieBudynki.Length; j++)
            {
                KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>();
                knpcs.Zablokowany = knpcs.blokowany;
            }
            for (byte i = 0; i < managerGryScript.ekwipunekGracza.Length; i++)
            {
                managerGryScript.ekwipunekGracza[i].ilośćDanejNagrody = 0;
            }
            managerGryScript.hpIdx = 0;
            managerGryScript.atkIdx = 0;
            managerGryScript.defIdx = 0;
        }
    }
    private static string ZwróćŚcieżkęZapisu(string nazwaPliku)
    {
        string s = null;
#if UNITY_STANDALONE_WIN || UNITY_IOS
        s = Application.persistentDataPath+"/"+nazwaPliku;
#endif
#if UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
        s = Application.dataPath+"/"+nazwaPliku;
#endif
#if UNITY_ANDROID
        s = Application.persistentDataPath + "/" + nazwaPliku;
#endif

        return s;
    }
    public static void ZapisDanychOpcje()
    {
        DaneOpcji daneO = new DaneOpcji();

        daneO.indeksJezyka = mainMenu.lastIdxJezyka;
        daneO.blokadaOrientacji = managerGryScript.blokowanieOrientacji;
        daneO.głośność = PomocniczeFunkcje.muzyka.muzykaTła.volume;
        daneO.czyLicznikFPSOn = PomocniczeFunkcje.mainMenu.CzyLFPSOn;

        string ścieżka = ZwróćŚcieżkęZapisu("daneOpcje.asc");

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fs;
        if (!File.Exists(ścieżka))
        {
            fs = File.Create(ścieżka);
        }
        else
        {
            File.Delete(ścieżka);
            fs = File.Create(ścieżka);
        }
        bf.Serialize(fs, daneO);
        fs.Close();
    }
    public static void LadujDaneOpcje()
    {
        string ścieżka = ZwróćŚcieżkęZapisu("daneOpcje.asc");
        if (File.Exists(ścieżka))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(ścieżka, FileMode.Open);
            DaneOpcji daneO = (DaneOpcji)bf.Deserialize(fs);

            fs.Close();

            if (mainMenu != null)
            {
                mainMenu.lastIdxJezyka = daneO.indeksJezyka;
                mainMenu.sliderDźwięku.value = daneO.głośność;
                mainMenu.CzyLFPSOn = daneO.czyLicznikFPSOn;
            }
            if (managerGryScript != null)
            {
                managerGryScript.blokowanieOrientacji = daneO.blokadaOrientacji;
            }
        }
    }
    public static string TagZEpoka(string aktTag, Epoki e, string rodzajObiektu = "")
    {
        return aktTag + "_" + e.ToString() + rodzajObiektu;
    }
    //Wylicza wartość modyfikatora zadawanych i otrzymywanych obrażeń
    public static float WyliczModyfikatorObrazeń(float bazowyModyfikator, ushort wartośćIndeksu)
    {
        float warMnoznika = Mathf.Pow(0.98f, wartośćIndeksu);
        return bazowyModyfikator + (0.002f * warMnoznika); //Do +5% na 37 poziomie
    }
}
[System.Serializable]
public struct DataSave
{
    [SerializeField] public int ilośćMonet;
    [SerializeField] public ushort _odblokowanyPoziomEpoki;
    [SerializeField] public byte _odblokowanieEpoki;
    [SerializeField] public ZapisSkrzynek[] _skrzynki;
    [SerializeField] public EnOrDisBudynki[] _zablokowaneBudynki;
    [SerializeField] public EkwiStruct[] _ekwipunek;
    [SerializeField] public int poziomHP;
    [SerializeField] public int poziomAtak;
    [SerializeField] public int poziomDef;
}
[System.Serializable]
public struct EnOrDisBudynki
{
    [SerializeField] public string nazwa;
    [SerializeField] public bool zablokowanie;
}
[System.Serializable]
public struct ZapisSkrzynek
{
    [SerializeField] public bool czyAktywna;
    [SerializeField] public byte godzina;
    [SerializeField] public byte minuta;
    [SerializeField] public int dzień;
    [SerializeField] public byte sekunda;
    [SerializeField] public byte miesiąc;
    [SerializeField] public int rok;
    [SerializeField] public bool czyIstniejeSkrzynka;

}
[System.Serializable]
public struct EkwiStruct
{
    [SerializeField] public byte iloscPrzedmiotu;
}
[System.Serializable]
public struct DaneOpcji
{
    [SerializeField] public sbyte indeksJezyka;
    [SerializeField] public bool blokadaOrientacji;
    [SerializeField] public float głośność;
    [SerializeField] public bool czyLicznikFPSOn;
}

