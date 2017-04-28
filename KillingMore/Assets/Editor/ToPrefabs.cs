using UnityEditor;
using UnityEngine;

public class ToPrefabs : EditorWindow {

    [MenuItem("EditorTool/ToPrefabs")]

    public static void ShowWindows()
    {
        GetWindow(typeof(ToPrefabs));
    }
    private void OnGUI()
    {
        if (GUILayout.Button("To Prefabs", GUILayout.Width(255)))
        {
            Go2Prefabs();
        }
    }
    private void Go2Prefabs()
    {
        if(Selection.activeGameObject!=null)
        PrefabUtility.CreatePrefab("Assets/"+ Selection.activeGameObject.name+ ".prefab", Selection.activeGameObject, ReplacePrefabOptions.Default);
        
    }
}
