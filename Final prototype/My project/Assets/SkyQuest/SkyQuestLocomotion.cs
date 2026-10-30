// ！！！文件功能：IP1 的角色移动、视角切换、Quest 追踪与斗篷飞行位移！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.XR 命名空间；下方可直接使用Quest 头显及手柄输入设备。
using UnityEngine.XR;
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;

// 定义 SkyQuestLocomotion 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestLocomotion : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Tracked rig")]
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 leftHand（左手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform leftHand;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // 声明 avatar（角色模型）：场景对象引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public GameObject avatar;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Movement")]
    // 把 Inspector 可调整的 groundSpeed（地面行走速度）设为默认值 2.7f；具体用途看它在下方的判断和计算。
    public float groundSpeed = 2.7f;
    // 把 Inspector 可调整的 launchSpeed（起飞速度）设为默认值 2.8f；具体用途看它在下方的判断和计算。
    public float launchSpeed = 2.8f;
    // 把 Inspector 可调整的 glideSpeed（滑翔速度）设为默认值 3.2f；具体用途看它在下方的判断和计算。
    public float glideSpeed = 3.2f;
    // 把 Inspector 可调整的 terminalFallSpeed（最高下落速度）设为默认值 1.2f；具体用途看它在下方的判断和计算。
    public float terminalFallSpeed = 1.2f;
    // 把 Inspector 可调整的 launchHandSpeed（触发起飞的手部速度）设为默认值 0.9f；具体用途看它在下方的判断和计算。
    public float launchHandSpeed = 0.9f;
    // 把 Inspector 可调整的 snapAngle（单次瞬间转向角度）设为默认值 20f；具体用途看它在下方的判断和计算。
    public float snapAngle = 20f;
    // 把 Inspector 可调整的 playRadius（场景可活动半径）设为默认值 85f；具体用途看它在下方的判断和计算。
    public float playRadius = 85f;
    // 把 Inspector 可调整的 groundHeight（地面基准高度）设为默认值 0f；具体用途看它在下方的判断和计算。
    public float groundHeight = 0f;
    // 把 Inspector 可调整的 stepHeight（角色可跨越台阶高度）设为默认值 0.38f；具体用途看它在下方的判断和计算。
    public float stepHeight = 0.38f;
    // 用 Inspector 特性限制可输入范围，减少配置出不合理数值的机会。
    [Range(1f, 4f)] public float movementScale = 2f;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("Comfort")]
    // 把 Inspector 可调整的 thirdPersonDistance（第三人称相机距离）设为默认值 4.5f；具体用途看它在下方的判断和计算。
    public float thirdPersonDistance = 4.5f;
    // 把 Inspector 可调整的 perspectiveBlendSpeed（视角切换过渡速度）设为默认值 3f；具体用途看它在下方的判断和计算。
    public float perspectiveBlendSpeed = 3f;
    // 把 Inspector 可调整的 vignetteAtSpeed（出现舒适暗角的速度阈值）设为默认值 4f；具体用途看它在下方的判断和计算。
    public float vignetteAtSpeed = 4f;
    // 是否显示舒适暗角是 Inspector 中的开关，默认 true；勾选状态决定相关功能是否运行。
    public bool useComfortVignette = true;
    // 声明 desktopControlled（是否使用桌面键鼠控制）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopControlled;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public bool IsFlying { get; private set; }
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public bool ThirdPerson { get; private set; }
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public float CurrentSpeed { get; private set; }

    // 声明 verticalSpeed（当前竖直移动速度）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float verticalSpeed;
    // 声明 smoothTurnVelocity（当前平滑转向速度）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float smoothTurnVelocity;
    // 定义不可在运行时改变的数值 SmoothTurnDegreesPerSecond（平滑转向最大角速度）；后续计算统一使用这个阈值、速度或尺寸。
    private const float SmoothTurnDegreesPerSecond = 85f;
    // 定义不可在运行时改变的数值 ComfortTurnDegreesPerSecond（舒适转向角速度）；后续计算统一使用这个阈值、速度或尺寸。
    private const float ComfortTurnDegreesPerSecond = 55f;
    // 定义不可在运行时改变的数值 SmoothTurnDeadzone（摇杆转向死区）；后续计算统一使用这个阈值、速度或尺寸。
    private const float SmoothTurnDeadzone = 0.18f;
    // 定义不可在运行时改变的数值 SmoothTurnResponse（转向响应速度）；后续计算统一使用这个阈值、速度或尺寸。
    private const float SmoothTurnResponse = 12f;
    // 声明 previousViewButton（previousViewButton）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousViewButton;
    // 声明 avatarStartPosition（角色模型初始局部位置）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 avatarStartPosition;
    // avatarRenderers 保存角色各部分渲染器（类型 Renderer[]）；下方方法依靠它记录或查找场景状态。
    private Renderer[] avatarRenderers;
    // 声明 vignetteMaterial（vignette材质）：Unity 材质资源；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Material vignetteMaterial;
    // 声明 vignette（vignette）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vignette;
    // 声明 horizontalVelocity（当前水平速度）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 horizontalVelocity;
    // 计算 Vector3.forward，把结果存进 desktopFacing（desktopFacing）；之后引用 desktopFacing 时用的就是这次结果。
    private Vector3 desktopFacing = Vector3.forward;
    // bodyController 保存角色碰撞移动组件（类型 CharacterController）；下方方法依靠它记录或查找场景状态。
    private CharacterController bodyController;
    // capeFlight 保存斗篷飞行控制组件（类型 CapeFlightController）；下方方法依靠它记录或查找场景状态。
    private CapeFlightController capeFlight;
    // capeEnergy 保存斗篷能量组件（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem capeEnergy;
    // fanHUD 保存屏幕能量扇形界面（类型 CapeEnergyFanHUD）；下方方法依靠它记录或查找场景状态。
    private CapeEnergyFanHUD fanHUD;
    // capeVisual 保存斗篷外观组件（类型 SkyQuestCapeVisual）；下方方法依靠它记录或查找场景状态。
    private SkyQuestCapeVisual capeVisual;
    // 声明 nextDesktopFlightCost（next桌面FlightCost）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextDesktopFlightCost;
    // 声明 vrSpeedTrack（VR 速度滑块轨道）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vrSpeedTrack;
    // 声明 vrSpeedBacking（VR 速度滑块底板）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vrSpeedBacking;
    // 声明 vrSpeedFill（VR 速度滑块进度条）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vrSpeedFill;
    // 声明 vrSpeedKnob（VR 速度滑块手柄）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vrSpeedKnob;
    // vrSpeedLabel 保存VR 速度数值文字（类型 TMPro.TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TMPro.TextMeshPro vrSpeedLabel;
    // vrSpeedHint 保存VR 速度提示文字（类型 TMPro.TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TMPro.TextMeshPro vrSpeedHint;
    // 速度条的局部选中范围；直接用于 HUD 射线检测，不依赖场景碰撞顺序。
    private BoxCollider vrSpeedHitArea;
    // 声明 speedSliderCapturesTrigger（speedSliderCapturesTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool speedSliderCapturesTrigger;
    // 创建新的 RaycastHit[] 并存为 avatarCastHits；构造参数决定其初始内容或对象名称。
    private readonly RaycastHit[] avatarCastHits = new RaycastHit[32];
    // 创建新的 Collider[] 并存为 avatarOverlapHits；构造参数决定其初始内容或对象名称。
    private readonly Collider[] avatarOverlapHits = new Collider[64];
    // 定义不可在运行时改变的数值 DesktopEyeHeight（桌面EyeHeight）；后续计算统一使用这个阈值、速度或尺寸。
    private const float DesktopEyeHeight = 1.65f;
    // 定义不可在运行时改变的数值 DesktopCameraRadius（桌面相机Radius）；后续计算统一使用这个阈值、速度或尺寸。
    private const float DesktopCameraRadius = 0.22f;
    // 定义不可在运行时改变的数值 MinVrAvatarDistance（MinVrAvatarDistance）；后续计算统一使用这个阈值、速度或尺寸。
    private const float MinVrAvatarDistance = 1.2f;

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Awake()
    {
// 以下代码只在 Unity 编辑器内编译，安装到 Quest 的 APK 不执行这一段。
#if UNITY_EDITOR
        // The Quest scene can also be previewed in the editor. Without a
        // tracked headset its VR action map has no movement source, so use
        // the same character movement path as the keyboard scene.
        // 步骤 1：这里用 && 要求两边条件同时成立：!desktopControlled && !HasTrackedHeadset()；任意一边为假就跳过本分支。
        if (!desktopControlled && !HasTrackedHeadset())
        {
            // 步骤 2：Unity 编辑器没有真实头显追踪时改用键盘鼠标输入，方便测试同一场景。
            desktopControlled = true;
            // 步骤 3：创建并连接桌面备用输入组件，以便编辑器中继续测试移动、兑换和聊天。
            EnableEditorDesktopControls();
        }
// 结束平台条件编译范围。
#endif
    }
    // ！！！功能结束：Awake！！！

