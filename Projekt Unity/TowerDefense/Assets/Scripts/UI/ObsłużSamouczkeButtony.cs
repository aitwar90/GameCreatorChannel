using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObsłużSamouczkeButtony : MonoBehaviour
{
    // Start is called before the first frame update
    public StrukturaDoPrzycisków[] strukturaPrzyciskówSamouczek;
    public Image[] obrazkiButtonów;
    //private Image mojImage;
    void Awake()
    {
        this.obrazkiButtonów = this.GetComponentsInChildren<Image>();
        //this.mojImage = this.GetComponent<Image>();
    }
    void Start()
    {
        //this.mojImage.enabled = false;
    }
    public void WyłączPrzyciski()
    {
        for(byte i = 0; i < obrazkiButtonów.Length; i++)
        {
            if(obrazkiButtonów[i].enabled)
                obrazkiButtonów[i].enabled = false;
        }
        //this.mojImage.enabled = false;
    }
    public void OdpalPrzyciski(string[] tablicaIndeksówDoOdpalenia)
    {
        byte idx = 0;
        //this.mojImage.enabled = true;
        for (byte i = 0; i < tablicaIndeksówDoOdpalenia.Length; i++)
        {
            this.obrazkiButtonów[idx].enabled = true;
            switch (tablicaIndeksówDoOdpalenia[i])
            {
                case "X":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[0].obrazekPrzycisku;
                break;
                case "Y":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[1].obrazekPrzycisku;
                break;
                case "A":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[2].obrazekPrzycisku;
                break;
                case "B":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[3].obrazekPrzycisku;
                break;
                case "Prawy":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[4].obrazekPrzycisku;
                break;
                case "PrawyDrążekObrót":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[5].obrazekPrzycisku;
                break;
                case "LewyDrążekObrót":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[6].obrazekPrzycisku;
                break;
                case "PrawyDrążekWciśnij":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[7].obrazekPrzycisku;
                break;
                case "Lewy":
                this.obrazkiButtonów[idx].enabled = true;
                this.obrazkiButtonów[idx].sprite = strukturaPrzyciskówSamouczek[8].obrazekPrzycisku;
                break;
            }
            idx++;
        }
    }
}
[System.Serializable]
public struct StrukturaDoPrzycisków
{
    public Sprite obrazekPrzycisku;
    public string opisObrazka;
}
