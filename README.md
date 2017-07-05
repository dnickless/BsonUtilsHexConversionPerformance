# BsonUtilsHexConversionPerformance

This repository contains a Benchmark.NET based demo application that showcases the performance improvement suggested to the BsonUtils type within the MongoDB C# driver. Test results on my machine are as follows (single-threaded):

``` ini

BenchmarkDotNet=v0.10.8, OS=Windows 7 SP1 (6.1.7601)
Processor=Intel Xeon CPU X5670 2.93GHzIntel Xeon CPU X5670 2.93GHz, ProcessorCount=24
Frequency=2857451 Hz, Resolution=349.9623 ns, Timer=TSC
  [Host]       : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2053.0
  Clr          : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2053.0
  Dry          : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2053.0
  LegacyJitX64 : Clr 4.0.30319.42000, 64bit LegacyJIT/clrjit-v4.7.2053.0;compatjit-v4.7.2053.0
  LegacyJitX86 : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.7.2053.0
  RyuJitX64    : Clr 4.0.30319.42000, 64bit RyuJIT-v4.7.2053.0
```
**LegacyJit | X86**

  |                Method |            Mean |
  |---------------------- |----------------:|
  |    'ToHexString: **old**' |   235,449.30 us |
  | 'ParseHexString: **old**' | 1,585,768.98 us |
  |      'ToHexChar: **old**' |        16.98 us |
  |    'ToHexString: **new**' |   145,015.31 us |
  | 'ParseHexString: **new**' |   160,424.46 us |
  |      'ToHexChar: **new**' |        26.45 us |
  
**LegacyJit | X64**

  |                Method |            Mean |
  |---------------------- |----------------:|
  |    'ToHexString: **old**' |   237,773.84 us |
  | 'ParseHexString: **old**' | 1,974,205.16 us |
  |      'ToHexChar: **old**' |        16.22 us |
  |    'ToHexString: **new**' |   129,303.31 us |
  | 'ParseHexString: **new**' |   145,404.89 us |
  |      'ToHexChar: **new**' |        20.85 us |
  
**RyuJit | X64**

  |                Method |            Mean |
  |---------------------- |----------------:|
  |    'ToHexString: **old**' |   242,496.68 us |
  | 'ParseHexString: **old**' | 1,699,714.73 us |
  |      'ToHexChar: **old**' |        39.27 us |
  |    'ToHexString: **new**' |   154,297.40 us |
  | 'ParseHexString: **new**' |   175,683.47 us |
  |      'ToHexChar: **new**' |        10.09 us |
