using Microsoft.EntityFrameworkCore;
using StudySpace.Common;
using StudySpace.Data.Models;
using StudySpace.Data.UnitOfWork;
using StudySpace.Service.Base;
using StudySpace.Service.BusinessModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.Services
{
    public interface IDashboardService
    {
        Task<IBusinessResult> CalculateStoreIncome(int storeId);
        Task<IBusinessResult> CalculateAll();
    }
    
    public class DashboardService : IDashboardService
    {
        private readonly UnitOfWork _unitOfWork;

        public DashboardService()
        {
            _unitOfWork ??= new UnitOfWork();
        }
        public async Task<IBusinessResult> CalculateAll()
        {
            try
            {
                var admins = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "Admin")
                .ToList();

                var users = _unitOfWork.AccountRepository
                    .FindByCondition(a => a.Role.RoleName == "User")
                .ToList();
                
                var stores = _unitOfWork.StoreRepository
                    .GetAll()
                .ToList();

                int totalAdmins = admins.Count;
                int totalUsers = users.Count;
                int totalStores = stores.Count;

                int totalAccounts = totalAdmins + totalUsers + totalStores;

                var transactions = await _unitOfWork.TransactionRepository.GetAllAsync();

                double totalIncomes = transactions.Sum(t => t.Amount ?? 0);

                var bookings = await _unitOfWork.BookingRepository.GetAllAsync();
                int totalBookings = bookings.Count;

                int totalTransactions = transactions.Count;

                var monthlyData = transactions
                    .Where(t => t.Date.HasValue)
                    .GroupBy(t => new
                    {
                        Month = t.Date.Value.Month,
                        Year = t.Date.Value.Year
                    })
                    .Select(group => new
                    {
                        Month = group.Key.Month,
                        Year = group.Key.Year,
                        TotalTransactions = group.Count(),
                        TotalAmount = group.Sum(t => t.Amount ?? 0)
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();

                var getBookingWithStore = await _unitOfWork.BookingRepository.GetBookingDetails().ToListAsync();
                var hotBookingStore = getBookingWithStore
                    .Where(b => b.Room.StoreId.HasValue)
                    .GroupBy(b => b.Room.StoreId)
                    .Select(gr => new
                    {
                        StoreId = gr.Key,
                        ImageUrl = gr.SelectMany(b => b.Room.ImageRooms)
                                     .Where(img => img != null)
                                     .Select(img => img.ImageUrl)
                                     .FirstOrDefault(),
                        TotalBookings = gr.Count()
                    })
                    .OrderByDescending(g => g.TotalBookings)
                    .Take(5)
                    .Join(stores, b => b.StoreId, s => s.Id, (b, s) => new
                    {
                        StoreName = s.Name,
                        Address = s.Address,
                        ImageURL = b.ImageUrl,
                        TotalBookings = b.TotalBookings,
                    })
                    .ToList();

                var totalBookingInMonth = bookings.Where(b => b.BookingDate.HasValue && b.BookingDate.Value.Month == DateTime.Now.Month && b.BookingDate.Value.Year == DateTime.Now.Year).Count();
                var totalTransactionInMonth = transactions.Where(t => t.Date.HasValue && t.Date.Value.Month == DateTime.Now.Month && t.Date.Value.Year == DateTime.Now.Year).Count();
                var totalRevenueInMonth = transactions.Where(t => t.Date.HasValue && t.Date.Value.Month == DateTime.Now.Month && t.Date.Value.Year == DateTime.Now.Year).Sum(t => t.Amount ?? 0);

                var response = new
                {
                    Accounts = new {
                        TotalAdmins = totalAdmins,
                        TotalUsers = totalUsers,
                        TotalStores = totalStores,
                        TotalAccounts = totalAccounts,
                    },

                    TotalInThisMonth = new
                    {
                        TotalBookingsInMonth = totalBookingInMonth,
                        TotalTransactionsInMonth = totalTransactionInMonth,
                        TotalRevenueInMonth = totalRevenueInMonth
                    },

                    MonthlyIncome = monthlyData.Select(m => new
                    {
                        Month = $"{m.Month}/{m.Year}",
                        TotalTransactions = m.TotalTransactions,
                        TotalAmount = m.TotalAmount
                    }).ToList(),

                    TotalIncome = totalIncomes,

                    TotalTransactions = totalTransactions,

                    TotalBookings = totalBookings,

                    HotBookingStore = hotBookingStore
                };

                return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
            }
            catch (Exception ex)
            {
                return new BusinessResult(Const.ERROR_EXEPTION, ex.Message);
            }
        }

        public async Task<IBusinessResult> CalculateStoreIncome(int storeId)
        {
            var store = await _unitOfWork.StoreRepository.GetByIdAsync(storeId);

            if (store == null)
            {
                return new BusinessResult(Const.WARNING_NO_DATA, Const.WARNING_NO_DATA_MSG);
            }

            var transactions = _unitOfWork.TransactionRepository.GetAllTransactions()
                                    .Where(t => t.BookingId != null && t.Booking.Room.StoreId == storeId)
                                    .ToList();

            double totalAmount = transactions.Sum(t => t.Amount ?? 0);

            var rooms = _unitOfWork.RoomRepository.GetAll()
                                                  .Where(r => r.StoreId == storeId)
                                                  .ToList();
            var bookings = _unitOfWork.BookingRepository.GetBookingDetails()
                                                .Where(b => b.Room.StoreId == storeId)
                                                .ToList();

            var popularRooms = bookings
                                    .GroupBy(b => b.Room)
                                    .Select(group => new PopularRoom
                                                        {
                                                            Name = group.Key.RoomName,
                                                            Type = group.Key.Type,
                                                            Image = group.Key.ImageRooms.FirstOrDefault()?.ImageUrl,
                                                            TotalBooking = group.Count()
                                                        })
                                    .OrderByDescending(p => p.TotalBooking)
                                    .Take(5)
                                    .ToList();

            var monthlyData = transactions
                    .Where(t => t.Date.HasValue)
                    .GroupBy(t => new
                    {
                        Month = t.Date.Value.Month,
                        Year = t.Date.Value.Year
                    })
                    .Select(group => new
                    {
                        Month = group.Key.Month,
                        Year = group.Key.Year,
                        TotalTransactions = group.Count(),
                        TotalAmount = group.Sum(t => t.Amount ?? 0)
                    })
                    .OrderBy(g => g.Year).ThenBy(g => g.Month)
                    .ToList();

            var totalBookingInMonth = bookings.Where(b => b.BookingDate.HasValue && b.BookingDate.Value.Month == DateTime.Now.Month && b.BookingDate.Value.Year == DateTime.Now.Year).Count();
            var totalTransactionInMonth = transactions.Where(t => t.Date.HasValue && t.Date.Value.Month == DateTime.Now.Month && t.Date.Value.Year == DateTime.Now.Year).Count();
            var totalRevenueInMonth = transactions.Where(t => t.Date.HasValue && t.Date.Value.Month == DateTime.Now.Month && t.Date.Value.Year == DateTime.Now.Year).Sum(t => t.Amount ?? 0);

            var response = new DashboardSupplierModel
            {
                TotalInThisMonth = new TotalInThisMonth
                {
                    TotalBookingsInMonth = totalBookingInMonth,
                    TotalRevenueInMonth = totalRevenueInMonth,
                    TotalTransactionsInMonth = totalRevenueInMonth
                },
                StoreName = store.Name,
                TotalIncome = totalAmount,
                TotalRoom = rooms.Count(),
                TotalBooking = bookings.Count(),
                MonthRevenue = monthlyData.Select(m => new MonthRevenue
                {
                    Month = $"{m.Month}/{m.Year}",
                    TransactionInMonth = m.TotalTransactions,
                    RevenueInMonth = m.TotalAmount
                }).ToList(),
                PopularRooms = popularRooms
            };

            return new BusinessResult(Const.SUCCESS_READ, Const.SUCCESS_READ_MSG, response);
        }
    }
}
