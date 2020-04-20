<?php
require_once("../App/Core.php");

$token = '';

if (!isset($_SERVER['HTTP_TOKEN'])) {
    echo 'Missing token';
    ReturnStatusCode(401);
}
else {
    $token = validateToken($_SERVER['HTTP_TOKEN']);

    if (!$token) {
        echo 'Invalid token';
        ReturnStatusCode(401);
    }
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $inputJSON = file_get_contents('php://input');
    $input = json_decode($inputJSON, true); //convert JSON into array

    if (!isset($input))
    {
        echo 'Invalid JSON';
        ReturnStatusCode(400);
    }
    else {
        $command = GetCommand($input);

        $ch = curl_init();
        $timeout = 3;
        $url = $baseUrl . $command;
        
        curl_setopt($ch, CURLOPT_URL, $url);
        curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
        curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, $timeout);
        curl_setopt($ch, CURLOPT_TIMEOUT, $timeout);

        $result = curl_exec($ch);
        $statusCode = curl_getinfo($ch, CURLINFO_HTTP_CODE);
        curl_close($ch);

        if ($statusCode == 200) {
            UpdateAndEchoStateFile($input);
            ReturnStatusCode(200); // Success
        }
        else if ($statusCode == 0) {
            echo 'Remote offline';
            ReturnStatusCode(408); // NodeMCU is offline
        }
        else {
            echo 'Remote error';
            ReturnStatusCode(500); // NodeMCU threw an error
        }
    }
}
else {
    header('Content-Type: application/json');
    echo file_get_contents($stateJsonPath);
}