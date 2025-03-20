using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    CircleCollider2D circleCollider;
    Animator anim;

    public float moveSpeed = 1f;
    public float changeInterval = 5f;
    public float randomMoveTimer = 0f;

    [SerializeField]private float selfLifeTime = 5f;
    private float selfTimer = 0f;
    public bool blow = false;

    private Vector3 targetPos;

    public int hp;
    public int exp = 5;
    public int damage = 5;

    public bool isDead = false;
    public bool mouseInRange = false;
    public bool isTargeted = false;

    public bool affectedByVortex = false;
    public bool affectedByBig = false;
    private bool changedToBig = false;

    void Start()
    {
        Transform transform = GetComponent<Transform>();
        circleCollider = transform.GetComponent<CircleCollider2D>();
        anim = GetComponent<Animator>();

        targetPos = GenerateRandomPosition();
    }


    void Update()
    {
        randomMoveTimer += Time.deltaTime;
        selfTimer += Time.deltaTime;
        if( selfTimer > selfLifeTime - 2f )
        {
            anim.SetBool( "BlowSoon",true );
        }
        if( selfTimer > selfLifeTime && !blow)
        {
            blow = true;
        }

        if(affectedByVortex)
        {
            targetPos = new Vector2( 0,0 );
            transform.position = Vector3.MoveTowards( transform.position,targetPos,moveSpeed * Time.deltaTime );
        }
        else
            RandomMove();
        
        if(affectedByBig && !changedToBig)
        {
            float bigScale = 1.5f;
            transform.localScale *= bigScale;
            changedToBig = true;
        }
    }

    private void RandomMove ()
    {
        if( randomMoveTimer > changeInterval )
        {
            targetPos = GenerateRandomPosition();
            randomMoveTimer = 0f;
        }

        transform.position = Vector3.MoveTowards( transform.position,targetPos,moveSpeed * Time.deltaTime );
    }

    private Vector3 GenerateRandomPosition()
    {
        Camera cam = Camera.main;
        float randomX = Random.Range( 0.3f,0.7f );
        float randomY = Random.Range( 0.0f,1.0f );
        Vector3 newPos = cam.ViewportToWorldPoint( new Vector3( randomX,randomY,cam.nearClipPlane ) );
        return newPos;
    }

    public void Delete()
    {
        isDead = true;
    }
}
