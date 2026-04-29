using UnityEngine;

public class CreateCubeAtOrigin : MonoBehaviour
{
    [ContextMenu("在原点创建立方体")]
    public void CreateCube()
    {
        // 在(0, 0, 0)位置创建一个立方体
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.position = new Vector3(0, 0, 0);
        cube.name = "OriginCube";

        // 可选：设置立方体的大小和颜色
        cube.transform.localScale = new Vector3(1, 1, 1);
        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.blue; // 使用蓝色以便识别
        }

        Debug.Log("已在(0, 0, 0)位置创建立方体：" + cube.name);
    }
}