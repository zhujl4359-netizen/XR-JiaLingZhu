using UnityEngine;

[DefaultExecutionOrder(100)]
public class SkyQuestAvatarWalk : MonoBehaviour
{
    [Header("Walk motion")]
    // Tuned against the supplied side-view walking recording: a brisk but
    // compact stride, hands close to the body, and only a little body bounce.
    public float legSwing = 23f;
    public float armSwing = 10f;
    public float kneeBend = 34f;
    public float elbowBend = 12f;
    public float stepsPerSecond = 1.7f;
    public float pelvisRise = 0.018f;
    public float pelvisSway = 0.015f;

    private SkyQuestLocomotion locomotion;
    private Transform modelRoot;
    private Transform leftArm;
    private Transform rightArm;
    private Transform leftLeg;
    private Transform rightLeg;
    private Transform leftKnee;
    private Transform rightKnee;
    private Transform leftFoot, rightFoot;
    private Transform pelvis, chest, headBone;
    private Vector3 leftThighAxis, rightThighAxis, leftShinAxis, rightShinAxis;
    private Transform leftElbow, rightElbow, leftWrist, rightWrist;
    private Vector3 leftUpperAxis, rightUpperAxis, leftLowerAxis, rightLowerAxis;
    private Quaternion leftElbowRest, rightElbowRest;
    private Quaternion leftArmRest;
    private Quaternion rightArmRest;
    private Quaternion leftLegRest;
    private Quaternion rightLegRest;
    private Quaternion leftKneeRest;
    private Quaternion rightKneeRest;
    private Quaternion leftFootRest, rightFootRest;
    private Quaternion pelvisRest, chestRest, headRest;
    private Vector3 modelRestPosition;
    private Quaternion modelRestRotation;
    private float restFootHeight;
    private float cycle;
    private float blend;
    private bool initialized;
    private float eatStarted = -10f;
    private const float EatGestureDuration = 1.15f;

    public Transform RightWrist => rightWrist;

    public void PlayEatGesture()
    {
        CacheRig();
        eatStarted = Time.time;
    }

    public void Initialize(SkyQuestLocomotion owner)
    {
        locomotion = owner;
        CacheRig();
    }

    void Start()
    {
        if (locomotion == null) locomotion = GetComponentInParent<SkyQuestLocomotion>();
        CacheRig();
    }

    private void CacheRig()
    {
        if (initialized) return;

        Animator animator = GetComponentInChildren<Animator>(true);
        if (animator != null && animator.isHuman)
        {
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            leftWrist = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            rightWrist = animator.GetBoneTransform(HumanBodyBones.RightHand);
            leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        // COTL_rigged uses Blender's default generic bone names. The two chains
        // branching from Bone.002 are the arms; the two chains branching from
        // the root Bone are the legs.
        leftArm ??= Find("Bone.005");
        rightArm ??= Find("Bone.008");
        leftElbow ??= Find("Bone.006");
        rightElbow ??= Find("Bone.009");
        leftWrist ??= Find("Bone.007");
        rightWrist ??= Find("Bone.010");
        leftLeg ??= Find("Bone.011");
        rightLeg ??= Find("Bone.014");
        leftKnee ??= Find("Bone.012");
        rightKnee ??= Find("Bone.015");
        leftFoot ??= Find("Bone.013");
        rightFoot ??= Find("Bone.016");
        pelvis = Find("Bone");
        chest = Find("Bone.002");
        headBone = Find("Bone.004");

        modelRoot = Find("COTL_rigged");
        if (modelRoot == null && transform.childCount > 0) modelRoot = transform.GetChild(0);
        if (modelRoot != null)
        {
            modelRestPosition = modelRoot.localPosition;
            modelRestRotation = modelRoot.localRotation;
        }

        if (leftArm != null) leftArmRest = leftArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        if (leftLeg != null) leftLegRest = leftLeg.localRotation;
        if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        if (leftKnee != null) leftKneeRest = leftKnee.localRotation;
        if (rightKnee != null) rightKneeRest = rightKnee.localRotation;
        if (leftFoot != null) leftFootRest = leftFoot.localRotation;
        if (rightFoot != null) rightFootRest = rightFoot.localRotation;
        if (pelvis != null) pelvisRest = pelvis.localRotation;
        if (chest != null) chestRest = chest.localRotation;
        if (headBone != null) headRest = headBone.localRotation;
        CacheArmAxis(leftLeg, leftKnee, out leftThighAxis);
        CacheArmAxis(rightLeg, rightKnee, out rightThighAxis);
        CacheArmAxis(leftKnee, leftFoot, out leftShinAxis);
        CacheArmAxis(rightKnee, rightFoot, out rightShinAxis);
        CacheArmAxis(leftArm, leftElbow, out leftUpperAxis);
        CacheArmAxis(rightArm, rightElbow, out rightUpperAxis);
        CacheArmAxis(leftElbow, leftWrist, out leftLowerAxis);
        CacheArmAxis(rightElbow, rightWrist, out rightLowerAxis);
        if (leftElbow != null) leftElbowRest = leftElbow.localRotation;
        if (rightElbow != null) rightElbowRest = rightElbow.localRotation;
        if (leftFoot != null && rightFoot != null)
            restFootHeight = Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y,
                transform.InverseTransformPoint(rightFoot.position).y);
        initialized = leftArm != null && rightArm != null && leftLeg != null && rightLeg != null;
    }

