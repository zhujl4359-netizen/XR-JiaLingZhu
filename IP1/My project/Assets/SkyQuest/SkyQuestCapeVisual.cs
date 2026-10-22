using UnityEngine;
using UnityEngine.Rendering;

// A lightweight cloth and energy display for the existing COTL avatar.
// Everything is visual: no collider, camera motion, or flight physics is added here.
[DefaultExecutionOrder(120)]
public sealed class SkyQuestCapeVisual : MonoBehaviour
{
    private const int PanelCount = 12;
    private const int PanelWidthSegments = 4;
    private const int LengthSegments = 12;
    private const int GemCount = 5;
    private const int StarCount = 15;
    private const int PanelVertexCount = (PanelWidthSegments + 1) * (LengthSegments + 1);
    private static readonly Color Rose = new Color(.62f, .24f, .42f);
    private static readonly Color Pink = new Color(.92f, .49f, .67f);
    private static readonly Color Pearl = new Color(1f, .87f, .86f);
    private static readonly Color GemOutline = new Color(.31f, .16f, .28f);
    private static readonly Color DimStar = new Color(.29f, .23f, .32f);

    private SkyQuestLocomotion locomotion;
    private CapeEnergySystem energy;
    private CapeFlightController flight;
    private Mesh capeMesh;
    private Mesh liningMesh;
    private Mesh hemMesh;
    private Mesh gemOutlineMesh;
    private Mesh starMesh;
    private Mesh haloMesh;
    private Mesh sparkMesh;
    private Vector3[] capeVertices;
    private Vector3[] liningVertices;
    private Vector3[] hemVertices;
    private Color[] fabricColors;
    private Color[] liningColors;
    private Color[] hemColors;
    private Vector3[] gemOutlineVertices;
    private Vector3[] starVertices;
    private Color[] starColors;
    private Vector3[] haloVertices;
    private Color[] haloColors;
    private TrailRenderer leftRibbon;
    private TrailRenderer rightRibbon;
    private ParticleSystem boostSparks;
    private ParticleSystem flightSparks;
    private LineRenderer takeoffRing;
    private float spread;
    private float boostPulse;
    private float ringAge = 10f;
    private bool initialized;

    public void Initialize(SkyQuestLocomotion owner, CapeEnergySystem source,
        CapeFlightController controller)
    {
        if (flight != null) flight.OnBoostSucceeded -= PlayBoost;
        locomotion = owner;
        energy = source;
        flight = controller;
        if (flight != null) flight.OnBoostSucceeded += PlayBoost;
        if (!initialized) BuildVisuals();
        RefreshEnergy();
    }

    public void PlayBoost()
    {
        if (!initialized) return;
        boostPulse = 1f;
        ringAge = 0f;
        leftRibbon?.Clear();
        rightRibbon?.Clear();
        if (boostSparks != null)
        {
            boostSparks.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            boostSparks.Play(true);
        }
    }

