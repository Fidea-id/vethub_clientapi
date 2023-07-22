using Dapper;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Requests.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using Infrastructure.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<IEnumerable<AppointmentsDetailResponse>> GetAllDetailList(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<AppointmentsDetailResponse>($"SELECT a.OwnersId, o.Name AS OwnersName, a.PatientsId, p.Name AS PatientsName," +
                $" a.ServiceId, s.Name AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate," +
                $" s.DurationType AS DurationTypeEstimate, " +
                $"CASE WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)" +
                $" WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) ELSE NULL END AS EndDateEstimate " +
                $"FROM appointments a JOIN owners o ON o.Id = a.OwnersId " +
                $"JOIN patients p ON p.Id = a.PatientsId " +
                $"JOIN services s ON s.Id = a.ServiceId " +
                $"JOIN profile pr ON pr.Id = a.StaffId " +
                $"JOIN appointmentsstatus st ON st.Id = a.StatusId ");
        }

        public async Task<AppointmentsDetailResponse> GetAllDetail(int id, string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryFirstAsync<AppointmentsDetailResponse>($"SELECT a.OwnersId, o.Name AS OwnersName, a.PatientsId, p.Name AS PatientsName," +
                $" a.ServiceId, s.Name AS ServiceName, a.StaffId, pr.Name AS StaffName, a.StatusId, st.Name AS StatusName, a.Notes, a.Date, s.Duration AS DurationEstimate," +
                $" s.DurationType AS DurationTypeEstimate, " +
                $"CASE WHEN s.DurationType = 'Minutes' THEN DATE_ADD(a.Date, INTERVAL s.Duration MINUTE) WHEN s.DurationType = 'Hours' THEN DATE_ADD(a.Date, INTERVAL s.Duration HOUR)" +
                $" WHEN s.DurationType = 'Days' THEN DATE_ADD(a.Date, INTERVAL s.Duration DAY) ELSE NULL END AS EndDateEstimate " +
                $"FROM appointments a JOIN owners o ON o.Id = a.OwnersId " +
                $"JOIN patients p ON p.Id = a.PatientsId " +
                $"JOIN services s ON s.Id = a.ServiceId " +
                $"JOIN profile pr ON pr.Id = a.StaffId " +
                $"JOIN appointmentsstatus st ON st.Id = a.StatusId " +
                $"WHERE a.Id = {id}");
        }

        public async Task<IEnumerable<AppointmentsStatus>> GetAllStatus(string dbName)
        {
            var _db = _dbFactory.GetDbConnection(dbName);
            return await _db.QueryAsync<AppointmentsStatus>($"SELECT * FROM AppointmentsStatus");
        }
    }
}
