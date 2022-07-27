using System;
using System.Threading.Tasks;

namespace Simcode.PazCheck.CentralServer.BusinessLogic
{
    public class Licence
    {
        public Task<Object> GetLicenceData()
        {
            var subProgs = new object[]
            {
                new {name = "dgst"},
                new {name = "tst"},
                new {name = "sft"},
                new {name = "hlp"},
                new {name = "cfg"}
            };
            var licence = new
            {
                progs = subProgs,
                units = 4,
                customer = "Тестовая лицензия"
            };

            return Task.FromResult<Object>(licence);
        }
    }
}
