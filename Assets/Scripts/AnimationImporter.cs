using System.IO;
using UnityEngine;

public class AnimationImporter : MonoBehaviour
{
    public Animator animator;
    public AnimationPlayer ap;

    private StreamReader sr;

    public void Start()
    {
        
    }

    private void Import(string path)
    {
        sr = new StreamReader(path);

        while (true)
        {
            var line = sr.ReadLine();
            if (line == default)
                break;
            if (line.StartsWith("#"))
                continue;
            LoadSkeletonPose(sr.ReadLine());
            break;
        }

    }


    public void LoadSkeletonPose(string line)
    {
        var data = new SkeletonData(line);

        ap.sd = data;
    }
}