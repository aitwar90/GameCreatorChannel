using UnityEngine;
using UnityEngine.UI;
public class SamouczekKliknijTuVisual : MonoBehaviour
{
    public Image image;
    private byte t = 255;
    private RectTransform rect;
    public bool mryganie = false;
    // Start is called before the first frame update
    void Awake()
    {
        rect = this.GetComponent<RectTransform>();
        Debug.Log("RectTransform.position = "+rect.position);   //Tego należy użyć
        //Debug.Log("RectTransform.sizeDelta = "+rect.sizeDelta);
        //Debug.Log("RectTransform.anchoredPosition = "+rect.anchoredPosition);
    }
    public void WłączObiekt()
    {
        if (!this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(true);
        }
    }
    public void WyłączObiekt()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.gameObject.SetActive(false);
        }
    }
    private void Mrygaj(bool wylMryg = false)
    {
        if (!wylMryg)
        {
            if (t > 255)
            {
                t = 50;
            }
            t++;
        }
        else
            t = 255;
        Color c = image.color;
        c.a = t;
        image.color = c;
    }
    public void UstawIkone(float cx, float cy, float ox, float oy)  //cx i cy to pozycja obiektu ox i oy to offset obiektu
    {
        float fx = cx + ox;
        float fy = cy + oy;
        rect.position = new Vector2(fx, fy);
    }
}
