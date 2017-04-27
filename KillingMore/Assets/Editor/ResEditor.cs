using UnityEditor;
using UnityEngine;

public class ResEditor : EditorWindow
{
    [MenuItem("EditorTools/ResEditor")]
    static void ShowWindows()
    {
        GetWindow(typeof(ResEditor));
    }
    private void OnGUI()
    {
        if (GUILayout.Button("To Prefab", GUILayout.Width(255)))
        {
            if(Selection.activeGameObject!=null)
            ToPrefab();
        }
    }
    void ToPrefab()
    {
        PrefabUtility.CreatePrefab("Assets/"+Selection.activeGameObject.ToString()+ ".prefab", Selection.activeGameObject, ReplacePrefabOptions.Default);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
