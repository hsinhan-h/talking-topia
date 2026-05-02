// 螢幕寬度小於767px時對應變化
function moveUserItem() {
    const screenWidth = window.innerWidth;
    const userItem = document.querySelector('.user-item');
    const navbarNav = document.querySelector('.navbar-nav');

    if (screenWidth <= 767) {
        // 如果螢幕寬度小於等於 767px，將 user-item 移動到 navbar-nav 的最前面
        if (userItem && navbarNav) {
            navbarNav.insertBefore(userItem, navbarNav.firstChild);
        }
    } else {
        // 如果螢幕寬度大於 767px，將 user-item 移動回 ms-auto 的最後面
        const navbarNavAuto = document.querySelector('.navbar-nav.ms-auto');
        if (userItem && navbarNavAuto) {
            navbarNavAuto.appendChild(userItem);
        }
    }
}




//選擇課程選單

//document.addEventListener('DOMContentLoaded', function () {
//    const firstColumnLinks = document.querySelectorAll('.first-column a');
//    const secondColumn = document.querySelector('.second-column');
//    //const thirdColumn = document.querySelector('.third-column');
//    const subMenus = document.querySelectorAll('.second-column .sub-menu');

//    firstColumnLinks.forEach(link => {
//        link.addEventListener('mouseenter', function () {
//            // 隱藏所有的子選單
//            subMenus.forEach(subMenu => {
//                subMenu.style.display = 'none';
//            });

//            // 顯示與此 link 對應的子選單
//            const targetMenu = document.querySelector(`.second-column .${this.dataset.target}`);
//            if (targetMenu) {
//                targetMenu.style.display = 'block';
//            }

//            // 顯示第二欄和第三欄
//            secondColumn.style.display = 'block';
//            /*thirdColumn.style.display = 'block';*/
//        });
//    });

//    // 當滑鼠移出第一欄時隱藏第二、第三欄
//    const firstColumn = document.querySelector('.first-column');
//    firstColumn.addEventListener('mouseleave', function () {
//        secondColumn.style.display = 'none';
//    //    thirdColumn.style.display = 'none';
//    });

//     //讓第二、第三欄不會在滑鼠移到其他地方時消失
//    secondColumn.addEventListener('mouseenter', function () {
//        secondColumn.style.display = 'block';
//    //    thirdColumn.style.display = 'block';
//    });

//    //thirdColumn.addEventListener('mouseenter', function () {
//    //    secondColumn.style.display = 'block';
//    //    thirdColumn.style.display = 'block';
//    //});

//    secondColumn.addEventListener('mouseleave', function () {
//        secondColumn.style.display = 'none';
//    //    thirdColumn.style.display = 'none';
//    });

//    //thirdColumn.addEventListener('mouseleave', function () {
//    //    secondColumn.style.display = 'none';
//    //    thirdColumn.style.display = 'none';
//    });






////Search
//$(document).ready(function () {
//    $('#difysearch').on('submit', function (e) {
//        e.preventDefault(); // 防止表單的預設提交

//        var query = $('#courseSearchInput').val();

//        $.ajax({
//            url: '/Search/Index',
//            type: 'GET',
//            data: { query: query },
//            success: function (response) {
//                if (response.success) {
//                    // 如果成功，使用返回的 redirectUrl 進行跳轉
//                    window.location.href = response.redirectUrl;
//                } else {
//                    // 如果找不到資料，顯示“無資料”
//                    $('.second-column').html('<p class="text-muted">' + response.message + '</p>');
//                }
//            },
//            error: function () {
//                $('.second-column').html('<p class="text-muted">搜尋時出現錯誤，請稍後再試</p>');
//            }
//        });
//    });
//});

//$(document).ready(function () {
//    // 防抖函數，延遲 500 毫秒後才執行搜尋提交
//    function debounce(func, delay) {
//        let timer;
//        return function (...args) {
//            clearTimeout(timer);
//            timer = setTimeout(() => func.apply(this, args), delay);
//        };
//    }

//    // 在搜尋輸入框上使用防抖函數，監聽 input 事件
//    $('#courseSearchInput').on('input', debounce(function () {
//        // 自動提交表單或進行 AJAX 請求
//        $('#difysearch').submit();
//    }, 500));
//});


// 檢查 Vue 實例是否已存在，存在的話先卸載
if (window.difyApp) {
    window.difyApp.unmount();
}

