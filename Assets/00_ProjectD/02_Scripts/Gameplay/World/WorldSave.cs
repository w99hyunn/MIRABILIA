using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace DoHwa.World
{
    public static class WorldSave
    {
        public static string PathFor(string slot)
        {
            if (string.IsNullOrWhiteSpace(slot) || slot.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || slot.Contains(".."))
                throw new ArgumentException("Invalid world slot", nameof(slot));
            return Path.Combine(Application.persistentDataPath, "Worlds", slot + ".json");
        }

        public static void Write(WorldState state, string slot)
        {
            string path = PathFor(slot);
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(state, true), Encoding.UTF8);
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
        }

        public static WorldState Read(string slot)
        {
            string path = PathFor(slot);
            if (!File.Exists(path)) return new WorldState();
            return WorldState.FromJson(File.ReadAllText(path));
        }
    }
}
