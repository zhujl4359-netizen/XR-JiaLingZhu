using System;
using System.IO;
using System.Xml;
using UnityEditor.Android;

public sealed class SkyQuestAndroidManifest : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 9999;
    private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        string manifestPath = Path.Combine(path, "src/main/AndroidManifest.xml");
        if (!File.Exists(manifestPath)) throw new FileNotFoundException("Unity Android manifest missing", manifestPath);
        XmlDocument document = new XmlDocument();
        document.Load(manifestPath);
        XmlElement root = document.DocumentElement;
        if (root == null) throw new InvalidOperationException("Invalid Android manifest.");
        bool hasMicrophonePermission = false;
        foreach (XmlNode node in root.ChildNodes)
        {
            if (node is XmlElement element && element.LocalName == "uses-permission" &&
                element.GetAttribute("name", AndroidNamespace) == "android.permission.RECORD_AUDIO")
            {
                hasMicrophonePermission = true;
                break;
            }
        }
        if (!hasMicrophonePermission)
        {
            XmlElement permission = document.CreateElement("uses-permission");
            permission.SetAttribute("name", AndroidNamespace, "android.permission.RECORD_AUDIO");
            root.PrependChild(permission);
        }
        XmlElement queries = null;
        foreach (XmlNode node in root.ChildNodes)
            if (node is XmlElement element && element.LocalName == "queries") { queries = element; break; }
        if (queries == null)
        {
            queries = document.CreateElement("queries");
            root.AppendChild(queries);
        }
        bool hasRecognitionQuery = false;
        foreach (XmlNode node in queries.GetElementsByTagName("action"))
            if (node is XmlElement action &&
                action.GetAttribute("name", AndroidNamespace) == "android.speech.RecognitionService")
                hasRecognitionQuery = true;
        if (!hasRecognitionQuery)
        {
            XmlElement intent = document.CreateElement("intent");
            XmlElement action = document.CreateElement("action");
            action.SetAttribute("name", AndroidNamespace, "android.speech.RecognitionService");
            intent.AppendChild(action);
            queries.AppendChild(intent);
        }
        document.Save(manifestPath);
    }
}
