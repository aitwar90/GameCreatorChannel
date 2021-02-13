using UnityEngine;
using UnityEngine.UI;
public class PanelDynamiczny : KontenerKomponentów
{
    public Text nazwaObiektu;
    public Text punktyZycia;
    public Text obrazenia;

    public override void UstawDane(string[] s)
    {
        nazwaObiektu.text = s[0];
        punktyZycia.text = s[1];
        obrazenia.text = s[2];
    }
    ///<summary>Metoda dynamicznie aktualizuje parametry panelu</summary>
    ///<param name="coZmienic">Parametr przesyła informację o indeksach co należy zmienić: 0-Punkty życia.</param>
    ///<param name="naCo">Jaka wartość ma zostać wstawiona w odpowiedni tekst.</param>
    public override void UstawDaneDynamiczne(byte coZmienic, string naCo)
    {
        switch (coZmienic)
        {
            case 0: //Punkty życia
                punktyZycia.text = naCo;
                break;
        }
    }
}
