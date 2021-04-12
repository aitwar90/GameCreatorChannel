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
    [Tooltip("System cząstek wyzwalany kiedy następuje atak")]
    public ParticleSystem efektyFxStart;
    [Tooltip("System cząstek wyzwalany kiedy nabój dosięga celu")]
    public ParticleSystem efektyFxKoniec;
    public Transform sprite = null;
    [Header("Ustaw broń przeciwnika"), Tooltip("Pozycja broni")]
    public Transform posRęki;
    [Tooltip("Obiekt będący wystrzałem od NPC")]
    public GameObject obiektAtakuDystansowego;
    [Header("Ustaw NavMeshAgent")]
    [Tooltip("Prędkość poruszania się npc")]
    public float prędkość = 1.0f;
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
    private MagazynObiektówAtaków _obiektAtaku = null;
    private Animator anima;
    private bool[] bufferAnima = new bool[] { false, false, false }; //isDeath, haveTarget, inRange
    private string nId;
    private bool czekamNaZatwierdzenieŚcieżki = false;
    private bool czyAtakJestAktywny = false;
    private bool czySynchronizuje = false;
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
            GameObject go = Instantiate(obiektAtakuDystansowego, posRęki);
            //go.transform.SetParent(posRęki);
            go.transform.localPosition = Vector3.zero;
            go.transform.localEulerAngles = Vector3.zero;
            if (this.typNPC == TypNPC.WalczyNaDystans)
                go.transform.localScale = new Vector3(0.5f, 0.5f, -0.5f);
            else
                go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            _obiektAtaku = new MagazynObiektówAtaków(0, 0, 0, 0, 0, go.transform);
        }
        if (ZwróćMiWartośćParametru(1) == 0)
        {
            ObsluzAnimacje(ref anima, "haveTarget", false);
        }
        if (ZwróćMiWartośćParametru(2) == 0)
        {
            ObsluzAnimacje(ref anima, "inRange", false);
        }
        nId = this.name.Split('(').GetValue(0).ToString();
        SetGłównyIndexDiffValue();
        RysujHPBar();
    }

    // Update is called once per frame
    protected override void UpdateMe()
    {
        if (!this.NieŻyję)
        {
            if (czyAtakJestAktywny && this.typNPC == TypNPC.WalczyNaDystans)
            {
                if(!czySynchronizuje)
                    Atakuj();
            }
            switch (głównyIndex)
            {
                case -1:
                    if (cel == null)
                    {
                        bool ff = PomocniczeFunkcje.ZwykłeAI(this);
                        if (!ff && _obiektAtaku.CzyAktywny)
                        {
                            _obiektAtaku.DeactivateObj();
                        }
                    }
                    ObsłużNavMeshAgent(cel.transform.position.x, cel.transform.position.z);
                    głównyIndex++;
                    break;

                case 0:
                    if (!czySynchronizuje && (!czyAtakJestAktywny || this.typNPC != TypNPC.WalczyNaDystans))
                        PomocniczeFunkcje.ZwykłeAI(this);
                    głównyIndex++;
                    break;

                case 1:
                    if (cel != null && (!czekamNaZatwierdzenieŚcieżki || this.agent.velocity != Vector3.zero))
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
                        SprawdźCzyWidocznaPozycja(true);
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
    protected override void UstawMiWartośćParametru(byte parametr, bool value)
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
        if (!rysujPasekŻycia && SpawnerHord.actualHPBars <= 25 && SprawdźCzyWidocznaPozycja())
        {
            rysujPasekŻycia = true;
            sprite.parent.gameObject.SetActive(true);
            SpawnerHord.actualHPBars++;
        }
        else if (rysujPasekŻycia && SpawnerHord.actualHPBars > 25)
        {
            rysujPasekŻycia = false;
            sprite.parent.gameObject.SetActive(false);
            if (SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars--;
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
        if (MainMenu.singelton.OdpalonyPanel)
        {
            if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt != null)
            {
                if (PomocniczeFunkcje.managerGryScript.zaznaczonyObiekt.GetInstanceID() == this.GetInstanceID())
                {
                    PanelDynamiczny ps = MainMenu.singelton.GetKontenerKomponentówDynamic;
                    if (ps != null)
                    {
                        ps.UstawDaneDynamiczne(0, AktualneŻycie.ToString() + "/" + this.maksymalneŻycie.ToString());
                    }
                }
            }
        }
    }
    protected override void UsuńJednostkę()
    {
        MuzykaScript.singleton.WłączTymczasowyClip(PomocniczeFunkcje.TagZEpoka("ŚmiercNPC", this.epokaNPC, this.tagRodzajDoDźwięków), this.transform.position);
        ObsluzAnimacje(ref anima, "isDeath", true);
        this.AktualneŻycie = -1;

        if (this.rysujPasekŻycia)
        {
            if (SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars--;
            this.rysujPasekŻycia = false;
        }
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚciezkę;
        ManagerGryScript.iloscAktywnychWrogów--;
        PomocniczeFunkcje.managerGryScript.ZmodyfikujIlośćCoinów(this.ileCoinówZaZabicie);
        //ManagerGryScript.iloscCoinów += this.ileCoinówZaZabicie;
        PomocniczeFunkcje.managerGryScript.DodajDoWartościStatystyk(4, this.ileCoinówZaZabicie);
        if (ManagerGryScript.iloscAktywnychWrogów == 0)
        {
            PomocniczeFunkcje.managerGryScript.RozgrywkaPoWalkaPrzełącz();
            /*
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
            PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćFal", SpawnerHord.actFala.ToString() + "/" + SpawnerHord.iloscFalNaKoncu.ToString());
            PomocniczeFunkcje.mainMenu.WłączWyłączPanel("ui_down", true);
            */
        }
        if(this.agent != null && this.agent.isOnNavMesh)
            this.agent.isStopped = true;
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
    ///<summary>Czyści dane przy kasowaniu wroga.</summary>
    ///<param name="wymuszonaKasacja">Czy kasacja jest wynikiem użycia cudu ocalenia?.</param>
    public void WyczyscDaneDynamic(bool wymuszonaKasacja = false)
    {
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚciezkę;
        if (!wymuszonaKasacja)
        {
            if (this.nastawienieNPC == NastawienieNPC.Wrogie)
            {
                PomocniczeFunkcje.DodajDoStosuTrupów(this);
                //this.GetComponent<NavMeshObstacle>().enabled = false;
                //this.GetComponent<SphereCollider>().enabled = false;
            }
            else
            {
                StartCoroutine(SkasujObject(3.0f));
            }
        }
        if(_obiektAtaku != null) _obiektAtaku.DeactivateObj();
        StartCoroutine(WyłObjTimer());
    }
    ///<summary>Metoda generuje trasę dla wroga. Określa logikę postępowania i rozdziela zadania.</summary>
    ///<param name="x">Pozycja na osi X zadanego celu, do którego NPC ma dążyć.</param>
    ///<param name="z">Pozycja na osi Z zadanego celu, do którego NPC ma dążyć.</param>
    private void ObsłużNavMeshAgent(float x, float z)
    {
        //https://www.binpress.com/unity-3d-ai-navmesh-navigation/
        //Logika nav mesha
        if (bufferAnima[0] == false && bufferAnima[2] == false)  //Żyję i nie jestem jeszcze w zasięgu ataku
        {
            if (this.agent.velocity == Vector3.zero && !this.agent.isStopped)
            {
                GenerujŚcieżke(x, z);
                return;
            }
        }
        if (agent.enabled && !agent.hasPath /*|| this.ostatniTargetPozycja != docelowaPozycja*/)
        {
            if (głównyIndex == -1)
            {
                GenerujŚcieżke(x, z);
            }
            else if (!czekamNaZatwierdzenieŚcieżki)
            {
                czekamNaZatwierdzenieŚcieżki = true;
                StartCoroutine(WyliczŚciezkę(UnityEngine.Random.Range(0f, 0.5f), x, z));
            }
        }
    }
    ///<summary>Dezaktywacja obiektu po 4sec.</summary>
    private IEnumerator WyłObjTimer()
    {
        yield return new WaitForSeconds(4.0f);
        WłWyłObj(false);
    }
    ///<summary>Metoda generuje trasę dla wroga po określonym czasie.</summary>
    ///<param name="f">Czas w sec, po których ma zostać wygenerowana nowa ścieżka dla NPC.</param>
    ///<param name="x">Pozycja na osi X zadanego celu, do którego NPC ma dążyć.</param>
    ///<param name="z">Pozycja na osi Z zadanego celu, do którego NPC ma dążyć.</param>
    private IEnumerator WyliczŚciezkę(float f, float x, float z)
    {
        yield return new WaitForSeconds(f);
        GenerujŚcieżke(x, z);
    }
    ///<summary>Metoda generuje trasę dla wroga.</summary>
    ///<param name="x">Pozycja na osi X zadanego celu, do którego NPC ma dążyć.</param>
    ///<param name="z">Pozycja na osi Z zadanego celu, do którego NPC ma dążyć.</param>
    private void GenerujŚcieżke(float x, float z)
    {
        if (ścieżka == null)
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
    ///<summary>Resetuje ścieżkę agenta navMesh jednostki.</summary>
    public void ResetujŚcieżki()
    {
        this.agent.ResetPath();
    }
    ///<summary>Metoda aktywuje lub dezaktywuje NPC.</summary>
    ///<param name="enab">Czy obiekt ma zostać aktywowany?</param>
    public void WłWyłObj(bool enab = false)
    {
        this.mainRenderer.enabled = enab;
        if (enab)
        {
            agent.enabled = enab;
            //this.GetComponent<NavMeshObstacle>().enabled = true;
            //this.GetComponent<SphereCollider>().enabled = true;
            this.agent.isStopped = !enab;
            anima.Rebind();
            ObsluzAnimacje(ref anima, "isDeath", !enab);
            sprite.localScale = new Vector3(1, 1, 1);
            short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position.x, this.transform.position.z);
            actXIdx = t[0];
            actZIdx = t[1];
            this.RysujHPBar();
            if (this.odgłosyNPC != null)
            {
                PomocniczeFunkcje.muzyka.ustawGłośność += this.UstawGłośnośćNPC;
            }
            SetGłównyIndexDiffValue();
            if (this._obiektAtaku != null)
            {
                this._obiektAtaku.BackWeapon();
                this._obiektAtaku.ActivateObj(posRęki.position.x, posRęki.position.z, true);
                this._obiektAtaku.UpdateSrartPos(posRęki.position.x, posRęki.position.y, posRęki.position.z);
            }
        }
        if (!enab)
        {
            sprite.parent.gameObject.SetActive(enab);
            ObsluzAnimacje(ref anima, "inRange", false);
            ObsluzAnimacje(ref anima, "haveTarget", false);
            if (this.odgłosyNPC != null)
            {
                PomocniczeFunkcje.muzyka.ustawGłośność -= this.UstawGłośnośćNPC;
            }
            this.transform.position = new Vector3(0, -20, 0);
            agent.enabled = enab;
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
    ///<summary>Dodaję komponent NavMeshAgent do NPC.</summary>
    private void DodajNavMeshAgent()
    {
        agent = this.gameObject.AddComponent<NavMeshAgent>();
        agent.stoppingDistance = (zasięgAtaku == 0) ? 1f : zasięgAtaku;
        agent.speed = prędkość;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.avoidancePriority = 99;
        agent.autoTraverseOffMeshLink = false;
        this.agent.radius = 0.15f;
        this.agent.height = 0.3f;
    }
    public override void Atakuj()
    {
        AtakujCel();
    }
    ///<summary>Obsłuż atak NPC.</summary>
    private void AtakujCel()
    {
        if (aktualnyReuseAtaku < szybkośćAtaku)
        {
            if(czyAtakJestAktywny)
            {
                aktualnyReuseAtaku += (this.typNPC == TypNPC.WalczyNaDystans) ? Time.deltaTime : Time.deltaTime*5.0f;
            }
            else
            {
                if(aktualnyReuseAtaku == 0 && this.typNPC == TypNPC.WalczyNaDystans)
                {
                    this.anima.Play("Atak1_Ruch_Rzut", -1, 0f);
                }
                aktualnyReuseAtaku += Time.deltaTime*5.0f;
            }
            float f = szybkośćAtaku - aktualnyReuseAtaku;
            if (f <= .25f)   //Jeśli strzela to się zaczyna
            {
                if (_obiektAtaku == null) return;
                if (!czyAtakJestAktywny)
                {
                    czyAtakJestAktywny = true;
                    if (typNPC == TypNPC.WalczyNaDystans)
                    {
                        _obiektAtaku.UpdateSrartPos(posRęki.position.x, posRęki.position.y, posRęki.position.z);
                        _obiektAtaku.ActivateObj(cel.transform.position.x, cel.transform.position.z, false);
                        _obiektAtaku.PrzełączSkalęLokalZ();
                    }
                    if (SprawdźCzyWidocznaPozycja())
                    {
                        if (efektyFxStart != null)
                        {
                            efektyFxStart.transform.position = this.transform.position;
                            efektyFxStart.Play();
                        }
                        PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, (this.typNPC == TypNPC.WalczyNaDystans || this.typNPC == TypNPC.WalczynaDystansIWZwarciu) ?
                            PomocniczeFunkcje.TagZEpoka("AtakNPCDystans", this.epokaNPC, this.tagRodzajDoDźwięków) :
                            PomocniczeFunkcje.TagZEpoka("AtakNPCZwarcie", this.epokaNPC, this.tagRodzajDoDźwięków), true);
                    }
                }
                else
                {
                    if (f < 0)
                    {
                        f = 0;
                        if (SprawdźCzyWidocznaPozycja())
                        {
                            if (efektyFxKoniec != null)
                            {
                                efektyFxKoniec.transform.position = cel.transform.position;
                                efektyFxKoniec.Play();
                            }
                            PomocniczeFunkcje.muzyka.WłączWyłączClip(ref this.odgłosyNPC, true, (this.typNPC == TypNPC.WalczyNaDystans || this.typNPC == TypNPC.WalczynaDystansIWZwarciu) ?
                            PomocniczeFunkcje.TagZEpoka("TrafienieNPC", this.epokaNPC, this.tagRodzajDoDźwięków) :
                            PomocniczeFunkcje.TagZEpoka("TrafienieNPC", this.epokaNPC, this.tagRodzajDoDźwięków), true);
                        }
                    }
                    else if (typNPC == TypNPC.WalczyNaDystans && SprawdźCzyWidocznaPozycja())
                    {
                        
                        _obiektAtaku.SetActPos(f * 4.5f, (typNPC == TypNPC.WalczyNaDystans) ? true : false);
                        //_obiektAtaku.transform.position = Vector3.Lerp(cel.transform.position, posRęki.position, f);
                    }
                }
            }
            return;
        }
        //Następuje zadawanie obrażeń
        aktualnyReuseAtaku = 0.0f;
        czyAtakJestAktywny = false;
        if (cel != null)
        {
            this.transform.LookAt(cel.transform.position);
            cel.ZmianaHP((short)Mathf.FloorToInt((zadawaneObrażenia * this.modyfikatorZadawanychObrażeń)));
        }
        if (this.typNPC != TypNPC.WalczyNaDystans)
        {
            if (cel == null)
                return;
            this.ZmianaHP(cel.ZwrócOdbiteObrażenia());
        }
        else
        {
            _obiektAtaku.BackWeapon();
            _obiektAtaku.PrzełączSkalęLokalZ();
            czySynchronizuje = true;
            StartCoroutine(Synchronizuj());
        }
    }
    private IEnumerator Synchronizuj()
    {
        yield return new WaitUntil(() => this.anima.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1);
        this.czySynchronizuje = false;
    }
    ///<summary>Funkcja zwraca najbliższy obiekt (Konkretny NPC Statyczny) postawiony przez gracza względem NPC.</summary>
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
    ///<summary>Metoda dodaje NPC do struktury, która przechowuje informację o zasięgu danych wież i jakie jednostki się znajdują w ich zasięgu.</summary>
    ///<param name="x">Pierwszy indeks tablicy przekształcony na element PomocniczeFunkce.tablicaWież.</param>
    ///<param name="z">Drugi indeks tablicy przekształcony na element PomocniczeFunkce.tablicaWież.</param>
    ///<param name="pierwszyRaz">Czy następuje pierwsze dodanie (Postawienie budynku przez gracza).</param>
    ///<param name="taWiezaPierwszyRaz">Referencja do nowo postawionej wieży (jeśli pierwszyRaz = true).</param>
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
    ///<summary>Metoda kasuje NPC ze struktury, która przechowuje informację o zasięgu danych wież i jakie jednostki się znajdują w ich zasięgu.</summary>
    ///<param name="czyCalkowicie">Czy metoda ma usunąć tę jednostkę całkowicie z pamięci wież (np w przypadku śmierci).</param>
    ///<param name="sQ">Tablica okreslająca stronę przesunięcia się wroga w tablicy wież względem ostatniej pozycji: (-X = 0), (+X = 1), (-Z = 2), (+Z = 3).</param>
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
    ///<summary>Metoda ustawia losową wartość zmiennej głównyIndex między -5 a -1 dla optymalizacji.</summary>
    public void SetGłównyIndexDiffValue()
    {
        głównyIndex = (sbyte)Random.Range(-5, -1);
    }
    ///<summary>Ustawia wartości dla NPC po ponownym odpaleniu.</summary>
    public void UstawWartościPoPoolowaniu()
    {
        AktualneŻycie = maksymalneŻycie;
        this.rysujPasekŻycia = true;
        NieŻyję = false;
        SetGłównyIndexDiffValue();
        WłWyłObj(true);
    }
}
