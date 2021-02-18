using UnityEngine;

public class MoveCameraScript : MonoBehaviour
{
    #region Zmienne publiczne
    [Range(1, 50), Tooltip("Promień odległości dla przeunięcia kamery od środka sceny")]
    public byte maxPrzesuniecieKamery = 25;
    public static Vector3 bazowePolozenieKameryGry = new Vector3(52.0f, 6.5f, 43.0f);
    #endregion

    #region Zmienne prywatne
#if UNITY_STANDALONE
    private float prędkoscPrzesunięciaKamery = 5f;
#endif
#if UNITY_ANDROID || UNITY_IOS
    private float prędkoscPrzesunięciaKamery = 0.05f;
#endif
    private Vector3 ostatniaPozycjaKamery = Vector3.zero;
    private Vector3 pierwotnePołożenieKamery = Vector3.zero;
    private byte granica = 50;
    private int szerokośćObrazu;
    private int wysokśćObrazu;
    private Vector2 offs = Vector2.zero;
    #endregion
    #region Getery i setery
    public float PrędkośćPrzesunięciaKamery
    {
        get
        {
            return prędkoscPrzesunięciaKamery;
        }
        set
        {
            prędkoscPrzesunięciaKamery = value;
        }
    }
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        pierwotnePołożenieKamery = bazowePolozenieKameryGry;
        ostatniaPozycjaKamery = pierwotnePołożenieKamery;
        szerokośćObrazu = Screen.width;
        wysokśćObrazu = Screen.height;
#if UNITY_ANDROID || UNITY_IOS
        if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            prędkoscPrzesunięciaKamery = 6.0f;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (PomocniczeFunkcje.mainMenu.CzyMogePrzesuwaćKamere() && PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
        {
            ObsluzKlawiature();
#if UNITY_STANDALONE
        ObsłużMysz();
#endif
#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            {
                ObsłużMysz();
            }
            else
            {
                ObsłużTouchPad();
            }
#endif
        }
        else if (PomocniczeFunkcje.mainMenu.CzyAktywnyPanelZBudynkami())
        {
#if UNITY_STANDALONE
            if(Input.mouseScrollDelta.y != 0)
            {
                PomocniczeFunkcje.mainMenu.PrzesuńBudynki(Input.mouseScrollDelta.y);
            }
#endif
#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            {
                if (Input.mouseScrollDelta.y != 0)
                {
                    PomocniczeFunkcje.mainMenu.PrzesuńBudynki(Input.mouseScrollDelta.y * 30);
                }
            }
            else
            {
                if (Input.touchCount == 1)
                {
                    Touch dotyk = Input.GetTouch(0);
                    if (dotyk.phase == TouchPhase.Moved)
                    {
                        PomocniczeFunkcje.mainMenu.PrzesuńBudynki(dotyk.deltaPosition.y);
                    }
                }
            }
#endif
        }
    }
    #region Metody i funkcje obsługujące przemieszczanie kamery
    private bool SprawdźCzyMogęPrzesunąćKamerę(Vector3 newPos)
    {
        float actDist = Vector3.Distance(newPos, pierwotnePołożenieKamery);
        float lastDist = Vector3.Distance(ostatniaPozycjaKamery, pierwotnePołożenieKamery);
        if (actDist >= maxPrzesuniecieKamery && actDist > lastDist)
        {
            return false;
        }
        ostatniaPozycjaKamery = newPos;
        return true;
    }
    void ObsłużMysz()       //Przesuwanie kamery przez najechanie kursorem myszy do krawędzi aplikacji
    {
        if (Input.mousePosition.x > szerokośćObrazu - granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        else if (Input.mousePosition.x < 0 + granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        if (Input.mousePosition.y > wysokśćObrazu - granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        else if (Input.mousePosition.y < 0 + granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
    }
    void ObsluzKlawiature()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Vector3 posDotyk = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(posDotyk))
            {
                this.transform.position = posDotyk;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        else if (Input.GetKey(KeyCode.S))
        {
            Vector3 posDotyk = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(posDotyk))
            {
                this.transform.position = posDotyk;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            Vector3 posDotyk = new Vector3(this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
            if (SprawdźCzyMogęPrzesunąćKamerę(posDotyk))
            {
                this.transform.position = posDotyk;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
        else if (Input.GetKey(KeyCode.D))
        {
            Vector3 posDotyk = new Vector3(this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
            if (SprawdźCzyMogęPrzesunąćKamerę(posDotyk))
            {
                this.transform.position = posDotyk;
                if (MainMenu.singelton.OdpalonyPanel)
                {
                    MainMenu.singelton.UstawPanelUI("", Vector2.zero);
                }
            }
        }
    }
    void ObsłużTouchPad()
    {
        if (Input.touchCount == 1)    //Przesuniecie kamery
        {
            bool klik = false;
            if (klik)
                return;
            Touch dotyk = Input.GetTouch(0);
            if (dotyk.phase == TouchPhase.Moved) //Jeśli wykrywa przesunięcie palcem po ekranie
            {
                Vector2 dPos = dotyk.deltaPosition * this.prędkoscPrzesunięciaKamery;
                Vector3 tmp = new Vector3(this.transform.position.x + dPos.x, this.transform.position.y, this.transform.position.z + dPos.y);
                if (SprawdźCzyMogęPrzesunąćKamerę(tmp))
                {
                    tmp.y = (ostatniaPozycjaKamery.y > 2.0) ? ostatniaPozycjaKamery.y : bazowePolozenieKameryGry.y;
                    this.transform.position = tmp;
                }
            }
        }
        if (Input.touchCount == 2)   //Oddalenie i przybliżenie kamery
        {
            Touch przybliżenie1 = Input.GetTouch(0);
            Touch przybliżenie2 = Input.GetTouch(1);
            //Przesuniecie w lewo deltaposition.x jest - w prawo + w góre y + w dół -
            //Debug.Log("Przybliżenie deltaPosition 1 "+przybliżenie1.deltaPosition+" Przyblizenie 2 delta position = "+przybliżenie2.deltaPosition);
            Vector2 przyb1Prev = przybliżenie1.deltaPosition - przybliżenie2.deltaPosition;
            float różnicaPrzybliżenia = DodajElementyWektora(ref przyb1Prev) * prędkoscPrzesunięciaKamery;
            Vector3 eNP = this.transform.position + (this.transform.forward * różnicaPrzybliżenia);
            if (Mathf.Abs(eNP.y - bazowePolozenieKameryGry.y) < 2.5)
            {
                transform.position = eNP;
                ostatniaPozycjaKamery = eNP;
            }
            if (MainMenu.singelton.OdpalonyPanel)
            {
                MainMenu.singelton.UstawPanelUI("", Vector2.zero);
            }
        }
    }
    private float DodajElementyWektora(ref Vector2 v)
    {
        v = v.normalized;
        return (v.x + v.y) / 2.0f;
    }
    #endregion
}
