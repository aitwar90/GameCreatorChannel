using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Klasa obsługuje statyczne NPC
*/
public class KonkretnyNPCStatyczny : NPCClass
{
    #region Zmienne publiczne
    public TypBudynku typBudynku;
    [Range(0, 20)]
    public byte odbiteObrażenia = 1;
    [Header("Granice obiektu względem środka obiektu"), Tooltip("Granica obiektu po osi X")]
    public float granicaX = 0.5f;
    [Tooltip("Granica obiektu po osi Z")]
    public float granicaZ = 0.5f;
    [Tooltip("Poziom na którym budynek może zostać odblokowany")]
    public byte poziomBudynku = 1;
    [Tooltip("Koszt badania odblokowania budynku")]
    public ushort kosztBadania = 0;
    [Tooltip("Typ ataku wieży")]
    public TypAtakuWieży typAtakuWieży;
    [Tooltip("Czy budynek jest zablokowany (jeśli tak to znaczy że nie zostały spełnione wymagania, lub nie został wynaleziony")]
    [SerializeField] public bool zablokowany = true;
    #endregion

    #region Zmienny prywatne
    public List<NPCClass> wrogowieWZasiegu = null;
    private byte idxAct = 0;
    public Transform sprite = null;
    #endregion

    #region Zmienne chronione
    #endregion

    #region Getery i setery

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
        if(sprite == null)
            sprite = this.transform.Find("HpGreen");
        RysujHPBar();
    }
    // Update is called once per frame
    protected override void RysujHPBar()
    {
        if (mainRenderer.isVisible)
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
                else if(cel != null && (wrogowieWZasiegu == null || wrogowieWZasiegu.Count == 0))
                {
                    cel = null;
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
        if (this.nastawienieNPC == NastawienieNPC.Wrogie)
        {
            StartCoroutine(SkasujObject(5.0f));
        }
        else    //Jeśli nastawienie jest przyjazne
        {
            //Podmień obiekt na zgruzowany
            PomocniczeFunkcje.SkasujElementDrzewa(ref PomocniczeFunkcje.korzeńDrzewaPozycji, this);
            Collider[] tablicaKoliderow = this.GetComponents<Collider>();
            for (byte i = 0; i < tablicaKoliderow.Length; i++)
            {
                tablicaKoliderow[i].enabled = false;
            }
            UnityEngine.AI.NavMeshObstacle tNVO = this.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            short[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
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
    void OnDrawGizmos()
    {
        if(cel != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position, cel.transform.position);
        }
    }
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
        }
        else
        {
            this.aktualnyReuseAtaku = 0;
            switch (typAtakuWieży)
            {
                case TypAtakuWieży.jedenTarget: //Jeden Target
                    cel.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    //Debug.Log("Cel.AktualneŻycie "+cel.AktualneŻycie);
                    if (cel.AktualneŻycie <= 0)
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
                    break;
                case TypAtakuWieży.wszyscyWZasiegu: //Wszystkie cele
                    for (byte i = 0; i < wrogowieWZasiegu.Count; i++)
                    {
                        wrogowieWZasiegu[i].ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
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
}
