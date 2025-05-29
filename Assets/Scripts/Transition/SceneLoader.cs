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

	[Header("事件监听")]
	public SceneLoadEventSO loadEventSO;
	public VoidEvent NewGameEvent;
	public VoidEvent backToMenuEvent;

	[Header("广播")]
	public VoidEvent afterSceneLoadedEvent;
	public FadeEventSO fadeEvent;
	public SceneLoadEventSO sceneUnLoadedEvent;

	[Header("场景")]
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
	/// 场景加载请求
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
			//场景变黑
			fadeEvent.FadeIn(fadeDuration);
		}
		yield return new WaitForSeconds(fadeDuration);

		//广播事件调整血量条显示
		sceneUnLoadedEvent.RaiseLoadRequestEvent(sceneToLoad, positionToGo, true);

		yield return currentLoadedScene.sceneReference.UnLoadScene();

		//关闭人物
		playerTrans.gameObject.SetActive(false);
		//加载新场景
		LoadNewScene();
	}

	private void LoadNewScene()
	{
		var loadingOption = sceneToLoad.sceneReference.LoadSceneAsync(LoadSceneMode.Additive, true);
		loadingOption.Completed += OnLoadCompleted;
	}

	/// <summary>
	/// 场景加载完成
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
			//场景变透明
			fadeEvent.FadeOut(fadeDuration);
		}

		isLoading = false;

		if(currentLoadedScene.sceneType != SceneType.Menu)
		{
			//场景加载后的事件
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
