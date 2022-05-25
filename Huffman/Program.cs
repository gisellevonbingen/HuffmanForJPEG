using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Huffman
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestString("AAAAAAABBCCCDEEEEFFFFFFGHIIJ");
            //TestAll(new string('f', 5) + new string('e', 9) + new string('c', 12) + new string('b', 13) + new string('d', 16) + new string('a', 45));

            TestSerialize(new byte[][] { new byte[] { }, new byte[] { 0x00, 0x01 }, new byte[] { 0x02, 0x11 }, new byte[] { 0x03, 0x21 }, new byte[] { 0x12, 0x31 }, new byte[] { 0x41 }, new byte[] { 0x04, 0x13, 0x22, 0x51 }, new byte[] { 0x32, 0x61 }, new byte[] { 0x71 }, new byte[] { 0x05, 0x14, 0x42 }, new byte[] { 0x23, 0x33, 0x81, 0x91 }, new byte[] { 0x15, 0xA1, 0xB1 }, new byte[] { 0x52 }, new byte[] { }, new byte[] { 0x62, 0xF0 }, new byte[] { 0xC1, 0xD1, 0xE1 }, });
        }

        public static void TestSerialize<T>(T[][] simbolTable)
        {
            var restoredRootNode = HuffmanNode<T>.FromTable(simbolTable);
            var restoredSimbolTable = restoredRootNode.ToSimbolTable();

            Console.WriteLine();
            Console.WriteLine($"Serialization As Table Result");

            if (restoredSimbolTable.Length != simbolTable.Length)
            {
                throw new InvalidProgramException("Table Changed");
            }

            for (var depth = 0; depth < restoredSimbolTable.Length; depth++)
            {
                var simbols = restoredSimbolTable[depth];

                Console.WriteLine();
                Console.WriteLine($"   Length : {depth + 1}");
                Console.WriteLine($"   Simbols : {string.Join(", ", simbols.Select(x => $"0x{x:X2}"))}");

                if (simbols.SequenceEqual(simbolTable[depth]) == false)
                {
                    throw new InvalidProgramException("Table Changed");
                }

            }

        }

        public static void TestString(string text)
        {
            var bytes = Encoding.ASCII.GetBytes(text);
            var rootNode = HuffmanNode<byte>.CreateRootNode(bytes);
            var nodeMap = rootNode.ToCodeMap();

            Console.WriteLine();
            Console.WriteLine("Nodes By Element");
            Console.WriteLine();

            foreach (var pair in nodeMap.OrderBy(p => p.Key))
            {
                Console.WriteLine($"    {pair.Key} : {pair.Value}");
            }

            Console.WriteLine();
            Console.WriteLine("Nodes By Table");

            var sombolTable = rootNode.ToSimbolTable();

            for (var depth = 0; depth < sombolTable.Length; depth++)
            {
                var simbols = sombolTable[depth];

                Console.WriteLine();
                Console.WriteLine($"   Length : {depth + 1}");
                Console.WriteLine($"   Simbols : {string.Join(", ", simbols)}");
            }

            Console.WriteLine();
            Console.WriteLine("Bitstream Encode/Decode Result");
            Console.WriteLine();
            Console.WriteLine($"    Original");
            Console.WriteLine($"        BitStream\t: {string.Join("", bytes.Select(n => nodeMap[n]))}");
            Console.WriteLine($"        ByteStream\t: {string.Join("", bytes.Select(n => $"{n:X2}"))}");
            Console.WriteLine($"        String\t\t: {text}");

            byte[] compresseds = null;

            using (var ms = new MemoryStream())
            {
                using (var hs = new HuffmanStream(ms, rootNode))
                {
                    hs.Write(bytes, 0, bytes.Length);
                }

                compresseds = ms.ToArray();
                Console.WriteLine();
                Console.WriteLine($"    Encode");
                Console.WriteLine($"        BitStream\t: {string.Join("", compresseds.Select(n => Convert.ToString(n, 2).PadLeft(8, '0')))}");
                Console.WriteLine($"        ByteStream\t: {string.Join("", compresseds.Select(n => $"{n:X2}"))}");
            }

            Console.WriteLine();
            Console.WriteLine("    Compare Size");

            using (var ms = new MemoryStream())
            {
                Console.WriteLine($"        Huffman\t\t: {bytes.Length} => {compresseds.Length} ({(bytes.Length - compresseds.Length) / (bytes.Length / 100.0F):F2}%)");

                using (var deflate = new DeflateStream(ms, CompressionLevel.Optimal))
                {
                    deflate.Write(bytes, 0, bytes.Length);
                }

                var deflateBytes = ms.ToArray();
                Console.WriteLine($"        Deflate\t\t: {bytes.Length} => {deflateBytes.Length} ({(bytes.Length - deflateBytes.Length) / (bytes.Length / 100.0F):F2}%)");
            }

            using (var ms = new MemoryStream(compresseds))
            {
                using (var hs = new HuffmanStream(ms, rootNode))
                {
                    using (var temp = new MemoryStream())
                    {
                        hs.CopyTo(temp);
                        var decompresseds = temp.ToArray();

                        Console.WriteLine();
                        Console.WriteLine($"    Decode");
                        Console.WriteLine($"        BitStream\t: {string.Join("", decompresseds.Select(n => nodeMap[n]))}");
                        Console.WriteLine($"        String\t\t: {Encoding.ASCII.GetString(decompresseds)}");
                    }

                }

            }

            Console.WriteLine();
            Console.WriteLine("Serialization As Table Result");
            Console.WriteLine();

            var restoredRootNode = HuffmanNode<byte>.FromTable(sombolTable);

            foreach (var pair in restoredRootNode.ToCodeMap())
            {
                var prevCode = nodeMap[pair.Key];
                var currCode = pair.Value;
                Console.WriteLine($"    {pair.Key} : {prevCode} => {currCode}");

                if (prevCode.Length != currCode.Length)
                {
                    throw new InvalidProgramException("Length Changed");
                }

            }

        }

    }

}
