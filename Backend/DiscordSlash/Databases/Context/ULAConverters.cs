using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace DexterSlash.Databases.Context
{
    public class ULAConverter : ValueConverter<ulong[], string>
    {
        public ULAConverter() :
            base(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => ulong.Parse(x)).ToArray(),
                null
            )
        { }
    }

    public class ULAComparer : ValueComparer<ulong[]>
    {
        public ULAComparer()
            : base(
                (c1, c2) => c1.SequenceEqual(c2),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => (ulong[])c.Clone()
            )
        { }

    }
}
