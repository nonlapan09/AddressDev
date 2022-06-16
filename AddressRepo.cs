using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using NetCoreWebServiceAPI_AddressTK.Model.Address.Person;
using NetCoreWebServiceAPI_AddressTK.Model.Dtos;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;

namespace NetCoreWebServiceAPI_AddressTK.Model.Address.Repository
{
    public class AddressRepo
    {
        private AddressDbContext _context;

        public AddressRepo(AddressDbContext context)
        {
            _context = context;
        }
        public IEnumerable<ResponsePostCode> GetAddressByPostcode(string sPostcode)
        {
            var items = _context.dbMasterAddressMain
                .Where(w => w.Postcode == sPostcode)
                .GroupBy(w => new {w.Province, w.District,w.SubDistrict})
                .Select(w => new {w.Key.Province,w.Key.District,w.Key.SubDistrict})
                .OrderBy(w=>w.District)
                .ToList();

            var Province = items.ToLookup(pair => pair.District, pair => pair.SubDistrict)
                                .ToDictionary(group => group.Key).ToList();

            var responseAddressMain = items.Select((s, index) => new ResponsePostCode
            {
                Seq = (long)index + 1,
               /* Hid = s.DopaHid.ToString() ?? "",
                Hno = (string)s.HNO ?? "",

                Village = (string)s.Village ?? "", //หมู่
                Lane = (string)s.Lane ?? "", //ซอบ
                Road = (string)s.Road ?? "", //ถนน
                Alley = (string)s.Alley ?? "",//ตรอก*/

                SubDistrict = (string)s.SubDistrict ?? "",
                District = (string)s.District ?? "",
                Province = (string)s.Province ?? "",
                /*Postcode = (string)s.Postcode ?? "",*/
            }).ToList().AsQueryable();

            return responseAddressMain;
        }

        public IEnumerable<ResponseAddressByPostcode> GetAddressByPostcodeWithHNO(string sHNO, string sPostcode)
        {
           
            var items = _context.dbMasterAddressMain
                .Where(w => w.HNO == sHNO)
                .Where(w => w.Postcode == sPostcode)
                .ToList();

            var responseAddressMain = items.Select((s, index) => new ResponseAddressByPostcode
            {
                Seq = (long)index + 1,
                Hid = (string)s.DopaHid.ToString() ?? "",
                Hno = (string)s.HNO ?? "",

                Village = (string)s.Village ?? "", //หมู่
                Lane = (string)s.Lane ?? "", //ซอบ
                Road = (string)s.Road ?? "", //ถนน
                Alley = (string)s.Alley ?? "",//ตรอก

                SubDistrict = (string)s.SubDistrict ?? "",
                District = (string)s.District ?? "",
                Province = (string)s.Province ?? "",
                Postcode = (string)s.Postcode ?? "",
            }).ToList().AsQueryable();

            return responseAddressMain;
        }

        public IEnumerable<ResponseProvince> GetMasterProvince(string sProvince)
        {
            var items = _context.dbMasterDistrictMain
                .Join(_context.dbMasterSubDistrictMain,
                        t_dis => t_dis.DistrictCode,
                        t_subDis => t_subDis.DistrictCode,
                        (t_dis, t_subdis) => new { t_dis, t_subdis }
                )
                .Join(_context.dbMasterProvinceMain,
                        t_prv => t_prv.t_dis.ProvinceCode,
                        t_prvname => t_prvname.ProvinceCode,
                        (t_prv, t_prvname) => new { t_prv, t_prvname }
                ).Where(w => w.t_prvname.Province == sProvince && w.t_prvname.Inactive == 0).ToList();


            var responseProvince = items.Select((s, index) => new ResponseProvince
            {
                Seq = (long)index + 1,
                ProvinceCode = (string)s.t_prvname.ProvinceCode.ToString(),
                Province = (string)s.t_prvname.Province ?? "",
                District = (string)s.t_prv.t_dis.District ?? "",
                SubDistrict = (string)s.t_prv.t_subdis.SubDistrict ?? ""

            }).ToList().AsQueryable();

            return responseProvince;
        }

