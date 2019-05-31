using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;
using Newtonsoft.Json;

namespace DnsServer
{
    public class DnsLocalCache
    {
        private readonly string cacheFileName;
        private Dictionary<string, HashSet<SerializableResourceRecord>> cachedRecords;
        private readonly JsonSerializerSettings jsonSerializerSettings;

        public DnsLocalCache(string cacheFileName)
        {
            this.cacheFileName = cacheFileName;

            SetUpJsonSerializerSettings(out jsonSerializerSettings);

            LoadCache();
        }

        public bool ToCacheResponse(IResponse response)
        {
            bool wasAdded = false;

            foreach (var resourceRecord in response.AnswerRecords.Concat(response.AuthorityRecords)
                                                   .Concat(response.AdditionalRecords)
                                                   .Where(record => record.Type != RecordType.PTR))
            {
                var cachedRecordKey = $"{resourceRecord.Name} {resourceRecord.Type}";

                if (cachedRecords.ContainsKey(cachedRecordKey))
                    wasAdded |= cachedRecords[cachedRecordKey].Add(new SerializableResourceRecord(resourceRecord));
                else
                {
                    cachedRecords[cachedRecordKey] =
                        new[] { new SerializableResourceRecord(resourceRecord) }.ToHashSet();
                    wasAdded = true;
                }
            }

            return wasAdded;
        }

        public void SaveCache()
        {
            Console.WriteLine("Storing cache...");

            try
            {
                using (var fileStream = new FileStream(cacheFileName, FileMode.OpenOrCreate, FileAccess.Write))
                    using (var streamWriter = new StreamWriter(fileStream, Encoding.UTF8, 8192, true))
                        streamWriter.WriteLine(JsonConvert.SerializeObject(cachedRecords, jsonSerializerSettings));
            }
            catch (Exception exception) when (exception is JsonException || exception is IOException)
            {
                Console.WriteLine(exception);
            }
        }

        public void UpdateCache()
        {
            cachedRecords = cachedRecords
                            .Where(pair =>
                            {
                                pair.Value.RemoveWhere(resourceRecord => !VerifyCachedRecord(resourceRecord));
                                return pair.Value.Count > 0;
                            })
                            .ToDictionary(pair => pair.Key, pair => pair.Value);

            bool VerifyCachedRecord(SerializableResourceRecord resourceRecord) =>
                DateTime.Now - resourceRecord.CachedTime < resourceRecord.TimeToLive;
        }

        public IEnumerable<ResourceRecord> FindResponse(Question question)
        {
            HashSet<SerializableResourceRecord> cachedResourceRecords = null;

            var questionName = $"{question.Name} {question.Type}";

            if (cachedRecords.ContainsKey(questionName))
                cachedResourceRecords = cachedRecords[questionName];

            return cachedResourceRecords?.Select(cachedRecordValue => new ResourceRecord(
                                                     cachedRecordValue.Name, cachedRecordValue.Data,
                                                     cachedRecordValue.Type, cachedRecordValue.Class,
                                                     cachedRecordValue.TimeToLive));
        }

        private void LoadCache()
        {
            if (File.Exists(cacheFileName))
                try
                {
                    Console.WriteLine("Loading existing cache file...");

                    cachedRecords =
                        JsonConvert.DeserializeObject<Dictionary<string, HashSet<SerializableResourceRecord>>>(
                            File.ReadAllText(cacheFileName), jsonSerializerSettings);
                }
                catch (Exception exception) when (exception is JsonException || exception is IOException)
                {
                    Console.WriteLine("Detected problem with cache file. File will recreate.");
                }
            else
            {
                Console.WriteLine("Will create new cache file...");

                cachedRecords = new Dictionary<string, HashSet<SerializableResourceRecord>>();
            }
        }

        private static void SetUpJsonSerializerSettings(out JsonSerializerSettings serializerSettings)
        {
            serializerSettings = new JsonSerializerSettings { Formatting = Formatting.Indented };

            serializerSettings.Converters.Add(
                new Converter<IPAddress>(reader => IPAddress.Parse((string)reader.Value)));

            serializerSettings.Converters.Add(new Converter<Domain>(reader => new Domain(reader.Value.ToString())));

            serializerSettings.Converters.Add(new Converter<RecordClass>(reader =>
            {
                Enum.TryParse<RecordClass>(reader.Value.ToString(), out var result);
                return result;
            }));

            serializerSettings.Converters.Add(new Converter<RecordType>(reader =>
            {
                Enum.TryParse<RecordType>(reader.Value.ToString(), out var result);
                return result;
            }));
        }
    }
}