using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YarnGenerator;

namespace YarnGenerator
{
    public class KnittingPatternEditor : EditorWindow
    {
        // [Header("Basic Pattern Options")]
        // Number of rows to render
        public int nRows = 1;

        // width of the yarn to render
        public float yarnWidth = 0.1f;

        // Material to use
        public Material material;

        // [Header("Cable Pattern Options")]
        // Number of cable blocks per row
        public int cableStitchesPerRow = 10;

        // size of cable (eg for 2x2)
        public int cableBlockSize = 4;

        // Number of rows between cable stitches (knit stitches in between) 
        public int cableLength = 4;

        // number of purl stitches separating each cable block
        public int cableSeparationSize = 2;

        // number of purl stitches on either side
        public int padding = 2;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Preview Pattern")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(KnittingPatternEditor));
        }

        Pattern GetPattern()
        {
            return new BasicPattern(yarnWidth, nRows);
            /*
            return new CablePattern(
                yarnWidth,
                nRows,
                padding,
                cableStitchesPerRow,
                cableBlockSize,
                cableSeparationSize,
                cableLength);
            */
        }

        void OnGUI()
    {
        GUILayout.Label("Basic Preview Options", EditorStyles.boldLabel);
        nRows = EditorGUILayout.IntSlider(
            "# Rows", nRows, 1, 300);
        yarnWidth = EditorGUILayout.Slider(
            "Yarn Width", yarnWidth, 0.0001f, 0.33f);
        material = (Material) EditorGUILayout.ObjectField(material, typeof(Material));
        
        GUILayout.Space(10);
        GUILayout.Label("Cable Pattern Options", EditorStyles.boldLabel);
        cableStitchesPerRow = EditorGUILayout.IntSlider(
            "Cables Per Row", cableStitchesPerRow, 1, 100);
        cableBlockSize = EditorGUILayout.IntPopup(
            "Cable Block Size", cableBlockSize, new string[] {"1x1", "2x2"}, new int[] {2, 4});
        cableLength = EditorGUILayout.IntSlider(
            "Cable Length", cableLength, 1, 10);
        cableSeparationSize = EditorGUILayout.IntSlider(
            "Cable Separation Size", cableSeparationSize, 0, 10);
        padding = EditorGUILayout.IntSlider(
            "Row padding", padding, 0, 10);


        if (GUILayout.Button("Generate Yarn"))
        {
            if (yarnWidth > 1.0f / 3.0f)
            {
                Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                               + $"Please choose a yarn width less than {2.0f / 6.0f}");
                return;
            }

            // Pattern GetPattern(float yarnWidth, int nRows, int padding, int cableStitchesPerRow, int cableBlockSize, int cableSeparationSize, int cableLength)

            Pattern pattern = GetPattern();
            pattern.RenderPreview(this.material);
        }
    }
}
}