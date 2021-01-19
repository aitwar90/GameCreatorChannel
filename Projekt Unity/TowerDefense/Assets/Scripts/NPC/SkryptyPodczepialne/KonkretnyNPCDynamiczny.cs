using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/*
Klasa obsługuje dynamiczne NPC
*/
public class KonkretnyNPCDynamiczny : NPCClass
{
    #region Zmienne publiczne
    public byte ileCoinówZaZabicie = 1;
    [Tooltip("Obiekt będący wystrzałem od NPC")]
    public GameObject obiektAtakuDystansowego;
    [Tooltip("System cząstek wyzwalany kiedy następuje wystrzał z wieży")]
    public ParticleSystem efektyFxStart;
    [Tooltip("System cząstek wyzwalany kiedy nabój dosięga celu")]
    public ParticleSystem efektyFxKoniec;
    public Transform sprite = null;
    #endregion
    #region Zmienny prywatne
    public bool rysujPasekŻycia = false;
    private NavMeshAgent agent = null;
    private NavMeshPath ścieżka = null;
    private sbyte głównyIndex = -1;
    private short actXIdx = 32767;
    private short actZIdx = 32767;
    private bool czyDodawac = false;
    private byte[] ostatnieStrony = null;
    private GameObject _obiektAtaku = null;
    private Animator anima;
    private bool[] bufferAnima = new bool[] { false, false, false }; //isDeath, haveTarget, inRange
    private string nId;
    private bool czekamNaZatwierdzenieŚcieżki = false;
    #endregion

    #region Zmienne chronione
    #endregion

