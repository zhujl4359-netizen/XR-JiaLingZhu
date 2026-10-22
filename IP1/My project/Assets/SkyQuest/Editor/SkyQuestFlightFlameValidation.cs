using System.Reflection;
using UnityEditor;
using UnityEngine;

// Runs the real course component in Play mode, without changing the saved scene.
public static class SkyQuestFlightFlameValidation
{
    [MenuItem("SKY/Validate flame finish and cape refill in Play mode")]
    private static void ValidateFinish()
    {
        SkyQuestFlightFlameGame game = Object.FindFirstObjectByType<SkyQuestFlightFlameGame>();
        if (!Application.isPlaying || game == null || game.player == null ||
            game.flames == null || game.flames.Length == 0)
        {
            Debug.LogError("Flight finish validation needs the desktop scene running in Play mode.");
            return;
        }

        const string bestKey = "SkyQuestFlightFlamesBest";
        bool hadBest = PlayerPrefs.HasKey(bestKey);
        int previousBest = PlayerPrefs.GetInt(bestKey, 0);
        game.SendMessage("BeginRound");
        CapeEnergySystem energy = game.player.GetComponent<CapeEnergySystem>();
        energy.TryConsumeBoostEnergy();
        float depleted = energy.CurrentEnergy;
        for (int i = 0; i < game.flames.Length; i++) game.SendMessage("Collect", i);
        bool finished = (bool)typeof(SkyQuestFlightFlameGame).GetField("won",
            BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game);
        bool passed = finished && depleted < energy.maxEnergy &&
            Mathf.Approximately(energy.CurrentEnergy, energy.maxEnergy);
        if (hadBest) PlayerPrefs.SetInt(bestKey, previousBest);
        else PlayerPrefs.DeleteKey(bestKey);
        PlayerPrefs.Save();
        if (passed)
            Debug.Log("Flight flame finish PASS: all flames completed the round and cape energy refilled from " +
                depleted + " to " + energy.CurrentEnergy + ".");
        else
            Debug.LogError("Flight flame finish FAIL: completed " + finished +
                ", energy " + depleted + " -> " + energy.CurrentEnergy + ".");
    }

    [MenuItem("SKY/Validate flame collection in Play mode")]
    private static void ValidateCollection()
    {
        SkyQuestFlightFlameGame game = Object.FindFirstObjectByType<SkyQuestFlightFlameGame>();
        if (!Application.isPlaying || game == null || game.player == null ||
            game.flames == null || game.flames.Length == 0)
        {
            Debug.LogError("Flight flame validation needs the desktop scene running in Play mode.");
            return;
        }

        game.SendMessage("BeginRound");
        SkyQuestFlightFlame flame = game.flames[0];
        flame.enabled = false;
        flame.transform.position = game.player.transform.position + Vector3.up * 1.1f;
        int startingFrame = Time.frameCount;
        EditorApplication.CallbackFunction check = null;
        check = () =>
        {
            if (Application.isPlaying && Time.frameCount <= startingFrame + 2) return;
            EditorApplication.update -= check;
            if (!Application.isPlaying || game == null) return;
            FieldInfo scoreField = typeof(SkyQuestFlightFlameGame).GetField("score",
                BindingFlags.NonPublic | BindingFlags.Instance);
            int score = (int)scoreField.GetValue(game);
            if (score == game.pointsPerFlame && !flame.gameObject.activeSelf)
                Debug.Log("Flight flame collection PASS: touch gave " + score +
                    " points and removed the collected flame.");
            else
                Debug.LogError("Flight flame collection FAIL: score " + score +
                    ", flame active " + flame.gameObject.activeSelf);
        };
        EditorApplication.update += check;
    }
}
