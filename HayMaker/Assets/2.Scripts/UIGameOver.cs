using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIGameOver : MonoBehaviour
{
    public TextMeshProUGUI tmpScoreVal;
    void Start()
    {}

    public void Setup(float iNailFactor)
    {
        if (iNailFactor == 0f)
            tmpScoreVal.text = "Nailed it!";
        else
            tmpScoreVal.text = iNailFactor.ToString();
    }
}