using Dapper;
using System.Data;
using System.Diagnostics.CodeAnalysis;

namespace komikaan.Handlers
{
    /// <summary>
    /// NPGSQL is able to handle both types normally, dapper just cant
    /// </summary>
    public class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override DateOnly Parse(object value)
        {
            if (value is DateOnly dateOnly)
            {
                return dateOnly;
            }

            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }

            return DateOnly.FromDateTime(Convert.ToDateTime(value));
        }

        public override void SetValue([DisallowNull] IDbDataParameter parameter, DateOnly value)
        {
            // Npgsql handles DateOnly natively now
            parameter.Value = value;
            parameter.DbType = DbType.Date;
        }
    }


    public class SqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly time)
        {
            parameter.Value = time;
            parameter.DbType = DbType.Time;
        }

        public override TimeOnly Parse(object value)
        {
            if (value is TimeOnly timeOnly)
            {
                return timeOnly;
            }

            if (value is TimeSpan timeSpan)
            {
                return TimeOnly.FromTimeSpan(timeSpan);
            }

            return TimeOnly.FromDateTime(Convert.ToDateTime(value));
        }
    }
}
