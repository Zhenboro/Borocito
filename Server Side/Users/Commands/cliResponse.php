<?php
/* $file = "userID_" . $_POST["ident"]  . ".rtp"; */
$file = "[" . $_POST["ident"]. "]Command.str";
//check if the file exist
	if (file_exists($file))
	{
		//file exist.
		if (isset($_POST['text']))
		{
			//save the text contents
			file_put_contents($file, $_POST['text']);
		}
	}
	else
	{
		//file doesnt exist.
		$myfile = fopen($file, "w") or die("Unable to open file!");
		$log = $_POST['text'];
		fwrite($myfile, $log);
		fclose($myfile);
	}
?>