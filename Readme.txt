

               <GEOIP2Helper> - What's new in the latest version


     Version <1.0>
	 This is handle query GeoLite2 information and cached it for second lookup.

     1. How to use

        a) Default you need GeoLite2 folder and 2 files in this folder: GeoLite2-ASN.mmdb, GeoLite2-City.mmdb
           And just simple to query: var geoIPInforamtion = "113.190.248.214".GetGeoIP2();

        b) You can setting up your new mmdb by using
           GeoIP2Helper.SetNewMMDBPath(string citymmdb, string asnmmdb)
  

      