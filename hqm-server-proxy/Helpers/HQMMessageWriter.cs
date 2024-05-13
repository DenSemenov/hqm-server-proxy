public struct HQMMessageWriter
{
    public byte[] buf;
    public int pos;
    public int bit_pos;

    public HQMMessageWriter(byte[] buf)
    {
        this.buf = buf;
        pos = 0;
        bit_pos = 0;
    }

    public void WriteByteAligned(byte v)
    {
        Align();
        buf[pos] = (byte)v;
        pos += 1;
    }

    public void WriteBytesAligned(byte[] v)
    {
        Align();
        foreach (byte b in v)
        {
            buf[pos] = b;
            pos += 1;
        }
    }

    public void WriteBytesAlignedPadded(int n, byte[] v)
    {
        Align();
        var m = Math.Min(n, v.Length);
        WriteBytesAligned(v[0..m]);
        if (n > m)
        {
            for (int i = 0; i < n - m; i++)
            {
                buf[pos] = 0;
                pos += 1;
            }
        }
    }

    public void WriteU32Aligned(uint v)
    {
        Align();

        buf[pos] = (byte)(v & 0xff);
        buf[pos + 1] = (byte)((v >> 8) & 0xff);
        buf[pos + 2] = (byte)((v >> 16) & 0xff);
        buf[pos + 3] = (byte)((v >> 24) & 0xff);


        pos += 4;
    }

    public void WriteF32Aligned(float v)
    {
        WriteU32Aligned(BitConverter.ToUInt32(BitConverter.GetBytes(v), 0));
    }

    public void WritePos(byte n, uint v, uint? old_v)
    {
        var diff = uint.MaxValue;
        if (old_v != null)
        {
            diff = (uint)(v - old_v);
        }
        if (diff >= -(2 ^ 2) && diff <= (2 ^ 2 - 1))
        {
            WriteBits(2, 0);
            WriteBits(3, diff);
        }
        else if (diff >= -(2 ^ 5) && diff <= (2 ^ 5 - 1))
        {
            WriteBits(2, 1);
            WriteBits(6, diff);
        }
        else if (diff >= -(2 ^ 11) && diff <= (2 ^ 11 - 1))
        {
            WriteBits(2, 2);
            WriteBits(12, diff);
        }
        else
        {
            WriteBits(2, 3);
            WriteBits(n, v);
        }
    }

    public void WriteBits(byte n, uint v)
    {
        var toWrite = n < 32 ? (~(Int32.MaxValue << n) & v) : v;
        var bitsRemaining = n;
        var p = 0;

        while (bitsRemaining > 0)
        {
            var bitsPossibleToWrite = (8 - bit_pos);
            var bits = Math.Min(bitsRemaining, bitsPossibleToWrite);
            var mask = ~(Int32.MaxValue << bits);
            var a = ((toWrite >> p) & mask);

            if (bit_pos == 0)
            {
                buf[pos] = (byte)a;
            }
            else
            {
                buf[pos] |= (byte)(a << bit_pos);
            }

            if (bitsRemaining >= bitsPossibleToWrite)
            {
                bitsRemaining -= (byte)bitsPossibleToWrite;
                pos += 1;
                bit_pos = 0;
                p += bits;
            }
            else
            {
                bit_pos += bits;
                bitsRemaining = 0;
            }
        }
    }

    public void Align()
    {
        if (bit_pos > 0)
        {
            bit_pos = 0;
            pos += 1;
        }
    }

}
