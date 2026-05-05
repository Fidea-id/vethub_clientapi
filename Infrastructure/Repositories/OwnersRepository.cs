using Dapper;
using Domain.Entities.DTOs;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Newtonsoft.Json;

namespace Infrastructure.Repositories
{
    public class OwnersRepository : GenericRepository<Owners, OwnersFilter>, IOwnersRepository
    {
        public OwnersRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<DataResultDTO<OwnerListSpending>> GetOwnerWithSpending(OwnersSpendingFilter filters, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var whereClauses = new List<string>();
                var havingClauses = new List<string>();
                var param = new DynamicParameters();

                // 🔎 Base filter (Owner level)
                whereClauses.Add("o.IsActive = 1");

                if (!string.IsNullOrEmpty(filters.Name))
                {
                    whereClauses.Add("o.Name LIKE @Name");
                    param.Add("@Name", $"%{filters.Name}%");
                }

                if (!string.IsNullOrEmpty(filters.Email))
                {
                    whereClauses.Add("o.Email LIKE @Email");
                    param.Add("@Email", $"%{filters.Email}%");
                }

                if (!string.IsNullOrEmpty(filters.PhoneNumber))
                {
                    whereClauses.Add("o.PhoneNumber LIKE @PhoneNumber");
                    param.Add("@PhoneNumber", $"%{filters.PhoneNumber}%");
                }

                if (!string.IsNullOrEmpty(filters.Address))
                {
                    whereClauses.Add("o.Address LIKE @Address");
                    param.Add("@Address", $"%{filters.Address}%");
                }

                if (!string.IsNullOrEmpty(filters.Title))
                {
                    whereClauses.Add("o.Title LIKE @Title");
                    param.Add("@Title", $"%{filters.Title}%");
                }

                // 🔎 Global search
                if (!string.IsNullOrEmpty(filters.Search))
                {
                    whereClauses.Add(@"
                (o.Name LIKE @Search 
                OR o.Email LIKE @Search 
                OR o.PhoneNumber LIKE @Search)");
                    param.Add("@Search", $"%{filters.Search}%");
                }

                // 🔥 HAVING (aggregate filter)
                if (filters.TotalAppointments.HasValue)
                {
                    havingClauses.Add("COUNT(DISTINCT a.Id) = @TotalAppointments");
                    param.Add("@TotalAppointments", filters.TotalAppointments);
                }

                if (filters.TotalSpending.HasValue)
                {
                    havingClauses.Add("COALESCE(SUM(mr.Total),0) = @TotalSpending");
                    param.Add("@TotalSpending", filters.TotalSpending);
                }

                var whereSql = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";
                var havingSql = havingClauses.Any() ? "HAVING " + string.Join(" AND ", havingClauses) : "";

                var baseQuery = $@"
            SELECT 
                o.Id,
                o.Title,
                o.Name,
                o.PhoneNumber,
                o.Address,
                o.Email,
                o.IsActive,

                COUNT(DISTINCT a.Id) AS TotalAppointments,
                COALESCE(SUM(mr.Total), 0) AS TotalSpending

            FROM Owners o

            LEFT JOIN Appointments a 
                ON a.OwnersId = o.Id
                AND a.StatusId = 6
                AND a.IsActive = 1

            LEFT JOIN MedicalRecords mr 
                ON mr.AppointmentId = a.Id
                AND mr.PaymentStatus = 'Paid'
                AND mr.IsActive = 1

            {whereSql}

            GROUP BY 
                o.Id, o.Title, o.Name, o.PhoneNumber, o.Address, o.Email, o.IsActive

            {havingSql}
        ";

                // 🔢 COUNT
                var countQuery = $@"SELECT COUNT(*) FROM ({baseQuery}) x";
                var totalData = await _db.QueryFirstOrDefaultAsync<int>(countQuery, param);

                // 🔽 SORTING
                var sortProp = string.IsNullOrEmpty(filters.SortProp) ? "TotalSpending" : filters.SortProp;
                var sortMode = string.IsNullOrEmpty(filters.SortMode) ? "DESC" : filters.SortMode;

                var allowedSort = new[]
                {
            "Id","Name","Email","PhoneNumber","TotalAppointments","TotalSpending"
        };

                if (!allowedSort.Contains(sortProp))
                    sortProp = "TotalSpending";

                var query = baseQuery + $" ORDER BY {sortProp} {sortMode}";

                // 📄 PAGINATION
                if (filters.Take.HasValue || filters.Skip.HasValue)
                {
                    query += " LIMIT @Skip, @Take";
                    param.Add("@Skip", filters.Skip ?? 0);
                    param.Add("@Take", filters.Take ?? 10);
                }

                var data = await _db.QueryAsync<OwnerListSpending>(query, param);

                return new DataResultDTO<OwnerListSpending>
                {
                    Data = data,
                    TotalData = totalData
                };
            }
        }

