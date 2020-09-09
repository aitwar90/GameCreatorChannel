using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Klasa obsługuje dynamiczne NPC
*/
public class KonkretnyNPCDynamiczny : NPCClass
{
    #region Zmienne publiczne
    #endregion

    #region Zmienny prywatne
    private bool rysujPasekŻycia = false;
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

    }

    // Update is called once per frame
    void Update()
    {
        if(aktualneŻycie <= 0)
        {
            UsuńJednostkę();
        }
    }
    protected override void RysujHPBar()
    {
        if(!rysujPasekŻycia && SpawnerHord.actualHPBars <= 10 && mainRenderer.isVisible)
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
        if(this.rysujPasekŻycia)
        {
            if(SpawnerHord.actualHPBars > 0)
                SpawnerHord.actualHPBars --;
            this.rysujPasekŻycia = false;
            ManagerGryScript.iloscAktywnychWrogów --;
            StartCoroutine(SkasujObject(5.0f));
        }
    }
}
