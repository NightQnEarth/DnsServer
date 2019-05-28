using DNS.Protocol;

namespace DnsServer
{
    public struct CachedRecordKey
    {
        private readonly string resourceRecordName;
        private readonly RecordType resourceRecordType;

        public CachedRecordKey(string resourceRecordName, RecordType resourceRecordType)
        {
            this.resourceRecordName = resourceRecordName;
            this.resourceRecordType = resourceRecordType;
        }

        public override bool Equals(object obj) => base.Equals(obj);

        public bool Equals(CachedRecordKey other) => resourceRecordName.Equals(other.resourceRecordName) &&
                                                     resourceRecordType == other.resourceRecordType;

        public override int GetHashCode() => unchecked(
            ((resourceRecordName != null ? resourceRecordName.GetHashCode() : 0) * 397) ^ (int)resourceRecordType);
    }
}