using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Proximity animation shared by the legacy ancestor and gallery ancestors.
// The motion follows the supplied Sky reference: a small white light wakes on
// the memorial, unfolds into a warm floating spirit, then folds back into the
// memorial when the player leaves. The exchange input is handled elsewhere.
public class SkyQuestAncestorEmergence : MonoBehaviour
{
    public float revealDuration = 1.45f;
    public float dismissDuration = 1.25f;

    private Transform spirit;
    private Transform memorial;
    private Renderer[] spiritRenderers;
    private bool[] rendererStates;
    private Material[][] spiritMaterials;
    private Color[][] spiritBaseColors;
    private readonly List<Renderer> memorialMarks = new List<Renderer>();
    private readonly List<Color> memorialBaseColors = new List<Color>();
    private Transform leftArm, rightArm, leftForearm, rightForearm, headBone;
    private Quaternion leftArmRest, rightArmRest, leftForearmRest, rightForearmRest, headRest;
    private readonly List<Transform> clothParts = new List<Transform>();
    private readonly List<Quaternion> clothRestRotations = new List<Quaternion>();
    private Vector3 finalPosition;
    private Quaternion finalRotation;
    private Vector3 finalScale;
    private ParticleSystem motes;
    private Light spiritLight;
    private Texture2D moteTexture;
    private Coroutine motionRoutine;
    private bool initialized;
    private bool visible;
    private bool transitioning;
    private float idleTime;
    private float currentVisibility;
    private Mesh particleBake;
    private float particleCarry;

    public void Initialize(Transform spiritActor, Transform memorialStone)
    {
        if (initialized)
        {
            if (memorial == null && memorialStone != null) memorial = memorialStone;
            return;
        }
        if (spiritActor == null) return;
        initialized = true;
        spirit = spiritActor;
        memorial = memorialStone;
        finalPosition = spirit.position;
        finalRotation = spirit.rotation;
        finalScale = spirit.localScale;
        spiritRenderers = spirit.GetComponentsInChildren<Renderer>(true);
        rendererStates = new bool[spiritRenderers.Length];
        for (int i = 0; i < spiritRenderers.Length; i++)
            rendererStates[i] = spiritRenderers[i] != null && spiritRenderers[i].enabled;
        CachePoseBones();
        PrepareSpiritMaterials();
        PrepareMemorialEffects();
    }

    public void PlayReveal()
    {
        if (!initialized) return;
        EnsureSpiritCache();
        StopMotion();
        visible = false;
        transitioning = true;
        idleTime = 0f;
        particleCarry = 0f;
        RestorePose();
        SetSpiritLook(0f);
        SetRenderers(false);
        if (motes != null)
        {
            motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            motes.Play(true);
        }
        motionRoutine = StartCoroutine(RevealFromMemorial());
    }

    public void PlayDismiss()
    {
        if (!initialized) return;
        if (!visible && !transitioning) { HideImmediate(); return; }
        StopMotion();
        transitioning = true;
        visible = false;
        if (motes != null && !motes.isPlaying) motes.Play(true);
        motionRoutine = StartCoroutine(ReturnToMemorial());
    }

    public void HideImmediate()
    {
        if (!initialized) return;
        StopMotion();
        transitioning = false;
        visible = false;
        currentVisibility = 0f;
        spirit.position = finalPosition;
        spirit.rotation = finalRotation;
        spirit.localScale = finalScale;
        RestorePose();
        SetSpiritLook(0f);
        SetMemorialGlow(0f);
        if (motes != null) motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (spiritLight != null) spiritLight.intensity = 0f;
        SetRenderers(false);
    }

