using System.Buffers.Binary;
using System.Text;

namespace SupercellProxy.Playground.Network.Streams;

public class SupercellStream(Stream stream, bool leaveOpen = true) : IAsyncDisposable
{
    public int Position { get; private set; }
    public bool CanRead => stream.CanRead;
    public bool CanWrite => stream.CanWrite;

    private int _booleanOffset;
    private byte _booleanAdditionalValue;

    private int _booleanWriteOffset;
    private byte _booleanWriteAccumulator;

    public byte ReadByte()
    {
        ResetBoolean();

        var value = stream.ReadByte();
        if (value < 0)
            throw new EndOfStreamException();

        Position += 1;
        return (byte)value;
    }

    public byte[] ReadBytes(int count)
    {
        ResetBoolean();

        if (count < 0)
            throw new ArgumentOutOfRangeException(nameof(count));

        var buffer = new byte[count];
        ReadExactly(buffer);
        return buffer;
    }

    public byte[] ReadByteArray()
    {
        var length = ReadInt32();

        if (length == 0)
            return [];

        if (length < 0)
            throw new InvalidDataException("Negative length for byte array.");

        return ReadBytes(length);
    }

    public bool ReadBoolean()
    {
        if (_booleanOffset == 0)
            _booleanAdditionalValue = ReadByte();

        var value = ((_booleanAdditionalValue >> _booleanOffset) & 1) != 0;
        _booleanOffset = (_booleanOffset + 1) & 7;

        return value;
    }

    public ushort ReadUInt16()
    {
        var buf = ReadBytes(2);
        return BinaryPrimitives.ReadUInt16BigEndian(buf);
    }

    public int ReadInt32()
    {
        var buf = ReadBytes(4);
        return BinaryPrimitives.ReadInt32BigEndian(buf);
    }

    public string? ReadString()
    {
        var length = ReadInt32();

        if (length < 0)
            return null;

        if (length == 0)
            return string.Empty;

        var bytes = ReadBytes(length);
        return Encoding.UTF8.GetString(bytes);
    }

    public int ReadVarInt()
    {
        var firstByte = ReadByte();
        var isNegative = (firstByte & 0x40) != 0;
        var accumulator = firstByte & 0x3FL;
        var consumedBitWidth = 6;

        var currentByte = firstByte;
        while ((currentByte & 0x80) != 0 && consumedBitWidth < 64)
        {
            currentByte = ReadByte();
            accumulator |= (long)(currentByte & 0x7F) << consumedBitWidth;
            consumedBitWidth += 7;
        }

        if (!isNegative)
            return (int)accumulator;

        var twoComplementBase = 1L << consumedBitWidth;
        accumulator -= twoComplementBase;

        return (int)accumulator;
    }

    public void WriteByte(byte value)
    {
        FlushBoolean();
        stream.Write([value]);
    }

    public void WriteBytes(ReadOnlySpan<byte> source)
    {
        FlushBoolean();
        stream.Write(source);
    }

    public void WriteByteArray(ReadOnlySpan<byte> source)
    {
        WriteInt32(source.Length);
        WriteBytes(source);
    }

    public void WriteBoolean(bool value)
    {
        if (_booleanWriteOffset == 0)
            _booleanWriteAccumulator = 0;

        if (value)
            _booleanWriteAccumulator |= (byte)(1 << _booleanWriteOffset);

        _booleanWriteOffset = (_booleanWriteOffset + 1) & 7;

        if (_booleanWriteOffset == 0)
            stream.Write([_booleanWriteAccumulator]);
    }

    public void WriteUInt16(ushort value)
    {
        FlushBoolean();

        Span<byte> buf = stackalloc byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(buf, value);
        stream.Write(buf);
    }

    public void WriteInt32(int value)
    {
        FlushBoolean();

        Span<byte> buf = stackalloc byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buf, value);
        stream.Write(buf);
    }

    public void WriteString(string? value)
    {
        if (value is null)
        {
            WriteInt32(-1);
            return;
        }

        var byteCount = Encoding.UTF8.GetByteCount(value);
        WriteInt32(byteCount);

        FlushBoolean();

        if (byteCount == 0)
            return;

        if (byteCount <= 1024)
        {
            Span<byte> buf = stackalloc byte[byteCount];
            Encoding.UTF8.GetBytes(value, buf);
            stream.Write(buf);
            return;
        }

        var rented = new byte[byteCount];
        Encoding.UTF8.GetBytes(value, 0, value.Length, rented, 0);
        stream.Write(rented, 0, rented.Length);
    }

    public void WriteVarInt(int value)
    {
        FlushBoolean();

        var isNegative = value < 0;
        var absolute = (ulong)(isNegative ? -(long)value : value);

        int[] widths = [6, 13, 20, 27, 34, 41, 48, 55, 62];
        var chosenWidth = 62;

        foreach (var candidate in widths)
        {
            var limit = 1UL << candidate;

            if (isNegative ? absolute > limit : absolute >= limit)
                continue;

            chosenWidth = candidate;
            break;
        }

        var encoded = isNegative ? (1UL << chosenWidth) - absolute : absolute;
        Span<byte> small = stackalloc byte[10];
        var idx = 0;

        var first = (byte)(encoded & 0x3FUL);

        if (isNegative)
            first |= 0x40;

        if (chosenWidth > 6)
            first |= 0x80;

        small[idx++] = first;

        encoded >>= 6;
        var remainingBits = chosenWidth - 6;

        while (remainingBits > 0)
        {
            var b = (byte)(encoded & 0x7FUL);
            encoded >>= 7;
            remainingBits -= 7;

            if (remainingBits > 0)
                b |= 0x80;

            small[idx++] = b;
        }

        stream.Write(small[..idx]);
    }

    private void ReadExactly(Span<byte> destination)
    {
        var total = 0;

        while (total < destination.Length)
        {
            var read = stream.Read(destination[total..]);
            if (read == 0)
                throw new EndOfStreamException();

            total += read;
        }

        Position += total;
    }

    private void FlushBoolean()
    {
        if (_booleanWriteOffset <= 0)
            return;

        stream.Write([_booleanWriteAccumulator]);
        
        _booleanWriteOffset = 0;
        _booleanWriteAccumulator = 0;
    }

    private void ResetBoolean()
    {
        if (_booleanOffset <= 0)
            return;

        _booleanOffset = 0;
        _booleanAdditionalValue = 0;
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);

        FlushBoolean();

        if (leaveOpen)
            return;

        await stream.DisposeAsync();
    }
}
