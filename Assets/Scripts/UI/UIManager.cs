using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
	public PlayerStatBar playerStatBar;
    [Header("事件监听")]
    public CharacterEventSO healthEvent;
	public SceneLoadEventSO unloadedSceneEvent;
	public VoidEvent loadDataEvent;
	public VoidEvent gameOverEvent;
	public VoidEvent backToMenuEvent;
	public FloatEventSO syncVolumeEvent;

	[Header("事件广播")]
	public VoidEvent pauseEvent;

	[Header("组件")]
	public GameObject gameOverPanel;
	public GameObject restartBtn;
	public GameObject mobileTouch;
	public Button settingBtn;
	public GameObject pausePanel;
	public Slider volumeSlider;

	private void Awake()
	{
		#if UNITY_STANDALONE
		mobileTouch.SetActive(false);
		#endif
		settingBtn.onClick.AddListener(TogglePausePanel);
	}

	private void OnEnable()
	{
		healthEvent.OnEventRaised += OnHealthEvent;
		unloadedSceneEvent.LoadRequestEvent += OnUnloadedSceneEvent;
		loadDataEvent.OnEventRaised += OnLoadDataEvent;
		gameOverEvent.OnEventRaised += OnGameOverEvent;
		backToMenuEvent.OnEventRaised += OnLoadDataEvent;
		syncVolumeEvent.OnEventRaised += OnSyncVolumeEvent;
	}

	private void OnDisable()
	{
		healthEvent.OnEventRaised -= OnHealthEvent;
		unloadedSceneEvent.LoadRequestEvent -= OnUnloadedSceneEvent;
		loadDataEvent.OnEventRaised -= OnLoadDataEvent;
		gameOverEvent.OnEventRaised -= OnGameOverEvent;
		backToMenuEvent.OnEventRaised -= OnLoadDataEvent;
		syncVolumeEvent.OnEventRaised -= OnSyncVolumeEvent;
	}

	private void OnSyncVolumeEvent(float amount)
	{
		volumeSlider.value = (amount + 80) / 100;
	}

	private void TogglePausePanel()
	{
		if(pausePanel.activeInHierarchy)
		{
			pausePanel.SetActive(false);
			Time.timeScale = 1.0f;
		}
		else
		{
			pauseEvent.RaiseEvent();
			pausePanel.SetActive(true);
			Time.timeScale = 0f;
		}
	}

	private void OnGameOverEvent()
	{
		gameOverPanel.SetActive(true);
		EventSystem.current.SetSelectedGameObject(restartBtn);
	}

	private void OnLoadDataEvent()
	{
		gameOverPanel.SetActive(false);
	}

	private void OnUnloadedSceneEvent(GameSceneSO SceneToLoad, Vector3 arg1, bool arg2)
	{
		var isMenu = (SceneToLoad.sceneType == SceneType.Menu);
		playerStatBar.gameObject.SetActive(!isMenu);
		
	}

	private void OnHealthEvent(Character character)
	{
		var persentage = character.currentHealth / character.maxHealth;
		playerStatBar.OnHealthChange(persentage);
		playerStatBar.OnPowerChange(character);
	}
}
