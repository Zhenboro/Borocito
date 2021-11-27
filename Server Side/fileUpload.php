<?php
	move_uploaded_file($_FILES["file"]["tmp_name"], "Files/" . $_FILES["file"]["name"]);
?>