    private void BuildVisuals()
    {
        initialized = true;
        Material cloth = Resources.Load<Material>("SkyCapeFabric");
        Material glow = Resources.Load<Material>("SkyCapeGlow");
        if (cloth == null) cloth = SkyQuestVisuals.Material(Pink);
        if (glow == null) glow = SkyQuestVisuals.Material(Pearl, true);

        // Each pleat has its own vertices and normals. The hems overlap visually
        // like the radial, scalloped panels in the supplied cape reference.
        GameObject fabric = NewVisual("Rose petal cape", cloth, out capeMesh);
        fabric.GetComponent<MeshRenderer>().shadowCastingMode = ShadowCastingMode.Off;
        NewVisual("Ivory cape lining", cloth, out liningMesh);
        NewVisual("Soft scalloped hem thickness", cloth, out hemMesh);
        int vertexCount = PanelCount * PanelVertexCount;
        capeVertices = new Vector3[vertexCount];
        liningVertices = new Vector3[vertexCount];
        fabricColors = new Color[vertexCount];
        liningColors = new Color[vertexCount];
        int[] faceTriangles = new int[PanelCount * LengthSegments * PanelWidthSegments * 6];
        int[] liningTriangles = new int[faceTriangles.Length];
        for (int panel = 0, n = 0; panel < PanelCount; panel++)
            for (int row = 0; row < LengthSegments; row++)
                for (int column = 0; column < PanelWidthSegments; column++)
                {
                    int a = panel * PanelVertexCount + row * (PanelWidthSegments + 1) + column;
                    int b = a + 1;
                    int c = a + PanelWidthSegments + 1;
                    int d = c + 1;
                    faceTriangles[n] = a; faceTriangles[n + 1] = c; faceTriangles[n + 2] = b;
                    faceTriangles[n + 3] = b; faceTriangles[n + 4] = c; faceTriangles[n + 5] = d;
                    liningTriangles[n] = a; liningTriangles[n + 1] = b; liningTriangles[n + 2] = c;
                    liningTriangles[n + 3] = b; liningTriangles[n + 4] = d; liningTriangles[n + 5] = c;
                    n += 6;
                }
        capeMesh.vertices = capeVertices;
        capeMesh.triangles = faceTriangles;
        capeMesh.MarkDynamic();
        liningMesh.vertices = liningVertices;
        liningMesh.triangles = liningTriangles;
        liningMesh.MarkDynamic();

        int hemColumns = PanelCount * (PanelWidthSegments + 1);
        hemVertices = new Vector3[hemColumns * 2];
        hemColors = new Color[hemVertices.Length];
        int[] hemTriangles = new int[PanelCount * PanelWidthSegments * 6];
        for (int panel = 0, n = 0; panel < PanelCount; panel++)
            for (int column = 0; column < PanelWidthSegments; column++)
            {
                int a = (panel * (PanelWidthSegments + 1) + column) * 2;
                int b = a + 2;
                hemTriangles[n++] = a; hemTriangles[n++] = a + 1; hemTriangles[n++] = b;
                hemTriangles[n++] = b; hemTriangles[n++] = a + 1; hemTriangles[n++] = b + 1;
            }
        hemMesh.vertices = hemVertices;
        hemMesh.triangles = hemTriangles;
        hemMesh.MarkDynamic();

        NewVisual("Five dark-rimmed cape diamonds", cloth, out gemOutlineMesh);
        gemOutlineVertices = new Vector3[GemCount * 4];
        int[] outlineTriangles = new int[GemCount * 6];
        for (int i = 0; i < GemCount; i++)
        {
            int v = i * 4, t = i * 6;
            outlineTriangles[t] = v; outlineTriangles[t + 1] = v + 1; outlineTriangles[t + 2] = v + 2;
            outlineTriangles[t + 3] = v; outlineTriangles[t + 4] = v + 2; outlineTriangles[t + 5] = v + 3;
        }
        gemOutlineMesh.vertices = gemOutlineVertices;
        gemOutlineMesh.triangles = outlineTriangles;
        gemOutlineMesh.colors = FillColors(gemOutlineVertices.Length, GemOutline);
        gemOutlineMesh.MarkDynamic();

        // Three independently dimming facets per diamond preserve all 15
        // energy steps while keeping the reference's five-gem silhouette.
        NewVisual("Fifteen cape energy facets", cloth, out starMesh);
        starVertices = new Vector3[StarCount * 4];
        starColors = new Color[starVertices.Length];
        int[] starTriangles = new int[StarCount * 6];
        for (int i = 0; i < StarCount; i++)
        {
            int v = i * 4, t = i * 6;
            starTriangles[t] = v; starTriangles[t + 1] = v + 2; starTriangles[t + 2] = v + 1;
            starTriangles[t + 3] = v; starTriangles[t + 4] = v + 3; starTriangles[t + 5] = v + 2;
        }
        starMesh.vertices = starVertices;
        starMesh.triangles = starTriangles;
        starMesh.MarkDynamic();

        NewVisual("Subtle diamond glints", glow, out haloMesh);
        haloVertices = new Vector3[GemCount * 9];
        haloColors = new Color[haloVertices.Length];
        int[] haloTriangles = new int[GemCount * 12 * 3];
        for (int i = 0, n = 0; i < GemCount; i++)
        {
            int v = i * 9;
            for (int side = 0; side < 4; side++)
            {
                int next = (side + 1) % 4;
                haloTriangles[n++] = v; haloTriangles[n++] = v + 1 + side;
                haloTriangles[n++] = v + 1 + next;
                haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + side;
                haloTriangles[n++] = v + 5 + next;
                haloTriangles[n++] = v + 1 + side; haloTriangles[n++] = v + 5 + next;
                haloTriangles[n++] = v + 1 + next;
            }
        }
        haloMesh.vertices = haloVertices;
        haloMesh.triangles = haloTriangles;
        haloMesh.MarkDynamic();

        sparkMesh = new Mesh { name = "Diamond takeoff glint" };
        sparkMesh.vertices = new[] {
            new Vector3(0f, .5f, 0f), new Vector3(.18f, 0f, 0f),
            new Vector3(0f, -.5f, 0f), new Vector3(-.18f, 0f, 0f) };
        sparkMesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        sparkMesh.RecalculateBounds();
        leftRibbon = CreateRibbon("Left silver flight ribbon", glow);
        rightRibbon = CreateRibbon("Right silver flight ribbon", glow);
        boostSparks = CreateParticles("Golden takeoff sparks", glow, false);
        flightSparks = CreateParticles("Cape flight glimmer", glow, true);
        takeoffRing = CreateRing(glow);
        UpdateGeometry(0f);
    }

