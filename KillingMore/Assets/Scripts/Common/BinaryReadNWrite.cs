using UnityEngine;
using System.Collections;
using System.IO;

public class BinaryReadNWrite
{
    public static string ReadAsString(string file)
    {
        string fullPath = AssetsManager.GetDataPath(file);

#if !UNITY_ANDROID
        if (System.IO.File.Exists(fullPath) == false)
        {
            Game.ErrorString = "Could not found the file " + fullPath;
            Debug.LogError(Game.ErrorString);
            return string.Empty;
        }
#endif

        string textContent = null;
        if (Application.platform == RuntimePlatform.Android)
        {

#if !UNITY_EDITOR
            WWW xmlwww = new WWW(fullPath);
            while (!xmlwww.isDone) { };

            if (xmlwww.text.Length > 0)
            {
                textContent = xmlwww.text;
            }
#else
            StreamReader reader = new StreamReader(fullPath);
            textContent = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
#endif
        }
        else //default
        {
            StreamReader reader = new StreamReader(fullPath);
            textContent = reader.ReadToEnd();
            reader.Close();
            reader.Dispose();
        }
        
        return textContent;
    }

    public static byte[] ReadAsBytes(string file)
    {
        string fullPath = AssetsManager.GetDataPath(file);

#if !UNITY_ANDROID
        if (System.IO.File.Exists(fullPath) == false)
        {
            Game.ErrorString = "Could not found the file " + fullPath;
            Debug.LogError(Game.ErrorString);
            return null;
        }
#endif

        byte[] bytes = null;
        if (Application.platform == RuntimePlatform.Android)
        {

#if !UNITY_EDITOR
            WWW xmlwww = new WWW(fullPath);
            while (!xmlwww.isDone) { };

            if (xmlwww.text.Length > 0)
            {
                bytes = xmlwww.bytes;
            }
#else
            FileStream fs = new FileStream(fullPath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            bytes = new byte[fs.Length];
            reader.Read(bytes, 0, (int)fs.Length);
            reader.Close();
            fs.Close();
            fs.Dispose();
#endif
        }
        else //default
        {
            FileStream fs = new FileStream(fullPath, FileMode.Open);
            BinaryReader reader = new BinaryReader(fs);
            bytes = new byte[fs.Length];
            reader.Read(bytes, 0, (int)fs.Length);
            reader.Close();
            fs.Close();
            fs.Dispose();
        }

        return bytes;
    }

    public static StreamWriter OpenFile2WriteAsString(string file, bool append)
    {
        string fullPath = AssetsManager.GetDataPath(file);
        StreamWriter sw = new StreamWriter(fullPath, append);
        return sw;
    }

    public static BinaryWriter OpenFile2WriteAsBytes(string file, bool append)
    {
        string fullPath = AssetsManager.GetDataPath(file);
        FileStream fs = new FileStream(fullPath, append ? FileMode.Append : FileMode.OpenOrCreate);
        BinaryWriter bw = new BinaryWriter(fs);
        return bw;
    }

    public static void EndWirte(StreamWriter sw)
    {
        sw.Close();
        sw.Dispose();
    }

    public static void EndWirte(BinaryWriter bw)
    {
        Stream s = bw.BaseStream;
        bw.Close();
        s.Close();
        s.Dispose();
    }
}
