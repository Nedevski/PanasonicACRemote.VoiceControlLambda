using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Alexa.NET.Response;
using Alexa.NET.Request;
using Alexa.NET.Request.Type;
using Newtonsoft.Json;
using Alexa.NET;
using System.Net.Http;
using System.Text;
using PanasonicACVoiceRemote.Model;
using System.Net;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace PanasonicACVoiceRemote
{
    public class PanasonicACRemoteFunction
    {
        private HttpClient _httpClient;

        //private readonly string serverUrl = Environment.GetEnvironmentVariable("SERVER_URL_DEV");
        private readonly string serverUrl = Environment.GetEnvironmentVariable("SERVER_URL_PROD");
        private readonly string authToken = Environment.GetEnvironmentVariable("SERVER_AUTH_TOKEN");

        public PanasonicACRemoteFunction()
        {
            _httpClient = new HttpClient();

            _httpClient.BaseAddress = new Uri(serverUrl);
            _httpClient.DefaultRequestHeaders.Add("Token", authToken);
        }

        public async Task<SkillResponse> ACRemoteHandler(SkillRequest input, ILambdaContext context)
        {
            ILambdaLogger log = context.Logger;
            log.LogLine($"Skill Request Object:" + JsonConvert.SerializeObject(input));

            Session session = input.Session;
            if (session.Attributes == null)
                session.Attributes = new Dictionary<string, object>();

            Type requestType = input.GetRequestType();
            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                return ResponseBuilder.Tell("AC REMOTE SKILL");
            }
            else if (input.GetRequestType() == typeof(SessionEndedRequest))
            {
                return ResponseBuilder.Tell("Goodbye!");
            }
            else if (input.GetRequestType() == typeof(IntentRequest))
            {
                var intentRequest = (IntentRequest)input.Request;
                switch (intentRequest.Intent.Name)
                {
                    default:
                    case "StatusCheck":
                        {
                            var response = await _httpClient.GetAsync("/");
                            ACState status = JsonConvert.DeserializeObject<ACState>(await response.Content.ReadAsStringAsync());

                            var voiceResponse = $"The AC is {status.Power}. It's set to ${status.Temperature} degrees on ${status.Mode} mode.";

                            if (status.Modifiers != Modifier.NotSet)
                            {
                                voiceResponse += $" The ${status.Modifiers} setting is on.";
                            }

                            if (status.Power == Power.Off)
                            {
                                voiceResponse += " Do you want to turn it on?";
                                return ResponseBuilder.Ask(voiceResponse, new Reprompt("Do you want to turn it on?"));
                            }
                            else
                            {
                                string cardText = string.Concat(voiceResponse,
                                    $" Fan is set to ${status.Fan}, swing position is set to ${status.Swing} and the fan modifier is ${status.Modifiers}.");

                                return ResponseBuilder.TellWithCard(voiceResponse, "AC Status", cardText);
                            }
                        }
                    case "TurnACOn":
                    case "AMAZON.YesIntent":
                        {
                            ACState state = new ACState()
                            {
                                Power = Power.On
                            };

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            return response.IsSuccessStatusCode
                                ? ResponseBuilder.Tell("Ok!")
                                : ErrorResponse(response);
                        }
                    case "TurnACOff":
                        {
                            ACState state = new ACState()
                            {
                                Power = Power.Off
                            };

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            return response.IsSuccessStatusCode
                                ? ResponseBuilder.Tell("Ok!")
                                : ErrorResponse(response);
                        }
                    case "UpdateAction":
                        {
                            ACState state = new ACState();

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            return response.IsSuccessStatusCode
                                ? ResponseBuilder.Tell("AC Updated")
                                : ErrorResponse(response);
                        }
                    case "TempControl":
                        {
                            string temperatureString = intentRequest.Intent.Slots["temperature"].Value;

                            if (!int.TryParse(temperatureString, out int temp))
                            {
                                return ResponseBuilder.Tell("Invalid temperature!");
                            }
                            else if (temp < 16) return ResponseBuilder.Tell("That's too low");
                            else if (temp > 30) return ResponseBuilder.Tell("That's too high");

                            ACState state = new ACState()
                            {
                                Power = Power.On,
                                Temperature = temp,
                            };

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            return response.IsSuccessStatusCode
                                ? ResponseBuilder.TellWithCard($"Changed to {temp}", "AC Remote", $"The temperature was set to {temp}")
                                : ErrorResponse(response);
                        }
                    case "ModeControl":
                        {
                            string modeStr = intentRequest.Intent.Slots["mode"].Value;

                            Mode mode = Mode.NotSet;
                            Modifier modifier = Modifier.NotSet;

                            if (modeStr == "auto" || modeStr == "automatic") mode = Mode.Auto;
                            else if (modeStr == "cool" || modeStr == "cold") mode = Mode.Cold;
                            else if (modeStr == "hot" || modeStr == "warm" || modeStr == "heat") mode = Mode.Hot;
                            else if (modeStr == "dry") mode = Mode.Dry;

                            if (modeStr == "normal") modifier = Modifier.Normal;
                            else if (modeStr == "quiet") modifier = Modifier.Quiet;
                            else if (modeStr == "powerful") modifier = Modifier.Powerful;

                            if (mode == Mode.NotSet && modifier == Modifier.NotSet)
                            {
                                return ResponseBuilder.Tell("Error parsing the mode.");
                            }

                            ACState state = new ACState()
                            {
                                Mode = mode,
                                Modifiers = modifier
                            };

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            if (response.IsSuccessStatusCode)
                            {
                                if (mode != Mode.NotSet)
                                {
                                    return ResponseBuilder.TellWithCard($"Mode set to {mode}", "AC Remote", $"Mode set to {mode}");
                                }

                                else
                                {
                                    return ResponseBuilder.TellWithCard($"Mode set to {modifier}", "AC Remote", $"Mode set to {modifier}");
                                }
                            }
                            else
                            {
                                return ErrorResponse(response);
                            }
                        }
                    case "SwingControl":
                        {
                            string swingStr = intentRequest.Intent.Slots["swing"].Value;

                            Swing swing = Swing.NotSet;

                            if (swingStr == "auto" || swingStr == "automatic") swing = Swing.Auto;
                            else if (swingStr == "high") swing = Swing.High;
                            else if (swingStr == "semi high") swing = Swing.SemiHigh;
                            else if (swingStr == "medium" || swingStr == "middle") swing = Swing.Middle;
                            else if (swingStr == "semi low") swing = Swing.SemiLow;
                            else if (swingStr == "low") swing = Swing.Low;

                            if (swing == Swing.NotSet)
                            {
                                return ResponseBuilder.Tell("Invalid swing position.");
                            }

                            ACState state = new ACState()
                            {
                                Power = Power.On,
                                Swing = swing
                            };

                            var response = await _httpClient.PostAsync("/", state.AsJson());

                            return response.IsSuccessStatusCode
                               ? ResponseBuilder.TellWithCard($"Swing set to {swing}", "AC Remote", $"The swing position was set to {swing}")
                               : ErrorResponse(response);
                        }
                    case "AMAZON.NoIntent":
                        {
                            return ResponseBuilder.Tell("Alright!");
                        }
                    case "AMAZON.CancelIntent":
                    case "AMAZON.StopIntent":
                        return ResponseBuilder.Tell("Goodbye!");
                }
            }
            return ResponseBuilder.Tell("Goodbye!");
        }

        private SkillResponse ErrorResponse(HttpResponseMessage response)
        {
            string message = "Unknown error";
            var statusCode = response.StatusCode; 

            if (statusCode == HttpStatusCode.BadRequest) message = "Command sent to the server was invalid";
            else if (statusCode == HttpStatusCode.Unauthorized) message = "The authorization token is invalid";
            else if (statusCode == HttpStatusCode.RequestTimeout) message = "The remote appears to be offline";
            else if (statusCode == HttpStatusCode.InternalServerError) message = "The remote returned an error";

            return ResponseBuilder.Tell(message);
        }
    }
}