'use strict';
var request = require('request');

var laptopUrl = 'http://acremote.dev.nedevski.com/ACRemote.php';
var raspberryUrl = 'http://plovdiv.nedevski.com:10000/ACRemote/';

var baseUrl = raspberryUrl;
var authToken = 'd3120232-1fad-4a91-a40a-724a9e142c07';
var debugEnabled = true;

exports.handler = (event, context) => {
    try {
        if (event.session.new) {
            // New Session
            console.log("NEW SESSION");
        }

        switch (event.request.type) {
            case "LaunchRequest":
                // Launch Request
                console.log(`LAUNCH REQUEST`);
                checkStatus(event, context);
                //generateValidationResponse(`AC remote standing by.`, context);
                break;

            case "IntentRequest":
                // Intent Request
                console.log(`INTENT REQUEST: `, event.request.intent.name);

                switch (event.request.intent.name) {
                    case "StatusCheck":
                        checkStatus(event, context);
                        break;
                        
                    case "TurnACOn":
                        changeState(true, event, context);
                        break;
                        
                    case "TurnACOff":
                        changeState(false, event, context);
                        break;
                        
                    case "TempControl":
                        changeTemperature(event, context);
                        break;

                    case "ModeControl":
                        changeMode(event, context);
                        break;
                        
                    case "SwingControl":
                        changeSwing(event, context);
                        break;
                        
                    case "UpdateAction":
                        updateWithDefaults(event, context);
                        break;
                        
                    case "AMAZON.YesIntent":
                        turnOnAC(event, context);
                        break;
                        
                    case "AMAZON.NoIntent":
                        generateValidationResponse(`Alright.`, context);
                        break;

                    default:
                        throw "Invalid intent";
                }

                break;

            case "SessionEndedRequest":
                // Session Ended Request
                console.log(`SESSION ENDED REQUEST`);
                break;

            default:
                context.fail(`INVALID REQUEST TYPE: ${event.request.type}`);

        }

    } catch (error) { context.fail(`Exception: ${error}`); }

};

// Intents:
function changeState(isTurnedOn, event, context) {
    var acState;
    
    if (isTurnedOn === true) {
        acState = 'on';
    }
    else if (isTurnedOn === false) {
        acState = 'off';
    }
    else {
        generateValidationResponse(`Please use on or off`, context);
    }
    
    var body = "";
    
    var data = {
        state: acState
    };
    
    request.post(generateOptions(data),
        function (error, response, body) {
            log([data, response.statusCode, body]);
            if (!error && response.statusCode == 200) {
                generateVoiceResponse(`Ok.`, `The air conditioner was turned ${acState}`, context, true);
            }
            else {
                generateErrorMessage(response.statusCode, context);
            }
        }
    );
}

function changeTemperature(event, context) {
    var temp = event.request.intent.slots.temperature.value;
    
    if (temp < 16) {
        generateValidationResponse(`That's too low.`, context);
    }
    else if (temp > 30) {
        generateValidationResponse(`That's too high.`, context);
    }
    else {
        var body = "";
        
        var data = {
            state: 'on',
            temp: temp
        };
        
        request.post(generateOptions(data),
            function (error, response, body) {
                log([data, response.statusCode, body]);
                if (!error && response.statusCode == 200) {
                    generateVoiceResponse(`Changed to ${temp}`, `Temperature was changed to ${temp} degrees`, context, true);
                }
                else {
                    generateErrorMessage(response.statusCode, context);
                }
            }
        );
    }
}

function changeMode(event, context) {
    var mode = event.request.intent.slots.mode.value;
    var parsedMode = false;
    var parsedModifier = false;
    
    if (mode == 'auto' || mode == 'automatic') parsedMode = 'auto';
    else if (mode == 'dry') parsedMode = 'dry';
    else if (mode == 'hot' || mode == 'warm' || mode === 'heat') parsedMode = 'heat';
    else if (mode == 'cool' || mode == 'cold') parsedMode = 'cool';
    
    if (mode == 'powerful' || mode == 'quiet' || mode == 'normal') parsedModifier = mode;
  
    if (parsedMode === false && parsedModifier === false) {
        generateValidationResponse(`Invalid mode.`, context);
    }
    else {
        var body = "";
        var data;
        
        if (parsedMode) {
            data = {
                state: 'on',
                mode: mode
            };
        }
        else {
            var modifier = 'off';
            
            if (parsedModifier == 'powerful' || parsedModifier == 'quiet') {
                modifier = parsedModifier;
            }
            
            data = {
                state: 'on',
                modifiers: modifier
            };
        }
        
        request.post(generateOptions(data),
            function (error, response, body) {
                log([data, response.statusCode, body]);
                if (!error && response.statusCode == 200) {
                    generateVoiceResponse(`Mode set to ${mode}`, `The air conditioner was set to ${mode} mode`, context, true);
                }
                else {
                    generateErrorMessage(response.statusCode, context);
                }
            }
        );
    }
}

