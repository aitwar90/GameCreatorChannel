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
    public bool wymusInicjacje = false;
    public TypAtakuWieży typAtakuWieży;
    #endregion

    #region Zmienny prywatne
    private List<NPCClass> wrogowieWZasiegu = null;
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
    }
    // Update is called once per frame
    protected override void RysujHPBar()
    {
        if (mainRenderer.isVisible)
        {
            Vector3 tempPos = this.transform.position;
            tempPos.y += 1.6f;
            Vector2 pozycjaPostaci = Camera.main.WorldToScreenPoint(tempPos);
            GUI.Box(new Rect(pozycjaPostaci.x - 40, Screen.height - pozycjaPostaci.y - 30, 80, 20), this.AktualneŻycie + " / " + maksymalneŻycie);
        }
    }
    protected override void UsuńJednostkę()
    {
        this.AktualneŻycie = -1;
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
            byte[] temp = PomocniczeFunkcje.ZwrócIndeksyWTablicy(this.transform.position);
            byte s = (byte)(this.zasięgAtaku / 5);
            if (s > 1) 
            {
                for (sbyte i = (sbyte)(temp[0] - s); i < (sbyte)(temp[0] + s); i++)
                {
                    if (i > -1 && i < 20)
                    {
                        for (sbyte j = (sbyte)(temp[1] - s); j < (sbyte)(temp[0] + s); j++)
                        {
                            if (j > -1 && j < 20)
                            {
                                UsuńMnieZListy((byte)i, (byte)j);
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
    private void UsuńMnieZListy(byte x, byte z)
    {
        List<InformacjeDlaPolWież> tInf = new List<InformacjeDlaPolWież>();
        byte dlListy = (byte)PomocniczeFunkcje.tablicaWież[x,z].Count;
        for(short i = 0; i < dlListy; i++)
        {
            if(PomocniczeFunkcje.tablicaWież[x,z][i].wieża == this)
            {
                continue;
            }
            else
            {
                tInf.Add(PomocniczeFunkcje.tablicaWież[x,z][i]);
            }
        }
        PomocniczeFunkcje.tablicaWież[x,z] = tInf;
    }
    public override void Atakuj(bool wZwarciu)
    {
        if (this.aktualnyReuseAtaku < szybkośćAtaku)
        {
            aktualnyReuseAtaku += Time.deltaTime;
        }
        else
        {
            this.aktualnyReuseAtaku = 0;
            switch ((byte)typAtakuWieży)
            {
                case 0: //Jeden Target
                    cel.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    if (cel.AktualneŻycie <= 0)
                    {
                        //Znajdź nowy target
                        wrogowieWZasiegu.Remove(cel);
                        ZnajdźNowyCel();
                    }
                    break;
                case 1: //Wybuch
                    Collider[] tabZasięgu = new Collider[4];
                    int iloscCol = Physics.OverlapSphereNonAlloc(cel.transform.position, 1.0f, tabZasięgu, (1 << 8), QueryTriggerInteraction.Collide);
                    for (byte i = 0; i < iloscCol; i++)
                    {
                        NPCClass klasa = tabZasięgu[i].GetComponent<NPCClass>();
                        klasa.ZmianaHP((short)(Mathf.CeilToInt(zadawaneObrażenia * modyfikatorZadawanychObrażeń)));
                    }
                    break;
                case 2: //Wszystkie cele
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
}
