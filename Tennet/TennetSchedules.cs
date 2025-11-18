using System;
using Energie.Car;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tennet.Models.Schedules;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace Tennet
{
    public class TennetSchedules
    {
        public HttpClient client = new HttpClient();
        private CultureInfo provider = CultureInfo.InvariantCulture;
        public TennetSchedules(String KlantConfig)
        {
            KC.KlantConfig = KlantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            client.BaseAddress = new Uri(KC.ScheduleUrl); //@"https://localhost:44343/"); // KC.ScheduleUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
       
        public async Task<List<ScheduleResponse>> SendSchedule(ScheduleRequest scheduleRequest)
        {
            List<ScheduleResponse> Result = null;

            string content = JsonConvert.SerializeObject(scheduleRequest);
            //var request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    //RequestUri = new Uri(@"api/ScheduleMarkets"),
            //    Content = new StringContent(content, Encoding.UTF8, "application/json")
            //};

            


            HttpResponseMessage response = await client.PostAsync(@"api/ScheduleMarkets", new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<ScheduleResponse>>(); 
            }

            return Result;
        }

        public async Task<List<ScheduleResponse>> UpdateScheduleMarket(String TransmissionId, int RevisionNr, ScheduleRequest scheduleRequest)
        {
            List<ScheduleResponse> Result = null;

            string content = JsonConvert.SerializeObject(scheduleRequest);
            //var request = new HttpRequestMessage
            //{
            //    Method = HttpMethod.Post,
            //    //RequestUri = new Uri(@"api/ScheduleMarkets"),
            //    Content = new StringContent(content, Encoding.UTF8, "application/json")
            //};

            String url = @"api/ScheduleMarkets/" + TransmissionId + "/" + RevisionNr.ToString();

            HttpResponseMessage response = await client.PostAsync(url, new StringContent(content, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<ScheduleResponse>>();
            }

            return Result;
        }
        public async Task<List<AnomalyResponse>> GetAnomalies(AnomalyRequest anomalyRequest)
        {
            List<AnomalyResponse> Result = null;

            string content = JsonConvert.SerializeObject(anomalyRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(@"/api/Anomalies"),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<AnomalyResponse>>();
            }

            return Result;
        }

        
        public async Task<List<AcknowledgementResponse>> PostAcknowledgement(AcknowledgementRequest acknowledgementRequest, String DomesticForeign)
        {
            List<AcknowledgementResponse> Result = null;

            string content = JsonConvert.SerializeObject(acknowledgementRequest);
            String url = @"/api/Acknowledgements";
            if (DomesticForeign != "") { url = url + @"/" + DomesticForeign; }
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<AcknowledgementResponse>>();
            }

            return Result;
        }

        public async Task<List<ConfirmationResponse>> PostConformation(ConfirmationRequest confirmationRequest, String DomesticForeign)
        {
            List<ConfirmationResponse> Result = null;

            string content = JsonConvert.SerializeObject(confirmationRequest);
            String url = @"/api/Confirmations";
            if (DomesticForeign != "") { url = url + @"/" + DomesticForeign; }
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<ConfirmationResponse>>();
            }

            return Result;
        }

    }
}
