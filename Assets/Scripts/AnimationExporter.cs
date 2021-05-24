using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class AnimationExporter : MonoBehaviour
{
    public DefaultAsset folder;
    private string folderPath;

    public Animator animator;

    public List<AnimationClip> animList = new List<AnimationClip>();

    public AnimatorOverrideController tOverrideController;

    public int timeScale = 1;
    public float fps = 60f;
    private static float dt;
    private float currentTime = 0f;

    private StreamWriter sw;

    private string JointNameLine =
        "mixamorig1:LeftUpLeg	mixamorig1:LeftLeg	mixamorig1:LeftFoot	mixamorig1:LeftToeBase	mixamorig1:LeftToe_End	mixamorig1:RightUpLeg	mixamorig1:RightLeg	mixamorig1:RightFoot	mixamorig1:Spine	mixamorig1:Spine1	mixamorig1:Spine2	mixamorig1:LeftShoulder	mixamorig1:LeftArm	mixamorig1:LeftForeArm	mixamorig1:LeftHand	mixamorig1:Neck	mixamorig1:Head	mixamorig1:HeadTop_End	mixamorig1:RightShoulder	mixamorig1:RightArm	mixamorig1:RightForeArm	mixamorig1:RightHand";

    public List<string> JointNames;
    public Dictionary<string, Transform> TransformDict;

    private void Start()
    {
        Time.timeScale = timeScale;
        Application.targetFrameRate = 30 * timeScale;
        dt = 1f / fps;
        JointNames = JointNameLine.Split('\t').ToList();
        TransformDict = animator.GetComponentsInChildren<Transform>()
            .Where(p=>JointNames.Contains(p.name)).ToDictionary(p=>p.name);
    }

    public void LoadFolder()
    {
        if (tOverrideController == default)
        {
            tOverrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = tOverrideController;
        }

        folderPath = AssetDatabase.GetAssetPath(folder);
        GetAllFilesInDirectory(folderPath);

        StartCoroutine(CaptureTransform());
    }

    private void GetAllFilesInDirectory(string dirPath)
    {
        var info = new DirectoryInfo(dirPath);
        var fileInfo = info.GetFiles("*.fbx", SearchOption.AllDirectories);

        animList.Clear();

        foreach (var file in fileInfo)
        {
            var absolutePath = file.FullName;
            absolutePath = absolutePath.Replace(Path.DirectorySeparatorChar, '/');
            var relativePath = "";
            if (absolutePath.StartsWith(Application.dataPath))
                relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            var fbxFile = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);

            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(relativePath)
                .Where(p => p as AnimationClip != null);

            foreach (var clip in clips)
            {
                var animClip = clip as AnimationClip;

                if (animClip != default && animClip.isHumanMotion)
                    animList.Add(animClip);
            }
        }
    }

    private IEnumerator CaptureTransform()
    {
        var t = animator.transform;
        // animator.speed = 0f;


        foreach (var anim in animList)
        {
            var motionData = new MotionData(2 + (int) (anim.length / dt));

            t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            tOverrideController["Handshake_001"] = anim;

            print(anim.name);
            sw = new StreamWriter($"output/{anim.name}.txt", false, Encoding.UTF8);
            // sw.WriteLine("#" + anim.name);

            sw.WriteLine(JointNameLine);

            while (currentTime < anim.length)
            {
                animator.Play("CurrentMotion", 0, currentTime / anim.length);

                yield return null;

                var skeletonData = GetSkeletonData();
                motionData.Data.Add(skeletonData);

                currentTime += dt;
            }

            CalculateVelocity(motionData);

            sw.Write(motionData.ToString());

            currentTime = 0f;
            sw.Close();
        }
    }

    private void CalculateVelocity(MotionData motionData)
    {
        var firstPose = motionData.Data[0];
        var secondPose = motionData.Data[1];
        firstPose.Root.Velocity = GetVelocity(firstPose.Root.Position, secondPose.Root.Position, dt);
        firstPose.Root.AngularVelocity = GetAngularVelocity(firstPose.Root.Rotation, secondPose.Root.Rotation, dt);

        var middleSize = motionData.Data.Count - 1;
        for (var i = 1; i < middleSize; ++i)
        {
            var prevPose = motionData.Data[i - 1];
            var curPose = motionData.Data[i];
            var nextPose = motionData.Data[i + 1];

            curPose.Root.Velocity = GetVelocity(prevPose.Root.Position, nextPose.Root.Position, 2 * dt);
            curPose.Root.AngularVelocity = GetAngularVelocity(prevPose.Root.Rotation, nextPose.Root.Rotation, 2 * dt);
        }

        var beforeLastPose = motionData.Data[middleSize - 1];
        var lastPose = motionData.Data[middleSize];
        lastPose.Root.Velocity = GetVelocity(beforeLastPose.Root.Position, lastPose.Root.Position, dt);
        lastPose.Root.AngularVelocity = GetAngularVelocity(beforeLastPose.Root.Rotation, lastPose.Root.Rotation, dt);
        
        
        foreach (var joint in motionData.Data[0].Joints)
        {
            firstPose = motionData.Data[0];
            secondPose = motionData.Data[1];
            firstPose.JointDict[joint].Velocity = GetVelocity(firstPose.JointDict[joint].Position, secondPose.JointDict[joint].Position, dt);
            firstPose.JointDict[joint].AngularVelocity = GetAngularVelocity(firstPose.JointDict[joint].Rotation, secondPose.JointDict[joint].Rotation, dt);

            middleSize = motionData.Data.Count - 1;
            for (var i = 1; i < middleSize; ++i)
            {
                var prevPose = motionData.Data[i - 1];
                var curPose = motionData.Data[i];
                var nextPose = motionData.Data[i + 1];

                curPose.JointDict[joint].Velocity = GetVelocity(prevPose.JointDict[joint].Position, nextPose.JointDict[joint].Position, 2 * dt);
                curPose.JointDict[joint].AngularVelocity = GetAngularVelocity(prevPose.JointDict[joint].Rotation, nextPose.JointDict[joint].Rotation, 2 * dt);
            }

            beforeLastPose = motionData.Data[middleSize - 1];
            lastPose = motionData.Data[middleSize];
            lastPose.JointDict[joint].Velocity = GetVelocity(beforeLastPose.JointDict[joint].Position, lastPose.JointDict[joint].Position, dt);
            lastPose.JointDict[joint].AngularVelocity = GetAngularVelocity(beforeLastPose.JointDict[joint].Rotation, lastPose.JointDict[joint].Rotation, dt);
        }
    }

    private SkeletonData GetSkeletonData()
    {
        var data = new SkeletonData();
        data.InitDict(JointNameLine);
        var t = animator.GetBoneTransform(HumanBodyBones.Hips);

        var rootPos = t.position;
        var rootRot = t.rotation;
        var rootInv = Quaternion.Inverse(rootRot);

        data.Root = new JointData {Position = rootPos, Rotation = rootRot};

        foreach (var ee in data.Joints)
        {
            if(!TransformDict.ContainsKey(ee)) print(ee);
            var eet = TransformDict[ee];

            var relativePos = rootInv * (eet.position - rootPos);
            var relativeRot = rootInv * eet.rotation;

            data.JointDict[ee] = new JointData {Position = relativePos, Rotation = relativeRot};
        }

        return data;
    }

    private static Vector3 GetVelocity(Vector3 from, Vector3 to, float deltaTime)
    {
        return (to - from) / deltaTime;
    }

    private static Vector3 GetAngularVelocity(Quaternion from, Quaternion to, float deltaTime)
    {
        var q = to * Quaternion.Inverse(from);

        if (Mathf.Abs(q.w) > 0.999999f)
            return new Vector3(0, 0, 0);

        float gain;
        if (q.w < 0.0f)
        {
            var angle = Mathf.Acos(-q.w);
            gain = -2.0f * angle / (Mathf.Sin(angle) * deltaTime);
        }
        else
        {
            var angle = Mathf.Acos(q.w);
            gain = 2.0f * angle / (Mathf.Sin(angle) * deltaTime);
        }

        return new Vector3(q.x * gain, q.y * gain, q.z * gain);
    }


    private void OnGUI()
    {
        GUILayout.Label($"{1 / Time.deltaTime} fps");
    }
}