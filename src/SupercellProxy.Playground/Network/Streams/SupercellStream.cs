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

    public async ValueTask SendMessageAsync(Message message, CancellationToken cancellationToken = default)
    {
        var header = new byte[7];
        var span = header.AsSpan();
        
        BinaryPrimitives.WriteUInt16BigEndian(span[..2], message.Id);

        var length = message.Payload.Length;
        
        span[2] = (byte)(length >> 16);
        span[3] = (byte)(length >> 8);
        span[4] = (byte)length;
        
        BinaryPrimitives.WriteUInt16BigEndian(span[5..7], message.Version);

        await WriteBytesAsync(header, cancellationToken);
        await WriteBytesAsync(message.Payload, cancellationToken);
    }

    public async ValueTask<Message> ReadMessageAsync(CancellationToken cancellationToken = default)
    {
        var header = await ReadBytesAsync(7, cancellationToken);
        var span = header.AsSpan();

        var id = BinaryPrimitives.ReadUInt16BigEndian(span[0..2]);
        var length = (span[2] << 16) | (span[3] << 8) | span[4];
        var version = BinaryPrimitives.ReadUInt16BigEndian(span[5..7]);

        return new Message(id, version, await ReadBytesAsync(length, cancellationToken));
    }

    public byte ReadByte()
    {
        ResetBoolean();

        var value = stream.ReadByte();
        if (value < 0)
            throw new EndOfStreamException();

        Position += 1;
        return (byte)value;
    }

    public async ValueTask<byte[]> ReadBytesAsync(int count, CancellationToken cancellationToken = default)
    {
        ResetBoolean();

        ArgumentOutOfRangeException.ThrowIfNegative(count);

        var buffer = new byte[count];
        await stream.ReadExactlyAsync(buffer, cancellationToken);
        
        return buffer;
    }

    public async ValueTask<byte[]> ReadByteArrayAsync(CancellationToken cancellationToken = default)
    {
        var length = await ReadInt32Async(cancellationToken);

        if (length == 0)
            return [];

        if (length < 0)
            throw new InvalidDataException("Negative length for byte array.");

        return await ReadBytesAsync(length, cancellationToken);
    }

    public bool ReadBoolean()
    {
        if (_booleanOffset == 0)
            _booleanAdditionalValue = ReadByte();

        var value = ((_booleanAdditionalValue >> _booleanOffset) & 1) != 0;
        _booleanOffset = (_booleanOffset + 1) & 7;

        return value;
    }

    public async ValueTask<ushort> ReadUInt16Async(CancellationToken cancellationToken = default)
    {
        var buf = await ReadBytesAsync(2, cancellationToken);
        return BinaryPrimitives.ReadUInt16BigEndian(buf);
    }

    public async ValueTask<int> ReadInt32Async(CancellationToken cancellationToken = default)
    {
        var buf = await ReadBytesAsync(4, cancellationToken);
        return BinaryPrimitives.ReadInt32BigEndian(buf);
    }

    public async ValueTask<string> ReadStringAsync(CancellationToken cancellationToken = default)
    {
        var length = await ReadInt32Async(cancellationToken);

        if (length < 0)
            throw new InvalidDataException("Negative length for string array.");

        if (length == 0)
            return string.Empty;

        var bytes = await ReadBytesAsync(length, cancellationToken);
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
        stream.WriteByte(value);
    }

    public async ValueTask WriteBytesAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        FlushBoolean();
        await stream.WriteAsync(source, cancellationToken);
    }

    public async ValueTask WriteByteArrayAsync(ReadOnlyMemory<byte> source, CancellationToken cancellationToken = default)
    {
        await WriteInt32Async(source.Length, cancellationToken);
        await WriteBytesAsync(source, cancellationToken);
    }

    public void WriteBoolean(bool value)
    {
        if (_booleanWriteOffset == 0)
            _booleanWriteAccumulator = 0;

        if (value)
            _booleanWriteAccumulator |= (byte)(1 << _booleanWriteOffset);

        _booleanWriteOffset = (_booleanWriteOffset + 1) & 7;

        if (_booleanWriteOffset == 0)
            stream.WriteByte(_booleanWriteAccumulator);
    }

    public async ValueTask WriteUInt16Async(ushort value, CancellationToken cancellationToken = default)
    {
        FlushBoolean();

        var buf = new byte[2];
        BinaryPrimitives.WriteUInt16BigEndian(buf, value);
        await stream.WriteAsync(buf, cancellationToken);
    }

    public async ValueTask WriteInt32Async(int value, CancellationToken cancellationToken = default)
    {
        FlushBoolean();

        var buf = new byte[4];
        BinaryPrimitives.WriteInt32BigEndian(buf, value);
        await stream.WriteAsync(buf, cancellationToken);
    }

    public async ValueTask WriteStringAsync(string? value, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            await WriteInt32Async(-1, cancellationToken);
            return;
        }

        var byteCount = Encoding.UTF8.GetByteCount(value);
        await WriteInt32Async(byteCount, cancellationToken);

        FlushBoolean();

        if (byteCount == 0)
            return;

        if (byteCount <= 1024)
        {
            var buf = new byte[byteCount];
            Encoding.UTF8.GetBytes(value, buf);
            await stream.WriteAsync(buf, cancellationToken);
            return;
        }

        var rented = new byte[byteCount];
        Encoding.UTF8.GetBytes(value, 0, value.Length, rented, 0);
        await stream.WriteAsync(rented, 0, rented.Length, cancellationToken);
    }

    public async ValueTask WriteVarIntAsync(int value, CancellationToken cancellationToken = default)
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
        var small = new byte[10];
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

        await stream.WriteAsync(small.AsMemory(0, idx), cancellationToken);
    }

    private void FlushBoolean()
    {
        if (_booleanWriteOffset <= 0)
            return;

        stream.WriteByte(_booleanWriteAccumulator);
        
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
