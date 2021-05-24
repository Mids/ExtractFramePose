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

    private string _jointNameLine;
    
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

        _jointNameLine = sr.ReadLine();

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
        var data = new SkeletonData();
        data.InitDict(_jointNameLine);
        data.InitData(line);

        _skeletonData.Add(data);
    }
}