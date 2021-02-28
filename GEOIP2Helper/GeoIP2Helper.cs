using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Net;
using System.IO;
using MaxMind.GeoIP2;
using MaxMind.Db;
using MaxMind.GeoIP2.Model;
using MaxMind.GeoIP2.Responses;
using System.Collections.Concurrent;

namespace Helpers
{
    public class GeoIPCachedData
    {
        //public List<GeoIP2Information> GeoIPCachedList { get; set; }
        public Dictionary<string, GeoIP2Information> GeoIPCachedList { get; set; }
        private static readonly object _cachedLock = new object();

        public GeoIPCachedData()
        {
            //GeoIPCachedList = new List<GeoIP2Information>();
            GeoIPCachedList = new Dictionary<string, GeoIP2Information>();
        }

        public GeoIP2Information Get(string ipAddress)
        {
            //int idx = GeoIPCachedList.IndexOf(new GeoIP2Information() { IPAddress = ipAddress });
            //if (idx >= 0)
            //    return GeoIPCachedList[idx];

            //return null;

            GeoIP2Information geoIP2Information = null;
            GeoIPCachedList.TryGetValue(ipAddress, out geoIP2Information);

            return geoIP2Information;
        }

        //public void Set(GeoIP2Information cache)
        //{
        //    lock (_cachedLock)
        //    {
        //        int idx = GeoIPCachedList.IndexOf(cache);
        //        if (idx < 0)
        //            this.GeoIPCachedList.Add(cache);    
        //    }
        //}

        public void Set(string ipAddr, GeoIP2Information cache)
        {
            lock (_cachedLock)
            {
                if (!GeoIPCachedList.ContainsKey(ipAddr))
                {
                    GeoIPCachedList[ipAddr] = cache;
                }
            }
        }
    }

    public class GeoIP2Information
    {
        public string IPAddress { get; set; }

        public string CountryCode { get; set; }
        public string CountryName { get; set; }

        public string CityName { get; set; }
        public string PostalCode { get; set; }

        /*Similar to ISP*/
        public long? AutonomousSystemNumber { get; set; }
        public string AutonomousSystemOrganization { get; set; }
        //

        public string Isp { get; set; }
        public string Organization { get; set; }
        public string Domain { get; internal set; }
        public string RegionCode { get; internal set; }
        public string RegionName { get; internal set; }
        public double? Longitude { get; internal set; }
        public double? Latitude { get; internal set; }

        public override int GetHashCode()
        {
            return this.IPAddress.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != this.GetType())
            {
                return false;
            }

            return (obj as GeoIP2Information).IPAddress.Equals(this.IPAddress);
        }
    }

    public static class GeoIP2Helper
    {
        private static GeoIPCachedData _cached = new GeoIPCachedData();

        // This is default in your APP\GeoIP2
        private static string _asnPath  = String.Format(@"{0}GeoIP2\GeoLite2-ASN.mmdb", AppDomain.CurrentDomain.BaseDirectory);
        private static string _cityPath = String.Format(@"{0}GeoIP2\GeoLite2-City.mmdb", AppDomain.CurrentDomain.BaseDirectory);

        // Set up new path
        public static void SetNewMMDBPath(string citymmdb, string asnmmdb)
        {
            _cityPath = citymmdb;
            _asnPath = asnmmdb;
        }

        private static GeoIP2Information lookup(this string ipAddress)
        {
            GeoIP2Information info = new GeoIP2Information() { IPAddress = ipAddress };

            try
            {
                /*AutonomousSystemOrganization*/
                using (var reader = new DatabaseReader(_asnPath))
                {
                    var asn = reader.Asn(ipAddress);
                
                    info.AutonomousSystemNumber = asn.AutonomousSystemNumber;
                    info.AutonomousSystemOrganization = asn.AutonomousSystemOrganization;
                }

                /*City*/
                using (var reader = new DatabaseReader(_cityPath))
                {
                    // Replace "City" with the appropriate method for your database, e.g.,
                    // "Country".
                    var city = reader.City(ipAddress);

                    info.CountryCode = city.Country.IsoCode; // 'US'
                    info.CountryName = city.Country.Name;    // 'United States'
                    info.PostalCode = city.Postal.Code;      // '55455'
                    info.CityName = city.City.Name;          // 'Minneapolis'

                    info.Latitude = city.Location.Latitude;
                    info.Longitude = city.Location.Longitude;

                    // free mmdb, no more using.
                    //var traits = city.Traits;
                    //if (traits != null)
                    //{
                    //    /*1239*/
                    //    info.AutonomousSystemNumber = traits.AutonomousSystemNumber;
                    //    /*Linkem IR WiMax Network*/
                    //    info.AutonomousSystemOrganization = traits.AutonomousSystemOrganization;
                    //    /*Linkem spa*/
                    //    info.Isp = traits.Isp;
                    //    /*Linkem IR WiMax Network*/
                    //    info.Organization = traits.Organization;
                    //    /*example.com*/
                    //    info.Domain = traits.Domain;
                    //}

                    var subdivisions = city.Subdivisions;
                    if (subdivisions != null && subdivisions.Count > 0)
                    {
                        var sub = subdivisions[0];

                        info.RegionCode = sub.IsoCode;
                        info.RegionName = sub.Name;
                    }
                }
            }
            catch
            {
                /*
                 * Skip IPAddress not found in DB.
                 * Make sure no more resolve IPAddress than one.
                 */
            }

            return info;
        }
        
        public static GeoIP2Information GetGeoIP2(this string ipAddress)
        {
            /*Check if exists cache.*/
            GeoIP2Information geoip2cached = _cached.Get(ipAddress);

            bool hasCached = geoip2cached != null;
            if (hasCached)
            {
                return geoip2cached;
            }

            /*Lookup*/
            geoip2cached = ipAddress.lookup();

            /*Cached*/
            _cached.Set(ipAddress, geoip2cached);

            return geoip2cached;
        }
    }
}