    void LateUpdate()
    {
        if (!initialized || locomotion == null) return;

        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        float speed01 = Mathf.InverseLerp(0.08f, Mathf.Max(0.8f, locomotion.groundSpeed * locomotion.movementScale),
            locomotion.CurrentSpeed);
        float targetBlend = locomotion.IsFlying ? 0f : speed01;
        blend = Mathf.MoveTowards(blend, targetBlend, dt * (targetBlend > blend ? 7f : 5f));

        if (blend > 0.001f)
        {
            // A complete cycle contains two footfalls. The reference keeps a
            // steady cadence rather than turning into a sprint at full input.
            float cyclesPerSecond = stepsPerSecond * .5f * Mathf.Lerp(.9f, 1.55f, speed01);
            cycle = Mathf.Repeat(cycle + dt * cyclesPerSecond * Mathf.PI * 2f,
                Mathf.PI * 2f);
        }

        float phase = Mathf.Repeat(cycle / (Mathf.PI * 2f), 1f);
        SampleWalkPose(phase, out float leftStride, out float leftKneePose,
            out float leftFootPitch);
        SampleWalkPose(Mathf.Repeat(phase + .5f, 1f), out float rightStride,
            out float rightKneePose, out float rightFootPitch);
        PoseBodyRoot(phase, dt);
        PoseUpperBody(phase, dt);
        float leftKneeAmount = Mathf.Lerp(4f, leftKneePose, blend);
        float rightKneeAmount = Mathf.Lerp(4f, rightKneePose, blend);
        PoseWalkingLeg(leftLeg, leftKnee, leftThighAxis, leftShinAxis,
            leftLegRest, leftKneeRest, leftStride * legSwing * blend, leftKneeAmount, dt);
        PoseWalkingLeg(rightLeg, rightKnee, rightThighAxis, rightShinAxis,
            rightLegRest, rightKneeRest, rightStride * legSwing * blend, rightKneeAmount, dt);
        PoseFoot(leftFoot, leftFootRest, leftFootPitch * blend, dt);
        PoseFoot(rightFoot, rightFootRest, rightFootPitch * blend, dt);
        PoseWalkingArm(leftArm, leftElbow, leftUpperAxis, leftLowerAxis,
            leftArmRest, leftElbowRest, -leftStride * armSwing * blend, -1f, dt);
        PoseWalkingArm(rightArm, rightElbow, rightUpperAxis, rightLowerAxis,
            rightArmRest, rightElbowRest, -rightStride * armSwing * blend, 1f, dt);

        PoseEatingArm();

        KeepSupportFootGrounded(phase, dt);
    }

