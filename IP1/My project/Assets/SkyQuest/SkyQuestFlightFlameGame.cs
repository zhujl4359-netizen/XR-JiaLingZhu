using TMPro;
using UnityEngine;

// The course is one scene object shared by the keyboard and Quest versions.
public sealed class SkyQuestFlightFlameGame : MonoBehaviour
{
    public SkyQuestLocomotion player;
    public Transform startMarker;
    public SkyQuestFlightFlame[] flames;
    public float roundSeconds = 90f;
    public int pointsPerFlame = 10;

    private enum RoundState { Ready, Flying, Finished }
    private RoundState state;
    private CapeEnergySystem energy;
    private SkyQuestStatus status;
    private TextMeshPro vrBoard;
    private Vector3 vrBoardDefaultScale;
    private int collected;
    private int score;
    private int bestScore;
    private float remaining;
    private float restartAllowedAt;
    private bool wasAtStart;
    private bool won;
    private bool[] collectedFlames;
    private GUIStyle headingStyle;
    private GUIStyle bodyStyle;
    private GUIStyle smallStyle;

    private void Start()
    {
        bestScore = PlayerPrefs.GetInt("SkyQuestFlightFlamesBest", 0);
        collectedFlames = new bool[flames != null ? flames.Length : 0];
        ResolvePlayer();
        ResetFlames();
    }

    private void Update()
    {
        if (player == null || !player.isActiveAndEnabled) ResolvePlayer();
        if (player == null || startMarker == null || flames == null) return;
        PlaceVrBoard();

        Vector3 body = player.desktopControlled ? player.transform.position + Vector3.up * 1.1f :
            player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;
        Vector3 startDelta = body - startMarker.position;
        bool atStart = new Vector2(startDelta.x, startDelta.z).sqrMagnitude < 2.25f &&
            Mathf.Abs(startDelta.y) < 2.5f;
        if (atStart && !wasAtStart && state != RoundState.Flying &&
            Time.time >= restartAllowedAt) BeginRound();
        wasAtStart = atStart;

        if (state != RoundState.Flying) return;
        remaining = Mathf.Max(0f, remaining - Time.deltaTime);
        for (int i = 0; i < flames.Length; i++)
        {
            SkyQuestFlightFlame flame = flames[i];
            if (flame == null || collectedFlames[i]) continue;
            if ((body - flame.transform.position).sqrMagnitude > flame.collectRadius * flame.collectRadius)
                continue;
            Collect(i);
            if (state != RoundState.Flying) break;
        }
        if (state == RoundState.Flying && remaining <= 0f) EndRound(false);
    }

    private void ResolvePlayer()
    {
        Camera main = Camera.main;
        SkyQuestLocomotion found = main != null ? main.GetComponentInParent<SkyQuestLocomotion>() : null;
        if (found == null)
        {
            foreach (SkyQuestLocomotion candidate in FindObjectsByType<SkyQuestLocomotion>(FindObjectsSortMode.None))
                if (candidate.isActiveAndEnabled && candidate.head != null &&
                    candidate.head.gameObject.activeInHierarchy)
                { found = candidate; break; }
        }
        if (found == null) return;
        player = found;
        energy = player.GetComponent<CapeEnergySystem>();
        status = player.GetComponent<SkyQuestStatus>();
        if (!player.desktopControlled && vrBoard == null && player.head != null)
        {
            vrBoard = SkyQuestVisuals.Text("Flight flame score in headset", "", player.head,
                new Vector3(.48f, .28f, 1.15f), 3.1f,
                new Color(1f, .95f, .75f), 2.6f);
            vrBoard.textWrappingMode = TextWrappingModes.NoWrap;
            vrBoard.overflowMode = TextOverflowModes.Overflow;
            vrBoardDefaultScale = vrBoard.transform.localScale;
        }
    }

    private void PlaceVrBoard()
    {
        if (vrBoard == null || player == null || player.head == null) return;
        bool thirdPerson = player.ThirdPerson;
        Transform parent = thirdPerson ? player.transform : player.head;
        if (vrBoard.transform.parent == parent) return;
        vrBoard.transform.SetParent(parent, false);
        vrBoard.transform.localPosition = thirdPerson ?
            new Vector3(.5f, 2.32f, 2.15f) : new Vector3(.48f, .28f, 1.15f);
        vrBoard.transform.localRotation = Quaternion.identity;
        vrBoard.transform.localScale = vrBoardDefaultScale * (thirdPerson ? 1.45f : 1f);
    }

    private void BeginRound()
    {
        state = RoundState.Flying;
        remaining = roundSeconds;
        collected = score = 0;
        won = false;
        ResetFlames();
        energy = player.GetComponent<CapeEnergySystem>();
        energy?.RestoreEnergy(energy.maxEnergy);
        status?.Show("Flight started! Collect all " + flames.Length + " flames.", 5f);
    }

    private void Collect(int index)
    {
        collectedFlames[index] = true;
        collected++;
        score += pointsPerFlame;
        flames[index].gameObject.SetActive(false);
        status?.PlayConfirm();
        status?.Show("Flame " + collected + "/" + flames.Length + "  +" + pointsPerFlame + " points", 2f);
        if (collected >= flames.Length) EndRound(true);
    }

