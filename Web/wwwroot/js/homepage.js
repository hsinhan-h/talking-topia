
$(document).ready(function () {
   
    // 監聽 tab 點擊事件
    $('.lh-layout-tab').on('click', function () {
        // 移除所有 tab 的 active 樣式
        $('.lh-layout-tab').removeClass('active');
        // 為當前點擊的 tab 添加 active 樣式
        $(this).addClass('active');

        // 獲取對應的 category 名稱 (中文)
        var category = $(this).data('category');

        loadCourses(category);
    });

    // 頁面載入時模擬點擊預設的 tab
    $('.lh-layout-tab.active').trigger('click');

    // 發送 AJAX 請求來獲取對應分類的課程
    function loadCourses(category) {
        $.ajax({
            url: '/Home/GetCoursesByCategory',
            type: 'GET',
            data: { categoryName: category },
            success: function (data) {
                renderCourses(data);  // 成功獲取數據後渲染
            },
            error: function (err) {
                console.error("Failed to load courses:", err);
            }
        });
    }

    // 渲染課程卡片
    function renderCourses(courses) {
        var courseList = $('#lh-layout-course-list');
        courseList.empty(); // 清空現有內容

        // 動態渲染課程
        courses.forEach(function (course) {
            var courseItem = `
            <a href="/Course/CourseList?page=1&subject=${encodeURIComponent(course.subjectName)}&sortOption=default" class="lh-layout-course-item">
                <img src="${course.tutorHeadShotImage}" alt="Teacher's Image" />
                <h3>${course.subjectName}</h3>
            </a>
        `;
            courseList.append(courseItem); // 動態添加每個課程卡片
        });
    }
});