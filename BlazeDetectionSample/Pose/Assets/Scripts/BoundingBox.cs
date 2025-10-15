using System;
using UnityEngine;

public class BoundingBox : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public Color color;
    public float width;

    void Start()
    {
        if (lineRenderer == null)
        {
            Debug.LogError("BoundingBox: LineRenderer reference is null. Assign a LineRenderer in the Inspector.");
            return;
        }

        // Ensure we have a simple visible material
        if (lineRenderer.sharedMaterial == null)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.material = mat;
        }

        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        // Use world space so the positions passed in are treated as world coordinates
        lineRenderer.useWorldSpace = true;
        // Put the bounding box on top of most geometry
        try { lineRenderer.sortingOrder = 1000; } catch { }
    }

    public void Set(bool active, Vector3 position, Vector2 size)
    {
        if (lineRenderer == null)
            return;

        gameObject.SetActive(active);
        if (!active)
            return;

        // Ensure four corners are drawn and the loop is enabled so the box closes
        lineRenderer.positionCount = 4;
        lineRenderer.SetPosition(0, position + new Vector3(-0.5f * size.x, -0.5f * size.y, 0));
        lineRenderer.SetPosition(1, position + new Vector3(-0.5f * size.x, +0.5f * size.y, 0));
        lineRenderer.SetPosition(2, position + new Vector3(+0.5f * size.x, +0.5f * size.y, 0));
        lineRenderer.SetPosition(3, position + new Vector3(+0.5f * size.x, -0.5f * size.y, 0));
        lineRenderer.loop = true;
        lineRenderer.enabled = true;
    }
}
