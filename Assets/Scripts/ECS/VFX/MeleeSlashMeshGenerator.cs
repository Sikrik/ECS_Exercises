using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeleeSlashMeshGenerator : MonoBehaviour
{
    public void GenerateSlashMesh(float radius, float angle, int segments)
    {
        Mesh mesh = new Mesh();
        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        int[] triangles = new int[segments * 3];

        vertices[0] = Vector3.zero; // 圆心
        uvs[0] = new Vector2(0, 0.5f); // 中心 UV

        float angleStep = angle / segments;
        for (int i = 0; i <= segments; i++)
        {
            // 计算每个顶点的弧度位置
            float currentAngle = (-angle / 2f + i * angleStep) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(currentAngle) * radius, Mathf.Sin(currentAngle) * radius, 0);
            
            // 重要：U 为 1 表示边缘，V 为比例（0-1）表示弧长位置
            uvs[i + 1] = new Vector2(1, (float)i / segments);

            if (i < segments)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 2;
                triangles[i * 3 + 2] = i + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        GetComponent<MeshFilter>().mesh = mesh;
    }
}