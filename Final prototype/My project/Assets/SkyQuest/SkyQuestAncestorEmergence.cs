// ！！！文件功能：先祖从纪念物显现和退回的动画、光效与姿态！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections 命名空间；下方可直接使用该库提供的类型和方法。
using System.Collections;
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;
// 引入 UnityEngine.Rendering 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEngine.Rendering;

// Proximity animation shared by the legacy ancestor and gallery ancestors.
// The motion follows the supplied Sky reference: a small white light wakes on
// the memorial, unfolds into a warm floating spirit, then folds back into the
// memorial when the player leaves. The exchange input is handled elsewhere.
// 定义 SkyQuestAncestorEmergence 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestAncestorEmergence : MonoBehaviour
{
    // 把 Inspector 可调整的 revealDuration（revealDuration）设为默认值 1.45f；具体用途看它在下方的判断和计算。
    public float revealDuration = 1.45f;
    // 把 Inspector 可调整的 dismissDuration（dismissDuration）设为默认值 1.25f；具体用途看它在下方的判断和计算。
    public float dismissDuration = 1.25f;

    // 声明 spirit（spirit）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform spirit;
    // 声明 memorial（memorial）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform memorial;
    // spiritRenderers 保存spiritRenderers（类型 Renderer[]）；下方方法依靠它记录或查找场景状态。
    private Renderer[] spiritRenderers;
    // rendererStates 保存rendererStates（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] rendererStates;
    // spiritMaterials 保存spirit材质（类型 Material[][]）；下方方法依靠它记录或查找场景状态。
    private Material[][] spiritMaterials;
    // spiritBaseColors 保存spiritBaseColors（类型 Color[][]）；下方方法依靠它记录或查找场景状态。
    private Color[][] spiritBaseColors;
    // 创建新的 List<Renderer> 并存为 memorialMarks；构造参数决定其初始内容或对象名称。
    private readonly List<Renderer> memorialMarks = new List<Renderer>();
    // 创建新的 List<Color> 并存为 memorialBaseColors；构造参数决定其初始内容或对象名称。
    private readonly List<Color> memorialBaseColors = new List<Color>();
    // 执行 private Transform leftArm, rightArm, leftForearm, rightForearm, headBone;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Transform leftArm, rightArm, leftForearm, rightForearm, headBone;
    // 执行 private Quaternion leftArmRest, rightArmRest, leftForearmRest, rightForearmRest, headRest;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
    private Quaternion leftArmRest, rightArmRest, leftForearmRest, rightForearmRest, headRest;
    // 创建新的 List<Transform> 并存为 clothParts；构造参数决定其初始内容或对象名称。
    private readonly List<Transform> clothParts = new List<Transform>();
    // 创建新的 List<Quaternion> 并存为 clothRestRotations；构造参数决定其初始内容或对象名称。
    private readonly List<Quaternion> clothRestRotations = new List<Quaternion>();
    // 声明 finalPosition（final位置）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 finalPosition;
    // finalRotation 保存final旋转（类型 Quaternion）；下方方法依靠它记录或查找场景状态。
    private Quaternion finalRotation;
    // 声明 finalScale（final缩放）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 finalScale;
    // motes 保存motes（类型 ParticleSystem）；下方方法依靠它记录或查找场景状态。
    private ParticleSystem motes;
    // spiritLight 保存spirit灯光（类型 Light）；下方方法依靠它记录或查找场景状态。
    private Light spiritLight;
    // moteTexture 保存mote贴图（类型 Texture2D）；下方方法依靠它记录或查找场景状态。
    private Texture2D moteTexture;
    // motionRoutine 保存motionRoutine（类型 Coroutine）；下方方法依靠它记录或查找场景状态。
    private Coroutine motionRoutine;
    // 声明 initialized（initialized）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool initialized;
    // 声明 visible（visible）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool visible;
    // 声明 transitioning（transitioning）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool transitioning;
    // 声明 idleTime（idleTime）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float idleTime;
    // 声明 currentVisibility（currentVisibility）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float currentVisibility;
    // particleBake 保存particleBake（类型 Mesh）；下方方法依靠它记录或查找场景状态。
    private Mesh particleBake;
    // 声明 particleCarry（particleCarry）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float particleCarry;

    // ！！！功能：Initialize——Initialize！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform spiritActor, Transform memorialStone 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void Initialize(Transform spiritActor, Transform memorialStone)
    {
        // 步骤 1：检查条件 initialized；只有为真才运行下面的步骤，否则沿原流程继续。
        if (initialized)
        {
            // 步骤 2：检查memorial、memorialStone是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 memorial = memorialStone;
            if (memorial == null && memorialStone != null) memorial = memorialStone;
            // 步骤 3：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 4：检查spiritActor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (spiritActor == null) return;
        // 步骤 5：把 initialized 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        initialized = true;
        // 步骤 6：把计算结果 spiritActor 保存为spirit；下一条判断或调用会读取这个具体值。
        spirit = spiritActor;
        // 步骤 7：把计算结果 memorialStone 保存为memorial；下一条判断或调用会读取这个具体值。
        memorial = memorialStone;
        // 步骤 8：把计算结果 spirit.position 保存为final位置；下一条判断或调用会读取这个具体值。
        finalPosition = spirit.position;
        // 步骤 9：把计算结果 spirit.rotation 保存为final旋转；下一条判断或调用会读取这个具体值。
        finalRotation = spirit.rotation;
        // 步骤 10：把计算结果 spirit.localScale 保存为final缩放；下一条判断或调用会读取这个具体值。
        finalScale = spirit.localScale;
        // 步骤 11：从目标 GameObject 查找并保存 spiritRenderers 组件；后面的移动或显示逻辑会直接使用这个缓存。
        spiritRenderers = spirit.GetComponentsInChildren<Renderer>(true);
        // 步骤 12：新建用于rendererStates的对象或数值结构，并把构造参数保存进去。
        rendererStates = new bool[spiritRenderers.Length];
        // 步骤 13：按 int i = 0; i < spiritRenderers.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < spiritRenderers.Length; i++)
            // 步骤 14：执行 rendererStates[i] = spiritRenderers[i] != null && spiritRenderers[i].enabled;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            rendererStates[i] = spiritRenderers[i] != null && spiritRenderers[i].enabled;
        // 步骤 15：调用 CachePoseBones：CachePoseBones。
        CachePoseBones();
        // 步骤 16：调用 PrepareSpiritMaterials：PrepareSpirit材质。
        PrepareSpiritMaterials();
        // 步骤 17：调用 PrepareMemorialEffects：PrepareMemorialEffects。
        PrepareMemorialEffects();
    }
    // ！！！功能结束：Initialize！！！

    // ！！！功能：PlayReveal——PlayReveal！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayReveal()
    {
        // 步骤 1：这里的 ! 表示取反；当 initialized 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!initialized) return;
        // 步骤 2：调用 EnsureSpiritCache：确保SpiritCache。
        EnsureSpiritCache();
        // 步骤 3：调用 StopMotion：StopMotion。
        StopMotion();
        // 步骤 4：把 visible 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        visible = false;
        // 步骤 5：把 transitioning 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        transitioning = true;
        // 步骤 6：把计算结果 0f 保存为idleTime；下一条判断或调用会读取这个具体值。
        idleTime = 0f;
        // 步骤 7：把计算结果 0f 保存为particleCarry；下一条判断或调用会读取这个具体值。
        particleCarry = 0f;
        // 步骤 8：调用 RestorePose：RestorePose。
        RestorePose();
        // 步骤 9：调用 SetSpiritLook：设置SpiritLook。
        SetSpiritLook(0f);
        // 步骤 10：调用 SetRenderers：设置Renderers。
        SetRenderers(false);
        // 步骤 11：确认motes已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (motes != null)
        {
            // 步骤 12：调用 Stop：Stop。
            motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // 步骤 13：调用 Play：Play。
            motes.Play(true);
        }
        // 步骤 14：把计算结果 StartCoroutine(RevealFromMemorial()) 保存为motionRoutine；下一条判断或调用会读取这个具体值。
        motionRoutine = StartCoroutine(RevealFromMemorial());
    }
    // ！！！功能结束：PlayReveal！！！

    // ！！！功能：PlayDismiss——PlayDismiss！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void PlayDismiss()
    {
        // 步骤 1：这里的 ! 表示取反；当 initialized 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!initialized) return;
        // 步骤 2：这里用 && 要求两边条件同时成立：!visible && !transitioning；任意一边为假就跳过本分支。 成立时执行 { HideImmediate(); return; }
        if (!visible && !transitioning) { HideImmediate(); return; }
        // 步骤 3：调用 StopMotion：StopMotion。
        StopMotion();
        // 步骤 4：把 transitioning 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        transitioning = true;
        // 步骤 5：把 visible 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        visible = false;
        // 步骤 6：检查motes是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时调用 Play：Play。
        if (motes != null && !motes.isPlaying) motes.Play(true);
        // 步骤 7：把计算结果 StartCoroutine(ReturnToMemorial()) 保存为motionRoutine；下一条判断或调用会读取这个具体值。
        motionRoutine = StartCoroutine(ReturnToMemorial());
    }
    // ！！！功能结束：PlayDismiss！！！

    // ！！！功能：HideImmediate——HideImmediate！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void HideImmediate()
    {
        // 步骤 1：这里的 ! 表示取反；当 initialized 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!initialized) return;
        // 步骤 2：调用 StopMotion：StopMotion。
        StopMotion();
        // 步骤 3：把 transitioning 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        transitioning = false;
        // 步骤 4：把 visible 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        visible = false;
        // 步骤 5：把计算结果 0f 保存为currentVisibility；下一条判断或调用会读取这个具体值。
        currentVisibility = 0f;
        // 步骤 6：把position更新为 finalPosition；世界或局部位置会立即影响物体摆放。
        spirit.position = finalPosition;
        // 步骤 7：把rotation更新为 finalRotation；物体朝向会在本帧改变。
        spirit.rotation = finalRotation;
        // 步骤 8：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        spirit.localScale = finalScale;
        // 步骤 9：调用 RestorePose：RestorePose。
        RestorePose();
        // 步骤 10：调用 SetSpiritLook：设置SpiritLook。
        SetSpiritLook(0f);
        // 步骤 11：调用 SetMemorialGlow：设置MemorialGlow。
        SetMemorialGlow(0f);
        // 步骤 12：确认motes已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Stop：Stop。
        if (motes != null) motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 步骤 13：确认spirit灯光已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 spiritLight.intensity = 0f;
        if (spiritLight != null) spiritLight.intensity = 0f;
        // 步骤 14：调用 SetRenderers：设置Renderers。
        SetRenderers(false);
    }
    // ！！！功能结束：HideImmediate！！！

    // ！！！功能：RevealFromMemorial——Reveal从Memorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator RevealFromMemorial()
    {
        // 步骤 1：计算 MemorialLightPoint()，把结果存进 lightPoint（lightPoint）；之后引用 lightPoint 时用的就是这次结果。
        Vector3 lightPoint = MemorialLightPoint();
        // 步骤 2：计算 finalScale * .08f，把结果存进 startScale（start缩放）；之后引用 startScale 时用的就是这次结果。
        Vector3 startScale = finalScale * .08f;
        // 步骤 3：把position更新为 lightPoint；世界或局部位置会立即影响物体摆放。
        spirit.position = lightPoint;
        // 步骤 4：把rotation更新为 finalRotation；物体朝向会在本帧改变。
        spirit.rotation = finalRotation;
        // 步骤 5：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        spirit.localScale = startScale;
        // 步骤 6：调用 SetRenderers：设置Renderers。
        SetRenderers(true);
        // 步骤 7：调用 EmitMotes：EmitMotes。
        EmitMotes(lightPoint, 18, .12f);
        // 步骤 8：把数值 0f 暂存为 elapsed；它只在当前方法里参与后续判断或计算。
        float elapsed = 0f;
        // 步骤 9：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (elapsed < revealDuration)
        {
            // 步骤 10：把每秒变化率乘本帧秒数，更新elapsed；帧率变化时速度仍接近一致。
            elapsed += Time.deltaTime;
            // 步骤 11：计算 t 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.1f, revealDuration));
            // 步骤 12：计算 EaseOutCubic(t)，把结果存进 eased（eased）；之后引用 eased 时用的就是这次结果。
            float eased = EaseOutCubic(t);
            // 步骤 13：计算 Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.04f, .66f, t))，把结果存进 visibility（visibility）；之后引用 visibility 时用的就是这次结果。
            float visibility = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.04f, .66f, t));
            // 步骤 14：把计算结果 visibility 保存为currentVisibility；下一条判断或调用会读取这个具体值。
            currentVisibility = visibility;
            // 步骤 15：对起点与终点插值来更新position；随进度变化呈现连续位移或旋转。
            spirit.position = Vector3.Lerp(lightPoint, finalPosition, eased) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .18f);
            // 步骤 16：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
            spirit.rotation = Quaternion.Slerp(finalRotation * Quaternion.Euler(0f, -12f, 0f), finalRotation, eased);
            // 步骤 17：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            spirit.localScale = Vector3.Lerp(startScale, finalScale, eased);
            // 步骤 18：调用 PoseSpirit：PoseSpirit。
            PoseSpirit(eased, elapsed, false);
            // 步骤 19：调用 SetSpiritLook：设置SpiritLook。
            SetSpiritLook(visibility);
            // 步骤 20：调用 SetMemorialGlow：设置MemorialGlow。
            SetMemorialGlow(Mathf.Sin(t * Mathf.PI));
            // 步骤 21：确认spirit灯光已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (spiritLight != null)
            {
                // 步骤 22：执行 spiritLight.transform.position = Vector3.Lerp(lightPoint, spirit.position + Vector3.up, eased);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                spiritLight.transform.position = Vector3.Lerp(lightPoint, spirit.position + Vector3.up, eased);
                // 步骤 23：把计算结果 Mathf.Sin(t * Mathf.PI) * 1.15f + visibility * .22f 保存为intensity；下一条判断或调用会读取这个具体值。
                spiritLight.intensity = Mathf.Sin(t * Mathf.PI) * 1.15f + visibility * .22f;
            }
            // 步骤 24：检查条件 t < .72f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 EmitMotes：EmitMotes。
            if (t < .72f) EmitMotes(Vector3.Lerp(lightPoint, spirit.position, eased), 9f, .08f);
            // 步骤 25：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        }
        // 步骤 26：把position更新为 finalPosition；世界或局部位置会立即影响物体摆放。
        spirit.position = finalPosition;
        // 步骤 27：把rotation更新为 finalRotation；物体朝向会在本帧改变。
        spirit.rotation = finalRotation;
        // 步骤 28：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        spirit.localScale = finalScale;
        // 步骤 29：把计算结果 1f 保存为currentVisibility；下一条判断或调用会读取这个具体值。
        currentVisibility = 1f;
        // 步骤 30：调用 SetSpiritLook：设置SpiritLook。
        SetSpiritLook(1f);
        // 步骤 31：调用 SetMemorialGlow：设置MemorialGlow。
        SetMemorialGlow(.12f);
        // 步骤 32：把 transitioning 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        transitioning = false;
        // 步骤 33：把 visible 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        visible = true;
        // 步骤 34：把计算结果 0f 保存为idleTime；下一条判断或调用会读取这个具体值。
        idleTime = 0f;
        // 步骤 35：清空 motionRoutine 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        motionRoutine = null;
    }
    // ！！！功能结束：RevealFromMemorial！！！

    // ！！！功能：ReturnToMemorial——Return到Memorial！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private IEnumerator ReturnToMemorial()
    {
        // 步骤 1：计算 spirit.position，把结果存进 startPosition（start位置）；之后引用 startPosition 时用的就是这次结果。
        Vector3 startPosition = spirit.position;
        // 步骤 2：计算 spirit.rotation，把结果存进 startRotation（start旋转）；之后引用 startRotation 时用的就是这次结果。
        Quaternion startRotation = spirit.rotation;
        // 步骤 3：计算 spirit.localScale，把结果存进 startScale（start缩放）；之后引用 startScale 时用的就是这次结果。
        Vector3 startScale = spirit.localScale;
        // 步骤 4：计算 MemorialLightPoint()，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Vector3 target = MemorialLightPoint();
        // 步骤 5：计算 Mathf.Max(.05f, currentVisibility)，把结果存进 startVisibility（startVisibility）；之后引用 startVisibility 时用的就是这次结果。
        float startVisibility = Mathf.Max(.05f, currentVisibility);
        // 步骤 6：调用 EmitMotes：EmitMotes。
        EmitMotes(spirit.position + Vector3.up * .8f, 8, .08f);
        // 步骤 7：把数值 0f 暂存为 elapsed；它只在当前方法里参与后续判断或计算。
        float elapsed = 0f;
        // 步骤 8：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (elapsed < dismissDuration)
        {
            // 步骤 9：把每秒变化率乘本帧秒数，更新elapsed；帧率变化时速度仍接近一致。
            elapsed += Time.deltaTime;
            // 步骤 10：计算 t 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.1f, dismissDuration));
            // 步骤 11：计算 Mathf.SmoothStep(0f, 1f, t)，把结果存进 eased（eased）；之后引用 eased 时用的就是这次结果。
            float eased = Mathf.SmoothStep(0f, 1f, t);
            // 步骤 12：计算 startVisibility * (1f - Mathf.SmoothStep(.38f, 1f, t))，把结果存进 visibility（visibility）；之后引用 visibility 时用的就是这次结果。
            float visibility = startVisibility * (1f - Mathf.SmoothStep(.38f, 1f, t));
            // 步骤 13：把计算结果 visibility 保存为currentVisibility；下一条判断或调用会读取这个具体值。
            currentVisibility = visibility;
            // 步骤 14：对起点与终点插值来更新position；随进度变化呈现连续位移或旋转。
            spirit.position = Vector3.Lerp(startPosition, target, eased) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .22f);
            // 步骤 15：对起点与终点插值来更新rotation；随进度变化呈现连续位移或旋转。
            spirit.rotation = Quaternion.Slerp(startRotation, finalRotation * Quaternion.Euler(-10f, 10f, 0f), eased);
            // 步骤 16：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
            spirit.localScale = Vector3.Lerp(startScale, finalScale * .07f, eased);
            // 步骤 17：调用 PoseSpirit：PoseSpirit。
            PoseSpirit(1f - eased, elapsed, false);
            // 步骤 18：调用 SetSpiritLook：设置SpiritLook。
            SetSpiritLook(visibility);
            // 步骤 19：调用 SetMemorialGlow：设置MemorialGlow。
            SetMemorialGlow(Mathf.Sin(t * Mathf.PI));
            // 步骤 20：确认spirit灯光已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (spiritLight != null)
            {
                // 步骤 21：执行 spiritLight.transform.position = spirit.position + Vector3.up * .45f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                spiritLight.transform.position = spirit.position + Vector3.up * .45f;
                // 步骤 22：把计算结果 visibility * .35f + Mathf.Sin(t * Mathf.PI) * .45f 保存为intensity；下一条判断或调用会读取这个具体值。
                spiritLight.intensity = visibility * .35f + Mathf.Sin(t * Mathf.PI) * .45f;
            }
            // 步骤 23：调用 EmitBodyMotes：EmitBodyMotes。
            EmitBodyMotes(22f * Mathf.Sin(t * Mathf.PI));
            // 步骤 24：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        }
        // 步骤 25：调用 EmitMotes：EmitMotes。
        EmitMotes(target, 22, .18f);
        // 步骤 26：调用 SetRenderers：设置Renderers。
        SetRenderers(false);
        // 步骤 27：把计算结果 0f 保存为currentVisibility；下一条判断或调用会读取这个具体值。
        currentVisibility = 0f;
        // 步骤 28：把position更新为 finalPosition；世界或局部位置会立即影响物体摆放。
        spirit.position = finalPosition;
        // 步骤 29：把rotation更新为 finalRotation；物体朝向会在本帧改变。
        spirit.rotation = finalRotation;
        // 步骤 30：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        spirit.localScale = finalScale;
        // 步骤 31：调用 RestorePose：RestorePose。
        RestorePose();
        // 步骤 32：把计算结果 0f 保存为elapsed；下一条判断或调用会读取这个具体值。
        elapsed = 0f;
        // 步骤 33：只要判断条件仍为真就重复当前步骤；条件变假时退出循环。
        while (elapsed < .65f)
        {
            // 步骤 34：把每秒变化率乘本帧秒数，更新elapsed；帧率变化时速度仍接近一致。
            elapsed += Time.deltaTime;
            // 步骤 35：调用 SetMemorialGlow：设置MemorialGlow。
            SetMemorialGlow(Mathf.Lerp(.75f, 0f, elapsed / .65f));
            // 步骤 36：确认spirit灯光已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 spiritLight.intensity = Mathf.Lerp(.45f, 0f, elapsed / .65f);
            if (spiritLight != null) spiritLight.intensity = Mathf.Lerp(.45f, 0f, elapsed / .65f);
            // 步骤 37：执行 yield return null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            yield return null;
        }
        // 步骤 38：确认motes已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Stop：Stop。
        if (motes != null) motes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        // 步骤 39：调用 SetMemorialGlow：设置MemorialGlow。
        SetMemorialGlow(0f);
        // 步骤 40：把 transitioning 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        transitioning = false;
        // 步骤 41：把 visible 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        visible = false;
        // 步骤 42：清空 motionRoutine 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        motionRoutine = null;
    }
    // ！！！功能结束：ReturnToMemorial！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LateUpdate()
    {
        // 步骤 1：检查spirit是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!visible || transitioning || spirit == null) return;
        // 步骤 2：把每秒变化率乘本帧秒数，更新idleTime；帧率变化时速度仍接近一致。
        idleTime += Time.deltaTime;
        // 步骤 3：把position更新为 finalPosition + Vector3.up * (Mathf.Sin(idleTime * 1.25f) * .035f)；世界或局部位置会立即影响物体摆放。
        spirit.position = finalPosition + Vector3.up * (Mathf.Sin(idleTime * 1.25f) * .035f);
        // 步骤 4：把rotation更新为 finalRotation * Quaternion.Euler(0f, Mathf.Sin(idleTime * .45f) * 1.4f, 0f)；物体朝向会在本帧改变。
        spirit.rotation = finalRotation * Quaternion.Euler(0f, Mathf.Sin(idleTime * .45f) * 1.4f, 0f);
        // 步骤 5：恢复 spirit.localScale 到初始缩放，避免拿取或归位后尺寸变化。
        spirit.localScale = finalScale;
        // 步骤 6：调用 PoseSpirit：PoseSpirit。
        PoseSpirit(1f, idleTime, true);
        // 步骤 7：调用 SetSpiritLook：设置SpiritLook。
        SetSpiritLook(1f);
        // 步骤 8：调用 SetMemorialGlow：设置MemorialGlow。
        SetMemorialGlow(.1f + Mathf.Sin(idleTime * 1.1f) * .035f);
        // 步骤 9：确认spirit灯光已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (spiritLight != null)
        {
            // 步骤 10：执行 spiritLight.transform.position = spirit.position + Vector3.up * .8f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            spiritLight.transform.position = spirit.position + Vector3.up * .8f;
            // 步骤 11：把计算结果 .2f + Mathf.Sin(idleTime * 1.1f) * .035f 保存为intensity；下一条判断或调用会读取这个具体值。
            spiritLight.intensity = .2f + Mathf.Sin(idleTime * 1.1f) * .035f;
        }
        // 步骤 12：检查条件 Mathf.Repeat(idleTime, 1.35f) < Time.deltaTime；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 EmitMotes：EmitMotes。
        if (Mathf.Repeat(idleTime, 1.35f) < Time.deltaTime) EmitMotes(spirit.position + Vector3.up * .2f, 2, .035f);
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：StopMotion——StopMotion！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void StopMotion()
    {
        // 步骤 1：确认motionRoutine已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 StopCoroutine：StopCoroutine。
        if (motionRoutine != null) StopCoroutine(motionRoutine);
        // 步骤 2：清空 motionRoutine 对对象的引用；后续访问前需要重新获取或检查是否为 null。
        motionRoutine = null;
    }
    // ！！！功能结束：StopMotion！！！

    // ！！！功能：MemorialLightPoint——Memorial灯光Point！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Vector3 类型的计算结果；执行到 return 时把该值交回调用者。
    private Vector3 MemorialLightPoint()
    {
        // 步骤 1：检查memorial是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (memorial == null) return finalPosition + Vector3.up * 1.3f;
        // 步骤 2：计算 null，把结果存进 fallback（fallback）；之后引用 fallback 时用的就是这次结果。
        Transform fallback = null;
        // 步骤 3：逐个读取 Transform child in memorial.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in memorial.GetComponentsInChildren<Transform>(true))
        {
            // 步骤 4：计算 child.name.ToLowerInvariant()，把结果存进 n（n）；之后引用 n 时用的就是这次结果。
            string n = child.name.ToLowerInvariant();
            // 步骤 5：这里用 || 表示任一条件成立就处理：n == "memorial star" || n == "memorial symbol"；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
            if (n == "memorial star" || n == "memorial symbol") return child.position;
            // 步骤 6：检查fallback是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 fallback = child;
            if (fallback == null && (n.Contains("star") || n.Contains("symbol"))) fallback = child;
        }
        // 步骤 7：把 fallback != null ? fallback.position : memorial.position + Vector3.up * 1.45f 作为方法结果返回给调用方；return 后本方法立即结束。
        return fallback != null ? fallback.position : memorial.position + Vector3.up * 1.45f;
    }
    // ！！！功能结束：MemorialLightPoint！！！

    // ！！！功能：CachePoseBones——CachePoseBones！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CachePoseBones()
    {
        // 步骤 1：从目标物体找到所需组件并存为 animator；使用前应检查是否为 null。
        Animator animator = spirit.GetComponentInChildren<Animator>(true);
        // 步骤 2：检查animator是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (animator != null && animator.isHuman)
        {
            // 步骤 3：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) 保存为leftArm；下一条判断或调用会读取这个具体值。
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            // 步骤 4：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightUpperArm) 保存为rightArm；下一条判断或调用会读取这个具体值。
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            // 步骤 5：把计算结果 animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) 保存为leftForearm；下一条判断或调用会读取这个具体值。
            leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            // 步骤 6：把计算结果 animator.GetBoneTransform(HumanBodyBones.RightLowerArm) 保存为rightForearm；下一条判断或调用会读取这个具体值。
            rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            // 步骤 7：把计算结果 animator.GetBoneTransform(HumanBodyBones.Head) 保存为headBone；下一条判断或调用会读取这个具体值。
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        // 步骤 8：把计算结果 FindAny("upperarm_l", "upper_arm_l", "leftarm", "left_arm", "arm_l", "hand_a_l") 保存为leftArm；下一条判断或调用会读取这个具体值。
        leftArm ??= FindAny("upperarm_l", "upper_arm_l", "leftarm", "left_arm", "arm_l", "hand_a_l");
        // 步骤 9：把计算结果 FindAny("upperarm_r", "upper_arm_r", "rightarm", "right_arm", "arm_r", "hand_a_right") 保存为rightArm；下一条判断或调用会读取这个具体值。
        rightArm ??= FindAny("upperarm_r", "upper_arm_r", "rightarm", "right_arm", "arm_r", "hand_a_right");
        // 步骤 10：把计算结果 FindAny("lowerarm_l", "forearm_l", "leftforearm", "left_forearm") 保存为leftForearm；下一条判断或调用会读取这个具体值。
        leftForearm ??= FindAny("lowerarm_l", "forearm_l", "leftforearm", "left_forearm");
        // 步骤 11：把计算结果 FindAny("lowerarm_r", "forearm_r", "rightforearm", "right_forearm") 保存为rightForearm；下一条判断或调用会读取这个具体值。
        rightForearm ??= FindAny("lowerarm_r", "forearm_r", "rightforearm", "right_forearm");
        // 步骤 12：把计算结果 FindAny("head", "head_bone", "neck") 保存为headBone；下一条判断或调用会读取这个具体值。
        headBone ??= FindAny("head", "head_bone", "neck");
        // 步骤 13：确认leftArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftArmRest = leftArm.localRotation;
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        // 步骤 14：确认rightArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightArmRest = rightArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        // 步骤 15：确认leftForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftForearmRest = leftForearm.localRotation;
        if (leftForearm != null) leftForearmRest = leftForearm.localRotation;
        // 步骤 16：确认rightForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightForearmRest = rightForearm.localRotation;
        if (rightForearm != null) rightForearmRest = rightForearm.localRotation;
        // 步骤 17：确认headBone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 headRest = headBone.localRotation;
        if (headBone != null) headRest = headBone.localRotation;
        // 步骤 18：逐个读取 Transform child in spirit.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in spirit.GetComponentsInChildren<Transform>(true))
        {
            // 步骤 19：这里用 || 表示任一条件成立就处理：child == spirit || clothParts.Count >= 8；用于合并几种停止或跳过原因。 成立时执行 continue;
            if (child == spirit || clothParts.Count >= 8) continue;
            // 步骤 20：计算 child.name.ToLowerInvariant()，把结果存进 n（n）；之后引用 n 时用的就是这次结果。
            string n = child.name.ToLowerInvariant();
            // 步骤 21：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (!n.Contains("cape") && !n.Contains("cloak") && !n.Contains("cloth") && !n.Contains("skirt") &&
                // 步骤 22：执行 !n.Contains("robe") && !n.Contains("dress") && !n.Contains("hem") && !n.Contains("doupeng") && !n.Co；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                !n.Contains("robe") && !n.Contains("dress") && !n.Contains("hem") && !n.Contains("doupeng") && !n.Contains("fabric")) continue;
            // 步骤 23：用 nested 记住此处真假结果（false），供下一条 if 选择执行路径。
            bool nested = false;
            // 步骤 24：逐个读取 Transform existing in clothParts 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (Transform existing in clothParts) if (child.IsChildOf(existing)) { nested = true; break; }
            // 步骤 25：检查条件 nested；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (nested) continue;
            // 步骤 26：把括号里的新元素加入 clothParts 集合；后续会按集合顺序使用它。
            clothParts.Add(child);
            // 步骤 27：把括号里的新元素加入 clothRestRotations 集合；后续会按集合顺序使用它。
            clothRestRotations.Add(child.localRotation);
        }
    }
    // ！！！功能结束：CachePoseBones！！！

    // ！！！功能：PoseSpirit——PoseSpirit！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float amount, float time, bool greeting 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PoseSpirit(float amount, float time, bool greeting)
    {
        // 步骤 1：计算 Mathf.Sin(time * 1.45f) * 2.2f * amount，把结果存进 breathe（breathe）；之后引用 breathe 时用的就是这次结果。
        float breathe = Mathf.Sin(time * 1.45f) * 2.2f * amount;
        // 步骤 2：用三元表达式为 wave 二选一：问号前条件为真取前值，为假取冒号后的值。
        float wave = greeting ? Mathf.Sin(time * 2.25f) * 8f : 0f;
        // 步骤 3：调用 PoseLocal：PoseLocal。
        PoseLocal(leftArm, leftArmRest, new Vector3(-12f - breathe, 0f, 18f) * amount);
        // 步骤 4：调用 PoseLocal：PoseLocal。
        PoseLocal(rightArm, rightArmRest, new Vector3(greeting ? -54f - wave : -12f - breathe, 0f, greeting ? -48f : -18f) * amount);
        // 步骤 5：调用 PoseLocal：PoseLocal。
        PoseLocal(leftForearm, leftForearmRest, new Vector3(0f, 0f, -10f) * amount);
        // 步骤 6：调用 PoseLocal：PoseLocal。
        PoseLocal(rightForearm, rightForearmRest, new Vector3(greeting ? -28f + wave * .7f : 0f, 0f, greeting ? -18f : 10f) * amount);
        // 步骤 7：调用 PoseLocal：PoseLocal。
        PoseLocal(headBone, headRest, new Vector3(-3f * amount, greeting ? -4f : 0f, 0f));
        // 步骤 8：按 int i = 0; i < clothParts.Count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < clothParts.Count; i++)
        {
            // 步骤 9：计算 Mathf.Sin(time * (1.05f + i * .07f) + i * .65f)，把结果存进 flutter（flutter）；之后引用 flutter 时用的就是这次结果。
            float flutter = Mathf.Sin(time * (1.05f + i * .07f) + i * .65f);
            // 步骤 10：这一行补充 clothParts[i].localRotation = clothRestRotations[i] * Quaternion.Euler(，与相邻行合起来完成同一次计算或对象配置。
            clothParts[i].localRotation = clothRestRotations[i] * Quaternion.Euler(
                // 步骤 11：执行 flutter * 2.1f * amount, 0f, Mathf.Sin(time * .8f + i) * 1.6f * amount);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                flutter * 2.1f * amount, 0f, Mathf.Sin(time * .8f + i) * 1.6f * amount);
        }
    }
    // ！！！功能结束：PoseSpirit！！！

    // ！！！功能：PoseLocal——PoseLocal！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Transform bone, Quaternion rest, Vector3 euler 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void PoseLocal(Transform bone, Quaternion rest, Vector3 euler)
    {
        // 步骤 1：确认bone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 bone.localRotation = rest * Quaternion.Euler(euler);
        if (bone != null) bone.localRotation = rest * Quaternion.Euler(euler);
    }
    // ！！！功能结束：PoseLocal！！！

    // ！！！功能：RestorePose——RestorePose！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void RestorePose()
    {
        // 步骤 1：确认leftArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftArm.localRotation = leftArmRest;
        if (leftArm != null) leftArm.localRotation = leftArmRest;
        // 步骤 2：确认rightArm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightArm.localRotation = rightArmRest;
        if (rightArm != null) rightArm.localRotation = rightArmRest;
        // 步骤 3：确认leftForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 leftForearm.localRotation = leftForearmRest;
        if (leftForearm != null) leftForearm.localRotation = leftForearmRest;
        // 步骤 4：确认rightForearm已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 rightForearm.localRotation = rightForearmRest;
        if (rightForearm != null) rightForearm.localRotation = rightForearmRest;
        // 步骤 5：确认headBone已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 headBone.localRotation = headRest;
        if (headBone != null) headBone.localRotation = headRest;
        // 步骤 6：按 int i = 0; i < clothParts.Count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < clothParts.Count; i++) if (clothParts[i] != null) clothParts[i].localRotation = clothRestRotations[i];
    }
    // ！！！功能结束：RestorePose！！！

    // ！！！功能：PrepareSpiritMaterials——PrepareSpirit材质！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PrepareSpiritMaterials()
    {
        // 步骤 1：检查spiritRenderers是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (spiritRenderers == null) return;
        // 步骤 2：新建用于spirit材质的对象或数值结构，并把构造参数保存进去。
        spiritMaterials = new Material[spiritRenderers.Length][];
        // 步骤 3：新建用于spiritBaseColors的对象或数值结构，并把构造参数保存进去。
        spiritBaseColors = new Color[spiritRenderers.Length][];
        // 步骤 4：按 int i = 0; i < spiritRenderers.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < spiritRenderers.Length; i++)
        {
            // 步骤 5：计算 spiritRenderers[i]，把结果存进 renderer（renderer）；之后引用 renderer 时用的就是这次结果。
            Renderer renderer = spiritRenderers[i];
            // 步骤 6：检查renderer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (renderer == null) continue;
            // 步骤 7：计算 renderer.materials，把结果存进 copies（copies）；之后引用 copies 时用的就是这次结果。
            Material[] copies = renderer.materials;
            // 步骤 8：执行 spiritMaterials[i] = copies;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            spiritMaterials[i] = copies;
            // 步骤 9：执行 spiritBaseColors[i] = new Color[copies.Length];；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            spiritBaseColors[i] = new Color[copies.Length];
            // 步骤 10：按 int j = 0; j < copies.Length; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < copies.Length; j++)
            {
                // 步骤 11：计算 copies[j]，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
                Material material = copies[j];
                // 步骤 12：检查material是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
                if (material == null) continue;
                // 步骤 13：执行 spiritBaseColors[i][j] = ReadColor(material);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                spiritBaseColors[i][j] = ReadColor(material);
                // 步骤 14：调用 ConfigureTransparent：配置Transparent。
                ConfigureTransparent(material);
                // 步骤 15：调用 EnableKeyword：EnableKeyword。
                material.EnableKeyword("_EMISSION");
            }
            // 步骤 16：把计算结果 copies 保存为materials；下一条判断或调用会读取这个具体值。
            renderer.materials = copies;
        }
    }
    // ！！！功能结束：PrepareSpiritMaterials！！！

    // ！！！功能：PrepareMemorialEffects——PrepareMemorialEffects！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PrepareMemorialEffects()
    {
        // 步骤 1：检查memorial是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (memorial == null) return;
        // 步骤 2：逐个读取 Renderer renderer in memorial.GetComponentsInChildren<Renderer>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Renderer renderer in memorial.GetComponentsInChildren<Renderer>(true))
        {
            // 步骤 3：计算 renderer.name.ToLowerInvariant()，把结果存进 n（n）；之后引用 n 时用的就是这次结果。
            string n = renderer.name.ToLowerInvariant();
            // 步骤 4：这里用 && 要求两边条件同时成立：!n.Contains("line") && !n.Contains("star") && !n.Contains("border") && !n.Contains("symbol") && !n.Contains("r；任意一边为假就跳过本分支。 成立时执行 continue;
            if (!n.Contains("line") && !n.Contains("star") && !n.Contains("border") && !n.Contains("symbol") && !n.Contains("ray")) continue;
            // 步骤 5：把括号里的新元素加入 memorialMarks 集合；后续会按集合顺序使用它。
            memorialMarks.Add(renderer);
            // 步骤 6：计算 renderer.material，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
            Material material = renderer.material;
            // 步骤 7：把括号里的新元素加入 memorialBaseColors 集合；后续会按集合顺序使用它。
            memorialBaseColors.Add(ReadColor(material));
            // 步骤 8：调用 EnableKeyword：EnableKeyword。
            material.EnableKeyword("_EMISSION");
        }
        // 步骤 9：创建新的 GameObject 并存为 lightObject；构造参数决定其初始内容或对象名称。
        GameObject lightObject = new GameObject("Soft white ancestor light");
        // 步骤 10：执行 lightObject.transform.SetParent(memorial, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        lightObject.transform.SetParent(memorial, false);
        // 步骤 11：把计算结果 lightObject.AddComponent<Light>() 保存为spirit灯光；下一条判断或调用会读取这个具体值。
        spiritLight = lightObject.AddComponent<Light>();
        // 步骤 12：把计算结果 LightType.Point 保存为type；下一条判断或调用会读取这个具体值。
        spiritLight.type = LightType.Point;
        // 步骤 13：新建用于color的对象或数值结构，并把构造参数保存进去。
        spiritLight.color = new Color(1f, .9f, .62f);
        // 步骤 14：把计算结果 2.8f 保存为range；下一条判断或调用会读取这个具体值。
        spiritLight.range = 2.8f;
        // 步骤 15：把计算结果 0f 保存为intensity；下一条判断或调用会读取这个具体值。
        spiritLight.intensity = 0f;
        // 步骤 16：把计算结果 LightShadows.None 保存为shadows；下一条判断或调用会读取这个具体值。
        spiritLight.shadows = LightShadows.None;

        // 步骤 17：创建新的 GameObject 并存为 particleObject；构造参数决定其初始内容或对象名称。
        GameObject particleObject = new GameObject("White memorial motes");
        // 步骤 18：执行 particleObject.transform.SetParent(memorial, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        particleObject.transform.SetParent(memorial, false);
        // 步骤 19：把计算结果 particleObject.AddComponent<ParticleSystem>() 保存为motes；下一条判断或调用会读取这个具体值。
        motes = particleObject.AddComponent<ParticleSystem>();
        // 步骤 20：调用 Stop：Stop。
        motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 步骤 21：计算 motes.main，把结果存进 main（main）；之后引用 main 时用的就是这次结果。
        ParticleSystem.MainModule main = motes.main;
        // 步骤 22：把 main.loop 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        main.loop = false;
        // 步骤 23：把计算结果 1f 保存为duration；下一条判断或调用会读取这个具体值。
        main.duration = 1f;
        // 步骤 24：新建用于startLifetime的对象或数值结构，并把构造参数保存进去。
        main.startLifetime = new ParticleSystem.MinMaxCurve(.45f, 1.15f);
        // 步骤 25：新建用于startSpeed的对象或数值结构，并把构造参数保存进去。
        main.startSpeed = new ParticleSystem.MinMaxCurve(.04f, .18f);
        // 步骤 26：新建用于startSize的对象或数值结构，并把构造参数保存进去。
        main.startSize = new ParticleSystem.MinMaxCurve(.012f, .034f);
        // 步骤 27：新建用于startColor的对象或数值结构，并把构造参数保存进去。
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, .94f, .72f, .82f));
        // 步骤 28：把计算结果 260 保存为maxParticles；下一条判断或调用会读取这个具体值。
        main.maxParticles = 260;
        // 步骤 29：把计算结果 ParticleSystemSimulationSpace.World 保存为simulationSpace；下一条判断或调用会读取这个具体值。
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        // 步骤 30：计算 motes.emission，把结果存进 emission（emission）；之后引用 emission 时用的就是这次结果。
        ParticleSystem.EmissionModule emission = motes.emission;
        // 步骤 31：把计算结果 0f 保存为rateOverTime；下一条判断或调用会读取这个具体值。
        emission.rateOverTime = 0f;
        // 步骤 32：计算 motes.shape，把结果存进 shape（shape）；之后引用 shape 时用的就是这次结果。
        ParticleSystem.ShapeModule shape = motes.shape;
        // 步骤 33：把 shape.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        shape.enabled = false;
        // 步骤 34：计算 motes.colorOverLifetime，把结果存进 colorLife（colorLife）；之后引用 colorLife 时用的就是这次结果。
        ParticleSystem.ColorOverLifetimeModule colorLife = motes.colorOverLifetime;
        // 步骤 35：把 colorLife.enabled 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        colorLife.enabled = true;
        // 步骤 36：创建新的 Gradient 并存为 gradient；构造参数决定其初始内容或对象名称。
        Gradient gradient = new Gradient();
        // 步骤 37：调用 SetKeys：设置Keys。
        gradient.SetKeys(
            // 步骤 38：继续填写上一行调用的参数：new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, .95f, .76f), 1f) }；参数位置决定它表示的坐标、速度或选项。
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, .95f, .76f), 1f) },
            // 步骤 39：执行 new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.95f, .16f), new GradientAlphaKey(0f, 1f)；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.95f, .16f), new GradientAlphaKey(0f, 1f) });
        // 步骤 40：把计算结果 gradient 保存为color；下一条判断或调用会读取这个具体值。
        colorLife.color = gradient;
        // 步骤 41：从目标物体找到所需组件并存为 particleRenderer；使用前应检查是否为 null。
        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        // 步骤 42：把计算结果 ParticleSystemRenderMode.Billboard 保存为render模式；下一条判断或调用会读取这个具体值。
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        // 步骤 43：计算 Shader.Find("Universal Render Pipeline/Particles/Unlit")，把结果存进 shader（shader）；之后引用 shader 时用的就是这次结果。
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        // 步骤 44：检查shader是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 shader = Shader.Find("Particles/Standard Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        // 步骤 45：确认shader已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (shader != null)
        {
            // 步骤 46：创建新的 Material 并存为 material；构造参数决定其初始内容或对象名称。
            Material material = new Material(shader);
            // 步骤 47：把计算结果 CreateSoftMoteTexture() 保存为mote贴图；下一条判断或调用会读取这个具体值。
            moteTexture = CreateSoftMoteTexture();
            // 步骤 48：检查条件 material.HasProperty("_BaseMap")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetTexture：设置贴图。
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", moteTexture);
            // 步骤 49：检查条件 material.HasProperty("_MainTex")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetTexture：设置贴图。
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", moteTexture);
            // 步骤 50：调用 WriteColor：WriteColor。
            WriteColor(material, Color.white);
            // 步骤 51：调用 ConfigureTransparent：配置Transparent。
            ConfigureTransparent(material);
            // 步骤 52：检查条件 material.HasProperty("_DstBlend")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.One);
            // 步骤 53：把计算结果 material 保存为material；下一条判断或调用会读取这个具体值。
            particleRenderer.material = material;
        }
    }
    // ！！！功能结束：PrepareMemorialEffects！！！

    // ！！！功能：CreateSoftMoteTexture——创建SoftMote贴图！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 Texture2D 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Texture2D CreateSoftMoteTexture()
    {
        // 步骤 1：创建新的 Texture2D 并存为 texture；构造参数决定其初始内容或对象名称。
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        // 步骤 2：把计算结果 "Soft ancestor mote" 保存为name；下一条判断或调用会读取这个具体值。
        texture.name = "Soft ancestor mote";
        // 步骤 3：把计算结果 TextureWrapMode.Clamp 保存为wrap模式；下一条判断或调用会读取这个具体值。
        texture.wrapMode = TextureWrapMode.Clamp;
        // 步骤 4：把计算结果 FilterMode.Bilinear 保存为filter模式；下一条判断或调用会读取这个具体值。
        texture.filterMode = FilterMode.Bilinear;
        // 步骤 5：按 int y = 0; y < 32; y++ 变化索引；循环体依次处理每个位置。
        for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
        {
            // 步骤 6：计算 (x + .5f) / 16f - 1f，把结果存进 dx（dx）；之后引用 dx 时用的就是这次结果。
            float dx = (x + .5f) / 16f - 1f;
            // 步骤 7：计算 (y + .5f) / 16f - 1f，把结果存进 dy（dy）；之后引用 dy 时用的就是这次结果。
            float dy = (y + .5f) / 16f - 1f;
            // 步骤 8：计算 alpha 并限制在安全上下界，防止不合理的大小影响移动或显示。
            float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
            // 步骤 9：调用 SetPixel：设置Pixel。
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
        }
        // 步骤 10：调用 Apply：应用。
        texture.Apply();
        // 步骤 11：把 texture 作为方法结果返回给调用方；return 后本方法立即结束。
        return texture;
    }
    // ！！！功能结束：CreateSoftMoteTexture！！！

    // ！！！功能：EmitMotes——EmitMotes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 position, int count, float speed 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EmitMotes(Vector3 position, int count, float speed)
    {
        // 步骤 1：检查motes是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (motes == null || count <= 0) return;
        // 步骤 2：按 int i = 0; i < count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < count; i++)
        {
            // 步骤 3：计算 Random.onUnitSphere，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
            Vector3 direction = Random.onUnitSphere;
            // 步骤 4：把计算结果 Mathf.Abs(direction.y) * .75f + .15f 保存为y；下一条判断或调用会读取这个具体值。
            direction.y = Mathf.Abs(direction.y) * .75f + .15f;
            // 步骤 5：创建新的 ParticleSystem.EmitParams 并存为 emit；构造参数决定其初始内容或对象名称。
            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams();
            // 步骤 6：把position更新为 position + Random.insideUnitSphere * .13f；世界或局部位置会立即影响物体摆放。
            emit.position = position + Random.insideUnitSphere * .13f;
            // 步骤 7：把计算结果 direction.normalized * Random.Range(speed * .45f, speed) 保存为velocity；下一条判断或调用会读取这个具体值。
            emit.velocity = direction.normalized * Random.Range(speed * .45f, speed);
            // 步骤 8：把计算结果 Random.Range(.012f, .034f) 保存为startSize；下一条判断或调用会读取这个具体值。
            emit.startSize = Random.Range(.012f, .034f);
            // 步骤 9：新建用于startColor的对象或数值结构，并把构造参数保存进去。
            emit.startColor = Color.Lerp(Color.white, new Color(1f, .94f, .72f, .88f), Random.value * .32f);
            // 步骤 10：调用 Emit：Emit。
            motes.Emit(emit, 1);
        }
    }
    // ！！！功能结束：EmitMotes！！！

    // ！！！功能：EmitMotes——EmitMotes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 position, float rate, float speed 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EmitMotes(Vector3 position, float rate, float speed)
    {
        // 步骤 1：把每秒变化率乘本帧秒数，更新particleCarry；帧率变化时速度仍接近一致。
        particleCarry += rate * Time.deltaTime;
        // 步骤 2：计算 Mathf.FloorToInt(particleCarry)，把结果存进 count（count）；之后引用 count 时用的就是这次结果。
        int count = Mathf.FloorToInt(particleCarry);
        // 步骤 3：在particleCarry原值上减少 count；旧状态与本次输入共同决定新状态。
        particleCarry -= count;
        // 步骤 4：调用 EmitMotes：EmitMotes。
        EmitMotes(position, count, speed);
    }
    // ！！！功能结束：EmitMotes！！！

    // ！！！功能：EmitBodyMotes——EmitBodyMotes！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float rate 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EmitBodyMotes(float rate)
    {
        // 步骤 1：检查motes、spiritRenderers是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (motes == null || spiritRenderers == null || spiritRenderers.Length == 0) return;
        // 步骤 2：把每秒变化率乘本帧秒数，更新particleCarry；帧率变化时速度仍接近一致。
        particleCarry += rate * Time.deltaTime;
        // 步骤 3：计算 Mathf.FloorToInt(particleCarry)，把结果存进 count（count）；之后引用 count 时用的就是这次结果。
        int count = Mathf.FloorToInt(particleCarry);
        // 步骤 4：在particleCarry原值上减少 count；旧状态与本次输入共同决定新状态。
        particleCarry -= count;
        // 步骤 5：检查条件 count <= 0；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (count <= 0) return;
        // 步骤 6：检查particleBake是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 particleBake = new Mesh();
        if (particleBake == null) particleBake = new Mesh();
        // 步骤 7：按 int n = 0; n < count; n++ 变化索引；循环体依次处理每个位置。
        for (int n = 0; n < count; n++)
        {
            // 步骤 8：计算 spiritRenderers[Random.Range(0, spiritRenderers.Length)]，把结果存进 renderer（renderer）；之后引用 renderer 时用的就是这次结果。
            Renderer renderer = spiritRenderers[Random.Range(0, spiritRenderers.Length)];
            // 步骤 9：检查renderer是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (renderer == null || !renderer.enabled) continue;
            // 步骤 10：计算 renderer.bounds.center + Vector3.Scale(Random.insideUnitSphere, renderer.bounds.extents)，把结果存进 point（point）；之后引用 point 时用的就是这次结果。
            Vector3 point = renderer.bounds.center + Vector3.Scale(Random.insideUnitSphere, renderer.bounds.extents);
            // 步骤 11：检查条件 renderer is SkinnedMeshRenderer skin；只有为真才运行下面的步骤，否则沿原流程继续。
            if (renderer is SkinnedMeshRenderer skin)
            {
                // 步骤 12：调用 BakeMesh：Bake网格。
                skin.BakeMesh(particleBake);
                // 步骤 13：计算 particleBake.vertices，把结果存进 vertices（vertices）；之后引用 vertices 时用的就是这次结果。
                Vector3[] vertices = particleBake.vertices;
                // 步骤 14：检查条件 vertices.Length > 0；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 point = renderer.transform.TransformPoint(vertices[Random.Range(0, vertices
                if (vertices.Length > 0) point = renderer.transform.TransformPoint(vertices[Random.Range(0, vertices.Length)]);
            }
            // 步骤 15：调用 EmitMotes：EmitMotes。
            EmitMotes(point, 1, .09f);
        }
    }
    // ！！！功能结束：EmitBodyMotes！！！

    // ！！！功能：SetSpiritLook——设置SpiritLook！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float visibility 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetSpiritLook(float visibility)
    {
        // 步骤 1：检查spirit材质、spiritBaseColors是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (spiritMaterials == null || spiritBaseColors == null) return;
        // 步骤 2：创建新的 Color 并存为 warmGold；构造参数决定其初始内容或对象名称。
        Color warmGold = new Color(1f, .9f, .66f, 1f);
        // 步骤 3：创建新的 Color 并存为 softWhite；构造参数决定其初始内容或对象名称。
        Color softWhite = new Color(1f, .985f, .93f, 1f);
        // 步骤 4：按 int i = 0; i < spiritMaterials.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < spiritMaterials.Length; i++)
        {
            // 步骤 5：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (spiritMaterials[i] == null || spiritBaseColors[i] == null) continue;
            // 步骤 6：按 int j = 0; j < spiritMaterials[i].Length; j++ 变化索引；循环体依次处理每个位置。
            for (int j = 0; j < spiritMaterials[i].Length; j++)
            {
                // 步骤 7：计算 spiritMaterials[i][j]，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
                Material material = spiritMaterials[i][j];
                // 步骤 8：检查material是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                if (material == null || j >= spiritBaseColors[i].Length) continue;
                // 步骤 9：计算 spiritBaseColors[i][j]，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
                Color source = spiritBaseColors[i][j];
                // 步骤 10：计算 tint 并限制在安全上下界，防止不合理的大小影响移动或显示。
                Color tint = Color.Lerp(warmGold, softWhite, Mathf.Clamp01(source.grayscale * .72f));
                // 步骤 11：计算 Color.Lerp(source, tint, .9f)，把结果存进 styled（styled）；之后引用 styled 时用的就是这次结果。
                Color styled = Color.Lerp(source, tint, .9f);
                // 步骤 12：把颜色的 alpha 设为 1（完全不透明），避免透视背景穿过场景画面。
                styled.a = Mathf.Clamp01(visibility) * .78f;
                // 步骤 13：调用 WriteColor：WriteColor。
                WriteColor(material, styled);
                // 步骤 14：检查条件 material.HasProperty("_EmissionColor")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetColor：设置Color。
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", tint * (.18f + visibility * .28f));
            }
        }
    }
    // ！！！功能结束：SetSpiritLook！！！

    // ！！！功能：SetMemorialGlow——设置MemorialGlow！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 float amount 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetMemorialGlow(float amount)
    {
        // 步骤 1：计算amount并用 Clamp 限定上下界；超出范围的输入会被压回安全区间。
        amount = Mathf.Clamp01(amount);
        // 步骤 2：按 int i = 0; i < memorialMarks.Count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < memorialMarks.Count; i++)
        {
            // 步骤 3：计算 memorialMarks[i]，把结果存进 renderer（renderer）；之后引用 renderer 时用的就是这次结果。
            Renderer renderer = memorialMarks[i];
            // 步骤 4：检查renderer是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (renderer == null) continue;
            // 步骤 5：计算 renderer.material，把结果存进 material（material）；之后引用 material 时用的就是这次结果。
            Material material = renderer.material;
            // 步骤 6：用三元表达式为 baseColor 二选一：问号前条件为真取前值，为假取冒号后的值。
            Color baseColor = i < memorialBaseColors.Count ? memorialBaseColors[i] : Color.white;
            // 步骤 7：调用 WriteColor：WriteColor。
            WriteColor(material, Color.Lerp(baseColor, new Color(1f, .96f, .75f), amount));
            // 步骤 8：检查条件 material.HasProperty("_EmissionColor")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetColor：设置Color。
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.white * (amount * 1.7f));
        }
    }
    // ！！！功能结束：SetMemorialGlow！！！

    // ！！！功能：EnsureSpiritCache——确保SpiritCache！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EnsureSpiritCache()
    {
        // 步骤 1：检查spirit是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (spirit == null) return;
        // 步骤 2：检查spiritRenderers是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 spiritRenderers = spirit.GetComponentsInChildren<Renderer>(true);
        if (spiritRenderers == null) spiritRenderers = spirit.GetComponentsInChildren<Renderer>(true);
        // 步骤 3：检查rendererStates是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (rendererStates == null || rendererStates.Length != spiritRenderers.Length)
        {
            // 步骤 4：新建用于rendererStates的对象或数值结构，并把构造参数保存进去。
            rendererStates = new bool[spiritRenderers.Length];
            // 步骤 5：按 int i = 0; i < spiritRenderers.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < spiritRenderers.Length; i++) rendererStates[i] = spiritRenderers[i] != null && spiritRenderers[i].enabled;
        }
        // 步骤 6：检查spirit材质、spiritBaseColors是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时调用 PrepareSpiritMaterials：PrepareSpirit材质。
        if (spiritMaterials == null || spiritBaseColors == null) PrepareSpiritMaterials();
    }
    // ！！！功能结束：EnsureSpiritCache！！！

    // ！！！功能：SetRenderers——设置Renderers！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool enabled 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SetRenderers(bool enabled)
    {
        // 步骤 1：检查spiritRenderers、rendererStates是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (spiritRenderers == null || rendererStates == null) return;
        // 步骤 2：按 int i = 0; i < spiritRenderers.Length && i < rendererStates.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < spiritRenderers.Length && i < rendererStates.Length; i++)
            // 步骤 3：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 spiritRenderers[i].enabled = enabled && rendererStates[i];
            if (spiritRenderers[i] != null) spiritRenderers[i].enabled = enabled && rendererStates[i];
    }
    // ！！！功能结束：SetRenderers！！！

    // ！！！功能：FindAny——查找Any！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 params string[] names 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Transform 类型的计算结果；执行到 return 时把该值交回调用者。
    private Transform FindAny(params string[] names)
    {
        // 步骤 1：逐个读取 Transform child in spirit.GetComponentsInChildren<Transform>(true) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform child in spirit.GetComponentsInChildren<Transform>(true))
        {
            // 步骤 2：计算 child.name.ToLowerInvariant()，把结果存进 candidate（candidate）；之后引用 candidate 时用的就是这次结果。
            string candidate = child.name.ToLowerInvariant();
            // 步骤 3：逐个读取 string name in names 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (string name in names) if (candidate == name || candidate.Contains(name)) return child;
        }
        // 步骤 4：没有找到有效结果，返回 null；调用方要检查后才能继续使用。
        return null;
    }
    // ！！！功能结束：FindAny！！！

    // 这一行补充 private static float EaseOutCubic(float t) { t = 1f - Mathf.Clamp01(t); return 1f - t * t * t; }，与相邻行合起来完成同一次计算或对象配置。
    private static float EaseOutCubic(float t) { t = 1f - Mathf.Clamp01(t); return 1f - t * t * t; }
    // ！！！功能：ReadColor——ReadColor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 Color 类型的计算结果；执行到 return 时把该值交回调用者。
    private static Color ReadColor(Material material)
    {
        // 步骤 1：检查条件 material.HasProperty("_BaseColor")；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        // 步骤 2：检查条件 material.HasProperty("_Color")；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (material.HasProperty("_Color")) return material.GetColor("_Color");
        // 步骤 3：把 Color.white 作为方法结果返回给调用方；return 后本方法立即结束。
        return Color.white;
    }
    // ！！！功能结束：ReadColor！！！
    // ！！！功能：WriteColor——WriteColor！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Material material, Color color 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void WriteColor(Material material, Color color)
    {
        // 步骤 1：检查条件 material.HasProperty("_BaseColor")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetColor：设置Color。
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        // 步骤 2：检查条件 material.HasProperty("_Color")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetColor：设置Color。
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }
    // ！！！功能结束：WriteColor！！！
    // ！！！功能：ConfigureTransparent——配置Transparent！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Material material 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ConfigureTransparent(Material material)
    {
        // 步骤 1：检查条件 material.HasProperty("_Surface")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        // 步骤 2：检查条件 material.HasProperty("_SrcBlend")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        // 步骤 3：检查条件 material.HasProperty("_DstBlend")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        // 步骤 4：检查条件 material.HasProperty("_ZWrite")；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetFloat：设置Float。
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        // 步骤 5：调用 EnableKeyword：EnableKeyword。
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        // 步骤 6：调用 DisableKeyword：DisableKeyword。
        material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        // 步骤 7：把计算结果 (int)RenderQueue.Transparent 保存为renderQueue；下一条判断或调用会读取这个具体值。
        material.renderQueue = (int)RenderQueue.Transparent;
    }
    // ！！！功能结束：ConfigureTransparent！！！
    // ！！！功能：OnDestroy——对象销毁时释放资源或取消订阅！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void OnDestroy()
    {
        // 步骤 1：确认particleBake已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (particleBake != null) Destroy(particleBake);
        // 步骤 2：确认mote贴图已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时调用 Destroy：Destroy。
        if (moteTexture != null) Destroy(moteTexture);
    }
    // ！！！功能结束：OnDestroy！！！
}
