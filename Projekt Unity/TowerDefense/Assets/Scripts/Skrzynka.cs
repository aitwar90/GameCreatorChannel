using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Skrzynka
{
    public Button button;
    /* UNITY_ANDROID
    [SerializeField] public DateTime pozostałyCzas;
    public Button buttonReklamy;
    public bool reuseTime = false;
    //public Text czasReusu;    UNITY_ANDROID
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
    */
    public Skrzynka()
    {

    }
    public Skrzynka(ref PrzyciskiSkrzynekIReklam s)
    {
        this.button = s.skrzynkaB;
        //this.buttonReklamy = s.reklamSkrzynkaB; //UNITY_ANDROID
        //reuseTime = false; UNITY_ANDROID
        this.button.interactable = false;
        //this.buttonReklamy.interactable = false; UNITY_ANDROID
        //this.czasReusu = this.buttonReklamy.transform.Find("TextTimer").GetComponent<Text>(); UNITY_ANDROID
    }
    /* UNITY_ANDROID
    public void SprawdźCzyReuseMinęło()
    {
        if (reuseTime)
        {

            if (pozostałyCzas.CompareTo(DateTime.Now) < 0)   //Reuse minęło
            {
                reuseTime = false;
                button.interactable = true;
                buttonReklamy.interactable = false;
                //this.czasReusu.text = ""; UNITY_ANDROID
            }
            else
            {
                if (!buttonReklamy.interactable)
                {
                    buttonReklamy.interactable = PomocniczeFunkcje.managerGryScript.CzyReklamaZaładowana;
                }
                //this.czasReusu.text = OkreślCzasDoTekstu();   UNITY_ANDROID
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
        byte hour = (byte)(minuty / 60f);
        minuty -= (byte)(hour * 60);
        return hour.ToString("00") + ":" + minuty.ToString("00");
    }
    */
}