    private IEnumerator RevealFromMemorial()
    {
        Vector3 lightPoint = MemorialLightPoint();
        Vector3 startScale = finalScale * .08f;
        spirit.position = lightPoint;
        spirit.rotation = finalRotation;
        spirit.localScale = startScale;
        SetRenderers(true);
        EmitMotes(lightPoint, 18, .12f);
        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.1f, revealDuration));
            float eased = EaseOutCubic(t);
            float visibility = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(.04f, .66f, t));
            currentVisibility = visibility;
            spirit.position = Vector3.Lerp(lightPoint, finalPosition, eased) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .18f);
            spirit.rotation = Quaternion.Slerp(finalRotation * Quaternion.Euler(0f, -12f, 0f), finalRotation, eased);
            spirit.localScale = Vector3.Lerp(startScale, finalScale, eased);
            PoseSpirit(eased, elapsed, false);
            SetSpiritLook(visibility);
            SetMemorialGlow(Mathf.Sin(t * Mathf.PI));
            if (spiritLight != null)
            {
                spiritLight.transform.position = Vector3.Lerp(lightPoint, spirit.position + Vector3.up, eased);
                spiritLight.intensity = Mathf.Sin(t * Mathf.PI) * 1.15f + visibility * .22f;
            }
            if (t < .72f) EmitMotes(Vector3.Lerp(lightPoint, spirit.position, eased), 9f, .08f);
            yield return null;
        }
        spirit.position = finalPosition;
        spirit.rotation = finalRotation;
        spirit.localScale = finalScale;
        currentVisibility = 1f;
        SetSpiritLook(1f);
        SetMemorialGlow(.12f);
        transitioning = false;
        visible = true;
        idleTime = 0f;
        motionRoutine = null;
    }

    private IEnumerator ReturnToMemorial()
    {
        Vector3 startPosition = spirit.position;
        Quaternion startRotation = spirit.rotation;
        Vector3 startScale = spirit.localScale;
        Vector3 target = MemorialLightPoint();
        float startVisibility = Mathf.Max(.05f, currentVisibility);
        EmitMotes(spirit.position + Vector3.up * .8f, 8, .08f);
        float elapsed = 0f;
        while (elapsed < dismissDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / Mathf.Max(.1f, dismissDuration));
            float eased = Mathf.SmoothStep(0f, 1f, t);
            float visibility = startVisibility * (1f - Mathf.SmoothStep(.38f, 1f, t));
            currentVisibility = visibility;
            spirit.position = Vector3.Lerp(startPosition, target, eased) + Vector3.up * (Mathf.Sin(t * Mathf.PI) * .22f);
            spirit.rotation = Quaternion.Slerp(startRotation, finalRotation * Quaternion.Euler(-10f, 10f, 0f), eased);
            spirit.localScale = Vector3.Lerp(startScale, finalScale * .07f, eased);
            PoseSpirit(1f - eased, elapsed, false);
            SetSpiritLook(visibility);
            SetMemorialGlow(Mathf.Sin(t * Mathf.PI));
            if (spiritLight != null)
            {
                spiritLight.transform.position = spirit.position + Vector3.up * .45f;
                spiritLight.intensity = visibility * .35f + Mathf.Sin(t * Mathf.PI) * .45f;
            }
            EmitBodyMotes(22f * Mathf.Sin(t * Mathf.PI));
            yield return null;
        }
        EmitMotes(target, 22, .18f);
        SetRenderers(false);
        currentVisibility = 0f;
        spirit.position = finalPosition;
        spirit.rotation = finalRotation;
        spirit.localScale = finalScale;
        RestorePose();
        elapsed = 0f;
        while (elapsed < .65f)
        {
            elapsed += Time.deltaTime;
            SetMemorialGlow(Mathf.Lerp(.75f, 0f, elapsed / .65f));
            if (spiritLight != null) spiritLight.intensity = Mathf.Lerp(.45f, 0f, elapsed / .65f);
            yield return null;
        }
        if (motes != null) motes.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        SetMemorialGlow(0f);
        transitioning = false;
        visible = false;
        motionRoutine = null;
    }

    private void LateUpdate()
    {
        if (!visible || transitioning || spirit == null) return;
        idleTime += Time.deltaTime;
        spirit.position = finalPosition + Vector3.up * (Mathf.Sin(idleTime * 1.25f) * .035f);
        spirit.rotation = finalRotation * Quaternion.Euler(0f, Mathf.Sin(idleTime * .45f) * 1.4f, 0f);
        spirit.localScale = finalScale;
        PoseSpirit(1f, idleTime, true);
        SetSpiritLook(1f);
        SetMemorialGlow(.1f + Mathf.Sin(idleTime * 1.1f) * .035f);
        if (spiritLight != null)
        {
            spiritLight.transform.position = spirit.position + Vector3.up * .8f;
            spiritLight.intensity = .2f + Mathf.Sin(idleTime * 1.1f) * .035f;
        }
        if (Mathf.Repeat(idleTime, 1.35f) < Time.deltaTime) EmitMotes(spirit.position + Vector3.up * .2f, 2, .035f);
    }

    private void StopMotion()
    {
        if (motionRoutine != null) StopCoroutine(motionRoutine);
        motionRoutine = null;
    }

    private Vector3 MemorialLightPoint()
    {
        if (memorial == null) return finalPosition + Vector3.up * 1.3f;
        Transform fallback = null;
        foreach (Transform child in memorial.GetComponentsInChildren<Transform>(true))
        {
            string n = child.name.ToLowerInvariant();
            if (n == "memorial star" || n == "memorial symbol") return child.position;
            if (fallback == null && (n.Contains("star") || n.Contains("symbol"))) fallback = child;
        }
        return fallback != null ? fallback.position : memorial.position + Vector3.up * 1.45f;
    }

    private void CachePoseBones()
    {
        Animator animator = spirit.GetComponentInChildren<Animator>(true);
        if (animator != null && animator.isHuman)
        {
            leftArm = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
            rightArm = animator.GetBoneTransform(HumanBodyBones.RightUpperArm);
            leftForearm = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
            rightForearm = animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
            headBone = animator.GetBoneTransform(HumanBodyBones.Head);
        }
        leftArm ??= FindAny("upperarm_l", "upper_arm_l", "leftarm", "left_arm", "arm_l", "hand_a_l");
        rightArm ??= FindAny("upperarm_r", "upper_arm_r", "rightarm", "right_arm", "arm_r", "hand_a_right");
        leftForearm ??= FindAny("lowerarm_l", "forearm_l", "leftforearm", "left_forearm");
        rightForearm ??= FindAny("lowerarm_r", "forearm_r", "rightforearm", "right_forearm");
        headBone ??= FindAny("head", "head_bone", "neck");
        if (leftArm != null) leftArmRest = leftArm.localRotation;
        if (rightArm != null) rightArmRest = rightArm.localRotation;
        if (leftForearm != null) leftForearmRest = leftForearm.localRotation;
        if (rightForearm != null) rightForearmRest = rightForearm.localRotation;
        if (headBone != null) headRest = headBone.localRotation;
        foreach (Transform child in spirit.GetComponentsInChildren<Transform>(true))
        {
            if (child == spirit || clothParts.Count >= 8) continue;
            string n = child.name.ToLowerInvariant();
            if (!n.Contains("cape") && !n.Contains("cloak") && !n.Contains("cloth") && !n.Contains("skirt") &&
                !n.Contains("robe") && !n.Contains("dress") && !n.Contains("hem") && !n.Contains("doupeng") && !n.Contains("fabric")) continue;
            bool nested = false;
            foreach (Transform existing in clothParts) if (child.IsChildOf(existing)) { nested = true; break; }
            if (nested) continue;
            clothParts.Add(child);
            clothRestRotations.Add(child.localRotation);
        }
    }

    private void PoseSpirit(float amount, float time, bool greeting)
    {
        float breathe = Mathf.Sin(time * 1.45f) * 2.2f * amount;
        float wave = greeting ? Mathf.Sin(time * 2.25f) * 8f : 0f;
        PoseLocal(leftArm, leftArmRest, new Vector3(-12f - breathe, 0f, 18f) * amount);
        PoseLocal(rightArm, rightArmRest, new Vector3(greeting ? -54f - wave : -12f - breathe, 0f, greeting ? -48f : -18f) * amount);
        PoseLocal(leftForearm, leftForearmRest, new Vector3(0f, 0f, -10f) * amount);
        PoseLocal(rightForearm, rightForearmRest, new Vector3(greeting ? -28f + wave * .7f : 0f, 0f, greeting ? -18f : 10f) * amount);
        PoseLocal(headBone, headRest, new Vector3(-3f * amount, greeting ? -4f : 0f, 0f));
        for (int i = 0; i < clothParts.Count; i++)
        {
            float flutter = Mathf.Sin(time * (1.05f + i * .07f) + i * .65f);
            clothParts[i].localRotation = clothRestRotations[i] * Quaternion.Euler(
                flutter * 2.1f * amount, 0f, Mathf.Sin(time * .8f + i) * 1.6f * amount);
        }
    }

    private static void PoseLocal(Transform bone, Quaternion rest, Vector3 euler)
    {
        if (bone != null) bone.localRotation = rest * Quaternion.Euler(euler);
    }

    private void RestorePose()
    {
        if (leftArm != null) leftArm.localRotation = leftArmRest;
        if (rightArm != null) rightArm.localRotation = rightArmRest;
        if (leftForearm != null) leftForearm.localRotation = leftForearmRest;
        if (rightForearm != null) rightForearm.localRotation = rightForearmRest;
        if (headBone != null) headBone.localRotation = headRest;
        for (int i = 0; i < clothParts.Count; i++) if (clothParts[i] != null) clothParts[i].localRotation = clothRestRotations[i];
    }

    private void PrepareSpiritMaterials()
    {
        if (spiritRenderers == null) return;
        spiritMaterials = new Material[spiritRenderers.Length][];
        spiritBaseColors = new Color[spiritRenderers.Length][];
        for (int i = 0; i < spiritRenderers.Length; i++)
        {
            Renderer renderer = spiritRenderers[i];
            if (renderer == null) continue;
            Material[] copies = renderer.materials;
            spiritMaterials[i] = copies;
            spiritBaseColors[i] = new Color[copies.Length];
            for (int j = 0; j < copies.Length; j++)
            {
                Material material = copies[j];
                if (material == null) continue;
                spiritBaseColors[i][j] = ReadColor(material);
                ConfigureTransparent(material);
                material.EnableKeyword("_EMISSION");
            }
            renderer.materials = copies;
        }
    }

    private void PrepareMemorialEffects()
    {
        if (memorial == null) return;
        foreach (Renderer renderer in memorial.GetComponentsInChildren<Renderer>(true))
        {
            string n = renderer.name.ToLowerInvariant();
            if (!n.Contains("line") && !n.Contains("star") && !n.Contains("border") && !n.Contains("symbol") && !n.Contains("ray")) continue;
            memorialMarks.Add(renderer);
            Material material = renderer.material;
            memorialBaseColors.Add(ReadColor(material));
            material.EnableKeyword("_EMISSION");
        }
        GameObject lightObject = new GameObject("Soft white ancestor light");
        lightObject.transform.SetParent(memorial, false);
        spiritLight = lightObject.AddComponent<Light>();
        spiritLight.type = LightType.Point;
        spiritLight.color = new Color(1f, .9f, .62f);
        spiritLight.range = 2.8f;
        spiritLight.intensity = 0f;
        spiritLight.shadows = LightShadows.None;

        GameObject particleObject = new GameObject("White memorial motes");
        particleObject.transform.SetParent(memorial, false);
        motes = particleObject.AddComponent<ParticleSystem>();
        motes.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ParticleSystem.MainModule main = motes.main;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = new ParticleSystem.MinMaxCurve(.45f, 1.15f);
        main.startSpeed = new ParticleSystem.MinMaxCurve(.04f, .18f);
        main.startSize = new ParticleSystem.MinMaxCurve(.012f, .034f);
        main.startColor = new ParticleSystem.MinMaxGradient(Color.white, new Color(1f, .94f, .72f, .82f));
        main.maxParticles = 260;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        ParticleSystem.EmissionModule emission = motes.emission;
        emission.rateOverTime = 0f;
        ParticleSystem.ShapeModule shape = motes.shape;
        shape.enabled = false;
        ParticleSystem.ColorOverLifetimeModule colorLife = motes.colorOverLifetime;
        colorLife.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(new Color(1f, .95f, .76f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(.95f, .16f), new GradientAlphaKey(0f, 1f) });
        colorLife.color = gradient;
        ParticleSystemRenderer particleRenderer = particleObject.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null) shader = Shader.Find("Particles/Standard Unlit");
        if (shader != null)
        {
            Material material = new Material(shader);
            moteTexture = CreateSoftMoteTexture();
            if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", moteTexture);
            if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", moteTexture);
            WriteColor(material, Color.white);
            ConfigureTransparent(material);
            if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.One);
            particleRenderer.material = material;
        }
    }

    private static Texture2D CreateSoftMoteTexture()
    {
        Texture2D texture = new Texture2D(32, 32, TextureFormat.RGBA32, false);
        texture.name = "Soft ancestor mote";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        for (int y = 0; y < 32; y++) for (int x = 0; x < 32; x++)
        {
            float dx = (x + .5f) / 16f - 1f;
            float dy = (y + .5f) / 16f - 1f;
            float alpha = Mathf.Clamp01(1f - Mathf.Sqrt(dx * dx + dy * dy));
            texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha * alpha));
        }
        texture.Apply();
        return texture;
    }

    private void EmitMotes(Vector3 position, int count, float speed)
    {
        if (motes == null || count <= 0) return;
        for (int i = 0; i < count; i++)
        {
            Vector3 direction = Random.onUnitSphere;
            direction.y = Mathf.Abs(direction.y) * .75f + .15f;
            ParticleSystem.EmitParams emit = new ParticleSystem.EmitParams();
            emit.position = position + Random.insideUnitSphere * .13f;
            emit.velocity = direction.normalized * Random.Range(speed * .45f, speed);
            emit.startSize = Random.Range(.012f, .034f);
            emit.startColor = Color.Lerp(Color.white, new Color(1f, .94f, .72f, .88f), Random.value * .32f);
            motes.Emit(emit, 1);
        }
    }

    private void EmitMotes(Vector3 position, float rate, float speed)
    {
        particleCarry += rate * Time.deltaTime;
        int count = Mathf.FloorToInt(particleCarry);
        particleCarry -= count;
        EmitMotes(position, count, speed);
    }

    private void EmitBodyMotes(float rate)
    {
        if (motes == null || spiritRenderers == null || spiritRenderers.Length == 0) return;
        particleCarry += rate * Time.deltaTime;
        int count = Mathf.FloorToInt(particleCarry);
        particleCarry -= count;
        if (count <= 0) return;
        if (particleBake == null) particleBake = new Mesh();
        for (int n = 0; n < count; n++)
        {
            Renderer renderer = spiritRenderers[Random.Range(0, spiritRenderers.Length)];
            if (renderer == null || !renderer.enabled) continue;
            Vector3 point = renderer.bounds.center + Vector3.Scale(Random.insideUnitSphere, renderer.bounds.extents);
            if (renderer is SkinnedMeshRenderer skin)
            {
                skin.BakeMesh(particleBake);
                Vector3[] vertices = particleBake.vertices;
                if (vertices.Length > 0) point = renderer.transform.TransformPoint(vertices[Random.Range(0, vertices.Length)]);
            }
            EmitMotes(point, 1, .09f);
        }
    }

    private void SetSpiritLook(float visibility)
    {
        if (spiritMaterials == null || spiritBaseColors == null) return;
        Color warmGold = new Color(1f, .9f, .66f, 1f);
        Color softWhite = new Color(1f, .985f, .93f, 1f);
        for (int i = 0; i < spiritMaterials.Length; i++)
        {
            if (spiritMaterials[i] == null || spiritBaseColors[i] == null) continue;
            for (int j = 0; j < spiritMaterials[i].Length; j++)
            {
                Material material = spiritMaterials[i][j];
                if (material == null || j >= spiritBaseColors[i].Length) continue;
                Color source = spiritBaseColors[i][j];
                Color tint = Color.Lerp(warmGold, softWhite, Mathf.Clamp01(source.grayscale * .72f));
                Color styled = Color.Lerp(source, tint, .9f);
                styled.a = Mathf.Clamp01(visibility) * .78f;
                WriteColor(material, styled);
                if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", tint * (.18f + visibility * .28f));
            }
        }
    }

    private void SetMemorialGlow(float amount)
    {
        amount = Mathf.Clamp01(amount);
        for (int i = 0; i < memorialMarks.Count; i++)
        {
            Renderer renderer = memorialMarks[i];
            if (renderer == null) continue;
            Material material = renderer.material;
            Color baseColor = i < memorialBaseColors.Count ? memorialBaseColors[i] : Color.white;
            WriteColor(material, Color.Lerp(baseColor, new Color(1f, .96f, .75f), amount));
            if (material.HasProperty("_EmissionColor")) material.SetColor("_EmissionColor", Color.white * (amount * 1.7f));
        }
    }

    private void EnsureSpiritCache()
    {
        if (spirit == null) return;
        if (spiritRenderers == null) spiritRenderers = spirit.GetComponentsInChildren<Renderer>(true);
        if (rendererStates == null || rendererStates.Length != spiritRenderers.Length)
        {
            rendererStates = new bool[spiritRenderers.Length];
            for (int i = 0; i < spiritRenderers.Length; i++) rendererStates[i] = spiritRenderers[i] != null && spiritRenderers[i].enabled;
        }
        if (spiritMaterials == null || spiritBaseColors == null) PrepareSpiritMaterials();
    }

    private void SetRenderers(bool enabled)
    {
        if (spiritRenderers == null || rendererStates == null) return;
        for (int i = 0; i < spiritRenderers.Length && i < rendererStates.Length; i++)
            if (spiritRenderers[i] != null) spiritRenderers[i].enabled = enabled && rendererStates[i];
    }

    private Transform FindAny(params string[] names)
    {
        foreach (Transform child in spirit.GetComponentsInChildren<Transform>(true))
        {
            string candidate = child.name.ToLowerInvariant();
            foreach (string name in names) if (candidate == name || candidate.Contains(name)) return child;
        }
        return null;
    }

    private static float EaseOutCubic(float t) { t = 1f - Mathf.Clamp01(t); return 1f - t * t * t; }
    private static Color ReadColor(Material material)
    {
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        if (material.HasProperty("_Color")) return material.GetColor("_Color");
        return Color.white;
    }
    private static void WriteColor(Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }
    private static void ConfigureTransparent(Material material)
    {
        if (material.HasProperty("_Surface")) material.SetFloat("_Surface", 1f);
        if (material.HasProperty("_SrcBlend")) material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (material.HasProperty("_DstBlend")) material.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (material.HasProperty("_ZWrite")) material.SetFloat("_ZWrite", 0f);
        material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        material.DisableKeyword("_SURFACE_TYPE_OPAQUE");
        material.renderQueue = (int)RenderQueue.Transparent;
    }
    private void OnDestroy()
    {
        if (particleBake != null) Destroy(particleBake);
        if (moteTexture != null) Destroy(moteTexture);
    }
}
