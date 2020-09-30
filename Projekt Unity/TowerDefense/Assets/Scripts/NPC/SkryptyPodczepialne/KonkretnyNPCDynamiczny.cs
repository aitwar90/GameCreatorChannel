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
    private byte actXIdx = 255;
    private byte actZIdx = 255;
    private bool czyDodawac = false;
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
        byte[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
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
            case 2: //Ustaw index tablicy dla npc
                byte[] t = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
                if (actXIdx != t[0] || actZIdx != t[1])
                {
                    //Usunięcie starych wież
                    UsuńMnieZTablicyWież();
                    czyDodawac = true;
                    //Dodanie do nowych wież
                    actXIdx = t[0];
                    actZIdx = t[1];
                }
                głównyIndex++;
                break;
            case 4:
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
        PomocniczeFunkcje.managerGryScript.wywołajResetŚcieżek -= ResetujŚcieżki;
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
    public override void ResetujŚciezkę()
    {
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
    private void DodajMnieDoListyWrogowWiezy(byte x, byte z, bool pierwszyRaz = false)
    {
        if (PomocniczeFunkcje.tablicaWież[x, z] == null)
        {
            PomocniczeFunkcje.tablicaWież[x, z] = new List<InformacjeDlaPolWież>();
        }
        for (byte i = 0; i < PomocniczeFunkcje.tablicaWież[x, z].Count; i++)
        {
            if (pierwszyRaz || (!pierwszyRaz && PomocniczeFunkcje.tablicaWież[x, z][i].odlOdGranicy == 1))
            {
                PomocniczeFunkcje.tablicaWież[x, z][i].wieża.DodajDoWrogów(this);
            }
        }
    }
    private void UsuńMnieZTablicyWież(bool czyCalkowicie = false)
    {
        if (PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx] == null || PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx].Count == 0)
            return;

        for (ushort i = 0; i < PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx].Count; i++)
        {
            if (czyCalkowicie)
            {
                //Usunięcie tego npc 
                PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].wieża.UsuńZWrogów(this);
            }
            else
            {
                if (PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].odlOdGranicy == 1)
                {
                    //Usunięcie tego npc 
                    PomocniczeFunkcje.tablicaWież[actXIdx, actZIdx][i].wieża.UsuńZWrogów(this);
                }
            }
        }
    }
}