// 以下代码只在 Unity 编辑器内编译，安装到 Quest 的 APK 不执行这一段。
#if UNITY_EDITOR
    // ！！！功能：EnableEditorDesktopControls——EnableEditor桌面Controls！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EnableEditorDesktopControls()
    {
        // The XR pose driver also writes the camera transform just before render.
        // In an untracked Editor preview that write cancels mouse look (and the
        // third-person camera offset), even though the keyboard script ran.
        // 步骤 1：确认头显相机已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (head != null)
            // 步骤 2：逐个读取 Behaviour component in head.GetComponents<Behaviour>() 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Behaviour component in head.GetComponents<Behaviour>())
                // 步骤 3：检查条件 component.GetType().Name == "TrackedPoseDriver"；只有为真才运行下面的步骤，否则沿原流程继续。
                if (component.GetType().Name == "TrackedPoseDriver")
                    // 步骤 4：把 component.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                    component.enabled = false;

        // 步骤 5：从目标物体找到所需组件并存为 ancestor；使用前应检查是否为 null。
        SkyQuestAncestor ancestor = GetComponent<SkyQuestAncestor>();
        // 步骤 6：从目标物体找到所需组件并存为 magic；使用前应检查是否为 null。
        SkyQuestMagicExchange magic = GetComponent<SkyQuestMagicExchange>();
        // 步骤 7：从目标物体找到所需组件并存为 social；使用前应检查是否为 null。
        SkyQuestSocial social = GetComponent<SkyQuestSocial>();
        // 步骤 8：从目标物体找到所需组件并存为 props；使用前应检查是否为 null。
        SkyQuestProps props = GetComponent<SkyQuestProps>();
        // 步骤 9：从目标物体找到所需组件并存为 communication；使用前应检查是否为 null。
        SkyQuestCommunication communication = GetComponent<SkyQuestCommunication>();
        // 步骤 10：从目标物体找到所需组件并存为 status；使用前应检查是否为 null。
        SkyQuestStatus status = GetComponent<SkyQuestStatus>();
        // 步骤 11：从目标物体找到所需组件并存为 immersion；使用前应检查是否为 null。
        SkyQuestImmersion immersion = GetComponent<SkyQuestImmersion>();

        // 步骤 12：确认ancestor已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 ancestor.desktopMode = true;
        if (ancestor != null) ancestor.desktopMode = true;
        // 步骤 13：确认magic已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 magic.desktopMode = true;
        if (magic != null) magic.desktopMode = true;
        // 步骤 14：确认social已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 social.desktopMode = true;
        if (social != null) social.desktopMode = true;
        // 步骤 15：确认props已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 props.desktopMode = true;
        if (props != null) props.desktopMode = true;
        // 步骤 16：确认communication已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 communication.desktopMode = true;
        if (communication != null) communication.desktopMode = true;
        // 步骤 17：确认status已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 status.desktopMode = true;
        if (status != null) status.desktopMode = true;
        // 步骤 18：确认immersion已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 immersion.desktopMode = true;
        if (immersion != null) immersion.desktopMode = true;

        // 步骤 19：从目标物体找到所需组件并存为 controls；使用前应检查是否为 null。
        SkyQuestDesktopControls controls = GetComponent<SkyQuestDesktopControls>();
        // 步骤 20：如果场景对象没有 SkyQuestDesktopControls 组件，就当场创建并保存，保证后续调用有对象可用。
        if (controls == null) controls = gameObject.AddComponent<SkyQuestDesktopControls>();
        // 步骤 21：把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        controls.head = head;
        // 步骤 22：把计算结果 this 保存为locomotion；下一条判断或调用会读取这个具体值。
        controls.locomotion = this;
        // 步骤 23：把计算结果 ancestor 保存为ancestor；下一条判断或调用会读取这个具体值。
        controls.ancestor = ancestor;
        // 步骤 24：把计算结果 magic 保存为magicExchange；下一条判断或调用会读取这个具体值。
        controls.magicExchange = magic;
        // 步骤 25：把计算结果 social 保存为social；下一条判断或调用会读取这个具体值。
        controls.social = social;
        // 步骤 26：把计算结果 props 保存为props；下一条判断或调用会读取这个具体值。
        controls.props = props;
        // 步骤 27：把计算结果 communication 保存为communication；下一条判断或调用会读取这个具体值。
        controls.communication = communication;
        // 步骤 28：把计算结果 status 保存为status；下一条判断或调用会读取这个具体值。
        controls.status = status;
    }
    // ！！！功能结束：EnableEditorDesktopControls！！！

    // ！！！功能：HasTrackedHeadset——HasTrackedHeadset！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private static bool HasTrackedHeadset()
    {
        // 步骤 1：创建新的 var 并存为 headsets；构造参数决定其初始内容或对象名称。
        var headsets = new List<UnityEngine.XR.InputDevice>();
        // 步骤 2：向 XR 系统取得所有头戴设备，下一步检查是否存在正在追踪的 Quest。
        InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.HeadMounted, headsets);
        // 步骤 3：逐个读取 UnityEngine.XR.InputDevice device in headsets 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (UnityEngine.XR.InputDevice device in headsets)
            // 步骤 4：这里用 && 要求两边条件同时成立：device.isValid && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) && tracke；任意一边为假就跳过本分支。
            if (device.isValid && device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.isTracked, out bool tracked) && tracked)
                // 步骤 5：本次检测或激活成功，返回 true 让调用方继续相应反馈。
                return true;
        // 步骤 6：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
        return false;
    }
    // ！！！功能结束：HasTrackedHeadset！！！
