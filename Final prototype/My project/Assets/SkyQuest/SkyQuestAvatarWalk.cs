// ！！！文件功能：角色走路步态、站立脚校正和吃物品的手臂动作！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
[DefaultExecutionOrder(100)]
// 定义 SkyQuestAvatarWalk 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestAvatarWalk : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Walk motion")]
    // Tuned against the supplied side-view walking recording: a brisk but
    // compact stride, hands close to the body, and only a little body bounce.
    // 把 Inspector 可调整的 legSwing（legSwing）设为默认值 23f；具体用途看它在下方的判断和计算。
    public float legSwing = 23f;
    // 把 Inspector 可调整的 armSwing（armSwing）设为默认值 10f；具体用途看它在下方的判断和计算。
    public float armSwing = 10f;
    // 把 Inspector 可调整的 kneeBend（kneeBend）设为默认值 34f；具体用途看它在下方的判断和计算。
    public float kneeBend = 34f;
    // 把 Inspector 可调整的 elbowBend（elbowBend）设为默认值 12f；具体用途看它在下方的判断和计算。
    public float elbowBend = 12f;
    // 把 Inspector 可调整的 stepsPerSecond（stepsPerSecond）设为默认值 1.7f；具体用途看它在下方的判断和计算。
    public float stepsPerSecond = 1.7f;
    // 把 Inspector 可调整的 pelvisRise（pelvisRise）设为默认值 0.018f；具体用途看它在下方的判断和计算。
    public float pelvisRise = 0.018f;
    // 把 Inspector 可调整的 pelvisSway（pelvisSway）设为默认值 0.015f；具体用途看它在下方的判断和计算。
    public float pelvisSway = 0.015f;

    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;
    // 声明 modelRoot（modelRoot）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform modelRoot;
    // 声明 leftArm（leftArm）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform leftArm;
    // 声明 rightArm（rightArm）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform rightArm;
    // 声明 leftLeg（leftLeg）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform leftLeg;
    // 声明 rightLeg（rightLeg）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform rightLeg;
    // 声明 leftKnee（leftKnee）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform leftKnee;
    // 声明 rightKnee（rightKnee）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform rightKnee;
    // 执行 private Transform leftFoot, rightFoot;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Transform leftFoot, rightFoot;
    // 执行 private Transform pelvis, chest, headBone;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Transform pelvis, chest, headBone;
    // 执行 private Vector3 leftThighAxis, rightThighAxis, leftShinAxis, rightShinAxis;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Vector3 leftThighAxis, rightThighAxis, leftShinAxis, rightShinAxis;
    // 执行 private Transform leftElbow, rightElbow, leftWrist, rightWrist;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Transform leftElbow, rightElbow, leftWrist, rightWrist;
    // 执行 private Vector3 leftUpperAxis, rightUpperAxis, leftLowerAxis, rightLowerAxis;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Vector3 leftUpperAxis, rightUpperAxis, leftLowerAxis, rightLowerAxis;
    // 执行 private Quaternion leftElbowRest, rightElbowRest;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Quaternion leftElbowRest, rightElbowRest;
    // leftArmRest 保存leftArmRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion leftArmRest;
    // rightArmRest 保存rightArmRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion rightArmRest;
    // leftLegRest 保存leftLegRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion leftLegRest;
    // rightLegRest 保存rightLegRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion rightLegRest;
    // leftKneeRest 保存leftKneeRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion leftKneeRest;
    // rightKneeRest 保存rightKneeRest（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion rightKneeRest;
    // 执行 private Quaternion leftFootRest, rightFootRest;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Quaternion leftFootRest, rightFootRest;
    // 执行 private Quaternion pelvisRest, chestRest, headRest;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Quaternion pelvisRest, chestRest, headRest;
    // 声明 modelRestPosition（modelRest位置）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 modelRestPosition;
    // modelRestRotation 保存modelRest旋转（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion modelRestRotation;
    // 声明 restFootHeight（restFootHeight）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float restFootHeight;
    // 声明 cycle（cycle）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float cycle;
    // 声明 blend（blend）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float blend;
    // 声明 initialized（initialized）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool initialized;
    // 把数值 -10f 暂存为 eatStarted；它只在当前方法里参与后续判断或计算。
    private float eatStarted = -10f;
    // 定义不可在运行时改变的数值 EatGestureDuration（EatGestureDuration）；后续计算统一使用这个阈值、速度或尺寸。
    private const float EatGestureDuration = 1.15f;

    // 计算 > rightWrist，把结果存进 RightWrist（RightWrist）；之后引用 RightWrist 时用的就是这次结果。
    public Transform RightWrist => rightWrist;

    // ！！！功能：PlayEatGesture——启动角色右手抬到嘴边的进食姿态！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayEatGesture()
    {
        // 步骤 1：调用 CacheRig：CacheRig。
        CacheRig();
        // 步骤 2：根据当前游戏时间计算并保存eatStarted，用于后续延迟或冷却判断。
        eatStarted = Time.time;
    }
    // ！！！功能结束：PlayEatGesture！！！

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 SkyQuestLocomotion owner 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(SkyQuestLocomotion owner)
    {
        // 步骤 1：把计算结果 owner 保存为locomotion；下一条判断或调用会读取这个具体值。
        locomotion = owner;
        // 步骤 2：调用 CacheRig：CacheRig。
        CacheRig();
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查locomotion是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 locomotion = GetComponentInParent<SkyQuestLocomotion>();
        if (locomotion == null) locomotion = GetComponentInParent<SkyQuestLocomotion>();
        // 步骤 2：调用 CacheRig：CacheRig。
        CacheRig();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：CacheRig——CacheRig！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CacheRig()
    {
        // 步骤 1：检查条件 initialized；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (initialized) return;

        // 步骤 2：从目标物体找到所需组件并存为 animator；使用前应检查是否为 null。
        Animator animator = GetComponentInChildren<Animator>(true);
        // 步骤 3：检查animator是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (animator != null && animator.isHuman)
        {
            // 步骤 4：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) 保存为leftArm；下一条判断或调用会读取这个具体值。
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            // 步骤 5：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightUpperArm) 保存为rightArm；下一条判断或调用会读取这个具体值。
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            // 步骤 6：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) 保存为leftElbow；下一条判断或调用会读取这个具体值。
            leftElbow = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            // 步骤 7：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightLowerArm) 保存为rightElbow；下一条判断或调用会读取这个具体值。
            rightElbow = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            // 步骤 8：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftHand) 保存为leftWrist；下一条判断或调用会读取这个具体值。
            leftWrist = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            // 步骤 9：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightHand) 保存为rightWrist；下一条判断或调用会读取这个具体值。
            rightWrist = animator.GetBoneTransform(HumanBodyBones.RightHand);
            // 步骤 10：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) 保存为leftLeg；下一条判断或调用会读取这个具体值。
            leftLeg = animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg);
            // 步骤 11：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) 保存为rightLeg；下一条判断或调用会读取这个具体值。
            rightLeg = animator.GetBoneTransform(HumanBodyBones.RightUpperLeg);
            // 步骤 12：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) 保存为leftKnee；下一条判断或调用会读取这个具体值。
            leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
            // 步骤 13：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) 保存为rightKnee；下一条判断或调用会读取这个具体值。
            rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
            // 步骤 14：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftFoot) 保存为leftFoot；下一条判断或调用会读取这个具体值。
            leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            // 步骤 15：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightFoot) 保存为rightFoot；下一条判断或调用会读取这个具体值。
            rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        }

        // COTL_rigged uses Blender's default generic bone names. The two chains
        // branching from Bone.002 are the arms; the two chains branching from
        // the root Bone are the legs.
        // 步骤 16：把计算结果 Find("Bone.005") 保存为leftArm；下一条判断或调用会读取这个具体值。
        leftArm ??= Find("Bone.005");
        // 步骤 17：把计算结果 Find("Bone.008") 保存为rightArm；下一条判断或调用会读取这个具体值。
        rightArm ??= Find("Bone.008");
        // 步骤 18：把计算结果 Find("Bone.006") 保存为leftElbow；下一条判断或调用会读取这个具体值。
        leftElbow ??= Find("Bone.006");
        // 步骤 19：把计算结果 Find("Bone.009") 保存为rightElbow；下一条判断或调用会读取这个具体值。
        rightElbow ??= Find("Bone.009");
        // 步骤 20：把计算结果 Find("Bone.007") 保存为leftWrist；下一条判断或调用会读取这个具体值。
        leftWrist ??= Find("Bone.007");
        // 步骤 21：把计算结果 Find("Bone.010") 保存为rightWrist；下一条判断或调用会读取这个具体值。
        rightWrist ??= Find("Bone.010");
        // 步骤 22：把计算结果 Find("Bone.011") 保存为leftLeg；下一条判断或调用会读取这个具体值。
        leftLeg ??= Find("Bone.011");
        // 步骤 23：把计算结果 Find("Bone.014") 保存为rightLeg；下一条判断或调用会读取这个具体值。
        rightLeg ??= Find("Bone.014");
        // 步骤 24：把计算结果 Find("Bone.012") 保存为leftKnee；下一条判断或调用会读取这个具体值。
        leftKnee ??= Find("Bone.012");
        // 步骤 25：把计算结果 Find("Bone.015") 保存为rightKnee；下一条判断或调用会读取这个具体值。
        rightKnee ??= Find("Bone.015");
        // 步骤 26：把计算结果 Find("Bone.013") 保存为leftFoot；下一条判断或调用会读取这个具体值。
        leftFoot ??= Find("Bone.013");
        // 步骤 27：把计算结果 Find("Bone.016") 保存为rightFoot；下一条判断或调用会读取这个具体值。
        rightFoot ??= Find("Bone.016");
        // 步骤 28：把计算结果 Find("Bone") 保存为pelvis；下一条判断或调用会读取这个具体值。
        pelvis = Find("Bone");
        // 步骤 29：把计算结果 Find("Bone.002") 保存为chest；下一条判断或调用会读取这个具体值。
        chest = Find("Bone.002");
        // 步骤 30：把计算结果 Find("Bone.004") 保存为headBone；下一条判断或调用会读取这个具体值。
        headBone = Find("Bone.004");

        // 步骤 31：把计算结果 Find("COTL_rigged") 保存为modelRoot；下一条判断或调用会读取这个具体值。
        modelRoot = Find("COTL_rigged");
        // 步骤 32：检查modelRoot是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 modelRoot = transform.GetChild(0);
        if (modelRoot == null && transform.childCount > 0) modelRoot = transform.GetChild(0);
        // 步骤 33：确认modelRoot已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (modelRoot != null)
        {
            // 步骤 34：把计算结果 modelRoot.localPosition 保存为modelRest位置；下一条判断或调用会读取这个具体值。
            modelRestPosition = modelRoot.localPosition;
            // 步骤 35：把计算结果 modelRoot.localRotation 保存为modelRest旋转；下一条判断或调用会读取这个具体值。
            modelRestRotation = modelRoot.localRotation;
        }

        // 步骤 36：确认leftArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftArmRest = leftArm.localRotation;
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        // 步骤 37：确认rightArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightArmRest = rightArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        // 步骤 38：确认leftLeg已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftLegRest = leftLeg.localRotation;
        if (leftLeg != null) leftLegRest = leftLeg.localRotation;
        // 步骤 39：确认rightLeg已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightLegRest = rightLeg.localRotation;
        if (rightLeg != null) rightLegRest = rightLeg.localRotation;
        // 步骤 40：确认leftKnee已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftKneeRest = leftKnee.localRotation;
        if (leftKnee != null) leftKneeRest = leftKnee.localRotation;
        // 步骤 41：确认rightKnee已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightKneeRest = rightKnee.localRotation;
        if (rightKnee != null) rightKneeRest = rightKnee.localRotation;
        // 步骤 42：确认leftFoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftFootRest = leftFoot.localRotation;
        if (leftFoot != null) leftFootRest = leftFoot.localRotation;
        // 步骤 43：确认rightFoot已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightFootRest = rightFoot.localRotation;
        if (rightFoot != null) rightFootRest = rightFoot.localRotation;
        // 步骤 44：确认pelvis已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 pelvisRest = pelvis.localRotation;
        if (pelvis != null) pelvisRest = pelvis.localRotation;
        // 步骤 45：确认chest已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 chestRest = chest.localRotation;
        if (chest != null) chestRest = chest.localRotation;
        // 步骤 46：确认headBone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 headRest = headBone.localRotation;
        if (headBone != null) headRest = headBone.localRotation;
        // 步骤 47：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(leftLeg, leftKnee, out leftThighAxis);
        // 步骤 48：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(rightLeg, rightKnee, out rightThighAxis);
        // 步骤 49：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(leftKnee, leftFoot, out leftShinAxis);
        // 步骤 50：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(rightKnee, rightFoot, out rightShinAxis);
        // 步骤 51：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(leftArm, leftElbow, out leftUpperAxis);
        // 步骤 52：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(rightArm, rightElbow, out rightUpperAxis);
        // 步骤 53：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(leftElbow, leftWrist, out leftLowerAxis);
        // 步骤 54：调用 CacheArmAxis：CacheArmAxis。
        CacheArmAxis(rightElbow, rightWrist, out rightLowerAxis);
        // 步骤 55：确认leftElbow已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftElbowRest = leftElbow.localRotation;
        if (leftElbow != null) leftElbowRest = leftElbow.localRotation;
        // 步骤 56：确认rightElbow已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightElbowRest = rightElbow.localRotation;
        if (rightElbow != null) rightElbowRest = rightElbow.localRotation;
        // 步骤 57：检查leftFoot、rightFoot是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (leftFoot != null && rightFoot != null)
            // 步骤 58：把计算结果 Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y, 保存为restFootHeight；下一条判断或调用会读取这个具体值。
            restFootHeight = Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y,
                // 步骤 59：调用 InverseTransformPoint：Inverse变换Point。
                transform.InverseTransformPoint(rightFoot.position).y);
        // 步骤 60：把计算结果 leftArm != null && rightArm != null && leftLeg != null && rightLeg != null 保存为initialized；下一条判断或调用会读取这个具体值。
        initialized = leftArm != null && rightArm != null && leftLeg != null && rightLeg != null;
    }
    // ！！！功能结束：CacheRig！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：动画在角色本帧位置确定后改局部骨骼姿态，减少行走与视角控制互相覆盖。
    void LateUpdate()
    {
        // 步骤 1：检查locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!initialized || locomotion == null) return;

        // 步骤 2：计算 Mathf.Max(Time.deltaTime, 0.001f)，把结果存进 dt（dt）；之后引用 dt 时用的就是这次结果。
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        // 步骤 3：把计算结果 Mathf.InverseLerp(0.08f, Mathf.Max(0.8f, locomotion.groundSpeed * locomotion.movementScale), 保存为speed01；下一条判断或调用会读取这个具体值。
        float speed01 = Mathf.InverseLerp(0.08f, Mathf.Max(0.8f, locomotion.groundSpeed * locomotion.movementScale),
            // 步骤 4：执行 locomotion.CurrentSpeed);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            locomotion.CurrentSpeed);
        // 步骤 5：用三元表达式为 targetBlend 二选一：问号前条件为真取前值，为假取冒号后的值。
        float targetBlend = locomotion.IsFlying ? 0f : speed01;
        // 步骤 6：让blend以限定的最大变化量逐帧靠近目标值，避免突然改变。
        blend = Mathf.MoveTowards(blend, targetBlend, dt * (targetBlend > blend ? 7f : 5f));

        // 步骤 7：检查条件 blend > 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (blend > 0.001f)
        {
            // A complete cycle contains two footfalls. The reference keeps a
            // steady cadence rather than turning into a sprint at full input.
            // 步骤 8：计算 stepsPerSecond * .5f * Mathf.Lerp(.9f, 1.55f, speed01)，把结果存进 cyclesPerSecond（cyclesPerSecond）；之后引用 cyclesPerSecond 时用的就是这次结果。
            float cyclesPerSecond = stepsPerSecond * .5f * Mathf.Lerp(.9f, 1.55f, speed01);
            // 步骤 9：把计算结果 Mathf.Repeat(cycle + dt * cyclesPerSecond * Mathf.PI * 2f, 保存为cycle；下一条判断或调用会读取这个具体值。
            cycle = Mathf.Repeat(cycle + dt * cyclesPerSecond * Mathf.PI * 2f,
                // 步骤 10：执行 Mathf.PI * 2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Mathf.PI * 2f);
        }

        // 步骤 11：计算 Mathf.Repeat(cycle / (Mathf.PI * 2f), 1f)，把结果存进 phase（phase）；之后引用 phase 时用的就是这次结果。
        float phase = Mathf.Repeat(cycle / (Mathf.PI * 2f), 1f);
        // 步骤 12：调用 SampleWalkPose：SampleWalkPose。
        SampleWalkPose(phase, out float leftStride, out float leftKneePose,
            // 步骤 13：执行 out float leftFootPitch);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            out float leftFootPitch);
        // 步骤 14：调用 SampleWalkPose：SampleWalkPose。
        SampleWalkPose(Mathf.Repeat(phase + .5f, 1f), out float rightStride,
            // 步骤 15：执行 out float rightKneePose, out float rightFootPitch);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            out float rightKneePose, out float rightFootPitch);
        // 步骤 16：调用 PoseBodyRoot：PoseBodyRoot。
        PoseBodyRoot(phase, dt);
        // 步骤 17：调用 PoseUpperBody：PoseUpperBody。
        PoseUpperBody(phase, dt);
        // 步骤 18：计算 Mathf.Lerp(4f, leftKneePose, blend)，把结果存进 leftKneeAmount（leftKneeAmount）；之后引用 leftKneeAmount 时用的就是这次结果。
        float leftKneeAmount = Mathf.Lerp(4f, leftKneePose, blend);
        // 步骤 19：计算 Mathf.Lerp(4f, rightKneePose, blend)，把结果存进 rightKneeAmount（rightKneeAmount）；之后引用 rightKneeAmount 时用的就是这次结果。
        float rightKneeAmount = Mathf.Lerp(4f, rightKneePose, blend);
        // 步骤 20：调用 PoseWalkingLeg：PoseWalkingLeg。
        PoseWalkingLeg(leftLeg, leftKnee, leftThighAxis, leftShinAxis,
            // 步骤 21：执行 leftLegRest, leftKneeRest, leftStride * legSwing * blend, leftKneeAmount, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            leftLegRest, leftKneeRest, leftStride * legSwing * blend, leftKneeAmount, dt);
        // 步骤 22：调用 PoseWalkingLeg：PoseWalkingLeg。
        PoseWalkingLeg(rightLeg, rightKnee, rightThighAxis, rightShinAxis,
            // 步骤 23：执行 rightLegRest, rightKneeRest, rightStride * legSwing * blend, rightKneeAmount, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rightLegRest, rightKneeRest, rightStride * legSwing * blend, rightKneeAmount, dt);
        // 步骤 24：调用 PoseFoot：PoseFoot。
        PoseFoot(leftFoot, leftFootRest, leftFootPitch * blend, dt);
        // 步骤 25：调用 PoseFoot：PoseFoot。
        PoseFoot(rightFoot, rightFootRest, rightFootPitch * blend, dt);
        // 步骤 26：调用 PoseWalkingArm：PoseWalkingArm。
        PoseWalkingArm(leftArm, leftElbow, leftUpperAxis, leftLowerAxis,
            // 步骤 27：执行 leftArmRest, leftElbowRest, -leftStride * armSwing * blend, -1f, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            leftArmRest, leftElbowRest, -leftStride * armSwing * blend, -1f, dt);
        // 步骤 28：调用 PoseWalkingArm：PoseWalkingArm。
        PoseWalkingArm(rightArm, rightElbow, rightUpperAxis, rightLowerAxis,
            // 步骤 29：执行 rightArmRest, rightElbowRest, -rightStride * armSwing * blend, 1f, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rightArmRest, rightElbowRest, -rightStride * armSwing * blend, 1f, dt);

        // 步骤 30：调用 PoseEatingArm：计算进食期间手臂抬起、停顿与放下的姿势。
        PoseEatingArm();

        // 步骤 31：调用 KeepSupportFootGrounded：校正支撑脚高度，减少走路时脚穿地或角色跳动。
        KeepSupportFootGrounded(phase, dt);
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：PoseEatingArm——计算进食期间手臂抬起、停顿与放下的姿势！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PoseEatingArm()
    {
        // 步骤 1：计算 Time.time - eatStarted，把结果存进 elapsed（elapsed）；之后引用 elapsed 时用的就是这次结果。
        float elapsed = Time.time - eatStarted;
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (elapsed < 0f || elapsed >= EatGestureDuration ||
            // 步骤 3：把计算结果 = null || rightElbow == null || headBone == null) return 保存为rightArm；下一条判断或调用会读取这个具体值。
            rightArm == null || rightElbow == null || headBone == null) return;

        // 步骤 4：把计算结果 elapsed < .34f ? Smooth01(elapsed / .34f) : 保存为weight；下一条判断或调用会读取这个具体值。
        float weight = elapsed < .34f ? Smooth01(elapsed / .34f) :
            // 步骤 5：执行 elapsed < .82f ? 1f : 1f - Smooth01((elapsed - .82f) / .33f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            elapsed < .82f ? 1f : 1f - Smooth01((elapsed - .82f) / .33f);
        // 步骤 6：计算 FacingForward()，把结果存进 forward（forward）；之后引用 forward 时用的就是这次结果。
        Vector3 forward = FacingForward();
        // 步骤 7：计算 FacingRight()，把结果存进 right（right）；之后引用 right 时用的就是这次结果。
        Vector3 right = FacingRight();
        // 步骤 8：计算 rightArm.position + forward * .22f + Vector3.down * .13f - right * .11f，把结果存进 elbowTarget（elbowTarget）；之后引用 elbowTarget 时用的就是这次结果。
        Vector3 elbowTarget = rightArm.position + forward * .22f + Vector3.down * .13f - right * .11f;
        // 步骤 9：计算 headBone.position + forward * .15f + Vector3.down * .09f，把结果存进 mouthTarget（mouthTarget）；之后引用 mouthTarget 时用的就是这次结果。
        Vector3 mouthTarget = headBone.position + forward * .15f + Vector3.down * .09f;

        // 步骤 10：把计算结果 Quaternion.FromToRotation( 保存为upperTarget；下一条判断或调用会读取这个具体值。
        Quaternion upperTarget = Quaternion.FromToRotation(
            // 步骤 11：调用 TransformDirection：变换Direction。
            rightArm.TransformDirection(rightUpperAxis),
            // 步骤 12：执行 (elbowTarget - rightArm.position).normalized) * rightArm.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (elbowTarget - rightArm.position).normalized) * rightArm.rotation;
        // 步骤 13：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        rightArm.rotation = Quaternion.Slerp(rightArm.rotation, upperTarget, weight);

        // 步骤 14：把计算结果 Quaternion.FromToRotation( 保存为lowerTarget；下一条判断或调用会读取这个具体值。
        Quaternion lowerTarget = Quaternion.FromToRotation(
            // 步骤 15：调用 TransformDirection：变换Direction。
            rightElbow.TransformDirection(rightLowerAxis),
            // 步骤 16：执行 (mouthTarget - rightElbow.position).normalized) * rightElbow.rotation;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (mouthTarget - rightElbow.position).normalized) * rightElbow.rotation;
        // 步骤 17：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        rightElbow.rotation = Quaternion.Slerp(rightElbow.rotation, lowerTarget, weight);
    }
    // ！！！功能结束：PoseEatingArm！！！

    // ！！！功能：PoseBodyRoot——PoseBodyRoot！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float phase, float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PoseBodyRoot(float phase, float dt)
    {
        // 步骤 1：检查modelRoot是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (modelRoot == null) return;
        // 步骤 2：计算 (PelvisHeight(phase) - .45f) * pelvisRise * .35f * blend，把结果存进 bob（bob）；之后引用 bob 时用的就是这次结果。
        float bob = (PelvisHeight(phase) - .45f) * pelvisRise * .35f * blend;
        // 步骤 3：计算 Mathf.Cos(phase * Mathf.PI * 2f) * pelvisSway * blend，把结果存进 weightShift（weightShift）；之后引用 weightShift 时用的就是这次结果。
        float weightShift = Mathf.Cos(phase * Mathf.PI * 2f) * pelvisSway * blend;
        // 步骤 4：计算 modelRestPosition + Vector3.up * bob + Vector3.right * weightShift，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Vector3 target = modelRestPosition + Vector3.up * bob + Vector3.right * weightShift;
        // 步骤 5：对起点与终点插值来更新local位置；随进度变化呈现连续位移或旋转。
        modelRoot.localPosition = Vector3.Lerp(modelRoot.localPosition, target,
            // 步骤 6：执行 1f - Mathf.Exp(-14f * dt));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1f - Mathf.Exp(-14f * dt));
        // 步骤 7：把计算结果 modelRestRotation * Quaternion.Euler( 保存为bodyTarget；下一条判断或调用会读取这个具体值。
        Quaternion bodyTarget = modelRestRotation * Quaternion.Euler(
            // 步骤 8：执行 -4f * blend, 0f, -Mathf.Cos(phase * Mathf.PI * 2f) * 1.1f * blend);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            -4f * blend, 0f, -Mathf.Cos(phase * Mathf.PI * 2f) * 1.1f * blend);
        // 步骤 9：恢复 modelRoot.localRotation 到初始本地朝向；本地表示相对于父对象。
        modelRoot.localRotation = Quaternion.Slerp(modelRoot.localRotation, bodyTarget,
            // 步骤 10：执行 1f - Mathf.Exp(-14f * dt));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1f - Mathf.Exp(-14f * dt));
    }
    // ！！！功能结束：PoseBodyRoot！！！

    // ！！！功能：PoseUpperBody——PoseUpperBody！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float phase, float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PoseUpperBody(float phase, float dt)
    {
        // 步骤 1：计算 Mathf.Cos(phase * Mathf.PI * 2f) * blend，把结果存进 stride（stride）；之后引用 stride 时用的就是这次结果。
        float stride = Mathf.Cos(phase * Mathf.PI * 2f) * blend;
        // 步骤 2：计算 Mathf.Sin(phase * Mathf.PI * 2f) * blend，把结果存进 passing（passing）；之后引用 passing 时用的就是这次结果。
        float passing = Mathf.Sin(phase * Mathf.PI * 2f) * blend;
        // 步骤 3：调用 PoseBodyBone：PoseBodyBone。
        PoseBodyBone(pelvis, pelvisRest, stride * 2.7f, -.8f * blend,
            // 步骤 4：执行 -passing * 1.8f, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            -passing * 1.8f, dt);
        // 步骤 5：调用 PoseBodyBone：PoseBodyBone。
        PoseBodyBone(chest, chestRest, -stride * 3.5f, -1.2f * blend,
            // 步骤 6：执行 passing * 2.2f, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            passing * 2.2f, dt);
        // 步骤 7：调用 PoseBodyBone：PoseBodyBone。
        PoseBodyBone(headBone, headRest, stride * 1.25f, 2f * blend,
            // 步骤 8：执行 -passing * .8f, dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            -passing * .8f, dt);
    }
    // ！！！功能结束：PoseUpperBody！！！

    // 继续填写上一行调用的参数：private void PoseBodyBone(Transform bone, Quaternion rest, float yaw；参数位置决定它表示的坐标、速度或选项。
    private void PoseBodyBone(Transform bone, Quaternion rest, float yaw,
        // 这一行补充 float pitch, float roll, float dt)，与相邻行合起来完成同一次计算或对象配置。
        float pitch, float roll, float dt)
    {
        // 检查bone、bone的parent是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (bone == null || bone.parent == null) return;
        // 计算 FacingRight()，把结果存进 facingRight（facingRight）；之后引用 facingRight 时用的就是这次结果。
        Vector3 facingRight = FacingRight();
        // 计算 FacingForward()，把结果存进 facingForward（facingForward）；之后引用 facingForward 时用的就是这次结果。
        Vector3 facingForward = FacingForward();
        // 计算 bone.parent.rotation * rest，把结果存进 baseWorld（baseWorld）；之后引用 baseWorld 时用的就是这次结果。
        Quaternion baseWorld = bone.parent.rotation * rest;
        // 把计算结果 Quaternion.AngleAxis(yaw, transform.up) * 保存为target；下一条判断或调用会读取这个具体值。
        Quaternion target = Quaternion.AngleAxis(yaw, transform.up) *
            // 调用 AngleAxis：AngleAxis。
            Quaternion.AngleAxis(-pitch, facingRight) *
            // 调用 AngleAxis：AngleAxis。
            Quaternion.AngleAxis(roll, facingForward) * baseWorld;
        // 对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        bone.rotation = Quaternion.Slerp(bone.rotation, target, 1f - Mathf.Exp(-14f * dt));
    }

    // ！！！功能：PoseFoot——PoseFoot！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform foot, Quaternion rest, float pitch, float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PoseFoot(Transform foot, Quaternion rest, float pitch, float dt)
    {
        // 步骤 1：检查foot、foot的parent是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (foot == null || foot.parent == null) return;
        // 步骤 2：计算 foot.parent.rotation * rest，把结果存进 baseWorld（baseWorld）；之后引用 baseWorld 时用的就是这次结果。
        Quaternion baseWorld = foot.parent.rotation * rest;
        // 步骤 3：计算 Quaternion.AngleAxis(-pitch, FacingRight()) * baseWorld，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Quaternion target = Quaternion.AngleAxis(-pitch, FacingRight()) * baseWorld;
        // 步骤 4：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        foot.rotation = Quaternion.Slerp(foot.rotation, target, 1f - Mathf.Exp(-20f * dt));
    }
    // ！！！功能结束：PoseFoot！！！

    // The imported COTL character is authored facing local -Z. Locomotion rotates
    // the avatar so its face follows travel, therefore animation axes must use
    // the visible face direction rather than Transform.forward/right.
    // ！！！功能：FacingForward——FacingForward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 FacingForward()
    {
        // 步骤 1：把原三维朝向投影到地面平面并存为 facing；行走或转向不受头部俯仰影响。
        Vector3 facing = Vector3.ProjectOnPlane(-transform.forward, Vector3.up).normalized;
        // 步骤 2：把 facing.sqrMagnitude > .01f ? facing : Vector3.forward 作为方法结果返回给调用方；return 后本方法立即结束。
        return facing.sqrMagnitude > .01f ? facing : Vector3.forward;
    }
    // ！！！功能结束：FacingForward！！！

    // ！！！功能：FacingRight——FacingRight！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 FacingRight()
    {
        // 步骤 1：把 Vector3.Cross(Vector3.up, FacingForward()).normalized 作为方法结果返回给调用方；return 后本方法立即结束。
        return Vector3.Cross(Vector3.up, FacingForward()).normalized;
    }
    // ！！！功能结束：FacingRight！！！

    // ！！！功能：KeepSupportFootGrounded——校正支撑脚高度，减少走路时脚穿地或角色跳动！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float phase, float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void KeepSupportFootGrounded(float phase, float dt)
    {
        // 步骤 1：检查modelRoot、leftFoot、rightFoot是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (modelRoot == null || leftFoot == null || rightFoot == null || blend < .01f) return;
        // 步骤 2：计算 modelRoot.localPosition.y - modelRestPosition.y，把结果存进 rootOffset（rootOffset）；之后引用 rootOffset 时用的就是这次结果。
        float rootOffset = modelRoot.localPosition.y - modelRestPosition.y;
        // 步骤 3：把计算结果 Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y, 保存为currentLowest；下一条判断或调用会读取这个具体值。
        float currentLowest = Mathf.Min(transform.InverseTransformPoint(leftFoot.position).y,
            // 步骤 4：调用 InverseTransformPoint：Inverse变换Point。
            transform.InverseTransformPoint(rightFoot.position).y);
        // 步骤 5：计算 currentLowest - rootOffset，把结果存进 lowestWithoutRootOffset（lowestWithoutRootOffset）；之后引用 lowestWithoutRootOffset 时用的就是这次结果。
        float lowestWithoutRootOffset = currentLowest - rootOffset;
        // 步骤 6：计算 groundedOffset 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float groundedOffset = Mathf.Clamp(restFootHeight - lowestWithoutRootOffset, -.055f, .055f);
        // 步骤 7：计算 (PelvisHeight(phase) - .45f) * pelvisRise * .18f * blend，把结果存进 bodyRhythm（bodyRhythm）；之后引用 bodyRhythm 时用的就是这次结果。
        float bodyRhythm = (PelvisHeight(phase) - .45f) * pelvisRise * .18f * blend;
        // 步骤 8：计算 modelRoot.localPosition，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Vector3 target = modelRoot.localPosition;
        // 步骤 9：把计算结果 modelRestPosition.y + groundedOffset * blend + bodyRhythm 保存为y；下一条判断或调用会读取这个具体值。
        target.y = modelRestPosition.y + groundedOffset * blend + bodyRhythm;
        // 步骤 10：对起点与终点插值来更新local位置；随进度变化呈现连续位移或旋转。
        modelRoot.localPosition = Vector3.Lerp(modelRoot.localPosition, target,
            // 步骤 11：执行 1f - Mathf.Exp(-22f * dt));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1f - Mathf.Exp(-22f * dt));
    }
    // ！！！功能结束：KeepSupportFootGrounded！！！

    // ！！！功能：CacheArmAxis——CacheArmAxis！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform bone, Transform tip, out Vector3 axis 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void CacheArmAxis(Transform bone, Transform tip, out Vector3 axis)
    {
        // 步骤 1：把计算结果 bone != null && tip != null 保存为axis；下一条判断或调用会读取这个具体值。
        axis = bone != null && tip != null
            // 步骤 2：三元表达式中，前面的条件为真时选这个值。
            ? bone.InverseTransformDirection(tip.position - bone.position).normalized
            // 步骤 3：三元表达式中，前面的条件为假时选这个备用值。
            : Vector3.up;
    }
    // ！！！功能结束：CacheArmAxis！！！

    // 继续填写上一行调用的参数：private void PoseWalkingArm(Transform upper, Transform lower, Vector3 upperAxis；参数位置决定它表示的坐标、速度或选项。
    private void PoseWalkingArm(Transform upper, Transform lower, Vector3 upperAxis,
        // 继续填写上一行调用的参数：Vector3 lowerAxis, Quaternion upperRest, Quaternion lowerRest；参数位置决定它表示的坐标、速度或选项。
        Vector3 lowerAxis, Quaternion upperRest, Quaternion lowerRest,
        // 这一行补充 float swing, float side, float dt)，与相邻行合起来完成同一次计算或对象配置。
        float swing, float side, float dt)
    {
        // 检查upper、lower是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (upper == null || lower == null) return;
        // 检查条件 locomotion.IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。
        if (locomotion.IsFlying)
        {
            // 恢复 upper.localRotation 到初始本地朝向；本地表示相对于父对象。
            upper.localRotation = Quaternion.Slerp(upper.localRotation, upperRest, 1f - Mathf.Exp(-10f * dt));
            // 恢复 lower.localRotation 到初始本地朝向；本地表示相对于父对象。
            lower.localRotation = Quaternion.Slerp(lower.localRotation, lowerRest, 1f - Mathf.Exp(-10f * dt));
            // 提前结束这个方法；不再执行下方语句。
            return;
        }
        // Aim the actual shoulder-to-elbow and elbow-to-wrist axes downward.
        // This removes the baked hands-on-hips pose regardless of bone orientation.
        // 计算 FacingRight()，把结果存进 facingRight（facingRight）；之后引用 facingRight 时用的就是这次结果。
        Vector3 facingRight = FacingRight();
        // 计算 (Vector3.down + facingRight * (side * .075f)).normalized，把结果存进 relaxedDown（relaxedDown）；之后引用 relaxedDown 时用的就是这次结果。
        Vector3 relaxedDown = (Vector3.down + facingRight * (side * .075f)).normalized;
        // 计算 Quaternion.AngleAxis(-swing, facingRight) * relaxedDown，把结果存进 upperDirection（upperDirection）；之后引用 upperDirection 时用的就是这次结果。
        Vector3 upperDirection = Quaternion.AngleAxis(-swing, facingRight) * relaxedDown;
        // 计算 Quaternion.FromToRotation(upper.TransformDirection(upperAxis), upperDirection) * upper.rot，把结果存进 upperTarget（upperTarget）；之后引用 upperTarget 时用的就是这次结果。
        Quaternion upperTarget = Quaternion.FromToRotation(upper.TransformDirection(upperAxis), upperDirection) * upper.rotation;
        // 对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        upper.rotation = Quaternion.Slerp(upper.rotation, upperTarget, 1f - Mathf.Exp(-16f * dt));
        // 计算 Mathf.Lerp(10f, elbowBend + Mathf.Max(0f, -swing) * .16f, blend)，把结果存进 flex（flex）；之后引用 flex 时用的就是这次结果。
        float flex = Mathf.Lerp(10f, elbowBend + Mathf.Max(0f, -swing) * .16f, blend);
        // Elbows always flex toward the face/front of the body. Subtracting flex
        // made the forearms point behind the character while the face looked ahead.
        // 计算 Quaternion.AngleAxis(-(swing + flex), facingRight) * relaxedDown，把结果存进 lowerDirection（lowerDirection）；之后引用 lowerDirection 时用的就是这次结果。
        Vector3 lowerDirection = Quaternion.AngleAxis(-(swing + flex), facingRight) * relaxedDown;
        // 计算 Quaternion.FromToRotation(lower.TransformDirection(lowerAxis), lowerDirection) * lower.rot，把结果存进 lowerTarget（lowerTarget）；之后引用 lowerTarget 时用的就是这次结果。
        Quaternion lowerTarget = Quaternion.FromToRotation(lower.TransformDirection(lowerAxis), lowerDirection) * lower.rotation;
        // 对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        lower.rotation = Quaternion.Slerp(lower.rotation, lowerTarget, 1f - Mathf.Exp(-16f * dt));
    }

    // Contact -> down -> passing -> up -> opposite contact. Smooth interpolation
    // preserves the characteristic key poses without the robotic sine-wave look.
    // 继续填写上一行调用的参数：private void SampleWalkPose(float phase, out float stride, out float knee；参数位置决定它表示的坐标、速度或选项。
    private void SampleWalkPose(float phase, out float stride, out float knee,
        // 这一行补充 out float footPitch)，与相邻行合起来完成同一次计算或对象配置。
        out float footPitch)
    {
        // 把计算结果 Mathf.Repeat(phase, 1f) 保存为phase；下一条判断或调用会读取这个具体值。
        phase = Mathf.Repeat(phase, 1f);
        // 检查条件 phase < .12f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (phase < .12f)
        {
            // 计算 Smooth01(phase / .12f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Smooth01(phase / .12f);
            // 把计算结果 Mathf.Lerp(1f, .72f, t) 保存为stride；下一条判断或调用会读取这个具体值。
            stride = Mathf.Lerp(1f, .72f, t);
            // 把计算结果 Mathf.Lerp(9f, 16f, t) 保存为knee；下一条判断或调用会读取这个具体值。
            knee = Mathf.Lerp(9f, 16f, t);
            // 把计算结果 Mathf.Lerp(-9f, 0f, t) 保存为footPitch；下一条判断或调用会读取这个具体值。
            footPitch = Mathf.Lerp(-9f, 0f, t);
        }
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 phase < .5f；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (phase < .5f)
        {
            // 计算 Smooth01((phase - .12f) / .38f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Smooth01((phase - .12f) / .38f);
            // 把计算结果 Mathf.Lerp(.72f, -1f, t) 保存为stride；下一条判断或调用会读取这个具体值。
            stride = Mathf.Lerp(.72f, -1f, t);
            // 把计算结果 Mathf.Lerp(16f, 7f, t) 保存为knee；下一条判断或调用会读取这个具体值。
            knee = Mathf.Lerp(16f, 7f, t);
            // 把计算结果 Mathf.Lerp(0f, 8f, t) 保存为footPitch；下一条判断或调用会读取这个具体值。
            footPitch = Mathf.Lerp(0f, 8f, t);
        }
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 phase < .64f；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (phase < .64f)
        {
            // 计算 Smooth01((phase - .5f) / .14f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Smooth01((phase - .5f) / .14f);
            // 把计算结果 Mathf.Lerp(-1f, -.7f, t) 保存为stride；下一条判断或调用会读取这个具体值。
            stride = Mathf.Lerp(-1f, -.7f, t);
            // 把计算结果 Mathf.Lerp(7f, kneeBend * .55f, t) 保存为knee；下一条判断或调用会读取这个具体值。
            knee = Mathf.Lerp(7f, kneeBend * .55f, t);
            // 把计算结果 Mathf.Lerp(8f, 19f, t) 保存为footPitch；下一条判断或调用会读取这个具体值。
            footPitch = Mathf.Lerp(8f, 19f, t);
        }
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 phase < .78f；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (phase < .78f)
        {
            // 计算 Smooth01((phase - .64f) / .14f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Smooth01((phase - .64f) / .14f);
            // 把计算结果 Mathf.Lerp(-.7f, .05f, t) 保存为stride；下一条判断或调用会读取这个具体值。
            stride = Mathf.Lerp(-.7f, .05f, t);
            // 把计算结果 Mathf.Lerp(kneeBend * .55f, kneeBend, t) 保存为knee；下一条判断或调用会读取这个具体值。
            knee = Mathf.Lerp(kneeBend * .55f, kneeBend, t);
            // 把计算结果 Mathf.Lerp(19f, -4f, t) 保存为footPitch；下一条判断或调用会读取这个具体值。
            footPitch = Mathf.Lerp(19f, -4f, t);
        }
        // 前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 计算 Smooth01((phase - .78f) / .22f)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = Smooth01((phase - .78f) / .22f);
            // 把计算结果 Mathf.Lerp(.05f, 1f, t) 保存为stride；下一条判断或调用会读取这个具体值。
            stride = Mathf.Lerp(.05f, 1f, t);
            // 把计算结果 Mathf.Lerp(kneeBend, 9f, t) 保存为knee；下一条判断或调用会读取这个具体值。
            knee = Mathf.Lerp(kneeBend, 9f, t);
            // 把计算结果 Mathf.Lerp(-4f, -9f, t) 保存为footPitch；下一条判断或调用会读取这个具体值。
            footPitch = Mathf.Lerp(-4f, -9f, t);
        }
    }

    // ！！！功能：PelvisHeight——PelvisHeight！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float phase 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private static float PelvisHeight(float phase)
    {
        // 步骤 1：计算 Mathf.Repeat(phase, .5f) * 2f，把结果存进 step（step）；之后引用 step 时用的就是这次结果。
        float step = Mathf.Repeat(phase, .5f) * 2f;
        // 步骤 2：检查条件 step < .22f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (step < .22f) return Mathf.Lerp(.45f, 0f, Smooth01(step / .22f));
        // 步骤 3：检查条件 step < .52f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (step < .52f) return Mathf.Lerp(0f, .72f, Smooth01((step - .22f) / .3f));
        // 步骤 4：检查条件 step < .78f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (step < .78f) return Mathf.Lerp(.72f, 1f, Smooth01((step - .52f) / .26f));
        // 步骤 5：把 Mathf.Lerp(1f, .45f, Smooth01((step - .78f) / .22f)) 作为方法结果返回给调用方；return 后本方法立即结束。
        return Mathf.Lerp(1f, .45f, Smooth01((step - .78f) / .22f));
    }
    // ！！！功能结束：PelvisHeight！！！

    // ！！！功能：Smooth01——Smooth01！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private static float Smooth01(float value)
    {
        // 步骤 1：计算value并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        value = Mathf.Clamp01(value);
        // 步骤 2：把 value * value * (3f - 2f * value) 作为方法结果返回给调用方；return 后本方法立即结束。
        return value * value * (3f - 2f * value);
    }
    // ！！！功能结束：Smooth01！！！

    // 继续填写上一行调用的参数：private void PoseWalkingLeg(Transform thigh, Transform shin, Vector3 thighAxis；参数位置决定它表示的坐标、速度或选项。
    private void PoseWalkingLeg(Transform thigh, Transform shin, Vector3 thighAxis,
        // 继续填写上一行调用的参数：Vector3 shinAxis, Quaternion thighRest, Quaternion shinRest；参数位置决定它表示的坐标、速度或选项。
        Vector3 shinAxis, Quaternion thighRest, Quaternion shinRest,
        // 这一行补充 float swing, float flex, float dt)，与相邻行合起来完成同一次计算或对象配置。
        float swing, float flex, float dt)
    {
        // 检查thigh、shin是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (thigh == null || shin == null) return;
        // 计算 1f - Mathf.Exp(-18f * dt)，把结果存进 smoothing（smoothing）；之后引用 smoothing 时用的就是这次结果。
        float smoothing = 1f - Mathf.Exp(-18f * dt);
        // 检查条件 locomotion.IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。
        if (locomotion.IsFlying)
        {
            // 恢复 thigh.localRotation 到初始本地朝向；本地表示相对于父对象。
            thigh.localRotation = Quaternion.Slerp(thigh.localRotation, thighRest, smoothing);
            // 恢复 shin.localRotation 到初始本地朝向；本地表示相对于父对象。
            shin.localRotation = Quaternion.Slerp(shin.localRotation, shinRest, smoothing);
            // 提前结束这个方法；不再执行下方语句。
            return;
        }
        // 计算 FacingRight()，把结果存进 facingRight（facingRight）；之后引用 facingRight 时用的就是这次结果。
        Vector3 facingRight = FacingRight();
        // 计算 Quaternion.AngleAxis(-swing, facingRight) * Vector3.down，把结果存进 thighDirection（thighDirection）；之后引用 thighDirection 时用的就是这次结果。
        Vector3 thighDirection = Quaternion.AngleAxis(-swing, facingRight) * Vector3.down;
        // 计算 Quaternion.FromToRotation(thigh.TransformDirection(thighAxis), thighDirection) * thigh.rot，把结果存进 thighTarget（thighTarget）；之后引用 thighTarget 时用的就是这次结果。
        Quaternion thighTarget = Quaternion.FromToRotation(thigh.TransformDirection(thighAxis), thighDirection) * thigh.rotation;
        // 对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        thigh.rotation = Quaternion.Slerp(thigh.rotation, thighTarget, smoothing);
        // The lower leg bends behind the thigh during the passing pose.
        // Adding flex here points the knee forward, opposite to a natural gait.
        // 计算 Quaternion.AngleAxis(-(swing - flex), facingRight) * Vector3.down，把结果存进 shinDirection（shinDirection）；之后引用 shinDirection 时用的就是这次结果。
        Vector3 shinDirection = Quaternion.AngleAxis(-(swing - flex), facingRight) * Vector3.down;
        // 计算 Quaternion.FromToRotation(shin.TransformDirection(shinAxis), shinDirection) * shin.rotatio，把结果存进 shinTarget（shinTarget）；之后引用 shinTarget 时用的就是这次结果。
        Quaternion shinTarget = Quaternion.FromToRotation(shin.TransformDirection(shinAxis), shinDirection) * shin.rotation;
        // 对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
        shin.rotation = Quaternion.Slerp(shin.rotation, shinTarget, smoothing);
    }

    // ！！！功能：Find——查找！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string exactName 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private Transform Find(string exactName)
    {
        // 步骤 1：逐个读取 Transform child in GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in GetComponentsInChildren<Transform>(true))
            // 步骤 2：检查条件 child.name == exactName；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
            if (child.name == exactName) return child;
        // 步骤 3：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：Find！！！
}
