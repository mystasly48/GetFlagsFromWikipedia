using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GetFlagsFromWikipedia {
  public class Program {
    public static string CountryListUrl = "https://ja.wikipedia.org/wiki/%E5%9B%BD%E3%81%AE%E4%B8%80%E8%A6%A7";

    public static void Main(string[] args) {
      Country[] countries = GetCountries(CountryListUrl);
      //Console.WriteLine(string.Join("\n", countries.Select(country => string.Format(
      //  "Name: {0}\nActualName: {1}\nOtherNames: {2}\nCapital: {3}\nFlagUrl: {4}\nWikipediaUrl: {5}\n", country.Name, country.ActualName, string.Join("/", country.OtherNames), country.Capital, country.FlagUrl, country.WikipediaUrl
      //))));
      Console.ReadKey();
    }

    public static Country[] GetCountries(string listUrl) {
      List<Country> countries = new List<Country>();
      using (WebClient client = new WebClient() { Encoding = Encoding.UTF8 }) {
        string html = client.DownloadString(listUrl);
        MatchCollection items = Regex.Matches(html,
        @"<td><b><a href=""(?<flagUrl>.*?)"".*?><.*?src=""(?<flagImageUrl>.*?)"".*?></a></b> <b><a href=""(?<wikipediaUrl>.*?)"" title=""(?<name>.*?)"">(?<actualName>.*?)</a></b>(?<otherNames>.*?)\n</td>",
        RegexOptions.Singleline);
        foreach (Match item in items) {
          Country country = new Country();
          country.Name = item.Groups["name"].Value.Trim();
          country.ActualName = item.Groups["actualName"].Value.Trim();
          string otherNames = Regex.Replace(item.Groups["otherNames"].Value.Trim(), @"<sup.*?/sup>", "").Replace("<br />", "");
          otherNames = string.IsNullOrEmpty(otherNames) ? "" : otherNames.Substring(1, otherNames.Length - 2);
          country.OtherNames = otherNames.Split('/').Select(x => x.Trim()).ToArray();
          string flagUrl = item.Groups["flagUrl"].Value.Trim();
          country.FlagUrl = "https:" + item.Groups["flagImageUrl"].Value.Trim().Replace("25px", "500px");
          country.WikipediaUrl = "https://ja.wikipedia.org" + item.Groups["wikipediaUrl"].Value.Trim();
          GetCountryDetail(ref country);
          // DEBUG BEGIN
          Console.WriteLine("Name: " + country.Name);
          Console.WriteLine("ActualName: " + country.ActualName);
          Console.WriteLine("OtherNames: " + string.Join(",", country.OtherNames));
          Console.WriteLine("Capital: " + country.Capital);
          Console.WriteLine("Language: " + country.Language);
          Console.WriteLine("Currency: " + country.Currency);
          Console.WriteLine("FlagImageUrl: " + country.FlagUrl);
          Console.WriteLine("WikipediaUrl: " + country.WikipediaUrl);
          Console.WriteLine();
          // DEBUG END
          countries.Add(country);
        }
      }
      return countries.ToArray();
    }

    public static void GetCountryDetail(ref Country country) {
      using (WebClient client = new WebClient() { Encoding = Encoding.UTF8 }) {
        string html = client.DownloadString(country.WikipediaUrl);
        Match item = Regex.Match(html,
          @"<tr>\n<th><a href=.*?>公用語</a>\n</th>\n<td>(?<languages>.*?)</td></tr>" + ".*?" +
          @"<tr>\n<th><a href=.*?>首都</a>\n</th>\n<td><a href=.*?>(?<capital>.*?)</a>.*?</td></tr>" + ".*?" +
          @"<tr>\n<th><a href=.*?>通貨</a>\n</th>\n<td><a href=.*?>(?<currency>.*?)</a>.*?</td></tr>",
          RegexOptions.Singleline);
        country.Capital = item.Groups["capital"].Value.Trim();
        country.Currency = item.Groups["currency"].Value.Trim();
        country.Language = string.Join("/", Regex.Replace(item.Groups["languages"].Value.Trim(), "<a href=.*?>|<sup.*?/sup>|</a>", ""));
      }
    }
  }

  public class Country {
    public string Name;
    public string ActualName;
    public string[] OtherNames;
    public string Capital;
    public string Currency;
    public string Language;
    public string FlagUrl;
    public string WikipediaUrl;
  }
}
