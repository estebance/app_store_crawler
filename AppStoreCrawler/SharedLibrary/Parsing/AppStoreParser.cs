﻿using HtmlAgilityPack;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace SharedLibrary.Parsing
{
    public class AppStoreParser
    {
        public IEnumerable<String> ParseCategoryUrls (string rootHtmlPage)
        {
            // Creating Html Map, and loading root page html on it
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (rootHtmlPage);

            // Reaching Nodes of Interest
            foreach (var htmlNode in map.DocumentNode.SelectNodes (Consts.XPATH_CATEGORIES_URLS))
            {
                // Checking for the Href Attribute
                HtmlAttribute href = htmlNode.Attributes["href"];

                // Sanity Check
                if (href != null)
                {
                    yield return href.Value;
                }
            }
        }

        public IEnumerable<String> ParseCharacterUrls (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            // Reaching nodes of interest
            foreach (HtmlNode characterNode in map.DocumentNode.SelectNodes (Consts.XPATH_CHARACTERS_URLS))
            {
                // Checking for Href Attribute within the node
                HtmlAttribute href = characterNode.Attributes["href"];

                // Sanity Check
                if (href != null)
                {
                    yield return href.Value;
                }
            }
        }

        public IEnumerable<String> ParseNumericUrls (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument();
            map.LoadHtml(htmlResponse);

            // Reaching nodes of interest
            foreach (HtmlNode characterNode in map.DocumentNode.SelectNodes(Consts.XPATH_NUMERIC_URLS))
            {
                // Checking for Href Attribute within the node
                HtmlAttribute href = characterNode.Attributes["href"];

                // Sanity Check
                if (href != null)
                {
                    yield return href.Value;
                }
            }
        }

        public bool IsLastPage (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            // Trying to reach "Next" node
            return map.DocumentNode.SelectSingleNode (Consts.XPATH_NEXT_PAGE) == null;
        }
        
        public String ParseLastPageUrl (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            return map.DocumentNode.SelectNodes (Consts.XPATH_LAST_PAGE).Last().Attributes["href"].Value;
        }

        public IEnumerable<String> ParseAppsUrls (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument();
            map.LoadHtml(htmlResponse);

            // Reaching nodes of interest
            foreach (HtmlNode characterNode in map.DocumentNode.SelectNodes (Consts.XPATH_APPS_URLS))
            {
                // Checking for Href Attribute within the node
                HtmlAttribute href = characterNode.Attributes["href"];

                // Sanity Check
                if (href != null)
                {
                    yield return href.Value;
                }
            }
        }

        public AppleStoreAppModel ParseAppPage (string htmlResponse)
        {
            // Creating HTML Map based on the html response
            HtmlDocument map = new HtmlDocument ();
            map.LoadHtml (htmlResponse);

            // Instantiating Empty Parsed App
            AppleStoreAppModel parsedApp = new AppleStoreAppModel ();

            // Reaching nodes of interest
            parsedApp.name              = GetNodeValue (map, Consts.XPATH_TITLE).Trim();
            parsedApp.developerName     = GetAppDeveloperName (map);
            parsedApp.developerUrl      = GetDeveloperUrl (map);
            parsedApp.price             = GetAppPrice (map);
            parsedApp.isFree            = parsedApp.price == 0.0 ? true : false;
            parsedApp.category          = GetAppCategory (map);
            parsedApp.updateDate        = GetAppUpdateDate (map);
            parsedApp.description       = GetAppDescription (map);
            parsedApp.version           = GetAppVersion (map);
            parsedApp.size              = GetAppSize (map);
            parsedApp.thumbnailUrl      = GetThumbnailUrl (map);
            parsedApp.languages         = GetLanguages (map);
            parsedApp.compatibility     = GetCompatibility (map);
            parsedApp.minimumAge        = GetMinimumAge (map);
            parsedApp.ageRatingReasons  = GetRatingReasons (map);
            parsedApp.rating            = GetRatings (map);
            parsedApp.topInAppPurchases = GetInAppPurchases (map);
            parsedApp.developerWebsite  = GetDeveloperWebsiteUrl (map);
            parsedApp.supportWebsite    = GetSupportWebsite (map);
            parsedApp.licenseAgreement  = GetLicenseAgreement (map);
            parsedApp.relatedApps       = GetRelatedApps(map);
            parsedApp.moreByDev         = GetMoreByDev(map);
            parsedApp.screenshots       = GetScreenshots(map);


            return parsedApp;
        }

        private string[] GetScreenshots(HtmlDocument map)
        {
            HtmlNodeCollection nodesCollection = map.DocumentNode.SelectNodes(Consts.XPATH_MORE_BY_DEV);
            // Sanity Check
            if (nodesCollection != null)
            {
                // Dumping "Src" attribute of each node to an array
                return nodesCollection.Select(t => t.Attributes["src"].Value).Distinct().ToArray();
            }

            return null;
        }

        private string[] GetMoreByDev(HtmlDocument map)
        {
            HtmlNodeCollection nodesCollection = map.DocumentNode.SelectNodes(Consts.XPATH_MORE_BY_DEV);
            // Sanity Check
            if (nodesCollection != null)
            {
                // Dumping "Src" attribute of each node to an array
                return nodesCollection.Select(t => t.Attributes["href"].Value).Distinct().ToArray();
            }

            return null;
        }

        private string[] GetRelatedApps(HtmlDocument map)
        {
            HtmlNodeCollection nodesCollection = map.DocumentNode.SelectNodes(Consts.XPATH_RELATED_APPS);
            // Sanity Check
            if (nodesCollection != null)
            {
                // Dumping "Src" attribute of each node to an array
                return nodesCollection.Select(t => t.Attributes["href"].Value).Distinct().ToArray();
            }

            return null;
        }

        private string GetAppDeveloperName (HtmlDocument map)
        {
            string developerName = GetNodeValue (map, Consts.XPATH_DEVELOPER_NAME);

            return String.IsNullOrEmpty (developerName) ? String.Empty : developerName.Replace ("By", String.Empty).Trim().ToUpper();
        }
        private string GetDeveloperUrl (HtmlDocument map)
        {
            // Developer Url Node
            HtmlNode devUrlNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_DEVELOPER_URL);

            // Returning Url
            return devUrlNode == null ? "No Developer Url Available" : devUrlNode.Attributes["href"].Value;
        }

        private double GetAppPrice (HtmlDocument map)
        {
            // Replacing App Price "dot" decimal separator with a comma to allow correct double conversion
            string stringPrice = GetNodeValue (map, Consts.XPATH_APP_PRICE).Replace ('.',',');

            // Checking for "free" app
            if (stringPrice.IndexOf ("free", StringComparison.InvariantCultureIgnoreCase) >= 0)
            {
                return 0;
            }

            // Else, parses the correct price out of the node
            // Culture info is used to determine the correct currency symbol to be removed. This might change depending on
            // the store country or your own IP sometimes
            CultureInfo cInfo = CultureInfo.GetCultureInfo (Consts.CURRENT_CULTURE_INFO);
            return Convert.ToDouble (stringPrice.Replace (cInfo.NumberFormat.CurrencySymbol, String.Empty));
        }

        private string GetAppCategory (HtmlDocument map)
        {
            // Reaching Category Node
            return GetNodeValue (map, Consts.XPATH_CATEGORY).Trim();
        }

        private DateTime GetAppUpdateDate (HtmlDocument map)
        {
            // Reaching Node that contains the update date
            HtmlNode updateDateNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_UPDATE_DATE);
            updateDateNode = updateDateNode.FirstChild.NextSibling;

            // Date Text
            string dateTxt = updateDateNode.InnerText;

            // Parsing Out 
            return DateTime.ParseExact (dateTxt, Consts.DATE_FORMAT, new CultureInfo (Consts.CURRENT_CULTURE_INFO));
        }

        private string GetAppDescription (HtmlDocument map)
        {
            return GetNodeValue (map, Consts.XPATH_DESCRIPTION);
        }

        private string GetAppVersion (HtmlDocument map)
        {
            // Reaching Node that contains the Version number
            HtmlNode versionNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_VERSION);
            versionNode = versionNode.ParentNode.FirstChild.NextSibling;

            return versionNode.InnerText.Trim();
        }

        private string GetAppSize (HtmlDocument map)
        {
            // Reaching Node that contains the Version number
            HtmlNode versionNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_APP_SIZE);
            versionNode = versionNode.ParentNode.FirstChild.NextSibling;

            return versionNode.InnerText.Trim ();
        }

        private string GetThumbnailUrl (HtmlDocument map)
        {
             // Reaching Thumbnail node
            HtmlNode thumbnailNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_THUMBNAIL);

            // Reaching Src attribute of the node
            return thumbnailNode.Attributes["src-swap-high-dpi"].Value;
        }

        private string[] GetLanguages (HtmlDocument map)
        {
            // Reaching Node that contains the Version number
            HtmlNode languagesNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_LANGUAGES);

            // Sanity Check
            if (languagesNode == null)
            {
                return null;
            }

            languagesNode          = languagesNode.FirstChild.NextSibling;

            return languagesNode.InnerText.Split (',').Select (t => t.Trim()).ToArray();
        }

        private string GetCompatibility (HtmlDocument map)
        {
            // Reaching Node that contains the Version number
            HtmlNode compatibilityNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_COMPATIBILITY);
            compatibilityNode = compatibilityNode.NextSibling;

            return compatibilityNode.InnerText;
        }

        private int GetMinimumAge (HtmlDocument map)
        {
            // Reaching inner text of the inner node
            string innerTxt = GetNodeValue (map, Consts.XPATH_MINIMUM_AGE);

            // Parsing result to int
            return Int32.Parse (String.Concat(innerTxt.Where (t => Char.IsDigit (t))));
        }

        private string[] GetRatingReasons (HtmlDocument map)
        {
            // List of parsed reasons
            List<String> reasons = new List<String> ();

            // Iterating over needed nodes
            try
            {
                foreach (var reasonNode in map.DocumentNode.SelectNodes (Consts.XPATH_RATING_REASONS))
                {
                    reasons.Add (reasonNode.InnerText.ToUpper ().Trim ());
                }
            }
            catch { } // Exception will be thrown in case there are no "rating reasons" for this app. This is ok and shouldn't be treated

            return reasons.ToArray ();
        }

        private Rating GetRatings (HtmlDocument map)
        {
            Rating appRating = new Rating ();

            // Reaching Rating Parent Node
            HtmlNode ratingNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_RATINGS);

            // Reaching child nodes for their data
            HtmlNode childNode = ratingNode.SelectSingleNode (".//div[@class='rating'][1]");

            // Checking if this app has any ratings
            if (childNode != null)
            {
                string attributeValue = childNode.Attributes["aria-label"].Value;
                int[]  parsedRatings   = RatingsParser (attributeValue);

                // Checking if this node is a "All Versions" one, or the "Current Version"
                if (childNode.PreviousSibling.PreviousSibling.InnerText.IndexOf ("all", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    appRating.starsVersionAllVersions = parsedRatings[0];
                    appRating.ratingsAllVersions      = parsedRatings[1];
                }
                else // Current Version
                {
                    appRating.starsRatingCurrentVersion = parsedRatings[0];
                    appRating.ratingsCurrentVersion     = parsedRatings[1];
                }

                // Checking if the second node exists
                childNode = ratingNode.SelectSingleNode (".//div[@class='rating'][2]");

                if (childNode != null)
                {
                    // Parsing attributes of the second node if it exists
                    attributeValue = childNode.Attributes["aria-label"].Value;
                    parsedRatings  = RatingsParser (attributeValue);

                    if (childNode.PreviousSibling.PreviousSibling.InnerText.IndexOf ("all", StringComparison.InvariantCultureIgnoreCase) >= 0)
                    {
                        appRating.starsVersionAllVersions = parsedRatings[0];
                        appRating.ratingsAllVersions      = parsedRatings[1];
                    }
                    else
                    {
                        appRating.starsRatingCurrentVersion = parsedRatings[0];
                        appRating.ratingsCurrentVersion     = parsedRatings[1];
                    }
                }                
            }

            return appRating;
        }

        private int[] RatingsParser (string attributeValue)
        {
            // Splitting the attribute in two and trimming the results
            string[] splitedAttribute = attributeValue.Split (',').Select (t => t.Trim()).ToArray();
            
            // Returning ratings
            return new int[]
            {
                Int32.Parse (String.Concat (splitedAttribute[0].Where ( t => Char.IsDigit (t)))),
                Int32.Parse (String.Concat (splitedAttribute[1].Where ( t => Char.IsDigit (t)))),
            };
        }

        private InAppPurchase[] GetInAppPurchases (HtmlDocument map)
        {
            List<InAppPurchase> inAppPurchases = new List<InAppPurchase> ();

            // Reaching root node of purchases
            HtmlNode inAppNode = map.DocumentNode.SelectSingleNode (Consts.XPATH_IN_APP_PURCHASES);

            // Checking if this page has the In App Purchase List
            if (inAppNode == null)
            {
                return null;
            }

            // Parsing out purchases
            int purchaseRanking = 1;
            CultureInfo cInfo   = CultureInfo.GetCultureInfo (Consts.CURRENT_CULTURE_INFO);
            foreach (HtmlNode purchaseListNode in inAppNode.SelectNodes (".//ol[@class='list']/li"))
            {
                // Parsing In App Purchase Name
                string inAppName = HttpUtility.HtmlDecode (purchaseListNode.SelectSingleNode ("./span[@class='in-app-title']").InnerText.Trim ());

                // Parsing In App Purchase Price
                string inAppPurchasePrice = purchaseListNode.SelectSingleNode ("./span[@class='in-app-price']").InnerText.Trim ().Replace ('.',',');

                double inAppPrice;

                // Converting String price to double
                if (inAppPurchasePrice.IndexOf ("free", StringComparison.InvariantCultureIgnoreCase) >= 0)
                {
                    inAppPrice = 0.0;
                }
                else
                {
                    inAppPrice = Convert.ToDouble (inAppPurchasePrice.Replace (cInfo.NumberFormat.CurrencySymbol, String.Empty));
                }

                // Creating Object
                inAppPurchases.Add (new InAppPurchase ()
                    {
                        ranking    = purchaseRanking++,
                        inAppName  = inAppName,
                        inAppPrice = inAppPrice
                    });
            }

            return inAppPurchases.ToArray ();
        }

        private string GetDeveloperWebsiteUrl (HtmlDocument map)
        {
            try
            {
                // Reaching Website Url
                return map.DocumentNode.SelectSingleNode (Consts.XPATH_WEBSITE_URL).Attributes["href"].Value;
            }
            catch
            {
                return null;
            }
        }

        private string GetSupportWebsite (HtmlDocument map)
        {
            try
            {
                // Reaching Support Url
                return map.DocumentNode.SelectSingleNode (Consts.XPATH_SUPPORT_URL).Attributes["href"].Value;
            }
            catch
            {
                return null;
            }
        }

        private string GetLicenseAgreement (HtmlDocument map)
        {
            try
            {
                return map.DocumentNode.SelectSingleNode (Consts.XPATH_LICENSE_URL).Attributes["href"].Value;
            }
            catch
            {
                return null;
            }
        }

        private string GetNodeValue (HtmlDocument map, string xPath)
        {
            var node = map.DocumentNode.SelectSingleNode (xPath);

            return node == null ? String.Empty : HttpUtility.HtmlDecode (node.InnerText);
        }
    }
}
