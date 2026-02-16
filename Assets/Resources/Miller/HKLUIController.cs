using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HKLUIController : MonoBehaviour
{
    public MillerPlaneInCell millerPlane;
    public TMP_InputField inputH;
    public TMP_InputField inputK;
    public TMP_InputField inputL;
    public Button applyButton;

    void Start()
    {
        applyButton.onClick.AddListener(Apply);
    }

    public void Apply()
    {
        int h = ParseInt(inputH.text);
        int k = ParseInt(inputK.text);
        int l = ParseInt(inputL.text);

        // 防止 (0,0,0)
        if (h == 0 && k == 0 && l == 0)
        {
            h = 1; k = 0; l = 0;
            inputH.text = "1";
            inputK.text = "0";
            inputL.text = "0";
        }

        millerPlane.SetHKL(h, k, l);
    }

    int ParseInt(string s)
    {
        return int.TryParse(s, out var v) ? v : 0;
    }
}