//{
//  "city":  {
//      "confidence": 25,
//      "geoname_id": 54321,
//      "names":  {
//          "de":     "Los Angeles",
//          "en":     "Los Angeles",
//          "es":     "Los Ángeles",
//          "fr":     "Los Angeles",
//          "ja":     "ロサンゼルス市",
//          "pt-BR":  "Los Angeles",
//          "ru":     "Лос-Анджелес",
//          "zh-CN":  "洛杉矶"
//      }
//  },
//  "continent":  {
//      "code":       "NA",
//      "geoname_id": 123456,
//      "names":  {
//          "de":    "Nordamerika",
//          "en":    "North America",
//          "es":    "América del Norte",
//          "fr":    "Amérique du Nord",
//          "ja":    "北アメリカ",
//          "pt-BR": "América do Norte",
//          "ru":    "Северная Америка",
//          "zh-CN": "北美洲"
 
//      }
//  },
//  "country":  {
//      "confidence":           75,
//      "geoname_id":           6252001,
//      "is_in_european_union": true,
//      "iso_code":             "US",
//      "names":  {
//          "de":     "USA",
//          "en":     "United States",
//          "es":     "Estados Unidos",
//          "fr":     "États-Unis",
//          "ja":     "アメリカ合衆国",
//          "pt-BR":  "Estados Unidos",
//          "ru":     "США",
//          "zh-CN":  "美国"
//      }
//  },
//  "location":  {
//      "accuracy_radius":     20,
//      "average_income":      128321,
//      "latitude":            37.6293,
//      "longitude":           -122.1163,
//      "metro_code":          807,
//      "population_density":  7122,
//      "time_zone":           "America/Los_Angeles"
//  },
//  "postal": {
//      "code":       "90001",
//      "confidence": 10
//  },
//  "registered_country":  {
//      "geoname_id":           6252001,
//      "is_in_european_union": true,
//      "iso_code":             "US",
//      "names":  {
//          "de":     "USA",
//          "en":     "United States",
//          "es":     "Estados Unidos",
//          "fr":     "États-Unis",
//          "ja":     "アメリカ合衆国",
//          "pt-BR":  "Estados Unidos",
//          "ru":     "США",
//          "zh-CN":  "美国"
//      }
//  },
//  "represented_country":  {
//      "geoname_id":           6252001,
//      "is_in_european_union": true,
//      "iso_code":             "US",
//      "names":  {
//          "de":     "USA",
//          "en":     "United States",
//          "es":     "Estados Unidos",
//          "fr":     "États-Unis",
//          "ja":     "アメリカ合衆国",
//          "pt-BR":  "Estados Unidos",
//          "ru":     "США",
//          "zh-CN":  "美国"
//      },
//      "type": "military"
//  },
//  "subdivisions":  [
//      {
//          "confidence": 50,
//          "geoname_id": 5332921,
//          "iso_code":   "CA",
//          "names":  {
//              "de":     "Kalifornien",
//              "en":     "California",
//              "es":     "California",
//              "fr":     "Californie",
//              "ja":     "カリフォルニア",
//              "ru":     "Калифорния",
//              "zh-CN":  "加州"
//          }
//      }
//  ],
//  "traits": {
//      "autonomous_system_number":       1239,
//      "autonomous_system_organization": "Linkem IR WiMax Network",
//      "domain":                         "example.com",
//      "is_anonymous":                   true,
//      "is_anonymous_proxy":             true,
//      "is_anonymous_vpn":               true,
//      "is_hosting_provider":            true,
//      "is_public_proxy":                true,
//      "is_satellite_provider":          true,
//      "is_tor_exit_node":               true,
//      "isp":                            "Linkem spa",
//      "ip_address":                     "1.2.3.4",
//      "organization":                   "Linkem IR WiMax Network",
//      "user_type":                      "traveler"
//  },
//  "maxmind": {
//      "queries_remaining":   54321
//  }
//}
