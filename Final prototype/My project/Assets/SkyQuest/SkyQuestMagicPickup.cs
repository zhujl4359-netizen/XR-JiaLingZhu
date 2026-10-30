// ！！！文件功能：兑换物的抓取、投掷、回收、进食与 Quest 交互！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections 命名空间；下方可直接使用该库提供的类型和方法。
using System.Collections;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;
// 引入 UnityEngine.UI 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.UI;

// Interaction for every physical reward pulled from an ancestor tree.
// Z picks up/drops one nearby item. Holding Q previews the shared ballistic
// arc; release launches portable items or places setup items at its landing.
// 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
[DefaultExecutionOrder(200)]
// 定义 SkyQuestMagicPickup 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestMagicPickup : MonoBehaviour
{
    // 声明 desktopHead（desktopHead）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform desktopHead;
    // 声明 desktopHand（desktopHand）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform desktopHand;
    // 声明 desktopAvatar（desktopAvatar）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform desktopAvatar;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // 声明 held（held）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool held;
    // 声明 placeable（placeable）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool placeable;
    // 声明 restoresCapeEnergy（restoresCapeEnergy）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool restoresCapeEnergy;
    public int capeEnergyGain = 3;
    // 声明 displayPottery（displayPottery）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool displayPottery;
    // 把 Inspector 可调整的 pickupRange（pickupRange）设为默认值 2.8f；具体用途看它在下方的判断和计算。
    public float pickupRange = 2.8f;
    // 把 Inspector 可调整的 throwSpeed（throwSpeed）设为默认值 9f；具体用途看它在下方的判断和计算。
    public float throwSpeed = 9f;

    // Keep every desktop throw in the same short, low style.  The last
    // preset still gives the player a useful choice without producing the
    // very tall 45/60 degree arcs used by the old version.
    // 计算 { 10f, 13f, 16f, 19f }，把结果存进 AnglePresets（AnglePresets）；之后引用 AnglePresets 时用的就是这次结果。
    private static readonly float[] AnglePresets = { 10f, 13f, 16f, 19f };
    // 定义不可在运行时改变的数值 MaximumLaunchSpeed（MaximumLaunchSpeed）；后续计算统一使用这个阈值、速度或尺寸。
    private const float MaximumLaunchSpeed = 9f;
    // 定义不可在运行时改变的数值 MaximumPreviewDistance（MaximumPreviewDistance）；后续计算统一使用这个阈值、速度或尺寸。
    private const float MaximumPreviewDistance = 8f;
    // 把数值 -1 暂存为 lastZFrame；它只在当前方法里参与后续判断或计算。
    private static int lastZFrame = -1;
    // 把数值 -1 暂存为 lastEFrame；它只在当前方法里参与后续判断或计算。
    private static int lastEFrame = -1;
    // 声明 lastEConsumed（lasteConsumed）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static bool lastEConsumed;
    // 声明 questGripWasDown（questGripWasDown）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static bool questGripWasDown;
    // 声明 questTriggerWasDown（questTriggerWasDown）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static bool questTriggerWasDown;
    // 声明 questTriggerHeldAtPress（questTriggerHeldAtPress）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static bool questTriggerHeldAtPress;
    // questTriggerHeldItem 保存questTriggerHeld物品（类型 SkyQuestMagicPickup）；下方方法依靠它记录或查找场景状态。
    private static SkyQuestMagicPickup questTriggerHeldItem;
    // questTriggerPendingTable 保存questTriggerPendingTable（类型 SkyQuestMagicPickup）；下方方法依靠它记录或查找场景状态。
    private static SkyQuestMagicPickup questTriggerPendingTable;
    // 声明 questTriggerStarted（questTriggerStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static float questTriggerStarted;
    // 把数值 -1f 暂存为 questEatNearFaceSince；它只在当前方法里参与后续判断或计算。
    private static float questEatNearFaceSince = -1f;
    // 声明 questPickupArc（quest拿取Arc）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private static LineRenderer questPickupArc;
    // questTooltipCanvas 保存questTooltipCanvas（类型 Canvas）；下方方法依靠它记录或查找场景状态。
    private static Canvas questTooltipCanvas;
    // questAimProgress 保存questAim进度（类型 TextMeshProUGUI）；下方方法依靠它记录或查找场景状态。
    private static TextMeshProUGUI questAimProgress;

    // 声明 body（body）：物理刚体引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Rigidbody body;
    // 声明 hitCollider（hitCollider）：物理碰撞体；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Collider hitCollider;
    // playerCollider 保存playerCollider（类型 CharacterController）；下方方法依靠它记录或查找场景状态。
    private CharacterController playerCollider;
    // 声明 handHint（handHint）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform handHint;
    // 声明 aimArc（aimArc）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer aimArc;
    // 声明 landingRing（landingRing）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer landingRing;
    // aimLabel 保存aimLabel（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro aimLabel;
    // 声明 carryAnchor（carryAnchor）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform carryAnchor;
    // 声明 firstPersonAnchor（firstPersonAnchor）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform firstPersonAnchor;
    // 声明 firstPersonHand（firstPersonHand）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject firstPersonHand;
    // 声明 firstPersonForearm（firstPersonForearm）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject firstPersonForearm;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;
    // walk 保存walk（类型 SkyQuestAvatarWalk）；下方方法依靠它记录或查找场景状态。
    private SkyQuestAvatarWalk walk;
    // 声明 looseScale（loose缩放）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 looseScale;
    // 声明 consuming（consuming）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool consuming;
    // 声明 consumeStarted（consumeStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float consumeStarted;
    // 声明 consumeName（consumeName）：文字内容；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private string consumeName;
    // consumingItem 保存consuming物品（类型 SkyQuestMagicPickup）；下方方法依靠它记录或查找场景状态。
    private static SkyQuestMagicPickup consumingItem;
    // 定义不可在运行时改变的数值 ConsumeDuration（ConsumeDuration）；后续计算统一使用这个阈值、速度或尺寸。
    private const float ConsumeDuration = .82f;
    // 声明 aiming（aiming）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool aiming;
    // 声明 aimStarted（aimStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float aimStarted;
    // 把数值 16f 暂存为 selectedAngle；它只在当前方法里参与后续判断或计算。
    private float selectedAngle = 16f;
    // 声明 aimLandingPoint（aimLandingPoint）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 aimLandingPoint;
    // 计算 Vector3.up，把结果存进 aimLandingNormal（aimLandingNormal）；之后引用 aimLandingNormal 时用的就是这次结果。
    private Vector3 aimLandingNormal = Vector3.up;
    // 声明 hasAimLanding（hasAimLanding）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool hasAimLanding;
    // 声明 aimLandingTime（aimLandingTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float aimLandingTime;
    // 声明 inFlight（inFlight）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool inFlight;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public static SkyQuestMagicPickup Held { get; private set; }

    // 让 Unity 在应用运行初始化阶段自动调用下方方法；用于清除上次 Play 留下的静态重启状态。
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    // ！！！功能：ResetQuestInput——复位Quest输入！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ResetQuestInput()
    {
        // 步骤 1：清空 Held 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        Held = null;
        // 步骤 2：清空 consumingItem 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        consumingItem = null;
        // 步骤 3：把 questGripWasDown 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        questGripWasDown = false;
        // 步骤 4：把 questTriggerWasDown 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        questTriggerWasDown = false;
        // 步骤 5：把 questTriggerHeldAtPress 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        questTriggerHeldAtPress = false;
        // 步骤 6：清空 questTriggerHeldItem 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        questTriggerHeldItem = null;
        // 步骤 7：清空 questTriggerPendingTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        questTriggerPendingTable = null;
        // 步骤 8：把计算结果 0f 保存为questTriggerStarted；下一条判断或调用会读取这个具体值。
        questTriggerStarted = 0f;
        // 步骤 9：把计算结果 -1f 保存为questEatNearFaceSince；下一条判断或调用会读取这个具体值。
        questEatNearFaceSince = -1f;
        // 步骤 10：清空 questPickupArc 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        questPickupArc = null;
        // 步骤 11：清空 questTooltipCanvas 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        questTooltipCanvas = null;
        // 步骤 12：清空 questAimProgress 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        questAimProgress = null;
    }
    // ！！！功能结束：ResetQuestInput！！！

    // 这些是同一个枚举中的选项，每个名称代表一种可由按钮触发的动作。
    public static void HandleQuestControls(Transform hand, Transform head,
        // 把计算结果 false) 保存为speedSliderCapturesTrigger；下一条判断或调用会读取这个具体值。
        bool speedSliderCapturesTrigger = false)
    {
        // 检查hand、头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (hand == null || head == null) return;
        // 用 grip 记住此处真假结果（SkyQuestInput.Grip(SkyQuestInput.Right) > .72f），供下一条 if 选择执行路径。
        bool grip = SkyQuestInput.Grip(SkyQuestInput.Right) > .72f;
        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (grip && !questGripWasDown && consumingItem == null && questTriggerHeldItem == null &&
            // 调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Left) < .65f && !ExchangeMenuOpen())
        {
            // 检查Held是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Drop：Drop。
            if (Held != null && Held.held) Held.Drop(false);
            // 前面的 if 条件为假时执行这里的备用处理。
            else
            {
                // 计算 FindNearestLoose(hand, 1.15f)，把结果存进 nearby（nearby）；之后引用 nearby 时用的就是这次结果。
                SkyQuestMagicPickup nearby = FindNearestLoose(hand, 1.15f);
                // 确认nearby已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetHeld：设置Held。
                if (nearby != null) nearby.SetHeld(hand);
            }
        }
        // 把计算结果 grip 保存为questGripWasDown；下一条判断或调用会读取这个具体值。
        questGripWasDown = grip;

        // 用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > .7f;
        // 计算 FindFirstObjectByType<SkyQuestCommunication>()，把结果存进 chat（chat）；之后引用 chat 时用的就是这次结果。
        SkyQuestCommunication chat = FindFirstObjectByType<SkyQuestCommunication>();
        // 用 menuOpen 记住此处真假结果（ExchangeMenuOpen()），供下一条 if 选择执行路径。
        bool menuOpen = ExchangeMenuOpen();
        // 用 chatOpen 记住此处真假结果（chat != null && chat.IsOpen），供下一条 if 选择执行路径。
        bool chatOpen = chat != null && chat.IsOpen;
        // 把计算结果 Held != null && Held.held && !Held.displayPottery && 保存为canEatHeld；下一条判断或调用会读取这个具体值。
        bool canEatHeld = Held != null && Held.held && !Held.displayPottery &&
            // 把计算结果 = null && !speedSliderCapturesTrigger && !menuOpen && !chatOpen 保存为consuming物品；下一条判断或调用会读取这个具体值。
            consumingItem == null && !speedSliderCapturesTrigger && !menuOpen && !chatOpen;
        // 用 nearFace 记住此处真假结果（canEatHeld && IsHeldRewardNearFace(hand, head, Held)），供下一条 if 选择执行路径。
        bool nearFace = canEatHeld && IsHeldRewardNearFace(hand, head, Held);
        // 这里用 || 表示任一条件成立就处理：!trigger || !nearFace；用于合并几种停止或跳过原因。 成立时执行 questEatNearFaceSince = -1f;
        if (!trigger || !nearFace) questEatNearFaceSince = -1f;
        // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 questEatNearFaceSince < 0f；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (questEatNearFaceSince < 0f) questEatNearFaceSince = Time.unscaledTime;

        // A reward may be grabbed with the trigger already down. Keep checking
        // while it is held, so moving it to the mouth can still consume it.
        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (trigger && nearFace && (!questTriggerWasDown ||
            // 这一行补充 Time.unscaledTime - questEatNearFaceSince >= .22f))，与相邻行合起来完成同一次计算或对象配置。
            Time.unscaledTime - questEatNearFaceSince >= .22f))
        {
            // 确认questTriggerHeld物品已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (questTriggerHeldItem != null)
            {
                // 把 questTriggerHeldItem.aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                questTriggerHeldItem.aiming = false;
                // 调用 SetAimVisible：设置AimVisible。
                questTriggerHeldItem.SetAimVisible(false);
            }
            // 调用 BeginConsume：开始右手举到嘴边的进食动画和咀嚼反馈。
            Held.BeginConsume();
            // 清空 questTriggerHeldItem 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerHeldItem = null;
            // 清空 questTriggerPendingTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerPendingTable = null;
            // 把 questTriggerHeldAtPress 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            questTriggerHeldAtPress = false;
            // 把计算结果 -1f 保存为questEatNearFaceSince；下一条判断或调用会读取这个具体值。
            questEatNearFaceSince = -1f;
            // 调用 ShowQuestPickupArc：ShowQuest拿取Arc。
            ShowQuestPickupArc(hand, head, null, false);
            // 把 questTriggerWasDown 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            questTriggerWasDown = true;
            // 提前结束这个方法；不再执行下方语句。
            return;
        }

        // 把计算结果 speedSliderCapturesTrigger || menuOpen || consumingItem != null || 保存为blocked；下一条判断或调用会读取这个具体值。
        bool blocked = speedSliderCapturesTrigger || menuOpen || consumingItem != null ||
            // 执行 nearFace || chatOpen;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            nearFace || chatOpen;
        // 检查questTriggerHeld物品是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (blocked && questTriggerHeldItem != null)
        {
            // 把 questTriggerHeldItem.aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            questTriggerHeldItem.aiming = false;
            // 调用 SetAimVisible：设置AimVisible。
            questTriggerHeldItem.SetAimVisible(false);
            // 清空 questTriggerHeldItem 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerHeldItem = null;
        }
        // 检查条件 blocked；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 questTriggerPendingTable = null;
        if (blocked) questTriggerPendingTable = null;

        // All loose rewards, including chat tables, share the same pointer.
        // A short table click opens chat; holding the trigger selects it.
        // 用三元表达式为 aimed 二选一：问号前条件为真取前值，为假取冒号后的值。
        SkyQuestMagicPickup aimed = !blocked && (Held == null || !Held.held) ? FindAimedLoose(hand) : null;
        // 调用 ShowQuestPickupArc：ShowQuest拿取Arc。
        ShowQuestPickupArc(hand, head, aimed,
            // 执行 !menuOpen && !chatOpen && consumingItem == null);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            !menuOpen && !chatOpen && consumingItem == null);
        // 这里用 && 要求两边条件同时成立：trigger && !questTriggerWasDown；任意一边为假就跳过本分支。
        if (trigger && !questTriggerWasDown)
        {
            // 把计算结果 Held != null && Held.held 保存为questTriggerHeldAtPress；下一条判断或调用会读取这个具体值。
            questTriggerHeldAtPress = Held != null && Held.held;
            // 这里的 ! 表示取反；当 blocked 不成立时才进入此分支。
            if (!blocked)
            {
                // 确认aimed已经有值；只有存在时才能安全读取它的属性或调用方法。
                if (aimed != null)
                {
                    // 确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。
                    if (aimed.GetComponent<SkyQuestTeaTable>() != null)
                        // 把计算结果 aimed 保存为questTriggerPendingTable；下一条判断或调用会读取这个具体值。
                        questTriggerPendingTable = aimed;
                    // 前面的 if 条件为假时执行这里的备用处理。
                    else
                    {
                        // 调用 SetHeld：设置Held。
                        aimed.SetHeld(hand);
                        // 把计算结果 aimed 保存为questTriggerHeld物品；下一条判断或调用会读取这个具体值。
                        questTriggerHeldItem = aimed;
                        // 调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                        SkyQuestInput.Pulse(SkyQuestInput.Right, .25f, .05f);
                    }
                }
                // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查Held是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
                else if (Held != null && Held.held) questTriggerHeldItem = Held;
                // 把计算结果 Time.unscaledTime 保存为questTriggerStarted；下一条判断或调用会读取这个具体值。
                questTriggerStarted = Time.unscaledTime;
            }
        }
        // A tree exchange can place a reward in the hand while the trigger is
        // already depressed. Begin its release/aim session when the menu closes.
        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (trigger && questTriggerWasDown && !blocked && questTriggerHeldItem == null &&
            // 把计算结果 = null && Held != null && Held.held) 保存为questTriggerPendingTable；下一条判断或调用会读取这个具体值。
            questTriggerPendingTable == null && Held != null && Held.held)
        {
            // 把计算结果 Held 保存为questTriggerHeld物品；下一条判断或调用会读取这个具体值。
            questTriggerHeldItem = Held;
            // 把 questTriggerHeldAtPress 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            questTriggerHeldAtPress = true;
            // 把计算结果 Time.unscaledTime 保存为questTriggerStarted；下一条判断或调用会读取这个具体值。
            questTriggerStarted = Time.unscaledTime;
        }
        // 这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (trigger && questTriggerPendingTable != null &&
            // 这一行补充 Time.unscaledTime - questTriggerStarted >= .32f)，与相邻行合起来完成同一次计算或对象配置。
            Time.unscaledTime - questTriggerStarted >= .32f)
        {
            // 这里的 ! 表示取反；当 questTriggerPendingTable.held 不成立时才进入此分支。
            if (!questTriggerPendingTable.held)
            {
                // 调用 SetHeld：设置Held。
                questTriggerPendingTable.SetHeld(hand);
                // 把计算结果 questTriggerPendingTable 保存为questTriggerHeld物品；下一条判断或调用会读取这个具体值。
                questTriggerHeldItem = questTriggerPendingTable;
                // 调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right, .25f, .05f);
            }
            // 清空 questTriggerPendingTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerPendingTable = null;
        }
        // 检查questTriggerHeld物品是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (trigger && questTriggerHeldItem != null && questTriggerHeldItem.held)
        {
            // 计算 Time.unscaledTime - questTriggerStarted，把结果存进 heldTime（heldTime）；之后引用 heldTime 时用的就是这次结果。
            float heldTime = Time.unscaledTime - questTriggerStarted;
            // 检查条件 heldTime >= .32f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (heldTime >= .32f)
            {
                // 把 questTriggerHeldItem.aiming 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                questTriggerHeldItem.aiming = true;
                // 计算preset并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
                int preset = Mathf.Clamp(Mathf.FloorToInt((heldTime - .32f) / .65f),
                    // 执行 0, AnglePresets.Length - 1);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    0, AnglePresets.Length - 1);
                // 把计算结果 AnglePresets[preset] 保存为selectedAngle；下一条判断或调用会读取这个具体值。
                questTriggerHeldItem.selectedAngle = AnglePresets[preset];
                // 调用 UpdateAimPreview：更新AimPreview。
                questTriggerHeldItem.UpdateAimPreview(hand, head, "Release trigger");
            }
        }
        // 这里用 && 要求两边条件同时成立：!trigger && questTriggerWasDown；任意一边为假就跳过本分支。
        if (!trigger && questTriggerWasDown)
        {
            // 检查questTriggerHeld物品是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (questTriggerHeldItem != null && questTriggerHeldItem.held)
            {
                // 检查条件 questTriggerHeldItem.aiming；只有为真才运行下面的步骤，否则沿原流程继续。
                if (questTriggerHeldItem.aiming)
                {
                    // 检查条件 questTriggerHeldItem.placeable；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 LaunchPlaceable：把可摆放物品从手中抛出或放到指定位置。
                    if (questTriggerHeldItem.placeable) questTriggerHeldItem.LaunchPlaceable(hand);
                    // 前面的 if 条件为假时执行这里的备用处理。
                    else questTriggerHeldItem.LaunchFromQuest(hand);
                }
                // 上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 questTriggerHeldAtPress；只有为真才运行下面的步骤，否则沿原流程继续。
                else if (questTriggerHeldAtPress) questTriggerHeldItem.Drop(false);
                // 前面的 if 条件为假时执行这里的备用处理。
                else questTriggerHeldItem.SetAimVisible(false);
            }
            // 清空 questTriggerHeldItem 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerHeldItem = null;
            // 清空 questTriggerPendingTable 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            questTriggerPendingTable = null;
            // 把 questTriggerHeldAtPress 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            questTriggerHeldAtPress = false;
        }
        // 把计算结果 trigger 保存为questTriggerWasDown；下一条判断或调用会读取这个具体值。
        questTriggerWasDown = trigger;
    }

    // 继续填写上一行调用的参数：private static bool IsHeldRewardNearFace(Transform hand, Transform head；参数位置决定它表示的坐标、速度或选项。
    private static bool IsHeldRewardNearFace(Transform hand, Transform head,
        // 这一行补充 SkyQuestMagicPickup reward)，与相邻行合起来完成同一次计算或对象配置。
        SkyQuestMagicPickup reward)
    {
        // 计算 head.position + head.forward * .09f - head.up * .12f，把结果存进 mouth（mouth）；之后引用 mouth 时用的就是这次结果。
        Vector3 mouth = head.position + head.forward * .09f - head.up * .12f;
        // 计算 Vector3.Distance(hand.position, mouth)，把结果存进 handDistance（handDistance）；之后引用 handDistance 时用的就是这次结果。
        float handDistance = Vector3.Distance(hand.position, mouth);
        // 计算 hand.position - mouth，把结果存进 handFromMouth（hand从Mouth）；之后引用 handFromMouth 时用的就是这次结果。
        Vector3 handFromMouth = hand.position - mouth;
        // 用 handInFront 记住此处真假结果（Vector3.Dot(handFromMouth, head.forward) > -.2f），供下一条 if 选择执行路径。
        bool handInFront = Vector3.Dot(handFromMouth, head.forward) > -.2f;
        // 计算 Vector3.Dot(handFromMouth, head.up)，把结果存进 handHeight（handHeight）；之后引用 handHeight 时用的就是这次结果。
        float handHeight = Vector3.Dot(handFromMouth, head.up);
        // 这里用 && 要求两边条件同时成立：handInFront && handHeight > -.24f && handDistance <= .48f；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
        if (handInFront && handHeight > -.24f && handDistance <= .48f) return true;

        // Different reward meshes extend different distances beyond the grip.
        // Let their visible surface reach the mouth, but require the hand to
        // remain nearby so large tables cannot be eaten from across the room.
        // 检查reward是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (!handInFront || handHeight < -.55f || handDistance > .75f || reward == null)
            // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        // 逐个读取 Renderer visual in reward.GetComponentsInChildren<Renderer>() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer visual in reward.GetComponentsInChildren<Renderer>())
        {
            // 这里用 || 表示任一条件成立就处理：!visual.enabled || visual is ParticleSystemRenderer || visual is LineRenderer；用于合并几种停止或跳过原因。
            if (!visual.enabled || visual is ParticleSystemRenderer || visual is LineRenderer)
                // 忽略当前无效元素，立刻转到下一轮循环。
                continue;
            // 检查条件 Vector3.Distance(visual.bounds.ClosestPoint(mouth), mouth) <= .16f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (Vector3.Distance(visual.bounds.ClosestPoint(mouth), mouth) <= .16f)
                // 本次检测或激活成功，返回 true 让调用方继续相应反馈。
                return true;
        }
        // 本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }

    // ！！！功能：FindAimedLoose——查找AimedLoose！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform hand 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static SkyQuestMagicPickup FindAimedLoose(Transform hand)
    {
        // 步骤 1：计算 null，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        SkyQuestMagicPickup nearest = null;
        // 步骤 2：把数值 10f 暂存为 best；它只在当前方法里参与后续判断或计算。
        float best = 10f;
        // 步骤 3：把计算结果 Physics.RaycastAll(hand.position, hand.forward, 保存为hits；下一条判断或调用会读取这个具体值。
        RaycastHit[] hits = Physics.RaycastAll(hand.position, hand.forward,
            // 步骤 4：执行 10f, ~0, QueryTriggerInteraction.Ignore);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            10f, ~0, QueryTriggerInteraction.Ignore);
        // 步骤 5：执行 System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        // 步骤 6：逐个读取 RaycastHit hit in hits 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in hits)
        {
            // 步骤 7：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 continue;
            if (hit.collider.GetComponentInParent<SkyQuestLocomotion>() != null) continue;
            // 步骤 8：从目标物体找到所需组件并存为 item；使用前应检查是否为 null。
            SkyQuestMagicPickup item = hit.collider.GetComponentInParent<SkyQuestMagicPickup>();
            // 步骤 9：检查item是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 break; // a wall or other prop occludes the reward
            if (item == null) break; // a wall or other prop occludes the reward
            // 步骤 10：这里用 || 表示任一条件成立就处理：item.held || item.inFlight || hit.distance >= best；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (item.held || item.inFlight || hit.distance >= best) continue;
            // 步骤 11：把计算结果 item 保存为nearest；下一条判断或调用会读取这个具体值。
            nearest = item;
            // 步骤 12：把计算结果 hit.distance 保存为best；下一条判断或调用会读取这个具体值。
            best = hit.distance;
            // 步骤 13：结束当前 switch 分支或循环，不继续落入后面的处理。
            break;
        }
        // 步骤 14：把 nearest 作为方法结果返回给调用方；return 后本方法立即结束。
        return nearest;
    }
    // ！！！功能结束：FindAimedLoose！！！

    // 继续填写上一行调用的参数：private static void ShowQuestPickupArc(Transform hand, Transform head；参数位置决定它表示的坐标、速度或选项。
    private static void ShowQuestPickupArc(Transform hand, Transform head,
        // 这一行补充 SkyQuestMagicPickup item, bool showHeldItem)，与相邻行合起来完成同一次计算或对象配置。
        SkyQuestMagicPickup item, bool showHeldItem)
    {
        // 检查quest拿取Arc是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (questPickupArc == null)
        {
            // 创建新的 GameObject 并存为 arc；构造参数决定其初始内容或对象名称。
            GameObject arc = new GameObject("Right controller pickup arc", typeof(LineRenderer));
            // 从目标 GameObject 查找并保存 quest拿取Arc 组件；后面的移动或显示逻辑会直接使用这个缓存。
            questPickupArc = arc.GetComponent<LineRenderer>();
            // 新建用于shared材质的对象或数值结构，并把构造参数保存进去。
            questPickupArc.sharedMaterial = SkyQuestVisuals.Material(new Color(.66f, .92f, 1f), true);
            // 把positionCount更新为 18；世界或局部位置会立即影响物体摆放。
            questPickupArc.positionCount = 18;
            // 把计算结果 .018f 保存为startWidth；下一条判断或调用会读取这个具体值。
            questPickupArc.startWidth = .018f;
            // 把计算结果 .009f 保存为endWidth；下一条判断或调用会读取这个具体值。
            questPickupArc.endWidth = .009f;
            // 把 questPickupArc.useWorldSpace 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            questPickupArc.useWorldSpace = true;
        }
        // 把计算结果 item != null 保存为enabled；下一条判断或调用会读取这个具体值。
        questPickupArc.enabled = item != null;
        // 用三元表达式为 selected 二选一：问号前条件为真取前值，为假取冒号后的值。
        SkyQuestMagicPickup selected = item != null ? item : questTriggerHeldItem;
        // 检查selected、Held是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 selected = Held;
        if (selected == null && showHeldItem && Held != null && Held.held) selected = Held;
        // 检查selected是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 { HideQuestAimProgress(); return; }
        if (selected == null) { HideQuestAimProgress(); return; }
        // 确认item已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (item != null)
        {
            // 计算 hand.position，把结果存进 start（start）；之后引用 start 时用的就是这次结果。
            Vector3 start = hand.position;
            // 用三元表达式为 end 二选一：问号前条件为真取前值，为假取冒号后的值。
            Vector3 end = item.hitCollider != null ? item.hitCollider.bounds.center : item.transform.position;
            // 按 int i = 0; i < questPickupArc.positionCount; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < questPickupArc.positionCount; i++)
            {
                // 计算 i / (float)(questPickupArc.positionCount - 1)，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
                float t = i / (float)(questPickupArc.positionCount - 1);
                // 设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
                questPickupArc.SetPosition(i, Vector3.Lerp(start, end, t) +
                    // 执行 Vector3.up * (.18f * 4f * t * (1f - t)));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    Vector3.up * (.18f * 4f * t * (1f - t)));
            }
        }
        // 检查questAim进度是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 BuildQuestTooltip：构建QuestTooltip。
        if (questAimProgress == null) BuildQuestTooltip(head);
        // 显示这个场景对象及其子对象；界面会立即反映这一状态。
        questTooltipCanvas.gameObject.SetActive(true);
        // 用 hasBounds 记住此处真假结果（selected.hitCollider != null && selected.hitCollider.enabled），供下一条 if 选择执行路径。
        bool hasBounds = selected.hitCollider != null && selected.hitCollider.enabled;
        // 把计算结果 hasBounds ? 保存为tooltip位置；下一条判断或调用会读取这个具体值。
        Vector3 tooltipPosition = hasBounds ?
            // 执行 selected.hitCollider.bounds.center : selected.transform.position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            selected.hitCollider.bounds.center : selected.transform.position;
        // 把计算结果 hasBounds ? 保存为y；下一条判断或调用会读取这个具体值。
        tooltipPosition.y = hasBounds ?
            // 这一行补充 selected.hitCollider.bounds.max.y + .18f :，与相邻行合起来完成同一次计算或对象配置。
            selected.hitCollider.bounds.max.y + .18f :
            // 执行 selected.transform.position.y + (selected.held ? .28f : .72f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            selected.transform.position.y + (selected.held ? .28f : .72f);
        // 检查条件 selected.held；只有为真才运行下面的步骤，否则沿原流程继续。
        if (selected.held)
        {
            // 计算 tooltipPosition - head.position，把结果存进 fromHead（fromHead）；之后引用 fromHead 时用的就是这次结果。
            Vector3 fromHead = tooltipPosition - head.position;
            // 检查条件 fromHead.sqrMagnitude < 1f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (fromHead.sqrMagnitude < 1f)
                // 把计算结果 head.position + 保存为tooltip位置；下一条判断或调用会读取这个具体值。
                tooltipPosition = head.position +
                    // 执行 (fromHead.sqrMagnitude > .0001f ? fromHead.normalized : head.forward);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (fromHead.sqrMagnitude > .0001f ? fromHead.normalized : head.forward);
        }
        // 取目标对象的 Transform 并存为 tooltip；Transform 提供位置、旋转和父子关系。
        Transform tooltip = questTooltipCanvas.transform;
        // 把position更新为 tooltipPosition；世界或局部位置会立即影响物体摆放。
        tooltip.position = tooltipPosition;
        // 把rotation更新为 Quaternion.LookRotation(tooltipPosition - head.position, Vector3.up)；物体朝向会在本帧改变。
        tooltip.rotation = Quaternion.LookRotation(tooltipPosition - head.position, Vector3.up);
        // 恢复 tooltip.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        tooltip.localScale = Vector3.one * (selected.held ? .00085f :
            // 这是上一行链式调用的下一步 .0015f * Mathf.Clamp(Vector3.Distance(head.position, tooltipPosition) / 3f, 1f, 3f));；前一结果会继续传给这里筛选或处理。
            .0015f * Mathf.Clamp(Vector3.Distance(head.position, tooltipPosition) / 3f, 1f, 3f));
        // 把新内容写入 questAimProgress.text；玩家看到的文字会在本帧更新。
        questAimProgress.text = selected.displayPottery ?
            // 这一行补充 "GRAB: RIGHT TRIGGER\nTHROW: HOLD RIGHT TRIGGER, RELEASE" :，与相邻行合起来完成同一次计算或对象配置。
            "GRAB: RIGHT TRIGGER\nTHROW: HOLD RIGHT TRIGGER, RELEASE" :
            // 这一行补充 (selected.restoresCapeEnergy ? "EAT +" + selected.capeEnergyGain + " ENERGY" : "EAT +5 CANDLES") +，与相邻行合起来完成同一次计算或对象配置。
            (selected.restoresCapeEnergy ? "EAT +" + selected.capeEnergyGain + " ENERGY" : "EAT +5 CANDLES") +
            // 执行 ": RIGHT TRIGGER NEAR FACE\nTHROW: HOLD RIGHT TRIGGER, RELEASE";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ": RIGHT TRIGGER NEAR FACE\nTHROW: HOLD RIGHT TRIGGER, RELEASE";
    }

    // ！！！功能：BuildQuestTooltip——构建QuestTooltip！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform head 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void BuildQuestTooltip(Transform head)
    {
        // 步骤 1：新建用于root的对象或数值结构，并把构造参数保存进去。
        GameObject root = new GameObject("Selected item tooltip",
            // 步骤 2：调用 typeof：typeof。
            typeof(RectTransform), typeof(Canvas));
        // 步骤 3：从目标物体找到所需组件并存为 rect；使用前应检查是否为 null。
        RectTransform rect = root.GetComponent<RectTransform>();
        // 步骤 4：新建用于sizeDelta的对象或数值结构，并把构造参数保存进去。
        rect.sizeDelta = new Vector2(590f, 106f);
        // 步骤 5：从目标 GameObject 查找并保存 questTooltipCanvas 组件；后面的移动或显示逻辑会直接使用这个缓存。
        questTooltipCanvas = root.GetComponent<Canvas>();
        // 步骤 6：把计算结果 RenderMode.WorldSpace 保存为render模式；下一条判断或调用会读取这个具体值。
        questTooltipCanvas.renderMode = RenderMode.WorldSpace;
        // 步骤 7：从目标 GameObject 查找并保存 world相机 组件；后面的移动或显示逻辑会直接使用这个缓存。
        questTooltipCanvas.worldCamera = head.GetComponent<Camera>();
        // 步骤 8：把 questTooltipCanvas.overrideSorting 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        questTooltipCanvas.overrideSorting = true;
        // 步骤 9：把计算结果 90 保存为sortingOrder；下一条判断或调用会读取这个具体值。
        questTooltipCanvas.sortingOrder = 90;

        // 步骤 10：新建用于plate的对象或数值结构，并把构造参数保存进去。
        GameObject plate = new GameObject("Bright interaction plate",
            // 步骤 11：调用 typeof：typeof。
            typeof(RectTransform), typeof(Image));
        // 步骤 12：从目标物体找到所需组件并存为 plateRect；使用前应检查是否为 null。
        RectTransform plateRect = plate.GetComponent<RectTransform>();
        // 步骤 13：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        plateRect.SetParent(rect, false);
        // 步骤 14：把 plateRect.anchorMin 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        plateRect.anchorMin = Vector2.zero;
        // 步骤 15：把计算结果 Vector2.one 保存为anchorMax；下一条判断或调用会读取这个具体值。
        plateRect.anchorMax = Vector2.one;
        // 步骤 16：把计算结果 plateRect.offsetMax = Vector2.zero 保存为offsetMin；下一条判断或调用会读取这个具体值。
        plateRect.offsetMin = plateRect.offsetMax = Vector2.zero;
        // 步骤 17：从目标物体找到所需组件并存为 background；使用前应检查是否为 null。
        Image background = plate.GetComponent<Image>();
        // 步骤 18：新建用于color的对象或数值结构，并把构造参数保存进去。
        background.color = new Color(1f, .9f, .12f, .95f);
        // 步骤 19：把 background.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        background.raycastTarget = false;

        // 步骤 20：新建用于text物品的对象或数值结构，并把构造参数保存进去。
        GameObject textObject = new GameObject("Reward description",
            // 步骤 21：调用 typeof：typeof。
            typeof(RectTransform), typeof(TextMeshProUGUI));
        // 步骤 22：从目标物体找到所需组件并存为 textRect；使用前应检查是否为 null。
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        // 步骤 23：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        textRect.SetParent(rect, false);
        // 步骤 24：把 textRect.anchorMin 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        textRect.anchorMin = Vector2.zero;
        // 步骤 25：把计算结果 Vector2.one 保存为anchorMax；下一条判断或调用会读取这个具体值。
        textRect.anchorMax = Vector2.one;
        // 步骤 26：新建用于offsetMin的对象或数值结构，并把构造参数保存进去。
        textRect.offsetMin = new Vector2(15f, 5f);
        // 步骤 27：新建用于offsetMax的对象或数值结构，并把构造参数保存进去。
        textRect.offsetMax = new Vector2(-15f, -5f);
        // 步骤 28：从目标 GameObject 查找并保存 questAim进度 组件；后面的移动或显示逻辑会直接使用这个缓存。
        questAimProgress = textObject.GetComponent<TextMeshProUGUI>();
        // 步骤 29：把计算结果 25f 保存为fontSize；下一条判断或调用会读取这个具体值。
        questAimProgress.fontSize = 25f;
        // 步骤 30：把计算结果 TextAlignmentOptions.Center 保存为alignment；下一条判断或调用会读取这个具体值。
        questAimProgress.alignment = TextAlignmentOptions.Center;
        // 步骤 31：把计算结果 Color.black 保存为color；下一条判断或调用会读取这个具体值。
        questAimProgress.color = Color.black;
        // 步骤 32：把 questAimProgress.enableWordWrapping 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        questAimProgress.enableWordWrapping = false;
        // 步骤 33：把 questAimProgress.raycastTarget 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        questAimProgress.raycastTarget = false;
        // 步骤 34：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        root.SetActive(false);
    }
    // ！！！功能结束：BuildQuestTooltip！！！

    // ！！！功能：HideQuestAimProgress——HideQuestAim进度！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void HideQuestAimProgress()
    {
        // 步骤 1：确认questTooltipCanvas已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (questTooltipCanvas != null) questTooltipCanvas.gameObject.SetActive(false);
    }
    // ！！！功能结束：HideQuestAimProgress！！！

    // ！！！功能：ExchangeMenuOpen——ExchangeMenuOpen！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private static bool ExchangeMenuOpen()
    {
        // 步骤 1：计算 FindFirstObjectByType<SkyQuestMagicExchange>()，把结果存进 magic（magic）；之后引用 magic 时用的就是这次结果。
        SkyQuestMagicExchange magic = FindFirstObjectByType<SkyQuestMagicExchange>();
        // 步骤 2：计算 FindFirstObjectByType<SkyQuestAncestor>()，把结果存进 ancestor（ancestor）；之后引用 ancestor 时用的就是这次结果。
        SkyQuestAncestor ancestor = FindFirstObjectByType<SkyQuestAncestor>();
        // 步骤 3：把 magic != null && magic.IsMenuOpen || ancestor != null && ancestor.IsMenuOpen 作为方法结果返回给调用方；return 后本方法立即结束。
        return magic != null && magic.IsMenuOpen || ancestor != null && ancestor.IsMenuOpen;
    }
    // ！！！功能结束：ExchangeMenuOpen！！！

    // ！！！功能：TryConsumeWithE——检查可食用的兑换物并开始回收与能量奖励流程！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform viewer 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    // 原理：只有面向可回收兑换物并满足距离条件时才吃掉物品，并把奖励交给能量或蜡烛系统。
    public static bool TryConsumeWithE(Transform viewer)
    {
        // 步骤 1：检查条件 lastEFrame == Time.frameCount；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (lastEFrame == Time.frameCount) return lastEConsumed;
        // 步骤 2：把计算结果 Time.frameCount 保存为lasteFrame；下一条判断或调用会读取这个具体值。
        lastEFrame = Time.frameCount;
        // 步骤 3：把 lastEConsumed 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        lastEConsumed = false;
        // 步骤 4：确认consuming物品已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (consumingItem != null) return lastEConsumed = true;

        // 步骤 5：计算 FindFirstObjectByType<SkyQuestCommunication>()，把结果存进 communication（communication）；之后引用 communication 时用的就是这次结果。
        SkyQuestCommunication communication = FindFirstObjectByType<SkyQuestCommunication>();
        // 步骤 6：检查communication是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (communication != null && communication.IsOpen) return false;

        // 步骤 7：用三元表达式为 target 二选一：问号前条件为真取前值，为假取冒号后的值。
        SkyQuestMagicPickup target = Held != null && Held.held ? Held : FindNearestLoose(viewer);
        // 步骤 8：检查target是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (target == null || target.displayPottery) return false;
        // 步骤 9：调用 BeginConsume：开始右手举到嘴边的进食动画和咀嚼反馈。
        target.BeginConsume();
        // 步骤 10：把 lastEConsumed 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        lastEConsumed = true;
        // 步骤 11：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：TryConsumeWithE！！！

    // ！！！功能：BeginConsume——开始右手举到嘴边的进食动画和咀嚼反馈！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BeginConsume()
    {
        // 步骤 1：这里的 ! 表示取反；当 held 不成立时才进入此分支。 成立时调用 SetHeld：设置Held。
        if (!held) SetHeld(desktopHand != null ? desktopHand : desktopHead);
        // 步骤 2：把计算结果 gameObject.name 保存为consumeName；下一条判断或调用会读取这个具体值。
        consumeName = gameObject.name;
        // 步骤 3：定义固定文字 rewardSuffix（rewardSuffix）；统一引用这段名称或路径，避免各处拼写不一致。
        const string rewardSuffix = " reward";
        // 步骤 4：检查条件 consumeName.EndsWith(rewardSuffix)；只有为真才运行下面的步骤，否则沿原流程继续。
        if (consumeName.EndsWith(rewardSuffix))
            // 步骤 5：把计算结果 consumeName.Substring(0, consumeName.Length - rewardSuffix.Length) 保存为consumeName；下一条判断或调用会读取这个具体值。
            consumeName = consumeName.Substring(0, consumeName.Length - rewardSuffix.Length);
        // 步骤 6：把 consuming 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        consuming = true;
        // 步骤 7：把计算结果 this 保存为consuming物品；下一条判断或调用会读取这个具体值。
        consumingItem = this;
        // 步骤 8：根据当前游戏时间计算并保存consumeStarted，用于后续延迟或冷却判断。
        consumeStarted = Time.time;
        // 步骤 9：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 10：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 11：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
        // 步骤 12：执行 walk?.PlayEatGesture();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        walk?.PlayEatGesture();
        // 步骤 13：执行 FindFirstObjectByType<SkyQuestStatus>()?.PlayCrunch();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        FindFirstObjectByType<SkyQuestStatus>()?.PlayCrunch();
    }
    // ！！！功能结束：BeginConsume！！！

    // ！！！功能：FinishConsume——进食动画结束后移除物品并增加对应数值！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void FinishConsume()
    {
        // 步骤 1：计算 FindFirstObjectByType<SkyQuestStatus>()，把结果存进 status（status）；之后引用 status 时用的就是这次结果。
        SkyQuestStatus status = FindFirstObjectByType<SkyQuestStatus>();
        // 步骤 2：检查条件 restoresCapeEnergy；只有为真才运行下面的步骤，否则沿原流程继续。
        if (restoresCapeEnergy)
        {
            // 步骤 3：计算 FindFirstObjectByType<CapeEnergySystem>()，把结果存进 energy（energy）；之后引用 energy 时用的就是这次结果。
            CapeEnergySystem energy = FindFirstObjectByType<CapeEnergySystem>();
            // 步骤 4：确认energy已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (energy != null)
            {
                // 步骤 5：调用 RestoreEnergy：把指定能量加回斗篷并限制不超过最大值。
                energy.RestoreEnergy(capeEnergyGain);
                // 步骤 6：这一行补充 status?.Show(consumeName + " consumed. +" + capeEnergyGain + " fly energy (" +，与相邻行合起来完成同一次计算或对象配置。
                status?.Show(consumeName + " consumed. +" + capeEnergyGain + " fly energy (" +
                    // 步骤 7：调用 RoundToInt：Round到Int。
                    Mathf.RoundToInt(energy.CurrentEnergy) + "/" + Mathf.RoundToInt(energy.maxEnergy) + ").", 4f);
            }
        }
        // 步骤 8：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 9：把括号里的新元素加入 SkyQuestCandleWallet 集合；后续会按集合顺序使用它。
            SkyQuestCandleWallet.Add(5);
            // 步骤 10：执行 status?.Show(consumeName + " consumed. +5 candles (" + SkyQuestCandleWallet.Balance + ").", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show(consumeName + " consumed. +5 candles (" + SkyQuestCandleWallet.Balance + ").", 4f);
        }
        // 步骤 11：检查条件 consumingItem == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 consumingItem = null;
        if (consumingItem == this) consumingItem = null;
        // 步骤 12：调用 Consume：Consume。
        Consume();
    }
    // ！！！功能结束：FinishConsume！！！

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Awake()
    {
        // 步骤 1：把计算结果 transform.localScale 保存为loose缩放；下一条判断或调用会读取这个具体值。
        looseScale = transform.localScale;
        // 步骤 2：从目标 GameObject 查找并保存 body 组件；后面的移动或显示逻辑会直接使用这个缓存。
        body = gameObject.GetComponent<Rigidbody>();
        // 步骤 3：如果场景对象没有 Rigidbody 组件，就当场创建并保存，保证后续调用有对象可用。
        if (body == null) body = gameObject.AddComponent<Rigidbody>();
        // 步骤 4：把计算结果 .35f 保存为mass；下一条判断或调用会读取这个具体值。
        body.mass = .35f;
        // 步骤 5：把计算结果 .24f 保存为linearDamping；下一条判断或调用会读取这个具体值。
        body.linearDamping = .24f;
        // 步骤 6：把计算结果 .3f 保存为angularDamping；下一条判断或调用会读取这个具体值。
        body.angularDamping = .3f;
        // 步骤 7：把 body.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.useGravity = true;
        // 步骤 8：把计算结果 CollisionDetectionMode.Continuous 保存为collisionDetection模式；下一条判断或调用会读取这个具体值。
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;
        // 步骤 9：从目标 GameObject 查找并保存 hitCollider 组件；后面的移动或显示逻辑会直接使用这个缓存。
        hitCollider = gameObject.GetComponent<Collider>();
        // 步骤 10：检查条件 hitCollider is MeshCollider；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hitCollider is MeshCollider)
        {
            // 步骤 11：把 hitCollider.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            hitCollider.enabled = false;
            // 步骤 12：清空 hitCollider 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            hitCollider = null;
        }
        // 步骤 13：如果场景对象没有 BoxCollider 组件，就当场创建并保存，保证后续调用有对象可用。
        if (hitCollider == null) hitCollider = gameObject.AddComponent<BoxCollider>();
        // 步骤 14：检查条件 hitCollider is BoxCollider box；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hitCollider is BoxCollider box)
            // 步骤 15：调用 FitVisibleBounds：FitVisibleBounds。
            FitVisibleBounds(box);
        // Imported ceramic models can carry small nested colliders that
        // intercept the pointer or interfere with rigidbody throws. The one
        // fitted root collider handles every shape consistently.
        // 步骤 16：逐个读取 Collider child in GetComponentsInChildren<Collider>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Collider child in GetComponentsInChildren<Collider>(true))
            // 步骤 17：检查条件 child != hitCollider；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 child.enabled = false;
            if (child != hitCollider) child.enabled = false;
        // Interaction and aiming visuals are built lazily. Creating all of
        // their primitives during a GUI drag caused a visible one-frame hitch.
    }
    // ！！！功能结束：Awake！！！

    // ！！！功能：FitVisibleBounds——FitVisibleBounds！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 BoxCollider box 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void FitVisibleBounds(BoxCollider box)
    {
        // 步骤 1：创建新的 Vector3 并存为 minimum；构造参数决定其初始内容或对象名称。
        Vector3 minimum = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        // 步骤 2：创建新的 Vector3 并存为 maximum；构造参数决定其初始内容或对象名称。
        Vector3 maximum = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        // 步骤 3：用 found 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool found = false;
        // 步骤 4：逐个读取 Renderer visual in GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer visual in GetComponentsInChildren<Renderer>(true))
        {
            // 步骤 5：检查条件 visual is ParticleSystemRenderer；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (visual is ParticleSystemRenderer) continue;
            // 步骤 6：计算 visual.bounds，把结果存进 bounds（bounds）；之后引用 bounds 时用的就是这次结果。
            Bounds bounds = visual.bounds;
            // 步骤 7：按 int corner = 0; corner < 8; corner++ 变化索引；循环体依次处理每个位置。
            for (int corner = 0; corner < 8; corner++)
            {
                // 步骤 8：新建用于world的对象或数值结构，并把构造参数保存进去。
                Vector3 world = new Vector3((corner & 1) == 0 ? bounds.min.x : bounds.max.x,
                    // 步骤 9：继续填写上一行调用的参数：(corner & 2) == 0 ? bounds.min.y : bounds.max.y；参数位置决定它表示的坐标、速度或选项。
                    (corner & 2) == 0 ? bounds.min.y : bounds.max.y,
                    // 步骤 10：执行 (corner & 4) == 0 ? bounds.min.z : bounds.max.z);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (corner & 4) == 0 ? bounds.min.z : bounds.max.z);
                // 步骤 11：计算 transform.InverseTransformPoint(world)，把结果存进 local（local）；之后引用 local 时用的就是这次结果。
                Vector3 local = transform.InverseTransformPoint(world);
                // 步骤 12：把计算结果 Vector3.Min(minimum, local) 保存为minimum；下一条判断或调用会读取这个具体值。
                minimum = Vector3.Min(minimum, local);
                // 步骤 13：把计算结果 Vector3.Max(maximum, local) 保存为maximum；下一条判断或调用会读取这个具体值。
                maximum = Vector3.Max(maximum, local);
                // 步骤 14：把 found 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                found = true;
            }
        }
        // 步骤 15：这里的 ! 表示取反；当 found 不成立时才进入此分支。
        if (!found)
        {
            // 步骤 16：新建用于center的对象或数值结构，并把构造参数保存进去。
            box.center = new Vector3(0f, .22f, 0f);
            // 步骤 17：新建用于size的对象或数值结构，并把构造参数保存进去。
            box.size = new Vector3(.62f, .72f, .62f);
            // 步骤 18：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 19：把计算结果 (minimum + maximum) * .5f 保存为center；下一条判断或调用会读取这个具体值。
        box.center = (minimum + maximum) * .5f;
        // 步骤 20：计算 maximum - minimum + Vector3.one * .12f，把结果存进 size（size）；之后引用 size 时用的就是这次结果。
        Vector3 size = maximum - minimum + Vector3.one * .12f;
        // 步骤 21：新建用于size的对象或数值结构，并把构造参数保存进去。
        box.size = new Vector3(Mathf.Max(.5f, size.x), Mathf.Max(.65f, size.y),
            // 步骤 22：调用 Max：Max。
            Mathf.Max(.5f, size.z));
    }
    // ！！！功能结束：FitVisibleBounds！！！

    // ！！！功能：AnchorOnDisplay——AnchorOnDisplay！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void AnchorOnDisplay()
    {
        // 步骤 1：检查body是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (body == null) return;
        // 步骤 2：把 displayPottery 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        displayPottery = true;
        // 步骤 3：把刚体平移速度清零，停止先前投掷或下落留下的运动。
        body.linearVelocity = Vector3.zero;
        // 步骤 4：把刚体旋转速度清零，停止先前投掷留下的自转。
        body.angularVelocity = Vector3.zero;
        // 步骤 5：把 body.isKinematic 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.isKinematic = true;
        // 步骤 6：把 body.useGravity 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.useGravity = false;
    }
    // ！！！功能结束：AnchorOnDisplay！！！

    // ！！！功能：SetHeld——设置Held！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform hand 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetHeld(Transform hand)
    {
        // 步骤 1：检查Held是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Drop：Drop。
        if (Held != null && Held != this) Held.Drop(true);
        // 步骤 2：把计算结果 this 保存为Held；下一条判断或调用会读取这个具体值。
        Held = this;
        // 步骤 3：把 held 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        held = true;
        // 步骤 4：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 5：把刚体平移速度清零，停止先前投掷或下落留下的运动。
        body.linearVelocity = Vector3.zero;
        // 步骤 6：把刚体旋转速度清零，停止先前投掷留下的自转。
        body.angularVelocity = Vector3.zero;
        // 步骤 7：把 body.isKinematic 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.isKinematic = true;
        // 步骤 8：把 body.useGravity 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.useGravity = false;
        // 步骤 9：把 body.detectCollisions 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.detectCollisions = false;
        // 步骤 10：确认hitCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hitCollider.enabled = false;
        if (hitCollider != null) hitCollider.enabled = false;
        // 步骤 11：检查locomotion、desktopHead是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (locomotion == null && desktopHead != null)
            // 步骤 12：从目标 GameObject 查找并保存 locomotion 组件；后面的移动或显示逻辑会直接使用这个缓存。
            locomotion = desktopHead.GetComponentInParent<SkyQuestLocomotion>();
        // 步骤 13：检查walk、desktopAvatar是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (walk == null && desktopAvatar != null)
            // 步骤 14：从目标 GameObject 查找并保存 walk 组件；后面的移动或显示逻辑会直接使用这个缓存。
            walk = desktopAvatar.GetComponent<SkyQuestAvatarWalk>();
        // 步骤 15：计算 ResolveCarryTarget(hand)，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Transform target = ResolveCarryTarget(hand);
        // 步骤 16：确认target已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (target != null)
        {
            // 步骤 17：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            transform.SetParent(target, false);
            // 步骤 18：把 transform.localPosition 清为零向量；清除上次移动、投掷或位置偏移的残留值。
            transform.localPosition = Vector3.zero;
            // 步骤 19：恢复 transform.localRotation 到初始本地朝向；本地表示相对于父对象。
            transform.localRotation = Quaternion.Euler(7f, 0f, -8f);
        }
        // 步骤 20：调用 UpdateHeldVisual：更新HeldVisual。
        UpdateHeldVisual();
        // 步骤 21：调用 SetHintVisible：设置HintVisible。
        SetHintVisible(false);
        // 步骤 22：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
    }
    // ！！！功能结束：SetHeld！！！

    // ！！！功能：Release——Release！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool throwForward 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Release(bool throwForward)
    {
        // 步骤 1：检查条件 throwForward；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Launch：Launch。
        if (throwForward) Launch(selectedAngle);
        // 步骤 2：前面的 if 条件为假时执行这里的备用处理。
        else Drop(false);
    }
    // ！！！功能结束：Release！！！

    // ！！！功能：TogglePickup——Toggle拿取！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform hand 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void TogglePickup(Transform hand)
    {
        // 步骤 1：检查条件 held；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Drop：Drop。
        if (held) Drop(false); else SetHeld(hand);
    }
    // ！！！功能结束：TogglePickup！！！

    // ！！！功能：Drop——Drop！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool rollAway 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Drop(bool rollAway)
    {
        // 步骤 1：这里的 ! 表示取反；当 held 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!held) return;
        // 步骤 2：调用 HideFirstPersonHand：HideFirstPersonHand。
        HideFirstPersonHand();
        // 步骤 3：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null, true);
        // 步骤 4：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        transform.localScale = looseScale;
        // 步骤 5：把 held 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        held = false;
        // 步骤 6：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 7：把 body.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.isKinematic = false;
        // 步骤 8：把 body.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.useGravity = true;
        // 步骤 9：把 body.detectCollisions 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.detectCollisions = true;
        // 步骤 10：确认hitCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hitCollider.enabled = true;
        if (hitCollider != null) hitCollider.enabled = true;
        // 步骤 11：调用 IgnorePlayerCollision：Ignore玩家碰撞。
        IgnorePlayerCollision();
        // 步骤 12：检查条件 rollAway；只有为真才运行下面的步骤，否则沿原流程继续。
        if (rollAway)
        {
            // 步骤 13：用三元表达式为 right 二选一：问号前条件为真取前值，为假取冒号后的值。
            Vector3 right = desktopHead != null ? desktopHead.right : Vector3.right;
            // 步骤 14：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
            Vector3 forward = desktopHead != null ? Vector3.ProjectOnPlane(desktopHead.forward, Vector3.up).normalized : Vector3.forward;
            // 步骤 15：把计算结果 right * .9f + forward * .45f + Vector3.up * .3f 保存为linearVelocity；下一条判断或调用会读取这个具体值。
            body.linearVelocity = right * .9f + forward * .45f + Vector3.up * .3f;
            // 步骤 16：新建用于angularVelocity的对象或数值结构，并把构造参数保存进去。
            body.angularVelocity = new Vector3(2.5f, 1.5f, -4.5f);
        }
        // 步骤 17：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 18：把计算结果 Vector3.down * .15f 保存为linearVelocity；下一条判断或调用会读取这个具体值。
            body.linearVelocity = Vector3.down * .15f;
            // 步骤 19：新建用于angularVelocity的对象或数值结构，并把构造参数保存进去。
            body.angularVelocity = new Vector3(.4f, 0f, -.8f);
        }
        // 步骤 20：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 21：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
    }
    // ！！！功能结束：Drop！！！

    // ！！！功能：Launch——Launch！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float angle 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Launch(float angle)
    {
        // 步骤 1：确认desktopHead已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 LaunchWithForward：Launch带有Forward。
        if (desktopHead != null) LaunchWithForward(desktopHead.forward, angle);
    }
    // ！！！功能结束：Launch！！！

    // ！！！功能：LaunchFromQuest——读取手柄方向和速度，把手中物品投掷出去！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform hand 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LaunchFromQuest(Transform hand)
    {
        // 步骤 1：确认hand已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 LaunchWithForward：Launch带有Forward。
        if (hand != null) LaunchWithForward(hand.forward, selectedAngle);
    }
    // ！！！功能结束：LaunchFromQuest！！！

    // Setup props travel along their previewed arc as well, then settle
    // upright at the marked point so tables remain usable for chat.
    // ！！！功能：LaunchPlaceable——把可摆放物品从手中抛出或放到指定位置！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform source 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LaunchPlaceable(Transform source)
    {
        // 步骤 1：检查反馈跟随的来源物品是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!held || source == null) return;
        // 步骤 2：这里用 || 表示任一条件成立就处理：!hasAimLanding || aimLandingNormal.y < .45f；用于合并几种停止或跳过原因。
        if (!hasAimLanding || aimLandingNormal.y < .45f)
        {
            // 步骤 3：调用 Drop：Drop。
            Drop(false);
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 5：把原三维朝向投影到地面平面并存为 forward；行走或转向不受头部俯仰影响。
        Vector3 forward = Vector3.ProjectOnPlane(source.forward, Vector3.up).normalized;
        // 步骤 6：检查条件 forward.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 forward = transform.forward;
        if (forward.sqrMagnitude < .01f) forward = transform.forward;
        // 步骤 7：计算 selectedAngle * Mathf.Deg2Rad，把结果存进 radians（radians）；之后引用 radians 时用的就是这次结果。
        float radians = selectedAngle * Mathf.Deg2Rad;
        // 步骤 8：把计算结果 (forward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * 保存为velocity；下一条判断或调用会读取这个具体值。
        Vector3 velocity = (forward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) *
            // 步骤 9：调用 EffectiveThrowSpeed：EffectiveThrowSpeed。
            EffectiveThrowSpeed();
        // 步骤 10：计算 transform.position，把结果存进 start（start）；之后引用 start 时用的就是这次结果。
        Vector3 start = transform.position;
        // 步骤 11：调用 HideFirstPersonHand：HideFirstPersonHand。
        HideFirstPersonHand();
        // 步骤 12：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null, true);
        // 步骤 13：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        transform.localScale = looseScale;
        // 步骤 14：把 held 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        held = false;
        // 步骤 15：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 16：把 inFlight 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        inFlight = true;
        // 步骤 17：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 18：把 body.isKinematic 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.isKinematic = true;
        // 步骤 19：把 body.useGravity 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.useGravity = false;
        // 步骤 20：把 body.detectCollisions 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.detectCollisions = false;
        // 步骤 21：确认hitCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hitCollider.enabled = false;
        if (hitCollider != null) hitCollider.enabled = false;
        // 步骤 22：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
        // 步骤 23：调用 StartCoroutine：StartCoroutine。
        StartCoroutine(FlyPlaceable(start, velocity, Mathf.Clamp(aimLandingTime, .25f, 2.3f)));
    }
    // ！！！功能结束：LaunchPlaceable！！！

    // ！！！功能：FlyPlaceable——FlyPlaceable！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 start, Vector3 velocity, float duration 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator FlyPlaceable(Vector3 start, Vector3 velocity, float duration)
    {
        // 步骤 1：把数值 0f 暂存为 elapsed；它只在当前方法里参与后续判断或计算。
        float elapsed = 0f;
        // 步骤 2：计算 aimLandingPoint + aimLandingNormal * .04f，把结果存进 landing（landing）；之后引用 landing 时用的就是这次结果。
        Vector3 landing = aimLandingPoint + aimLandingNormal * .04f;
        // 步骤 3：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (elapsed < duration)
        {
            // 步骤 4：把每秒变化率乘本帧秒数，更新elapsed；帧率变化时速度仍接近一致。
            elapsed = Mathf.Min(duration, elapsed + Time.deltaTime);
            // 步骤 5：计算 start + velocity * elapsed + Physics.gravity * (.5f * elapsed * elapsed)，把结果存进 ballistic（ballistic）；之后引用 ballistic 时用的就是这次结果。
            Vector3 ballistic = start + velocity * elapsed + Physics.gravity * (.5f * elapsed * elapsed);
            // 步骤 6：计算 Mathf.InverseLerp(duration * .85f, duration, elapsed)，把结果存进 finish（finish）；之后引用 finish 时用的就是这次结果。
            float finish = Mathf.InverseLerp(duration * .85f, duration, elapsed);
            // 步骤 7：对起点与终点插值来更新position；随进度变化呈现连续位移或旋转。
            transform.position = Vector3.Lerp(ballistic, landing, finish * finish);
            // 步骤 8：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        }
        // 步骤 9：调用 PlaceAtAim：PlaceAtAim。
        PlaceAtAim();
        // 步骤 10：把 inFlight 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        inFlight = false;
    }
    // ！！！功能结束：FlyPlaceable！！！

    // ！！！功能：LaunchWithForward——Launch带有Forward！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 direction, float angle 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LaunchWithForward(Vector3 direction, float angle)
    {
        // 步骤 1：这里的 ! 表示取反；当 held 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!held) return;
        // 步骤 2：调用 HideFirstPersonHand：HideFirstPersonHand。
        HideFirstPersonHand();
        // 步骤 3：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null, true);
        // 步骤 4：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        transform.localScale = looseScale;
        // 步骤 5：把 held 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        held = false;
        // 步骤 6：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 7：把 body.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.isKinematic = false;
        // 步骤 8：把 body.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.useGravity = true;
        // 步骤 9：把 body.detectCollisions 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.detectCollisions = true;
        // The preview is a ballistic arc, so disable drag while airborne to
        // keep the actual throw on that same curve for every reward shape.
        // 步骤 10：把计算结果 0f 保存为linearDamping；下一条判断或调用会读取这个具体值。
        body.linearDamping = 0f;
        // 步骤 11：确认hitCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hitCollider.enabled = true;
        if (hitCollider != null) hitCollider.enabled = true;
        // 步骤 12：调用 IgnorePlayerCollision：Ignore玩家碰撞。
        IgnorePlayerCollision();
        // 步骤 13：把原三维朝向投影到地面平面并存为 flatForward；行走或转向不受头部俯仰影响。
        Vector3 flatForward = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        // 步骤 14：检查条件 flatForward.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 flatForward = transform.forward;
        if (flatForward.sqrMagnitude < .01f) flatForward = transform.forward;
        // 步骤 15：计算 angle * Mathf.Deg2Rad，把结果存进 radians（radians）；之后引用 radians 时用的就是这次结果。
        float radians = angle * Mathf.Deg2Rad;
        // 步骤 16：把计算结果 (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed() 保存为linearVelocity；下一条判断或调用会读取这个具体值。
        body.linearVelocity = (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed();
        // 步骤 17：新建用于angularVelocity的对象或数值结构，并把构造参数保存进去。
        body.angularVelocity = new Vector3(2.2f, 4f, -1.4f);
        // 步骤 18：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 19：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
    }
    // ！！！功能结束：LaunchWithForward！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：先根据菜单和视角状态筛选输入，再分别处理拿取、投掷和进食，防止同一按键触发两种动作。
    void Update()
    {
        // 步骤 1：检查条件 consuming；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (consuming) return;
        // 步骤 2：检查desktopHead是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (desktopHead == null) return;
        // 步骤 3：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。
        if (!desktopMode)
        {
            // 步骤 4：这里的 ! 表示取反；当 held 不成立时才进入此分支。 成立时调用 UpdateLooseHint：更新LooseHint。
            if (!held) UpdateLooseHint();
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 6：计算 Keyboard.current，把结果存进 keyboard（keyboard）；之后引用 keyboard 时用的就是这次结果。
        Keyboard keyboard = Keyboard.current;
        // 步骤 7：检查keyboard是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (keyboard == null) return;

        // 步骤 8：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (keyboard.eKey.wasPressedThisFrame)
            // 步骤 9：调用 TryConsumeWithE：检查可食用的兑换物并开始回收与能量奖励流程。
            TryConsumeWithE(desktopAvatar != null ? desktopAvatar : desktopHead);

        // 步骤 10：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
        if (keyboard.zKey.wasPressedThisFrame && lastZFrame != Time.frameCount)
        {
            // 步骤 11：把计算结果 Time.frameCount 保存为lastzFrame；下一条判断或调用会读取这个具体值。
            lastZFrame = Time.frameCount;
            // 步骤 12：确认Held已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Drop：Drop。
            if (Held != null) Held.Drop(false);
            // 步骤 13：前面的 if 条件为假时执行这里的备用处理。
            else PickNearest(desktopAvatar != null ? desktopAvatar : desktopHead);
        }

        // 步骤 14：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。
        if (Held == this)
        {
            // 步骤 15：只在按键从未按到按下的这一帧触发一次，不会因为持续按住而重复执行。
            if (keyboard.qKey.wasPressedThisFrame)
            {
                // 步骤 16：把 aiming 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                aiming = true;
                // 步骤 17：根据当前游戏时间计算并保存aimStarted，用于后续延迟或冷却判断。
                aimStarted = Time.time;
                // 步骤 18：把计算结果 AnglePresets[0] 保存为selectedAngle；下一条判断或调用会读取这个具体值。
                selectedAngle = AnglePresets[0];
            }
            // 步骤 19：这里用 && 要求两边条件同时成立：aiming && keyboard.qKey.isPressed；任意一边为假就跳过本分支。
            if (aiming && keyboard.qKey.isPressed)
            {
                // 步骤 20：计算 preset 并限制在安全上下界，防止不合理的大小影响移动或显示。
                int preset = Mathf.Clamp(Mathf.FloorToInt((Time.time - aimStarted) / .65f), 0, AnglePresets.Length - 1);
                // 步骤 21：把计算结果 AnglePresets[preset] 保存为selectedAngle；下一条判断或调用会读取这个具体值。
                selectedAngle = AnglePresets[preset];
                // 步骤 22：调用 UpdateAimPreview：更新AimPreview。
                UpdateAimPreview();
            }
            // 步骤 23：只在按键松开的那一帧执行一次，用于放下物品。
            if (aiming && keyboard.qKey.wasReleasedThisFrame)
            {
                // 步骤 24：检查条件 placeable；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 LaunchPlaceable：把可摆放物品从手中抛出或放到指定位置。
                if (placeable) LaunchPlaceable(desktopHead);
                // 步骤 25：前面的 if 条件为假时执行这里的备用处理。
                else Launch(selectedAngle);
            }
        }
        // 步骤 26：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 27：调用 SetAimVisible：设置AimVisible。
            SetAimVisible(false);
            // 步骤 28：调用 UpdateLooseHint：更新LooseHint。
            UpdateLooseHint();
        }
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：UpdateLooseHint——更新LooseHint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateLooseHint()
    {
        // 步骤 1：用三元表达式为 viewer 二选一：问号前条件为真取前值，为假取冒号后的值。
        Transform viewer = desktopAvatar != null ? desktopAvatar : desktopHead;
        // 步骤 2：调用 SetHintVisible：设置HintVisible。
        SetHintVisible(Held == null && FindNearestLoose(viewer) == this);
        // 步骤 3：检查handHint是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (handHint != null && handHint.gameObject.activeSelf)
        {
            // 步骤 4：把position更新为 transform.position + Vector3.up * HintHeight()；世界或局部位置会立即影响物体摆放。
            handHint.position = transform.position + Vector3.up * HintHeight();
            // 步骤 5：把rotation更新为 Quaternion.LookRotation(handHint.position - desktopHead.position, Vector3.up)；物体朝向会在本帧改变。
            handHint.rotation = Quaternion.LookRotation(handHint.position - desktopHead.position, Vector3.up);
        }
    }
    // ！！！功能结束：UpdateLooseHint！！！

    // ！！！功能：IgnorePlayerCollision——Ignore玩家碰撞！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void IgnorePlayerCollision()
    {
        // 步骤 1：检查hitCollider是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (hitCollider == null) return;
        // 步骤 2：检查playerCollider、locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (playerCollider == null && locomotion != null)
            // 步骤 3：从目标 GameObject 查找并保存 playerCollider 组件；后面的移动或显示逻辑会直接使用这个缓存。
            playerCollider = locomotion.GetComponent<CharacterController>();
        // 步骤 4：检查playerCollider是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (playerCollider == null) return;
        // Unity resets IgnoreCollision when a collider is disabled. Reapply it
        // each time this prop leaves the hand. A loose reward is collectible,
        // never a step that should lift the player or snag the camera.
        // 步骤 5：调用 IgnoreCollision：Ignore碰撞。
        Physics.IgnoreCollision(hitCollider, playerCollider, true);
    }
    // ！！！功能结束：IgnorePlayerCollision！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void LateUpdate()
    {
        // 步骤 1：这里的 ! 表示取反；当 held 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!held) return;
        // 步骤 2：调用 UpdateHeldVisual：更新HeldVisual。
        UpdateHeldVisual();
        // 步骤 3：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (consuming && Time.time - consumeStarted >= ConsumeDuration)
            // 步骤 4：调用 FinishConsume：进食动画结束后移除物品并增加对应数值。
            FinishConsume();
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：PickNearest——PickNearest！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform viewer 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void PickNearest(Transform viewer)
    {
        // 步骤 1：计算 FindNearestLoose(viewer)，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        SkyQuestMagicPickup nearest = FindNearestLoose(viewer);
        // 步骤 2：确认nearest已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetHeld：设置Held。
        if (nearest != null) nearest.SetHeld(nearest.desktopHand != null ? nearest.desktopHand : viewer);
    }
    // ！！！功能结束：PickNearest！！！

    // ！！！功能：Consume——Consume！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Consume()
    {
        // 步骤 1：把 held 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        held = false;
        // 步骤 2：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 3：调用 HideFirstPersonHand：HideFirstPersonHand。
        HideFirstPersonHand();
        // 步骤 4：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 5：检查条件 consumingItem == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 consumingItem = null;
        if (consumingItem == this) consumingItem = null;
        // 步骤 6：调用 SetHintVisible：设置HintVisible。
        SetHintVisible(false);
        // 步骤 7：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
        // 步骤 8：调用 Destroy：Destroy。
        Destroy(gameObject);
    }
    // ！！！功能结束：Consume！！！

    // ！！！功能：FindNearestLoose——查找NearestLoose！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform viewer 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static SkyQuestMagicPickup FindNearestLoose(Transform viewer) =>
        // 步骤 1：调用 FindNearestLoose：查找NearestLoose。
        FindNearestLoose(viewer, float.PositiveInfinity);

    // ！！！功能：FindNearestLoose——查找NearestLoose！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform viewer, float maxDistance 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static SkyQuestMagicPickup FindNearestLoose(Transform viewer, float maxDistance)
    {
        // 步骤 1：检查观看者头部或相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (viewer == null) return null;
        // 步骤 2：计算 null，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        SkyQuestMagicPickup nearest = null;
        // 步骤 3：计算 float.PositiveInfinity，把结果存进 best（best）；之后引用 best 时用的就是这次结果。
        float best = float.PositiveInfinity;
        // 步骤 4：逐个读取 SkyQuestMagicPickup item in FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMod 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (SkyQuestMagicPickup item in FindObjectsByType<SkyQuestMagicPickup>(FindObjectsSortMode.None))
        {
            // 步骤 5：检查item是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (item == null || item.held || item.inFlight) continue;
            // 步骤 6：计算 Vector3.Distance(item.transform.position, viewer.position)，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
            float distance = Vector3.Distance(item.transform.position, viewer.position);
            // 步骤 7：这里用 && 要求两边条件同时成立：distance <= Mathf.Min(item.pickupRange, maxDistance) && distance < best；任意一边为假就跳过本分支。
            if (distance <= Mathf.Min(item.pickupRange, maxDistance) && distance < best)
            {
                // 步骤 8：把计算结果 distance 保存为best；下一条判断或调用会读取这个具体值。
                best = distance;
                // 步骤 9：把计算结果 item 保存为nearest；下一条判断或调用会读取这个具体值。
                nearest = item;
            }
        }
        // 步骤 10：把 nearest 作为方法结果返回给调用方；return 后本方法立即结束。
        return nearest;
    }
    // ！！！功能结束：FindNearestLoose！！！

    // ！！！功能：PlaceAtAim——PlaceAtAim！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PlaceAtAim()
    {
        // 步骤 1：检查desktopHead是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (desktopHead == null || !hasAimLanding || aimLandingNormal.y < .45f)
        {
            // 步骤 2：调用 Drop：Drop。
            Drop(false);
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 4：调用 HideFirstPersonHand：HideFirstPersonHand。
        HideFirstPersonHand();
        // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        transform.SetParent(null, true);
        // 步骤 6：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        transform.localScale = looseScale;
        // 步骤 7：把position更新为 aimLandingPoint + aimLandingNormal * .04f；世界或局部位置会立即影响物体摆放。
        transform.position = aimLandingPoint + aimLandingNormal * .04f;
        // 步骤 8：把rotation更新为 Quaternion.Euler(0f, desktopHead.eulerAngles.y, 0f)；物体朝向会在本帧改变。
        transform.rotation = Quaternion.Euler(0f, desktopHead.eulerAngles.y, 0f);
        // 步骤 9：把 held 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        held = false;
        // 步骤 10：把 aiming 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        aiming = false;
        // 步骤 11：把 body.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        body.isKinematic = false;
        // 步骤 12：把 body.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.useGravity = true;
        // 步骤 13：把 body.detectCollisions 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        body.detectCollisions = true;
        // 步骤 14：把刚体平移速度清零，停止先前投掷或下落留下的运动。
        body.linearVelocity = Vector3.zero;
        // 步骤 15：把刚体旋转速度清零，停止先前投掷留下的自转。
        body.angularVelocity = Vector3.zero;
        // 步骤 16：确认hitCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hitCollider.enabled = true;
        if (hitCollider != null) hitCollider.enabled = true;
        // 步骤 17：调用 IgnorePlayerCollision：Ignore玩家碰撞。
        IgnorePlayerCollision();
        // 步骤 18：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 19：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
    }
    // ！！！功能结束：PlaceAtAim！！！

    // ！！！功能：CreateHandHint——创建HandHint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateHandHint()
    {
        // 步骤 1：确认handHint已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (handHint != null) return;
        // 步骤 2：新建用于handHint的对象或数值结构，并把构造参数保存进去。
        handHint = new GameObject("Pickup hand hint").transform;
        // 步骤 3：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        handHint.SetParent(transform, false);
        // 步骤 4：恢复 handHint.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        handHint.localScale = Vector3.one * .82f;
        // 步骤 5：创建新的 Color 并存为 white；构造参数决定其初始内容或对象名称。
        Color white = new Color(1f, .98f, .9f);
        // 步骤 6：调用 AddHintPart：添加HintPart。
        AddHintPart("Palm", PrimitiveType.Sphere, new Vector3(0f, -.02f, 0f), new Vector3(.18f, .22f, .055f), white);
        // 步骤 7：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
            // 步骤 8：调用 AddHintPart：添加HintPart。
            AddHintPart("Finger " + i, PrimitiveType.Capsule, new Vector3(-.105f + i * .07f, .18f + Mathf.Abs(1.5f - i) * .018f, 0f), new Vector3(.035f, .105f + (i == 1 || i == 2 ? .035f : 0f), .035f), white);
        // 步骤 9：创建新的 GameObject 并存为 thumb；构造参数决定其初始内容或对象名称。
        GameObject thumb = AddHintPart("Thumb", PrimitiveType.Capsule, new Vector3(-.17f, .02f, 0f), new Vector3(.04f, .1f, .04f), white);
        // 步骤 10：执行 thumb.transform.localRotation = Quaternion.Euler(0f, 0f, 58f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        thumb.transform.localRotation = Quaternion.Euler(0f, 0f, 58f);
        // 步骤 11：隐藏这个场景对象及其子对象；界面会立即反映这一状态。
        handHint.gameObject.SetActive(false);
    }
    // ！！！功能结束：CreateHandHint！！！

    // The desktop tracked hand is a camera child. A separate first-person
    // anchor keeps the prop in the lower-right of the view; third person uses
    // the visible model's wrist after the walking animation has posed it.
    // ！！！功能：ResolveCarryTarget——ResolveCarryTarget！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform fallback 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private Transform ResolveCarryTarget(Transform fallback)
    {
        // 步骤 1：检查locomotion、desktopHead是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (desktopMode && locomotion != null && !locomotion.ThirdPerson && desktopHead != null)
        {
            // 步骤 2：检查firstPersonAnchor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 CreateFirstPersonAnchor：创建FirstPersonAnchor。
            if (firstPersonAnchor == null) CreateFirstPersonAnchor();
            // 步骤 3：把 firstPersonAnchor 作为方法结果返回给调用方；return 后本方法立即结束。
            return firstPersonAnchor;
        }
        // 步骤 4：检查desktopAvatar、locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (desktopAvatar != null && locomotion != null && locomotion.ThirdPerson)
        {
            // 步骤 5：检查carryAnchor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
            if (carryAnchor == null)
            {
                // 步骤 6：新建用于carryAnchor的对象或数值结构，并把构造参数保存进去。
                carryAnchor = new GameObject("Visible right-hand prop anchor").transform;
                // 步骤 7：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
                carryAnchor.SetParent(desktopAvatar, false);
            }
            // 步骤 8：把 carryAnchor 作为方法结果返回给调用方；return 后本方法立即结束。
            return carryAnchor;
        }
        // 步骤 9：把 fallback != null ? fallback : desktopHand != null ? desktopHand : desktopHead 作为方法结果返回给调用方；return 后本方法立即结束。
        return fallback != null ? fallback : desktopHand != null ? desktopHand : desktopHead;
    }
    // ！！！功能结束：ResolveCarryTarget！！！

    // ！！！功能：UpdateDesktopCarryAnchor——更新桌面CarryAnchor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateDesktopCarryAnchor()
    {
        // 步骤 1：检查carryAnchor、desktopAvatar是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (carryAnchor == null || desktopAvatar == null) return;
        // 步骤 2：把原三维朝向投影到地面平面并存为 facing；行走或转向不受头部俯仰影响。
        Vector3 facing = Vector3.ProjectOnPlane(-desktopAvatar.forward, Vector3.up).normalized;
        // 步骤 3：检查条件 facing.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 facing = Vector3.forward;
        if (facing.sqrMagnitude < .01f) facing = Vector3.forward;
        // 步骤 4：通过叉积算出与原方向垂直的向量，存为 right 供左右移动使用。
        Vector3 right = Vector3.Cross(Vector3.up, facing).normalized;
        // 步骤 5：把计算结果 walk != null && walk.RightWrist != null 保存为rest；下一条判断或调用会读取这个具体值。
        Vector3 rest = walk != null && walk.RightWrist != null
            // 步骤 6：三元表达式中，前面的条件为真时选这个值。
            ? walk.RightWrist.position + facing * .035f
            // 步骤 7：三元表达式中，前面的条件为假时选这个备用值。
            : desktopAvatar.position + Vector3.up * .9f + right * .34f + facing * .1f;
        // 步骤 8：检查walk、walk的RightWrist是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (consuming && (walk == null || walk.RightWrist == null))
        {
            // 步骤 9：把计算结果 Mathf.SmoothStep(0f, 1f, 保存为reach；下一条判断或调用会读取这个具体值。
            float reach = Mathf.SmoothStep(0f, 1f,
                // 步骤 10：调用 Clamp01：Clamp01。
                Mathf.Clamp01((Time.time - consumeStarted) / .35f));
            // 步骤 11：计算 desktopAvatar.position + Vector3.up * 1.36f + facing * .2f，把结果存进 mouth（mouth）；之后引用 mouth 时用的就是这次结果。
            Vector3 mouth = desktopAvatar.position + Vector3.up * 1.36f + facing * .2f;
            // 步骤 12：对起点与终点插值来更新rest；随进度变化呈现连续位移或旋转。
            rest = Vector3.Lerp(rest, mouth, reach);
        }
        // 步骤 13：把position更新为 rest；世界或局部位置会立即影响物体摆放。
        carryAnchor.position = rest;
        // 步骤 14：把rotation更新为 Quaternion.LookRotation(facing, Vector3.up)；物体朝向会在本帧改变。
        carryAnchor.rotation = Quaternion.LookRotation(facing, Vector3.up);
    }
    // ！！！功能结束：UpdateDesktopCarryAnchor！！！

    // ！！！功能：CreateFirstPersonAnchor——创建FirstPersonAnchor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateFirstPersonAnchor()
    {
        // 步骤 1：新建用于firstPersonAnchor的对象或数值结构，并把构造参数保存进去。
        firstPersonAnchor = new GameObject("First-person right-hand prop anchor").transform;
        // 步骤 2：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        firstPersonAnchor.SetParent(desktopHead, false);
        // 步骤 3：新建用于firstPersonHand的对象或数值结构，并把构造参数保存进去。
        firstPersonHand = new GameObject("Visible first-person right hand");
        // 步骤 4：执行 firstPersonHand.transform.SetParent(firstPersonAnchor, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        firstPersonHand.transform.SetParent(firstPersonAnchor, false);
        // 步骤 5：创建新的 Color 并存为 skin；构造参数决定其初始内容或对象名称。
        Color skin = new Color(.86f, .9f, .94f);
        // 步骤 6：创建新的 Color 并存为 sleeve；构造参数决定其初始内容或对象名称。
        Color sleeve = new Color(.64f, .75f, .88f);
        // 步骤 7：把计算结果 SkyQuestVisuals.Primitive("Right palm", PrimitiveType.Sphere, 保存为palm；下一条判断或调用会读取这个具体值。
        GameObject palm = SkyQuestVisuals.Primitive("Right palm", PrimitiveType.Sphere,
            // 步骤 8：继续填写上一行调用的参数：firstPersonHand.transform, new Vector3(.025f, -.035f, -.005f)；参数位置决定它表示的坐标、速度或选项。
            firstPersonHand.transform, new Vector3(.025f, -.035f, -.005f),
            // 步骤 9：用 new 创建 Vector3(.13f, .075f, .12f), skin);；构造参数决定新对象的初始内容。
            new Vector3(.13f, .075f, .12f), skin);
        // 步骤 10：从目标物体找到所需组件并存为 palmCollider；使用前应检查是否为 null。
        Collider palmCollider = palm.GetComponent<Collider>();
        // 步骤 11：确认palmCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 palmCollider.enabled = false;
        if (palmCollider != null) palmCollider.enabled = false;
        // 步骤 12：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(palm);
        // 步骤 13：把计算结果 SkyQuestVisuals.Primitive("Right forearm", PrimitiveType.Capsule, 保存为firstPersonForearm；下一条判断或调用会读取这个具体值。
        firstPersonForearm = SkyQuestVisuals.Primitive("Right forearm", PrimitiveType.Capsule,
            // 步骤 14：执行 desktopHead, Vector3.zero, new Vector3(.08f, .25f, .08f), sleeve);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            desktopHead, Vector3.zero, new Vector3(.08f, .25f, .08f), sleeve);
        // 步骤 15：从目标物体找到所需组件并存为 forearmCollider；使用前应检查是否为 null。
        Collider forearmCollider = firstPersonForearm.GetComponent<Collider>();
        // 步骤 16：确认forearmCollider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 forearmCollider.enabled = false;
        if (forearmCollider != null) forearmCollider.enabled = false;
        // 步骤 17：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(firstPersonForearm);
    }
    // ！！！功能结束：CreateFirstPersonAnchor！！！

    // ！！！功能：HideFirstPersonHand——HideFirstPersonHand！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void HideFirstPersonHand()
    {
        // 步骤 1：确认firstPersonHand已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (firstPersonHand != null) firstPersonHand.SetActive(false);
        // 步骤 2：确认firstPersonForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (firstPersonForearm != null) firstPersonForearm.SetActive(false);
    }
    // ！！！功能结束：HideFirstPersonHand！！！

    // ！！！功能：UpdateHeldVisual——更新HeldVisual！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateHeldVisual()
    {
        // 步骤 1：这里的 ! 表示取反；当 held 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!held) return;
        // 步骤 2：计算 ResolveCarryTarget(desktopHand)，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Transform target = ResolveCarryTarget(desktopHand);
        // 步骤 3：检查target是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (target == null) return;
        // 步骤 4：检查条件 transform.parent != target；只有为真才运行下面的步骤，否则沿原流程继续。
        if (transform.parent != target)
            // 步骤 5：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            transform.SetParent(target, false);
        // 步骤 6：把 transform.localPosition 清为零向量；清除上次移动、投掷或位置偏移的残留值。
        transform.localPosition = Vector3.zero;
        // 步骤 7：恢复 transform.localRotation 到初始本地朝向；本地表示相对于父对象。
        transform.localRotation = Quaternion.Euler(7f, 0f, -8f);

        // 步骤 8：用 firstPerson 记住此处真假结果（desktopMode && target == firstPersonAnchor），供下一条 if 选择执行路径。
        bool firstPerson = desktopMode && target == firstPersonAnchor;
        // 步骤 9：这里的 ! 表示取反；当 firstPerson 不成立时才进入此分支。 成立时调用 HideFirstPersonHand：HideFirstPersonHand。
        if (!firstPerson) HideFirstPersonHand();
        // 步骤 10：检查条件 firstPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (firstPerson)
        {
            // 步骤 11：确认firstPersonHand已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
            if (firstPersonHand != null) firstPersonHand.SetActive(true);
            // 步骤 12：确认firstPersonForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
            if (firstPersonForearm != null) firstPersonForearm.SetActive(true);
            // 步骤 13：把计算结果 consuming ? Mathf.SmoothStep(0f, 1f, 保存为reach；下一条判断或调用会读取这个具体值。
            float reach = consuming ? Mathf.SmoothStep(0f, 1f,
                // 步骤 14：调用 Clamp01：Clamp01。
                Mathf.Clamp01((Time.time - consumeStarted) / .37f)) : 0f;
            // 步骤 15：对起点与终点插值来更新local位置；随进度变化呈现连续位移或旋转。
            firstPersonAnchor.localPosition = Vector3.Lerp(
                // 步骤 16：用 new 创建 Vector3(.39f, -.42f, .82f), new Vector3(.12f, -.15f, .7f), reach);；构造参数决定新对象的初始内容。
                new Vector3(.39f, -.42f, .82f), new Vector3(.12f, -.15f, .7f), reach);
            // 步骤 17：恢复 firstPersonAnchor.localRotation 到初始本地朝向；本地表示相对于父对象。
            firstPersonAnchor.localRotation = Quaternion.Euler(-12f * reach, -8f * reach, 20f * reach);
            // 步骤 18：把计算结果 consuming ? Mathf.Lerp(1f, .38f, 保存为shrink；下一条判断或调用会读取这个具体值。
            float shrink = consuming ? Mathf.Lerp(1f, .38f,
                // 步骤 19：调用 Clamp01：Clamp01。
                Mathf.Clamp01((Time.time - consumeStarted - .52f) / .28f)) : 1f;
            // 步骤 20：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            transform.localScale = looseScale * (.48f * Mathf.Lerp(1f, .66f, reach) * shrink);
            // 步骤 21：确认firstPersonForearm已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (firstPersonForearm != null)
            {
                // 步骤 22：创建新的 Vector3 并存为 basePoint；构造参数决定其初始内容或对象名称。
                Vector3 basePoint = new Vector3(.65f, -.81f, .52f);
                // 步骤 23：创建新的 Vector3 并存为 handPoint；构造参数决定其初始内容或对象名称。
                Vector3 handPoint = firstPersonAnchor.localPosition + new Vector3(.025f, -.04f, 0f);
                // 步骤 24：计算 handPoint - basePoint，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
                Vector3 direction = handPoint - basePoint;
                // 步骤 25：执行 firstPersonForearm.transform.localPosition = (basePoint + handPoint) * .5f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                firstPersonForearm.transform.localPosition = (basePoint + handPoint) * .5f;
                // 步骤 26：执行 firstPersonForearm.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                firstPersonForearm.transform.localRotation = Quaternion.FromToRotation(Vector3.up, direction);
                // 步骤 27：执行 firstPersonForearm.transform.localScale = new Vector3(.08f, direction.magnitude * .5f, .08f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                firstPersonForearm.transform.localScale = new Vector3(.08f, direction.magnitude * .5f, .08f);
            }
        }
        // 步骤 28：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查条件 target == carryAnchor；只有为真才运行下面的步骤，否则沿原流程继续。
        else if (target == carryAnchor)
        {
            // 步骤 29：调用 UpdateDesktopCarryAnchor：更新桌面CarryAnchor。
            UpdateDesktopCarryAnchor();
            // 步骤 30：把计算结果 consuming ? Mathf.Lerp(1f, .2f, 保存为shrink；下一条判断或调用会读取这个具体值。
            float shrink = consuming ? Mathf.Lerp(1f, .2f,
                // 步骤 31：调用 Clamp01：Clamp01。
                Mathf.Clamp01((Time.time - consumeStarted - .54f) / .25f)) : 1f;
            // 步骤 32：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            transform.localScale = looseScale * (.55f * shrink);
        }
        // 步骤 33：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 34：恢复 transform.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            transform.localScale = looseScale * .55f;
            // 步骤 35：检查desktopHead是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (consuming && desktopHead != null)
            {
                // 步骤 36：把计算结果 Mathf.SmoothStep(0f, 1f, 保存为reach；下一条判断或调用会读取这个具体值。
                float reach = Mathf.SmoothStep(0f, 1f,
                    // 步骤 37：调用 Clamp01：Clamp01。
                    Mathf.Clamp01((Time.time - consumeStarted) / .38f));
                // 步骤 38：计算 desktopHead.position + desktopHead.forward * .36f - desktopHead.up * .18f，把结果存进 mouth（mouth）；之后引用 mouth 时用的就是这次结果。
                Vector3 mouth = desktopHead.position + desktopHead.forward * .36f - desktopHead.up * .18f;
                // 步骤 39：对起点与终点插值来更新position；随进度变化呈现连续位移或旋转。
                transform.position = Vector3.Lerp(target.position, mouth, reach);
            }
        }
    }
    // ！！！功能结束：UpdateHeldVisual！！！

    // ！！！功能：AddHintPart——添加HintPart！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject AddHintPart(string name, PrimitiveType type, Vector3 localPosition, Vector3 localScale, Color color)
    {
        // 步骤 1：计算 SkyQuestVisuals.Primitive(name, type, handHint, localPosition, localScale, color, true)，把结果存进 part（part）；之后引用 part 时用的就是这次结果。
        GameObject part = SkyQuestVisuals.Primitive(name, type, handHint, localPosition, localScale, color, true);
        // 步骤 2：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(part);
        // 步骤 3：把 part 作为方法结果返回给调用方；return 后本方法立即结束。
        return part;
    }
    // ！！！功能结束：AddHintPart！！！

    // ！！！功能：HintHeight——HintHeight！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private float HintHeight()
    {
        // 步骤 1：把 hitCollider != null ? Mathf.Max(.7f, hitCollider.bounds.extents.y + .42f) : .9f 作为方法结果返回给调用方；return 后本方法立即结束。
        return hitCollider != null ? Mathf.Max(.7f, hitCollider.bounds.extents.y + .42f) : .9f;
    }
    // ！！！功能结束：HintHeight！！！

    // ！！！功能：SetHintVisible——设置HintVisible！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetHintVisible(bool value)
    {
        // 步骤 1：检查handHint是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 CreateHandHint：创建HandHint。
        if (value && handHint == null) CreateHandHint();
        // 步骤 2：检查handHint是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 SetActive：设置Active。
        if (handHint != null && handHint.gameObject.activeSelf != value) handHint.gameObject.SetActive(value);
    }
    // ！！！功能结束：SetHintVisible！！！

    // ！！！功能：CreateAimPreview——创建AimPreview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateAimPreview()
    {
        // 步骤 1：确认aimArc已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (aimArc != null) return;
        // 步骤 2：创建新的 GameObject 并存为 arcObject；构造参数决定其初始内容或对象名称。
        GameObject arcObject = new GameObject("Launch angle preview");
        // 步骤 3：把计算结果 arcObject.AddComponent<LineRenderer>() 保存为aimArc；下一条判断或调用会读取这个具体值。
        aimArc = arcObject.AddComponent<LineRenderer>();
        // 步骤 4：新建用于material的对象或数值结构，并把构造参数保存进去。
        aimArc.material = SkyQuestVisuals.Material(new Color(1f, .9f, .55f), true);
        // 步骤 5：把计算结果 .035f 保存为startWidth；下一条判断或调用会读取这个具体值。
        aimArc.startWidth = .035f;
        // 步骤 6：把计算结果 .012f 保存为endWidth；下一条判断或调用会读取这个具体值。
        aimArc.endWidth = .012f;
        // 步骤 7：把positionCount更新为 56；世界或局部位置会立即影响物体摆放。
        aimArc.positionCount = 56;
        // 步骤 8：把 aimArc.useWorldSpace 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        aimArc.useWorldSpace = true;
        // 步骤 9：把计算结果 3 保存为numCapVertices；下一条判断或调用会读取这个具体值。
        aimArc.numCapVertices = 3;

        // 步骤 10：创建新的 GameObject 并存为 ringObject；构造参数决定其初始内容或对象名称。
        GameObject ringObject = new GameObject("Launch landing ring");
        // 步骤 11：把计算结果 ringObject.AddComponent<LineRenderer>() 保存为landingRing；下一条判断或调用会读取这个具体值。
        landingRing = ringObject.AddComponent<LineRenderer>();
        // 步骤 12：新建用于material的对象或数值结构，并把构造参数保存进去。
        landingRing.material = SkyQuestVisuals.Material(new Color(1f, .9f, .55f), true);
        // 步骤 13：把计算结果 .075f 保存为startWidth；下一条判断或调用会读取这个具体值。
        landingRing.startWidth = .075f;
        // 步骤 14：把计算结果 .075f 保存为endWidth；下一条判断或调用会读取这个具体值。
        landingRing.endWidth = .075f;
        // 步骤 15：把positionCount更新为 49；世界或局部位置会立即影响物体摆放。
        landingRing.positionCount = 49;
        // 步骤 16：把 landingRing.loop 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        landingRing.loop = true;
        // 步骤 17：把 landingRing.useWorldSpace 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        landingRing.useWorldSpace = true;
        // 步骤 18：把计算结果 3 保存为numCapVertices；下一条判断或调用会读取这个具体值。
        landingRing.numCapVertices = 3;
        // 步骤 19：新建用于aimLabel的对象或数值结构，并把构造参数保存进去。
        aimLabel = SkyQuestVisuals.Text("Launch angle label", "", null, Vector3.zero, 2.5f, new Color(1f, .96f, .78f), 2.4f);
        // 步骤 20：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(false);
    }
    // ！！！功能结束：CreateAimPreview！！！

    // ！！！功能：UpdateAimPreview——更新AimPreview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateAimPreview()
    {
        // 步骤 1：调用 UpdateAimPreview：更新AimPreview。
        UpdateAimPreview(desktopHead, desktopHead, "Q release");
    }
    // ！！！功能结束：UpdateAimPreview！！！

    // ！！！功能：UpdateAimPreview——更新AimPreview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform source, Transform viewer, string releaseHint 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateAimPreview(Transform source, Transform viewer, string releaseHint)
    {
        // 步骤 1：检查aimArc是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时调用 CreateAimPreview：创建AimPreview。
        if (aimArc == null) CreateAimPreview();
        // 步骤 2：检查aimArc、aimLabel、反馈跟随的来源物品、观看者头部或相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (aimArc == null || aimLabel == null || source == null || viewer == null) return;
        // 步骤 3：调用 SetAimVisible：设置AimVisible。
        SetAimVisible(true);
        // 步骤 4：定义不可在运行时改变的数值 maxPoints（maxPoints）；后续计算统一使用这个阈值、速度或尺寸。
        const int maxPoints = 48;
        // 步骤 5：定义不可在运行时改变的数值 step（step）；后续计算统一使用这个阈值、速度或尺寸。
        const float step = .05f;
        // 步骤 6：把positionCount更新为 maxPoints；世界或局部位置会立即影响物体摆放。
        aimArc.positionCount = maxPoints;
        // 步骤 7：把原三维朝向投影到地面平面并存为 flatForward；行走或转向不受头部俯仰影响。
        Vector3 flatForward = Vector3.ProjectOnPlane(source.forward, Vector3.up).normalized;
        // 步骤 8：检查条件 flatForward.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 flatForward = transform.forward;
        if (flatForward.sqrMagnitude < .01f) flatForward = transform.forward;
        // 步骤 9：计算 selectedAngle * Mathf.Deg2Rad，把结果存进 radians（radians）；之后引用 radians 时用的就是这次结果。
        float radians = selectedAngle * Mathf.Deg2Rad;
        // 步骤 10：计算 (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed，把结果存进 velocity（velocity）；之后引用 velocity 时用的就是这次结果。
        Vector3 velocity = (flatForward * Mathf.Cos(radians) + Vector3.up * Mathf.Sin(radians)) * EffectiveThrowSpeed();
        // 步骤 11：计算 transform.position，把结果存进 start（start）；之后引用 start 时用的就是这次结果。
        Vector3 start = transform.position;
        // 步骤 12：计算 start，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Vector3 previous = start;
        // 步骤 13：用 foundLanding 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool foundLanding = false;
        // 步骤 14：计算 default，把结果存进 landing（landing）；之后引用 landing 时用的就是这次结果。
        RaycastHit landing = default;
        // 步骤 15：把数值 1 暂存为 usedPoints；它只在当前方法里参与后续判断或计算。
        int usedPoints = 1;
        // 步骤 16：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
        aimArc.SetPosition(0, start);
        // 步骤 17：按 int i = 1; i < maxPoints; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 1; i < maxPoints; i++)
        {
            // 步骤 18：计算 i * step，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = i * step;
            // 步骤 19：计算 start + velocity * t + Physics.gravity * (.5f * t * t)，把结果存进 next（next）；之后引用 next 时用的就是这次结果。
            Vector3 next = start + velocity * t + Physics.gravity * (.5f * t * t);
            // 步骤 20：检查条件 TryTrajectoryHit(previous, next, out landing)；只有为真才运行下面的步骤，否则沿原流程继续。
            if (TryTrajectoryHit(previous, next, out landing))
            {
                // 步骤 21：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
                aimArc.SetPosition(i, landing.point + landing.normal * .025f);
                // 步骤 22：把计算结果 i + 1 保存为usedPoints；下一条判断或调用会读取这个具体值。
                usedPoints = i + 1;
                // 步骤 23：把 foundLanding 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                foundLanding = true;
                // 步骤 24：把计算结果 t 保存为aimLandingTime；下一条判断或调用会读取这个具体值。
                aimLandingTime = t;
                // 步骤 25：结束当前 switch 分支或循环，不继续落入后面的处理。
                break;
            }
            // 步骤 26：检查条件 Vector3.ProjectOnPlane(next - start, Vector3.up).magnitude >= MaximumPreviewDistance；只有为真才运行下面的步骤，否则沿原流程继续。
            if (Vector3.ProjectOnPlane(next - start, Vector3.up).magnitude >= MaximumPreviewDistance)
            {
                // 步骤 27：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
                aimArc.SetPosition(i, next);
                // 步骤 28：把计算结果 i + 1 保存为usedPoints；下一条判断或调用会读取这个具体值。
                usedPoints = i + 1;
                // 步骤 29：检查条件 TryFindGroundBelow(next, out landing)；只有为真才运行下面的步骤，否则沿原流程继续。
                if (TryFindGroundBelow(next, out landing))
                {
                    // 步骤 30：把 foundLanding 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                    foundLanding = true;
                    // 步骤 31：把计算结果 t 保存为aimLandingTime；下一条判断或调用会读取这个具体值。
                    aimLandingTime = t;
                }
                // 步骤 32：结束当前 switch 分支或循环，不继续落入后面的处理。
                break;
            }
            // 步骤 33：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            aimArc.SetPosition(i, next);
            // 步骤 34：把计算结果 i + 1 保存为usedPoints；下一条判断或调用会读取这个具体值。
            usedPoints = i + 1;
            // 步骤 35：把计算结果 next 保存为previous；下一条判断或调用会读取这个具体值。
            previous = next;
        }
        // 步骤 36：把positionCount更新为 usedPoints；世界或局部位置会立即影响物体摆放。
        aimArc.positionCount = usedPoints;
        // 步骤 37：这里用 && 要求两边条件同时成立：foundLanding && landing.normal.y < .45f；任意一边为假就跳过本分支。
        if (foundLanding && landing.normal.y < .45f)
            // 步骤 38：把计算结果 TryFindGroundBelow(landing.point, out landing) 保存为foundLanding；下一条判断或调用会读取这个具体值。
            foundLanding = TryFindGroundBelow(landing.point, out landing);
        // 步骤 39：把计算结果 foundLanding && landing.normal.y >= .45f 保存为hasAimLanding；下一条判断或调用会读取这个具体值。
        hasAimLanding = foundLanding && landing.normal.y >= .45f;
        // 步骤 40：检查条件 hasAimLanding；只有为真才运行下面的步骤，否则沿原流程继续。
        if (hasAimLanding)
        {
            // 步骤 41：把计算结果 landing.point 保存为aimLandingPoint；下一条判断或调用会读取这个具体值。
            aimLandingPoint = landing.point;
            // 步骤 42：把计算结果 landing.normal 保存为aimLandingNormal；下一条判断或调用会读取这个具体值。
            aimLandingNormal = landing.normal;
        }
        // 步骤 43：调用 SetLandingRing：设置LandingRing。
        SetLandingRing(foundLanding, landing.point, landing.normal);
        // 步骤 44：把新内容写入 aimLabel.text；玩家看到的文字会在本帧更新。
        aimLabel.text = releaseHint + " · " + Mathf.RoundToInt(selectedAngle) + "°" + (placeable ? " · place" : "");
        // 步骤 45：执行 aimLabel.transform.position = start + Vector3.up * .45f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        aimLabel.transform.position = start + Vector3.up * .45f;
        // 步骤 46：执行 aimLabel.transform.rotation = Quaternion.LookRotation(aimLabel.transform.position - viewer.position,；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        aimLabel.transform.rotation = Quaternion.LookRotation(aimLabel.transform.position - viewer.position, Vector3.up);
    }
    // ！！！功能结束：UpdateAimPreview！！！

    // ！！！功能：SetAimVisible——设置AimVisible！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool value 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetAimVisible(bool value)
    {
        // 步骤 1：确认aimArc已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 aimArc.enabled = value;
        if (aimArc != null) aimArc.enabled = value;
        // 步骤 2：检查landingRing是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 landingRing.enabled = false;
        if (landingRing != null && !value) landingRing.enabled = false;
        // 步骤 3：确认aimLabel已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 SetActive：设置Active。
        if (aimLabel != null) aimLabel.gameObject.SetActive(value);
    }
    // ！！！功能结束：SetAimVisible！！！

    // ！！！功能：TryTrajectoryHit——TryTrajectoryHit！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 from, Vector3 to, out RaycastHit bestHit 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool TryTrajectoryHit(Vector3 from, Vector3 to, out RaycastHit bestHit)
    {
        // 步骤 1：把计算结果 default 保存为bestHit；下一条判断或调用会读取这个具体值。
        bestHit = default;
        // 步骤 2：计算 to - from，把结果存进 segment（segment）；之后引用 segment 时用的就是这次结果。
        Vector3 segment = to - from;
        // 步骤 3：计算 segment.magnitude，把结果存进 length（length）；之后引用 length 时用的就是这次结果。
        float length = segment.magnitude;
        // 步骤 4：检查条件 length < .001f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (length < .001f) return false;
        // 步骤 5：计算 float.PositiveInfinity，把结果存进 closest（closest）；之后引用 closest 时用的就是这次结果。
        float closest = float.PositiveInfinity;
        // 步骤 6：逐个读取 RaycastHit hit in Physics.RaycastAll(from, segment / length, length, ~0, QueryTrigger 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(from, segment / length, length, ~0, QueryTriggerInteraction.Ignore))
        {
            // 步骤 7：检查hit的collider是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (hit.collider == null || hit.transform == transform || hit.transform.IsChildOf(transform)) continue;
            // 步骤 8：检查desktopAvatar是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (desktopAvatar != null && hit.transform.IsChildOf(desktopAvatar)) continue;
            // 步骤 9：检查条件 hit.distance >= closest；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (hit.distance >= closest) continue;
            // 步骤 10：把计算结果 hit.distance 保存为closest；下一条判断或调用会读取这个具体值。
            closest = hit.distance;
            // 步骤 11：把计算结果 hit 保存为bestHit；下一条判断或调用会读取这个具体值。
            bestHit = hit;
        }
        // 步骤 12：把 closest < float.PositiveInfinity 作为方法结果返回给调用方；return 后本方法立即结束。
        return closest < float.PositiveInfinity;
    }
    // ！！！功能结束：TryTrajectoryHit！！！

    // ！！！功能：EffectiveThrowSpeed——EffectiveThrowSpeed！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 float 类型的计算结果；执行到 return 时把该值交回调用者。
    private float EffectiveThrowSpeed()
    {
        // 步骤 1：把 Mathf.Clamp(throwSpeed, 5f, MaximumLaunchSpeed) 作为方法结果返回给调用方；return 后本方法立即结束。
        return Mathf.Clamp(throwSpeed, 5f, MaximumLaunchSpeed);
    }
    // ！！！功能结束：EffectiveThrowSpeed！！！

    // Resolve a proper walkable surface for the target marker.  This second
    // ground query keeps the circle off walls, props and mid-air endpoints.
    // ！！！功能：TryFindGroundBelow——向下射线寻找可落脚地面，供投掷落点提示使用！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 point, out RaycastHit bestHit 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool TryFindGroundBelow(Vector3 point, out RaycastHit bestHit)
    {
        // 步骤 1：把计算结果 default 保存为bestHit；下一条判断或调用会读取这个具体值。
        bestHit = default;
        // 步骤 2：计算 point + Vector3.up * 3f，把结果存进 origin（origin）；之后引用 origin 时用的就是这次结果。
        Vector3 origin = point + Vector3.up * 3f;
        // 步骤 3：计算 float.PositiveInfinity，把结果存进 closest（closest）；之后引用 closest 时用的就是这次结果。
        float closest = float.PositiveInfinity;
        // 步骤 4：逐个读取 RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 9f, ~0, QueryTriggerIntera 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (RaycastHit hit in Physics.RaycastAll(origin, Vector3.down, 9f, ~0, QueryTriggerInteraction.Ignore))
        {
            // 步骤 5：检查hit的collider是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (hit.collider == null || hit.transform == transform || hit.transform.IsChildOf(transform) || hit.normal.y < .45f) continue;
            // 步骤 6：检查desktopAvatar是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (desktopAvatar != null && hit.transform.IsChildOf(desktopAvatar)) continue;
            // 步骤 7：检查条件 hit.distance >= closest；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (hit.distance >= closest) continue;
            // 步骤 8：把计算结果 hit.distance 保存为closest；下一条判断或调用会读取这个具体值。
            closest = hit.distance;
            // 步骤 9：把计算结果 hit 保存为bestHit；下一条判断或调用会读取这个具体值。
            bestHit = hit;
        }
        // 步骤 10：把 closest < float.PositiveInfinity 作为方法结果返回给调用方；return 后本方法立即结束。
        return closest < float.PositiveInfinity;
    }
    // ！！！功能结束：TryFindGroundBelow！！！

    // ！！！功能：SetLandingRing——设置LandingRing！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool visible, Vector3 point, Vector3 normal 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetLandingRing(bool visible, Vector3 point, Vector3 normal)
    {
        // 步骤 1：检查landingRing是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (landingRing == null) return;
        // 步骤 2：把计算结果 visible 保存为enabled；下一条判断或调用会读取这个具体值。
        landingRing.enabled = visible;
        // 步骤 3：这里的 ! 表示取反；当 visible 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!visible) return;
        // 步骤 4：用三元表达式为 up 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 up = normal.sqrMagnitude > .01f ? normal.normalized : Vector3.up;
        // 步骤 5：通过叉积算出与原方向垂直的向量，存为 tangent 供左右移动使用。
        Vector3 tangent = Vector3.Cross(up, Vector3.forward);
        // 步骤 6：检查条件 tangent.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 tangent = Vector3.Cross(up, Vector3.right);
        if (tangent.sqrMagnitude < .01f) tangent = Vector3.Cross(up, Vector3.right);
        // 步骤 7：调用 Normalize：Normalize。
        tangent.Normalize();
        // 步骤 8：通过叉积算出与原方向垂直的向量，存为 bitangent 供左右移动使用。
        Vector3 bitangent = Vector3.Cross(up, tangent).normalized;
        // 步骤 9：计算 1.35f + Mathf.Sin(Time.time * 5f) * .035f，把结果存进 pulse（pulse）；之后引用 pulse 时用的就是这次结果。
        float pulse = 1.35f + Mathf.Sin(Time.time * 5f) * .035f;
        // 步骤 10：计算 point + up * .018f，把结果存进 center（center）；之后引用 center 时用的就是这次结果。
        Vector3 center = point + up * .018f;
        // 步骤 11：按 int i = 0; i < landingRing.positionCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < landingRing.positionCount; i++)
        {
            // 步骤 12：计算 i / (float)(landingRing.positionCount - 1) * Mathf.PI * 2f，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = i / (float)(landingRing.positionCount - 1) * Mathf.PI * 2f;
            // 步骤 13：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            landingRing.SetPosition(i, center + (tangent * Mathf.Cos(angle) + bitangent * Mathf.Sin(angle)) * pulse);
        }
    }
    // ！！！功能结束：SetLandingRing！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void OnDestroy()
    {
        // 步骤 1：检查条件 Held == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 Held = null;
        if (Held == this) Held = null;
        // 步骤 2：检查条件 consumingItem == this；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 consumingItem = null;
        if (consumingItem == this) consumingItem = null;
        // 步骤 3：确认aimArc已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (aimArc != null) Destroy(aimArc.gameObject);
        // 步骤 4：确认landingRing已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (landingRing != null) Destroy(landingRing.gameObject);
        // 步骤 5：确认aimLabel已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (aimLabel != null) Destroy(aimLabel.gameObject);
        // 步骤 6：确认carryAnchor已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (carryAnchor != null) Destroy(carryAnchor.gameObject);
        // 步骤 7：确认firstPersonAnchor已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (firstPersonAnchor != null) Destroy(firstPersonAnchor.gameObject);
        // 步骤 8：确认firstPersonForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (firstPersonForearm != null) Destroy(firstPersonForearm);
    }
    // ！！！功能结束：OnDestroy！！！

    // ！！！功能：IsAnyHeld——IsAnyHeld！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    public static bool IsAnyHeld() => Held != null && Held.held;
    // ！！！功能结束：IsAnyHeld！！！
}
