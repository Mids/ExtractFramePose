using System.Collections.Generic;
using System.Linq;

public class MotionData
{
    public List<SkeletonData> Data;

    public MotionData(List<SkeletonData> data)
    {
        Data = data;
    }

    public MotionData(int totalFrame)
    {
        Data = new List<SkeletonData>(totalFrame);
    }

    public override string ToString()
    {
        return Data.Aggregate("", (current, datum) => current + $"{datum}\n");
    }
}