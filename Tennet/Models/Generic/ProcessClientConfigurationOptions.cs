
using Microsoft.Extensions.Options;

namespace Tennet.Models.Generic
{
    internal class ProcessClientConfigurationOptions : IOptions<GenerationLoadProcessConfiguration>
    {
        public ProcessClientConfigurationOptions(string url, int? timeout)
        {
            Value = new GenerationLoadProcessConfiguration { BaseUrl = url, TimeoutInSeconds = timeout };
        }

        public GenerationLoadProcessConfiguration Value { get; private set; }
    }
}