<?php
require_once("../App/Core.php");

if ($_SERVER['REQUEST_METHOD'] !== 'POST') {
    echo 'Only POST requests supported';
    ReturnStatusCode(400);
}

if (!isset($_POST) || !isset($_POST['user']) || !isset($_POST['pass']))
{
    echo 'Credentials missing';
    ReturnStatusCode(400);
}

$userToken = getTokenForUser($_POST['user'], $_POST['pass']);

if ($userToken) {
    echo $userToken;
    ReturnStatusCode(200);
}
else {
    echo "User not found";
    ReturnStatusCode(401);
}