    private GameObject NewVisual(string name, Material material, out Mesh mesh)
    {
        GameObject obj = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        obj.transform.SetParent(transform, false);
        mesh = new Mesh { name = name };
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        return obj;
    }

    private TrailRenderer CreateRibbon(string name, Material material)
    {
        GameObject obj = new GameObject(name, typeof(TrailRenderer));
        obj.transform.SetParent(transform, false);
        TrailRenderer ribbon = obj.GetComponent<TrailRenderer>();
        ribbon.sharedMaterial = material;
        ribbon.time = .7f;
        ribbon.minVertexDistance = .065f;
        ribbon.widthMultiplier = .055f;
        ribbon.numCapVertices = 0;
        ribbon.numCornerVertices = 0;
        ribbon.shadowCastingMode = ShadowCastingMode.Off;
        ribbon.emitting = false;
        ribbon.colorGradient = new Gradient
        {
            colorKeys = new[] {
                new GradientColorKey(new Color(1f, .97f, .83f), 0f),
                new GradientColorKey(new Color(.59f, .89f, 1f), 1f) },
            alphaKeys = new[] {
                new GradientAlphaKey(.86f, 0f), new GradientAlphaKey(0f, 1f) }
        };
        return ribbon;
    }

    private ParticleSystem CreateParticles(string name, Material material, bool continuous)
    {
        GameObject obj = new GameObject(name, typeof(ParticleSystem));
        obj.transform.SetParent(transform, false);
        obj.transform.localPosition = continuous ? new Vector3(0f, .78f, .45f) :
            new Vector3(0f, .77f, .42f);
        ParticleSystem particles = obj.GetComponent<ParticleSystem>();
        particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        var main = particles.main;
        main.playOnAwake = false;
        main.loop = continuous;
        main.duration = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = continuous ? 32 : 64;
        main.startLifetime = continuous ? new ParticleSystem.MinMaxCurve(.25f, .5f) :
            new ParticleSystem.MinMaxCurve(.25f, .65f);
        main.startSpeed = continuous ? new ParticleSystem.MinMaxCurve(.12f, .45f) :
            new ParticleSystem.MinMaxCurve(.7f, 2f);
        main.startSize = continuous ? new ParticleSystem.MinMaxCurve(.025f, .055f) :
            new ParticleSystem.MinMaxCurve(.035f, .095f);
        main.startColor = continuous ? new ParticleSystem.MinMaxGradient(
            new Color(.74f, .9f, 1f, .7f), new Color(1f, .94f, .7f, .9f)) :
            new ParticleSystem.MinMaxGradient(
                new Color(1f, .85f, .47f, 1f), new Color(1f, 1f, .9f, 1f));
        var emission = particles.emission;
        emission.rateOverTime = continuous ? 0f : 0f;
        if (!continuous) emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 32) });
        var shape = particles.shape;
        shape.shapeType = continuous ? ParticleSystemShapeType.Box : ParticleSystemShapeType.Cone;
        if (continuous) shape.scale = new Vector3(1.4f, .12f, .15f);
        else { shape.radius = .28f; shape.angle = 55f; }
        var color = particles.colorOverLifetime;
        color.enabled = true;
        color.color = new ParticleSystem.MinMaxGradient(new Gradient
        {
            colorKeys = new[] {
                new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            alphaKeys = new[] {
                new GradientAlphaKey(0f, 0f), new GradientAlphaKey(1f, .14f),
                new GradientAlphaKey(0f, 1f) }
        });
        ParticleSystemRenderer renderer = obj.GetComponent<ParticleSystemRenderer>();
        renderer.sharedMaterial = material;
        renderer.renderMode = ParticleSystemRenderMode.Mesh;
        renderer.mesh = sparkMesh;
        renderer.alignment = ParticleSystemRenderSpace.View;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        if (continuous) particles.Play();
        return particles;
    }

    private LineRenderer CreateRing(Material material)
    {
        GameObject obj = new GameObject("Takeoff light ripple", typeof(LineRenderer));
        obj.transform.SetParent(transform, false);
        LineRenderer ring = obj.GetComponent<LineRenderer>();
        ring.sharedMaterial = material;
        ring.useWorldSpace = false;
        ring.positionCount = 49;
        ring.widthMultiplier = .045f;
        ring.shadowCastingMode = ShadowCastingMode.Off;
        ring.enabled = false;
        return ring;
    }

    private void LateUpdate()
    {
        if (!initialized || locomotion == null) return;
        float dt = Mathf.Min(Time.deltaTime, .05f);
        bool flying = locomotion.IsFlying;
        spread = Mathf.MoveTowards(spread, flying ? 1f : 0f, dt * (flying ? 4.5f : 3f));
        boostPulse = Mathf.MoveTowards(boostPulse, 0f, dt * 2.8f);
        UpdateGeometry(Time.time);
        UpdateRibbons(flying);
        UpdateRing(dt);
        if (flightSparks != null)
        {
            var emission = flightSparks.emission;
            emission.rateOverTime = flying ? 17f : 0f;
        }
    }

    private static Color[] FillColors(int count, Color color)
    {
        Color[] colors = new Color[count];
        for (int i = 0; i < count; i++) colors[i] = color;
        return colors;
    }

    private Vector3 CapePoint(float u, float v, float time)
    {
        float side = Mathf.Abs(u);
        float panelPosition = (u + 1f) * .5f * PanelCount;
        float acrossPleat = panelPosition - Mathf.Floor(panelPosition);
        float petalRound = Mathf.Sin(Mathf.PI * acrossPleat);
        float width = Mathf.Lerp(.16f, Mathf.Lerp(.45f, .92f, spread),
            Mathf.Pow(v, .78f));
        float wingFeathers = spread * Mathf.Pow(side, 6f) * v *
            .025f * Mathf.Sin(v * Mathf.PI * 7f);
        float x = u * width + Mathf.Sign(u) * wingFeathers;
        float hemHeight = Mathf.Lerp(.70f, .80f, spread);
        float wingLift = spread * Mathf.Pow(side, 1.7f) * .56f *
            Mathf.Pow(v, .85f);
        float restingShoulder = (1f - spread) * side * side * v * .11f;
        float petalHem = .047f * petalRound * Mathf.Pow(v, 7f);
        float y = Mathf.Lerp(1.32f, hemHeight, v) + wingLift +
            restingShoulder - petalHem;
        float depth = Mathf.Lerp(.10f, .39f + spread * .08f, v);
        float backCurve = Mathf.Lerp(
            .15f * (1f - side * side) - .25f * side * side,
            .09f * (1f - side * side), spread) * v;
        float rib = .022f * petalRound * Mathf.Pow(v, .8f);
        float flutter = Mathf.Sin(time * (3f + spread * 2.6f) +
            u * 7.5f + v * 4f) * (.006f + spread * .028f) * v * v;
        return new Vector3(x, y, depth + backCurve + rib + flutter +
            Mathf.Sin(time * 6f + u * 8f) * boostPulse * .035f * v);
    }

    private void UpdateGeometry(float time)
    {
        if (capeMesh == null) return;
        for (int panel = 0; panel < PanelCount; panel++)
            for (int row = 0; row <= LengthSegments; row++)
                for (int column = 0; column <= PanelWidthSegments; column++)
                {
                    float v = (float)row / LengthSegments;
                    float t = (float)column / PanelWidthSegments;
                    float u = -1f + 2f * (panel + t) / PanelCount;
                    int index = panel * PanelVertexCount +
                        row * (PanelWidthSegments + 1) + column;
                    capeVertices[index] = CapePoint(u, v, time);
                    liningVertices[index] = capeVertices[index] - Vector3.forward * .025f;
                    float pinkAmount = Mathf.SmoothStep(0f, 1f, v);
                    Color color = Color.Lerp(Rose, Pink, pinkAmount);
                    color = Color.Lerp(color, Pearl,
                        Mathf.SmoothStep(.70f, 1f, v) * .48f);
                    float pleatShade = .91f + .08f * Mathf.Sin(Mathf.PI * t) +
                        (panel % 2 == 0 ? .015f : 0f);
                    fabricColors[index] = color * pleatShade;
                    liningColors[index] = Color.Lerp(
                        new Color(.94f, .68f, .76f), new Color(1f, .95f, .88f), v);
                }
        capeMesh.vertices = capeVertices;
        capeMesh.colors = fabricColors;
        capeMesh.RecalculateNormals();
        capeMesh.RecalculateBounds();
        liningMesh.vertices = liningVertices;
        liningMesh.colors = liningColors;
        liningMesh.RecalculateNormals();
        liningMesh.RecalculateBounds();

        for (int panel = 0; panel < PanelCount; panel++)
            for (int column = 0; column <= PanelWidthSegments; column++)
            {
                float u = -1f + 2f * (panel +
                    (float)column / PanelWidthSegments) / PanelCount;
                int index = (panel * (PanelWidthSegments + 1) + column) * 2;
                Vector3 edge = CapePoint(u, 1f, time);
                hemVertices[index] = edge + Vector3.forward * .004f;
                hemVertices[index + 1] = edge - Vector3.forward * .028f;
                hemColors[index] = Pearl;
                hemColors[index + 1] = new Color(1f, .95f, .88f);
            }
        hemMesh.vertices = hemVertices;
        hemMesh.colors = hemColors;
        hemMesh.RecalculateNormals();
        hemMesh.RecalculateBounds();

        for (int gem = 0; gem < GemCount; gem++)
        {
            float v = .18f + gem * .16f;
            Vector3 surface = CapePoint(0f, v, time);
            float w = .029f + gem * .001f;
            float h = .036f + gem * .001f;
            Vector3 outline = surface + Vector3.forward * .055f;
            int b = gem * 4;
            gemOutlineVertices[b] = outline + Vector3.up * h;
            gemOutlineVertices[b + 1] = outline + Vector3.right * w;
            gemOutlineVertices[b + 2] = outline - Vector3.up * h;
            gemOutlineVertices[b + 3] = outline - Vector3.right * w;

            Vector3 facetCenter = outline + Vector3.forward * .008f;
            float innerW = w * .78f, innerH = h * .78f;
            for (int facet = 0; facet < 3; facet++)
            {
                int i = gem * 3 + facet, s = i * 4;
                float top = innerH * (1f - 2f * facet / 3f);
                float bottom = innerH * (1f - 2f * (facet + 1) / 3f);
                float topWidth = innerW * (1f - Mathf.Abs(top) / innerH);
                float bottomWidth = innerW * (1f - Mathf.Abs(bottom) / innerH);
                starVertices[s] = facetCenter + new Vector3(-topWidth, top, 0f);
                starVertices[s + 1] = facetCenter + new Vector3(topWidth, top, 0f);
                starVertices[s + 2] = facetCenter + new Vector3(bottomWidth, bottom, 0f);
                starVertices[s + 3] = facetCenter + new Vector3(-bottomWidth, bottom, 0f);
                float charge = energy != null ?
                    Mathf.Clamp01(energy.CurrentEnergy - i) : 1f;
                float twinkle = .95f + .05f * Mathf.Sin(time * 4f + i * 1.3f);
                Color facetColor = Color.Lerp(DimStar,
                    new Color(1f, .96f, .87f), charge * twinkle);
                for (int corner = 0; corner < 4; corner++)
                    starColors[s + corner] = facetColor;
            }

            int g = gem * 9;
            Vector3 glowCenter = facetCenter + Vector3.forward * .004f;
            float glowCharge = 0f;
            for (int facet = 0; facet < 3; facet++)
                glowCharge += energy != null ?
                    Mathf.Clamp01(energy.CurrentEnergy - gem * 3 - facet) : 1f;
            glowCharge /= 3f;
            haloVertices[g] = glowCenter;
            haloColors[g] = new Color(1f, .94f, .78f,
                glowCharge * (.07f + boostPulse * .15f));
            for (int corner = 0; corner < 4; corner++)
            {
                Vector3 offset = corner == 0 ? Vector3.up :
                    corner == 1 ? Vector3.right :
                    corner == 2 ? Vector3.down : Vector3.left;
                haloVertices[g + 1 + corner] = glowCenter +
                    offset * h * 1.08f;
                haloVertices[g + 5 + corner] = glowCenter +
                    offset * h * 1.75f;
                haloColors[g + 1 + corner] = new Color(1f, .93f, .75f,
                    glowCharge * (.045f + boostPulse * .09f));
                haloColors[g + 5 + corner] = new Color(1f, .93f, .75f, 0f);
            }
        }
        gemOutlineMesh.vertices = gemOutlineVertices;
        gemOutlineMesh.RecalculateNormals();
        gemOutlineMesh.RecalculateBounds();
        starMesh.vertices = starVertices;
        starMesh.colors = starColors;
        starMesh.RecalculateNormals();
        starMesh.RecalculateBounds();
        haloMesh.vertices = haloVertices;
        haloMesh.colors = haloColors;
        haloMesh.RecalculateBounds();
    }

    private void UpdateRibbons(bool flying)
    {
        if (leftRibbon == null || rightRibbon == null) return;
        // The ribbon anchors ignore the cloth flutter. Otherwise small wing
        // ripples turn the long silver trails into jagged hanging zigzags.
        leftRibbon.transform.localPosition = CapePoint(-.92f, .93f, 0f);
        rightRibbon.transform.localPosition = CapePoint(.92f, .93f, 0f);
        bool stream = flying && spread > .92f && locomotion.CurrentSpeed > .5f;
        leftRibbon.emitting = stream;
        rightRibbon.emitting = stream;
        if (!flying && spread <= .01f)
        {
            leftRibbon.Clear();
            rightRibbon.Clear();
        }
    }

    private void UpdateRing(float dt)
    {
        if (takeoffRing == null) return;
        ringAge += dt;
        if (ringAge >= .52f) { takeoffRing.enabled = false; return; }
        takeoffRing.enabled = true;
        float t = ringAge / .52f;
        float radius = Mathf.Lerp(.22f, 1.4f, t);
        float alpha = (1f - t) * (1f - t) * .8f;
        takeoffRing.startColor = new Color(1f, .94f, .65f, alpha);
        takeoffRing.endColor = new Color(.7f, .92f, 1f, alpha);
        for (int i = 0; i < takeoffRing.positionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / (takeoffRing.positionCount - 1);
            takeoffRing.SetPosition(i, new Vector3(Mathf.Cos(angle) * radius,
                .22f - t * .14f, Mathf.Sin(angle) * radius));
        }
    }

    private void RefreshEnergy()
    {
        if (initialized) UpdateGeometry(Time.time);
    }

    private void OnDestroy()
    {
        if (flight != null) flight.OnBoostSucceeded -= PlayBoost;
        if (capeMesh != null) Destroy(capeMesh);
        if (liningMesh != null) Destroy(liningMesh);
        if (hemMesh != null) Destroy(hemMesh);
        if (gemOutlineMesh != null) Destroy(gemOutlineMesh);
        if (starMesh != null) Destroy(starMesh);
        if (haloMesh != null) Destroy(haloMesh);
        if (sparkMesh != null) Destroy(sparkMesh);
    }
}
