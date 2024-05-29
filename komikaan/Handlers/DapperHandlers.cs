using Dapper;
using System.Data;

namespace komikaan.Handlers
{
    /// <summary>
    /// NPGSQL is able to handle both types normally, dapper just cant
    /// </summary>
    public class SqlDateOnlyTypeHandler : SqlMapper.TypeHandler<DateOnly>
    {
        public override void SetValue(IDbDataParameter parameter, DateOnly date)
        {
            parameter.Value = date;
            parameter.DbType = DbType.Date;
        }

        public override DateOnly Parse(object value) => DateOnly.FromDateTime((DateTime)value);
    }

    public class SqlTimeOnlyTypeHandler : SqlMapper.TypeHandler<TimeOnly>
    {
        public override void SetValue(IDbDataParameter parameter, TimeOnly time)
        {
            parameter.Value = time;
            parameter.DbType = DbType.Time;
        }

        public override TimeOnly Parse(object value) => TimeOnly.FromTimeSpan((TimeSpan)value);
    }
}
