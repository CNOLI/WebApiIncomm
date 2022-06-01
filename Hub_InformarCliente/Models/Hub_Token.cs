using System;
using System.Collections.Generic;
using System.Text;

namespace Hub_InformarCliente
{
    public class Hub_Token
    {
        public class AccessToken
        {
            public string token { get; set; }
        }
        public class UserToken 
        {
            public string UserName { get; set; }
            public string Password { get; set; }
        }
    }
}
