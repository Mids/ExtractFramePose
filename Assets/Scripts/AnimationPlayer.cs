using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    public SkeletonData sd;
    private Animator _animator;

    public GameObject EE;

    public Dictionary<HumanBodyBones, Transform> EEDict = new Dictionary<HumanBodyBones, Transform>();

    private void Start()
    {
        _animator = GetComponent<Animator>();

        // EEDict[HumanBodyBones.Hips] = Instantiate(EE).transform;
        // EEDict[HumanBodyBones.LeftFoot] = Instantiate(EE).transform;
        // EEDict[HumanBodyBones.RightFoot] = Instantiate(EE).transform;
        // EEDict[HumanBodyBones.LeftHand] = Instantiate(EE).transform;
        // EEDict[HumanBodyBones.RightHand] = Instantiate(EE).transform;
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (sd != default)
        {
            var rootPos = sd[HumanBodyBones.Hips].Position;
            var rootRot = sd[HumanBodyBones.Hips].Rotation;
            transform.localPosition = rootPos;
            transform.localRotation = rootRot;

            _animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            _animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            _animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);


            _animator.SetIKPosition(AvatarIKGoal.LeftFoot, rootPos + rootRot * sd[HumanBodyBones.LeftFoot].Position);
            
            // _animator.SetIKRotation(AvatarIKGoal.LeftFoot, rootRot * sd[HumanBodyBones.LeftFoot].Rotation);
            _animator.SetIKRotation(AvatarIKGoal.LeftFoot, _animator.GetBoneTransform(HumanBodyBones.LeftFoot).rotation);
            // _animator.SetIKRotation(AvatarIKGoal.LeftFoot, Quaternion.identity);

            _animator.SetIKPosition(AvatarIKGoal.RightFoot, rootPos + rootRot * sd[HumanBodyBones.RightFoot].Position);
            _animator.SetIKRotation(AvatarIKGoal.RightFoot, rootRot * sd[HumanBodyBones.RightFoot].Rotation);
            // _animator.SetIKRotation(AvatarIKGoal.RightFoot, Quaternion.identity);

            _animator.SetIKPosition(AvatarIKGoal.LeftHand, rootPos + rootRot * sd[HumanBodyBones.LeftHand].Position);
            _animator.SetIKRotation(AvatarIKGoal.LeftHand, rootRot * sd[HumanBodyBones.LeftHand].Rotation);
            // _animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.identity);

            _animator.SetIKPosition(AvatarIKGoal.RightHand, rootPos + rootRot * sd[HumanBodyBones.RightHand].Position);
            _animator.SetIKRotation(AvatarIKGoal.RightHand, rootRot * sd[HumanBodyBones.RightHand].Rotation);
            // _animator.SetIKRotation(AvatarIKGoal.RightHand,Quaternion.identity);


            // EEDict[HumanBodyBones.Hips].position = rootPos;
            // EEDict[HumanBodyBones.Hips].rotation = rootRot;
            //
            // EEDict[HumanBodyBones.LeftFoot].position = rootPos + rootRot * sd[HumanBodyBones.LeftFoot].Position;
            // EEDict[HumanBodyBones.LeftFoot].rotation = rootRot * sd[HumanBodyBones.LeftFoot].Rotation;
            //
            // EEDict[HumanBodyBones.RightFoot].position = rootPos + rootRot * sd[HumanBodyBones.RightFoot].Position;
            // EEDict[HumanBodyBones.RightFoot].rotation = rootRot * sd[HumanBodyBones.RightFoot].Rotation;
            //
            // EEDict[HumanBodyBones.LeftHand].position = rootPos + rootRot * sd[HumanBodyBones.LeftHand].Position;
            // EEDict[HumanBodyBones.LeftHand].rotation = rootRot * sd[HumanBodyBones.LeftHand].Rotation;
            //
            // EEDict[HumanBodyBones.RightHand].position = rootPos + rootRot * sd[HumanBodyBones.RightHand].Position;
            // EEDict[HumanBodyBones.RightHand].rotation = rootRot * sd[HumanBodyBones.RightHand].Rotation;
        }
    }
}