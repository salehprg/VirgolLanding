using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

using System.Text.Json;
using System.Text.Json.Serialization;

using System.Net;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using virgollanding.Helper;
using System.Net.Http;
using System.Net.Http.Headers;
using virgollanding.FarazSms;
using virgollanding.Models.FarazSms;
using virgollanding.Models;

namespace virgollanding.Helper
{
    public class FarazSmsApi {

        public enum SocialType
        {
            Telegram,
            Viber
        }

        
        private string Username;
        private string Password;
        private string ApiKey;
        private string BaseUrl;
        private string FromNumber;

        public FarazSmsApi()
        {
            BaseUrl = AppSettings.FarazAPI_URL;
            Username = AppSettings.FarazAPI_Username;
            Password = AppSettings.FarazAPI_Password;
            ApiKey = AppSettings.FarazAPI_ApiKey;
            FromNumber = AppSettings.FarazAPI_SendNumber;
        }   

        bool SendData(string JsonData , string Method)
        {
            bool result = false;

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("AccessKey" , ApiKey);
                
                HttpResponseMessage responseMessage = client.PostAsync(BaseUrl + Method, 
                    new StringContent(JsonData, Encoding.UTF8, "application/json")).Result;
                string response = responseMessage.Content.ReadAsStringAsync().Result;

                result = responseMessage.IsSuccessStatusCode;
            }

            return result;
		
        }
        public bool SendVerifySms(string Number , string code)
        {
            VerifyCodeModel verifyCodeModel = new VerifyCodeModel();
            verifyCodeModel.verifyCode = code;

            SendPatternModel<VerifyCodeModel> patternModel = new SendPatternModel<VerifyCodeModel>();

            patternModel.pattern_code = "38x9cgatmn";
            patternModel.originator = FromNumber;
            patternModel.recipient = Number;
            patternModel.values = verifyCodeModel;

            string json = JsonConvert.SerializeObject(patternModel);

            return SendData(json , "/v1/messages/patterns/send");
            
        }
        public bool SendCustomerInfo(string Number , ReqForm reqForm)
        {
            CustomerInfoModel customerInfoModel = new CustomerInfoModel();
            customerInfoModel.firstName = reqForm.FirstName;
            customerInfoModel.lastName = reqForm.LastName;
            customerInfoModel.phoneNumber = reqForm.PhoneNumber;
            customerInfoModel.melliCode = reqForm.Mellicode;

            SendPatternModel<CustomerInfoModel> patternModel = new SendPatternModel<CustomerInfoModel>();

            patternModel.pattern_code = "vxhpazlnda";
            patternModel.originator = FromNumber;
            patternModel.recipient = Number;
            patternModel.values = customerInfoModel;

            string json = JsonConvert.SerializeObject(patternModel);
            
            return SendData(json , "/v1/messages/patterns/send");
            
        }

        public bool SendReqIdSms(string Number , string reqId)
        {
            ReqFormIdModel reqForm = new ReqFormIdModel();
            reqForm.reqId = reqId;

            SendPatternModel<ReqFormIdModel> patternModel = new SendPatternModel<ReqFormIdModel>();

            patternModel.pattern_code = "txul94s24e";
            patternModel.originator = FromNumber;
            patternModel.recipient = Number;
            patternModel.values = reqForm;

            string json = JsonConvert.SerializeObject(patternModel);
            
            return SendData(json , "/v1/messages/patterns/send");
            
        }

        public bool SendSms(string[] Numbers , string Message)
        {
            SendSmsModel smsModel = new SendSmsModel();

            smsModel.originator = FromNumber;
            smsModel.recipients = Numbers;
            smsModel.message = Message;

            string json = JsonConvert.SerializeObject(smsModel);

            // string postData = "op=send&uname=" + Username + "&pass=" + Password + "&message=" + Message +"&to="+json+"&from=+98" + FromNumber;

            return SendData(json , "/v1/messages");
        }
  
    }
}