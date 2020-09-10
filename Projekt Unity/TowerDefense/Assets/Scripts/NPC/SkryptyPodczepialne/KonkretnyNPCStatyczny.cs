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
    #endregion

    #region Zmienny prywatne

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
            
            Collider[] tablicaKoliderow = this.GetComponents<Collider>();
            for(byte i = 0; i < tablicaKoliderow.Length; i++)
            {
                tablicaKoliderow[i].enabled = false;
            }
            UnityEngine.AI.NavMeshObstacle tNVO = this.GetComponent<UnityEngine.AI.NavMeshObstacle>();
            tNVO.enabled = false;
        }
    }
    public void AtakujeMnie(KonkretnyNPCDynamiczny knpcd)
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Budynek")
        {
            pozwalamNaBudowe = false;
        }
    }
    void OnTriggerExit(Collider other)
    {
        if(other.tag == "Budynek")
        {
            pozwalamNaBudowe = true;
        }
    }
}
