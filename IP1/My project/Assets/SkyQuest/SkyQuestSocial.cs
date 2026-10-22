using TMPro;
using UnityEngine;

public class SkyQuestSocial : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    public Transform ancestor;
    public SkyQuestAncestor itemTree;
    public SkyQuestStatus status;
    public float interactionDistance = 3.2f;
    public bool desktopMode;

    public bool HeartShared { get; private set; }
    public bool CandleOffered { get; private set; }
    public bool AncestorStoryUnlocked { get; private set; }

    private TextMeshPro socialLabel;
    private float heartTimer;
    private float candleTimer;
    private float waveTimer;
    private int waveChanges;
    private int lastWaveDirection;
    private Vector3 previousRight;
    private bool previousStoryTrigger;
    private Transform storyRoot;
    private readonly string[] storyLines =
    {
        "The Ancestor followed a lantern through the wind.",
        "A shared light helped friends cross the dark sky.",
        "Their journey lives on in the gifts of this tree."
    };

    void Start()
    {
        if (ancestor == null) return;
        socialLabel = SkyQuestVisuals.Text("Social prompt", "", ancestor,
            new Vector3(0f, 2.7f, 0f), 2.4f, new Color(1f, 0.8f, 0.7f), 2.7f);
        if (rightHand != null) previousRight = rightHand.position;
    }

    void Update()
    {
        if (head == null || leftHand == null || rightHand == null || ancestor == null) return;
        bool near = Vector3.Distance(desktopMode ? transform.position + Vector3.up * 1.2f : head.position,
            ancestor.position) <= interactionDistance;
        if (socialLabel != null)
        {
            socialLabel.gameObject.SetActive(near && (itemTree == null || !itemTree.IsMenuOpen));
            socialLabel.text = desktopMode ?
                (!HeartShared ? "H: share heart-light" : !CandleOffered ? "C: offer candle" :
                 !AncestorStoryUnlocked ? "G: greet the Ancestor" : "7-9: read stories") :
                (!HeartShared ? "Bring both hands to your chest and hold grips to share heart-light" :
                 !CandleOffered ? "Hold right trigger toward the Ancestor to offer a candle" :
                 !AncestorStoryUnlocked ? "Wave your right hand to unlock the story" :
                 "Friendship complete — open the Ancestor item tree");
            socialLabel.transform.rotation = Quaternion.LookRotation(socialLabel.transform.position - head.position);
        }
        if (!near || (itemTree != null && itemTree.IsMenuOpen))
        {
            heartTimer = candleTimer = waveTimer = 0f;
            previousRight = rightHand.position;
            return;
        }
        if (desktopMode) return;

        Vector3 chest = head.position - Vector3.up * 0.3f;
        bool handsAtChest = Vector3.Distance(leftHand.position, chest) < 0.38f &&
                            Vector3.Distance(rightHand.position, chest) < 0.38f;
        if (!HeartShared)
        {
            heartTimer = handsAtChest && SkyQuestInput.Grip(SkyQuestInput.Left) > 0.7f &&
                SkyQuestInput.Grip(SkyQuestInput.Right) > 0.7f ? heartTimer + Time.deltaTime : 0f;
            if (heartTimer >= 0.6f)
            {
                HeartShared = true;
                SpawnHeartLight(chest);
                status?.Show("Heart-light shared. Friendship begun.", 4f);
                SkyQuestInput.Pulse(SkyQuestInput.Left);
                SkyQuestInput.Pulse(SkyQuestInput.Right);
            }
            previousRight = rightHand.position;
            return;
        }

        if (!CandleOffered)
        {
            Vector3 toAncestor = (ancestor.position + Vector3.up * 1.2f - rightHand.position).normalized;
            bool pointing = Vector3.Dot(rightHand.forward, toAncestor) > 0.7f;
            candleTimer = pointing && SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.75f
                ? candleTimer + Time.deltaTime : 0f;
            if (candleTimer >= 0.7f)
            {
                CandleOffered = true;
                SpawnCandle();
                status?.Show("White candle offered. Wave to hear the Ancestor's story.", 4f);
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.5f, 0.12f);
            }
            previousRight = rightHand.position;
            return;
        }

        if (!AncestorStoryUnlocked)
        {
            float velocityX = (rightHand.position.x - previousRight.x) / Mathf.Max(Time.deltaTime, 0.01f);
            int direction = velocityX > 0.45f ? 1 : velocityX < -0.45f ? -1 : 0;
            if (direction != 0 && direction != lastWaveDirection)
            {
                waveChanges++;
                lastWaveDirection = direction;
                waveTimer = Time.time;
            }
            if (Time.time - waveTimer > 2.2f) { waveChanges = 0; lastWaveDirection = 0; }
            if (waveChanges >= 4)
            {
                AncestorStoryUnlocked = true;
                status?.Show("Ancestor story unlocked. Point to a star and press right trigger.", 6f);
                CreateStoryTree();
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.65f, 0.2f);
            }
        }
        if (AncestorStoryUnlocked) UpdateStorySelection();
        previousRight = rightHand.position;
    }

    private void CreateStoryTree()
    {
        storyRoot = new GameObject("Ancestor narrative tree").transform;
        storyRoot.position = ancestor.position + Vector3.up * 2.5f;
        for (int i = 0; i < storyLines.Length; i++)
        {
            float x = (i - 1) * 0.45f;
            GameObject star = SkyQuestVisuals.Primitive("Story star " + (i + 1),
                PrimitiveType.Sphere, storyRoot, new Vector3(x, 0.28f, 0f),
                Vector3.one * 0.15f, new Color(1f, 0.84f, 0.35f), true);
            star.AddComponent<SkyQuestStoryNode>().index = i;
            SkyQuestVisuals.Text("Story label", "STORY " + (i + 1), storyRoot,
                new Vector3(x, 0f, 0f), 2f, Color.white, 0.4f);
        }
    }

    private void UpdateStorySelection()
    {
        bool trigger = SkyQuestInput.Trigger(SkyQuestInput.Right) > 0.7f;
        if (trigger && !previousStoryTrigger)
        {
            SkyQuestStoryNode nearest = null;
            float distance = float.PositiveInfinity;
            foreach (RaycastHit hit in Physics.RaycastAll(rightHand.position, rightHand.forward, 4f))
            {
                SkyQuestStoryNode node = hit.collider.GetComponent<SkyQuestStoryNode>();
                if (node == null || hit.distance >= distance) continue;
                nearest = node;
                distance = hit.distance;
            }
            if (nearest != null)
            {
                status?.Show(storyLines[nearest.index], 7f);
                SkyQuestInput.Pulse(SkyQuestInput.Right, 0.35f, 0.08f);
            }
        }
        previousStoryTrigger = trigger;
    }

    private void SpawnHeartLight(Vector3 chest)
    {
        GameObject heart = SkyQuestVisuals.Primitive("Shared heart-light", PrimitiveType.Sphere,
            transform, Vector3.zero, Vector3.one * 0.14f, new Color(1f, 0.35f, 0.55f), true);
        heart.transform.position = Vector3.Lerp(chest, ancestor.position + Vector3.up * 1.4f, 0.5f);
        SkyQuestVisuals.RemoveCollider(heart);
        Destroy(heart, 2f);
    }

    private void SpawnCandle()
    {
        GameObject candle = SkyQuestVisuals.Primitive("Friendship candle", PrimitiveType.Cylinder,
            ancestor, new Vector3(0.4f, 1.15f, 0.35f), new Vector3(0.04f, 0.2f, 0.04f), Color.white);
        SkyQuestVisuals.RemoveCollider(candle);
        GameObject flame = SkyQuestVisuals.Primitive("Candle flame", PrimitiveType.Sphere,
            candle.transform, new Vector3(0f, 0.65f, 0f), Vector3.one * 0.65f,
            new Color(1f, 0.65f, 0.17f), true);
        SkyQuestVisuals.RemoveCollider(flame);
    }

    public void DesktopShareHeart()
    {
        if (!desktopMode || HeartShared || !DesktopNear()) return;
        HeartShared = true;
        SpawnHeartLight(head.position - Vector3.up * 0.3f);
        status?.Show("Heart-light shared. Press C to offer a candle.", 5f);
    }

    public void DesktopOfferCandle()
    {
        if (!desktopMode || CandleOffered || !HeartShared || !DesktopNear()) return;
        CandleOffered = true;
        SpawnCandle();
        status?.Show("Candle offered. Press G to greet the Ancestor.", 5f);
    }

    public void DesktopGreet()
    {
        if (!desktopMode || AncestorStoryUnlocked || !CandleOffered || !DesktopNear()) return;
        AncestorStoryUnlocked = true;
        CreateStoryTree();
        status?.Show("Ancestor story unlocked. Press 7, 8 or 9 to read.", 5f);
    }

    public void DesktopReadStory(int index)
    {
        if (!desktopMode || !AncestorStoryUnlocked || !DesktopNear() ||
            index < 0 || index >= storyLines.Length) return;
        status?.Show(storyLines[index], 7f);
    }

    private bool DesktopNear()
    {
        return ancestor != null &&
            Vector3.Distance(transform.position + Vector3.up * 1.2f, ancestor.position) <= interactionDistance;
    }
}

public class SkyQuestStoryNode : MonoBehaviour
{
    public int index;
}
