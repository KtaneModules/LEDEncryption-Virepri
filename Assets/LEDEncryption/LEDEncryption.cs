using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class LEDEncryption : MonoBehaviour
{
    public TextMesh LayerText;
    public KMSelectable[] buttons;
    public GameObject[] LEDs;
    public Material[] mats;
    public Material off;

    int layer = 0;
    int[] layerMultipliers;
    bool isActivated = false;

    int moduleId;
    static int moduleIdCounter = 1;

    static string[] buttonNames = { "top-left", "top-right", "bottom-left", "bottom-right" };

    // Use this for initialization
    void Start()
    {
        moduleId = moduleIdCounter++;

        for (int i = 0; i < LEDs.Length; i++)
        {
            LEDs[i].GetComponent<MeshRenderer>().material = off;
        }

        layerMultipliers = new int[Random.Range(2, 6)];

        for (int i = 0; i < layerMultipliers.Length; i++)
        {
            LEDs[i].SetActive(true);
            layerMultipliers[i] = Random.Range(2, 8);
        }

        LoadLayer();
        GetComponent<KMBombModule>().OnActivate += ActivateModule;
    }

    void ActivateModule()
    {
        isActivated = true;
        for (int i = 0; i < layerMultipliers.Length; i++)
        {
            LEDs[i].GetComponent<MeshRenderer>().material = mats[layerMultipliers[i] - 2];
        }
    }

    static List<T> Shuffle<T>(List<T> list)
    {
        for (int j = list.Count; j >= 1; j--)
        {
            int item = Random.Range(0, j);
            if (item < j - 1)
            {
                var t = list[item];
                list[item] = list[j - 1];
                list[j - 1] = t;
            }
        }
        return list;
    }

    void LoadLayer()
    {
        LayerText.text = (layer + 1).ToString();

        // Choose which button will have the correct answer.
        var correctIndex = Random.Range(0, 4);
        var layerLetters = new int[4];

        retry:
        // Take all 26 letters of the alphabet (represented by the numbers 0–25) in random order.
        var letters = Shuffle(Enumerable.Range(0, 26).ToList());

        // Choose a random letter to display on the correct button.
        layerLetters[correctIndex] = letters[0];
        letters.RemoveAt(0);

        // Find out which letter needs to be diagonally opposite for this to be the correct answer.
        layerLetters[3 - correctIndex] = (layerLetters[correctIndex] * layerMultipliers[layer]) % 26;
        letters.Remove(layerLetters[3 - correctIndex]);

        // If that’s the same letter, try again.
        if (layerLetters[3 - correctIndex] == layerLetters[correctIndex])
            goto retry;

        // Put random letters on the other two buttons (these are necessarily different because we removed the other two from the list)
        layerLetters[correctIndex ^ 1] = letters[0];
        layerLetters[(3 - correctIndex) ^ 1] = letters[1];

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponentInChildren<TextMesh>().text = ((char) ('A' + layerLetters[i])).ToString();
            var isCorrect = (layerLetters[i] * layerMultipliers[layer]) % 26 == layerLetters[3 - i];
            var buttonName = buttonNames[i];
            buttons[i].OnInteract = delegate () { OnPress(isCorrect, buttonName); return false; };
        }

        Debug.LogFormat("[LED Encryption #{0}] Letters in layer {1} ({2}) are: {3}", moduleId, layer + 1, new[] { "red", "green", "blue", "yellow", "purple", "orange" }[layerMultipliers[layer] - 2], string.Join("", layerLetters.Select(ch => ((char) ('A' + ch)).ToString()).ToArray()));
        Debug.LogFormat("[LED Encryption #{0}] Correct button: {1}", moduleId, buttonNames[correctIndex]);
    }

    void OnPress(bool isCorrect, string buttonName)
    {
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        GetComponent<KMSelectable>().AddInteractionPunch();

        if (!isActivated)
        {
            Debug.LogFormat("[LED Encryption #{0}] Pressed button before module activated.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
        }
        else
        {
            //Do nothing more once module is solved.
            //If you wish for strikes to be caused on the incorrect buttons, move this to inside of the
            //if (isCorrect) block before the layer++ line.
            if (layer.Equals(layerMultipliers.Length)) return;

            Debug.LogFormat("[LED Encryption #{0}] Pressed {1} button, which is {2}.", moduleId, buttonName, isCorrect ? "correct" : "wrong");
            if (isCorrect)
            {
                layer++;
                if (layer.Equals(layerMultipliers.Length))
                {
                    Debug.LogFormat("[LED Encryption #{0}] Module solved.", moduleId);
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

    KMSelectable[] ProcessTwitchCommand(string command)
    {
        var commandList = command.ToLowerInvariant().Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries);
        var buttonLookup = new Dictionary<string, int>
        {
            {"topleft", 0}, {"lefttop", 0}, {"tl", 0}, {"lt", 0}, {"0", 0},
            {buttons[0].GetComponentInChildren<TextMesh>().text.ToLowerInvariant(), 0},

            {"topright", 1}, {"righttop", 1}, {"tr", 1}, {"rt", 1}, {"1", 1},
            {buttons[1].GetComponentInChildren<TextMesh>().text.ToLowerInvariant(), 1},

            {"bottomleft", 2}, {"leftbottom", 2}, {"bl", 2}, {"lb", 2}, {"2", 2},
            {buttons[2].GetComponentInChildren<TextMesh>().text.ToLowerInvariant(), 2},

            {"bottomright", 3}, {"rightbottom", 3}, {"br", 3}, {"rb", 3}, {"3", 3},
            {buttons[3].GetComponentInChildren<TextMesh>().text.ToLowerInvariant(), 3},
        };
        if (commandList[0] != "press" || commandList.Length != 2 || !buttonLookup.ContainsKey(commandList[1]))
            return null;

        return new[] {buttons[buttonLookup[commandList[1]]]};

    }
}