        public async Task<CheckValidDTO> CheckOwnerPatientValidList(IEnumerable<BulkOwnerPatient> data, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var result = new CheckValidDTO();
                result.ValidationMessage = new List<string>();
                var listMessage = new List<string>();
                var dataFix = new List<BulkOwnerPatient>();

                var existingOwners = new HashSet<string>(); // To track unique Owners (Name + Email)
                var existingPatients = new HashSet<string>(); // To track unique Patients under each Owner

                foreach (var item in data)
                {
                    string ownerKey = $"{item.ownerName?.Trim().ToLower()}|{item.ownerEmail?.Trim().ToLower()}";
                    string patientKey = $"{ownerKey}|{item.patientName?.Trim().ToLower()}";

                    // --- Required Field Checks (based on non-nullable properties) ---
                    //if (string.IsNullOrEmpty(item.ownerTitle))
                    //    listMessage.Add($"Row {item.row}: Owner Title is required!");

                    if (string.IsNullOrEmpty(item.ownerName))
                        listMessage.Add($"Row {item.row}: Owner Name is required!");

                    //if (string.IsNullOrEmpty(item.ownerEmail))
                    //    listMessage.Add($"Row {item.row}: Owner Email is required!");

                    if (string.IsNullOrEmpty(item.patientName))
                        listMessage.Add($"Row {item.row}: Patient Name is required!");

                    if (string.IsNullOrEmpty(item.patientSpecies))
                        listMessage.Add($"Row {item.row}: Patient Species is required!");

                    if (string.IsNullOrEmpty(item.patientBreed))
                        listMessage.Add($"Row {item.row}: Patient Breed is required!");

                    if (string.IsNullOrEmpty(item.ownerPhone))
                        listMessage.Add($"Row {item.row}: Owner Phone Number is required!");

                    // --- Duplicate Checks ---
                    if (dataFix.Any(x => x.row == item.row))
                        listMessage.Add($"Row {item.row}: Duplicate row detected!");

                    //// Check if Owner already exists in the uploaded file
                    //if (existingOwners.Contains(ownerKey))
                    //    listMessage.Add($"Row {item.row}: Duplicate Owner detected in this file (Same Name & Email)!");
                    //else
                    //    existingOwners.Add(ownerKey);

                    // Check if Patient already exists under the same Owner in the uploaded file
                    if (existingPatients.Contains(patientKey))
                        listMessage.Add($"Row {item.row}: Duplicate Patient detected under the same Owner!");
                    else
                        existingPatients.Add(patientKey);

                    var patientExists = await _db.ExecuteScalarAsync<bool>(
                        "SELECT COUNT(1) FROM Patients p " +
                        "JOIN Owners o ON p.OwnersId = o.Id " +
                        "WHERE LOWER(o.Name) = @OwnerName AND LOWER(o.PhoneNumber) = @OwnerPhone " +
                        "AND LOWER(p.Name) = @PatientName",
                        new
                        {
                            OwnerName = item.ownerName.Trim().ToLower(),
                            OwnerEmail = item.ownerEmail.Trim().ToLower(),
                            PatientName = item.patientName.Trim().ToLower()
                        }
                    );

                    if (patientExists)
                        listMessage.Add($"Row {item.row}: Patient already exists under this Owner in the database!");

                    dataFix.Add(item);
                }

                // --- Response Handling ---
                if (listMessage.Count > 0)
                {
                    result.ValidationMessage = listMessage;
                    result.Data = null;
                    result.Status = 400;
                    result.Message = "One or more fields in the template are invalid.";
                }
                else
                {
                    result.ValidationMessage = null;
                    result.Data = JsonConvert.SerializeObject(dataFix);
                    result.Status = 200;
                    result.Message = "Valid";
                }

                return result;
            }
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetOwnerChart(string dbName, string dateFilter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var filterQuery = "YEAR(CreatedAt) = YEAR(CURRENT_DATE()) AND CreatedAt <= CURRENT_DATE() AND IsActive = 1";
                if (dateFilter != null)
                {
                    filterQuery = dateFilter;
                }
                string query = @"SELECT
                                DATE_FORMAT(CreatedAt, '%d') AS Date,
                                DATE_FORMAT(CreatedAt, '%m') AS Month,
                                DATE_FORMAT(CreatedAt, '%Y') AS Year,
                                COUNT(*) AS Total
                            FROM
                                Owners
                            WHERE 
            ";
                query += filterQuery;
                query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
                return await _db.QueryAsync<MonthlyDataChart>(query);
            }
        }

        public async Task<Owners> ReadByPatientIdAsync(int id, string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                return await _db.QueryFirstOrDefaultAsync<Owners>($"SELECT O.* FROM Owners O JOIN Patients P ON P.OwnersId = O.Id WHERE P.IsActive = true AND O.IsActive = 1 AND P.Id = {id}");
            }
        }

    }
}
