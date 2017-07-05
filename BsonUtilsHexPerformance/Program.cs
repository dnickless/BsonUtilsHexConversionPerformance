using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Exporters;
using BenchmarkDotNet.Attributes.Jobs;
using BenchmarkDotNet.Running;

namespace BsonUtilsHexPerformance
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (!CheckCorrectness())
            {
                throw new Exception("not correct");
            }

            BenchmarkRunner.Run<MyBenchmark>();
        }

        private static bool CheckCorrectness()
        {
            if (BsonUtilsOld.ToHexString(MyBenchmark.Bytes) != BsonUtilsNew.ToHexString(MyBenchmark.Bytes))
            {
                return false;
            }

            if (!BsonUtilsOld.ParseHexString(MyBenchmark.HexString).SequenceEqual(BsonUtilsNew.ParseHexString(MyBenchmark.HexString)))
            {
                return false;
            }

            for (var i = 0; i < 15; i++)
            {
                if (BsonUtilsOld.ToHexChar(i) != BsonUtilsNew.ToHexChar(i))
                {
                    return false;
                }
            }

            return true;
        }
    }

    [DryJob]
    [LegacyJitX86Job, LegacyJitX64Job, RyuJitX64Job]
    [ClrJob, CoreJob, MonoJob]
    [HtmlExporter]
    public class MyBenchmark
    {
        public static readonly byte[] Bytes = File.ReadAllBytes(@"words.txt");
        public static readonly string HexString = BsonUtilsOld.ToHexString(Bytes);
        public static readonly int[] Ints = GetRandomInts().ToArray();

        private static IEnumerable<int> GetRandomInts()
        {
            var r = new Random(17);
            for (var i = 0; i < 10000; i++)
            {
                yield return r.Next(0, 15);
            }
        }

        [Benchmark(Description = "ToHexString: old")]
        public void ToHexStringOld()
        {
            for (var i = 0; i < 10; i++)
            {
                BsonUtilsOld.ToHexString(Bytes);
            }
        }

        [Benchmark(Description = "ParseHexString: old")]
        public void ParseHexStringOld()
        {
            for (var i = 0; i < 10; i++)
            {
                BsonUtilsOld.ParseHexString(HexString);
            }
        }

        [Benchmark(Description = "ToHexChar: old")]
        public void ToHexCharOld()
        {
            for (var i = 0; i < Ints.Length; i++)
            {
                BsonUtilsOld.ToHexChar(Ints[i]);
            }
        }

        [Benchmark(Description = "ToHexString: new")]
        public void ToHexStringNew()
        {
            for (var i = 0; i < 10; i++)
            {
                BsonUtilsNew.ToHexString(Bytes);
            }
        }

        [Benchmark(Description = "ParseHexString: new")]
        public void ParseHexStringNew()
        {
            for (var i = 0; i < 10; i++)
            {
                BsonUtilsNew.ParseHexString(HexString);
            }
        }

        [Benchmark(Description = "ToHexChar: new")]
        public void ToHexCharNew()
        {
            for (var i = 0; i < Ints.Length; i++)
            {
                BsonUtilsNew.ToHexChar(Ints[i]);
            }
        }
    }
}