using Dapper;
using Domain.Entities.DTOs.Clients;
using Domain.Entities.Filters.Clients;
using Domain.Entities.Models.Clients;
using Domain.Entities.Responses.Clients;
using Domain.Interfaces.Clients;
using Infrastructure.Data;
using System.Globalization;

namespace Infrastructure.Repositories
{
    public class MedicalRecordsRepository : GenericRepository<MedicalRecords, MedicalRecordsFilter>, IMedicalRecordsRepository
    {
        public MedicalRecordsRepository(IDBFactory context) : base(context)
        {
        }

        public async Task<MedicalRecords> GetByAppointmentId(string dbName, int appointmentId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var data = await _db.QueryFirstAsync<MedicalRecords>($"SELECT * FROM MedicalRecords WHERE AppointmentId = @Id AND IsActive = 1", new { Id = appointmentId });
                return data;
            }
        }

        public async Task<IEnumerable<MedicalRecordsDetailResponse>> GetDetailList(string dbName, string flag)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
                -- Medical Records
                SELECT * FROM MedicalRecords WHERE IsActive = 1;

                -- Appointments
                SELECT * FROM Appointments 
                WHERE Id IN (SELECT AppointmentId FROM MedicalRecords WHERE IsActive = 1) 
                AND IsActive = 1;

                -- Services
                SELECT * FROM Services 
                WHERE Id IN (SELECT ServiceId FROM Appointments 
                            WHERE Id IN (SELECT AppointmentId FROM MedicalRecords WHERE IsActive = 1) 
                            AND IsActive = 1) 
                AND IsActive = 1;

                -- Staff
                SELECT * FROM Profile 
                WHERE Id IN (SELECT StaffId FROM Appointments WHERE IsActive = 1);

                -- Patients
                SELECT * FROM Patients 
                WHERE Id IN (SELECT PatientId FROM MedicalRecords WHERE IsActive = 1);

                -- Owners
                SELECT * FROM Owners 
                WHERE Id IN (SELECT OwnersId FROM Patients 
                            WHERE Id IN (SELECT PatientId FROM MedicalRecords WHERE IsActive = 1) 
                            AND IsActive = 1);

                -- Notes
                SELECT * FROM MedicalRecordsNotes 
                WHERE MedicalRecordsId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                AND IsActive = 1 AND (@flag != 'no_notes' OR @flag IS NULL);

                -- Diagnoses
                SELECT * FROM MedicalRecordsDiagnoses 
                WHERE MedicalRecordsId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                AND IsActive = 1;

                -- Prescriptions
                SELECT * FROM MedicalRecordsPrescriptions 
                WHERE MedicalRecordsId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                AND IsActive = 1;

                -- Payments
                SELECT * FROM OrdersPayment 
                WHERE OrderId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                AND Type = 'MedicalRecord' AND IsActive = 1;

                -- Payment Methods
                SELECT * FROM PaymentMethod 
                WHERE Id IN (
                    SELECT PaymentMethodId FROM OrdersPayment 
                    WHERE OrderId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                    AND Type = 'MedicalRecord' AND IsActive = 1
                );

                -- Opname Patients
                SELECT * FROM OpnamePatients 
                WHERE MedicalRecordId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                AND IsActive = 1;

                -- Opnames
                SELECT * FROM Opnames 
                WHERE Id IN (SELECT OpnameId FROM OpnamePatients 
                            WHERE MedicalRecordId IN (SELECT Id FROM MedicalRecords WHERE IsActive = 1) 
                            AND IsActive = 1) 
                AND IsActive = 1;
            ";

                using var multi = await _db.QueryMultipleAsync(sql, new { flag });

                // Read all data sets
                var medicalRecords = await multi.ReadAsync<MedicalRecords>();
                var appointments = await multi.ReadAsync<Appointments>();
                var services = await multi.ReadAsync<Services>();
                var staff = await multi.ReadAsync<Profile>();
                var patients = await multi.ReadAsync<Patients>();
                var owners = await multi.ReadAsync<Owners>();
                var notes = await multi.ReadAsync<MedicalRecordsNotes>();
                var diagnoses = await multi.ReadAsync<MedicalRecordsDiagnoses>();
                var prescriptions = await multi.ReadAsync<MedicalRecordsPrescriptions>();
                var payments = await multi.ReadAsync<OrdersPayment>();
                var paymentMethods = await multi.ReadAsync<PaymentMethod>();
                var opnamePatients = await multi.ReadAsync<OpnamePatients>();
                var opnames = await multi.ReadAsync<Opnames>();