// 創建新的 Vue 實例
window.difyApp = Vue.createApp({
    data() {
        return {
            productTitle: '', // 用於 Dify 搜尋推薦
            difyCreateButtonStatus: false,
            isLoading: false,
            searchQuery: '', // 用於搜尋功能
            searchResultMessage: '' // 搜尋結果訊息
        };
    },
    methods: {
        // 透過 Dify 取得推薦課程分類
        generateProductDetailsByDify() {
            this.showLoading();

            fetch("/api/Dify/CreateSearchRecommendation", {
                method: 'POST',
                body: JSON.stringify({
                    "product_name": this.productTitle
                }),
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => response.json())
                .then(data => {
                    if (data.isSuccess) {
                        const categoryName = data.body.category_name;
                        this.productDescription = data.body.description;

                        // 根據分類名稱進行頁面跳轉
                        if (categoryName) {
                            this.navigateToCategoryPage(categoryName);
                        }
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                })
                .finally(() => {
                    this.hideLoading();
                });
        },
        // 根據分類名稱跳轉頁面
        navigateToCategoryPage(categoryName) {
            let url = '';
            switch (categoryName) {
                case '英文':
                    url = '/Course/CourseList?page=1&subject=英文';
                    break;
                case '日文':
                    url = '/Course/CourseList?page=1&subject=日文';
                    break;
                case '中文':
                    url = '/Course/CourseList?page=1&subject=中文';
                    break;
                case '德文':
                    url = '/Course/CourseList?page=1&subject=德文';
                    break;
                case '法文':
                    url = '/Course/CourseList?page=1&subject=法文';
                    break;
                case '西班牙文':
                    url = '/Course/CourseList?page=1&subject=西班牙文';
                    break;
                case 'HTML/CSS':
                    url = '/Course/CourseList?page=1&subject=HTML/CSS';
                    break;
                case 'JavaScript':
                    url = '/Course/CourseList?page=1&subject=JavaScript';
                    break;
                case 'C#':
                    url = '/Course/CourseList?page=1&subject=C%23';
                    break;
                case 'SQL':
                    url = '/Course/CourseList?page=1&subject=SQL';
                    break;
                case 'Python':
                    url = '/Course/CourseList?page=1&subject=Python';
                    break;
                case 'Java':
                    url = '/Course/CourseList?page=1&subject=Java';
                    break;
                case '數學':
                    url = '/Course/CourseList?page=1&subject=數學';
                    break;
                case '物理':
                    url = '/Course/CourseList?page=1&subject=物理';
                    break;
                case '化學':
                    url = '/Course/CourseList?page=1&subject=化學';
                    break;
                case '歷史':
                    url = '/Course/CourseList?page=1&subject=歷史';
                    break;
                case '地理':
                    url = '/Course/CourseList?page=1&subject=地理';
                    break;
                case '生物':
                    url = '/Course/CourseList?page=1&subject=生物';
                    break;
                default:
                    url = '/Course/CourseList';
            }

            // 使用 window.location.href 進行跳轉
            window.location.href = url;
        },
        // 顯示 Loading 畫面
        showLoading() {
            this.isLoading = true;
        },
        // 隱藏 Loading 畫面
        hideLoading() {
            this.isLoading = false;
        },
        // 搜尋課程
        searchCourses() {
            if (this.searchQuery.trim() === '') {
                this.searchResultMessage = '請輸入搜尋內容';
                return;
            }

            fetch(`/Search/Index?query=${encodeURIComponent(this.searchQuery)}`, {
                method: 'GET'
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        // 如果成功，使用返回的 redirectUrl 進行跳轉
                        window.location.href = data.redirectUrl;
                    } else {
                        // 顯示無資料訊息
                        this.searchResultMessage = data.message || '無資料';
                    }
                })
                .catch(error => {
                    console.error('Error:', error);
                    this.searchResultMessage = '搜尋時出現錯誤，請稍後再試';
                });
        }
    },
    watch: {
        productTitle: function (val) {
            this.difyCreateButtonStatus = val.length >= 2 && val.length <= 30;
        }
    }
});

// 挂載 Vue 實例
window.difyApp.mount('#dify-search-app');


//const app = Vue.createApp({
//    data() {
//        return {
//            productTitle: '', // 用於 Dify 搜尋推薦
//            difyCreateButtonStatus: false,
//            isLoading: false,
//            searchQuery: '', // 用於搜尋功能
//            searchResultMessage: '' // 搜尋結果訊息
//        };
//    },
//    methods: {
//        // 透過 Dify 取得推薦課程分類
//        generateProductDetailsByDify() {
//            this.showLoading();

//            fetch("/api/Dify/CreateSearchRecommendation", {
//                method: 'POST',
//                body: JSON.stringify({
//                    "product_name": this.productTitle
//                }),
//                headers: {
//                    'Content-Type': 'application/json'
//                }
//            })
//                .then(response => response.json())
//                .then(data => {
//                    if (data.isSuccess) {
//                        const categoryName = data.body.category_name;
//                        this.productDescription = data.body.description;

//                        // 根據分類名稱進行頁面跳轉
//                        if (categoryName) {
//                            this.navigateToCategoryPage(categoryName);
//                        }
//                    }
//                })
//                .catch(error => {
//                    console.error('Error:', error);
//                })
//                .finally(() => {
//                    this.hideLoading();
//                });
//        },
//        // 根據分類名稱跳轉頁面
//        navigateToCategoryPage(categoryName) {
//            let url = '';
//            switch (categoryName) {
//                case '英文':
//                    url = '/Course/CourseList?page=1&subject=英文';
//                    break;
//                case '日文':
//                    url = '/Course/CourseList?page=1&subject=日文';
//                    break;
//                case '中文':
//                    url = '/Course/CourseList?page=1&subject=中文';
//                    break;
//                case '德文':
//                    url = '/Course/CourseList?page=1&subject=德文';
//                    break;
//                case '法文':
//                    url = '/Course/CourseList?page=1&subject=法文';
//                    break;
//                case '西班牙文':
//                    url = '/Course/CourseList?page=1&subject=西班牙文';
//                    break;
//                case 'HTML/CSS':
//                    url = '/Course/CourseList?page=1&subject=HTML/CSS';
//                    break;
//                case 'JavaScript':
//                    url = '/Course/CourseList?page=1&subject=JavaScript';
//                    break;
//                case 'C#':
//                    url = '/Course/CourseList?page=1&subject=C%23';
//                    break;
//                case 'SQL':
//                    url = '/Course/CourseList?page=1&subject=SQL';
//                    break;
//                case 'Python':
//                    url = '/Course/CourseList?page=1&subject=Python';
//                    break;
//                case 'Java':
//                    url = '/Course/CourseList?page=1&subject=Java';
//                    break;             
//                case '數學':
//                    url = '/Course/CourseList?page=1&subject=數學';
//                    break;
//                case '物理':
//                    url = '/Course/CourseList?page=1&subject=物理';
//                    break;
//                case '化學':
//                    url = '/Course/CourseList?page=1&subject=化學';
//                    break;
//                case '歷史':
//                    url = '/Course/CourseList?page=1&subject=歷史';
//                    break;
//                case '地理':
//                    url = '/Course/CourseList?page=1&subject=地理';
//                    break;
//                case '生物':
//                    url = '/Course/CourseList?page=1&subject=生物';
//                    break;
//                // 添加其他分類的處理
//                default:
//                    url = '/Course/CourseList';
//            }

//            // 使用 window.location.href 進行跳轉
//            window.location.href = url;
//        },
//        // 顯示 Loading 畫面
//        showLoading() {
//            this.isLoading = true;
//        },
//        // 隱藏 Loading 畫面
//        hideLoading() {
//            this.isLoading = false;
//        },
//        // 搜尋課程
//        searchCourses() {
//            if (this.searchQuery.trim() === '') {
//                this.searchResultMessage = '請輸入搜尋內容';
//                return;
//            }

//            fetch(`/Search/Index?query=${encodeURIComponent(this.searchQuery)}`, {
//                method: 'GET'
//            })
//                .then(response => response.json())
//                .then(data => {
//                    if (data.success) {
//                        // 如果成功，使用返回的 redirectUrl 進行跳轉
//                        window.location.href = data.redirectUrl;
//                    } else {
//                        // 顯示無資料訊息
//                        this.searchResultMessage = data.message || '無資料';
//                    }
//                })
//                .catch(error => {
//                    console.error('Error:', error);
//                    this.searchResultMessage = '搜尋時出現錯誤，請稍後再試';
//                });
//        }
//    },
//    watch: {
//        productTitle: function (val) {
//            this.difyCreateButtonStatus = val.length >= 5 && val.length <= 30;
//        }
//    }
//});

//app.mount('#app');

