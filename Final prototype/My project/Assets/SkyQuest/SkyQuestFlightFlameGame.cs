// ！！！文件功能：飞行竞技玩法的开始、收集、结算和重置！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// The course is one scene object shared by the keyboard and Quest versions.
// 定义 SkyQuestFlightFlameGame 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public sealed class SkyQuestFlightFlameGame : MonoBehaviour
{
    // player 保存player（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    public SkyQuestLocomotion player;
    // 声明 startMarker（startMarker）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform startMarker;
    // flames 保存flames（类型 SkyQuestFlightFlame[]）；下方方法依靠它记录或查找场景状态。
    public SkyQuestFlightFlame[] flames;
    // 把 Inspector 可调整的 roundSeconds（roundSeconds）设为默认值 90f；具体用途看它在下方的判断和计算。
    public float roundSeconds = 90f;
    // 把 Inspector 可调整的 pointsPerFlame（pointsPerFlame）设为默认值 10；具体用途看它在下方的判断和计算。
    public int pointsPerFlame = 10;

    // 这一行补充 private enum RoundState { Ready, Flying, Finished }，与相邻行合起来完成同一次计算或对象配置。
    private enum RoundState { Ready, Flying, Finished }
    // state 保存state（类型 RoundState）；下方方法依靠它记录或查找场景状态。
    private RoundState state;
    // energy 保存energy（类型 CapeEnergySystem）；下方方法依靠它记录或查找场景状态。
    private CapeEnergySystem energy;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    private SkyQuestStatus status;
    // vrBoard 保存vrBoard（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro vrBoard;
    private Transform vrBoardRoot;
    private static AudioClip flameCollectClip;
    private static Material flameBurstMaterial;
    private GUIStyle topHudStyle;
    // 声明 vrBoardDefaultScale（vrBoardDefault缩放）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 vrBoardDefaultScale;
    // 声明 collected（collected）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int collected;
    // 声明 score（score）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int score;
    // 声明 bestScore（bestScore）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int bestScore;
    // 声明 remaining（remaining）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float remaining;
    // 声明 restartAllowedAt（restartAllowedAt）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float restartAllowedAt;
    // 声明 wasAtStart（wasAtStart）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool wasAtStart;
    // 声明 won（won）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool won;
    // collectedFlames 保存collectedFlames（类型 bool[]）；下方方法依靠它记录或查找场景状态。
    private bool[] collectedFlames;
    // headingStyle 保存headingStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle headingStyle;
    // bodyStyle 保存bodyStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle bodyStyle;
    // smallStyle 保存smallStyle（类型 GUIStyle）；下方方法依靠它记录或查找场景状态。
    private GUIStyle smallStyle;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Start()
    {
        // 步骤 1：把计算结果 PlayerPrefs.GetInt("SkyQuestFlightFlamesBest", 0) 保存为bestScore；下一条判断或调用会读取这个具体值。
        bestScore = PlayerPrefs.GetInt("SkyQuestFlightFlamesBest", 0);
        // 步骤 2：新建用于collectedFlames的对象或数值结构，并把构造参数保存进去。
        collectedFlames = new bool[flames != null ? flames.Length : 0];
        // 步骤 3：调用 ResolvePlayer：Resolve玩家。
        ResolvePlayer();
        // 步骤 4：调用 ResetFlames：复位Flames。
        ResetFlames();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Update()
    {
        // 步骤 1：检查player是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时调用 ResolvePlayer：Resolve玩家。
        if (player == null || !player.isActiveAndEnabled) ResolvePlayer();
        // 步骤 2：检查player、startMarker、flames是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (player == null || startMarker == null || flames == null) return;
        // 步骤 3：调用 PlaceVrBoard：PlaceVrBoard。
        PlaceVrBoard();

        // 步骤 4：把计算结果 player.desktopControlled ? player.transform.position + Vector3.up * 1.1f : 保存为body；下一条判断或调用会读取这个具体值。
        Vector3 body = player.desktopControlled ? player.transform.position + Vector3.up * 1.1f :
            // 步骤 5：执行 player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;
        // 步骤 6：计算 body - startMarker.position，把结果存进 startDelta（startDelta）；之后引用 startDelta 时用的就是这次结果。
        Vector3 startDelta = body - startMarker.position;
        // 步骤 7：新建用于atStart的对象或数值结构，并把构造参数保存进去。
        bool atStart = new Vector2(startDelta.x, startDelta.z).sqrMagnitude < 2.25f &&
            // 步骤 8：调用 Abs：Abs。
            Mathf.Abs(startDelta.y) < 2.5f;
        // 步骤 9：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (atStart && !wasAtStart && state != RoundState.Flying &&
            // 步骤 10：执行 Time.time >= restartAllowedAt) BeginRound();；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Time.time >= restartAllowedAt) BeginRound();
        // 步骤 11：把计算结果 atStart 保存为wasAtStart；下一条判断或调用会读取这个具体值。
        wasAtStart = atStart;

        // 步骤 12：检查条件 state != RoundState.Flying；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (state != RoundState.Flying) return;
        // 步骤 13：把每秒变化率乘本帧秒数，更新remaining；帧率变化时速度仍接近一致。
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        // 步骤 14：按 int i = 0; i < flames.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < flames.Length; i++)
        {
            // 步骤 15：计算 flames[i]，把结果存进 flame（flame）；之后引用 flame 时用的就是这次结果。
            SkyQuestFlightFlame flame = flames[i];
            // 步骤 16：检查flame是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (flame == null || collectedFlames[i]) continue;
            // 步骤 17：检查条件 (body - flame.transform.position).sqrMagnitude > flame.collectRadius * flame.collectRadius；只有为真才运行下面的步骤，否则沿原流程继续。
            // Count contact with the avatar's body, not only the headset/camera point.
            Vector3 feet = player.transform.position + Vector3.up * .35f;
            Vector3 crown = player.desktopControlled ?
                player.transform.position + Vector3.up * 1.8f : body;
            Vector3 span = crown - feet;
            float fraction = span.sqrMagnitude > .0001f ?
                Mathf.Clamp01(Vector3.Dot(flame.transform.position - feet, span) / span.sqrMagnitude) : 0f;
            Vector3 nearestBodyPoint = feet + span * fraction;
            if ((nearestBodyPoint - flame.transform.position).sqrMagnitude >
                Mathf.Min(flame.collectRadius, .72f) * Mathf.Min(flame.collectRadius, .72f)) continue;
            // 步骤 19：调用 Collect：Collect。
            Collect(i);
            // 步骤 20：检查条件 state != RoundState.Flying；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 break;
            if (state != RoundState.Flying) break;
        }
        // 步骤 21：这里用 && 要求两边条件同时成立：state == RoundState.Flying && remaining <= 0f；任意一边为假就跳过本分支。 成立时调用 EndRound：EndRound。
        if (state == RoundState.Flying && remaining <= 0f) EndRound(false);
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：ResolvePlayer——Resolve玩家！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ResolvePlayer()
    {
        // 步骤 1：计算 Camera.main，把结果存进 main（main）；之后引用 main 时用的就是这次结果。
        Camera main = Camera.main;
        // 步骤 2：从目标物体找到所需组件并存为 found；使用前应检查是否为 null。
        SkyQuestLocomotion found = main != null ? main.GetComponentInParent<SkyQuestLocomotion>() : null;
        // 步骤 3：检查found是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (found == null)
        {
            // 步骤 4：逐个读取 SkyQuestLocomotion candidate in FindObjectsByType<SkyQuestLocomotion>(FindObjectsSort 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (SkyQuestLocomotion candidate in FindObjectsByType<SkyQuestLocomotion>(FindObjectsSortMode.None))
                // 步骤 5：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
                if (candidate.isActiveAndEnabled && candidate.head != null &&
                    // 步骤 6：这一行补充 candidate.head.gameObject.activeInHierarchy)，与相邻行合起来完成同一次计算或对象配置。
                    candidate.head.gameObject.activeInHierarchy)
                // 步骤 7：这一行补充 { found = candidate; break; }，与相邻行合起来完成同一次计算或对象配置。
                { found = candidate; break; }
        }
        // 步骤 8：检查found是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (found == null) return;
        // 步骤 9：把计算结果 found 保存为player；下一条判断或调用会读取这个具体值。
        player = found;
        // 步骤 10：从目标 GameObject 查找并保存 energy 组件；后面的移动或显示逻辑会直接使用这个缓存。
        energy = player.GetComponent<CapeEnergySystem>();
        // 步骤 11：从目标 GameObject 查找并保存 status 组件；后面的移动或显示逻辑会直接使用这个缓存。
        status = player.GetComponent<SkyQuestStatus>();
        // 步骤 12：检查vrBoard、player的头显相机是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (!player.desktopControlled && vrBoard == null && player.head != null)
        {
            vrBoardRoot = new GameObject("Flight countdown and live score").transform;
            vrBoardRoot.SetParent(player.head, false);
            vrBoardRoot.localPosition = new Vector3(0f, .15f, 1.45f);
            GameObject backing = SkyQuestVisuals.Primitive("Translucent yellow countdown plate",
                PrimitiveType.Cube, vrBoardRoot, Vector3.zero,
                new Vector3(.78f, .2f, .006f), new Color(1f, .84f, .2f, .58f));
            SkyQuestVisuals.RemoveCollider(backing);
            backing.GetComponent<Renderer>().sharedMaterial = CreateCountdownMaterial();
            vrBoard = SkyQuestVisuals.Text("Flight countdown and score", "", vrBoardRoot,
                new Vector3(0f, 0f, -.009f), 2.45f,
                new Color(.14f, .1f, .02f), 5.7f);
            vrBoard.rectTransform.sizeDelta = new Vector2(5.7f, 1.55f);
            vrBoard.textWrappingMode = TextWrappingModes.NoWrap;
            vrBoard.overflowMode = TextOverflowModes.Overflow;
            vrBoardDefaultScale = vrBoardRoot.localScale;
        }
    }
    // ！！！功能结束：ResolvePlayer！！！

    // ！！！功能：PlaceVrBoard——PlaceVrBoard！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void PlaceVrBoard()
    {
        // 步骤 1：检查vrBoard、player、player的头显相机是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (vrBoardRoot == null || player == null || player.head == null) return;
        bool thirdPerson = player.ThirdPerson;
        Transform parent = thirdPerson ? player.transform : player.head;
        if (vrBoardRoot.parent == parent) return;
        vrBoardRoot.SetParent(parent, false);
        vrBoardRoot.localPosition = thirdPerson ?
            new Vector3(0f, 2.32f, 2.15f) : new Vector3(0f, .15f, 1.45f);
        vrBoardRoot.localRotation = Quaternion.identity;
        vrBoardRoot.localScale = vrBoardDefaultScale * (thirdPerson ? 1.45f : 1f);
    }
    // ！！！功能结束：PlaceVrBoard！！！

    // ！！！功能：BeginRound——BeginRound！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void BeginRound()
    {
        // 步骤 1：把计算结果 RoundState.Flying 保存为state；下一条判断或调用会读取这个具体值。
        state = RoundState.Flying;
        // 步骤 2：把计算结果 roundSeconds 保存为remaining；下一条判断或调用会读取这个具体值。
        remaining = roundSeconds;
        // 步骤 3：把计算结果 score = 0 保存为collected；下一条判断或调用会读取这个具体值。
        collected = score = 0;
        // 步骤 4：把 won 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        won = false;
        // 步骤 5：调用 ResetFlames：复位Flames。
        ResetFlames();
        // 步骤 6：从目标 GameObject 查找并保存 energy 组件；后面的移动或显示逻辑会直接使用这个缓存。
        energy = player.GetComponent<CapeEnergySystem>();
        // 步骤 7：执行 energy?.RestoreEnergy(energy.maxEnergy);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        energy?.RestoreEnergy(energy.maxEnergy);
        // 步骤 8：执行 status?.Show("Flight started! Collect all " + flames.Length + " flames.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Flight started! Collect all " + flames.Length + " flames.", 5f);
    }
    // ！！！功能结束：BeginRound！！！

    // ！！！功能：Collect——Collect！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void Collect(int index)
    {
        // 步骤 1：执行 collectedFlames[index] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        collectedFlames[index] = true;
        // 步骤 2：执行 collected++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        collected++;
        // 步骤 3：在score原值上增加 pointsPerFlame；旧状态与本次输入共同决定新状态。
        score += pointsPerFlame;
        Vector3 collectPosition = flames[index].transform.position;
        PlayCollectFeedback(collectPosition);
        flames[index].PlayCollection();
        // 步骤 6：执行 status?.Show("Flame " + collected + "/" + flames.Length + "  +" + pointsPerFlame + " points", 2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Flame " + collected + "/" + flames.Length + "  +" + pointsPerFlame + " points", 2f);
        // 步骤 7：检查条件 collected >= flames.Length；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 EndRound：EndRound。
        if (collected >= flames.Length) EndRound(true);
    }
    // ！！！功能结束：Collect！！！

    // ！！！功能：EndRound——EndRound！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 bool completed 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void EndRound(bool completed)
    {
        // 步骤 1：检查条件 state != RoundState.Flying；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (state != RoundState.Flying) return;
        // 步骤 2：把计算结果 RoundState.Finished 保存为state；下一条判断或调用会读取这个具体值。
        state = RoundState.Finished;
        // 步骤 3：把计算结果 completed 保存为won；下一条判断或调用会读取这个具体值。
        won = completed;
        // 步骤 4：检查条件 completed；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 score += Mathf.CeilToInt(remaining) * 2;
        if (completed) score += Mathf.CeilToInt(remaining) * 2;
        // 步骤 5：检查条件 score > bestScore；只有为真才运行下面的步骤，否则沿原流程继续。
        if (score > bestScore)
        {
            // 步骤 6：把计算结果 score 保存为bestScore；下一条判断或调用会读取这个具体值。
            bestScore = score;
            // 步骤 7：调用 SetInt：设置Int。
            PlayerPrefs.SetInt("SkyQuestFlightFlamesBest", bestScore);
            // 步骤 8：调用 Save：保存。
            PlayerPrefs.Save();
        }
        // 步骤 9：从目标 GameObject 查找并保存 energy 组件；后面的移动或显示逻辑会直接使用这个缓存。
        energy = player != null ? player.GetComponent<CapeEnergySystem>() : null;
        // 步骤 10：执行 energy?.RestoreEnergy(energy.maxEnergy);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        energy?.RestoreEnergy(energy.maxEnergy);
        // 步骤 11：根据当前游戏时间计算并保存restartAllowedAt，用于后续延迟或冷却判断。
        restartAllowedAt = Time.time + 1f;
        // 步骤 12：这一行补充 status?.Show((completed ? "Flight complete!" : "Time up!") +，与相邻行合起来完成同一次计算或对象配置。
        status?.Show((completed ? "Flight complete!" : "Time up!") +
            // 步骤 13：执行 " Score " + score + ". Cape full.", 6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            " Score " + score + ". Cape full.", 6f);
    }
    // ！！！功能结束：EndRound！！！

    // ！！！功能：ResetFlames——复位Flames！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ResetFlames()
    {
        // 步骤 1：检查flames是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (flames == null) return;
        // 步骤 2：检查collectedFlames是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。
        if (collectedFlames == null || collectedFlames.Length != flames.Length)
            // 步骤 3：新建用于collectedFlames的对象或数值结构，并把构造参数保存进去。
            collectedFlames = new bool[flames.Length];
        // 步骤 4：按 int i = 0; i < flames.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < flames.Length; i++)
        {
            // 步骤 5：执行 collectedFlames[i] = false;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            collectedFlames[i] = false;
            // 步骤 6：确认对象引用已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 flames[i].gameObject.SetActive(true);
            if (flames[i] != null) flames[i].ResetCollection();
        }
    }
    // ！！！功能结束：ResetFlames！！！

    // ！！！功能：FlameMarks——FlameMarks！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private string FlameMarks()
    {
        // 步骤 1：计算 ""，把结果存进 marks（marks）；之后引用 marks 时用的就是这次结果。
        string marks = "";
        // 步骤 2：按 int i = 0; i < flames.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < flames.Length; i++)
            // 步骤 3：在marks原值上增加 collectedFlames != null && i < collectedFlames.Length && collectedFlames[i] ? "◆ " : "◇ "；旧状态与本次输入共同决定新状态。
            marks += collectedFlames != null && i < collectedFlames.Length && collectedFlames[i] ? "◆ " : "◇ ";
        // 步骤 4：把 marks 作为方法结果返回给调用方；return 后本方法立即结束。
        return marks;
    }
    // ！！！功能结束：FlameMarks！！！

    // ！！！功能：NearestFlameHint——NearestFlameHint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 string 类型的计算结果；执行到 return 时把该值交回调用者。
    private string NearestFlameHint()
    {
        // 步骤 1：检查player、flames是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (state != RoundState.Flying || player == null || flames == null) return "";
        // 步骤 2：把计算结果 player.desktopControlled ? player.transform.position + Vector3.up * 1.1f : 保存为origin；下一条判断或调用会读取这个具体值。
        Vector3 origin = player.desktopControlled ? player.transform.position + Vector3.up * 1.1f :
            // 步骤 3：执行 player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;
        // 步骤 4：用三元表达式为 facing 二选一：问号前条件为真取前值，为假取冒号后的值。
        Vector3 facing = player.head != null ? player.head.forward : player.transform.forward;
        // 步骤 5：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        facing.y = 0f;
        // 步骤 6：检查条件 facing.sqrMagnitude < .01f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 facing = player.transform.forward;
        if (facing.sqrMagnitude < .01f) facing = player.transform.forward;
        // 步骤 7：调用 Normalize：Normalize。
        facing.Normalize();
        // 步骤 8：计算 float.PositiveInfinity，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
        float nearest = float.PositiveInfinity;
        // 步骤 9：计算 Vector3.zero，把结果存进 target（target）；之后引用 target 时用的就是这次结果。
        Vector3 target = Vector3.zero;
        // 步骤 10：按 int i = 0; i < flames.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < flames.Length; i++)
        {
            // 步骤 11：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (flames[i] == null || collectedFlames[i]) continue;
            // 步骤 12：计算 (flames[i].transform.position - origin).sqrMagnitude，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
            float distance = (flames[i].transform.position - origin).sqrMagnitude;
            // 步骤 13：检查条件 distance >= nearest；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (distance >= nearest) continue;
            // 步骤 14：把计算结果 distance 保存为nearest；下一条判断或调用会读取这个具体值。
            nearest = distance;
            // 步骤 15：把计算结果 flames[i].transform.position 保存为target；下一条判断或调用会读取这个具体值。
            target = flames[i].transform.position;
        }
        // 步骤 16：检查条件 float.IsPositiveInfinity(nearest)；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (float.IsPositiveInfinity(nearest)) return "";
        // 步骤 17：计算 target - origin，把结果存进 direction（direction）；之后引用 direction 时用的就是这次结果。
        Vector3 direction = target - origin;
        // 步骤 18：把计算结果 0f 保存为y；下一条判断或调用会读取这个具体值。
        direction.y = 0f;
        // 步骤 19：调用 Normalize：Normalize。
        direction.Normalize();
        // 步骤 20：计算 Vector3.Dot(facing, direction)，把结果存进 ahead（ahead）；之后引用 ahead 时用的就是这次结果。
        float ahead = Vector3.Dot(facing, direction);
        // 步骤 21：通过叉积算出与原方向垂直的向量，存为 side 供左右移动使用。
        float side = Vector3.Dot(Vector3.Cross(Vector3.up, facing), direction);
        // 步骤 22：把计算结果 ahead > .72f ? "AHEAD" : ahead < -.72f ? "BEHIND" : 保存为bearing；下一条判断或调用会读取这个具体值。
        string bearing = ahead > .72f ? "AHEAD" : ahead < -.72f ? "BEHIND" :
            // 步骤 23：执行 side > 0f ? "RIGHT" : "LEFT";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            side > 0f ? "RIGHT" : "LEFT";
        // 步骤 24：计算 target.y - origin.y，把结果存进 height（height）；之后引用 height 时用的就是这次结果。
        float height = target.y - origin.y;
        // 步骤 25：把计算结果 height > 1f ? " UP " + Mathf.CeilToInt(height) + "m" : 保存为altitude；下一条判断或调用会读取这个具体值。
        string altitude = height > 1f ? " UP " + Mathf.CeilToInt(height) + "m" :
            // 步骤 26：执行 height < -1f ? " DOWN " + Mathf.CeilToInt(-height) + "m" : "";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            height < -1f ? " DOWN " + Mathf.CeilToInt(-height) + "m" : "";
        // 步骤 27：把 "Next " + Mathf.CeilToInt(Mathf.Sqrt(nearest)) + "m " + bearing + altitude 作为方法结果返回给调用方；return 后本方法立即结束。
        return "Next " + Mathf.CeilToInt(Mathf.Sqrt(nearest)) + "m " + bearing + altitude;
    }
    // ！！！功能结束：NearestFlameHint！！！

    // ！！！功能：LateUpdate——在其他组件完成本帧更新后修正视角或视觉对象！！！
    // 何时执行：Unity 在普通 Update 之后每帧调用；适合使用本帧稳定后的追踪姿态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void LateUpdate()
    {
        // 步骤 1：检查vrBoard是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (vrBoard == null) return;
        // 步骤 2：把新内容写入 vrBoard.text；玩家看到的文字会在本帧更新。
        int seconds = Mathf.CeilToInt(state == RoundState.Ready ? roundSeconds : remaining);
        string time = (seconds / 60).ToString("00") + ":" + (seconds % 60).ToString("00");
        vrBoard.text = (state == RoundState.Flying ? "TIME " + time :
            state == RoundState.Finished ? (won ? "COMPLETE" : "TIME UP") : "READY " + time) +
            "    SCORE " + score + "\nFLAMES " + collected + "/" +
            (flames?.Length ?? 0) + "    BEST " + bestScore;
    }
    // ！！！功能结束：LateUpdate！！！

    // ！！！功能：OnGUI——用 Unity 即时模式绘制屏幕文字、按钮或提示！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static Material CreateCountdownMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) shader = Shader.Find("Standard");
        Material material = new Material(shader);
        material.color = new Color(1f, .84f, .2f, .58f);
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_Blend")) material.SetFloat("_Blend", 0f);
        if (material.HasProperty("_SrcBlend"))
            material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend"))
            material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.SetOverrideTag("RenderType", "Transparent");
        material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        return material;
    }

    private static AudioClip CollectChime()
    {
        if (flameCollectClip != null) return flameCollectClip;
        const int rate = 24000;
        const float duration = .82f;
        int count = Mathf.RoundToInt(rate * duration);
        float[] samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / rate;
            float attack = Mathf.Clamp01(t / .018f);
            float decay = Mathf.Exp(-4.8f * t);
            float upper = t > .11f ? Mathf.Sin(2f * Mathf.PI * 1568f * (t - .11f)) *
                Mathf.Exp(-7f * (t - .11f)) : 0f;
            samples[i] = attack * decay * (
                .36f * Mathf.Sin(2f * Mathf.PI * 784f * t) +
                .24f * Mathf.Sin(2f * Mathf.PI * 1176f * t)) + .18f * upper;
        }
        flameCollectClip = AudioClip.Create("Flight flame collection chime", count, 1, rate, false);
        flameCollectClip.SetData(samples, 0);
        return flameCollectClip;
    }

    private static void PlayCollectFeedback(Vector3 position)
    {
        GameObject burst = new GameObject("Flight flame golden collection burst");
        burst.transform.position = position;
        ParticleSystem particles = burst.AddComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.duration = 1f;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = new ParticleSystem.MinMaxCurve(.55f, .95f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(.8f, 2.4f);
        main.startSize = new ParticleSystem.MinMaxCurve(.07f, .18f);
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, .63f, .12f), new Color(1f, 1f, .78f));
        main.gravityModifier = -.08f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        var emission = particles.emission;
        emission.rateOverTime = 0f;
        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = .18f;
        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            colorKeys = new[] { new GradientColorKey(Color.white, 0f),
                new GradientColorKey(new Color(1f, .66f, .18f), 1f) },
            alphaKeys = new[] { new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(.9f, .35f), new GradientAlphaKey(0f, 1f) }
        });
        if (flameBurstMaterial == null)
            flameBurstMaterial = Resources.Load<Material>("FlightFlameCollection");
        if (flameBurstMaterial != null)
            particles.GetComponent<ParticleSystemRenderer>().sharedMaterial = flameBurstMaterial;
        particles.Emit(52);
        particles.Emit(new ParticleSystem.EmitParams
            { startSize = .75f, velocity = Vector3.zero, startLifetime = .18f }, 1);
        AudioSource sound = burst.AddComponent<AudioSource>();
        sound.playOnAwake = false;
        sound.spatialBlend = .3f;
        sound.minDistance = 3f;
        sound.maxDistance = 25f;
        sound.volume = 1f;
        sound.pitch = Random.Range(.96f, 1.08f);
        sound.PlayOneShot(CollectChime());
        Object.Destroy(burst, 2f);
        SkyQuestInput.Pulse(SkyQuestInput.Right, .38f, .11f);
    }

    private void OnGUI()
    {
        // 步骤 1：检查player、flames是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (player == null || !player.desktopControlled || flames == null) return;
        if (topHudStyle == null)
        {
            topHudStyle = new GUIStyle(GUI.skin.label) { fontSize = 19,
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            topHudStyle.normal.textColor = new Color(.13f, .1f, .03f);
        }
        float topWidth = Mathf.Min(380f, Screen.width * .46f);
        Rect topPlate = new Rect((Screen.width - topWidth) * .5f, 155f, topWidth, 62f);
        Color oldColor = GUI.color;
        GUI.color = new Color(1f, .84f, .2f, .67f);
        GUI.DrawTexture(topPlate, Texture2D.whiteTexture);
        GUI.color = oldColor;
        int timeLeft = Mathf.CeilToInt(state == RoundState.Ready ? roundSeconds : remaining);
        string clock = (timeLeft / 60).ToString("00") + ":" + (timeLeft % 60).ToString("00");
        GUI.Label(new Rect(topPlate.x, topPlate.y + 2f, topWidth, 29f),
            (state == RoundState.Flying ? "TIME " + clock :
             state == RoundState.Finished ? (won ? "COMPLETE" : "TIME UP") : "READY " + clock), topHudStyle);
        GUI.Label(new Rect(topPlate.x, topPlate.y + 30f, topWidth, 28f),
            "SCORE " + score + "   ·   FLAMES " + collected + "/" + flames.Length,
            topHudStyle);
        if (headingStyle == null)
        {
            // 步骤 3：新建用于headingStyle的对象或数值结构，并把构造参数保存进去。
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 20,
                // 步骤 4：把计算结果 FontStyle.Bold, alignment = TextAnchor.MiddleCenter } 保存为fontStyle；下一条判断或调用会读取这个具体值。
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            // 步骤 5：执行 headingStyle.normal.textColor = new Color(1f, .9f, .62f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            headingStyle.normal.textColor = new Color(1f, .9f, .62f);
            // 步骤 6：新建用于bodyStyle的对象或数值结构，并把构造参数保存进去。
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
            // 步骤 7：执行 bodyStyle.normal.textColor = Color.white;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            bodyStyle.normal.textColor = Color.white;
            // 步骤 8：新建用于smallStyle的对象或数值结构，并把构造参数保存进去。
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter,
                // 步骤 9：把计算结果 true } 保存为wordWrap；下一条判断或调用会读取这个具体值。
                wordWrap = true };
            // 步骤 10：执行 smallStyle.normal.textColor = new Color(.86f, .96f, 1f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            smallStyle.normal.textColor = new Color(.86f, .96f, 1f);
        }
        // 步骤 11：计算 Mathf.Min(268f, Screen.width * .38f)，把结果存进 width（width）；之后引用 width 时用的就是这次结果。
        float width = Mathf.Min(268f, Screen.width * .38f);
        // 步骤 12：创建新的 Rect 并存为 panel；构造参数决定其初始内容或对象名称。
        Rect panel = new Rect(Screen.width - width - 15f, 100f, width, 210f);
        // 步骤 13：计算 GUI.color，把结果存进 previous（previous）；之后引用 previous 时用的就是这次结果。
        Color previous = GUI.color;
        // 步骤 14：新建用于color的对象或数值结构，并把构造参数保存进去。
        GUI.color = new Color(.04f, .1f, .16f, .82f);
        // 步骤 15：调用 DrawTexture：Draw贴图。
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        // 步骤 16：把计算结果 previous 保存为color；下一条判断或调用会读取这个具体值。
        GUI.color = previous;
        // 步骤 17：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 10f, 108f, width - 20f, 30f), "FLIGHT FLAMES", headingStyle);
        // 步骤 18：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 18f, 142f, width - 30f, 23f),
            // 步骤 19：执行 "Score " + score + "    Best " + bestScore, bodyStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            "Score " + score + "    Best " + bestScore, bodyStyle);
        // 步骤 20：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 18f, 167f, width - 30f, 23f),
            // 步骤 21：这一行补充 "Flames " + collected + "/" + flames.Length + "    " +，与相邻行合起来完成同一次计算或对象配置。
            "Flames " + collected + "/" + flames.Length + "    " +
            // 步骤 22：执行 (state == RoundState.Flying ? "Time " + Mathf.CeilToInt(remaining) + "s" : "Time --"), bodyStyle);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            (state == RoundState.Flying ? "Time " + Mathf.CeilToInt(remaining) + "s" : "Time --"), bodyStyle);
        // 步骤 23：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 8f, 194f, width - 16f, 26f), FlameMarks(), smallStyle);
        // 步骤 24：把计算结果 state == RoundState.Flying ? "Fly through the floating flames" : 保存为instruction；下一条判断或调用会读取这个具体值。
        string instruction = state == RoundState.Flying ? "Fly through the floating flames" :
            // 步骤 25：把计算结果 = RoundState.Finished ? 保存为state；下一条判断或调用会读取这个具体值。
            state == RoundState.Finished ?
                // 步骤 26：这一行补充 (won ? "Complete!" : "Time up!") + " Energy full · Return to start" :，与相邻行合起来完成同一次计算或对象配置。
                (won ? "Complete!" : "Time up!") + " Energy full · Return to start" :
                // 步骤 27：执行 "Touch the blue start ring to begin";；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Touch the blue start ring to begin";
        // 步骤 28：调用 Label：在指定 GUI 矩形区域显示说明文字。
        GUI.Label(new Rect(panel.x + 12f, 224f, width - 24f, 44f), instruction, smallStyle);
        // 步骤 29：检查条件 state == RoundState.Flying；只有为真才运行下面的步骤，否则沿原流程继续。
        if (state == RoundState.Flying)
            // 步骤 30：调用 Label：在指定 GUI 矩形区域显示说明文字。
            GUI.Label(new Rect(panel.x + 12f, 271f, width - 24f, 27f), NearestFlameHint(), smallStyle);
    }
    // ！！！功能结束：OnGUI！！！
}
