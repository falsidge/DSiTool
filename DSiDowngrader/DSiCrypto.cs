using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.IO;

namespace DSiDowngrader
{

    class DSi_CTR
    {
        public byte[] _Key = new byte[16];
        public byte[] Key
        {
            get { return _Key; }
            set { Array.Reverse(value as byte[]); _Key = value; }
        }

        public byte[] _Ctr = new byte[16];
        public byte[] Ctr
        {
            get { return _Ctr; }
            set { Array.Reverse(value as byte[]); _Ctr = value; }
        }
        public void add_ctr(int carry)
        {
            byte sum;
            int i;

            for (i = 15; i >= 0; i--)
            {

                sum = Convert.ToByte((_Ctr[i] + carry)%256 );

                carry = (_Ctr[i] + carry) / 256;

                _Ctr[i] = sum;
            }
        }
        AesManaged aesAlg;
        ICryptoTransform encryptor = null;
        public byte[] Crypt(byte[] input)
        {
            if (encryptor == null)
            {
                aesAlg = new AesManaged
                {
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.None,
                    BlockSize = 128
                };
                encryptor = aesAlg.CreateEncryptor(_Key, null);
            }
            byte[] output = new byte[16];

            byte[] stream = new byte[16];
            stream = encryptor.TransformFinalBlock(_Ctr, 0, 16);
            if (input != null)
            {
                for (int i = 0; i < 16; i++)
                {
                    output[i] = Convert.ToByte(stream[15 - i] ^ input[i]); ;
                }
            }
            else
            {
                for (int i = 0; i < 16; i++)
                {
                    output[i] = Convert.ToByte(stream[15 - i]);
                }
            }
            this.add_ctr(1);
            return output;
        }
        public byte[] Crypt_all(Stream input, int size)
        {
            using (MemoryStream ms = new MemoryStream())
            {

                    byte[] buffer = new byte[0x10];
                    for (int i = 0; i < size; i += 0x10)
                    {

                        input.Read(buffer, 0, 0x10);
                        ms.Write(Crypt(buffer),0,0x10);
                    }

                return ms.ToArray();
            }

        }
    }
    class DSi_CCM
    {
        public byte[] Key = new byte[16];
        public int maclen;
        private byte[] mac = new byte[16];
        private byte[] ctr = new byte[16];
        private byte[] S0 = new byte[16];
        public DSi_CCM(byte[] key, int maclength, int payloadlength, int assoclength, byte[] nonce)
        {
            this.Key = key;
            this.maclen = maclength;

            maclength = (maclength - 2) / 2;
            payloadlength = (payloadlength + 15) & ~15;

            mac[0] = Convert.ToByte((maclength << 3) | 2);
            if (assoclength !=0)
                mac[0] |= (1 << 6);
            for (int i = 0; i < 12; i++)
                mac[1 + i] = nonce[11 - i];
            mac[13] = Convert.ToByte(payloadlength >> 16);
            mac[14] = Convert.ToByte(payloadlength >> 8);
            mac[15] = Convert.ToByte((payloadlength >> 0)%256);

            AesManaged aesAlg = new AesManaged
            {
                Mode = CipherMode.ECB
            };
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(key, null);
            mac = encryptor.TransformFinalBlock(mac, 0, 16);

            ctr[0] = 2;
            for (int i = 0; i < 12; i++)
                ctr[1 + i] = nonce[11 - i];
            ctr[13] = 0;
            ctr[14] = 0;
            ctr[15] = 0;
            DSi_CTR cryptoctx = new DSi_CTR()
            {
                _Key = key,
                _Ctr = ctr
            };
            S0 = cryptoctx.Crypt(null);
        }
        public byte[] Decrypt_Block(byte[] input, ref byte[] paramac)
        {
            DSi_CTR cryptoctx = new DSi_CTR()
            {
                _Key = Key,
                _Ctr = ctr
            };
            byte[] output = cryptoctx.Crypt(input);
            for (int i = 0; i < 16; i++)
                mac[i] ^= output[15 - i];

            AesManaged aesAlg = new AesManaged
            {
                Mode = CipherMode.ECB
            };
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(Key, null);
            mac = encryptor.TransformFinalBlock(mac, 0, 16);
            for (int i = 0; i < 16; i++)
                paramac[i] = Convert.ToByte(mac[15 - i] ^ S0[i]);
            return output;
        }
        public byte[] Decrypt(byte[] input, out byte[] paramac)
        {
            byte[] block = new byte[16];
            byte[] tblock = new byte[16];
            byte[] tctr = new byte[16];
            paramac = new byte[16];
            int size = input.Length -0x20;
            byte[] outblock = new byte[size];

            int i = 0;
            Array.Copy(input, i, tblock, 0, 16);
            while (size > 16)
            {
                byte[] oblock = Decrypt_Block(tblock, ref paramac);
                Array.Copy(oblock, 0, outblock, i, 16);
                size -= 16;
                i += 16;
                Array.Copy(input, i, tblock, 0, 16);
            }
            DSi_CTR cryptoctx = new DSi_CTR()
            {
                _Key = Key,
                _Ctr = ctr.Clone() as byte[]
            };
            
            block = cryptoctx.Crypt(block);
            Array.Copy(input, i, block, 0, size);

            block = Decrypt_Block(block, ref paramac);
            Array.Copy(block, 0, outblock, i, size);

            return outblock;
        }
    }
    class DSi_ES
    {
        public byte[] Key = new byte[16];

