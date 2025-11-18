using SupercellProxy.Playground.Network.Sides;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

var upstreamHost = args.Length > 0 ? args[0] : "game.haydaygame.com"; // "game.squadbustersgame.com"; //
var upstreamPort = args.Length > 1 && int.TryParse(args[1], out var up) ? up : 9339;
var listenAddress = args.Length > 2 ? args[2] : "0.0.0.0";
var listenPort = args.Length > 3 && int.TryParse(args[3], out var lp) ? lp : 9339;

var client = new Client(upstreamHost, upstreamPort);
var server = new Server(listenAddress, listenPort);
var proxy = new Proxy(upstreamHost, upstreamPort, listenAddress, listenPort);

// await client.RunAsync();
// await server.RunAsync();
// await proxy.RunAsync();


unsafe
{
    const int Size = 512;
    byte* stack1 = stackalloc byte[Size];
    NativePort.sub_100017714(stack1, 24);
    byte* stack2 = stackalloc byte[Size];
    ObfuscatedDecoder.Sub_100017714(new Span<byte>(stack2, Size), 24);
    byte* stack3 = stackalloc byte[Size];
    ObfuscatedPort.sub_100017714(stack3, 24);

    Console.WriteLine(Convert.ToHexString(new Span<byte>(stack1, Size)));
    Console.WriteLine(Convert.ToHexString(new Span<byte>(stack2, Size)));
    Console.WriteLine(Convert.ToHexString(new Span<byte>(stack3, Size)));
}




static unsafe class NativePort
{
    // This must be initialized externally to point at the base of the
    // 32-byte obfuscated qword region (equivalent of qword_10227E0C8).
    public static byte* qword_10227E0C8;

    // word_1018E87F8[64]
    static readonly ushort[] word_1018E87F8 =
    [
        0x0286, 0xC7D5, 0x3284, 0x3E9E, 0x769A, 0x073F, 0x249F,
        0x5671, 0x64C1, 0x2D9B, 0x360E, 0xC7B1, 0xCB3C, 0xE53A,
        0x3CFE, 0x54EC, 0xCEA8, 0xFA22, 0x2C4D, 0x6702, 0xBC30,
        0x6EE4, 0x42AE, 0xDE05, 0xD60E, 0x2B43, 0x0B0D, 0xC2F8,
        0x2F05, 0x4C88, 0x65F1, 0x370E, 0xC2F5, 0x650E, 0xACC2,
        0x2F88, 0xC14A, 0x0BF8, 0x07C0, 0xD643, 0xD1CA, 0x4205,
        0x6D60, 0xBCE4, 0x2DD9, 0x2C02, 0x3D71, 0xCE22, 0x323C,
        0x3CEC, 0xBC2B, 0xCB3A, 0x58C2, 0x36B1, 0xAFBF, 0x649B,
        0x0CC8, 0x2471, 0x98DB, 0x763F, 0x3084, 0x329E, 0x3EBD,
        0x02D5
    ];

    // xmmword_1018E8AF8
    static readonly byte[] xmmword_1018E8AF8 =
    [
        0x08, 0xC9, 0xBC, 0xF3, 0x67, 0xE6, 0x09, 0x6A,
        0x3B, 0xA7, 0xCA, 0x84, 0x85, 0xAE, 0x67, 0xBB
    ];

    // xmmword_1018E8B08
    static readonly byte[] xmmword_1018E8B08 =
    [
        0x2B, 0xF8, 0x94, 0xFE, 0x72, 0xF3, 0x6E, 0x3C,
        0xF1, 0x36, 0x1D, 0x5F, 0x3A, 0xF5, 0x4F, 0xA5
    ];

    // xmmword_1018E8B18
    static readonly byte[] xmmword_1018E8B18 =
    [
        0xD1, 0x82, 0xE6, 0xAD, 0x7F, 0x52, 0x0E, 0x51,
        0x1F, 0x6C, 0x3E, 0x2B, 0x8C, 0x68, 0x05, 0x9B
    ];

    // xmmword_1018E8B28
    static readonly byte[] xmmword_1018E8B28 =
    [
        0x6B, 0xBD, 0x41, 0xFB, 0xAB, 0xD9, 0x83, 0x1F,
        0x79, 0x21, 0x7E, 0x13, 0x19, 0xCD, 0xE0, 0x5B
    ];

