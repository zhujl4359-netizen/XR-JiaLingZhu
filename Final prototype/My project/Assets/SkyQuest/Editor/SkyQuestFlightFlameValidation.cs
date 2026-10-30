// ！！！文件功能：在编辑器校验飞行火焰收集和终点流程！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System.Reflection 命名空间；下方可直接使用该库提供的类型和方法。
using System.Reflection;
// 引入 UnityEditor 命名空间；下方可直接使用仅在 Unity 编辑器内使用的资源和菜单接口。
using UnityEditor;
// 引入 UnityEngine 命名空间；下方可直接使用GameObject、Transform、Vector3 等 Unity 基础类型。
using UnityEngine;

// Runs the real course component in Play mode, without changing the saved scene.
// 定义 SkyQuestFlightFlameValidation 类；字段保存本功能的数据，方法按执行步骤处理这些数据。
public static class SkyQuestFlightFlameValidation
{
    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Validate flame finish and cape refill in Play mode")]
    // ！！！功能：ValidateFinish——验证Finish！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateFinish()
    {
        // 步骤 1：计算 Object.FindFirstObjectByType<SkyQuestFlightFlameGame>()，把结果存进 game（game）；之后引用 game 时用的就是这次结果。
        SkyQuestFlightFlameGame game = Object.FindFirstObjectByType<SkyQuestFlightFlameGame>();
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!Application.isPlaying || game == null || game.player == null ||
            // 步骤 3：把计算结果 = null || game.flames.Length == 0) 保存为flames；下一条判断或调用会读取这个具体值。
            game.flames == null || game.flames.Length == 0)
        {
            // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("Flight finish validation needs the desktop scene running in Play mode.");
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 6：定义固定文字 bestKey（bestKey）；统一引用这段名称或路径，避免各处拼写不一致。
        const string bestKey = "SkyQuestFlightFlamesBest";
        // 步骤 7：用 hadBest 记住此处真假结果（PlayerPrefs.HasKey(bestKey)），供下一条 if 选择执行路径。
        bool hadBest = PlayerPrefs.HasKey(bestKey);
        // 步骤 8：计算 PlayerPrefs.GetInt(bestKey, 0)，把结果存进 previousBest（previousBest）；之后引用 previousBest 时用的就是这次结果。
        int previousBest = PlayerPrefs.GetInt(bestKey, 0);
        // 步骤 9：调用 SendMessage：SendMessage。
        game.SendMessage("BeginRound");
        // 步骤 10：从目标物体找到所需组件并存为 energy；使用前应检查是否为 null。
        CapeEnergySystem energy = game.player.GetComponent<CapeEnergySystem>();
        // 步骤 11：调用 TryConsumeBoostEnergy：先验证剩余能量，再扣除一次加速所需数值。
        energy.TryConsumeBoostEnergy();
        // 步骤 12：计算 energy.CurrentEnergy，把结果存进 depleted（depleted）；之后引用 depleted 时用的就是这次结果。
        float depleted = energy.CurrentEnergy;
        // 步骤 13：按 int i = 0; i < game.flames.Length; i++ 变化索引；循环体依次处理每个位置。
        for (int i = 0; i < game.flames.Length; i++) game.SendMessage("Collect", i);
        // 步骤 14：把计算结果 (bool)typeof(SkyQuestFlightFlameGame).GetField("won", 保存为finished；下一条判断或调用会读取这个具体值。
        bool finished = (bool)typeof(SkyQuestFlightFlameGame).GetField("won",
            // 步骤 15：执行 BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
        // 步骤 16：把计算结果 finished && depleted < energy.maxEnergy && 保存为passed；下一条判断或调用会读取这个具体值。
        bool passed = finished && depleted < energy.maxEnergy &&
            // 步骤 17：调用 Approximately：Approximately。
            Mathf.Approximately(energy.CurrentEnergy, energy.maxEnergy);
        // 步骤 18：检查条件 hadBest；只有为真才运行下面的步骤，否则沿原流程继续。 成立时调用 SetInt：设置Int。
        if (hadBest) PlayerPrefs.SetInt(bestKey, previousBest);
        // 步骤 19：前面的 if 条件为假时执行这里的备用处理。
        else PlayerPrefs.DeleteKey(bestKey);
        // 步骤 20：调用 Save：保存。
        PlayerPrefs.Save();
        // 步骤 21：检查条件 passed；只有为真才运行下面的步骤，否则沿原流程继续。
        if (passed)
            // 步骤 22：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.Log("Flight flame finish PASS: all flames completed the round and cape energy refilled from " +
                // 步骤 23：执行 depleted + " to " + energy.CurrentEnergy + ".");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                depleted + " to " + energy.CurrentEnergy + ".");
        // 步骤 24：前面的 if 条件为假时执行这里的备用处理。
        else
            // 步骤 25：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("Flight flame finish FAIL: completed " + finished +
                // 步骤 26：执行 ", energy " + depleted + " -> " + energy.CurrentEnergy + ".");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                ", energy " + depleted + " -> " + energy.CurrentEnergy + ".");
    }
    // ！！！功能结束：ValidateFinish！！！

    // 注册 Unity 顶部菜单入口；点击该菜单才执行下方编辑器方法。
    [MenuItem("SKY/Validate flame collection in Play mode")]
    // ！！！功能：ValidateCollection——验证Collection！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：没有调用参数；读取 Inspector 中保存的字段或当前场景状态。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    private static void ValidateCollection()
    {
        // 步骤 1：计算 Object.FindFirstObjectByType<SkyQuestFlightFlameGame>()，把结果存进 game（game）；之后引用 game 时用的就是这次结果。
        SkyQuestFlightFlameGame game = Object.FindFirstObjectByType<SkyQuestFlightFlameGame>();
        // 步骤 2：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
        if (!Application.isPlaying || game == null || game.player == null ||
            // 步骤 3：把计算结果 = null || game.flames.Length == 0) 保存为flames；下一条判断或调用会读取这个具体值。
            game.flames == null || game.flames.Length == 0)
        {
            // 步骤 4：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
            Debug.LogError("Flight flame validation needs the desktop scene running in Play mode.");
            // 步骤 5：提前结束这个方法；不再执行下方语句。
            return;
        }

        // 步骤 6：调用 SendMessage：SendMessage。
        game.SendMessage("BeginRound");
        // 步骤 7：计算 game.flames[0]，把结果存进 flame（flame）；之后引用 flame 时用的就是这次结果。
        SkyQuestFlightFlame flame = game.flames[0];
        // 步骤 8：把 flame.enabled 设为 false（停用/尚未发生）；后续条件判断会依据这个状态。
        flame.enabled = false;
        // 步骤 9：执行 flame.transform.position = game.player.transform.position + Vector3.up * 1.1f;；这一句的结果会改变相应状态，或作为前一步调用的一部分。
        flame.transform.position = game.player.transform.position + Vector3.up * 1.1f;
        // 步骤 10：计算 Time.frameCount，把结果存进 startingFrame（startingFrame）；之后引用 startingFrame 时用的就是这次结果。
        float checkAfter = Time.realtimeSinceStartup + .7f;
        // 步骤 11：计算 null，把结果存进 check（check）；之后引用 check 时用的就是这次结果。
        EditorApplication.CallbackFunction check = null;
        // 步骤 12：把计算结果 () => 保存为check；下一条判断或调用会读取这个具体值。
        check = () =>
        {
            // 步骤 13：这里用 && 要求两边条件同时成立：Application.isPlaying && Time.frameCount <= startingFrame + 2；任意一边为假就跳过本分支。 条件成立时立即退出当前方法。
            if (Application.isPlaying && Time.realtimeSinceStartup < checkAfter) return;
            // 步骤 14：在update原值上减少 check；旧状态与本次输入共同决定新状态。
            EditorApplication.update -= check;
            // 步骤 15：检查game是否为空，并结合其余条件；其中一项满足时才进入这一分支，避免无效引用。 条件成立时立即退出当前方法。
            if (!Application.isPlaying || game == null) return;
            // 步骤 16：把计算结果 typeof(SkyQuestFlightFlameGame).GetField("score", 保存为scoreField；下一条判断或调用会读取这个具体值。
            FieldInfo scoreField = typeof(SkyQuestFlightFlameGame).GetField("score",
                // 步骤 17：执行 BindingFlags.NonPublic | BindingFlags.Instance);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                BindingFlags.NonPublic | BindingFlags.Instance);
            // 步骤 18：计算 (int)scoreField.GetValue(game)，把结果存进 score（score）；之后引用 score 时用的就是这次结果。
            int score = (int)scoreField.GetValue(game);
            // 步骤 19：这里用 && 要求两边条件同时成立：score == game.pointsPerFlame && !flame.gameObject.activeSelf；任意一边为假就跳过本分支。
            GameObject burst = GameObject.Find("Flight flame golden collection burst");
            bool feedbackReady = burst != null && burst.GetComponent<ParticleSystem>() != null &&
                burst.GetComponent<AudioSource>() != null;
            if (score == game.pointsPerFlame && !flame.gameObject.activeSelf && feedbackReady)
                // 步骤 20：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                Debug.Log("Flight flame collection PASS: touch gave " + score +
                    // 步骤 21：执行 " points and removed the collected flame.");；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    " points and removed the collected flame.");
            // 步骤 22：前面的 if 条件为假时执行这里的备用处理。
            else
                // 步骤 23：把当前状态写入 Unity Console，方便核对流程是否执行到这里。
                Debug.LogError("Flight flame collection FAIL: score " + score +
                    // 步骤 24：执行 ", flame active " + flame.gameObject.activeSelf);；这一句的结果会改变相应状态，或作为前一步调用的一部分。
                    ", flame active " + flame.gameObject.activeSelf + ", feedback " + feedbackReady);
        };
        // 步骤 25：在update原值上增加 check；旧状态与本次输入共同决定新状态。
        EditorApplication.update += check;
    }
    // ！！！功能结束：ValidateCollection！！！
}
