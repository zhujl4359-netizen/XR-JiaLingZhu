using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public static class CapeFlightValidation
{
    private const string Report = "Assets/SkyQuest/CapeFlightValidationReport.txt";

    [MenuItem("SKY/Validate cape flight system")]
    public static void Run()
    {
        List<string> lines = new List<string> { "Cape flight editor validation — " + DateTime.Now.ToString("u") };
        GameObject rig = null;
        GameObject firstZone = null;
        GameObject secondZone = null;
        try
        {
            InputActionAsset actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(
                "Assets/SkyQuest/Resources/CapeFlightActions.inputactions");
            Check(actions != null && actions.FindActionMap("Cape Flight") != null,
                "OpenXR Input Actions asset and map import", lines);
            Check(actions.FindAction("Cape Flight/Boost") != null &&
                actions.FindAction("Cape Flight/Move") != null,
                "Boost and steering actions exist", lines);
            Check(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>("Assets/SkyQuest/CapeFlightTest.unity") != null,
                "Saved VR test scene exists", lines);

            rig = new GameObject("Cape flight validation rig");
            CapeEnergySystem energy = rig.AddComponent<CapeEnergySystem>();
            energy.ResetEnergy();
            Check(Mathf.Approximately(energy.CurrentEnergy, 15f), "Initial energy is 15", lines);
            CapeFlightController flight = rig.AddComponent<CapeFlightController>();
            flight.Initialize(energy);
            Check(flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 14f) &&
                flight.State == CapeFlightState.Takeoff, "Ground takeoff spends exactly 1", lines);
            Check(!flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 14f),
                "Cooldown rejects repeat without charge", lines);
            flight.ReportCollision(CollisionFlags.Sides);
            Check(flight.State == CapeFlightState.Takeoff, "Wall contact does not cause landing", lines);
            flight.ReportCollision(CollisionFlags.Below);
            Check(flight.State == CapeFlightState.Takeoff, "Upward contact does not cancel takeoff", lines);
            flight.SafeStop();
            flight.ReportCollision(CollisionFlags.Below);
            Check(flight.State == CapeFlightState.Landing, "Downward ground contact enters landing", lines);
            flight.ReportCollision(CollisionFlags.Below);
            Check(flight.State == CapeFlightState.Grounded, "Stable support enters grounded", lines);

            energy.initialEnergy = 0f;
            energy.ResetEnergy();
            Check(!flight.TryBoost() && Mathf.Approximately(energy.CurrentEnergy, 0f),
                "Zero energy cannot boost", lines);
            CapeRechargeReceiver receiver = rig.AddComponent<CapeRechargeReceiver>();
            receiver.Initialize(energy, flight);
            firstZone = Zone("Slow recharge", 1f);
            secondZone = Zone("Fast recharge", 2f);
            CapeRechargeZone slow = firstZone.GetComponent<CapeRechargeZone>();
            CapeRechargeZone fast = secondZone.GetComponent<CapeRechargeZone>();
            receiver.Register(slow);
            receiver.Register(fast);
            receiver.Step(0.5f);
            Check(Mathf.Approximately(energy.CurrentEnergy, 1f),
                "Overlapping zones use highest rate only", lines);
            receiver.Unregister(fast);
            receiver.Step(0.5f);
            Check(Mathf.Approximately(energy.CurrentEnergy, 1.5f),
                "Leaving fast zone leaves slow contribution", lines);
            receiver.Unregister(slow);
            receiver.Step(0.5f);
            Check(Mathf.Approximately(energy.CurrentEnergy, 1.5f),
                "Leaving all zones stops recharge", lines);
            energy.RestoreEnergy(100f);
            Check(Mathf.Approximately(energy.CurrentEnergy, energy.maxEnergy),
                "Recharge clamps to maximum", lines);
            lines.Add("RESULT: PASS — editor checks only; Quest controller/headset checks remain manual.");
            Debug.Log("Cape flight validation passed. " + Report);
        }
        catch (Exception exception)
        {
            lines.Add("RESULT: FAIL — " + exception);
            Debug.LogError("Cape flight validation failed: " + exception);
        }
        finally
        {
            if (secondZone != null) UnityEngine.Object.DestroyImmediate(secondZone);
            if (firstZone != null) UnityEngine.Object.DestroyImmediate(firstZone);
            if (rig != null) UnityEngine.Object.DestroyImmediate(rig);
            File.WriteAllLines(Report, lines);
            AssetDatabase.ImportAsset(Report);
        }
    }

    private static GameObject Zone(string name, float rate)
    {
        GameObject obj = new GameObject(name);
        SphereCollider collider = obj.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        obj.AddComponent<CapeRechargeZone>().rechargePerSecond = rate;
        return obj;
    }

    private static void Check(bool condition, string name, List<string> lines)
    {
        lines.Add((condition ? "PASS " : "FAIL ") + name);
        if (!condition) throw new Exception(name);
    }
}
