using System.Collections.Generic;
using UnityEngine;

public class MorseDecoder : MonoBehaviour
{
    private Dictionary<string, string> morseMap;

    private void Awake()
    {
        morseMap = new Dictionary<string, string>
        {
            { ".-", "A" },
            { "-...", "B" },
            { "-.-.", "C" },
            { "-..", "D" },
            { ".", "E" },
            { "..-.", "F" },
            { "--.", "G" },
            { "....", "H" },
            { "..", "I" },
            { ".---", "J" },
            { "-.-", "K" },
            { ".-..", "L" },
            { "--", "M" },
            { "-.", "N" },
            { "---", "O" },
            { ".--.", "P" },
            { "--.-", "Q" },
            { ".-.", "R" },
            { "...", "S" },
            { "-", "T" },
            { "..-", "U" },
            { "...-", "V" },
            { ".--", "W" },
            { "-..-", "X" },
            { "-.--", "Y" },
            { "--..", "Z" }
        };
    }

    public bool TryDecode(string morse, out string result)
    {
        return morseMap.TryGetValue(morse, out result);
    }
}