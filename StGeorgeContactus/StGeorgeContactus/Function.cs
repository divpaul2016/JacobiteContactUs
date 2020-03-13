using System;
using System.Net;
using Amazon.Lambda.Core;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Newtonsoft.Json;
using StGeorgeContactus.Model;
using JsonSerializer = Amazon.Lambda.Serialization.Json.JsonSerializer;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace StGeorgeContactus
{
    public class Function
    {
        private readonly IConfigurationManager _configurationManager;
        private readonly IAmazonSimpleNotificationService _snsService;

        public Function()
        {
            _snsService = new AmazonSimpleNotificationServiceClient();
            _configurationManager = new ConfigurationManager();
        }

        public string FunctionHandler(ContactUsRequest request, ILambdaContext context)
        {
            try
            {
                if (request == null)
                {
                    LambdaLogger.Log($"{nameof(request)} was null or empty.");
                    return "failed";
                }
                LambdaLogger.Log($"ContactUs Request received and request is : {request}");
                LambdaLogger.Log(JsonConvert.SerializeObject(request));
                if (!IsValidRequest(request))
                {
                    return "invalid request";
                }
                var message = RequestMapToSnsEvent(request);
                return !SendMessage(message) ? "failed" : "success";
            }
            catch (Exception e)
            {
                LambdaLogger.Log(e.Message);
                LambdaLogger.Log(e.StackTrace);
                throw;
            }
        }

        private bool IsValidRequest(ContactUsRequest request)
        {
            return request.Firstname.Length > 2 && request.Firstname.Length > 2 && request.PhoneNumber.Length == 10 &&
                   request.EmailAddress.Length > 3 && request.RequestFor.Length > 2;
        }

        private string RequestMapToSnsEvent(ContactUsRequest request)
        {
            string message = "";
            if (request.RequestFor.Length > 2 && request.RequestFor.ToLower() == "contactus")
            {
                message = ($"We have received contact us request from {request.Firstname} {request.Lastname}.The enquirer Phone Number is {request.PhoneNumber} and" +
                    $" Email Address is {request.EmailAddress}.");
            }

            else if (request.RequestFor.Length > 2 && request.RequestFor.ToLower() == "prayerrequest")
            {
                message = (
                    $"We have received special prayer request from {request.Firstname} {request.Lastname}.The requester Phone Number is {request.PhoneNumber} and" +
                    $" Email Address is {request.EmailAddress}.");
            }
            else if (request.RequestFor.Length > 2 && request.RequestFor.ToLower()=="perunnalshare")
            {
                message = (
                    $"  We have received perunnal share request from {request.Firstname} {request.Lastname}.The requester Phone Number is {request.PhoneNumber} and" +
                    $" Email Address is {request.EmailAddress} No of share is {request.NumberOfShares} for the {request.RequestFor}."
                );
            }
            else if (request.RequestFor.Length > 2 && request.RequestFor.ToLower() == "onlinegiving")
            {
                message = (
                    $"  We have received Online Giving request from {request.Firstname} {request.Lastname}.The requester Phone Number is {request.PhoneNumber} and" +
                    $" Email Address is {request.EmailAddress} and would like to Donate $:{request.DonationAmount}."
                );
            }
            else
            {
                message = (
                    $"  We have received qurbana/service request from {request.Firstname} {request.Lastname}.The requester Phone Number is {request.PhoneNumber} and" +
                    $" Email Address is {request.EmailAddress}. The request of the service is {request.RequestFor}."
                );
            }

            if (!string.IsNullOrEmpty(request.Message))
            {
                message += $"The message from the requester is {request.Message}";
            }

            return message;
        }

        private bool SendMessage(string message)
        {
            var topicArn = _configurationManager.GetSnsTopic();

            LambdaLogger.Log($"SNS topic is :{topicArn}");

            var client = new AmazonSimpleNotificationServiceClient();

            var request = new PublishRequest
            {
                Message = message,
                TopicArn = topicArn
            };

            try
            {
                LambdaLogger.Log($"Message is {message} and it is ready to send through SNS:");
                var response = client.PublishAsync(request);
                if (response.Result.HttpStatusCode != HttpStatusCode.OK)
                {
                    LambdaLogger.Log(
                        $"Unable to publish message. AWS responded with status code: {response.Result.HttpStatusCode} and metadata:{response.Result.ResponseMetadata}.");
                    return false;
                }

                LambdaLogger.Log("Message send through SNS:");
                return true;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log("Caught exception publishing request:");
                LambdaLogger.Log(ex.Message);
            }

            return false;
        }
    }
}