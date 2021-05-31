using System;

[Serializable]
public struct SkeletonData
{
    public int frameNumber;
    public JointData[] joints;

    public SkeletonData(int nubmerOfJoints)
    {
        frameNumber = -1;
        joints = new JointData[nubmerOfJoints];
    }
}