        public IEnumerable<ResponseDistrict> GetMasterDistrict(string sDistrict)
        {
            var items = _context.dbMasterDistrictMain
                .Join(_context.dbMasterSubDistrictMain,
                            t1 => t1.DistrictCode,
                            t2 => t2.DistrictCode,
                            (t1, t2) => new { t1, t2 }
                     )
                .Join(_context.dbMasterProvinceMain,
                        t3 => t3.t1.ProvinceCode,
                        t4 => t4.ProvinceCode,
                        (t3, t4) => new { t3, t4 }
                      )
                .Where(w => w.t3.t1.District == sDistrict && w.t4.Inactive == 0)
                .ToList();

            var responseDistricts = items.Select((s, index) => new ResponseDistrict
            {
                Seq = (long)index + 1,
                Province = (string)s.t4.Province ?? "",
                DistrictCode = (string)s.t3.t1.DistrictCode ?? "",
                District = (string)s.t3.t1.District ?? "",
                SubDistrict = (string)s.t3.t2.SubDistrict ?? ""
            }).ToList().AsQueryable();

            return responseDistricts;
        }

        public IEnumerable<ResponseSubDistrict> GetMasterSubDistrict(string sSubDistrict)
        {
            var items = _context.dbMasterDistrictMain
                .Join(_context.dbMasterSubDistrictMain,
                            t1 => t1.DistrictCode,
                            t2 => t2.DistrictCode,
                            (t1, t2) => new { t1, t2 }
                     )
                .Join(_context.dbMasterProvinceMain,
                        t3 => t3.t1.ProvinceCode,
                        t4 => t4.ProvinceCode,
                        (t3, t4) => new { t3, t4 }
                      )
                .Where(w => w.t3.t2.SubDistrict == sSubDistrict && w.t4.Inactive == 0)
                .ToList();

            var responseSubDistrict = items.Select((s, index) => new ResponseSubDistrict
            {
                Seq = (long)index + 1,
                Province = (string)s.t4.Province ?? "",
                District = (string)s.t3.t1.District ?? "",
                SubDistrictCode = (string)s.t3.t2.SubDistrictCode ?? "",
                SubDistrict = (string)s.t3.t2.SubDistrict ?? ""
            }).ToList().AsQueryable();

            return responseSubDistrict;
        }

        public IEnumerable<ResponsePersonDataByFirstName> GetResponsePersonDataByFirstNames(string sFirstName)
        {
            string[] fStr = Regex.Replace(sFirstName, @"\s+", " ").Split(' ');
            string fname = sFirstName;
            string surname = "";
            if (fStr.Count() > 1)
            {
                fname = fStr[0];
                surname = fStr[1];
            }
            if (fStr.Count() > 2)
            {
                fname = fStr[1];
                surname = fStr[2];
            }


            var items = _context.dbPersonMainModel
                .Join(_context.dbPhoneNumberMainModel,
                        t1 => t1.PersonID,
                        t2 => t2.PersonID,
                        (t1, t2) => new { t1, t2 }
                    )
                .Join(_context.dbPersonHIDMainModel,
                      t3 => t3.t1.PersonID,
                      t4 => t4.PersonID,
                      (t3, t4) => new { t3, t4 }
                    )
                .Join(_context.dbMasterAddressMain,
                       t5 => t5.t4.Hid,
                       t6 => t6.DopaHid,
                       (t5, t6) => new { t5, t6 }
                    )
                .Where(w => w.t5.t3.t1.FirstNameTH == fname  && w.t5.t3.t1.SurnameTH == surname && w.t5.t3.t1.Inactive == 0).ToList();
            /*.Where(w => w.t5.t3.t1.FullNameTH.Contains(sFirstName +" "+ sLastName)).ToList();*/
            /* var upweight = _context.dbPersonHIDMainModel
                 .Single(e => e.Hid == 17204);*/
            var personID = items.Select((s, index)=>s.t5.t4.PersonID).ToList();

            var detailAddress = items.Select((s, index) => new ResponseAddressByPostcode
            {
                Seq = (long)index + 1,
                Hid = (string)s.t5.t4.Hid.ToString() ?? "",
                Hno = (string)s.t6.HNO ?? "",
                Village = (string)s.t6.Village ?? "", //หมู่
                Lane = (string)s.t6.Lane ?? "", //ซอบ
                Road = (string)s.t6.Road ?? "", //ถนน
                Alley = (string)s.t6.Alley ?? "",//ตรอก
                SubDistrict = (string)s.t6.SubDistrict ?? "",
                District = (string)s.t6.District ?? "",
                Province = (string)s.t6.Province ?? "",
                Postcode = (string)s.t6.Postcode ?? "",
            }).ToList().AsQueryable();

           var responsePersonDataByFirstName = items.Select((s, index) => new ResponsePersonDataByFirstName
            {
                Seq = (long)index + 1,
                PersonID = (long)s.t5.t3.t1.PersonID,
                TitleTH = (string)s.t5.t3.t1.TitleTH ?? "",
                FirstNameTH = (string)s.t5.t3.t1.FirstNameTH ?? "",
                SurnameTH = (string)s.t5.t3.t1.SurnameTH ?? "",
                FullNameTH = (string)s.t5.t3.t1.FullNameTH ?? "",
                TitleEN = (string)s.t5.t3.t1.TitleEN ?? "",
                FirstNameEN = (string)s.t5.t3.t1.FirstNameEN ?? "",
                SurnameEN = (string)s.t5.t3.t1.SurnameEN ?? "",
                FullNameEN = (string)s.t5.t3.t1.FullNameEN ?? "",
                PhoneNumber = (string)s.t5.t3.t2.PhoneNumber ?? "",
                DetailAddress = detailAddress.ToList(),
                CreatedDate = s.t5.t3.t1.CreatedDate
            }).ToList().AsQueryable().Take(1);
            return responsePersonDataByFirstName;
        }

