using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using Microsoft.Extensions.Configuration;

namespace WaterCounter.Server.ComPortReader.Repository
{
    internal class RawDataRepository : IRawDataRepository
    {
        private readonly IDbConnection _dbConnection;

        public RawDataRepository(IConfiguration configuration)
        {
            _dbConnection = new SqlConnection(configuration.GetConnectionString("DatabaseConnection"));
        }

        public long Add(int counterNumber)
        {
            const string sql = @"INSERT INTO [dbo].[RawData] ([Time], [WaterCounterNumber]) VALUES (@Time, @Number)
                                 SELECT CAST(SCOPE_IDENTITY() as bigint)";
            var id = _dbConnection.Query<long>(sql, new
            {
                Time = DateTime.Now,
                Number = counterNumber
            }).Single();
            return id;
        }
    }
}