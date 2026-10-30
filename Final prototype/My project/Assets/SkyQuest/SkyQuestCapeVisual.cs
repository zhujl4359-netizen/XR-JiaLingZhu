// ！！！文件功能：角色斗篷网格、能量星星和起飞视觉特效！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.Rendering 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering;

// A lightweight cloth and energy display for the existing COTL avatar.
// Everything is visual: no collider, camera motion, or flight physics is added here.
// 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
[DefaultExecutionOrder(120)]
// 定义 SkyQuestCapeVisual 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestCapeVisual : MonoBehaviour
{
    // 定义不可在运行时改变的数值 PanelCount（面板Count）；后续计算统一使用这个阈值、速度或尺寸。
    private const int PanelCount = 12;
    // 定义不可在运行时改变的数值 PanelWidthSegments（面板WidthSegments）；后续计算统一使用这个阈值、速度或尺寸。
    private const int PanelWidthSegments = 4;
    // 定义不可在运行时改变的数值 LengthSegments（LengthSegments）；后续计算统一使用这个阈值、速度或尺寸。
    private const int LengthSegments = 12;
    // 定义不可在运行时改变的数值 GemCount（GemCount）；后续计算统一使用这个阈值、速度或尺寸。
    private const int GemCount = 5;
    // 定义不可在运行时改变的数值 StarCount（StarCount）；后续计算统一使用这个阈值、速度或尺寸。
    private const int StarCount = 15;
    // 定义不可在运行时改变的数值 PanelVertexCount（面板VertexCount）；后续计算统一使用这个阈值、速度或尺寸。
    private const int PanelVertexCount = (PanelWidthSegments + 1) * (LengthSegments + 1);
    // 创建新的 Color 并存为 Rose；构造参数决定其初始内容或对象名称。
    private static readonly Color Rose = new Color(.62f, .24f, .42f);
    // 创建新的 Color 并存为 Pink；构造参数决定其初始内容或对象名称。
    private static readonly Color Pink = new Color(.92f, .49f, .67f);
    // 创建新的 Color 并存为 Pearl；构造参数决定其初始内容或对象名称。
    private static readonly Color Pearl = new Color(1f, .87f, .86f);
    // 创建新的 Color 并存为 GemOutline；构造参数决定其初始内容或对象名称。
    private static readonly Color GemOutline = new Color(.31f, .16f, .28f);
    // 创建新的 Color 并存为 DimStar；构造参数决定其初始内容或对象名称。
    private static readonly Color DimStar = new Color(.29f, .23f, .32f);

    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    private SkyQuestLocomotion locomotion;
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // flight 保存flight（类型 CapeFlightController）；下方方法依靠它记录或查找场景状态。
    private CapeFlightController flight;
    // capeMesh 保存cape网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh capeMesh;
    // liningMesh 保存lining网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh liningMesh;
    // hemMesh 保存hem网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh hemMesh;
    // gemOutlineMesh 保存gemOutline网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh gemOutlineMesh;
    // starMesh 保存star网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh starMesh;
    // haloMesh 保存halo网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh haloMesh;
    // sparkMesh 保存spark网格（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh sparkMesh;
    // capeVertices 保存capeVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] capeVertices;
    // liningVertices 保存liningVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] liningVertices;
    // hemVertices 保存hemVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] hemVertices;
    // fabricColors 保存fabricColors（类型 Color[]）；下方方法依靠它记录或查找场景状态。
    private Color[] fabricColors;
    // liningColors 保存liningColors（类型 Color[]）；下方方法依靠它记录或查找场景状态。
    private Color[] liningColors;
    // hemColors 保存hemColors（类型 Color[]）；下方方法依靠它记录或查找场景状态。
    private Color[] hemColors;
    // gemOutlineVertices 保存gemOutlineVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] gemOutlineVertices;
    // starVertices 保存starVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] starVertices;
    // starColors 保存starColors（类型 Color[]）；下方方法依靠它记录或查找场景状态。
    private Color[] starColors;
    // haloVertices 保存haloVertices（类型 Vector3[]）；下方方法依靠它记录或查找场景状态。
    private Vector3[] haloVertices;
    // haloColors 保存haloColors（类型 Color[]）；下方方法依靠它记录或查找场景状态。
    private Color[] haloColors;
    // leftRibbon 保存leftRibbon（类型 TrailRenderer）；下方方法依靠它记录或查找场景状态。
    private TrailRenderer leftRibbon;
    // rightRibbon 保存rightRibbon（类型 TrailRenderer）；下方方法依靠它记录或查找场景状态。
    private TrailRenderer rightRibbon;
    // boostSparks 保存boostSparks（类型 ParticleSystem）；下方方法依靠它记录或查找场景状态。
    private ParticleSystem boostSparks;
    // flightSparks 保存flightSparks（类型 ParticleSystem）；下方方法依靠它记录或查找场景状态。
    private ParticleSystem flightSparks;
    // 声明 takeoffRing（takeoffRing）：可见射线组件引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private LineRenderer takeoffRing;
    // 声明 spread（spread）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float spread;
    // 声明 boostPulse（boostPulse）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float boostPulse;
    // 把数值 10f 暂存为 ringAge；它只在当前方法里参与后续判断或计算。
    private float ringAge = 10f;
    // 声明 initialized（initialized）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool initialized;

    // 这些是同一个枚举中的选项，每个名称代表一种可由按钮触发的动作。
    public void Initialize(SkyQuestLocomotion owner, CapeEnergySystem source,
        // 这一行补充 CapeFlightController controller)，与相邻行合起来完成同一次计算或对象配置。
        CapeFlightController controller)
    {
        // 确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flight.OnBoostSucceeded -= PlayBoost;
        if (flight != null) flight.OnBoostSucceeded -= PlayBoost;
        // 把计算结果 owner 保存为locomotion；下一条判断或调用会读取这个具体值。
        locomotion = owner;
        // 把计算结果 source 保存为energy；下一条判断或调用会读取这个具体值。
        energy = source;
        // 把计算结果 controller 保存为flight；下一条判断或调用会读取这个具体值。
        flight = controller;
        // 确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flight.OnBoostSucceeded += PlayBoost;
        if (flight != null) flight.OnBoostSucceeded += PlayBoost;
        // 这里的 ! 表示取反；当 initialized 不成立时才进入此分支。 成立时调用 BuildVisuals：创建斗篷布面、星星与起飞特效对象。
        if (!initialized) BuildVisuals();
        // 调用 RefreshEnergy：根据现有能量点亮或熄灭斗篷上的星星。
        RefreshEnergy();
    }

    // ！！！功能：PlayBoost——PlayBoost！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayBoost()
    {
        // 步骤 1：这里的 ! 表示取反；当 initialized 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!initialized) return;
        // 步骤 2：把计算结果 1f 保存为boostPulse；下一条判断或调用会读取这个具体值。
        boostPulse = 1f;
        // 步骤 3：把计算结果 0f 保存为ringAge；下一条判断或调用会读取这个具体值。
        ringAge = 0f;
        // 步骤 4：执行 leftRibbon?.Clear();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        leftRibbon?.Clear();
        // 步骤 5：执行 rightRibbon?.Clear();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rightRibbon?.Clear();
        // 步骤 6：确认boostSparks已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (boostSparks != null)
        {
            // 步骤 7：调用 Stop：Stop。
            boostSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // 步骤 8：调用 Play：Play。
            boostSparks.Play(true);
        }
    }
    // ！！！功能结束：PlayBoost！！！

    // ！！！功能：BuildVisuals——创建斗篷布面、星星与起飞特效对象！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BuildVisuals()
    {
        // 步骤 1：把 initialized 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        initialized = true;
        // 步骤 2：计算 Resources.Load<Material>("SkyCapeFabric")，把结果存进 cloth（cloth）；之后引用 cloth 时用的就是这次结果。
        Material cloth = Resources.Load<Material>("SkyCapeFabric");
        // 步骤 3：计算 Resources.Load<Material>("SkyCapeGlow")，把结果存进 glow（glow）；之后引用 glow 时用的就是这次结果。
        Material glow = Resources.Load<Material>("SkyCapeGlow");
        // 步骤 4：检查cloth是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 cloth = SkyQuestVisuals.Material(Pink);
        if (cloth == null) cloth = SkyQuestVisuals.Material(Pink);
        // 步骤 5：检查glow是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 glow = SkyQuestVisuals.Material(Pearl, true);
        if (glow == null) glow = SkyQuestVisuals.Material(Pearl, true);

        // Each pleat has its own vertices and normals. The hems overlap visually
        // like the radial, scalloped panels in the supplied cape reference.
        // 步骤 6：计算 NewVisual("Rose petal cape", cloth, out capeMesh)，把结果存进 fabric（fabric）；之后引用 fabric 时用的就是这次结果。
        GameObject fabric = NewVisual("Rose petal cape", cloth, out capeMesh);
        // 步骤 7：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        fabric.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        // 步骤 8：调用 NewVisual：NewVisual。
        NewVisual("Ivory cape lining", cloth, out liningMesh);
        // 步骤 9：调用 NewVisual：NewVisual。
        NewVisual("Soft scalloped hem thickness", cloth, out hemMesh);
        // 步骤 10：计算 PanelCount * PanelVertexCount，把结果存进 vertexCount（vertexCount）；之后引用 vertexCount 时用的就是这次结果。
        int vertexCount = PanelCount * PanelVertexCount;
        // 步骤 11：新建用于capeVertices的对象或数值结构，并把构造参数保存进去。
        capeVertices = new Vector3[vertexCount];
        // 步骤 12：新建用于liningVertices的对象或数值结构，并把构造参数保存进去。
        liningVertices = new Vector3[vertexCount];
        // 步骤 13：新建用于fabricColors的对象或数值结构，并把构造参数保存进去。
        fabricColors = new Color[vertexCount];
        // 步骤 14：新建用于liningColors的对象或数值结构，并把构造参数保存进去。
        liningColors = new Color[vertexCount];
        // 步骤 15：创建新的 int[] 并存为 faceTriangles；构造参数决定其初始内容或对象名称。
        int[] faceTriangles = new int[PanelCount * LengthSegments * PanelWidthSegments * 6];
        // 步骤 16：创建新的 int[] 并存为 liningTriangles；构造参数决定其初始内容或对象名称。
        int[] liningTriangles = new int[faceTriangles.Length];
        // 步骤 17：按 int panel = 0, n = 0; panel < PanelCount; panel++ 变化索引；循环体依次处理每个位置。
        for (int panel = 0, n = 0; panel < PanelCount; panel++)
            // 步骤 18：按 int row = 0; row < LengthSegments; row++ 变化索引；循环体依次处理每个位置。
            for (int row = 0; row < LengthSegments; row++)
                // 步骤 19：按 int column = 0; column < PanelWidthSegments; column++ 变化索引；循环体依次处理每个位置。
                for (int column = 0; column < PanelWidthSegments; column++)
                {
                    // 步骤 20：计算 panel * PanelVertexCount + row * (PanelWidthSegments + 1) + column，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
                    int a = panel * PanelVertexCount + row * (PanelWidthSegments + 1) + column;
                    // 步骤 21：计算 a + 1，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
                    int b = a + 1;
                    // 步骤 22：计算 a + PanelWidthSegments + 1，把结果存进 c（c）；之后引用 c 时用的就是这次结果。
                    int c = a + PanelWidthSegments + 1;
                    // 步骤 23：计算 c + 1，把结果存进 d（d）；之后引用 d 时用的就是这次结果。
                    int d = c + 1;
                    // 步骤 24：执行 faceTriangles[n] = a; faceTriangles[n + 1] = c; faceTriangles[n + 2] = b;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    faceTriangles[n] = a; faceTriangles[n + 1] = c; faceTriangles[n + 2] = b;
                    // 步骤 25：执行 faceTriangles[n + 3] = b; faceTriangles[n + 4] = c; faceTriangles[n + 5] = d;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    faceTriangles[n + 3] = b; faceTriangles[n + 4] = c; faceTriangles[n + 5] = d;
                    // 步骤 26：执行 liningTriangles[n] = a; liningTriangles[n + 1] = b; liningTriangles[n + 2] = c;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    liningTriangles[n] = a; liningTriangles[n + 1] = b; liningTriangles[n + 2] = c;
                    // 步骤 27：执行 liningTriangles[n + 3] = b; liningTriangles[n + 4] = d; liningTriangles[n + 5] = c;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    liningTriangles[n + 3] = b; liningTriangles[n + 4] = d; liningTriangles[n + 5] = c;
                    // 步骤 28：在n原值上增加 6；旧状态与本次输入共同决定新状态。
                    n += 6;
                }
        // 步骤 29：把计算结果 capeVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        capeMesh.vertices = capeVertices;
        // 步骤 30：把计算结果 faceTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        capeMesh.triangles = faceTriangles;
        // 步骤 31：调用 MarkDynamic：MarkDynamic。
        capeMesh.MarkDynamic();
        // 步骤 32：把计算结果 liningVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        liningMesh.vertices = liningVertices;
        // 步骤 33：把计算结果 liningTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        liningMesh.triangles = liningTriangles;
        // 步骤 34：调用 MarkDynamic：MarkDynamic。
        liningMesh.MarkDynamic();

        // 步骤 35：计算 PanelCount * (PanelWidthSegments + 1)，把结果存进 hemColumns（hemColumns）；之后引用 hemColumns 时用的就是这次结果。
        int hemColumns = PanelCount * (PanelWidthSegments + 1);
        // 步骤 36：新建用于hemVertices的对象或数值结构，并把构造参数保存进去。
        hemVertices = new Vector3[hemColumns * 2];
        // 步骤 37：新建用于hemColors的对象或数值结构，并把构造参数保存进去。
        hemColors = new Color[hemVertices.Length];
        // 步骤 38：创建新的 int[] 并存为 hemTriangles；构造参数决定其初始内容或对象名称。
        int[] hemTriangles = new int[PanelCount * PanelWidthSegments * 6];
        // 步骤 39：按 int panel = 0, n = 0; panel < PanelCount; panel++ 变化索引；循环体依次处理每个位置。
        for (int panel = 0, n = 0; panel < PanelCount; panel++)
            // 步骤 40：按 int column = 0; column < PanelWidthSegments; column++ 变化索引；循环体依次处理每个位置。
            for (int column = 0; column < PanelWidthSegments; column++)
            {
                // 步骤 41：计算 (panel * (PanelWidthSegments + 1) + column) * 2，把结果存进 a（a）；之后引用 a 时用的就是这次结果。
                int a = (panel * (PanelWidthSegments + 1) + column) * 2;
                // 步骤 42：计算 a + 2，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
                int b = a + 2;
                // 步骤 43：执行 hemTriangles[n++] = a; hemTriangles[n++] = a + 1; hemTriangles[n++] = b;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemTriangles[n++] = a; hemTriangles[n++] = a + 1; hemTriangles[n++] = b;
                // 步骤 44：执行 hemTriangles[n++] = b; hemTriangles[n++] = a + 1; hemTriangles[n++] = b + 1;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemTriangles[n++] = b; hemTriangles[n++] = a + 1; hemTriangles[n++] = b + 1;
            }
        // 步骤 45：把计算结果 hemVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        hemMesh.vertices = hemVertices;
        // 步骤 46：把计算结果 hemTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        hemMesh.triangles = hemTriangles;
        // 步骤 47：调用 MarkDynamic：MarkDynamic。
        hemMesh.MarkDynamic();

        // 步骤 48：调用 NewVisual：NewVisual。
        NewVisual("Five dark-rimmed cape diamonds", cloth, out gemOutlineMesh);
        // 步骤 49：新建用于gemOutlineVertices的对象或数值结构，并把构造参数保存进去。
        gemOutlineVertices = new Vector3[GemCount * 4];
        // 步骤 50：创建新的 int[] 并存为 outlineTriangles；构造参数决定其初始内容或对象名称。
        int[] outlineTriangles = new int[GemCount * 6];
        // 步骤 51：按 int i = 0; i < GemCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < GemCount; i++)
        {
            // 步骤 52：计算 i * 4, t = i * 6，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
            int v = i * 4, t = i * 6;
            // 步骤 53：执行 outlineTriangles[t] = v; outlineTriangles[t + 1] = v + 1; outlineTriangles[t + 2] = v + 2;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            outlineTriangles[t] = v; outlineTriangles[t + 1] = v + 1; outlineTriangles[t + 2] = v + 2;
            // 步骤 54：执行 outlineTriangles[t + 3] = v; outlineTriangles[t + 4] = v + 2; outlineTriangles[t + 5] = v + 3;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            outlineTriangles[t + 3] = v; outlineTriangles[t + 4] = v + 2; outlineTriangles[t + 5] = v + 3;
        }
        // 步骤 55：把计算结果 gemOutlineVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        gemOutlineMesh.vertices = gemOutlineVertices;
        // 步骤 56：把计算结果 outlineTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        gemOutlineMesh.triangles = outlineTriangles;
        // 步骤 57：把计算结果 FillColors(gemOutlineVertices.Length, GemOutline) 保存为colors；下一条判断或调用会读取这个具体值。
        gemOutlineMesh.colors = FillColors(gemOutlineVertices.Length, GemOutline);
        // 步骤 58：调用 MarkDynamic：MarkDynamic。
        gemOutlineMesh.MarkDynamic();

        // Three independently dimming facets per diamond preserve all 15
        // energy steps while keeping the reference's five-gem silhouette.
        // 步骤 59：调用 NewVisual：NewVisual。
        NewVisual("Fifteen cape energy facets", cloth, out starMesh);
        // 步骤 60：新建用于starVertices的对象或数值结构，并把构造参数保存进去。
        starVertices = new Vector3[StarCount * 4];
        // 步骤 61：新建用于starColors的对象或数值结构，并把构造参数保存进去。
        starColors = new Color[starVertices.Length];
        // 步骤 62：创建新的 int[] 并存为 starTriangles；构造参数决定其初始内容或对象名称。
        int[] starTriangles = new int[StarCount * 6];
        // 步骤 63：按 int i = 0; i < StarCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < StarCount; i++)
        {
            // 步骤 64：计算 i * 4, t = i * 6，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
            int v = i * 4, t = i * 6;
            // 步骤 65：执行 starTriangles[t] = v; starTriangles[t + 1] = v + 2; starTriangles[t + 2] = v + 1;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            starTriangles[t] = v; starTriangles[t + 1] = v + 2; starTriangles[t + 2] = v + 1;
            // 步骤 66：执行 starTriangles[t + 3] = v; starTriangles[t + 4] = v + 3; starTriangles[t + 5] = v + 2;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            starTriangles[t + 3] = v; starTriangles[t + 4] = v + 3; starTriangles[t + 5] = v + 2;
        }
        // 步骤 67：把计算结果 starVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        starMesh.vertices = starVertices;
        // 步骤 68：把计算结果 starTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        starMesh.triangles = starTriangles;
        // 步骤 69：调用 MarkDynamic：MarkDynamic。
        starMesh.MarkDynamic();

        // 步骤 70：调用 NewVisual：NewVisual。
        NewVisual("Subtle diamond glints", glow, out haloMesh);
        // 步骤 71：新建用于haloVertices的对象或数值结构，并把构造参数保存进去。
        haloVertices = new Vector3[GemCount * 9];
        // 步骤 72：新建用于haloColors的对象或数值结构，并把构造参数保存进去。
        haloColors = new Color[haloVertices.Length];
        // 步骤 73：创建新的 int[] 并存为 haloTriangles；构造参数决定其初始内容或对象名称。
        int[] haloTriangles = new int[GemCount * 12 * 3];
        // 步骤 74：按 int i = 0, n = 0; i < GemCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0, n = 0; i < GemCount; i++)
        {
            // 步骤 75：计算 i * 9，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
            int v = i * 9;
            // 步骤 76：按 int side = 0; side < 4; side++ 变化索引；循环体依次处理每个位置。
            for (int side = 0; side < 4; side++)
            {
                // 步骤 77：计算 (side + 1) % 4，把结果存进 next（next）；之后引用 next 时用的就是这次结果。
                int next = (side + 1) % 4;
                // 步骤 78：执行 haloTriangles[n++] = v; haloTriangles[n++] = v + 1 + side;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v; haloTriangles[n++] = v + 1 + side;
                // 步骤 79：执行 haloTriangles[n++] = v + 1 + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v + 1 + next;
                // 步骤 80：执行 haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + side;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + side;
                // 步骤 81：执行 haloTriangles[n++] = v + 5 + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v + 5 + next;
                // 步骤 82：执行 haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + next;
                // 步骤 83：执行 haloTriangles[n++] = v + 1 + next;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloTriangles[n++] = v + 1 + next;
            }
        }
        // 步骤 84：把计算结果 haloVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        haloMesh.vertices = haloVertices;
        // 步骤 85：把计算结果 haloTriangles 保存为triangles；下一条判断或调用会读取这个具体值。
        haloMesh.triangles = haloTriangles;
        // 步骤 86：调用 MarkDynamic：MarkDynamic。
        haloMesh.MarkDynamic();

        // 步骤 87：新建用于spark网格的对象或数值结构，并把构造参数保存进去。
        sparkMesh = new Mesh { name = "Diamond takeoff glint" };
        // 步骤 88：把计算结果 new[] { 保存为vertices；下一条判断或调用会读取这个具体值。
        sparkMesh.vertices = new[] {
            // 步骤 89：用 new 创建 Vector3(0f, .5f, 0f), new Vector3(.18f, 0f, 0f),；构造参数决定新对象的初始内容。
            new Vector3(0f, .5f, 0f), new Vector3(.18f, 0f, 0f),
            // 步骤 90：用 new 创建 Vector3(0f, -.5f, 0f), new Vector3(-.18f, 0f, 0f) };；构造参数决定新对象的初始内容。
            new Vector3(0f, -.5f, 0f), new Vector3(-.18f, 0f, 0f) };
        // 步骤 91：把计算结果 new[] { 0, 1, 2, 0, 2, 3 } 保存为triangles；下一条判断或调用会读取这个具体值。
        sparkMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        // 步骤 92：调用 RecalculateBounds：RecalculateBounds。
        sparkMesh.RecalculateBounds();
        // 步骤 93：把计算结果 CreateRibbon("Left silver flight ribbon", glow) 保存为leftRibbon；下一条判断或调用会读取这个具体值。
        leftRibbon = CreateRibbon("Left silver flight ribbon", glow);
        // 步骤 94：把计算结果 CreateRibbon("Right silver flight ribbon", glow) 保存为rightRibbon；下一条判断或调用会读取这个具体值。
        rightRibbon = CreateRibbon("Right silver flight ribbon", glow);
        // 步骤 95：把计算结果 CreateParticles("Golden takeoff sparks", glow, false) 保存为boostSparks；下一条判断或调用会读取这个具体值。
        boostSparks = CreateParticles("Golden takeoff sparks", glow, false);
        // 步骤 96：把计算结果 CreateParticles("Cape flight glimmer", glow, true) 保存为flightSparks；下一条判断或调用会读取这个具体值。
        flightSparks = CreateParticles("Cape flight glimmer", glow, true);
        // 步骤 97：把计算结果 CreateRing(glow) 保存为takeoffRing；下一条判断或调用会读取这个具体值。
        takeoffRing = CreateRing(glow);
        // 步骤 98：调用 UpdateGeometry：更新Geometry。
        UpdateGeometry(0f);
    }
    // ！！！功能结束：BuildVisuals！！！

    // ！！！功能：NewVisual——NewVisual！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Material material, out Mesh mesh 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject NewVisual(string name, Material material, out Mesh mesh)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        // 步骤 2：执行 obj.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(transform, false);
        // 步骤 3：新建用于mesh的对象或数值结构，并把构造参数保存进去。
        mesh = new Mesh { name = name };
        // 步骤 4：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        // 步骤 5：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        // 步骤 6：把计算结果 material 保存为shared材质；下一条判断或调用会读取这个具体值。
        renderer.sharedMaterial = material;
        // 步骤 7：把计算结果 ShadowCastingMode.Off 保存为shadowCasting模式；下一条判断或调用会读取这个具体值。
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        // 步骤 8：把 renderer.receiveShadows 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        renderer.receiveShadows = false;
        // 步骤 9：把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }
    // ！！！功能结束：NewVisual！！！

    // ！！！功能：CreateRibbon——创建Ribbon！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private TrailRenderer CreateRibbon(string name, Material material)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(TrailRenderer));
        // 步骤 2：执行 obj.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(transform, false);
        // 步骤 3：从目标物体找到所需组件并存为 ribbon；使用前应检查是否为 null。
        TrailRenderer ribbon = obj.GetComponent<TrailRenderer>();
        // 步骤 4：把计算结果 material 保存为shared材质；下一条判断或调用会读取这个具体值。
        ribbon.sharedMaterial = material;
        // 步骤 5：把计算结果 .7f 保存为time；下一条判断或调用会读取这个具体值。
        ribbon.time = .7f;
        // 步骤 6：把计算结果 .065f 保存为minVertexDistance；下一条判断或调用会读取这个具体值。
        ribbon.minVertexDistance = .065f;
        // 步骤 7：把计算结果 .055f 保存为widthMultiplier；下一条判断或调用会读取这个具体值。
        ribbon.widthMultiplier = .055f;
        // 步骤 8：把计算结果 0 保存为numCapVertices；下一条判断或调用会读取这个具体值。
        ribbon.numCapVertices = 0;
        // 步骤 9：把计算结果 0 保存为numCornerVertices；下一条判断或调用会读取这个具体值。
        ribbon.numCornerVertices = 0;
        // 步骤 10：把计算结果 ShadowCastingMode.Off 保存为shadowCasting模式；下一条判断或调用会读取这个具体值。
        ribbon.shadowCastingMode = ShadowCastingMode.Off;
        // 步骤 11：把 ribbon.emitting 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        ribbon.emitting = false;
        // 步骤 12：新建用于colorGradient的对象或数值结构，并把构造参数保存进去。
        ribbon.colorGradient = new Gradient
        {
            // 步骤 13：把计算结果 new[] { 保存为colorKeys；下一条判断或调用会读取这个具体值。
            colorKeys = new[] {
                // 步骤 14：用 new 创建 GradientColorKey(new Color(1f, .97f, .83f), 0f),；构造参数决定新对象的初始内容。
                new GradientColorKey(new Color(1f, .97f, .83f), 0f),
                // 步骤 15：用 new 创建 GradientColorKey(new Color(.59f, .89f, 1f), 1f) },；构造参数决定新对象的初始内容。
                new GradientColorKey(new Color(.59f, .89f, 1f), 1f) },
            // 步骤 16：把计算结果 new[] { 保存为alphaKeys；下一条判断或调用会读取这个具体值。
            alphaKeys = new[] {
                // 步骤 17：用 new 创建 GradientAlphaKey(.86f, 0f), new GradientAlphaKey(0f, 1f) }；构造参数决定新对象的初始内容。
                new GradientAlphaKey(.86f, 0f), new GradientAlphaKey(0f, 1f) }
        };
        // 步骤 18：把 ribbon 作为方法结果返回给调用方；return 后本方法立即结束。
        return ribbon;
    }
    // ！！！功能结束：CreateRibbon！！！

    // ！！！功能：CreateParticles——创建Particles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Material material, bool continuous 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private ParticleSystem CreateParticles(string name, Material material, bool continuous)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject(name, typeof(ParticleSystem));
        // 步骤 2：执行 obj.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(transform, false);
        // 步骤 3：这一行补充 obj.transform.localPosition = continuous ? new Vector3(0f, .78f, .45f) :，与相邻行合起来完成同一次计算或对象配置。
        obj.transform.localPosition = continuous ? new Vector3(0f, .78f, .45f) :
            // 步骤 4：用 new 创建 Vector3(0f, .77f, .42f);；构造参数决定新对象的初始内容。
            new Vector3(0f, .77f, .42f);
        // 步骤 5：从目标物体找到所需组件并存为 particles；使用前应检查是否为 null。
        ParticleSystem particles = obj.GetComponent<ParticleSystem>();
        // 步骤 6：调用 Stop：Stop。
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 步骤 7：计算 particles.main，把结果存进 main（main）；之后引用 main 时用的就是这次结果。
        var main = particles.main;
        // 步骤 8：把 main.playOnAwake 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        main.playOnAwake = false;
        // 步骤 9：把计算结果 continuous 保存为loop；下一条判断或调用会读取这个具体值。
        main.loop = continuous;
        // 步骤 10：把计算结果 1f 保存为duration；下一条判断或调用会读取这个具体值。
        main.duration = 1f;
        // 步骤 11：把计算结果 ParticleSystemSimulationSpace.World 保存为simulationSpace；下一条判断或调用会读取这个具体值。
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // 步骤 12：把计算结果 continuous ? 32 : 64 保存为maxParticles；下一条判断或调用会读取这个具体值。
        main.maxParticles = continuous ? 32 : 64;
        // 步骤 13：新建用于startLifetime的对象或数值结构，并把构造参数保存进去。
        main.startLifetime = continuous ? new ParticleSystem.MinMaxCurve(.25f, .5f) :
            // 步骤 14：用 new 创建 ParticleSystem.MinMaxCurve(.25f, .65f);；构造参数决定新对象的初始内容。
            new ParticleSystem.MinMaxCurve(.25f, .65f);
        // 步骤 15：新建用于startSpeed的对象或数值结构，并把构造参数保存进去。
        main.startSpeed = continuous ? new ParticleSystem.MinMaxCurve(.12f, .45f) :
            // 步骤 16：用 new 创建 ParticleSystem.MinMaxCurve(.7f, 2f);；构造参数决定新对象的初始内容。
            new ParticleSystem.MinMaxCurve(.7f, 2f);
        // 步骤 17：新建用于startSize的对象或数值结构，并把构造参数保存进去。
        main.startSize = continuous ? new ParticleSystem.MinMaxCurve(.025f, .055f) :
            // 步骤 18：用 new 创建 ParticleSystem.MinMaxCurve(.035f, .095f);；构造参数决定新对象的初始内容。
            new ParticleSystem.MinMaxCurve(.035f, .095f);
        // 步骤 19：新建用于startColor的对象或数值结构，并把构造参数保存进去。
        main.startColor = continuous ? new ParticleSystem.MinMaxGradient(
            // 步骤 20：用 new 创建 Color(.74f, .9f, 1f, .7f), new Color(1f, .94f, .7f, .9f)) :；构造参数决定新对象的初始内容。
            new Color(.74f, .9f, 1f, .7f), new Color(1f, .94f, .7f, .9f)) :
            // 步骤 21：用 new 创建 ParticleSystem.MinMaxGradient(；构造参数决定新对象的初始内容。
            new ParticleSystem.MinMaxGradient(
                // 步骤 22：用 new 创建 Color(1f, .85f, .47f, 1f), new Color(1f, 1f, .9f, 1f));；构造参数决定新对象的初始内容。
                new Color(1f, .85f, .47f, 1f), new Color(1f, 1f, .9f, 1f));
        // 步骤 23：计算 particles.emission，把结果存进 emission（emission）；之后引用 emission 时用的就是这次结果。
        var emission = particles.emission;
        // 步骤 24：把计算结果 continuous ? 0f : 0f 保存为rateOverTime；下一条判断或调用会读取这个具体值。
        emission.rateOverTime = continuous ? 0f : 0f;
        // 步骤 25：这里的 ! 表示取反；当 continuous 不成立时才进入此分支。 成立时调用 SetBursts：设置Bursts。
        if (!continuous) emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 32) });
        // 步骤 26：计算 particles.shape，把结果存进 shape（shape）；之后引用 shape 时用的就是这次结果。
        var shape = particles.shape;
        // 步骤 27：把计算结果 continuous ? ParticleSystemShapeType.Box : ParticleSystemShapeType.Cone 保存为shapeType；下一条判断或调用会读取这个具体值。
        shape.shapeType = continuous ? ParticleSystemShapeType.Box : ParticleSystemShapeType.Cone;
        // 步骤 28：检查条件 continuous；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 shape.scale = new Vector3(1.4f, .12f, .15f);
        if (continuous) shape.scale = new Vector3(1.4f, .12f, .15f);
        // 步骤 29：前面的 if 条件为假时执行这里的备用处理。
        else { shape.radius = .28f; shape.angle = 55f; }
        // 步骤 30：计算 particles.colorOverLifetime，把结果存进 color（color）；之后引用 color 时用的就是这次结果。
        var color = particles.colorOverLifetime;
        // 步骤 31：把 color.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        color.enabled = true;
        // 步骤 32：新建用于color的对象或数值结构，并把构造参数保存进去。
        color.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            // 步骤 33：把计算结果 new[] { 保存为colorKeys；下一条判断或调用会读取这个具体值。
            colorKeys = new[] {
                // 步骤 34：用 new 创建 GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },；构造参数决定新对象的初始内容。
                new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            // 步骤 35：把计算结果 new[] { 保存为alphaKeys；下一条判断或调用会读取这个具体值。
            alphaKeys = new[] {
                // 步骤 36：用 new 创建 GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .14f),；构造参数决定新对象的初始内容。
                new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .14f),
                // 步骤 37：用 new 创建 GradientAlphaKey(0f, 1f) }；构造参数决定新对象的初始内容。
                new GradientAlphaKey(0f, 1f) }
        // 步骤 38：执行 });；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        });
        // 步骤 39：从目标物体找到所需组件并存为 renderer；使用前应检查是否为 null。
        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        // 步骤 40：把计算结果 material 保存为shared材质；下一条判断或调用会读取这个具体值。
        renderer.sharedMaterial = material;
        // 步骤 41：把计算结果 ParticleSystemRenderMode.Mesh 保存为render模式；下一条判断或调用会读取这个具体值。
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        // 步骤 42：把计算结果 sparkMesh 保存为mesh；下一条判断或调用会读取这个具体值。
        renderer.mesh = sparkMesh;
        // 步骤 43：把计算结果 ParticleSystemRenderSpace.View 保存为alignment；下一条判断或调用会读取这个具体值。
        renderer.alignment = ParticleSystemRenderSpace.View;
        // 步骤 44：把计算结果 ShadowCastingMode.Off 保存为shadowCasting模式；下一条判断或调用会读取这个具体值。
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        // 步骤 45：检查条件 continuous；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 Play：Play。
        if (continuous) particles.Play();
        // 步骤 46：把 particles 作为方法结果返回给调用方；return 后本方法立即结束。
        return particles;
    }
    // ！！！功能结束：CreateParticles！！！

    // ！！！功能：CreateRing——创建Ring！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private LineRenderer CreateRing(Material material)
    {
        // 步骤 1：创建新的 GameObject 并存为 obj；构造参数决定其初始内容或对象名称。
        GameObject obj = new GameObject("Takeoff light ripple", typeof(LineRenderer));
        // 步骤 2：执行 obj.transform.SetParent(transform, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        obj.transform.SetParent(transform, false);
        // 步骤 3：从目标物体找到所需组件并存为 ring；使用前应检查是否为 null。
        LineRenderer ring = obj.GetComponent<LineRenderer>();
        // 步骤 4：把计算结果 material 保存为shared材质；下一条判断或调用会读取这个具体值。
        ring.sharedMaterial = material;
        // 步骤 5：把 ring.useWorldSpace 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        ring.useWorldSpace = false;
        // 步骤 6：把positionCount更新为 49；世界或局部位置会立即影响物体摆放。
        ring.positionCount = 49;
        // 步骤 7：把计算结果 .045f 保存为widthMultiplier；下一条判断或调用会读取这个具体值。
        ring.widthMultiplier = .045f;
        // 步骤 8：把计算结果 ShadowCastingMode.Off 保存为shadowCasting模式；下一条判断或调用会读取这个具体值。
        ring.shadowCastingMode = ShadowCastingMode.Off;
        // 步骤 9：把 ring.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        ring.enabled = false;
        // 步骤 10：把 ring 作为方法结果返回给调用方；return 后本方法立即结束。
        return ring;
    }
    // ！！！功能结束：CreateRing！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LateUpdate()
    {
        // 步骤 1：检查locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!initialized || locomotion == null) return;
        // 步骤 2：计算 Mathf.Min(Time.deltaTime, .05f)，把结果存进 dt（dt）；之后引用 dt 时用的就是这次结果。
        float dt = Mathf.Min(Time.deltaTime, .05f);
        // 步骤 3：用 flying 记住此处真假结果（locomotion.IsFlying），供下一条 if 选择执行路径。
        bool flying = locomotion.IsFlying;
        // 步骤 4：让spread以限定的最大变化量逐帧靠近目标值，避免突然改变。
        spread = Mathf.MoveTowards(spread, flying ? 1f : 0f, dt * (flying ? 4.5f : 3f));
        // 步骤 5：让boostPulse以限定的最大变化量逐帧靠近目标值，避免突然改变。
        boostPulse = Mathf.MoveTowards(boostPulse, 0f, dt * 2.8f);
        // 步骤 6：调用 UpdateGeometry：更新Geometry。
        UpdateGeometry(Time.time);
        // 步骤 7：调用 UpdateRibbons：更新Ribbons。
        UpdateRibbons(flying);
        // 步骤 8：调用 UpdateRing：更新Ring。
        UpdateRing(dt);
        // 步骤 9：确认flightSparks已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (flightSparks != null)
        {
            // 步骤 10：计算 flightSparks.emission，把结果存进 emission（emission）；之后引用 emission 时用的就是这次结果。
            var emission = flightSparks.emission;
            // 步骤 11：把计算结果 flying ? 17f : 0f 保存为rateOverTime；下一条判断或调用会读取这个具体值。
            emission.rateOverTime = flying ? 17f : 0f;
        }
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：FillColors——FillColors！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int count, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Color[] FillColors(int count, Color color)
    {
        // 步骤 1：创建新的 Color[] 并存为 colors；构造参数决定其初始内容或对象名称。
        Color[] colors = new Color[count];
        // 步骤 2：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++) colors[i] = color;
        // 步骤 3：把 colors 作为方法结果返回给调用方；return 后本方法立即结束。
        return colors;
    }
    // ！！！功能结束：FillColors！！！

    // ！！！功能：CapePoint——CapePoint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float u, float v, float time 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 CapePoint(float u, float v, float time)
    {
        // 步骤 1：取输入的绝对值并存为 side；只比较幅度时暂时不考虑正负方向。
        float side = Mathf.Abs(u);
        // 步骤 2：计算 (u + 1f) * .5f * PanelCount，把结果存进 panelPosition（panel位置）；之后引用 panelPosition 时用的就是这次结果。
        float panelPosition = (u + 1f) * .5f * PanelCount;
        // 步骤 3：计算 panelPosition - Mathf.Floor(panelPosition)，把结果存进 acrossPleat（acrossPleat）；之后引用 acrossPleat 时用的就是这次结果。
        float acrossPleat = panelPosition - Mathf.Floor(panelPosition);
        // 步骤 4：计算 Mathf.Sin(Mathf.PI * acrossPleat)，把结果存进 petalRound（petalRound）；之后引用 petalRound 时用的就是这次结果。
        float petalRound = Mathf.Sin(Mathf.PI * acrossPleat);
        // 步骤 5：把计算结果 Mathf.Lerp(.16f, Mathf.Lerp(.45f, .92f, spread), 保存为width；下一条判断或调用会读取这个具体值。
        float width = Mathf.Lerp(.16f, Mathf.Lerp(.45f, .92f, spread),
            // 步骤 6：调用 Pow：Pow。
            Mathf.Pow(v, .78f));
        // 步骤 7：把计算结果 spread * Mathf.Pow(side, 6f) * v * 保存为wingFeathers；下一条判断或调用会读取这个具体值。
        float wingFeathers = spread * Mathf.Pow(side, 6f) * v *
            // 步骤 8：这是上一行链式调用的下一步 .025f * Mathf.Sin(v * Mathf.PI * 7f);；前一结果会继续传给这里筛选或处理。
            .025f * Mathf.Sin(v * Mathf.PI * 7f);
        // 步骤 9：计算 u * width + Mathf.Sign(u) * wingFeathers，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
        float x = u * width + Mathf.Sign(u) * wingFeathers;
        // 步骤 10：计算 Mathf.Lerp(.70f, .80f, spread)，把结果存进 hemHeight（hemHeight）；之后引用 hemHeight 时用的就是这次结果。
        float hemHeight = Mathf.Lerp(.70f, .80f, spread);
        // 步骤 11：把计算结果 spread * Mathf.Pow(side, 1.7f) * .56f * 保存为wingLift；下一条判断或调用会读取这个具体值。
        float wingLift = spread * Mathf.Pow(side, 1.7f) * .56f *
            // 步骤 12：调用 Pow：Pow。
            Mathf.Pow(v, .85f);
        // 步骤 13：计算 (1f - spread) * side * side * v * .11f，把结果存进 restingShoulder（restingShoulder）；之后引用 restingShoulder 时用的就是这次结果。
        float restingShoulder = (1f - spread) * side * side * v * .11f;
        // 步骤 14：计算 .047f * petalRound * Mathf.Pow(v, 7f)，把结果存进 petalHem（petalHem）；之后引用 petalHem 时用的就是这次结果。
        float petalHem = .047f * petalRound * Mathf.Pow(v, 7f);
        // 步骤 15：把计算结果 Mathf.Lerp(1.32f, hemHeight, v) + wingLift + 保存为y；下一条判断或调用会读取这个具体值。
        float y = Mathf.Lerp(1.32f, hemHeight, v) + wingLift +
            // 步骤 16：执行 restingShoulder - petalHem;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            restingShoulder - petalHem;
        // 步骤 17：计算 Mathf.Lerp(.10f, .39f + spread * .08f, v)，把结果存进 depth（depth）；之后引用 depth 时用的就是这次结果。
        float depth = Mathf.Lerp(.10f, .39f + spread * .08f, v);
        // 步骤 18：把计算结果 Mathf.Lerp( 保存为backCurve；下一条判断或调用会读取这个具体值。
        float backCurve = Mathf.Lerp(
            // 步骤 19：这是上一行链式调用的下一步 .15f * (1f - side * side) - .25f * side * side,；前一结果会继续传给这里筛选或处理。
            .15f * (1f - side * side) - .25f * side * side,
            // 步骤 20：这是上一行链式调用的下一步 .09f * (1f - side * side), spread) * v;；前一结果会继续传给这里筛选或处理。
            .09f * (1f - side * side), spread) * v;
        // 步骤 21：计算 .022f * petalRound * Mathf.Pow(v, .8f)，把结果存进 rib（rib）；之后引用 rib 时用的就是这次结果。
        float rib = .022f * petalRound * Mathf.Pow(v, .8f);
        // 步骤 22：把计算结果 Mathf.Sin(time * (3f + spread * 2.6f) + 保存为flutter；下一条判断或调用会读取这个具体值。
        float flutter = Mathf.Sin(time * (3f + spread * 2.6f) +
            // 步骤 23：执行 u * 7.5f + v * 4f) * (.006f + spread * .028f) * v * v;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            u * 7.5f + v * 4f) * (.006f + spread * .028f) * v * v;
        // 步骤 24：把 new Vector3(x, y, depth + backCurve + rib + flutter + 作为方法结果返回给调用方；return 后本方法立即结束。
        return new Vector3(x, y, depth + backCurve + rib + flutter +
            // 步骤 25：调用 Sin：Sin。
            Mathf.Sin(time * 6f + u * 8f) * boostPulse * .035f * v);
    }
    // ！！！功能结束：CapePoint！！！

    // ！！！功能：UpdateGeometry——更新Geometry！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float time 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateGeometry(float time)
    {
        // 步骤 1：检查cape网格是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (capeMesh == null) return;
        // 步骤 2：按 int panel = 0; panel < PanelCount; panel++ 变化索引；循环体依次处理每个位置。
        for (int panel = 0; panel < PanelCount; panel++)
            // 步骤 3：按 int row = 0; row <= LengthSegments; row++ 变化索引；循环体依次处理每个位置。
            for (int row = 0; row <= LengthSegments; row++)
                // 步骤 4：按 int column = 0; column <= PanelWidthSegments; column++ 变化索引；循环体依次处理每个位置。
                for (int column = 0; column <= PanelWidthSegments; column++)
                {
                    // 步骤 5：计算 (float)row / LengthSegments，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
                    float v = (float)row / LengthSegments;
                    // 步骤 6：计算 (float)column / PanelWidthSegments，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
                    float t = (float)column / PanelWidthSegments;
                    // 步骤 7：计算 -1f + 2f * (panel + t) / PanelCount，把结果存进 u（u）；之后引用 u 时用的就是这次结果。
                    float u = -1f + 2f * (panel + t) / PanelCount;
                    // 步骤 8：把计算结果 panel * PanelVertexCount + 保存为index；下一条判断或调用会读取这个具体值。
                    int index = panel * PanelVertexCount +
                        // 步骤 9：执行 row * (PanelWidthSegments + 1) + column;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        row * (PanelWidthSegments + 1) + column;
                    // 步骤 10：执行 capeVertices[index] = CapePoint(u, v, time);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    capeVertices[index] = CapePoint(u, v, time);
                    // 步骤 11：执行 liningVertices[index] = capeVertices[index] - Vector3.forward * .025f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    liningVertices[index] = capeVertices[index] - Vector3.forward * .025f;
                    // 步骤 12：计算 Mathf.SmoothStep(0f, 1f, v)，把结果存进 pinkAmount（pinkAmount）；之后引用 pinkAmount 时用的就是这次结果。
                    float pinkAmount = Mathf.SmoothStep(0f, 1f, v);
                    // 步骤 13：计算 Color.Lerp(Rose, Pink, pinkAmount)，把结果存进 color（color）；之后引用 color 时用的就是这次结果。
                    Color color = Color.Lerp(Rose, Pink, pinkAmount);
                    // 步骤 14：把计算结果 Color.Lerp(color, Pearl, 保存为color；下一条判断或调用会读取这个具体值。
                    color = Color.Lerp(color, Pearl,
                        // 步骤 15：调用 SmoothStep：SmoothStep。
                        Mathf.SmoothStep(.70f, 1f, v) * .48f);
                    // 步骤 16：把计算结果 .91f + .08f * Mathf.Sin(Mathf.PI * t) + 保存为pleatShade；下一条判断或调用会读取这个具体值。
                    float pleatShade = .91f + .08f * Mathf.Sin(Mathf.PI * t) +
                        // 步骤 17：执行 (panel % 2 == 0 ? .015f : 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                        (panel % 2 == 0 ? .015f : 0f);
                    // 步骤 18：执行 fabricColors[index] = color * pleatShade;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    fabricColors[index] = color * pleatShade;
                    // 步骤 19：这一行补充 liningColors[index] = Color.Lerp(，与相邻行合起来完成同一次计算或对象配置。
                    liningColors[index] = Color.Lerp(
                        // 步骤 20：用 new 创建 Color(.94f, .68f, .76f), new Color(1f, .95f, .88f), v);；构造参数决定新对象的初始内容。
                        new Color(.94f, .68f, .76f), new Color(1f, .95f, .88f), v);
                }
        // 步骤 21：把计算结果 capeVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        capeMesh.vertices = capeVertices;
        // 步骤 22：把计算结果 fabricColors 保存为colors；下一条判断或调用会读取这个具体值。
        capeMesh.colors = fabricColors;
        // 步骤 23：调用 RecalculateNormals：RecalculateNormals。
        capeMesh.RecalculateNormals();
        // 步骤 24：调用 RecalculateBounds：RecalculateBounds。
        capeMesh.RecalculateBounds();
        // 步骤 25：把计算结果 liningVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        liningMesh.vertices = liningVertices;
        // 步骤 26：把计算结果 liningColors 保存为colors；下一条判断或调用会读取这个具体值。
        liningMesh.colors = liningColors;
        // 步骤 27：调用 RecalculateNormals：RecalculateNormals。
        liningMesh.RecalculateNormals();
        // 步骤 28：调用 RecalculateBounds：RecalculateBounds。
        liningMesh.RecalculateBounds();

        // 步骤 29：按 int panel = 0; panel < PanelCount; panel++ 变化索引；循环体依次处理每个位置。
        for (int panel = 0; panel < PanelCount; panel++)
            // 步骤 30：按 int column = 0; column <= PanelWidthSegments; column++ 变化索引；循环体依次处理每个位置。
            for (int column = 0; column <= PanelWidthSegments; column++)
            {
                // 步骤 31：把计算结果 -1f + 2f * (panel + 保存为u；下一条判断或调用会读取这个具体值。
                float u = -1f + 2f * (panel +
                    // 步骤 32：执行 (float)column / PanelWidthSegments) / PanelCount;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    (float)column / PanelWidthSegments) / PanelCount;
                // 步骤 33：计算 (panel * (PanelWidthSegments + 1) + column) * 2，把结果存进 index（index）；之后引用 index 时用的就是这次结果。
                int index = (panel * (PanelWidthSegments + 1) + column) * 2;
                // 步骤 34：计算 CapePoint(u, 1f, time)，把结果存进 edge（edge）；之后引用 edge 时用的就是这次结果。
                Vector3 edge = CapePoint(u, 1f, time);
                // 步骤 35：执行 hemVertices[index] = edge + Vector3.forward * .004f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemVertices[index] = edge + Vector3.forward * .004f;
                // 步骤 36：执行 hemVertices[index + 1] = edge - Vector3.forward * .028f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemVertices[index + 1] = edge - Vector3.forward * .028f;
                // 步骤 37：执行 hemColors[index] = Pearl;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemColors[index] = Pearl;
                // 步骤 38：执行 hemColors[index + 1] = new Color(1f, .95f, .88f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                hemColors[index + 1] = new Color(1f, .95f, .88f);
            }
        // 步骤 39：把计算结果 hemVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        hemMesh.vertices = hemVertices;
        // 步骤 40：把计算结果 hemColors 保存为colors；下一条判断或调用会读取这个具体值。
        hemMesh.colors = hemColors;
        // 步骤 41：调用 RecalculateNormals：RecalculateNormals。
        hemMesh.RecalculateNormals();
        // 步骤 42：调用 RecalculateBounds：RecalculateBounds。
        hemMesh.RecalculateBounds();

        // 步骤 43：按 int gem = 0; gem < GemCount; gem++ 变化索引；循环体依次处理每个位置。
        for (int gem = 0; gem < GemCount; gem++)
        {
            // 步骤 44：计算 .18f + gem * .16f，把结果存进 v（v）；之后引用 v 时用的就是这次结果。
            float v = .18f + gem * .16f;
            // 步骤 45：计算 CapePoint(0f, v, time)，把结果存进 surface（surface）；之后引用 surface 时用的就是这次结果。
            Vector3 surface = CapePoint(0f, v, time);
            // 步骤 46：计算 .029f + gem * .001f，把结果存进 w（w）；之后引用 w 时用的就是这次结果。
            float w = .029f + gem * .001f;
            // 步骤 47：计算 .036f + gem * .001f，把结果存进 h（h）；之后引用 h 时用的就是这次结果。
            float h = .036f + gem * .001f;
            // 步骤 48：计算 surface + Vector3.forward * .055f，把结果存进 outline（outline）；之后引用 outline 时用的就是这次结果。
            Vector3 outline = surface + Vector3.forward * .055f;
            // 步骤 49：计算 gem * 4，把结果存进 b（b）；之后引用 b 时用的就是这次结果。
            int b = gem * 4;
            // 步骤 50：执行 gemOutlineVertices[b] = outline + Vector3.up * h;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            gemOutlineVertices[b] = outline + Vector3.up * h;
            // 步骤 51：执行 gemOutlineVertices[b + 1] = outline + Vector3.right * w;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            gemOutlineVertices[b + 1] = outline + Vector3.right * w;
            // 步骤 52：执行 gemOutlineVertices[b + 2] = outline - Vector3.up * h;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            gemOutlineVertices[b + 2] = outline - Vector3.up * h;
            // 步骤 53：执行 gemOutlineVertices[b + 3] = outline - Vector3.right * w;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            gemOutlineVertices[b + 3] = outline - Vector3.right * w;

            // 步骤 54：计算 outline + Vector3.forward * .008f，把结果存进 facetCenter（facetCenter）；之后引用 facetCenter 时用的就是这次结果。
            Vector3 facetCenter = outline + Vector3.forward * .008f;
            // 步骤 55：计算 w * .78f, innerH = h * .78f，把结果存进 innerW（innerw）；之后引用 innerW 时用的就是这次结果。
            float innerW = w * .78f, innerH = h * .78f;
            // 步骤 56：按 int facet = 0; facet < 3; facet++ 变化索引；循环体依次处理每个位置。
            for (int facet = 0; facet < 3; facet++)
            {
                // 步骤 57：计算 gem * 3 + facet, s = i * 4，把结果存进 i（i）；之后引用 i 时用的就是这次结果。
                int i = gem * 3 + facet, s = i * 4;
                // 步骤 58：计算 innerH * (1f - 2f * facet / 3f)，把结果存进 top（top）；之后引用 top 时用的就是这次结果。
                float top = innerH * (1f - 2f * facet / 3f);
                // 步骤 59：计算 innerH * (1f - 2f * (facet + 1) / 3f)，把结果存进 bottom（bottom）；之后引用 bottom 时用的就是这次结果。
                float bottom = innerH * (1f - 2f * (facet + 1) / 3f);
                // 步骤 60：取输入的绝对值并存为 topWidth；只比较幅度时暂时不考虑正负方向。
                float topWidth = innerW * (1f - Mathf.Abs(top) / innerH);
                // 步骤 61：取输入的绝对值并存为 bottomWidth；只比较幅度时暂时不考虑正负方向。
                float bottomWidth = innerW * (1f - Mathf.Abs(bottom) / innerH);
                // 步骤 62：执行 starVertices[s] = facetCenter + new Vector3(-topWidth, top, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                starVertices[s] = facetCenter + new Vector3(-topWidth, top, 0f);
                // 步骤 63：执行 starVertices[s + 1] = facetCenter + new Vector3(topWidth, top, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                starVertices[s + 1] = facetCenter + new Vector3(topWidth, top, 0f);
                // 步骤 64：执行 starVertices[s + 2] = facetCenter + new Vector3(bottomWidth, bottom, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                starVertices[s + 2] = facetCenter + new Vector3(bottomWidth, bottom, 0f);
                // 步骤 65：执行 starVertices[s + 3] = facetCenter + new Vector3(-bottomWidth, bottom, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                starVertices[s + 3] = facetCenter + new Vector3(-bottomWidth, bottom, 0f);
                // 步骤 66：把计算结果 energy != null ? 保存为charge；下一条判断或调用会读取这个具体值。
                float charge = energy != null ?
                    // 步骤 67：调用 Clamp01：Clamp01。
                    Mathf.Clamp01(energy.CurrentEnergy - i) : 1f;
                // 步骤 68：计算 .95f + .05f * Mathf.Sin(time * 4f + i * 1.3f)，把结果存进 twinkle（twinkle）；之后引用 twinkle 时用的就是这次结果。
                float twinkle = .95f + .05f * Mathf.Sin(time * 4f + i * 1.3f);
                // 步骤 69：把计算结果 Color.Lerp(DimStar, 保存为facetColor；下一条判断或调用会读取这个具体值。
                Color facetColor = Color.Lerp(DimStar,
                    // 步骤 70：用 new 创建 Color(1f, .96f, .87f), charge * twinkle);；构造参数决定新对象的初始内容。
                    new Color(1f, .96f, .87f), charge * twinkle);
                // 步骤 71：按 int corner = 0; corner < 4; corner++ 变化索引；循环体依次处理每个位置。
                for (int corner = 0; corner < 4; corner++)
                    // 步骤 72：执行 starColors[s + corner] = facetColor;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    starColors[s + corner] = facetColor;
            }

            // 步骤 73：计算 gem * 9，把结果存进 g（g）；之后引用 g 时用的就是这次结果。
            int g = gem * 9;
            // 步骤 74：计算 facetCenter + Vector3.forward * .004f，把结果存进 glowCenter（glowCenter）；之后引用 glowCenter 时用的就是这次结果。
            Vector3 glowCenter = facetCenter + Vector3.forward * .004f;
            // 步骤 75：把数值 0f 暂存为 glowCharge；它只在当前方法里参与后续判断或计算。
            float glowCharge = 0f;
            // 步骤 76：按 int facet = 0; facet < 3; facet++ 变化索引；循环体依次处理每个位置。
            for (int facet = 0; facet < 3; facet++)
                // 步骤 77：在glowCharge原值上增加 energy != null ?；旧状态与本次输入共同决定新状态。
                glowCharge += energy != null ?
                    // 步骤 78：调用 Clamp01：Clamp01。
                    Mathf.Clamp01(energy.CurrentEnergy - gem * 3 - facet) : 1f;
            // 步骤 79：执行 glowCharge /= 3f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            glowCharge /= 3f;
            // 步骤 80：执行 haloVertices[g] = glowCenter;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            haloVertices[g] = glowCenter;
            // 步骤 81：继续填写上一行调用的参数：haloColors[g] = new Color(1f, .94f, .78f；参数位置决定它表示的坐标、速度或选项。
            haloColors[g] = new Color(1f, .94f, .78f,
                // 步骤 82：执行 glowCharge * (.07f + boostPulse * .15f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                glowCharge * (.07f + boostPulse * .15f));
            // 步骤 83：按 int corner = 0; corner < 4; corner++ 变化索引；循环体依次处理每个位置。
            for (int corner = 0; corner < 4; corner++)
            {
                // 步骤 84：把计算结果 corner == 0 ? Vector3.up : 保存为offset；下一条判断或调用会读取这个具体值。
                Vector3 offset = corner == 0 ? Vector3.up :
                    // 步骤 85：把计算结果 = 1 ? Vector3.right : 保存为corner；下一条判断或调用会读取这个具体值。
                    corner == 1 ? Vector3.right :
                    // 步骤 86：把计算结果 = 2 ? Vector3.down : Vector3.left 保存为corner；下一条判断或调用会读取这个具体值。
                    corner == 2 ? Vector3.down : Vector3.left;
                // 步骤 87：这一行补充 haloVertices[g + 1 + corner] = glowCenter +，与相邻行合起来完成同一次计算或对象配置。
                haloVertices[g + 1 + corner] = glowCenter +
                    // 步骤 88：执行 offset * h * 1.08f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    offset * h * 1.08f;
                // 步骤 89：这一行补充 haloVertices[g + 5 + corner] = glowCenter +，与相邻行合起来完成同一次计算或对象配置。
                haloVertices[g + 5 + corner] = glowCenter +
                    // 步骤 90：执行 offset * h * 1.75f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    offset * h * 1.75f;
                // 步骤 91：继续填写上一行调用的参数：haloColors[g + 1 + corner] = new Color(1f, .93f, .75f；参数位置决定它表示的坐标、速度或选项。
                haloColors[g + 1 + corner] = new Color(1f, .93f, .75f,
                    // 步骤 92：执行 glowCharge * (.045f + boostPulse * .09f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    glowCharge * (.045f + boostPulse * .09f));
                // 步骤 93：执行 haloColors[g + 5 + corner] = new Color(1f, .93f, .75f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                haloColors[g + 5 + corner] = new Color(1f, .93f, .75f, 0f);
            }
        }
        // 步骤 94：把计算结果 gemOutlineVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        gemOutlineMesh.vertices = gemOutlineVertices;
        // 步骤 95：调用 RecalculateNormals：RecalculateNormals。
        gemOutlineMesh.RecalculateNormals();
        // 步骤 96：调用 RecalculateBounds：RecalculateBounds。
        gemOutlineMesh.RecalculateBounds();
        // 步骤 97：把计算结果 starVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        starMesh.vertices = starVertices;
        // 步骤 98：把计算结果 starColors 保存为colors；下一条判断或调用会读取这个具体值。
        starMesh.colors = starColors;
        // 步骤 99：调用 RecalculateNormals：RecalculateNormals。
        starMesh.RecalculateNormals();
        // 步骤 100：调用 RecalculateBounds：RecalculateBounds。
        starMesh.RecalculateBounds();
        // 步骤 101：把计算结果 haloVertices 保存为vertices；下一条判断或调用会读取这个具体值。
        haloMesh.vertices = haloVertices;
        // 步骤 102：把计算结果 haloColors 保存为colors；下一条判断或调用会读取这个具体值。
        haloMesh.colors = haloColors;
        // 步骤 103：调用 RecalculateBounds：RecalculateBounds。
        haloMesh.RecalculateBounds();
    }
    // ！！！功能结束：UpdateGeometry！！！

    // ！！！功能：UpdateRibbons——更新Ribbons！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool flying 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateRibbons(bool flying)
    {
        // 步骤 1：检查leftRibbon、rightRibbon是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (leftRibbon == null || rightRibbon == null) return;
        // The ribbon anchors ignore the cloth flutter. Otherwise small wing
        // ripples turn the long silver trails into jagged hanging zigzags.
        // 步骤 2：执行 leftRibbon.transform.localPosition = CapePoint(-.92f, .93f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        leftRibbon.transform.localPosition = CapePoint(-.92f, .93f, 0f);
        // 步骤 3：执行 rightRibbon.transform.localPosition = CapePoint(.92f, .93f, 0f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        rightRibbon.transform.localPosition = CapePoint(.92f, .93f, 0f);
        // 步骤 4：用 stream 记住此处真假结果（flying && spread > .92f && locomotion.CurrentSpeed > .5f），供下一条 if 选择执行路径。
        bool stream = flying && spread > .92f && locomotion.CurrentSpeed > .5f;
        // 步骤 5：把计算结果 stream 保存为emitting；下一条判断或调用会读取这个具体值。
        leftRibbon.emitting = stream;
        // 步骤 6：把计算结果 stream 保存为emitting；下一条判断或调用会读取这个具体值。
        rightRibbon.emitting = stream;
        // 步骤 7：这里用 && 要求两边条件同时成立：!flying && spread <= .01f；任意一边为假就跳过本分支。
        if (!flying && spread <= .01f)
        {
            // 步骤 8：调用 Clear：Clear。
            leftRibbon.Clear();
            // 步骤 9：调用 Clear：Clear。
            rightRibbon.Clear();
        }
    }
    // ！！！功能结束：UpdateRibbons！！！

    // ！！！功能：UpdateRing——更新Ring！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float dt 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateRing(float dt)
    {
        // 步骤 1：检查takeoffRing是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (takeoffRing == null) return;
        // 步骤 2：在ringAge原值上增加 dt；旧状态与本次输入共同决定新状态。
        ringAge += dt;
        // 步骤 3：检查条件 ringAge >= .52f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 { takeoffRing.enabled = false; return; }
        if (ringAge >= .52f) { takeoffRing.enabled = false; return; }
        // 步骤 4：把 takeoffRing.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        takeoffRing.enabled = true;
        // 步骤 5：计算 ringAge / .52f，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
        float t = ringAge / .52f;
        // 步骤 6：计算 Mathf.Lerp(.22f, 1.4f, t)，把结果存进 radius（radius）；之后引用 radius 时用的就是这次结果。
        float radius = Mathf.Lerp(.22f, 1.4f, t);
        // 步骤 7：计算 (1f - t) * (1f - t) * .8f，把结果存进 alpha（alpha）；之后引用 alpha 时用的就是这次结果。
        float alpha = (1f - t) * (1f - t) * .8f;
        // 步骤 8：新建用于startColor的对象或数值结构，并把构造参数保存进去。
        takeoffRing.startColor = new Color(1f, .94f, .65f, alpha);
        // 步骤 9：新建用于endColor的对象或数值结构，并把构造参数保存进去。
        takeoffRing.endColor = new Color(.7f, .92f, 1f, alpha);
        // 步骤 10：按 int i = 0; i < takeoffRing.positionCount; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < takeoffRing.positionCount; i++)
        {
            // 步骤 11：计算 i * Mathf.PI * 2f / (takeoffRing.positionCount - 1)，把结果存进 angle（angle）；之后引用 angle 时用的就是这次结果。
            float angle = i * Mathf.PI * 2f / (takeoffRing.positionCount - 1);
            // 步骤 12：设置 LineRenderer 的一个端点；两个端点共同组成玩家看到的手柄射线。
            takeoffRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius,
                // 步骤 13：这是上一行链式调用的下一步 .22f - t * .14f, Mathf.Sin(angle) * radius));；前一结果会继续传给这里筛选或处理。
                .22f - t * .14f, Mathf.Sin(angle) * radius));
        }
    }
    // ！！！功能结束：UpdateRing！！！

    // ！！！功能：RefreshEnergy——根据现有能量点亮或熄灭斗篷上的星星！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RefreshEnergy()
    {
        // 步骤 1：检查条件 initialized；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 UpdateGeometry：更新Geometry。
        if (initialized) UpdateGeometry(Time.time);
    }
    // ！！！功能结束：RefreshEnergy！！！

    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认flight已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flight.OnBoostSucceeded -= PlayBoost;
        if (flight != null) flight.OnBoostSucceeded -= PlayBoost;
        // 步骤 2：确认cape网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (capeMesh != null) Destroy(capeMesh);
        // 步骤 3：确认lining网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (liningMesh != null) Destroy(liningMesh);
        // 步骤 4：确认hem网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (hemMesh != null) Destroy(hemMesh);
        // 步骤 5：确认gemOutline网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (gemOutlineMesh != null) Destroy(gemOutlineMesh);
        // 步骤 6：确认star网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (starMesh != null) Destroy(starMesh);
        // 步骤 7：确认halo网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (haloMesh != null) Destroy(haloMesh);
        // 步骤 8：确认spark网格已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (sparkMesh != null) Destroy(sparkMesh);
    }
    // ！！！功能结束：OnDestroy！！！
}
