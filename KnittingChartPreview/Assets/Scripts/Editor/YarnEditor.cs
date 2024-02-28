using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using YarnGenerator;

namespace YarnGenerator
{
    public class YarnEditor : EditorWindow
    {
        // Parameters for yarn
        int loopsPerRow = 10;
        int nRows = 1;
        float yarnWidth = 0.1f;
        Material material;


        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/Create Yarn")]
        public static void ShowWindow()
        {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(YarnEditor));
        }

        int GetCableStitchesPerRow(int loopsPerRow, int padding, int cableBlockSize, int sepSize)
        {
            // number of cables in row
            // int numCables = 0;
            // number of cable stitch separators per row 
            // int numSep = numCables - 1;
            // numCables * cableBlockSize + numSep * sepSize = (loopsPerRow - 2 * padding) 
            // numCables = (loopsPerRow - 2*padding + sepSize)/(cableBlockSize + sepSize)
            int num = (int)((float) (loopsPerRow - 2 * padding + sepSize) / (float) (cableBlockSize + sepSize));
            return num;
        }
        
        int GetTotalStitchesPerRow(int padding, int cableStitchesPerRow, int sepSize)
        {
            Debug.Log($"GetTotalStitchesPerRow: {cableStitchesPerRow} + {Math.Max((cableStitchesPerRow - 1) * sepSize, 0)}, {2 * padding}");
            return cableStitchesPerRow + Math.Max((cableStitchesPerRow - 1) * sepSize, 0) + 2 * padding;
        }
        
        int GetActualLoopsPerRow(int padding, int cableBlockSize, int sepSize, int cableStitchesPerRow)
        {
            return cableStitchesPerRow * cableBlockSize + Math.Max(cableStitchesPerRow - 1, 0) * sepSize + 2 * padding;
        }

        Pattern GetPattern()
        {
            // number of purl stitches on either side
            int padding = 2;
            // size of cable (eg for 2x2)
            int cableBlockSize = 4;
            // number of purl stitches separating each cable block
            int sepSize = 0;
            // Number of rows continuing cable pattern in knit stitch 
            int knitRows = 3;
            // calculate number of cable stitches per row
            int cableStitchesPerRow = GetCableStitchesPerRow(
                loopsPerRow, padding, cableBlockSize, sepSize);
            // calculate # stitches per row
            int stitchesPerRow = GetTotalStitchesPerRow(
                padding, cableStitchesPerRow, sepSize);

            // Set the number of loops per row to reflect the number of loops for this pattern
            loopsPerRow = GetActualLoopsPerRow(
                padding,  cableBlockSize, sepSize, cableStitchesPerRow);
            Debug.Log($"Pattern.  cableStitchesPerRow {cableStitchesPerRow} stitchesPerRow {stitchesPerRow} loopsPerRow {loopsPerRow}");

            Row[] rows = new Row[nRows];
            for (int rowNumber = 0; rowNumber < nRows; rowNumber++)
            {
                StitchType[] stitches = new StitchType[stitchesPerRow];

                int stitchIndex = 0;
                for (int i = 0; i < padding; i++)
                {
                    stitches[stitchIndex] = StitchType.PurlStitch;
                    stitchIndex += 1;
                }

                for (int i = 0; i < cableStitchesPerRow; i++)
                {
                    if (rowNumber % (knitRows + 1) == 0)
                    {
                        stitches[stitchIndex] = StitchType.Cable2Lo2RStitch;
                    }
                    else
                    {
                        stitches[stitchIndex] = StitchType.CableKnitStitch4;
                    }
                    stitchIndex += 1;
                    if (cableStitchesPerRow > 1 && i < cableStitchesPerRow - 1)
                    {
                        for (int j = 0; j < sepSize; j++)
                        {
                            stitches[stitchIndex] = StitchType.PurlStitch;
                            stitchIndex += 1;
                        }
                    }
                }

                for (int i = 0; i < padding; i++)
                {
                    stitches[stitchIndex] = StitchType.PurlStitch;
                    stitchIndex += 1;
                }

                rows[rowNumber] = new Row(rowNumber, stitches);
                foreach (Stitch stitch in rows[rowNumber].stitches)
                {
                    Debug.Log($"    rowNumber {rowNumber} index {stitch.index} stitch.stitchType {stitch.stitchType}");
                }
            }
            return new Pattern(rows);
        }

    void OnGUI()
    {
        GUILayout.Label("Yarn Settings", EditorStyles.boldLabel);

        loopsPerRow = EditorGUILayout.IntSlider(
            "Stitches Per Row", loopsPerRow, 1, 300);
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

            Pattern pattern = GetPattern();
            pattern.RenderPreview(yarnWidth, this.material);
        }
    }
}
}