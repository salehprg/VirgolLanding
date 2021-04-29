using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Identity;
using virgollanding.Models;
using virgollanding.Helper;
using virgollanding.AuthurizationService;
using System.Net;
using Microsoft.AspNetCore.Http;

namespace virgollanding.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class LandingController : ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private readonly IHttpContextAccessor httpContextAccessor; 
        private FarazSmsApi smsApi;

        const int validSMSCodeMinute = 15;
        public LandingController(AppDbContext dbContext , IHttpContextAccessor _httpContextAccessor)
        {
            httpContextAccessor = _httpContextAccessor;
            smsApi = new FarazSmsApi();
            appDbContext = dbContext;
        }

        public async Task<IActionResult> SendSMSCode(string PhoneNumber)
        {
            try
            {
                List<VerificationCodeModel> verificationCode = appDbContext.VerificationCodes.Where(x => x.phoneNumber == PhoneNumber).ToList();
                List<VerificationCodeModel> verifLimit = new List<VerificationCodeModel>();

                foreach (var verifCode in verificationCode)
                {
                    if((DateTime.Now - verifCode.LastSend).TotalMinutes < validSMSCodeMinute)
                    {
                        verifLimit.Add(verifCode);
                    }
                }
                if(verifLimit.Count > 3)
                    return BadRequest("شما حداکثر تعداد درخواست خودرا ثبت نموده اید لطفا منتظر بمانید");

                VerificationCodeModel verification = new VerificationCodeModel();
                verification.IPAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();

                List<VerificationCodeModel> FormIPCheck = appDbContext.VerificationCodes.Where(x => x.IPAddress == verification.IPAddress && x.LastSend >= DateTime.Now.AddHours(-4)).OrderByDescending(x => x.LastSend).ToList();
                
                if(FormIPCheck.Count > 0)
                {
                    if((DateTime.Now - FormIPCheck.FirstOrDefault().LastSend).TotalHours <= 4 && FormIPCheck.Count >= 16)
                        return BadRequest("این IP حداکثر تعداد درخواست مجاز خود را ثبت کرده است");
                }


                string verifycode = RandomPassword.GeneratePassword(false , false , true , 6);

                string message = string.Format("کد تایید درخواست پنل  شما از سامانه ویرگول عبارت است از :\n {0}" , verifycode);

                message = message.Replace("\n" , Environment.NewLine);
                bool result = smsApi.SendSms(new string[]{PhoneNumber} , message);
                if(result)
                {
                   
                    verification.LastSend = DateTime.Now;
                    verification.phoneNumber = PhoneNumber;
                    verification.VerificationCode = verifycode;

                    appDbContext.VerificationCodes.Add(verification);
                    await appDbContext.SaveChangesAsync();
                    return Ok("پیامک با موفقیت ارسال شد");
                }

                return BadRequest("در ارسال پیامک مشکلی بوجود آمد");

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("لطفا مجددا تلاش کنید");
            }
        }

        [HttpPost]
        public async Task<IActionResult> PanelRequest([FromBody]ReqForm reqForm , string VerifyCode)
        {
            try
            {
                reqForm.IPAddress = httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString();
                List<ReqForm> FormIPCheck = appDbContext.ReqForms.Where(x => x.IPAddress == reqForm.IPAddress && x.TimeRequest >= DateTime.Now.AddHours(-4)).OrderBy(x => x.TimeRequest).ToList();
                
                if(FormIPCheck.Count > 0)
                {
                    if((DateTime.Now - FormIPCheck.FirstOrDefault().TimeRequest).TotalHours <= 4 && FormIPCheck.Count >= 16)
                        return BadRequest("این IP حداکثر تعداد درخواست مجاز خود را ثبت کرده است");
                }


                if(string.IsNullOrEmpty(reqForm.PhoneNumber))
                    return BadRequest("شماره تلفن نبايد خالي باشد");
                if(string.IsNullOrEmpty(reqForm.FirstName) || string.IsNullOrEmpty(reqForm.LastName))
                    return BadRequest("اطلاعات به درستي تكميل نشده است");

                ReqForm oldReqForm = appDbContext.ReqForms.Where(x => x.PhoneNumber == reqForm.PhoneNumber).FirstOrDefault();
                if(oldReqForm != null && oldReqForm.Status != ReqStatus.Closed)
                    return BadRequest("درخواست قبلی شما درحال پردازش میباشد");

                DateTime now = DateTime.Now;
                List<VerificationCodeModel> verificationCode = appDbContext.VerificationCodes.Where(x => x.phoneNumber == reqForm.PhoneNumber).OrderByDescending(x => x.LastSend).ToList();

                VerificationCodeModel result = new VerificationCodeModel();
                foreach (var verifCode in verificationCode)
                {
                    if((DateTime.Now - verifCode.LastSend).TotalMinutes < validSMSCodeMinute)
                        result = verifCode;
                }
                
                if(result != null && VerifyCode != result.VerificationCode)
                    return BadRequest("کد تایید وارد شده صحیح نمیباشد");

                
                reqForm.Status = ReqStatus.Open;
                await appDbContext.ReqForms.AddAsync(reqForm);
                await appDbContext.SaveChangesAsync();

                string message = string.Format("درخواست ثبت مدرسه جديد ثبت شد.\nاطلاعات درخواست : \n" + 
                                                "{0} {1}\n كد ملي : {2}  \n شماره تماس : {3}" , reqForm.FirstName , reqForm.LastName , (string.IsNullOrEmpty(reqForm.Mellicode) ? "ندارد" : reqForm.Mellicode) , reqForm.PhoneNumber);

                message = message.Replace("\n" , Environment.NewLine);
                string adminPhone = AppSettings.GetValueFromDatabase(appDbContext , "Admin_Phone");

                smsApi.SendSms(new string[]{adminPhone} , message);

                if(reqForm.email != null)
                {
                    MailHelper mailHelper = new MailHelper(AppSettings.GetValueFromDatabase(appDbContext , "SupportEmail"));
                    string emailMessage = string.Format(" {0} {1} درخواست شما براي پنل مدرسه با موفقیت ثبت شد  \n کد پیگیری : {2}" , reqForm.FirstName , reqForm.LastName , reqForm.Id);
                    emailMessage = emailMessage.Replace("\n" , Environment.NewLine);
                    mailHelper.Send(reqForm.email , "درخواست پنل مدرسه - سامانه ویرگول -" , emailMessage , MimeKit.Text.TextFormat.Text);
                }


                return Ok("درخواست شما با موفقيت ثبت شد");
            }
            catch (Exception ex)
            {
                if(reqForm.Id != 0)
                {
                    appDbContext.ReqForms.Remove(reqForm);
                    await appDbContext.SaveChangesAsync();
                }
                Console.WriteLine(ex.Message);
                return BadRequest("پردازش درخواست با مشکل روبرو شد لطفا بعدا تلاش نمایید");
                throw;
            }
        }
    
    
    }
}