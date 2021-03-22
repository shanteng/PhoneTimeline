using UnityEditor;
using UnityEngine;

public class DeleteCom : MonoBehaviour
{
    [MenuItem("GameObject/清理选中Prefab内丢失的Script", priority = 20)]
    public static void DeleteTTT()
    {
        if (Selection.activeGameObject == null)
        {
            return;
        }
        var gos = Selection.activeGameObject.GetComponentsInChildren<Transform>(true);
        foreach (var item in gos)
        {
            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(item.gameObject);
        }
        AssetDatabase.Refresh();
        Debug.Log("清理完成!");
    }
}
