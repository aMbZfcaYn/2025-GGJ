using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ExpText : MonoBehaviour
{
    public Text text;
    public GameManager manager;
    public string textToDisplay = 0.ToString();
    void Start()
    {
        text = GetComponent<Text>();
    }


    void Update()
    {
        textToDisplay = manager.playerExp.ToString();
        text.text = textToDisplay;
    }
}
