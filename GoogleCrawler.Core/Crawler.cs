using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GoogleCrawler.Core;

public class Crawler : IDisposable
{
    public Crawler()
    {
        browserArgs.Add("sandbox", "--no-sandbox");
    }

    private readonly string _UrlRegex = @"http.*/@(?<lat>-?\d*\.\d*),(?<lon>-?\d*\.\d*),(?<zzz>\d*z).*";
    private readonly string _GoogleMapUrl = "https://google.com/maps/search";

    private string phrase;
    private string url;
    private double searchRadius;
    private string browserPath;
    private Dictionary<string, string> browserArgs = new Dictionary<string, string>();

    private PuppeteerSharp.Browser browser;
    private PuppeteerSharp.Page page;
    private bool isFirstPage = true;

    public Crawler SetPhrase(string phrase)
    {
        this.phrase = phrase.Replace(" ", "+");
        this.url = $"{_GoogleMapUrl}/{this.phrase}";
        return this;
    }
    public double SearchRadius { set => this.searchRadius = value; get => searchRadius; }
    public string BrowserPath { set => this.browserPath = value; get => browserPath; }
    private double sourceLat;
    private double sourceLon;


    private HtmlDocument doc = new HtmlDocument();
    public Crawler SetProxy(string host, int port)
    {
        if (browserArgs.ContainsKey("proxy"))
        {
            browserArgs["proxy"] = string.Concat("--proxy-server=", host, ":", port);
        }
        else
        {
            browserArgs.Add("proxy", string.Concat("--proxy-server=", host, ":", port));
        }

        return this;
    }

    public async Task Launch()
    {
        this.browser = await PuppeteerSharp.Puppeteer.LaunchAsync(new PuppeteerSharp.LaunchOptions
        {
            Headless = true,
            Timeout = 60 * 1000,
            ExecutablePath = browserPath,
            //SlowMo = 30,
            //IgnoreDefaultArgs = false,
            //IgnoredDefaultArgs = new[] { "enable-automation" },
            Args = browserArgs.Values.ToArray(),
        });

        page = (await browser.PagesAsync())[0];
    }

    public async Task Run(CancellationToken token)
    {
        
        var response = await page.GoToAsync(url);
        if (response?.Ok ?? false)
            doc.LoadHtml(await page.GetContentAsync());
        else
        {
            this.Dispose();

            Exceptions.BadOrNoResponseException badResponseException;
            if (response == null)
                badResponseException = new("There is no response from server. Please check your connection.");
            else 
                badResponseException = new(response.StatusText);

            throw badResponseException;
        }

        await ResolvePlaces(token);
    }

    private Task ResolvePlaces(CancellationToken token)
    {
        while(true && !token.IsCancellationRequested)
        {
            if (isFirstPage)
            {
                var match = Regex.Match(page.Url, _UrlRegex);
                var lat = match.Groups["lat"].Value;
                var lon = match.Groups["lon"].Value;

                if (double.TryParse(lat, out var tempLat) && double.TryParse(lon, out var tempLon))
                {
                    sourceLat = tempLat;
                    sourceLon = tempLon;
                }

                isFirstPage = false;
            }

            var names = doc.DocumentNode.SelectNodes("//h3[contains(@class, 'section-result-title')]");
            var results = doc.DocumentNode.SelectNodes("//*[@id='pane']/div/div[1]/div/div/div[4]/div[1]/div[contains(@class, 'section-result')]");

            for (var i = 0; i < results.Count; i++)
            {

            }
        }

        if(token.IsCancellationRequested)
            Dispose();

        return Task.CompletedTask;
    }



#pragma warning disable CA1816 // Dispose methods should call SuppressFinalize
    public void Dispose()
#pragma warning restore CA1816 // Dispose methods should call SuppressFinalize
    {
        browser.Dispose();
    }
}

