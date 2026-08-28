using UnityEngine;

public class ProximityHighlighter : MonoBehaviour
{
    public float proximityDistance = 3f;
    public Color normalColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Renderer objectRenderer;
    private bool isPlayerNearby = false;
    private GameObject player;

    void Start()
    {
        // 获取用于更改颜色的渲染器组件
        objectRenderer = GetComponent<Renderer>();
        // 设置初始颜色
        objectRenderer.material.color = normalColor;
        // 查找一次玩家并存储引用
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        // 计算到玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

        // 检查玩家是否在接近距离内
        bool wasNearby = isPlayerNearby;
        isPlayerNearby = distanceToPlayer <= proximityDistance;

        // 处理接近高亮
        if (isPlayerNearby && !wasNearby)
        {
            // 玩家刚刚进入接近范围 - 高亮显示对象
            objectRenderer.material.color = highlightColor;
            Debug.Log(gameObject.name + " 现在已高亮！");
        }
        else if (!isPlayerNearby && wasNearby)
        {
            // 玩家刚刚离开接近范围 - 恢复正常颜色
            objectRenderer.material.color = normalColor;
            Debug.Log(gameObject.name + " 恢复了正常颜色");
        }
    }
}