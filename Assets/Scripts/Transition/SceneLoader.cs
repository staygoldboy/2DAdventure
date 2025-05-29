using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour, ISaveable
{
	public Transform playerTrans;
	public Vector3 firstPosition;
	public Vector3 menuPosition;

	[Header("�¼�����")]
	public SceneLoadEventSO loadEventSO;
	public VoidEvent NewGameEvent;
	public VoidEvent backToMenuEvent;

	[Header("�㲥")]
	public VoidEvent afterSceneLoadedEvent;
	public FadeEventSO fadeEvent;
	public SceneLoadEventSO sceneUnLoadedEvent;

	[Header("����")]
	public GameSceneSO firstLoadScene;
	public GameSceneSO menuScene;
	public GameSceneSO currentLoadedScene;
	private GameSceneSO sceneToLoad;
	private Vector3 positionToGo;
	private bool fadeScreen;
	private bool isLoading;
	public float fadeDuration;

	private void Awake()
	{
		//Addressables.LoadSceneAsync(firstLoadScene.sceneReference, LoadSceneMode.Additive);
		//currentLoadedScene = firstLoadScene;
		//currentLoadedScene.sceneReference.LoadSceneAsync(LoadSceneMode.Additive);
		
	}


	private void Start()
	{
		//NewGame();
		loadEventSO.RaiseLoadRequestEvent(menuScene, menuPosition, true);
	}

	private void OnEnable()
	{
		loadEventSO.LoadRequestEvent += OnLoadRequestEvent;
		NewGameEvent.OnEventRaised += NewGame;
		backToMenuEvent.OnEventRaised += OnBackToMenuEvent;

		ISaveable saveable = this;
		saveable.RegisterSaveData();
	}
	private void OnDisable()
	{
		loadEventSO.LoadRequestEvent -= OnLoadRequestEvent;
		NewGameEvent.OnEventRaised -= NewGame;
		backToMenuEvent.OnEventRaised -= OnBackToMenuEvent;
		ISaveable saveable = this;
		saveable.UnRegisterSaveData();
	}

	private void OnBackToMenuEvent()
	{
		sceneToLoad = menuScene;
		loadEventSO.RaiseLoadRequestEvent(sceneToLoad, menuPosition, true);
	}

	private void NewGame()
	{
		sceneToLoad = firstLoadScene;
		//OnLoadRequestEvent(sceneToLoad, firstPosition, true);
		loadEventSO.RaiseLoadRequestEvent(sceneToLoad, firstPosition, true);
	}

	/// <summary>
	/// ������������
	/// </summary>
	/// <param name="locationToLoad"></param>
	/// <param name="posToGo"></param>
	/// <param name="fadeScreen"></param>
	private void OnLoadRequestEvent(GameSceneSO locationToLoad, Vector3 posToGo, bool fadeScreen)
	{
		if (isLoading)
		{
			return;
		}
		isLoading = true;
		sceneToLoad = locationToLoad;
		positionToGo = posToGo;
		this.fadeScreen = fadeScreen;
		if(currentLoadedScene != null)
		{
			StartCoroutine(UnLoadPreviousScene());
		}
		else
		{
			LoadNewScene();
		}
	}

	private IEnumerator UnLoadPreviousScene()
	{
		if(fadeScreen)
		{
			//�������
			fadeEvent.FadeIn(fadeDuration);
		}
		yield return new WaitForSeconds(fadeDuration);

		//�㲥�¼�����Ѫ������ʾ
		sceneUnLoadedEvent.RaiseLoadRequestEvent(sceneToLoad, positionToGo, true);

		yield return currentLoadedScene.sceneReference.UnLoadScene();

		//�ر�����
		playerTrans.gameObject.SetActive(false);
		//�����³���
		LoadNewScene();
	}

	private void LoadNewScene()
	{
		var loadingOption = sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
		loadingOption.Completed += OnLoadCompleted;
	}

	/// <summary>
	/// �����������
	/// </summary>
	/// <param name="handle"></param>
	/// <exception cref="NotImplementedException"></exception>

	private void OnLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
	{
		currentLoadedScene = sceneToLoad;

		playerTrans.position = positionToGo;
		playerTrans.gameObject.SetActive(true);
		if(fadeScreen)
		{
			//������͸��
			fadeEvent.FadeOut(fadeDuration);
		}

		isLoading = false;

		if(currentLoadedScene.sceneType != SceneType.Menu)
		{
			//�������غ���¼�
			afterSceneLoadedEvent.RaiseEvent();
		}
		
	}

	public DataDefinition GetDataID()
	{
		return GetComponent<DataDefinition>();
	}

	public void GetSaveData(Data data)
	{
		data.SaveGameScene(currentLoadedScene);
	}

	public void LoadData(Data data)
	{
		var playerID = playerTrans.GetComponent<DataDefinition>().ID;
		if(data.characterPosDict.ContainsKey(playerID))
		{
			positionToGo = data.characterPosDict[playerID].ToVector3();
			sceneToLoad = data.GetSavedScene();

			OnLoadRequestEvent(sceneToLoad, positionToGo, true);
		}
	}
}
