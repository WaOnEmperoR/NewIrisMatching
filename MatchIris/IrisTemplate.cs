using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MatchIris
{
    public class IrisTemplate
    {
        public static byte[] Export(IrisCode irisCode)
        {
            byte[] template;
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8))
                {
                    checked
                    {
                        // 4B magic "11215"
                        writer.Write("IRIS".ToCharArray());

                        // 4B ex
                        writer.Write("CODE".ToCharArray());

                        // 4B total length (including header, will be updated later)
                        writer.Write(0);

                        // 1B rubbish (eye position, zeroed)
                        writer.Write((byte)0);

                        // 1B rubbish (zeroed)
                        writer.Write((byte)0);

                        int ByteIndex;
                        byte[] irisBit = new byte[irisCode.size / 8];
                        byte[] irisMask = new byte[irisCode.size / 8];
                        for (int i = 0; i < irisCode.size / 8; i++)
                        {
                            ByteIndex = (i * 8);
                            irisBit[i] = (byte)(irisCode.bit[ByteIndex] * 128 + irisCode.bit[ByteIndex + 1] * 64 +
                                                irisCode.bit[ByteIndex + 2] * 32 + irisCode.bit[ByteIndex + 3] * 16 +
                                                irisCode.bit[ByteIndex + 4] * 8 + irisCode.bit[ByteIndex + 5] * 4 +
                                                irisCode.bit[ByteIndex + 6] * 2 + irisCode.bit[ByteIndex + 7]);
                            //      1B bitImage
                            writer.Write(irisBit[i]);
                        }

                        // 2B rubbish (extra data length, zeroed)
                        writer.Write((short)0);

                        for (int i = 0; i < irisCode.size / 8; i++)
                        {
                            ByteIndex = (i * 8);
                            irisMask[i] = (byte)(irisCode.mask[ByteIndex] * 128 + irisCode.mask[ByteIndex + 1] * 64 +
                                                irisCode.mask[ByteIndex + 2] * 32 + irisCode.mask[ByteIndex + 3] * 16 +
                                                irisCode.mask[ByteIndex + 4] * 8 + irisCode.mask[ByteIndex + 5] * 4 +
                                                irisCode.mask[ByteIndex + 6] * 2 + irisCode.mask[ByteIndex + 7]);
                            //      1B maskImage (ignored, zeroed)
                            writer.Write(irisMask[i]);
                        }

                        // 2B rubbish (extra data length, zeroed)
                        writer.Write((short)0);
                    }
                }
                // update length
                template = stream.ToArray();
                BitConverter.GetBytes(IPAddress.HostToNetworkOrder(template.Length)).CopyTo(template, 8);
            }
            return template;
        }

        public static IrisCode Import(byte[] template)
        {
            IrisCode irisCode = new IrisCode();

            int size = 2048;

            using (MemoryStream stream = new MemoryStream(template))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {

                    // 4B magic "IRIS"
                    AssertException.Check(new String(reader.ReadChars(4)) == "IRIS", "This is not an IRIS template.");

                    // 4B ex
                    AssertException.Check(new String(reader.ReadChars(4)) == "CODE", "This is not an IRIS template.");

                    // 4B total length (including header)
                    AssertException.Check(IPAddress.NetworkToHostOrder(reader.ReadInt32()) == template.Length, "Invalid template length.");

                    // 1B rubbish (eye position, zeroed)
                    reader.ReadByte();

                    // 1B rubbish (zeroed)
                    reader.ReadByte();

                    irisCode.size = size;
                    irisCode.bit = new byte[size];
                    irisCode.mask = new byte[size];

                    for (int i = 0; i < size / 8; ++i)
                    {
                        byte num = reader.ReadByte();

                        int ByteIndex = i * 8;
                        string str = ToBinary(num);
                        for (byte s = 0; s < 8; s++)
                        {
                            if (str[s] == '1')
                            {
                                irisCode.bit[ByteIndex + s] = 1;
                            }
                            else
                            {
                                irisCode.bit[ByteIndex + s] = 0;
                            }
                        }
                    }
                    // 2B rubbish (extra data length, zeroed)
                    reader.ReadInt16();
                    for (int i = 0; i < size / 8; ++i)
                    {
                        byte num = reader.ReadByte();
                        int ByteIndex = i * 8;
                        string str = ToBinary(num);
                        for (byte s = 0; s < 8; s++)
                        {
                            if (str[s] == '1')
                            {
                                irisCode.mask[ByteIndex + s] = 1;
                            }
                            else
                            {
                                irisCode.mask[ByteIndex + s] = 0;
                            }
                        }
                    }

                    // 2B rubbish (extra data length, zeroed)

                }
            }
            return irisCode;
        }

        public static IrisCode NewImport(byte[] template)
        {
            IrisCode irisCode = new IrisCode();

            int size = 2048;

            using (MemoryStream stream = new MemoryStream(template))
            {
                using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8))
                {

                    // 4B magic "IRIS"
                    AssertException.Check(new String(reader.ReadChars(4)) == "IRIS", "This is not an IRIS template.");

                    // 4B ex
                    AssertException.Check(new String(reader.ReadChars(4)) == "CODE", "This is not an IRIS template.");

                    // 4B total length (including header)
                    AssertException.Check(IPAddress.NetworkToHostOrder(reader.ReadInt32()) == template.Length, "Invalid template length.");

                    // 1B rubbish (eye position, zeroed)
                    reader.ReadByte();

                    // 1B rubbish (zeroed)
                    reader.ReadByte();

                    irisCode.size = size;
                    irisCode.bit = new byte[size];
                    irisCode.mask = new byte[size];
                    irisCode.newBit = new uint[size / 32];
                    irisCode.newMask = new uint[size / 32];

                    for (int i = 0; i < size / 32; ++i)
                    {
                        uint num = ReverseBytes(reader.ReadUInt32());
                        irisCode.newBit[i] = num;
                    }
                    // 2B rubbish (extra data length, zeroed)
                    reader.ReadInt16();
                    for (int i = 0; i < size / 32; ++i)
                    {
                        uint num = ReverseBytes(reader.ReadUInt32());
                        irisCode.newMask[i] = num;
                    }

                }
            }
            return irisCode;
        }

        private static uint ReverseBytes(uint value)
        {
            return (value & 0x000000FFU) << 24 | (value & 0x0000FF00U) << 8 |
                (value & 0x00FF0000U) >> 8 | (value & 0xFF000000U) >> 24;
        }

        private static string ToBinary(Int64 Decimal)
        {

            Int64 BinaryHolder;

            char[] BinaryArray;

            string BinaryResult = "";

            while (Decimal > 0)
            {

                BinaryHolder = Decimal % 2;

                BinaryResult += BinaryHolder;

                Decimal = Decimal / 2;

            }

            BinaryArray = BinaryResult.ToCharArray();

            Array.Reverse(BinaryArray);

            BinaryResult = new string(BinaryArray);

            if (BinaryResult.Length == 7)
                BinaryResult = "0" + BinaryResult;
            else if (BinaryResult.Length == 6)
                BinaryResult = "00" + BinaryResult;
            else if (BinaryResult.Length == 5)
                BinaryResult = "000" + BinaryResult;
            else if (BinaryResult.Length == 4)
                BinaryResult = "0000" + BinaryResult;
            else if (BinaryResult.Length == 3)
                BinaryResult = "00000" + BinaryResult;
            else if (BinaryResult.Length == 2)
                BinaryResult = "000000" + BinaryResult;
            else if (BinaryResult.Length == 1)
                BinaryResult = "0000000" + BinaryResult;
            else if (BinaryResult.Length == 0)
                BinaryResult = "00000000";

            return BinaryResult;

        }

        public static void WriteTemplate(byte[] template, String templateName)
        {
            using (FileStream fileStream = new FileStream(templateName, FileMode.Create))
            {
                if (template == null)
                {
                    return;
                }

                for (int x = 0; x < template.Length; x++)
                {
                    fileStream.WriteByte(template[x]);
                }
            }
        }

        public static byte[] ReadTemplate(String templateName)
        {
            byte[] template = File.ReadAllBytes(templateName);
            return template;
        }
    }
}
