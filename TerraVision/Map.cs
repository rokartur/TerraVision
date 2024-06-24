using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using Newtonsoft.Json.Linq;
using TerraVision.Models;

namespace TerraVision
{
    public partial class Map : UtilsForm
    {
        private readonly Panel _leftContainer;
        private readonly ListView _countryList;
        private readonly GMapControl _gmap;
        private readonly HttpClient _httpClient;
        private readonly ComboBox _searchBox;
        private readonly Label _loggedInUserLabel;
        
        public Map(User loggedInUser)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Size = new System.Drawing.Size((int)(Screen.PrimaryScreen.Bounds.Width * 0.6), (int)(Screen.PrimaryScreen.Bounds.Height * 0.6));
            this.CenterToScreen();
            var users = LoadUsers();
            var initUser = users.Find(u => u.Username == loggedInUser.Username);
            Cursor.Current = Cursors.WaitCursor;
            
            _httpClient = new HttpClient();

            _leftContainer = new Panel();
            _leftContainer.Dock = DockStyle.Left;
            _leftContainer.Padding = new Padding(10);
            _leftContainer.Width = this.Width / 5;
            _leftContainer.BackColor = Color.White;
            Controls.Add(_leftContainer);
            
            _countryList = new ListView();
            _countryList.Dock = DockStyle.Fill;
            _countryList.View = View.Details;
            _countryList.HeaderStyle = ColumnHeaderStyle.Nonclickable;
            var flagColumn = _countryList.Columns.Add("Flag");
            flagColumn.Width = 32;
            var countryColumn = _countryList.Columns.Add("Country");
            countryColumn.Width = 152;
            _leftContainer.Controls.Add(_countryList);
            
            LoadCountries();
            