    // Stack frame block: v4, v5, v6, v7[15]
    // Layout matches:
    //   byte  v4;
    //   short v5;
    //   byte  v6;
    //   uint  v7[15];
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    unsafe struct LocalBlock
    {
        public byte v4;
        public short v5;
        public byte v6;
        public fixed uint v7[15];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ushort RotateAndXorFull(ushort value, int shift, ushort xor)
    {
        int l = shift & 0xF;
        int r = (-shift) & 0xF;
        ushort rotated = (ushort)((value << l) | (value >> r));
        return (ushort)(rotated ^ xor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static ushort RotateAndXorLowByte(ushort value, int shift, ushort xor)
    {
        int l = shift & 0xF;
        int r = (-shift) & 0xF;
        int left = ((byte)value << l);
        int right = value >> r;
        ushort rotated = (ushort)(left | right);
        return (ushort)(rotated ^ xor);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static byte DecodeGenericByte(int offset, int shift, ushort baseWord)
    {
        if ((offset & 1) != 0)
        {
            int even = offset & ~1;
            ushort w1 = word_1018E87F8[31 - even + 32];
            ushort w2 = word_1018E87F8[offset];
            ushort w3 = word_1018E87F8[even];
            ushort v = (ushort)(w2 ^ w1 | w1 ^ w3);
            ushort t = RotateAndXorFull(v, shift, baseWord);
            return (byte)(t >> 8);
        }
        else
        {
            ushort w1 = word_1018E87F8[31 - offset + 32];
            ushort w2 = word_1018E87F8[offset + 1];
            ushort w3 = word_1018E87F8[offset];
            ushort v = (ushort)(w2 ^ w1 | w1 ^ w3);
            ushort t = RotateAndXorLowByte(v, shift, baseWord);
            return (byte)t;
        }
    }

    static unsafe void Copy16(byte* dst, byte[] src)
    {
        for (int i = 0; i < 16; i++)
            dst[i] = src[i];
    }

    // DecodeObfuscatedQwordAt @ 0x100016ff4
    public static unsafe ulong DecodeObfuscatedQwordAt(byte* a1)
    {
        uint v1 = (uint)(a1 - qword_10227E0C8);
        if (v1 > 0x1F)
            return *(ulong*)a1;

        // First byte (special pattern)
        int half0 = (int)(v1 >> 1);
        int v3 = half0 - 8;
        if (v1 < 0x10)
            v3 = half0;
        int v4 = 11 - v3;
        ushort v5 = word_1018E87F8[31 - half0 + 32];

        byte b0;
        if ((v1 & 1u) != 0)
        {
            int idxLow = (int)(v1 & 0x1F);
            int idxEven = (int)(v1 & 0x1E);
            ushort v10 = word_1018E87F8[(idxEven ^ 0x1F) + 32];
            ushort v11 = (ushort)(word_1018E87F8[idxLow] ^ v10 | v10 ^ word_1018E87F8[idxEven]);
            ushort t = RotateAndXorFull(v11, v4, v5);
            b0 = (byte)(t >> 8);
        }
        else
        {
            int idxLow = (int)(v1 & 0x1F);
            int idxEven = (int)(v1 & 0x1E);
            ushort v6 = word_1018E87F8[(idxLow ^ 0x1F) + 32];
            ushort v7 = (ushort)(word_1018E87F8[idxEven + 1] ^ v6 | v6 ^ word_1018E87F8[idxLow]);
            ushort t = RotateAndXorLowByte(v7, v4, v5);
            b0 = (byte)t;
        }

        // Remaining 7 bytes (generic pattern)
        byte b1, b2, b3, b4, b5b, b6, b7;

        // i = 1
        {
            int offset = (int)v1 + 1;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b1 = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 2
        {
            int offset = (int)v1 + 2;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b2 = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 3
        {
            int offset = (int)v1 + 3;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b3 = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 4
        {
            int offset = (int)v1 + 4;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b4 = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 5
        {
            int offset = (int)v1 + 5;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b5b = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 6
        {
            int offset = (int)v1 + 6;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b6 = DecodeGenericByte(offset, shift, baseWord);
        }

        // i = 7
        {
            int offset = (int)v1 + 7;
            int half = offset >> 1;
            int k = half & 7;
            if (half <= 0)
                k = -(-half & 7);
            int shift = 11 - k;
            ushort baseWord = word_1018E87F8[31 - half + 32];
            b7 = DecodeGenericByte(offset, shift, baseWord);
        }

        ulong result = b0;
        result |= (ulong)b1 << 8;
        result |= (ulong)b2 << 16;
        result |= (ulong)b3 << 24;
        result |= (ulong)b4 << 32;
        result |= (ulong)b5b << 40;
        result |= (ulong)b6 << 48;
        result |= (ulong)b7 << 56;

        return result;
    }

    // sub_100017714 @ 0x100017714
    public static unsafe long sub_100017714(byte* a1, int a2)
    {
        // if ((unsigned int)(a2 - 65) < 0xFFFFFFC0) return 0xFFFFFFFFLL;
        if ((uint)(a2 - 65) < 0xFFFFFFC0u)
            return -1;

        LocalBlock local = default;
        local.v4 = (byte)a2;
        local.v5 = 256;
        local.v6 = 1;
        // local.v7[...] already zeroed by default; matches memset(v7, 0, sizeof(v7));

        // Zero 0x0138 bytes from a1+64 to a1+352 (19 OWORDs) and then byte at +368
        for (int offset = 64; offset <= 352; offset += 16)
        {
            *(ulong*)(a1 + offset) = 0;
            *(ulong*)(a1 + offset + 8) = 0;
        }
        a1[368] = 0;

        // Initialize first 64 bytes with xmmword constants
        Copy16(a1 + 0, xmmword_1018E8AF8);
        Copy16(a1 + 16, xmmword_1018E8B08);
        Copy16(a1 + 32, xmmword_1018E8B18);
        Copy16(a1 + 48, xmmword_1018E8B28);

        ulong* q = (ulong*)a1;

        LocalBlock* plocal = &local;

        q[0] = DecodeObfuscatedQwordAt((byte*)&plocal->v4) ^ 0x6A09E667F3BCC908UL;
        q[1] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[1]) ^ 0xBB67AE8584CAA73BUL;
        q[2] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[3]) ^ 0x3C6EF372FE94F82BUL;
        q[3] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[5]) ^ 0xA54FF53A5F1D36F1UL;
        q[4] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[7]) ^ 0x510E527FADE682D1UL;
        q[5] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[9]) ^ 0x9B05688C2B3E6C1FUL;
        q[6] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[11]) ^ 0x1F83D9ABFB41BD6BUL;
        q[7] = DecodeObfuscatedQwordAt((byte*)&plocal->v7[13]) ^ 0x5BE0CD19137E2179UL;


        return 0;
    }
}

static class ObfuscatedDecoder
{
    // qword_10227E0C8 – you must set this externally to point to the base
    // equivalent of that BSS symbol. In the original, it's an address.
    // Here we treat it as a base "offset" into some buffer.
    public static long Qword10227E0C8;

    // word_1018E87F8 table, byte-to-byte from DCW (unsigned short)
    static readonly ushort[] word_1018E87F8 =
    {
        0x0286, 0xC7D5, 0x3284, 0x3E9E, 0x769A, 0x073F, 0x249F,
        0x5671, 0x64C1, 0x2D9B, 0x360E, 0xC7B1, 0xCB3C, 0xE53A,
        0x3CFE, 0x54EC, 0xCEA8, 0xFA22, 0x2C4D, 0x6702, 0xBC30,
        0x6EE4, 0x42AE, 0xDE05, 0xD60E, 0x2B43, 0x0B0D, 0xC2F8,
        0x2F05, 0x4C88, 0x65F1, 0x370E, 0xC2F5, 0x650E, 0xACC2,
        0x2F88, 0xC14A, 0x0BF8, 0x07C0, 0xD643, 0xD1CA, 0x4205,
        0x6D60, 0xBCE4, 0x2DD9, 0x2C02, 0x3D71, 0xCE22, 0x323C,
        0x3CEC, 0xBC2B, 0xCB3A, 0x58C2, 0x36B1, 0xAFBF, 0x649B,
        0x0CC8, 0x2471, 0x98DB, 0x763F, 0x3084, 0x329E, 0x3EBD,
        0x02D5
    };

    // Constants xmmword_1018E8AF8, xmmword_1018E8B08, xmmword_1018E8B18, xmmword_1018E8B28
    // stored as raw 16-byte blocks; order is exactly as in DCB listing.
    static readonly byte[] xmmword_1018E8AF8 =
    {
        0x08, 0xC9, 0xBC, 0xF3, 0x67, 0xE6, 0x09, 0x6A, 0x3B, 0xA7,
        0xCA, 0x84, 0x85, 0xAE, 0x67, 0xBB
    };

    static readonly byte[] xmmword_1018E8B08 =
    {
        0x2B, 0xF8, 0x94, 0xFE, 0x72, 0xF3, 0x6E, 0x3C, 0xF1, 0x36,
        0x1D, 0x5F, 0x3A, 0xF5, 0x4F, 0xA5
    };

    static readonly byte[] xmmword_1018E8B18 =
    {
        0xD1, 0x82, 0xE6, 0xAD, 0x7F, 0x52, 0x0E, 0x51, 0x1F, 0x6C,
        0x3E, 0x2B, 0x8C, 0x68, 0x05, 0x9B
    };

    static readonly byte[] xmmword_1018E8B28 =
    {
        0x6B, 0xBD, 0x41, 0xFB, 0xAB, 0xD9, 0x83, 0x1F, 0x79, 0x21,
        0x7E, 0x13, 0x19, 0xCD, 0xE0, 0x5B
    };

    // This is the C# equivalent of DecodeObfuscatedQwordAt
    // a1 in original is a pointer (address). Here we pass it as a "byte offset"
    // relative to some base, and we also pass the base span where data lives.
    public static ulong DecodeObfuscatedQwordAt(Span<byte> baseSpan, long a1Offset)
    {
        uint v1 = (uint)(a1Offset - Qword10227E0C8);
        if (v1 > 0x1F)
        {
            // return *(_QWORD *)a1;
            return MemoryMarshal.Read<ulong>(baseSpan.Slice((int)a1Offset, 8));
        }

        int v2 = (int)a1Offset + 1;
        int v3 = ((int)v1 >> 1) - 8;
        if (v1 < 0x10)
        {
            v3 = (int)v1 >> 1;
        }

        byte v4 = (byte)(11 - v3);
        ushort v5 = word_1018E87F8[31 - ((int)v1 >> 1) + 32];

        int v8;
        if ((v1 & 1) != 0)
        {
            ushort v10 = word_1018E87F8[((int)v1 & 0x1E ^ 0x1F) + 32];
            ushort v11 = (ushort)((word_1018E87F8[(int)v1] ^ v10) | (v10 ^ word_1018E87F8[(int)v1 & 0x1E]));
            ushort rotated = (ushort)((v11 << (v4 & 0xF)) | (v11 >> ((-v4) & 0xF)));
            v8 = (byte)(((ushort)(rotated ^ v5)) >> 8);
        }
        else
        {
            ushort v6 = word_1018E87F8[((int)v1 & 0x1F ^ 0x1F) + 32];
            ushort v7 = (ushort)((word_1018E87F8[((int)v1 & 0x1E) + 1] ^ v6) | (v6 ^ word_1018E87F8[(int)v1]));
            ushort rotated = (ushort)(((byte)v7 << (v4 & 0xF)) | (v7 >> ((-v4) & 0xF)));
            v8 = (byte)(rotated ^ v5);
        }

        int v12 = (int)a1Offset + 2;
        int v13 = v2 - (int)Qword10227E0C8;
        int v14 = (v2 - (int)Qword10227E0C8) >> 1;
        int v15;
        {
            int tmp = v14 & 7;
            if (v14 <= 0)
            {
                tmp = -(-v14 & 7);
            }
            v15 = tmp;
        }

        byte v16 = (byte)(11 - v15);
        ushort v17 = word_1018E87F8[31 - v14 + 32];
        long v18 = v13;
        int v21;
        if ((v13 & 1) != 0)
        {
            ushort v22 = word_1018E87F8[31 - (int)(v13 & ~1L) + 32];
            ushort v23 = (ushort)((word_1018E87F8[(int)v18] ^ v22) | (v22 ^ word_1018E87F8[(int)(v18 & ~1L)]));
            ushort rotated = (ushort)((v23 << (v16 & 0xF)) | (v23 >> ((-v16) & 0xF)));
            v21 = (byte)(((ushort)(rotated ^ v17)) >> 8);
        }
        else
        {
            ushort v19 = word_1018E87F8[31 - v13 + 32];
            int index = (int)((2L * v13) | 2L);
            ushort v20 = (ushort)(ReadWordFromTable(index) ^ v19 | (v19 ^ word_1018E87F8[v13]));
            ushort rotated = (ushort)(((byte)v20 << (v16 & 0xF)) | (v20 >> ((-v16) & 0xF)));
            v21 = (byte)(rotated ^ v17);
        }

        ulong v24 = (ulong)(v8 | (v21 << 8));

        int v25 = (int)a1Offset + 3;
        int v26 = v12 - (int)Qword10227E0C8;
        int v27 = (v12 - (int)Qword10227E0C8) >> 1;
        int v28;
        {
            int tmp = v27 & 7;
            if (v27 <= 0)
            {
                tmp = -(-v27 & 7);
            }
            v28 = tmp;
        }

        byte v29 = (byte)(11 - v28);
        ushort v30 = word_1018E87F8[31 - v27 + 32];
        long v31 = v26;
        uint v34;
        if ((v26 & 1) != 0)
        {
            ushort v35 = word_1018E87F8[31 - (int)(v26 & ~1L) + 32];
            ushort v36 = (ushort)((word_1018E87F8[(int)v31] ^ v35) | (v35 ^ word_1018E87F8[(int)(v31 & ~1L)]));
            ushort rotated = (ushort)((v36 << (v29 & 0xF)) | (v36 >> ((-v29) & 0xF)));
            v34 = (byte)(((ushort)(rotated ^ v30)) >> 8);
        }
        else
        {
            ushort v32 = word_1018E87F8[31 - v26 + 32];
            int index = (int)((2L * v26) | 2L);
            ushort v33 = (ushort)(ReadWordFromTable(index) ^ v32 | (v32 ^ word_1018E87F8[v26]));
            ushort rotated = (ushort)(((byte)v33 << (v29 & 0xF)) | (v33 >> ((-v29) & 0xF)));
            v34 = (byte)(rotated ^ v30);
        }

        ulong v37 = v24 | ((ulong)v34 << 16);

        int v38 = (int)a1Offset + 4;
        int v39 = v25 - (int)Qword10227E0C8;
        int v40 = (v25 - (int)Qword10227E0C8) >> 1;
        int v41;
        {
            int tmp = v40 & 7;
            if (v40 <= 0)
            {
                tmp = -(-v40 & 7);
            }
            v41 = tmp;
        }

        byte v42 = (byte)(11 - v41);
        ushort v43 = word_1018E87F8[31 - v40 + 32];
        long v44 = v39;
        uint v47;
        if ((v39 & 1) != 0)
        {
            ushort v48 = word_1018E87F8[31 - (int)(v39 & ~1L) + 32];
            ushort v49 = (ushort)((word_1018E87F8[(int)v44] ^ v48) | (v48 ^ word_1018E87F8[(int)(v44 & ~1L)]));
            ushort rotated = (ushort)((v49 << (v42 & 0xF)) | (v49 >> ((-v42) & 0xF)));
            v47 = (byte)(((ushort)(rotated ^ v43)) >> 8);
        }
        else
        {
            ushort v45 = word_1018E87F8[31 - v39 + 32];
            int index = (int)((2L * v39) | 2L);
            ushort v46 = (ushort)(ReadWordFromTable(index) ^ v45 | (v45 ^ word_1018E87F8[v39]));
            ushort rotated = (ushort)(((byte)v46 << (v42 & 0xF)) | (v46 >> ((-v42) & 0xF)));
            v47 = (byte)(rotated ^ v43);
        }

        ulong v50 = v37 | ((ulong)v47 << 24);

        int v51 = (int)a1Offset + 5;
        int v52 = v38 - (int)Qword10227E0C8;
        int v53 = (v38 - (int)Qword10227E0C8) >> 1;
        int v54;
        {
            int tmp = v53 & 7;
            if (v53 <= 0)
            {
                tmp = -(-v53 & 7);
            }
            v54 = tmp;
        }

        byte v55 = (byte)(11 - v54);
        ushort v56 = word_1018E87F8[31 - v53 + 32];
        long v57 = v52;
        ulong v60;
        if ((v52 & 1) != 0)
        {
            ushort v61 = word_1018E87F8[31 - (int)(v52 & ~1L) + 32];
            ushort v62 = (ushort)((word_1018E87F8[(int)v57] ^ v61) | (v61 ^ word_1018E87F8[(int)(v57 & ~1L)]));
            ushort rotated = (ushort)((v62 << (v55 & 0xF)) | (v62 >> ((-v55) & 0xF)));
            v60 = (byte)(((ushort)(rotated ^ v56)) >> 8);
        }
        else
        {
            ushort v58 = word_1018E87F8[31 - v52 + 32];
            int index = (int)((2L * v52) | 2L);
            ushort v59 = (ushort)(ReadWordFromTable(index) ^ v58 | (v58 ^ word_1018E87F8[v52]));
            ushort rotated = (ushort)(((byte)v59 << (v55 & 0xF)) | (v59 >> ((-v55) & 0xF)));
            v60 = (byte)(rotated ^ v56);
        }

        ulong v63 = v50 | (v60 << 32);

        int v64 = (int)a1Offset + 6;
        int v65 = v51 - (int)Qword10227E0C8;
        int v66 = (v51 - (int)Qword10227E0C8) >> 1;
        int v67;
        {
            int tmp = v66 & 7;
            if (v66 <= 0)
            {
                tmp = -(-v66 & 7);
            }
            v67 = tmp;
        }

        byte v68 = (byte)(11 - v67);
        ushort v69 = word_1018E87F8[31 - v66 + 32];
        long v70 = v65;
        ulong v73;
        if ((v65 & 1) != 0)
        {
            ushort v74 = word_1018E87F8[31 - (int)(v65 & ~1L) + 32];
            ushort v75 = (ushort)((word_1018E87F8[(int)v70] ^ v74) | (v74 ^ word_1018E87F8[(int)(v70 & ~1L)]));
            ushort rotated = (ushort)((v75 << (v68 & 0xF)) | (v75 >> ((-v68) & 0xF)));
            v73 = (byte)(((ushort)(rotated ^ v69)) >> 8);
        }
        else
        {
            ushort v71 = word_1018E87F8[31 - v65 + 32];
            int index = (int)((2L * v65) | 2L);
            ushort v72 = (ushort)(ReadWordFromTable(index) ^ v71 | (v71 ^ word_1018E87F8[v65]));
            ushort rotated = (ushort)(((byte)v72 << (v68 & 0xF)) | (v72 >> ((-v68) & 0xF)));
            v73 = (byte)(rotated ^ v69);
        }

        ulong v76 = v63 | (v73 << 40);

        int v77 = (int)a1Offset + 7;
        int v78 = v64 - (int)Qword10227E0C8;
        int v79 = (v64 - (int)Qword10227E0C8) >> 1;
        int v80;
        {
            int tmp = v79 & 7;
            if (v79 <= 0)
            {
                tmp = -(-v79 & 7);
            }
            v80 = tmp;
        }

        byte v81 = (byte)(11 - v80);
        ushort v82 = word_1018E87F8[31 - v79 + 32];
        long v83 = v78;
        ulong v86;
        if ((v78 & 1) != 0)
        {
            ushort v87 = word_1018E87F8[31 - (int)(v78 & ~1L) + 32];
            ushort v88 = (ushort)((word_1018E87F8[(int)v83] ^ v87) | (v87 ^ word_1018E87F8[(int)(v83 & ~1L)]));
            ushort rotated = (ushort)((v88 << (v81 & 0xF)) | (v88 >> ((-v81) & 0xF)));
            v86 = (byte)(((ushort)(rotated ^ v82)) >> 8);
        }
        else
        {
            ushort v84 = word_1018E87F8[31 - v78 + 32];
            int index = (int)((2L * v78) | 2L);
            ushort v85 = (ushort)(ReadWordFromTable(index) ^ v84 | (v84 ^ word_1018E87F8[v78]));
            ushort rotated = (ushort)(((byte)v85 << (v81 & 0xF)) | (v85 >> ((-v81) & 0xF)));
            v86 = (byte)(rotated ^ v82);
        }

        ulong v89 = v76 | (v86 << 48);

        int v90 = v77 - (int)Qword10227E0C8;
        int v91 = (v77 - (int)Qword10227E0C8) >> 1;
        int tmp76 = v91 & 7;
        if (v91 <= 0)
        {
            tmp76 = -(-v91 & 7);
        }

        byte v92 = (byte)(11 - tmp76);
        ushort v93 = word_1018E87F8[31 - v91 + 32];
        long v94 = v90;
        ulong v97;
        if ((v90 & 1) != 0)
        {
            ushort v98 = word_1018E87F8[31 - (int)(v90 & ~1L) + 32];
            int v99 = (ushort)((word_1018E87F8[(int)v94] ^ v98) | (v98 ^ word_1018E87F8[(int)(v94 & ~1L)]));
            ushort rotated = (ushort)((v99 << (v92 & 0xF)) | ((ushort)v99 >> ((-v92) & 0xF)));
            v97 = (byte)(((ushort)(rotated ^ v93)) >> 8);
        }
        else
        {
            ushort v95 = word_1018E87F8[31 - v90 + 32];
            int index = (int)((2L * v90) | 2L);
            int v96 = (ushort)(ReadWordFromTable(index) ^ v95 | (v95 ^ word_1018E87F8[v90]));
            ushort rotated = (ushort)((v96 << (v92 & 0xF)) | ((ushort)v96 >> ((-v92) & 0xF)));
            v97 = (byte)(rotated ^ v93);
        }

        return v89 | (v97 << 56);
    }

    // Helper equivalent of *(unsigned __int16 *)((char *)word_1018E87F8 + index)
    static ushort ReadWordFromTable(int byteOffset)
    {
        int elementIndex = byteOffset >> 1;
        return word_1018E87F8[elementIndex];
    }

    // sub_100017714 — initialization function
    // a1: base pointer to state (at least 368+ bytes)
    // a2: input int
    public static long Sub_100017714(Span<byte> state, int a2)
    {
        if ((uint)(a2 - 65) < 0xFFFFFFC0u)
        {
            return -1;
        }

        byte v4 = (byte)a2;
        short v5 = 256;
        byte v6 = 1;
        Span<uint> v7 = stackalloc uint[15];
        v7.Clear();

        // Zero huge region: *(_OWORD *)(a1 + x) = 0u; etc.
        state.Slice(64, 16 * 18 + 16).Clear(); // 64..(64+288) + other, but to be safe we clear 64..368
        state[368] = 0;

        // Copy xmmwords into state at 0,16,32,48
        xmmword_1018E8AF8.CopyTo(state.Slice(0, 16));
        xmmword_1018E8B08.CopyTo(state.Slice(16, 16));
        xmmword_1018E8B18.CopyTo(state.Slice(32, 16));
        xmmword_1018E8B28.CopyTo(state.Slice(48, 16));

        // Now perform DecodeObfuscatedQwordAt calls.
        Span<byte> stackStruct = stackalloc byte[1 + 2 + 1 + 15 * 4]; // v4, v5, v6, v7[15]
        stackStruct[0] = v4;
        MemoryMarshal.Write(stackStruct.Slice(1, 2), ref v5);
        stackStruct[3] = v6;
        // v7 is zeros already; keep them as @ stackStruct[4..]

        // We need a "baseSpan" that includes this stackStruct contiguous
        // because original takes &v4 and &v7[1], etc.
        // Simplest: treat stackStruct as the baseSpan and use offsets within it.
        Span<byte> baseSpan = stackStruct;

        ulong q0 = DecodeObfuscatedQwordAt(baseSpan, 0) ^ 0x6A09E667F3BCC908UL;
        ulong q1 = DecodeObfuscatedQwordAt(baseSpan, 4) ^ 0xBB67AE8584CAA73BUL;   // &v7[1] == &stackStruct[4]
        ulong q2 = DecodeObfuscatedQwordAt(baseSpan, 12) ^ 0x3C6EF372FE94F82BUL;  // &v7[3] == &stackStruct[12]
        ulong q3 = DecodeObfuscatedQwordAt(baseSpan, 20) ^ 0xA54FF53A5F1D36F1UL;  // &v7[5]
        ulong q4 = DecodeObfuscatedQwordAt(baseSpan, 28) ^ 0x510E527FADE682D1UL;  // &v7[7]
        ulong q5 = DecodeObfuscatedQwordAt(baseSpan, 36) ^ 0x9B05688C2B3E6C1FUL;  // &v7[9]
        ulong q6 = DecodeObfuscatedQwordAt(baseSpan, 44) ^ 0x1F83D9ABFB41BD6BUL;  // &v7[11]
        ulong q7 = DecodeObfuscatedQwordAt(baseSpan, 52) ^ 0x5BE0CD19137E2179UL;  // &v7[13]

        MemoryMarshal.Write(state.Slice(0, 8), ref q0);
        MemoryMarshal.Write(state.Slice(8, 8), ref q1);
        MemoryMarshal.Write(state.Slice(16, 8), ref q2);
        MemoryMarshal.Write(state.Slice(24, 8), ref q3);
        MemoryMarshal.Write(state.Slice(32, 8), ref q4);
        MemoryMarshal.Write(state.Slice(40, 8), ref q5);
        MemoryMarshal.Write(state.Slice(48, 8), ref q6);
        MemoryMarshal.Write(state.Slice(56, 8), ref q7);

        return 0;
    }
}

static unsafe class ObfuscatedPort
{
    // __bss:000000010227E0C8 qword_10227E0C8 % 8
    // Global pointer used as the base for the decode offsets.
    // In the original binary this will be set somewhere else.
    // Here it defaults to 0 (like BSS).
    static byte* qword_10227E0C8;

    // Helper so you can set the same base pointer logic as the original, if you know it.
    public static void SetDecodeBase(byte* basePtr) => qword_10227E0C8 = basePtr;

    // __const:00000001018E87F8 ; unsigned __int16 word_1018E87F8[64]
    static readonly ushort[] word_1018E87F8 = new ushort[]
    {
        0x0286, 0xC7D5, 0x3284, 0x3E9E, 0x769A, 0x073F, 0x249F,
        0x5671, 0x64C1, 0x2D9B, 0x360E, 0xC7B1, 0xCB3C, 0xE53A,
        0x3CFE, 0x54EC, 0xCEA8, 0xFA22, 0x2C4D, 0x6702, 0xBC30,
        0x6EE4, 0x42AE, 0xDE05, 0xD60E, 0x2B43, 0x0B0D, 0xC2F8,
        0x2F05, 0x4C88, 0x65F1, 0x370E, 0xC2F5, 0x650E, 0xACC2,
        0x2F88, 0xC14A, 0x0BF8, 0x07C0, 0xD643, 0xD1CA, 0x4205,
        0x6D60, 0xBCE4, 0x2DD9, 0x2C02, 0x3D71, 0xCE22, 0x323C,
        0x3CEC, 0xBC2B, 0xCB3A, 0x58C2, 0x36B1, 0xAFBF, 0x649B,
        0x0CC8, 0x2471, 0x98DB, 0x763F, 0x3084, 0x329E, 0x3EBD,
        0x02D5,
    };

    // __int64 __fastcall sub_100017714(__int64 a1, int a2)
    public static long sub_100017714(byte* a1, int a2)
    {
        // Locals as in the decompiled signature / stack layout
        byte v4;
        short v5;
        byte v6;
        uint* v7 = stackalloc uint[15];

        // if ( (unsigned int)(a2 - 65) < 0xFFFFFFC0 )
        //     return 0xFFFFFFFFLL;
        if ((uint)(a2 - 65) < 0xFFFFFFC0u)
            return -1; // 0xFFFFFFFF sign-extended

        v4 = (byte)a2;
        v5 = 256;
        v6 = 1;

        for (int i = 0; i < 15; i++)
            v7[i] = 0;

        // *(_OWORD *)(a1 + 64)  = 0u;
        // ...
        // *(_OWORD *)(a1 + 352) = 0u;
        // 19 consecutive 16-byte blocks
        for (int offset = 64; offset <= 352; offset += 16)
        {
            *(ulong*)(a1 + offset) = 0;
            *(ulong*)(a1 + offset + 8) = 0;
        }

        // *(_BYTE *)(a1 + 368) = 0;
        a1[368] = 0;

        // NOTE: the xmmword_1018E8AF8 / B08 / B18 / B28 initial stores are
        // omitted here because they are completely overwritten by the
        // QWORD assignments below. If you really need them, you can add
        // four 16-byte copies before these eight QWORD stores.

        ulong* q = (ulong*)a1;

        q[0] = (ulong)DecodeObfuscatedQwordAt(&v4) ^ 0x6A09E667F3BCC908UL;
        q[1] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[1]) ^ 0xBB67AE8584CAA73BUL;
        q[2] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[3]) ^ 0x3C6EF372FE94F82BUL;
        q[3] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[5]) ^ 0xA54FF53A5F1D36F1UL;
        q[4] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[7]) ^ 0x510E527FADE682D1UL;
        q[5] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[9]) ^ 0x9B05688C2B3E6C1FUL;
        q[6] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[11]) ^ 0x1F83D9ABFB41BD6BUL;
        q[7] = (ulong)DecodeObfuscatedQwordAt((byte*)&v7[13]) ^ 0x5BE0CD19137E2179UL;

        return 0;
    }

    // __int64 __fastcall DecodeObfuscatedQwordAt(__int64 a1)
    // Ported in a factored form that is algebraically equivalent to the
    // decompiled blocks (same table indices, same rotates, same high/low byte use).
    public static long DecodeObfuscatedQwordAt(byte* a1)
    {
        long diff = a1 - qword_10227E0C8;

        // if ( (unsigned int)(a1 - qword_10227E0C8) > 0x1F )
        //   return *(_QWORD *)a1;
        if ((uint)diff > 0x1F)
            return *(long*)a1;

        int baseOffset = (int)diff;
        ulong result = 0;

        for (int i = 0; i < 8; i++)
        {
            int d = baseOffset + i;   // effective byte offset from qword_10227E0C8
            int half = d >> 1;        // v1 >> 1 etc.

            // The decompiled code has two different-looking formulas for the
            // shift, but they reduce to this:
            // v4 / v16 / v29 / v42 / v55 / v68 / v81 / v92 = 11 - ((d >> 1) & 7)
            int shift = 11 - (half & 7);
            int shLeft = shift & 0xF;
            int shRight = (-shift) & 0xF;

            // v5 / v17 / v30 / v43 / v56 / v69 / v82 / v93 = word_...[31 - half + 32]
            ushort mid = word_1018E87F8[63 - half];

            byte b;

            if ((d & 1) != 0)
            {
                // Odd offset branch (v1 & 1 != 0, v13 & 1 != 0, ...)
                int evenIndex = d & ~1; // v1 & 0x1E, v13 & ~1, etc.

                // v10/v22/v35/v48/v61/v74/v87/v98 = word[31 - (evenIndex) + 32]
                ushort a = word_1018E87F8[63 - evenIndex];

                // v11/v23/v36/v49/v62/v75/v88/v99 = word[d] ^ a | a ^ word[evenIndex]
                ushort mix = (ushort)(
                    (word_1018E87F8[d] ^ a) |
                    (a ^ word_1018E87F8[evenIndex])
                );

                // (unsigned __int16)(((mix << (shift & 0xF)) | (mix >> (-shift & 0xF))) ^ mid) >> 8
                ushort rotated = (ushort)(((mix << shLeft) | (mix >> shRight)) ^ mid);
                b = (byte)(rotated >> 8);
            }
            else
            {
                // Even offset branch
                // v6/v19/v32/v45/v58/v71/v84/v95 = word[31 - d + 32]
                ushort a = word_1018E87F8[63 - d];

                // v7/v20/v33/v46/v59/v72/v85/v96 =
                //   word[d+1] ^ a | a ^ word[d]
                ushort mix = (ushort)(
                    (word_1018E87F8[d + 1] ^ a) |
                    (a ^ word_1018E87F8[d])
                );

                // (((_BYTE)mix << (shift & 0xF)) | (mix >> (-shift & 0xF))) ^ mid
                int temp = (((int)(byte)mix << shLeft) | (mix >> shRight)) ^ mid;
                b = (byte)temp;
            }

            result |= (ulong)b << (8 * i);
        }

        return (long)result;
    }
}