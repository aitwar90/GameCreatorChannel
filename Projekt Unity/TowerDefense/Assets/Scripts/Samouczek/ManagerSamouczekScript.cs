using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSamouczekScript : MonoBehaviour
{
    public static ManagerSamouczekScript mssInstance = null;
    private SamouczekInfoPanelScript sips = null;
    private SamouczekKliknijTuVisual sktv = null;
    private byte idxProgresuSamouczka = 0;
    public TextAsset plikTekstowySamouczka;
    private string[] zaladujTextKonkretne = null;
    // Start is called before the first frame update
    void Awake()
    {
        if(mssInstance == null)
        {
            mssInstance = this;
            sips = this.GetComponentInChildren<SamouczekInfoPanelScript>();
            sktv = this.GetComponentInChildren<SamouczekKliknijTuVisual>();
        }
        else
        {
            Destroy(this);
        }
    }
    public void WywolajProgress()
    {
        idxProgresuSamouczka++;
        switch(idxProgresuSamouczka)
        {
            case 0:
            break;
            default:
            Debug.Log("Nic nie robie");
            break;
        }
    }
    public void ZaladujText()
    {
        if(plikTekstowySamouczka != null)
        {
            zaladujTextKonkretne = null;
            sbyte jez = PomocniczeFunkcje.mainMenu.lastIdxJezyka;   //0 - Polski, 1 - Angielski
            List<string> listaOpisu = new List<string>();
            string fs = plikTekstowySamouczka.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            for(ushort i = 0; i < fLines.Length; i++)
            {
                string[] lot = fLines[i].Split('^');
                listaOpisu.Add(lot[jez]);
            }
            zaladujTextKonkretne = new string[listaOpisu.Count];
        }
        else
        {
            Debug.Log("Brak pliku tekstowego");
        }
    }
}
