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
using System.IO;
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

            // Ẩn dấu vết của Selenium
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            string cookie = dgvMain.Rows[indexRow].Cells["CookieFacebook"].FormattedValue.ToString();
            var cookies = Common1.setCookie(cookie);
            using (var driver = new ChromeDriver(options)) // Sử dụng 'using' để tự động quản lý tài nguyên
            {
                try
                {
                    driver.Navigate().GoToUrl("https://www.facebook.com");
                    cookies.ForEach(x => driver.Manage().Cookies.AddCookie(new OpenQA.Selenium.Cookie(x.key, x.value)));
                    driver.Navigate().GoToUrl("https://www.facebook.com");

                    // Gọi API
                    string idfb = dgvMain.Rows[indexRow].Cells["IDAcc"].FormattedValue.ToString();
                    string TDS_token = dgvMain.Rows[indexRow].Cells["Accesstoken"].FormattedValue.ToString();

                    await SetConfig(idfb, TDS_token, indexRow).ConfigureAwait(false);
                    await GetMission("like", TDS_token, driver, indexRow).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi");
                }
            }
        }


        private async Task SetConfig(string idfb, string TDS_token, int index)
        {
            string url = $"https://traodoisub.com/api/?fields=run&id={idfb}&access_token={TDS_token}";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync(url).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var apiResponse = JsonConvert.DeserializeObject<ApiResponseConfig>(responseBody);

                    AddToResultList(apiResponse.data.msg, index);
                }
            }
            catch (HttpRequestException ex)
            {
                MessageBox.Show($"Lỗi yêu cầu: {ex.Message}", "Lỗi");
            }
        }

        private void AddToResultList(string message, int index, string totalCoin = "")
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(() => AddToResultList(message, index, totalCoin)));
                return;
            }

            lock (listViewArray) // Sử dụng lock để đảm bảo an toàn với luồng
            {
                if (listViewArray.Length <= index)
                {
                    MessageBox.Show("Index phải nằm trong khoảng từ 1 đến 12.");
                    return;
                }

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
                                var waitPage1 = new WebDriverWait(driver, TimeSpan.FromSeconds(60));
                                if (currentUrl.Contains("reel"))
                                {
                                    likeButton = waitPage1.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//div[@aria-label='Thích' and @role='button']")));
                                }
                                else
                                {
                                    likeButton = waitPage1.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//span[text()='Thích']")));
                                }
                                likeButton.Click();

                                await Task.Delay(2000);

                                string idMission = item.id;
                                GetCoin(type, idMission, TDS_token, driver, index, indexRow);

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


        private void GetCoin(string type, string id_job, string TDS_token, ChromeDriver driver, int index, int indexRow)
        {
            Task.Run(async () =>
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
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex != -1 && e.RowIndex != -1)
            {
                if (dgvMain.Columns[e.ColumnIndex] is DataGridViewButtonColumn)
                {
                    Task.Run(() => testChromeFacebook(e.RowIndex));
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
            string xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "file", "Account.xml");

            List<Account> lstAccount = AccountManager.ReadAccountsFromFile(xmlFilePath);
            foreach (Account account in lstAccount)
            {
                object[] data =
                {
                    account.NameAcc,
                    account.AccessToken,
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
