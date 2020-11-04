using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Button nastepnyPoziom;
    public Button powtorzPoziom;
    public PrzyciskiSkrzynekIReklam[] buttonSkrzynki;
    public Button wróćDoMenu;
    public Button rekZaWyzszaNagrode;
    public Button kup;
    public Button wróc;
    public Button użyjPrzedmiotu;
    public Button zmianaJezyka;
    public Button rotacjaBudynku;

    #region TextUI
    public Text ilośćCoinów;
    public Text ilośćFal;
    #endregion

    public Dropdown wybórPrzedmiotuZEkwipunku;
    public static bool czyMenuEnable = true;
    private GameObject menu;
    private GameObject uiGry;
    private GameObject optionsMenu;
    private Button przyciskWznów;
    private Vector3 lastPosCam = Vector3.zero;
    private sbyte wybranyPrzedmiot = -1;
    public sbyte lastIdxJezyka = 0;
    private static MainMenu singelton = null;

    public sbyte UstawLubPobierzOstatniIdexJezyka
    {
        get
        {
            return lastIdxJezyka;
        }
        set
        {
            lastIdxJezyka = (sbyte)(value - 1);
            ZmieńJęzyk();
        }
    }
    void Awake()
    {
        if(singelton == null)
        {
            singelton = this;
        }
        else
        {
            Destroy(this);
            return;
        }
        menu = this.transform.Find("Menu/MainMenu").gameObject;
        uiGry = this.transform.Find("UIGry").gameObject;
        optionsMenu = this.transform.Find("Menu/OptionsMenu").gameObject;
        przyciskWznów = this.transform.Find("Menu/MainMenu/ResumeButton").GetComponent<Button>();
        uiGry.SetActive(false);
        optionsMenu.SetActive(false);
        przyciskWznów.interactable = false;
        PomocniczeFunkcje.oCam = Camera.main;
        nastepnyPoziom.gameObject.SetActive(false);
        powtorzPoziom.gameObject.SetActive(false);
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        użyjPrzedmiotu.gameObject.SetActive(false);
        UstawPrzyciskObrotu(false);
        WłWylPrzyciskiKupna(false);
    }
    public void PlayGame()
    {
        if (SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene((byte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka, LoadSceneMode.Additive);
        }
        else
        {
            //Reset scene
            ResetSceny();
        }
        PomocniczeFunkcje.oCam.transform.position = MoveCameraScript.bazowePolozenieKameryGry;
        lastPosCam = MoveCameraScript.bazowePolozenieKameryGry;
        PrzełączUI(false);
    }
    public void ResetSceny()
    {
        PomocniczeFunkcje.ResetujWszystko();
        SceneManager.UnloadSceneAsync(1);
        PomocniczeFunkcje.managerGryScript.CzyScenaZostałaZaładowana = false;
        SceneManager.LoadSceneAsync((byte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka, LoadSceneMode.Additive);
    }
    public void OptionsMenu(bool actButton)
    {
        menu.SetActive(!actButton);
        optionsMenu.SetActive(actButton);
    }
    public void QuitGame()
    {
        PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    public void PrzełączUI(bool aktywujeMenu)
    {
        przyciskWznów.interactable = true;
        czyMenuEnable = aktywujeMenu;
        menu.SetActive(aktywujeMenu);
        uiGry.SetActive(!aktywujeMenu);
        if (aktywujeMenu)
        {
            PomocniczeFunkcje.UstawTimeScale(0);
            lastPosCam = PomocniczeFunkcje.oCam.transform.position;
            PomocniczeFunkcje.oCam.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        }
        else
        {
            PomocniczeFunkcje.UstawTimeScale(1);
            PomocniczeFunkcje.oCam.transform.position = lastPosCam;
        }
    }
    public void SkrzynkaKlik(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KliknietyPrzycisk((byte)idx);
        buttonSkrzynki[idx].skrzynkaB.interactable = false;
    }
    public void KliknietyPrzyciskRewardZPoziomuZReklama()
    {
        rekZaWyzszaNagrode.gameObject.SetActive(false);
        PomocniczeFunkcje.managerGryScript.KliknietyButtonZwiekszeniaNagrodyPoLvlu();
    }
    public void PrzyciskiKupna(bool kupujący)
    {
        WłWylPrzyciskiKupna(false);
        PomocniczeFunkcje.spawnBudynki.OdblokujBudynek(kupujący);
    }
    public void WłWylPrzyciskiKupna(bool f)
    {
        PomocniczeFunkcje.mainMenu.kup.gameObject.SetActive(f);
        PomocniczeFunkcje.mainMenu.wróc.gameObject.SetActive(f);
    }
    public void WybierzWybranyPrzedmiot()
    {
        if (this.wybórPrzedmiotuZEkwipunku.value > 0)
        {
            wybranyPrzedmiot = (sbyte)(this.wybórPrzedmiotuZEkwipunku.value - 1);   //Konwert na tablice przedmiotów
            użyjPrzedmiotu.gameObject.SetActive(true);
        }
    }
    public void UstawDropDownEkwipunku(ref EkwipunekScript es)
    {
        List<string> listaOpcji = new List<string>();
            listaOpcji.Add("NONE");
        if (es != null && es.przedmioty != null && es.przedmioty.Length > 0)
        {
            for (byte i = 0; i < es.przedmioty.Length; i++)
            {
                listaOpcji.Add(es.przedmioty[i].nazwaPrzedmiotu + " " + es.przedmioty[i].ilośćDanejNagrody.ToString());
            }
        }
        this.wybórPrzedmiotuZEkwipunku.ClearOptions();
        this.wybórPrzedmiotuZEkwipunku.AddOptions(listaOpcji);
    }
    public void UżyjKlikniętegoPrzedmiotu()
    {
        if (wybranyPrzedmiot > -1)
        {
            użyjPrzedmiotu.gameObject.SetActive(false);
            PomocniczeFunkcje.managerGryScript.UzyciePrzedmiotu((byte)wybranyPrzedmiot);
        }
        wybranyPrzedmiot = -1;
        this.wybórPrzedmiotuZEkwipunku.value = 0;

    }
    public void AktualizujInfoOIlosci(byte idx, string orginalnaNazwa, ushort actIlosc)
    {
        idx++;
        if (idx < this.wybórPrzedmiotuZEkwipunku.options.Count)
        {
            this.wybórPrzedmiotuZEkwipunku.options[idx].text = orginalnaNazwa + " " + actIlosc.ToString();
        }
    }
    public bool SprawdźCzyNazwaPasujeItemDropDown(string szukanaNazwa)  //Wyszukuje nazwę w opcjach dropdawna
    {
        for (byte i = 0; i < this.wybórPrzedmiotuZEkwipunku.options.Count; i++)
        {
            string[] s = this.wybórPrzedmiotuZEkwipunku.options[i].text.Split(' ');
            if (s[0] == szukanaNazwa)
            {
                return true;
            }
        }
        return false;
    }
    public void KliknąłemReklame(int idx)
    {
        PomocniczeFunkcje.managerGryScript.KlikniętaReklamaButtonSkrzynki((byte)idx);
    }
    public void ZmieńJęzyk()
    {
        lastIdxJezyka++;
        if(lastIdxJezyka < 0 || lastIdxJezyka > 1)  //Tu należy zmienić liczbę jesli dodany zostanie nowy jezyk
            lastIdxJezyka = 0;
        PomocniczeFunkcje.managerGryScript.ZmianaJęzyka((byte)lastIdxJezyka);
    }
    public void ObrótBudynku()
    {
        PomocniczeFunkcje.spawnBudynki.ObróćBudynek();
    }
    public void UstawPrzyciskObrotu(bool wartośćPrzycisku)
    {
        rotacjaBudynku.gameObject.SetActive(wartośćPrzycisku);
    }
    public void UstawTextUI(string nazwaTekstu, string tekst)
    {
        if(nazwaTekstu == "ilośćCoinów")
        {
            ilośćCoinów.text = tekst;
        }
        else if(nazwaTekstu == "ilośćFal")
        {
            ilośćFal.text = tekst;
        }
    }
}
