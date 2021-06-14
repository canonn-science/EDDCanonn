//#define STAGING
//#define NO_NETWORK

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static EDDDLLInterfaces.EDDDLLIF;

namespace EDDCanonn
{
    public class EDDClass
    {
        public string EDDInitialise(string vstr, string dllfolder, EDDDLLInterfaces.EDDDLLIF.EDDCallBacks edd)
        {
            // Check whitelist
            try
            {
                HttpWebRequest request = WebRequest.Create("https://us-central1-canonn-api-236217.cloudfunctions.net/postEventWhitelist") as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string body = reader.ReadToEnd();
                JArray whitelist = JArray.Parse(body);
                foreach (JObject l in whitelist)
                {
                    var definition = l["definition"];
                    if (definition == null) continue;

                    var e = definition["event"];
                    if (e == null) continue;

                    m_whitelist.Add(e.ToString());
                }
            }
            catch (Exception)
            {
                return null;
            }

            return "1.0.0.0";
        }

        void LogCodexEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry entry)
        {
            if (!m_whitelist.Contains("CodexEntry")) return;

            dynamic o = JsonConvert.DeserializeObject(entry.json);

            dynamic e = new JObject();
            e.gameState = new JObject();
            e.rawEvents = new JObject();

            // Mandatories
            e.gameState.systemName = o.System;
            e.gameState.systemAddress = o.SystemAddress;
            e.gameState.systemCoordinates = JArray.FromObject(new double[] { entry.x, entry.y, entry.z });
            e.gameState.clientVersion = "EDDCanonn v1.0";
            e.gameState.latitude = o.Latitude;
            e.gameState.longitude = o.Longitude;
            e.gameState.platform = "PC";
            if (entry.islanded) e.gameState.bodyName = entry.whereami;
            if (m_temperature >= 0) e.gameState.temperature = m_temperature;

#if STAGING
            e.gameState.isBeta = true;
            e.cmdrName = "TEST";
#else
            e.gameState.isBeta = false;
            e.cmdrName = entry.cmdrname;
#endif

            // #todo?
            //e.gameSystem.bodyId = "#todo";

            e.rawEvents = JArray.FromObject(new dynamic[1] { o });

            string result = JsonConvert.SerializeObject(new dynamic[1] { e });

            // #todo handle response
            StringContent content = new StringContent(result, Encoding.UTF8, "application/json");
#if !NO_NETWORK
            try
            {
                HttpResponseMessage response = m_client.PostAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/postEvent", content).Result;
            }
            catch (Exception)
            {
            }
#endif
        }

        void LogScanOrganic(EDDDLLInterfaces.EDDDLLIF.JournalEntry entry)
        {
            if (!m_whitelist.Contains("ScanOrganic")) return;

            dynamic o = JsonConvert.DeserializeObject(entry.json);

            dynamic e = new JObject();
            e.gameState = new JObject();
            e.rawEvents = new JObject();

            // Mandatories
            e.gameState.systemName = entry.systemname;
            e.gameState.systemAddress = o.SystemAddress;
            e.gameState.systemCoordinates = JArray.FromObject(new double[] { entry.x, entry.y, entry.z });
            e.gameState.clientVersion = "EDDCanonn v1.0";
            e.gameState.bodyName = entry.whereami;
            e.gameState.bodyId = o.Body;
            e.gameState.platform = "PC";
            e.gameState.odyssey = true;
            if (m_temperature >= 0) e.gameState.temperature = m_temperature;

#if STAGING
            e.gameState.isBeta = true;
            e.cmdrName = "TEST";
#else
            e.gameState.isBeta = false;
            e.cmdrName = entry.cmdrname;
#endif

            // #todo?
            //e.gameState.latitude = "#todo";
            //e.gameState.longitude = "#todo";

            e.rawEvents = JArray.FromObject(new dynamic[1] { o });

            string result = JsonConvert.SerializeObject(new dynamic[1] { e });

            // #todo handle response
            StringContent content = new StringContent(result, Encoding.UTF8, "application/json");
#if !NO_NETWORK
            try
            {
                HttpResponseMessage response = m_client.PostAsync("https://us-central1-canonn-api-236217.cloudfunctions.net/postEvent", content).Result;
            }
            catch (Exception)
            {
            }
#endif
        }

        public void EDDNewJournalEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry entry)
        {
            string type = entry.eventid;
            switch (type)
            {
                case "CodexEntry":
                    LogCodexEntry(entry);
                    break;
                case "ScanOrganic":
                    LogScanOrganic(entry);
                    break;
            }
        }

        public void EDDRefresh(string cmdname, EDDDLLInterfaces.EDDDLLIF.JournalEntry lastje)
        {
            System.Diagnostics.Debug.WriteLine("Refresh");
        }

        void UpdateTemperature(dynamic o)
        {
            m_temperature = o.Temperature;
        }

        void UpdateOverallStatus(dynamic o)
        {
            m_temperature = o.Temperature;
        }

        public void EDDNewUIEvent(string json)
        {
            dynamic o = JsonConvert.DeserializeObject(json);
            string type = o.EventTypeStr;
            switch (type)
            {
                case "Temperature":
                    UpdateTemperature(o);
                    break;
                case "OverallStatus":
                    UpdateOverallStatus(o);
                    break;
            }
        }

        HashSet<string> m_whitelist = new HashSet<string>();
        HttpClient m_client = new HttpClient();
        double m_temperature = -1;
    }
}
