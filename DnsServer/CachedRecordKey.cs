using DNS.Protocol;

namespace DnsServer
{
    public struct CachedRecordKey
    {
        public readonly string ResourceRecordName;
        public readonly RecordType ResourceRecordType;

        public CachedRecordKey(Domain resourceRecordName, RecordType resourceRecordType)
        {
            ResourceRecordName = resourceRecordName.ToString().ToLower();
            ResourceRecordType = resourceRecordType;
        }

        public override bool Equals(object obj) =>
            obj is CachedRecordKey cachedRecordKey && cachedRecordKey.Equals(this);

        // ReSharper disable once MemberCanBePrivate.Global
        public bool Equals(CachedRecordKey other) => ResourceRecordName.Equals(other.ResourceRecordName) &&
                                                     ResourceRecordType == other.ResourceRecordType;

        public override int GetHashCode() => unchecked(
            ((ResourceRecordName != null ? ResourceRecordName.GetHashCode() : 0) * 397) ^ (int)ResourceRecordType);
    }
}