    #region Getery i setery
    public bool RysujPasekŻycia
    {
        get
        {
            return rysujPasekŻycia;
        }
        set
        {
            rysujPasekŻycia = value;
        }
    }
    public sbyte SetGłównyIndex
    {
        set
        {
            głównyIndex = value;
        }
    }
    public string NID
    {
        get
        {
            return nId;
        }
    }
    public bool GetIsOnnavmesh
    {
        get
        {
            return agent.isOnNavMesh;
        }
    }
    public Animator GetAnimator
    {
        get
        {
            return this.anima;
        }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        anima = this.GetComponent<Animator>();
        if (agent == null)
        {
            DodajNavMeshAgent();
        }
        this.AktualneŻycie = this.maksymalneŻycie;
        short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position.x, this.transform.position.z);
        this.DodajMnieDoListyWrogowWiezy(t[0], t[1], true);
        actXIdx = t[0];
        actZIdx = t[1];
        if (sprite == null)
            sprite = this.transform.Find("HpGreen");
        if (!rysujPasekŻycia)
        {
            sprite.parent.gameObject.SetActive(false);
        }
        if (obiektAtakuDystansowego != null)
        {
            _obiektAtaku = Instantiate(obiektAtakuDystansowego, this.transform.position, this.transform.rotation);
            _obiektAtaku.transform.SetParent(this.transform);
        }
        if (ZwróćMiWartośćParametru(1) == 0)
        {
            ObsluzAnimacje(ref anima, "haveTarget", false);
            UstawMiWartośćParametru(1, false);
        }
        if (ZwróćMiWartośćParametru(2) == 0)
        {
            ObsluzAnimacje(ref anima, "inRange", false);
            UstawMiWartośćParametru(2, false);
        }
        nId = this.name.Split('(').GetValue(0).ToString();
        RysujHPBar();
    }

    // Update is called once per frame
    protected override void UpdateMe()
    {
        switch (głównyIndex)
        {
            case -1:
                if (cel == null)
                {
                    bool ff = PomocniczeFunkcje.ZwykłeAI(this);
                    if (!ff && _obiektAtaku.activeInHierarchy)
                    {
                        _obiektAtaku.SetActive(false);
                    }
                }
                ObsłużNavMeshAgent(cel.transform.position.x, cel.transform.position.z);
                głównyIndex++;
                break;
            case 0:
                bool f = PomocniczeFunkcje.ZwykłeAI(this);
                if (!f && _obiektAtaku.activeInHierarchy)
                {
                    _obiektAtaku.SetActive(false);
                }
                głównyIndex++;
                break;
            case 1:
                if (cel != null && !czekamNaZatwierdzenieŚcieżki)
                {
                    ObsłużNavMeshAgent(cel.transform.position.x, cel.transform.position.z);
                }
                głównyIndex++;
                break;
            case 2: //Ustaw index tablicy dla npc i usuń stare wieże
                short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position.x, this.transform.position.z);
                List<byte> sQt = new List<byte>();
                bool c = false;
                if (actXIdx > t[0])
                {
                    sQt.Add(0);
                    c = true;
                }
                else if (actXIdx < t[0])
                {
                    sQt.Add(1);
                    c = true;
                }
                if (actZIdx > t[1])
                {
                    sQt.Add(2);
                    c = true;
                }
                else if (actZIdx < t[1])
                {
                    sQt.Add(3);
                    c = true;
                }
                if (c)
                {
                    //Usunięcie starych wież
                    ostatnieStrony = sQt.ToArray();
                    UsuńMnieZTablicyWież(false, ostatnieStrony);
                    czyDodawac = true;
                    actXIdx = t[0];
                    actZIdx = t[1];
                    //Debug.Log("Nowe ustawienie X = "+actXIdx+" Z = "+actZIdx);
                }
                głównyIndex++;
                break;
            case 4: //Dodanie do nowych wież
                if (czyDodawac)
                {
                    DodajMnieDoListyWrogowWiezy(actXIdx, actZIdx);
                    czyDodawac = false;
                }
                głównyIndex++;
                break;
            case 5:
                //Ustaw widoczniść paskaHP
                if (sprite != null)
                    sprite.parent.forward = -PomocniczeFunkcje.oCam.transform.forward;
                głównyIndex = 0;
                break;
            default:
                głównyIndex++;
                break;
        }
    }
    public override sbyte ZwróćMiWartośćParametru(byte i)
    {
        sbyte toRet = -1;
        switch (i)
        {
            case 0:
                toRet = (bufferAnima[i] == true) ? (sbyte)0 : (sbyte)1;
                break;
            case 1:
                toRet = (bufferAnima[i] == true) ? (sbyte)0 : (sbyte)1;
                break;
            case 2:
                toRet = (bufferAnima[i] == true) ? (sbyte)0 : (sbyte)1;
                break;
        }
        return toRet;
    }
    public override void UstawMiWartośćParametru(byte parametr, bool value)
    {
        switch (parametr)
        {
            case 0:
                bufferAnima[0] = value;
                break;
            case 1:
                bufferAnima[1] = value;
                break;
            case 2:
                bufferAnima[2] = value;
                break;
        }
    }
    protected override void RysujHPBar()
    {
        if (!rysujPasekŻycia && SpawnerHord.actualHPBars <= 20 && mainRenderer.isVisible)
        {
            rysujPasekŻycia = true;
            sprite.parent.gameObject.SetActive(true);
            SpawnerHord.actualHPBars++;
        }
        else if (rysujPasekŻycia && SpawnerHord.actualHPBars > 20)
        {
            rysujPasekŻycia = false;
            sprite.parent.gameObject.SetActive(false);
        }
        if (rysujPasekŻycia)
        {
            float actScaleX = (float)this.AktualneŻycie / this.maksymalneŻycie;
            sprite.localScale = new Vector3(actScaleX, 1, 1);
            /*
            Vector3 tempPos = this.transform.position;
            tempPos.y += 1.6f;
            Vector2 pozycjaPostaci = Camera.main.WorldToScreenPoint(tempPos);
            GUI.Box(new Rect(pozycjaPostaci.x - 30, Screen.height - pozycjaPostaci.y - 30, 60, 20), this.AktualneŻycie + " / " + maksymalneŻycie);
            */
        }
    }
    protected override void UsuńJednostkę()
    {
        PomocniczeFunkcje.muzyka.WłączTymczasowyClip(PomocniczeFunkcje.TagZEpoka("ŚmiercNPC", this.epokaNPC, this.tagRodzajDoDźwięków), this.transform.position);
        this.AktualneŻycie = -1;

        if (this.rysujPasekŻycia)
        {
            if (SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars--;
        }
        this.rysujPasekŻycia = false;
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚciezkę;
        ManagerGryScript.iloscAktywnychWrogów--;
        ManagerGryScript.iloscCoinów += this.ileCoinówZaZabicie;
        if (ManagerGryScript.iloscAktywnychWrogów == 0)
        {
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćFal", SpawnerHord.actFala.ToString() + "/" + SpawnerHord.iloscFalNaKoncu.ToString());
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
        }
        WyczyscDaneDynamic();
        UsuńMnieZTablicyWież(true);
        actXIdx = 32767;
        actZIdx = 32767;
        cel = null;
    }
    public override void UstawPanel(Vector2 pos)
    {
        string s = "DYNAMICZNY_" + this.nazwa.ToString() + "_" + this.AktualneŻycie.ToString() + "/" + this.maksymalneŻycie.ToString() + "_" + this.zadawaneObrażenia.ToString();
        PomocniczeFunkcje.mainMenu.UstawPanelUI(s, pos);
    }
    public void WyczyscDaneDynamic(bool wymuszonaKasacja = false)
    {
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚciezkę;
        if (!wymuszonaKasacja)
        {
            if (this.nastawienieNPC == NastawienieNPC.Wrogie)
            {
                PomocniczeFunkcje.DodajDoStosuTrupów(this);
            }
            else
            {
                StartCoroutine(SkasujObject(3.0f));
            }
        }
        WłWyłObj(false);
    }
    private void ObsłużNavMeshAgent(float x, float z)
    {
        //https://www.binpress.com/unity-3d-ai-navmesh-navigation/
        //Logika nav mesha
        if (agent.enabled && !agent.hasPath /*|| this.ostatniTargetPozycja != docelowaPozycja*/)
        {
            if(głównyIndex == -1)
            {
                GenerujŚcieżke(x, z);
            }
            else if(!czekamNaZatwierdzenieŚcieżki)
            {
                czekamNaZatwierdzenieŚcieżki = true;
                StartCoroutine(WyliczŚciezkę(UnityEngine.Random.Range(0f, 0.5f), x, z));
            }
        }
    }
    private IEnumerator WyliczŚciezkę(float f, float x, float z)
    {
        yield return new WaitForSeconds(f);
        GenerujŚcieżke(x, z);
    }
    private void GenerujŚcieżke(float x, float z)
    {
        if(ścieżka == null)
            ścieżka = new NavMeshPath();
        bool czyOdnalzazłemŚcieżkę = agent.CalculatePath(new Vector3(x, 0, z), ścieżka);
        if (ścieżka.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(ścieżka);
            czekamNaZatwierdzenieŚcieżki = false;
            /*
            GameObject goo = new GameObject("waypoints");
            for(ushort i = 0; i < ścieżka.corners.Length; i++)
            {
                GameObject gooo = new GameObject(i+" i");
                Transform t = gooo.GetComponent<Transform>();
                t.position = ścieżka.corners[i];
                t.SetParent(goo.transform);
            }
            //if (ostatniTargetPozycja != docelowaPozycja)
            //    ostatniTargetPozycja = docelowaPozycja;
            */
        }
        else
        {
            cel = WyszukajNajbliższyObiekt() as KonkretnyNPCStatyczny;
            //ObsłużNavMeshAgent(cel.transform.position.x, cel.transform.position.z);
        }
    }
    public void ResetujŚcieżki()
    {
        this.agent.ResetPath();
    }
    public void WłWyłObj(bool enab = false)
    {
        if (enab)
        {
            sprite.localScale = new Vector3(1, 1, 1);
            this.gameObject.SetActive(true);
            short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position.x, this.transform.position.z);
            actXIdx = t[0];
            actZIdx = t[1];
            //ObsluzAnimacje(ref anima, "isDeath", false);
            this.RysujHPBar();
            if (this.odgłosyNPC != null)
            {
                PomocniczeFunkcje.muzyka.ustawGłośność += this.UstawGłośnośćNPC;
            }
            this._obiektAtaku.transform.position = this.transform.position;
            anima.speed = 1.0f;
            głównyIndex = -1;
        }
        agent.enabled = enab;
        if (!enab)
        {
            //ObsluzAnimacje(ref anima, "isDeath", true);
            sprite.parent.gameObject.SetActive(false);
            anima.speed = 0.0f;
            this.gameObject.SetActive(false);
            if (this.odgłosyNPC != null)
            {
                PomocniczeFunkcje.muzyka.ustawGłośność -= this.UstawGłośnośćNPC;
            }
        }
    }
    public override void ResetujŚciezkę(KonkretnyNPCStatyczny taWiezaPierwszyRaz = null)
    {
        if (taWiezaPierwszyRaz != null)
        {
            DodajMnieDoListyWrogowWiezy(actXIdx, actZIdx, false, taWiezaPierwszyRaz);
        }
        ResetujŚcieżki();
    }
    private void DodajNavMeshAgent()
    {
        agent = this.gameObject.AddComponent<NavMeshAgent>();
        agent.stoppingDistance = (zasięgAtaku == 0) ? 1.5f : zasięgAtaku;
    }
    public override void Atakuj(bool wZwarciu)
    {
        AtakujCel(wZwarciu);
    }
    private void AtakujCel(bool czyWZwarciu)
    {
        if (aktualnyReuseAtaku < szybkośćAtaku)
        {
            aktualnyReuseAtaku += Time.deltaTime * 2f;
            float f = szybkośćAtaku - aktualnyReuseAtaku;
            if (f <= 0.1f)
            {
                bool czyPrzetwarzac = ((czyWZwarciu && mainRenderer.isVisible) || !czyWZwarciu) ? true : false;
                if (_obiektAtaku != null)
                {
                    if (!_obiektAtaku.activeInHierarchy)
                    {
                        if (czyPrzetwarzac)
                        {
                            _obiektAtaku.SetActive(true);
                            if (czyWZwarciu && mainRenderer.isVisible)
                            {
                                if (efektyFxStart != null && czyPrzetwarzac)
                                {
                                    efektyFxStart.transform.position = this.transform.position;
                                    efektyFxStart.Play();
                                }
                                _obiektAtaku.transform.LookAt(cel.transform.position);
                            }
                        }
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, (this.typNPC == TypNPC.WalczyNaDystans || this.typNPC == TypNPC.WalczynaDystansIWZwarciu) ?
                        PomocniczeFunkcje.TagZEpoka("AtakNPCDystans", this.epokaNPC, this.tagRodzajDoDźwięków) :
                        PomocniczeFunkcje.TagZEpoka("AtakNPCZwarcie", this.epokaNPC, this.tagRodzajDoDźwięków), true);
                    }
                    else
                    {
                        if (f < 0)
                        {
                            f = 0;
                            if (efektyFxKoniec != null && czyPrzetwarzac)
                            {
                                efektyFxKoniec.transform.position = cel.transform.position;
                                efektyFxKoniec.Play();
                            }
                            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, (this.typNPC == TypNPC.WalczyNaDystans || this.typNPC == TypNPC.WalczynaDystansIWZwarciu) ?
                            PomocniczeFunkcje.TagZEpoka("TrafienieNPC", this.epokaNPC, this.tagRodzajDoDźwięków) :
                            PomocniczeFunkcje.TagZEpoka("TrafienieNPC", this.epokaNPC, this.tagRodzajDoDźwięków), true);
                        }
                        else
                        {
                            f *= 10f;
                        }
                        if (czyPrzetwarzac)
                            _obiektAtaku.transform.position = Vector3.Lerp(cel.transform.position, this.transform.position, f);
                    }
                }

            }
            return;
        }
        if (_obiektAtaku.activeInHierarchy)
        {
            _obiektAtaku.SetActive(false);
        }
        this.transform.LookAt(cel.transform.position);
        aktualnyReuseAtaku = 0.0f;
        cel.ZmianaHP((short)Mathf.FloorToInt((zadawaneObrażenia * this.modyfikatorZadawanychObrażeń)));
        if (czyWZwarciu)
            this.ZmianaHP(cel.ZwrócOdbiteObrażenia());
    }
    public KonkretnyNPCStatyczny WyszukajNajbliższyObiekt()
    {
        Component knpcs = PomocniczeFunkcje.WyszukajWDrzewie(ref PomocniczeFunkcje.korzeńDrzewaPozycji, this.transform.position);
        if (knpcs == null)
        {
            return null;
        }
        else
        {
            return (KonkretnyNPCStatyczny)knpcs;
        }
    }
    private void DodajMnieDoListyWrogowWiezy(short x, short z, bool pierwszyRaz = false, KonkretnyNPCStatyczny taWiezaPierwszyRaz = null)
    {
        if (PomocniczeFunkcje.SprawdźCzyWykraczaPozaZakresTablicy(x, z))
        {
            return;
        }

        if (PomocniczeFunkcje.tablicaWież[x, z] == null)
        {
            PomocniczeFunkcje.tablicaWież[x, z] = new List<InformacjeDlaPolWież>();
        }
        if (pierwszyRaz)
        {
            for (byte i = 0; i < PomocniczeFunkcje.tablicaWież[x, z].Count; i++)
            {
                PomocniczeFunkcje.tablicaWież[x, z][i].wieża.DodajDoWrogów(this);
            }
        }
        else if (taWiezaPierwszyRaz != null)
        {
            for (byte i = 0; i < PomocniczeFunkcje.tablicaWież[x, z].Count; i++)
            {
                if (PomocniczeFunkcje.tablicaWież[x, z][i].wieża == taWiezaPierwszyRaz)
                {
                    PomocniczeFunkcje.tablicaWież[x, z][i].wieża.DodajDoWrogów(this);
                }
            }
        }
        else if (ostatnieStrony != null)
        {
            for (byte sq = 0; sq < ostatnieStrony.Length; sq++)
            {
                if (ostatnieStrony[sq] == 0)
                    ostatnieStrony[sq] = 1;
                else if (ostatnieStrony[sq] == 1)
                    ostatnieStrony[sq] = 0;
                else if (ostatnieStrony[sq] == 3)
                    ostatnieStrony[sq] = 2;
                else if (ostatnieStrony[sq] == 2)
                    ostatnieStrony[sq] = 3;

                for (byte i = 0; i < PomocniczeFunkcje.tablicaWież[x, z].Count; i++)
                {
                    if (PomocniczeFunkcje.tablicaWież[x, z][i].ZwrócCzyWieżaPosiadaStrone(ostatnieStrony[sq]))
                    {
                        if (PomocniczeFunkcje.tablicaWież[x, z][i].odlOdGranicy == 1)
                        {
                            PomocniczeFunkcje.tablicaWież[x, z][i].wieża.DodajDoWrogów(this);
                        }
                    }
                }
            }
        }
        ostatnieStrony = null;
    }
    private void UsuńMnieZTablicyWież(bool czyCalkowicie = false, byte[] sQ = null)
    {
        if (PomocniczeFunkcje.SprawdźCzyWykraczaPozaZakresTablicy(actXIdx, actZIdx) || PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx] == null
            || PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx].Count == 0)
            return;

        if (czyCalkowicie)
        {
            for (ushort i = 0; i < PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx].Count; i++)
            {
                //Usunięcie tego npc 
                PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].wieża.UsuńZWrogów(this);
            }
        }
        else
        {
            if (sQ != null && sQ.Length > 0)
            {
                for (ushort i = 0; i < PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx].Count; i++)
                {
                    for (byte sq = 0; sq < sQ.Length; sq++)
                    {
                        /*
                        if(sQ[sq] == 0)
                            sQ[sq] = 1;
                        else if(sQ[sq] == 1)
                            sQ[sq] = 0;
                        else if(sQ[sq] == 2)
                            sQ[sq] = 3;
                        else if(sQ[sq] == 3)
                            sQ[sq] = 2;
                            */
                        if (PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].odlOdGranicy == 1)
                        {
                            if (PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].ZwrócCzyWieżaPosiadaStrone(sQ[sq]))
                            {
                                PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].wieża.UsuńZWrogów(this);
                            }
                        }
                    }
                }
            }
        }
    }
}
