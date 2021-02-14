using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Klasa obsługuje statyczne NPC
*/
public class KonkretnyNPCStatyczny : NPCClass, ICzekajAz
{
    #region Zmienne publiczne
    public TypBudynku typBudynku;
    [Range(0, 20)]
    public byte odbiteObrażenia = 1;
    [Header("Granice obiektu względem środka obiektu"), Tooltip("Granica obiektu po osi X")]
    public float granicaX = 0.5f;
    [Tooltip("Granica obiektu po osi Z")]
    public float granicaZ = 0.5f;
    [Tooltip("Koszt badania odblokowania budynku")]
    public ushort kosztBadania = 0;
    [Tooltip("Typ ataku wieży")]
    public TypAtakuWieży typAtakuWieży;
    [Tooltip("Czy budynek jest zablokowany (jeśli tak to znaczy że nie zostały spełnione wymagania, lub nie został wynaleziony")]
    public bool blokowany = true;
    [Tooltip("Obiekt, który atakuje z wieży")]
    public GameObject obiektAtaku;
    [Tooltip("System cząstek wyzwalany kiedy następuje wystrzał z wieży")]
    public ParticleSystem efektyFxStart;
    [Tooltip("System cząstek wyzwalany kiedy nabój dosięga celu")]
    public ParticleSystem efektyFxKoniec;
    [Tooltip("Określ przesunięcie atakującego obiektu względem pozycji obiektu")]
    public float offWysokość = 1.5f;
    public Sprite obrazekDoBudynku = null;
    public Transform sprite = null;
    public string opisBudynku;
    #endregion

    #region Zmienny prywatne
    private MagazynWZasięguWieży rootEnemy = null;
    private KonkretnyNPCDynamiczny[] wrogowieWZasiegu = null;
    public byte iloscWrogowWZasiegu = 0;
    private byte idxAct = 0;
    private ushort kosztNaprawy = 0;
    private Stack<MagazynObiektówAtaków> instaObjOff = null;
    private MagazynObiektówAtaków[] tabActAtakObj = null;
    private bool instaObjIsActive = false;
    [HideInInspector, SerializeField] private bool zablokowany = true;
    #endregion

    #region Zmienne chronione
    #endregion

    #region Getery i setery
    public bool Zablokowany
    {
        get
        {
            return zablokowany;
        }
        set
        {
            zablokowany = value;
        }
    }
    public int ZwrócPoziomOgólny
    {
        get
        {
            if (this.epokaNPC == Epoki.None)
                return -1;
            return (byte)(this.epokaNPC - 1) * 100 + this.poziom;
        }
    }
    #endregion

