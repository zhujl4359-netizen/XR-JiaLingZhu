// ！！！文件功能：统一场景字体、材质和碰撞配置的辅助方法！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.TextCore.LowLevel 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.TextCore.LowLevel;

// 定义 SkyQuestVisuals 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestVisuals
{
    // 执行 private static readonly Dictionary<int, Material> SharedMaterials = new Dictionary<int, Material>();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private static readonly Dictionary<int, Material> SharedMaterials = new Dictionary<int, Material>();
    // sharedShader 保存sharedShader（类型 Shader）；下方方法依靠它记录或查找场景状态。
    private static Shader sharedShader;
    // chineseFont 保存chineseFont（类型 TMP_FontAsset）；下方方法依靠它记录或查找场景状态。
    private static TMP_FontAsset chineseFont;

    // ！！！功能：EnsureChineseFont——确保ChineseFont！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void EnsureChineseFont()
    {
        // 步骤 1：确认chineseFont已经有值；只有存在时才能安全读取它的属性或调用方法。 条件成立时立即退出当前方法。
        if (chineseFont != null) return;
        // 步骤 2：计算 Resources.Load<Font>("NotoSansCJKsc-Regular")，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
        Font source = Resources.Load<Font>("NotoSansCJKsc-Regular");
        // 步骤 3：检查反馈跟随的来源物品是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (source == null)
        {
            // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogWarning("SkyQuest Chinese font was not imported.");
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 6：把计算结果 TMP_FontAsset.CreateFontAsset(source, 64, 8, 保存为chineseFont；下一条判断或调用会读取这个具体值。
        chineseFont = TMP_FontAsset.CreateFontAsset(source, 64, 8,
            // 步骤 7：执行 GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            GlyphRenderMode.SDFAA, 1024, 1024, AtlasPopulationMode.Dynamic, true);
        // 步骤 8：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (chineseFont != null && TMP_Settings.fallbackFontAssets != null &&
            // 步骤 9：这一行补充 !TMP_Settings.fallbackFontAssets.Contains(chineseFont))，与相邻行合起来完成同一次计算或对象配置。
            !TMP_Settings.fallbackFontAssets.Contains(chineseFont))
            // 步骤 10：把括号里的新元素加入 TMP_Settings.fallbackFontAssets 集合；后续会按集合顺序使用它。
            TMP_Settings.fallbackFontAssets.Add(chineseFont);
    }
    // ！！！功能结束：EnsureChineseFont！！！

    // ！！！功能：Material——材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Color color, bool glow = false 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Material 类型的计算结果；执行到 return 时把该值交回调用者。
    public static Material Material(Color color, bool glow = false)
    {
        // 步骤 1：计算 color，把结果存进 compact（compact）；之后引用 compact 时用的就是这次结果。
        Color32 compact = color;
        // 步骤 2：计算 compact.r | (compact.g << 8) | (compact.b << 16) | (compact.a << 24)，把结果存进 key（key）；之后引用 key 时用的就是这次结果。
        int key = compact.r | (compact.g << 8) | (compact.b << 16) | (compact.a << 24);
        // 步骤 3：把计算结果 (key * 397) ^ (glow ? 1 : 0) 保存为key；下一条判断或调用会读取这个具体值。
        key = (key * 397) ^ (glow ? 1 : 0);
        // 步骤 4：检查cached是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (SharedMaterials.TryGetValue(key, out Material cached) && cached != null) return cached;

        // 步骤 5：检查sharedShader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 sharedShader = Shader.Find("Universal Render Pipeline/Lit");
        if (sharedShader == null) sharedShader = Shader.Find("Universal Render Pipeline/Lit");
        // 步骤 6：检查sharedShader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 sharedShader = Shader.Find("Standard");
        if (sharedShader == null) sharedShader = Shader.Find("Standard");
        // 步骤 7：创建新的 Material 并存为 material；构造参数决定其初始内容或对象名称。
        Material material = new Material(sharedShader);
        // 步骤 8：把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
        material.color = color;
        // 步骤 9：检查条件 glow；只有为真才运行下面的步骤，否则沿原流程继续。
        if (glow)
        {
            // 步骤 10：调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_EMISSION");
            // 步骤 11：设置这个渲染器的发光颜色；三元表达式根据悬停状态选择亮色或黑色。
            material.SetColor("_EmissionColor", color * 1.5f);
        }
        // 步骤 12：执行 SharedMaterials[key] = material;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        SharedMaterials[key] = material;
        // 步骤 13：把 material 作为方法结果返回给调用方；return 后本方法立即结束。
        return material;
    }
    // ！！！功能结束：Material！！！

    // 这些是同一个枚举中的选项，每个名称代表一种可由按钮触发的动作。
    public static GameObject Primitive(string name, PrimitiveType type, Transform parent,
        // 这一行补充 Vector3 localPosition, Vector3 localScale, Color color, bool glow = false)，与相邻行合起来完成同一次计算或对象配置。
        Vector3 localPosition, Vector3 localScale, Color color, bool glow = false)
    {
        // 计算 GameObject.CreatePrimitive(type)，把结果存进 obj（obj）；之后引用 obj 时用的就是这次结果。
        GameObject obj = GameObject.CreatePrimitive(type);
        // 把计算结果 name 保存为name；下一条判断或调用会读取这个具体值。
        obj.name = name;
        // 执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 执行 obj.transform.localPosition = localPosition;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localPosition = localPosition;
        // 执行 obj.transform.localScale = localScale;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localScale = localScale;
        // 从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<Renderer>().sharedMaterial = Material(color, glow);
        // 把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }

    // 这些是同一个枚举中的选项，每个名称代表一种可由按钮触发的动作。
    public static TextMeshPro Text(string name, string value, Transform parent, Vector3 localPosition,
        // 这一行补充 float fontSize, Color color, float width = 3f)，与相邻行合起来完成同一次计算或对象配置。
        float fontSize, Color color, float width = 3f)
    {
        // 创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name);
        // 执行 obj.transform.SetParent(parent, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(parent, false);
        // 执行 obj.transform.localPosition = localPosition;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localPosition = localPosition;
        // World-space labels use a compact scale so guidance stays readable in VR
        // and does not cover the entire view.
        // 执行 obj.transform.localScale = Vector3.one * 0.12f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.localScale = Vector3.one * 0.12f;
        // 计算 obj.AddComponent<TextMeshPro>()，把结果存进 text（text）；之后引用 text 时用的就是这次结果。
        TextMeshPro text = obj.AddComponent<TextMeshPro>();
        // 把新内容写入 text.text；玩家看到的文字会在本帧更新。
        text.text = value;
        // 把计算结果 fontSize 保存为fontSize；下一条判断或调用会读取这个具体值。
        text.fontSize = fontSize;
        // 把计算结果 color 保存为color；下一条判断或调用会读取这个具体值。
        text.color = color;
        // 把计算结果 TextAlignmentOptions.Center 保存为alignment；下一条判断或调用会读取这个具体值。
        text.alignment = TextAlignmentOptions.Center;
        // 执行 text.rectTransform.sizeDelta = new Vector2(width, 1.3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        text.rectTransform.sizeDelta = new Vector2(width, 1.3f);
        // 把 text 作为方法结果返回给调用方；return 后本方法立即结束。
        return text;
    }

    // ！！！功能：RemoveCollider——移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 GameObject obj 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public static void RemoveCollider(GameObject obj)
    {
        // 步骤 1：从目标物体找到所需组件并存为 collider；使用前应检查是否为 null。
        Collider collider = obj.GetComponent<Collider>();
        // 步骤 2：检查collider是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (collider == null) return;
        // Destroy is deferred until frame end; disable now so newly created
        // decoration cannot lift the player or interrupt the camera cast.
        // 步骤 3：把 collider.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        collider.enabled = false;
        // 步骤 4：调用 Destroy：Destroy。
        Object.Destroy(collider);
    }
    // ！！！功能结束：RemoveCollider！！！
}
