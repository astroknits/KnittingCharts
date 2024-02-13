using UnityEditor;
using UnityEngine;

public class YarnEditor : EditorWindow
{
    // Parameters for yarn
    int nRadialPoints = 8;
    int nPoints = 100;
    float width = 0.1f;
    float length = 2f;
    
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
            "Width", width, 0.0001f, 5f);
        length = EditorGUILayout.Slider(
                "Length", length, 1f, 300f);

        if (GUILayout.Button("Generate Yarn"))
        {
            Yarn.Generate(nRadialPoints, nPoints, width, length);
        }

    }
}