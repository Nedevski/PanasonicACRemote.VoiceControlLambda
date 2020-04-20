<?php
require_once("IRValues.php");

// Set up DB
$authJsonPath =     __DIR__ . '/DB/users.json';
$stateJsonPath =    __DIR__ . '/DB/acRemoteState.json';
$credsJsonPath =    __DIR__ . '/DB/acRemoteCredentials.json';

// Get credentials
$creds = json_decode(file_get_contents($credsJsonPath), true);
$baseUrl = $creds['url'];
$authToken = $creds['token'];

$irCodes = [
    'state' => $state,
    'mode' => $mode,
    'temp' => $temp,
    'fan' => $fan,
    'swing' => $swing,
    'modifiers' => $modifiers
];

function GetCommand($input)
{
    global $stateJsonPath;
    global $baseCommand;
    global $modifiers;
    global $fan;
    $cmd = $baseCommand;

    global $defaultState;
    $defaultState = json_decode(file_get_contents($stateJsonPath), true);

    // Receive the appropriate IR hex codes for the settings
    $acState =  GetOrDefault('state',       $input, $defaultState);
    $acMode =   GetOrDefault('mode',        $input, $defaultState);
    $acTemp =   GetOrDefault('temp',        $input, $defaultState);
    $acFan =    GetOrDefault('fan',         $input, $defaultState);
    $acSwing =  GetOrDefault('swing',       $input, $defaultState);
    $acMods =   GetOrDefault('modifiers',   $input, $defaultState);

    // if modifier is None or Powerful, set fan to auto
    if ($acMods == $modifiers['1'] || $acMods == $modifiers['3']) {
        $acFan = $fan['0']; // 0 = auto
    }
    else if ($acMods == $modifiers['2']) { // quiet
        $acFan = $fan['1'];
    }

    $cmd[5] = $acState . $acMode;
    $cmd[6] = $acTemp;
    $cmd[8] = $acSwing . $acFan;
    $cmd[13] = $acMods;

    $cmd[18] = CalculateChecksum($cmd);
    return implode('', $cmd);
}

function GetOrDefault($nameOfSetting, $receivedCommand, $jsonDefaults)
{
    global $irCodes;
    $valueOfSetting = isset($receivedCommand[$nameOfSetting]) ? $receivedCommand[$nameOfSetting] : $jsonDefaults[$nameOfSetting];
    return $irCodes[$nameOfSetting][$valueOfSetting];
}

function CalculateChecksum($command) {
    $sum = 0;

    // loop through elements 0-18, ignoring 19 - the actual checksum
    for($i = 0; $i < 18; $i++) {
        $hex2bool = base_convert($command[$i], 16, 2); // convert from hex to binary
        $formattedBool = sprintf("%08s", $hex2bool); // format binary with leading zeros
        $reversedBool = strrev($formattedBool); // reverse binary
        $sum += intval(base_convert($reversedBool, 2, 10)); // convert to decimal, convert to int, add to sum
    }

    $rawCheckSum = base_convert($sum, 10, 2); // convert result to boolean string
    $reversedChecksum = strrev($rawCheckSum); // reverse the string back to normal
    $checkSum = substr($reversedChecksum, 0, 8); // ignore info overflowing over 1B (8 chars)

    $hexCheckSum = base_convert($checkSum, 2, 16); // convert checksum to hex
    return strtoupper(sprintf("%02s", $hexCheckSum)); // format with leading zero and cast to uppercase
}

function ReturnStatusCode($code) {
    http_response_code($code);
    die;
}

function UpdateAndEchoStateFile($receivedCommand) {
    global $defaultState;
    global $stateJsonPath;

    foreach ($receivedCommand as $key => $val) {
        if (isset($defaultState[$key])) {
            $defaultState[$key] = $val;

            if ($key == 'modifiers') {
                if ($val == '1' || $val == '3') { // normal or powerful
                    $defaultState['fan'] = '0';
                }
                else if ($val == '2') { // quiet
                    $defaultState['fan'] = '1';
                }
            }
        }
    }

    $newSettings = json_encode($defaultState);
    file_put_contents($stateJsonPath, $newSettings);

    header('Content-Type: application/json');
    echo file_get_contents($stateJsonPath);
}

function validateToken($token) {
    global $authJsonPath;

    $authInfo = file_get_contents($authJsonPath);
    $authData = json_decode($authInfo, true);
    $users = $authData['users'];

    foreach ($users as $i => $user) {
        if ($user['token'] == $token){
            return $user['name'];
        }
    }

    return false;
}

function getTokenForUser($name, $pass) {
    global $authJsonPath;

    $authInfo = file_get_contents($authJsonPath);
    $authData = json_decode($authInfo, true);
    $users = $authData['users'];

    foreach ($users as $i => $user) {
        if (strtolower($user['name']) == strtolower($name) && $user['pass'] == $pass){
            return $user['token'];
        }
    }

    return false;
}