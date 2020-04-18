<?php
/**
 * Created by PhpStorm.
 * User: Nikola
 * Date: 19-Feb-17
 * Time: 12:42
 */

$baseCommand = [
    '40', // 0
    '04', // 1
    '07',
    '20',
    '00',
    'XX', // 5 - State/Mode
    'XX', // 6 - Temperature
    '01',
    'XX', // 8 - Swing/Fan
    '00',
    '00',
    '60',
    '06',
    'XX', // 13 - Powerful/Quiet
    '00',
    '01',
    '00',
    '60',
    'XX', // 18 - Checksum
];

// Key values correspond to the .NET lambda enums
$defaultState = [
    'state' => '1',     // on
    'mode' => '2',      // heat
    'temp' => '25', 
    'fan' => '1',       // auto
    'swing' => '2',     // highest
    'modifiers' => '1'  // none
];

$state = [
    '1' => '8', // on
    '2' => '0'  // off
];

$mode = [
    '1' => '0'  // auto
    '2' => '2', // heat
    '3' => 'C', // cool
    '4' => '4', // dry
];

$temp = [
    '16' => '04',
    '17' => '44',
    '18' => '24',
    '19' => '64',
    '20' => '14',
    '21' => '51',
    '22' => '34',
    '23' => '74',
    '24' => '0C',
    '25' => '4C',
    '26' => '2C',
    '27' => '6C',
    '28' => '1C',
    '29' => '5C',
    '30' => '3C'
];

$fan = [
    '1' => '5', // auto
    '2' => 'C', // weakest
    '3' => '2',
    '4' => 'A',
    '5' => '6',
    '6' => 'E'
];

$swing = [
    '1' => 'F', // auto
    '2' => '8', // highest
    '3' => '4',
    '4' => 'C',
    '5' => '2',
    '6' => 'A'  // lowest
];

$modifiers = [
    '1' => '00', // none
    '2' => '04', // quiet
    '3' => '80', // powerful
];
