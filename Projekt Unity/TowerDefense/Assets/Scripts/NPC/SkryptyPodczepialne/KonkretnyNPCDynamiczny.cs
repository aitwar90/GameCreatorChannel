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
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        if (agent == null)
        {
            DodajNavMeshAgent();
        }
        this.aktualneŻycie = this.maksymalneŻycie;
    }

    // Update is called once per frame
    void Update()
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
                głównyIndex = 0;
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
            GUI.Box(new Rect(pozycjaPostaci.x - 30, Screen.height - pozycjaPostaci.y - 30, 60, 20), aktualneŻycie + " / " + maksymalneŻycie);
            SpawnerHord.actualHPBars++;
        }
    }
    protected override void UsuńJednostkę()
    {
        if (this.rysujPasekŻycia)
        {
            if (SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars--;
        }
        this.rysujPasekŻycia = false;
        ManagerGryScript.iloscAktywnychWrogów--;
        StartCoroutine(SkasujObject(3.0f));
    }
    private void ObsłużNavMeshAgent(Vector3 docelowaPozycja)
    {
        //https://www.binpress.com/unity-3d-ai-navmesh-navigation/
        //Logika nav mesha
        if (!agent.hasPath)
        {
            ścieżka = new NavMeshPath();
            bool czyOdnalzazłemŚcieżkę = agent.CalculatePath(docelowaPozycja, ścieżka);
            if (ścieżka.status == NavMeshPathStatus.PathComplete)
            {
                agent.SetPath(ścieżka);
                StartCoroutine(WyliczŚciezkęPonownie(Random.Range(5.5f, 10.5f)));
            }
            else if (ścieżka.status == NavMeshPathStatus.PathPartial)
            {
                agent.SetPath(ścieżka);
                StartCoroutine(WyliczŚciezkęPonownie(Random.Range(5.5f, 10.5f)));
            }
            else if (ścieżka.status == NavMeshPathStatus.PathInvalid)
            {
                Debug.Log("Agent nie potrafi dojść do celu, ścieżka nie została znaleziona");
                //Tu należy odnaleźć najbliższy obiekt do niszczenie
                cel = WyszukajNajbliższyObiekt() as KonkretnyNPCStatyczny;
                StartCoroutine(WyliczŚciezkęPonownie(Random.Range(5.5f, 10.5f)));
            }
        }
    }
    private IEnumerator WyliczŚciezkęPonownie(float f)
    {
        yield return new WaitForSeconds(f);
        agent.ResetPath();
        ObsłużNavMeshAgent(cel.transform.position);
    }
    private void DodajNavMeshAgent()
    {
        agent = this.gameObject.AddComponent<NavMeshAgent>();
        agent.stoppingDistance = (zasięgAtaku == 0) ? 1.5f : zasięgAtaku;
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
        Debug.Log("Rozpoczynam wyszukiwanie najbliższego obiektu");
        Component knpcs = PomocniczeFunkcje.WyszukajWDrzewie(ref PomocniczeFunkcje.korzeńDrzewaPozycji, this.transform.position);
        if (knpcs == null)
        {
            Debug.Log("Nie odnaleziono najbliższego obiektu");
            return null;
        }
        else
        {
            Debug.Log("Znaleziony obiekt ma nazwę " + knpcs.transform.name);
            return (KonkretnyNPCStatyczny)knpcs;
        }
    }
    public override void Atakuj(bool wZwarciu)
    {
        AtakujCel(wZwarciu);
    }
    public override void WłWyłNavMeshAgent(bool value)
    {
        //this.agent.isStopped = value;
    }
}
