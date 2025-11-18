using System;
using Energie.Car;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Tennet.Models.GLDPM;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Edbg.GenerationLoad.Client.Clients;
using Microsoft.Extensions.Options;
using Tennet.Models.Generic;
using Edbg.Basis.Calendar;
using System.Linq;
using System.Data.SqlClient;
using Edbg.GenerationLoad.Client.Models;

namespace Tennet
{
    public class TennetGLDPMClient
    {
        private HttpClient client = new HttpClient();
        private CultureInfo provider = CultureInfo.InvariantCulture;
        private readonly DateTimeHelper dateTimeHelper = new DateTimeHelper();
        public TennetGLDPMClient(String KlantConfig)
        {
            KC.KlantConfig = KlantConfig;
            KC.ConnStringPortaal = Energie.DataAccess.Configurations.GetApplicationSetting("KLANTCONFIG");

            client.BaseAddress = new Uri(KC.GLDPMUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
        
        public async Task<List<GetAcknowledgementResponse>> GetAcknowledgement(string TransmissionId, String Revision, Boolean Acknowledged, DateTime StartDateTime)
        {
            List<GetAcknowledgementResponse> getAcknowledgementResponse = null;
            HttpResponseMessage response = await client.GetAsync(@"/api/GenerationLoads?TransmissionId=" + TransmissionId + "&Revision=" + Revision + "&Acknowledged=" + (Acknowledged == true?"1":"0") + "&StartDateTime=" + StartDateTime.ToString("yyyy-MM-dd HH:mm", provider));
            if (response.IsSuccessStatusCode)
            {
                getAcknowledgementResponse = await response.Content.ReadAsAsync<List<GetAcknowledgementResponse>>();
            }
            return getAcknowledgementResponse;
        }

        public async Task<Acknowledgement> ProcessGenerationLoadResultsAsync(string responsiblePartyEan, DateTime processDate)
        {
            var result = new Acknowledgement
            {
                CreationTime = DateTime.UtcNow,
                ProcessDate = processDate,
                ResponsibleParty = responsiblePartyEan,
                Items = new List<AcknowledgementItem>()

            };

            SqlConnection conn = new SqlConnection(KC.ConnString);
            conn.Open();

            String strSql = "INSERT INTO dbo.GLDPMAcknowledgement \n";
            strSql += "(CreationTime \n";
            strSql += ",ProcessDate \n";
            strSql += ",ResponsibleParty \n";
            strSql += ",Success \n";
            strSql += ",Message) \n";
            strSql += "VALUES \n";
            strSql += "(@CreationTime \n";
            strSql += ",@ProcessDate \n";
            strSql += ",@ResponsibleParty \n";
            strSql += ",@Success \n";
            strSql += ",@Message);SELECT SCOPE_IDENTITY()";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@CreationTime", DateTime.UtcNow);
            cmd.Parameters.AddWithValue("@ProcessDate", processDate);
            cmd.Parameters.AddWithValue("@ResponsibleParty", responsiblePartyEan);
            int processId = (int)cmd.ExecuteScalar();

            //List<GLDPMAckResponse> result = new List<GLDPMAckResponse>();
            var processConfig = new ProcessClientConfigurationOptions("https://localhost:44331/", 600);//KC.GLDPMUrl
            GenerationLoadDocumentClient generationLoadDocumentClient = new GenerationLoadDocumentClient((IOptions < Edbg.GenerationLoad.Client.GenerationLoadConfiguration > )processConfig);


            try
            {
                var data = (await generationLoadDocumentClient.GetGenerationLoadDocumentsAsync(null, null, false,
                    dateTimeHelper.NlDateToUtcDateTime(processDate))).ToList();
                foreach (var gridOperatorData in data)
                {
                    var itemResult = await HandleAcknowledgementAsync(responsiblePartyEan, gridOperatorData, processId, result);
                    result.Items = result.Items.Union(itemResult).ToList();
                    if (itemResult.Any(ir => !ir.Success || ir.HasRejections == true))
                    {
                        result.Success = false;
                    }
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = ex.Message;
            }

            strSql = "UPDATE GLDPMAcknowledgement \n";
            strSql += "SET Success = @Success \n";
            strSql += ",Message = @Message \n";
            strSql += "WHERE Id = @ProcessId";
            cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@Success", result.Success);
            cmd.Parameters.AddWithValue("@Message", result.Message);
            cmd.Parameters.AddWithValue("@ProcessId", processId);
            cmd.ExecuteNonQuery();

            conn.Close();

            return result;
        }

        private async Task<IEnumerable<AcknowledgementItem>> HandleAcknowledgementAsync(string responsiblePartyEan, GenerationLoadDocumentResponseModel gridOperatorData,
            int processId, Acknowledgement result)
        {
            var processConfig = new ProcessClientConfigurationOptions("https://localhost:44331/", 600);//KC.GLDPMUrl
            AcknowledgementClient acknowledgementClient = new AcknowledgementClient((IOptions<Edbg.GenerationLoad.Client.GenerationLoadConfiguration>)processConfig);

            try
            {
                var request = new AcknowledgementRequestModel
                {
                    CorrelationId = gridOperatorData.CorrelationId,
                    Operator = gridOperatorData.Operator,
                    ResponsableParty = responsiblePartyEan
                };
                var itemResult =
                    await acknowledgementClient.ReceiveAcknowledgementsAsync(request);

                List<AcknowledgementItem> items = new List<AcknowledgementItem>();

                foreach (var ean in gridOperatorData.TimeSeries.Select(ts => ts.Ean18Code).Distinct().ToList())
                {

                    if (!itemResult.Any())
                    {
                        // no ack, so every connection is N/A
                        items.Add(await HandleAcknowledgementItemAsync(false, gridOperatorData.CorrelationId, gridOperatorData.Operator, ean, gridOperatorData.RevisionNumber, result.ProcessDate, false, null, processId));
                    }
                    else
                    {

                        if (HasRejections(itemResult, ean))
                        {
                            // rejected
                            items.Add(await HandleAcknowledgementItemAsync(true, gridOperatorData.CorrelationId, gridOperatorData.Operator, ean, gridOperatorData.RevisionNumber, result.ProcessDate, true, itemResult.FirstOrDefault()?.TechnicalMessageId, processId));
                        }
                        else
                        {
                            // accepted
                            items.Add(await HandleAcknowledgementItemAsync(true, gridOperatorData.CorrelationId, gridOperatorData.Operator, ean, gridOperatorData.RevisionNumber, result.ProcessDate, false, itemResult.FirstOrDefault()?.TechnicalMessageId, processId));
                        }

                    }

                }
                return items;
            }
            catch (Exception ex)
            {
                var processItems = gridOperatorData.TimeSeries.Select(ts => ts.Ean18Code).Distinct().Select(ean => new AcknowledgementItem { CorrelationId = gridOperatorData.CorrelationId, Ean18Code = ean, Operator = gridOperatorData.Operator, Success = false, Message = ex.Message }).ToList();
                foreach (var item in processItems)
                {
                    //using (var uow = UnitOfWorkManager.Begin())
                    //{
                    //    await dataService.AddItemAsync(processId, item);
                    //    await uow.CompleteAsync();
                    //}
                }
                return processItems;
            }

        }

        private async Task<AcknowledgementItem> HandleAcknowledgementItemAsync(bool success, string correlatieId, string gridOperator, string ean18Code, int revisionNumber, DateTime processDate, bool rejected, string technicalMessageId, int processId)
        {
            
            var item = new AcknowledgementItem
            {
                CorrelationId = correlatieId,
                Operator = gridOperator,
                Ean18Code = ean18Code,
                Success = success,
                HasRejections = rejected,
                Message = success ? "" : "acknowledgement is not available yet"
            };

            SqlConnection conn = new SqlConnection(KC.ConnString);
            conn.Open();
            String strSql = "INSERT INTO dbo.GLDPMAcknowledgementItems /n";
            strSql += "(ProcessId /n";
            strSql += ",Operator /n";
            strSql += ",CorrelationId /n";
            strSql += ",Ean18Code /n";
            strSql += ",HasRejections /n";
            strSql += ",Success /n";
            strSql += ",Message) /n";
            strSql += "VALUES /n";
            strSql += "(@ProcessId /n";
            strSql += ",@Operator /n";
            strSql += ",@CorrelationId /n";
            strSql += ",@Ean18Code /n";
            strSql += ",@HasRejections /n";
            strSql += ",@Success /n";
            strSql += ",@Message)";
            SqlCommand cmd = new SqlCommand(strSql, conn);
            cmd.Parameters.AddWithValue("@ProcessId", processId);
            cmd.Parameters.AddWithValue("@Operator", item.Operator);
            cmd.Parameters.AddWithValue("@CorrelationId", item.CorrelationId);
            cmd.Parameters.AddWithValue("@Ean18Code", item.Ean18Code);
            cmd.Parameters.AddWithValue("@HasRejections", item.HasRejections);
            cmd.Parameters.AddWithValue("@Success", item.Success);
            cmd.Parameters.AddWithValue("@Message", item.Message);
            await cmd.ExecuteNonQueryAsync();
            conn.Close();

            return item;
        }

        private static bool HasRejections(IEnumerable<AcknowledgementModel> itemResult, string ean18Code)
        {
            return itemResult.Any(it => it.Responses.Any(r => r.Rejections.Any() || r.TimePeriods.Any(t => t.Rejections.Any())
                  || r.TimeSeries.Any(ts => ts.SeriesId.Contains(ean18Code) && (ts.Rejections.Any() || ts.Periods.Any(tp => tp.Rejections.Any())))));
        }

        public async Task<List<GLDPMAckResponse>> GetAcknowledgement(GLDPMAckRequest gLDPMAckRequest)
        {
            List<GLDPMAckResponse> Result = null;

            string content = JsonConvert.SerializeObject(gLDPMAckRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(@"/api/Acknowledgements"),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<GLDPMAckResponse>>();
            }

            return Result;
        }

        public async Task<List<GenerationLoadResponse>> SendGLDPM(GenerationLoadRequest generationLoadRequest)
        {
            List<GenerationLoadResponse> Result = null;
           
            string content = JsonConvert.SerializeObject(generationLoadRequest);
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(@"/api/GenerationLoads"),
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                Result = await response.Content.ReadAsAsync<List<GenerationLoadResponse>>();
            }
            
            return Result;
        }


       
    }
}
