// ！！！文件功能：道具预览、摆放和桌面键盘操作！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 SkyQuestProps 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestProps : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // itemTree 保存itemTree（类型 SkyQuestAncestor）；下方方法依靠它记录或查找场景状态。
    public SkyQuestAncestor itemTree;
    // communication 保存communication（类型 SkyQuestCommunication）；下方方法依靠它记录或查找场景状态。
    public SkyQuestCommunication communication;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // 计算 ~0，把结果存进 placementLayers（placementLayers）；之后引用 placementLayers 时用的就是这次结果。
    public LayerMask placementLayers = ~0;

    // 声明 preview（preview）：场景对象引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private GameObject preview;
    // 声明 wasSecondary（wasSecondary）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasSecondary;
    // 声明 wasTrigger（wasTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasTrigger;
    // 声明 validPlacement（validPlacement）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool validPlacement;
    // 声明 placementPoint（placementPoint）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 placementPoint;
    // hint 保存hint（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro hint;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：把计算结果 FindFirstObjectByType<SkyQuestLocomotion>() 保存为locomotion；下一条判断或调用会读取这个具体值。
        locomotion = FindFirstObjectByType<SkyQuestLocomotion>();
        // 步骤 2：确认头显相机已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (head != null)
        {
            // 步骤 3：把计算结果 SkyQuestVisuals.Text("Prop instructions", "", head, 保存为hint；下一条判断或调用会读取这个具体值。
            hint = SkyQuestVisuals.Text("Prop instructions", "", head,
                // 步骤 4：用 new 创建 Vector3(0f, -0.36f, 1.1f), 2f, Color.white, 1.5f);；构造参数决定新对象的初始内容。
                new Vector3(0f, -0.36f, 1.1f), 2f, Color.white, 1.5f);
        }
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查hint、locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (hint != null && !desktopMode && locomotion != null)
            // 步骤 2：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            hint.gameObject.SetActive(!locomotion.ThirdPerson);
        // 步骤 3：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 4：调用 UpdateDesktopPreview：更新桌面Preview。
            UpdateDesktopPreview();
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 6：检查右手柄位置是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (rightHand == null) return;
        // 步骤 7：检查communication是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (communication != null && communication.IsOpen)
        {
            // 步骤 8：确认preview已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { Destroy(preview); preview = null; }
            if (preview != null) { Destroy(preview); preview = null; }
            // 步骤 9：确认hint已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hint.text = "";
            if (hint != null) hint.text = "";
            // 步骤 10：把计算结果 SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f 保存为wasTrigger；下一条判断或调用会读取这个具体值。
            wasTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            // 步骤 11：把计算结果 SkyQuestInput.Secondary(SkyQuestInput.Left) 保存为wasSecondary；下一条判断或调用会读取这个具体值。
            wasSecondary = SkyQuestInput.Secondary(SkyQuestInput.Left);
            // 步骤 12：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 13：用 button 记住此处真假结果（SkyQuestInput.Secondary(SkyQuestInput.Left)），供下一条 if 选择执行路径。
        bool button = SkyQuestInput.Secondary(SkyQuestInput.Left);
        // 步骤 14：检查itemTree是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 TogglePreview：TogglePreview。
        if (button && !wasSecondary && (itemTree == null || !itemTree.IsMenuOpen)) TogglePreview();
        // 步骤 15：把计算结果 button 保存为wasSecondary；下一条判断或调用会读取这个具体值。
        wasSecondary = button;
        // 步骤 16：检查preview是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (preview == null)
        {
            // 步骤 17：确认hint已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hint.text = "Y: place tea table";
            if (hint != null) hint.text = "Y: place tea table";
            // 步骤 18：把计算结果 SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f 保存为wasTrigger；下一条判断或调用会读取这个具体值。
            wasTrigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
            // 步骤 19：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 20：确认hint已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hint.text = "Aim at ground. Right trigger: place; Y: cancel";
        if (hint != null) hint.text = "Aim at ground. Right trigger: place; Y: cancel";
        // 步骤 21：创建新的 Ray 并存为 ray；构造参数决定其初始内容或对象名称。
        Ray ray = new Ray(rightHand.position, rightHand.forward);
        // 步骤 22：把计算结果 Physics.Raycast(ray, out RaycastHit hit, 7f, placementLayers, 保存为validPlacement；下一条判断或调用会读取这个具体值。
        validPlacement = Physics.Raycast(ray, out RaycastHit hit, 7f, placementLayers,
            // 步骤 23：执行 QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        // 步骤 24：检查条件 validPlacement；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 placementPoint = hit.point;
        if (validPlacement) placementPoint = hit.point;
        // 步骤 25：前面的 if 条件为假时执行这里的备用处理。
        else placementPoint = rightHand.position + rightHand.forward * 2f;
        // 步骤 26：执行 preview.transform.position = placementPoint;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        preview.transform.position = placementPoint;
        // 步骤 27：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        // 步骤 28：这里用 && 要求两边条件同时成立：trigger && !wasTrigger && validPlacement；任意一边为假就跳过本分支。
        if (trigger && !wasTrigger && validPlacement)
        {
            // 步骤 29：把计算结果 "Placed tea table" 保存为name；下一条判断或调用会读取这个具体值。
            preview.name = "Placed tea table";
            // 步骤 30：调用 SetOpacity：设置Opacity。
            SetOpacity(preview, 1f);
            // 步骤 31：执行 preview.AddComponent<SkyQuestTeaTable>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            preview.AddComponent<SkyQuestTeaTable>();
            // 步骤 32：清空 preview 对对象的引用；后续访问前需要重新获取或检查是否为 null。
            preview = null;
            // 步骤 33：执行 status?.Show("Tea table placed. Point and click it to chat.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Tea table placed. Point and click it to chat.");
            // 步骤 34：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Right);
        }
        // 步骤 35：把计算结果 trigger 保存为wasTrigger；下一条判断或调用会读取这个具体值。
        wasTrigger = trigger;
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：TogglePreview——TogglePreview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void TogglePreview()
    {
        // 步骤 1：确认preview已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { Destroy(preview); preview = null; return; }
        if (preview != null) { Destroy(preview); preview = null; return; }
        // 步骤 2：把计算结果 CreateTable() 保存为preview；下一条判断或调用会读取这个具体值。
        preview = CreateTable();
        // 步骤 3：调用 SetOpacity：设置Opacity。
        SetOpacity(preview, 0.5f);
        // 步骤 4：这一行补充 status?.Show(desktopMode ? "Aim toward the ground and press R to place a tea table." :，与相邻行合起来完成同一次计算或对象配置。
        status?.Show(desktopMode ? "Aim toward the ground and press R to place a tea table." :
            // 步骤 5：执行 "Aim at the ground and press the right trigger to place a tea table.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Aim at the ground and press the right trigger to place a tea table.", 5f);
    }
    // ！！！功能结束：TogglePreview！！！

    // ！！！功能：UpdateDesktopPreview——更新桌面Preview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateDesktopPreview()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：确认hint已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 hint.text = preview == null ? "P: preview tea table" :
        if (hint != null) hint.text = preview == null ? "P: preview tea table" :
            // 步骤 3：执行 "Arrow Up/Down: aim    R: place    P: cancel";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Arrow Up/Down: aim    R: place    P: cancel";
        // 步骤 4：检查preview是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (preview == null) return;
        // 步骤 5：创建新的 Ray 并存为 ray；构造参数决定其初始内容或对象名称。
        Ray ray = new Ray(head.position, head.forward);
        // 步骤 6：把计算结果 Physics.Raycast(ray, out RaycastHit hit, 8f, placementLayers, 保存为validPlacement；下一条判断或调用会读取这个具体值。
        validPlacement = Physics.Raycast(ray, out RaycastHit hit, 8f, placementLayers,
            // 步骤 7：执行 QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            QueryTriggerInteraction.Ignore) && Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        // 步骤 8：这里的 ! 表示取反；当 validPlacement 不成立时才进入此分支。
        if (!validPlacement)
        {
            // 步骤 9：把原三维朝向投影到地面平面并存为 ahead；行走或转向不受头部俯仰影响。
            Vector3 ahead = head.position + Vector3.ProjectOnPlane(head.forward, Vector3.up).normalized * 2f;
            // 步骤 10：把计算结果 Physics.Raycast(ahead + Vector3.up * 2f, Vector3.down, out hit, 保存为validPlacement；下一条判断或调用会读取这个具体值。
            validPlacement = Physics.Raycast(ahead + Vector3.up * 2f, Vector3.down, out hit,
                // 步骤 11：这一行补充 6f, placementLayers, QueryTriggerInteraction.Ignore) &&，与相邻行合起来完成同一次计算或对象配置。
                6f, placementLayers, QueryTriggerInteraction.Ignore) &&
                // 步骤 12：调用 Dot：Dot。
                Vector3.Dot(hit.normal, Vector3.up) > 0.65f;
        }
        // 步骤 13：执行 preview.transform.position = validPlacement ? hit.point : head.position + head.forward * 2f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        preview.transform.position = validPlacement ? hit.point : head.position + head.forward * 2f;
    }
    // ！！！功能结束：UpdateDesktopPreview！！！

    // ！！！功能：DesktopTogglePreview——桌面TogglePreview！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopTogglePreview()
    {
        // 步骤 1：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 TogglePreview：TogglePreview。
        if (desktopMode) TogglePreview();
    }
    // ！！！功能结束：DesktopTogglePreview！！！

    // ！！！功能：DesktopPlace——桌面Place！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopPlace()
    {
        // 步骤 1：检查preview是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!desktopMode || preview == null || !validPlacement) return;
        // 步骤 2：把计算结果 "Placed tea table" 保存为name；下一条判断或调用会读取这个具体值。
        preview.name = "Placed tea table";
        // 步骤 3：调用 SetOpacity：设置Opacity。
        SetOpacity(preview, 1f);
        // 步骤 4：执行 preview.AddComponent<SkyQuestTeaTable>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        preview.AddComponent<SkyQuestTeaTable>();
        // 步骤 5：清空 preview 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        preview = null;
        // 步骤 6：执行 status?.Show("Tea table placed. Click it to chat.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Tea table placed. Click it to chat.");
    }
    // ！！！功能结束：DesktopPlace！！！

    // ！！！功能：CreateTable——创建Table！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject CreateTable()
    {
        // 步骤 1：创建新的 GameObject 并存为 table；构造参数决定其初始内容或对象名称。
        GameObject table = new GameObject("Tea table preview");
        // 步骤 2：调用 Primitive：Primitive。
        SkyQuestVisuals.Primitive("Table top", PrimitiveType.Cylinder, table.transform,
            // 步骤 3：用 new 创建 Vector3(0f, 0.62f, 0f), new Vector3(0.55f, 0.05f, 0.55f), new Color(0.5f, 0.；构造参数决定新对象的初始内容。
            new Vector3(0f, 0.62f, 0f), new Vector3(0.55f, 0.05f, 0.55f), new Color(0.5f, 0.34f, 0.2f));
        // 步骤 4：按 int i = 0; i < 4; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 4; i++)
        {
            // 步骤 5：用三元表达式为 x 二选一：问号前条件为真取前值，为假取冒号后的值。
            float x = (i % 2 == 0 ? -1f : 1f) * 0.38f;
            // 步骤 6：用三元表达式为 z 二选一：问号前条件为真取前值，为假取冒号后的值。
            float z = (i < 2 ? -1f : 1f) * 0.38f;
            // 步骤 7：调用 Primitive：Primitive。
            SkyQuestVisuals.Primitive("Table leg", PrimitiveType.Cylinder, table.transform,
                // 步骤 8：用 new 创建 Vector3(x, 0.3f, z), new Vector3(0.05f, 0.3f, 0.05f), new Color(0.45f, 0.28f；构造参数决定新对象的初始内容。
                new Vector3(x, 0.3f, z), new Vector3(0.05f, 0.3f, 0.05f), new Color(0.45f, 0.28f, 0.15f));
        }
        // 步骤 9：调用 Primitive：Primitive。
        SkyQuestVisuals.Primitive("Teapot", PrimitiveType.Sphere, table.transform,
            // 步骤 10：用 new 创建 Vector3(0f, 0.78f, 0f), new Vector3(0.18f, 0.12f, 0.18f), new Color(0.92f, 0；构造参数决定新对象的初始内容。
            new Vector3(0f, 0.78f, 0f), new Vector3(0.18f, 0.12f, 0.18f), new Color(0.92f, 0.8f, 0.55f));
        // 步骤 11：把 table 作为方法结果返回给调用方；return 后本方法立即结束。
        return table;
    }
    // ！！！功能结束：CreateTable！！！

    // ！！！功能：SetOpacity——设置Opacity！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject item, float opacity 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetOpacity(GameObject item, float opacity)
    {
        // 步骤 1：逐个读取 Renderer renderer in item.GetComponentsInChildren<Renderer>() 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in item.GetComponentsInChildren<Renderer>())
        {
            // 步骤 2：计算 renderer.material，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
            Material material = renderer.material;
            // 步骤 3：检查条件 material.HasProperty("_Surface")；只有为真才运行下面的步骤，否则沿原流程继续。
            if (material.HasProperty("_Surface"))
            {
                // 步骤 4：用 transparent 记住此处真假结果（opacity < 1f），供下一条 if 选择执行路径。
                bool transparent = opacity < 1f;
                // 步骤 5：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                material.SetFloat("_Surface", transparent ? 1f : 0f);
                // 步骤 6：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                material.SetFloat("_SrcBlend", (float)(transparent ? UnityEngine.Rendering.BlendMode.SrcAlpha : UnityEngine.Rendering.BlendMode.One));
                // 步骤 7：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                material.SetFloat("_DstBlend", (float)(transparent ? UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha : UnityEngine.Rendering.BlendMode.Zero));
                // 步骤 8：设置材质中指定名称的浮点参数；这里按当前交互状态调节显示强度。
                material.SetFloat("_ZWrite", transparent ? 0f : 1f);
                // 步骤 9：检查条件 transparent；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 EnableKeyword：EnableKeyword。
                if (transparent) material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                // 步骤 10：前面的 if 条件为假时执行这里的备用处理。
                else material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                // 步骤 11：把计算结果 transparent ? 3000 : -1 保存为renderQueue；下一条判断或调用会读取这个具体值。
                material.renderQueue = transparent ? 3000 : -1;
            }
            // 步骤 12：计算 material.color，把结果存进 color（color）；之后引用 color 时用的就是这次结果。
            Color color = material.color;
            // 步骤 13：把颜色的 alpha 设为 1（完全不透明），避免透视背景穿过场景画面。
            color.a = opacity;
            // 步骤 14：把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
            material.color = color;
            // 步骤 15：从目标物体找到所需组件并存为 collider；使用前应检查是否为 null。
            Collider collider = renderer.GetComponent<Collider>();
            // 步骤 16：确认collider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 collider.enabled = opacity >= 1f;
            if (collider != null) collider.enabled = opacity >= 1f;
        }
    }
    // ！！！功能结束：SetOpacity！！！
}

// A marker on placed tea tables lets either mouse clicks or Quest rays open chat.
// 定义 SkyQuestTeaTable 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestTeaTable : MonoBehaviour
{
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public float readyAt { get; private set; }

    // ！！！功能：Awake——对象创建时建立初始数据和所需组件引用！！！
    // 何时执行：Unity 创建并启用对象时执行一次，适合寻找组件和记录初始状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Awake()
    {
        // The trigger that places a table must not also click it in that frame.
        // 步骤 1：把计算结果 Time.unscaledTime + .2f 保存为readyAt；下一条判断或调用会读取这个具体值。
        readyAt = Time.unscaledTime + .2f;
    }
    // ！！！功能结束：Awake！！！
}
