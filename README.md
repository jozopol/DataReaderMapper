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
<thead><tr><th>          Method</th><th>RowCount</th><th>   Mean</th><th>Error</th><th>StdDev</th><th> Median</th><th>Scaled</th><th>ScaledSD</th><th>Gen 0</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>   ManualMapping</td><td> 1</td><td>1.063 us</td><td>0.0202 us</td><td>0.0179 us</td><td>1.056 us</td><td>1.00</td><td>0.00</td><td>0.1940</td><td>912 B</td>
</tr><tr><td>          Mapper</td><td> 1</td><td>1.156 us</td><td>0.0041 us</td><td>0.0039 us</td><td>1.156 us</td><td>1.09</td><td>0.02</td><td>0.1945</td><td>912 B</td>
</tr><tr><td>ManualMappingNullables</td><td> 1</td><td>1.632 us</td><td>0.0320 us</td><td>0.0393 us</td><td>1.619 us</td><td>1.54</td><td>0.04</td><td>0.2018</td><td>912 B</td>
</tr><tr><td> MapperNullables</td><td> 1</td><td>1.732 us</td><td>0.0363 us</td><td>0.0586 us</td><td>1.701 us</td><td>1.63</td><td>0.06</td><td>0.1910</td><td>912 B</td>
</tr><tr><td>   ManualMapping</td><td>100</td><td>48.410 us</td><td>0.2824 us</td><td>0.2503 us</td><td>48.453 us</td><td>1.00</td><td>0.00</td><td>1.7415</td><td>10417 B</td>
</tr><tr><td>          Mapper</td><td>100</td><td>60.191 us</td><td>1.1728 us</td><td>1.1519 us</td><td>60.096 us</td><td>1.24</td><td>0.02</td><td>1.7415</td><td>10417 B</td>
</tr><tr><td>ManualMappingNullables</td><td>100</td><td>99.035 us</td><td>2.0346 us</td><td>3.2271 us</td><td>97.455 us</td><td>2.05</td><td>0.07</td><td>1.2370</td><td>10417 B</td>
</tr><tr><td> MapperNullables</td><td>100</td><td>110.121 us</td><td>1.3666 us</td><td>1.2115 us</td><td>110.050 us</td><td>2.27</td><td>0.03</td><td>1.2370</td><td>10417 B</td>
</tr><tr><td>   ManualMapping</td><td>10000</td><td>5,459.895 us</td><td>17.3924 us</td><td>14.5235 us</td><td>5,459.823 us</td><td>1.00</td><td>0.00</td><td>153.6458</td><td>960915 B</td>
</tr><tr><td>          Mapper</td><td>10000</td><td>6,607.959 us</td><td>178.8386 us</td><td>183.6540 us</td><td>6,588.717 us</td><td>1.21</td><td>0.03</td><td>161.9792</td><td>960915 B</td>
</tr><tr><td>ManualMappingNullables</td><td>10000</td><td>10,341.382 us</td><td>199.8632 us</td><td>177.1735 us</td><td>10,363.080 us</td><td>1.89</td><td>0.03</td><td>128.7500</td><td>961040 B</td>
</tr><tr><td> MapperNullables</td><td>10000</td><td>11,609.333 us</td><td>195.8278 us</td><td>183.1774 us</td><td>11,592.456 us</td><td>2.13</td><td>0.03</td><td>178.4958</td><td>961046 B</td>
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
