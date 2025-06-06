using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Event/SceneLoadEventSO")]
public class SceneLoadEventSO : ScriptableObject
{
    public UnityAction<GameSceneSO, Vector3, bool> LoadRequestEvent;

    /// <summary>
    /// 场景加载请求
    /// </summary>
    /// <param name="LocationToGo">要加载的场景</param>
    /// <param name="posToGo">player的目的坐标</param>
    /// <param name="fadeScreen">是否渐入渐出</param>

    public void RaiseLoadRequestEvent(GameSceneSO LocationToGo, Vector3 posToGo, bool fadeScreen)
    {
        LoadRequestEvent?.Invoke(LocationToGo, posToGo, fadeScreen);
    }
}
