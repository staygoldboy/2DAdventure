using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SavePoint : MonoBehaviour, IInteractable
{
	[Header("广播")]
	public VoidEvent saveDataEvent;

	[Header("参数")]
    public SpriteRenderer spriteRenderer;
	public GameObject lightObj;
    public Sprite darkSprite;
    public Sprite lightSprite;
	public bool isDone;

	private void OnEnable()
	{
		spriteRenderer.sprite = isDone ? lightSprite : darkSprite;
		lightObj.SetActive(isDone);
	}

	public void TriggerAction()
	{
		if(!isDone)
		{
			isDone = true;
			spriteRenderer.sprite = lightSprite;
			lightObj.SetActive(true);
			//TODO：保存数据
			saveDataEvent.RaiseEvent();

			this.gameObject.tag = "Untagged";
		}
	}
}
