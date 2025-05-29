using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPoint : MonoBehaviour, IInteractable
{
	[Header("¹ã²¥")]
	public SceneLoadEventSO loadEventSO;
	public GameSceneSO sceneToGo;
	public Vector3 positionToGo;

	public void TriggerAction()
	{
		loadEventSO.RaiseLoadRequestEvent(sceneToGo, positionToGo, true);
	}
}
