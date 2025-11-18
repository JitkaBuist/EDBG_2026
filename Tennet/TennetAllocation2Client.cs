using System;
using Energie.Car;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tennet.Models.Allocatie2;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using Tennet.Models.Generic;

namespace Tennet
{
    public class TennetAllocation2Client
    {
        private HttpClient client = new HttpClient();
        public TennetAllocation2Client(String KlantConfig)
        {
            KC.KlantConfig = KlantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            client.BaseAddress = new Uri(KC.Allocation2Url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }

        
        public async Task<List<ListMessages>> GetMessages(string ResponsibleParty, string CounterParty, Boolean ListNew, Boolean ListExisting)
        {
            List<ListMessages> VolumeSerie = null;
            HttpResponseMessage response = await client.GetAsync("/api/VolumeSeries/Messages/" + ResponsibleParty + "/"+ CounterParty + "?listNew=" + (ListNew == true?"true":"false") + "&listExisting=" + (ListExisting == true ? "true" : "false")) ;
            if (response.IsSuccessStatusCode)
            {
                VolumeSerie = await response.Content.ReadAsAsync<List<ListMessages>>();
            }
            return VolumeSerie;
        }

        public async Task<VolumeSeriesResult> GetVolumeSeries(string TechnicalMessageId)
        {
            VolumeSeriesResult VolumeSerie = null;
            HttpResponseMessage response = await client.GetAsync(@"/api/VolumeSeries/" + TechnicalMessageId.Trim());
            if (response.IsSuccessStatusCode)
            {
                VolumeSerie = await response.Content.ReadAsAsync<VolumeSeriesResult>();
            }
            return VolumeSerie;
        }
        public async Task<MeasurementSeriesResult> GetMeasurementSeries(string TechnicalMessageId)
        {
            MeasurementSeriesResult MeasurementSerie = null;
            HttpResponseMessage response = await client.GetAsync(@"/api/MeasurementSeries/" + TechnicalMessageId);
            if (response.IsSuccessStatusCode)
            {
                MeasurementSerie = await response.Content.ReadAsAsync<MeasurementSeriesResult>();
            }
            return MeasurementSerie;
        }
        public async Task<String> AcknowledgeVolumeSerie(string TechnicalMessageId, AckVolumeSeries ackVolumeSeries)
        {
            String Result = "";
            string content = JsonConvert.SerializeObject(ackVolumeSeries);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(@"/api/VolumeSeries/" + TechnicalMessageId + "/Acknowledgements"),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = "OK";
            }
            else
            {
                Result = "Failed";
            }
            return Result;
        }

        public async Task<String> AcknowledgeMeasurementSeries(string TechnicalMessageId, AckMeasurementSeries ackMeasurementSeries)
        {
            String Result = "";
            string content = JsonConvert.SerializeObject(ackMeasurementSeries);
            String url = client.BaseAddress +  @"api/MeasurementSeries/" + TechnicalMessageId + @"/Acknowledgements";
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = "OK";
            }
            else
            {
                Result = "Failed";
            }
            return Result;
        }
    }
}
