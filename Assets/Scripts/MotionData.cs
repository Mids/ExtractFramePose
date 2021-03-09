using System.Collections.Generic;
using System.Linq;

public class MotionData
{
    public List<SkeletonData> Data;

    public MotionData(int totalFrame)
    {
        Data = new List<SkeletonData>(totalFrame);
    }

    public override string ToString()
    {
        return Data.Aggregate("", (current, datum) => current + $"{datum}\n");
    }
}