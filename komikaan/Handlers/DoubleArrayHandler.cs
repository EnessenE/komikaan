using Dapper;
using System.Data;

namespace komikaan.Handlers;

/// <summary>
/// Dapper extension of the double[] type some of our functions return
/// </summary>
/// I personally find it bad practice to return arrays like this
/// But honestly I can justify so it so meh
public class DoubleArrayHandler : SqlMapper.TypeHandler<double[][]>
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "AV1706:Identifier contains an abbreviation or is too short", Justification = "TODO")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Naming", "AV1532", Justification = "TODO")]
    public override double[][] Parse(object value)
    {
        var array = (double[,])value;
        int rows = array.GetLength(0);
        int cols = array.GetLength(1);

        var result = new double[rows][];
        for (int i = 0; i < rows; i++)
        {
            result[i] = new double[cols];
            for (int j = 0; j < cols; j++)
            {
                result[i][j] = array[i, j];
            }
        }

        return result;
    }

    public override void SetValue(IDbDataParameter parameter, double[][]? value)
    {
        // Not needed unless you are inserting/updating arrays in the database
        throw new NotImplementedException();
    }
}
