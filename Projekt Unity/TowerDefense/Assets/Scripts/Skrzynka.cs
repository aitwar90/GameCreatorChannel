using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Skrzynka
{
    [SerializeField]public DateTime pozostałyCzas;
    public Button button;
    public Button buttonReklamy;
    private bool reuseTime = false;
    public bool ReuseTimer
    {
        get
        {
            return reuseTime;
        }
        set
        {
            reuseTime = value;
        }
    }
    public Skrzynka()
    {

    }
    public Skrzynka(ref PrzyciskiSkrzynekIReklam s)
    {
        this.button = s.skrzynkaB;
        this.buttonReklamy = s.reklamSkrzynkaB;
        reuseTime = false;
        this.button.interactable = false;
        this.buttonReklamy.interactable = false;
    }
    public void SprawdźCzyReuseMinęło()
    {
        if(reuseTime)
        {
            if(pozostałyCzas.CompareTo(DateTime.Now) < 0)   //Reuse minęło
            {
                reuseTime = false;
                button.interactable = true;
                buttonReklamy.interactable = false;
            }
            else
            {
                buttonReklamy.interactable = PomocniczeFunkcje.managerGryScript.CzyReklamaZaładowana;
            }
        }
    }
    public void OdejmnijCzas(float offTime = -30)
    {
        if(reuseTime)
        {
            pozostałyCzas = pozostałyCzas.AddMinutes(offTime);
        }
    }
    public void RozpocznijOdliczanie()
    {
        pozostałyCzas = DateTime.Now;
        pozostałyCzas = pozostałyCzas.AddHours(2);
        reuseTime = true;
        if(button.interactable)
            button.interactable = false;
    }
}
