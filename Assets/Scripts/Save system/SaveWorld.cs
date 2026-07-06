using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Random = UnityEngine.Random;



/// <summary>
/// Static persistence layer. Saves the world seed plus player-modified chunks
/// (one binary file per chunk); unmodified chunks are regenerated from the seed
/// on load, so they are never written to disk.
/// </summary>
public static class SaveWorld
{

    private static string path = Path.Combine(Application.persistentDataPath, "save");
    static string ChunkPath(Vector3Int coord) => Path.Combine(path, $"chunk_{coord.x}_{coord.z}.bin");
    public static void Save(int seed, Dictionary<Vector3Int, Chunk> chunks)
    {
        Directory.CreateDirectory(path);
        File.WriteAllBytes(Path.Combine(path, "seed.bin"), BitConverter.GetBytes(seed));
        // Only modified chunks are written; untouched ones are regenerated
        // from the seed, keeping saves small and load logic simple.
        foreach (var chunk in chunks)
        {
            if (chunk.Value.IsModified)
            {
                string chunkPath = ChunkPath(chunk.Key);
                File.WriteAllBytes(chunkPath, chunk.Value.Serialize());
                chunk.Value.IsModified = false;

            }

        }
    }


    public static int GetSeed()
    {
        string seedPath = Path.Combine(path, "seed.bin");
        if (File.Exists(seedPath))
        {
            byte[] seedData = File.ReadAllBytes(seedPath);
            return BitConverter.ToInt32(seedData, 0);
        }
        else
        {
          
            int seed = Random.Range(int.MinValue, int.MaxValue);
            // No seed file means a new world with a new seed. Any leftover chunk files
            // belong to the old world and would mix foreign terrain into this one,
            // so wipe the save directory before starting fresh.
            DeleteSave();
            return seed;
        }
    }


    public static bool TryLoadChunk(Vector3Int coord, out Chunk chunk)
    {
        string chunkPath = ChunkPath(coord);
        if (File.Exists(chunkPath))
        {
            byte[] chunkData = File.ReadAllBytes(chunkPath);
            chunk = new Chunk(coord).Deserialize(chunkData);
            return true;
        }
        chunk = null;
        return false;
    }


    public static void DeleteSave()
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
}