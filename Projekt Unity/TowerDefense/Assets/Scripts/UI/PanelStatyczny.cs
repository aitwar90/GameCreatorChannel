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
}
