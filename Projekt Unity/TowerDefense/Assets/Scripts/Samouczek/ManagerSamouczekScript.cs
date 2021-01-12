using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManagerSamouczekScript : MonoBehaviour
{
    public static ManagerSamouczekScript mssInstance = null;
    private SamouczekInfoPanelScript sips = null;
    private SamouczekKliknijTuVisual sktv = null;
    private byte idxProgresuSamouczka = 0;
    private bool sprawdzajCzyZaliczone = false;
    public TextAsset plikTekstowySamouczka;
    public string[] zaladujTextKonkretne = null;
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
            sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[0]);
            break;
            case 1:
            sips.ZaladujTekstPanelu(ref zaladujTextKonkretne[1]);
            break;
            default:
            Debug.Log("Nic nie robie");
            break;
        }
    }
    void Update()
    {
        if(sprawdzajCzyZaliczone)
        {
            switch(idxProgresuSamouczka)
            {
                case 0: //Przesunięcie kamery
                Vector3 tOcaPos = PomocniczeFunkcje.oCam.transform.position;
                if(tOcaPos!= MoveCameraScript.bazowePolozenieKameryGry 
                && tOcaPos != Vector3.zero)
                {
                    WywolajProgress();
                    sprawdzajCzyZaliczone = false;
                }
                break;
            }
        }
    }
    public void ZaladujText()
    {
        if(plikTekstowySamouczka != null)
        {
            zaladujTextKonkretne = null;
            sbyte jez = PomocniczeFunkcje.mainMenu.lastIdxJezyka;   //0 - Polski, 1 - Angielski
            Debug.Log("Jez = "+jez);
            List<string> listaOpisu = new List<string>();
            string fs = plikTekstowySamouczka.text;
            fs = fs.Replace("\n", "");
            fs = fs.Replace("\r", "");
            string[] fLines = fs.Split(';');
            for(ushort i = 0; i < fLines.Length; i++)
            {
                if(fLines[i] == "")
                    continue;
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
    public void ZamknijPanel()
    {
        sprawdzajCzyZaliczone = true;
        this.sips.ZamknijPanel();
    }
}
