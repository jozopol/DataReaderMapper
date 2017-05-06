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
<thead><tr><th>     Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Scaled</th><th>ScaledSD</th><th>Gen 0</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>ManualMappingOnce</td><td>1.080 us</td><td>0.0213 us</td><td>0.0449 us</td><td>1.00</td><td>0.00</td><td>0.1930</td><td>912 B</td>
</tr><tr><td> MapperOnce</td><td>1.723 us</td><td>0.0425 us</td><td>0.0377 us</td><td>1.60</td><td>0.07</td><td>0.1905</td><td>912 B</td>
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
