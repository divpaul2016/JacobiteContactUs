using System;
using System.Collections.Generic;
using System.Text;

namespace StGeorgeContactus.Model
{
    public class ContactUsRequest
    {
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string NumberOfShares { get; set; }
        public string RequestFor { get; set; }
        public string DonationAmount { get; set; }
        public string Message { get; set; }
    }
}
