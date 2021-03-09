using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AnimationExporter))]
public class AnimationExporterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (GUILayout.Button("Export")) 
            (target as AnimationExporter)?.LoadFolder();
    }
}