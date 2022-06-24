using System.IO;
using System.Reflection;
using System.Threading;
using Ra.Trail;
using UnityEditor;
using UnityEngine;

public class WorkerCreator : EditorWindow
{
    private static bool created;
    [MenuItem("Assets/Create/Workers/Create Worker %SPACE", false, -100)]
    static void Init()
    {
        if (created) return;
        created = true;
        WorkerCreator window = ScriptableObject.CreateInstance<WorkerCreator>();
        window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 100);
        window.ShowPopup();
    }

    private string nameText;

    void OnGUI()
    {
        EditorGUILayout.LabelField("Create Worker", EditorStyles.wordWrappedLabel);
        GUI.SetNextControlName("WorkerName");
        nameText = EditorGUILayout.TextField("WorkerName", nameText);
        EditorGUI.FocusTextInControl("WorkerName");
        if (GUILayout.Button("Create") || Event.current.keyCode == KeyCode.Return)
        {
            var path = "";
            if(!TryGetActiveFolderPath(out path)) return;
            var sr = File.CreateText( path + "/" + nameText + ".cs");
            sr.WriteLine (
                "using UnityEngine;\n \n" +
                "[CreateAssetMenu(fileName = nameof(" 
                + nameText + "), menuName = \"Workers/\" + nameof(" 
                + nameText + "), order = 1)] \n" +
                "public class " + nameText + " : Worker \n{\n" + 
                @"
    public override void Start()
    {
        
    }

    public override void Update()
    {
        
    }
" + "}\n");
            sr.Close();
            Close();
            AssetDatabase.Refresh();
        }
        if (GUILayout.Button("Exit") || Event.current.keyCode == KeyCode.Escape)
        {
            created = false;
            Close();   
        }
    }
    private static bool TryGetActiveFolderPath( out string path )
    {
        var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod( "TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic );

        object[] args = new object[] { null };
        bool found = (bool)_tryGetActiveFolderPath.Invoke( null, args );
        path = (string)args[0];

        return found;
    }
    
}