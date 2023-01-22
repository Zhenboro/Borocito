<?php
	if (isset($_POST['ident']) && $_POST['ident']!="") {
		if (isset($_POST['log']) && $_POST['log']!="") {
			$fp = fopen("telemetry_" . $_POST["ident"] . ".tlm", "a");
			fwrite($fp, "\n" . $_POST["log"]);
			fclose($fp);
		}
	} else {
		die("
<h1>Actualizar telemetria de instancias</h1>
<h3>Borocito Server-Side</h3>
<p>
	<li>Envia <em>UID as ident</em> y <em>content as log</em>, y sobreescribe la telemetria  (.tlm) en una nueva linea.</li>
	<ul>
		<strong>Uso</strong>:
		<pre>
	[POST form-data:
		params:
			ident
			log
	]
		</pre>
		<ul>
			<li>ident: UID from Borocito CLI Instance</li>
			<li>log: String content to save</li>
		</ul>
	</ul>
</p>
		");
	}
?>