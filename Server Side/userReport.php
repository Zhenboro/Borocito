<?php
	if (isset($_POST['ident']) && $_POST['ident']!="") {
		$ubicacion = "Users/";
		$fileName = "userID_" . $_POST["ident"];
		$nombrearchivo = $fileName . ".rtp";
		if (isset($_POST['log']) && $_POST['log']!="") {
			$myfile = fopen($ubicacion.$nombrearchivo, "w") or die("Unable to open file!");
			fwrite($myfile, $_POST["log"]);
			fclose($myfile);
		}
	} else {
		die("
<h1>Reporte de usuarios</h1>
<h3>Borocito Server-Side</h3>
<p>
	<li>Envia <em>UID as ident</em> y <em>content as log</em>, y guarda el contenido en un archivo como reporte (.rtp)</li>
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