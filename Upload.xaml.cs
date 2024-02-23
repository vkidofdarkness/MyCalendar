using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Web;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MyCalendar
{
    public partial class Upload : Window
    {
        private DBManager _databaseManager = new DBManager();

        private readonly HttpClientHandler _handler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseCookies = true,
            CookieContainer = new System.Net.CookieContainer()
        };
        private readonly HttpClient _httpClient;

        private const string ClientId = "student-personal-cabinet";
        private const string RedirectUri = "https://my.itmo.ru/login/callback";
        private const string Provider = "https://id.itmo.ru/auth/realms/itmo";
        private const string ApiBaseUrl = "https://my.itmo.ru/api";

        public Upload()
        {
            InitializeComponent();
            _httpClient = new HttpClient(_handler);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void Auth_Click(object sender, RoutedEventArgs e)
        {

            IsEnabled = false;

            try
            {
                var login = Login.Text;
                var password = Password.Password;

                if (String.IsNullOrEmpty(login) || String.IsNullOrEmpty(password)
                    || String.IsNullOrEmpty(SearchDate1.Text) || String.IsNullOrEmpty(SearchDate2.Text))
                {
                    MessageBox.Show("Заполните все поля.");
                    return;
                }

                var accessToken = await GetAccessToken(login, password);
                var lessons = await GetRawLessons(accessToken);

                string _TaskDate = "";
                string _TaskName = "";
                string _TaskDescription = "";

                foreach (var lesson in lessons)
                {
                    _TaskName = "";

                    foreach (var kvp in lesson)
                    {
                        if (kvp.Key == "date")
                        {
                            _TaskDate = kvp.Value;
                            continue;
                        }
                        else if (kvp.Key == "subject")
                        {
                            _TaskName += kvp.Value;
                            continue;
                        }
                        else if (kvp.Key == "type")
                        {
                            _TaskName += ", " + kvp.Value;
                            continue;
                        }
                        else if (kvp.Key == "time_start")
                        {
                            _TaskName += ", " + kvp.Value;
                            continue;
                        }
                        else if (kvp.Key == "time_end")
                        {
                            _TaskName += " - " + kvp.Value;
                            continue;
                        }
                    }

                    if (_TaskName != "" && _TaskDate != "")
                    {
                        _databaseManager.AddEvent(_TaskName, _TaskDescription, _TaskDate);
                    }
                }
                MessageBox.Show("Расписание успешно импортировано.");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка импорта расписания: {ex.Message}");
            }
            finally
            {
                IsEnabled = true;
            }
        }

        private async Task<string> GetAccessToken(string username, string password)
        {
            var codeVerifier = GenerateCodeVerifier();
            var codeChallenge = GetCodeChallenge(codeVerifier);

            var authResponse = await _httpClient.GetAsync($"{Provider}/protocol/openid-connect/auth?" +
                $"protocol=oauth2&response_type=code&client_id={ClientId}&redirect_uri={HttpUtility.UrlEncode(RedirectUri)}&" +
                $"scope=openid&state=im_not_a_browser&code_challenge_method=S256&code_challenge={codeChallenge}");
            authResponse.EnsureSuccessStatusCode();

            var authResponseHtml = await authResponse.Content.ReadAsStringAsync();
            var formActionMatch = Regex.Match(authResponseHtml, @"<form\s+.*?\s+action=""(.*?)""", RegexOptions.Singleline);
            if (!formActionMatch.Success)
            {
                throw new Exception("Соответствие регулярному выражению действия формы Keycloak не найдено.");
            }

            var formAction = HttpUtility.HtmlDecode(formActionMatch.Groups[1].Value);
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password),
            });

            var formResponse = await _httpClient.PostAsync(formAction, formData);

            if (formResponse.StatusCode != System.Net.HttpStatusCode.Redirect)
            {
                throw new Exception("Неправильное имя пользователя или пароль.");
            }

            var redirectUri = formResponse.Headers.Location.ToString();
            var authCode = HttpUtility.ParseQueryString(new Uri(redirectUri).Query).Get("code");

            var tokenResponse = await _httpClient.PostAsync($"{Provider}/protocol/openid-connect/token", new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("client_id", ClientId),
                new KeyValuePair<string, string>("redirect_uri", RedirectUri),
                new KeyValuePair<string, string>("code", authCode),
                new KeyValuePair<string, string>("code_verifier", codeVerifier),
            }));
            tokenResponse.EnsureSuccessStatusCode();

            var tokenResponseContent = await tokenResponse.Content.ReadAsStringAsync();
            var tokenJson = JsonSerializer.Deserialize<JsonElement>(tokenResponseContent);
            var accessToken = tokenJson.GetProperty("access_token").GetString();

            return accessToken;
        }

        private static string GenerateCodeVerifier()
        {
            using var rng = RandomNumberGenerator.Create();
            var bytes = new byte[32];
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private static string GetCodeChallenge(string codeVerifier)
        {
            using var sha256 = SHA256.Create();
            var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
            return Convert.ToBase64String(challengeBytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        private async Task<IEnumerable<Dictionary<string, string>>> GetRawLessons(string accessToken)
        {
            var calendarData = await GetCalendarData("/schedule/schedule/personal", accessToken);
            var lessons = new List<Dictionary<string, string>>();

            foreach (var day in calendarData.GetProperty("data").EnumerateArray())
            {
                var date = day.GetProperty("date").GetString();
                foreach (var lesson in day.GetProperty("lessons").EnumerateArray())
                {
                    var lessonDict = new Dictionary<string, string>
                    {
                        ["date"] = date
                    };
                    foreach (var prop in lesson.EnumerateObject())
                    {
                        lessonDict[prop.Name] = prop.Value.ToString();
                    }
                    lessons.Add(lessonDict);
                }
            }

            return lessons;
        }

        private Dictionary<string, string> GetDateRangeParams() => new Dictionary<string, string>
        {
            ["date_start"] = SearchDate1.SelectedDate.Value.ToString("yyyy-MM-dd"),
            ["date_end"] = SearchDate2.SelectedDate.Value.ToString("yyyy-MM-dd"),
        };

        private async Task<JsonElement> GetCalendarData(string path, string authToken)
        {
            var dateRangeParams = GetDateRangeParams();
            var queryParams = new FormUrlEncodedContent(dateRangeParams).ReadAsStringAsync().Result;

            var url = $"{ApiBaseUrl}{path}?{queryParams}";

            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            requestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authToken);

            var response = await _httpClient.SendAsync(requestMessage);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<JsonElement>(content);
        }
    }
}
