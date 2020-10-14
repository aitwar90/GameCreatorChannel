using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public static bool czyMenuEnable = true;
    private GameObject menu;
    private GameObject uiGry;
    private GameObject optionsMenu;
    private Button przyciskWznów;
    private Vector3 lastPosCam = Vector3.zero;

    void Awake()
    {
        menu = GameObject.Find("Menu").gameObject;
        uiGry = GameObject.Find("UIGry").gameObject;
        optionsMenu = GameObject.Find("OptionsMenu").gameObject;
        przyciskWznów = GameObject.Find("ResumeButton").GetComponent<Button>();
        uiGry.SetActive(false);
        optionsMenu.SetActive(false);
        przyciskWznów.enabled = false;
        PomocniczeFunkcje.oCam = Camera.main;
    }
    public void PlayGame()
    {
        if(SceneManager.sceneCount == 1)
        {
            SceneManager.LoadScene((byte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka, LoadSceneMode.Additive);
        }
        else
        {
            //Reset scene
            PomocniczeFunkcje.ResetujWszystko();
            SceneManager.UnloadSceneAsync(1);
            PomocniczeFunkcje.managerGryScript.CzyScenaZostałaZaładowana = false;
            SceneManager.LoadScene((byte)PomocniczeFunkcje.managerGryScript.aktualnaEpoka, LoadSceneMode.Additive);
        }
            PomocniczeFunkcje.oCam.transform.position = new Vector3(50.0f, 8.0f, 42.5f);
            lastPosCam = new Vector3(50.0f, 8.0f, 42.5f);
            PrzełączUI(false);
    }
    public void OptionsMenu(bool actButton)
    {
        menu.SetActive(!actButton);
        optionsMenu.SetActive(actButton);
    }
    public void QuitGame()
    {
        Debug.Log("MainMenu 55: Quit");
        PomocniczeFunkcje.ZapiszDane();
        Application.Quit();
    }
    public void PrzełączUI(bool aktywujeMenu)
    {
        przyciskWznów.enabled = true;
        czyMenuEnable = aktywujeMenu;
        menu.SetActive(aktywujeMenu);
        uiGry.SetActive(!aktywujeMenu);
        if (aktywujeMenu)
        {
            Time.timeScale = 0.0f;
            lastPosCam = PomocniczeFunkcje.oCam.transform.position;
            PomocniczeFunkcje.oCam.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
        }
        else
        {
            Time.timeScale = 1.0f;
            PomocniczeFunkcje.oCam.transform.position = lastPosCam;
        }
    }
}
