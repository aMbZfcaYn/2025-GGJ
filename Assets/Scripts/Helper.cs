using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Helper : MonoBehaviour
{
    public Vector3 targetPos;

    [SerializeField]private float moveSpeed = 10f;

    void Start()
    {
        Transform transform = GetComponent<Transform>();
    }

    void Update()
    {
        if (targetPos != null) 
            transform.position = Vector3.MoveTowards( transform.position,targetPos,moveSpeed * Time.deltaTime );
    }
}
