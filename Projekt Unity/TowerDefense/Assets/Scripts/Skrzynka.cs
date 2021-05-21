using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[System.Serializable]
public class Skrzynka : MonoBehaviour
{
    [SerializeField] public DateTime pozostałyCzas;
    public Button button;
    public Button buttonReklamy;
    public bool reuseTime = false;
    public Text czasReusu;
    public bool ReuseTimer
    {
        get
        {
            return reuseTime;
        }
        set
        {
            reuseTime = value;
            if(value)
            {
                if(!PomocniczeFunkcje.managerGryScript.sąReklamyLubSkrzynki) PomocniczeFunkcje.managerGryScript.sąReklamyLubSkrzynki = true;
            }
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
            if (pozostałyCzas.CompareTo(DateTime.Now) < 0)   //Reuse minęło
            {
                reuseTime = false;
                button.interactable = true;
                buttonReklamy.interactable = false;
                this.czasReusu.text = "";
                if (!MainMenu.singelton.odpalReklamy.interactable)
                {
                    Debug.Log("Odpalam button reklam w grze");
                    MainMenu.singelton.odpalReklamy.interactable = true;
                }
            }
            else
            {
                if (!buttonReklamy.interactable)
                {
                    bool czyZał = PomocniczeFunkcje.managerGryScript.CzyReklamaZaładowana;
                    buttonReklamy.interactable = czyZał;
                    if (czyZał)
                    {
                        if (!MainMenu.singelton.odpalReklamy.interactable)
                        {
                            Debug.Log("Odpalam button reklam w grze");
                            MainMenu.singelton.odpalReklamy.interactable = true;
                        }
                    }
                    else
                    {
                        Debug.Log("Czekam na załadowanie reklamy");
                        StartCoroutine(CzekajAz());
                    }
                }
                this.czasReusu.text = OkreślCzasDoTekstu();
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
        if(!PomocniczeFunkcje.managerGryScript.sąReklamyLubSkrzynki) PomocniczeFunkcje.managerGryScript.sąReklamyLubSkrzynki = true;
    }
    private string OkreślCzasDoTekstu()
    {
        TimeSpan ts = pozostałyCzas.Subtract(DateTime.Now);
        byte minuty = (byte)ts.TotalMinutes;
        byte hour = (byte)(minuty / 60f);
        minuty -= (byte)(hour * 60);
        return hour.ToString("00") + ":" + minuty.ToString("00");
    }
    private IEnumerator CzekajAz()
    {
        yield return new WaitUntil(() => PomocniczeFunkcje.managerGryScript.CzyReklamaZaładowana);
        Debug.Log("Reklama załadowana, odpalam button reklamy z poziomu gry");
        if (!MainMenu.singelton.odpalReklamy.interactable)
        {
            MainMenu.singelton.odpalReklamy.interactable = true;
        }
    }
}
