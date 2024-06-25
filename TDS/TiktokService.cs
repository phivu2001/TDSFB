using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace TDS
{
    public class TiktokService
    {
        private Common1 Common = new Common1(); 
        private bool isSetConfig;
        private Form1 form1;
       public TiktokService(Form1 form1)
        {
            this.form1 = form1;
        }
     
        public async Task testChromeTiktok(string Cookie, string idTiktok, string TDS_token)
        {

            var options = new ChromeOptions();
            var cookies = Common.setCookie(Cookie);
            var driver = UndetectedChromeDriver.Create(
                options: options,
                hideCommandPromptWindow: true,
                logLevel: 3,
                driverExecutablePath: await new ChromeDriverInstaller().Auto()
                );

            driver.GoToUrl("https://www.tiktok.com");

            cookies.ForEach(x =>
            {
                driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(x.key, x.value));
            });

            driver.GoToUrl("https://www.tiktok.com");

            // call API
            await Task.Run(() =>
            {
                SetConfig(idTiktok, TDS_token);
            });
            await Task.Run(() =>
            {
                while (true)
                {
                    if (isSetConfig == true)
                    {
                        GetMission("tiktok_comment", TDS_token, driver);
                        break;
                    }
                }
            });
        }

        private async void SetConfig(string idTiktok, string TDS_token)
        {
            string url = $"https://traodoisub.com/api/?fields=tiktok_run&id={idTiktok}&access_token={TDS_token}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    isSetConfig = true;
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}", "Error");
            }
        }

        private async void GetMission(string type, string TDS_token, UndetectedChromeDriver driver)
        {
            string url = $"https://traodoisub.com/api/?fields={type}&access_token={TDS_token}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<List<ApiResponseMisson>>(responseBody);
                    foreach (var item in apiResponse)
                    {
                        driver.SwitchTo().NewWindow(WindowType.Tab);
                        driver.GoToUrl(item.link);
                        string content = item.noidung;
                        form1.SetClipBoard(content);
                        try
                        {
                            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                            IWebElement commentArea = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[text()='Add comment...']")));
                            commentArea.Click();
                            Actions builder = new Actions(driver);
                            Thread.Sleep(1000);
                            builder.KeyDown(OpenQA.Selenium.Keys.Control).SendKeys("v").KeyUp(OpenQA.Selenium.Keys.Control);

                            builder.Build().Perform();
                            Thread.Sleep(1000);
                            IWebElement element = driver.FindElement(By.XPath("//div[@aria-label='Post' and @role='button']"));
                            element.Click();

                        }
                        catch (WebDriverTimeoutException)
                        {
                            Console.WriteLine("Không tìm thấy phần tử, bỏ qua và sang phần tử tiếp theo.");
                            continue;
                        }
                        catch (StaleElementReferenceException)
                        {
                            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                            IWebElement commentArea = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[text()='Add comment...']")));
                            commentArea.SendKeys("xin chao");
                        }

                        try
                        {

                            System.Threading.Thread.Sleep(2000);
                            string idMission = item.id;
                            string link = item.link;
                            form1.Showlst(link);
                            GetCoin(type, idMission, TDS_token);
                            System.Threading.Thread.Sleep(100);
                        }
                        catch (NoSuchElementException ex)
                        {
                            Console.WriteLine(ex);
                            continue;
                        }
                    }


                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}", "Error");
            }
        }

        private async void GetCoin(string type, string id_job, string TDS_token)
        {
            form1.Showlst(id_job);
            Thread.Sleep(15000);
            string url = $"https://traodoisub.com/api/coin/?type={type}&id={id_job}&access_token={TDS_token}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseCoin>(responseBody);
                    if(apiResponse.data != null) {
                        form1.Showlst(apiResponse.data.msg);
                    }
                    else
                    {
                        var error = JsonConvert.DeserializeObject<dynamic>(responseBody);
                        string err = error.error;
                        form1.Showlst(err);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Request error: {ex.Message}", "Error");
            }
        }
        private class ApiResponseConfig
        {
            public int success { get; set; }
            public DataConfig data { get; set; }
        }
        private class DataConfig
        {
            public string id { get; set; }
            public string uniqueID { get; set; }
            public string msg { get; set; }
        }
        private class ApiResponseMisson
        {
            public string id { get; set; }
            public string link { get; set; }
            public string type { get; set; }
            public string noidung { get; set; }
        }
        public class ApiResponseCoin
        {
            public int success { get; set; }
            public DataCoin data { get; set; }
        }
        public class DataCoin
        {
            public string xu { get; set; }
            public string xu_them { get; set; }
            public string job_success { get; set; }
            public string msg { get; set; }
        }
    }
}
