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
        else if (iNailFactor < 1.8f)
            tmpScoreVal.text = "Average";
        else if (iNailFactor < 2.6f)
            tmpScoreVal.text = "Weak midget";
        else
            tmpScoreVal.text = "Shrimp";
    }
}