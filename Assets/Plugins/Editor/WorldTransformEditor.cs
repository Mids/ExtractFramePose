using UnityEditor;
using UnityEngine;

// [CustomEditor(typeof(Transform))]
public class WorldTransformEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        var t = target as Transform;
        if (t == default) return;
        
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Separator();
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Vector3Field("World Pos", t.position);
        EditorGUILayout.EndHorizontal();
        
        var localRot = t.localRotation;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Vector4Field("Local Rot",
            new Vector4(localRot.x, localRot.y, localRot.z, localRot.w));
        //this will display the target's world pos.
        EditorGUILayout.EndHorizontal();
        
        var rotation = t.rotation;
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Vector4Field("World Rot",
            new Vector4(rotation.x, rotation.y, rotation.z, rotation.w));
        //this will display the target's world pos.
        EditorGUILayout.EndHorizontal();
        EditorGUI.EndDisabledGroup();
    }
}