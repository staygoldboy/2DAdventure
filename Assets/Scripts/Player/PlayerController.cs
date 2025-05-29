using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("�����¼�")]
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

    [Header("�������")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;



    [Header("״̬")]
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
        //��Ծ
        inputControl.Gameplay.Jump.started += Jump;

		//ǿ�ƶ��£��ı���ײ��
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


		//ǿ����·
		inputControl.Gameplay.Walk.started += WalkStarted;
        inputControl.Gameplay.Walk.canceled += WalkCanceled;

        //����
        inputControl.Gameplay.Attack.started += PlayerAttack;

        //����
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


	//��ײ�˺�

	//private void OnTriggerStay2D(Collider2D collision)
	//{
	//    Debug.Log(collision.name);
	//}

    //�������ع���ֹͣ����
	private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2)
	{
		inputControl.Gameplay.Disable();
	}

    //��ȡ��Ϸ����
	private void OnLoadDataEvent()
	{
		isDead = false;
	}

	//������������������
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
        //���﷭ת
        transform.localScale = new Vector3(faceDir, 1, 1);

        if(isWalk==true)
        {
            Rb.velocity = new Vector2(speed/2 *Time.deltaTime*inputDirection.x, Rb.velocity.y);
        }

		//�¶�
		
		if (isCrouch)
		{
			// �޸���ײ���С��λ��
			coll.size = new Vector2(originalSize.x, originalSize.y - 0.3f);
			coll.offset = new Vector2(originalOffset.x, originalOffset.y - 0.15f);
		}
		else
		{
			// ��ԭ֮ǰ��ײ�����
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

            //��ϻ���Э��
            isSlide = false;
            StopAllCoroutines();
        }
        else if(physicsCheck.onWall)
        {
            Rb.AddForce(new Vector2(-inputDirection.x, 2.5f) * wallJumpForce, ForceMode2D.Impulse);
            wallJump = true;
        }
        
        
    }

	//����
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

            //����������ײǽ
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
