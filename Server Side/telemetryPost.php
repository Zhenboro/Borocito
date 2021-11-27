<?php
	//Crea telemetria sobre el arranque y funcionamiento del software
	$ubicacion = "Telemetry/";
	$fileName = "telemetry_" . $_POST["ident"];
	$nombrearchivo = $fileName . ".tlm";
	$thefile = $ubicacion . $nombrearchivo;
	//Ver si el archivo ya existe
	if (file_exists($ubicacion . $nombrearchivo))
	{
		//file exist
		//ref Telemetry/tlmRefresh.php
		$fp = fopen($ubicacion . $nombrearchivo, "a");
		fwrite($fp, "\n" . $_POST["log"]);
		fclose($fp);
	}
	else
	{
		//file doesnt exist
		$myfile = fopen($ubicacion . $nombrearchivo, "w") or die("Unable to open file!");
		fwrite($myfile, $_POST["log"]);
		fclose($myfile);
	}
?>