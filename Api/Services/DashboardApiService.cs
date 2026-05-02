using Api.Dtos;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;

namespace Api.Services
{
    public class DashboardApiService
    {
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Member> _memberRepository;
        private readonly IRepository<Course> _courseRepository;
        private readonly IRepository<Booking> _bookingRepository;
        private readonly IRepository<OrderDetail> _orderDetailRepository;

        public DashboardApiService(IRepository<Order> orderReoisitory,
                                   IRepository<Member> memberReoisitory,
                                   IRepository<Course> courseReoisitory,
                                   IRepository<Booking> bookingReoisitory,
                                   IRepository<OrderDetail> orderDetailReoisitory)
        {
            _orderRepository = orderReoisitory;
            _memberRepository = memberReoisitory;
            _courseRepository = courseReoisitory;
            _bookingRepository = bookingReoisitory;
            _orderDetailRepository = orderDetailReoisitory;
        }

        public async Task<DashboardDto> GetAllData()
        {
            var result = new DashboardDto();

            // 會員今日新增數量
            var members = await _memberRepository.ListAsync(m => m.Cdate > DateTime.Today);
            int membersAddCount = members.Count();

            result.MemberTodayCount = membersAddCount;

            // 本日業績較昨日成長
            var todayOrders = await _orderRepository.ListAsync(o => o.TransactionDate > DateTime.Today);
            var yesterdayOrders = await _orderRepository.ListAsync(o => o.TransactionDate >= DateTime.Today.AddDays(-1)
                                                               && o.TransactionDate < DateTime.Today);
            int todayOrderTotal = (int)todayOrders.Sum(o => o.TotalPrice);
            int yesterdayOrderTotal = (int)yesterdayOrders.Sum(o => o.TotalPrice);
            double proportion = 0;
            if (yesterdayOrderTotal != 0)
            {
                proportion = ((double)(todayOrderTotal - yesterdayOrderTotal) / yesterdayOrderTotal) * 100;
            }
            else
            {
                proportion = (todayOrderTotal) / 1.0;
            }

            result.Proportion = Math.Round(proportion,2);

            // 本日熱銷課程
            var orders = await _orderRepository.ListAsync(o => o.TransactionDate > DateTime.Today);
            if (orders.Count > 0)
            {
                var orderIds = orders.Select(o => o.OrderId).ToList();
                var orderDetails = await _orderDetailRepository.ListAsync(od => orderIds.Contains(od.OrderId));
                var courses = orderDetails.GroupBy(od => od.CourseId)
                         .Select(g => new
                         {
                             CourseId = g.Key,
                             SalesCount = g.Count(),
                         }).OrderByDescending(cs => cs.SalesCount).ToList();
                var topSellingCourse = courses.FirstOrDefault();

                if (topSellingCourse != null)
                {
                    var topCourse = await _courseRepository.GetByIdAsync(topSellingCourse.CourseId);
                    result.PopularCourse = topCourse.Title;
                }
            }

            // 最熱門科目及次熱門科目
            var halfYearOrders = await _orderRepository.ListAsync(o => o.TransactionDate.Month >= 7 && o.TransactionDate.Month <= 12);
            List<int> halfYearOrdersId = new List<int>();
            List<OrderDetail> halfOrderDetails = new List<OrderDetail>();
            if (halfYearOrders.Count > 0)
            {
                halfYearOrdersId = halfYearOrders.Select(o => o.OrderId).ToList();
                halfOrderDetails = await _orderDetailRepository.ListAsync(od => halfYearOrdersId.Contains(od.OrderId));

                var halfCourseSales = halfOrderDetails.GroupBy(od => new { od.CourseId, od.Order.TransactionDate.Month })
                                                       .Select(g => new
                                                       {
                                                           g.Key.CourseId,
                                                           g.Key.Month,
                                                           SalesTotal = g.Sum(od => od.TotalPrice)
                                                       }).ToList();
                var groupedCourseSales = halfCourseSales.GroupBy(s => s.CourseId)
                                                        .Select(g => new
                                                        {
                                                            CourseId = g.Key,
                                                            MonthlySales = Enumerable.Range(7, 6)
                                                                                     .Select(month => g.Where(s => s.Month == month)
                                                                                                       .Sum(s => s.SalesTotal))
                                                                                     .Select(s => (int)s)
                                                                                     .ToArray()
                                                        })
                                                       .OrderByDescending(g => g.MonthlySales.Sum()).Take(2).ToList();

                var courseIds = groupedCourseSales.Select(c => c.CourseId).ToList();
                var courses = await _courseRepository.ListAsync(c => courseIds.Contains(c.CourseId));

                var lineChartResult = groupedCourseSales.Select(gc =>
                {
                    var courseName = courses.FirstOrDefault(c => c.CourseId == gc.CourseId)?.Title ?? "不具名的課程";
                    return (courseName, gc.MonthlySales);
                }).ToList();

                result.FirstCourseName = lineChartResult[0].courseName;
                result.SecondCourseName = lineChartResult[1].courseName;
                result.FirstCourseData = lineChartResult[0].MonthlySales.ToArray();
                result.SecondCourseData = lineChartResult[1].MonthlySales.ToArray();
            }

            // 預訂課程總堂數
            var halfYearBookings = await _bookingRepository.ListAsync(b => b.BookingDate.Month >= 7 && b.BookingDate.Month <= 12);
            var bookingCourseCounts = halfYearBookings.GroupBy(b => b.BookingDate.Month)
                                                      .Select(g => new
                                                      {
                                                          Month = g.Key,
                                                          TotalBookings = g.Count()
                                                      })
                                                      .OrderBy(g => g.Month)
                                                      .ToList();
            // 銷售課程總堂數
            var salesCourseCounts = halfOrderDetails.GroupBy(od => od.Order.TransactionDate.Month)
                                        .Select(g => new
                                        {
                                            Month = g.Key,
                                            TotalCourses = g.Sum(od => od.Quantity)
                                        })
                                        .OrderBy(g => g.Month)
                                        .ToList();

            result.OrderQuantityData = Enumerable.Range(7, 6)
                                                 .Select(month => salesCourseCounts.FirstOrDefault(sc => sc.Month == month)?.TotalCourses ?? 0)
                                                 .ToArray();
            result.BookingQuantiyData = Enumerable.Range(7, 6)
                                                  .Select(month => bookingCourseCounts.FirstOrDefault(bc => bc.Month == month)?.TotalBookings ?? 0)
                                                  .ToArray();

            return result;
        }
    }
}
