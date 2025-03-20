using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverText : MonoBehaviour
{
    public Text text;
    public GameManager manager;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = manager.playerExp.ToString();
    }
}
