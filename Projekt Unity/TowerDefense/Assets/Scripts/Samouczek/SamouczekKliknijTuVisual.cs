using UnityEngine;
using UnityEngine.UI;
public class SamouczekKliknijTuVisual : MonoBehaviour
{
    public Image image;
    private byte t = 255;
    public bool mryganie = false;
    // Start is called before the first frame update
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
}