    private void PoseEatingArm()
    {
        float elapsed = Time.time - eatStarted;
        if (elapsed < 0f || elapsed >= EatGestureDuration ||
            rightArm == null || rightElbow == null || headBone == null) return;

        float weight = elapsed < .34f ? Smooth01(elapsed / .34f) :
            elapsed < .82f ? 1f : 1f - Smooth01((elapsed - .82f) / .33f);
        Vector3 forward = FacingForward();
        Vector3 right = FacingRight();
        Vector3 elbowTarget = rightArm.position + forward * .22f + Vector3.down * .13f - right * .11f;
        Vector3 mouthTarget = headBone.position + forward * .15f + Vector3.down * .09f;

        Quaternion upperTarget = Quaternion.FromToRotation(
            rightArm.TransformDirection(rightUpperAxis),
            (elbowTarget - rightArm.position).normalized) * rightArm.rotation;
        rightArm.rotation = Quaternion.Slerp(rightArm.rotation, upperTarget, weight);

        Quaternion lowerTarget = Quaternion.FromToRotation(
            rightElbow.TransformDirection(rightLowerAxis),
            (mouthTarget - rightElbow.position).normalized) * rightElbow.rotation;
        rightElbow.rotation = Quaternion.Slerp(rightElbow.rotation, lowerTarget, weight);
    }