    ///<summary>Metoda ustawia niezbędne dane dla stawianego budynku.</summary>
    public void InicjacjaBudynku()
    {
        UnityEngine.AI.NavMeshObstacle tmp = null;
        if (!this.gameObject.TryGetComponent<UnityEngine.AI.NavMeshObstacle>(out tmp))
        {
            this.gameObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
        }
        this.AktualneŻycie = this.maksymalneŻycie;
        this.nastawienieNPC = NastawienieNPC.Przyjazne;
        if (sprite == null)
            sprite = this.transform.Find("HpGreen");
        if (this.odgłosyNPC != null)
        {
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, "PostawB", true);
        }
        else
        {
            StartCoroutine(CzekajAz());
        }
        if (this.typAtakuWieży == TypAtakuWieży.wszyscyWZasiegu)
            tabActAtakObj = new MagazynObiektówAtaków[10];
        else
            tabActAtakObj = new MagazynObiektówAtaków[1];
        this.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 0.02f, this.transform.position.z);
        RysujHPBar();
    }
    ///<summary>Metoda odtwarza dźwięk stawianego budynku.</summary>
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, "PostawB", true);
    }
    ///<summary>Metoda czeka póki nie zostanie zainicjowany Component AudioSource odgłosyNPC.</summary>
    public IEnumerator CzekajAz()
    {
        yield return new WaitUntil(() => this.odgłosyNPC != null);
        MetodaDoOdpaleniaPoWyczekaniu();
        yield return null;
    }
    // Update is called once per frame
    protected override void RysujHPBar()
    {
        if ((KonkretnyNPCStatyczny)PomocniczeFunkcje.celWrogów == this)
        {
            //Jeśli to główna baza
            PomocniczeFunkcje.mainMenu.UstawHPGłównegoPaska((float)this.AktualneŻycie / this.maksymalneŻycie);
        }
        if (mainRenderer.isVisible && sprite != null)
        {
            float actScaleX = (float)this.AktualneŻycie / this.maksymalneŻycie;
            sprite.localScale = new Vector3(actScaleX, 1, 1);
            /*
            Vector3 tempPos = this.transform.position;
            tempPos.y += 1.6f;
            Vector2 pozycjaPostaci = Camera.main.WorldToScreenPoint(tempPos);
            GUI.Box(new Rect(pozycjaPostaci.x - 40, Screen.height - pozycjaPostaci.y - 30, 80, 20), this.AktualneŻycie + " / " + maksymalneŻycie);
        */
        }
        kosztNaprawy = (ushort)((1 - (this.AktualneŻycie / (float)this.maksymalneŻycie)) * kosztJednostki * 1.05);
        if (MainMenu.singelton.OdpalonyPanel)
        {
            if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt != null)
            {
                if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt.GetInstanceID() == this.GetInstanceID())
                {
                    PanelStatyczny ps = MainMenu.singelton.GetKontenerKomponentówStatic;
                    if (ps != null)
                    {
                        ps.UstawDaneDynamiczne(new byte[] { 0, 1 }, new string[] { AktualneŻycie.ToString() + "/" + this.maksymalneŻycie.ToString(), kosztNaprawy.ToString() });
                    }
                }
            }
        }
    }
    protected override void UpdateMe()
    {
        switch (idxAct)
        {
            case 0:
                if (cel == null && rootEnemy != null)
                {
                    ZnajdźNowyCel();
                }
                idxAct++;
                break;
            case 1:
                if (cel != null)
                {
                    Atakuj();
                }
                idxAct++;
                break;
            case 2:
                if (sprite != null)
                    sprite.parent.forward = -PomocniczeFunkcje.oCam.transform.forward;
                idxAct = 0;
                break;
        }
    }
    protected override void UsuńJednostkę()
    {
        if (this == PomocniczeFunkcje.celWrogów)
        {
            return;
        }
        if (this.nastawienieNPC == NastawienieNPC.Wrogie)
        {
            StartCoroutine(SkasujObject(5.0f));
        }
        else    //Jeśli nastawienie jest przyjazne
        {
            //Podmień obiekt na zgruzowany

            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, PomocniczeFunkcje.TagZEpoka("ŚmiercB", this.epokaNPC, this.tagRodzajDoDźwięków));
            PomocniczeFunkcje.SkasujElementDrzewa(ref PomocniczeFunkcje.korzeńDrzewaPozycji, this);
            Collider[] tablicaKoliderow = this.GetComponents<Collider>();
            for (byte i = 0; i < tablicaKoliderow.Length; i++)
            {
                tablicaKoliderow[i].enabled = false;
            }
            UnityEngine.AI.NavMeshObstacle tNVO = this.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            short[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position.x, this.transform.position.z);
            byte s = (byte)Mathf.CeilToInt(this.zasięgAtaku / PomocniczeFunkcje.distXZ);
            if (s > 1)
            {
                for (short i = (short)(temp[0] - s); i < (short)(temp[0] + s); i++)
                {
                    if (i > -1 && i < 20)
                    {
                        for (short j = (short)(temp[1] - s); j < (short)(temp[0] + s); j++)
                        {
                            if (j > -1 && j < 20)
                            {
                                UsuńMnieZListy((short)i, (short)j);
                            }
                        }
                    }
                }
            }
            if (tNVO != null)
                tNVO.enabled = false;
            cel = null;
            rootEnemy = null;
            tabActAtakObj = null;
            instaObjOff.Clear();
            StartCoroutine(SkasujObject(2.0f));
        }
        if (this.odgłosyNPC != null)
        {
            PomocniczeFunkcje.muzyka.ustawGłośność -= this.UstawGłośnośćNPC;
        }
    }
    /*
    void OnDrawGizmosSelected()
    {
        if(this.AktualneŻycie > 0)
        {
            short[] tmp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
            byte s = (byte)Mathf.CeilToInt(this.zasięgAtaku / PomocniczeFunkcje.distXZ);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3(tmp[0]*PomocniczeFunkcje.distXZ + PomocniczeFunkcje.aktualneGranicaTab, 1.0f, tmp[1]*PomocniczeFunkcje.distXZ+PomocniczeFunkcje.aktualneGranicaTab), 
            new Vector3(s*PomocniczeFunkcje.distXZ, 1.0f, s*PomocniczeFunkcje.distXZ));
        }
    }
    */
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (cel != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, cel.transform.position);
        }
    }