        public IEnumerable<ResponsePersonDataByPhone> GetResponsePersonDataByPhones(string sPhone)
        {
            var items = _context.dbPersonMainModel
                .Join(_context.dbPhoneNumberMainModel,
                        t1 => t1.PersonID,
                        t2 => t2.PersonID,
                        (t1, t2) => new { t1, t2 }
                    )
                .Join(_context.dbPersonHIDMainModel,
                      t3 => t3.t1.PersonID,
                      t4 => t4.PersonID,
                      (t3, t4) => new { t3, t4 }
                    )
                .Join(_context.dbMasterAddressMain,
                       t5 => t5.t4.Hid,
                       t6 => t6.DopaHid,
                       (t5, t6) => new { t5, t6 }
                    )
                .Where(w => w.t5.t3.t2.PhoneNumber == sPhone).ToList();

            var detailAddress = items.Select((s, index) => new ResponseAddressByPostcode
            {
                Seq = (long)index + 1,
                Hid = (string)s.t5.t4.Hid.ToString() ?? "",
                Hno = (string)s.t6.HNO ?? "",
                Village = (string)s.t6.Village ?? "", //หมู่
                Lane = (string)s.t6.Lane ?? "", //ซอบ
                Road = (string)s.t6.Road ?? "", //ถนน
                Alley = (string)s.t6.Alley ?? "",//ตรอก
                SubDistrict = (string)s.t6.SubDistrict ?? "",
                District = (string)s.t6.District ?? "",
                Province = (string)s.t6.Province ?? "",
                Postcode = (string)s.t6.Postcode ?? "",
            }).ToList().AsQueryable();

            var responsePersonDataByPhone = items.Select((s, index) => new ResponsePersonDataByPhone
            {
                Seq = (long)index + 1,
                PersonID = (long)s.t5.t3.t1.PersonID,
                TitleTH = (string)s.t5.t3.t1.TitleTH ?? "",
                FirstNameTH = (string)s.t5.t3.t1.FirstNameTH ?? "",
                SurnameTH = (string)s.t5.t3.t1.SurnameTH ?? "",
                FullNameTH = (string)s.t5.t3.t1.FullNameTH ?? "",

                TitleEN = (string)s.t5.t3.t1.TitleEN ?? "",
                FirstNameEN = (string)s.t5.t3.t1.FirstNameEN ?? "",
                SurnameEN = (string)s.t5.t3.t1.SurnameEN ?? "",
                FullNameEN = (string)s.t5.t3.t1.FullNameEN ?? "",
                PhoneID = (long)s.t5.t3.t2.PhoneID,
                PhoneNumber = (string)s.t5.t3.t2.PhoneNumber ?? "",
                DetailAddress = detailAddress.ToList(),
                CreatedDate = s.t5.t3.t2.CreatedDate
            }).ToList().AsQueryable().Take(1);
            return responsePersonDataByPhone;


        }

