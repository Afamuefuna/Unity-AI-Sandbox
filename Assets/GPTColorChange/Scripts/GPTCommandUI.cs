using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GPTCommandUI : MonoBehaviour
{
    [Header("GUI Elements")]
    [SerializeField]
    private TMP_InputField inputField;
    
    [Header("Dependencies")]
    [SerializeField]
    private GPTCommand command;
    
    void Start()
    {
        inputField.onEndEdit.AddListener(command.SendPromptToChatGPT);
    }
}