    private void PoseBodyRoot(float phase, float dt)
    {
        if (modelRoot == null) return;
        float bob = (PelvisHeight(phase) - .45f) * pelvisRise * .35f * blend;
        float weightShift = Mathf.Cos(phase * Mathf.PI * 2f) * pelvisSway * blend;
        Vector3 target = modelRestPosition + Vector3.up * bob + Vector3.right * weightShift;
        modelRoot.localPosition = Vector3.Lerp(modelRoot.localPosition, target,
            1f - Mathf.Exp(-14f * dt));
        Quaternion bodyTarget = modelRestRotation * Quaternion.Euler(
            -4f * blend, 0f, -Mathf.Cos(phase * Mathf.PI * 2f) * 1.1f * blend);
        modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, bodyTarget,
            1f - Mathf.Exp(-14f * dt));
    }

    private void PoseUpperBody(float phase, float dt)
    {
        float stride = Mathf.Cos(phase * Mathf.PI * 2f) * blend;
        float passing = Mathf.Sin(phase * Mathf.PI * 2f) * blend;
        PoseBodyBone(pelvis, pelvisRest, stride * 2.7f, -.8f * blend,
            -passing * 1.8f, dt);
        PoseBodyBone(chest, chestRest, -stride * 3.5f, -1.2f * blend,
            passing * 2.2f, dt);
        PoseBodyBone(headBone, headRest, stride * 1.25f, 2f * blend,
            -passing * .8f, dt);
    }

    private void PoseBodyBone(Transform bone, Quaternion rest, float yaw,
        float pitch, float roll, float dt)
    {
        if (bone == null || bone.parent == null) return;
        Vector3 facingRight = FacingRight();
        Vector3 facingForward = FacingForward();
        Quaternion baseWorld = bone.parent.rotation * rest;
        Quaternion target = Quaternion.AngleAxis(yaw, transform.up) *
            Quaternion.AngleAxis(-pitch, facingRight) *
            Quaternion.AngleAxis(roll, facingForward) * baseWorld;
        bone.rotation = Quaternion.Slerp(bone.rotation, target, 1f - Mathf.Exp(-14f * dt));
    }

    private void PoseFoot(Transform foot, Quaternion rest, float pitch, float dt)
    {
        if (foot == null || foot.parent == null) return;
        Quaternion baseWorld = foot.parent.rotation * rest;
        Quaternion target = Quaternion.AngleAxis(-pitch, FacingRight()) * baseWorld;
        foot.rotation = Quaternion.Slerp(foot.rotation, target, 1f - Mathf.Exp(-20f * dt));
    }

    // The imported COTL character is authored facing local -Z. Locomotion rotates
    // the avatar so its face follows travel, therefore animation axes must use
    // the visible face direction rather than Transform.forward/right.
    private Vector3 FacingForward()
    {
        Vector3 facing = Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized;
        return facing.sqrMagnitude > .01f ? facing : Vector3.forward;
    }

    private Vector3 FacingRight()
    {
        return Vector3.Cross(Vector3.up, FacingForward()).normalized;
    }

    private void KeepSupportFootGrounded(float phase, float dt)
    {
        if (modelRoot == null || leftFoot == null || rightFoot == null || blend < .01f) return;
        float rootOffset = modelRoot.localPosition.y - modelRestPosition.y;
        float currentLowest = Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y,
            transform.InverseTransformPoint(rightFoot.position).y);
        float lowestWithoutRootOffset = currentLowest - rootOffset;
        float groundedOffset = Mathf.Clamp(restFootHeight - lowestWithoutRootOffset, -.055f, .055f);
        float bodyRhythm = (PelvisHeight(phase) - .45f) * pelvisRise * .18f * blend;
        Vector3 target = modelRoot.localPosition;
        target.y = modelRestPosition.y + groundedOffset * blend + bodyRhythm;
        modelRoot.localPosition = Vector3.Lerp(modelRoot.localPosition, target,
            1f - Mathf.Exp(-22f * dt));
    }

    private static void CacheArmAxis(Transform bone, Transform tip, out Vector3 axis)
    {
        axis = bone != null && tip != null
            ? bone.InverseTransformDirection(tip.position - bone.position).normalized
            : Vector3.up;
    }

    private void PoseWalkingArm(Transform upper, Transform lower, Vector3 upperAxis,
        Vector3 lowerAxis, Quaternion upperRest, Quaternion lowerRest,
        float swing, float side, float dt)
    {
        if (upper == null || lower == null) return;
        if (locomotion.IsFlying)
        {
            upper.localRotation = Quaternion.Slerp(upper.localRotation, upperRest, 1f - Mathf.Exp(-10f * dt));
            lower.localRotation = Quaternion.Slerp(lower.localRotation, lowerRest, 1f - Mathf.Exp(-10f * dt));
            return;
        }
        // Aim the actual shoulder-to-elbow and elbow-to-wrist axes downward.
        // This removes the baked hands-on-hips pose regardless of bone orientation.
        Vector3 facingRight = FacingRight();
        Vector3 relaxedDown = (Vector3.down + facingRight * (side * .075f)).normalized;
        Vector3 upperDirection = Quaternion.AngleAxis(-swing, facingRight) * relaxedDown;
        Quaternion upperTarget = Quaternion.FromToRotation(upper.TransformDirection(upperAxis), upperDirection) * upper.rotation;
        upper.rotation = Quaternion.Slerp(upper.rotation, upperTarget, 1f - Mathf.Exp(-16f * dt));
        float flex = Mathf.Lerp(10f, elbowBend + Mathf.Max(0f, -swing) * .16f, blend);
        // Elbows always flex toward the face/front of the body. Subtracting flex
        // made the forearms point behind the character while the face looked ahead.
        Vector3 lowerDirection = Quaternion.AngleAxis(-(swing + flex), facingRight) * relaxedDown;
        Quaternion lowerTarget = Quaternion.FromToRotation(lower.TransformDirection(lowerAxis), lowerDirection) * lower.rotation;
        lower.rotation = Quaternion.Slerp(lower.rotation, lowerTarget, 1f - Mathf.Exp(-16f * dt));
    }

    // Contact -> down -> passing -> up -> opposite contact. Smooth interpolation
    // preserves the characteristic key poses without the robotic sine-wave look.
    private void SampleWalkPose(float phase, out float stride, out float knee,
        out float footPitch)
    {
        phase = Mathf.Repeat(phase, 1f);
        if (phase < .12f)
        {
            float t = Smooth01(phase / .12f);
            stride = Mathf.Lerp(1f, .72f, t);
            knee = Mathf.Lerp(9f, 16f, t);
            footPitch = Mathf.Lerp(-9f, 0f, t);
        }
        else if (phase < .5f)
        {
            float t = Smooth01((phase - .12f) / .38f);
            stride = Mathf.Lerp(.72f, -1f, t);
            knee = Mathf.Lerp(16f, 7f, t);
            footPitch = Mathf.Lerp(0f, 8f, t);
        }
        else if (phase < .64f)
        {
            float t = Smooth01((phase - .5f) / .14f);
            stride = Mathf.Lerp(-1f, -.7f, t);
            knee = Mathf.Lerp(7f, kneeBend * .55f, t);
            footPitch = Mathf.Lerp(8f, 19f, t);
        }
        else if (phase < .78f)
        {
            float t = Smooth01((phase - .64f) / .14f);
            stride = Mathf.Lerp(-.7f, .05f, t);
            knee = Mathf.Lerp(kneeBend * .55f, kneeBend, t);
            footPitch = Mathf.Lerp(19f, -4f, t);
        }
        else
        {
            float t = Smooth01((phase - .78f) / .22f);
            stride = Mathf.Lerp(.05f, 1f, t);
            knee = Mathf.Lerp(kneeBend, 9f, t);
            footPitch = Mathf.Lerp(-4f, -9f, t);
        }
    }

    private static float PelvisHeight(float phase)
    {
        float step = Mathf.Repeat(phase, .5f) * 2f;
        if (step < .22f) return Mathf.Lerp(.45f, 0f, Smooth01(step / .22f));
        if (step < .52f) return Mathf.Lerp(0f, .72f, Smooth01((step - .22f) / .3f));
        if (step < .78f) return Mathf.Lerp(.72f, 1f, Smooth01((step - .52f) / .26f));
        return Mathf.Lerp(1f, .45f, Smooth01((step - .78f) / .22f));
    }

    private static float Smooth01(float value)
    {
        value = Mathf.Clamp01(value);
        return value * value * (3f - 2f * value);
    }

    private void PoseWalkingLeg(Transform thigh, Transform shin, Vector3 thighAxis,
        Vector3 shinAxis, Quaternion thighRest, Quaternion shinRest,
        float swing, float flex, float dt)
    {
        if (thigh == null || shin == null) return;
        float smoothing = 1f - Mathf.Exp(-18f * dt);
        if (locomotion.IsFlying)
        {
            thigh.localRotation = Quaternion.Slerp(thigh.localRotation, thighRest, smoothing);
            shin.localRotation = Quaternion.Slerp(shin.localRotation, shinRest, smoothing);
            return;
        }
        Vector3 facingRight = FacingRight();
        Vector3 thighDirection = Quaternion.AngleAxis(-swing, facingRight) * Vector3.down;
        Quaternion thighTarget = Quaternion.FromToRotation(thigh.TransformDirection(thighAxis), thighDirection) * thigh.rotation;
        thigh.rotation = Quaternion.Slerp(thigh.rotation, thighTarget, smoothing);
        // The lower leg bends behind the thigh during the passing pose.
        // Adding flex here points the knee forward, opposite to a natural gait.
        Vector3 shinDirection = Quaternion.AngleAxis(-(swing - flex), facingRight) * Vector3.down;
        Quaternion shinTarget = Quaternion.FromToRotation(shin.TransformDirection(shinAxis), shinDirection) * shin.rotation;
        shin.rotation = Quaternion.Slerp(shin.rotation, shinTarget, smoothing);
    }

    private Transform Find(string exactName)
    {
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            if (child.name == exactName) return child;
        return null;
    }
}
