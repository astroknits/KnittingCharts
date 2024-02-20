using System.Linq;
using UnityEditor;
using UnityEngine;
using YarnGenerator;

namespace YarnGenerator
{
    public class YarnEditor : EditorWindow
    {
        // Parameters for yarn
        int stitchesPerRow = 10;
        int nRows = 1;
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

            stitchesPerRow = EditorGUILayout.IntSlider(
                "Stitches Per Row", stitchesPerRow, 1, 300);
            nRows = EditorGUILayout.IntSlider(
                "# Rows", nRows, 1, 300);
            yarnWidth = EditorGUILayout.Slider(
                "Width", yarnWidth, 0.0001f, 0.33f);
            material = (Material) EditorGUILayout.ObjectField(material, typeof(Material));


            if (GUILayout.Button("Generate Yarn"))
            {
                if (yarnWidth > 1.0f / 3.0f)
                {
                    Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                                   + $"Please choose a yarn width less than {2.0f / 6.0f}");
                    return;
                }

                StitchType[,] pattern = new StitchType[nRows, stitchesPerRow];
                for (int rowNumber = 0; rowNumber < nRows; rowNumber++)
                {
                    StitchType[] stitches = new StitchType[stitchesPerRow];
                    pattern[rowNumber, 0] = StitchType.PurlStitch;
                    pattern[rowNumber, 1] = StitchType.PurlStitch;
                    for (int i = 2; i < stitchesPerRow - 2; i++)
                    {
                        if (i % 2 == 0)
                        {
                            pattern[rowNumber, i] = StitchType.PurlStitch;
                        }
                        else
                        {
                            if (rowNumber % 4 == 0)
                            {
                                pattern[rowNumber, i] = StitchType.Cable2Lo2RStitch;
                            }
                            else
                            {
                                pattern[rowNumber, i] = StitchType.CableKnitStitch4;
                            }
                        }
                    }
                    pattern[rowNumber, stitchesPerRow - 2] = StitchType.PurlStitch;
                    pattern[rowNumber, stitchesPerRow - 1] = StitchType.PurlStitch;

                    /* Knit 2 Purl 2
                    for (int i = 0; i < stitchesPerRow; i++)
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
                    */
                }
                yarnCache.GeneratePattern(pattern, yarnWidth, material);
            }

        }
    }
}