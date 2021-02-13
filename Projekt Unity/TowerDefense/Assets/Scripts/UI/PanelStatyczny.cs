using UnityEngine;
using UnityEngine.UI;

public class PanelStatyczny : KontenerKomponentów
{
    public Button naprawButton;
    public Text nazwaObiektu;
    public Text punktyZycia;
    public Text kosztNaprawy;
    public Text obrazenia;
    public Text opis;
    private KonkretnyNPCStatyczny budynek = null;
    public KonkretnyNPCStatyczny KNPCS
    {
        get
        {
            return budynek;
        }
        set
        {
            budynek = value;
        }
    }
    void Awake()
    {
        naprawButton.interactable = false;
    }
    public void NaprawBudynek()
    {
        if(budynek != null)
        {
            budynek.Napraw();
            naprawButton.interactable = false;
        }
    }
    public override void UstawDane(string[] s)
    {
        nazwaObiektu.text = s[0];
        punktyZycia.text = s[1];
        kosztNaprawy.text = s[2];
        obrazenia.text = s[3];
        opis.text = s[4];
    }
    ///<summary>Metoda dynamicznie aktualizuje parametry panelu</summary>
    ///<param name="coZmienic">Parametr przesyła informację o indeksach co należy zmienić: 0-Punkty życia, 1 - koszt</param>
    ///<param name="naCo">Jaka wartość ma zostać wstawiona w odpowiedni tekst</param>
    public override void UstawDaneDynamiczne(byte[] coZmienic, string[] naCo)
    {
        for(byte i = 0; i < coZmienic.Length; i++)
        {
            switch(coZmienic[i])
            {
                case 0: //Punkty życia
                punktyZycia.text = naCo[i];
                break;
                case 1: //Koszt naprawy
                kosztNaprawy.text = naCo[i];
                break;
            }
        }
    }
    ///<summary>Metoda dynamicznie aktualizuje parametry panelu</summary>
    ///<param name="coZmienic">Parametr przesyła informację o indeksach co należy zmienić: 0-Punkty życia.</param>
    ///<param name="naCo">Jaka wartość ma zostać wstawiona w odpowiedni tekst</param>
    public override void UstawDaneDynamiczne(byte coZmienic, string naCo)
    {
        switch (coZmienic)
        {
            case 0: //Punkty życia
                punktyZycia.text = naCo;
                break;
            case 1: //koszt naprawy
                kosztNaprawy.text = naCo;
                break;
        }
    }
}
