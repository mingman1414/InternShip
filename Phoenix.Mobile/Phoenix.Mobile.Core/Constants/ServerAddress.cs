﻿namespace Phoenix.Mobile.Core.Constants
{
    public class ServerAddress
    {
#if DEBUG        
      //public const string ServerBaseUrl = "http://172.31.99.6:63199/api";
        public const string ServerBaseUrl = "http://192.168.79.253:63199/api";
        //public const string ServerBaseUrl = "http://192.168.1.9:63199/api";
        //public const string ServerBaseUrl = "http://172.18.131.226:63199/api";
#else

        //public const string ServerBaseUrl = "http://192.168.1.99:2345/api";
       
#endif
    }
}
