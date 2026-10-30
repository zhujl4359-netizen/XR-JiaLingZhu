// ！！！文件功能：鹿头物品的靠近提示、拾取和松手后的物理状态！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.InputSystem 命名空间；下方可直接使用新版键盘、鼠标与输入动作。
using UnityEngine.InputSystem;

/// <summary>
/// 鹿头物品的靠近高亮、E 键拾取和投掷后物理状态恢复。
/// </summary>
// 定义 MaskInteraction 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class MaskInteraction : MonoBehaviour
{
    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("玩家")]
    // 声明 player（player）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform player;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("材质")]
    // 声明 normalMaterial（normal材质）：Unity 材质资源；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Material normalMaterial;
    // 声明 highlightMaterial（highlight材质）：Unity 材质资源；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Material highlightMaterial;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("拾取位置")]
    // 声明 holdPoint（holdPoint）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform holdPoint;

    // 这行只为 Inspector 参数添加分组标题，不会在游戏运行时执行。
    [Header("互动距离")]
    // 把 Inspector 可调整的 interactionDistance（允许靠近先祖的距离）设为默认值 3f；具体用途看它在下方的判断和计算。
    public float interactionDistance = 3f;

    // maskRenderer 保存maskRenderer（类型 Renderer）；下方方法依靠它记录或查找场景状态。
    private Renderer maskRenderer;
    // 声明 rb（rb）：物理刚体引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Rigidbody rb;

    // 用 isNear 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool isNear = false;
    // 用 isPickedUp 记住此处真假结果（false），供下一条 if 选择执行路径。
    private bool isPickedUp = false;

    // 让其他脚本可以知道鹿头是否被拿着
    // 这一行补充 public bool IsPickedUp，与相邻行合起来完成同一次计算或对象配置。
    public bool IsPickedUp
    {
        // 这一行补充 get { return isPickedUp; }，与相邻行合起来完成同一次计算或对象配置。
        get { return isPickedUp; }
    }

    /// <summary>
    /// 获取渲染器与刚体，并设置鹿头的普通材质。
    /// </summary>
    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：从目标 GameObject 查找并保存 maskRenderer 组件；后面的移动或显示逻辑会直接使用这个缓存。
        maskRenderer = GetComponent<Renderer>();
        // 步骤 2：从目标 GameObject 查找并保存 rb 组件；后面的移动或显示逻辑会直接使用这个缓存。
        rb = GetComponent<Rigidbody>();

        // 步骤 3：检查maskRenderer、normal材质是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (maskRenderer != null && normalMaterial != null)
        {
            // 步骤 4：把计算结果 normalMaterial 保存为material；下一条判断或调用会读取这个具体值。
            maskRenderer.material = normalMaterial;
        }
    }
    // ！！！功能结束：Start！！！

    /// <summary>
    /// 计算玩家与鹿头的距离：靠近时高亮并允许按 E 拾取，离开后恢复材质。
    /// </summary>
    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查player是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (player == null)
            // 步骤 2：提前结束这个方法；不再执行下方语句。
            return;

        // 步骤 3：把计算结果 Vector3.Distance( 保存为distance；下一条判断或调用会读取这个具体值。
        float distance = Vector3.Distance(
            // 步骤 4：继续填写上一行调用的参数：player.position；参数位置决定它表示的坐标、速度或选项。
            player.position,
            // 步骤 5：这一行补充 transform.position，与相邻行合起来完成同一次计算或对象配置。
            transform.position
        );

        // 步骤 6：把计算结果 distance <= interactionDistance 保存为isNear；下一条判断或调用会读取这个具体值。
        isNear = distance <= interactionDistance;

        // =====================================
        // 靠近 → 变色
        // =====================================

        // 步骤 7：这里用 && 要求两边条件同时成立：isNear && !isPickedUp；任意一边为假就跳过本分支。
        if (isNear && !isPickedUp)
        {
            // 步骤 8：检查maskRenderer、highlight材质是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (maskRenderer != null && highlightMaterial != null)
            {
                // 步骤 9：把计算结果 highlightMaterial 保存为material；下一条判断或调用会读取这个具体值。
                maskRenderer.material = highlightMaterial;
            }

            // E 拾取
            // 步骤 10：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (Keyboard.current != null &&
                // 步骤 11：这一行补充 Keyboard.current.eKey.wasPressedThisFrame)，与相邻行合起来完成同一次计算或对象配置。
                Keyboard.current.eKey.wasPressedThisFrame)
            {
                // 步骤 12：调用 PickUp：PickUp。
                PickUp();
            }
        }
        // 步骤 13：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。这里的 ! 表示取反；当 isPickedUp 不成立时才进入此分支。
        else if (!isPickedUp)
        {
            // 步骤 14：检查maskRenderer、normal材质是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
            if (maskRenderer != null && normalMaterial != null)
            {
                // 步骤 15：把计算结果 normalMaterial 保存为material；下一条判断或调用会读取这个具体值。
                maskRenderer.material = normalMaterial;
            }
        }
    }
    // ！！！功能结束：Update！！！

    // =====================================
    // 拾取
    // =====================================

    /// <summary>
    /// 冻结鹿头刚体并挂到手持点，使物品跟随角色。
    /// </summary>
    // ！！！功能：PickUp——PickUp！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void PickUp()
    {
        // 步骤 1：把 isPickedUp 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        isPickedUp = true;

        // 步骤 2：确认rb已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (rb != null)
        {
            // 步骤 3：把 rb.isKinematic 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            rb.isKinematic = true;
            // 步骤 4：把 rb.useGravity 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            rb.useGravity = false;

            // 步骤 5：把刚体平移速度清零，停止先前投掷或下落留下的运动。
            rb.linearVelocity = Vector3.zero;
            // 步骤 6：把刚体旋转速度清零，停止先前投掷留下的自转。
            rb.angularVelocity = Vector3.zero;
        }

        // 步骤 7：确认holdPoint已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (holdPoint != null)
        {
            // 步骤 8：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            transform.SetParent(holdPoint);

            // 步骤 9：把 transform.localPosition 清为零向量；清除上次移动、投掷或位置偏移的残留值。
            transform.localPosition = Vector3.zero;
            // 步骤 10：恢复 transform.localRotation 到初始本地朝向；本地表示相对于父对象。
            transform.localRotation = Quaternion.identity;
        }
    }
    // ！！！功能结束：PickUp！！！

    // =====================================
    // 被投掷后调用
    // =====================================

    /// <summary>
    /// 投掷脚本调用后解除手持状态，重新开启刚体和重力。
    /// </summary>
    // ！！！功能：OnThrown——OnThrown！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void OnThrown()
    {
        // 步骤 1：把 isPickedUp 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        isPickedUp = false;

        // 步骤 2：确认rb已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (rb != null)
        {
            // 步骤 3：把 rb.isKinematic 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
            rb.isKinematic = false;
            // 步骤 4：把 rb.useGravity 设为 true（启用/已发生）；后续条件判断会依据这个状态。
            rb.useGravity = true;
        }

        // 确保脱离手部
        // 步骤 5：确认transform的parent已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (transform.parent != null)
        {
            // 步骤 6：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
            transform.SetParent(null);
        }
    }
    // ！！！功能结束：OnThrown！！！
}