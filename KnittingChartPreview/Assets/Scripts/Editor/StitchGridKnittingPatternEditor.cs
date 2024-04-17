using UnityEditor;
using UnityEngine;

namespace YarnGenerator
{
    internal class StitchGridKnittingPatternEditor: EditorWindow
    {
        // [Header("Basic Pattern Options")]
        // Number of rows to render
        public int nRows = 1;

        // Number of stitches per row
        public int stitchesPerRow = 10;

        // width of the yarn to render
        public float yarnWidth = 0.1f;

        // Material to use
        public Material material;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Stitch Grid")]
        internal static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(StitchGridKnittingPatternEditor));
        }

        StitchGrid GetJerseyStitchGrid()
        {
            return new JerseyStitchGrid(nRows, stitchesPerRow);
        }

        void OnGUI()
        {
            GUILayout.Label("Basic Preview Options", EditorStyles.boldLabel);
            nRows = EditorGUILayout.IntSlider(
                "# Rows", nRows, 1, 50);
            nRows = EditorGUILayout.IntSlider(
                "# Stitches Per Row", stitchesPerRow, 1, 50);
            yarnWidth = EditorGUILayout.Slider(
                "Yarn Width", yarnWidth, 0.0001f, 0.33f);
            material = (Material) EditorGUILayout.ObjectField(material, typeof(Material));

            GUILayout.Space(10);


            if (GUILayout.Button("Generate Jersey Pattern"))
            {
                if (yarnWidth > 1.0f / 3.0f)
                {
                    Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                                   + $"Please choose a yarn width less than {2.0f / 6.0f}");
                    return;
                }

                StitchGrid stitchGrid = GetJerseyStitchGrid();
                stitchGrid.RenderPreview(yarnWidth, material);
            }
        }
    }
}