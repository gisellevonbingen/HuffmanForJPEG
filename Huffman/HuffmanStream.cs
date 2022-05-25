using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    public class HuffmanStream : Stream
    {
        private readonly Stream BaseStream;
        private readonly Dictionary<byte, HuffmanCode> Caches;

        private int ReadingByte = 0;
        private int ReadingPosition = 0;
        private int WritingByte = 0;
        private int WritingPosition = 0;

        public HuffmanStream(Stream baseStream, HuffmanNode<byte> rootNode)
        {
            this.BaseStream = baseStream;
            this.Caches = rootNode.ToCodeMap();
        }

        private int ReadBit()
        {
            if (this.ReadingPosition == 0)
            {
                this.ReadingPosition = 8;
                this.ReadingByte = this.BaseStream.ReadByte();
            }

            if (this.ReadingByte == -1)
            {
                return -1;
            }

            var shift = this.ReadingPosition - 1;
            var bitMask = 1 << shift;
            var bit = (this.ReadingByte & bitMask) >> shift;
            this.ReadingPosition--;

            return bit;
        }

        public override int ReadByte()
        {
            var rawCode = 0;

            for (var i = 0; ; i++)
            {
                var bit = this.ReadBit();

                if (bit == -1)
                {
                    return bit;
                }

                rawCode = rawCode << 1 | bit;
                var length = i + 1;

                foreach (var pair in this.Caches)
                {
                    var code = pair.Value;

                    if (code.Raw == rawCode && code.Length == length)
                    {
                        return pair.Key;
                    }

                }

            }

        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var b = this.ReadByte();

                if (b == -1)
                {
                    return i;
                }

                buffer[offset + i] = (byte)b;
            }

            return count;
        }

        private void WriteBit(int bit)
        {
            this.WritingByte |= bit << (7 - this.WritingPosition);
            this.WritingPosition++;

            if (this.WritingPosition == 8)
            {
                this.BaseStream.WriteByte((byte)this.WritingByte);
                this.WritingByte = 0;
                this.WritingPosition = 0;
            }

        }

        public override void WriteByte(byte value)
        {
            if (this.Caches.TryGetValue(value, out var code) == false)
            {
                throw new ArgumentException($"Byte 0x{value:X2} is not exist in HuffmanTable");
            }

            for (var i = 0; i < code.Length; i++)
            {
                var shift = code.Length - 1 - i;
                var bitMask = 1 << shift;
                var bit = (code.Raw & bitMask) >> shift;
                this.WriteBit(bit);
            }

        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            for (var i = 0; i < count; i++)
            {
                this.WriteByte(buffer[offset + i]);
            }

        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (this.WritingPosition > 0)
            {
                this.BaseStream.WriteByte((byte)this.WritingByte);
                this.ReadingByte = 0;
                this.WritingPosition = 0;
            }

        }

        public override bool CanRead => this.BaseStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => this.BaseStream.CanWrite;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

    }

}
