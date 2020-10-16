using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PrzedmiotScript : MonoBehaviour
{
    public string nazwaPrzedmiotu;
    public Image obrazek;
    public TypPrzedmiotu typPrzedmiotu;
    public ushort minParam;
    public ushort maxParam;
    public byte liczbaItemówOtrzymywanych = 1;
    public byte ilośćDanejNagrody = 0;
    public void AktywujPrzedmiot()
    {
        if (ilośćDanejNagrody > 0)
        {
            switch (typPrzedmiotu)
            {
                case TypPrzedmiotu.Coiny:
                    ManagerGryScript.iloscCoinów += (ushort)Random.Range(minParam, maxParam);
                    break;
                case TypPrzedmiotu.CudOcalenia:
                    PomocniczeFunkcje.managerGryScript.CudOcalenia();
                    break;
                case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:
                    for (byte i = 0; i < 4; i++)
                    {
                        Skrzynka s = PomocniczeFunkcje.managerGryScript.ZwróćSkrzynkeOIndeksie(i);
                        if (s.ReuseTimer)
                        {
                            s.OdejmnijCzas(Random.Range(minParam, maxParam));
                            break;
                        }
                    }
                    break;
                case TypPrzedmiotu.DodatkowaNagroda:

                    break;
            }
            ilośćDanejNagrody--;
        }
        else
        {
            Debug.Log("Brak przedmiotu");
        }
    }
}

