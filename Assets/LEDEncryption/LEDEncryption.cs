using UnityEngine;
using System.Collections;
using System.Text;

public class LEDEncryption : MonoBehaviour {

    public TextMesh LayerText;
    int layer = -1;

    public KMSelectable[] buttons;
    public GameObject[] LEDs;
    int[] LEDDat;
    public Material[] mats;
    public Material off;

    int correctIndex;
    bool isActivated = false;

    string letters = "abcdefghijklmnopqrstuvwxyz";
    string layerletters = "";

    // Use this for initialization
    void Start() {
        for (int i = 0; i < LEDs.Length; i++)
        {
            LEDs[i].GetComponent<MeshRenderer>().material = off;
        }

        LEDDat = new int[Random.Range(2,6)];

        for(int i = 0; i < LEDDat.Length; i++)
        {
            LEDs[i].SetActive(true);
            LEDDat[i] = Random.Range(2,8);
        }

        LoadLayer();
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
	}

    void ActivateModule()
    {
        isActivated = true;
        for(int i = 0; i < LEDDat.Length;i++)
        {
            LEDs[i].GetComponent<MeshRenderer>().material = mats[LEDDat[i] - 2];
        }
    }

    void LoadLayer()
    {
        layer++;
        LayerText.text = (layer + 1).ToString();
        correctIndex = Random.Range(0, 4);
        layerletters = "";
        for(int i = 0;i < 4; i++)
        {
            string t = letters[Random.Range(0,letters.Length-1)].ToString();
            while (layerletters.IndexOf(t) != -1)
            {
                t = letters[Random.Range(0, letters.Length - 1)].ToString();
            }
            layerletters += t;
        }

        StringBuilder tsb = new StringBuilder(layerletters);
        int ol = (letters.IndexOf(tsb.ToString()[correctIndex]) * LEDDat[layer]) % (letters.Length);
        while(tsb.ToString().Contains(letters[ol].ToString())) {
            string t = letters[Random.Range(0, letters.Length - 1)].ToString();
            while (tsb.ToString().IndexOf(t) != -1)
            {
                t = letters[Random.Range(0, letters.Length - 1)].ToString();
            }
            tsb[correctIndex] = t[0];
            ol = (letters.IndexOf(tsb.ToString()[correctIndex]) * LEDDat[layer]) % (letters.Length);
        }
        tsb[3 - correctIndex] = letters[ol];
        layerletters = tsb.ToString();

        for(int i = 0;i < buttons.Length;i++)
        {
            buttons[i].GetComponentInChildren<TextMesh>().text = layerletters[i].ToString();
        }

        for (int i = 0; i < buttons.Length; i++)
        {
            int reallylocal = i;
            int tl = (letters.IndexOf(layerletters[reallylocal]) * LEDDat[layer]) % (letters.Length);
            buttons[i].OnInteract = delegate () { OnPress(letters[tl].Equals(layerletters[3 - reallylocal]),reallylocal); return true; };
        }
    }

    void OnPress(bool correctButton, int i)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if(!isActivated)
        {
            Debug.Log("Pressed button before it's been activated");
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            Debug.Log("Pressed " + correctButton + " button, correctIndex: " + correctIndex.ToString() + " button index: " + i.ToString());
            if (correctButton)
            {
                if (layer.Equals(LEDDat.Length-1))
                {
                    GetComponent<KMBombModule>().HandlePass();
                }
                else
                {
                    LoadLayer();
                }
            }
            else
            {
                GetComponent<KMBombModule>().HandleStrike();
            }
        }
    }
}
