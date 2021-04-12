using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
using PlayerPrefsSwitch;

public static class PomocniczeFunkcje
{
    #region Zmienne
    public static StrukturaDrzewa korzeńDrzewaPozycji = null;   //Root dla drzewa pozycji budynków gracza postawionych na scenie
    public static NPCClass celWrogów = null;                    //Budynek - baza
    private static string test = "Assets/Resources/Debug.txt";  //Ścieżka dla debugera błędów, których nie może wykryć aplikacja Unity
    public static Dictionary<string, StrukturaDoPoolowania> stosTrupów = null;  //Stos przeciwników używany do poolowania
    public static SpawnerHord spawnerHord = null;               //Referencja do klasy generującej fale wrogów na mapie
    public static ManagerGryScript managerGryScript = null;     //Referencja dla klasy obsługującej grę
    public static MainMenu mainMenu = null;                     //Referencja do klasy obsługującej UI
    public static SpawnBudynki spawnBudynki = null;             //Referencja do klasy odpowiedzialnej za stawianie budynków na scenie
    public static MuzykaScript muzyka = null;                   //Referencja do klasy odpowiedzialnej za przechowywanie klipów audio
    public static List<InformacjeDlaPolWież>[,] tablicaWież = null; //Lista pól do obsługi wież na terenie
    public static float distXZ = 5;                             //Dystans klatki dla tablicy wież
    public static ushort odblokowanyPoziomEpoki = 1;            //Ile poziomów najnowszej epoki odblokował gracz
    public static byte odblokowaneEpoki = 1;                    //Jakie epoki odblokował gracz        
    public static ushort aktualneGranicaTab = 0;                //Odległość między krawędzią terenu a obszarem działania gracza     
    public static Camera oCam = null;                           //Referencja do głównej kamery (optymalizacyjna zmienna)
    public static EventSystem eSystem = null;                   //Event system wykorzystywany do sprawdzenia czy gracz przypadkiem nie działa na UI                
    public static sbyte poHerbacie = -1;                        //Zmienna przechowująca informacje o tym, czy w tej klatce generowano promień dla tabHit
    private static RaycastHit[] tabHit = null;                  //Tablica trafień dla promienia
    private static short[] bufferpozycji = new short[2];        //Buffer pozycji wyliczanej dla jednostki przemieszczającej się po scenie gry
    public static sbyte czyKliknąłemUI = -1;
    public static byte kameraZostalaPrzesunieta = 2;            //0 - brak ruchu kamery, poprawiona isVisibility, 1 - brak ruchu kamery, niepoprawione isVisibility, 2 - ruch kamery
    #endregion
    #region Obsługa położenia myszy względem ekranu
    /*
    Metoda zwraca punkt styku promienia generowanego przez kursor a obiektem natrafiającym na collider
    */
    public static Vector3 OkreślPozycjęŚwiataKursora(Vector3 lastPos, ref bool hitUI, bool ustawLastPos = false)
    {
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        Vector2 posK = Vector2.zero;
        /* UNITY_ANDROID
        #if UNITY_STANDALONE
                posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                posKursoraCanvas = posK;
        #endif
        #if UNITY_ANDROID || UNITY_IOS */
        if (!ustawLastPos)
        {
            if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            {
                posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mainMenu.UstawKursorNa(posK.x, posK.y);
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    posK = Input.GetTouch(0).position;
                    mainMenu.UstawKursorNa(posK.x, posK.y);
                }
                else
                {
                    return lastPos;
                }
            }
            //#endif
            if (CzyKliknalemUI())
            {
                hitUI = true;
                return lastPos;
            }
            else
            {
                hitUI = false;
            }
        }
        else
        {
            posK.x = lastPos.x;
            posK.y = lastPos.y;
            hitUI = false;
        }
        RaycastHit[] rh = ZwrócHity(ref oCam, posK);
        if (rh == null)
            return lastPos;
        if (poHerbacie == 1)
        {
            if (rh[0].collider.GetType() == typeof(TerrainCollider))
            {
                return rh[0].point;
            }
        }
        return lastPos;
    }
    /*
    Metoda zwraca informację o ewentualnym klikniętym obiekcie przez gracza
    */
    public static NPCClass OkreślKlikniętyNPC(ref NPCClass lastNPCCLass, bool pobierzInfoVirtualnegoKursora = false)
    {
        if (oCam == null)
        {
            oCam = Camera.main;
        }
        float[] posKursora = mainMenu.ZwrócPozycjeKursora;
        Vector2 posK = (pobierzInfoVirtualnegoKursora) ? new Vector2(posKursora[0], posKursora[1]) : Vector2.negativeInfinity;
        /* UNITY_ANDROID
#if UNITY_STANDALONE
        posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
#endif
#if UNITY_ANDROID || UNITY_IOS */
        if (posK.x == Mathf.NegativeInfinity)
        {
            if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            {
                posK = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                mainMenu.UstawKursorNa(posK.x, posK.y);
            }
            else
            {
                if (Input.touchCount > 0)
                {
                    posK = Input.GetTouch(0).position;
                    mainMenu.UstawKursorNa(posK.x, posK.y);
                }
                else
                {
                    return lastNPCCLass;
                }
            }
        }
        //#endif
        RaycastHit[] rh = ZwrócHity(ref oCam, posK);
        if (rh == null)
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
                if (rays[i].collider.CompareTag("Budynek"))
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
            if (CzyKliknalemUI() && !mainMenu.OdpalonyPanel)
                return lastClass;
            else
                return null;
        }
        else
            return lastClass;
    }
    /*
    Metoda pomocnicza zwracająca trafione promieniem obiekty z colliderem mającym layer default lub NPCS
    */
    private static RaycastHit[] ZwrócHity(ref Camera camera, Vector2 pozycjaK)
    {
        if (poHerbacie < 0) //Już raz wyszukałem w tej turze colidery
        {
            Ray ray = camera.ScreenPointToRay(pozycjaK);
            ray.direction = ray.direction * 40f;
            int layerMask = ~((0 << 8) | (0 << 0));
            if (tabHit == null)
                tabHit = new RaycastHit[2];
            poHerbacie = (sbyte)Physics.RaycastNonAlloc(ray, tabHit, 40f, layerMask, QueryTriggerInteraction.Collide);
        }
        if (poHerbacie > 0)
            return tabHit;
        else
            return null;
    }
    /*
    Metoda zwraca informację czy kliknięty był obiekt UI czy też nie
    */
    public static bool CzyKliknalemUI()
    {
        if (czyKliknąłemUI == 0)
            return false;
        else if (czyKliknąłemUI == 1)
            return true;
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
            czyKliknąłemUI = 1;
            return true;
        }
        else
        {
            czyKliknąłemUI = 0;
            return false;
        }
    }
    /*
    Metoda resetuje dane bufforów dla promienia (LateUpdate())
    */
    public static void ResetujDaneRaycast()
    {
        poHerbacie = -1;
        tabHit = null;
    }
    #endregion
    #region Drzewo pozycji
    /*
    Funkcja zwraca informację o tym, czy npc znajduje się w obszarze gracza (Funkcja ustawiana zgodnie z ilością pól dla wież)
    */
    public static bool SprawdźCzyWykraczaPozaZakresTablicy(short x, short z)
    {
        if (x < 0 || x > 21 || z < 0 || z > 21)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    /*
    Funkcja zwraca o indeksie w liście tablicaWież dla danego punktu
    */
    public static short[] ZwrócIndeksyWTablicy(float posx, float posz)
    {
        bufferpozycji[0] = (short)(Mathf.FloorToInt((posx - aktualneGranicaTab) / distXZ));
        bufferpozycji[1] = (short)(Mathf.FloorToInt((posz - aktualneGranicaTab) / distXZ));
        return bufferpozycji;
    }
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
    /*
    Metoda kasuje element drzewa wysłany jako parametr (wywołuje się podczas kiedy budynek gracza jest niszczony)
    */
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
    /*
    Funkcja zwraca informację o elementach będących dziećmi elementu wysyłanego w parametrze (Wywoływana podczas kasowania elementu w drzewie)
    */
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
    /*
    Metoda dodaje do drzewa pozycji budynków elementy dzieci, których rodzic jest kasowany
    */
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
    /*
    Metoda dodaje do drzewa pozycji nowy element.
    Metoda działa na zasadzie pozycji względem rodzica tj rootem a inne budynki są wstawiane w określone gałęzie zgodnie z ćwiartką osi położenia względem gałęzi nadrzędnej.
    */
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
    /*
    Funkcja zwraca wartość true, kiedy NPC znalazł, atakuje lub dąży do celu.
    */
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
                if (pObiekt.ZwróćMiWartośćParametru(1) > 0)
                {
                    pObiekt.ObsluzAnimacje("haveTarget", true);
                }
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
                    bool ha = (pObiekt.cel == null) ? false : true;
                    if (pObiekt.ZwróćMiWartośćParametru(1) > 0)
                    {
                        pObiekt.ObsluzAnimacje("haveTarget", ha);
                    }
                    return false;
                }
                else
                {
                    if (pObiekt.ZwróćMiWartośćParametru(1) == 1)
                    {
                        pObiekt.ObsluzAnimacje("haveTarget", false);
                    }
                }
            }
            else
            {
                float gObiektuAtakowanego = cObiekt.PobierzGranice();
                float d = Vector3.Distance(pObiekt.transform.position, cObiekt.transform.position) - 0.25f - gObiektuAtakowanego;
                if (d <= pObiekt.zasięgAtaku)
                {
                    //Atakuj
                    pObiekt.Atakuj();
                    if (pObiekt.ZwróćMiWartośćParametru(2) == 1)
                    {
                        pObiekt.ObsluzAnimacje("inRange", true);
                    }
                }
                else
                {
                    if (!pObiekt.odgłosyNPC.isPlaying)
                    {
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref pObiekt.odgłosyNPC, true, TagZEpoka("Poruszanie", pObiekt.epokaNPC, pObiekt.tagRodzajDoDźwięków), true);
                    }
                    if (pObiekt.ZwróćMiWartośćParametru(2) > 0)
                    {
                        pObiekt.ObsluzAnimacje("inRange", false);
                    }
                }
            }
        }
        return true;
    }
    /*
    Metoda dodaje do stosu NPC, którzy w danej fali zostali pokonani (Pooling)
    */
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
    }
    /*
    Funkcja zwraca obiekt, który został wygenerowany w jednej z poprzednich fal zgodny z życzeniem Generatora Fal
    */
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
    #region Ładowanie czyszczenie i zapis danych
    /*
    Metoda zapisuje dane do pliku Debugującego coś co chcemy zdebugować, a potem poddać ręcznej analizie.
    */
    public static void SaveInformationInDebug(string s)
    {
        StreamWriter writer = new StreamWriter(test, true);
        writer.WriteLine(s);
        writer.Close();
    }
    public static void PrzypiszFontyDoNiemajacychPrzypisanychTextow(ref Font fontDoPrzypisania)
    {
        mainMenu.tekstCoWygrales.font = fontDoPrzypisania;
        mainMenu.UstawFontDlaStatystyk(ref fontDoPrzypisania);
    }
    /*
    Metoda przywraca podstawowe dane projektu (ładowanie nowej sceny np)
    */
    public static void ResetujWszystko()
    {
        korzeńDrzewaPozycji = null;
        celWrogów = null;
        stosTrupów = null;
        tablicaWież = null;
        tabHit = null;
        poHerbacie = -1;
        ObslugaScenScript.indeksAktualnejSceny = -1;
        managerGryScript.ResetManagerGryScript();
        spawnBudynki.DestroyBuildings();
        UstawTenDomyslnyButton.ResetujDanePrzycisków();
        if (spawnerHord != null)
        {
            spawnerHord.UsunWszystkieJednostki();
            spawnerHord = null;
        }
    }
    /*
    Metoda zapisuje dane do pliku odnośnie postępów gracza w grze
    */
    public static void ZapiszDane()
    {
        string sDoZapisu = "";
        for (byte i = 0; i < 4; i++)
        {
            Skrzynka s = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
            sDoZapisu = sDoZapisu + s.button.interactable.ToString() + "_";
        }
        PlayerPrefs.SetString("ButtonySkrzynek", sDoZapisu);
        sDoZapisu = "";
        for (byte i = 0; i < managerGryScript.ekwipunekGracza.Length; i++)
        {
            sDoZapisu = sDoZapisu + managerGryScript.ekwipunekGracza[i].ilośćDanejNagrody.ToString() + "_";
        }
        PlayerPrefs.SetString("Ekwipunek", sDoZapisu);
        //Zapis Data Save
        PlayerPrefs.SetString("DataSave", ManagerGryScript.iloscCoinów.ToString() + "_" + odblokowaneEpoki.ToString() +
        "_" + odblokowanyPoziomEpoki.ToString() + "_" + PomocniczeFunkcje.managerGryScript.hpIdx.ToString() + "_" +
        PomocniczeFunkcje.managerGryScript.atkIdx.ToString() + "_" + PomocniczeFunkcje.managerGryScript.defIdx.ToString());
        sDoZapisu = "_";
        for (ushort i = 0; i < spawnBudynki.wszystkieBudynki.Length; i++)
        {
            KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
            PlayerPrefs.SetInt("BlokadaBudynków_" + knpcs.name, (knpcs.Zablokowany) ? 0 : 1);
        }
        PlayerPrefs.Save();
        /*UNITY_ANDROID
        DataSave ds = new DataSave();

        ds.ilośćMonet = ManagerGryScript.iloscCoinów;
        ds._odblokowanieEpoki = odblokowaneEpoki;
        ds._odblokowanyPoziomEpoki = odblokowanyPoziomEpoki;
        ds.poziomHP = PomocniczeFunkcje.managerGryScript.hpIdx;
        ds.poziomAtak = PomocniczeFunkcje.managerGryScript.atkIdx;
        ds.poziomDef = PomocniczeFunkcje.managerGryScript.defIdx;
        /*UNITY_ANDROID
        List<ZapisSkrzynek> zsl = new List<ZapisSkrzynek>();
        for (byte i = 0; i < 4; i++)
        {
            Skrzynka s = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
            if ( s.ReuseTimer ||  s.button.enabled)
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
        */
    }
    /*
    Metoda ładuje dane odnośnie postępów gracza w grze z pliku
    */
    public static void ŁadujDane()
    {
        if (PlayerPrefs.HasKey("DataSave"))
        {
            //Debug.Log("Przeszedłem HasKey");
            string ładowaneDane = PlayerPrefs.GetString("DataSave");
            string[] s = ładowaneDane.Split('_');

            ManagerGryScript.iloscCoinów = (int)System.Int32.Parse(s[0]); if (ManagerGryScript.iloscCoinów < 70) { ManagerGryScript.iloscCoinów = 70; }
            odblokowaneEpoki = (byte)System.Int16.Parse(s[1]);
            odblokowanyPoziomEpoki = (byte)System.Int16.Parse(s[2]);
            managerGryScript.hpIdx = (ushort)System.Int16.Parse(s[3]);
            managerGryScript.atkIdx = (ushort)System.Int16.Parse(s[4]);
            managerGryScript.defIdx = (ushort)System.Int16.Parse(s[5]);


            ładowaneDane = PlayerPrefs.GetString("Ekwipunek");
            s = ładowaneDane.Split('_');
            for (byte i = 0; i < managerGryScript.ekwipunekGracza.Length; i++)
            {
                managerGryScript.ekwipunekGracza[i].ilośćDanejNagrody = (byte)System.Int16.Parse(s[i]);
            }


            ładowaneDane = PlayerPrefs.GetString("ButtonySkrzynek");
            s = ładowaneDane.Split('_');
            for (byte i = 0; i < 4; i++)
            {
                Skrzynka ss = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
                ss.button.interactable = (s[i] == "True") ? true : false;
            }


            ładowaneDane = PlayerPrefs.GetString("BlokadaBudynków");
            s = ładowaneDane.Split('_');
            for (int i = 0; i < spawnBudynki.wszystkieBudynki.Length; i++)
            {
                KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
                if (PlayerPrefs.HasKey("BlokadaBudynków_" + knpcs.name))
                {
                    int t = PlayerPrefs.GetInt("BlokadaBudynków_" + knpcs.name);
                    //Zablokowany 0 odblokowany 1
                    if (t == 1)
                    {
                        knpcs.Zablokowany = false;
                    }
                    else
                    {
                        knpcs.Zablokowany = true;
                    }
                }
                else
                {
                    knpcs.Zablokowany = knpcs.blokowany;
                }
                /*
                bool czyZnalazlem = false;
                for (ushort j = 0; i < s.Length; j += 2)
                {
                    if (knpcs.name == s[j])
                    {
                        knpcs.Zablokowany = (s[j + 1] == "True") ? true : false;
                        czyZnalazlem = true;
                        break;
                    }
                }
                if (!czyZnalazlem)
                {
                    knpcs.Zablokowany = knpcs.blokowany;
                }
                */
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
        /*UNITY_ANDROID
        string ścieżka = ZwróćŚcieżkęZapisu("dataBaseTDv1.asc");
        if (File.Exists(ścieżka))
        {
            Debug.Log(ścieżka);
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = File.Open(ścieżka, FileMode.Open);
            DataSave ds = (DataSave)bf.Deserialize(fs);

            fs.Close();

            ManagerGryScript.iloscCoinów = (ds.ilośćMonet <= 50) ? (ushort)50 : (ushort)ds.ilośćMonet;
            odblokowaneEpoki = ds._odblokowanieEpoki;
            odblokowanyPoziomEpoki = ds._odblokowanyPoziomEpoki;
            managerGryScript.hpIdx = (ushort)ds.poziomHP;
            managerGryScript.atkIdx = (ushort)ds.poziomAtak;
            managerGryScript.defIdx = (ushort)ds.poziomDef;
            for (ushort i = 0; i < ds._skrzynki.Length; i++)
            {
                if (ds._skrzynki[i].czyIstniejeSkrzynka || ds._skrzynki[i].czyAktywna)
                {
                    Skrzynka s = managerGryScript.ZwróćSkrzynkeOIndeksie((byte)i);
                    /* UNITY_ANDROID
                    int offsetD = ds._skrzynki[i].dzień - System.DateTime.Now.Day;
                    int offsetG = ds._skrzynki[i].godzina - System.DateTime.Now.Hour;
                    int offsetM = ds._skrzynki[i].minuta - System.DateTime.Now.Minute;
                    int offsetS = ds._skrzynki[i].sekunda - System.DateTime.Now.Second;
                    int offsetMSC = ds._skrzynki[i].miesiąc - System.DateTime.Now.Month;
                    int offsetR = ds._skrzynki[i].rok - System.DateTime.Now.Year;
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
                    //s.ReuseTimer = ds._skrzynki[i].czyIstniejeSkrzynka;
                }
            }
            for (byte i = 0; i < spawnBudynki.wszystkieBudynki.Length; i++)
            {
                bool czyZnalazlem = false;
                KonkretnyNPCStatyczny knpcs = spawnBudynki.wszystkieBudynki[i].GetComponent<KonkretnyNPCStatyczny>();
                for (byte j = 0; j < ds._zablokowaneBudynki.Length; j++)
                {
                    if (knpcs.name == ds._zablokowaneBudynki[j].nazwa)
                    {
                        knpcs.Zablokowany = ds._zablokowaneBudynki[j].zablokowanie;
                        czyZnalazlem = true;
                        break;
                    }
                }
                if (!czyZnalazlem)
                {
                    knpcs.Zablokowany = knpcs.blokowany;
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
        */
    }
    /*
    Funkcja zwraca ścieżkę zapisu plików
    */
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
    /*
    Zapisuje ustawienia opcji do pliku
    */
    public static void ZapisDanychOpcje()
    {
        string sDoZapisu = "_";
        sDoZapisu = mainMenu.lastIdxJezyka.ToString() + "_" + PomocniczeFunkcje.muzyka.muzykaTła.volume.ToString();
        PlayerPrefs.SetString("DaneOpcji", sDoZapisu);
        PlayerPrefs.Save();
        /* UNITY_ANDROID
        DaneOpcji daneO = new DaneOpcji();

        daneO.indeksJezyka = mainMenu.lastIdxJezyka;
        daneO.głośność = PomocniczeFunkcje.muzyka.muzykaTła.volume;
        
        daneO.blokadaOrientacji = managerGryScript.blokowanieOrientacji;
        daneO.czyOdwracaćPrzesuwanie = MoveCameraScript.odwrócPrzesuwanie;
        daneO.czyLicznikFPSOn = PomocniczeFunkcje.mainMenu.CzyLFPSOn;
        daneO.czyPostProcessing = mainMenu.CzyPostProcesing;

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
        */
    }
    /*
    Ładuje opcje gracza z pliku
    */
    public static void LadujDaneOpcje()
    {
        if (PlayerPrefs.HasKey("DaneOpcji"))
        {
            string daneOpcje = PlayerPrefs.GetString("DaneOpcji");
            string[] s = daneOpcje.Split('_');
            mainMenu.lastIdxJezyka = (sbyte)System.Int16.Parse(s[0]);
            mainMenu.sliderDźwięku.value = float.Parse(s[1]);
        }
        else
        {
            mainMenu.lastIdxJezyka = 1;
            mainMenu.sliderDźwięku.value = 1.0f;
        }
        mainMenu.UstawGłośność();
        /* UNITY_ANDROID
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
                /* UNITY_ANDROID
                mainMenu.CzyLFPSOn = daneO.czyLicznikFPSOn;
                MoveCameraScript.mscInstance.UstawPostProcessing(daneO.czyPostProcessing);
                MoveCameraScript.odwrócPrzesuwanie = daneO.czyOdwracaćPrzesuwanie;
                mainMenu.SetToogleOdwrocenieKamery(daneO.czyOdwracaćPrzesuwanie);
                
                mainMenu.UstawGłośność();
            }
            
            if (managerGryScript != null)
            {
                managerGryScript.blokowanieOrientacji = daneO.blokadaOrientacji;
            }
        }
        */
    }
    public static void KasujZapis()
    {
        PlayerPrefs.DeleteAll();
        /*
        string ścieżka = ZwróćŚcieżkęZapisu("dataBaseTDv1.asc");
        if (File.Exists(ścieżka))
        {
            File.Delete(ścieżka);
        }
        */
    }
    #endregion
    /*
    Funkcja Tag z epoką zwraca informację o tagu dla audio clip jaki ma zostac załadowany do Audio Source
    */
    public static string TagZEpoka(string aktTag, Epoki e, string rodzajObiektu = "")
    {
        return aktTag + "_" + e.ToString() + rodzajObiektu;
    }
    //Wylicza wartość modyfikatora zadawanych i otrzymywanych obrażeń
    public static float WyliczModyfikatorObrazeń(float bazowyModyfikator, ushort wartośćIndeksu, bool czyDefence = false)
    {
        return (czyDefence) ? Mathf.LerpUnclamped(bazowyModyfikator, bazowyModyfikator - 0.15f, wartośćIndeksu / 500f) : Mathf.LerpUnclamped(bazowyModyfikator, bazowyModyfikator + 0.75f, wartośćIndeksu / 500f);
        /*
        float warMnoznika = Mathf.Pow(0.98f, wartośćIndeksu);
        return (czyDefence) ? bazowyModyfikator - (0.002f * warMnoznika) : bazowyModyfikator + (0.002f * warMnoznika); //Do +5% na 37 poziomie
        */
    }
    public static void UstawTimeScale(float tScale)
    {
        Time.timeScale = tScale;
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
    /* UNITY_ANDROID
    [SerializeField] public byte godzina;
    [SerializeField] public byte minuta;
    [SerializeField] public int dzień;
    [SerializeField] public byte sekunda;
    [SerializeField] public byte miesiąc;
    [SerializeField] public int rok;
    */
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
    [SerializeField] public float głośność;
    /* UNITY_ANDROID
    [SerializeField] public bool blokadaOrientacji;
    [SerializeField] public bool czyLicznikFPSOn;
    [SerializeField] public bool czyOdwracaćPrzesuwanie;
    [SerializeField] public bool czyPostProcessing;
    */
}