function changeSwing(event, context) {
    var position = event.request.intent.slots.swing.value;

    var valid = (position == 'auto' || position == 'automatic'  || position == 'high'|| position == 'semi high'  || position == 'medium' || position == 'semi low' || position == 'low');
    
    if  (!valid) {
        generateValidationResponse(`Use high, medium or low for setting the swing position.`, context);
    }
    else {
        var swingPosition = 'auto';
        
        if (position == 'high') swingPosition = 1;
        else if (position == 'semi high') swingPosition = 2;
        else if (position == 'medium') swingPosition = 3;
        else if (position == 'semi low') swingPosition = 4;
        else if (position == 'low') swingPosition = 5;
        
        var body = "";
        
        var data = {
            state: 'on',
            swing: swingPosition
        };
        
        request.post(generateOptions(data),
            function (error, response, body) {
                log([data, response.statusCode, body]);
                if (!error && response.statusCode == 200) {
                    generateVoiceResponse(`Swing set to ${position}`, `The swing position was changed to ${position}`, context, true);
                }
                else {
                    generateErrorMessage(response.statusCode, context);
                }
            }
        );
    }
}

function updateWithDefaults(response, context) {
    var body = "";
        
    var data = {};
    
    request.post(generateOptions(data),
        function (error, response, body) {
            log([data, response.statusCode, body]);
            if (!error && response.statusCode == 200) {
                generateVoiceResponse(`Ok.`, `Air conditioner was updated with the latest values`, context, true);
            }
            else {
                generateErrorMessage(response.statusCode, context);
            }
        }
    );
}

function turnOnAC(response, context) {
      var body = "";
        
        var data = {
            state: 'on'
        };
        
        request.post(generateOptions(data),
            function (error, response, body) {
                log([data, response.statusCode, body]);
                if (!error && response.statusCode == 200) {
                    generateVoiceResponse(`Ok`, `The AC was turned on.`, context, true);
                }
                else {
                    generateErrorMessage(response.statusCode, context);
                }
            }
        );
}

function checkStatus(response, context) {
    var body = "";
        
    var options = {
        uri: baseUrl,
        headers: {
            'Token': authToken
        }
    };
    
    // GET REQUEST
    request(options, function (error, response, body) {
            var status = JSON.parse(body);
            log([options, response.statusCode, status]);
            if (!error && response.statusCode == 200) {
                var endSession = true;
                var voiceResponse = `The AC is ${status.state}. It's set to ${status.temp} degrees on ${status.mode} mode.`;
                
                if (status.modifiers != 'off') {
                    voiceResponse += ` The ${status.modifiers} setting is on.`;
                }
                
                if (status.state == 'off') {
                    voiceResponse += ` Do you want to turn it on?`;
                    endSession = false;
                }
                
                var cardResponse = voiceResponse + ` Fan is set to ${status.fan}, swing position is set to ${status.swing} and the fan modifier is ${status.modifiers}.`;
                
                generateVoiceResponse(voiceResponse, cardResponse, context, endSession);
            }
            else {
                generateErrorMessage(response.statusCode, context);
            }
        }
    );
}

function generateOptions(data) {
    return {
        uri: baseUrl,
        method: 'POST',
        headers: {
            'Token': authToken
        },
        json: data
    };
}

function log(data) {
    if (debugEnabled) {
        console.log(data);
    }
}


function generateVoiceResponse(response, cardBody, context, shouldEndSession) {
    context.succeed(
        generateResponse(response, "AC Remote", cardBody, {}, shouldEndSession)
    );
}

function generateValidationResponse(response, context) {
    context.succeed(
        generateResponse(response, null, null, {}, true)
    );
}

function generateErrorMessage(statusCode, context) {
    var message = 'Unknown error.';
    
    if (statusCode == 400) message = "Command sent to the server was invalid";
    else if (statusCode == 401) message = "The authorization token is invalid";
    else if (statusCode == 408) message = "The remote appears to be offline";
    else if (statusCode == 500) message = "The remote returned an error";
    
    context.succeed(
        generateResponse(message, "AC Remote error", message, {}, true)
    );
}

function generateResponse(voiceMessage, cardTitle, cardBody, sessionAttributes, shouldEndSession) {
    var returnValue = {
        version: "1.0",
        response: {
            outputSpeech: {
                type: "PlainText",
                text: voiceMessage
            },
            shouldEndSession: shouldEndSession
        },
        sessionAttributes: sessionAttributes
    };
    
    if (cardTitle !== null && cardBody !== null) {
        returnValue.response.card = {
            type: "Simple",
            title: cardTitle,
            content: cardBody
        };
    }
    
    return returnValue;
}