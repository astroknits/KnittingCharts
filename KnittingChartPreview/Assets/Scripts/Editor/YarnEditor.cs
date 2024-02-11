using UnityEditor;
using UnityEngine;

public class YarnEditor : EditorWindow
{
    // Parameters for yarn
    int nRadialPoints = 4;
    int nPoints = 1;
    float width = 1f;
    float length = 4f;
    
    // Add menu item named "My Window" to the Window menu
    [MenuItem("Window/Create Yarn")]
    public static void ShowWindow()
    {
        //Show existing window instance. If one doesn't exist, make one.
        EditorWindow.GetWindow(typeof(YarnEditor));
    }
    
    void OnGUI()
    {
        GUILayout.Label ("Yarn Settings", EditorStyles.boldLabel);
        
        nRadialPoints = (int)EditorGUILayout.IntSlider(
            "Radial Segments", nRadialPoints, 4, 20);
        nPoints = (int)EditorGUILayout.IntSlider(
            "Segments", nPoints, 1, 200);
        width = EditorGUILayout.Slider(
            "Width", width, 1f, 5f);
        length = EditorGUILayout.Slider(
                "Length", length, 1f, 300f);

        if (GUILayout.Button("Generate Yarn"))
        {
            Yarn.GenerateYarn(nRadialPoints, nPoints, width, length);
        }

    }
}