const bookingTableBody = document.getElementById("bookingTableBody");
const bookingTableHeader = document.getElementById("bookingTableHeader");
const bookingTableBodyWrapper = document.getElementById("bookingTableBodyWrapper");
const weekRange = document.getElementById("weekRange");
const prevWeekBtn = document.getElementById("prevWeek");
const nextWeekBtn = document.getElementById("nextWeek");


//確認預約Modal
const confirmBookingModal = new bootstrap.Modal(document.getElementById("confirmBookingModal"));
const confirmBookingModalTutorHeadshot = document.getElementById("confirmBookingModalTutorHeadshot");
const confirmBookingModalCourseTitle = document.getElementById("confirmBookingModalCourseTitle");
const confirmBookingModalDate = document.getElementById("confirmBookingModalDate");
const confirmBookingModalTime = document.getElementById("confirmBookingModalTime");
const addToCartBtn = document.getElementById("confirmBookingModalAddToCartBtn");


//初始化資料
let bookingDateStart = new Date();
bookingDateStart.setDate(bookingDateStart.getDate());
let globCourseId = 1;
let tutorSlots = [];
let bookedSlots = [];
let tutorHeadShot = "";
let courseTitle = "";
let selectedBookingDate = null;
let selectedBookingTime = null;


//Booking Table渲染
export async function generateBookingTable(weekStart, courseId) {

    globCourseId = courseId;
    const fetchedData = await fetchBookingTableData(courseId);
    console.log(fetchedData);
    tutorSlots = fetchedData.availableTimeSlots;
    bookedSlots = fetchedData.bookedTimeSlots;
    tutorHeadShot = fetchedData.tutorHeadShotImage;
    courseTitle = fetchedData.courseTitle;


    bookingTableBody.innerHTML = "";
    bookingTableHeader.innerHTML = "";
    const dates = [];
    const standardWeekdays = ["日", "一", "二", "三", "四", "五", "六"];
    const today = new Date().getDay();
    let weekDaysBeginFromToday = standardWeekdays
        .slice(today)
        .concat(standardWeekdays.slice(0, today));

    for (let i = 0; i < 7; i++) {
        const date = new Date(weekStart);
        date.setDate(weekStart.getDate() + i);
        dates.push(date);

        const divTableHead = document.createElement("div");
        divTableHead.innerHTML = `${weekDaysBeginFromToday[i]
            }<br>${date.getDate()}`;
        bookingTableHeader.appendChild(divTableHead);
    }
    weekRange.textContent = `${formatDate(dates[0])} 
  至 ${formatDate(dates[6])}`;

    //產出表格內時間
    const times = generateTimeSlots();

    for (const date of dates) {
        const column = document.createElement("div");
        const weekday = date.getDay();
        for (const time of times) {
            const cell = document.createElement("div");
            cell.textContent = time;
            cell.dataset.time = `${formatDate(date)} ${time}`;
            cell.className = isBooked(date, time, bookedSlots)
                ? "booked"
                : "available";

            //如果日期不在教師的教課時間內, 隱藏日期
            if (!inTutorTime(weekday, time)) {
                cell.classList.add("d-none");
            }

            const bookingTime = new Date(date);
            bookingTime.setHours(parseInt(time.split(':')[0]), 0, 0, 0);
            const currentTime = new Date();
            const timeDifference = bookingTime - currentTime;  //毫秒

            //如果預約時間在6小時內或在當前時間之前, 加past-time class
            if (timeDifference < 0) {
                cell.style.color = '#999';
                cell.removeAttribute("title");
                cell.removeAttribute("data-bs-custom-class");

                if (cell._tooltipInstance) {
                    cell._tooltipInstance.dispose();  
                }
            }
            else if (timeDifference < 6 * 60 * 60 * 1000) {
                cell.style.color = '#999';
                cell.setAttribute("title", "6小時內不可預約");
                cell.setAttribute("data-bs-custom-class", "custom-tooltip-unavailable");
            }

            //如果時段還沒被預約, 加入confirmBookingModal事件
            else if (!isBooked(date, time, bookedSlots)) {
                cell.addEventListener("click", () => {
                    confirmBookingModalCourseTitle.textContent = courseTitle;
                    confirmBookingModalTutorHeadshot.src = tutorHeadShot;

                    selectedBookingDate = date;
                    selectedBookingTime = time;

                    confirmBookingModalDate.textContent = `${formatDate(date)} (${standardWeekdays[date.getDay()]
                        })`;
                    confirmBookingModalTime.textContent = time;
                    confirmBookingModal.show();

                    //將選定的時間戳寫入local storage
                    localStorage.setItem("selectedBookingTimeStamp", bookingTime.getTime());
                });
                cell.setAttribute("title", "預約此時段");
                cell.setAttribute("data-bs-custom-class", "custom-table-tooltip");
            }
            column.appendChild(cell);
        }
        bookingTableBody.appendChild(column);
    }

    // Initialize tooltips
    const tooltipTriggerList = Array.from(document.querySelectorAll("[title]"));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            delay: { show: 100, hide: 0 },
        });
    });
}

