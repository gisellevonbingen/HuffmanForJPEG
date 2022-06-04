using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Huffman
{
    public class BitStream : Stream
    {
        protected readonly Stream BaseStream;
        protected readonly bool LeaveOpen;

        private int ReadingByte = 0;
        private int ReadingPosition = -1;
        private int ReadingLength = 0;
        private int WritingByte = 0;
        private int WritingPosition = 0;

        public long InBits { get; protected set; }
        public long InBytes { get; protected set; }
        public long OutBits { get; protected set; }
        public long OutBytes { get; protected set; }

        public BitStream(Stream baseStream) : this(baseStream, false)
        {

        }

        public BitStream(Stream baseStream, bool leaveOpen)
        {
            this.BaseStream = baseStream;
            this.LeaveOpen = leaveOpen;
        }

        protected virtual int ReadEncodedByte(out int length)
        {
            length = 8;
            return this.ReadEncodedByte();
        }

        protected virtual int ReadEncodedByte()
        {
            return this.BaseStream.ReadByte();
        }

        protected virtual bool TryWriteEncodedByte(byte value, int position, bool disposing)
        {
            if (position == 8 | disposing == true)
            {
                this.BaseStream.WriteByte(value);
                return true;
            }
            else
            {
                return false;
            }

        }

        public int ReadBit()
        {
            if (this.ReadingPosition == -1 || this.ReadingPosition == this.ReadingLength)
            {
                this.ReadingByte = this.ReadEncodedByte(out var length);
                this.ReadingPosition = 0;
                this.ReadingLength = length;
            }

            if (this.ReadingByte == -1)
            {
                return -1;
            }

            var shift = 7 - this.ReadingPosition;
            var bitMask = 1 << shift;
            var bit = (this.ReadingByte & bitMask) >> shift;
            this.ReadingPosition++;
            this.InBits++;

            return bit;
        }

        public override int ReadByte()
        {
            var value = 0;

            for (var i = 0; i < 8; i++)
            {
                var bit = this.ReadBit();

                if (bit == -1)
                {
                    return bit;
                }

                var shift = 7 - i;
                value |= bit << shift;
            }

            this.InBytes++;
            return value;
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

        public void WriteBit(bool bit) => this.WriteBit(bit ? 1 : 0);

        public void WriteBit(int bit)
        {
            var shift = 7 - this.WritingPosition;
            this.WritingByte |= bit << shift;
            this.WritingPosition++;
            this.OutBits++;

            if (this.TryWriteEncodedByte((byte)this.WritingByte, this.WritingPosition, false) == true)
            {
                this.WritingByte = 0;
                this.WritingPosition = 0;
            }

        }

        public override void WriteByte(byte value)
        {
            for (var i = 0; i < 8; i++)
            {
                var shift = 7 - i;
                var bitMask = 1 << shift;
                var bit = (value & bitMask) >> shift;
                this.WriteBit(bit);
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
                this.TryWriteEncodedByte((byte)this.WritingByte, this.WritingPosition, true);
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
