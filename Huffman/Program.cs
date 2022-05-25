using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;

namespace Huffman
{
    public class Program
    {
        public static void Main(string[] args)
        {
            TestAll("AAAAAAABBCCCDEEEEFFFFFFGHIIJ");
            //TestAll(new string('f', 5) + new string('e', 9) + new string('c', 12) + new string('b', 13) + new string('d', 16) + new string('a', 45));

            TestSerialize(new byte[][] { new byte[] { }, new byte[] { 0x00, 0x01 }, new byte[] { 0x02, 0x11 }, new byte[] { 0x03, 0x21 }, new byte[] { 0x12, 0x31 }, new byte[] { 0x41 }, new byte[] { 0x04, 0x13, 0x22, 0x51 }, new byte[] { 0x32, 0x61 }, new byte[] { 0x71 }, new byte[] { 0x05, 0x14, 0x42 }, new byte[] { 0x23, 0x33, 0x81, 0x91 }, new byte[] { 0x15, 0xA1, 0xB1 }, new byte[] { 0x52 }, new byte[] { }, new byte[] { 0x62, 0xF0 }, new byte[] { 0xC1, 0xD1, 0xE1 }, });
        }

        public static void TestSerialize<T>(T[][] huffmanTable)
        {
            var restoredRootNode = HuffmanNode<T>.FromTable(huffmanTable);
            var restoredHuffmanTable = restoredRootNode.ToTable();

            Console.WriteLine();
            Console.WriteLine($"Serialization As Table Result");

            if (restoredHuffmanTable.Length != huffmanTable.Length)
            {
                throw new InvalidProgramException("Table Changed");
            }

            for (var depth = 0; depth < restoredHuffmanTable.Length; depth++)
            {
                var values = restoredHuffmanTable[depth];

                Console.WriteLine();
                Console.WriteLine($"   Length : {depth + 1}");
                Console.WriteLine($"   Values : {string.Join(", ", values.Select(x => $"0x{x:X2}"))}");

                if (values.SequenceEqual(huffmanTable[depth]) == false)
                {
                    throw new InvalidProgramException("Table Changed");
                }

            }

        }

        public static void TestAll<T>(IEnumerable<T> collection)
        {
            var rootNode = HuffmanNode<T>.CreateRootNode(collection);
            var nodeMap = rootNode.ToMap();

            Console.WriteLine();
            Console.WriteLine("Nodes By Element");
            Console.WriteLine();

            foreach (var pair in nodeMap.OrderBy(p => p.Key))
            {
                Console.WriteLine($"    {pair.Key} : {pair.Value}");
            }

            Console.WriteLine();
            Console.WriteLine("Nodes By Table");

            var table = rootNode.ToTable();

            for (var depth = 0; depth < table.Length; depth++)
            {
                var values = table[depth];

                Console.WriteLine();
                Console.WriteLine($"   Length : {depth + 1}");
                Console.WriteLine($"   Values : {string.Join(", ", values)}");
            }

            Console.WriteLine();
            Console.WriteLine("Bitstream Encode Result");
            Console.WriteLine();
            Console.WriteLine($"    {string.Join("", collection.Select(n => nodeMap[n]))}");

            Console.WriteLine();
            Console.WriteLine("Serialization As Table Result");
            Console.WriteLine();

            var restoredRootNode = HuffmanNode<T>.FromTable(table);

            foreach (var pair in restoredRootNode.ToMap())
            {
                var prevPath = nodeMap[pair.Key];
                var currPath = pair.Value;
                Console.WriteLine($"    {pair.Key} : {prevPath} => {currPath}");

                if (prevPath.Length != currPath.Length)
                {
                    throw new InvalidProgramException("Length Changed");
                }

            }

        }

        public class HuffmanNode<T> : IEnumerable<HuffmanNode<T>>
        {
            public static HuffmanNode<T> FromTable(T[][] table)
            {
                var prevDepthNodes = new List<HuffmanNode<T>>();

                for (var depth = table.Length - 1; depth > -1; depth--)
                {
                    var nodes = table[depth].Select(v => new HuffmanNode<T>(v)).ToList();
                    nodes.AddRange(prevDepthNodes);

                    var carry = new List<HuffmanNode<T>>();

                    for (var i = 0; i < nodes.Count; i += 2)
                    {
                        var left = nodes[i];
                        var right = (i + 1 < nodes.Count) ? nodes[i + 1] : null;
                        var node = new HuffmanNode<T>(left, right);
                        carry.Add(node);
                    }

                    prevDepthNodes.Clear();
                    prevDepthNodes.AddRange(carry);
                }

                if (prevDepthNodes.Count == 1)
                {
                    return prevDepthNodes[0];
                }
                else if (prevDepthNodes.Count == 2)
                {
                    return new HuffmanNode<T>(prevDepthNodes[0], prevDepthNodes[1]);
                }
                else
                {
                    throw new ArgumentException("Invalid HuffmanTable", nameof(table));
                }

            }

