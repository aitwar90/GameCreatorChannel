using UnityEngine;
using UnityEngine.UI;
using System;

[System.Serializable]
public class Skrzynka
{
    [SerializeField]public DateTime pozostałyCzas;
    public Button button;
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
    public Skrzynka(Button _button)
    {
        this.button = _button;
        reuseTime = false;
        this.button.enabled = false;
    }
    public void UstawButtonSkrzynki(Button _button)
    {
        this.button = _button;
        reuseTime = false;
        this.button.enabled = false;
    }
    public void SprawdźCzyReuseMinęło()
    {
        if(reuseTime)
        {
            if(pozostałyCzas.CompareTo(DateTime.Now) < 0)   //Reuse minęło
            {
                reuseTime = false;
                button.enabled = true;
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
        if(button.enabled)
            button.enabled = false;
    }
}
