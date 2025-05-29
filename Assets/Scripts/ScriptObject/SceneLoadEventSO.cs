using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent;

    /// <summary>
    /// ������������
    /// </summary>
    /// <param name="LocationToGo">Ҫ���صĳ���</param>
    /// <param name="posToGo">player��Ŀ������</param>
    /// <param name="fadeScreen">�Ƿ��뽥��</param>

    public void RaiseLoadRequestEvent(GameSceneSO LocationToGo, Vector3 posToGo, bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(LocationToGo, posToGo, fadeScreen);
    }
}