            _countryList.MouseClick += async (sender, e) =>
            {
                var hitTest = _countryList.HitTest(e.Location);
                if (hitTest.Item != null && (hitTest.Item.SubItems[0] == hitTest.SubItem || hitTest.Item.SubItems[1] == hitTest.SubItem))
                {
                    var countryName = hitTest.Item.SubItems[1].Text;
                    var countryService = new CountryService();
                    var country = await countryService.GetCountryByName(countryName, _loggedInUserLabel, _searchBox);

                    if (country == null)
                    {
                        MessageBox.Show("Country not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    var countryInfoForm = new CountryInfo();
                    countryInfoForm.ShowCountryInfo(country);
                    countryInfoForm.ShowDialog();
                }
            };
            
            var rightContainer = new Panel();
            rightContainer.Dock = DockStyle.Right;
            rightContainer.Padding = new Padding(10);
            rightContainer.Width = this.Width / 5;
            rightContainer.BackColor = Color.White;
            Controls.Add(rightContainer);
                
            _gmap = new GMapControl();
            _gmap.Dock = DockStyle.Fill;
            Controls.Add(_gmap);
            
            var sidebar = new Panel();
            sidebar.Dock = DockStyle.Fill;
            sidebar.BackColor = Color.White;
            rightContainer.Controls.Add(sidebar);
            
            var logoutButton = new Button();
            logoutButton.Text = "Logout";
            logoutButton.Dock = DockStyle.Bottom;
            logoutButton.Click += LogoutButton_Click;
            sidebar.Controls.Add(logoutButton);
            
            var searchButton = new Button();
            searchButton.Text = "Szukaj";
            searchButton.Dock = DockStyle.Top;
            searchButton.Click += async (sender, e) =>
            {
                var countryService = new CountryService();
                var country = await countryService.GetCountryByName(_searchBox.Text, _loggedInUserLabel, _searchBox);

                if (country == null)
                {
                    MessageBox.Show("Country not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var countryInfoForm = new CountryInfo();
                countryInfoForm.ShowCountryInfo(country);
                countryInfoForm.ShowDialog();
            };
            sidebar.Controls.Add(searchButton);
            
            _searchBox = new ComboBox();
            _searchBox.Dock = DockStyle.Top;
            _searchBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
            _searchBox.AutoCompleteSource = AutoCompleteSource.ListItems;
            _searchBox.KeyDown += SearchBox_KeyDown;
            sidebar.Controls.Add(_searchBox);

            var searchLabel = new Label();
            searchLabel.Text = "Search country by name:";
            searchLabel.Dock = DockStyle.Top;
            sidebar.Controls.Add(searchLabel);
            
            _loggedInUserLabel = new Label();
            _loggedInUserLabel.Text = $"Zalogowany jako: {loggedInUser.Username}";
            _loggedInUserLabel.Dock = DockStyle.Top;
            sidebar.Controls.Add(_loggedInUserLabel);
            
            _gmap.MapProvider = GMapProviders.GoogleMap;
            _gmap.DragButton = MouseButtons.Left;
            _gmap.Position = new PointLatLng(51.5074, -0.1278); // London
            _gmap.MinZoom = 3;
            _gmap.MaxZoom = 5;
            _gmap.Zoom = 3;
            _gmap.AutoScroll = true;

            _gmap.OnMapClick += async (latLng, mouseEventArgs) =>
            {
                if (mouseEventArgs.Button != MouseButtons.Left) return;
                Cursor.Current = Cursors.WaitCursor;
                var country = await GetCountryInfoByItude(latLng.Lat, latLng.Lng);
                if (country == null)
                {
                    Cursor.Current = Cursors.Default;
                    return;
                }
                var countryInfoForm = new CountryInfo();
                countryInfoForm.ShowCountryInfo(country);
                countryInfoForm.ShowDialog();
                Cursor.Current = Cursors.Default;
            };

            if (initUser.SearchHistory == null) return;
            initUser.SearchHistory.Reverse();
            foreach (var searchItem in initUser.SearchHistory)
            {
                _searchBox.Items.Add(searchItem);
            }
        }
        
        private void LogoutButton_Click(object sender, EventArgs e)
        {
            var login = new Login();
            login.Show();
            this.Close();
        }
        private async Task LoadCountries()
        {
            var allCountriesResponse = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");
            var allCountriesData = JArray.Parse(allCountriesResponse);

            var countryNames = new List<string>();
            var countryFlags = new List<string>();
            for (int i = 0; i < allCountriesData.Count; i++)
            {
                var countryName = allCountriesData[i]["name"]?["common"]?.ToString();
                var countryFlag = allCountriesData[i]["flags"]?["png"]?.ToString();
                countryNames.Add(countryName);
                countryFlags.Add(countryFlag);
            }

            var sortedCountries = countryNames.Zip(countryFlags, (name, flag) => new { Name = name, Flag = flag })
                .OrderBy(country => country.Name)
                .ToList();

            var imageList = new ImageList();
            for (int i = 0; i < sortedCountries.Count; i++)
            {
                var request = WebRequest.Create(sortedCountries[i].Flag);

                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                {
                    var originalImage = Image.FromStream(stream);
                    var flagImage = originalImage.GetThumbnailImage(28, 16, null, IntPtr.Zero);
                    imageList.Images.Add(flagImage);
                }

                _countryList.Items.Add(new ListViewItem(new[] { "", sortedCountries[i].Name }) { ImageIndex = imageList.Images.Count - 1 });
            }

            _countryList.SmallImageList = imageList;
            Cursor.Current = Cursors.Default;
        }
        private async Task<Country> GetCountryInfoByItude(double lat, double lng)
        {
            var countryInfoResponse = await _httpClient.GetStringAsync($"https://api.bigdatacloud.net/data/reverse-geocode-client?latitude={lat}&longitude={lng}&localityLanguage=en");
            var countryInfoData = JObject.Parse(countryInfoResponse);
            var countryCode = countryInfoData["countryCode"]?.ToString();
            var countryTimezoneName = countryInfoData["localityInfo"]?["informative"]?[1]?["name"]?.ToString();
            
            if (countryCode != null && countryCode.Length == 0)
            {
                MessageBox.Show("Country not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            
            var allCountries = await _httpClient.GetStringAsync("https://restcountries.com/v3.1/all");
            var countriesData = JArray.Parse(allCountries);
            var country = countriesData.SelectToken($"$[?(@.cca2 == '{countryCode}')]");
            
            if (country == null) return null;
            
            var countryCommonName = country["name"]?["common"]?.ToString();
            var countryOfficialName = country["name"]?["official"]?.ToString();
            var countryCapital = country["capital"]?[0]?.ToString();
            var countryCurrencies = country["currencies"]?.ToString();
            var countryContinents = country["continents"]?[0]?.ToString();
            var countrySubRegion = country["subregion"]?.ToString();
            var countryLanguages = country["languages"]?.ToString();
            var countryPopulation = country["population"]?.ToString();
            var countryTimezones = country["timezones"]?.ToString();
            var countryArea = country["area"].ToString();
            var countryFlag = country["flags"]["png"].ToString();
            var countryLat = Convert.ToDouble(country["latlng"][0]);
            var countryLng = Convert.ToDouble(country["latlng"][1]);

            var countryWeatherResponse = await _httpClient.GetStringAsync(
                $"https://api.open-meteo.com/v1/forecast?latitude={countryLat}&longitude={countryLng}&current=temperature_2m,wind_speed_10m");
            var countryWeatherData = JObject.Parse(countryWeatherResponse);
            var temperatureUnit = countryWeatherData["current_units"]["temperature_2m"].ToString();
            var speedUnit = countryWeatherData["current_units"]["wind_speed_10m"].ToString();
            var currentTemperature = Convert.ToDouble(countryWeatherData["current"]["temperature_2m"]);
            var currentWindSpeed = Convert.ToDouble(countryWeatherData["current"]["wind_speed_10m"]);

            var weather = new Weather
            {
                TemperatureUnit = temperatureUnit,
                SpeedUnit = speedUnit,
                CurrentTemperature = currentTemperature,
                CurrentWindSpeed = currentWindSpeed
            };
            
            var newCountry = new Country()
            {
                Code = countryCode,
                CommonName = countryCommonName,
                OfficialName = countryOfficialName,
                Flag = countryFlag,
                Capital = countryCapital,
                Population = int.Parse(countryPopulation),
                Area = countryArea,
                Continent = new Continent { Name = countryContinents },
                Subregion = countrySubRegion,
                Languages = countryLanguages.Split(','),
                Currencies = countryCurrencies.Split(','),
                Timezones = countryTimezones.Split(','),
                Lat = countryLat,
                Lng = countryLng,
                CurrentWeather = weather
            };

            return newCountry;
        }
        
        private async void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            var countryService = new CountryService();
            var country = await countryService.GetCountryByName(_searchBox.Text, _loggedInUserLabel, _searchBox);
            e.SuppressKeyPress = true;
            Cursor.Current = Cursors.WaitCursor;

            if (country == null)
            {
                MessageBox.Show("Country not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Cursor.Current = Cursors.Default;
                return;
            }

            var countryInfoForm = new CountryInfo();
            countryInfoForm.ShowCountryInfo(country);
            countryInfoForm.ShowDialog();
            Cursor.Current = Cursors.Default;
        }
    }
}
