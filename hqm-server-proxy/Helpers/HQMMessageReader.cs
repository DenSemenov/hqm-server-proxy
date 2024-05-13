using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public struct HQMMessageReader
{
    public HQMMessageReader(byte[] input_data)
    {
        this.buf = new BitArray(input_data);
        this.buf_length = this.buf.Length;
    }

    // Token: 0x0600004A RID: 74 RVA: 0x00006B13 File Offset: 0x00004D13
    public int GetPos()
    {
        return this.pos;
    }

    // Token: 0x0600004B RID: 75 RVA: 0x00006B1C File Offset: 0x00004D1C
    public byte SafeGetByte(int in_pos)
    {
        if (in_pos < this.buf_length)
        {
            for (int i = 0; i < 8; i++)
            {
                this.safe_get_byte_chunk[i] = this.buf[in_pos + i];
            }
            this.safe_get_byte_chunk.CopyTo(this.safe_get_byte_resultarray, 0);
            return this.safe_get_byte_resultarray[0];
        }
        return 0;
    }

    // Token: 0x0600004C RID: 76 RVA: 0x00006B74 File Offset: 0x00004D74
    public byte ReadByteAligned()
    {
        this.Align();
        this.read_byte_aligned_res = this.SafeGetByte(this.pos);
        this.pos += 8;
        return this.read_byte_aligned_res;
    }

    // Token: 0x0600004D RID: 77 RVA: 0x00006BA4 File Offset: 0x00004DA4
    public List<byte> ReadBytesAligned(int n)
    {
        this.Align();
        this.read_bytes_aligned_res.Clear();
        for (int i = this.pos; i < this.pos + n * 8; i += 8)
        {
            this.read_bytes_aligned_res.Add(this.SafeGetByte(i));
        }
        this.pos += n * 8;
        return this.read_bytes_aligned_res;
    }

    // Token: 0x0600004E RID: 78 RVA: 0x00006C04 File Offset: 0x00004E04
    public ushort ReadU16Aligned()
    {
        this.Align();
        this.read_u16_aligned_b1 = (ushort)this.SafeGetByte(this.pos);
        this.read_u16_aligned_b2 = (ushort)this.SafeGetByte(this.pos + 8);
        this.pos += 16;
        return (ushort)((int)this.read_u16_aligned_b1 | (int)this.read_u16_aligned_b2 << 8);
    }

    // Token: 0x0600004F RID: 79 RVA: 0x00006C5C File Offset: 0x00004E5C
    public uint ReadU32Aligned()
    {
        this.Align();
        this.read_u32_aligned_b1 = this.SafeGetByte(this.pos);
        this.read_u32_aligned_b2 = this.SafeGetByte(this.pos + 8);
        this.read_u32_aligned_b3 = this.SafeGetByte(this.pos + 16);
        this.read_u32_aligned_b4 = this.SafeGetByte(this.pos + 24);
        this.pos += 32;
        this.read_u32_aligned_bytes[0] = this.read_u32_aligned_b1;
        this.read_u32_aligned_bytes[1] = this.read_u32_aligned_b2;
        this.read_u32_aligned_bytes[2] = this.read_u32_aligned_b3;
        this.read_u32_aligned_bytes[3] = this.read_u32_aligned_b4;
        return BitConverter.ToUInt32(this.read_u32_aligned_bytes, 0);
    }

    // Token: 0x06000050 RID: 80 RVA: 0x00006D12 File Offset: 0x00004F12
    public float ReadF32Aligned()
    {
        this.read_f32_aligned_i = this.ReadU32Aligned();
        return BitConverter.ToSingle(BitConverter.GetBytes(this.read_f32_aligned_i), 0);
    }

    // Token: 0x06000051 RID: 81 RVA: 0x00006D34 File Offset: 0x00004F34
    public uint ReadPos(byte b, uint old_value)
    {
        this.read_pos_type = this.ReadBits(2);
        this.read_pos_signed_old_value = (int)old_value;
        this.read_pos_diff = 0;
        switch (this.read_pos_type)
        {
            case 0U:
                this.read_pos_diff = this.ReadBitsSigned(3);
                return (uint)Math.Max(0, this.read_pos_signed_old_value + this.read_pos_diff);
            case 1U:
                this.read_pos_diff = this.ReadBitsSigned(6);
                return (uint)Math.Max(0, this.read_pos_signed_old_value + this.read_pos_diff);
            case 2U:
                this.read_pos_diff = this.ReadBitsSigned(12);
                return (uint)Math.Max(0, this.read_pos_signed_old_value + this.read_pos_diff);
            case 3U:
                return this.ReadBits(b);
            default:
                return 0U;
        }
    }

    // Token: 0x06000052 RID: 82 RVA: 0x00006DF2 File Offset: 0x00004FF2
    public int ReadBitsSigned(byte b)
    {
        this.read_bits_signed_a = (int)this.ReadBits(b);
        if (this.read_bits_signed_a >= 1 << (int)(b - 1))
        {
            return -1 << (int)b | this.read_bits_signed_a;
        }
        return this.read_bits_signed_a;
    }

    // Token: 0x06000053 RID: 83 RVA: 0x00006E28 File Offset: 0x00005028
    public uint ReadBits(byte b)
    {
        this.read_bits_bits_remaining = b;
        this.read_bits_res = 0U;
        this.read_bits_p = 0;
        while (this.read_bits_bits_remaining > 0)
        {
            this.read_bits_pos_w_bits = (byte)(8 - bit_pos);
            this.read_bits_bits = Math.Min(this.read_bits_bits_remaining, this.read_bits_pos_w_bits);
            this.read_bits_mask = ~(uint.MaxValue << (int)this.read_bits_bits);
            this.read_bits_a = ((uint)this.SafeGetByte(this.pos) >> (int)this.bit_pos & this.read_bits_mask);
            this.read_bits_res |= this.read_bits_a << (int)this.read_bits_p;
            if (this.read_bits_bits_remaining >= this.read_bits_pos_w_bits)
            {
                this.read_bits_bits_remaining -= this.read_bits_pos_w_bits;
                this.bit_pos = 0;
                this.pos += 8;
                this.read_bits_p += this.read_bits_bits;
            }
            else
            {
                this.bit_pos += this.read_bits_bits_remaining;
                this.read_bits_bits_remaining = 0;
            }
        }
        return this.read_bits_res;
    }

    // Token: 0x06000054 RID: 84 RVA: 0x00006F41 File Offset: 0x00005141
    public void Align()
    {
        if (this.bit_pos > 0)
        {
            this.bit_pos = 0;
            this.pos += 8;
        }
    }

    // Token: 0x06000055 RID: 85 RVA: 0x00006F61 File Offset: 0x00005161
    public void Next()
    {
        this.bit_pos = 0;
        this.pos += 8;
    }

    // Token: 0x040000B4 RID: 180
    private int buf_length;

    // Token: 0x040000B5 RID: 181
    private BitArray buf;

    // Token: 0x040000B6 RID: 182
    public int pos;

    // Token: 0x040000B7 RID: 183
    private byte bit_pos;

    // Token: 0x040000B8 RID: 184
    private BitArray safe_get_byte_chunk = new BitArray(8);

    // Token: 0x040000B9 RID: 185
    private byte[] safe_get_byte_resultarray = new byte[1];

    // Token: 0x040000BA RID: 186
    private byte read_byte_aligned_res;

    // Token: 0x040000BB RID: 187
    private List<byte> read_bytes_aligned_res = new List<byte>();

    // Token: 0x040000BC RID: 188
    private ushort read_u16_aligned_b1;

    // Token: 0x040000BD RID: 189
    private ushort read_u16_aligned_b2;

    // Token: 0x040000BE RID: 190
    private byte read_u32_aligned_b1;

    // Token: 0x040000BF RID: 191
    private byte read_u32_aligned_b2;

    // Token: 0x040000C0 RID: 192
    private byte read_u32_aligned_b3;

    // Token: 0x040000C1 RID: 193
    private byte read_u32_aligned_b4;

    // Token: 0x040000C2 RID: 194
    private byte[] read_u32_aligned_bytes = new byte[4];

    // Token: 0x040000C3 RID: 195
    private uint read_f32_aligned_i;

    // Token: 0x040000C4 RID: 196
    private uint read_pos_type;

    // Token: 0x040000C5 RID: 197
    private int read_pos_signed_old_value;

    // Token: 0x040000C6 RID: 198
    private int read_pos_diff;

    // Token: 0x040000C7 RID: 199
    private int read_bits_signed_a;

    // Token: 0x040000C8 RID: 200
    private byte read_bits_bits_remaining;

    // Token: 0x040000C9 RID: 201
    private uint read_bits_res;

    // Token: 0x040000CA RID: 202
    private byte read_bits_p;

    // Token: 0x040000CB RID: 203
    private byte read_bits_pos_w_bits;

    // Token: 0x040000CC RID: 204
    private byte read_bits_bits;

    // Token: 0x040000CD RID: 205
    private uint read_bits_mask;

    // Token: 0x040000CE RID: 206
    private uint read_bits_a;
}

public class HQMSkaterPacket
{
    public (uint, uint, uint) pos;
    public (uint, uint) rot;
    public (uint, uint, uint) stick_pos;
    public (uint, uint) stick_rot;
    public uint body_turn;
    public uint body_lean;
}

public class HQMPuckPacket
{
    public (uint, uint, uint) pos;
    public (uint, uint) rot;
}