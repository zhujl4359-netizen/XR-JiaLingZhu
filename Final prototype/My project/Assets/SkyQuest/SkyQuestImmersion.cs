// ！！！文件功能：环境音、灯光、可触碰物件和舒适瞬移体验！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：public 字段可在 Unity Inspector 配置；MonoBehaviour 的 Awake/Update 等方法由 Unity 自动调用。
// 引入 System.Collections.Generic 命名空间；下方可直接使用List 列表、HashSet 集合和 Dictionary 字典。
using System.Collections.Generic;
// 引入 TMPro 命名空间；下方可直接使用TextMeshPro 文字组件。
using TMPro;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// Runtime immersion layer: positional audio, low-cost environmental motion,
// tactile lantern/candle interactions and a comfort-friendly optional teleport mode.
// 定义 SkyQuestImmersion 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestImmersion : MonoBehaviour
{
    // 声明 head（头显相机）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform head;
    // 声明 leftHand（左手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform leftHand;
    // 声明 rightHand（右手柄位置）：场景位置与朝向引用；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public Transform rightHand;
    // locomotion 保存locomotion（类型 SkyQuestLocomotion）；下方方法依靠它记录或查找场景状态。
    public SkyQuestLocomotion locomotion;
    // sanctuary 保存sanctuary（类型 SkyQuestSanctuary）；下方方法依靠它记录或查找场景状态。
    public SkyQuestSanctuary sanctuary;
    // status 保存status（类型 SkyQuestStatus）；下方方法依靠它记录或查找场景状态。
    public SkyQuestStatus status;
    // 声明 desktopMode（desktop模式）：真假开关；可在 Unity Inspector 指定，赋值前是该类型的默认值。
    public bool desktopMode;
    // enableProcedural音频是 Inspector 中的开关，默认 true；勾选状态决定相关功能是否运行。
    public bool enableProceduralAudio = true;
    // enablePhysicalInteractions是 Inspector 中的开关，默认 true；勾选状态决定相关功能是否运行。
    public bool enablePhysicalInteractions = true;

    // 创建新的 List<Transform> 并存为 swayObjects；构造参数决定其初始内容或对象名称。
    private readonly List<Transform> swayObjects = new List<Transform>();
    // 创建新的 List<Vector3> 并存为 swayBase；构造参数决定其初始内容或对象名称。
    private readonly List<Vector3> swayBase = new List<Vector3>();
    // 创建新的 List<GameObject> 并存为 lanterns；构造参数决定其初始内容或对象名称。
    private readonly List<GameObject> lanterns = new List<GameObject>();
    // 创建新的 List<GameObject> 并存为 candles；构造参数决定其初始内容或对象名称。
    private readonly List<GameObject> candles = new List<GameObject>();
    // 创建新的 List<bool> 并存为 candleLit；构造参数决定其初始内容或对象名称。
    private readonly List<bool> candleLit = new List<bool>();
    // windSource 保存windSource（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource windSource;
    // waterSource 保存waterSource（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource waterSource;
    // fireSource 保存fireSource（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource fireSource;
    // ancestorSource 保存ancestorSource（类型 AudioSource）；下方方法依靠它记录或查找场景状态。
    private AudioSource ancestorSource;
    // cloudParticles 保存cloudParticles（类型 ParticleSystem）；下方方法依靠它记录或查找场景状态。
    private ParticleSystem cloudParticles;
    // comfortHint 保存comfortHint（类型 TextMeshPro）；下方方法依靠它记录或查找场景状态。
    private TextMeshPro comfortHint;
    // 声明 previousLeftTrigger（previousLeftTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousLeftTrigger;
    // 声明 leftTriggerStarted（leftTriggerStarted）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float leftTriggerStarted;
    // 声明 leftTriggerStartedWithItem（leftTriggerStarted带有物品）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool leftTriggerStartedWithItem;
    // 声明 previousTeleportToggle（previousTeleportToggle）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousTeleportToggle;
    // 声明 previousCalibrate（previousCalibrate）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousCalibrate;
    // 声明 nextHaptic（nextHaptic）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextHaptic;
    // 声明 nextTeleport（nextTeleport）：小数数值；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private float nextTeleport;
    // 声明 gateStart（gateStart）：三维向量；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private Vector3 gateStart;
    // 声明 teleportMode（teleport模式）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool teleportMode;
    // 声明 previousTeleportTrigger（previousTeleportTrigger）：真假开关；只供本脚本内部保存状态，赋值前是该类型的默认值。
    private bool previousTeleportTrigger;

    // ！！！功能：Start——Unity 在第一帧前初始化组件引用、界面和游戏状态！！！
    // 何时执行：Unity 在第一帧更新前执行一次，适合连接场景对象和初始化界面。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Start()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：调用 FindSwayObjects：查找SwayObjects。
        FindSwayObjects();
        // 步骤 3：检查条件 enableProceduralAudio；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 CreateAudioField：创建音频Field。
        if (enableProceduralAudio) CreateAudioField();
        // 步骤 4：调用 CreateCloudParticles：创建CloudParticles。
        CreateCloudParticles();
        // 步骤 5：调用 CreatePhysicalObjects：创建PhysicalObjects。
        CreatePhysicalObjects();
        // 步骤 6：检查sanctuary、sanctuary的Gate是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。 成立时执行 gateStart = sanctuary.Gate.position;
        if (sanctuary != null && sanctuary.Gate != null) gateStart = sanctuary.Gate.position;
        // 步骤 7：这里的 ! 表示取反；当 desktopMode 不成立时才进入此分支。
        if (!desktopMode)
            // 步骤 8：把计算结果 SkyQuestVisuals.Text("Comfort hint", "", head, 保存为comfortHint；下一条判断或调用会读取这个具体值。
            comfortHint = SkyQuestVisuals.Text("Comfort hint", "", head,
                // 步骤 9：用 new 创建 Vector3(0f, -0.38f, 1.05f), 1.4f,；构造参数决定新对象的初始内容。
                new Vector3(0f, -0.38f, 1.05f), 1.4f,
                // 步骤 10：用 new 创建 Color(0.78f, 0.9f, 1f), 3.5f);；构造参数决定新对象的初始内容。
                new Color(0.78f, 0.9f, 1f), 3.5f);
        // 步骤 11：调用 ShowComfortHint：ShowComfortHint。
        ShowComfortHint();
    }
    // ！！！功能结束：Start！！！

    // ！！！功能：Update——每帧读取玩家输入并更新当前状态！！！
    // 何时执行：Unity 每帧自动调用一次；读取输入和更新当帧状态。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    void Update()
    {
        // 步骤 1：检查头显相机是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (head == null) return;
        // 步骤 2：调用 AnimateEnvironment：AnimateEnvironment。
        AnimateEnvironment();
        // 步骤 3：调用 UpdateAudio：更新音频。
        UpdateAudio();
        // 步骤 4：调用 UpdateCloudParticles：更新CloudParticles。
        UpdateCloudParticles();
        // 步骤 5：检查comfortHint、locomotion是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (comfortHint != null && locomotion != null)
            // 步骤 6：根据括号中的布尔表达式显示或隐藏场景对象；true 为显示，false 为隐藏。
            comfortHint.gameObject.SetActive(desktopMode || !locomotion.ThirdPerson);
        // 步骤 7：这里用 && 要求两边条件同时成立：!desktopMode && enablePhysicalInteractions；任意一边为假就跳过本分支。 成立时调用 UpdatePhysicalInteractions：更新PhysicalInteractions。
        if (!desktopMode && enablePhysicalInteractions) UpdatePhysicalInteractions();
        // 步骤 8：调用 UpdateComfortTeleport：更新ComfortTeleport。
        UpdateComfortTeleport();
    }
    // ！！！功能结束：Update！！！

    // ！！！功能：FindSwayObjects——查找SwayObjects！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void FindSwayObjects()
    {
        // 步骤 1：逐个读取 Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None) 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (Transform t in FindObjectsByType<Transform>(FindObjectsSortMode.None))
        {
            // 步骤 2：计算 t.name.ToLowerInvariant()，把结果存进 n（n）；之后引用 n 时用的就是这次结果。
            string n = t.name.ToLowerInvariant();
            // 步骤 3：这里用 && 要求两边条件同时成立：!n.Contains("banner") && !n.Contains("cloth") && !n.Contains("lantern") && !n.Contains("candle")；任意一边为假就跳过本分支。 成立时执行 continue;
            if (!n.Contains("banner") && !n.Contains("cloth") && !n.Contains("lantern") && !n.Contains("candle")) continue;
            // 步骤 4：检查条件 t.IsChildOf(transform)；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (t.IsChildOf(transform)) continue;
            // 步骤 5：把括号里的新元素加入 swayObjects 集合；后续会按集合顺序使用它。
            swayObjects.Add(t);
            // 步骤 6：把括号里的新元素加入 swayBase 集合；后续会按集合顺序使用它。
            swayBase.Add(t.localPosition);
            // 步骤 7：检查条件 swayObjects.Count >= 36；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 break;
            if (swayObjects.Count >= 36) break;
        }
    }
    // ！！！功能结束：FindSwayObjects！！！

    // ！！！功能：AnimateEnvironment——AnimateEnvironment！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void AnimateEnvironment()
    {
        // 步骤 1：计算 Mathf.Sin(Time.time * 0.72f) * 0.035f，把结果存进 wind（wind）；之后引用 wind 时用的就是这次结果。
        float wind = Mathf.Sin(Time.time * 0.72f) * 0.035f;
        // 步骤 2：按 int i = 0; i < swayObjects.Count; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < swayObjects.Count; i++)
        {
            // 步骤 3：计算 swayObjects[i]，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            Transform t = swayObjects[i];
            // 步骤 4：检查t是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
            if (t == null) continue;
            // 步骤 5：新建用于local位置的对象或数值结构，并把构造参数保存进去。
            t.localPosition = swayBase[i] + new Vector3(wind * Mathf.Sin(i * 1.7f),
                // 步骤 6：执行 wind * 0.25f, wind * Mathf.Cos(i * 1.3f));；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                wind * 0.25f, wind * Mathf.Cos(i * 1.3f));
            // 步骤 7：恢复 t.localRotation 到初始本地朝向；本地表示相对于父对象。
            t.localRotation = Quaternion.Euler(0f, wind * 18f, wind * 24f);
        }
    }
    // ！！！功能结束：AnimateEnvironment！！！

    // ！！！功能：CreateAudioField——创建音频Field！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateAudioField()
    {
        // 步骤 1：新建用于windSource的对象或数值结构，并把构造参数保存进去。
        windSource = MakeLoop("Spatial wind", head.position + new Vector3(-6f, 1.6f, 8f),
            // 步骤 2：调用 MakeNoise：MakeNoise。
            MakeNoise("Wind", 1.4f, 0.12f), 0.22f, 10f, 42f);
        // 步骤 3：新建用于waterSource的对象或数值结构，并把构造参数保存进去。
        waterSource = MakeLoop("Spatial water", head.position + new Vector3(7f, 0.2f, 4f),
            // 步骤 4：调用 MakeNoise：MakeNoise。
            MakeNoise("Water", 1.1f, 0.08f), 0.18f, 4f, 22f);
        // 步骤 5：新建用于fireSource的对象或数值结构，并把构造参数保存进去。
        fireSource = MakeLoop("Spatial candle fire", head.position + new Vector3(2f, 1.2f, 6f),
            // 步骤 6：调用 MakeTone：MakeTone。
            MakeTone("Fire", 188f, 1.1f, 0.06f), 0.16f, 3f, 15f);
        // 步骤 7：新建用于ancestorSource的对象或数值结构，并把构造参数保存进去。
        ancestorSource = MakeLoop("Spatial ancestor hum", head.position + new Vector3(0f, 1.5f, -5f),
            // 步骤 8：调用 MakeTone：MakeTone。
            MakeTone("Ancestor", 132f, 2f, 0.045f), 0.12f, 3f, 18f);
        // 步骤 9：创建新的 GameObject 并存为 zoneObject；构造参数决定其初始内容或对象名称。
        GameObject zoneObject = new GameObject("Sanctuary room acoustics");
        // 步骤 10：执行 zoneObject.transform.position = head.position + Vector3.forward * 4f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        zoneObject.transform.position = head.position + Vector3.forward * 4f;
        // 步骤 11：计算 zoneObject.AddComponent<AudioReverbZone>()，把结果存进 zone（zone）；之后引用 zone 时用的就是这次结果。
        AudioReverbZone zone = zoneObject.AddComponent<AudioReverbZone>();
        // 步骤 12：把计算结果 AudioReverbPreset.Hallway 保存为reverbPreset；下一条判断或调用会读取这个具体值。
        zone.reverbPreset = AudioReverbPreset.Hallway;
        // 步骤 13：把计算结果 5f 保存为minDistance；下一条判断或调用会读取这个具体值。
        zone.minDistance = 5f;
        // 步骤 14：把计算结果 24f 保存为maxDistance；下一条判断或调用会读取这个具体值。
        zone.maxDistance = 24f;
    }
    // ！！！功能结束：CreateAudioField！！！

    // 继续填写上一行调用的参数：private AudioSource MakeLoop(string name, Vector3 position, AudioClip clip, float volume；参数位置决定它表示的坐标、速度或选项。
    private AudioSource MakeLoop(string name, Vector3 position, AudioClip clip, float volume,
        // 这一行补充 float minDistance, float maxDistance)，与相邻行合起来完成同一次计算或对象配置。
        float minDistance, float maxDistance)
    {
        // 创建新的 GameObject 并存为 sourceObject；构造参数决定其初始内容或对象名称。
        GameObject sourceObject = new GameObject(name);
        // 执行 sourceObject.transform.position = position;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        sourceObject.transform.position = position;
        // 计算 sourceObject.AddComponent<AudioSource>()，把结果存进 source（反馈跟随的来源物品）；之后引用 source 时用的就是这次结果。
        AudioSource source = sourceObject.AddComponent<AudioSource>();
        // 把计算结果 clip 保存为clip；下一条判断或调用会读取这个具体值。
        source.clip = clip;
        // 把 source.loop 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        source.loop = true;
        // 把 source.playOnAwake 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        source.playOnAwake = true;
        // 用 volume 更新音源音量；播放器之后以这个强度输出声音。
        source.volume = volume;
        // 把计算结果 1f 保存为spatialBlend；下一条判断或调用会读取这个具体值。
        source.spatialBlend = 1f;
        // 把计算结果 minDistance 保存为minDistance；下一条判断或调用会读取这个具体值。
        source.minDistance = minDistance;
        // 把计算结果 maxDistance 保存为maxDistance；下一条判断或调用会读取这个具体值。
        source.maxDistance = maxDistance;
        // 把计算结果 AudioRolloffMode.Logarithmic 保存为rolloff模式；下一条判断或调用会读取这个具体值。
        source.rolloffMode = AudioRolloffMode.Logarithmic;
        // 把计算结果 65f 保存为spread；下一条判断或调用会读取这个具体值。
        source.spread = 65f;
        // 调用 Play：Play。
        source.Play();
        // 把 source 作为方法结果返回给调用方；return 后本方法立即结束。
        return source;
    }

    // ！！！功能：MakeTone——MakeTone！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, float frequency, float seconds, float amplitude 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 AudioClip 类型的计算结果；执行到 return 时把该值交回调用者。
    private AudioClip MakeTone(string name, float frequency, float seconds, float amplitude)
    {
        // 步骤 1：定义不可在运行时改变的数值 rate（rate）；后续计算统一使用这个阈值、速度或尺寸。
        const int rate = 22050;
        // 步骤 2：计算 Mathf.CeilToInt(rate * seconds)，把结果存进 samples（samples）；之后引用 samples 时用的就是这次结果。
        int samples = Mathf.CeilToInt(rate * seconds);
        // 步骤 3：计算 AudioClip.Create(name, samples, 1, rate, false)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        // 步骤 4：创建新的 float[] 并存为 data；构造参数决定其初始内容或对象名称。
        float[] data = new float[samples];
        // 步骤 5：按 int i = 0; i < samples; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < samples; i++)
        {
            // 步骤 6：计算 i / (float)rate，把结果存进 t（t）；之后引用 t 时用的就是这次结果。
            float t = i / (float)rate;
            // 步骤 7：执行 data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * amplitude;
        }
        // 步骤 8：调用 SetData：设置Data。
        clip.SetData(data, 0);
        // 步骤 9：把 clip 作为方法结果返回给调用方；return 后本方法立即结束。
        return clip;
    }
    // ！！！功能结束：MakeTone！！！

    // ！！！功能：MakeNoise——MakeNoise！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, float seconds, float amplitude 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 AudioClip 类型的计算结果；执行到 return 时把该值交回调用者。
    private AudioClip MakeNoise(string name, float seconds, float amplitude)
    {
        // 步骤 1：定义不可在运行时改变的数值 rate（rate）；后续计算统一使用这个阈值、速度或尺寸。
        const int rate = 22050;
        // 步骤 2：计算 Mathf.CeilToInt(rate * seconds)，把结果存进 samples（samples）；之后引用 samples 时用的就是这次结果。
        int samples = Mathf.CeilToInt(rate * seconds);
        // 步骤 3：计算 AudioClip.Create(name, samples, 1, rate, false)，把结果存进 clip（clip）；之后引用 clip 时用的就是这次结果。
        AudioClip clip = AudioClip.Create(name, samples, 1, rate, false);
        // 步骤 4：创建新的 float[] 并存为 data；构造参数决定其初始内容或对象名称。
        float[] data = new float[samples];
        // 步骤 5：把数值 0f 暂存为 filtered；它只在当前方法里参与后续判断或计算。
        float filtered = 0f;
        // 步骤 6：按 int i = 0; i < samples; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < samples; i++)
        {
            // 步骤 7：把计算结果 Mathf.Lerp(filtered, Random.Range(-1f, 1f), 0.08f) 保存为filtered；下一条判断或调用会读取这个具体值。
            filtered = Mathf.Lerp(filtered, Random.Range(-1f, 1f), 0.08f);
            // 步骤 8：执行 data[i] = filtered * amplitude;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            data[i] = filtered * amplitude;
        }
        // 步骤 9：调用 SetData：设置Data。
        clip.SetData(data, 0);
        // 步骤 10：把 clip 作为方法结果返回给调用方；return 后本方法立即结束。
        return clip;
    }
    // ！！！功能结束：MakeNoise！！！

    // ！！！功能：CreateCloudParticles——创建CloudParticles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreateCloudParticles()
    {
        // 步骤 1：创建新的 GameObject 并存为 particleObject；构造参数决定其初始内容或对象名称。
        GameObject particleObject = new GameObject("Cloud flight particles");
        // 步骤 2：执行 particleObject.transform.SetParent(head, false);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        particleObject.transform.SetParent(head, false);
        // 步骤 3：执行 particleObject.transform.localPosition = new Vector3(0f, 0f, 2.4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        particleObject.transform.localPosition = new Vector3(0f, 0f, 2.4f);
        // 步骤 4：把计算结果 particleObject.AddComponent<ParticleSystem>() 保存为cloudParticles；下一条判断或调用会读取这个具体值。
        cloudParticles = particleObject.AddComponent<ParticleSystem>();
        // 步骤 5：计算 cloudParticles.main，把结果存进 main（main）；之后引用 main 时用的就是这次结果。
        var main = cloudParticles.main;
        // 步骤 6：把 main.loop 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        main.loop = true;
        // 步骤 7：把 main.playOnAwake 设为 true（启用/已发生）；后续条件判断会依据这个状态。
        main.playOnAwake = true;
        // 步骤 8：把计算结果 0.7f 保存为startLifetime；下一条判断或调用会读取这个具体值。
        main.startLifetime = 0.7f;
        // 步骤 9：把计算结果 0.35f 保存为startSpeed；下一条判断或调用会读取这个具体值。
        main.startSpeed = 0.35f;
        // 步骤 10：把计算结果 0.018f 保存为startSize；下一条判断或调用会读取这个具体值。
        main.startSize = 0.018f;
        // 步骤 11：新建用于startColor的对象或数值结构，并把构造参数保存进去。
        main.startColor = new Color(0.72f, 0.9f, 1f, 0.34f);
        // 步骤 12：把计算结果 28 保存为maxParticles；下一条判断或调用会读取这个具体值。
        main.maxParticles = 28;
        // 步骤 13：计算 cloudParticles.emission，把结果存进 emission（emission）；之后引用 emission 时用的就是这次结果。
        var emission = cloudParticles.emission;
        // 步骤 14：把计算结果 0f 保存为rateOverTime；下一条判断或调用会读取这个具体值。
        emission.rateOverTime = 0f;
        // 步骤 15：计算 cloudParticles.shape，把结果存进 shape（shape）；之后引用 shape 时用的就是这次结果。
        var shape = cloudParticles.shape;
        // 步骤 16：把计算结果 ParticleSystemShapeType.Cone 保存为shapeType；下一条判断或调用会读取这个具体值。
        shape.shapeType = ParticleSystemShapeType.Cone;
        // 步骤 17：把计算结果 12f 保存为angle；下一条判断或调用会读取这个具体值。
        shape.angle = 12f;
        // 步骤 18：把计算结果 0.8f 保存为radius；下一条判断或调用会读取这个具体值。
        shape.radius = 0.8f;
        // Use the same Quest-compatible glow as the cape. The default particle
        // material renders as magenta squares in the Android/URP flight view.
        // 步骤 19：计算 Resources.Load<Material>("SkyCapeGlow")，把结果存进 capeGlow（capeGlow）；之后引用 capeGlow 时用的就是这次结果。
        Material capeGlow = Resources.Load<Material>("SkyCapeGlow");
        // 步骤 20：确认capeGlow已经有值；只有存在时才能安全读取它的属性或调用方法。
        if (capeGlow != null)
            // 步骤 21：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            particleObject.GetComponent<ParticleSystemRenderer>().sharedMaterial = capeGlow;
        // 步骤 22：调用 Stop：Stop。
        cloudParticles.Stop();
    }
    // ！！！功能结束：CreateCloudParticles！！！

    // ！！！功能：UpdateCloudParticles——更新CloudParticles！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateCloudParticles()
    {
        // 步骤 1：检查cloudParticles、locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (cloudParticles == null || locomotion == null) return;
        // 步骤 2：计算 cloudParticles.emission，把结果存进 emission（emission）；之后引用 emission 时用的就是这次结果。
        var emission = cloudParticles.emission;
        // 步骤 3：用 visible 记住此处真假结果（locomotion.IsFlying && (desktopMode || !locomotion.ThirdPerson)），供下一条 if 选择执行路径。
        bool visible = locomotion.IsFlying && (desktopMode || !locomotion.ThirdPerson);
        // 步骤 4：把计算结果 visible ? 18f : 0f 保存为rateOverTime；下一条判断或调用会读取这个具体值。
        emission.rateOverTime = visible ? 18f : 0f;
        // 步骤 5：这里用 && 要求两边条件同时成立：visible && !cloudParticles.isPlaying；任意一边为假就跳过本分支。 成立时调用 Play：Play。
        if (visible && !cloudParticles.isPlaying) cloudParticles.Play();
        // 步骤 6：这里用 && 要求两边条件同时成立：!visible && cloudParticles.isPlaying；任意一边为假就跳过本分支。 成立时调用 Stop：Stop。
        if (!visible && cloudParticles.isPlaying) cloudParticles.Stop();
    }
    // ！！！功能结束：UpdateCloudParticles！！！

    // ！！！功能：CreatePhysicalObjects——创建PhysicalObjects！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void CreatePhysicalObjects()
    {
        // 步骤 1：检查sanctuary、sanctuary的beacons是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        if (sanctuary != null && sanctuary.beacons != null)
        {
            // 步骤 2：按 int i = 0; i < sanctuary.beacons.Length; i++ 变化索引；循环体依次处理每个位置。
            for (int i = 0; i < sanctuary.beacons.Length; i++)
            {
                // 步骤 3：计算 sanctuary.beacons[i]，把结果存进 beacon（beacon）；之后引用 beacon 时用的就是这次结果。
                Transform beacon = sanctuary.beacons[i];
                // 步骤 4：检查beacon是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 continue;
                if (beacon == null) continue;
                // 步骤 5：把计算结果 CreateGlowObject("Touchable lantern " + (i + 1), 保存为lantern；下一条判断或调用会读取这个具体值。
                GameObject lantern = CreateGlowObject("Touchable lantern " + (i + 1),
                    // 步骤 6：执行 beacon.position + Vector3.up * 0.42f, new Color(0.35f, 0.8f, 1f), 0.22f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    beacon.position + Vector3.up * 0.42f, new Color(0.35f, 0.8f, 1f), 0.22f);
                // 步骤 7：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
                lantern.GetComponent<SkyQuestPhysicalObject>().index = i;
                // Lanterns share the exact same distant trigger pickup and
                // ballistic release as every ancestor reward and display jar.
                // 步骤 8：从目标物体找到所需组件并存为 solid；使用前应检查是否为 null。
                SphereCollider solid = lantern.GetComponent<SphereCollider>();
                // 步骤 9：确认solid已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 solid.isTrigger = false;
                if (solid != null) solid.isTrigger = false;
                // 步骤 10：计算 lantern.AddComponent<SkyQuestMagicPickup>()，把结果存进 pickup（pickup）；之后引用 pickup 时用的就是这次结果。
                SkyQuestMagicPickup pickup = lantern.AddComponent<SkyQuestMagicPickup>();
                // 步骤 11：把计算结果 head 保存为desktopHead；下一条判断或调用会读取这个具体值。
                pickup.desktopHead = head;
                // 步骤 12：把计算结果 rightHand 保存为desktopHand；下一条判断或调用会读取这个具体值。
                pickup.desktopHand = rightHand;
                // 步骤 13：把计算结果 locomotion != null && locomotion.avatar != null ? 保存为desktopAvatar；下一条判断或调用会读取这个具体值。
                pickup.desktopAvatar = locomotion != null && locomotion.avatar != null ?
                    // 步骤 14：执行 locomotion.avatar.transform : null;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    locomotion.avatar.transform : null;
                // 步骤 15：把计算结果 desktopMode 保存为desktop模式；下一条判断或调用会读取这个具体值。
                pickup.desktopMode = desktopMode;
                // 步骤 16：把 pickup.displayPottery 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                pickup.displayPottery = true;
                // 步骤 17：调用 AnchorOnDisplay：AnchorOnDisplay。
                pickup.AnchorOnDisplay();
                // 步骤 18：把括号里的新元素加入 lanterns 集合；后续会按集合顺序使用它。
                lanterns.Add(lantern);
            }
        }
        // 步骤 19：创建新的 Vector3[] 并存为 candlePositions；构造参数决定其初始内容或对象名称。
        Vector3[] candlePositions = { new Vector3(30f, 1.05f, 12f), new Vector3(44f, 1.05f, 12f), new Vector3(55f, 1.05f, -4f) };
        // 步骤 20：按 int i = 0; i < candlePositions.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < candlePositions.Length; i++)
        {
            // 步骤 21：把计算结果 CreateGlowObject("Touchable candle " + (i + 1), candlePositions[i], 保存为candle；下一条判断或调用会读取这个具体值。
            GameObject candle = CreateGlowObject("Touchable candle " + (i + 1), candlePositions[i],
                // 步骤 22：用 new 创建 Color(1f, 0.65f, 0.22f), 0.08f);；构造参数决定新对象的初始内容。
                new Color(1f, 0.65f, 0.22f), 0.08f);
            // 步骤 23：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            candle.GetComponent<SkyQuestPhysicalObject>().index = i;
            // 步骤 24：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            candle.GetComponent<SkyQuestPhysicalObject>().candle = true;
            // 步骤 25：把括号里的新元素加入 candles 集合；后续会按集合顺序使用它。
            candles.Add(candle);
            // 步骤 26：把括号里的新元素加入 candleLit 集合；后续会按集合顺序使用它。
            candleLit.Add(false);
        }
    }
    // ！！！功能结束：CreatePhysicalObjects！！！

    // ！！！功能：CreateGlowObject——创建Glow物品！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string name, Vector3 position, Color color, float size 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：返回 GameObject 类型的计算结果；执行到 return 时把该值交回调用者。
    private GameObject CreateGlowObject(string name, Vector3 position, Color color, float size)
    {
        // 步骤 1：把计算结果 SkyQuestVisuals.Primitive(name, PrimitiveType.Sphere, null, position, 保存为obj；下一条判断或调用会读取这个具体值。
        GameObject obj = SkyQuestVisuals.Primitive(name, PrimitiveType.Sphere, null, position,
            // 步骤 2：执行 Vector3.one * size, color, true);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Vector3.one * size, color, true);
        // 步骤 3：从目标物体找到所需组件并存为 collider；使用前应检查是否为 null。
        SphereCollider collider = obj.GetComponent<SphereCollider>();
        // 步骤 4：确认collider已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { collider.isTrigger = true; collider.radius = 1.6f; }
        if (collider != null) { collider.isTrigger = true; collider.radius = 1.6f; }
        // 步骤 5：计算 obj.AddComponent<SkyQuestPhysicalObject>()，把结果存进 physical（physical）；之后引用 physical 时用的就是这次结果。
        SkyQuestPhysicalObject physical = obj.AddComponent<SkyQuestPhysicalObject>();
        // 步骤 6：把计算结果 this 保存为owner；下一条判断或调用会读取这个具体值。
        physical.owner = this;
        // 步骤 7：把 obj 作为方法结果返回给调用方；return 后本方法立即结束。
        return obj;
    }
    // ！！！功能结束：CreateGlowObject！！！

    // ！！！功能：UpdatePhysicalInteractions——更新PhysicalInteractions！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdatePhysicalInteractions()
    {
        // 步骤 1：检查左手柄位置、右手柄位置是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (leftHand == null || rightHand == null) return;
        // 步骤 2：用 leftTrigger 记住此处真假结果（SkyQuestInput.Trigger(SkyQuestInput.Left) > 0.7f），供下一条 if 选择执行路径。
        bool leftTrigger = SkyQuestInput.Trigger(SkyQuestInput.Left) > 0.7f;
        // 步骤 3：这里用 && 要求两边条件同时成立：leftTrigger && !previousLeftTrigger；任意一边为假就跳过本分支。
        if (leftTrigger && !previousLeftTrigger)
        {
            // 步骤 4：把计算结果 Time.unscaledTime 保存为leftTriggerStarted；下一条判断或调用会读取这个具体值。
            leftTriggerStarted = Time.unscaledTime;
            // 步骤 5：把计算结果 SkyQuestMagicPickup.Held != null 保存为leftTriggerStarted带有物品；下一条判断或调用会读取这个具体值。
            leftTriggerStartedWithItem = SkyQuestMagicPickup.Held != null;
        }
        // 步骤 6：把计算结果 !leftTrigger && previousLeftTrigger && 保存为shortTap；下一条判断或调用会读取这个具体值。
        bool shortTap = !leftTrigger && previousLeftTrigger &&
            // 步骤 7：执行 Time.unscaledTime - leftTriggerStarted < .6f && !leftTriggerStartedWithItem;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            Time.unscaledTime - leftTriggerStarted < .6f && !leftTriggerStartedWithItem;
        // 步骤 8：按 int i = 0; i < candles.Count && shortTap; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < candles.Count && shortTap; i++)
        {
            // 步骤 9：检查对象引用是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 成立时执行 continue;
            if (candleLit[i] || candles[i] == null) continue;
            // 步骤 10：检查条件 Vector3.Distance(leftHand.position, candles[i].transform.position) > 0.42f；只有为真才运行下面的步骤，否则沿原流程继续。 成立时执行 continue;
            if (Vector3.Distance(leftHand.position, candles[i].transform.position) > 0.42f) continue;
            // 步骤 11：执行 candleLit[i] = true;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            candleLit[i] = true;
            // 步骤 12：从指定 GameObject 取出一个组件；组件不存在时结果为 null，后续要检查。
            candles[i].GetComponent<Renderer>().material.color = new Color(1f, 0.95f, 0.55f);
            // 步骤 13：执行 status?.Show("Candle lit. A warm light answers your hand.", 3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Candle lit. A warm light answers your hand.", 3f);
            // 步骤 14：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.3f, 0.1f);
        }
        // 步骤 15：调用 UpdateGatePush：更新GatePush。
        UpdateGatePush();
        // 步骤 16：把计算结果 leftTrigger 保存为previousLeftTrigger；下一条判断或调用会读取这个具体值。
        previousLeftTrigger = leftTrigger;
    }
    // ！！！功能结束：UpdatePhysicalInteractions！！！

    // ！！！功能：UpdateGatePush——更新GatePush！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateGatePush()
    {
        // 步骤 1：检查sanctuary、sanctuary的Gate是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (sanctuary == null || sanctuary.Gate == null) return;
        // 步骤 2：把计算结果 Vector3.Distance((leftHand.position + rightHand.position) * 0.5f, 保存为gateDistance；下一条判断或调用会读取这个具体值。
        float gateDistance = Vector3.Distance((leftHand.position + rightHand.position) * 0.5f,
            // 步骤 3：执行 sanctuary.Gate.position);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            sanctuary.Gate.position);
        // 步骤 4：把计算结果 gateDistance < 2.6f && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f && 保存为pushing；下一条判断或调用会读取这个具体值。
        bool pushing = gateDistance < 2.6f && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.65f &&
            // 步骤 5：调用 Grip：Grip。
            SkyQuestInput.Grip(SkyQuestInput.Right) > 0.65f;
        // 步骤 6：这里的 ! 表示取反；当 pushing 不成立时才进入此分支。 条件成立时立即退出当前方法。
        if (!pushing) return;
        // 步骤 7：继续填写上一行调用的参数：sanctuary.Gate.position = Vector3.MoveTowards(sanctuary.Gate.position；参数位置决定它表示的坐标、速度或选项。
        sanctuary.Gate.position = Vector3.MoveTowards(sanctuary.Gate.position,
            // 步骤 8：执行 gateStart + Vector3.up * 4.3f, 0.85f * Time.deltaTime);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            gateStart + Vector3.up * 4.3f, 0.85f * Time.deltaTime);
        // 步骤 9：把当前时间与计划时刻比较；未到时间时继续等待，到点后才切换状态。
        if (Time.time > nextHaptic)
        {
            // 步骤 10：执行 status?.Show("Stone gate moved by both hands.", 2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Stone gate moved by both hands.", 2f);
            // 步骤 11：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Left, 0.25f, 0.08f);
            // 步骤 12：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Right, 0.25f, 0.08f);
            // 步骤 13：根据当前游戏时间计算并保存nextHaptic，用于后续延迟或冷却判断。
            nextHaptic = Time.time + 0.5f;
        }
    }
    // ！！！功能结束：UpdateGatePush！！！

    // ！！！功能：UpdateAudio——更新音频！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateAudio()
    {
        // 步骤 1：检查windSource是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (windSource == null) return;
        // 步骤 2：用三元表达式为 flying 二选一：问号前条件为真取前值，为假取冒号后的值。
        float flying = locomotion != null && locomotion.IsFlying ? 1f : 0.45f;
        // 步骤 3：用 0.18f + flying * 0.12f 更新音源音量；播放器之后以这个强度输出声音。
        windSource.volume = 0.18f + flying * 0.12f;
        // 步骤 4：用 0.12f + (Mathf.Sin(Time.time * 2.3f) + 1f) * 0.018f 更新音源音量；播放器之后以这个强度输出声音。
        fireSource.volume = 0.12f + (Mathf.Sin(Time.time * 2.3f) + 1f) * 0.018f;
    }
    // ！！！功能结束：UpdateAudio！！！

    // ！！！功能：UpdateComfortTeleport——更新ComfortTeleport！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void UpdateComfortTeleport()
    {
        // 步骤 1：用 toggle 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool toggle = false;
        // 步骤 2：用 calibrate 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool calibrate = false;
        // 步骤 3：用 toggleHeld 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool toggleHeld = false;
        // 步骤 4：用 calibrateHeld 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool calibrateHeld = false;
        // 步骤 5：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 6：计算 UnityEngine.InputSystem.Keyboard.current，把结果存进 keyboard（keyboard）；之后引用 keyboard 时用的就是这次结果。
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            // 步骤 7：确认keyboard已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 { toggle = keyboard.tKey.wasPressedThisFrame; calibrate = keyboard.f1Key.wa
            if (keyboard != null) { toggle = keyboard.tKey.wasPressedThisFrame; calibrate = keyboard.f1Key.wasPressedThisFrame; }
        }
        // 步骤 8：前面的 if 条件为假时执行这里的备用处理。
        else
        {
            // 步骤 9：把计算结果 SkyQuestInput.Grip(SkyQuestInput.Left) > 0.75f && 保存为toggleHeld；下一条判断或调用会读取这个具体值。
            toggleHeld = SkyQuestInput.Grip(SkyQuestInput.Left) > 0.75f &&
                // 步骤 10：调用 Secondary：Secondary。
                SkyQuestInput.Secondary(SkyQuestInput.Right);
            // 步骤 11：把计算结果 SkyQuestInputExtensions.StickClick(SkyQuestInput.Left) && 保存为calibrateHeld；下一条判断或调用会读取这个具体值。
            calibrateHeld = SkyQuestInputExtensions.StickClick(SkyQuestInput.Left) &&
                // 步骤 12：调用 StickClick：StickClick。
                SkyQuestInputExtensions.StickClick(SkyQuestInput.Right);
            // 步骤 13：把计算结果 toggleHeld && !previousTeleportToggle 保存为toggle；下一条判断或调用会读取这个具体值。
            toggle = toggleHeld && !previousTeleportToggle;
            // 步骤 14：把计算结果 calibrateHeld && !previousCalibrate 保存为calibrate；下一条判断或调用会读取这个具体值。
            calibrate = calibrateHeld && !previousCalibrate;
        }
        // 步骤 15：检查条件 toggle；只有为真才运行下面的步骤，否则沿原流程继续。
        if (toggle)
        {
            // 步骤 16：把计算结果 !teleportMode 保存为teleport模式；下一条判断或调用会读取这个具体值。
            teleportMode = !teleportMode;
            // 步骤 17：这一行补充 status?.Show(teleportMode ?，与相邻行合起来完成同一次计算或对象配置。
            status?.Show(teleportMode ?
                // 步骤 18：这一行补充 (desktopMode ? "Teleport ON: look at the ground and press V." :，与相邻行合起来完成同一次计算或对象配置。
                (desktopMode ? "Teleport ON: look at the ground and press V." :
                    // 步骤 19：这一行补充 "Teleport ON: aim at the ground and press right trigger.") :，与相邻行合起来完成同一次计算或对象配置。
                    "Teleport ON: aim at the ground and press right trigger.") :
                // 步骤 20：执行 "Comfort teleport OFF.", 4f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                "Comfort teleport OFF.", 4f);
        }
        // 步骤 21：检查条件 calibrate；只有为真才运行下面的步骤，否则沿原流程继续。
        if (calibrate)
        {
            // 步骤 22：确认头显相机已经有值；只有存在时才能安全读取它的属性或调用方法。 成立时执行 transform.position -= Vector3.up * (head.localPosition.y - 1.65f);
            if (head != null) transform.position -= Vector3.up * (head.localPosition.y - 1.65f);
            // 步骤 23：执行 status?.Show("Seated height calibrated.", 3f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            status?.Show("Seated height calibrated.", 3f);
        }
        // 步骤 24：把计算结果 toggleHeld 保存为previousTeleportToggle；下一条判断或调用会读取这个具体值。
        previousTeleportToggle = toggleHeld;
        // 步骤 25：把计算结果 calibrateHeld 保存为previousCalibrate；下一条判断或调用会读取这个具体值。
        previousCalibrate = calibrateHeld;
        // 步骤 26：用 vrTrigger 记住此处真假结果（!desktopMode && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f），供下一条 if 选择执行路径。
        bool vrTrigger = !desktopMode && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        // 步骤 27：用 vrFire 记住此处真假结果（vrTrigger && !previousTeleportTrigger），供下一条 if 选择执行路径。
        bool vrFire = vrTrigger && !previousTeleportTrigger;
        // 步骤 28：把计算结果 vrTrigger 保存为previousTeleportTrigger；下一条判断或调用会读取这个具体值。
        previousTeleportTrigger = vrTrigger;
        // 步骤 29：检查locomotion是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
        if (!teleportMode || locomotion == null || locomotion.IsFlying) return;
        // 步骤 30：检查条件 desktopMode；只有为真才运行下面的步骤，否则沿原流程继续。
        if (desktopMode)
        {
            // 步骤 31：计算 UnityEngine.InputSystem.Keyboard.current，把结果存进 keyboard（keyboard）；之后引用 keyboard 时用的就是这次结果。
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            // 步骤 32：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (keyboard != null && keyboard.vKey.wasPressedThisFrame && Time.time > nextTeleport &&
                // 步骤 33：调用 Raycast：Raycast。
                Physics.Raycast(head.position, head.forward, out RaycastHit desktopHit, 12f,
                    // 步骤 34：这一行补充 ~0, QueryTriggerInteraction.Ignore) && desktopHit.normal.y > 0.65f)，与相邻行合起来完成同一次计算或对象配置。
                    ~0, QueryTriggerInteraction.Ignore) && desktopHit.normal.y > 0.65f)
                // 步骤 35：调用 TeleportTo：扣除当前头部与 XR 原点的水平偏移，把头部准确落在目标点。
                TeleportTo(desktopHit.point);
        }
        // 步骤 36：上一种情况不成立时再检查这个条件；多条件按顺序选择第一条可用路径。检查右手柄位置是否为空，并结合其余条件；全部满足时才进入这一分支，避免无效引用。
        else if (rightHand != null && vrFire && Time.time > nextTeleport &&
            // 步骤 37：调用 Raycast：Raycast。
            Physics.Raycast(rightHand.position, rightHand.forward, out RaycastHit hit, 12f,
                // 步骤 38：这一行补充 ~0, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.65f)，与相邻行合起来完成同一次计算或对象配置。
                ~0, QueryTriggerInteraction.Ignore) && hit.normal.y > 0.65f)
        {
            // 步骤 39：调用 TeleportTo：扣除当前头部与 XR 原点的水平偏移，把头部准确落在目标点。
            TeleportTo(hit.point);
            // 步骤 40：调用 Pulse：检测设备震动能力并发送短促触觉反馈。
            SkyQuestInput.Pulse(SkyQuestInput.Right, 0.35f, 0.08f);
        }
        // 步骤 41：调用 ShowComfortHint：ShowComfortHint。
        ShowComfortHint();
    }
    // ！！！功能结束：UpdateComfortTeleport！！！

    // ！！！功能：TeleportTo——扣除当前头部与 XR 原点的水平偏移，把头部准确落在目标点！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 Vector3 destination 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void TeleportTo(Vector3 destination)
    {
        // 步骤 1：把position更新为 destination + Vector3.up * 0.05f -；世界或局部位置会立即影响物体摆放。
        transform.position = destination + Vector3.up * 0.05f -
            // 步骤 2：这一行补充 (desktopMode ? Vector3.zero :，与相邻行合起来完成同一次计算或对象配置。
            (desktopMode ? Vector3.zero :
                // 步骤 3：调用 ProjectOnPlane：ProjectOnPlane。
                Vector3.ProjectOnPlane(head.position - transform.position, Vector3.up));
        // 步骤 4：根据当前游戏时间计算并保存nextTeleport，用于后续延迟或冷却判断。
        nextTeleport = Time.time + 0.5f;
        // 步骤 5：执行 status?.Show("Teleported.", 2f);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        status?.Show("Teleported.", 2f);
    }
    // ！！！功能结束：TeleportTo！！！

    // ！！！功能：ShowComfortHint——ShowComfortHint！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private void ShowComfortHint()
    {
        // 步骤 1：检查comfortHint是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 条件成立时立即退出当前方法。
        if (comfortHint == null) return;
        // 步骤 2：把新内容写入 comfortHint.text；玩家看到的文字会在本帧更新。
        comfortHint.text = "Left grip+B: teleport  •  both stick clicks: calibrate";
    }
    // ！！！功能结束：ShowComfortHint！！！
}

// 定义 SkyQuestPhysicalObject 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public class SkyQuestPhysicalObject : MonoBehaviour
{
    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [HideInInspector] public SkyQuestImmersion owner;
    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [HideInInspector] public int index;
    // 方括号是 C# 特性：向 Unity 声明下方字段或方法的显示、保存或调用规则。
    [HideInInspector] public bool candle;
}