                // Convert to dictionaries for faster lookup
                var appointmentsDict = appointments.ToDictionary(a => a.Id);
                var servicesDict = services.ToDictionary(s => s.Id);
                var staffDict = staff.ToDictionary(s => s.Id);
                var patientsDict = patients.ToDictionary(p => p.Id);
                var ownersDict = owners.ToDictionary(o => o.Id);
                var notesDict = notes.GroupBy(n => n.MedicalRecordsId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                var diagnosesDict = diagnoses.GroupBy(d => d.MedicalRecordsId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                var prescriptionsDict = prescriptions.GroupBy(p => p.MedicalRecordsId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                var paymentsDict = payments.GroupBy(p => p.OrderId).ToDictionary(g => g.Key, g => g.AsEnumerable());
                var paymentMethodsDict = paymentMethods.ToDictionary(pm => pm.Id);
                var opnamePatientsDict = opnamePatients.GroupBy(op => op.MedicalRecordId).ToDictionary(g => g.Key, g => g.FirstOrDefault());
                var opnamesDict = opnames.ToDictionary(o => o.Id);

                // Build response list
                var result = new List<MedicalRecordsDetailResponse>();

                foreach (var medicalRecord in medicalRecords)
                {
                    // Get related appointment
                    appointmentsDict.TryGetValue(medicalRecord.AppointmentId, out var appointment);

                    // Get related service
                    Services service = null;
                    if (appointment != null)
                    {
                        servicesDict.TryGetValue(appointment.ServiceId, out service);
                    }

                    // Get related entities
                    staffDict.TryGetValue(medicalRecord.StaffId, out var staffMember);
                    patientsDict.TryGetValue(medicalRecord.PatientId, out var patient);

                    Owners owner = null;
                    if (patient != null)
                    {
                        ownersDict.TryGetValue(patient.OwnersId, out owner);
                    }

                    // Get collections for this medical record
                    notesDict.TryGetValue(medicalRecord.Id, out var recordNotes);
                    diagnosesDict.TryGetValue(medicalRecord.Id, out var recordDiagnoses);
                    prescriptionsDict.TryGetValue(medicalRecord.Id, out var recordPrescriptions);
                    paymentsDict.TryGetValue(medicalRecord.Id, out var recordPayments);

                    // Calculate payment information
                    var totalLastPayment = recordPayments?.Sum(p => p.Total) ?? 0;
                    string statusPayment = recordPayments?.Any() == true
                        ? (totalLastPayment < medicalRecord.Total ? "Paid Less" : "Paid")
                        : "Unpaid";

                    string paymentMethodName = "No Payment Yet";
                    if (recordPayments?.Any() == true)
                    {
                        var firstPayment = recordPayments.First();
                        if (paymentMethodsDict.TryGetValue(firstPayment.PaymentMethodId, out var paymentMethod))
                        {
                            paymentMethodName = paymentMethod.Name;
                        }
                    }

                    // Get opname detail
                    OpnameDetailResponse opnameDetail = null;
                    if (opnamePatientsDict.TryGetValue(medicalRecord.Id, out var dataOpnamePatient))
                    {
                        if (opnamesDict.TryGetValue(dataOpnamePatient.OpnameId, out var opname))
                        {
                            opnameDetail = new OpnameDetailResponse
                            {
                                OpnameName = opname.Name,
                                StartTime = dataOpnamePatient.StartTime,
                                EstimatedDays = dataOpnamePatient.EstimateDays,
                                EndTime = dataOpnamePatient.EndTime,
                                OpnameId = dataOpnamePatient.OpnameId,
                                OpnamePatientsId = dataOpnamePatient.Id,
                                Price = dataOpnamePatient.Price,
                                Status = dataOpnamePatient.Status,
                                TotalPrice = dataOpnamePatient.TotalPrice
                            };
                        }
                    }

                    // Create response object
                    var response = new MedicalRecordsDetailResponse
                    {
                        Id = medicalRecord.Id,
                        Code = medicalRecord.Code,
                        Appointments = appointment,
                        Patients = patient,
                        Services = service,
                        Owners = owner,
                        Staff = staffMember,
                        StartDate = medicalRecord.StartDate,
                        DiscountMethod = medicalRecord.DiscountMethod,
                        DiscountValue = medicalRecord.DiscountValue,
                        DiscountTotal = medicalRecord.DiscountTotal,
                        TotalDiscounted = medicalRecord.TotalDiscounted,
                        EndDate = medicalRecord.EndDate ?? DateTime.MinValue,
                        TotalPrice = medicalRecord.Total,
                        TotalPaid = totalLastPayment,
                        Prescriptions = recordPrescriptions ?? Enumerable.Empty<MedicalRecordsPrescriptions>(),
                        Diagnoses = recordDiagnoses ?? Enumerable.Empty<MedicalRecordsDiagnoses>(),
                        StatusPayment = statusPayment,
                        Notes = recordNotes ?? Enumerable.Empty<MedicalRecordsNotes>(),
                        PaymentMethod = paymentMethodName,
                        OpnameDetail = opnameDetail
                    };

                    result.Add(response);
                }

                return result;
            }
        }

        public async Task<MedicalRecordsDetailResponse> GetDetailById(string dbName, int medicalRecordId, string flag)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
                -- Medical Record
                SELECT * FROM MedicalRecords WHERE Id = @medicalRecordId AND IsActive = 1;

                -- Appointment
                SELECT * FROM Appointments 
                WHERE Id = (SELECT AppointmentId FROM MedicalRecords WHERE Id = @medicalRecordId AND IsActive = 1) 
                AND IsActive = 1;

                -- Service
                SELECT * FROM Services 
                WHERE Id = (SELECT ServiceId FROM Appointments 
                            WHERE Id = (SELECT AppointmentId FROM MedicalRecords WHERE Id = @medicalRecordId AND IsActive = 1) 
                            AND IsActive = 1) 
                AND IsActive = 1;

                -- Staff
                SELECT p.*
                FROM Profile p
                INNER JOIN Appointments a ON p.Id = a.StaffId
                INNER JOIN MedicalRecords mr ON a.Id = mr.AppointmentId
                WHERE mr.Id = @medicalRecordId
                  AND mr.IsActive = 1
                  AND a.IsActive = 1;

                -- Patient and Owner
                SELECT * FROM Patients 
                WHERE Id = (SELECT PatientId FROM MedicalRecords WHERE Id = @medicalRecordId AND IsActive = 1);

                SELECT * FROM Owners 
                WHERE Id = (SELECT OwnersId FROM Patients 
                            WHERE Id = (SELECT PatientId FROM MedicalRecords WHERE Id = @medicalRecordId AND IsActive = 1) 
                            AND IsActive = 1);

                -- Notes (selalu return, data bisa kosong)
                SELECT * FROM MedicalRecordsNotes 
                WHERE MedicalRecordsId = @medicalRecordId AND IsActive = 1 AND (@flag != 'no_notes' OR @flag IS NULL);

                -- Diagnoses
                SELECT * FROM MedicalRecordsDiagnoses WHERE MedicalRecordsId = @medicalRecordId AND IsActive = 1;

                -- Prescriptions
                SELECT * FROM MedicalRecordsPrescriptions WHERE MedicalRecordsId = @medicalRecordId AND IsActive = 1;

                -- Payments
                SELECT * FROM OrdersPayment 
                WHERE OrderId = @medicalRecordId AND Type = 'MedicalRecord' AND IsActive = 1;

                -- Payment Method
                SELECT * FROM PaymentMethod 
                WHERE Id IN (
                    SELECT PaymentMethodId FROM OrdersPayment 
                    WHERE OrderId = @medicalRecordId AND Type = 'MedicalRecord' AND IsActive = 1
                );

                -- Opname Patients
                SELECT * FROM OpnamePatients 
                WHERE MedicalRecordId = @medicalRecordId AND IsActive = 1;

                -- Opnames
                SELECT * FROM Opnames 
                WHERE Id = (SELECT OpnameId FROM OpnamePatients WHERE MedicalRecordId = @medicalRecordId AND IsActive = 1 LIMIT 1) 
                AND IsActive = 1;
            ";

                using var multi = await _db.QueryMultipleAsync(sql, new { medicalRecordId, flag });

                var medicalRecord = await multi.ReadSingleOrDefaultAsync<MedicalRecords>();
                if (medicalRecord == null) return null;

                var appointment = await multi.ReadSingleOrDefaultAsync<Appointments>();
                var service = await multi.ReadSingleOrDefaultAsync<Services>();
                var staff = await multi.ReadSingleOrDefaultAsync<Profile>();
                var patient = await multi.ReadSingleOrDefaultAsync<Patients>();
                var owner = await multi.ReadSingleOrDefaultAsync<Owners>();
                var notes = await multi.ReadAsync<MedicalRecordsNotes>();

                var diagnoses = await multi.ReadAsync<MedicalRecordsDiagnoses>();
                var prescriptions = await multi.ReadAsync<MedicalRecordsPrescriptions>();

                var payments = await multi.ReadAsync<OrdersPayment>();
                var totalLastPayment = payments.Sum(p => p.Total);
                string statusPayment = payments.Any() ? (totalLastPayment < medicalRecord.Total ? "Paid Less" : "Paid") : "Unpaid";

                var paymentMethods = await multi.ReadAsync<PaymentMethod>();
                var paymentMethodName = paymentMethods.Any()
                    ? paymentMethods.First().Name
                    : "No Payment Yet";

                var opnamePatients = await multi.ReadAsync<OpnamePatients>();
                var dataOpnamePatient = opnamePatients.FirstOrDefault();
                OpnameDetailResponse opnameDetail = null;

                if (dataOpnamePatient != null)
                {
                    var opname = await multi.ReadSingleOrDefaultAsync<Opnames>();
                    opnameDetail = new OpnameDetailResponse
                    {
                        OpnameName = opname?.Name,
                        StartTime = dataOpnamePatient.StartTime,
                        EstimatedDays = dataOpnamePatient.EstimateDays,
                        EndTime = dataOpnamePatient.EndTime,
                        OpnameId = dataOpnamePatient.OpnameId,
                        OpnamePatientsId = dataOpnamePatient.Id,
                        Price = dataOpnamePatient.Price,
                        Status = dataOpnamePatient.Status,
                        TotalPrice = dataOpnamePatient.TotalPrice
                    };
                }

                return new MedicalRecordsDetailResponse
                {
                    Id = medicalRecord.Id,
                    Code = medicalRecord.Code,
                    Appointments = appointment,
                    Patients = patient,
                    Services = service,
                    Owners = owner,
                    Staff = staff,
                    StartDate = medicalRecord.StartDate,
                    DiscountMethod = medicalRecord.DiscountMethod,
                    DiscountValue = medicalRecord.DiscountValue,
                    DiscountTotal = medicalRecord.DiscountTotal,
                    TotalDiscounted = medicalRecord.TotalDiscounted,
                    EndDate = medicalRecord.EndDate ?? DateTime.MinValue,
                    TotalPrice = medicalRecord.Total,
                    TotalPaid = totalLastPayment,
                    Prescriptions = prescriptions,
                    Diagnoses = diagnoses,
                    StatusPayment = statusPayment,
                    Notes = notes,
                    PaymentMethod = paymentMethodName,
                    OpnameDetail = opnameDetail
                };
            }
        }

        public async Task<PharmacyMedicalRecordDetailResponse> GetPharmacyDetailById(string dbName, int medicalRecordId)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
                SELECT
                    mr.Id,
                    mr.Code,
                    mr.StartDate,
                    mr.EndDate,
                    a.Id AS AppointmentId,
                    s.Id AS ServiceId,
                    s.Name AS ServiceName,
                    s.Price AS ServicePrice,
                    p.Id AS StaffId,
                    p.Name AS StaffName,
                    pat.Id AS PatientId,
                    pat.Name AS PatientName,
                    pat.Species,
                    pat.Breed,
                    pat.Color,
                    o.Id AS OwnerId,
                    o.Name AS OwnerName,
                    o.Title AS OwnerTitle
                FROM MedicalRecords mr
                INNER JOIN Appointments a ON a.Id = mr.AppointmentId AND a.IsActive = 1
                LEFT JOIN Services s ON s.Id = a.ServiceId AND s.IsActive = 1
                LEFT JOIN Profile p ON p.Id = a.StaffId AND p.IsActive = 1
                LEFT JOIN Patients pat ON pat.Id = mr.PatientId AND pat.IsActive = 1
                LEFT JOIN Owners o ON o.Id = pat.OwnersId AND o.IsActive = 1
                WHERE mr.Id = @medicalRecordId AND mr.IsActive = 1;

                SELECT
                    mrp.ProductId,
                    mrp.ProductName,
                    mrp.PrescriptionFrequency,
                    mrp.Type,
                    mrp.MixId,
                    mrp.MixName,
                    mrp.PrescriptionAmount,
                    mrp.Price,
                    mrp.Quantity,
                    mrp.Total,
                    ps.VolumeUnit AS ProductVolumeUnit
                FROM MedicalRecordsPrescriptions mrp
                LEFT JOIN (
                    SELECT ProductId, MAX(VolumeUnit) AS VolumeUnit
                    FROM ProductStocks
                    WHERE IsActive = 1
                    GROUP BY ProductId
                ) ps ON ps.ProductId = mrp.ProductId
                WHERE mrp.MedicalRecordsId = @medicalRecordId AND mrp.IsActive = 1
                ORDER BY mrp.Id;
            ";

                using var multi = await _db.QueryMultipleAsync(sql, new { medicalRecordId });

                var detail = await multi.ReadSingleOrDefaultAsync<PharmacyMedicalRecordDetailRow>();
                if (detail == null)
                {
                    return null;
                }

                var prescriptions = await multi.ReadAsync<PharmacyPrescriptionItemResponse>();

                return new PharmacyMedicalRecordDetailResponse
                {
                    Id = detail.Id,
                    Code = detail.Code,
                    StartDate = detail.StartDate,
                    EndDate = detail.EndDate,
                    Appointments = new PharmacyAppointmentSummaryResponse
                    {
                        Id = detail.AppointmentId
                    },
                    Services = detail.ServiceId.HasValue
                        ? new PharmacyServiceSummaryResponse
                        {
                            Id = detail.ServiceId.Value,
                            Name = detail.ServiceName,
                            Price = detail.ServicePrice
                        }
                        : null,
                    Staff = detail.StaffId.HasValue
                        ? new PharmacyStaffSummaryResponse
                        {
                            Id = detail.StaffId.Value,
                            Name = detail.StaffName
                        }
                        : null,
                    Patients = detail.PatientId.HasValue
                        ? new PharmacyPatientSummaryResponse
                        {
                            Id = detail.PatientId.Value,
                            Name = detail.PatientName,
                            Species = detail.Species,
                            Breed = detail.Breed,
                            Color = detail.Color
                        }
                        : null,
                    Owners = detail.OwnerId.HasValue
                        ? new PharmacyOwnerSummaryResponse
                        {
                            Id = detail.OwnerId.Value,
                            Name = detail.OwnerName,
                            Title = detail.OwnerTitle
                        }
                        : null,
                    Prescriptions = prescriptions
                };
            }
        }

        public async Task<string> GetLatestCode(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                string query = "SELECT Code FROM MedicalRecords Where IsActive = 1 ORDER BY Id DESC";
                return await _db.QueryFirstOrDefaultAsync<string>(query);
            }
        }

        public async Task<List<MedicalRecordServicesReportDto>> GetMedicalRecordServicesReportAsync(
        string dbName,
        string? startDate = null,
        string? endDate = null,
        int? staffId = null,
        bool? onlyModifiedPrices = null)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
            SELECT 
                mr.Code AS MedicalRecordCode,
                mr.StartDate AS Date,
                p.Name AS StaffName,
                s.Name AS ServiceName,
                s.Price AS ServiceBasePrice,
                mrp.Price AS ServiceActualPrice,
                (mrp.Price - s.Price) AS PriceDifference,
                CASE WHEN mrp.Price != s.Price THEN 1 ELSE 0 END AS IsPriceModified,
                mrp.PrescriptionAmount,
                mrp.Quantity,
                mrp.Total,
                mrp.PrescriptionFrequency,
                mr.PaymentStatus,
                mr.EndDate,
                s.Duration AS ServiceDuration,
                s.DurationType AS ServiceDurationType
            FROM MedicalRecords mr
            INNER JOIN Profile p ON mr.StaffId = p.Id
            INNER JOIN MedicalRecordsPrescriptions mrp ON mr.Id = mrp.MedicalRecordsId
            INNER JOIN Services s ON mrp.ProductId = s.Id
            WHERE mr.IsActive = 1 
                  AND mrp.IsActive = 1 
                  AND mrp.Type = 'Service'
                  AND p.IsActive = 1
                  AND s.IsActive = 1";

                var parameters = new DynamicParameters();

                // Add date filters if provided
                if (!string.IsNullOrEmpty(startDate))
                {
                    if (DateTime.TryParseExact(startDate, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime parsedStartDate))
                    {
                        sql += " AND mr.StartDate >= @StartDate";
                        parameters.Add("@StartDate", parsedStartDate);
                    }
                }

                if (!string.IsNullOrEmpty(endDate))
                {
                    if (DateTime.TryParseExact(endDate, "dd-MM-yyyy", null, DateTimeStyles.None, out DateTime parsedEndDate))
                    {
                        // Add 1 day to include the entire end date
                        sql += " AND mr.StartDate <= @EndDate";
                        parameters.Add("@EndDate", parsedEndDate.AddDays(1).AddSeconds(-1));
                    }
                }

                // Add staff filter if provided
                if (staffId.HasValue)
                {
                    sql += " AND mr.StaffId = @StaffId";
                    parameters.Add("@StaffId", staffId.Value);
                }

                // Add price modification filter if provided
                if (onlyModifiedPrices.HasValue && onlyModifiedPrices.Value)
                {
                    sql += " AND mrp.Price != s.Price";
                }

                sql += " ORDER BY mr.StartDate";

                var data = await _db.QueryAsync<MedicalRecordServicesReportDto>(sql, parameters);
                return data.ToList();
            }
        }

        public async Task<IEnumerable<RevenueResponse>> GetRevenueData(string dbName)
        {
            using (var conn = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
                WITH LastPayment AS (
                        SELECT
                            op.OrderId,
                            op.Type,
                            op.PaymentMethodId,
                            ROW_NUMBER() OVER (PARTITION BY op.OrderId, op.Type ORDER BY op.Date DESC) AS rn
                        FROM OrdersPayment op
                        WHERE op.IsActive = 1
                    )
                    -- MedicalRecords
                    SELECT 
                        mr.CreatedAt,
                        mr.Id,
                        mr.Code AS Code,
                        mr.Total AS Total,
                        mr.DiscountTotal AS Discount,
                        COALESCE(mr.TotalDiscounted, (mr.Total - IFNULL(mr.DiscountTotal,0))) AS TotalAfterDiscount,
                        IF(mr.PatientId = 0, 'Guest', o.Name) AS ClientName,
                        'Medical' AS Type,
                        mr.StartDate AS Date,
                        mr.PaymentStatus AS Status,
                        pm.Name AS PaymentMethod,
                        '' AS Details
                    FROM MedicalRecords mr
                    LEFT JOIN Patients p ON mr.PatientId = p.Id
                    LEFT JOIN Owners o ON p.OwnersId = o.Id
                    LEFT JOIN LastPayment lp 
                           ON lp.OrderId = mr.Id 
                          AND lp.Type = 'MedicalRecord' 
                          AND lp.rn = 1
                    LEFT JOIN PaymentMethod pm ON lp.PaymentMethodId = pm.Id
                    WHERE mr.PaymentStatus = 'Paid'
                      AND mr.Total > 0

                    UNION ALL

                    -- Orders
                    SELECT 
                        o.CreatedAt,
                        o.Id,
                        o.OrderNumber AS Code,
                        o.TotalPrice AS Total,
                        o.TotalDiscount AS Discount,
                        COALESCE(o.TotalDiscountedPrice, (o.TotalPrice - IFNULL(o.TotalDiscount,0))) AS TotalAfterDiscount,
                        IF(o.ClientId = 0, 'Guest', o.ClientName) AS ClientName,
                        'Order' AS Type,
                        o.Date AS Date,
                        o.Status AS Status,
                        pm.Name AS PaymentMethod,
                        '' AS Details
                    FROM Orders o
                    LEFT JOIN LastPayment lp 
                           ON lp.OrderId = o.Id 
                          AND lp.Type = 'Order' 
                          AND lp.rn = 1
                    LEFT JOIN PaymentMethod pm ON lp.PaymentMethodId = pm.Id
                    WHERE o.Status = 'Paid'
                      AND o.TotalPrice > 0

                    ORDER BY CreatedAt DESC;

                ";

                return await conn.QueryAsync<RevenueResponse>(sql);
            }
        }
        public async Task<RevenueSummaryResponse> GetRevenueDataSummary(string dbName, DateTime? startDate, DateTime? endDate)
        {
            using (var conn = _dbFactory.GetDbConnection(dbName))
            {
                var sql = @"
            WITH LastPayment AS (
                SELECT
                    op.OrderId,
                    op.Type,
                    op.PaymentMethodId,
                    ROW_NUMBER() OVER (PARTITION BY op.OrderId, op.Type ORDER BY op.Date DESC) AS rn
                FROM OrdersPayment op
                WHERE op.IsActive = 1
            ),
            RevenueData AS (

                -- MedicalRecords
                SELECT 
                    COALESCE(
                        mr.TotalDiscounted,
                        (mr.Total - IFNULL(mr.DiscountTotal, 0))
                    ) AS TotalAfterDiscount
                FROM MedicalRecords mr
                LEFT JOIN LastPayment lp 
                       ON lp.OrderId = mr.Id 
                      AND lp.Type = 'MedicalRecord' 
                      AND lp.rn = 1
                WHERE mr.PaymentStatus = 'Paid'
                  AND mr.Total > 0
                  AND (@StartDate IS NULL OR mr.StartDate >= @StartDate)
                  AND (@EndDate   IS NULL OR mr.StartDate <=  @EndDate)

                UNION ALL

                -- Orders
                SELECT 
                    COALESCE(
                        o.TotalDiscountedPrice,
                        (o.TotalPrice - IFNULL(o.TotalDiscount, 0))
                    ) AS TotalAfterDiscount
                FROM Orders o
                LEFT JOIN LastPayment lp 
                       ON lp.OrderId = o.Id 
                      AND lp.Type = 'Order' 
                      AND lp.rn = 1
                WHERE o.Status = 'Paid'
                  AND o.TotalPrice > 0
                  AND (@StartDate IS NULL OR o.Date >= @StartDate)
                  AND (@EndDate   IS NULL OR o.Date <=  @EndDate)

            )

            SELECT 
                COALESCE(SUM(TotalAfterDiscount), 0) AS TotalRevenue
            FROM RevenueData;
        ";

                return await conn.QueryFirstAsync<RevenueSummaryResponse>(
                    sql,
                    new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                );
            }
        }

        public async Task<IEnumerable<string>> GetRevenueDataFilter(string dbName, string filterField)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                // Mapping supaya aman (hindari SQL injection)
                var allowedFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Code", "Code" },
                { "OwnerName", "OwnerName" },
                { "PatientName", "PatientName" },
                { "StaffName", "StaffName" },
                { "Service", "Service" },
                { "Date", "Date" },
                { "Total", "Total" },
                { "ClientName", "ClientName" },
                { "Type", "Type" },
                { "Discount", "Discount" },
                { "TotalAfterDiscount", "TotalAfterDiscount" },
                { "Status", "Status" },
                { "PaymentMethod", "PaymentMethod" }
            };

                if (!allowedFields.ContainsKey(filterField))
                    throw new ArgumentException("Invalid filter field");

                var column = allowedFields[filterField];

                var sql = $@"
                SELECT DISTINCT {column} 
                FROM (
                    SELECT 
                        mr.CreatedAt,
                        mr.Id,
                        mr.Code AS Code,
                        mr.Total AS Total,
                        mr.DiscountTotal AS Discount,
                        COALESCE(mr.TotalDiscounted, (mr.Total - IFNULL(mr.DiscountTotal,0))) AS TotalAfterDiscount,
                        IF(mr.PatientId = 0, 'Guest', o.Name) AS ClientName,
                        'Medical' AS Type,
                        mr.StartDate AS Date,
                        mr.PaymentStatus AS Status,
                        pm.Name AS PaymentMethod,
                        '' AS Details
                    FROM MedicalRecords mr
                    LEFT JOIN Patients p ON mr.PatientId = p.Id
                    LEFT JOIN Owners o ON p.OwnersId = o.Id
                    LEFT JOIN (
                        SELECT op.OrderId, op.Type, op.PaymentMethodId
                        FROM OrdersPayment op
                        INNER JOIN (
                            SELECT OrderId, Type, MAX(Date) AS MaxDate
                            FROM OrdersPayment
                            WHERE IsActive = 1
                            GROUP BY OrderId, Type
                        ) last_op
                            ON op.OrderId = last_op.OrderId
                           AND op.Type = last_op.Type
                           AND op.Date = last_op.MaxDate
                    ) lastpay ON lastpay.OrderId = mr.Id AND lastpay.Type = 'MedicalRecord'
                    LEFT JOIN PaymentMethod pm ON lastpay.PaymentMethodId = pm.Id
                    WHERE mr.PaymentStatus = 'Paid'

                    UNION ALL

                    SELECT 
                        o.CreatedAt,
                        o.Id,
                        o.OrderNumber AS Code,
                        o.TotalPrice AS Total,
                        o.TotalDiscount AS Discount,
                        COALESCE(o.TotalDiscountedPrice, (o.TotalPrice - IFNULL(o.TotalDiscount,0))) AS TotalAfterDiscount,
                        IF(o.ClientId = 0, 'Guest', o.ClientName) AS ClientName,
                        'Order' AS Type,
                        o.Date AS Date,
                        o.Status AS Status,
                        pm.Name AS PaymentMethod,
                        '' AS Details
                    FROM Orders o
                    LEFT JOIN (
                        SELECT op.OrderId, op.Type, op.PaymentMethodId
                        FROM OrdersPayment op
                        INNER JOIN (
                            SELECT OrderId, Type, MAX(Date) AS MaxDate
                            FROM OrdersPayment
                            WHERE IsActive = 1
                            GROUP BY OrderId, Type
                        ) last_op
                            ON op.OrderId = last_op.OrderId
                           AND op.Type = last_op.Type
                           AND op.Date = last_op.MaxDate
                    ) lastpay ON lastpay.OrderId = o.Id AND lastpay.Type = 'Order'
                    LEFT JOIN PaymentMethod pm ON lastpay.PaymentMethodId = pm.Id
                    WHERE o.Status = 'Paid'
                    ORDER BY CreatedAt DESC
                ) revenue
                WHERE {column} IS NOT NULL
                ORDER BY {column};
            ";

                return await _db.QueryAsync<string>(sql);
            }
        }

        public async Task<int> GetRevenuePagedData(string dbName)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                string queryFinal = @"
                SELECT COUNT(*) FROM (
                    SELECT CreatedAt
                    FROM MedicalRecords
                    WHERE PaymentStatus = 'Paid'
                    UNION ALL
                    SELECT CreatedAt
                    FROM Orders
                    WHERE Status = 'Paid'
                ) AS CombinedData;
            ";
                var total = await _db.ExecuteScalarAsync<int>(queryFinal);
                return total;
            }
        }

        public async Task<RevenueDataResponse> GetSalesDetail(string dbName, string query)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var filter = "";
                if (!string.IsNullOrEmpty(query))
                {
                    filter = $"And {query}";
                }
                string queryFinal = @"SELECT SUM(mrp.Total), SUM(CASE WHEN mrp.Type = 'Product' THEN mrp.Total ELSE 0 END) AS ProductsTotal, SUM(CASE WHEN mrp.Type = 'Service' THEN mrp.Total ELSE 0 END) AS ServicesTotal, " +
                    $"(SELECT COALESCE(SUM(mr.DiscountTotal), 0) FROM MedicalRecords mr WHERE mr.PaymentStatus = 'Paid' AND mr.IsActive = 1 {filter}) AS TotalDiscount " +
                    $"FROM MedicalRecords mr " +
                    $"JOIN MedicalRecordsPrescriptions mrp ON mrp.MedicalRecordsId = mr.Id " +
                    $"JOIN Appointments a ON mr.AppointmentId = a.Id " +
                    $"WHERE mr.PaymentStatus = 'Paid' AND a.IsActive = 1 AND mr.IsActive = 1 AND mrp.IsActive = 1 {filter}";
                return await _db.QueryFirstOrDefaultAsync<RevenueDataResponse>(queryFinal);
            }
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetTotalMedicalSales(string dbName, string dateFilter)
        {
            using (var _db = _dbFactory.GetDbConnection(dbName))
            {
                var filterQuery = "YEAR(mr.StartDate) = YEAR(CURRENT_DATE()) AND mr.StartDate <= CURRENT_DATE()";
                if (dateFilter != null)
                {
                    filterQuery = dateFilter;
                }
                string query = @"SELECT
                              DATE_FORMAT(mr.StartDate, '%d') AS Date,
                              DATE_FORMAT(mr.StartDate, '%m') AS Month,
                              DATE_FORMAT(mr.StartDate, '%Y') AS Year,
                              SUM(DistinctTotal) AS Total
                            FROM
                              MedicalRecords mr
                            JOIN (
                              SELECT 
                                MedicalRecordsId, 
                                ProductId, 
                                Type,
                                SUM(Total) AS DistinctTotal
                              FROM 
                                (SELECT DISTINCT MedicalRecordsId, ProductId, Type, Total 
                                 FROM MedicalRecordsPrescriptions WHERE IsActive = 1) AS DeduplicatedRecords
                              GROUP BY MedicalRecordsId, ProductId, Type
                            ) mrp ON mrp.MedicalRecordsId = mr.Id
                            WHERE
                              mr.PaymentStatus = 'Paid' AND mr.IsActive = 1 AND 
            ";
                query += filterQuery;
                query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
                return await _db.QueryAsync<MonthlyDataChart>(query);
            }
        }

        public async Task<IEnumerable<MonthlyDataChart>> GetVisitYearly(string dbName, string? dateFilter)
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
                                MedicalRecords
                            WHERE 
            ";
                query += filterQuery;
                query += " GROUP BY Date, Month, Year ORDER BY Date, Year, Month;";
                return await _db.QueryAsync<MonthlyDataChart>(query);
            }
        }
        public async Task<IEnumerable<DoctorPerformanceRawDto>> GetDoctorPerformanceRaw(string dbName, int year)
        {
            using var _db = _dbFactory.GetDbConnection(dbName);

            string sql = @"
                SELECT 
                    p.Id AS DoctorId,
                    p.Name AS DoctorName,
                    MONTH(a.Date) AS Month,
                    COUNT(mr.Id) AS Total
                FROM MedicalRecords mr
                JOIN Appointments a ON mr.AppointmentId = a.Id
                JOIN Profile p ON a.StaffId = p.Id
                WHERE 
                    a.StatusId = 6
                    AND mr.IsActive = 1
                    AND a.IsActive = 1
                    AND p.IsActive = 1
                    AND p.Roles <> 'Admin'
                    AND YEAR(a.Date) = @Year
                GROUP BY 
                    p.Id, p.Name, MONTH(a.Date)
                ORDER BY 
                    p.Name, Month;
            ";

            return await _db.QueryAsync<DoctorPerformanceRawDto>(sql, new { Year = year });
        }

    }

    internal sealed class PharmacyMedicalRecordDetailRow
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int AppointmentId { get; set; }
        public int? ServiceId { get; set; }
        public string ServiceName { get; set; }
        public double ServicePrice { get; set; }
        public int? StaffId { get; set; }
        public string StaffName { get; set; }
        public int? PatientId { get; set; }
        public string PatientName { get; set; }
        public string Species { get; set; }
        public string Breed { get; set; }
        public string Color { get; set; }
        public int? OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string OwnerTitle { get; set; }
    }
}
