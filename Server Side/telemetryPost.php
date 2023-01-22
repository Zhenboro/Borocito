<?php
	if (isset($_POST['ident']) && $_POST['ident']!="") {
		$ubicacion = "Telemetry/";
		$fileName = "telemetry_" . $_POST["ident"];
		$nombrearchivo = $fileName . ".tlm";
		$thefile = $ubicacion . $nombrearchivo;
		if (isset($_POST['log']) && $_POST['log']!="") {
			if (file_exists($ubicacion . $nombrearchivo)) {
				$fp = fopen($ubicacion . $nombrearchivo, "a");
				fwrite($fp, "\n" . $_POST["log"]);
				fclose($fp);
			} else {
				$myfile = fopen($ubicacion . $nombrearchivo, "w") or die("Unable to open file!");
				fwrite($myfile, $_POST["log"]);
				fclose($myfile);
			}
		}
	} else {
		die("
<h1>Telemetria de instancias</h1>
<h3>Borocito Server-Side</h3>
<p>
	<li>Envia <em>UID as ident</em> y <em>content as log</em>, y guarda el contenido en un archivo como telemetria (.tlm)</li>
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