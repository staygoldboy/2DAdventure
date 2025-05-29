using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("监听事件")]
    public SceneLoadEventSO sceneLoadEvent;
    public VoidEvent afterSceneLoadedEvent;
    public VoidEvent loadDataEvent;
    public VoidEvent backToMenuEvent;

    public PlayerInputControl inputControl;
    public Vector2 inputDirection;
    private Rigidbody2D Rb;
	private CapsuleCollider2D coll;
	private PhysicsCheck physicsCheck;
    private PlayerAnimation playerAnimation;
    private Character character;

    public float speed;
    public float jumpForce;
    public float wallJumpForce;
    public float hurtForce;
    public float slideDistance;
    public float slideSpeed;
    public int slidePowerCost;
	private Vector2 originalOffset;
	private Vector2 originalSize;

    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;



    [Header("状态")]
	public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool isWalk = false;
    public bool isCrouch;
    public bool wallJump;
    public bool isSlide;

    private void Awake()
    {
        inputControl = new PlayerInputControl();
        Rb = GetComponent<Rigidbody2D>();
        physicsCheck = GetComponent<PhysicsCheck>();
        playerAnimation = GetComponent<PlayerAnimation>();
        character = GetComponent<Character>();
        //跳跃
        inputControl.Gameplay.Jump.started += Jump;

		//强制蹲下，改变碰撞体
		coll = GetComponent<CapsuleCollider2D>();
		originalOffset = coll.offset;
		originalSize = coll.size;
		inputControl.Gameplay.Crouch.performed += ctx =>
		{
			isCrouch = true;    
		};
		inputControl.Gameplay.Crouch.canceled += ctx =>
		{
			isCrouch = false;
		};


		//强制走路
		inputControl.Gameplay.Walk.started += WalkStarted;
        inputControl.Gameplay.Walk.canceled += WalkCanceled;

        //攻击
        inputControl.Gameplay.Attack.started += PlayerAttack;

        //滑铲
        inputControl.Gameplay.Slide.started += Slide;

        inputControl.Enable();
    }


	private void OnEnable()
    {
        
        sceneLoadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;
    }

    private void OnDisable()
    {
        inputControl.Disable();
		sceneLoadEvent.LoadRequestEvent -= OnLoadEvent;
		afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
		loadDataEvent.OnEventRaised -= OnLoadDataEvent;
		backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
	}


	private void Update()
    {
        inputDirection=inputControl.Gameplay.Move.ReadValue<Vector2>();

        CheckState();
    }

    private void FixedUpdate()
    {
        if(!isHurt && !isAttack)
        {
            Move();
        }
        
    }


	//碰撞伤害

	//private void OnTriggerStay2D(Collider2D collision)
	//{
	//    Debug.Log(collision.name);
	//}

    //场景加载过程停止控制
	private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
	{
		inputControl.Gameplay.Disable();
	}

    //读取游戏进度
	private void OnLoadDataEvent()
	{
		isDead = false;
	}

	//场景加载完启动控制
	private void OnAfterSceneLoadedEvent()
	{
		inputControl.Gameplay.Enable();
	}

	public void Move()
    {
        if(!isCrouch && !wallJump)
        {
            Rb.velocity = new Vector2(inputDirection.x * speed * Time.deltaTime, Rb.velocity.y);
        }
            

        int faceDir = (int)transform.localScale.x;

        if (inputDirection.x > 0)
        {
            faceDir = 1;
        }
        else if (inputDirection.x < 0)
        {
            faceDir = -1;
        }
        //人物翻转
        transform.localScale = new Vector3(faceDir, 1, 1);

        if(isWalk==true)
        {
            Rb.velocity = new Vector2(speed/2 *Time.deltaTime*inputDirection.x, Rb.velocity.y);
        }

		//下蹲
		
		if (isCrouch)
		{
			// 修改碰撞体大小和位移
			coll.size = new Vector2(originalSize.x, originalSize.y - 0.3f);
			coll.offset = new Vector2(originalOffset.x, originalOffset.y - 0.15f);
		}
		else
		{
			// 还原之前碰撞体参数
			coll.size = originalSize;
			coll.offset = originalOffset;
		}

	}
    private void Jump(InputAction.CallbackContext context)
    {
        //Debug.Log("JUMP");
        if (physicsCheck.isGround)
        {
            Rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            GetComponent<AudioDefination>()?.PlayAudioClip();

            //打断滑铲协程
            isSlide = false;
            StopAllCoroutines();
        }
        else if(physicsCheck.onWall)
        {
            Rb.AddForce(new Vector2(-inputDirection.x, 2.5f) * wallJumpForce, ForceMode2D.Impulse);
            wallJump = true;
        }
        
        
    }

	//滑铲
	private void Slide(InputAction.CallbackContext context)
	{
        if (!isSlide && physicsCheck.isGround && character.currentPower >= slidePowerCost) 
        {
            isSlide = true;
            var targetPos = new Vector3(transform.position.x + slideDistance * transform.localScale.x, transform.position.y);

            gameObject.layer = LayerMask.NameToLayer("Enemy");
            StartCoroutine(TriggerSlide(targetPos));

            character.OnSlide(slidePowerCost);
        }
	}

    private IEnumerator TriggerSlide(Vector3 target)
    {
        do
        {
            yield return null;

            if(!physicsCheck.isGround)
            {
                break;
            }

            //滑动过程中撞墙
            if ((physicsCheck.touchLeftWall && transform.localScale.x < 0f) ||  (physicsCheck.touchRightWall && transform.localScale.x > 0f))
            {
                isSlide = false;
                break;
            }

            Rb.MovePosition(new Vector2(transform.position.x + transform.localScale.x * slideSpeed, transform.position.y));
        } while (MathF.Abs(target.x - transform.position.x) > 0.1f);

        isSlide = false;
		gameObject.layer = LayerMask.NameToLayer("Player");
	}



	private void PlayerAttack(InputAction.CallbackContext context)
	{
        playerAnimation.PlayerAttack();
        isAttack = true;
	}

	private void WalkCanceled(InputAction.CallbackContext context)
    {
        isWalk = false;
    }

    private void WalkStarted(InputAction.CallbackContext context)
    {
        isWalk = true;
    }



	public void GetHurt(Transform attacker)
    {
        isHurt = true;
        Rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attacker.position.x), 0).normalized;

        Rb.AddForce (dir * hurtForce, ForceMode2D.Impulse);
    }

    public void PlayerDead()
    {
        isDead = true;
        inputControl.Gameplay.Disable();
    }

    private void CheckState()
    {
        coll.sharedMaterial = physicsCheck.isGround ? normal : wall;

        if(physicsCheck.onWall)
        {
            Rb.velocity = new Vector2(Rb.velocity.x, Rb.velocity.y / 2f);
        }
        else
        {
            Rb.velocity = new Vector2(Rb.velocity.x, Rb.velocity.y);
        }

        if(wallJump && Rb.velocity.y < 0f)
        {
            wallJump = false;
        }
    }
	
}
