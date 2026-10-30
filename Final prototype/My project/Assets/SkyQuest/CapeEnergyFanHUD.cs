// ！！！文件功能：在屏幕上显示斗篷能量扇形图标并随数值刷新！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// A screen-space counterpart to the VR wrist meter. Both read the same energy source.
// 定义 CapeEnergyFanHUD 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class CapeEnergyFanHUD : MonoBehaviour
{
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    public CapeEnergySystem energy;
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // 声明 vrFan（vrFan）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform vrFan;
    // vrSegments 保存vrSegments（类型 Renderer[]）；下方方法依靠它记录或查找场景状态。
    private Renderer[] vrSegments;
    // vrCount 保存vrCount（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro vrCount;
    // jewelStyle 保存jewelStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle jewelStyle;
    // countStyle 保存countStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle countStyle;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize()
    {
        // 步骤 1：检查energy、头显相机、vrFan是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (desktopMode || energy == null || head == null || vrFan != null) return;
        // 步骤 2：新建用于vrFan的对象或数值结构，并把构造参数保存进去。
        vrFan = new GameObject("Cape energy fan HUD").transform;
        // 步骤 3：调用 SetParent：把当前对象挂到指定父对象下，形成场景层级。
        vrFan.SetParent(head, false);
        // 步骤 4：新建用于local位置的对象或数值结构，并把构造参数保存进去。
        vrFan.localPosition = new Vector3(0f, .30f, 1.05f);
        // 步骤 5：恢复 vrFan.localRotation 到初始本地朝向；本地表示相对于父对象。
        vrFan.localRotation = Quaternion.identity;
        // 步骤 6：新建用于vrSegments的对象或数值结构，并把构造参数保存进去。
        vrSegments = new Renderer[15];
        // 步骤 7：按 int i = 0; i < vrSegments.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < vrSegments.Length; i++)
        {
            // 步骤 8：计算 Mathf.Lerp(-68f, 68f, i / 14f)，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = Mathf.Lerp(-68f, 68f, i / 14f);
            // 步骤 9：计算 angle * Mathf.Deg2Rad，把结果存进 radians（radians）；之后引用 radians 时用的就是这次结果。
            float radians = angle * Mathf.Deg2Rad;
            // 步骤 10：把计算结果 SkyQuestVisuals.Primitive("Energy ray " + (i + 1), 保存为bar；下一条判断或调用会读取这个具体值。
            GameObject bar = SkyQuestVisuals.Primitive("Energy ray " + (i + 1),
                // 步骤 11：继续填写上一行调用的参数：PrimitiveType.Cube, vrFan；参数位置决定它表示的坐标、速度或选项。
                PrimitiveType.Cube, vrFan,
                // 步骤 12：用 new 创建 Vector3(Mathf.Sin(radians) * .092f, -Mathf.Cos(radians) * .092f, 0f),；构造参数决定新对象的初始内容。
                new Vector3(Mathf.Sin(radians) * .092f, -Mathf.Cos(radians) * .092f, 0f),
                // 步骤 13：用 new 创建 Vector3(.009f, .052f, .004f), new Color(1f, .84f, .34f), true);；构造参数决定新对象的初始内容。
                new Vector3(.009f, .052f, .004f), new Color(1f, .84f, .34f), true);
            // 步骤 14：执行 bar.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            bar.transform.localRotation = Quaternion.Euler(0f, 0f, -angle);
            // 步骤 15：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
            SkyQuestVisuals.RemoveCollider(bar);
            // 步骤 16：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            vrSegments[i] = bar.GetComponent<Renderer>();
        }
        // 步骤 17：把计算结果 SkyQuestVisuals.Primitive("Energy jewel", PrimitiveType.Cube, 保存为jewel；下一条判断或调用会读取这个具体值。
        GameObject jewel = SkyQuestVisuals.Primitive("Energy jewel", PrimitiveType.Cube,
            // 步骤 18：继续填写上一行调用的参数：vrFan, new Vector3(0f, .004f, 0f), new Vector3(.025f, .025f, .005f)；参数位置决定它表示的坐标、速度或选项。
            vrFan, new Vector3(0f, .004f, 0f), new Vector3(.025f, .025f, .005f),
            // 步骤 19：用 new 创建 Color(1f, .96f, .75f), true);；构造参数决定新对象的初始内容。
            new Color(1f, .96f, .75f), true);
        // 步骤 20：执行 jewel.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        jewel.transform.localRotation = Quaternion.Euler(0f, 0f, 45f);
        // 步骤 21：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(jewel);
        // 步骤 22：把计算结果 SkyQuestVisuals.Text("Energy remaining", "15 / 15", vrFan, 保存为vrCount；下一条判断或调用会读取这个具体值。
        vrCount = SkyQuestVisuals.Text("Energy remaining", "15 / 15", vrFan,
            // 步骤 23：用 new 创建 Vector3(0f, -.159f, -.006f), 1.8f, new Color(1f, .94f, .75f), .65f);；构造参数决定新对象的初始内容。
            new Vector3(0f, -.159f, -.006f), 1.8f, new Color(1f, .94f, .75f), .65f);
        // 步骤 24：在OnEnergyChanged原值上增加 RefreshVr；旧状态与本次输入共同决定新状态。
        energy.OnEnergyChanged += RefreshVr;
        // 步骤 25：调用 RefreshVr：刷新Vr。
        RefreshVr(energy.CurrentEnergy, energy.maxEnergy);
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：SetVisible——设置Visible！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool visible 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void SetVisible(bool visible)
    {
        // 步骤 1：检查vrFan是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (vrFan != null && vrFan.gameObject.activeSelf != visible)
            // 步骤 2：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            vrFan.gameObject.SetActive(visible);
    }
    // ！！！功能结束：SetVisible！！！

    // ！！！功能：RefreshVr——刷新Vr！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float current, float maximum 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshVr(float current, float maximum)
    {
        // 步骤 1：检查vrSegments是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (vrSegments == null) return;
        // 步骤 2：计算 lit 并限制在安全上下界，防止不合理的大小影响移动或显示。
        int lit = Mathf.Clamp(Mathf.CeilToInt(current / Mathf.Max(1f, maximum) * 15f - .001f), 0, 15);
        // 步骤 3：按 int i = 0; i < vrSegments.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < vrSegments.Length; i++)
            // 步骤 4：这一行补充 vrSegments[i].sharedMaterial = SkyQuestVisuals.Material(i < lit ?，与相邻行合起来完成同一次计算或对象配置。
            vrSegments[i].sharedMaterial = SkyQuestVisuals.Material(i < lit ?
                // 步骤 5：用 new 创建 Color(1f, .84f, .34f) : new Color(.22f, .27f, .30f), i < lit);；构造参数决定新对象的初始内容。
                new Color(1f, .84f, .34f) : new Color(.22f, .27f, .30f), i < lit);
        // 步骤 6：确认vrCount已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 vrCount.text = Mathf.CeilToInt(current) + " / " + Mathf.RoundToInt(maximum)
        if (vrCount != null) vrCount.text = Mathf.CeilToInt(current) + " / " + Mathf.RoundToInt(maximum);
    }
    // ！！！功能结束：RefreshVr！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认energy已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 energy.OnEnergyChanged -= RefreshVr;
        if (energy != null) energy.OnEnergyChanged -= RefreshVr;
        // 步骤 2：确认vrFan已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (vrFan != null) Destroy(vrFan.gameObject);
    }
    // ！！！功能结束：OnDestroy！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnGUI()
    {
        // 步骤 1：检查energy是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (energy == null || !desktopMode) return;
        // 步骤 2：检查jewelStyle是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (jewelStyle == null)
        {
            // 步骤 3：新建用于jewelStyle的对象或数值结构，并把构造参数保存进去。
            jewelStyle = new GUIStyle(GUI.skin.label)
            {
                // 步骤 4：把计算结果 TextAnchor.MiddleCenter, fontSize = 23, 保存为alignment；下一条判断或调用会读取这个具体值。
                alignment = TextAnchor.MiddleCenter, fontSize = 23,
                // 步骤 5：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
                fontStyle = FontStyle.Bold
            };
            // 步骤 6：执行 jewelStyle.normal.textColor = new Color(1f, .98f, .85f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            jewelStyle.normal.textColor = new Color(1f, .98f, .85f);
            // 步骤 7：新建用于countStyle的对象或数值结构，并把构造参数保存进去。
            countStyle = new GUIStyle(GUI.skin.label)
            {
                // 步骤 8：把计算结果 TextAnchor.MiddleCenter, fontSize = 13, 保存为alignment；下一条判断或调用会读取这个具体值。
                alignment = TextAnchor.MiddleCenter, fontSize = 13,
                // 步骤 9：把计算结果 FontStyle.Bold 保存为fontStyle；下一条判断或调用会读取这个具体值。
                fontStyle = FontStyle.Bold
            };
            // 步骤 10：执行 countStyle.normal.textColor = new Color(1f, .95f, .78f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            countStyle.normal.textColor = new Color(1f, .95f, .78f);
        }

        // 步骤 11：计算 filled 并限制在安全上下界，防止不合理的大小影响移动或显示。
        int filled = Mathf.Clamp(Mathf.CeilToInt(energy.CurrentEnergy - .001f), 0, 15);
        // 步骤 12：计算 Screen.width * .5f，把结果存进 cx（cx）；之后引用 cx 时用的就是这次结果。
        float cx = Screen.width * .5f;
        // 步骤 13：把数值 32f 暂存为 top；它只在当前方法里参与后续判断或计算。
        float top = 32f;
        // 步骤 14：创建新的 Vector2 并存为 pivot；构造参数决定其初始内容或对象名称。
        Vector2 pivot = new Vector2(cx, top + 12f);
        // 步骤 15：计算 GUI.matrix，把结果存进 oldMatrix（oldMatrix）；之后引用 oldMatrix 时用的就是这次结果。
        Matrix4x4 oldMatrix = GUI.matrix;
        // 步骤 16：计算 GUI.color，把结果存进 oldColor（oldColor）；之后引用 oldColor 时用的就是这次结果。
        Color oldColor = GUI.color;
        // 步骤 17：按 int i = 0; i < 15; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < 15; i++)
        {
            // 步骤 18：把计算结果 oldMatrix 保存为matrix；下一条判断或调用会读取这个具体值。
            GUI.matrix = oldMatrix;
            // 步骤 19：调用 RotateAroundPivot：RotateAroundPivot。
            GUIUtility.RotateAroundPivot(Mathf.Lerp(-78f, 78f, i / 14f), pivot);
            // 步骤 20：新建用于color的对象或数值结构，并把构造参数保存进去。
            GUI.color = i < filled ? new Color(1f, .88f, .32f, .98f) :
                // 步骤 21：用 new 创建 Color(.18f, .23f, .28f, .7f);；构造参数决定新对象的初始内容。
                new Color(.18f, .23f, .28f, .7f);
            // Start the bars away from the jewel so adjacent energy marks remain
            // visibly separate, including at the narrow inner edge of the fan.
            // 步骤 22：调用 DrawTexture：Draw贴图。
            GUI.DrawTexture(new Rect(cx - 3f, top + 55f, 6f, 34f), Texture2D.whiteTexture);
        }
        // 步骤 23：把计算结果 oldMatrix 保存为matrix；下一条判断或调用会读取这个具体值。
        GUI.matrix = oldMatrix;
        // 步骤 24：把计算结果 oldColor 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = oldColor;
        // 步骤 25：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(cx - 14f, top - 4f, 28f, 28f), "◆", jewelStyle);
        // 步骤 26：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(cx - 36f, top + 103f, 72f, 21f),
            // 步骤 27：调用 RoundToInt：Round到Int。
            Mathf.RoundToInt(energy.CurrentEnergy) + "/15", countStyle);
    }
    // ！！！功能结束：OnGUI！！！
}