        private const uint KeySize = 128;
        public bool randomnonce = true;
        private byte[] _nonce;
        public byte[] nonce
        {
            get { return _nonce; }
            set { _nonce = value; randomnonce = false; }
        }
        public byte[] Decrypt(byte[] buffer, byte[] metablock)
        {
            byte[] ctr = new byte[16];
            byte[] dnonce = new byte[12];
            byte[] scratchpad = new byte[16];
            byte[] chkmac = new byte[16];
            byte[] genmac = new byte[16];
            byte[] blockcrypt = new byte[16];
            int chksize;

            Array.Copy(metablock, chkmac, 16);
            Array.Copy(metablock, 16, ctr, 0, 16);
            Array.Copy(metablock, 16, blockcrypt, 0, 16);
            ctr[0] = 0;
            ctr[13] = 0;
            ctr[14] = 0;
            ctr[15] = 0;

            DSi_CTR cryptoctx = new DSi_CTR()
            {
                Key = Key,
                Ctr = ctr
            };
            scratchpad = cryptoctx.Crypt(blockcrypt);
            chksize = (scratchpad[13] << 16) | (scratchpad[14] << 8) | (scratchpad[15] << 0);

           if (scratchpad[0] != 0x3A || chksize != buffer.Length-0x20)
                return null;
            Array.Copy(metablock, 17, dnonce, 0, 12);

            DSi_CCM crypt = new DSi_CCM(Key, 16, buffer.Length - 0x20, 0, dnonce);
            buffer = crypt.Decrypt(buffer, out genmac);

            return buffer;
        }
    }
    public static class KeyCrypto
    {
        public static void n128_lrot(ref byte[]  num , int shift )
        {
            UInt64[] tmp = new UInt64[2];

            tmp[0] = BitConverter.ToUInt64(num, 0) << shift;
            tmp[1] = BitConverter.ToUInt64(num, 8) << shift;
            tmp[0] |= (BitConverter.ToUInt64(num, 8) >> (64 - shift));
            tmp[1] |= (BitConverter.ToUInt64(num, 0) >> (64 - shift));

            Array.Copy(BitConverter.GetBytes(tmp[0]), num, 8);
            Array.Copy(BitConverter.GetBytes(tmp[1]), 0, num, 8, 8);
        }
        public static void n128_add(ref byte[] a, ref byte[] b)
        {
            UInt64 a64 = BitConverter.ToUInt64(a, 0);
            UInt64 a641 = BitConverter.ToUInt64(a, 8);
            UInt64 b64 = BitConverter.ToUInt64(b, 0);
            UInt64 tmp = (a64 >> 1) + (b64 >> 1) + (a64 & b64 & 1);

            tmp = tmp >> 63;
            a64 += b64;
            a641 += BitConverter.ToUInt64(b, 8) + tmp;
            a = new byte[16];

            Array.Copy(BitConverter.GetBytes(a64), a, 8);
            Array.Copy(BitConverter.GetBytes(a641), 0, a, 8, 8);
        }

        public static void F_XY(out byte[]  key, ref byte[] key_x, ref byte[] key_y)
        {
            int i;
            byte[] key_xy = new byte[16];

            for (i = 0; i < 16; i++) key_xy[i] = Convert.ToByte((key_x)[i] ^ (key_y)[i]);
            key = new byte[]{
                0x79, 0x3e, 0x4f, 0x1a,
                0x5f, 0x0f, 0x68, 0x2a,
                0x58, 0x02,0x59,0x29,
                0x4e,0xfb,0xfe,0xff};


            n128_add(ref key, ref key_xy);

            n128_lrot( ref key, 42);
        }

    }
}
