using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using SeleniumUndetectedChromeDriver;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace TDS
{
    public partial class Form1 : Form
    {
        private bool isSetConfig = false;
        private Common1 Common1 = new Common1();
        public eSocial eSocial;
        private TiktokService tiktokService;
        private System.Windows.Forms.ListView[] listViewArray = new System.Windows.Forms.ListView[] { };
        private System.Windows.Forms.TextBox[] textBoxArray = new System.Windows.Forms.TextBox[] { };
        public Form1()
        {
            InitializeComponent();
            tiktokService = new TiktokService(this);
        }

        public async Task MainMethod()
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < dgvMain.Rows.Count; i++)
            {
                int indexRow = i;
                tasks.Add(Task.Run(() => testChromeFacebook(indexRow)));
            }

            await Task.WhenAll(tasks);
        }

        public async Task testChromeFacebook(int indexRow)
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-gpu");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-blink-features=AutomationControlled");

            // Hide traces of Selenium
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            string cookie = dgvMain.Rows[indexRow].Cells["CookieFacebook"].FormattedValue.ToString();
            var cookies = Common1.setCookie(cookie);
            var driver = new ChromeDriver(options);
            try
            {
                await Task.Run(() =>
                {
                    driver.Navigate().GoToUrl("https://www.facebook.com");
                    cookies.ForEach(x => driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(x.key, x.value)));
                    driver.Navigate().GoToUrl("https://www.facebook.com");
                });

                // Call API
                string idfb = dgvMain.Rows[indexRow].Cells["IDAcc"].FormattedValue.ToString();
                string TDS_token = dgvMain.Rows[indexRow].Cells["Accesstoken"].FormattedValue.ToString();

                await SetConfig(idfb, TDS_token, indexRow);
                await GetMission("like", TDS_token, driver, indexRow);
            }
            finally
            {
                driver.Quit();
            }
        }

        private async Task SetConfig(string idfb, string TDS_token, int index)
        {
            await Task.Run(async () =>
            {
                string url = $"https://traodoisub.com/api/?fields=run&id={idfb}&access_token={TDS_token}";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponseConfig>(responseBody);

                        AddToResultList(apiResponse.data.msg, index);
                        isSetConfig = true;
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Lỗi yêu cầu: {ex.Message}", "Lỗi");
                }
            });
        }

        private void AddToResultList(string message, int index, string totalCoin = "")
        {
            Task.Run(() =>
            {
                if (listViewArray.Length <= index)
                {
                    MessageBox.Show("Index must be between 1 and 12.");
                    return;
                }
                if (this.InvokeRequired)
                {
                    this.Invoke(new Action(() => AddToResultList(message, index, totalCoin)));
                    return;
                }
                {
                    System.Windows.Forms.ListView targetListView = listViewArray[index];
                    System.Windows.Forms.TextBox targetTextbox = textBoxArray[index];
                    if (totalCoin != "")
                    {
                        targetTextbox.Text = totalCoin;
                    }
                    targetListView.Items.Add(new ListViewItem(message));
                    if (targetListView.Items.Count > 0)
                    {
                        targetListView.EnsureVisible(targetListView.Items.Count - 1);
                    }
                }
            });
        }

        private async Task GetMission(string type, string TDS_token, ChromeDriver driver, int indexRow)
        {
            await Task.Run(async () =>
            {
                int index = 0;
                string url = $"https://traodoisub.com/api/?fields={type}&access_token={TDS_token}";
                string responseBody = "";

                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        responseBody = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<List<ApiResponseMisson>>(responseBody);

                        AddToResultList(apiResponse.Count.ToString(), indexRow);

                        foreach (var item in apiResponse)
                        {
                            index++;
                            driver.SwitchTo().NewWindow(WindowType.Tab);
                            driver.Navigate().GoToUrl($"https://www.facebook.com/{item.id}");

                            var waitPage = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                            waitPage.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                            string currentUrl = driver.Url;
                            if (currentUrl.Contains("checkpoint"))
                            {
                                driver.Close();
                                driver.SwitchTo().Window(driver.WindowHandles[0]);
                                continue;
                            }

                            try
                            {
                                IWebElement likeButton = null;
                                var waitPage1 = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                                if (currentUrl.Contains("reel"))
                                {
                                    likeButton = waitPage1.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@aria-label='Thích' and @role='button']")));
                                }
                                else
                                {
                                    likeButton = waitPage1.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Thích']")));
                                }
                                likeButton?.Click();

                                await Task.Delay(2000);

                                string idMission = item.id;
                                await GetCoin(type, idMission, TDS_token, driver, index, indexRow);

                                await Task.Delay(20000);
                            }
                            catch (NoSuchElementException)
                            {
                                continue;
                            }
                            catch (WebDriverTimeoutException)
                            {
                                continue;
                            }
                            finally
                            {
                                driver.Close();
                                driver.SwitchTo().Window(driver.WindowHandles[0]);
                            }
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Lỗi yêu cầu: {ex.Message}", "Lỗi");
                }
                catch (Exception ex)
                {
                    var error = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    Console.WriteLine(error.ToString());
                    string err = error.error;
                    Console.WriteLine("Loi excpetion" + ex.ToString());
                    AddToResultList(err, indexRow);
                    int countdown = error.countdown;
                    await Task.Delay(countdown * 1000);
                }

                await GetMission(type, TDS_token, driver, indexRow);
            });
        }


        private async Task GetCoin(string type, string id_job, string TDS_token, ChromeDriver driver, int index, int indexRow)
        {
            await Task.Run(async () =>
            {
                string url = $"https://traodoisub.com/api/coin/?type={type}&id={id_job}&access_token={TDS_token}";
                try
                {
                    using (HttpClient client = new HttpClient())
                    {
                        HttpResponseMessage response = await client.GetAsync(url);
                        response.EnsureSuccessStatusCode();
                        string responseBody = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<ApiResponseCoin>(responseBody);
                        if (apiResponse.data == null)
                        {
                            var error = JsonConvert.DeserializeObject<dynamic>(responseBody);
                            string err = error.error;
                            AddToResultList(err, indexRow);
                            try
                            {
                                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                                IWebElement likeButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Thích']")));
                                likeButton.Click();
                            }
                            catch (WebDriverTimeoutException)
                            {
                                Console.WriteLine("Không tìm thấy phần tử, bỏ qua và sang phần tử tiếp theo. Get coin");
                            }
                            catch (StaleElementReferenceException)
                            {
                                WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                                IWebElement likeButton = wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Thích']")));
                                likeButton.Click();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error: {ex.Message}");
                            }
                        }
                        else
                        {
                            string totalCoin = apiResponse.data.xu;
                            string msg = apiResponse.data.msg + " " + index;
                            AddToResultList(msg, indexRow, totalCoin);
                        }
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show($"Request error: {ex.Message}", "Error");
                }
            });
        }



        public class ApiResponseConfig
        {
            public int success { get; set; }
            public DataConfig data { get; set; }
        }
        public class DataConfig
        {
            public string id { get; set; }
            public string msg { get; set; }
        }
        public class ApiResponseCoin
        {
            public int success { get; set; }
            public DataCoin data { get; set; }
        }
        public class DataCoin
        {
            public string xu { get; set; }
            public string id { get; set; }
            public string msg { get; set; }
        }
        public class ApiResponseMisson
        {
            public string id { get; set; }
        }
        public void SetClipBoard(string value)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(SetClipBoard), value);
                return;
            }

            // Đặt dữ liệu vào Clipboard
            Clipboard.SetText(value);
        }
        public void Showlst(string test)
        {
            //if (lstResult.InvokeRequired)
            //{
            //    lstResult.Invoke((MethodInvoker)delegate
            //    {
            //        lstResult.Items.Add(test);
            //        lstResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //    });
            //}
            //else
            //{
            //    lstResult.Items.Add(test);
            //    lstResult.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
            //}
        }
        private async void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1 && e.RowIndex != -1)
            {
                if (dgvMain.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                {
                    await Task.Run(async () =>
                    {
                        await testChromeFacebook(e.RowIndex);
                    });
                    //await Task.Run(async () =>
                    //{
                    //    string test = "_ttp=2dBGrxO2A50Di16PsxcrSO2WDuz; tt_chain_token=vfxns2MfkehDu0D79R8llw==; passport_csrf_token=334a4a4d0cd24ca0e5f3c726419a1280; passport_csrf_token_default=334a4a4d0cd24ca0e5f3c726419a1280; store-country-code-src=uid; tt_csrf_token=iJwWQdeI-WqznQZQGCLaNDbU3lY7Yue4aX0w; ak_bmsc=DFC4D6AD52C29920043191938AC5E2FF~000000000000000000000000000000~YAAQNPrSFyzkFpmPAQAAViIloBevYj2VhaUWFFp26xb4GKIX0346z9IRbwlDyCY06cdHZwd+wjgG/PzFB3nNkcibbqNUho/LkfD+4FbPiZBQlQQJFkvDMamVEj1S5ft8JWyF7JmbKrIDZX1kXjdwChAN4OTuY6GAxGHZj2WbuOxneRvAhy63v7ss28xTX12UctRCOCc8Si0UFKMbC1rKxl0cM64f7jV+s7SVAj8yfN9NEijZ6EP+OVksXyQ5cpMV6K/AnJTpl8xPdet+GSjYuUHyTejL3TQeimR6PKVv+34a4Hy8BjBlxz8jBmFXKeBgz3arlLUoOv6JZma+cQZO3JMA7iK9Nvr1KNtGCvaag0ulEfFoeTNRPNOOyKtcyW5h3zCtQqgbzYL0W0w=; s_v_web_id=verify_lwhrlnzy_ZiaMyNsC_QTtJ_49Va_9t3K_M4MjHqiOaann; multi_sids=7259568914643436549%3A69d21a44a31cd5b1dc976076d81e704a; cmpl_token=AgQQAPOFF-RO0rSBxdCD-J08_HM_y5nW_5MOYNfnnA; passport_auth_status=98c1c84e1f8f3f67b5d7818e57c940a5%2C; passport_auth_status_ss=98c1c84e1f8f3f67b5d7818e57c940a5%2C; sid_guard=69d21a44a31cd5b1dc976076d81e704a%7C1716378768%7C15551999%7CMon%2C+18-Nov-2024+11%3A52%3A47+GMT; uid_tt=dbf3bde7b0a1c564eb88d056a0e28d547c62a19aab558361b69e5f9a6d2b60c9; uid_tt_ss=dbf3bde7b0a1c564eb88d056a0e28d547c62a19aab558361b69e5f9a6d2b60c9; sid_tt=69d21a44a31cd5b1dc976076d81e704a; sessionid=69d21a44a31cd5b1dc976076d81e704a; sessionid_ss=69d21a44a31cd5b1dc976076d81e704a; sid_ucp_v1=1.0.0-KDFlZDMzNGI4ODMwZjBlMjg5YWZhYTY3OTkxZjQyYzI1MjU0ZjcwOTUKHwiFiKne8O7K32QQkLm3sgYYswsgDDCf1_ylBjgIQBIQAxoGbWFsaXZhIiA2OWQyMWE0NGEzMWNkNWIxZGM5NzYwNzZkODFlNzA0YQ; ssid_ucp_v1=1.0.0-KDFlZDMzNGI4ODMwZjBlMjg5YWZhYTY3OTkxZjQyYzI1MjU0ZjcwOTUKHwiFiKne8O7K32QQkLm3sgYYswsgDDCf1_ylBjgIQBIQAxoGbWFsaXZhIiA2OWQyMWE0NGEzMWNkNWIxZGM5NzYwNzZkODFlNzA0YQ; store-idc=alisg; store-country-code=vn; tt-target-idc=alisg; tt-target-idc-sign=ejjVg9BWPNJ6nQ2BIWdcYCfXqOz30qZewoVpb52ceLkgbsD_xSX6VNGo-ey7HNbCWI9Q3puPCHlbd0ZmOs6AaNsmmS5G4GiZOVwVBaFGPkk6HtAfUJUf3gfCkjljfe2in2nZOx2tWQOMRbS5oaDIcbApbG3cWfR1IrxDjuKrlpQkNQQzOVcEcXostAFQgw3H5K3fgs8Sa5Amck2aHfeKwIMU_K5zb28j2crswIz_Ag0qpscocKBPYZEI7EcUJe-FkcamaO1MCdzMMqqCtILvOHHhubL4sGHkP7AyiQI4bR3AtEPUHUra9nDXX3eqDjvrSNAah5dxg6HK4jIbmODoeUrmPYaBSLDuEXubYSV0kS09SyhPBwHKp1h17qQVsNJIXcds9TCxCv814m8u5OfuAglFRVS3eS_5_jvliI12VwGjZUcxPSgJ7cKcu1uIJ20mYQsCV7-45wasfMrEYjEDigKxNw-9n8KOuFyerIBpsTvtvlQ8dCn7bDUqbdDQJS53; ttwid=1%7CDqVNzvDmFpRSVpbi2AgEsjv554Brh5PUeKQMwsKC28w%7C1716380800%7Cda35cc22059638db2ebdc72c202abd1f0c7ea5f1326253a24814e4e482c5c1d6; odin_tt=c0581e8d5cf516210955dc04ad964ed628f5913bbef9b22c4991ff80d67acd8c7f2c5d84faee80576bdf3848ebd4586f8bcd54e83ac268469aca55ca9a0c0a06decd1bc33291fc51cc2e705f6676fd38; msToken=nW1_e89H3ZD9k8L6BUY-Atq5FIX2hc-v0c0jB_p7dwtwT_1y2zGq6EhCKym5E_zwzcSFnacJCq9_QQO1gtRxV0bPhvA7BH_Yhj2mG_V3Wd3A9a5y4qJsaZ_vOaR0NqIIbWKIsQ==; bm_sv=1F2298157235FF279FDCEE7FE5A00A62~YAAQV/rSF+ZcKKCPAQAAKNNLoBcvTnkd00bQF+v8GG6tTiNMFS0xRTOx16rtk64vtVPpEYWr9CTgeBLHWjS3W5Cej13T5HCAXST8fSK0oYmzMJ50SZjAZ6ovIhzNHpSBFLXiB4Yqoq5yO8fS5v6WYM+CoeBVPA+OrN0pkcmvx4EBFbYFNH2VrsCfe/EmAoLZ6jvRK5EYplwKMEyHU7Bm/sEn9jv12E2Gvt4i1RIVHCZE5VAEibh0kSKZSnDEf35PkA==~1";
                    //    string test1 = "7259568914643436549";
                    //    string test2 = "TDSQfiIjclZXZzJiOiIXZ2V2ciwiIwMjM4YnbhVHeiojIyV2c1Jye";
                    //    await tiktokService.testChromeTiktok(test, test1, test2);
                    //});

                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            DataGridViewComboBoxColumn socialColumn = dgvMain.Columns["Social"] as DataGridViewComboBoxColumn;
            DataGridViewComboBoxColumn fieldsColumn = dgvMain.Columns["Fields"] as DataGridViewComboBoxColumn;
            socialColumn.DataSource = Common.lstSocial;
            if (socialColumn.Items.Count > 0)
            {
                socialColumn.DefaultCellStyle.NullValue = Common.lstSocial[(int)eSocial.Facebook];
            }
            fieldsColumn.DataSource = Common.lstFieldsFacebook;
            if (fieldsColumn.Items.Count > 0)
            {
                fieldsColumn.DefaultCellStyle.NullValue = Common.lstFieldsFacebook[0];
            }
            List<Account> lstAccount = AccountManager.ReadAccountsFromFile(@"D:\Project\TDSS\file\Account.xml");
            foreach (Account account in lstAccount)
            {
                object[] data =
                {
                    account.Name,
                    account.Accesstocken,
                    account.Idacc,
                    account.Cookie
                };
                dgvMain.Rows.Add(data);
            }

            int listViewWidth = 477;
            int listViewHeight = 79;
            int textBoxHeight = 20;
            int startX = 7;
            int startY = 299;
            int spacingX = 15;
            int spacingY = 8;
            textBoxArray = new System.Windows.Forms.TextBox[dgvMain.Rows.Count];
            listViewArray = new System.Windows.Forms.ListView[dgvMain.Rows.Count];
            for (int i = 0; i < dgvMain.Rows.Count; i++)
            {
                textBoxArray[i] = new System.Windows.Forms.TextBox();
                listViewArray[i] = new System.Windows.Forms.ListView();
                System.Windows.Forms.TextBox textBox = new System.Windows.Forms.TextBox
                {
                    Size = new Size(listViewWidth, textBoxHeight),
                    Location = new Point(startX + (i % 3) * (listViewWidth + spacingX),
                                        startY + (i / 3) * (listViewHeight + spacingY + textBoxHeight) - textBoxHeight),
                };

                textBoxArray[i] = textBox;

                this.Controls.Add(textBox);
                System.Windows.Forms.ListView listView = new System.Windows.Forms.ListView
                {
                    Size = new Size(listViewWidth, listViewHeight),
                    Location = new Point(startX + (i % 3) * (listViewWidth + spacingX),
                                         startY + (i / 3) * (listViewHeight + spacingY + textBoxHeight)),
                    View = View.Details 
                };
                listView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
                listView.Columns.Add("", listViewWidth - 4);

                listViewArray[i] = listView;

                this.Controls.Add(listView);

            }


            //dgvMain.Rows[0].Cells["IDAcc"].Value = "100093531398711";
            //dgvMain.Rows[0].Cells["CookieFacebook"].Value = "sb=nrtRZvkZ95mzHlxJy9sEkPh1; datr=nrtRZreW6yP0EoP61yJWLaQS; dpr=1.25; c_user=100093531398711; xs=48%3AYrDpLryvI7fwnA%3A2%3A1716632492%3A-1%3A7207%3A%3AAcVBzzdZw-eEmUoE983qYVxitH4E0WoeAIDnPUkuWQ; fr=1cBWiWjWKGy19a2oP.AWVw5puzi_dG0GEIYKwrnmnGuMU.BmUveC..AAA.0.0.BmUveC.AWVydpUfY_Y; presence=C%7B%22t3%22%3A%5B%5D%2C%22utc3%22%3A1716713372781%2C%22v%22%3A1%7D; wd=982x730\r\n";
            //dgvMain.Rows[0].Cells["Accesstoken"].Value = "TDSQfiIjclZXZzJiOiIXZ2V2ciwiIwMjM4YnbhVHeiojIyV2c1Jye";
        }

        private void dgvMain_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewComboBoxCell cbSocial = (DataGridViewComboBoxCell)dgvMain.Rows[e.RowIndex].Cells["Social"];
                cbSocial.Value = cbSocial.FormattedValue;
                if (cbSocial.Value != null)
                {
                    dgvMain.Invalidate();

                    DataGridViewComboBoxColumn fieldsColumn = dgvMain.Columns["Fields"] as DataGridViewComboBoxColumn;
                    if (cbSocial.Value.ToString() == Common.lstSocial[(int)eSocial.Facebook])
                    {
                        fieldsColumn.DataSource = Common.lstFieldsFacebook;
                    }
                    else if (cbSocial.Value.ToString() == Common.lstSocial[(int)eSocial.Tiktok])
                    {
                        fieldsColumn.DataSource = Common.lstFieldsTiktok;
                    }
                    else if (cbSocial.Value.ToString() == Common.lstSocial[(int)eSocial.Intagram])
                    {
                        fieldsColumn.DataSource = Common.lstFieldsIntagram;
                    }
                }
            }
        }
        private void dgvMain_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvMain.IsCurrentCellDirty)
            {
                dgvMain.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }
    }
}
