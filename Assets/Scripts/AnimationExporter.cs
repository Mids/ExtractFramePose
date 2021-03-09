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

    public float fps = 60f;
    private static float dt;
    private float currentTime = 0f;

    private StreamWriter sw;

    private readonly HumanBodyBones[] _ees =
        {HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftHand, HumanBodyBones.RightHand};

    private readonly HumanBodyBones[] _all =
    {
        HumanBodyBones.Hips, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot, HumanBodyBones.LeftHand,
        HumanBodyBones.RightHand
    };

    private void Start()
    {
        Application.targetFrameRate = 120;
        dt = 1f / fps;

        LoadFolder();
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


        StartCoroutine(CaptureTransform());
    }

    private IEnumerator CaptureTransform()
    {
        var t = animator.transform;
        // animator.speed = 0f;


        foreach (var anim in animList)
        {
            if (anim.name.StartsWith("H"))
            {
                print(anim.name);
                continue;
                
            }
            var motionData = new MotionData(2 + (int) (anim.length / dt));

            t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

            tOverrideController["Handshake_001"] = anim;

            print(anim.name);
            sw = new StreamWriter($"output/{anim.name}.txt", false, Encoding.UTF8);
            sw.WriteLine("#" + anim.name);

            while (currentTime < anim.length)
            {
                animator.Play("CurrentMotion", 0, currentTime / anim.length);

                yield return new WaitForEndOfFrame();

                var skeletonData = GetSkeletonData(t);
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
        foreach (var ee in _all)
        {
            var firstPose = motionData.Data[0];
            var secondPose = motionData.Data[1];
            firstPose[ee].Velocity = GetVelocity(firstPose[ee].Position, secondPose[ee].Position, dt);
            firstPose[ee].AngularVelocity = GetAngularVelocity(firstPose[ee].Rotation, secondPose[ee].Rotation, dt);

            var middleSize = motionData.Data.Count - 1;
            for (var i = 1; i < middleSize; ++i)
            {
                var prevPose = motionData.Data[i - 1];
                var curPose = motionData.Data[i];
                var nextPose = motionData.Data[i + 1];

                curPose[ee].Velocity = GetVelocity(prevPose[ee].Position, nextPose[ee].Position, 2 * dt);
                curPose[ee].AngularVelocity = GetAngularVelocity(prevPose[ee].Rotation, nextPose[ee].Rotation, 2 * dt);
            }

            var beforeLastPose = motionData.Data[middleSize - 1];
            var lastPose = motionData.Data[middleSize];
            firstPose[ee].Velocity = GetVelocity(beforeLastPose[ee].Position, lastPose[ee].Position, dt);
            firstPose[ee].AngularVelocity = GetAngularVelocity(beforeLastPose[ee].Rotation, lastPose[ee].Rotation, dt);
        }
    }

    private SkeletonData GetSkeletonData(Transform t)
    {
        var data = new SkeletonData {Root = new JointData {Position = t.localPosition, Rotation = t.localRotation}};

        foreach (var ee in _ees)
        {
            var eet = animator.GetBoneTransform(ee);
            var rootInv = Quaternion.Inverse(t.rotation);

            var relativePos = rootInv * (eet.position - t.position);
            var relativeRot = rootInv * eet.rotation;

            data[ee] = new JointData {Position = relativePos, Rotation = relativeRot};
        }

        return data;
    }

    private void WriteOutput(Vector3 v)
    {
        sw.Write($"{v.x}\t{v.y}\t{v.z}\t");
    }

    private void WriteOutput(Quaternion q)
    {
        sw.Write($"{q.x}\t{q.y}\t{q.z}\t{q.w}\t");
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