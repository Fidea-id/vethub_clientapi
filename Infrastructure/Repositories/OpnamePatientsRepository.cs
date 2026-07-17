using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;

namespace Infrastructure.Repositories
{
    public class OpnamePatientsRepository : GenericRepository<OpnamePatients, OpnamePatientsFilter>, IOpnamePatientsRepository
    {
        public OpnamePatientsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<DataResultDTO<OpnamePatients>> GetByMedId(string dbName, int id)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryAsync<OpnamePatients>($"SELECT * FROM OpnamePatients WHERE MedicalRecordId = @Id AND IsActive = 1", new { Id = id });
                var result = new DataResultDTO<OpnamePatients>
                {
                    Data = data,
                    TotalData = data.Count()
                };
                return result;
            }
        }

        public async Task<DataResultDTO<OpnamePatients>> GetByOpnameId(string dbName, int id)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryAsync<OpnamePatients>($"SELECT * FROM OpnamePatients WHERE OpnameId = @Id AND IsActive = 1", new { Id = id });
                var result = new DataResultDTO<OpnamePatients>
                {
                    Data = data,
                    TotalData = data.Count()
                };
                return result;
            }
        }

        public async Task<DataResultDTO<OpnamePatientsDetailResponse>> GetDetailList(string dbName, OpnamePatientsFilter filter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var parameters = new DynamicParameters();
                var whereClause = new List<string>
                {
                    "op.IsActive = 1",
                    "mr.IsActive = 1",
                    "a.IsActive = 1",
                    "p.IsActive = 1",
                    "o.IsActive = 1"
                };

                if (filter?.Id.HasValue == true)
                {
                    whereClause.Add("op.Id = @Id");
                    parameters.Add("Id", filter.Id.Value);
                }

                if (filter?.MedicalRecordId.HasValue == true)
                {
                    whereClause.Add("op.MedicalRecordId = @MedicalRecordId");
                    parameters.Add("MedicalRecordId", filter.MedicalRecordId.Value);
                }

                if (filter?.OpnameId.HasValue == true)
                {
                    whereClause.Add("op.OpnameId = @OpnameId");
                    parameters.Add("OpnameId", filter.OpnameId.Value);
                }

                var statusFilter = filter?.Status?.Trim();
                var normalizedStatus = string.IsNullOrWhiteSpace(statusFilter)
                    ? "Active"
                    : statusFilter;

                if (string.Equals(normalizedStatus, "All", StringComparison.OrdinalIgnoreCase))
                {
                    // no status filter
                }
                else if (string.Equals(normalizedStatus, "Done", StringComparison.OrdinalIgnoreCase))
                {
                    whereClause.Add("op.Status = 'Done'");
                }
                else if (string.Equals(normalizedStatus, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    whereClause.Add("COALESCE(op.Status, '') <> 'Done'");
                }
                else
                {
                    whereClause.Add("op.Status = @Status");
                    parameters.Add("Status", normalizedStatus);
                }

                if (!string.IsNullOrWhiteSpace(filter?.Search))
                {
                    whereClause.Add("(o.Name LIKE @Search OR p.Name LIKE @Search OR mr.Code LIKE @Search)");
                    parameters.Add("Search", $"%{filter.Search}%");
                }

                var pagingTake = filter?.Take > 0 ? filter.Take.Value : 10;
                var pagingSkip = filter?.Skip > 0 ? filter.Skip.Value : 0;
                parameters.Add("Take", pagingTake);
                parameters.Add("Skip", pagingSkip);

                var countSql = $@"
                    SELECT COUNT(1)
                    FROM OpnamePatients op
                    INNER JOIN MedicalRecords mr ON mr.Id = op.MedicalRecordId
                    INNER JOIN Appointments a ON a.Id = mr.AppointmentId
                    INNER JOIN Patients p ON p.Id = a.PatientsId
                    INNER JOIN Opnames o ON o.Id = op.OpnameId
                    WHERE {string.Join(" AND ", whereClause)}";

                var totalData = await _db.ExecuteScalarAsync<int>(countSql, parameters);

                var query = $@"
                    SELECT
                        op.Id AS Id,
                        p.Name AS PatientName,
                        op.MedicalRecordId AS MedicalRecordId,
                        p.Id AS PatientId,
                        a.Id AS AppointmentId,
                        op.OpnameId AS OpnameId,
                        o.Name AS OpnameName,
                        op.StartTime AS StartTime,
                        op.EndTime AS EndTime,
                        op.EstimateDays AS EstimatedDays,
                        op.Status AS Status,
                        op.Price AS Price,
                        op.TotalPrice AS TotalPrice
                    FROM OpnamePatients op
                    INNER JOIN MedicalRecords mr ON mr.Id = op.MedicalRecordId
                    INNER JOIN Appointments a ON a.Id = mr.AppointmentId
                    INNER JOIN Patients p ON p.Id = a.PatientsId
                    INNER JOIN Opnames o ON o.Id = op.OpnameId
                    WHERE {string.Join(" AND ", whereClause)}
                    ORDER BY op.Id DESC
                    LIMIT @Take OFFSET @Skip;";

                var data = await _db.QueryAsync<OpnamePatientsDetailResponse>(query, parameters);

                return new DataResultDTO<OpnamePatientsDetailResponse>
                {
                    Data = data,
                    TotalData = totalData
                };
            }
        }
    }
}
