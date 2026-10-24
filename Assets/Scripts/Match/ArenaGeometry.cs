using UnityEngine;

// 모든 피어가 같은 위치에 바닥과 벽을 만든다. 네트워크 오브젝트가 아니므로 판정용 충돌만 담당한다.
public static class ArenaGeometry
{
    public static void Ensure()
    {
        if (GameObject.Find("ArenaGround") == null)
        {
            CreateBox("ArenaGround", new Vector3(0f, -0.5f, 0f), new Vector3(22f, 1f, 6f), new Color(0.18f, 0.2f, 0.24f));
        }

        float wall = GameConfig.StageLimit + 0.6f;
        if (GameObject.Find("FightingWallLeft") == null)
        {
            CreateBox("FightingWallLeft", new Vector3(-wall, 1.5f, 0f), new Vector3(0.6f, 4f, 6f), new Color(0.12f, 0.13f, 0.16f));
            CreateBox("FightingWallRight", new Vector3(wall, 1.5f, 0f), new Vector3(0.6f, 4f, 6f), new Color(0.12f, 0.13f, 0.16f));
        }

        for (int i = 0; i < GameConfig.MaxPlayers; i++)
        {
            Vector3 point = GameConfig.SpawnPoint(i);
            var marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = "SpawnPoint_" + i;
            marker.transform.position = new Vector3(point.x, 0.02f, point.z);
            marker.transform.localScale = new Vector3(0.8f, 0.02f, 0.8f);
            Object.Destroy(marker.GetComponent<Collider>());
            Paint(marker, new Color(0.95f, 0.55f, 0.15f, 0.85f));
        }
    }

    static void CreateBox(string name, Vector3 position, Vector3 scale, Color color)
    {
        var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name = name;
        box.transform.position = position;
        box.transform.localScale = scale;
        Paint(box, color);
    }

    static void Paint(GameObject target, Color color)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer == null)
        {
            return;
        }

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        if (shader == null)
        {
            renderer.material.color = color;
            return;
        }

        var material = new Material(shader);
        material.color = color;
        renderer.material = material;
    }
}
