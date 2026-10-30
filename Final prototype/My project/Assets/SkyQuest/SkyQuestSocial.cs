// ！！！文件功能：社交树、爱心、蜡烛与玩家招呼动作！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// 定义 SkyQuestSocial 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestSocial : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 leftHand（左手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform leftHand;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // 声明 ancestor（ancestor）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform ancestor;
    // itemTree 保存itemTree（类型 SkyQuestAncestor）；下方方法依靠它记录或查找场景状态。
    public SkyQuestAncestor itemTree;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // 把 Inspector 可调整的 interactionDistance（允许靠近先祖的距离）设为默认值 3.2f；具体用途看它在下方的判断和计算。
    public float interactionDistance = 3.2f;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;

    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public bool HeartShared { get; private set; }
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public bool CandleOffered { get; private set; }
    // 这是属性声明；外部可以读取，但 private set 限制只有本类内部能修改。
    public bool AncestorStoryUnlocked { get; private set; }

    // socialLabel 保存socialLabel（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro socialLabel;
    // 声明 heartTimer（heartTimer）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float heartTimer;
    // 声明 candleTimer（candleTimer）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float candleTimer;
    // 声明 waveTimer（waveTimer）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float waveTimer;
    // 声明 waveChanges（waveChanges）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int waveChanges;
    // 声明 lastWaveDirection（lastWaveDirection）：整数计数；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private int lastWaveDirection;
    // 声明 previousRight（previousRight）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 previousRight;
    // 声明 previousStoryTrigger（previous故事Trigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousStoryTrigger;
    // 声明 storyRoot（storyRoot）：场景位置与朝向引用；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Transform storyRoot;
    // 这一行补充 private readonly string[] storyLines =，与相邻行合起来完成同一次计算或对象配置。
    private readonly string[] storyLines =
    {
        // 继续填写上一行调用的参数："The Ancestor followed a lantern through the wind."；参数位置决定它表示的坐标、速度或选项。
        "The Ancestor followed a lantern through the wind.",
        // 继续填写上一行调用的参数："A shared light helped friends cross the dark sky."；参数位置决定它表示的坐标、速度或选项。
        "A shared light helped friends cross the dark sky.",
        // 这一行补充 "Their journey lives on in the gifts of this tree."，与相邻行合起来完成同一次计算或对象配置。
        "Their journey lives on in the gifts of this tree."
    };

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查ancestor是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (ancestor == null) return;
        // 步骤 2：把计算结果 SkyQuestVisuals.Text("Social prompt", "", ancestor, 保存为socialLabel；下一条判断或调用会读取这个具体值。
        socialLabel = SkyQuestVisuals.Text("Social prompt", "", ancestor,
            // 步骤 3：用 new 创建 Vector3(0f, 2.7f, 0f), 2.4f, new Color(1f, 0.8f, 0.7f), 2.7f);；构造参数决定新对象的初始内容。
            new Vector3(0f, 2.7f, 0f), 2.4f, new Color(1f, 0.8f, 0.7f), 2.7f);
        // 步骤 4：确认右手柄位置已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 previousRight = rightHand.position;
        if (rightHand != null) previousRight = rightHand.position;
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查头显相机、左手柄位置、右手柄位置、ancestor是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (head == null || leftHand == null || rightHand == null || ancestor == null) return;
        // 步骤 2：把计算结果 Vector3.Distance(desktopMode ? transform.position + Vector3.up * 1.2f : head.position, 保存为near；下一条判断或调用会读取这个具体值。
        bool near = Vector3.Distance(desktopMode ? transform.position + Vector3.up * 1.2f : head.position,
            // 步骤 3：执行 ancestor.position) <= interactionDistance;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ancestor.position) <= interactionDistance;
        // 步骤 4：确认socialLabel已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (socialLabel != null)
        {
            // 步骤 5：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            socialLabel.gameObject.SetActive(near && (itemTree == null || !itemTree.IsMenuOpen));
            // 步骤 6：把新内容写入 socialLabel.text；玩家看到的文字会在本帧更新。
            socialLabel.text = desktopMode ?
                // 步骤 7：这一行补充 (!HeartShared ? "H: share heart-light" : !CandleOffered ? "C: offer candle" :，与相邻行合起来完成同一次计算或对象配置。
                (!HeartShared ? "H: share heart-light" : !CandleOffered ? "C: offer candle" :
                 // 步骤 8：这一行补充 !AncestorStoryUnlocked ? "G: greet the Ancestor" : "7-9: read stories") :，与相邻行合起来完成同一次计算或对象配置。
                 !AncestorStoryUnlocked ? "G: greet the Ancestor" : "7-9: read stories") :
                // 步骤 9：这一行补充 (!HeartShared ? "Bring both hands to your chest and hold grips to share heart-light" :，与相邻行合起来完成同一次计算或对象配置。
                (!HeartShared ? "Bring both hands to your chest and hold grips to share heart-light" :
                 // 步骤 10：这一行补充 !CandleOffered ? "Hold right trigger toward the Ancestor to offer a candle" :，与相邻行合起来完成同一次计算或对象配置。
                 !CandleOffered ? "Hold right trigger toward the Ancestor to offer a candle" :
                 // 步骤 11：这一行补充 !AncestorStoryUnlocked ? "Wave your right hand to unlock the story" :，与相邻行合起来完成同一次计算或对象配置。
                 !AncestorStoryUnlocked ? "Wave your right hand to unlock the story" :
                 // 步骤 12：执行 "Friendship complete — open the Ancestor item tree");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                 "Friendship complete — open the Ancestor item tree");
            // 步骤 13：执行 socialLabel.transform.rotation = Quaternion.LookRotation(socialLabel.transform.position - head.posit；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            socialLabel.transform.rotation = Quaternion.LookRotation(socialLabel.transform.position - head.position);
        }
        // 步骤 14：检查itemTree是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (!near || (itemTree != null && itemTree.IsMenuOpen))
        {
            // 步骤 15：把计算结果 candleTimer = waveTimer = 0f 保存为heartTimer；下一条判断或调用会读取这个具体值。
            heartTimer = candleTimer = waveTimer = 0f;
            // 步骤 16：把计算结果 rightHand.position 保存为previousRight；下一条判断或调用会读取这个具体值。
            previousRight = rightHand.position;
            // 步骤 17：提前结束这个方法；不再执行下方语句。
            return;
        }
        // 步骤 18：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。 条件成立时立即退出当前方法。
        if (desktopMode) return;

        // 步骤 19：计算 head.position - Vector3.up * 0.3f，把结果存进 chest（chest）；之后引用 chest 时用的就是这次结果。
        Vector3 chest = head.position - Vector3.up * 0.3f;
        // 步骤 20：把计算结果 Vector3.Distance(leftHand.position, chest) < 0.38f && 保存为handsAtChest；下一条判断或调用会读取这个具体值。
        bool handsAtChest = Vector3.Distance(leftHand.position, chest) < 0.38f &&
                            // 步骤 21：调用 Distance：Distance。
                            Vector3.Distance(rightHand.position, chest) < 0.38f;
        // 步骤 22：这里的 ! 表示取反；当 HeartShared 不成立时才进入此分支。
        if (!HeartShared)
        {
            // 步骤 23：把计算结果 handsAtChest && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.7f && 保存为heartTimer；下一条判断或调用会读取这个具体值。
            heartTimer = handsAtChest && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.7f &&
                // 步骤 24：调用 Grip：Grip。
                SkyQuestInput.Grip(SkyQuestInput.Right) > 0.7f ? heartTimer + Time.deltaTime : 0f;
            // 步骤 25：检查条件 heartTimer >= 0.6f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (heartTimer >= 0.6f)
            {
                // 步骤 26：把 HeartShared 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                HeartShared = true;
                // 步骤 27：调用 SpawnHeartLight：SpawnHeart灯光。
                SpawnHeartLight(chest);
                // 步骤 28：执行 status?.Show("Heart-light shared. Friendship begun.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show("Heart-light shared. Friendship begun.", 4f);
                // 步骤 29：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Left);
                // 步骤 30：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right);
            }
            // 步骤 31：把计算结果 rightHand.position 保存为previousRight；下一条判断或调用会读取这个具体值。
            previousRight = rightHand.position;
            // 步骤 32：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 33：这里的 ! 表示取反；当 CandleOffered 不成立时才进入此分支。
        if (!CandleOffered)
        {
            // 步骤 34：计算 (ancestor.position + Vector3.up * 1.2f - rightHand.position).normalized，把结果存进 toAncestor（toAncestor）；之后引用 toAncestor 时用的就是这次结果。
            Vector3 toAncestor = (ancestor.position + Vector3.up * 1.2f - rightHand.position).normalized;
            // 步骤 35：用 pointing 记住此处真假结果（Vector3.Dot(rightHand.forward, toAncestor) > 0.7f），供下一条 if 选择执行路径。
            bool pointing = Vector3.Dot(rightHand.forward, toAncestor) > 0.7f;
            // 步骤 36：把计算结果 pointing && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.75f 保存为candleTimer；下一条判断或调用会读取这个具体值。
            candleTimer = pointing && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.75f
                // 步骤 37：三元表达式中，前面的条件为真时选这个值。
                ? candleTimer + Time.deltaTime : 0f;
            // 步骤 38：检查条件 candleTimer >= 0.7f；只有为真才运行下面的步骤，否则沿原流程继续。
            if (candleTimer >= 0.7f)
            {
                // 步骤 39：把 CandleOffered 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                CandleOffered = true;
                // 步骤 40：调用 SpawnCandle：SpawnCandle。
                SpawnCandle();
                // 步骤 41：执行 status?.Show("White candle offered. Wave to hear the Ancestor's story.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show("White candle offered. Wave to hear the Ancestor's story.", 4f);
                // 步骤 42：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.5f, 0.12f);
            }
            // 步骤 43：把计算结果 rightHand.position 保存为previousRight；下一条判断或调用会读取这个具体值。
            previousRight = rightHand.position;
            // 步骤 44：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 45：这里的 ! 表示取反；当 AncestorStoryUnlocked 不成立时才进入此分支。
        if (!AncestorStoryUnlocked)
        {
            // 步骤 46：计算 (rightHand.position.x - previousRight.x) / Mathf.Max(Time.deltaTime, 0.01f)，把结果存进 velocityX（velocityx）；之后引用 velocityX 时用的就是这次结果。
            float velocityX = (rightHand.position.x - previousRight.x) / Mathf.Max(Time.deltaTime, 0.01f);
            // 步骤 47：用三元表达式为 direction 二选一：问号前条件为真取前值，为假取冒号后的值。
            int direction = velocityX > 0.45f ? 1 : velocityX < -0.45f ? -1 : 0;
            // 步骤 48：这里用 && 要求两边条件同时成立：direction != 0 && direction != lastWaveDirection；任意一边为假就跳过本分支。
            if (direction != 0 && direction != lastWaveDirection)
            {
                // 步骤 49：执行 waveChanges++;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                waveChanges++;
                // 步骤 50：把计算结果 direction 保存为lastWaveDirection；下一条判断或调用会读取这个具体值。
                lastWaveDirection = direction;
                // 步骤 51：根据当前游戏时间计算并保存waveTimer，用于后续延迟或冷却判断。
                waveTimer = Time.time;
            }
            // 步骤 52：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。 成立时执行 { waveChanges = 0; lastWaveDirection = 0; }
            if (Time.time - waveTimer > 2.2f) { waveChanges = 0; lastWaveDirection = 0; }
            // 步骤 53：检查条件 waveChanges >= 4；只有为真才运行下面的步骤，否则沿原流程继续。
            if (waveChanges >= 4)
            {
                // 步骤 54：把 AncestorStoryUnlocked 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                AncestorStoryUnlocked = true;
                // 步骤 55：执行 status?.Show("Ancestor story unlocked. Point to a star and press right trigger.", 6f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show("Ancestor story unlocked. Point to a star and press right trigger.", 6f);
                // 步骤 56：调用 CreateStoryTree：创建故事Tree。
                CreateStoryTree();
                // 步骤 57：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.65f, 0.2f);
            }
        }
        // 步骤 58：检查条件 AncestorStoryUnlocked；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 UpdateStorySelection：更新故事Selection。
        if (AncestorStoryUnlocked) UpdateStorySelection();
        // 步骤 59：把计算结果 rightHand.position 保存为previousRight；下一条判断或调用会读取这个具体值。
        previousRight = rightHand.position;
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：CreateStoryTree——创建故事Tree！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateStoryTree()
    {
        // 步骤 1：新建用于storyRoot的对象或数值结构，并把构造参数保存进去。
        storyRoot = new GameObject("Ancestor narrative tree").transform;
        // 步骤 2：把position更新为 ancestor.position + Vector3.up * 2.5f；世界或局部位置会立即影响物体摆放。
        storyRoot.position = ancestor.position + Vector3.up * 2.5f;
        // 步骤 3：按 int i = 0; i < storyLines.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < storyLines.Length; i++)
        {
            // 步骤 4：计算 (i - 1) * 0.45f，把结果存进 x（x）；之后引用 x 时用的就是这次结果。
            float x = (i - 1) * 0.45f;
            // 步骤 5：把计算结果 SkyQuestVisuals.Primitive("Story star " + (i + 1), 保存为star；下一条判断或调用会读取这个具体值。
            GameObject star = SkyQuestVisuals.Primitive("Story star " + (i + 1),
                // 步骤 6：继续填写上一行调用的参数：PrimitiveType.Sphere, storyRoot, new Vector3(x, 0.28f, 0f)；参数位置决定它表示的坐标、速度或选项。
                PrimitiveType.Sphere, storyRoot, new Vector3(x, 0.28f, 0f),
                // 步骤 7：执行 Vector3.one * 0.15f, new Color(1f, 0.84f, 0.35f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                Vector3.one * 0.15f, new Color(1f, 0.84f, 0.35f), true);
            // 步骤 8：执行 star.AddComponent<SkyQuestStoryNode>().index = i;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            star.AddComponent<SkyQuestStoryNode>().index = i;
            // 步骤 9：调用 Text：Text。
            SkyQuestVisuals.Text("Story label", "STORY " + (i + 1), storyRoot,
                // 步骤 10：用 new 创建 Vector3(x, 0f, 0f), 2f, Color.white, 0.4f);；构造参数决定新对象的初始内容。
                new Vector3(x, 0f, 0f), 2f, Color.white, 0.4f);
        }
    }
    // ！！！功能结束：CreateStoryTree！！！

    // ！！！功能：UpdateStorySelection——更新故事Selection！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateStorySelection()
    {
        // 步骤 1：用 trigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f），供下一条 if 选择执行路径。
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        // 步骤 2：这里用 && 要求两边条件同时成立：trigger && !previousStoryTrigger；任意一边为假就跳过本分支。
        if (trigger && !previousStoryTrigger)
        {
            // 步骤 3：计算 null，把结果存进 nearest（nearest）；之后引用 nearest 时用的就是这次结果。
            SkyQuestStoryNode nearest = null;
            // 步骤 4：计算 float.PositiveInfinity，把结果存进 distance（distance）；之后引用 distance 时用的就是这次结果。
            float distance = float.PositiveInfinity;
            // 步骤 5：逐个读取 RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 4f) 中的元素；每轮处理当前一个，直到全部遍历完。
            foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 4f))
            {
                // 步骤 6：从目标物体找到所需组件并存为 node；使用前应检查是否为 null。
                SkyQuestStoryNode node = hit.collider.GetComponent<SkyQuestStoryNode>();
                // 步骤 7：检查node是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
                if (node == null || hit.distance >= distance) continue;
                // 步骤 8：把计算结果 node 保存为nearest；下一条判断或调用会读取这个具体值。
                nearest = node;
                // 步骤 9：把计算结果 hit.distance 保存为distance；下一条判断或调用会读取这个具体值。
                distance = hit.distance;
            }
            // 步骤 10：确认nearest已经有值；只有存在时才能安全读取它的属性或调用方法。
            if (nearest != null)
            {
                // 步骤 11：执行 status?.Show(storyLines[nearest.index], 7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                status?.Show(storyLines[nearest.index], 7f);
                // 步骤 12：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.35f, 0.08f);
            }
        }
        // 步骤 13：把计算结果 trigger 保存为previous故事Trigger；下一条判断或调用会读取这个具体值。
        previousStoryTrigger = trigger;
    }
    // ！！！功能结束：UpdateStorySelection！！！

    // ！！！功能：SpawnHeartLight——SpawnHeart灯光！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 chest 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SpawnHeartLight(Vector3 chest)
    {
        // 步骤 1：把计算结果 SkyQuestVisuals.Primitive("Shared heart-light", PrimitiveType.Sphere, 保存为heart；下一条判断或调用会读取这个具体值。
        GameObject heart = SkyQuestVisuals.Primitive("Shared heart-light", PrimitiveType.Sphere,
            // 步骤 2：执行 transform, Vector3.zero, Vector3.one * 0.14f, new Color(1f, 0.35f, 0.55f), true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            transform, Vector3.zero, Vector3.one * 0.14f, new Color(1f, 0.35f, 0.55f), true);
        // 步骤 3：执行 heart.transform.position = Vector3.Lerp(chest, ancestor.position + Vector3.up * 1.4f, 0.5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        heart.transform.position = Vector3.Lerp(chest, ancestor.position + Vector3.up * 1.4f, 0.5f);
        // 步骤 4：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(heart);
        // 步骤 5：调用 Destroy：Destroy。
        Destroy(heart, 2f);
    }
    // ！！！功能结束：SpawnHeartLight！！！

    // ！！！功能：SpawnCandle——SpawnCandle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void SpawnCandle()
    {
        // 步骤 1：把计算结果 SkyQuestVisuals.Primitive("Friendship candle", PrimitiveType.Cylinder, 保存为candle；下一条判断或调用会读取这个具体值。
        GameObject candle = SkyQuestVisuals.Primitive("Friendship candle", PrimitiveType.Cylinder,
            // 步骤 2：执行 ancestor, new Vector3(0.4f, 1.15f, 0.35f), new Vector3(0.04f, 0.2f, 0.04f), Color.white);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            ancestor, new Vector3(0.4f, 1.15f, 0.35f), new Vector3(0.04f, 0.2f, 0.04f), Color.white);
        // 步骤 3：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(candle);
        // 步骤 4：把计算结果 SkyQuestVisuals.Primitive("Candle flame", PrimitiveType.Sphere, 保存为flame；下一条判断或调用会读取这个具体值。
        GameObject flame = SkyQuestVisuals.Primitive("Candle flame", PrimitiveType.Sphere,
            // 步骤 5：继续填写上一行调用的参数：candle.transform, new Vector3(0f, 0.65f, 0f), Vector3.one * 0.65f；参数位置决定它表示的坐标、速度或选项。
            candle.transform, new Vector3(0f, 0.65f, 0f), Vector3.one * 0.65f,
            // 步骤 6：用 new 创建 Color(1f, 0.65f, 0.17f), true);；构造参数决定新对象的初始内容。
            new Color(1f, 0.65f, 0.17f), true);
        // 步骤 7：调用 RemoveCollider：移除只用于展示模型的碰撞体，避免挡住玩家或手柄射线。
        SkyQuestVisuals.RemoveCollider(flame);
    }
    // ！！！功能结束：SpawnCandle！！！

    // ！！！功能：DesktopShareHeart——桌面ShareHeart！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopShareHeart()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || HeartShared || !DesktopNear()；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || HeartShared || !DesktopNear()) return;
        // 步骤 2：把 HeartShared 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        HeartShared = true;
        // 步骤 3：调用 SpawnHeartLight：SpawnHeart灯光。
        SpawnHeartLight(head.position - Vector3.up * 0.3f);
        // 步骤 4：执行 status?.Show("Heart-light shared. Press C to offer a candle.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Heart-light shared. Press C to offer a candle.", 5f);
    }
    // ！！！功能结束：DesktopShareHeart！！！

    // ！！！功能：DesktopOfferCandle——桌面OfferCandle！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopOfferCandle()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || CandleOffered || !HeartShared || !DesktopNear()；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || CandleOffered || !HeartShared || !DesktopNear()) return;
        // 步骤 2：把 CandleOffered 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        CandleOffered = true;
        // 步骤 3：调用 SpawnCandle：SpawnCandle。
        SpawnCandle();
        // 步骤 4：执行 status?.Show("Candle offered. Press G to greet the Ancestor.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Candle offered. Press G to greet the Ancestor.", 5f);
    }
    // ！！！功能结束：DesktopOfferCandle！！！

    // ！！！功能：DesktopGreet——桌面Greet！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopGreet()
    {
        // 步骤 1：这里用 || 表示任一条件成立就处理：!desktopMode || AncestorStoryUnlocked || !CandleOffered || !DesktopNear()；用于合并几种停止或跳过原因。 条件成立时立即退出当前方法。
        if (!desktopMode || AncestorStoryUnlocked || !CandleOffered || !DesktopNear()) return;
        // 步骤 2：把 AncestorStoryUnlocked 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        AncestorStoryUnlocked = true;
        // 步骤 3：调用 CreateStoryTree：创建故事Tree。
        CreateStoryTree();
        // 步骤 4：执行 status?.Show("Ancestor story unlocked. Press 7, 8 or 9 to read.", 5f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Ancestor story unlocked. Press 7, 8 or 9 to read.", 5f);
    }
    // ！！！功能结束：DesktopGreet！！！

    // ！！！功能：DesktopReadStory——桌面Read故事！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 int index 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void DesktopReadStory(int index)
    {
        // 步骤 1：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!desktopMode || !AncestorStoryUnlocked || !DesktopNear() ||
            // 步骤 2：执行 index < 0 || index >= storyLines.Length) return;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            index < 0 || index >= storyLines.Length) return;
        // 步骤 3：执行 status?.Show(storyLines[index], 7f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show(storyLines[index], 7f);
    }
    // ！！！功能结束：DesktopReadStory！！！

    // ！！！功能：DesktopNear——桌面Near！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：返回 bool 类型的计算结果；执行到 return 时把该值交回调用者。
    private bool DesktopNear()
    {
        // 步骤 1：把 ancestor != null && 作为方法结果返回给调用方；return 后本方法立即结束。
        return ancestor != null &&
            // 步骤 2：调用 Distance：Distance。
            Vector3.Distance(transform.position + Vector3.up * 1.2f, ancestor.position) <= interactionDistance;
    }
    // ！！！功能结束：DesktopNear！！！
}

// 定义 SkyQuestStoryNode 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestStoryNode : MonoBehaviour
{
    // 声明 index（index）：整数计数；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public int index;
}
