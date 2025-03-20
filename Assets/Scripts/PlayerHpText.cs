using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpText : MonoBehaviour
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

        textToDisplay = manager.playerHp.ToString();
        text.text = textToDisplay;
    }
}
