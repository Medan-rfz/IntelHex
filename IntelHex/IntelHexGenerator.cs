using System.Text;

namespace IntelHex;

public enum IntelHexLineType : byte
{
    Data = 0x00,
    EndOfFile = 0x01,
    ExtendSegmentAddr = 0x02,
    StartSegmentAddr = 0x03,
    ExtrendLinearAddr = 0x04,
    StartLinearAddr = 0x05
}

public struct IntelHexLine
{
    public byte length;
    public UInt16 address;
    public IntelHexLineType type;
    public byte[] data;
    public byte checksum;

    public byte[] SerializeWithoutChecksum()
    {
        using (MemoryStream stream = new MemoryStream())
        using (BinaryWriter writer = new BinaryWriter(stream))
        {
            writer.Write(length);
            writer.Write(address);
            writer.Write(((byte)type));
            writer.Write(data);
            return stream.ToArray();
        }
    }
}

public class IntelHexGenerator
{
    public string GetIntelHex(UInt32 startAddr, byte[] inputBin)
    {
        UInt32 subAddr = startAddr & 0xFFFF;
        UInt16 extendAddr = (UInt16)((startAddr >> 16) & 0xFFFF);

        if (inputBin.Length <= 0) return "";

        StringBuilder sb = new StringBuilder();

        sb.AppendLine(GetExtrendLinearAddrLine(extendAddr));

        for (int i = 0; i < inputBin.Length; i += 16)
        {
            var subArray = inputBin.Skip(i).Take(16).ToArray();
            sb.AppendLine(GetDataLine((UInt16)subAddr, subArray));
            subAddr += (UInt32)subArray.Length;

            if(subAddr > 0xFFFF)
            {
                subAddr -= 0xFFFF;
                extendAddr++;
                sb.AppendLine(GetExtrendLinearAddrLine(extendAddr));
            }
        }

        sb.AppendLine(GetEndOfFileLine());
        return sb.ToString();
    }

    private string GetEndOfFileLine()
    {
        return ":00000001FF";
    }

    private string GetDataLine(UInt16 address, byte[] data)
    {
        byte length = (byte)((data.Length < 16) ? data.Length : 16);

        var line = new IntelHexLine();
        line.length = length;
        line.address = address;
        line.type = IntelHexLineType.Data;
        line.data = data.Take(length).ToArray();

        return GetHexLine(line);
    }

    private string GetExtrendLinearAddrLine(UInt16 extendedAddr)
    {
        var line = new IntelHexLine();
        line.length = 2;
        line.address = 0;
        line.type = IntelHexLineType.ExtrendLinearAddr;
        line.data = BitConverter.GetBytes(extendedAddr).Reverse().ToArray();

        return GetHexLine(line);
    }

    private string GetHexLine(IntelHexLine intelHexLine)
    {
        intelHexLine.checksum = GetChecksum(intelHexLine);

        StringBuilder sb = new StringBuilder(":");
        sb.Append(intelHexLine.length.ToString("X2"));
        sb.Append(intelHexLine.address.ToString("X4"));
        sb.Append(((byte)intelHexLine.type).ToString("X2"));
        foreach (byte b in intelHexLine.data)
            sb.Append(b.ToString("X2"));
        sb.Append(intelHexLine.checksum.ToString("X2"));

        return sb.ToString();
    }

    private byte GetChecksum(IntelHexLine intelHexLine)
    {
        byte result = 0;
        var arr = intelHexLine.SerializeWithoutChecksum();

        foreach (byte b in arr)
            result += b;

        result = (byte)(~result + 1);

        return result;
    }
}
