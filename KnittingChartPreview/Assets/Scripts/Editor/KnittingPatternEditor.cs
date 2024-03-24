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
        
        // number of stitches per row, for basic pattern
        public int basicStitchesPerRow = 8;

        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Preview Pattern")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(KnittingPatternEditor));
        }

        Pattern GetBasicPattern()
        {
            return new BasicPattern(yarnWidth, nRows, basicStitchesPerRow);
        }

        Pattern GetCablePattern()
        {
           return new CablePattern(
                yarnWidth,
                nRows,
                padding,
                cableStitchesPerRow,
                cableBlockSize,
                cableSeparationSize,
                cableLength);
        }

        void OnGUI()
    {
        GUILayout.Label("Basic Preview Options", EditorStyles.boldLabel);
        nRows = EditorGUILayout.IntSlider(
            "# Rows", nRows, 1, 50);
        yarnWidth = EditorGUILayout.Slider(
            "Yarn Width", yarnWidth, 0.0001f, 0.33f);
        material = (Material) EditorGUILayout.ObjectField(material, typeof(Material));
        
        GUILayout.Space(10);
        GUILayout.Label("Cable Pattern Options", EditorStyles.boldLabel);
        cableStitchesPerRow = EditorGUILayout.IntSlider(
            "Cables Per Row", cableStitchesPerRow, 1, 10);
        cableBlockSize = EditorGUILayout.IntPopup(
            "Cable Block Size", cableBlockSize, new string[] {"1x1", "2x2"}, new int[] {2, 4});
        cableLength = EditorGUILayout.IntSlider(
            "Cable Length", cableLength, 1, 8);
        cableSeparationSize = EditorGUILayout.IntSlider(
            "Cable Separation Size", cableSeparationSize, 0, 8);
        padding = EditorGUILayout.IntSlider(
            "Row padding", padding, 0, 8);


        if (GUILayout.Button("Generate Cable Pattern"))
        {
            if (yarnWidth > 1.0f / 3.0f)
            {
                Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                               + $"Please choose a yarn width less than {2.0f / 6.0f}");
                return;
            }
            
            Pattern pattern = GetCablePattern();
            pattern.RenderPreview(this.material);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Basic Pattern Options", EditorStyles.boldLabel);
        basicStitchesPerRow = EditorGUILayout.IntSlider(
            "Stitches Per Row", basicStitchesPerRow, 1, 10);
        
        if (GUILayout.Button("Generate Basic Pattern"))
        {
            if (yarnWidth > 1.0f / 3.0f)
            {
                Debug.LogError("Yarn Width needs to be less than 1/6 the stitch length"
                               + $"Please choose a yarn width less than {2.0f / 6.0f}");
                return;
            }
            
            Pattern pattern = GetBasicPattern();
            pattern.RenderPreview(this.material);
        }
    }
}
}