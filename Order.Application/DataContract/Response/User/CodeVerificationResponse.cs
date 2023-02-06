using System;
using System.Collections.Generic;
using System.Text;

namespace Order.Application.DataContract.Response.User
{
    public class CodeVerificationResponse
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string CodeVerification { get; set; }
    }
}
