using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Newtonsoft.Json;
using System.IO;

[DefaultExecutionOrder(order: -100)]
public class DataManager : MonoBehaviour
{
    public static DataManager instance;

	[Header("�¼�����")]
	public VoidEvent saveDataEvent;
	public VoidEvent loadDataEvent;

	private List<ISaveable> saveableList = new List<ISaveable>();
	private Data saveData;
	private string jsonFolder;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
			Destroy(this.gameObject);

		saveData = new Data();

		jsonFolder = Application.persistentDataPath + "/SAVE DATA/";

		ReadSaveData();
	}

	private void OnEnable()
	{
		saveDataEvent.OnEventRaised += Save;
		loadDataEvent.OnEventRaised += Load;
	}

	private void OnDisable()
	{
		saveDataEvent.OnEventRaised -= Save;
		loadDataEvent.OnEventRaised -= Load;
	}

	

	private void Update()
	{
		if(Keyboard.current.lKey.wasPressedThisFrame)
		{
			Load();
		}
	}

	public void RegisterSaveData(ISaveable saveable)
	{
		if (!saveableList.Contains(saveable))
		{
			saveableList.Add(saveable);
		}
	}

	public void UnRegisterSaveData(ISaveable saveable)
	{
		saveableList.Remove(saveable);
	}

	public void Save()
	{
		foreach(var saveable in saveableList)
		{
			saveable.GetSaveData(saveData);
		}

		var resultPath = jsonFolder + "data.sav";

		var jsonData = JsonConvert.SerializeObject(saveData);

		if (!File.Exists(resultPath))
		{
			Directory.CreateDirectory(jsonFolder);
		}
		File.WriteAllText(resultPath, jsonData);

		//foreach(var item in saveData.characterPosDict)
		//{
		//	Debug.Log(item.Key + "  " + item.Value);
		//}
	}

	public void Load()
	{
		foreach (var saveable in saveableList)
		{
			saveable.LoadData(saveData);
		}
	}

	private void ReadSaveData()
	{
		var resultPath = jsonFolder + "data.sav";
		if (File.Exists(resultPath))
		{
			var stringData = File.ReadAllText(resultPath);

			var jsonData = JsonConvert.DeserializeObject<Data>(stringData);

			saveData = jsonData;
		}
	}

}
