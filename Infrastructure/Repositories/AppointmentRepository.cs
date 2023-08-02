﻿using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using System.Globalization;

namespace Infrastructure.Repositories
{
    public class AppointmentRepository : GenericRepository<Appointments, AppointmentsFilter>, IAppointmentRepository
    {
        protected readonly string _tableStatus;

        public AppointmentRepository(IDBFactory context) : base(context)
        {
            _tableStatus = typeof(AppointmentsStatus).Name;
        }

        public async Task AddStatusRange(IEnumerable<AppointmentsStatus> entities, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            foreach (var item in entities)
            {
                var propertyNames = QueryGenerator.GetPropertyNames(item);
                var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
                var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

                var subquery = $"INSERT INTO {_tableStatus} ({columnNames}) VALUES ({parameterNames}) ";
                await _db.ExecuteAsync(subquery, item);
            }
        }

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetAllDetailList(string dbName, AppointmentDetailFilter filter)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            // Start building the SQL query
            var sqlQuery = $"SELECT a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed," +
                $" a.ServiceId, s.Name AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate," +
                $" s.DurationType AS DurationTypeEstimate, " +
                $"CASE WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)" +
                $" WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) ELSE NULL END AS EndDateEstimate " +
                $"FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId " +
                $"JOIN Patients p ON p.Id = a.PatientsId " +
                $"JOIN Services s ON s.Id = a.ServiceId " +
                $"JOIN Profile pr ON pr.Id = a.StaffId " +
                $"JOIN AppointmentsStatus st ON st.Id = a.StatusId ";

            // Check if any filter parameters are provided
            bool isFilterApplied = false;
            if (filter != null)
            {
                var whereClause = new List<string>();

                // Check and add StatusId filter
                if (filter.StatusId.HasValue)
                {
                    whereClause.Add($"a.StatusId = {filter.StatusId.Value}");
                    isFilterApplied = true;
                }

                // Check and add StaffId filter
                if (filter.StaffId.HasValue)
                {
                    whereClause.Add($"a.StaffId = {filter.StaffId.Value}");
                    isFilterApplied = true;
                }

                // Check and add Date filter
                if (!string.IsNullOrEmpty(filter.Date))
                {
                    // Parse the date range if it's in the format '[start] - [end]'
                    if (filter.Date.Contains("-"))
                    {
                        var dateRangeParts = filter.Date.Split('-');
                        if (dateRangeParts.Length == 2)
                        {
                            string startDateStr = dateRangeParts[0].Trim();
                            string endDateStr = dateRangeParts[1].Trim();

                            DateTime startDate, endDate;
                            if (DateTime.TryParseExact(startDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate) &&
                                DateTime.TryParseExact(endDateStr, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
                            {
                                whereClause.Add($"a.Date >= '{startDate:yyyy-MM-dd}' AND a.Date <= '{endDate:yyyy-MM-dd}'");
                                isFilterApplied = true;
                            }
                        }
                    }
                    else // Single date case
                    {
                        if (DateTime.TryParseExact(filter.Date, "dd MMMM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                        {
                            whereClause.Add($"a.Date = '{date:yyyy-MM-dd}'");
                            isFilterApplied = true;
                        }
                    }
                }

                // Combine all where conditions
                if (whereClause.Count > 0)
                {
                    sqlQuery += " WHERE " + string.Join(" AND ", whereClause);
                }
            }

            // Execute the SQL query
            return await _db.QueryAsync<AppointmentsDetailResponse>(sqlQuery);
        }

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetAllDetailListToday(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            DateTime currentDate = DateTime.Now.Date; // Get the current date with time component set to midnight (00:00:00)
            return await _db.QueryAsync<AppointmentsDetailResponse>($"SELECT a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed," +
                $" a.ServiceId, s.Name AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate," +
                $" s.DurationType AS DurationTypeEstimate, " +
                $"CASE WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)" +
                $" WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) ELSE NULL END AS EndDateEstimate " +
                $"FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId " +
                $"JOIN Patients p ON p.Id = a.PatientsId " +
                $"JOIN Services s ON s.Id = a.ServiceId " +
                $"JOIN Profile pr ON pr.Id = a.StaffId " +
                $"JOIN AppointmentsStatus st ON st.Id = a.StatusId " +
                $"WHERE DATE(a.Date) = @CurrentDate", new { CurrentDate = currentDate });
        }

        public async Task<AppointmentsDetailResponse> GetAllDetail(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstAsync<AppointmentsDetailResponse>($"SELECT a.OwnersId, o.Name AS OwnersName, o.Title AS OwnersTitle, a.PatientsId, p.Name AS PatientsName, p.Breed AS PatientsBreed," +
                $" a.ServiceId, s.Name AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate," +
                $" s.DurationType AS DurationTypeEstimate, " +
                $"CASE WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)" +
                $" WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) ELSE NULL END AS EndDateEstimate " +
                $"FROM Appointments a JOIN Owners o ON o.Id = a.OwnersId " +
                $"JOIN Patients p ON p.Id = a.PatientsId " +
                $"JOIN Services s ON s.Id = a.ServiceId " +
                $"JOIN Profile pr ON pr.Id = a.StaffId " +
                $"JOIN AppointmentsStatus st ON st.Id = a.StatusId " +
                $"WHERE a.Id = {id}");
        }

        public async Task<IEnumerable<AppointmentsStatus>> GetAllStatus(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<AppointmentsStatus>($"SELECT * FROM AppointmentsStatus");
        }

        public async Task<int> AddActivity(AppointmentsActivity entity, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);

            var propertyNames = QueryGenerator.GetPropertyNames(entity);

            var columnNames = string.Join(", ", propertyNames.Select(p => p.Name));
            var parameterNames = string.Join(", ", propertyNames.Select(p => $"@{p.Name}"));

            var query = $"INSERT INTO AppointmentsActivity ({columnNames}) VALUES ({parameterNames}); SELECT LAST_INSERT_ID();";
            return await _db.ExecuteScalarAsync<int>(query, entity);
        }
    }
}
