using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatystykiScript : MonoBehaviour
{
    [Tooltip("0-Bilans, 1-Badania, 2-Rozwój, 3-Budowa, 4-Naprawa, 5-Z wrogów, 6-Za poziom")]
    public Text[] wartościDlaTekstu;
    // Start is called before the first frame update
    void Start()
    {
        ResetujMnie();
    }
    ///<summary>Ustaw poszczególne wartości dla statystyk.</summary>
    ///<param name="wartości">Wartości dla poszczególnych elementów (0-Badania, 1-Rozwój z akademii, 2-Budowa budynków, 3-Naprawa budynków, 4-Zyski z pokonanych wrogów, 5-Ilość nagrody za ukończony poziom).</param>
    public void UstawWartościIOdpalMnie(ref int[] wartości)
    {
        int bilans = 0;
        for(byte i = 0; i < wartości.Length; i++)
        {
            bilans += wartości[i];
            wartościDlaTekstu[i+1].text = wartości[i].ToString();
        }
        wartościDlaTekstu[0].text = bilans.ToString();
        this.gameObject.SetActive(true);
    }
    public void WyłączMnie()
    {
        this.gameObject.SetActive(false);
        ResetujMnie();
    }
    private void ResetujMnie()
    {
        for(byte i = 0; i < wartościDlaTekstu.Length; i++)
        {
            wartościDlaTekstu[i].text = "00";
        }
    }
}
