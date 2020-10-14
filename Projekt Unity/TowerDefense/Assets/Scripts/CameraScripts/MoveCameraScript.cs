using UnityEngine;

public class MoveCameraScript : MonoBehaviour
{
    #region Zmienne publiczne
    [Range(1, 50), Tooltip("Promień odległości dla przeunięcia kamery od środka sceny")]
    public byte maxPrzesuniecieKamery = 25;

    #endregion

    #region Zmienne prywatne
    private float prędkoscPrzesunięciaKamery = 2.5f;
    private Vector3 ostatniaPozycjaKamery = Vector3.zero;
    private Vector3 pierwotnePołożenieKamery = Vector3.zero;
    private byte granica = 50;
    private int szerokośćObrazu;
    private int wysokśćObrazu;
    private Vector3 offs = Vector3.zero;
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
        pierwotnePołożenieKamery = new Vector3(50.0f, 5.0f, 45.0f);
        ostatniaPozycjaKamery = pierwotnePołożenieKamery;
        szerokośćObrazu = Screen.width;
        wysokśćObrazu = Screen.height;
    }

    // Update is called once per frame
    void Update()
    {
        if (!MainMenu.czyMenuEnable && PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
        {
#if UNITY_STANDALONE
        ObsłużMysz();
#endif
#if UNITY_ANDROID
            if (Input.mousePresent)
            {
                ObsłużMysz();
            }
            else
            {
                ObsłużTouchPad();
            }
        }
#endif
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
            }
        }
        else if (Input.mousePosition.x < 0 + granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
            }
        }
        if (Input.mousePosition.y > wysokśćObrazu - granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
            }
        }
        else if (Input.mousePosition.y < 0 + granica)
        {
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime);
            if (SprawdźCzyMogęPrzesunąćKamerę(newPos))
            {
                this.transform.position = newPos;
            }
        }
    }
    void ObsłużTouchPad()
    {
        if (Input.touchCount == 1)    //Przesuniecie kamery
        {
            Touch dotyk = Input.GetTouch(0);    //Pobierz informację o pierwszym dotknięciu
            Vector3 posDotyk = PomocniczeFunkcje.OkreślPozycjęŚwiataKursora(ostatniaPozycjaKamery);
            if(dotyk.phase == TouchPhase.Began)
            {
                offs = posDotyk;
            }
            else if (dotyk.phase == TouchPhase.Moved) //Jeśli wykrywa przesunięcie palcem po ekranie
            {
                Vector3 tmpOfs = (posDotyk-offs)*prędkoscPrzesunięciaKamery;
                Vector3 tmp = ostatniaPozycjaKamery + tmpOfs;
                if (SprawdźCzyMogęPrzesunąćKamerę(tmp))
                {
                    tmp.y = 5.0f;
                    this.transform.position = tmp;
                    offs = posDotyk;
                }
            }
        }
        /*
        if (Input.touchCount == 2)   //Oddalenie i przybliżenie kamery
        {
            Touch przybliżenie1 = Input.GetTouch(0);
            Touch przybliżenie2 = Input.GetTouch(1);

            Vector2 przyb1Prev = przybliżenie1.position - przybliżenie1.deltaPosition;
            Vector2 przyb2Prev = przybliżenie2.position - przybliżenie2.deltaPosition;

            float prevTouchDeltaMag = (przyb1Prev - przyb2Prev).magnitude;
            float touchDeltaMag = (przybliżenie1.position - przybliżenie2.position).magnitude;

            float różnicaPrzybliżenia = ((prevTouchDeltaMag - touchDeltaMag) * -1) * prędkoscPrzesunięciaKamery;
            if (Mathf.Abs(różnicaPrzybliżenia - 5.0f) < 2)
                transform.Translate(0, 0, różnicaPrzybliżenia);
        }
        */
    }
    #endregion
}
