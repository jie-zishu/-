using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeScript : MonoBehaviour
{
    [Header("Tape Strip Visual (绞出的磁带条)")]
    [SerializeField] private Transform stripPoint1;
    [SerializeField] private Transform stripPoint2;
    [SerializeField] private float stripWidth = 0.015f;
    [SerializeField] private Color stripColor = new Color(0.35f, 0.2f, 0.1f);
    private LineRenderer tapeLine;
    private void CreateTapeStripVisual()
    {
        if (stripPoint1 == null || stripPoint2 == null) return;

        tapeLine = gameObject.AddComponent<LineRenderer>();
        tapeLine.material = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        tapeLine.material.color = stripColor;
        tapeLine.useWorldSpace = false;
        tapeLine.numCornerVertices = 4;
        tapeLine.numCapVertices = 4;

        RebuildTapeStrip();
    }
    [ContextMenu("重建磁带条")]
    public void RebuildTapeStrip()
    {
        if (tapeLine == null || stripPoint1 == null || stripPoint2 == null) return;

        var controlPoints = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            if (child.name.StartsWith("TapePoint_") && child != stripPoint1 && child != stripPoint2)
                controlPoints.Add(child);
        }
        controlPoints.Sort((a, b) => string.Compare(a.name, b.name, System.StringComparison.Ordinal));

        // 路径: stripPoint1 → control points → stripPoint2
        int totalPoints = 2 + controlPoints.Count;
        tapeLine.positionCount = totalPoints;
        tapeLine.startWidth = stripWidth;
        tapeLine.endWidth = stripWidth;

        tapeLine.SetPosition(0, stripPoint1.localPosition);
        for (int i = 0; i < controlPoints.Count; i++)
            tapeLine.SetPosition(i + 1, controlPoints[i].localPosition);
        tapeLine.SetPosition(totalPoints - 1, stripPoint2.localPosition);
    }
    
}
