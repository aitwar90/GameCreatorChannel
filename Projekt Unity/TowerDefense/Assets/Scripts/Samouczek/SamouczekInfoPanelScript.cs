using UnityEngine;
using UnityEngine.UI;
public class SamouczekInfoPanelScript : MonoBehaviour
{
    public Text wyśwTekstNaPanelu;
    public Button przyciskDalejButtonu;
    private Image tłoEnabeld = null;
    // Start is called before the first frame update
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
}
