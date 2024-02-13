using UnityEditor;
using UnityEngine;

public class YarnEditor : EditorWindow
{
    // Parameters for yarn
    int nRadialPoints = 8;
    int nPoints = 100;
    float rowLength = 2f;
    float stitchLength = 2f;
    float yarnWidth = 0.1f;
    
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
        rowLength = EditorGUILayout.Slider(
            "Row Length", rowLength, 1f, 300f);
        stitchLength = EditorGUILayout.Slider(
            "Stitch Length", stitchLength, 0.5f, 5f);
        yarnWidth = EditorGUILayout.Slider(
            "Width", yarnWidth, 0.0001f, 0.5f);


        if (GUILayout.Button("Generate Yarn"))
        {
            if (yarnWidth > stitchLength / 8.0f)
            {
                Debug.LogError("Yarn Width needs to be less than 1/8 the stitch length"
                + $"Please choose a yarn width less than {stitchLength/8.0f}");
                return;}
            Yarn.Generate(nRadialPoints, nPoints, yarnWidth, stitchLength);
        }

    }
}