using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

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
    public static List<InformacjeDlaPolWież>[,] tablicaWież = null;
    public static float distXZ = 5;
    public static byte odblokowanyPoziomEpoki = 100;
    public static byte odblokowaneEpoki = 6;
    public static ushort aktualneGranicaTab = 0;
    public static Camera oCam = null;
    #region Obsługa położenia myszy względem ekranu
    public static Vector3 OkreślPozycjęŚwiataKursora(Vector3 lastPos)
    {
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        Ray ray;
#if UNITY_STANDALONE
        ray = oCam.ScreenPointToRay(Input.mousePosition);
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            ray = oCam.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                ray = oCam.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                return lastPos;
            }
        }
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
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        Ray ray;
#if UNITY_STANDALONE
        ray = oCam.ScreenPointToRay(Input.mousePosition);
#endif
#if UNITY_ANDROID
        if (Input.mousePresent)
        {
            ray = oCam.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            if (Input.touchCount > 0)
            {
                ray = oCam.ScreenPointToRay(Input.GetTouch(0).position);
            }
            else
            {
                return lastNPCCLass;
            }
        }
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
            return false;
        if (pObiekt.cel == null)
        {
            //Wyszukaj cel
            pObiekt.cel = PomocniczeFunkcje.WyszukajWDrzewie(ref korzeńDrzewaPozycji, pObiekt.transform.position) as NPCClass;
            if (pObiekt.cel != null)
            {
                pObiekt.ObsluzAnimacje("haveTarget", true);
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
        stosTrupów = null;
        tablicaWież = null;
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
        List<ZapisSkrzynek> zsl = new List<ZapisSkrzynek>();
        for (byte i = 0; i < 4; i++)
        {
            if (managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).ReuseTimer || managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).button.enabled)
            {
                ZapisSkrzynek t = new ZapisSkrzynek();
                t.czyAktywna = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).button.interactable;
                t.dzień = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.Day;
                t.godzina = (byte)managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.Hour;
                t.minuta = (byte)managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.Minute;
                t.sekunda = (byte)managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.Second;
                t.rok = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.Year;
                if (t.czyAktywna || managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).ReuseTimer)
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
            eodb.nazwa = knpcs.nazwa;
            eodb.zablokowanie = knpcs.zablokowany;
            tmp.Add(eodb);
        }
        ds._zablokowaneBudynki = tmp.ToArray();
        if (managerGryScript.ekwipunek != null && managerGryScript.ekwipunek.przedmioty != null && managerGryScript.ekwipunek.przedmioty.Length > 0)
        {
            List<EkwiStruct> ekwi = new List<EkwiStruct>();
            for (byte i = 0; i < managerGryScript.ekwipunek.przedmioty.Length; i++)
            {
                for (byte j = 0; j < managerGryScript.ekwipunekGracza.Length; j++)
                {
                    if (managerGryScript.ekwipunek.przedmioty[i].nazwaPrzedmiotu == managerGryScript.ekwipunekGracza[j].nazwaPrzedmiotu)
                    {
                        EkwiStruct es = new EkwiStruct();
                        es.idxTab = j;
                        es.iloscPrzedmiotu = managerGryScript.ekwipunek.przedmioty[i].ilośćDanejNagrody;
                        ekwi.Add(es);
                        break;
                    }
                }
            }
            ds._ekwipunek = ekwi.ToArray();
        }
        else
        {
            ds._ekwipunek = new EkwiStruct[1];
            ds._ekwipunek[0].idxTab = 255;
        }
        string ścieżka = ZwróćŚcieżkęZapisu();

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
        string ścieżka = ZwróćŚcieżkęZapisu();
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
            for (ushort i = 0; i < ds._skrzynki.Length; i++)
            {
                if (ds._skrzynki[i].czyIstniejeSkrzynka || ds._skrzynki[i].czyAktywna)
                {
                    int offsetD = ds._skrzynki[i].dzień - System.DateTime.Now.Day;
                    int offsetG = ds._skrzynki[i].godzina - System.DateTime.Now.Hour;
                    int offsetM = ds._skrzynki[i].minuta - System.DateTime.Now.Minute;
                    int offsetS = ds._skrzynki[i].sekunda - System.DateTime.Now.Second;
                    int offsetR = ds._skrzynki[i].rok - System.DateTime.Now.Year;
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas = System.DateTime.Now;
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.AddDays(offsetD);
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.AddHours(offsetG);
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.AddMinutes(offsetM);
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.AddSeconds(offsetS);
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).pozostałyCzas.AddYears(offsetR);
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).button.interactable = ds._skrzynki[i].czyAktywna;
                    managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i).ReuseTimer = ds._skrzynki[i].czyIstniejeSkrzynka;
                }
            }
            for (ushort i = 0; i < ds._zablokowaneBudynki.Length; i++)
            {
                for (byte j = 0; j < spawnBudynki.wszystkieBudynki.Length; j++)
                {
                    KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[j].GetComponent<KonkretnyNPCStatyczny>();
                    if (knpcs.nazwa == ds._zablokowaneBudynki[i].nazwa)
                    {
                        knpcs.zablokowany = ds._zablokowaneBudynki[i].zablokowanie;
                    }
                }
            }
            if (ds._ekwipunek.Length == 1 && ds._ekwipunek[0].idxTab == 255)
            {
                //Niestety brak przedmiotów w ekwipunku
                managerGryScript.ekwipunek = null;
            }
            else
            {
                List<PrzedmiotScript> ps = new List<PrzedmiotScript>();
                for (ushort i = 0; i < ds._ekwipunek.Length; i++)
                {
                    //Istnieją przedmioty w ekwipunku
                    ps.Add(managerGryScript.ekwipunekGracza[ds._ekwipunek[i].idxTab]);
                    ps[i].ilośćDanejNagrody = ds._ekwipunek[i].iloscPrzedmiotu;
                }
                managerGryScript.ekwipunek = new EkwipunekScript(ps.ToArray());
            }
        }
        else
        {
            Debug.Log("Nie odnalazłem zapisu");
        }
    }
    private static string ZwróćŚcieżkęZapisu()
    {
        string s = null;
#if UNITY_STANDALONE_WIN || UNITY_IOS
        s = Application.persistentDataPath+"/dataBaseTDv1.asc";
#endif
#if UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
        s = Application.dataPath+"/dataBaseTDv1.asc";
#endif
#if UNITY_ANDROID
        s = Application.persistentDataPath + "/dataBaseTDv1.asc";
#endif

        return s;
    }
}
[System.Serializable]
public struct DataSave
{
    [SerializeField] public int ilośćMonet;
    [SerializeField] public byte _odblokowanyPoziomEpoki;
    [SerializeField] public byte _odblokowanieEpoki;
    [SerializeField] public ZapisSkrzynek[] _skrzynki;
    [SerializeField] public EnOrDisBudynki[] _zablokowaneBudynki;
    [SerializeField] public EkwiStruct[] _ekwipunek;
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
    [SerializeField] public int rok;
    [SerializeField] public bool czyIstniejeSkrzynka;

}
[System.Serializable]
public struct EkwiStruct
{
    [SerializeField] public byte idxTab;
    [SerializeField] public byte iloscPrzedmiotu;
}

