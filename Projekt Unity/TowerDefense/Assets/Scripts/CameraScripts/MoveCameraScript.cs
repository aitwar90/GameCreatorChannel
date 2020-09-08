using UnityEngine;

public class MoveCameraScript : MonoBehaviour
{
    #region Zmienne publiczne
    [Range(1, 50)]
    public byte maxPrzesuniecieKamery = 25;

    #endregion

    #region Zmienne prywatne
    private float prędkoscPrzesunięciaKamery = 0.005f;
    private Vector3 ostatniaPozycjaKamery = Vector3.zero;
    private Vector3 pierwotnePołożenieKamery = Vector3.zero;
#if UNITY_STANDALONE
    int szerokośćObrazu;
    int wysokśćObrazu;
    byte granica = 50;
#endif
    #endregion
    // Start is called before the first frame update
    void Start()
    {
        pierwotnePołożenieKamery = this.transform.position;
#if UNITY_STANDALONE
        prędkoscPrzesunięciaKamery = 1.5f;
        szerokośćObrazu = Screen.width;
        wysokśćObrazu = Screen.height;
#endif
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_STANDALONE
        ObsłużMysz();
#endif
#if UNITY_ANDROID
        ObsłużTouchPad();
#endif
    }
    #region Metody i funkcje obsługujące przemieszczanie kamery
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
        if (Input.touchCount > 0)    //Przesuniecie kamery
        {
            Touch dotyk = Input.GetTouch(0);    //Pobierz informację o pierwszym dotknięciu
            Vector3 posDotyk = new Vector3(dotyk.position.x, dotyk.position.y, 0f);
            if (dotyk.phase == TouchPhase.Moved) //Jeśli wykrywa przesunięcie palcem po ekranie
            {
                Vector3 przesuniecie = ((ostatniaPozycjaKamery - posDotyk) * prędkoscPrzesunięciaKamery);
                if(SprawdźCzyMogęPrzesunąćKamerę(ostatniaPozycjaKamery+przesuniecie))
                {
                    this.transform.Translate(przesuniecie);
                    ostatniaPozycjaKamery = posDotyk;
                }
            }
        }
        if (Input.touchCount == 2)   //Oddalenie i przybliżenie kamery
        {
            Touch przybliżenie1 = Input.GetTouch(0);
            Touch przybliżenie2 = Input.GetTouch(1);

            Vector2 przyb1Prev = przybliżenie1.position - przybliżenie1.deltaPosition;
            Vector2 przyb2Prev = przybliżenie2.position - przybliżenie2.deltaPosition;

            float prevTouchDeltaMag = (przyb1Prev - przyb2Prev).magnitude;
            float touchDeltaMag = (przybliżenie1.position - przybliżenie2.position).magnitude;

            float różnicaPrzybliżenia = (prevTouchDeltaMag - touchDeltaMag) * -1;

            transform.Translate(0, 0, różnicaPrzybliżenia);
        }
    }
    private bool SprawdźCzyMogęPrzesunąćKamerę(Vector3 newPos)
    {
        float actDist = Vector3.Distance(newPos, pierwotnePołożenieKamery);
        float lastDist = Vector3.Distance(ostatniaPozycjaKamery,pierwotnePołożenieKamery);
        if (actDist >= maxPrzesuniecieKamery && actDist > lastDist)
        {
            return false;
        }
        ostatniaPozycjaKamery = newPos;
        return true;
    }
    #endregion
}
