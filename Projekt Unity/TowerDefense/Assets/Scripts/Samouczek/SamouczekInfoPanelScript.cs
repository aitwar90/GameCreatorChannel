using UnityEngine;
using UnityEngine.UI;
using System.Collections;
public class SamouczekInfoPanelScript : MonoBehaviour
{
    public Text wyśwTekstNaPanelu;
    public Button przyciskDalejButtonu;
    private Image tłoEnabeld = null;
    private Scrollbar scrollbar = null;
    public void ZaladujTekstPanelu(ref string opis)
    {
        wyśwTekstNaPanelu.text = opis;
        OdpalPanel();
    }
    private void OdpalPanel()
    {
        if(!this.gameObject.activeInHierarchy)
        {
            PomocniczeFunkcje.mainMenu.wróćDoMenu.interactable = false;
            PomocniczeFunkcje.UstawTimeScale(0);
            this.gameObject.SetActive(true);
            tłoEnabeld.enabled = true;
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
            PomocniczeFunkcje.mainMenu.wróćDoMenu.interactable = true;
            PomocniczeFunkcje.UstawTimeScale(1);
            this.gameObject.SetActive(false);
            tłoEnabeld.enabled = false;
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
