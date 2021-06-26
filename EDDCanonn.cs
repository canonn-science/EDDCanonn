//#define STAGING
//#define NO_NETWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static EDDDLLInterfaces.EDDDLLIF;
using BaseUtils.JSON;

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

                    m_whitelist.Add((string)e);
                }
            }
            catch (Exception)
            {
                return null;
            }

            return "1.1.0.0";
        }

        void LogCodexEntry(EDDDLLInterfaces.EDDDLLIF.JournalEntry entry)
        {
            if (!m_whitelist.Contains("CodexEntry")) return;

            JToken o = JToken.Parse(entry.json);

            JObject e = new JObject();
            JObject game_state = new JObject();
            JObject raw_events = new JObject();
            e["gameState"] = game_state;

            // Mandatories
            game_state["systemName"] = o["System"];
            game_state["systemAddress"] = o["SystemAddress"];
            game_state["systemCoordinates"] = JArray.FromObject(new double[] { entry.x, entry.y, entry.z });
            game_state["clientVersion"] = "EDDCanonn v1.1.0";
            game_state["latitude"] = o["Latitude"];
            game_state["longitude"] = o["Longitude"];
            game_state["platform"] = "PC";
            if (entry.islanded) game_state["bodyName"] = entry.whereami;
            if (m_temperature >= 0) game_state["temperature"] = m_temperature;

#if STAGING
            game_state["isBeta"] = true;
            e["cmdrName"] = "TEST";
#else
            game_state["isBeta"] = false;
            e["cmdrName"] = entry.cmdrname;
#endif

            // #todo?
            //e["gameSystem"].bodyId = "#todo";

            e["rawEvents"] = JArray.Parse("[" + entry.json + "]");

            string result = "[" + e.ToString() + "]";

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

            JToken o = JToken.Parse(entry.json);

            JObject e = new JObject();
            JObject game_state = new JObject();
            JObject raw_events = new JObject();
            e["gameState"] = game_state;

            // Mandatories
            game_state["systemName"] = entry.systemname;
            game_state["systemAddress"] = o["SystemAddress"];
            game_state["systemCoordinates"] = JArray.FromObject(new double[] { entry.x, entry.y, entry.z });
            game_state["clientVersion"] = "EDDCanonn v1.0";
            game_state["bodyName"] = entry.whereami;
            game_state["bodyId"] = o["Body"];
            game_state["platform"] = "PC";
            game_state["odyssey"] = true;
            if (m_temperature >= 0) game_state["temperature"] = m_temperature;

#if STAGING
            game_state["isBeta"] = true;
            e["cmdrName"] = "TEST";
#else
            game_state["isBeta"] = false;
            e["cmdrName"] = entry.cmdrname;
#endif

            // #todo?
            //game_state["latitude"] = "#todo";
            //game_state["longitude"] = "#todo";

            e["rawEvents"] = JArray.Parse("[" + entry.json + "]");

            string result = "[" + e.ToString() + "]";

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

        void UpdateTemperature(JToken o)
        {
            m_temperature = (double)o["Temperature"];
        }

        void UpdateOverallStatus(JToken o)
        {
            m_temperature = (double)o["Temperature"];
        }

        public void EDDNewUIEvent(string json)
        {
            JToken o = JToken.Parse(json);
            if (o == null) return;

            string type = (string)(o["EventTypeStr"]);
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
