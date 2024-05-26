using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static AbilityLevels;

public static class SpeedrunData
{
    private const string TimeSaveFile = "BestTimes.bwuda";
    private const string SaveVersionKey = "Save Version";

    private static Dictionary<string, float> _times;
    public static Dictionary<string, float> Times
    {
        get {
            if (_times == null)
                LoadData();
            return _times; 
        }
        private set { _times = value; }
    }

    public static void LoadData()
    {
        if (File.Exists(UserPath + TimeSaveFile))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream stream = new FileStream(UserPath + TimeSaveFile, FileMode.Open);

            Times = (Dictionary<string, float>)bf.Deserialize(stream);
            if (Times[SaveVersionKey]!= SaveVersion)
            {
                throw new Exception($"File migration is not setup ({Times[SaveVersionKey]} -> {SaveVersion})");
            }

            stream.Close();

            foreach (string s in Times.Keys)
            {
                Debug.Log(s + ": " + Times[s]);
            }
        }
        else
        {
            Times = new Dictionary<string, float>
            {
                { SaveVersionKey, SaveVersion }
            };
        }
    }

    public static bool SaveData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        FileStream stream = new FileStream(UserPath + TimeSaveFile, FileMode.Create);

        bf.Serialize(stream, Times);
        stream.Close();

        return true;
    }

    public static bool SetTime(string name, float time)
    {
        if (!Times.ContainsKey(name))
            Times.Add(name, time);
        else
            Times[name] = time;

        return SaveData();
    }

    public static float GetTime(string key)
    {
        return Times.ContainsKey(key) ? Times[key] : -1;
    }
}
