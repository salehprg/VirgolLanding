using System;

namespace virgollanding.Models
{
    public class ReqForm
    {
        public int Id {get; set;}
        public string FirstName {get; set;}
        public string LastName {get; set;}
        public string Mellicode {get; set;}
        public string PhoneNumber {get; set;}
        public string Status {get; set;}
        public string IPAddress {get; set;}
        public string email {get; set;}
        public string desc {get; set;}
        public DateTime TimeRequest {get; set;}
    }
}