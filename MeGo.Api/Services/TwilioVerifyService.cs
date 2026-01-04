using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Verify.V2.Service;

namespace MeGo.Api.Services
{
    public class TwilioVerifyService
    {
        private readonly string _accountSid;
        private readonly string _authToken;
        private readonly string _serviceSid;

        public TwilioVerifyService(IConfiguration config)
        {
            _accountSid = config["Twilio:AccountSid"];
            _authToken = config["Twilio:AuthToken"];
            _serviceSid = config["Twilio:ServiceSid"];
            TwilioClient.Init(_accountSid, _authToken);
        }

        // ✅ Send OTP
        public async Task SendOtpAsync(string phone)
        {
            await VerificationResource.CreateAsync(
                to: phone,                  // e.g. +923001234567
                channel: "sms",
                pathServiceSid: _serviceSid
            );
        }

        // ✅ Verify OTP
        public async Task<bool> VerifyOtpAsync(string phone, string code)
        {
            var verification = await VerificationCheckResource.CreateAsync(
                to: phone,
                code: code,
                pathServiceSid: _serviceSid
            );
            return verification.Status == "approved";
        }
    }
}
