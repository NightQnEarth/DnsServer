using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;

namespace DnsServer
{
    public class DnsLocalCache
    {
        private readonly string cacheFileName;
        private Dictionary<CachedRecordKey, HashSet<CachedRecordValue>> cachedRecords;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public DnsLocalCache(string cacheFileName)
        {
            this.cacheFileName = cacheFileName;

            jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.Converters.Add(new IPAddressConverter());
            jsonSerializerSettings.Converters.Add(new DomainConverter());
            jsonSerializerSettings.Converters.Add(new CachedRecordKeyConverter());
            jsonSerializerSettings.Converters.Add(new RecordTypeConverter());
            jsonSerializerSettings.Formatting = Formatting.Indented;
        }

        public bool Empty => cachedRecords is null || cachedRecords.Count == 0;

        public void LoadCache()
        {
            if (File.Exists(cacheFileName))
            {
                Console.WriteLine("Loading existing cache...");

                cachedRecords = JsonConvert.DeserializeObject<Dictionary<CachedRecordKey, HashSet<CachedRecordValue>>>(
                    File.ReadAllText(cacheFileName), jsonSerializerSettings);

                if (!Empty)
                    return;
            }
            else
                Console.WriteLine("Will create new cache...");

            cachedRecords = new Dictionary<CachedRecordKey, HashSet<CachedRecordValue>>();
        }

        public void SaveCache()
        {
            Console.WriteLine("Storing cache...");

            try
            {
                using (var fileStream = new FileStream(cacheFileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8, 4096, true))
                        streamWriter.WriteLine(JsonConvert.SerializeObject(cachedRecords, jsonSerializerSettings));
            }
            catch (IOException exception)
            {
                Console.WriteLine(exception);
            }
        }

        public void UpdateCache()
        {
            foreach (var cachedRecord in cachedRecords)
                cachedRecord.Value.RemoveWhere(cachedRecordValue => !VerifyCachedRecord(cachedRecordValue));

//            cachedRecords = cachedRecords.Where(pair => pair.Value.Count > 0)
//                                         .ToDictionary(pair => pair.Key, pair => pair.Value);

            bool VerifyCachedRecord(CachedRecordValue recordValue) =>
                DateTime.Now - recordValue.CachedTime < recordValue.ResourceRecord.TimeToLive;
        }

        public void CacheResponse(IResponse response)
        {
            foreach (var resourceRecord in response.AnswerRecords.Concat(response.AuthorityRecords)
                                                   .Concat(response.AdditionalRecords))
            {
                var cachedRecordKey = new CachedRecordKey(resourceRecord.Name, resourceRecord.Type);

                if (cachedRecords.ContainsKey(cachedRecordKey))
                    cachedRecords[cachedRecordKey].Add(new CachedRecordValue(resourceRecord));
                else
                    cachedRecords[cachedRecordKey] = new[] { new CachedRecordValue(resourceRecord) }.ToHashSet();
            }
        }

        public IEnumerable<IResourceRecord> FindResponse(Question question)
        {
            cachedRecords.TryGetValue(new CachedRecordKey(question.Name, question.Type), out var cachedResourceRecords);

            return cachedResourceRecords?.Select(cachedRecordValue => cachedRecordValue.ResourceRecord);
        }
    }
}