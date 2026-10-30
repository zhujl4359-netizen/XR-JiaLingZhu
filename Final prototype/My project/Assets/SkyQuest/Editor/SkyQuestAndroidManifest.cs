// ！！！文件功能：构建 Android 项目后写入 Quest 所需清单配置！！！
// 阅读方法：先看每段的“功能、何时执行、输入、结果”，再按“步骤”从上往下读；有关键计算时另写“原理”。
// 语法速读：if 表示条件；else 是条件不成立时的分支；return 会结束方法；null 表示没有对象；{ } 包住同一段代码。
// 说明：此文件位于 Assets/Editor，仅用于 Unity 编辑器的搭建与验证，玩家安装的 APK 不执行这里的方法。
// 引入 System 命名空间；下方可直接使用该库提供的类型和方法。
using System;
// 引入 System.IO 命名空间；下方可直接使用File 文件与 Directory 文件夹操作。
using System.IO;
// 引入 System.Xml 命名空间；下方可直接使用该库提供的类型和方法。
using System.Xml;
// 引入 UnityEditor.Android 命名空间；下方可直接使用该库提供的类型和方法。
using UnityEditor.Android;

// 定义 SkyQuestAndroidManifest 数据类；它把外部资源清单里的相关字段放在一起，便于一次读取与赋值。
public sealed class SkyQuestAndroidManifest : IPostGenerateGradleAndroidProject
{
    // 计算 > 9999，把结果存进 callbackOrder（callbackOrder）；之后引用 callbackOrder 时用的就是这次结果。
    public int callbackOrder => 9999;
    // 定义固定文字 AndroidNamespace（AndroidNamespace）；统一引用这段名称或路径，避免各处拼写不一致。
    private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

    // ！！！功能：OnPostGenerateGradleAndroidProject——OnPostGenerateGradleAndroidProject！！！
    // 何时执行：由其他方法、界面按钮或 Unity 编辑器菜单调用；执行后返回调用处。
    // 输入：括号中的 string path 是输入；它告诉方法本次要处理的对象或数值。
    // 结果：直接更新场景对象、界面或脚本内部状态；没有额外的返回值。
    public void OnPostGenerateGradleAndroidProject(string path)
    {
        // 步骤 1：计算 Path.Combine(path, "src/main/AndroidManifest.xml")，把结果存进 manifestPath（manifest路径）；之后引用 manifestPath 时用的就是这次结果。
        string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
        // 步骤 2：这里的 ! 表示取反；当 File.Exists(manifestPath) 不成立时才进入此分支。 成立时执行 throw new FileNotFoundException("Unity Android manifest missing", manifestP
        if (!File.Exists(manifestPath)) throw new FileNotFoundException("Unity Android manifest missing", manifestPath);
        // 步骤 3：创建新的 XmlDocument 并存为 document；构造参数决定其初始内容或对象名称。
        XmlDocument document = new XmlDocument();
        // 步骤 4：调用 Load：加载。
        document.Load(manifestPath);
        // 步骤 5：计算 document.DocumentElement，把结果存进 root（root）；之后引用 root 时用的就是这次结果。
        XmlElement root = document.DocumentElement;
        // 步骤 6：检查root是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。 成立时执行 throw new InvalidOperationException("Invalid Android manifest.");
        if (root == null) throw new InvalidOperationException("Invalid Android manifest.");
        // 步骤 7：用 hasMicrophonePermission 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool hasMicrophonePermission = false;
        // 步骤 8：逐个读取 XmlNode node in root.ChildNodes 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (XmlNode node in root.ChildNodes)
        {
            // 步骤 9：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (node is XmlElement element && element.LocalName == "uses-permission" &&
                // 步骤 10：调用 GetAttribute：获取Attribute。
                element.GetAttribute("name", AndroidNamespace) == "android.permission.RECORD_AUDIO")
            {
                // 步骤 11：把 hasMicrophonePermission 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                hasMicrophonePermission = true;
                // 步骤 12：结束当前 switch 分支或循环，不继续落入后面的处理。
                break;
            }
        }
        // 步骤 13：这里的 ! 表示取反；当 hasMicrophonePermission 不成立时才进入此分支。
        if (!hasMicrophonePermission)
        {
            // 步骤 14：计算 document.CreateElement("uses-permission")，把结果存进 permission（permission）；之后引用 permission 时用的就是这次结果。
            XmlElement permission = document.CreateElement("uses-permission");
            // 步骤 15：调用 SetAttribute：设置Attribute。
            permission.SetAttribute("name", AndroidNamespace, "android.permission.RECORD_AUDIO");
            // 步骤 16：调用 PrependChild：PrependChild。
            root.PrependChild(permission);
        }
        // 步骤 17：计算 null，把结果存进 queries（queries）；之后引用 queries 时用的就是这次结果。
        XmlElement queries = null;
        // 步骤 18：逐个读取 XmlNode node in root.ChildNodes 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (XmlNode node in root.ChildNodes)
            // 步骤 19：这里用 && 要求两边条件同时成立：node is XmlElement element && element.LocalName == "queries"；任意一边为假就跳过本分支。 成立时执行 { queries = element; break; }
            if (node is XmlElement element && element.LocalName == "queries") { queries = element; break; }
        // 步骤 20：检查queries是否还没有值；为空时进入这条分支，通常要补建组件或跳过操作。
        if (queries == null)
        {
            // 步骤 21：把计算结果 document.CreateElement("queries") 保存为queries；下一条判断或调用会读取这个具体值。
            queries = document.CreateElement("queries");
            // 步骤 22：调用 AppendChild：AppendChild。
            root.AppendChild(queries);
        }
        // 步骤 23：用 hasRecognitionQuery 记住此处真假结果（false），供下一条 if 选择执行路径。
        bool hasRecognitionQuery = false;
        // 步骤 24：逐个读取 XmlNode node in queries.GetElementsByTagName("action") 中的元素；每轮处理当前一个，直到全部遍历完。
        foreach (XmlNode node in queries.GetElementsByTagName("action"))
            // 步骤 25：这个 if 的条件写在多行：当前行提出第一项检查，下一行继续补足；所有必要条件成立才执行后续语句。
            if (node is XmlElement action &&
                // 步骤 26：调用 GetAttribute：获取Attribute。
                action.GetAttribute("name", AndroidNamespace) == "android.speech.RecognitionService")
                // 步骤 27：把 hasRecognitionQuery 设为 true（启用/已发生）；后续条件判断会依据这个状态。
                hasRecognitionQuery = true;
        // 步骤 28：这里的 ! 表示取反；当 hasRecognitionQuery 不成立时才进入此分支。
        if (!hasRecognitionQuery)
        {
            // 步骤 29：计算 document.CreateElement("intent")，把结果存进 intent（intent）；之后引用 intent 时用的就是这次结果。
            XmlElement intent = document.CreateElement("intent");
            // 步骤 30：计算 document.CreateElement("action")，把结果存进 action（按钮对应的操作枚举）；之后引用 action 时用的就是这次结果。
            XmlElement action = document.CreateElement("action");
            // 步骤 31：调用 SetAttribute：设置Attribute。
            action.SetAttribute("name", AndroidNamespace, "android.speech.RecognitionService");
            // 步骤 32：调用 AppendChild：AppendChild。
            intent.AppendChild(action);
            // 步骤 33：调用 AppendChild：AppendChild。
            queries.AppendChild(intent);
        }
        // 步骤 34：调用 Save：保存。
        document.Save(manifestPath);
    }
    // ！！！功能结束：OnPostGenerateGradleAndroidProject！！！
}
