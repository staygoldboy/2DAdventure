using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour, ISaveable
{
	[Header("�¼�����")]
	public VoidEvent newGameEvent;

	[Header("��������")]
	public float maxHealth;
	public float currentHealth;
	public float maxPower;
	public float currentPower;
	public float powerRecoverSpeed;

	[Header("����")]
	public float invulnerableDuration;
	private float invulnerableCounter;
	public bool invulnerable;

	public UnityEvent<Character> OnHealthChange;
	public UnityEvent<Transform> onTakeDamage;
	public UnityEvent onDie;

	private void NewGame()
	{
		currentHealth = maxHealth;
		currentPower = maxPower;
		OnHealthChange?.Invoke(this);
	}

	private void OnEnable()
	{
		newGameEvent.OnEventRaised += NewGame;
		ISaveable saveable = this;
		saveable.RegisterSaveData();
	}

	private void OnDisable()
	{
		newGameEvent.OnEventRaised -= NewGame;
		ISaveable saveable = this;
		saveable.UnRegisterSaveData();
	}

	private void Update()
	{
		if (invulnerable)
		{
			invulnerableCounter -= Time.deltaTime;
			if (invulnerableCounter <=0)
			{
				invulnerable = false;
			}
		}

		if(currentPower < maxPower)
		{
			currentPower += Time.deltaTime * powerRecoverSpeed;
		}

	}

	private void OnTriggerStay2D(Collider2D collision)
	{
		if(collision.CompareTag("Water"))
		{
			if(currentHealth > 0)
			{
				//����������Ѫ��
				currentHealth = 0;
				OnHealthChange?.Invoke(this);
				onDie?.Invoke();
			}
			
		}
	}

	public void TakeDamage(Attack attacker)
	{
		if (invulnerable)
		{
			return;
		}
		//Debug.Log(attacker.damage);
		if(currentHealth -attacker.damage > 0)
		{
			currentHealth -= attacker.damage;

			TriggerInvulnerable();
			//ִ������
			onTakeDamage?.Invoke(attacker.transform);
		}
		else
		{
			currentHealth = 0;
			//��������
			onDie?.Invoke();
		}

		OnHealthChange?.Invoke(this);		
	}

	public void TriggerInvulnerable()
	{
		if (!invulnerable)
		{
			invulnerable = true;
			invulnerableCounter = invulnerableDuration;
		}
	}

	public void OnSlide(int cost)
	{
		currentPower -= cost;
		OnHealthChange?.Invoke(this);
	}


	public DataDefinition GetDataID()
	{
		return GetComponent<DataDefinition>();
	}

	public void GetSaveData(Data data)
	{
		if(data.characterPosDict.ContainsKey(GetDataID().ID))
		{
			data.characterPosDict[GetDataID().ID] = new SerializeVector3(transform.position);
			data.floatSaveData[GetDataID().ID + "health"] = this.currentHealth;
			data.floatSaveData[GetDataID().ID + "power"] = this.currentPower;
		}
		else
		{
			data.characterPosDict.Add(GetDataID().ID, new SerializeVector3(transform.position));
			data.floatSaveData.Add(GetDataID().ID + "health", this.currentHealth);
			data.floatSaveData.Add(GetDataID().ID + "power", this.currentPower);
		}
	}

	public void LoadData(Data data)
	{
		var def = GetDataID();
		if (def == null)
		{
			Debug.LogError($"{name} ��û�й��� DataDefinition��");
			return;
		}
		if (data.characterPosDict.ContainsKey(GetDataID().ID))
		{
			transform.position = data.characterPosDict[GetDataID().ID].ToVector3();
			this.currentHealth = data.floatSaveData[GetDataID().ID + "health"];
			this.currentPower = data.floatSaveData[GetDataID().ID + "power"];

			//֪ͨUI������
			OnHealthChange?.Invoke(this);
		}
	}
}
