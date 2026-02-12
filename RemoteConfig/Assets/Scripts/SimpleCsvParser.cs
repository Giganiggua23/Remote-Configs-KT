using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ConfigParser
{
    public static List<Weapon> ParseWeapons(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return null;

        try
        {
            return text.TrimStart().StartsWith("{")
                ? ParseJson(text)
                : ParseCsv(text);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Parse error: {ex.Message}");
            return null;
        }
    }

    static List<Weapon> ParseJson(string json)
    {
        var config = JsonUtility.FromJson<WeaponConfigList>(json);
        return config?.weapons?.Length > 0 && ValidateWeapons(config.weapons)
            ? new List<Weapon>(config.weapons)
            : null;
    }

    static List<Weapon> ParseCsv(string csv)
    {
        var lines = csv.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return null;

        var header = lines[0].Split(',');
        var idIdx = Array.FindIndex(header, h => h.Trim().Equals("id", StringComparison.OrdinalIgnoreCase));
        var dmgIdx = Array.FindIndex(header, h => h.Trim().Equals("damage", StringComparison.OrdinalIgnoreCase));
        var cdIdx = Array.FindIndex(header, h => h.Trim().Equals("cooldown", StringComparison.OrdinalIgnoreCase));

        if (idIdx == -1 || dmgIdx == -1 || cdIdx == -1) return null;

        var weapons = new List<Weapon>();
        for (int i = 1; i < lines.Length; i++)
        {
            var cols = lines[i].Split(',');
            if (cols.Length <= idIdx) continue;

            var id = cols[idIdx].Trim();
            if (string.IsNullOrEmpty(id)) continue;

            if (!int.TryParse(cols[dmgIdx], out int dmg) ||
                !float.TryParse(cols[cdIdx], System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float cd))
                return null;

            weapons.Add(new Weapon(id, dmg, cd));
        }

        return weapons.Count > 0 && ValidateWeapons(weapons) ? weapons : null;
    }

    static bool ValidateWeapons(IEnumerable<Weapon> weapons)
    {
        foreach (var w in weapons)
            if (w == null || string.IsNullOrEmpty(w.id) || w.damage < 0 || w.cooldown <= 0)
                return false;
        return true;
    }
}