using System.Collections.Generic;
using UnityEngine;



/// <summary>
/// Builds the render mesh for a single chunk. Generates only faces adjacent to air
/// (face culling) and colors them via vertex colors. Pure view layer - chunk data
/// lives in Chunk and is owned by World.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ChunkGenerator : MonoBehaviour
{

    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Color> colors = new List<Color>();
    List<Vector2> uvs = new List<Vector2>();





    void AddFace(Vector3 dir, Vector3Int localPos, Color color) { 
    
        int startIndex = vertices.Count;

        if (dir == Vector3.up)
        {
            vertices.Add(localPos + new Vector3(0, 1, 0));
            vertices.Add(localPos + new Vector3(0, 1, 1));
            vertices.Add(localPos + new Vector3(1, 1, 1));
            vertices.Add(localPos + new Vector3(1, 1, 0));
        }
        else if (dir == Vector3.down)
        {
            vertices.Add(localPos + new Vector3(0, 0, 0));
            vertices.Add(localPos + new Vector3(1, 0, 0));
            vertices.Add(localPos + new Vector3(1, 0, 1));
            vertices.Add(localPos + new Vector3(0, 0, 1));
        }
        else if (dir == Vector3.left)
        {
            vertices.Add(localPos + new Vector3(0, 0, 0));
            vertices.Add(localPos + new Vector3(0, 0, 1));
            vertices.Add(localPos + new Vector3(0, 1, 1));
            vertices.Add(localPos + new Vector3(0, 1, 0));
        }
        else if (dir == Vector3.right)
        {
            vertices.Add(localPos + new Vector3(1, 0, 0));
            vertices.Add(localPos + new Vector3(1, 1, 0));
            vertices.Add(localPos + new Vector3(1, 1, 1));
            vertices.Add(localPos + new Vector3(1, 0, 1));
        }
        else if (dir == Vector3.forward)
        {
            vertices.Add(localPos + new Vector3(0, 0, 1));
            vertices.Add(localPos + new Vector3(1, 0, 1));
            vertices.Add(localPos + new Vector3(1, 1, 1));
            vertices.Add(localPos + new Vector3(0, 1, 1));
        }
        else if (dir == Vector3.back)
        {
            vertices.Add(localPos + new Vector3(0, 0, 0));
            vertices.Add(localPos + new Vector3(0, 1, 0));
            vertices.Add(localPos + new Vector3(1, 1, 0));
            vertices.Add(localPos + new Vector3(1, 0, 0));
        }

        for(int i = 0; i < 4; i++)
        {
            colors.Add(color);
        }

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(0, 1));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(1, 0));


        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 1);
        triangles.Add(startIndex + 2);

        triangles.Add(startIndex + 0);
        triangles.Add(startIndex + 2);
        triangles.Add(startIndex + 3);
    }
    void BuildMesh() {
        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh mesh = filter.sharedMesh;
        if (mesh == null)
        {
            mesh = new Mesh();
            filter.sharedMesh = mesh;
        }
        mesh.Clear();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors = colors.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        MeshCollider collider = GetComponent<MeshCollider>();
        collider.sharedMesh = null;
        collider.sharedMesh = mesh;

    }


     public void GenerateMesh(Chunk chunk, Material material)
    {
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        uvs.Clear();
        
        GetComponent<MeshRenderer>().sharedMaterial = material;

        for (int x = 0; x < WorldSettings.ChunkWidth; x++)
            for (int y = 0; y < WorldSettings.ChunkHeight; y++)
                for (int z = 0; z < WorldSettings.ChunkWidth; z++)
                {
                    Vector3Int localPos = new Vector3Int(x, y, z);
                    BlockType type = chunk.GetBlock(localPos);

                    if (type == BlockType.Air) continue; 

                    CheckAndAddFace(chunk, localPos, Vector3Int.up);
                    CheckAndAddFace(chunk, localPos, Vector3Int.down);
                    CheckAndAddFace(chunk, localPos, Vector3Int.left);
                    CheckAndAddFace(chunk, localPos, Vector3Int.right);
                    CheckAndAddFace(chunk, localPos, new Vector3Int(0, 0, 1));  
                    CheckAndAddFace(chunk, localPos, new Vector3Int(0, 0, -1)); 
                }

        BuildMesh();
    }

    void CheckAndAddFace(Chunk chunk, Vector3Int localPos, Vector3Int direction)
    {
        Vector3Int neighborPos = localPos + direction;
        BlockType neighborType = chunk.GetBlock(neighborPos);

        if (neighborType == BlockType.Air)
        {
            BlockType currentType = chunk.GetBlock(localPos);
            Color color = BlockTypeHelper.GetColor(currentType);
            AddFace(direction, localPos, color);
        }
    }

    void OnDestroy()
    {
        MeshFilter filter = GetComponent<MeshFilter>();
        if (filter != null && filter.sharedMesh != null)
            Destroy(filter.sharedMesh);
    }
}
