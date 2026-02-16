using UnityEngine;
using TMPro;

public class MillerCanvasInput : MonoBehaviour
{
    public TMP_InputField hInput;
    public TMP_InputField kInput;
    public TMP_InputField lInput;

    public MillerIndexGenerator generator;

    void Start()
    {
        // Initialize UI from generator values
        hInput.text = generator.h.ToString();
        kInput.text = generator.k.ToString();
        lInput.text = generator.l.ToString();

        // Listen for text changes
        hInput.onEndEdit.AddListener(_ => UpdateValues());
        kInput.onEndEdit.AddListener(_ => UpdateValues());
        lInput.onEndEdit.AddListener(_ => UpdateValues());
    }

    void UpdateValues()
    {
        if (int.TryParse(hInput.text, out int h))
            generator.h = h;

        if (int.TryParse(kInput.text, out int k))
            generator.k = k;

        if (int.TryParse(lInput.text, out int l))
            generator.l = l;

        generator.SendMessage("GeneratePlane", SendMessageOptions.DontRequireReceiver);
    }
}
