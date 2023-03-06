using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace DataProcessor
{
public class MotionData : ScriptableObject
{
    public string characterName;
    public string motionName;
    public int totalFrames;
    public float fps = 60;
    public List<PoseData> data;

    public void Init(MotionData source)
    {
        characterName = source.characterName;
        motionName = source.motionName;
        totalFrames = source.totalFrames;
        fps = source.fps;
        data = new List<PoseData>(source.data.Count);
        for (var i = 0; i < source.data.Count; i++)
            data.Add(new PoseData(source.data[i]));
    }

    public void Init(int frameCount)
    {
        data = new List<PoseData>(frameCount);
    }

    public void Save(string outputDir)
    {
#if UNITY_EDITOR
        var path = $"Assets/{outputDir}/{motionName}.motion.asset";
        Debug.Log($"Saving to {path}");
        if (!AssetDatabase.IsValidFolder($"Assets/{outputDir}"))
        {
            AssetDatabase.CreateFolder("Assets", outputDir);
        }

        AssetDatabase.CreateAsset(this, path);
        AssetDatabase.SaveAssets();
#endif // UNITY_EDITOR
    }
}
}