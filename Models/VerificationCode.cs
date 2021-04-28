using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;


namespace virgollanding.Models
{
    public class VerificationCodeModel {

        public int Id {get; set;}
        public int ReqFormId {get; set;}
        public string VerificationCode {get; set;}
        public string melliCode {get; set;}
        public DateTime LastSend {get; set;}
    }
}