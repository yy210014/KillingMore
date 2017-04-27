using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class PathManager
{
    // Head : version(2), num of paths(2)  4
    // chunk : name (32) num of pts (2), total 34
    // pt : <x, z)> (8) rotation (16) veloc (4) behavior (2) keyPt (1) behavior duration (4) Disturbance(4) total 39

    public const int PathBinaryVersion = 3;
    public const int HeadSize = 4;
    public const int ChunkSize = 34;
    public const int PtSize = 43;
    public const string DefPathFileName = "path.b";

    public static PathManager Singleton
    {
        get
        {
            return msSingleton;
        }
    }

    public int NumPaths
    {
        get
        {
            return mChunks.Count;
        }
    }

    public void Initialize()
    {
        mChunks.Clear();
        byte[] bytes = BinaryReadNWrite.ReadAsBytes(DefPathFileName);
        if (bytes != null)
        {
            if (bytes.Length < HeadSize + ChunkSize + PtSize)
            {
                Game.ErrorString = "错误的path文件长度！！";
                Game.LogError(Game.ErrorString);
                return;
            }

            // 读head
            int offset = 0;
            int v = (int)BitConverter.ToInt16(bytes, offset);
            if (v != PathBinaryVersion)
            {
                Game.ErrorString = "path文件版本不匹配，当前期望 " + PathBinaryVersion + " ，文件版本为 " + v;
                Game.LogError(Game.ErrorString);
                return;
            }

            offset += 2;
            int num = (int)BitConverter.ToInt16(bytes, offset);
            if (num <= 0)
            {
                Game.ErrorString = "错误的path数量!";
                Game.LogError(Game.ErrorString);
                return;
            }

            offset += 2;

            // 具体读取所有path
            for (int i = 0; i < num; ++i)
            {
                ReadChunk(bytes, ref offset);
            }

            Game.DebugString = "Loaded " + num + " paths.";
        }
        else
        {
            Game.ErrorString = "Failed to read data from " + DefPathFileName;
        }
    }

    public NPPath CreatePath(string name)
    {
        // slow!
        for (int i = 0; i < mChunks.Count; ++i)
        {
            if (mChunks[i].Name == name)
            {
                List<NPPath.Pt> pts = new List<NPPath.Pt>();
                int numPts = mChunks[i].Pts.Count;
                _Chunk ck = mChunks[i];
                for (int j = 0; j < numPts; ++j)
                {
                    Vector3 pos = Vector3.zero;
                    pos.x = ck.Pts[j].X;
                    pos.y = ck.Pts[j].Y;
                    pos.z = ck.Pts[j].Z;
                    pts.Add(NPPath.MakePt(pos, ck.Pts[j].Rot, ck.Pts[j].Veloc
                        , ck.Pts[j].Behavior, ck.Pts[j].BehaviorDuration, ck.Pts[j].KeyPt,
                        ck.Pts[j].Disturbance));
                }

                NPPath path = new NPPath(pts);

                Game.DebugString = "Created path " + name;

                return path;
            }
        }

        Game.ErrorString = "Failed to create path " + name;

        return null;
    }

    public void ReadChunk(byte[] bytes, ref int offset)
    {
        if (bytes.Length - offset < ChunkSize)
        {
            return;
        }

        // 32, 2, 4, 2
        _Chunk ck = new _Chunk();
        ck.Name = System.Text.Encoding.UTF8.GetString(bytes, offset, 32).TrimEnd('\0');
        offset += 32;

        int num = (int)BitConverter.ToInt16(bytes, offset);
        offset += 2;

        if (bytes.Length - offset < PtSize * num)
        {
            return;
        }

        // read pt, 8, 16, 4, 2, 1, 4, 4
        for (int i = 0; i < num; ++i)
        {
            _Pt pt;
            pt.X = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Y = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Z = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Rot.x = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Rot.y = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Rot.z = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Rot.w = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Veloc = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Behavior = BitConverter.ToInt16(bytes, offset);
            offset += 2;
            pt.KeyPt = BitConverter.ToBoolean(bytes, offset);
            offset += 1;
            pt.BehaviorDuration = BitConverter.ToSingle(bytes, offset);
            offset += 4;
            pt.Disturbance = BitConverter.ToSingle(bytes, offset);
            offset += 4;

            ck.Pts.Add(pt);
        }

        mChunks.Add(ck);

        return;
    }

    public void SavePaths(List<PathEditGroup> paths, string fileName = DefPathFileName)
    {
        int totalSize = HeadSize;
        List<NPPath> realPaths = new List<NPPath>();
        for (int i = 0; i < paths.Count; ++i)
        {
            NPPath p = paths[i].BuildNPPath();
            totalSize += ChunkSize;
            totalSize += PtSize * p.NumPts;
            realPaths.Add(p);
        }

        byte[] buffer = new byte[totalSize];
        int offset = 0;

        // head 2, 2
        byte[] vbuf = BitConverter.GetBytes((short)PathBinaryVersion);
        for (int i = 0; i < 2; ++i)
        {
            buffer[offset + i] = vbuf[i];
        }
        offset += 2;

        byte[] nbuf = BitConverter.GetBytes((short)realPaths.Count);
        for (int i = 0; i < 2; ++i)
        {
            buffer[offset + i] = nbuf[i];
        }
        offset += 2;

        for (int i = 0; i < realPaths.Count; ++i)
        {
            WriteChunk(buffer, ref offset, realPaths[i], paths[i].name, paths[i].Slice, paths[i].MinInterpolateDist);
        }

        BinaryWriter bw = BinaryReadNWrite.OpenFile2WriteAsBytes("bin/" + DefPathFileName, false);
        bw.Write(buffer);
        BinaryReadNWrite.EndWirte(bw);
    }

    public void WriteChunk(byte[] buffer, ref int offset, NPPath path, string name, int slices, float minInterploateDist)
    {
        // 32 2 4 2
        byte[] nameBuf = System.Text.Encoding.UTF8.GetBytes(name);
        for (int j = 0; j < nameBuf.Length && j < 32; ++j)
        {
            buffer[offset + j] = nameBuf[j];
        }
        offset += 32;

        byte[] numBuf = BitConverter.GetBytes((short)path.NumPts);
        for (int i = 0; i < 2; ++i)
        {
            buffer[offset + i] = numBuf[i];
        }
        offset += 2;

        for (int i = 0; i < path.NumPts; ++i)
        {
            WritePt(path.GetPtByIndex(i), buffer, ref offset);
        }
    }

    public void WritePt(NPPath.Pt pt, byte[] buffer, ref int offset)
    {
        // 8, 16, 4, 1, 1, 4, 4
        byte[] x = BitConverter.GetBytes(pt.Pos.x);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = x[i];
        }
        offset += 4;

        byte[] y = BitConverter.GetBytes(pt.Pos.y);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = y[i];
        }
        offset += 4;

        byte[] z = BitConverter.GetBytes(pt.Pos.z);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = z[i];
        }
        offset += 4;

        byte[] rx = BitConverter.GetBytes(pt.Rot.x);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = rx[i];
        }
        offset += 4;

        byte[] ry = BitConverter.GetBytes(pt.Rot.y);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = ry[i];
        }
        offset += 4;

        byte[] rz = BitConverter.GetBytes(pt.Rot.z);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = rz[i];
        }
        offset += 4;

        byte[] rw = BitConverter.GetBytes(pt.Rot.w);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = rw[i];
        }
        offset += 4;

        byte[] v = BitConverter.GetBytes(pt.Veloc);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = v[i];
        }
        offset += 4;

        // behavior
        byte[] bh = BitConverter.GetBytes(pt.Behavior);
        buffer[offset] = bh[0];
        buffer[offset + 1] = bh[1];
        offset += 2;

        // key point
        byte[] kp = BitConverter.GetBytes(pt.KeyPt);
        buffer[offset] = kp[0];
        offset += 1;

        byte[] bhd = BitConverter.GetBytes(pt.BehaviorDuration);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = bhd[i];
        }
        offset += 4;

        // Disturbance
        byte[] dis = BitConverter.GetBytes(pt.Disturbance);
        for (int i = 0; i < 4; ++i)
        {
            buffer[offset + i] = dis[i];
        }
        offset += 4;
    }

    struct _Pt
    {
        public float X;
        public float Y;
        public float Z;
        public Quaternion Rot;
        public float Veloc;
        public short Behavior;
        public bool KeyPt;
        public float BehaviorDuration;
        public float Disturbance;
    }

    class _Chunk
    {
        public string Name;
        //public int Slices;
        //public float MinInterploateDist;
        public List<_Pt> Pts = new List<_Pt>();
    }

    byte[] mNameBuffer = new byte[32];
    List<_Chunk> mChunks = new List<_Chunk>();
    static readonly PathManager msSingleton = new PathManager();
}
