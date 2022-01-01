using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace WhatTimeIsEastendersOn
{
    public partial class MainPage : ContentPage
    {
        private readonly string _url = "https://www.bbc.co.uk/iplayer/guide";
        private readonly HttpClient _httpClient = new HttpClient();

        public string StartTime { get; set; }
        public string Synopsis { get; set; }

        public MainPage()
        {
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

            BindingContext = this;
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            var schedule = await GetScheduleHtml();
            SetProgrammeStrings(schedule);

            StartTimeText.Text = $"Start time: {StartTime}";
            SynopsisText.Text = Synopsis;
            SetIsEnabled(!string.IsNullOrWhiteSpace(StartTime));

            DateText.Text = DateTime.Now.ToString("dd'/'MM'/'yyyy");
        }

        private void SetProgrammeStrings(string html)
        {
            var document = new HtmlDocument();
            document.LoadHtml(html);

            var scheduleItems = document.DocumentNode.QuerySelectorAll(".schedule-item");

            foreach (var item in scheduleItems)
            {
                var anchor = item?.QuerySelector("div.gel-layout > div > a");
                var ariaLabel = anchor?.GetAttributes()?.FirstOrDefault(x => x?.Name == "aria-label");

                if (ariaLabel == null || !ariaLabel.Value.ToUpper().Contains("EASTENDERS"))
                    continue;

                var startTime = item.QuerySelector(".schedule-item__start-time");

                if (startTime != null)
                    StartTime = startTime.InnerText;

                var synopsis = item.QuerySelector("p.list-content-item__synopsis");

                if (synopsis != null)
                    Synopsis = synopsis.InnerText;
            }
        }

        private async Task<string> GetScheduleHtml()
        {
            var response = await _httpClient.GetAsync(_url);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            return string.Empty;
        }

        private void SetIsEnabled(bool isScheduled)
        {
            StartTimeText.IsEnabled = isScheduled;
            SynopsisText.IsEnabled = isScheduled;
            NotOnText.IsEnabled = !isScheduled;
        }
    }
}
