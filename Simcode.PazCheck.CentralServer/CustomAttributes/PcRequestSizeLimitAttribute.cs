using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ssz.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer
{
    public class PcRequestSizeLimitAttribute : RequestSizeLimitAttribute
    {
        public PcRequestSizeLimitAttribute() :
            base(GetBytes())
        {
        }

        private static long GetBytes()
        {
            IConfiguration configuration = Program.Host.Services.GetRequiredService<IConfiguration>();
            return ConfigurationHelper.GetValue<long>(configuration, @"MaxUploadFileSize", 1024_000_000);
        }
    }
}
