using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Klasa obsługuje statyczne NPC
*/
public class KonkretnyNPCStatyczny : NPCClass
{
    #region Zmienne publiczne
    public bool pozwalamNaBudowe = true;
    public TypBudynku typBudynku;
    [Range(0, 20)]
    public byte odbiteObrażenia = 1;
    #endregion

    #region Zmienny prywatne
    private byte indeksWejscia = 0;

    #endregion

    #region Zmienne chronione
    #endregion

    #region Getery i setery

    #endregion
    void Start()
    {
        InicjacjaBudynku();
    }
    private void InicjacjaBudynku()
    {
        this.gameObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
    }
    // Update is called once per frame
    protected override void RysujHPBar()
    {
        if (mainRenderer.isVisible)
        {
            Vector3 tempPos = this.transform.position;
            tempPos.y += 1.6f;
            Vector2 pozycjaPostaci = Camera.main.WorldToScreenPoint(tempPos);
            GUI.Box(new Rect(pozycjaPostaci.x - 40, Screen.height - pozycjaPostaci.y - 30, 80, 20), aktualneŻycie + " / " + maksymalneŻycie);
        }
    }
     protected override void UsuńJednostkę()
    {
        if(this.nastawienieNPC == NastawienieNPC.Wrogie)
        {
            StartCoroutine(SkasujObject(5.0f));
        }
        else    //Jeśli nastawienie jest przyjazne
        {
            //Podmień obiekt na zgruzowany
            PomocniczeFunkcje.SkasujElementDrzewa(ref PomocniczeFunkcje.korzeńDrzewaPozycji, this);
            Collider[] tablicaKoliderow = this.GetComponents<Collider>();
            for(byte i = 0; i < tablicaKoliderow.Length; i++)
            {
                tablicaKoliderow[i].enabled = false;
            }
            UnityEngine.AI.NavMeshObstacle tNVO = this.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            tNVO.enabled = false;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Budynek" || other.tag == "NPC")
        {
            indeksWejscia++;
            pozwalamNaBudowe = false;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Budynek" || other.tag == "NPC")
        {
            indeksWejscia--;
            if(indeksWejscia == 0)
                pozwalamNaBudowe = true;
        }
    }
    public override byte ZwrócOdbiteObrażenia()
    {
        return (byte)Mathf.CeilToInt(odbiteObrażenia * this.modyfikatorZadawanychObrażeń);
    }
}
