using UnityEngine;
using UnityEngine.Assertions;

public class SkeletonData
{
    public JointData Root;
    public JointData LeftFoot;
    public JointData RightFoot;
    public JointData LeftHand;
    public JointData RightHand;

    public JointData this[HumanBodyBones bone]
    {
        get
        {
            switch (bone)
            {
                case HumanBodyBones.Hips:
                    return Root;
                case HumanBodyBones.LeftFoot:
                    return LeftFoot;
                case HumanBodyBones.RightFoot:
                    return RightFoot;
                case HumanBodyBones.LeftHand:
                    return LeftHand;
                case HumanBodyBones.RightHand:
                    return RightHand;
            }

            Assert.IsTrue(false, "Undefined Error!");
            return Root;
        }
        set
        {
            switch (bone)
            {
                case HumanBodyBones.Hips:
                    Root = value;
                    return;
                case HumanBodyBones.LeftFoot:
                    LeftFoot = value;
                    return;
                case HumanBodyBones.RightFoot:
                    RightFoot = value;
                    return;
                case HumanBodyBones.LeftHand:
                    LeftHand = value;
                    return;
                case HumanBodyBones.RightHand:
                    RightHand = value;
                    return;
            }

            Assert.IsTrue(false, "Undefined Error!");
        }
    }

    public override string ToString()
    {
        return $"{Root}\t{LeftFoot}\t{RightFoot}\t{LeftHand}\t{RightHand}";
    }
}