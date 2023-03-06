using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace DataProcessor.Editor
{
public class MotionConverterWindow : EditorWindow
{
    private const string Title = "Motion Converter";

    private static Transform _pelvis;
    private static Animator _animator;
    private static DefaultAsset _folder;
    private static AnimationClip _clip;

    private static float _fps = 30;
    private static string _outputDir = "OutputMotions";

    private static bool _converting = false;
    private static bool _done = false;

    private static AnimationClip[] _clips;
    private Vector3[] _positions;
    private Quaternion[] _quaternions;

    [MenuItem("Motion/FBX2Motion")]
    private static void Init() => GetWindow<MotionConverterWindow>(Title).Show();

    private void OnGUI()
    {
        EditorUtility.ClearProgressBar();
        if (_converting)
        {
            EditorGUI.BeginDisabledGroup(true);
        }
        else
        {
            EditorGUI.BeginDisabledGroup(false);
        }

        var originalLabelWidth = EditorGUIUtility.labelWidth;
        originalLabelWidth = Mathf.Min(originalLabelWidth, EditorGUIUtility.currentViewWidth / 2);
        EditorGUIUtility.labelWidth = originalLabelWidth;
        GUILayout.Label(Title, EditorStyles.boldLabel);

        _pelvis = (Transform)EditorGUILayout.ObjectField("Pelvis", _pelvis, typeof(Transform), true);
        _animator = (Animator)EditorGUILayout.ObjectField("Animator", _animator, typeof(Animator), true);

        GUILayout.Space(5);
        GUILayout.Label("Choose folder or clip", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        if (!_clip && !_folder)
        {
            EditorGUIUtility.labelWidth = originalLabelWidth / 2;
        }

        if (!_clip)
        {
            _folder = (DefaultAsset)EditorGUILayout.ObjectField("Folder", _folder, typeof(DefaultAsset), false);
        }

        if (!_folder)
        {
            _clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", _clip, typeof(AnimationClip), false);
        }

        Assert.IsFalse(_clip && _folder, "Choose one between Folder or Clip");

        EditorGUIUtility.labelWidth = originalLabelWidth;
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        _fps = EditorGUILayout.FloatField("FPS", _fps);
        _outputDir = EditorGUILayout.TextField("Output Directory", _outputDir);

        if (GUILayout.Button("Convert!"))
        {
            _converting = true;

            var clips = GetClips();
            BackUpTransform();
            var clipsCount = clips.Count;
            for (var clipIndex = 0; clipIndex < clipsCount; clipIndex++)
            {
                var clip = clips[clipIndex];
                EditorUtility.DisplayProgressBar("Converting Motions", $"{clipIndex} / {clipsCount}",
                    (float)clipIndex / clipsCount);
                CaptureTransform(clip);
            }

            ResetTransform();

            _converting = false;

            _done = true;
        }

        if (_done)
        {
            EditorGUILayout.LabelField("Finished.");
        }
    }

    private IEnumerator Convert()
    {
        yield break;
    }

    private List<AnimationClip> GetClips()
    {
        if (_clip)
        {
            return new List<AnimationClip> { _clip };
        }

        var folderPath = AssetDatabase.GetAssetPath(_folder);
        var folderInfo = new DirectoryInfo(folderPath);
        var fileInfos = folderInfo.GetFiles("*.fbx", SearchOption.AllDirectories);
        var allClips = new List<AnimationClip>();

        foreach (var fileInfo in fileInfos)
        {
            var absolutePath = fileInfo.FullName.Replace(Path.DirectorySeparatorChar, '/');
            Assert.IsTrue(absolutePath.StartsWith(Application.dataPath));
            var relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(relativePath)
                .OfType<AnimationClip>();

            allClips.AddRange(clips);
        }

        return allClips;
    }

    private void BackUpTransform()
    {
        var ts = _animator.GetComponentsInChildren<Transform>();
        _positions = new Vector3[ts.Length];
        _quaternions = new Quaternion[ts.Length];

        for (var i = 0; i < ts.Length; i++)
        {
            _positions[i] = ts[i].localPosition;
            _quaternions[i] = ts[i].localRotation;
        }
    }

    private void ResetTransform()
    {
        var ts = _animator.GetComponentsInChildren<Transform>();

        for (var i = 0; i < ts.Length; i++)
        {
            ts[i].localPosition = _positions[i];
            ts[i].localRotation = _quaternions[i];
        }
    }

    private void CaptureTransform(AnimationClip clip)
    {
        var t = _animator.transform;

        t.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        // Set Info
        var motionData = CreateInstance<MotionData>();
        motionData.Init(2 + (int)(clip.length * _fps));
        motionData.characterName = t.name;
        motionData.motionName = clip.name;
        motionData.fps = _fps;

        int frame = 0;
        float time = 0f;

        while (time < clip.length)
        {
            clip.SampleAnimation(_animator.gameObject, time);

            var PoseData = GetPoseData(frame++);
            motionData.data.Add(PoseData);

            time += 1 / _fps;
        }

        motionData.totalFrames = motionData.data.Count;

        PostProcess(motionData);

        motionData.Save(_outputDir);
    }

    private PoseData GetPoseData(int frameNumber)
    {
        var transforms = _pelvis.GetComponentsInChildren<Transform>();
        var data = new PoseData(transforms.Length) { frameNumber = frameNumber };

        #region Root

        var root = transforms[0];
        var rootPos = root.position;
        var rootRot = root.rotation;
        var rootInv = Quaternion.Inverse(rootRot);

        data.joints[0] = new JointData
        {
            position = rootPos,
            rotation = rootRot,
            jointIdx = 0,
            jointName = root.name,
            inverseRotation = rootInv,
            parentIdx = -1,
            childrenIdx = new List<int>() { },
        };

        #endregion

        #region Joints

        for (var index = 1; index < data.joints.Length; index++)
        {
            var jointTransform = transforms[index];

            var relativePos = rootInv * (jointTransform.position - rootPos);
            var relativeRot = rootInv * jointTransform.rotation;
            var inverseRot = Quaternion.Inverse(relativeRot);

            var parent = jointTransform.parent;
            var parentIdx = Array.IndexOf(transforms, parent);

            data.joints[index] = new JointData
            {
                position = relativePos,
                rotation = relativeRot,
                jointIdx = index,
                jointName = jointTransform.name,
                inverseRotation = inverseRot,
                parentIdx = parentIdx,
                childrenIdx = new List<int>() { },
            };

            data.joints[parentIdx].childrenIdx.Add(index);
        }

        #endregion

        return data;
    }

    private void PostProcess(MotionData motionData)
    {
        CalculateVelocity(motionData);
    }

    private void CalculateVelocity(MotionData motionData)
    {
        for (var i = 0; i < motionData.data.Count; ++i)
        {
            var prevFrame = Mathf.Max(0, i - 1);
            var nextFrame = Mathf.Min(i + 1, motionData.data.Count - 1);
            var t = 1 / _fps * (nextFrame - prevFrame);

            var cur = motionData.data[i];
            var prev = motionData.data[prevFrame];
            var next = motionData.data[nextFrame];

            for (var j = 0; j < cur.joints.Length; ++j)
            {
                cur.joints[j].velocity = GetVelocity(prev.joints[j].position, next.joints[j].position, t);
                cur.joints[j].angularVelocity =
                    GetAngularVelocity(prev.joints[j].rotation, next.joints[j].rotation, t);
            }

            cur.centerOfMassVelocity = GetVelocity(prev.centerOfMass, next.centerOfMass, t);
        }
    }

    private static Vector3 GetVelocity(Vector3 from, Vector3 to, float deltaTime)
    {
        return (to - from) / deltaTime;
    }

    private static Vector3 GetAngularVelocity(Quaternion from, Quaternion to, float deltaTime)
    {
        var q = to * Quaternion.Inverse(from);

        if (Mathf.Abs(q.w) > 0.999999f)
        {
            return new Vector3(0, 0, 0);
        }

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

    //     private void GetAllFilesInDirectory(string dirPath)
    // {
    //     var info = new DirectoryInfo(dirPath);
    //     var fileInfo = info.GetFiles("*.fbx", SearchOption.AllDirectories);
    //
    //     animList.Clear();
    //
    //     foreach (var file in fileInfo)
    //     {
    //         var absolutePath = file.FullName;
    //         absolutePath = absolutePath.Replace(Path.DirectorySeparatorChar, '/');
    //         var relativePath = "";
    //         if (absolutePath.StartsWith(Application.dataPath))
    //             relativePath = "Assets" + absolutePath.Substring(Application.dataPath.Length);
    //         var fbxFile = AssetDatabase.LoadAssetAtPath<GameObject>(relativePath);
    //
    //         var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(relativePath)
    //             .Where(p => p as AnimationClip != null);
    //
    //         foreach (var clip in clips)
    //         {
    //             var animClip = clip as AnimationClip;
    //
    //             if (animClip != default && animClip.isHumanMotion)
    //                 animList.Add(animClip);
    //         }
    //     }
    // }
}
}