            public static HuffmanNode<T> CreateRootNode(IEnumerable<T> collection)
            {
                var counts = new Dictionary<HuffmanNode<T>, int>();
                var nodes = new List<HuffmanNode<T>>();

                foreach (var pair in collection.GroupBy(c => c).Select(g => (Count: g.Count(), Value: g.Key)).OrderByDescending(n => n.Count))
                {
                    var node = new HuffmanNode<T>(pair.Value);
                    counts[node] = pair.Count;
                    nodes.Add(node);
                }

                if (nodes.Count > 1)
                {
                    while (nodes.Count > 1)
                    {
                        //Console.WriteLine(string.Join(", ", nodes));
                        var index1 = nodes.Count - 1;
                        var node1 = nodes[index1];
                        nodes.RemoveAt(index1);
                        var count1 = counts[node1];
                        counts.Remove(node1);

                        var index2 = nodes.Count - 1;
                        var node2 = nodes[index2];
                        nodes.RemoveAt(index2);
                        var count2 = counts[node2];
                        counts.Remove(node2);

                        var test = count2 > count1;
                        var left = test ? node1 : node2;
                        var right = test ? node2 : node1;

                        var node = new HuffmanNode<T>(left, right);
                        var count = count1 + count2;
                        //Console.WriteLine($"{node} => {count}");
                        var insertIndex = 0;

                        for (insertIndex = nodes.Count; insertIndex > 0; insertIndex--)
                        {
                            if (count <= counts[nodes[insertIndex - 1]])
                            {
                                break;
                            }

                        }

                        if (insertIndex == -1)
                        {
                            nodes.Add(node);
                        }
                        else
                        {
                            nodes.Insert(insertIndex, node);
                        }

                        counts[node] = count;
                    }

                    return nodes.First();
                }
                else
                {
                    return new HuffmanNode<T>(nodes.First(), default);
                }

            }

            public T Value { get; }
            public HuffmanNode<T> Left { get; }
            public HuffmanNode<T> Right { get; }
            public bool HasChildren { get; }
            public int Depth => (this.Max(n => n?.Depth) ?? 0) + 1;

            public HuffmanNode(T value)
            {
                this.Value = value;
            }

            public HuffmanNode(HuffmanNode<T> left, HuffmanNode<T> right)
            {
                this.Value = default;
                this.Left = left;
                this.Right = right;
                this.HasChildren = true;
            }

            public T[][] ToTable()
            {
                var table = new List<T[]>();
                var nextNodes = new List<HuffmanNode<T>>(this);

                while (nextNodes.Count > 0)
                {
                    var nodes = new List<HuffmanNode<T>>(nextNodes);
                    nextNodes.Clear();

                    var values = new List<T>();

                    foreach (var node in nodes)
                    {
                        if (node.HasChildren == true)
                        {
                            nextNodes.AddRange(node);
                        }
                        else
                        {
                            values.Add(node.Value);
                        }

                    }

                    table.Add(values.ToArray());
                }

                return table.ToArray();
            }

            public Dictionary<T, HuffmanPath> ToMap() => this.ToMap(default);

            public Dictionary<T, HuffmanPath> ToMap(HuffmanPath path)
            {
                var map = new Dictionary<T, HuffmanPath>();

                if (this.HasChildren == true)
                {
                    var children = Enumerable.ToArray(this);

                    for (var i = 0; i < children.Length; i++)
                    {
                        foreach (var pair in children[i].ToMap(new HuffmanPath(path, i)))
                        {
                            map[pair.Key] = pair.Value;
                        }

                    }

                }
                else
                {
                    map[this.Value] = path;
                }

                return map;
            }

            public override int GetHashCode() => this.ToString().GetHashCode();

            public override string ToString()
            {
                var builder = new StringBuilder();

                if (this.HasChildren == true)
                {
                    builder.Append($"[{string.Join(", ", this)}]");
                }
                else
                {
                    builder.Append($"{this.Value}");
                }

                return builder.ToString();
            }

            public IEnumerator<HuffmanNode<T>> GetEnumerator()
            {
                if (this.Left != null)
                {
                    yield return this.Left;
                }

                if (this.Right != null)
                {
                    yield return this.Right;
                }

            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            public struct HuffmanPath : IEquatable<HuffmanPath>, IComparable<HuffmanPath>
            {
                public int Path { get; set; }
                public int Length { get; set; }

                public HuffmanPath(HuffmanPath path, int a)
                {
                    this.Path = path.Path << 1 | a;
                    this.Length = path.Length + 1;
                }

                public override bool Equals(object obj) => obj is HuffmanNode<T> other && this.Equals(other);

                public bool Equals(HuffmanPath other) => this.Path == other.Path && this.Length == other.Length;

                public override int GetHashCode() => HashCode.Combine(this.Path, this.Length);

                public override string ToString() => Convert.ToString(this.Path, 2).PadLeft(this.Length, '0');

                public int CompareTo(HuffmanPath other) => this.ToString().CompareTo(other.ToString());

                public static bool operator ==(HuffmanPath left, HuffmanPath right) => left.Equals(right);

                public static bool operator !=(HuffmanPath left, HuffmanPath right) => !(left == right);

            }

        }

    }

}
