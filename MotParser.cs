using System;
using System.IO;
using System.Collections.Generic;

namespace Afteralive {

class BinaryReaderBE : BinaryReader {
    private bool IsLE;
    public BinaryReaderBE(System.IO.Stream stream)  : base(stream) {
	IsLE = BitConverter.IsLittleEndian;
    }

    public override int ReadInt32() {
	return ReadInt32() - 0x7ffffff;
    }

    public override UInt32 ReadUInt32() {
        var data = base.ReadBytes(4);
        if(IsLE) Array.Reverse(data);
        return BitConverter.ToUInt32(data, 0);
    }

    public override Int16 ReadInt16() {
	return (Int16)((Int16)(ReadUInt16()) - (Int16)0x7ff);
    }

    public override UInt16 ReadUInt16() {
	var data = base.ReadBytes(2);
	if(IsLE) Array.Reverse(data);
	return BitConverter.ToUInt16(data, 0);
    }

    public override Int64 ReadInt64() {
        var data = base.ReadBytes(8);
        if(IsLE) Array.Reverse(data);
        return BitConverter.ToInt64(data, 0);
    }
}

enum MotIdType {
    Long = 0,
    Step,
    PosX,
    PosY,
    PartPosDir,
    PartPosDis,
    PartAngle,
    PartPicture,
    PartScaleX,
    PartScaleY
}

public class MotParser {
    public static MotFile Parse(Stream s) {
	var r = new BinaryReaderBE(s);
	var d = new MotFile();
	int cmd = 0;
	do {
	    cmd = r.ReadByte();
	    ExecuteCmd(cmd, r, d);
	   }while(cmd != 255 && r.BaseStream.Position < r.BaseStream.Length); // eof
	return d;
    }

    private static void ExecuteCmd(int cmd, BinaryReader r, MotFile s) { // s: Subject
	switch((MotIdType)cmd) {
	    case MotIdType.Long:
		s.Long = (int)r.ReadUInt16();
		break;
	    case MotIdType.Step: {
		var i = r.ReadByte();
		s.KeyFrames[i] = r.ReadUInt16();
		break;
	    }
	    case MotIdType.PosX: {
		var i = r.ReadByte();
		s.PosX[i] = r.ReadInt16();
		break;
	    }
	    case MotIdType.PosY: {
		var i = r.ReadByte();
		s.PosY[i] = r.ReadInt16();
		break;
	    }
	    // Read Parts
	    case MotIdType.PartPosDir: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).Directions[f] = r.ReadInt32();
		break;
	    }
	    case MotIdType.PartPosDis: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).Distances[f] = r.ReadInt32();
		break;
	    }
	    case MotIdType.PartAngle: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).Angles[f] = r.ReadInt16();
		break;
	    }
	    case MotIdType.PartPicture: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).Pictures[f] = r.ReadInt16();
		break;
	    }
	    case MotIdType.PartScaleX: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).ScaleX[f] = r.ReadInt16();
		break;
	    }
	    case MotIdType.PartScaleY: {
		byte f, p;
		f = r.ReadByte();p = r.ReadByte();
		s.GetPart(p).ScaleY[f] = r.ReadInt16();
		break;
	    }
	}
    }
}

public class MotPartData {
    //    private class
    public int PartId {get; set;}
    public Dictionary<int,int> Directions {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> Distances {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> Angles {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> Pictures {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> ScaleX {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> ScaleY {get; set;} = new Dictionary<int,int>();
}

public class MotFile {
    public int Long {get; set;} = 0;
    public Dictionary<int,int> KeyFrames {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> PosX {get; set;} = new Dictionary<int,int>();
    public Dictionary<int,int> PosY {get; set;} = new Dictionary<int,int>();

    public Dictionary<int,MotPartData> Parts {get; set;} = new Dictionary<int,MotPartData>();
    public MotPartData GetPart(int id) {
	MotPartData sub;
	if(Parts.TryGetValue(id, out sub)) {
	    return sub;
	}else{
	    sub = new MotPartData();
	    sub.PartId = id;
	    Parts[id] = sub;
	    return sub;
	}
    }
}
}
