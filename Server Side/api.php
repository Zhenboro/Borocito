<?php
    // CONSTS
    const STATUS = "status";
    const DESCRIPTION = "description";

    const USERS_DIR = "Users/";
    const TELEMETRY_DIR = "Telemetry/";
    const COMMANDS_DIR = USERS_DIR . "Commands/";

    const COMMAND_PREFIX = "[";
    const COMMAND_EXTENSION = "]Command.str";

    const REPORT_PREFIX = "userID_";
    const REPORT_EXTESION = ".rtp";

    const TELEMETRY_PREFIX = "telemetry_";
    const TELEMETRY_EXTESION = ".tlm";

$retorno = array(
    STATUS => "",
    DESCRIPTION => ""
);
$clase = null;
$ident = null;
$content = null;
$headers = getallheaders();
function VerifySession() {
    global $headers, $clase, $retorno, $ident;
    if (!array_key_exists('Clase', $headers)) {
        http_response_code(400);
        $retorno[STATUS] = "NO_CLASS_HEADER";
        die(json_encode($retorno));
    } if ((!isset($headers['Clase'])) || $headers['Clase'] == "") {
        http_response_code(400);
        $retorno[STATUS] = "CLASS_IS_EMPTY";
        die(json_encode($retorno));
    } if (!array_key_exists('Ident', $headers)) {
        http_response_code(400);
        $retorno[STATUS] = "NO_UID_HEADER";
        die(json_encode($retorno));
    } if ((!isset($headers['Ident'])) || $headers['Ident'] == "") {
        http_response_code(400);
        $retorno[STATUS] = "UID_IS_EMPTY";
        die(json_encode($retorno));
    }

    $ident = $headers['Ident'];
    $clase = $headers['Clase'];

    if (!file_exists(USERS_DIR.REPORT_PREFIX.$ident.REPORT_EXTESION)) {
        http_response_code(401);
        $retorno[STATUS] = "WHO_THE_FUCK_ARE_YOU";
        $retorno[DESCRIPTION] = "YOU FUCKING BITCH, WHO THE FUCKA RE U LMAOO";
        die(json_encode($retorno));
    }
} VerifySession();
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    header('Content-Type: application/json; charset=UTF-8');
    if ((!isset($_POST['content'])) || $_POST['content'] == "") {
        http_response_code(400);
        $retorno[STATUS] = "CONTENT_IS_EMPTY";
        die(json_encode($retorno));
    }
    $content = $_POST['content'];
    switch ($clase) {
        case "PING":
            http_response_code(200);
            $retorno[STATUS] = "PONG!";
            $retorno[DESCRIPTION] = $clase;
            die(json_encode($retorno));
            break;
        case "COMMAND":
            $archivo = COMMANDS_DIR . COMMAND_PREFIX . $ident . COMMAND_EXTENSION;
            if (file_exists($archivo)) {
                file_put_contents($archivo, $content);
            } else {
                $myfile = fopen($archivo, "w");
                fwrite($myfile, $content);
                fclose($myfile);
            }
            break;
        case "TELEMETRY":
            $archivo = TELEMETRY_DIR . TELEMETRY_PREFIX . $ident . TELEMETRY_EXTESION;
            if (file_exists($archivo)) {
                $fp = fopen($archivo, "a");
                fwrite($fp, "\n" . $content);
                fclose($fp);
            } else {
                $myfile = fopen($archivo, "w");
                fwrite($myfile, $content);
                fclose($myfile);
            }
            break;
        case "USER_REPORT":
            $archivo = USERS_DIR . REPORT_PREFIX . $ident . REPORT_EXTESION;
            $myfile = fopen($archivo, "w");
            fwrite($myfile, $content);
            fclose($myfile);
            break;
            break;
        default:
            http_response_code(400);
            $retorno[STATUS] = "SWITCH_DEFAULT";
            die(json_encode($retorno));
    }
    http_response_code(200);
    $retorno[STATUS] = "DONE";
    $retorno[DESCRIPTION] = $clase;
    die(json_encode($retorno));
} elseif ($_SERVER['REQUEST_METHOD'] === 'PUT') {
    header('Content-Type: application/json; charset=UTF-8');
    $retorno[DESCRIPTION] = array();
    if (!is_dir(USERS_DIR)) {
        mkdir(USERS_DIR);
        $retorno[STATUS] = true;
        $retorno[DESCRIPTION]['USERS_DIR'] = array(USERS_DIR, true);
    } else {
        $retorno[STATUS] = true;
        $retorno[DESCRIPTION]['USERS_DIR'] = true;
    }
    if (!is_dir(TELEMETRY_DIR)) {
        mkdir(TELEMETRY_DIR);
        $retorno[DESCRIPTION]['TELEMETRY_DIR'] = array(TELEMETRY_DIR, true);
    } else {
        $retorno[STATUS] = true;
        $retorno[DESCRIPTION]['TELEMETRY_DIR'] = true;
    }
    if (!is_dir(COMMANDS_DIR)) {
        mkdir(COMMANDS_DIR);
        $retorno[DESCRIPTION]['COMMANDS_DIR'] = array(COMMANDS_DIR, true);
    } else {
        $retorno[STATUS] = true;
        $retorno[DESCRIPTION]['COMMANDS_DIR'] = true;
    }
    die(json_encode($retorno));
} elseif ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $retorno = null;
    switch ($clase) {
        case "PING":
            header('Content-Type: application/json; charset=UTF-8');
            http_response_code(200);
            $retorno[STATUS] = "PONG!";
            $retorno[DESCRIPTION] = $clase;
            die(json_encode($retorno));
            break;
        case "COMMAND":
            $archivo = COMMANDS_DIR . COMMAND_PREFIX . $ident . COMMAND_EXTENSION;
            if (file_exists($archivo)) {
                $retorno = file_get_contents($archivo);
            }
            break;
        case "TELEMETRY":
            $archivo = TELEMETRY_DIR . TELEMETRY_PREFIX . $ident . TELEMETRY_EXTESION;
            if (file_exists($archivo)) {
                $retorno = file_get_contents($archivo);
            }
            break;
        case "USER_REPORT":
            $archivo = USERS_DIR . REPORT_PREFIX . $ident . REPORT_EXTESION;
            if (file_exists($archivo)) {
                $retorno = file_get_contents($archivo);
            }
            break;
        default:
            http_response_code(400);
            $retorno = "<h1>SWITCH_DEFAULT</h1>";
            die($retorno);
    }
    http_response_code(200);
    die($retorno);
} else {
    http_response_code(418);
    $retorno[STATUS] = "METHOD NOT ALLOWED";
    $retorno[DESCRIPTION] = "Get the fuck away from here.";
    die(json_encode($retorno));
}