#endif
    ///<summary>Usuń wroga z tablicy PomocniczeFunkcje.tablicaWież.</summary>
    ///<param name="x">Indeks pozycji wroga na osi X względem podziału PomocniczeFunkcje.tablicaWież (parametr 1).</param>
    ///<param name="z">Indeks pozycji wroga na osi Z względem podziału PomocniczeFunkcje.tablicaWież (parametr 2).</param>
    private void UsuńMnieZListy(short x, short z)
    {
        if (PomocniczeFunkcje.tablicaWież[x, z] == null)
            return;
        List<InformacjeDlaPolWież> tInf = new List<InformacjeDlaPolWież>();
        short dlListy = (short)PomocniczeFunkcje.tablicaWież[x, z].Count;
        for (short i = 0; i < dlListy; i++)
        {
            if (PomocniczeFunkcje.tablicaWież[x, z][i].wieża == this)
            {
                continue;
            }
            else
            {
                tInf.Add(PomocniczeFunkcje.tablicaWież[x, z][i]);
            }
        }
        PomocniczeFunkcje.tablicaWież[x, z] = tInf;
    }
    public override void Atakuj()
    {
        if (this.aktualnyReuseAtaku < szybkośćAtaku)
        {
            aktualnyReuseAtaku += Time.deltaTime * 3.0f;
            float f = szybkośćAtaku - aktualnyReuseAtaku;
            if (f <= .1f)
            {
                if (!instaObjIsActive)
                {
                    instaObjIsActive = true;
                    string s = "";
                    switch (typAtakuWieży)
                    {
                        case TypAtakuWieży.jedenTarget:
                            tabActAtakObj[0] = GetInstaObjFromStack(cel.transform.position.x, cel.transform.position.z);
                            s = PomocniczeFunkcje.TagZEpoka("AtakBJeden", this.epokaNPC, this.tagRodzajDoDźwięków);
                            break;
                        case TypAtakuWieży.wybuch:
                            tabActAtakObj[0] = GetInstaObjFromStack(cel.transform.position.x, cel.transform.position.z);
                            s = PomocniczeFunkcje.TagZEpoka("AtakBObszar", this.epokaNPC, this.tagRodzajDoDźwięków);
                            break;
                        case TypAtakuWieży.wszyscyWZasiegu:
                            if (rootEnemy != null)
                            {
                                wrogowieWZasiegu = rootEnemy.ZwróćMiKonkretneNpc(iloscWrogowWZasiegu);
                                for (byte i = 0; i < wrogowieWZasiegu.Length && i < 10; i++)
                                {
                                    tabActAtakObj[i] = GetInstaObjFromStack(wrogowieWZasiegu[i].transform.position.x, wrogowieWZasiegu[i].transform.position.z);
                                }
                                s = PomocniczeFunkcje.TagZEpoka("AtakBAll", this.epokaNPC, this.tagRodzajDoDźwięków);
                            }
                            break;
                    }
                    if (s != "")
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, s, true);
                    if (efektyFxStart != null)
                    {
                        efektyFxStart.transform.position = this.transform.position;
                        efektyFxStart.Play();
                    }
                }
                else
                {
                    if (f < 0)
                    {
                        f = 0;
                        if (efektyFxKoniec != null)
                        {
                            efektyFxKoniec.transform.position = cel.transform.position;
                            efektyFxKoniec.Play();
                        }
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, PomocniczeFunkcje.TagZEpoka("TrafienieB", this.epokaNPC, this.tagRodzajDoDźwięków), true);
                    }
                    for (byte i = 0; i < tabActAtakObj.Length; i++)
                    {
                        if (tabActAtakObj[i] == null)
                            break;
                        tabActAtakObj[i].SetActPos(f * 10.0f);
                    }
                }
            }
        }
        else  //
        {
            for (byte i = 0; i < tabActAtakObj.Length; i++)
            {
                if (tabActAtakObj[i] == null)
                    break;
                DodajDoMagazynuObiektówAtaku(ref tabActAtakObj[i]);
                tabActAtakObj[i].DeactivateObj();
                tabActAtakObj[i] = null;
            }
            this.aktualnyReuseAtaku = 0;
            instaObjIsActive = false;
            switch (typAtakuWieży)
            {
                case TypAtakuWieży.jedenTarget: //Jeden Target
                    cel.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    if (cel.NieŻyję)
                    {
                        //Debug.Log("No to jedziemy");
                        //Znajdź nowy target
                        UsuńZWrogów(cel);
                        ZnajdźNowyCel();
                    }
                    break;
                case TypAtakuWieży.wybuch: //Wybuch
                    Collider[] tabZasięgu = new Collider[4];
                    int iloscCol = Physics.OverlapSphereNonAlloc(cel.transform.position, 1.0f, tabZasięgu, (1 << 8), QueryTriggerInteraction.Collide);
                    for (byte i = 0; i < iloscCol; i++)
                    {
                        NPCClass klasa = tabZasięgu[i].GetComponent<NPCClass>();
                        klasa.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    }
                    if (cel.NieŻyję)
                    {
                        //Debug.Log("No to jedziemy");
                        //Znajdź nowy target
                        UsuńZWrogów(cel);
                        ZnajdźNowyCel();
                    }
                    break;
                case TypAtakuWieży.wszyscyWZasiegu: //Wszystkie cele
                    if (rootEnemy != null)
                    {
                        for (byte i = 0; i < wrogowieWZasiegu.Length; i++)
                        {
                            wrogowieWZasiegu[i].ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                        }
                        if (cel.NieŻyję)
                        {
                            //Debug.Log("No to jedziemy");
                            //Znajdź nowy target
                            //UsuńZWrogów(cel);
                            ZnajdźNowyCel();
                        }
                    }
                    break;

            }
        }
    }
    public override byte ZwrócOdbiteObrażenia()
    {
        return (byte)Mathf.CeilToInt(odbiteObrażenia * this.modyfikatorZadawanychObrażeń);
    }
    public override float PobierzGranice()
    {
        return (granicaX + granicaZ) / 2f;
    }
    public override void UstawPanel(Vector2 pos)
    {
        if (pos.x == float.NegativeInfinity)    //Jeśli odpalany jest panel budynku
        {
            string p = "";
            string c = ";ZIELONY";
            if (PomocniczeFunkcje.odblokowaneEpoki >= (byte)this.epokaNPC)
            {
                if (PomocniczeFunkcje.odblokowanyPoziomEpoki < this.poziom || PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki < this.poziom)
                    c = ";CZERWONY";
                p = ";" + this.poziom.ToString() + ";";
            }
            else
            {
                p = ";ERROR;";
                c = ";CZERWONY";
            }
            string kosztBadaniaS = "0";
            if (this.zablokowany)
            {
                kosztBadaniaS = kosztBadania.ToString();
            }
            PomocniczeFunkcje.mainMenu.UstawPanelUI("PANEL;" + this.nazwa +
            ";" + this.maksymalneŻycie.ToString() + ";" + this.kosztJednostki.ToString() +
            ";" + kosztBadaniaS + ";" + this.zadawaneObrażenia +
            p + this.opisBudynku + c,
            Vector2.zero);
        }
        else
        {
            bool czyOdbl = false;
            if (this.AktualneŻycie < this.maksymalneŻycie && ManagerGryScript.iloscCoinów >= this.kosztNaprawy)
                czyOdbl = true;
            string s = "STATYCZNY;" + czyOdbl.ToString() + ";" + this.nazwa + ";" + this.AktualneŻycie.ToString() + "/" + this.maksymalneŻycie.ToString() + ";" + kosztNaprawy.ToString() + ";" + zadawaneObrażenia.ToString() + ";" + opisBudynku + ";" + "0";
            PomocniczeFunkcje.mainMenu.UstawPanelUI(s, pos, this);
            if (this.typBudynku == TypBudynku.Akademia)
            {
                PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(true);
            }
            else
            {
                PomocniczeFunkcje.mainMenu.OdpalButtonyAkademii(false);
            }
        }
    }
    ///<summary>Znajdź nowy cel do ataku dla wiezy.</summary>
    private void ZnajdźNowyCel()
    {
        if (rootEnemy != null)
        {
            if (rootEnemy != null)
            {
                cel = (NPCClass)rootEnemy.ZwróćMiKonkretneNpc(1).GetValue(0);
                return;
            }
        }
        cel = null;
    }
    ///<summary>Dodaj wroga ze struktury wrogów.</summary>
    ///<param name="knpcd">Referencja wroga, który ma zostać dodany do struktury wrogów.</param>
    public void DodajDoWrogów(KonkretnyNPCDynamiczny knpcd)
    {
        if (rootEnemy == null)
        {
            rootEnemy = new MagazynWZasięguWieży(ref knpcd);
            iloscWrogowWZasiegu++;
        }
        else
        {
            rootEnemy.AddMagazyn(ref knpcd);
            iloscWrogowWZasiegu++;
        }
    }
    ///<summary>Usuń wroga ze struktury wrogów.</summary>
    ///<param name="knpcd">Referencja wroga, który ma zostać usunięty ze struktury wrogów.</param>
    public void UsuńZWrogów(NPCClass knpcd)
    {
        if (rootEnemy == null)
        {
            return;
        }
        else
        {
            MagazynWZasięguWieży mzw = rootEnemy.DeleteMe(ref knpcd);
            if (mzw != null)
            {
                if (rootEnemy == mzw)
                    rootEnemy = null;
                else
                {
                    rootEnemy = mzw;
                }
            }
            iloscWrogowWZasiegu--;
        }
    }
    ///<summary>Metoda naprawiająca budynek.</summary>
    ///<param name="payForRepair">Czy gracz płaci za naprawę budynku.</param>
    public void Napraw(bool payForRepair = true)
    {
        if (payForRepair)
        {
            ManagerGryScript.iloscCoinów -= kosztNaprawy;
            MainMenu.singelton.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
        }
        this.AktualneŻycie = this.maksymalneŻycie;
        kosztNaprawy = 0;
        RysujHPBar();
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)   //Jeśli samouczek
        {
            if (ManagerSamouczekScript.mssInstance.CzyZgadzaSięIDXGłówny(2))    //Naprawa głównego budynku
            {
                ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = 1;
            }
        }
    }
    public override void UstawJezykNPC(string coZmieniam, string podmianaWartosci)
    {
        base.UstawJezykNPC(coZmieniam, podmianaWartosci);
        if (coZmieniam == "opis")
        {
            this.opisBudynku = podmianaWartosci;
            return;
        }
    }
    ///<summary>Metoda naprawiająca budynek bez opłacania.</summary>
    public void HealMe()
    {
        Napraw(false);
    }
    ///<summary>Metoda ulepszająca budynki przez Akademię.</summary>
    ///<param name="whatUpgrade">Parametr określający co ma zostać ulepszone (0 - MaxHP), (1 - Atak), (2 - Obrona).</param>
    public void UpgradeMe(int whatUpgrade)
    {
        switch (whatUpgrade)
        {
            case 0: //HP
                this.maksymalneŻycie = (short)(this.maksymalneŻycie + 10 * PomocniczeFunkcje.managerGryScript.hpIdx);
                kosztNaprawy = (ushort)((1 - (this.AktualneŻycie / this.maksymalneŻycie)) * kosztJednostki * 1.05);
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt != null)
                    {
                        if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt.GetInstanceID() == this.GetInstanceID())
                        {
                            PanelStatyczny ps = MainMenu.singelton.GetKontenerKomponentówStatic;
                            if (ps != null)
                            {
                                ps.UstawDaneDynamiczne(new byte[] { 0, 1 }, new string[] { AktualneŻycie.ToString() + "/" + this.maksymalneŻycie.ToString(), kosztNaprawy.ToString() });
                            }
                        }
                    }
                }
                break;
            case 1: //Attack
                this.modyfikatorZadawanychObrażeń = PomocniczeFunkcje.WyliczModyfikatorObrazeń(this.modyfikatorZadawanychObrażeń, PomocniczeFunkcje.managerGryScript.atkIdx);
                break;
            case 2: //Defence
                this.modyfikatorOtrzymywanychObrażeń = PomocniczeFunkcje.WyliczModyfikatorObrazeń(this.modyfikatorOtrzymywanychObrażeń, PomocniczeFunkcje.managerGryScript.defIdx);
                break;
        }
    }
    private void DodajDoMagazynuObiektówAtaku(ref MagazynObiektówAtaków moa)
    {
        if (instaObjOff == null)
        {
            instaObjOff = new Stack<MagazynObiektówAtaków>();
        }
        instaObjOff.Push(moa);
    }
    ///<summary>Funkcja zwraca obiekt ataku ze stotu, lub go generuje jeśli stos jest pusty.</summary>
    ///<param name="x">Parametr określający pozycję docelową ataku na osi X.</param>
    ///<param name="z">Parametr określający pozycję docelową ataku na osi Z.</param>
    private MagazynObiektówAtaków GetInstaObjFromStack(float x, float z)
    {
        if (instaObjOff != null && instaObjOff.Count > 0)
        {
            MagazynObiektówAtaków moa = instaObjOff.Pop();
            moa.ActivateObj(x, z);
            return moa;
        }
        else
        {
            if (obiektAtaku == null)
            {
                Debug.Log("Obiekt ataku jest null");
                return null;
            }
            GameObject gos = GameObject.Instantiate(obiektAtaku, this.transform.position + new Vector3(0, offWysokość, 0), Quaternion.identity);
            gos.transform.SetParent(this.transform);
            MagazynObiektówAtaków moa = new MagazynObiektówAtaków(x, z, this.transform.position.x, offWysokość, this.transform.position.z, gos.transform);
            return moa;
        }
    }
}
