document.addEventListener("DOMContentLoaded", function () {
    let isFormSubmitted = false;
    const submitBtn = document.querySelector('.submitButton');
    const memberId = localStorage.getItem('memberId'); 
    document.querySelectorAll('.memberInfoItem').forEach(item => {
        if (item.value) {
            item.disabled = true;
        }
    });
    let isEditing = false; // 用來判斷是否處於編輯狀態
    const spinner = submitBtn.querySelector('.submitspinner');

   
    submitBtn.addEventListener('click', function (event) {
        // 如果按鈕是處於編輯狀態，阻止提交表單
        if (!isEditing) {
            event.preventDefault();
            // 允許編輯
            document.querySelectorAll('.memberInfoItem').forEach(item => {
                item.disabled = false;
            });
            // 改變按鈕文本為 "提交"
            submitBtn.childNodes[0].textContent = "提交";  // 只更改文本部分
            // 更改狀態，讓下一次按下時可以進行提交
            isFormSubmitted = true
            isEditing = true;
        } else {
            // 顯示 spinner
            spinner.classList.remove('d-none');
            // 第二次按下則允許表單提交
            document.querySelector('.tutorDataTimeForm').submit();
            
        }
    });

    const submitTimeBtn = document.querySelector('.submitTimeButton');
    submitTimeBtn.addEventListener('click', function () {

        const hasChecked = Array.from(document.querySelectorAll('.form-check-input.Weekday')).some(checkbox => checkbox.checked);

        if (!hasChecked) {
            
            toastr.info('請至少勾選一個時段！');
            event.preventDefault(); 
            return; 
        }

        // 取得 spinner 元素
        const spinner = document.querySelector('.spinner-grow');

        // 顯示 spinner
        spinner.classList.remove('d-none');

        // 提交表單
        isFormSubmitted = true
        document.querySelector('.tutorDataTimeForm').submit();
        
    });

    var toastElement = document.getElementById('toast');
    var message = toastElement.getAttribute('data-message');

    // 確保 message 和 header 都存在且非空
    if (message && message.trim() !== "" ) {
        toastr.success(`${message}`);
    }

    const weekdays = [
        "monday",
        "tuesday",
        "wednesday",
        "thursday",
        "friday",
        "saturday",
        "sunday",
    ];
    const timeslotRow = document.getElementById("timeslot-row");//父層

    weekdays.forEach((day) => {
        // 創建隱藏的時段區塊
        const timeslotDiv = document.createElement("div");
        timeslotDiv.className = "col-12 col-md-6 mt-3 weekday-timeslot";
        timeslotDiv.id = `timeslot-${day}`;
        timeslotDiv.style.display = "none"; // 初始隱藏

        // 添加標題
        const title = document.createElement("span");
        title.className = "fw-bolder";
        title.textContent = `${day.charAt(0).toUpperCase() + day.slice(1)}時段`;
        timeslotDiv.appendChild(title);

        // 創建時間選項
        const timeslotWrapper = document.createElement("div");
        timeslotWrapper.className = "d-flex flex-wrap mt-2";

        // 創建上午、下午、傍晚、深夜的選擇框
        const timePeriods = [
            { label: "上午", value: "morning", range: [6, 12] },
            { label: "中午", value: "afternoon", range: [12, 18] },
            { label: "傍晚", value: "evening", range: [18, 24] },
            { label: "深夜", value: "lateNight", range: [0, 6] },
        ];

        timePeriods.forEach((period) => {
            const formCheck = document.createElement("div");
            formCheck.className = "form-check col-3 mb-3";

            const input = document.createElement("input");
            input.className = `form-check-input ${day}-period`;
            input.type = "checkbox";
            input.value = period.value;
            input.id = `${day}-${period.value}`;

            const label = document.createElement("label");
            label.className = "form-check-label fw-bold";
            label.htmlFor = input.id;
            label.textContent = period.label;

            formCheck.appendChild(input);
            formCheck.appendChild(label);
            timeslotWrapper.appendChild(formCheck);

            // 根據選擇框狀態勾選或取消勾選對應的時段
            input.addEventListener("change", function () {
                const checkboxes = timeslotDiv.querySelectorAll(`input.${day}-time`);
                checkboxes.forEach((checkbox) => {
                    const hour = parseInt(checkbox.value, 10);
                    if ((hour - 1) >= period.range[0] && (hour - 1) < period.range[1]) {
                        checkbox.checked = this.checked;
                    }
                });
            });
        });

        // 創建具體時間選項
        const dayIndex = getDayIndex(day); // 這裡的 getDayIndex 是你用來決定 day 的索引（例如星期一是 1）

        for (let hour = 0; hour < 24; hour++) {
            const timeValue = hour.toString().padStart(2, "0") + ":00";
            const checkboxId = `${day}-${hour}`;

            const formCheck = document.createElement("div");
            formCheck.className = "form-check col-3";

            const input = document.createElement("input");
            input.className = `form-check-input ${day}-time`;
            input.type = "checkbox";

            const adjustedValue = (hour + 1).toString(); // 1 對應 0:00, 2 對應 1:00 ...
            input.value = adjustedValue.toString(); // 設定 value 從 1 到 24

            input.id = checkboxId;
            // 使用 dayIndex 來構造正確的 name 屬性
            input.name = `Schedule[${dayIndex}].CouseHoursId[]`; // 這裡設置 name 為類似 Schedule[1].CouseHoursId[]

            const label = document.createElement("label");
            label.className = "form-check-label";
            label.htmlFor = checkboxId;
            label.textContent = timeValue; // 時間顯示為 00:00, 01:00 等

            formCheck.appendChild(input);
            formCheck.appendChild(label);
            timeslotWrapper.appendChild(formCheck);
        }

        // 輔助函數，用來將 day 轉換為 dayIndex，例如 'monday' 對應 1
        function getDayIndex(day) {
            const daysMap = {
                'monday': 1,
                'tuesday': 2,
                'wednesday': 3,
                'thursday': 4,
                'friday': 5,
                'saturday': 6,
                'sunday': 0
            };
            return daysMap[day.toLowerCase()];
        }

        timeslotDiv.appendChild(timeslotWrapper);
        timeslotRow.appendChild(timeslotDiv);

        // 選擇日選擇框
        const dayCheckboxes = document.querySelectorAll('input[class="form-check-input Weekday"]');

        // 為每個日選擇框添加變更事件
        dayCheckboxes.forEach((checkbox, index) => {
            checkbox.addEventListener("change", function () {
                const timeslotDiv = document.getElementById(`timeslot-${weekdays[index]}`);
                timeslotDiv.style.display = this.checked ? "block" : "none"; // 根據選擇框狀態顯示或隱藏
            });
        });
    });

    //渲染card用
    if (memberId !== null) {
        const apiUrl = `/api/GetTutorReserveApi/GetTutorReserveTimeJson?memberId=${memberId}`;

        fetch(apiUrl)
            .then(response => response.json())
            .then(data => {
                console.log('API response:', data); // 查看返回的資料

                // 檢查是否有回傳的 Json 裡 availableReservation 資料
                if (!data || !data.availableReservation || !Array.isArray(data.availableReservation)) {
                    throw new Error('error');
                }

                const editButton = document.getElementById('edit-button');
                if (data.availableReservation.length > 0) {
                    updateReservationList(data.availableReservation);
                    editButton.classList.remove('d-none');
                    editButton.classList.add('d-block');
                    // 禁用全局 checkbox
                    document.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                        checkbox.disabled = true;
                    });
                }

                function getWeeking(dayNumber) {
                    const weekdayMapping = {
                        0: '星期日',
                        1: '星期一',
                        2: '星期二',
                        3: '星期三',
                        4: '星期四',
                        5: '星期五',
                        6: '星期六'
                    };
                    return weekdayMapping[dayNumber];
                }

                function updateReservationList(reservations) {
                    const reservationList = document.getElementById("reservation-list");
                    reservationList.innerHTML = ""; // 清空列表

                    const groupedReservations = {}; // 用來按 weekday 分組

                    // 將 reservation(JSON) 根據 weekday (KEY) 分組
                    reservations.forEach(reservation => {
                        const day = reservation.weekday;//Json中的key
                        if (!groupedReservations[day]) {
                            groupedReservations[day] = []; // 如果還沒有這個 weekday，則新建一個陣列
                        }
                        groupedReservations[day].push(reservation); // 將 reservation 加入對應的 weekday 的object
                    });

                    // 生成每個 weekday 的卡片區塊
                    Object.keys(groupedReservations).forEach(day => {
                        const dayReservations = groupedReservations[day];
                        const cardDiv = document.createElement("div");
                        cardDiv.classList.add("card", "m-2");
                        cardDiv.style.width = "18rem";

                        // 創建卡片 header 顯示星期幾
                        const cardHeader = document.createElement("div");
                        cardHeader.classList.add("card-header");
                        cardHeader.textContent = getWeeking(day); // 顯示星期

                        const timeList = document.createElement("ul");
                        timeList.classList.add("list-group", "list-group-flush");

                        // 創建該星期的時段列表
                        dayReservations.forEach(reservation => {
                            const listItem = document.createElement("li");
                            listItem.classList.add("list-group-item");

                            const startTime = reservation.coursehours.split(":")[0]; // 取得開始小時部分


                            listItem.textContent = `${getWeeking(day)} ${startTime}:00`;
                            timeList.appendChild(listItem);
                        });

                        cardDiv.appendChild(cardHeader);
                        cardDiv.appendChild(timeList);
                        reservationList.appendChild(cardDiv); // 把每個星期的卡片加入到 reservationList
                    });
                }

                //以下是fetch後渲染checkbox用


                // 要確認資料庫的星期幾順序
                const weekdayMap = {
                    0: 'sunday',
                    1: 'monday',
                    2: 'tuesday',
                    3: 'wednesday',
                    4: 'thursday',
                    5: 'friday',
                    6: 'saturday',
                };

                const timePeriodMap = {
                    morning: [6, 12],    // 上午 6:00 - 12:00
                    afternoon: [12, 18], // 下午 12:00 - 18:00
                    evening: [18, 24],   // 傍晚 18:00 - 24:00
                    lateNight: [0, 6]    // 深夜 0:00 - 6:00
                };

                const selectedWeekdays = new Set();  // 存放所有選中的星期，避免重複處理

                // 處理 availableReservation 資料
                data.availableReservation.forEach(reservation => {
                    const weekday = weekdayMap[reservation.weekday]; // 根據數字轉換為星期幾的名稱
                    const timesDiv = document.getElementById(`timeslot-${weekday}`);

                    // 勾選對應的星期
                    if (!selectedWeekdays.has(reservation.weekday)) {
                        const weekdayCheckbox = document.getElementById(`weekday-${reservation.weekday}`);
                        if (weekdayCheckbox) {
                            weekdayCheckbox.checked = true;
                            weekdayCheckbox.disabled = true; // 禁止編輯
                            selectedWeekdays.add(reservation.weekday); // 防止重複勾選
                        }
                    }

                    if (timesDiv) {
                        timesDiv.style.display = "block"; // 顯示時段區塊

                        // 勾選具體的 coursehours (如: 07:00, 08:00 等)
                        const timeCheckbox = document.getElementById(`${weekday}-${parseInt(reservation.coursehours.split(":")[0])}`);
                        if (timeCheckbox) {
                            timeCheckbox.checked = true;
                            timeCheckbox.disabled = true; // 禁止編輯
                        }

                        // 處理時段的勾選 (上午、下午、傍晚、深夜)
                        const hour = parseInt(reservation.coursehours.split(":")[0]);
                        Object.keys(timePeriodMap).forEach(period => {
                            const [start, end] = timePeriodMap[period];
                            if (hour >= start && hour < end) {
                                const periodCheckbox = document.getElementById(`${weekday}-${period}`);
                                if (periodCheckbox) {
                                    periodCheckbox.checked = true;
                                    periodCheckbox.disabled = true; // 禁止編輯
                                }
                            }
                        });
                    }
                });
            })
            .catch(error => {
                console.error('Error fetching or processing data:', error);
            });


        // 事件監聽器，用來控制時段選項的顯示和隱藏
        const weekdayInputs = document.querySelectorAll('input[name="weekday"]');
        weekdayInputs.forEach(function (input) {
            input.addEventListener("change", function () {
                const timesweekDiv = document.getElementById(`timeslot-${this.id}`); // 選到生成的星期區塊
                if (timesweekDiv) {
                    if (this.checked) {
                        timesweekDiv.style.display = "block";
                    } else {
                        timesweekDiv.style.display = "none";
                        // 當取消選擇該星期時，同時取消勾選所有對應的時間段
                        const checkboxes = timesweekDiv.querySelectorAll('input[type="checkbox"]');
                        checkboxes.forEach(function (checkbox) {
                            checkbox.checked = false;
                        });
                    }
                }
            });
        });

    }
    // 點擊事件內調用函數
    document.getElementById("edit-button").addEventListener("click", function () {
        const enablecheckbox = document.querySelectorAll('input[type="checkbox"]');
        enablecheckbox.forEach(checkbox => { checkbox.disabled = false })
    });
});
