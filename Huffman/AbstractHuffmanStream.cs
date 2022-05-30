using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    public abstract class AbstractHuffmanStream : Stream
    {
        private readonly Stream BaseStream;
        private readonly bool LeaveOpen;

        private int ReadingByte = 0;
        private int ReadingPosition = 0;
        private int WritingByte = 0;
        private int WritingPosition = 0;

        public long InBits { get; private set; }
        public long InBytes { get; private set; }
        public long OutBits { get; private set; }
        public long OutBytes { get; private set; }

        protected AbstractHuffmanStream(Stream baseStream) : this(baseStream, false)
        {

        }

        protected AbstractHuffmanStream(Stream baseStream, bool leaveOpen)
        {
            this.BaseStream = baseStream;
            this.LeaveOpen = leaveOpen;
        }

        protected abstract Dictionary<byte, HuffmanCode> NextReadingCodes();

        protected abstract Dictionary<byte, HuffmanCode> NextWritingCodes();

        protected virtual int ReadEncodedByte() => this.BaseStream.ReadByte();

        protected virtual void WriteEncodedByte(byte value) => this.BaseStream.WriteByte(value);

        public int ReadEncodedBit()
        {
            if (this.ReadingPosition == 0)
            {
                this.ReadingPosition = 8;
                this.ReadingByte = this.ReadEncodedByte();
            }

            if (this.ReadingByte == -1)
            {
                return -1;
            }

            var shift = this.ReadingPosition - 1;
            var bitMask = 1 << shift;
            var bit = (this.ReadingByte & bitMask) >> shift;
            this.ReadingPosition--;
            this.InBits++;

            return bit;
        }

        public override int ReadByte()
        {
            var rawCode = 0;
            var codes = this.NextReadingCodes();

            for (var i = 0; ; i++)
            {
                var bit = this.ReadEncodedBit();

                if (bit == -1)
                {
                    return bit;
                }

                rawCode = rawCode << 1 | bit;
                var length = i + 1;
                var looksMaxLength = 1;

                foreach (var pair in codes)
                {
                    var code = pair.Value;

                    if (code.Raw == rawCode && code.Length == length)
                    {
                        this.InBytes++;
                        return pair.Key;
                    }
                    else if (code.Length > looksMaxLength)
                    {
                        looksMaxLength = code.Length;
                    }

                }

                if (length >= looksMaxLength)
                {
                    throw new ArgumentException($"RawCode 0x{rawCode:X2} is not exist in HuffmanTable");
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

        public void WriteEncodedBit(bool bit) => this.WriteEncodedBit(bit ? 1 : 0);

        public void WriteEncodedBit(int bit)
        {
            this.WritingByte = (this.WritingByte << 1) | bit;
            this.WritingPosition++;
            this.OutBits++;

            if (this.WritingPosition == 8)
            {
                this.WriteEncodedByte((byte)this.WritingByte);
                this.WritingByte = 0;
                this.WritingPosition = 0;
            }

        }

        public override void WriteByte(byte value)
        {
            var codes = this.NextWritingCodes();

            if (codes.TryGetValue(value, out var code) == false)
            {
                throw new ArgumentException($"Byte 0x{value:X2} is not exist in HuffmanTable");
            }

            for (var i = 0; i < code.Length; i++)
            {
                var shift = code.Length - 1 - i;
                var bitMask = 1 << shift;
                var bit = (code.Raw & bitMask) >> shift;
                this.WriteEncodedBit(bit);
            }

            this.OutBytes++;
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
                this.WritingByte = 0;
                this.WritingPosition = 0;
            }

            if (this.LeaveOpen == false)
            {
                this.BaseStream.Dispose();
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
