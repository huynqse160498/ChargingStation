using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Repositories.DTOs;
using Repositories.Models;
using Services.Interfaces;

namespace Services.Implementations
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly ChargeStationContext _db;
        public AnalyticsService(ChargeStationContext db) => _db = db;

        // ───────────────────────── helpers: month window (UTC+7) ─────────────────────────
        private static DateTime StartOfMonthUtc7(int year, int month) =>
            new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(7);

        private static DateTime EndOfMonthUtc7(int year, int month) =>
            StartOfMonthUtc7(year, month).AddMonths(1);

        private IQueryable<ChargingSession> BaseQuery(int month, int year, int? companyId, bool adminView)
        {
            var from = StartOfMonthUtc7(year, month);
            var to = EndOfMonthUtc7(year, month);

            var q = _db.ChargingSessions
                .Include(s => s.Company)
                .Include(s => s.PricingRule)
                .Include(s => s.Vehicle)
                .Include(s => s.Port).ThenInclude(p => p.Charger).ThenInclude(c => c.Station)
                .Where(s => s.StartedAt >= from && s.StartedAt < to && s.Status == "Completed");

            if (companyId.HasValue)
                q = q.Where(s => s.CompanyId == companyId.Value);

            if (!adminView && !companyId.HasValue)
                q = q.Where(s => false); 

            return q.AsNoTracking();
        }


        // ─────────────────────────────── summary ───────────────────────────────
        public async Task<AnalyticsSummaryDto> GetSummaryAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView);

            var list = await q.Select(s => new
            {
                Subtotal = s.Subtotal ?? 0M,
                Tax = s.Tax ?? 0M,
                Total = s.Total ?? 0M,
                EnergyKwh = s.EnergyKwh ?? 0M,
                s.DurationMin,
                s.IdleMin
            }).ToListAsync();

            decimal kwh = list.Sum(x => x.EnergyKwh);
            decimal subtotal = list.Sum(x => x.Subtotal);
            decimal tax = list.Sum(x => x.Tax);
            decimal total = list.Sum(x => x.Total);
            int sessions = list.Count;
            int duration = list.Sum(x => x.DurationMin ?? 0);
            int idle = list.Sum(x => x.IdleMin ?? 0);
            decimal avg = kwh > 0M ? Math.Round(total / kwh, 2) : 0M;

            return new AnalyticsSummaryDto
            {
                Month = month,
                Year = year,
                SessionCount = sessions,
                EnergyKwh = kwh,
                Subtotal = subtotal,
                Tax = tax,
                Total = total,
                DurationMin = duration,
                IdleMin = idle,
                AvgPricePerKwh = avg
            };
        }

        // ───────────────────────── revenue sources ─────────────────────────
        public async Task<RevenueSourceDto> GetRevenueSourcesAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView);

            var rows = await q.Select(s => new
            {
                s.Total,
                s.CustomerId,
                s.CompanyId
            }).ToListAsync();

            decimal customer = rows.Where(r => r.CustomerId != null).Sum(r => r.Total ?? 0M);
            decimal company = rows.Where(r => r.CompanyId != null).Sum(r => r.Total ?? 0M);
            decimal guest = rows.Where(r => r.CustomerId == null && r.CompanyId == null).Sum(r => r.Total ?? 0M);

            return new RevenueSourceDto
            {
                Month = month,
                Year = year,
                CustomerTotal = customer,
                CompanyTotal = company,
                GuestTotal = adminView ? guest : 0M 
            };
        }

        // ───────────────────────────── breakdown helpers ─────────────────────────────
        private static BreakdownItemDto MapCommon(string key, string? key2, IEnumerable<ChargingSession> src)
        {
            var sessions = src.Count();
            decimal kwh = src.Sum(s => s.EnergyKwh ?? 0M);
            decimal tot = src.Sum(s => s.Total ?? 0M);
            int dur = src.Sum(s => s.DurationMin ?? 0);
            int idle = src.Sum(s => s.IdleMin ?? 0);

            return new BreakdownItemDto
            {
                Key = key,
                Key2 = key2,
                SessionCount = sessions,
                EnergyKwh = kwh,
                Total = tot,
                DurationMin = dur,
                IdleMin = idle
            };
        }

        // ───────────────── breakdown: company (admin) ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByCompanyAsync(int month, int year)
        {
            var q = BaseQuery(month, year, null, adminView: true)
                .Where(s => s.CompanyId != null);

            var rows = await q.Select(s => new
            {
                s,
                s.CompanyId,
                CompanyName = s.Company != null ? s.Company.Name : null
            }).ToListAsync();

            var groups = rows.GroupBy(r => new { r.CompanyId, r.CompanyName })
                             .OrderByDescending(g => g.Sum(x => x.s.Total ?? 0M));

            return groups.Select(g => MapCommon(
                g.Key.CompanyName ?? $"Company#{g.Key.CompanyId}",
                null,
                g.Select(x => x.s))).ToList();
        }

        // ───────────────── breakdown: vehicle (company self) ─────────────────
        public async Task<List<VehicleBreakdownItemDto>> GetBreakdownByVehicleAsync(int month, int year, int companyId)
        {
            var q = BaseQuery(month, year, companyId, adminView: false)
                .Where(s => s.CompanyId == companyId && s.Vehicle != null);

            var rows = await q.Select(s => new
            {
                s,
                s.VehicleId,
                s.Vehicle.LicensePlate,
                s.Vehicle.VehicleType
            }).ToListAsync();

            var grp = rows.GroupBy(r => new { r.VehicleId, r.LicensePlate, r.VehicleType })
                          .OrderByDescending(g => g.Sum(x => x.s.Total ?? 0M));

            return grp.Select(g =>
            {
                var baseDto = MapCommon(
                    key: g.Key.LicensePlate ?? $"Vehicle#{g.Key.VehicleId}",
                    key2: g.Key.VehicleType,
                    src: g.Select(x => x.s));

                return new VehicleBreakdownItemDto
                {
                    VehicleId = g.Key.VehicleId,
                    LicensePlate = g.Key.LicensePlate,
                    VehicleType = g.Key.VehicleType,
                    Key = baseDto.Key,
                    Key2 = baseDto.Key2,
                    SessionCount = baseDto.SessionCount,
                    EnergyKwh = baseDto.EnergyKwh,
                    Total = baseDto.Total,
                    DurationMin = baseDto.DurationMin,
                    IdleMin = baseDto.IdleMin
                };
            }).ToList();
        }

        // ───────────────── breakdown: station ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByStationAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView)
                .Where(s => s.Port != null && s.Port.Charger != null && s.Port.Charger.Station != null);

            var rows = await q.Select(s => new
            {
                s,
                StationId = s.Port!.Charger!.Station!.StationId,
                StationName = s.Port!.Charger!.Station!.StationName
            }).ToListAsync();

            var grp = rows.GroupBy(r => new { r.StationId, r.StationName });
            return grp.Select(g => MapCommon(
                        g.Key.StationName ?? $"Station#{g.Key.StationId}",
                        null,
                        g.Select(x => x.s)))
                      .OrderByDescending(x => x.Total)
                      .ToList();
        }

        // ───────────────── breakdown: charger ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByChargerAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView)
                .Where(s => s.Port != null && s.Port.Charger != null);

            var rows = await q.Select(s => new
            {
                s,
                ChargerId = s.Port!.Charger!.ChargerId,
                ChargerCode = s.Port!.Charger!.Code,
                StationName = s.Port!.Charger!.Station!.StationName
            }).ToListAsync();

            return rows.GroupBy(r => new { r.ChargerId, r.ChargerCode, r.StationName })
                       .Select(g => MapCommon(
                           g.Key.ChargerCode ?? $"Charger#{g.Key.ChargerId}",
                           g.Key.StationName,
                           g.Select(x => x.s)))
                       .OrderByDescending(x => x.Total)
                       .ToList();
        }

        // ───────────────── breakdown: port ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByPortAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView)
                .Where(s => s.Port != null && s.Port.Charger != null);

            var rows = await q.Select(s => new
            {
                s,
                PortId = s.Port!.PortId,
                PortCode = s.Port!.Code,
                ChargerCode = s.Port!.Charger!.Code
            }).ToListAsync();

            return rows.GroupBy(r => new { r.PortId, r.PortCode, r.ChargerCode })
                       .Select(g => MapCommon(
                           g.Key.PortCode ?? $"Port#{g.Key.PortId}",
                           g.Key.ChargerCode,
                           g.Select(x => x.s)))
                       .OrderByDescending(x => x.Total)
                       .ToList();
        }

        // ───────────────── breakdown: connector type ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByConnectorTypeAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView).Where(s => s.Port != null);
            var rows = await q.Select(s => new { s, ConnectorType = s.Port!.ConnectorType }).ToListAsync();

            return rows.GroupBy(r => r.ConnectorType ?? "Unknown")
                       .Select(g => MapCommon(g.Key, null, g.Select(x => x.s)))
                       .OrderByDescending(x => x.Total)
                       .ToList();
        }

        // ───────────────── breakdown: vehicle type ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByVehicleTypeAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView);
            var rows = await q.Select(s => new { s, VehicleType = s.Vehicle!.VehicleType }).ToListAsync();

            return rows.GroupBy(r => r.VehicleType ?? "Unknown")
                       .Select(g => MapCommon(g.Key, null, g.Select(x => x.s)))
                       .OrderByDescending(x => x.Total)
                       .ToList();
        }

        // ───────────────── breakdown: time range ─────────────────
        public async Task<List<BreakdownItemDto>> GetBreakdownByTimeRangeAsync(int month, int year, int? companyId, bool adminView)
        {
            var q = BaseQuery(month, year, companyId, adminView);
            var rows = await q.Select(s => new { s, TimeRange = s.PricingRule!.TimeRange }).ToListAsync();

            return rows.GroupBy(r => r.TimeRange ?? "Unknown")
                       .Select(g => MapCommon(g.Key, null, g.Select(x => x.s)))
                       .OrderByDescending(x => x.Total)
                       .ToList();
        }

        // ───────────────── utilization (station/charger/port) ─────────────────
        public async Task<List<HardwareUtilDto>> GetHardwareUtilizationAsync(
     int month, int year, int? companyId, bool adminView,
     string scope, double minUtilization = 0.05, int minSessions = 3)
        {
            var q = BaseQuery(month, year, companyId, adminView);
            decimal totalMinutesInMonth = (decimal)(EndOfMonthUtc7(year, month) - StartOfMonthUtc7(year, month)).TotalMinutes;

            if (scope.Equals("Station", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await q.Select(s => new
                {
                    s.DurationMin,
                    s.EnergyKwh,
                    StationName = s.Port!.Charger!.Station!.StationName
                }).ToListAsync();

                var grp = rows.GroupBy(r => r.StationName ?? "Unknown");
                return grp.Select(g => new HardwareUtilDto
                {
                    Scope = "Station",
                    Code = g.Key,
                    ParentCode = null,
                    SessionCount = g.Count(),
                    ChargingMinutes = g.Sum(x => x.DurationMin ?? 0),
                    EnergyKwh = g.Sum(x => x.EnergyKwh ?? 0M),
                    Utilization = totalMinutesInMonth == 0 ? 0M
                        : Math.Round(((decimal)g.Sum(x => x.DurationMin ?? 0)) / totalMinutesInMonth, 4)
                })
                .Where(x => x.Utilization >= (decimal)minUtilization && x.SessionCount >= minSessions)
                .OrderByDescending(x => x.Utilization)
                .ToList();
            }
            else if (scope.Equals("Charger", StringComparison.OrdinalIgnoreCase))
            {
                var rows = await q.Select(s => new
                {
                    s.DurationMin,
                    s.EnergyKwh,
                    ChargerCode = s.Port!.Charger!.Code,
                    StationName = s.Port!.Charger!.Station!.StationName
                }).ToListAsync();

                var grp = rows.GroupBy(r => new { r.ChargerCode, r.StationName });
                return grp.Select(g => new HardwareUtilDto
                {
                    Scope = "Charger",
                    Code = g.Key.ChargerCode ?? "Unknown",
                    ParentCode = g.Key.StationName,
                    SessionCount = g.Count(),
                    ChargingMinutes = g.Sum(x => x.DurationMin ?? 0),
                    EnergyKwh = g.Sum(x => x.EnergyKwh ?? 0M),
                    Utilization = totalMinutesInMonth == 0 ? 0M
                        : Math.Round(((decimal)g.Sum(x => x.DurationMin ?? 0)) / totalMinutesInMonth, 4)
                })
                .Where(x => x.Utilization >= (decimal)minUtilization && x.SessionCount >= minSessions)
                .OrderByDescending(x => x.Utilization)
                .ToList();
            }
            else // Port
            {
                var rows = await q.Select(s => new
                {
                    s.DurationMin,
                    s.EnergyKwh,
                    PortCode = s.Port!.Code,
                    ChargerCode = s.Port!.Charger!.Code
                }).ToListAsync();

                var grp = rows.GroupBy(r => new { r.PortCode, r.ChargerCode });
                return grp.Select(g => new HardwareUtilDto
                {
                    Scope = "Port",
                    Code = g.Key.PortCode ?? "Unknown",
                    ParentCode = g.Key.ChargerCode,
                    SessionCount = g.Count(),
                    ChargingMinutes = g.Sum(x => x.DurationMin ?? 0),
                    EnergyKwh = g.Sum(x => x.EnergyKwh ?? 0M),
                    Utilization = totalMinutesInMonth == 0 ? 0M
                        : Math.Round(((decimal)g.Sum(x => x.DurationMin ?? 0)) / totalMinutesInMonth, 4)
                })
                .Where(x => x.Utilization >= (decimal)minUtilization && x.SessionCount >= minSessions)
                .OrderByDescending(x => x.Utilization)
                .ToList();
            }
        }


        // ───────────────── top / under / zero activity (theo Port) ─────────────────
        public async Task<TopListDto> GetTopAndUnderAsync(
            int month, int year, int? companyId, bool adminView,
            double minUtilization = 0.05, int minSessions = 3)
        {
            var q = BaseQuery(month, year, companyId, adminView);
            int totalMinutesInMonth = (int)(EndOfMonthUtc7(year, month) - StartOfMonthUtc7(year, month)).TotalMinutes;

            var rows = await q.Select(s => new
            {
                s.Total,
                s.DurationMin,
                EnergyKwh = s.EnergyKwh ?? 0M,        
                PortCode = s.Port!.Code,
                ChargerCode = s.Port!.Charger!.Code
            }).ToListAsync();

            var grouped = rows.GroupBy(r => new { r.PortCode, r.ChargerCode })
                              .Select(g => new
                              {
                                  g.Key.PortCode,
                                  g.Key.ChargerCode,
                                  SessionCount = g.Count(),
                                  ChargingMinutes = g.Sum(x => x.DurationMin ?? 0),
                                  EnergyKwh = g.Sum(x => x.EnergyKwh),  
                                  Total = g.Sum(x => x.Total ?? 0M),
                                  Util = totalMinutesInMonth == 0 ? 0
                                        : (double)g.Sum(x => x.DurationMin ?? 0) / totalMinutesInMonth
                              })
                              .ToList();

            var top = grouped.OrderByDescending(x => x.Total).Take(10)
                             .Select(x => new BreakdownItemDto
                             {
                                 Key = x.PortCode ?? "Port",
                                 Key2 = x.ChargerCode,
                                 SessionCount = x.SessionCount,
                                 DurationMin = x.ChargingMinutes,
                                 Total = x.Total,
                                 EnergyKwh = x.EnergyKwh  
                             }).ToList();

            var under = grouped.Where(x => x.Util < minUtilization || x.SessionCount < minSessions)
                               .OrderBy(x => x.Util)
                               .Take(20)
                               .Select(x => new BreakdownItemDto
                               {
                                   Key = x.PortCode ?? "Port",
                                   Key2 = x.ChargerCode,
                                   SessionCount = x.SessionCount,
                                   DurationMin = x.ChargingMinutes,
                                   Total = x.Total,
                                   EnergyKwh = x.EnergyKwh   
                               }).ToList();

            // zero-activity ports (tháng đó không có session)
            var allPorts = await _db.Ports.Include(p => p.Charger).ToListAsync();
            var activePortCodes = grouped.Select(g => g.PortCode).Where(c => c != null).ToHashSet();
            var zero = allPorts.Where(p => !activePortCodes.Contains(p.Code))
                               .Select(p => new BreakdownItemDto
                               {
                                   Key = p.Code ?? $"Port#{p.PortId}",
                                   Key2 = p.Charger?.Code
                               })
                               .Take(50)
                               .ToList();

            return new TopListDto
            {
                TopActive = top,
                UnderUtilized = under,
                ZeroActivity = zero
            };
        }
    }
}
