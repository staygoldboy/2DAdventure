using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rb;
    [HideInInspector]public Animator anim;
	[HideInInspector]public PhysicsCheck physicsCheck;
    [Header("基本参数")]

    public float normalSpeed;
    public float chaseSpeed;
    public float currentSpeed;
    public Vector3 faceDir;
	public Transform attacker;
	public float hurtForce;

	[Header("检测")]

	public Vector2 centerOffset;
	public Vector2 checkSize;
	public float checkDistance;
	public LayerMask attackLayer;

	[Header("计时器")]
	public float waitTime;
	public float waitTimeCounter;
	public bool wait;
	public float lostTime;
	public float lostTimeCounter;

	[Header("状态")]
	public bool isHurt;
	public bool isDead;

	public BaseState currentState;
	public BaseState patrolState;
	public BaseState chaseState;

	protected virtual void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
		physicsCheck = GetComponent<PhysicsCheck>();
		currentSpeed = normalSpeed;
		//waitTimeCounter = waitTime;
	}

	private void OnEnable()
	{
		currentState = patrolState;
		currentState.OnEnter(this);
	}

	private void Update()
	{
        faceDir = new Vector3(-transform.localScale.x, 0, 0);
		
		currentState.LogicUpdate();
		TimeCounter();
	}

	private void FixedUpdate()
	{
		if(!isHurt && !isDead && !wait )
		{
			Move();
		}
		currentState.physicsUpdate();
		
	}

	private void OnDisable()
	{
		currentState.OnExit();
	}

	public virtual void Move()
    {
        rb.velocity = new Vector2(currentSpeed * faceDir.x * Time.deltaTime, rb.velocity.y);
    }

	public void TimeCounter()
	{
		if (wait)
		{
			waitTimeCounter -= Time.deltaTime;
			if (waitTimeCounter <= 0)
			{
				wait = false;
				waitTimeCounter = waitTime;
				transform.localScale = new Vector3(faceDir.x, 1, 1);
			}
		}
		if(!FoundPlayer() && lostTimeCounter > 0)
		{
			lostTimeCounter -= Time.deltaTime;

		}
		//else
		//{
		//	lostTimeCounter = lostTime;
		//}
	}

	public bool FoundPlayer()
	{
		return Physics2D.BoxCast(transform.position + (Vector3)centerOffset, checkSize, 0, faceDir, checkDistance, attackLayer);
	}

	public void SwitchState(NPCstate state)
	{
		var newState = state switch
		{
			NPCstate.Patrol => patrolState,
			NPCstate.Chase => chaseState,
			_ => null
		};

		currentState.OnExit();
		currentState = newState;
		currentState.OnEnter(this);
	}

	#region 事件执行
	public void OnTakeDamage(Transform attackTrans)
	{
		attacker = attackTrans;
		//转身
		if(attackTrans.position.x - transform.position.x > 0)
		{
			transform.localScale = new Vector3(-1, 1, 1);

		}
		if (attackTrans.position.x - transform.position.x < 0)
		{
			transform.localScale = new Vector3(1, 1, 1);

		}
		//受伤被击退
		isHurt = true;
		anim.SetTrigger("hurt");

		Vector2 dir = new Vector2((transform.position.x - attackTrans.position.x), 0).normalized;
		rb.velocity = new Vector2(0, rb.velocity.y);
		StartCoroutine(OnHurt(dir));
	}

	private IEnumerator OnHurt(Vector2 dir)
	{
		rb.AddForce(dir *hurtForce, ForceMode2D.Impulse);
		yield return new WaitForSeconds(0.7f);
		isHurt = false;

	}

	public void onDie()
	{
		gameObject.layer = 2;
		anim.SetBool("dead",true);
		isDead = true;
	}

	public void DestroyAfterAnimation()
	{
		Destroy(this.gameObject);
	}
	#endregion

	private void OnDrawGizmos()
	{
		Gizmos.DrawCube((Vector2)transform.position + centerOffset + new Vector2(checkDistance*-transform.localScale.x,0), checkSize);
	}
}