//非模組化環境使用時, 將function掛載到window上
if (typeof window !== "undefined") {
    window.generateBookingTable = generateBookingTable;
}


//產生00:00 ~ 23:00 的時間段陣列
function generateTimeSlots() {
    const times = [];
    for (let hour = 0; hour < 24; hour++) {
        times.push(`${String(hour).padStart(2, "0")}:00`);
    }
    return times;
}

//把date轉換成yyyy-mm-dd
function formatDate(date) {
    let newDate = new Date(date);
    let year = newDate.getFullYear();
    let month = String(newDate.getMonth() + 1).padStart(2, '0');
    let day = String(newDate.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
}

function isBooked(date, time, bookedSlots) {
    return bookedSlots.some(bs =>
        formatDate(bs.date) == formatDate(date) && `${String(bs.startHour - 1).padStart(2, "0")}:00` == time);
}

function inTutorTime(weekday, time) {
    for (const tutorSlot of tutorSlots) {
        if (tutorSlot.weekday == weekday && `${String(tutorSlot.startHour - 1).padStart(2, "0")}:00` == time) {
            return true;
        }
    }
    return false;
}

//前一周&後一周換頁鈕
prevWeekBtn.addEventListener("click", () => {
    bookingDateStart.setDate(bookingDateStart.getDate() - 7);
    generateBookingTable(bookingDateStart, globCourseId);
});

nextWeekBtn.addEventListener("click", () => {
    bookingDateStart.setDate(bookingDateStart.getDate() + 7);
    generateBookingTable(bookingDateStart, globCourseId);
});

//提交預約表單 並存入local storage (for購物車使用)
addToCartBtn.addEventListener("click", function () {
    document.getElementById("formCourseId").value = globCourseId;
    document.getElementById("formBookingDate").value = selectedBookingDate.toLocaleDateString('zh-TW');
    document.getElementById("formBookingTime").value = parseInt(selectedBookingTime.split(':')[0]) + 1;
    document.getElementById("addToCartForm").submit();
    localStorage.setItem("CourseId", globCourseId);
    localStorage.setItem("BookingDate", selectedBookingDate.toLocaleDateString('zh-TW'));
    localStorage.setItem("BookingTime", parseInt(selectedBookingTime.split(':')[0]) + 1);
})

//fetch BookingTable API
async function fetchBookingTableData(courseId) {
    const url = `/api/BookingTableApi?courseId=${courseId}`;
    try {
        const response = await fetch(url);

        if (!response.ok) {
            console.error(`網路發生錯誤, status: ${response.status} `);
            return null;
        }

        const bookingTableData = await response.json();
        if (!bookingTableData) {
            console.error('沒有fetching到任何BookingTable資料!');
            return null;
        }

        return bookingTableData;

    } catch (error) {
        console.error('Fetching BookingTableData時發生錯誤:', error);
    }
}

// 排程掃描檢查local storage是否有booking的時間戳
setInterval(() => {
    const currentTimestamp = Date.now();
    const selectedBookingTimeStamp = localStorage.getItem("selectedBookingTimeStamp");

    //檢查booking的時間戳是否存在, 以及目前的時間是否大於預約時間 + 6小時
    if (selectedBookingTimeStamp && currentTimestamp > (parseInt(selectedBookingTimeStamp) + 6 * 60 * 60 * 1000)) {
        alert("目前時間已超過此預約的時段，更新頁面");
        localStorage.removeItem("selectedBookingTimeStamp");

        location.reload();
    }
}, 60000)  //每分鐘檢查一次

