using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 在编辑器模拟右手射线和扳机，不需要连接头显；测试结束恢复偏好并清理临时对象。
public static class SkyQuestSpeedSliderValidation
{
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
    private const string Pref = "SkyQuestMovementScaleV2";
    private const string Report = "Library/QuestSpeedSliderValidation.txt";

    [MenuItem("SKY/Validate VR speed slider")]
    public static void Run()
    {
        var lines = new List<string> { "VR speed slider validation: " + DateTime.Now.ToString("u") };
        bool hadPref = PlayerPrefs.HasKey(Pref);
        float savedPref = PlayerPrefs.GetFloat(Pref);
        var root = new GameObject("Temporary speed slider validation") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            root.transform.position = new Vector3(9000f, 9000f, 9000f);
            var rig = root.AddComponent<SkyQuestLocomotion>();
            rig.desktopControlled = false;
            var hud = new GameObject("HUD").transform;
            hud.SetParent(root.transform, false);
            hud.localPosition = new Vector3(0f, -.55f, 1.05f);
            hud.localScale = Vector3.one * .7225f;
            var track = new GameObject("Track").transform;
            track.SetParent(hud, false);
            track.localPosition = new Vector3(0f, -.037f, 0f);
            track.localScale = new Vector3(.34f, .027f, .022f);
            var area = track.gameObject.AddComponent<BoxCollider>();
            area.size = new Vector3(1.3f, 6f, 3f);
            area.isTrigger = true;
            typeof(SkyQuestLocomotion).GetField("vrSpeedTrack", Private).SetValue(rig, track);
            typeof(SkyQuestLocomotion).GetField("vrSpeedHitArea", Private).SetValue(rig, area);
            var update = typeof(SkyQuestLocomotion).GetMethod("UpdateVrSpeedSlider", Private,
                null, new[] { typeof(Ray), typeof(bool) }, null);
            var capture = typeof(SkyQuestLocomotion).GetField("speedSliderCapturesTrigger", Private);
            Ray Aim(float x, float y = 0f) => new Ray(
                track.TransformPoint(new Vector3(x, y, 0f)) - track.forward, track.forward);
            void Step(float x, bool pressed = true, float y = 0f) =>
                update.Invoke(rig, new object[] { Aim(x, y), pressed });
            bool Captured() => (bool)capture.GetValue(rig);
            void Check(bool pass, string label)
            {
                lines.Add((pass ? "PASS: " : "FAIL: ") + label);
                if (!pass) throw new Exception(label);
            }
            bool Speed(float expected) => Mathf.Abs(rig.movementScale - expected) < .02f;

            var blocker = new GameObject("Avatar or held-object blocker");
            blocker.transform.SetParent(root.transform, false);
            blocker.transform.position = Aim(0f).GetPoint(.5f);
            blocker.transform.localScale = Vector3.one * .15f;
            var obstruction = blocker.AddComponent<BoxCollider>();
            Physics.SyncTransforms();
            Check(Physics.Raycast(Aim(0f), out var hit, 3f) && hit.collider == obstruction,
                "Regression setup: the original scene ray hits an obstruction before the bar");
            Step(0f);
            Check(Captured() && Speed(2.5f), "Right trigger selects the bar despite an intervening collider");
            Step(-.5f);
            Check(Speed(1f), "Left endpoint selects 1x");
            Step(.5f);
            Check(Speed(4f), "Right endpoint selects 4x");
            Step(-2f);
            Check(Speed(1f), "Dragging beyond the left endpoint clamps at 1x");
            Step(2f);
            Check(Speed(4f), "Dragging beyond the right endpoint clamps at 4x");
            Step(0f, false);
            Check(!Captured() && Speed(4f), "Release ends capture without changing the speed");
            Step(2f);
            Check(!Captured(), "Holding the trigger away from the bar does not capture it");
            Step(0f);
            Check(Captured() && Speed(2.5f), "Aiming onto the bar while already holding the trigger starts dragging");
            Step(0f, false);
            Step(.6f, true, 2.5f);
            Check(Captured() && Speed(4f), "The larger selection area works with the reduced HUD scale");
            track.gameObject.SetActive(false);
            Step(0f);
            Check(!Captured() && Speed(4f), "Hidden HUD releases capture and cannot change speed");
            track.gameObject.SetActive(true);
            Step(0f);
            typeof(SkyQuestLocomotion).GetProperty("ThirdPerson").GetSetMethod(true).Invoke(rig, new object[] { true });
            Step(-.5f);
            Check(!Captured() && Speed(2.5f), "Third-person mode releases capture");
            typeof(SkyQuestLocomotion).GetProperty("ThirdPerson").GetSetMethod(true).Invoke(rig, new object[] { false });
            root.transform.position += new Vector3(2f, 1f, -3f);
            root.transform.rotation = Quaternion.Euler(15f, 35f, 5f);
            Step(-.25f);
            Check(Captured() && Speed(1.75f), "Head movement and rotation use the current HUD pose without a physics sync");
            lines.Add("ALL PASSED");
            Debug.Log("VR speed slider validation passed. Report: " + Report);
        }
        catch (Exception ex)
        {
            lines.Add(ex.ToString());
            throw;
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            if (hadPref) PlayerPrefs.SetFloat(Pref, savedPref); else PlayerPrefs.DeleteKey(Pref);
            File.WriteAllLines(Report, lines);
        }
    }

    [MenuItem("SKY/Validate speed slider and build Quest APK")]
    public static void ValidateAndBuildQuest()
    {
        Run();
        SkyQuestBuild.BuildQuestApk();
    }
}
