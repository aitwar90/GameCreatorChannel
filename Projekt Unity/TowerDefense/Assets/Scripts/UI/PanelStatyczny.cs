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
}
