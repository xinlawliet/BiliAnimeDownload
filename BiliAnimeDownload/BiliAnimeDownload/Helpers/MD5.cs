﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BiliAnime.Helpers
{
    public sealed class MD5 : IDisposable
    {
        private readonly byte[] _buffer = new byte[0x40];
        private readonly uint[] _count = new uint[2];
        private readonly uint[] _state = new uint[4];
        private const int HashSizeValue = 0x80;
        private byte[] HashValue;
        private static readonly byte[] PADDING;
        private const byte S11 = 7;
        private const byte S12 = 12;
        private const byte S13 = 0x11;
        private const byte S14 = 0x16;
        private const byte S21 = 5;
        private const byte S22 = 9;
        private const byte S23 = 14;
        private const byte S24 = 20;
        private const byte S31 = 4;
        private const byte S32 = 11;
        private const byte S33 = 0x10;
        private const byte S34 = 0x17;
        private const byte S41 = 6;
        private const byte S42 = 10;
        private const byte S43 = 15;
        private const byte S44 = 0x15;
        private int State;

        static MD5()
        {
            byte[] buffer = new byte[0x40];
            buffer[0] = 0x80;
            PADDING = buffer;
        }

        internal MD5()
        {
            this.Initialize();
        }

        public void Clear()
        {
            this.Dispose(true);
        }

        public byte[] ComputeHash(byte[] buffer)
        {
            return this.ComputeHash(buffer, 0, buffer.Length);
        }

        public byte[] ComputeHash(Stream inputStream)
        {
            int num;
            this.Initialize();
            byte[] buffer = new byte[0x1000];
            while (0 < (num = inputStream.Read(buffer, 0, 0x1000)))
            {
                this.HashCore(buffer, 0, num);
            }
            this.HashValue = this.HashFinal();
            return (byte[])this.HashValue.Clone();
        }

        public byte[] ComputeHash(byte[] buffer, int offset, int count)
        {
            this.Initialize();
            this.HashCore(buffer, offset, count);
            this.HashValue = this.HashFinal();
            return (byte[])this.HashValue.Clone();
        }

        public static MD5 Create()
        {
            return new MD5();
        }

        public static MD5 Create(string hashName)
        {
            if (hashName != "MD5")
            {
                throw new NotSupportedException();
            }
            return new MD5();
        }

        private static void Decode(uint[] output, int outputOffset, byte[] input, int inputOffset, int count)
        {
            int num3 = inputOffset + count;
            int index = outputOffset;
            for (int i = inputOffset; i < num3; i += 4)
            {
                output[index] = ((uint)(input[i] | (input[i + 1] << 8)) | (uint)(input[i + 2] << 0x10)) | (uint)(input[i + 3] << 0x18);
                index++;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (!disposing)
            {
                this.Initialize();
            }
        }

        private static void Encode(byte[] output, int outputOffset, uint[] input, int inputOffset, int count)
        {
            int num3 = outputOffset + count;
            int index = inputOffset;
            for (int i = outputOffset; i < num3; i += 4)
            {
                output[i] = (byte)(input[index] & 0xff);
                output[i + 1] = (byte)((input[index] >> 8) & 0xff);
                output[i + 2] = (byte)((input[index] >> 0x10) & 0xff);
                output[i + 3] = (byte)((input[index] >> 0x18) & 0xff);
                index++;
            }
        }

        private static uint F(uint x, uint y, uint z)
        {
            return ((x & y) | (~x & z));
        }

        private static void FF(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
        {
            a += (F(b, c, d) + x) + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private static uint G(uint x, uint y, uint z)
        {
            return ((x & z) | (y & ~z));
        }

        public static byte[] GetMd5Bytes(byte[] source)
        {
            return Create().ComputeHash(source);
        }

        public static string GetMd5String(string source)
        {
            MD5 md = Create();
            byte[] bytes = new UTF8Encoding().GetBytes(source);
            byte[] buffer2 = md.ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            foreach (byte num in buffer2)
            {
                builder.Append(((byte)num).ToString("x2"));
            }
            return builder.ToString();
        }

        private static void GG(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
        {
            a += (G(b, c, d) + x) + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private static uint H(uint x, uint y, uint z)
        {
            return ((x ^ y) ^ z);
        }

        private void HashCore(byte[] input, int offset, int count)
        {
            int num;
            int num2 = (int)((this._count[0] >> 3) & 0x3f);
            if ((this._count[0] += (uint)(count << 3)) < (count << 3))
            {
                this._count[1]++;
            }
            this._count[1] += (uint)count >> 0x1d;
            int num3 = 0x40 - num2;
            if (count >= num3)
            {
                Buffer.BlockCopy(input, offset, this._buffer, num2, num3);
                this.Transform(this._buffer, 0);
                for (num = num3; (num + 0x3f) < count; num += 0x40)
                {
                    this.Transform(input, offset + num);
                }
                num2 = 0;
            }
            else
            {
                num = 0;
            }
            Buffer.BlockCopy(input, offset + num, this._buffer, num2, count - num);
        }

        private byte[] HashFinal()
        {
            byte[] output = new byte[0x10];
            byte[] buffer2 = new byte[8];
            Encode(buffer2, 0, this._count, 0, 8);
            int num = (int)((this._count[0] >> 3) & 0x3f);
            int count = (num < 0x38) ? (0x38 - num) : (120 - num);
            this.HashCore(PADDING, 0, count);
            this.HashCore(buffer2, 0, 8);
            Encode(output, 0, this._state, 0, 0x10);
            this._count[0] = this._count[1] = 0;
            this._state[0] = 0;
            this._state[1] = 0;
            this._state[2] = 0;
            this._state[3] = 0;
            this.Initialize();
            return output;
        }

        private static void HH(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
        {
            a += (H(b, c, d) + x) + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        private static uint I(uint x, uint y, uint z)
        {
            return (y ^ (x | ~z));
        }

        private static void II(ref uint a, uint b, uint c, uint d, uint x, byte s, uint ac)
        {
            a += (I(b, c, d) + x) + ac;
            a = ROTATE_LEFT(a, s);
            a += b;
        }

        public void Initialize()
        {
            this._count[0] = this._count[1] = 0;
            this._state[0] = 0x67452301;
            this._state[1] = 0xefcdab89;
            this._state[2] = 0x98badcfe;
            this._state[3] = 0x10325476;
        }

        private static uint ROTATE_LEFT(uint x, byte n)
        {
            return ((x << n) | (x >> (0x20 - n)));
        }

        private void Transform(byte[] block, int offset)
        {
            uint a = this._state[0];
            uint b = this._state[1];
            uint c = this._state[2];
            uint d = this._state[3];
            uint[] output = new uint[0x10];
            Decode(output, 0, block, offset, 0x40);
            FF(ref a, b, c, d, output[0], 7, 0xd76aa478);
            FF(ref d, a, b, c, output[1], 12, 0xe8c7b756);
            FF(ref c, d, a, b, output[2], 0x11, 0x242070db);
            FF(ref b, c, d, a, output[3], 0x16, 0xc1bdceee);
            FF(ref a, b, c, d, output[4], 7, 0xf57c0faf);
            FF(ref d, a, b, c, output[5], 12, 0x4787c62a);
            FF(ref c, d, a, b, output[6], 0x11, 0xa8304613);
            FF(ref b, c, d, a, output[7], 0x16, 0xfd469501);
            FF(ref a, b, c, d, output[8], 7, 0x698098d8);
            FF(ref d, a, b, c, output[9], 12, 0x8b44f7af);
            FF(ref c, d, a, b, output[10], 0x11, 0xffff5bb1);
            FF(ref b, c, d, a, output[11], 0x16, 0x895cd7be);
            FF(ref a, b, c, d, output[12], 7, 0x6b901122);
            FF(ref d, a, b, c, output[13], 12, 0xfd987193);
            FF(ref c, d, a, b, output[14], 0x11, 0xa679438e);
            FF(ref b, c, d, a, output[15], 0x16, 0x49b40821);
            GG(ref a, b, c, d, output[1], 5, 0xf61e2562);
            GG(ref d, a, b, c, output[6], 9, 0xc040b340);
            GG(ref c, d, a, b, output[11], 14, 0x265e5a51);
            GG(ref b, c, d, a, output[0], 20, 0xe9b6c7aa);
            GG(ref a, b, c, d, output[5], 5, 0xd62f105d);
            GG(ref d, a, b, c, output[10], 9, 0x2441453);
            GG(ref c, d, a, b, output[15], 14, 0xd8a1e681);
            GG(ref b, c, d, a, output[4], 20, 0xe7d3fbc8);
            GG(ref a, b, c, d, output[9], 5, 0x21e1cde6);
            GG(ref d, a, b, c, output[14], 9, 0xc33707d6);
            GG(ref c, d, a, b, output[3], 14, 0xf4d50d87);
            GG(ref b, c, d, a, output[8], 20, 0x455a14ed);
            GG(ref a, b, c, d, output[13], 5, 0xa9e3e905);
            GG(ref d, a, b, c, output[2], 9, 0xfcefa3f8);
            GG(ref c, d, a, b, output[7], 14, 0x676f02d9);
            GG(ref b, c, d, a, output[12], 20, 0x8d2a4c8a);
            HH(ref a, b, c, d, output[5], 4, 0xfffa3942);
            HH(ref d, a, b, c, output[8], 11, 0x8771f681);
            HH(ref c, d, a, b, output[11], 0x10, 0x6d9d6122);
            HH(ref b, c, d, a, output[14], 0x17, 0xfde5380c);
            HH(ref a, b, c, d, output[1], 4, 0xa4beea44);
            HH(ref d, a, b, c, output[4], 11, 0x4bdecfa9);
            HH(ref c, d, a, b, output[7], 0x10, 0xf6bb4b60);
            HH(ref b, c, d, a, output[10], 0x17, 0xbebfbc70);
            HH(ref a, b, c, d, output[13], 4, 0x289b7ec6);
            HH(ref d, a, b, c, output[0], 11, 0xeaa127fa);
            HH(ref c, d, a, b, output[3], 0x10, 0xd4ef3085);
            HH(ref b, c, d, a, output[6], 0x17, 0x4881d05);
            HH(ref a, b, c, d, output[9], 4, 0xd9d4d039);
            HH(ref d, a, b, c, output[12], 11, 0xe6db99e5);
            HH(ref c, d, a, b, output[15], 0x10, 0x1fa27cf8);
            HH(ref b, c, d, a, output[2], 0x17, 0xc4ac5665);
            II(ref a, b, c, d, output[0], 6, 0xf4292244);
            II(ref d, a, b, c, output[7], 10, 0x432aff97);
            II(ref c, d, a, b, output[14], 15, 0xab9423a7);
            II(ref b, c, d, a, output[5], 0x15, 0xfc93a039);
            II(ref a, b, c, d, output[12], 6, 0x655b59c3);
            II(ref d, a, b, c, output[3], 10, 0x8f0ccc92);
            II(ref c, d, a, b, output[10], 15, 0xffeff47d);
            II(ref b, c, d, a, output[1], 0x15, 0x85845dd1);
            II(ref a, b, c, d, output[8], 6, 0x6fa87e4f);
            II(ref d, a, b, c, output[15], 10, 0xfe2ce6e0);
            II(ref c, d, a, b, output[6], 15, 0xa3014314);
            II(ref b, c, d, a, output[13], 0x15, 0x4e0811a1);
            II(ref a, b, c, d, output[4], 6, 0xf7537e82);
            II(ref d, a, b, c, output[11], 10, 0xbd3af235);
            II(ref c, d, a, b, output[2], 15, 0x2ad7d2bb);
            II(ref b, c, d, a, output[9], 0x15, 0xeb86d391);
            this._state[0] += a;
            this._state[1] += b;
            this._state[2] += c;
            this._state[3] += d;
            for (int i = 0; i < output.Length; i++)
            {
                output[i] = 0;
            }
        }

        public int TransformBlock(byte[] inputBuffer, int inputOffset, int inputCount, byte[] outputBuffer, int outputOffset)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if ((inputCount < 0) || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException("inputCount");
            }
            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (this.State == 0)
            {
                this.Initialize();
                this.State = 1;
            }
            this.HashCore(inputBuffer, inputOffset, inputCount);
            if ((inputBuffer != outputBuffer) || (inputOffset != outputOffset))
            {
                Buffer.BlockCopy(inputBuffer, inputOffset, outputBuffer, outputOffset, inputCount);
            }
            return inputCount;
        }

        public byte[] TransformFinalBlock(byte[] inputBuffer, int inputOffset, int inputCount)
        {
            if (inputBuffer == null)
            {
                throw new ArgumentNullException("inputBuffer");
            }
            if (inputOffset < 0)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if ((inputCount < 0) || (inputCount > inputBuffer.Length))
            {
                throw new ArgumentException("inputCount");
            }
            if ((inputBuffer.Length - inputCount) < inputOffset)
            {
                throw new ArgumentOutOfRangeException("inputOffset");
            }
            if (this.State == 0)
            {
                this.Initialize();
            }
            this.HashCore(inputBuffer, inputOffset, inputCount);
            this.HashValue = this.HashFinal();
            byte[] buffer = new byte[inputCount];
            Buffer.BlockCopy(inputBuffer, inputOffset, buffer, 0, inputCount);
            this.State = 0;
            return buffer;
        }

        public bool CanReuseTransform
        {
            get
            {
                return true;
            }
        }

        public bool CanTransformMultipleBlocks
        {
            get
            {
                return true;
            }
        }

        public byte[] Hash
        {
            get
            {
                if (this.State != 0)
                {
                    throw new InvalidOperationException();
                }
                return (byte[])this.HashValue.Clone();
            }
        }

        public int HashSize
        {
            get
            {
                return 0x80;
            }
        }

        public int InputBlockSize
        {
            get
            {
                return 1;
            }
        }

        public int OutputBlockSize
        {
            get
            {
                return 1;
            }
        }
    }

}
