using System;
using System.Text;

namespace Shared
{
    public class NetworkMessage
    {
        private byte[] _buffer;
        private readonly int _bufferSize = 16394;
        private int _length;
        private int _position;

        public int Length => _length;

        public int Position => _position;


        public byte[] Buffer => _buffer;

        public int BufferSize => _bufferSize;

        public NetworkMessage() => Reset();

        public NetworkMessage(byte[] data)
        {
            _buffer = data;
            _length = data.Length;
            _position = 0;
        }

        public NetworkMessage(int startingIndex) => Reset(startingIndex);

        public void Reset(int startingIndex)
        {
            _buffer = new byte[_bufferSize];
            _length = startingIndex;
            _position = startingIndex;
        }

        public void Reset() => Reset(0);

        public byte GetByte()
        {
            if (Position + 1 > Length)
                throw new IndexOutOfRangeException("NetworkMessage GetByte() out of range.");

            return _buffer[_position++];
        }

        public byte[] GetBytes(int count)
        {
            if (Position + count > Length)
                throw new IndexOutOfRangeException("NetworkMessage GetBytes() out of range.");

            byte[] t = new byte[count];
            Array.Copy(_buffer, Position, t, 0, count);

            _position += count;
            return t;
        }
        
        public string GetString()
        {
            int len = GetUInt16();
            string t = Encoding.Default.GetString(_buffer, Position, len);

            _position += len;
            return t;
        }

        public ushort GetUInt16() => BitConverter.ToUInt16(GetBytes(2), 0);

        public uint GetUInt32() => BitConverter.ToUInt32(GetBytes(4), 0);

        public float GetFloat() => BitConverter.ToSingle(GetBytes(4), 0);

        public NetworkTagPacket GetTagPacket() => (NetworkTagPacket)GetUInt32();

        public void AddTagPacket(NetworkTagPacket networkTagPacket) => AddUInt32((uint)networkTagPacket);

        public void AddByte(byte value)
        {
            if (1 + Length > _bufferSize)
                throw new Exception("NetworkMessage buffer is full.");

            AddBytes(new[] { value });
        }

        public void AddBytes(byte[] value)
        {
            if (value.Length + Length > _bufferSize)
                throw new Exception("NetworkMessage buffer is full.");

            Array.Copy(value, 0, _buffer, Position, value.Length);

            _position += value.Length;

            if (Position > Length)
                _length = Position;
        }

        public void AddString(string value)
        {
            AddUInt16((ushort)value.Length);
            AddBytes(Encoding.Default.GetBytes(value));
        }

        public void AddUInt16(ushort value) => AddBytes(BitConverter.GetBytes(value));
        
        public void AddUInt32(uint value) => AddBytes(BitConverter.GetBytes(value));

        public void AddFloat(float value) => AddBytes(BitConverter.GetBytes(value));

        public void AddPaddingBytes(int count)
        {
            _position += count;

            if (Position > Length)
                _length = Position;
        }
        
        public byte PeekByte() => _buffer[Position];

        public void Resize(int size)
        {
            _length = size;
            _position = 0;
        }

        public byte[] PeekBytes(int count)
        {
            byte[] t = new byte[count];
            Array.Copy(_buffer, Position, t, 0, count);
            return t;
        }

        public ushort PeekUInt16() => BitConverter.ToUInt16(PeekBytes(2), 0);

        public uint PeekUInt32() => BitConverter.ToUInt32(PeekBytes(4), 0);

        public string PeekString()
        {
            int len = PeekUInt16();
            return Encoding.ASCII.GetString(PeekBytes(len + 2), 2, len);
        }

        public void ReplaceBytes(int index, byte[] value)
        {
            if (Length - index >= value.Length)
                Array.Copy(value, 0, _buffer, index, value.Length);
        }

        public void SkipBytes(int count)
        {
            if (Position + count > Length)
                throw new IndexOutOfRangeException("NetworkMessage SkipBytes() out of range.");

            _position += count;
        }
    }
}
