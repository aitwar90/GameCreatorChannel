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

    #endregion
    #region Zmienny prywatne
    private bool rysujPasekŻycia = false;
    private NavMeshAgent agent = null;
    private NavMeshPath ścieżka = null;
    private sbyte głównyIndex = -1;
    private short actXIdx = 32767;
    private short actZIdx = 32767;
    private bool czyDodawac = false;
    private byte[] ostatnieStrony = null;
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
    public bool GetIsOnnavmesh
    {
        get
        {
            return agent.isOnNavMesh;
        }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            DodajNavMeshAgent();
        }
        this.AktualneŻycie = this.maksymalneŻycie;
        short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
        this.DodajMnieDoListyWrogowWiezy(t[0], t[1], true);
        actXIdx = t[0];
        actZIdx = t[1];
    }

    // Update is called once per frame
    protected override void UpdateMe()
    {
        switch (głównyIndex)
        {
            case -1:
                if (cel == null)
                {
                    PomocniczeFunkcje.ZwykłeAI(this);
                }
                ObsłużNavMeshAgent(cel.transform.position);
                głównyIndex++;
                break;
            case 0:
                PomocniczeFunkcje.ZwykłeAI(this);
                głównyIndex++;
                break;
            case 1:
                if (cel != null)
                {
                    ObsłużNavMeshAgent(cel.transform.position);
                }
                głównyIndex++;
                break;
            case 2: //Ustaw index tablicy dla npc i usuń stare wieże
                short[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
                List<byte> sQ = new List<byte>();
                bool c = false;
                if (actXIdx < t[0])
                {
                    sQ.Add(0);
                    c = true;
                }
                else if (actXIdx > t[0])
                {
                    sQ.Add(1);
                    c = true;
                }
                if (actZIdx < t[1])
                {
                    sQ.Add(2);
                    c = true;
                }
                else if (actZIdx > t[1])
                {
                    sQ.Add(3);
                    c = true;
                }
                if (c)
                {
                    //Usunięcie starych wież
                    ostatnieStrony = sQ.ToArray();
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
                głównyIndex = 0;
                break;
            default:
                głównyIndex++;
                break;
        }
    }
    protected override void RysujHPBar()
    {
        if (!rysujPasekŻycia && SpawnerHord.actualHPBars <= 10 && mainRenderer.isVisible)
        {
            rysujPasekŻycia = true;
        }
        if (rysujPasekŻycia)
        {
            Vector3 tempPos = this.transform.position;
            tempPos.y += 1.6f;
            Vector2 pozycjaPostaci = Camera.main.WorldToScreenPoint(tempPos);
            GUI.Box(new Rect(pozycjaPostaci.x - 30, Screen.height - pozycjaPostaci.y - 30, 60, 20), this.AktualneŻycie + " / " + maksymalneŻycie);
            SpawnerHord.actualHPBars++;
        }
    }
    protected override void UsuńJednostkę()
    {
        this.AktualneŻycie = -1;
        UsuńMnieZTablicyWież(true);
        if (this.rysujPasekŻycia)
        {
            if (SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars--;
        }
        this.rysujPasekŻycia = false;
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚciezkę;
        ManagerGryScript.iloscAktywnychWrogów--;
        WłWyłObj(false);
        if (this.nastawienieNPC == NastawienieNPC.Wrogie)
        {
            PomocniczeFunkcje.DodajDoStosuTrupów(this);
        }
        else
        {
            StartCoroutine(SkasujObject(3.0f));
        }
    }
    private void ObsłużNavMeshAgent(Vector3 docelowaPozycja)
    {
        //https://www.binpress.com/unity-3d-ai-navmesh-navigation/
        //Logika nav mesha
        if (!agent.hasPath /*|| this.ostatniTargetPozycja != docelowaPozycja*/)
        {
            /*
            if (agent.hasPath)
            {
                agent.ResetPath();
                //Debug.Log("Resetuję ścieżkę");
            }
            */
            //Debug.Log("Has Path = " + agent.hasPath + " ostatni target pozycja " + ostatniTargetPozycja + " agent.destination = " + agent.destination);

            StartCoroutine(WyliczŚciezkę(UnityEngine.Random.Range(0f, 0.5f), docelowaPozycja));
        }
    }
    private IEnumerator WyliczŚciezkę(float f, Vector3 docelowaPozycja)
    {
        yield return new WaitForSeconds(f);
        ścieżka = new NavMeshPath();
        bool czyOdnalzazłemŚcieżkę = agent.CalculatePath(docelowaPozycja, ścieżka);
        if (ścieżka.status == NavMeshPathStatus.PathComplete)
        {
            agent.SetPath(ścieżka);
            //if (ostatniTargetPozycja != docelowaPozycja)
            //    ostatniTargetPozycja = docelowaPozycja;
        }
        else
        {
            cel = WyszukajNajbliższyObiekt() as KonkretnyNPCStatyczny;
            ObsłużNavMeshAgent(cel.transform.position);
        }
    }
    public void ResetujŚcieżki()
    {
        this.agent.ResetPath();
    }
    public void WłWyłObj(bool enab = false)
    {
        if (enab)
            this.gameObject.SetActive(true);
        agent.enabled = enab;
        if (!enab)
            this.gameObject.SetActive(false);
    }
    public override void ResetujŚciezkę(KonkretnyNPCStatyczny taWiezaPierwszyRaz = null)
    {
        if (taWiezaPierwszyRaz != null)
            DodajMnieDoListyWrogowWiezy(actXIdx, actZIdx, false, taWiezaPierwszyRaz);
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
            aktualnyReuseAtaku += Time.deltaTime * 4f;
            return;
        }
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
        else
        {
            for (byte sq = 0; sq < ostatnieStrony.Length; sq++)
            {
                /*
                if (ostatnieStrony[sq] == 0)
                    ostatnieStrony[sq] = 1;
                else if (ostatnieStrony[sq] == 1)
                    ostatnieStrony[sq] = 0;
                else if (ostatnieStrony[sq] == 3)
                    ostatnieStrony[sq] = 2;
                else if (ostatnieStrony[sq] == 2)
                    ostatnieStrony[sq] = 3;
                */
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
                        if(sQ[sq] == 0)
                            sQ[sq] = 1;
                        else if(sQ[sq] == 1)
                            sQ[sq] = 0;
                        else if(sQ[sq] == 2)
                            sQ[sq] = 3;
                        else if(sQ[sq] == 3)
                            sQ[sq] = 2;
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
