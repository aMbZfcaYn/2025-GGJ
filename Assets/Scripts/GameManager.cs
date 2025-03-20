using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameObject prefabBubble;
    public GameObject prefabBoss;
    public GameObject prefabPotato;
    public Camera cam;
    public ExpText expText;
    public GameObject helper;
    public GameObject potato;
    private AudioSource audioSource;

    public GameObject pauseMenuUI; // 暂停菜单的UI
    public GameObject getAPowerMenu;
    public GameObject APowerIcon;
    public GameObject choosePowerMenu_1;
    public GameObject choosePowerMenu_2;
    public GameObject timePowerIcon;
    public GameObject vortexPowerIcon;
    public GameObject tntPowerIcon;
    public GameObject potatoPowerIcon;
    public GameObject helperPowerIcon;
    public GameObject bigPowerIcon;
    public GameObject gameoverMenu;

    private bool isPaused = false;
    private bool isInAPowerMenu = false;
    private bool isInBPowerMenu = false;
    private bool BPowerGot_1 = false;
    private bool BPowerGot_2 = false;
    private bool gameover = false;

    public int bubbleNum = 0;
    public List<GameObject> objects = new List<GameObject>();
    public List<bool> objectsFlag = new List<bool>();

    private Vector2 mousePos;
    private Vector2 lastMousePos;

    public float addTime = 1.5f;
    private float addBubbleTimer = 0f;

    private bool canFreeze = false;
    [SerializeField] private float freezeCD = 10f;
    [SerializeField] private float freezeTime = 3f;
    private float freezeCDTimer = 0;
    private float freezeTimer = 0;
    private bool isFreezing = false;

    private bool canVortex = false;

    private bool canClear = false;
    [SerializeField] private float clearCD = 30f;
    private float clearCDTimer = 0;

    private bool canHelper = false;

    private bool canPotato = false;
    private bool havePotato = false;

    private bool canBig = false;

    public int playerHp = 100;
    public int playerExp = 0;

    public bool playerClickAbility = true;
    public bool playerCutAbility = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        
        Vector3[] positions = new Vector3[]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 1, 0),
                new Vector3(2, 2, 0),
                new Vector3(3, 3, 0)
            };
        
            foreach( Vector3 position in positions )
            {
                GameObject obj = Instantiate( prefabBubble,position,Quaternion.identity );
                objects.Add( obj );
                objectsFlag.Add( true );
                bubbleNum += 1;
            }
    }
    void Update()
    {
        CheckPause();
        CheckAPowerMenu();
        CheckGameOver();

        if( isPaused || isInAPowerMenu || isInBPowerMenu || gameover ) return;

        mousePos = Input.mousePosition;
        lastMousePos = cam.ScreenToWorldPoint( mousePos );

        AddMethod();
        UpdateList();

        PotatoAbility();
        BigAbility();
        HelperAbility();
        FreezeAbility();
        VortexAbility();
        ClearAbility();

        if( playerClickAbility ) ClickMethod();
        if( playerCutAbility ) CutMethod();

        CheckDelete();
        CheckAbilityGet();
    }


    private void UpdateList ()
    {
        List<GameObject> newObjects = new List<GameObject>();
        List<bool> newObjectsFlag = new List<bool>();

        int newNum = 0;
        for( int i = 0 ; i < bubbleNum ; i++ )
        {
            if( objectsFlag[i] )
            {
                newObjects.Add( objects[i] );
                newObjectsFlag.Add( true );
                newNum += 1;
            }
            else
            {
                DestroyImmediate( objects[i] );
            }
        }
        objects = newObjects;
        objectsFlag = newObjectsFlag;
        bubbleNum = newNum;
    }
    
    private void CheckGameOver ()
    {
        if( playerHp <= 0 )
        {
            Time.timeScale = 0f;
            gameoverMenu.SetActive( true );
            gameover = true;
        }
        if( gameover && Input.GetKeyDown( KeyCode.R ) )
        {
            // 获取当前场景的名称
            string currentSceneName = SceneManager.GetActiveScene().name;

            // 重新加载当前场景
            SceneManager.LoadScene( currentSceneName );
            Time.timeScale = 1;
        }
    }
    
    private void CheckDelete ()
    {
        for( int i = 0 ; i < bubbleNum ; i++ )
        {
            GameObject obj = objects[i];
            Bubble bubble = obj.GetComponent<Bubble>();
            if( bubble.isDead )
            {
                if( bubble.blow ) playerHp -= bubble.damage;
                objectsFlag[i] = false;
            }
        }
    }
    
    private void PotatoAbility ()
    {
        if( canPotato )
        {
            if( Input.GetKeyDown( KeyCode.E ) )
            {
                if( !havePotato )
                {
                    potato = Instantiate( prefabPotato,cam.ScreenToWorldPoint( new Vector3( mousePos.x,mousePos.y,cam.nearClipPlane ) ),Quaternion.identity );
                    havePotato = true;
                }
                else
                {
                    Vector3 potatoPos = potato.GetComponent<Transform>().position;
                    CircleCollider2D potatoCollider = potato.GetComponent<CircleCollider2D>();
                    for( int i = 0 ; i < bubbleNum ; i++ )
                    {
                        GameObject obj = objects[i];
                        Bubble bubble = obj.GetComponent<Bubble>();
                        CircleCollider2D circleCollider = obj.GetComponent<CircleCollider2D>();
                        Vector3 bubblePos = bubble.transform.position;
                        if( Vector2.Distance( potatoPos,bubblePos ) < potatoCollider.radius + circleCollider.radius )
                        {
                            playerExp += bubble.exp;
                            obj.GetComponent<Animator>().SetBool( "Blow",true );
                            audioSource.Play();
                        }
                    }

                    DestroyImmediate( potato );
                    havePotato = false;
                }
            }
        }
    }
    
    private void BigAbility ()
    {
        if( canBig )
        {
            for( int i = 0 ; i < bubbleNum ; i++ )
            {
                GameObject obj = objects[i];
                Bubble bubble = obj.GetComponent<Bubble>();
                bubble.affectedByBig = true;
            }
        }
    }
    
    private void HelperAbility ()
    {
        if( canHelper )
        {
            GameObject target = null;
            helper.SetActive( true );
            Helper help = helper.GetComponent<Helper>();
            CircleCollider2D helperCollider = help.GetComponent<CircleCollider2D>();

            bool setTar = false;
            for( int i = 0 ; i < bubbleNum ; i++ )
            {
                GameObject obj = objects[i];
                Bubble bubble = obj.GetComponent<Bubble>();

                if( bubble.isTargeted )
                {
                    help.targetPos = bubble.transform.position;
                    setTar = true;
                    target = obj;
                    break;
                }
            }

            if( !setTar )
            {
                for( int i = 0 ; i < bubbleNum ; i++ )
                {
                    GameObject obj = objects[i];
                    Bubble bubble = obj.GetComponent<Bubble>();
                    if( bubble.damage == 10 )//是boss泡泡
                    {
                        bubble.isTargeted = true;
                        help.targetPos = bubble.transform.position;
                        target = obj;
                        break;
                    }
                }
            }

            if(target != null)
            {
                CircleCollider2D targetCollider = target.GetComponent<CircleCollider2D>();
            if( Vector2.Distance( target.transform.position,help.transform.position ) < helperCollider.radius + targetCollider.radius )
                {
                    playerExp += target.GetComponent<Bubble>().exp;
                    target.GetComponent<Animator>().SetBool( "Blow",true );
                    audioSource.Play();
                }
            }
        }
    }
    
    private void ClearAbility ()
    {
        clearCDTimer += Time.deltaTime;
        if( canClear )
        {
            if( clearCDTimer > 0 && Input.GetKeyDown( KeyCode.W ) )
            {
                clearCDTimer = -clearCD;
                for(int i = 0 ;i<bubbleNum ;i++ )
                {
                    GameObject obj = objects[i];
                    Bubble bubble = obj.GetComponent<Bubble>();
                    playerExp += bubble.exp;
                    obj.GetComponent<Animator>().SetBool( "Blow",true );
                    audioSource.Play();
                }
            }
        }
    }

    private void VortexAbility ()
    {
        if( canVortex )
        {
            for( int i = 0 ; i < bubbleNum ; i++ )
            {
                GameObject obj = objects[i];
                Bubble bubble = obj.GetComponent<Bubble>();
                bubble.affectedByVortex = true;
            }
        }
    }

    private void FreezeAbility ()
    {
        if( isFreezing )
        {
            freezeTimer += Time.deltaTime * 10;
            if( freezeTimer > freezeTime )
            {
                freezeTimer = 0;
                isFreezing = false;
                Time.timeScale = 1;
                freezeCDTimer = -freezeCD;
            }
        }

        freezeCDTimer += Time.deltaTime;
        if( freezeCDTimer > 0 && canFreeze )
        {
            if( Input.GetKeyDown( KeyCode.Q ) )
            {
                Time.timeScale = .1f;
                isFreezing = true;
            }
        }
    }

    private void CheckAbilityGet ()
    {
        if( playerExp >= 100 && !playerCutAbility )
        {
            playerCutAbility = true;
            playerClickAbility = false;
            Time.timeScale = 0;
            getAPowerMenu.SetActive( true );
            isInAPowerMenu = true;
            APowerIcon.SetActive( true );
            addTime -= 0.3f;
        }

        if(playerExp >= 200 && !BPowerGot_1)
        {
            choosePowerMenu_1.SetActive( true );
            isInBPowerMenu = true;
            Time.timeScale = 0;
            BPowerGot_1 = true;
            addTime -= 0.2f;
        }

        if( playerExp >= 300 && !BPowerGot_2 )
        {
            choosePowerMenu_2.SetActive( true );
            isInBPowerMenu = true;
            Time.timeScale = 0;
            BPowerGot_2 = true;
            addTime -= 0.2f;
        }

    }

    private void CheckAPowerMenu ()
    {
        if( Input.GetKeyDown( KeyCode.Space ) && isInAPowerMenu )
        {
            Time.timeScale = 1;
            getAPowerMenu.SetActive( false );
            isInAPowerMenu = false;
        }
    }

    private void CheckPause ()
    {
        if( Input.GetKeyDown( KeyCode.Escape ) )
        {
            if( !isPaused )
            {
                Time.timeScale = 0;
                pauseMenuUI.SetActive( true );
                isPaused = true;
            }
            else
            {
                Time.timeScale = 1;
                pauseMenuUI.SetActive( false );
                isPaused = false;
            }
        }
    }

    private void AddMethod ()
    {
        addBubbleTimer += Time.deltaTime;
        if( addBubbleTimer > addTime )
        {
            addBubbleTimer = 0f;
            GameObject prefab = GetNewbubble();
            AddBubble( prefab );
        }
    }

    private void ClickMethod ()
    {
        Vector3 WorldPos = cam.ScreenToWorldPoint( new Vector3( mousePos.x,mousePos.y,0 ) );

        for( int i = 0 ; i < bubbleNum ; i++ )
        {
            GameObject obj = objects[i];
            CircleCollider2D circleCollider = obj.GetComponent<CircleCollider2D>();
            Bubble bubble = obj.GetComponent<Bubble>();  

            float dis = Vector2.Distance( WorldPos,obj.transform.position );
            float radius = circleCollider.bounds.size.x / 2;

            if( bubble.blow )
            {
                obj.GetComponent<Animator>().SetBool( "Blow",true );
                audioSource.Play();
            }

            if( radius > dis && Input.GetKeyDown( KeyCode.Mouse0 ) )
            {
                bubble.hp -= 1;
                if( bubble.hp == 0 )
                {
                    playerExp += bubble.exp;
                    obj.GetComponent<Animator>().SetBool( "Blow",true );
                    audioSource.Play();
                }
            }
        }
    }

    private void CutMethod ()
    {
        Vector2 worldPos = cam.ScreenToWorldPoint( new Vector2( mousePos.x,mousePos.y ) ); // 当前鼠标的世界位置

        RaycastHit2D hit = Physics2D.Linecast( lastMousePos,worldPos );
        Vector2 hitpoint = hit.point;

        for( int i = 0 ; i < bubbleNum ; i++ )
        {
            GameObject obj = objects[i];
            CircleCollider2D circleCollider = obj.GetComponent<CircleCollider2D>();
            Bubble bubble = obj.GetComponent<Bubble>();

            if( circleCollider.OverlapPoint(worldPos) && !bubble.mouseInRange )
            {
                bubble.hp -= 1;
                
                if( bubble.hp == 0 )
                {
                    playerExp += bubble.exp;
                    obj.GetComponent<Animator>().SetBool( "Blow",true );
                    audioSource.Play();
                }
            }

            if( bubble.blow )
            {
                obj.GetComponent<Animator>().SetBool( "Blow",true );
                audioSource.Play();
            }

            obj.GetComponent<Bubble>().mouseInRange = circleCollider.OverlapPoint(worldPos);
        }
    }

    private void AddBubble (GameObject prefab)
    {
        //Debug.Log( "Add new one" );
        float randomX = Random.Range( 0.3f,0.7f );
        float randomY = Random.Range( 0.0f,1.0f );

        Vector3 randomPosition = cam.ViewportToWorldPoint( new Vector3( randomX,randomY,cam.nearClipPlane ) );

        GameObject newObj = Instantiate( prefab,randomPosition,Quaternion.identity );
        objects.Add( newObj );
        objectsFlag.Add( true );
        bubbleNum += 1;
    }

    private GameObject GetNewbubble()
    {
        int bossBubble = 10;

        int random = Random.Range( 1,100 );

        if( random < bossBubble )
            return prefabBoss;
        else
            return prefabBubble;
    }

    public void GetClear()
    {
        canClear = true;
        tntPowerIcon.SetActive( true );
        chooseMenu_1_Quit();
    }

    public void GetVortex()
    {
        canVortex = true;
        vortexPowerIcon.SetActive( true );
        chooseMenu_1_Quit();
    }

    public void GetFreeze()
    {
        canFreeze = true;
        timePowerIcon.SetActive( true );
        chooseMenu_1_Quit();
    }    
    private void chooseMenu_1_Quit ()
    {
        choosePowerMenu_1.SetActive( false );
        isInBPowerMenu = false;
        Time.timeScale = 1;
    }

    public void GetHelper ()
    {
        canHelper = true;
        helperPowerIcon.SetActive( true );
        chooseMenu_2_Quit();
    }

    public void GetPotato ()
    {
        canPotato = true;
        potatoPowerIcon.SetActive( true );
        chooseMenu_2_Quit();
    }

    public void GetBig ()
    {
        canBig = true;
        bigPowerIcon.SetActive( true );
        chooseMenu_2_Quit();
    }
    private void chooseMenu_2_Quit ()
    {
        choosePowerMenu_2.SetActive( false );
        isInBPowerMenu = false;
        Time.timeScale = 1;
    }
}
