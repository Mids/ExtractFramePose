using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AnimationImporter : MonoBehaviour
{
    public AnimationPlayer ap;

    private StreamReader sr;

    private List<SkeletonData> _skeletonData = new List<SkeletonData>(100);

    private int frame = 0;

    
    private void Start()
    {
        Import($"output/Handshake_001.txt");
    }
    public void Update()
    {
        if(_skeletonData.Count > frame)
            ap.sd = _skeletonData[frame++];
    }

    private MotionData Import(string path)
    {
        sr = new StreamReader(path);

        while (true) 
        {
            var line = sr.ReadLine();
            if (line == default)
                break;

            LoadSkeletonPose(line);
        }

        return new MotionData(_skeletonData);
    }


    public void LoadSkeletonPose(string line)
    {
        var data = new SkeletonData(line);

        _skeletonData.Add(data);
    }
}