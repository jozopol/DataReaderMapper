# DataReaderMapper
A mapper for classes which implement IDataReader interface. DataReaderMapper maps DTO properties based on attribute decoration using Mappable/MappableSource attributes which allow you to specify custom column names from which the property should be mapped.

<b>Currently supporting:</b><br>

Properties (primitive/nested classes, can be nullable) excluding collection properties<br>
Customizable convertor expressions for specified output types (via Dictionary<Type, Expression> constructor injection)<br>
<br>

<b>TODO list:</b><br>
need more ideas...

<h2> Benchmarks </h2>

All benchmarks are created via https://github.com/dotnet/BenchmarkDotNet and are included in the source files.<br>

<h3>Mapping</h3>
<p> - average time to map one simple class with 10 properties (string types)</p>

<br>
<pre><code>
BenchmarkDotNet=v0.10.5, OS=Windows 10.0.10240
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3419248 Hz, Resolution=292.4620 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.127.1
  Mapper_Map : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.127.1
</code></pre>
<pre><code>Job=Mapper_Map  Jit=RyuJit  Platform=X64  
Runtime=Clr  
</code></pre>

<table>
<thead><tr><th>          Method</th><th>RowCount</th><th>   Mean</th><th>Error</th><th>StdDev</th><th>Scaled</th><th>Gen 0</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>   ManualMapping</td><td> 1</td><td>1.106 us</td><td>0.0245 us</td><td>0.0402 us</td><td>1.00</td><td>0.2051</td><td>912 B</td>
</tr><tr><td>          Mapper</td><td> 1</td><td>1.199 us</td><td>0.0239 us</td><td>0.0461 us</td><td>1.09</td><td>0.2108</td><td>912 B</td>
</tr><tr><td>ManualMappingNullables</td><td> 1</td><td>1.581 us</td><td>0.0320 us</td><td>0.0427 us</td><td>1.43</td><td>0.2022</td><td>912 B</td>
</tr><tr><td> MapperNullables</td><td> 1</td><td>1.728 us</td><td>0.0345 us</td><td>0.0596 us</td><td>1.56</td><td>0.2010</td><td>912 B</td>
</tr><tr><td>   ManualMapping</td><td>100</td><td>48.666 us</td><td>0.9192 us</td><td>0.8599 us</td><td>1.00</td><td>1.9531</td><td>10417 B</td>
</tr><tr><td>          Mapper</td><td>100</td><td>60.152 us</td><td>1.2015 us</td><td>1.3355 us</td><td>1.24</td><td>1.7578</td><td>10417 B</td>
</tr><tr><td>ManualMappingNullables</td><td>100</td><td>96.485 us</td><td>0.9192 us</td><td>0.8148 us</td><td>1.98</td><td>1.1068</td><td>10418 B</td>
</tr><tr><td> MapperNullables</td><td>100</td><td>109.881 us</td><td>0.2059 us</td><td>0.1826 us</td><td>2.26</td><td>1.1393</td><td>10418 B</td>
</tr><tr><td>   ManualMapping</td><td>10000</td><td>5,366.466 us</td><td>7.3970 us</td><td>6.1768 us</td><td>1.00</td><td>149.4792</td><td>960915 B</td>
</tr><tr><td>          Mapper</td><td>10000</td><td>6,475.412 us</td><td>20.4185 us</td><td>19.0995 us</td><td>1.21</td><td>141.1458</td><td>960915 B</td>
</tr><tr><td>ManualMappingNullables</td><td>10000</td><td>10,111.148 us</td><td>189.9836 us</td><td>186.5893 us</td><td>1.88</td><td>81.2500</td><td>961033 B</td>
</tr><tr><td> MapperNullables</td><td>10000</td><td>11,500.481 us</td><td>192.0987 us</td><td>179.6892 us</td><td>2.14</td><td>68.7500</td><td>961033 B</td>
</tr></tbody></table>
<hr>

<h3>Configuration</h3>

<p>- performance test of the Configure method for classes with 1/5/10 properties (primitive types).</p>
<br>
<pre><code>
BenchmarkDotNet=v0.10.5, OS=Windows 10.0.10240
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3419248 Hz, Resolution=292.4620 ns, Timer=TSC
  [Host]           : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.127.1
  Mapper_Configure : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.127.1
</code></pre>
<pre><code>Job=Mapper_Configure  Jit=RyuJit  Platform=X64  
Runtime=Clr  
</code></pre>

<table>
<thead><tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Scaled</th><th>ScaledSD</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>Configure1</td><td>17.05 ns</td><td>0.3593 ns</td><td>0.3361 ns</td><td>1.00</td><td>0.00</td><td>0 B</td>
</tr><tr><td>Configure5</td><td>16.95 ns</td><td>0.2348 ns</td><td>0.2081 ns</td><td>0.99</td><td>0.02</td><td>0 B</td>
</tr><tr><td>Configure10</td><td>17.68 ns</td><td>0.3421 ns</td><td>0.3032 ns</td><td>1.04</td><td>0.03</td><td>0 B</td>
</tr></tbody></table>
