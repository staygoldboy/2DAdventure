using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

public class Sign : MonoBehaviour
{
	private Animator anim;
	private PlayerInputControl playerInput;
	public Transform playerTrans;
	public GameObject signSprite;
	private IInteractable targetItem;
	private bool canPress;

	private void Awake()
	{
		anim = signSprite.GetComponent<Animator>();
		playerInput = new PlayerInputControl();
		playerInput.Enable();
	}

	private void OnEnable()
	{
		InputSystem.onActionChange += OnActionChange;
		playerInput.Gameplay.Confirm.started += OnConfirm;
	}

	private void OnDisable()
	{
		canPress = false;
	}

	private void OnConfirm(InputAction.CallbackContext context)
	{
		if(canPress)
		{
			targetItem.TriggerAction();
			GetComponent<AudioDefination>()?.PlayAudioClip();
		}
	}

	private void OnActionChange(object obj, InputActionChange actionChange)
	{
		if(actionChange == InputActionChange.ActionStarted)
		{
			var d = ((InputAction)obj).activeControl.device;

			switch(d.device)
			{
				case Keyboard:
					anim.Play("Keyboard");
					break;
				case DualShockGamepad:
					anim.Play("ps");
					break;
			}
		}
	}

	private void Update()
	{
		signSprite.GetComponent<SpriteRenderer>().enabled = canPress;
		signSprite.transform.localScale = playerTrans.localScale;
	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if(collision.CompareTag("Interactable"))
		{
			canPress = true;
			targetItem = collision.GetComponent<IInteractable>();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		canPress = false;
	}

}
