using System.Runtime.Serialization;

namespace DotNetToolsOutdated.JsonModels;

public partial class AppSettings
{
    [DataMember(Name = "httpClient")]
    public HttpClientSettings? HttpClient{ get; set; }

}


public partial class HttpClientSettings
{
    // the default values: (based on  https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler )
    //allowAutoRedirect: true,
    //checkCertificateRevocationList: true,
    //preAuthenticate: false,
    //useDefaultCredentials: false,
    //supportsProxy: true,
    //useCookies: true,
    //useProxy: true,

    [DataMember(Name = "allowAutoRedirect")]
    public bool AllowAutoRedirect { get; } = true;

    [DataMember(Name = "checkCertificateRevocationList")]
    public bool CheckCertificateRevocationList { get; } = true;

    [DataMember(Name = "preAuthenticate")]
    public bool PreAuthenticate { get; }

    [DataMember(Name = "useDefaultCredentials")]
    public bool UseDefaultCredentials { get; }
    
    [DataMember(Name = "useCookies")]
    public bool UseCookies { get; } = true;

    [DataMember(Name = "useProxy")]
    public bool UseProxy { get; } = true;

    [DataMember(Name = "useDefaultWebProxy")]
    public bool UseDefaultWebProxy { get; } = true;
}

