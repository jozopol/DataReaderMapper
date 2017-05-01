# DataReaderMapper
A mapper for classes which implement IDataReader interface. DataReaderMapper maps DTO properties based on attribute decoration using Mappable/MappableSource attributes which allow you to specify custom column names from which the property should be mapped.

<b>Currently supporting:</b><br>
Properties (primitive/nested classes) excluding collection properties<br><br>

<b>TODO list:</b><br>
customizable convertors for specified types<br>
collection properties<br><br>

All benchmarks are created via https://github.com/dotnet/BenchmarkDotNet and are included in the source files.<br>

<h2>Mapping</h2>
<p> - average time to map one simple class with 10 properties (primitive types)</p>
<br>
<pre><code>
BenchmarkDotNet=v0.10.5, OS=Windows 10.0.10240
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3419250 Hz, Resolution=292.4618 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.127.1
  Mapper_Map : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.127.1
</code></pre>
<pre><code>Job=Mapper_Map  Jit=RyuJit  Platform=X64  
Runtime=Clr  
</code></pre>

<table>
<thead><tr><th>     Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Scaled</th><th>ScaledSD</th><th>Gen 0</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>ManualMappingOnce</td><td>1.409 us</td><td>0.0229 us</td><td>0.0203 us</td><td>1.00</td><td>0.00</td><td>0.1940</td><td>912 B</td>
</tr><tr><td> MapperOnce</td><td>1.534 us</td><td>0.0167 us</td><td>0.0148 us</td><td>1.09</td><td>0.02</td><td>0.1956</td><td>912 B</td>
</tr></tbody></table>
<hr>

<h2>Configuration</h2>

<p>- performance test of the Configure method for classes with 1/5/10 properties (primitive types).</p>
<br>
<pre><code>
BenchmarkDotNet=v0.10.5, OS=Windows 10.0.10240
Processor=Intel Core i7-4770 CPU 3.40GHz (Haswell), ProcessorCount=8
Frequency=3419250 Hz, Resolution=292.4618 ns, Timer=TSC
  [Host]           : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.127.1
  Mapper_Configure : Clr 4.0.30319.42000, 64bit RyuJIT-v4.6.127.1
</code></pre>
<pre><code>Job=Mapper_Configure  Jit=RyuJit  Platform=X64  
Runtime=Clr  
</code></pre>

<table>
<thead><tr><th>Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Scaled</th><th>ScaledSD</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>Configure1</td><td>24.69 ns</td><td>0.4656 ns</td><td>0.4128 ns</td><td>1.00</td><td>0.00</td><td>0 B</td>
</tr><tr><td>Configure5</td><td>24.89 ns</td><td>0.4843 ns</td><td>0.4293 ns</td><td>1.01</td><td>0.02</td><td>0 B</td>
</tr><tr><td>Configure10</td><td>25.54 ns</td><td>0.5066 ns</td><td>0.4739 ns</td><td>1.03</td><td>0.02</td><td>0 B</td>
</tr></tbody></table>
