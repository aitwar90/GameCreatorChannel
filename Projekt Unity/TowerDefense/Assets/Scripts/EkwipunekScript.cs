using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EkwipunekScript
{
    public string nazwaPrzedmiotu;
    public Image obrazekPrzedmiotu;
    public Button aktywatorPrzedmiotu;
    public List<Component> listaPrzedmiotów = null;

    public EkwipunekScript(string _nazwaPrzedmiotu, Image _obrazekPrzedmiotu, Button _aktywatorPrzedmiotu)
    {
        this.nazwaPrzedmiotu = _nazwaPrzedmiotu;
        this.obrazekPrzedmiotu = _obrazekPrzedmiotu;
        this.aktywatorPrzedmiotu = _aktywatorPrzedmiotu;
        if(listaPrzedmiotów == null)
        {
            listaPrzedmiotów = new List<Component>();
        }
    }
}
