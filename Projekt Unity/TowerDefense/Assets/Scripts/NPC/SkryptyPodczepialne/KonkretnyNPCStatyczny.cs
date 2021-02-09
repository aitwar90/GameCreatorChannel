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
    private List<NPCClass> wrogowieWZasiegu = null;
    private byte idxAct = 0;
    private ushort kosztNaprawy = 0;
    private GameObject instaObj;
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
    #endregion
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
        if (obiektAtaku != null)
        {
            instaObj = Instantiate(obiektAtaku, this.transform.position, Quaternion.identity);
            instaObj.transform.position = new Vector3(instaObj.transform.position.x, instaObj.transform.position.y + 0.8f, instaObj.transform.position.z);
            instaObj.transform.SetParent(this.transform);
        }
        if (this.odgłosyNPC != null)
        {
            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, "PostawB", true);
        }
        else
        {
            StartCoroutine(CzekajAz());
        }
        RysujHPBar();
    }
    public void MetodaDoOdpaleniaPoWyczekaniu()
    {
        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, "PostawB", true);
    }
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
        kosztNaprawy = (ushort)(1 - (this.AktualneŻycie / this.maksymalneŻycie) * kosztJednostki * 1.05);
    }
    protected override void UpdateMe()
    {
        switch (idxAct)
        {
            case 0:
                if (cel == null && wrogowieWZasiegu != null && wrogowieWZasiegu.Count > 0)
                {
                    ZnajdźNowyCel();
                }
                else if (cel != null && (wrogowieWZasiegu == null || wrogowieWZasiegu.Count == 0))
                {
                    cel = null;
                    if (instaObj.activeInHierarchy)
                    {
                        instaObj.SetActive(false);
                    }
                }
                idxAct++;
                break;
            case 1:
                if (cel != null)
                {
                    Atakuj(false);
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
            wrogowieWZasiegu = null;
            StartCoroutine(SkasujObject(5.0f));
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
    public override void Atakuj(bool wZwarciu)
    {
        if (this.aktualnyReuseAtaku < szybkośćAtaku)
        {
            aktualnyReuseAtaku += Time.deltaTime * 2;
            float f = szybkośćAtaku - aktualnyReuseAtaku;
            if (f <= .2f)
            {
                if (!instaObj.activeInHierarchy)
                {
                    instaObj.SetActive(true);
                    instaObj.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + offWysokość, this.transform.position.z);
                    instaObj.transform.LookAt(cel.transform.position);
                    if (efektyFxStart != null)
                    {
                        efektyFxStart.transform.position = this.transform.position;
                        efektyFxStart.Play();
                    }
                    string s = "";
                    switch (typAtakuWieży)
                    {
                        case TypAtakuWieży.jedenTarget:
                            s = PomocniczeFunkcje.TagZEpoka("AtakBJeden", this.epokaNPC, this.tagRodzajDoDźwięków);
                            break;
                        case TypAtakuWieży.wybuch:
                            s = PomocniczeFunkcje.TagZEpoka("AtakBObszar", this.epokaNPC, this.tagRodzajDoDźwięków);
                            break;
                        case TypAtakuWieży.wszyscyWZasiegu:
                            s = PomocniczeFunkcje.TagZEpoka("AtakBAll", this.epokaNPC, this.tagRodzajDoDźwięków);
                            break;
                    }
                    if (s != "")
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, s, true);
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
                    else
                        f *= 10f;

                    instaObj.transform.position = Vector3.Lerp(cel.transform.position, new Vector3(this.transform.position.x, this.transform.position.y + offWysokość, this.transform.position.z), f);
                }
            }
        }
        else
        {
            instaObj.SetActive(false);
            this.aktualnyReuseAtaku = 0;
            switch (typAtakuWieży)
            {
                case TypAtakuWieży.jedenTarget: //Jeden Target
                    cel.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    //Debug.Log("Cel.AktualneŻycie "+cel.AktualneŻycie);
                    break;
                case TypAtakuWieży.wybuch: //Wybuch
                    Collider[] tabZasięgu = new Collider[4];
                    int iloscCol = Physics.OverlapSphereNonAlloc(cel.transform.position, 1.0f, tabZasięgu, (1 << 8), QueryTriggerInteraction.Collide);
                    for (byte i = 0; i < iloscCol; i++)
                    {
                        NPCClass klasa = tabZasięgu[i].GetComponent<NPCClass>();
                        klasa.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    }
                    break;
                case TypAtakuWieży.wszyscyWZasiegu: //Wszystkie cele
                    for (byte i = 0; i < wrogowieWZasiegu.Count; i++)
                    {
                        wrogowieWZasiegu[i].ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    }
                    break;

            }
            if (cel.NieŻyję)
            {
                //Debug.Log("No to jedziemy");
                //Znajdź nowy target
                UsuńZWrogów(cel);
                ZnajdźNowyCel();
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
                if (PomocniczeFunkcje.odblokowanyPoziomEpoki < this.poziom)
                    c = ";CZERWONY";
                p = ";"+this.poziom.ToString()+";";
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
            if (this.AktualneŻycie < this.maksymalneŻycie)
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
    public void ZnajdźNowyCel()
    {
        if (wrogowieWZasiegu != null)
        {
            if (wrogowieWZasiegu.Count > 0)
            {
                cel = wrogowieWZasiegu[0];
                return;
            }
        }
        cel = null;
    }
    public void DodajDoWrogów(KonkretnyNPCDynamiczny knpcd)
    {
        if (wrogowieWZasiegu == null)
        {
            wrogowieWZasiegu = new List<NPCClass>();
        }
        for (byte i = 0; i < wrogowieWZasiegu.Count; i++)
        {
            if (wrogowieWZasiegu[i] == knpcd)
            {
                return;
            }
        }
        wrogowieWZasiegu.Add(knpcd);
    }
    public void UsuńZWrogów(NPCClass knpcd)
    {
        if (wrogowieWZasiegu == null || wrogowieWZasiegu.Count == 0)
        {
            return;
        }
        List<NPCClass> temp = new List<NPCClass>();
        for (ushort i = 0; i < wrogowieWZasiegu.Count; i++)
        {
            if (wrogowieWZasiegu[i] == knpcd)
            {
                continue;
            }
            else
            {
                temp.Add(wrogowieWZasiegu[i]);
            }
        }
        wrogowieWZasiegu = temp;
    }
    public void Napraw()
    {
        ManagerGryScript.iloscCoinów -= kosztNaprawy;
        this.AktualneŻycie = this.maksymalneŻycie;
        kosztNaprawy = 0;
        RysujHPBar();
        if (PomocniczeFunkcje.managerGryScript.aktualnyPoziomEpoki == 255)   //Jeśli samouczek
        {
            ManagerSamouczekScript.mssInstance.ZmiennaPomocnicza = 1;
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
    public void HealMe()
    {
        if (this.AktualneŻycie > 0)
            this.AktualneŻycie = this.maksymalneŻycie;
    }
}
