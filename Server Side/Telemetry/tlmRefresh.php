<?php
	//Agrega una nueva linea al fichero de telemetria
	$fp = fopen("telemetry_" . $_POST["ident"] . ".tlm", "a");
	fwrite($fp, "\n" . $_POST["log"]);
	fclose($fp);
?>