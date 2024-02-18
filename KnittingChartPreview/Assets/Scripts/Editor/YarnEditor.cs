using System.Linq;
using UnityEditor;
using UnityEngine;
using YarnGenerator;

namespace YarnGenerator
{
    public class YarnEditor : EditorWindow
    {
        // Parameters for yarn
        int rowLength = 10;
        int nRows = 1;
        float gauge = 2f;
        float yarnWidth = 0.1f;
        Material material;

        private YarnCache yarnCache = YarnCache.GetInstance();

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
            nRows = EditorGUILayout.IntSlider(
                "# Rows", nRows, 1, 300);
            gauge = EditorGUILayout.Slider(
                "Stitch Length", gauge, 0.5f, 5f);
            yarnWidth = EditorGUILayout.Slider(
                "Width", yarnWidth, 0.0001f, 0.5f);
            material = (Material) EditorGUILayout.ObjectField(material, typeof(Material));


            if (GUILayout.Button("Generate Yarn"))
            {
                if (yarnWidth > gauge / 6.0f)
                {
                    Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                                   + $"Please choose a yarn width less than {gauge / 6.0f}");
                    return;
                }

                StitchType[,] pattern = new StitchType[nRows, rowLength];
                for (int rowNumber = 0; rowNumber < nRows; rowNumber++)
                {
                    StitchType[] stitches = new StitchType[rowLength];
                    for (int i = 0; i < rowLength; i++)
                    {
                        if (i % 4 < 2)
                        {
                            pattern[rowNumber, i] = StitchType.KnitStitch;
                        }
                        else
                        {
                            pattern[rowNumber, i] = StitchType.PurlStitch;
                        }
                    }
                }
                yarnCache.GeneratePattern(pattern, yarnWidth, gauge, material);
            }

        }
    }
}