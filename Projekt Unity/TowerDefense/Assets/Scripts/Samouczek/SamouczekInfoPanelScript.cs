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
        PomocniczeFunkcje.mainMenu.wróćDoMenu.interactable = false;
            PomocniczeFunkcje.UstawTimeScale(0);
        if(!this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(true);
            //1.0f;  //c
           //this.GetComponentInChildren<Scrollbar>().value = 1.0f;
        }
        tłoEnabeld.enabled = true;
            if(scrollbar == null)
                this.scrollbar = this.GetComponentInChildren<Scrollbar>();
            StartCoroutine(UpdateScrollbar());
        MoveCameraScript.blokujKamere = true;
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
        MoveCameraScript.blokujKamere = false;
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
