using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class AppleStoreAppModel
    {
        public String   _id                      { get; set; }
        public string   url                      { get; set; }
        public string   name                     { get; set; }
        public string   developerName            { get; set; }
        public string   developerUrl             { get; set; }
        public double   price                    { get; set; }
        public bool     isFree                   { get; set; }
        [BsonIgnoreIfNull]
        public string   description              { get; set; }
        public string   thumbnailUrl             { get; set; }
        public string   compatibility            { get; set; }
        public string   category                 { get; set; }
        public DateTime updateDate               { get; set; }
        public string   version                  { get; set; }
        public string   size                     { get; set; }
        public string[] languages                { get; set; }
        public int      minimumAge               { get; set; }
        public string[] ageRatingReasons         { get; set; }
        public Rating   rating                   { get; set; }
        [BsonIgnoreIfNull]
        public InAppPurchase[] topInAppPurchases { get; set; }
        public string          developerWebsite  { get; set; }
        public string          supportWebsite    { get; set; }
        public string          licenseAgreement  { get; set; }
        public string[]        relatedApps       { get; set; }
        public string[]        moreByDev         { get; set; }
        public string[]        screenshots       { get; set; }

        public string ToJson ()
        {
            return JsonConvert.SerializeObject (this);
        }

        public static AppleStoreAppModel FromJson (string json)
        {
            return JsonConvert.DeserializeObject<AppleStoreAppModel> (json);
        }
    }

    public class Rating
    {
        public int starsRatingCurrentVersion { get; set; }
        public int starsVersionAllVersions   { get; set; }
        public int ratingsCurrentVersion     { get; set; }
        public int ratingsAllVersions        { get; set; }

        public Rating ()
        {
            starsRatingCurrentVersion = 0;
            starsVersionAllVersions   = 0;
            ratingsCurrentVersion     = 0;
            ratingsAllVersions        = 0;
        }
    }

    public class InAppPurchase
    {
        public int ranking       { get; set; }
        public string inAppName  { get; set; }
        public double inAppPrice { get; set; }
    }
}
