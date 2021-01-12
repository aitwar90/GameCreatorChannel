using UnityEngine;
using UnityEngine.UI;
public class SamouczekInfoPanelScript : MonoBehaviour
{
    public Text wyśwTekstNaPanelu;
    public Button przyciskDalejButtonu;
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
            PomocniczeFunkcje.UstawTimeScale(0);
            this.gameObject.SetActive(true);
        }
    }
    public void ZamknijPanel()
    {
        if(this.gameObject.activeInHierarchy)
        {
            PomocniczeFunkcje.UstawTimeScale(1);
            this.gameObject.SetActive(false);
        }
    }
}
