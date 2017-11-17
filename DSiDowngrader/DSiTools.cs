using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DSiDowngrader
{

    class DSiTools
    {
        static readonly byte[]  tadsrl_keyX = { 0x4a, 0x00, 0x00, 0x4e, 0x4e, 0x00, 0x00, 0x4a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
        static readonly byte[] tadsrl_keyY = { 0xcc, 0xfc, 0xa7, 0x03, 0x20, 0x61, 0xbe, 0x84, 0xd3, 0xeb, 0xa4, 0x26, 0xb8, 0x6d, 0xbe, 0xc2 };
        static readonly byte[] sd_key = { 0x3d, 0xa3, 0xea, 0x33, 0x4c, 0x86, 0xa6, 0xb0, 0x2a, 0xae, 0xdb, 0x51, 0x16, 0xea, 0x92, 0x62 };
        // UInt32[] emmc_keyY = { 0x0AB9DC76, 0xBD4DC4D3, 0x202DDD1D, 0xE1A00005 };
        byte[] emmc_keyY = {
            0x76,0xDC,0xB9,0x0A,
            0xD3, 0xC4, 0x4D, 0xBD,
            0x1D, 0xDD, 0x2D, 0x20,
            0x05,0x00,0xA0,0xE1};
        static byte[] modcrypt_shared_key = Encoding.ASCII.GetBytes("Nintendo");
        public byte[] CID;
        public byte[] ConsoleID;
        const int EOFF_BANNER = 0;
        const int ESIZE_BANNER = 0x4020;
        const int EOFF_TNA4 = (EOFF_BANNER + ESIZE_BANNER);
        const int ESIZE_TNA4 = 0xd4;
        public const int EOFF_FOOTER = (EOFF_TNA4 + ESIZE_TNA4);
        public const int ESIZE_FOOTER = 0x460;  
        //		rv = decrypt_to_buffer(sd_key, mapped_file+EOFF_FOOTER, footer_buffer,		ESIZE_FOOTER, NULL); 
        public byte[] GetCID(byte[] content)
        {
            Console.WriteLine(content[0]);
            uint enc_size = ESIZE_FOOTER;
            uint bytes_to_dec = 0;
            uint total_dec_bytes = 0;
            uint src_index = 0;
            uint dst_index = 0;
            byte[] dst = new byte[0x440];
            byte[] buffer = new byte[0x460];
            DSi_ES dec = new DSi_ES()
            {
                Key = sd_key.Clone() as byte[]
            };
            while (enc_size > 0)
            {
                bytes_to_dec = 0x20000;
                if (bytes_to_dec > enc_size - 0x20)
                {
                    bytes_to_dec = enc_size - 0x20;
                }

                Array.Copy(content, src_index, buffer, 0, bytes_to_dec);

                byte[] metablock = new byte[0x20];
                Array.Copy(content, src_index + bytes_to_dec, metablock, 0, 0x20);
                byte[] decryptblock = dec.Decrypt(buffer, metablock);

                Array.Copy(decryptblock, 0, dst, dst_index, bytes_to_dec);

                total_dec_bytes += bytes_to_dec;
                src_index += bytes_to_dec + 0x20;
                dst_index += bytes_to_dec;
                enc_size -= bytes_to_dec + 0x20;
            }
            byte[] ConIDhex = new byte[16];//remember ConsoleID is actually 8 hexadecimals
            ConsoleID = new byte[8];
            Array.Copy(dst, 0x38F, ConIDhex, 0, 16);
           
            string ConIDhexstring = Encoding.ASCII.GetString(ConIDhex);
            for (int i = 0; i < ConsoleID.Length; i++)
            {
                string byteValue = ConIDhexstring.Substring(i * 2, 2);
                ConsoleID[i] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return ConsoleID;
        } 

        public void cryptNAND(Stream nand, Stream nandwrite, ProgressBar nandprog)
        {
            byte[] emmc_keyX = new byte[16];
            byte[] emmc_normalkey = new byte[16];
            byte[] emmc_cid_hash = new byte[20];
            byte[] base_ctr = new byte[16];

            SHA1 sha = new SHA1CryptoServiceProvider();
            emmc_cid_hash = sha.ComputeHash(CID);
            Array.Copy(emmc_cid_hash, base_ctr, 16);

            Array.Reverse(ConsoleID);
            Array.Copy(ConsoleID, emmc_keyX, 4);

            Array.Copy(BitConverter.GetBytes(BitConverter.ToUInt32(ConsoleID, 0) ^ 0x24EE6906), 0, emmc_keyX, 4, 4);
            Array.Copy(BitConverter.GetBytes(BitConverter.ToUInt32(ConsoleID, 4) ^ 0xE65B601D), 0, emmc_keyX, 8, 4);

            Array.Copy(ConsoleID, 4, emmc_keyX, 12, 4);

            KeyCrypto.F_XY(out emmc_normalkey, ref emmc_keyX,ref emmc_keyY);
            DSi_CTR ctx = new DSi_CTR()
            {
                Ctr = base_ctr.Clone() as byte[],
                Key = emmc_normalkey
            };

            
            byte[] mbr = ctx.Crypt_all(nand, 0x200);
            Console.WriteLine("{0} {1}",mbr[0x1FE],mbr[0x1FF]); //magic numbers
//            byte[] temp_ctr = base_ctr.Clone() as byte[];

            ctx.Ctr = base_ctr.Clone() as byte[];
            //MemoryStream nandcrypt;
            {
                file_copy_append(nand, nandwrite, ctx, 0, 0x200, nandprog);
                file_copy_append(nand, nandwrite, null, 0x200, 0x10EE00, null);

                ctx.Ctr = base_ctr.Clone() as byte[];
                ctx.add_ctr((0x10EE00 / 0x10));
                file_copy_append(nand, nandwrite, ctx, 0x10EE00, 0x0CF00000, nandprog);
                file_copy_append(nand, nandwrite, null, 0x0CF00000, 0x0CF09A00, null);

                ctx.Ctr = base_ctr.Clone() as byte[];
                ctx.add_ctr((0x0CF09A00 / 0x10));
                file_copy_append(nand, nandwrite, ctx, 0x0CF09A00, 0x0EFC0000, nandprog);
                file_copy_append(nand, nandwrite, null, 0x0EFC0000, 0x0F000000, null);

            }
        }
        void file_copy_append(Stream nand,  Stream writer, DSi_CTR ctx, int start_addr, int end_addr, ProgressBar nandprog)
        {
            const int buf_size = 0x100000;
            byte[] buf = new byte[buf_size];


            nand.Seek(start_addr, SeekOrigin.Begin);
            for (int i = start_addr; i < end_addr; i +=buf_size)
            {
                int cur_size = (end_addr - i) >= buf_size ? buf_size : end_addr - i;

                if (ctx != null)
                {

                    buf = ctx.Crypt_all(nand, cur_size);
                }
                else
                {
                    nand.Read(buf, 0, cur_size);
                }
                writer.Write(buf, 0, cur_size);
                if (nandprog != null)
                {
                    nandprog.Value = Convert.ToInt32(100.0 * (i - start_addr) / (end_addr - start_addr));
                }
            }
            if (nandprog != null)
            {
                nandprog.Value = 100;
            }
        }
    }
}
    