        public IEnumerable<ResponseRoad> GetMasterRoad(string sRoad)
        {
            var items = _context.dbRoadMainModel
                .Join(_context.dbRCodeMainModel,
                            t1 => t1.RCode.ToString(),
                            t2 => t2.RCode,
                            (t1, t2) => new { t1,t2}
                     )
                .Where(w => w.t1.Road == sRoad)
                /*.GroupBy(q => q.t2.SubDistrict,
                         q => q.t2.SubDistrictCode,
                         (key,q) => new { }
                    )*/
                .ToList();

            var responseRoad = items.Select((s, index) => new ResponseRoad
            {
                Seq = (long)index + 1,
                Road = (string)s.t1.Road ?? "",
                RCode = (string)s.t2.RCode ?? "",
                Province = (string)s.t2.Province ?? "",
                District = (string)s.t2.District ?? "",
                SubDistrict = (string)s.t2.SubDistrict ?? ""
            }).ToList().AsQueryable();

            return responseRoad;
        }
        public IEnumerable<ResponseLane> GetMasterLane(string sLane)
        {
            var items = _context.dbLaneMainModel
              .Join(_context.dbRCodeMainModel,
                    t1 => t1.Rcode.ToString(),
                    t2 => t2.RCode,
                    (t1, t2) => new { t1, t2 })
              //.Where(s => s.t1.Lane == sLane)
              .Where(s => s.t1.Lane.Contains(sLane))
              .OrderBy(s => s.t1.Lane)
              .ToList();

            var responseLaneDetails = items.Select((s, index) => new ResponseLane
            {
                Seq = (long)index + 1,
                LaneCode = (long)s.t1.LaneCode,
                Lane = (string)s.t1.Lane ?? "",
                SubDistrictCode = (string)s.t2.SubDistrictCode ?? "",
                SubDistrict = (string)s.t2.SubDistrict ?? "",
                RCode = (string)s.t2.RCode ?? ""

            }).ToList().AsQueryable();

            return responseLaneDetails;
        }

        public IEnumerable<ResponseAlley> GetMasterAlley(string sAlley)
        {
            var items = _context.dbAlleyMainModel
               .Join(_context.dbRCodeMainModel,
                     t1 => t1.RCode.ToString(),
                     t2 => t2.RCode,
                     (t1, t2) => new { t1, t2 })
               .Where(s => s.t1.Alley.Contains(sAlley)).OrderBy(w=>w.t1.Alley)
               /*.Where(s => s.t1.Alley == sAlley)*/
               .ToList();


            var responseAddress = items.Select((s, index) => new ResponseAlley
            {
                Seq = (long)index + 1,
                AlleyCode = (string)s.t1.AlleyCode.ToString() ?? "",
                Alley = (string)s.t1.Alley.ToString() ?? "",
                SubDistrictCode = (string)s.t2.SubDistrictCode ?? "",
                SubDistrict = (string)s.t2.SubDistrict ?? "",
                RCode = (string)s.t2.RCode ?? "",


            }).ToList().AsQueryable();

            return responseAddress;
        }
       
        
        public IEnumerable<ResponsePersonID> CheckPersonDataByFirstName(string sfirstName,string sSurName) 
        {
            try 
            {
                string str = Regex.Replace(sfirstName, @"\s+", "");
                if (Regex.IsMatch(str, @"^[a-zA-Z]+$")) 
                {
                    var itemEng = _context.dbPersonMainModel
                    .Where(w => w.FirstNameEN == sfirstName && w.SurnameEN == sSurName).Take(1).ToList();
                     var responsePersonEN = itemEng.Select((s, index) => new ResponsePersonID 
                    {
                        PersonID = s.PersonID,
                        TitleTH = s.TitleTH,
                        FirstNameTH = s.FirstNameTH,
                        SurnameTH = s.SurnameTH,
                        TitleEN = s.TitleEN,
                        FirstNameEN = s.FirstNameEN,
                        SurnameEN = s.SurnameEN
                    }).ToList().AsQueryable();
                    return responsePersonEN;
                }
                var itemTH = _context.dbPersonMainModel
                     .Where(w => w.FirstNameTH == sfirstName && w.SurnameTH == sSurName).Take(1).ToList();
                 var responsePersonTH = itemTH.Select((s, index) => new ResponsePersonID
                {
                    PersonID = s.PersonID,
                    TitleTH = s.TitleTH,
                    FirstNameTH = s.FirstNameTH,
                    SurnameTH = s.SurnameTH,
                    TitleEN = s.TitleEN,
                    FirstNameEN = s.FirstNameEN,
                    SurnameEN = s.SurnameEN
                }).ToList().AsQueryable();
                return responsePersonTH;
            }
            catch(Exception ex) 
            { 
                return null; 
            }
        }

