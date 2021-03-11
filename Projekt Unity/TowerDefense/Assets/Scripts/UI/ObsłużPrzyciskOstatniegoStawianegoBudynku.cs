using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObsłużPrzyciskOstatniegoStawianegoBudynku : MonoBehaviour
{
    // Start is called before the first frame update
    public short indeks;
    public int koszt;
    public Sprite domyslnyObrazek;
    private Button tenPrzycisk;

    void Awake()
    {
        tenPrzycisk = this.GetComponent<Button>();
        tenPrzycisk.interactable = false;
    }
    public void OnCLick()
    {
        if(indeks < 0)
            return;
        bool f = PomocniczeFunkcje.spawnBudynki.KliknietyBudynekWPanelu((short)indeks);
        if (!f && tenPrzycisk.interactable)
        {
            RestartPrzycisku();
        }
        else if (f)
        {
            PomocniczeFunkcje.mainMenu.KliknijPrzyciskPostawBudynek();
        }
    }
    public void PrzypiszDanePrzyciskowi(short indeks, int koszt, Sprite obrazek)
    {
        this.indeks = indeks;
        this.koszt = koszt;
        tenPrzycisk.image.sprite = obrazek;
        tenPrzycisk.interactable = true;
    }
    public void RestartPrzycisku()
    {
        indeks = -1;
        koszt = -1;
        tenPrzycisk.interactable = false;
        if (domyslnyObrazek != null)
            tenPrzycisk.image.sprite = domyslnyObrazek;
    }
}
