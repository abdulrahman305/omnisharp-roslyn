using OmniSharp.Mef;
using OmniSharp.Models;

namespace OmniSharp.DotNetTest.Models
{
    [OmniSharpEndpoint(OmniSharpEndpoints.V2.RunTest, typeof(RunTestRequest), typeof(RunTestResponse))]
    public class RunTestRequest : Request
    {
        public string MethodName { get; set; }
        public string TestFrameworkName { get; set; }
    }
}
