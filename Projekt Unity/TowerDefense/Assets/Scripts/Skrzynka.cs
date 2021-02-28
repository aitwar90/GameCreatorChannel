using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Skrzynka
{
    [SerializeField] public DateTime pozostałyCzas;
    public Button button;
    public Button buttonReklamy;
    public bool reuseTime = false;
    private Text czasReusu;
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
        this.czasReusu = this.buttonReklamy.transform.Find("TextTimer").GetComponent<Text>();
    }
    public void SprawdźCzyReuseMinęło()
    {
        if (reuseTime)
        {
            if (MainMenu.singelton.CzyOdpalonyPanelReklam)
            {
                if (pozostałyCzas.CompareTo(DateTime.Now) < 0)   //Reuse minęło
                {
                    reuseTime = false;
                    button.interactable = true;
                    buttonReklamy.interactable = false;
                    this.czasReusu.text = "";
                }
                else
                {
                    if (!buttonReklamy.interactable)
                    {
                        buttonReklamy.interactable = PomocniczeFunkcje.managerGryScript.CzyReklamaZaładowana;
                    }
                    this.czasReusu.text = OkreślCzasDoTekstu();
                }
            }
        }
    }
    public void OdejmnijCzas(float offTime = -30)
    {
        if (reuseTime)
        {
            pozostałyCzas = pozostałyCzas.AddMinutes(offTime);
            //Debug.Log("Dodałem minut = " + offTime + " i pozostały czas to " + pozostałyCzas.Hour + " h / " + pozostałyCzas.Minute + " m.");
        }
    }
    public void RozpocznijOdliczanie()
    {
        pozostałyCzas = DateTime.Now;
        pozostałyCzas = pozostałyCzas.AddHours(2);
        reuseTime = true;
    }
    private string OkreślCzasDoTekstu()
    {
        TimeSpan ts = pozostałyCzas.Subtract(DateTime.Now);
        byte minuty = (byte)ts.TotalMinutes;
        byte hour = (byte)(minuty/60f);
        minuty -= (byte)(hour*60);
        return hour.ToString("00")+":"+minuty.ToString("00");
    }
}