// 结束平台条件编译范围。
#endif

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：默认使用第三人称，让玩家开场能看到完整角色与斗篷。
        ThirdPerson = true;
        // 步骤 2：定义固定文字 speedKey（speedKey）；统一引用这段名称或路径，避免各处拼写不一致。
        const string speedKey = "SkyQuestMovementScaleV2";
        // 步骤 3：这里的 ! 表示取反；当 PlayerPrefs.HasKey(speedKey) 不成立时才进入此分支。
        if (!PlayerPrefs.HasKey(speedKey))
        {
            // 步骤 4：把计算结果 2f 保存为玩家可调速度倍率；下一条判断或调用会读取这个具体值。
            movementScale = 2f;
            // 步骤 5：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            PlayerPrefs.SetFloat(speedKey, movementScale);
            // 步骤 6：调用 Save：保存。
            PlayerPrefs.Save();
        }
        // 步骤 7：前面的 if 条件为假时执行这里的备用处理。
        else movementScale = Mathf.Clamp(PlayerPrefs.GetFloat(speedKey, movementScale), 1f, 4f);
        // 步骤 8：查找玩家已有的碰撞移动组件；移动时用它阻止穿墙。
        bodyController = GetComponent<CharacterController>();
        // 步骤 9：场景未配置 CharacterController 时补上一个，确保角色移动仍有碰撞。
        if (bodyController == null) bodyController = gameObject.AddComponent<CharacterController>();
        // 步骤 10：把计算结果 0.25f 保存为radius；下一条判断或调用会读取这个具体值。
        bodyController.radius = 0.25f;
        // 步骤 11：把计算结果 0.035f 保存为skinWidth；下一条判断或调用会读取这个具体值。
        bodyController.skinWidth = 0.035f;
        // 步骤 12：把计算结果 48f 保存为slopeLimit；下一条判断或调用会读取这个具体值。
        bodyController.slopeLimit = 48f;
        // 步骤 13：把计算结果 stepHeight 保存为stepOffset；下一条判断或调用会读取这个具体值。
        bodyController.stepOffset = stepHeight;
        // 步骤 14：把计算结果 0f 保存为minMoveDistance；下一条判断或调用会读取这个具体值。
        bodyController.minMoveDistance = 0f;
        // 步骤 15：调用 UpdateBodyHeight：按头显高度调整 CharacterController 的碰撞胶囊。
        UpdateBodyHeight();
        // 步骤 16：从目标 GameObject 查找并保存 斗篷能量组件 组件；后面的移动或显示逻辑会直接使用这个缓存。
        capeEnergy = GetComponent<CapeEnergySystem>();
        // 步骤 17：如果场景对象没有 CapeEnergySystem 组件，就当场创建并保存，保证后续调用有对象可用。
        if (capeEnergy == null) capeEnergy = gameObject.AddComponent<CapeEnergySystem>();
        // 步骤 18：把斗篷能量上限设为十五格；界面和飞行消耗都读取这个上限。
        capeEnergy.maxEnergy = 15f;
        // 步骤 19：把开场斗篷能量设为十五格，随后 ResetEnergy 会写入当前能量。
        capeEnergy.initialEnergy = 15f;
        // 步骤 20：把当前能量恢复到初始值，确保开场图标全部点亮。
        capeEnergy.ResetEnergy();
        // 步骤 21：从目标 GameObject 查找并保存 屏幕能量扇形界面 组件；后面的移动或显示逻辑会直接使用这个缓存。
        fanHUD = GetComponent<CapeEnergyFanHUD>();
        // 步骤 22：如果场景对象没有 CapeEnergyFanHUD 组件，就当场创建并保存，保证后续调用有对象可用。
        if (fanHUD == null) fanHUD = gameObject.AddComponent<CapeEnergyFanHUD>();
        // 步骤 23：把计算结果 capeEnergy 保存为energy；下一条判断或调用会读取这个具体值。
        fanHUD.energy = capeEnergy;
        // 步骤 24：把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        fanHUD.head = head;
        // 步骤 25：把计算结果 desktopControlled 保存为desktop模式；下一条判断或调用会读取这个具体值。
        fanHUD.desktopMode = desktopControlled;
        // 步骤 26：把括号里的依赖对象交给 fanHUD 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
        fanHUD.Initialize();
        // 步骤 27：从目标物体找到所需组件并存为 hoverInfo；使用前应检查是否为 null。
        SkyQuestItemHoverInfo hoverInfo = GetComponent<SkyQuestItemHoverInfo>();
        // 步骤 28：如果场景对象没有 SkyQuestItemHoverInfo 组件，就当场创建并保存，保证后续调用有对象可用。
        if (hoverInfo == null) hoverInfo = gameObject.AddComponent<SkyQuestItemHoverInfo>();
        // 步骤 29：把计算结果 head 保存为头显相机；下一条判断或调用会读取这个具体值。
        hoverInfo.head = head;
        // 步骤 30：把计算结果 rightHand 保存为右手柄位置；下一条判断或调用会读取这个具体值。
        hoverInfo.rightHand = rightHand;
        // 步骤 31：把计算结果 desktopControlled 保存为desktop模式；下一条判断或调用会读取这个具体值。
        hoverInfo.desktopMode = desktopControlled;
        // 步骤 32：这里的 ! 表示取反；当 desktopControlled 不成立时才进入此分支。
        if (!desktopControlled)
        {
            // 步骤 33：从目标 GameObject 查找并保存 斗篷飞行控制组件 组件；后面的移动或显示逻辑会直接使用这个缓存。
            capeFlight = GetComponent<CapeFlightController>();
            // 步骤 34：检查斗篷飞行控制组件是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
            if (capeFlight == null)
            {
                // 步骤 35：把计算结果 gameObject.AddComponent<CapeFlightController>() 保存为斗篷飞行控制组件；下一条判断或调用会读取这个具体值。
                capeFlight = gameObject.AddComponent<CapeFlightController>();
                // 步骤 36：把计算结果 groundSpeed 保存为地面行走速度；下一条判断或调用会读取这个具体值。
                capeFlight.groundSpeed = groundSpeed;
            }
            // 步骤 37：把括号里的依赖对象交给 capeFlight 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            capeFlight.Initialize(capeEnergy);
            // The left stick should move toward the direction the player is
            // looking, even when their tracked head is turned inside the rig.
            // 步骤 38：把 capeFlight.headRelativeSteering 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            capeFlight.headRelativeSteering = true;
            // 步骤 39：从目标物体找到所需组件并存为 energyUI；使用前应检查是否为 null。
            CapeEnergyUI energyUI = GetComponent<CapeEnergyUI>();
            // 步骤 40：如果场景对象没有 CapeEnergyUI 组件，就当场创建并保存，保证后续调用有对象可用。
            if (energyUI == null) energyUI = gameObject.AddComponent<CapeEnergyUI>();
            // 步骤 41：把括号里的依赖对象交给 energyUI 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            energyUI.Initialize(capeEnergy, leftHand);
            // 步骤 42：从目标物体找到所需组件并存为 feedback；使用前应检查是否为 null。
            CapeFlightFeedback feedback = GetComponent<CapeFlightFeedback>();
            // 步骤 43：如果场景对象没有 CapeFlightFeedback 组件，就当场创建并保存，保证后续调用有对象可用。
            if (feedback == null) feedback = gameObject.AddComponent<CapeFlightFeedback>();
            // 步骤 44：把括号里的依赖对象交给 feedback 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            feedback.Initialize(capeFlight, capeEnergy, head);
            // 步骤 45：从目标物体找到所需组件并存为 receiver；使用前应检查是否为 null。
            CapeRechargeReceiver receiver = GetComponent<CapeRechargeReceiver>();
            // 步骤 46：如果场景对象没有 CapeRechargeReceiver 组件，就当场创建并保存，保证后续调用有对象可用。
            if (receiver == null) receiver = gameObject.AddComponent<CapeRechargeReceiver>();
            // 步骤 47：把括号里的依赖对象交给 receiver 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            receiver.Initialize(capeEnergy, capeFlight);
        }
        // 步骤 48：确认角色模型已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (avatar != null)
        {
            // 步骤 49：把计算结果 avatar.transform.localPosition 保存为角色模型初始局部位置；下一条判断或调用会读取这个具体值。
            avatarStartPosition = avatar.transform.localPosition;
            // 步骤 50：从目标 GameObject 查找并保存 角色各部分渲染器 组件；后面的移动或显示逻辑会直接使用这个缓存。
            avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
            // 步骤 51：从目标物体找到所需组件并存为 walk；使用前应检查是否为 null。
            SkyQuestAvatarWalk walk = avatar.GetComponent<SkyQuestAvatarWalk>();
            // 步骤 52：如果场景对象没有 SkyQuestAvatarWalk 组件，就当场创建并保存，保证后续调用有对象可用。
            if (walk == null) walk = avatar.AddComponent<SkyQuestAvatarWalk>();
            // 步骤 53：把括号里的依赖对象交给 walk 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            walk.Initialize(this);
            // 步骤 54：从目标 GameObject 查找并保存 斗篷外观组件 组件；后面的移动或显示逻辑会直接使用这个缓存。
            capeVisual = avatar.GetComponent<SkyQuestCapeVisual>();
            // 步骤 55：如果场景对象没有 SkyQuestCapeVisual 组件，就当场创建并保存，保证后续调用有对象可用。
            if (capeVisual == null) capeVisual = avatar.AddComponent<SkyQuestCapeVisual>();
            // 步骤 56：把括号里的依赖对象交给 capeVisual 初始化；这个组件之后才能读取正确的玩家、相机或能量数据。
            capeVisual.Initialize(this, capeEnergy, capeFlight);
            // Include the generated cloth, stars and flight effects in the
            // existing first/third-person visibility control.
            // 步骤 57：从目标 GameObject 查找并保存 角色各部分渲染器 组件；后面的移动或显示逻辑会直接使用这个缓存。
            avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
        }
        // 步骤 58：把三维方向投影到水平面并存为desktopFacing，剔除头显抬头低头造成的竖直分量。
        desktopFacing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        // 步骤 59：检查条件 desktopFacing.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 desktopFacing = Vector3.forward;
        if (desktopFacing.sqrMagnitude < 0.01f) desktopFacing = Vector3.forward;
        // 步骤 60：调用 CreateVignette：创建Vignette。
        CreateVignette();
        // 步骤 61：这里的 ! 表示取反；当 desktopControlled 不成立时才进入此分支。
        if (!desktopControlled)
        {
            // 步骤 62：调用 FixRightControllerMaterials：修正Right控制器材质。
            FixRightControllerMaterials();
            // 步骤 63：调用 CreateVrSpeedSlider：在 VR 空间内创建可调整行走速度的滑块。
            CreateVrSpeedSlider();
        }
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：FixRightControllerMaterials——修正Right控制器材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void FixRightControllerMaterials()
    {
        // 步骤 1：检查右手柄位置是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (rightHand == null) return;
        // The Starter Assets controller mesh contains a material that renders
        // as a purple error block in the Quest URP build. Keep the controller
        // model and tracking, but use the same runtime-safe material as our
        // other world primitives for its mesh parts.
        // 步骤 2：创建新的 Material 并存为 neutral；构造参数决定其初始内容或对象名称。
        Material neutral = SkyQuestVisuals.Material(new Color(.76f, .82f, .88f));
        // 步骤 3：逐个读取 Renderer renderer in rightHand.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in rightHand.GetComponentsInChildren<Renderer>(true))
        {
            // XR hands can instantiate their visual children after Start. Never
            // recolor a reward once it has been attached to the hand.
            // 步骤 4：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 continue;
            if (renderer.GetComponentInParent<SkyQuestMagicPickup>() != null) continue;
            // 步骤 5：计算 renderer.sharedMaterials，把结果存进 slots（slots）；之后引用 slots 时用的就是这次结果。
            Material[] slots = renderer.sharedMaterials;
            // 步骤 6：检查条件 slots.Length == 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 slots = new Material[1];
            if (slots.Length == 0) slots = new Material[1];
            // 步骤 7：按 int i = 0; i < slots.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < slots.Length; i++) slots[i] = neutral;
            // 步骤 8：把计算结果 slots 保存为shared材质；下一条判断或调用会读取这个具体值。
            renderer.sharedMaterials = slots;
        }
    }
    // ！！！功能结束：FixRightControllerMaterials！！！

    // 声明 nextMaterialRepair（next材质修复）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextMaterialRepair;

    // ！！！功能：RepairQuestErrorMaterials——修复QuestError材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RepairQuestErrorMaterials()
    {
        // 步骤 1：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。 条件成立时立即退出当前方法。
        if (Time.unscaledTime < nextMaterialRepair) return;
        // 步骤 2：把计算结果 Time.unscaledTime + 2f 保存为next材质修复；下一条判断或调用会读取这个具体值。
        nextMaterialRepair = Time.unscaledTime + 2f;
        // 步骤 3：调用 FixRightControllerMaterials：修正Right控制器材质。
        FixRightControllerMaterials();
        // 步骤 4：创建新的 Material 并存为 neutral；构造参数决定其初始内容或对象名称。
        Material neutral = SkyQuestVisuals.Material(new Color(.82f, .86f, .92f));
        // 步骤 5：计算 Resources.Load<Material>("SkyCapeGlow")，把结果存进 particle（particle）；之后引用 particle 时用的就是这次结果。
        Material particle = Resources.Load<Material>("SkyCapeGlow");
        // 步骤 6：逐个读取 Renderer renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObj 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
        {
            // 步骤 7：计算 renderer.sharedMaterials，把结果存进 slots（slots）；之后引用 slots 时用的就是这次结果。
            Material[] slots = renderer.sharedMaterials;
            // 步骤 8：用 changed 记住此处真假结果（false），供下一条 if 选择执行路径。
            bool changed = false;
            // 步骤 9：按 int i = 0; i < slots.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < slots.Length; i++)
            {
                // 步骤 10：计算 slots[i]，把结果存进 slot（slot）；之后引用 slot 时用的就是这次结果。
                Material slot = slots[i];
                // 步骤 11：检查slot、slot的shader是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                if (slot != null && slot.shader != null && slot.shader.name != "Hidden/InternalErrorShader") continue;
                // 步骤 12：执行 slots[i] = renderer is ParticleSystemRenderer && particle != null ? particle : neutral;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                slots[i] = renderer is ParticleSystemRenderer && particle != null ? particle : neutral;
                // 步骤 13：把 changed 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                changed = true;
                // 步骤 14：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                Debug.LogWarning("SkyQuest repaired unsupported Quest material on " + renderer.name);
            }
            // 步骤 15：检查条件 changed；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 renderer.sharedMaterials = slots;
            if (changed) renderer.sharedMaterials = slots;
        }
    }
    // ！！！功能结束：RepairQuestErrorMaterials！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：所有移动由这个组件统一提交，避免飞行、手柄和键盘分别推角色造成跳动或视角错位。
    void Update()
    {
        // 步骤 1：检查头显相机、左手柄位置、右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (head == null || leftHand == null || rightHand == null) return;
        // 步骤 2：计算 Mathf.Max(Time.deltaTime, 0.001f)，把结果存进 dt（dt）；之后引用 dt 时用的就是这次结果。
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        // 步骤 3：检查条件 desktopControlled；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopControlled)
        {
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 5：调用 UpdateBodyHeight：按头显高度调整 CharacterController 的碰撞胶囊。
        UpdateBodyHeight();
        // 步骤 6：调用 RepairQuestErrorMaterials：修复QuestError材质。
        RepairQuestErrorMaterials();
        // 步骤 7：调用 UpdateVrSpeedSlider：检测手柄射线和扳机，允许玩家拖动速度滑块。
        UpdateVrSpeedSlider();
        // 步骤 8：调用 HandleQuestControls：HandleQuestControls。
        SkyQuestMagicPickup.HandleQuestControls(rightHand, head, speedSliderCapturesTrigger);
        // 步骤 9：计算 SkyQuestInput.Right，把结果存进 right（right）；之后引用 right 时用的就是这次结果。
        var right = SkyQuestInput.Right;

        // 步骤 10：把计算结果 SkyQuestInput.Secondary(right) && 保存为viewButton；下一条判断或调用会读取这个具体值。
        bool viewButton = SkyQuestInput.Secondary(right) &&
            // 步骤 11：调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Left) <= 0.75f;
        // 步骤 12：这里用 && 要求两边条件同时成立：viewButton && !previousViewButton；任意一边为假就跳过本分支。 成立时调用 TogglePerspective：根据输入在第一与第三人称之间切换。
        if (viewButton && !previousViewButton) TogglePerspective();
        // 步骤 13：把计算结果 viewButton 保存为previousViewButton；下一条判断或调用会读取这个具体值。
        previousViewButton = viewButton;
        // 步骤 14：调用 UpdatePerspectiveVisuals：按当前视角状态更新角色、斗篷和相机表现。
        UpdatePerspectiveVisuals();

        // 步骤 15：用三元表达式为 turn 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector2 turn = capeFlight != null ? capeFlight.ReadTurn() : Vector2.zero;
        // 步骤 16：把计算结果 Mathf.Sign(turn.x) * 保存为turnAmount；下一条判断或调用会读取这个具体值。
        float turnAmount = Mathf.Sign(turn.x) *
            // 步骤 17：调用 Clamp01：Clamp01。
            Mathf.Clamp01((Mathf.Abs(turn.x) - SmoothTurnDeadzone) / (1f - SmoothTurnDeadzone));
        // 步骤 18：把计算结果 capeFlight != null && capeFlight.comfortMode ? 保存为turnRate；下一条判断或调用会读取这个具体值。
        float turnRate = capeFlight != null && capeFlight.comfortMode ?
            // 步骤 19：执行 ComfortTurnDegreesPerSecond : SmoothTurnDegreesPerSecond;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ComfortTurnDegreesPerSecond : SmoothTurnDegreesPerSecond;
        // 步骤 20：计算 Mathf.Min(dt, 0.05f)，把结果存进 turnDt（turnDt）；之后引用 turnDt 时用的就是这次结果。
        float turnDt = Mathf.Min(dt, 0.05f);
        // 步骤 21：计算 turnAmount * turnRate，把结果存进 targetTurnVelocity（targetTurnVelocity）；之后引用 targetTurnVelocity 时用的就是这次结果。
        float targetTurnVelocity = turnAmount * turnRate;
        // 步骤 22：把计算结果 Mathf.Lerp(smoothTurnVelocity, targetTurnVelocity, 保存为当前平滑转向速度；下一条判断或调用会读取这个具体值。
        smoothTurnVelocity = Mathf.Lerp(smoothTurnVelocity, targetTurnVelocity,
            // 步骤 23：执行 1f - Mathf.Exp(-SmoothTurnResponse * turnDt));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            1f - Mathf.Exp(-SmoothTurnResponse * turnDt));
        // 步骤 24：检查条件 Mathf.Abs(smoothTurnVelocity) > 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (Mathf.Abs(smoothTurnVelocity) > 0.01f)
            // 步骤 25：调用 RotateAround：RotateAround。
            transform.RotateAround(head.position, Vector3.up, smoothTurnVelocity * turnDt);

        // 步骤 26：把计算结果 capeFlight != null && capeFlight.enabled ? 保存为displacement；下一条判断或调用会读取这个具体值。
        Vector3 displacement = capeFlight != null && capeFlight.enabled ?
            // 步骤 27：调用 Tick：Tick。
            capeFlight.Tick(dt, transform.forward, head.forward, movementScale) : Vector3.zero;
        // 步骤 28：调用 MoveRig：根据输入、重力或飞行结果移动整个玩家根节点。
        MoveRig(displacement, dt);
        // 步骤 29：把计算结果 capeFlight != null && capeFlight.IsFlying 保存为IsFlying；下一条判断或调用会读取这个具体值。
        IsFlying = capeFlight != null && capeFlight.IsFlying;
        // 步骤 30：调用 UpdateAvatar：更新Avatar。
        UpdateAvatar(dt);
        // 步骤 31：调用 UpdateVignette：更新Vignette。
        UpdateVignette();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：MoveRig——根据输入、重力或飞行结果移动整个玩家根节点！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 displacement, float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：把不同输入汇合成一次角色位移；CharacterController 会处理建筑和地面的碰撞。
    private void MoveRig(Vector3 displacement, float dt)
    {
        // 步骤 1：计算 transform.position + displacement，把结果存进 next（next）；之后引用 next 时用的就是这次结果。
        Vector3 next = transform.position + displacement;
        // 步骤 2：创建新的 Vector2 并存为 radial；构造参数决定其初始内容或对象名称。
        Vector2 radial = new Vector2(next.x, next.z);
        // 步骤 3：检查条件 radial.magnitude > playRadius；只有为真才运行下面的步骤，否则沿原流程继续。
        if (radial.magnitude > playRadius)
        {
            // 步骤 4：把计算结果 radial.normalized * playRadius 保存为radial；下一条判断或调用会读取这个具体值。
            radial = radial.normalized * playRadius;
            // 步骤 5：把计算结果 radial.x 保存为x；下一条判断或调用会读取这个具体值。
            next.x = radial.x;
            // 步骤 6：把计算结果 radial.y 保存为z；下一条判断或调用会读取这个具体值。
            next.z = radial.y;
        }
        // 步骤 7：计算 transform.position，把结果存进 before（before）；之后引用 before 时用的就是这次结果。
        Vector3 before = transform.position;
        // 步骤 8：检查角色碰撞移动组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (bodyController != null && bodyController.enabled)
        {
            // 步骤 9：计算 bodyController.Move(next - before)，把结果存进 flags（flags）；之后引用 flags 时用的就是这次结果。
            CollisionFlags flags = bodyController.Move(next - before);
            // 步骤 10：检查斗篷飞行控制组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (!desktopControlled && capeFlight != null)
            {
                // 步骤 11：调用 ReportCollision：Report碰撞。
                capeFlight.ReportCollision(flags);
                // 步骤 12：把计算结果 capeFlight.IsFlying 保存为IsFlying；下一条判断或调用会读取这个具体值。
                IsFlying = capeFlight.IsFlying;
            }
            // 步骤 13：这里用 && 要求两边条件同时成立：desktopControlled && (flags & CollisionFlags.Below) != 0；任意一边为假就跳过本分支。
            if (desktopControlled && (flags & CollisionFlags.Below) != 0)
            {
                // 步骤 14：把 IsFlying 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
                IsFlying = false;
                // 步骤 15：把计算结果 0f 保存为当前竖直移动速度；下一条判断或调用会读取这个具体值。
                verticalSpeed = 0f;
            }
            // 步骤 16：这里用 && 要求两边条件同时成立：desktopControlled && (flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f；任意一边为假就跳过本分支。 成立时执行 verticalSpeed = 0f;
            if (desktopControlled && (flags & CollisionFlags.Above) != 0 && verticalSpeed > 0f) verticalSpeed = 0f;
        }
        // 步骤 17：前面的 if 条件为假时执行这里的备用处理。
        else transform.position = next;
        // 步骤 18：检查条件 transform.position.y < groundHeight - 0.1f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (transform.position.y < groundHeight - 0.1f)
        {
            // 步骤 19：新建用于position的对象或数值结构，并把构造参数保存进去。
            transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);
            // 步骤 20：检查条件 desktopControlled；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { IsFlying = false; verticalSpeed = 0f; }
            if (desktopControlled) { IsFlying = false; verticalSpeed = 0f; }
            // 步骤 21：前面的 if 条件为假时执行这里的备用处理。
            else { capeFlight?.ReportCollision(CollisionFlags.Below); IsFlying = false; }
        }
        // 步骤 22：把计算结果 Vector3.Distance(before, transform.position) / dt 保存为CurrentSpeed；下一条判断或调用会读取这个具体值。
        CurrentSpeed = Vector3.Distance(before, transform.position) / dt;
    }
    // ！！！功能结束：MoveRig！！！

    // ！！！功能：UpdateBodyHeight——按头显高度调整 CharacterController 的碰撞胶囊！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateBodyHeight()
    {
        // 步骤 1：检查角色碰撞移动组件、头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (bodyController == null || head == null) return;
        // 步骤 2：计算 height 并限制在安全上下界，防止不合理的大小影响移动或显示。
        float height = Mathf.Clamp(head.position.y - transform.position.y + 0.1f, 1.2f, 2.2f);
        // 步骤 3：把计算结果 height 保存为height；下一条判断或调用会读取这个具体值。
        bodyController.height = height;
        // 步骤 4：计算 transform.InverseTransformPoint(head.position)，把结果存进 headLocal（headLocal）；之后引用 headLocal 时用的就是这次结果。
        Vector3 headLocal = transform.InverseTransformPoint(head.position);
        // 步骤 5：新建用于center的对象或数值结构，并把构造参数保存进去。
        bodyController.center = new Vector3(headLocal.x, height * 0.5f + 0.035f, headLocal.z);
        // 步骤 6：把计算结果 Mathf.Min(stepHeight, height - 0.05f) 保存为stepOffset；下一条判断或调用会读取这个具体值。
        bodyController.stepOffset = Mathf.Min(stepHeight, height - 0.05f);
    }
    // ！！！功能结束：UpdateBodyHeight！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：这里的 ! 表示取反；当 desktopControlled 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!desktopControlled) return;
        // 步骤 2：新建用于panel的对象或数值结构，并把构造参数保存进去。
        Rect panel = new Rect(Screen.width - 326f, Mathf.Max(110f, Screen.height * .4f - 58f),
            // 步骤 3：执行 310f, 116f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            310f, 116f);
        // 步骤 4：调用 Box：创建长方体场景部件，并按参数设置名称、尺寸、位置和材质。
        GUI.Box(panel, "MOVEMENT SPEED  " + movementScale.ToString("0.0") + "x");
        // 步骤 5：检查条件 GUI.Button(new Rect(panel.x + 14f, panel.y + 40f, 37f, 35f), "-")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (GUI.Button(new Rect(panel.x + 14f, panel.y + 40f, 37f, 35f), "-"))
            // 步骤 6：调用 SetMovementScale：设置Movement缩放。
            SetMovementScale(Mathf.Round((movementScale - .1f) * 10f) / 10f);
        // 步骤 7：新建用于selected的对象或数值结构，并把构造参数保存进去。
        float selected = GUI.HorizontalSlider(new Rect(panel.x + 62f, panel.y + 45f, 187f, 30f),
            // 步骤 8：执行 movementScale, 1f, 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            movementScale, 1f, 4f);
        // 步骤 9：调用 SetMovementScale：设置Movement缩放。
        SetMovementScale(selected);
        // 步骤 10：检查条件 GUI.Button(new Rect(panel.x + 259f, panel.y + 40f, 37f, 35f), "+")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (GUI.Button(new Rect(panel.x + 259f, panel.y + 40f, 37f, 35f), "+"))
            // 步骤 11：调用 SetMovementScale：设置Movement缩放。
            SetMovementScale(Mathf.Round((movementScale + .1f) * 10f) / 10f);
        // 步骤 12：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 61f, panel.y + 82f, 190f, 22f),
            // 步骤 13：执行 "Slow 1.0x                         Fast 4.0x");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Slow 1.0x                         Fast 4.0x");
    }
    // ！！！功能结束：OnGUI！！！

    // ！！！功能：CreateVrSpeedSlider——在 VR 空间内创建可调整行走速度的滑块！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateVrSpeedSlider()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // Scale the backing, text, slider and hit area together around the HUD.
        // 步骤 2：创建新的 Transform 并存为 speedHud；构造参数决定其初始内容或对象名称。
        Transform speedHud = new GameObject("VR speed control panel").transform;
        // 步骤 3：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        speedHud.SetParent(head, false);
        // Center below the Left grip+B comfort hint, with space for its text.
        // 步骤 4：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        speedHud.localPosition = new Vector3(0f, -.55f, 1.05f);
        // 步骤 5：恢复 speedHud.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        speedHud.localScale = Vector3.one * .7225f;
        // 步骤 6：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 backing（backing）；之后引用 backing 时用的就是这次结果。
        GameObject backing = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 7：把计算结果 "VR speed control backing" 保存为name；下一条判断或调用会读取这个具体值。
        backing.name = "VR speed control backing";
        // 步骤 8：把计算结果 backing.transform 保存为VR 速度滑块底板；下一条判断或调用会读取这个具体值。
        vrSpeedBacking = backing.transform;
        // 步骤 9：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrSpeedBacking.SetParent(speedHud, false);
        // 步骤 10：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        vrSpeedBacking.localPosition = new Vector3(0f, -.035f, .055f);
        // 步骤 11：恢复 vrSpeedBacking.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        vrSpeedBacking.localScale = new Vector3(.48f, .19f, .008f);
        // 步骤 12：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(backing);
        // 步骤 13：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        backing.GetComponent<Renderer>().sharedMaterial =
            // 步骤 14：调用 Material：材质。
            SkyQuestVisuals.Material(new Color(.065f, .12f, .19f));

        // 步骤 15：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 track（track）；之后引用 track 时用的就是这次结果。
        GameObject track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 16：把计算结果 "VR movement speed slider" 保存为name；下一条判断或调用会读取这个具体值。
        track.name = "VR movement speed slider";
        // 步骤 17：把计算结果 track.transform 保存为VR 速度滑块轨道；下一条判断或调用会读取这个具体值。
        vrSpeedTrack = track.transform;
        // 步骤 18：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrSpeedTrack.SetParent(speedHud, false);
        // 步骤 19：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        vrSpeedTrack.localPosition = new Vector3(0f, -.037f, 0f);
        // 步骤 20：恢复 vrSpeedTrack.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        vrSpeedTrack.localScale = new Vector3(.34f, .027f, .022f);
        vrSpeedHitArea = track.GetComponent<BoxCollider>();
        // 只扩大可选中范围，保留速度条的位置和视觉缩放。
        vrSpeedHitArea.size = new Vector3(1.3f, 6f, 3f);
        vrSpeedHitArea.isTrigger = true;
        // 步骤 24：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        track.GetComponent<Renderer>().sharedMaterial =
            // 步骤 25：调用 Material：材质。
            SkyQuestVisuals.Material(new Color(.34f, .48f, .59f));

        // 步骤 26：计算 GameObject.CreatePrimitive(PrimitiveType.Cube)，把结果存进 fill（fill）；之后引用 fill 时用的就是这次结果。
        GameObject fill = GameObject.CreatePrimitive(PrimitiveType.Cube);
        // 步骤 27：把计算结果 "Speed selected range" 保存为name；下一条判断或调用会读取这个具体值。
        fill.name = "Speed selected range";
        // 步骤 28：把计算结果 fill.transform 保存为VR 速度滑块进度条；下一条判断或调用会读取这个具体值。
        vrSpeedFill = fill.transform;
        // 步骤 29：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrSpeedFill.SetParent(vrSpeedTrack, false);
        // 步骤 30：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(fill);
        // 步骤 31：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        fill.GetComponent<Renderer>().sharedMaterial =
            // 步骤 32：调用 Material：材质。
            SkyQuestVisuals.Material(new Color(.44f, .86f, .91f), true);

        // 步骤 33：计算 GameObject.CreatePrimitive(PrimitiveType.Sphere)，把结果存进 knob（knob）；之后引用 knob 时用的就是这次结果。
        GameObject knob = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        // 步骤 34：把计算结果 "VR speed slider handle" 保存为name；下一条判断或调用会读取这个具体值。
        knob.name = "VR speed slider handle";
        // 步骤 35：把计算结果 knob.transform 保存为VR 速度滑块手柄；下一条判断或调用会读取这个具体值。
        vrSpeedKnob = knob.transform;
        // 步骤 36：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrSpeedKnob.SetParent(vrSpeedTrack, false);
        // 步骤 37：恢复 vrSpeedKnob.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        vrSpeedKnob.localScale = new Vector3(.16f, 2f, 2f);
        // 步骤 38：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(knob);
        // 步骤 39：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        knob.GetComponent<Renderer>().sharedMaterial =
            // 步骤 40：调用 Material：材质。
            SkyQuestVisuals.Material(new Color(1f, 0.78f, 0.35f));
        // 步骤 41：把计算结果 SkyQuestVisuals.Text("Speed label", "", speedHud, 保存为VR 速度数值文字；下一条判断或调用会读取这个具体值。
        vrSpeedLabel = SkyQuestVisuals.Text("Speed label", "", speedHud,
            // 步骤 42：用 new 创建 Vector3(0f, .025f, -.01f), 2.2f, Color.white, 3.3f);；构造参数决定新对象的初始内容。
            new Vector3(0f, .025f, -.01f), 2.2f, Color.white, 3.3f);
        // 步骤 43：把 TMPro.TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        vrSpeedLabel.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        // 步骤 44：把计算结果 SkyQuestVisuals.Text("Speed slider instructions", 保存为VR 速度提示文字；下一条判断或调用会读取这个具体值。
        vrSpeedHint = SkyQuestVisuals.Text("Speed slider instructions",
            // 步骤 45：继续填写上一行调用的参数："1.0x     HOLD RIGHT TRIGGER TO ADJUST     4.0x", speedHud；参数位置决定它表示的坐标、速度或选项。
            "1.0x     HOLD RIGHT TRIGGER TO ADJUST     4.0x", speedHud,
            // 步骤 46：用 new 创建 Vector3(0f, -.105f, -.01f), 1.15f,；构造参数决定新对象的初始内容。
            new Vector3(0f, -.105f, -.01f), 1.15f,
            // 步骤 47：用 new 创建 Color(.82f, .94f, 1f), 3.8f);；构造参数决定新对象的初始内容。
            new Color(.82f, .94f, 1f), 3.8f);
        // 步骤 48：把 TMPro.TextWrappingModes.NoWrap 写入textWrapping模式；TextMeshPro 会把新文字显示给玩家。
        vrSpeedHint.textWrappingMode = TMPro.TextWrappingModes.NoWrap;
        // 步骤 49：调用 RefreshVrSpeedSlider：把当前速度倍率写回滑块和文字显示。
        RefreshVrSpeedSlider();
    }
    // ！！！功能结束：CreateVrSpeedSlider！！！

    // ！！！功能：UpdateVrSpeedSlider——检测手柄射线和扳机，允许玩家拖动速度滑块！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateVrSpeedSlider()
    {
        if (rightHand == null)
        {
            speedSliderCapturesTrigger = false;
            return;
        }
        UpdateVrSpeedSlider(new Ray(rightHand.position, rightHand.forward),
            SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f);
    }

    private void UpdateVrSpeedSlider(Ray pointer, bool trigger)
    {
        if (!trigger || desktopControlled || ThirdPerson || vrSpeedTrack == null ||
            vrSpeedHitArea == null || !vrSpeedHitArea.enabled ||
            !vrSpeedHitArea.gameObject.activeInHierarchy)
        {
            speedSliderCapturesTrigger = false;
            return;
        }

        // 头显 HUD 直接检测当前平面，避免角色、手持物和滞后的物理位置挡住选择。
        Plane face = new Plane(vrSpeedTrack.forward, vrSpeedTrack.position);
        if (!face.Raycast(pointer, out float distance) || distance > 3f) return;
        Vector3 localHit = vrSpeedTrack.InverseTransformPoint(pointer.GetPoint(distance));
        if (!speedSliderCapturesTrigger)
        {
            Vector3 offset = localHit - vrSpeedHitArea.center;
            Vector3 halfSize = vrSpeedHitArea.size * .5f;
            if (Mathf.Abs(offset.x) > halfSize.x || Mathf.Abs(offset.y) > halfSize.y) return;
            // 允许先按住扳机再瞄准；捕获后持续拖动，直到松开扳机。
            speedSliderCapturesTrigger = true;
            SkyQuestInput.Pulse(SkyQuestInput.Right, .18f, .04f);
        }
        SetMovementScale(Mathf.Lerp(1f, 4f, Mathf.InverseLerp(-.5f, .5f, localHit.x)));
    }
    // ！！！功能结束：UpdateVrSpeedSlider！！！

    // ！！！功能：RefreshVrSpeedSlider——把当前速度倍率写回滑块和文字显示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshVrSpeedSlider()
    {
        // 步骤 1：计算 Mathf.InverseLerp(1f, 4f, movementScale)，把结果存进 progress（progress）；之后引用 progress 时用的就是这次结果。
        float progress = Mathf.InverseLerp(1f, 4f, movementScale);
        // 步骤 2：确认VR 速度滑块进度条已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (vrSpeedFill != null)
        {
            // 步骤 3：恢复 vrSpeedFill.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            vrSpeedFill.localScale = new Vector3(Mathf.Max(.01f, progress), 1.08f, 1.25f);
            // 步骤 4：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            vrSpeedFill.localPosition = new Vector3(-.5f + progress * .5f, 0f, -.53f);
        }
        // 步骤 5：确认VR 速度滑块手柄已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (vrSpeedKnob != null)
            // 步骤 6：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            vrSpeedKnob.localPosition = new Vector3(Mathf.Lerp(-.5f, .5f, progress), 0f, -.9f);
        // 步骤 7：确认VR 速度数值文字已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (vrSpeedLabel != null)
            // 步骤 8：把新内容写入 vrSpeedLabel.text；玩家看到的文字会在本帧更新。
            vrSpeedLabel.text = "MOVE SPEED  " + movementScale.ToString("0.0") + "x";
    }
    // ！！！功能结束：RefreshVrSpeedSlider！！！

    // ！！！功能：UpdatePerspectiveVisuals——按当前视角状态更新角色、斗篷和相机表现！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdatePerspectiveVisuals()
    {
        // 步骤 1：检查条件 desktopControlled；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (desktopControlled) return;
        // 步骤 2：用 firstPerson 记住此处真假结果（!ThirdPerson），供下一条 if 选择执行路径。
        bool firstPerson = !ThirdPerson;
        // 步骤 3：检查VR 速度滑块轨道是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrSpeedTrack != null && vrSpeedTrack.gameObject.activeSelf != firstPerson)
            // 步骤 4：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            vrSpeedTrack.gameObject.SetActive(firstPerson);
        // 步骤 5：检查VR 速度滑块底板是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrSpeedBacking != null && vrSpeedBacking.gameObject.activeSelf != firstPerson)
            // 步骤 6：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            vrSpeedBacking.gameObject.SetActive(firstPerson);
        // 步骤 7：检查VR 速度数值文字是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrSpeedLabel != null && vrSpeedLabel.gameObject.activeSelf != firstPerson)
            // 步骤 8：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            vrSpeedLabel.gameObject.SetActive(firstPerson);
        // 步骤 9：检查VR 速度提示文字是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrSpeedHint != null && vrSpeedHint.gameObject.activeSelf != firstPerson)
            // 步骤 10：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            vrSpeedHint.gameObject.SetActive(firstPerson);
        // 步骤 11：执行 fanHUD?.SetVisible(firstPerson);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        fanHUD?.SetVisible(firstPerson);
    }
    // ！！！功能结束：UpdatePerspectiveVisuals！！！

    // ！！！功能：SetMovementScale——设置Movement缩放！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float selected 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetMovementScale(float selected)
    {
        // 步骤 1：计算selected并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        selected = Mathf.Clamp(selected, 1f, 4f);
        // 步骤 2：检查条件 Mathf.Abs(selected - movementScale) < 0.001f；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (Mathf.Abs(selected - movementScale) < 0.001f) return;
        // 步骤 3：把计算结果 selected 保存为玩家可调速度倍率；下一条判断或调用会读取这个具体值。
        movementScale = selected;
        // 步骤 4：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
        PlayerPrefs.SetFloat("SkyQuestMovementScaleV2", movementScale);
        // 步骤 5：调用 RefreshVrSpeedSlider：把当前速度倍率写回滑块和文字显示。
        RefreshVrSpeedSlider();
    }
    // ！！！功能结束：SetMovementScale！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void LateUpdate()
    {
        // 步骤 1：检查头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!desktopControlled || head == null) return;
        // 步骤 2：调用 UpdateAvatar：更新Avatar。
        UpdateAvatar(Time.deltaTime);
        // 步骤 3：调用 UpdateVignette：更新Vignette。
        UpdateVignette();
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：DesktopMove——把键盘方向转换成水平移动并处理桌面碰撞！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector2 axes, bool ascend, bool descend, bool launch 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopMove(Vector2 axes, bool ascend, bool descend, bool launch)
    {
        // 步骤 1：检查头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!desktopControlled || head == null) return;
        // 步骤 2：计算 Mathf.Max(Time.deltaTime, 0.001f)，把结果存进 dt（dt）；之后引用 dt 时用的就是这次结果。
        float dt = Mathf.Max(Time.deltaTime, 0.001f);
        // 步骤 3：这里用 && 要求两边条件同时成立：launch && !IsFlying；任意一边为假就跳过本分支。
        if (launch && !IsFlying)
        {
            // 步骤 4：检查斗篷能量组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (capeEnergy != null && capeEnergy.TryConsumeBoostEnergy())
            {
                // 步骤 5：把 IsFlying 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                IsFlying = true;
                // 步骤 6：把计算结果 1.5f 保存为当前竖直移动速度；下一条判断或调用会读取这个具体值。
                verticalSpeed = 1.5f;
                // 步骤 7：根据当前游戏时间计算并保存next桌面FlightCost，用于后续延迟或冷却判断。
                nextDesktopFlightCost = Time.time + .8f;
                // 步骤 8：执行 capeVisual?.PlayBoost();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                capeVisual?.PlayBoost();
            }
        }
        // 步骤 9：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (IsFlying && ascend && Time.time >= nextDesktopFlightCost)
        {
            // 步骤 10：检查斗篷能量组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (capeEnergy != null && capeEnergy.TryConsumeBoostEnergy())
            {
                // 步骤 11：根据当前游戏时间计算并保存next桌面FlightCost，用于后续延迟或冷却判断。
                nextDesktopFlightCost = Time.time + .8f;
                // 步骤 12：执行 capeVisual?.PlayBoost();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                capeVisual?.PlayBoost();
            }
            // 步骤 13：前面的 if 条件为假时执行这里的备用处理。
            else ascend = false;
        }
        // The third-person avatar belongs to the player rig. A tracked head
        // rotation only changes the view; it must not orbit the character.
        // 步骤 14：把三维方向投影到水平面并存为forward，剔除头显抬头低头造成的竖直分量。
        Vector3 forward = Vector3.ProjectOnPlane(ThirdPerson ? transform.forward : head.forward,
            // 步骤 15：执行 Vector3.up).normalized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.up).normalized;
        // 步骤 16：检查条件 forward.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 forward = transform.forward;
        if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
        // 步骤 17：用竖直向量与前方向做叉积，算出与前方垂直的右方向。
        Vector3 right = Vector3.Cross(Vector3.up, forward);
        // 步骤 18：把计算结果 Vector3.ClampMagnitude(forward * axes.y + right * axes.x, 1f) * 保存为target；下一条判断或调用会读取这个具体值。
        Vector3 target = Vector3.ClampMagnitude(forward * axes.y + right * axes.x, 1f) *
            // 步骤 19：执行 (IsFlying ? 2.3f : groundSpeed) * movementScale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (IsFlying ? 2.3f : groundSpeed) * movementScale;
        // 步骤 20：检查条件 target.sqrMagnitude > 0.0025f；只有为真才运行下面的步骤，否则沿原流程继续。
        if (target.sqrMagnitude > 0.0025f)
            // 步骤 21：把计算结果 target.normalized 保存为desktopFacing；下一条判断或调用会读取这个具体值。
            desktopFacing = target.normalized;
        // 步骤 22：这里用 && 要求两边条件同时成立：axes.sqrMagnitude < 0.001f && !IsFlying；任意一边为假就跳过本分支。
        if (axes.sqrMagnitude < 0.001f && !IsFlying)
            // 步骤 23：把 horizontalVelocity 清为零向量；清除上次移动、投掷或位置偏移的残留值。
            horizontalVelocity = Vector3.zero;
        // 步骤 24：前面的 if 条件为假时执行这里的备用处理。
        else
            // 步骤 25：把计算结果 Vector3.MoveTowards(horizontalVelocity, target, 12f * dt) 保存为当前水平速度；下一条判断或调用会读取这个具体值。
            horizontalVelocity = Vector3.MoveTowards(horizontalVelocity, target, 12f * dt);
        // 步骤 26：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。
        if (IsFlying)
            // 步骤 27：让当前竖直移动速度以限定的最大变化量逐帧靠近目标值，避免突然改变。
            verticalSpeed = Mathf.MoveTowards(verticalSpeed,
                // 步骤 28：这一行补充 ascend && capeEnergy != null && capeEnergy.CanBoost() ? 1.4f :，与相邻行合起来完成同一次计算或对象配置。
                ascend && capeEnergy != null && capeEnergy.CanBoost() ? 1.4f :
                // 步骤 29：执行 descend ? -1.2f : -0.45f, 2.5f * dt);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                descend ? -1.2f : -0.45f, 2.5f * dt);
        // 步骤 30：调用 MoveRig：根据输入、重力或飞行结果移动整个玩家根节点。
        MoveRig((horizontalVelocity + Vector3.up * (IsFlying ? verticalSpeed : -2f)) * dt, dt);
    }
    // ！！！功能结束：DesktopMove！！！

    // ！！！功能：TogglePerspective——根据输入在第一与第三人称之间切换！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void TogglePerspective()
    {
        // 步骤 1：把计算结果 !ThirdPerson 保存为ThirdPerson；下一条判断或调用会读取这个具体值。
        ThirdPerson = !ThirdPerson;
        // 步骤 2：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
        SkyQuestInput.Pulse(SkyQuestInput.Right);
    }
    // ！！！功能结束：TogglePerspective！！！

    // ！！！功能：SetPerspective——调整相机、角色可见性和第三人称跟随距离！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool thirdPerson 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    // 原理：切视角只改变显示位置和角色可见性，头显追踪仍决定真实视点。
    public void SetPerspective(bool thirdPerson)
    {
        // 步骤 1：把计算结果 thirdPerson 保存为ThirdPerson；下一条判断或调用会读取这个具体值。
        ThirdPerson = thirdPerson;
    }
    // ！！！功能结束：SetPerspective！！！

    // ！！！功能：AddWindLift——添加WindLift！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void AddWindLift(float amount)
    {
        // 步骤 1：检查斗篷飞行控制组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 { capeFlight.AddWindLift(amount); return; }
        if (!desktopControlled && capeFlight != null) { capeFlight.AddWindLift(amount); return; }
        // 步骤 2：检查条件 IsFlying；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 verticalSpeed = Mathf.Min(verticalSpeed + amount, launchSpeed);
        if (IsFlying) verticalSpeed = Mathf.Min(verticalSpeed + amount, launchSpeed);
    }
    // ！！！功能结束：AddWindLift！！！

    // ！！！功能：ShowEquippedAppearance——ShowEquippedAppearance！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void ShowEquippedAppearance()
    {
        // 步骤 1：默认使用第三人称，让玩家开场能看到完整角色与斗篷。
        ThirdPerson = true;
        // 步骤 2：确认角色模型已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
        if (avatar != null) avatarRenderers = avatar.GetComponentsInChildren<Renderer>(true);
    }
    // ！！！功能结束：ShowEquippedAppearance！！！

    // ！！！功能：UpdateAvatar——更新Avatar！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateAvatar(float dt)
    {
        // 步骤 1：检查角色模型是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (avatar == null) return;
        // 步骤 2：检查条件 desktopControlled；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopControlled)
        {
            // 步骤 3：调用 UpdateDesktopCameraAndAvatar：更新桌面相机和Avatar。
            UpdateDesktopCameraAndAvatar(dt);
            // 步骤 4：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 5：把三维方向投影到水平面并存为forward，剔除头显抬头低头造成的竖直分量。
        Vector3 forward = Vector3.ProjectOnPlane(
            // 步骤 6：执行 ThirdPerson ? transform.forward : head.forward, Vector3.up).normalized;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ThirdPerson ? transform.forward : head.forward, Vector3.up).normalized;
        // 步骤 7：检查条件 forward.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 forward = transform.forward;
        if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
        // Room-scale movement changes the real eye position without moving the rig root.
        // 步骤 8：把计算结果 ThirdPerson ? transform.position : 保存为viewAnchor；下一条判断或调用会读取这个具体值。
        Vector3 viewAnchor = ThirdPerson ? transform.position :
            // 步骤 9：用 new 创建 Vector3(head.position.x, transform.position.y, head.position.z);；构造参数决定新对象的初始内容。
            new Vector3(head.position.x, transform.position.y, head.position.z);
        // 步骤 10：用三元表达式为 avatarDistance 二选一：问号前条件为真取前值，为假取冒号后的值。
        float avatarDistance = ThirdPerson ? thirdPersonDistance : 0f;
        // 步骤 11：检查条件 ThirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (ThirdPerson)
        {
            // 步骤 12：把计算结果 Physics.SphereCastNonAlloc(viewAnchor + Vector3.up * 0.9f, 保存为hitCount；下一条判断或调用会读取这个具体值。
            int hitCount = Physics.SphereCastNonAlloc(viewAnchor + Vector3.up * 0.9f,
                // 步骤 13：继续填写上一行调用的参数：0.55f, forward, avatarCastHits, thirdPersonDistance；参数位置决定它表示的坐标、速度或选项。
                0.55f, forward, avatarCastHits, thirdPersonDistance,
                // 步骤 14：执行 ~0, QueryTriggerInteraction.Ignore);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                ~0, QueryTriggerInteraction.Ignore);
            // 步骤 15：按 int i = 0; i < hitCount; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < hitCount; i++)
            {
                // 步骤 16：计算 avatarCastHits[i]，把结果存进 hit（hit）；之后引用 hit 时用的就是这次结果。
                RaycastHit hit = avatarCastHits[i];
                // 步骤 17：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (hit.collider == null || hit.collider.transform == transform ||
                    // 步骤 18：这一行补充 hit.collider.transform.IsChildOf(transform) ||，与相邻行合起来完成同一次计算或对象配置。
                    hit.collider.transform.IsChildOf(transform) ||
                    // 步骤 19：这一行补充 hit.collider.transform.IsChildOf(avatar.transform) ||，与相邻行合起来完成同一次计算或对象配置。
                    hit.collider.transform.IsChildOf(avatar.transform) ||
                    // 步骤 20：调用 IsRewardCollider：IsRewardCollider。
                    IsRewardCollider(hit.collider)) continue;
                // 步骤 21：把计算结果 Mathf.Min(avatarDistance, Mathf.Max(0f, hit.distance - 0.55f)) 保存为avatarDistance；下一条判断或调用会读取这个具体值。
                avatarDistance = Mathf.Min(avatarDistance, Mathf.Max(0f, hit.distance - 0.55f));
            }

            // Keep the full avatar capsule clear of columns, rails, and walls.
            // Search backward from the desired camera distance until the body fits.
            // 步骤 22：计算 avatarDistance，把结果存进 safeDistance（safeDistance）；之后引用 safeDistance 时用的就是这次结果。
            float safeDistance = avatarDistance;
            // 步骤 23：按 int i = 0; i < 10 && !AvatarFits(viewAnchor + forward * safeDistance + 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < 10 && !AvatarFits(viewAnchor + forward * safeDistance +
                // 步骤 24：这一行补充 Vector3.up * avatarStartPosition.y); i++)，与相邻行合起来完成同一次计算或对象配置。
                Vector3.up * avatarStartPosition.y); i++)
                // 步骤 25：把计算结果 Mathf.Max(0f, safeDistance - 0.22f) 保存为safeDistance；下一条判断或调用会读取这个具体值。
                safeDistance = Mathf.Max(0f, safeDistance - 0.22f);
            // 步骤 26：把计算结果 safeDistance 保存为avatarDistance；下一条判断或调用会读取这个具体值。
            avatarDistance = safeDistance;
        }
        // 步骤 27：用 clearThirdPersonView 记住此处真假结果（!ThirdPerson || avatarDistance >= MinVrAvatarDistance），供下一条 if 选择执行路径。
        bool clearThirdPersonView = !ThirdPerson || avatarDistance >= MinVrAvatarDistance;
        // 步骤 28：把计算结果 viewAnchor + forward * avatarDistance + 保存为target；下一条判断或调用会读取这个具体值。
        Vector3 target = viewAnchor + forward * avatarDistance +
            // 步骤 29：执行 Vector3.up * avatarStartPosition.y;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.up * avatarStartPosition.y;
        // 步骤 30：检查条件 clearThirdPersonView；只有为真才运行下面的步骤，否则沿原流程继续。
        if (clearThirdPersonView)
            // 步骤 31：继续填写上一行调用的参数：avatar.transform.position = Vector3.MoveTowards(avatar.transform.position, target；参数位置决定它表示的坐标、速度或选项。
            avatar.transform.position = Vector3.MoveTowards(avatar.transform.position, target,
                // 步骤 32：调用 Max：Max。
                Mathf.Max(perspectiveBlendSpeed, 10f) * dt);
        // Keep the third-person avatar still when the player merely turns
        // their head. During a walk, face the actual stick-driven direction.
        // 步骤 33：计算 forward，把结果存进 facing（facing）；之后引用 facing 时用的就是这次结果。
        Vector3 facing = forward;
        // 步骤 34：检查斗篷飞行控制组件是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (ThirdPerson && capeFlight != null)
        {
            // 步骤 35：把原三维朝向投影到地面平面并存为 walking；行走或转向不受头部俯仰影响。
            Vector3 walking = Vector3.ProjectOnPlane(capeFlight.Velocity, Vector3.up);
            // 步骤 36：检查条件 walking.sqrMagnitude > .09f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 facing = walking.normalized;
            if (walking.sqrMagnitude > .09f) facing = walking.normalized;
        }
        // The imported COTL avatar faces local -Z in both scenes.
        // 步骤 37：继续填写上一行调用的参数：avatar.transform.rotation = Quaternion.Slerp(avatar.transform.rotation；参数位置决定它表示的坐标、速度或选项。
        avatar.transform.rotation = Quaternion.Slerp(avatar.transform.rotation,
            // 步骤 38：调用 LookRotation：Look旋转。
            Quaternion.LookRotation(-facing, Vector3.up), Mathf.Clamp01(8f * dt));
        // 步骤 39：检查角色各部分渲染器是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (avatarRenderers == null) return;
        // 步骤 40：把原三维朝向投影到地面平面并存为 visibleDistance；行走或转向不受头部俯仰影响。
        float visibleDistance = Vector3.ProjectOnPlane(avatar.transform.position - head.position, Vector3.up).magnitude;
        // 步骤 41：用 showAvatar 记住此处真假结果（ThirdPerson && clearThirdPersonView && visibleDistance >= MinVrAvatarDistance），供下一条 if 选择执行路径。
        bool showAvatar = ThirdPerson && clearThirdPersonView && visibleDistance >= MinVrAvatarDistance;
        // 步骤 42：逐个读取 Renderer renderer in avatarRenderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in avatarRenderers) if (renderer != null) renderer.enabled = showAvatar;
    }
    // ！！！功能结束：UpdateAvatar！！！

    // ！！！功能：UpdateDesktopCameraAndAvatar——更新桌面相机和Avatar！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateDesktopCameraAndAvatar(float dt)
    {
        // The rig and its CharacterController are the player's actual position.
        // Keep the visual character there and move only the camera for third person.
        // 步骤 1：执行 avatar.transform.position = transform.position + Vector3.up * avatarStartPosition.y;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        avatar.transform.position = transform.position + Vector3.up * avatarStartPosition.y;
        // 步骤 2：用三元表达式为 facing 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 facing = desktopFacing.sqrMagnitude > 0.01f ? desktopFacing : transform.forward;
        // 步骤 3：执行 avatar.transform.rotation = Quaternion.LookRotation(-facing, Vector3.up);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        avatar.transform.rotation = Quaternion.LookRotation(-facing, Vector3.up);

        // 步骤 4：把原三维朝向投影到地面平面并存为 viewForward；行走或转向不受头部俯仰影响。
        Vector3 viewForward = Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized;
        // 步骤 5：检查条件 viewForward.sqrMagnitude < 0.01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 viewForward = transform.forward;
        if (viewForward.sqrMagnitude < 0.01f) viewForward = transform.forward;
        // 步骤 6：用三元表达式为 distance 二选一：问号前条件为真取前值，为假取冒号后的值。
        float distance = ThirdPerson ? Mathf.Max(3.8f, thirdPersonDistance) : 0f;
        // 步骤 7：检查条件 ThirdPerson；只有为真才运行下面的步骤，否则沿原流程继续。
        if (ThirdPerson)
        {
            // 步骤 8：计算 transform.position + Vector3.up * DesktopEyeHeight，把结果存进 origin（origin）；之后引用 origin 时用的就是这次结果。
            Vector3 origin = transform.position + Vector3.up * DesktopEyeHeight;
            // 步骤 9：把计算结果 Physics.SphereCastNonAlloc(origin, DesktopCameraRadius, 保存为count；下一条判断或调用会读取这个具体值。
            int count = Physics.SphereCastNonAlloc(origin, DesktopCameraRadius,
                // 步骤 10：执行 -viewForward, avatarCastHits, distance, ~0, QueryTriggerInteraction.Ignore);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                -viewForward, avatarCastHits, distance, ~0, QueryTriggerInteraction.Ignore);
            // 步骤 11：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < count; i++)
            {
                // 步骤 12：计算 avatarCastHits[i]，把结果存进 hit（hit）；之后引用 hit 时用的就是这次结果。
                RaycastHit hit = avatarCastHits[i];
                // 步骤 13：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (hit.collider == null || hit.collider.transform == transform ||
                    // 步骤 14：这一行补充 hit.collider.transform.IsChildOf(transform) ||，与相邻行合起来完成同一次计算或对象配置。
                    hit.collider.transform.IsChildOf(transform) ||
                    // 步骤 15：调用 IsRewardCollider：IsRewardCollider。
                    IsRewardCollider(hit.collider)) continue;
                // 步骤 16：把计算结果 Mathf.Min(distance, Mathf.Max(0.35f, hit.distance - DesktopCameraRadius)) 保存为distance；下一条判断或调用会读取这个具体值。
                distance = Mathf.Min(distance, Mathf.Max(0.35f, hit.distance - DesktopCameraRadius));
            }
        }
        // 步骤 17：把position更新为 transform.position + Vector3.up * DesktopEyeHeight - viewForward * distance；世界或局部位置会立即影响物体摆放。
        head.position = transform.position + Vector3.up * DesktopEyeHeight - viewForward * distance;
        // 步骤 18：检查角色各部分渲染器是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (avatarRenderers == null) return;
        // 步骤 19：逐个读取 Renderer renderer in avatarRenderers 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in avatarRenderers)
            // 步骤 20：确认renderer已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 renderer.enabled = ThirdPerson;
            if (renderer != null) renderer.enabled = ThirdPerson;
    }
    // ！！！功能结束：UpdateDesktopCameraAndAvatar！！！

    // ！！！功能：AvatarFits——AvatarFits！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 position 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool AvatarFits(Vector3 position)
    {
        // Keep the feet above the support surface; the old capsule crossed the floor
        // and treated every otherwise clear avatar position as blocked.
        // 步骤 1：计算 position + Vector3.up * 0.55f，把结果存进 bottom（bottom）；之后引用 bottom 时用的就是这次结果。
        Vector3 bottom = position + Vector3.up * 0.55f;
        // 步骤 2：计算 position + Vector3.up * 1.55f，把结果存进 top（top）；之后引用 top 时用的就是这次结果。
        Vector3 top = position + Vector3.up * 1.55f;
        // 步骤 3：把计算结果 Physics.OverlapCapsuleNonAlloc(bottom, top, 0.43f, avatarOverlapHits, 保存为count；下一条判断或调用会读取这个具体值。
        int count = Physics.OverlapCapsuleNonAlloc(bottom, top, 0.43f, avatarOverlapHits,
            // 步骤 4：执行 ~0, QueryTriggerInteraction.Ignore);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ~0, QueryTriggerInteraction.Ignore);
        // 步骤 5：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 6：计算 avatarOverlapHits[i]，把结果存进 collider（collider）；之后引用 collider 时用的就是这次结果。
            Collider collider = avatarOverlapHits[i];
            // 步骤 7：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (collider == null || collider.transform == transform ||
                // 步骤 8：这一行补充 collider.transform.IsChildOf(transform) ||，与相邻行合起来完成同一次计算或对象配置。
                collider.transform.IsChildOf(transform) ||
                // 步骤 9：这一行补充 (avatar != null && collider.transform.IsChildOf(avatar.transform)) ||，与相邻行合起来完成同一次计算或对象配置。
                (avatar != null && collider.transform.IsChildOf(avatar.transform)) ||
                // 步骤 10：调用 IsRewardCollider：IsRewardCollider。
                IsRewardCollider(collider)) continue;
            // 步骤 11：本次检测或激活失败，返回 false 告诉调用方不要继续处理。
            return false;
        }
        // 步骤 12：本次检测或激活成功，返回 true 让调用方继续相应反馈。
        return true;
    }
    // ！！！功能结束：AvatarFits！！！

    // ！！！功能：IsRewardCollider——IsRewardCollider！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Collider collider 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private static bool IsRewardCollider(Collider collider)
    {
        // 步骤 1：把 collider.GetComponentInParent<SkyQuestMagicPickup>() != null 作为方法结果返回给调用方；return 后本方法立即结束。
        return collider.GetComponentInParent<SkyQuestMagicPickup>() != null;
    }
    // ！！！功能结束：IsRewardCollider！！！

    // ！！！功能：CreateVignette——创建Vignette！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateVignette()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：计算 Shader.Find("Universal Render Pipeline/Unlit")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        // 步骤 3：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        // 步骤 4：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (shader == null) return;
        // 步骤 5：新建用于vignette材质的对象或数值结构，并把构造参数保存进去。
        vignetteMaterial = new Material(shader) { color = new Color(0f, 0f, 0f, 0f) };
        // 步骤 6：检查条件 vignetteMaterial.HasProperty("_Surface")；只有为真才运行下面的步骤，否则沿原流程继续。
        if (vignetteMaterial.HasProperty("_Surface"))
        {
            // 步骤 7：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            vignetteMaterial.SetFloat("_Surface", 1f);
            // 步骤 8：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            vignetteMaterial.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            // 步骤 9：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            vignetteMaterial.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            // 步骤 10：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
            vignetteMaterial.SetFloat("_ZWrite", 0f);
            // 步骤 11：调用 EnableKeyword：EnableKeyword。
            vignetteMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            // 步骤 12：把计算结果 3000 保存为renderQueue；下一条判断或调用会读取这个具体值。
            vignetteMaterial.renderQueue = 3000;
        }
        // 步骤 13：创建新的 GameObject 并存为 ring；构造参数决定其初始内容或对象名称。
        GameObject ring = new GameObject("Motion comfort vignette", typeof(MeshFilter), typeof(MeshRenderer));
        // 步骤 14：把计算结果 ring.transform 保存为vignette；下一条判断或调用会读取这个具体值。
        vignette = ring.transform;
        // 步骤 15：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vignette.SetParent(head, false);
        // 步骤 16：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        vignette.localPosition = new Vector3(0f, 0f, 0.52f);
        // 步骤 17：恢复 vignette.localRotation 到初始本地朝向；本地表示相对于父对象。
        vignette.localRotation = Quaternion.identity;
        // 步骤 18：创建新的 Mesh 并存为 mesh；构造参数决定其初始内容或对象名称。
        Mesh mesh = new Mesh();
        // 步骤 19：定义不可在运行时改变的数值 segments（segments）；后续计算统一使用这个阈值、速度或尺寸。
        const int segments = 64;
        // 步骤 20：创建新的 Vector3[] 并存为 vertices；构造参数决定其初始内容或对象名称。
        Vector3[] vertices = new Vector3[segments * 2];
        // 步骤 21：创建新的 Color[] 并存为 colors；构造参数决定其初始内容或对象名称。
        Color[] colors = new Color[vertices.Length];
        // 步骤 22：创建新的 int[] 并存为 triangles；构造参数决定其初始内容或对象名称。
        int[] triangles = new int[segments * 6];
        // 步骤 23：按 int i = 0; i < segments; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < segments; i++)
        {
            // 步骤 24：计算 i * Mathf.PI * 2f / segments，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = i * Mathf.PI * 2f / segments;
            // 步骤 25：执行 vertices[i * 2] = new Vector3(Mathf.Cos(angle) * 0.36f, Mathf.Sin(angle) * 0.36f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            vertices[i * 2] = new Vector3(Mathf.Cos(angle) * 0.36f, Mathf.Sin(angle) * 0.36f, 0f);
            // 步骤 26：执行 vertices[i * 2 + 1] = new Vector3(Mathf.Cos(angle) * 0.75f, Mathf.Sin(angle) * 0.75f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            vertices[i * 2 + 1] = new Vector3(Mathf.Cos(angle) * 0.75f, Mathf.Sin(angle) * 0.75f, 0f);
            // 步骤 27：执行 colors[i * 2] = new Color(0f, 0f, 0f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            colors[i * 2] = new Color(0f, 0f, 0f, 0f);
            // 步骤 28：执行 colors[i * 2 + 1] = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            colors[i * 2 + 1] = Color.white;
            // 步骤 29：计算 (i + 1) % segments，把结果存进 j（j）；之后引用 j 时用的就是这次结果。
            int j = (i + 1) % segments;
            // 步骤 30：计算 i * 6，把结果存进 k（k）；之后引用 k 时用的就是这次结果。
            int k = i * 6;
            // 步骤 31：执行 triangles[k] = i * 2; triangles[k + 1] = j * 2; triangles[k + 2] = i * 2 + 1;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[k] = i * 2; triangles[k + 1] = j * 2; triangles[k + 2] = i * 2 + 1;
            // 步骤 32：执行 triangles[k + 3] = i * 2 + 1; triangles[k + 4] = j * 2; triangles[k + 5] = j * 2 + 1;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            triangles[k + 3] = i * 2 + 1; triangles[k + 4] = j * 2; triangles[k + 5] = j * 2 + 1;
        }
        // 步骤 33：把计算结果 vertices; mesh.colors = colors; mesh.triangles = triangles 保存为vertices；下一条判断或调用会读取这个具体值。
        mesh.vertices = vertices; mesh.colors = colors; mesh.triangles = triangles;
        // 步骤 34：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        ring.GetComponent<MeshFilter>().sharedMesh = mesh;
        // 步骤 35：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        ring.GetComponent<MeshRenderer>().sharedMaterial = vignetteMaterial;
    }
    // ！！！功能结束：CreateVignette！！！

    // ！！！功能：UpdateVignette——更新Vignette！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateVignette()
    {
        // 步骤 1：检查vignette材质是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (vignetteMaterial == null) return;
        // 步骤 2：把计算结果 useComfortVignette && !ThirdPerson ? 保存为amount；下一条判断或调用会读取这个具体值。
        float amount = useComfortVignette && !ThirdPerson ?
            // 步骤 3：调用 Clamp01：Clamp01。
            Mathf.Clamp01((CurrentSpeed - 2f) / Mathf.Max(0.1f, vignetteAtSpeed)) * 0.55f : 0f;
        // 步骤 4：新建用于color的对象或数值结构，并把构造参数保存进去。
        vignetteMaterial.color = new Color(0f, 0f, 0f, amount);
    }
    // ！！！功能结束：UpdateVignette！！！
}
