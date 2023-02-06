using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Application.DataContract.Request.User
{
    public class CodeVerificationRequest
    {
        public string Email { get; set; }
        public string CodeVerification { get; set; }
    }
}
