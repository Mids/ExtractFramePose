using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Animator targetAnimator;

    private Animator _animator;

    public Transform RandomObject;

    private readonly Dictionary<HumanBodyBones, Quaternion> _TPoseDict = new Dictionary<HumanBodyBones, Quaternion>();

    private readonly Dictionary<HumanBodyBones, AvatarIKGoal> _eeDict = new Dictionary<HumanBodyBones, AvatarIKGoal>
    {
        {HumanBodyBones.LeftFoot, AvatarIKGoal.LeftFoot}, {HumanBodyBones.RightFoot, AvatarIKGoal.RightFoot},
        {HumanBodyBones.LeftHand, AvatarIKGoal.LeftHand}, {HumanBodyBones.RightHand, AvatarIKGoal.RightHand}
    };

    // Start is called before the first frame update
    private void Start()
    {
        _animator = GetComponent<Animator>();

        GetTPose();
    }

    private void GetTPose()
    {
        foreach (var ee in _eeDict.Keys) _TPoseDict[ee] = _animator.GetBoneTransform(ee).rotation;
    }

    private void LateUpdate()
    {
        foreach (var ee in _eeDict)
        {
            var eeTargetT = targetAnimator.GetBoneTransform(ee.Key);
            var eeT = _animator.GetBoneTransform(ee.Key);

            eeT.rotation = eeTargetT.rotation;
        }
    }

    // Update is called once per frame
    private void OnAnimatorIK()
    {
        var targetT = targetAnimator.GetBoneTransform(HumanBodyBones.Hips);
        var t = _animator.transform;
        t.position = targetT.position;
        t.rotation = targetT.rotation;


        foreach (var ee in _eeDict)
        {
            var eeTargetT = targetAnimator.GetBoneTransform(ee.Key);
            var eeT = _animator.GetBoneTransform(ee.Key);

            _animator.SetIKPositionWeight(ee.Value, 1);
            _animator.SetIKPosition(ee.Value, eeTargetT.position);
            // if (ee.Value == AvatarIKGoal.LeftFoot || ee.Value == AvatarIKGoal.RightFoot)
            //     _animator.SetIKRotationWeight(ee.Value, 1);
            // else
            //     _animator.SetIKRotationWeight(ee.Value, 0);
            // _animator.SetIKRotation(ee.Value, eeTargetT.rotation * Quaternion.Inverse(_TPoseDict[ee.Key]));
        }
    }
}