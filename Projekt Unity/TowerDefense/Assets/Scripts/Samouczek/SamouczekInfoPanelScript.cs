using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class SamouczekInfoPanelScript : MonoBehaviour
{
    public Text wyśwTekstNaPanelu;
    public Button przyciskDalejButtonu;
    private Image tłoEnabeld = null;
    private Scrollbar scrollbar = null;
    public void ZaladujTekstPanelu(ref string opis, Font f)
    {
        wyśwTekstNaPanelu.text = opis;
        if(f != null)
            wyśwTekstNaPanelu.font = f;
        OdpalPanel();
    }
    private void OdpalPanel()
    {
        if(!this.gameObject.activeInHierarchy)
        {
            //PomocniczeFunkcje.mainMenu.wróćDoMenu.interactable = false;
            PomocniczeFunkcje.mainMenu.OdpalKursor = true;
            PomocniczeFunkcje.UstawTimeScale(0);
            this.gameObject.SetActive(true);
            tłoEnabeld.enabled = true;
            PomocniczeFunkcje.eSystem.SetSelectedGameObject(przyciskDalejButtonu.gameObject);
            PomocniczeFunkcje.mainMenu.OdpalKursor = false;
            if(scrollbar == null)
                this.scrollbar = this.GetComponentInChildren<Scrollbar>();
            StartCoroutine(UpdateScrollbar());
            //1.0f;  //c
           //this.GetComponentInChildren<Scrollbar>().value = 1.0f;
        }
    }
    public void ZamknijPanel()
    {
        if(this.gameObject.activeInHierarchy)
        {
            //PomocniczeFunkcje.mainMenu.wróćDoMenu.interactable = true;
            PomocniczeFunkcje.UstawTimeScale(1);
            this.gameObject.SetActive(false);
            tłoEnabeld.enabled = false;
            if(UstawTenDomyslnyButton.aktualnyStanNaEkranie == 10 || UstawTenDomyslnyButton.aktualnyStanNaEkranie == 9)
            {
                PomocniczeFunkcje.mainMenu.OdpalKursor = true;
            }
            UstawTenDomyslnyButton.UstawDomyślnyButton();
        }
    }
    public void UstawTłoEnabeld(Image img)
    {
        tłoEnabeld = img;
    }
    IEnumerator UpdateScrollbar()
    {
        yield return new WaitForEndOfFrame();
        scrollbar.value = 1.0f;//scrollbar.size;
    }
}
