using UnityEditor;
using UnityEngine;
using YarnGenerator;

namespace YarnGenerator
{
    public class YarnEditor : EditorWindow
    {
        // Parameters for yarn
        int rowLength = 10;
        float gauge = 2f;
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
            GUILayout.Label("Yarn Settings", EditorStyles.boldLabel);

            rowLength = EditorGUILayout.IntSlider(
                "Row Length", rowLength, 1, 300);
            gauge = EditorGUILayout.Slider(
                "Stitch Length", gauge, 0.5f, 5f);
            yarnWidth = EditorGUILayout.Slider(
                "Width", yarnWidth, 0.0001f, 0.5f);


            if (GUILayout.Button("Generate Yarn"))
            {
                if (yarnWidth > gauge / 8.0f)
                {
                    Debug.LogError("Yarn Width needs to be less than 1/8 the stitch length"
                                   + $"Please choose a yarn width less than {gauge / 8.0f}");
                    return;
                }

                StitchType[] stitches = new StitchType[rowLength];
                for (int i = 0; i < rowLength; i++)
                {
                    stitches[i] = StitchType.KnitStitch;
                }
                Yarn.GenerateRow(stitches, yarnWidth, gauge);
            }

        }
    }
}