using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreWebServiceAPI_AddressTK.Model.Dtos
{
    public class RequestAddressByPostcodeHnoAndVillage
    {
        [Required]
        public string Postcode { get; set; }
        [Required]
        public string Hno { get; set; }
        [Required]
        public string Village { get; set; }
        [Required]
        [DefaultValue(false)]
        public Boolean ClearCache { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
