using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/VoidEventSO")]
public class VoidEvent : ScriptableObject
{
	public UnityAction OnEventRaised;

	public void RaiseEvent()
	{
		OnEventRaised?.Invoke();
	}
}
