using UnityEngine;

public class MoveCameraScript : MonoBehaviour
{
    public static MoveCameraScript mscInstance = null;
    #region Zmienne publiczne
    [Range(1, 50), Tooltip("Promień odległości dla przeunięcia kamery od środka sceny")]
    public byte maxPrzesuniecieKamery = 25;
    public static Vector3 bazowePolozenieKameryGry = new Vector3(52.0f, 6.5f, 52.0f);   //z - 43
    public static bool odwrócPrzesuwanie = false;   //Czy należy zmienić przesuwanie kamery (X - Z // Z - X)
    public GameObject volume;
    #endregion

    #region Zmienne prywatne
    //#if UNITY_STANDALONE
    //    private float prędkoscPrzesunięciaKamery = 5f;
    //#endif
    //#if UNITY_ANDROID || UNITY_IOS
    private float prędkoscPrzesunięciaKamery = 0.035f;
    private static readonly float[] zoomBounds = new float[] { 30f, 80f };
    //#endif
    private Vector3 ostatniaPozycjaKamery = Vector3.zero;
    private Vector3 pierwotnePołożenieKamery = Vector3.zero;
    private byte granica = 5;
    private int szerokośćObrazu;
    private int wysokśćObrazu;
    public bool wasZoomingLastFrame;
    private Vector2[] lastZoomPositions;
    bool przesuniecie = true;
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
    public int[] ZwrócWielkośćEkranu
    {
        get
        {
            return new int[] {szerokośćObrazu, wysokśćObrazu};
        }
    }
    public bool CzyOdwrocenieKamery
    {
        set
        {
            odwrócPrzesuwanie = !odwrócPrzesuwanie;
        }
    }
    #endregion
    void Awake()
    {
        if (mscInstance == null)
        {
            mscInstance = this;
            if (volume == null)
                volume = GameObject.Find("Volume");
        }
        else
        {
            Destroy(this);
        }
    }
    void Start()
    {
        pierwotnePołożenieKamery = bazowePolozenieKameryGry;
        ostatniaPozycjaKamery = pierwotnePołożenieKamery;
        szerokośćObrazu = Screen.width;
        wysokśćObrazu = Screen.height;
#if UNITY_ANDROID || UNITY_IOS
        if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            prędkoscPrzesunięciaKamery = 5.0f;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (PomocniczeFunkcje.mainMenu.CzyAktywnyPanelZBudynkami())
        {
            if (Input.touchCount == 1)
            {
                Touch dotyk = Input.GetTouch(0);
                if (dotyk.phase == TouchPhase.Moved)
                {
                    PomocniczeFunkcje.mainMenu.PrzesuńBudynki(dotyk.deltaPosition.y);
                }
            }
            else
            {
                if (PomocniczeFunkcje.eSystem.currentSelectedGameObject != null && PomocniczeFunkcje.eSystem.currentSelectedGameObject.name == "PrzyciskBudynku(Clone)")
                {
                    float iGV = Input.GetAxisRaw("Vertical");
                    if (iGV != 0)
                    {
                        PomocniczeFunkcje.mainMenu.AutomatycznieUstawBudynkiWPanelu(PomocniczeFunkcje.eSystem.currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>());
                    }
                }
            }
        }
        else if (PomocniczeFunkcje.mainMenu.CzyMogePrzesuwaćKamere())
        {
            ObsłużInputySwitch();
        }
        /* UNITY_ANDROID
        if (PomocniczeFunkcje.mainMenu.CzyMogePrzesuwaćKamere() && PomocniczeFunkcje.spawnBudynki.aktualnyObiekt == null)
        {
            ObsluzKlawiature();
//#if UNITY_STANDALONE
//        ObsłużMysz();
//#endif
//#if UNITY_ANDROID || UNITY_IOS
            if (Input.mousePresent && !ManagerGryScript.odpalamNaUnityRemote)
            {
                ObsłużMysz();
            }
            else
            {
                ObsłużTouchPad();
            }
//#endif
        }
        else if (PomocniczeFunkcje.mainMenu.CzyAktywnyPanelZBudynkami())
        {
//#if UNITY_STANDALONE
//            if(Input.mouseScrollDelta.y != 0)
//            {
//                PomocniczeFunkcje.mainMenu.PrzesuńBudynki(Input.mouseScrollDelta.y);
//            }
//#endif
//#if UNITY_ANDROID || UNITY_IOS
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
//#endif
        }
        */
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
        PomocniczeFunkcje.kameraZostalaPrzesunieta = 2;
        return true;
    }
    void ObsłużSwitch(float pion, float poziom, float scrolwheel)
    {
        if (pion == 0 && poziom == 0 && scrolwheel == 0)
            return;
        float posDotykZ = this.transform.position.z;
        float posDotykX = this.transform.position.x;
        bool przes = false;
        if (pion != 0)
        {
            posDotykZ -= pion * 8f * Time.deltaTime;
            przes = true;
            PomocniczeFunkcje.spawnBudynki.offsetPrzyPrzesuwaniuKamery.y = pion * 8f * Time.deltaTime;
        }
        if (poziom != 0)
        {
            posDotykX -= poziom * 8f * Time.deltaTime;
            przes = true;
            PomocniczeFunkcje.spawnBudynki.offsetPrzyPrzesuwaniuKamery.x = -poziom * 8f * Time.deltaTime;
        }
        if (przes)
        {
            Vector3 v3 = new Vector3(posDotykX, this.transform.position.y, posDotykZ);
            if (SprawdźCzyMogęPrzesunąćKamerę(v3))
                this.transform.position = new Vector3(posDotykX, this.transform.position.y, posDotykZ);
        }
        if (scrolwheel != 0)
        {
            ZoomingMeNew(scrolwheel, 3.0f);
        }
    }
    void ObsłużInputySwitch()
    {
        float iGV = Input.GetAxisRaw("Vertical");
        float iGH = Input.GetAxisRaw("Horizontal");
        float scrWhl = Input.GetAxis("Mouse ScrollWheel");
        ObsłużSwitch(iGV, iGH, scrWhl);
        iGH = 0.0f; iGV = 0.0f;
        iGV = Input.GetAxisRaw("StickRVertical") * 4f;
        iGH = Input.GetAxisRaw("StickRHorizontal") * 4f;
        if (iGH != 0 || iGV != 0)
        {
            PomocniczeFunkcje.mainMenu.PrzesunKursorO(iGH, -iGV, szerokośćObrazu, wysokśćObrazu);
        }
    }
    void ObsłużMysz()       //Przesuwanie kamery przez najechanie kursorem myszy do krawędzi aplikacji
    {
        if (Input.mousePosition.x > szerokośćObrazu - granica)
        {
            Vector3 newPos = new Vector3((!odwrócPrzesuwanie) ? this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
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
            Vector3 newPos = new Vector3((odwrócPrzesuwanie) ? this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
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
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, (!odwrócPrzesuwanie) ? this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
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
            Vector3 newPos = new Vector3(this.transform.position.x, this.transform.position.y, (odwrócPrzesuwanie) ? this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
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
            Vector3 posDotyk = new Vector3(this.transform.position.x, this.transform.position.y, (!odwrócPrzesuwanie) ? this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
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
            Vector3 posDotyk = new Vector3(this.transform.position.x, this.transform.position.y, (odwrócPrzesuwanie) ? this.transform.position.z - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.z + prędkoscPrzesunięciaKamery * Time.deltaTime);
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
            Vector3 posDotyk = new Vector3((odwrócPrzesuwanie) ? this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
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
            Vector3 posDotyk = new Vector3((!odwrócPrzesuwanie) ? this.transform.position.x - prędkoscPrzesunięciaKamery * Time.deltaTime : this.transform.position.x + prędkoscPrzesunięciaKamery * Time.deltaTime, this.transform.position.y, this.transform.position.z);
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
                Vector3 tmp = new Vector3((!odwrócPrzesuwanie) ? this.transform.position.x - dPos.x : this.transform.position.x + dPos.x, this.transform.position.y, (!odwrócPrzesuwanie) ? this.transform.position.z - dPos.y : this.transform.position.z + dPos.y);
                if (SprawdźCzyMogęPrzesunąćKamerę(tmp))
                {
                    tmp.y = (ostatniaPozycjaKamery.y > 2.0) ? ostatniaPozycjaKamery.y : bazowePolozenieKameryGry.y;
                    this.transform.position = tmp;
                }
            }
        }
        else if (Input.touchCount == 2)   //Oddalenie i przybliżenie kamery
        {
            Touch przybliżenie1 = Input.GetTouch(0);
            Touch przybliżenie2 = Input.GetTouch(1);
            //https://kylewbanks.com/blog/unity3d-panning-and-pinch-to-zoom-camera-with-touch-and-mouse-input
            Vector2[] newPositions = new Vector2[] { przybliżenie1.position, przybliżenie2.position };
            if (!wasZoomingLastFrame)
            {
                lastZoomPositions = newPositions;
                wasZoomingLastFrame = true;
            }
            else
            {
                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                float offset = newDistance - oldDistance;

                ZoomingMeNew(offset, prędkoscPrzesunięciaKamery);
                lastZoomPositions = newPositions;
            }
            //Mój sposób    
            //Przesuniecie w lewo deltaposition.x jest - w prawo + w góre y + w dół -
            //Debug.Log("Przybliżenie deltaPosition 1 "+przybliżenie1.deltaPosition+" Przyblizenie 2 delta position = "+przybliżenie2.deltaPosition);
            /*
            Vector2 przyb1Prev = przybliżenie1.deltaPosition - przybliżenie2.deltaPosition;
            float różnicaPrzybliżenia = DodajElementyWektora(ref przyb1Prev) * prędkoscPrzesunięciaKamery;
            Vector3 eNP = this.transform.position + (this.transform.forward * różnicaPrzybliżenia);
            if (Mathf.Abs(eNP.y - bazowePolozenieKameryGry.y) < 2.5)
            {
                transform.position = eNP;
                ostatniaPozycjaKamery = eNP;
            }
            */
            if (MainMenu.singelton.OdpalonyPanel)
            {
                MainMenu.singelton.UstawPanelUI("", Vector2.zero);
            }
        }
        else
            wasZoomingLastFrame = false;
    }
    void ZoomingMeNew(float offset, float speed)
    {
        if (offset == 0)
            return;

        PomocniczeFunkcje.oCam.fieldOfView = Mathf.Clamp(PomocniczeFunkcje.oCam.fieldOfView - (offset * speed), zoomBounds[0], zoomBounds[1]);
        PomocniczeFunkcje.kameraZostalaPrzesunieta = 2;
    }
    private float DodajElementyWektora(ref Vector2 v)
    {
        v = v.normalized;
        return (v.x + v.y) / 2.0f;
    }
    /* UNITY_ANDROID
    ///<summary>Ustawia wartość dla opcji Post-Processing.</summary>
    ///<param name="c">Czy Post processing ma zostać włączony?</param>
    ///<param name="czyPrzezOpcje">Czy zmiana jest wywoływana przez zmianę w opcjach przez użytkownika?</param>
    public void UstawPostProcessing(bool c, bool czyPrzezOpcje = false)
    {
        volume.SetActive(c);
        this.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>().enabled = c;
        if(!czyPrzezOpcje)
            PomocniczeFunkcje.mainMenu.CzyPostProcesing = c;
    }
    */
    #endregion
}
