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
    public Transform cel;
    #endregion

    #region Zmienny prywatne
    private bool rysujPasekŻycia = false;
    private NavMeshAgent agent = null;
    private NavMeshPath ścieżka = null;
    private NPCClass aktualnyCelAtaku;  //Indeks aktualnej akcji navmesha (0 - idź, 1 - stój itp)
    private byte akcjaNavMesh = 0;
    private byte głównyIndex = 0;
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
        if (this.typNPC != TypNPC.WalczyNaDystans)
        {
            SphereCollider sc = this.gameObject.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = 2.5f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (aktualneŻycie <= 0)
        {
            UsuńJednostkę();
            return;
        }
        switch (głównyIndex)
        {
            case 0:

                ObsłużNavMeshAgent(cel.position);
                głównyIndex++;
                break;
            case 1:
                if (kolejkaAtaku != null && akcjaNavMesh == 1)
                {
                    AtakujCel();
                }
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
            this.rysujPasekŻycia = false;
            ManagerGryScript.iloscAktywnychWrogów--;
            StartCoroutine(SkasujObject(3.0f));
        }
    }
    private void ObsłużNavMeshAgent(Vector3 docelowaPozycja)
    {
        //https://www.binpress.com/unity-3d-ai-navmesh-navigation/
        //Logika nav mesha
        if (cel == null)
        {
            cel = WyszukajNajbliższyObiekt().transform;
            if (cel == null)
                return;
        }
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
                cel = WyszukajNajbliższyObiekt().transform;
            }
        }
        else
        {
            if (Vector3.Distance(agent.destination, this.transform.position) <= agent.stoppingDistance)
            {
                akcjaNavMesh = 1;
            }
        }
        //Akcje navMeshAgenta
        switch (akcjaNavMesh)
        {
            case 0: //Nav Mesh idzie
                if (agent.isStopped)
                    agent.isStopped = false;
                break;
            case 1:
                if (!agent.isStopped)
                    agent.isStopped = true;
                break;
        }
    }
    private IEnumerator WyliczŚciezkęPonownie(float f)
    {
        yield return new WaitForSeconds(f);
        agent.ResetPath();
        ObsłużNavMeshAgent(cel.position);
    }
    private void DodajNavMeshAgent()
    {
        agent = this.gameObject.AddComponent<NavMeshAgent>();
        agent.stoppingDistance = (zasięgAtaku == 0) ? 1.5f : zasięgAtaku;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Budynek" || other.tag == "NPC")
        {
            NPCClass knpcs = other.GetComponent<NPCClass>();
            if (knpcs.NastawienieNonPlayerCharacter != this.nastawienieNPC)
            {
                DodajNPCDoKolejkiAtaku(ref knpcs);
                //Atakuj wroga
                akcjaNavMesh = 1;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Budynek" || other.tag == "NPC")
        {
            NPCClass knpcs = other.GetComponent<NPCClass>();
            if (knpcs.NastawienieNonPlayerCharacter != this.nastawienieNPC)
            {
                OdepnijOdKolejkiAtaku(ref knpcs);
                //Atakuj wroga
                akcjaNavMesh = 0;
            }
        }
    }
    private void AtakujCel()
    {
        if (aktualnyReuseAtaku < szybkośćAtaku)
        {
            aktualnyReuseAtaku += Time.deltaTime;
            return;
        }
        NPCClass npcKlas = null;
        while (true)
        {
            if (kolejkaAtaku.Count == 0)
            {
                break;
            }
            npcKlas = kolejkaAtaku.Peek();
            if (npcKlas.AktualneŻycie > 0)
            {
                break;
            }
            else
            {
                kolejkaAtaku.Dequeue();
            }
        }
        aktualnyReuseAtaku = 0.0f;
        if (npcKlas == null)
        {
            //Koniec ataku zmień na ruch
            kolejkaAtaku = null;
            akcjaNavMesh = 0;
        }
        else
        {
            npcKlas.ZmianaHP((short)Mathf.FloorToInt((zadawaneObrażenia * this.modyfikatorZadawanychObrażeń)));
            if (akcjaNavMesh == 1)
                this.ZmianaHP(npcKlas.ZwrócOdbiteObrażenia());
        }
    }
    public KonkretnyNPCStatyczny WyszukajNajbliższyObiekt()
    {
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
}
