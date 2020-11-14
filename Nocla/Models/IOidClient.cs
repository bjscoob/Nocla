using Nocla.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nocla.Models
{
    //IODClient CLASS: Interface for Dependency Injection. These methods can be called from either the Android or Apple base platform
    public interface IOidClient
    {
        string LoginAsyncResult { get; }
        string getDeviceID { get; }
        string PNS { get; set; }
        void registerUsername( string u);
    }
}