    private void EndRound(bool completed)
    {
        if (state != RoundState.Flying) return;
        state = RoundState.Finished;
        won = completed;
        if (completed) score += Mathf.CeilToInt(remaining) * 2;
        if (score > bestScore)
        {
            bestScore = score;
            PlayerPrefs.SetInt("SkyQuestFlightFlamesBest", bestScore);
            PlayerPrefs.Save();
        }
        energy = player != null ? player.GetComponent<CapeEnergySystem>() : null;
        energy?.RestoreEnergy(energy.maxEnergy);
        restartAllowedAt = Time.time + 1f;
        status?.Show((completed ? "Flight complete!" : "Time up!") +
            " Score " + score + ". Cape full.", 6f);
    }

    private void ResetFlames()
    {
        if (flames == null) return;
        if (collectedFlames == null || collectedFlames.Length != flames.Length)
            collectedFlames = new bool[flames.Length];
        for (int i = 0; i < flames.Length; i++)
        {
            collectedFlames[i] = false;
            if (flames[i] != null) flames[i].gameObject.SetActive(true);
        }
    }

    private string FlameMarks()
    {
        string marks = "";
        for (int i = 0; i < flames.Length; i++)
            marks += collectedFlames != null && i < collectedFlames.Length && collectedFlames[i] ? "◆ " : "◇ ";
        return marks;
    }

    private string NearestFlameHint()
    {
        if (state != RoundState.Flying || player == null || flames == null) return "";
        Vector3 origin = player.desktopControlled ? player.transform.position + Vector3.up * 1.1f :
            player.head != null ? player.head.position : player.transform.position + Vector3.up * 1.1f;
        Vector3 facing = player.head != null ? player.head.forward : player.transform.forward;
        facing.y = 0f;
        if (facing.sqrMagnitude < .01f) facing = player.transform.forward;
        facing.Normalize();
        float nearest = float.PositiveInfinity;
        Vector3 target = Vector3.zero;
        for (int i = 0; i < flames.Length; i++)
        {
            if (flames[i] == null || collectedFlames[i]) continue;
            float distance = (flames[i].transform.position - origin).sqrMagnitude;
            if (distance >= nearest) continue;
            nearest = distance;
            target = flames[i].transform.position;
        }
        if (float.IsPositiveInfinity(nearest)) return "";
        Vector3 direction = target - origin;
        direction.y = 0f;
        direction.Normalize();
        float ahead = Vector3.Dot(facing, direction);
        float side = Vector3.Dot(Vector3.Cross(Vector3.up, facing), direction);
        string bearing = ahead > .72f ? "AHEAD" : ahead < -.72f ? "BEHIND" :
            side > 0f ? "RIGHT" : "LEFT";
        float height = target.y - origin.y;
        string altitude = height > 1f ? " UP " + Mathf.CeilToInt(height) + "m" :
            height < -1f ? " DOWN " + Mathf.CeilToInt(-height) + "m" : "";
        return "Next " + Mathf.CeilToInt(Mathf.Sqrt(nearest)) + "m " + bearing + altitude;
    }

    private void LateUpdate()
    {
        if (vrBoard == null) return;
        vrBoard.text = "FLIGHT FLAMES\n" + collected + "/" + (flames?.Length ?? 0) +
            "  SCORE " + score + "\n" +
            (state == RoundState.Flying ? "TIME " + Mathf.CeilToInt(remaining) + "s" :
             state == RoundState.Finished ? (won ? "COMPLETE" : "TIME UP") : "TOUCH BLUE START RING") +
            (state == RoundState.Flying ? "\n" + NearestFlameHint() : "");
    }

    private void OnGUI()
    {
        if (player == null || !player.desktopControlled || flames == null) return;
        if (headingStyle == null)
        {
            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 20,
                fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            headingStyle.normal.textColor = new Color(1f, .9f, .62f);
            bodyStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, alignment = TextAnchor.MiddleLeft };
            bodyStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, alignment = TextAnchor.MiddleCenter,
                wordWrap = true };
            smallStyle.normal.textColor = new Color(.86f, .96f, 1f);
        }
        float width = Mathf.Min(268f, Screen.width * .38f);
        Rect panel = new Rect(Screen.width - width - 15f, 100f, width, 210f);
        Color previous = GUI.color;
        GUI.color = new Color(.04f, .1f, .16f, .82f);
        GUI.DrawTexture(panel, Texture2D.whiteTexture);
        GUI.color = previous;
        GUI.Label(new Rect(panel.x + 10f, 108f, width - 20f, 30f), "FLIGHT FLAMES", headingStyle);
        GUI.Label(new Rect(panel.x + 18f, 142f, width - 30f, 23f),
            "Score " + score + "    Best " + bestScore, bodyStyle);
        GUI.Label(new Rect(panel.x + 18f, 167f, width - 30f, 23f),
            "Flames " + collected + "/" + flames.Length + "    " +
            (state == RoundState.Flying ? "Time " + Mathf.CeilToInt(remaining) + "s" : "Time --"), bodyStyle);
        GUI.Label(new Rect(panel.x + 8f, 194f, width - 16f, 26f), FlameMarks(), smallStyle);
        string instruction = state == RoundState.Flying ? "Fly through the floating flames" :
            state == RoundState.Finished ?
                (won ? "Complete!" : "Time up!") + " Energy full · Return to start" :
                "Touch the blue start ring to begin";
        GUI.Label(new Rect(panel.x + 12f, 224f, width - 24f, 44f), instruction, smallStyle);
        if (state == RoundState.Flying)
            GUI.Label(new Rect(panel.x + 12f, 271f, width - 24f, 27f), NearestFlameHint(), smallStyle);
    }
}