        public ResponseAddPersonData AddPersonData(AddPersonDataModel JsonRequest)
        {
            try
            {

                long pid,phoneid;
                var connection = _context.Database.GetDbConnection();
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select (TBL_PERSON_SEQ.NEXTVAL) as Seq from dual";

                    var obj = cmd.ExecuteScalar();
                    pid = Convert.ToInt64(obj.ToString());

                    //Select Seq PhoneID
                    cmd.CommandText = "select (TBL_PHONE_NUMBER_SEQ.NEXTVAL) as Seq from dual";

                    var obj2 = cmd.ExecuteScalar();
                    phoneid = Convert.ToInt64(obj2.ToString());
                }
                
                var person = new PersonMainModel
                {
                    PersonID = pid,
                    IDCard = (string)JsonRequest.IDCard.ToString().Trim() ?? "",
                    TitleTH = (string)JsonRequest.TitleTH.ToString().Trim() ?? "",
                    FirstNameTH = (string)JsonRequest.FirstnameTH.ToString().Trim() ?? "",
                    SurnameTH = (string)JsonRequest.SurnameTH.Trim() ?? "",
                    FullNameTH = Regex.Replace(JsonRequest.TitleTH.ToString().Trim() + " " + (string)JsonRequest.FirstnameTH.ToString().Trim() + " " + (string)JsonRequest.SurnameTH.ToString().Trim(), @"\s+", " "),
                    TitleEN = (string)JsonRequest.TitleEN.ToString().Trim() ?? "",
                    FirstNameEN = (string)JsonRequest.FirstnameEN.ToString().Trim() ?? "",
                    SurnameEN = (string)JsonRequest.SurnameEN.ToString().Trim() ?? "",
                    FullNameEN = Regex.Replace(JsonRequest.TitleEN.ToString().Trim() + " " + (string)JsonRequest.FirstnameEN.ToString().Trim() + " " + (string)JsonRequest.SurnameEN.ToString().Trim(), @"\s+", " "),
                    SystemType = (int)JsonRequest.SystemType,
                    Inactive = (int)JsonRequest.Inactive,
                    CreatedDate = DateTime.Now
                    /*LastModifiedDate = JsonRequest.LastModifiedDate,
                    TimeStampDate = JsonRequest.TimeStampDate,*/
                };
                var phone = new PhoneNumberMainModel
                {
                    PhoneID = (long)phoneid,
                    PhoneNumber = (string)JsonRequest.PhoneNumber.ToString().Trim() ?? "",
                    PersonID = (long)pid,
                    Inactive = (long)0,
                    CreatedDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    TimeStampDate = DateTime.Now
                };
                var logBiding = new LogBindingMainModel
                {
                    PersonID = (long)pid,
                    PhoneID = (long)phoneid,
                    Actions = "C",
                    UserID = (long)JsonRequest.UserID,
                    TitleTHNew = (string)JsonRequest.TitleTH,
                    FirstnameTHNew = (string)JsonRequest.FirstnameTH,
                    SurnameTHNew = (string)JsonRequest.SurnameTH,
                    FullnameTHNew = (string)JsonRequest.TitleTH + " " + JsonRequest.FirstnameTH + " " + JsonRequest.SurnameTH,
                    TitleENNew = (string)JsonRequest.TitleEN,
                    FirstnameENNew = (string)JsonRequest.FirstnameEN,
                    SurnameENNew = (string)JsonRequest.SurnameEN,
                    FullnameENNew = (string)JsonRequest.TitleEN + " " + JsonRequest.FirstnameEN + " " + JsonRequest.SurnameEN,
                    CreatedDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    TimestampDate = DateTime.Now,
                };
                //Insert Person
                _context.dbPersonMainModel.Add(person);
                if (_context.SaveChanges() > 0)
                {
                    //Insert Phone Number
                    _context.dbPhoneNumberMainModel.Add(phone);
                    if (_context.SaveChanges() > 0)
                    {
                        _context.dbLogBindingMainModel.Add(logBiding);
                        if (_context.SaveChanges() > 0)
                        {
                            var responsePersonData = new ResponseAddPersonData();
                            responsePersonData.PersonID = pid;
                            responsePersonData.TitleTH = JsonRequest.TitleTH.ToString().Trim();
                            responsePersonData.FirstNameTH = JsonRequest.FirstnameTH.ToString().Trim();
                            responsePersonData.SurnameTH = JsonRequest.SurnameTH.ToString().Trim();
                            responsePersonData.TitleEN = JsonRequest.TitleEN.ToString().Trim();
                            responsePersonData.FirstNameEN = JsonRequest.FirstnameEN.ToString().Trim();
                            responsePersonData.SurnameEN = JsonRequest.SurnameEN.ToString().Trim();
                            responsePersonData.PhoneID = phoneid;
                            responsePersonData.PhoneNumber = JsonRequest.PhoneNumber.ToString().Trim();
                            return responsePersonData;
                        }

                    }
                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public ResponseAddPersonData EditPersonData(RequestEditPersonData JsonRequest) 
        {
            try {
                var item = _context.dbPersonMainModel
                .Single(e => e.PersonID == JsonRequest.PersonID);
                if (JsonRequest.FirstnameTHNew != "")
                {
                    item.TitleTH = JsonRequest.TitleTHNew.Trim();
                    item.FirstNameTH = JsonRequest.FirstnameTHNew.Trim();
                    item.SurnameTH = JsonRequest.SurnameTHNew.Trim();
                    item.FullNameTH = JsonRequest.TitleTHNew.Trim() + " " + JsonRequest.FirstnameTHNew.Trim() + " " + JsonRequest.SurnameTHNew.Trim();
                    item.LastModifiedDate = DateTime.Now;
                }
                if (JsonRequest.FirstnameENNew != "")
                {
                    item.TitleEN = JsonRequest.TitleENNew.Trim();
                    item.FirstNameEN = JsonRequest.FirstnameENNew.Trim();
                    item.SurnameEN = JsonRequest.SurnameENNew.Trim();
                    item.FullNameEN = JsonRequest.TitleENNew.Trim() + " " + JsonRequest.FirstnameENNew.Trim() + " " + JsonRequest.SurnameENNew.Trim();
                    item.LastModifiedDate = DateTime.Now;
                }
                if (_context.SaveChanges() > 0) 
                {
                    var logbinding = new LogBindingMainModel
                    {
                        PersonID = (long)JsonRequest.PersonID,
                        PhoneID = (long)JsonRequest.PhoneID,
                        Actions = (string)"U",
                        UserID = (long)JsonRequest.UserID,
                        TitleTHNew = (string)JsonRequest.TitleTHNew,
                        FirstnameTHNew = (string)JsonRequest.FirstnameTHNew,
                        SurnameTHNew = (string)JsonRequest.SurnameTHNew,
                        FullnameTHNew = (string)JsonRequest.TitleTHNew + " " + JsonRequest.FirstnameTHNew + " " + JsonRequest.SurnameTHNew,
                        TitleENNew = (string)JsonRequest.TitleENNew,
                        FirstnameENNew = (string)JsonRequest.FirstnameENNew,
                        SurnameENNew = (string)JsonRequest.SurnameENNew,
                        FullnameENNew = (string)JsonRequest.TitleENNew + " " + JsonRequest.FirstnameENNew + " " + JsonRequest.SurnameENNew,
                        TitleTHOld = (string)JsonRequest.TitleTHOld,
                        FirstnameTHOld = (string)JsonRequest.FirstnameTHOld,
                        SurnameTHOld = (string)JsonRequest.SurnameTHOld,
                        FullnameTHOld = (string)JsonRequest.TitleTHOld + " " + JsonRequest.FirstnameTHOld + " " + JsonRequest.SurnameTHOld,
                        TitleENOld = (string)JsonRequest.TitleENOld,
                        FirstnameENOld = (string)JsonRequest.FirstnameENOld,
                        SurnameENOld = (string)JsonRequest.SurnameENOld,
                        FullnameENOld = (string)JsonRequest.TitleENOld + " " + JsonRequest.FirstnameENOld + " " + JsonRequest.SurnameENOld,
                        CreatedDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        TimestampDate = DateTime.Now
                    };
                    _context.dbLogBindingMainModel.Add(logbinding);
                    if (_context.SaveChanges() > 0) 
                    {
                        var responsePersonData = new ResponseAddPersonData();
                        responsePersonData.PersonID = (long)JsonRequest.PersonID;
                        responsePersonData.TitleTH = (string)JsonRequest.TitleTHNew.ToString().Trim();
                        responsePersonData.FirstNameTH = (string)JsonRequest.FirstnameTHNew.ToString().Trim();
                        responsePersonData.SurnameTH = (string)JsonRequest.SurnameTHNew.ToString().Trim();
                        responsePersonData.TitleEN = (string)JsonRequest.TitleENNew.ToString().Trim();
                        responsePersonData.FirstNameEN = (string)JsonRequest.FirstnameENNew.ToString().Trim();
                        responsePersonData.SurnameEN = (string)JsonRequest.SurnameENNew.ToString().Trim();
                        responsePersonData.PhoneID = (long)JsonRequest.PhoneID;
                        responsePersonData.PhoneNumber = (string)JsonRequest.PhoneNumber.ToString().Trim();
                        return responsePersonData;
                    }
                }
                return null;
            } 
            catch (Exception ex) 
            { 
                return null; 
            }
            
        }

        public bool AddPersonHID(RequestAddPersonHID JsonRequest) 
        {
            try 
            {
                var personHID = new PersonHIDMainModel
                {
                    /*TblID = (long)id,*/
                    PersonID = (long)JsonRequest.PersonID,
                    Hid = (long)JsonRequest.Hid,
                    Inactive = (long)0,
                    Weight = (long)0,
                    CreatedDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    TimeStampDate = DateTime.Now  
                };
                _context.dbPersonHIDMainModel.Add(personHID);
                if (_context.SaveChanges() > 0) 
                {
                    var logbinding = new LogBindingMainModel
                    {
                        PersonID = (long)JsonRequest.PersonID,
                        PhoneID = (long)JsonRequest.PhoneID,
                        Actions = (string)"C",
                        UserID = (long)JsonRequest.UserID,
                        CreatedDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        TimestampDate = DateTime.Now
                    };
                    _context.dbLogBindingMainModel.Add(logbinding);
                    if (_context.SaveChanges() > 0)
                    {
                        return true;
                    }
                }
                
                return false; 
            }
            catch (Exception ex) 
            { 
                return false; 
            }
        }

        public ResponsePhoneID AddPhoneNumber(long sUserID,long sPersonID,string sPhonenumber)
        {
            try
            {
                long PhoneID;
                var connection = _context.Database.GetDbConnection();
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "select (TBL_PHONE_NUMBER_SEQ.NEXTVAL) as Seq from dual";
                    var obj2 = cmd.ExecuteScalar();
                    PhoneID = Convert.ToInt64(obj2.ToString());
                }
                var phoneData = new PhoneNumberMainModel
                {
                    PhoneID = (long)PhoneID,
                    PhoneNumber = (string)sPhonenumber.Trim(),
                    PersonID = (long)sPersonID,
                    Inactive = (long)0,
                    CreatedDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    TimeStampDate = DateTime.Now
                };
                _context.dbPhoneNumberMainModel.Add(phoneData);
                if (_context.SaveChanges() > 0) 
                {
                   bool log = AddLogbindingPhone(sUserID, sPersonID, PhoneID);
                    var ResPhone = new ResponsePhoneID
                    {
                        PhoneID = (long)PhoneID,
                        PersonID = (long)sPersonID,
                        PhoneNumber = (string)sPhonenumber.Trim(),
                    };
                    return ResPhone;
                }
                return null;
            }
            catch (Exception ex) 
            {
                return null;
            }
        }

        public bool AddLogbindingPhone (long sUserID,long sPersonID,long sPhoneID)
        {
            try 
            {
                var logphone = new LogBindingMainModel
                {
                    UserID = sUserID,
                    PersonID = sPersonID,
                    PhoneID = sPhoneID,
                    Actions = "C",
                    CreatedDate = DateTime.Now,
                    LastModifyDate = DateTime.Now,
                    TimestampDate = DateTime.Now
                };
                _context.dbLogBindingMainModel.AddAsync(logphone);
                _context.SaveChanges();
                return _context.SaveChanges() > 0 ? true : false;

            } 
            catch (Exception ex) 
            {
                return false;
            }
        }

        public ResponseUpdateStatisticsHID UpdateStatisticsHID(long sPersonID,long sHid) 
        {
            try 
            {
                var item = _context.dbPersonHIDMainModel
                    .Single(e => e.PersonID == sPersonID && e.Hid == sHid);
                    item.Weight = item.Weight +1;
                if (_context.SaveChanges() > 0) 
                {
                    var responUpHid = new ResponseUpdateStatisticsHID
                    {
                        PersonID = (long)item.PersonID,
                        Hid = (long)item.Hid,
                        Total = (long)item.Weight
                    };

                    return responUpHid;
                }
                return null;
            } 
            catch (Exception ex) 
            { 
                return null; 
            }
        }
        public List<string> GetAllPhoneNumber() 
        {
            var results = _context.dbPhoneNumberMainModel
                        .Where(w => w.PhoneNumber != null && w.Inactive == 0)
                        .GroupBy(p => new { p.PhoneNumber })
                        .Select(g =>g.Key.PhoneNumber).Take(10).ToList();
                        /*.Distinct().Select(w => w.PhoneNumber).Take(10).ToList();*/
            return results;
        }
        public List<string> GetAllPostcode() 
        {
            var results = _context.dbMasterAddressMain
                        .GroupBy(p => new { p.Postcode }).OrderBy(w => w.Key.Postcode) //.OrderByDescending(w => w.Key.Postcode)
                        .Select(g => g.Key.Postcode).Take(10)
                        .ToList();
            return results;
        }


        public IEnumerable<ResponseAddressByPostcode> GetAddressByPostcodeWithPerson(string sFirstname, string sSurname, string sPostcode)
        {
            try
            {
                var items = _context.dbMasterAddressMain
                    .Join(_context.dbPersonHIDMainModel,
                     t1 => t1.DopaHid,
                     t2 => t2.Hid,
                     (t1, t2) => new { t1, t2 })
                     .Join(_context.dbPersonMainModel,
                     t3 => t3.t2.PersonID,
                     t4 => t4.PersonID,
                     (t3, t4) => new { t3, t4 })
               .Where(w => w.t4.FirstNameTH == sFirstname || w.t4.FirstNameEN == sFirstname)
               .Where(w => w.t4.SurnameTH == sSurname || w.t4.SurnameEN == sSurname)
               .Where(w => w.t3.t1.Postcode == sPostcode)
               .ToList();

                var responseAddressMain = items.Select((s, index) => new ResponseAddressByPostcode
                {
                    Seq = (long)index + 1,
                    Hid = (string)s.t3.t1.DopaHid.ToString() ?? "",
                    Hno = (string)s.t3.t1.HNO ?? "",

                    Village = (string)s.t3.t1.Village ?? "", //หมู่
                    Lane = (string)s.t3.t1.Lane ?? "", //ซอบ
                    Road = (string)s.t3.t1.Road ?? "", //ถนน
                    Alley = (string)s.t3.t1.Alley ?? "",//ตรอก

                    SubDistrict = (string)s.t3.t1.SubDistrict ?? "",
                    District = (string)s.t3.t1.District ?? "",
                    Province = (string)s.t3.t1.Province ?? "",
                    Postcode = (string)s.t3.t1.Postcode ?? ""

                }).ToList().AsQueryable();

                return responseAddressMain;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
            }
            return null;
        }

        public IEnumerable<ResponseAddressByPostcode> GetAddressByPostcodeHnoAndVillage(string sPostcode, string sHno, string sVillage)
        {
            try
            {
                var items = _context.dbMasterAddressMain
                    .Join(_context.dbPersonHIDMainModel,
                     t1 => t1.DopaHid,
                     t2 => t2.Hid,
                     (t1, t2) => new { t1, t2 })
                     .Join(_context.dbPersonMainModel,
                     t3 => t3.t2.PersonID,
                     t4 => t4.PersonID,
                     (t3, t4) => new { t3, t4 })
               .Where(w => w.t3.t1.Postcode == sPostcode)
               .Where(w => w.t3.t1.HNO == sHno)
               .Where(w => w.t3.t1.Village == sVillage)
               .ToList();

                var responseAddressMain = items.Select((s, index) => new ResponseAddressByPostcode
                {
                    Seq = (long)index + 1,
                    Hid = (string)s.t3.t1.DopaHid.ToString() ?? "",
                    Hno = (string)s.t3.t1.HNO ?? "",
                    Village = (string)s.t3.t1.Village ?? "", //หมู่
                    Lane = (string)s.t3.t1.Lane ?? "", //ซอย
                    Road = (string)s.t3.t1.Road ?? "", //ถนน
                    Alley = (string)s.t3.t1.Alley ?? "",//ตรอก
                    SubDistrict = (string)s.t3.t1.SubDistrict ?? "",
                    District = (string)s.t3.t1.District ?? "",
                    Province = (string)s.t3.t1.Province ?? "",
                    Postcode = (string)s.t3.t1.Postcode ?? ""

                }).ToList().AsQueryable();

                return responseAddressMain;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Error");
            }
            return null;
        }

    }
}
