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
                    ManagerGryScript.iloscCoinów += (ushort)Random.Range(minParam, maxParam + 1);
                    PomocniczeFunkcje.mainMenu.UstawTextUI("ilośćCoinów", ManagerGryScript.iloscCoinów.ToString());
                    ilośćDanejNagrody--;
                    break;
                case TypPrzedmiotu.CudOcalenia:
                    if (PomocniczeFunkcje.celWrogów.AktualneŻycie > 0)
                    {
                        PomocniczeFunkcje.managerGryScript.CudOcalenia();
                        ilośćDanejNagrody--;
                    }
                    break;
                case TypPrzedmiotu.DodatkowaNagroda:
                    ilośćDanejNagrody--;
                    DodajNagrode(true);
                    break;
                case TypPrzedmiotu.SkrócenieCzasuDoSkrzynki:
                    for (byte i = 0; i < 4; i++)
                    {
                        Skrzynka s = PomocniczeFunkcje.managerGryScript.ZwróćSkrzynkeOIndeksie(i);
                        if (s.ReuseTimer)
                        {
                            s.OdejmnijCzas(Random.Range(minParam, maxParam + 1));
                            ilośćDanejNagrody--;
                            break;
                        }
                    }
                    break;
            }
        }
        else
        {
            Debug.Log("Brak przedmiotu");
        }
    }
    public byte DodajNagrode(bool czyDodatkowa = false)
    {
        byte mP = (byte)(System.Enum.GetValues(typeof(TypPrzedmiotu)).Length);
        byte losowany = (byte)Random.Range(0, mP);
        PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].ilośćDanejNagrody += (czyDodatkowa) ? (byte)(PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].liczbaItemówOtrzymywanych*2) : PomocniczeFunkcje.managerGryScript.ekwipunekGracza[losowany].liczbaItemówOtrzymywanych;
        return losowany;
    }
}

