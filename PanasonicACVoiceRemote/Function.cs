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



// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace PanasonicACVoiceRemote
{
    public class Function
    {
        private HttpClient _httpClient;

        private readonly string devUrl = "http://acremote.dev.nedevski.com/ACRemote.php";
        private readonly string raspberryUrl = "http://plovdiv.nedevski.com:10000/ACRemote/";

        private readonly string authToken = "d3120232-1fad-4a91-a40a-724a9e142c07";

        public Function()
        {
            _httpClient = new HttpClient();

            _httpClient.BaseAddress = new Uri(raspberryUrl);
            _httpClient.DefaultRequestHeaders.Add("Token", authToken);
        }

        public SkillResponse FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            ILambdaLogger log = context.Logger;
            log.LogLine($"Skill Request Object:" + JsonConvert.SerializeObject(input));

            Session session = input.Session;
            if (session.Attributes == null)
                session.Attributes = new Dictionary<string, object>();

            Type requestType = input.GetRequestType();
            if (input.GetRequestType() == typeof(LaunchRequest))
            {
                string speech = "Welcome! Say new game to start";
                Reprompt rp = new Reprompt("Say new game to start");
                return ResponseBuilder.Ask(speech, rp, session);
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
                    case "AMAZON.CancelIntent":
                    case "AMAZON.StopIntent":
                        return ResponseBuilder.Tell("Goodbye!");
                    case "AMAZON.HelpIntent":
                        {
                            Reprompt rp = new Reprompt("What's next?");
                            return ResponseBuilder.Ask("Here's some help. What's next?", rp, session);
                        }
                    case "TurnACOn":
                        {
                            bool isTurnedOn = true;

                            ACState state = new ACState();

                            if (isTurnedOn == true)
                            {
                                state.Power = Power.On;
                            }
                            else if (isTurnedOn == false)
                            {
                                state.Power = Power.Off;
                            }
                            else
                            {
                                return ResponseBuilder.Tell("Please use on or off.");
                            }
                            
                            _httpClient.PostAsync("/", state.AsJson());

                            return ResponseBuilder.Tell("Ok!");
                        }
                    case "NewGameIntent":
                        {
                            return ResponseBuilder.Tell("Template!");
                        }
                    default:
                        {
                            return ResponseBuilder.Tell("Default!");
                        }
                }
            }
            return ResponseBuilder.Tell("Goodbye!");
        }

        private StringContent AsJson(object toSerialize)
        {
            return new StringContent(JsonConvert.SerializeObject(toSerialize), Encoding.UTF8);
        }
    }
}