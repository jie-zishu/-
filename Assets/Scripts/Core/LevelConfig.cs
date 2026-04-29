using UnityEngine;

public class LevelConfig : MonoBehaviour
{
    [SerializeField] private int levelIndex;
    [SerializeField] private GameObject playerSpawnPoint;
    [SerializeField] private GameObject redCube;
    [SerializeField] private GameObject[] walls;
    [SerializeField] private GameObject ground;

    private void Awake()
    {
        // 确保红色正方体有正确的标签和组件
        if (redCube != null)
        {
            redCube.tag = "Interactable";
            if (redCube.GetComponent<InteractableObject>() == null)
            {
                redCube.AddComponent<InteractableObject>();
            }
            redCube.GetComponent<InteractableObject>().levelIndex = levelIndex;
        }

        // 确保玩家出生点设置正确
        if (playerSpawnPoint != null)
        {
            // 这里可以添加玩家出生点设置逻辑
        }

        // 确保墙壁有正确的碰撞器
        foreach (GameObject wall in walls)
        {
            if (wall.GetComponent<BoxCollider>() == null)
            {
                wall.AddComponent<BoxCollider>();
            }
        }

        // 确保地面有正确的碰撞器
        if (ground != null && ground.GetComponent<BoxCollider>() == null)
        {
            ground.AddComponent<BoxCollider>();
        }
    }

    // 在Unity编辑器中调用此方法来配置场景
    public void ConfigureLevel()
    {
        // 重置场景配置
        ResetScene();

        // 配置红色正方体
        ConfigureRedCube();

        // 配置玩家出生点
        ConfigurePlayerSpawnPoint();

        // 配置墙壁
        ConfigureWalls();

        // 配置地面
        ConfigureGround();
    }

    private void ResetScene()
    {
        // 清理现有配置
    }

    private void ConfigureRedCube()
    {
        if (redCube != null)
        {
            redCube.tag = "Interactable";
            if (redCube.GetComponent<InteractableObject>() == null)
            {
                redCube.AddComponent<InteractableObject>();
            }
            redCube.GetComponent<InteractableObject>().levelIndex = levelIndex;

            // 设置红色正方体的材质为红色
            Renderer renderer = redCube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Color.red;
            }
        }
    }

    private void ConfigurePlayerSpawnPoint()
    {
        if (playerSpawnPoint != null)
        {
            // 设置玩家出生点位置
        }
    }

    private void ConfigureWalls()
    {
        foreach (GameObject wall in walls)
        {
            if (wall.GetComponent<BoxCollider>() == null)
            {
                wall.AddComponent<BoxCollider>();
            }
        }
    }

    private void ConfigureGround()
    {
        if (ground != null && ground.GetComponent<BoxCollider>() == null)
        {
            ground.AddComponent<BoxCollider>();
        }
    }
}