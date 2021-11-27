<?php
	$ubicacion = "Users/";
	$fileName = "userID_" . $_POST["ident"];
	$nombrearchivo = $fileName . ".rtp";
	$myfile = fopen($ubicacion.$nombrearchivo, "w") or die("Unable to open file!");
		fwrite($myfile, $_POST["log"]);
		fclose($myfile);
?>