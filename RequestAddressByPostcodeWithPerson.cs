using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreWebServiceAPI_AddressTK.Model.Dtos
{
    public class RequestAddressByPostcodeWithPerson
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Postcode { get; set; }
        [Required]
        [DefaultValue(false)]
        public Boolean ClearCache { get; set; }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
