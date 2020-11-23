

using System.Threading.Tasks;

namespace Nocla
{
    //IODClient CLASS: Interface for Dependency Injection. These methods can be called from either the Android or Apple base platform
    public interface IOidClient
    {
        string getDeviceID { get; }
        string PNS { get; set; }
        Task<bool> registerUsername( string u);

    }


}
