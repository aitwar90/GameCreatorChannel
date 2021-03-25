using UnityEngine;
using UnityEngine.UI;
public class CzyJestemWyłączonyIZaznaczony : MonoBehaviour
{
    private Button ja;
    private Image image;
    private bool czyOznaczonyJestem = false;
    byte idx = 0;
    // Start is called before the first frame update
    void Awake()
    {
        ja = this.GetComponent<Button>();
        image = this.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (idx == 5)
        {
            if (!this.ja.interactable)
            {
                GameObject go = PomocniczeFunkcje.eSystem.currentSelectedGameObject;
                if (!czyOznaczonyJestem)
                {
                    if (go != null && go == this.ja.gameObject)
                    {
                        czyOznaczonyJestem = true;
                        image.color = this.ja.colors.selectedColor;
                    }
                }
                else
                {
                    if (go != null && go != this.ja.gameObject)
                    {
                        czyOznaczonyJestem = false;
                        image.color = this.ja.colors.normalColor;
                    }
                }
            }
            idx = 0;
        }
        else
        {
            idx++;
        }
    }
}
