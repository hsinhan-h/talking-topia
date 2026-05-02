
function checkLoginStatus(callback) {
    fetch('/api/FindMember/IsLoggedIn')
        .then(response => response.json())
        .then(data => {
            if (!data.isLoggedIn) {
                // 如果未登入，重導向到登入頁面
                window.location.href = '/Account/Account';
            } else {
                // 如果已登入，執行其他邏輯
                callback();
            }
        });
}

const { createApp, ref } = Vue;
const courseId = document.getElementById('app').dataset.courseId;
const memberId = document.getElementById('app').dataset.memberId;

const call = createApp({
    data() {
        return {
            rating: null,
            newReviewContent:'',
            fetchedRating: null ,
            courseId: courseId,      // 從 DOM 中取得的課程 ID
            isFollowing: false,  // 初始關注狀態，從後端來決定是否已關注
            FollowerId: memberId,       // 假設當前使用者的 ID
            FollowedCourseId: courseId,    // 假設要關注的對象 ID
            courseReviews: [],        // 用來儲存課程評論列表
            selectedRatings: [],
            tutorName: [],
            bookedLessons: [],
            hasCommented: false,
            hasTakenClass: false,
            commentIsChecked: null
        }
    },
    computed: {
        // 根據選中的評分篩選評論
        filteredReviews() {
            console.log('Selected Ratings:', this.selectedRatings);
            if (this.selectedRatings.length === 0) {
                return this.courseReviews;
            }
            const filtered = this.courseReviews.filter(review =>
                this.selectedRatings.includes(Number(review.CommentRating))
            );
            console.log('Filtered Reviews:', filtered);
            return filtered;
        }
    },
    methods: {
        // 根據評分顯示不同的描述
        getRatingText(rating) {
            switch (rating) {
                case 5: return '非常好';
                case 4: return '很好';
                case 3: return '普通';
                case 2: return '不好';
                case 1: return '非常糟';
                default: return '';
            }
        },
        getRatingPercentage(rating) {
            const totalReviews = this.courseReviews.length;
            if (totalReviews === 0) return 0;  // 避免除以 0 的錯誤
            const ratingCount = this.courseReviews.filter(review => review.CommentRating === rating).length;
            return ((ratingCount / totalReviews) * 100).toFixed(1);
        },
        reviewContentChecked() {            
            fetch('/api/OpenAI/SubmitContent', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(this.newReviewContent)
            })
                .then(response => response.json())
                .then(data => {
                    // 根據返回結果進行處理
                    if (data.success) {
                        // 如果審查通過，允許提交評論
                        toastr.success('評論內容很棒喔！');
                        this.commentIsChecked=true; 
                    } else {
                        // 如果審查未通過，顯示警告通知
                        toastr.warning(data.message);
                    }
                })
                .catch(error => console.error('Error:', error));
        },
        submitRating() {
            // 檢查是否已經上課
            if (!this.hasTakenClass) {
                toastr.error('您尚未上過課程，無法提交評論');
                return; // 阻止提交
            }

            // 檢查是否已經評論過
            if (this.hasCommented) {
                toastr.warning('您已經提交過評論，無法再次提交');
                return; // 阻止提交
            }
            if (this.rating === null) {
                toastr.warning('您尚未為課程評分!!');
                return; // 阻止提交
            }
            if (this.newReviewContent === '') {
                toastr.warning('您尚未填寫評論內容!!');
                return; // 阻止提交
            }
            checkLoginStatus(() => {
                // 構建 FormData 並提交 rating 資料
                let formData = new FormData();
                formData.append('Rating', this.rating);
                formData.append('NewReviewContent', this.newReviewContent);
                formData.append('CourseId', courseId);
                formData.append('MemberId', memberId);

                // 使用 fetch 提交資料到後端控制器
                fetch('/api/CourseReview/CreateCourseReview', {
                    method: 'POST',
                    body: formData
                })
                    .then(response => response.json())
                    .then(data => {
                        toastr.success('評論提交成功！');
                        this.rating = null;
                        this.newReviewContent = '';
                        this.hasCommented = true;
                        this.fetchCourseReviews();
                    })
                    .catch(error => {
                        toastr.error('提交時發生錯誤，請稍後再試');
                        console.error('錯誤:', error);
                    });
            });
        },
        fetchRatingFromServer() {
            // 從後端獲取 Rating 資料
            fetch(`/api/CourseReview/CourseReviewApi?courseId=${this.courseId}`) 
                .then(response => response.json()) // 解析成 JSON 格式
                .then(data => {
                    this.fetchedRating = data;  // 將獲取到的評分設置到 fetchedRating 中
                })
                .catch(error => {
                    console.error('錯誤:', error);
                });
        },
        fetchCourseReviews() {
            // 從後端 API 獲取課程評論列表
            fetch(`/api/CourseReview/GetCourseReviewList?courseId=${this.courseId}`)
                .then(response => response.json())
                .then(data => {
                    this.courseReviews = data; // 將評論列表數據存入 Vue.js 的 courseReviews
                })
                .catch(error => {
                    console.error('Error fetching course reviews:', error);
                });
        },
        fetchFollowStatusFromServer() {
            // 假設從後端 API 獲取當前是否已關注狀態
            fetch(`/api/Following/GetFollowStatus?courseId=${this.courseId}`)
                .then(response => response.json())
                .then(data => {
                    this.isFollowing = data;
                })
                .catch(error => {
                    console.error('錯誤:', error);
                });
        },
        fetchCommentCondition() {
            fetch(`/api/CourseReview/ReviewRules?courseId=${this.courseId}`)
                .then(response => response.json())
                .then(data => {
                    this.hasCommented = data.HasCommented,
                    this.hasTakenClass = data.HasTakenClass
                })
                .catch(error => {
                    console.error('錯誤:', error);
                });
        },
        // 關注功能
        follow() {
            checkLoginStatus(() => {
                fetch(`/api/Following/AddFollowing?courseId=${this.courseId}`, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        FollowerId: memberId,
                        FollowedCourseId: courseId,
                    }),
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            this.isFollowing = true;
                            toastr.success(data.message);
                        }
                    })
                    .catch(error => console.error('Error:', error));
            });
        },
        // 取消關注功能
        unfollow() {
            fetch(`/api/Following/DeleteFollowingCourse?courseId=${this.courseId}`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    FollowerId: memberId,
                    FollowedCourseId: courseId,
                }),
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        this.isFollowing = false;
                        toastr.success(data.message);
                    }
                })
                .catch(error => console.error('Error:', error));
        },
    },
    mounted() {
        // 在 Vue 應用掛載後自動呼叫 fetchRatingFromServer 來獲取資料
        this.fetchRatingFromServer();
        this.fetchFollowStatusFromServer();
        this.fetchCourseReviews();
        this.fetchCommentCondition();
    }

})

call.use(PrimeVue.Config);

call.component('p-rating', PrimeVue.Rating);

call.mount('#app');




const twentyfive_mins = document.querySelector("#min-25");
const fiftyfive_mins = document.querySelector("#min-50");

fiftyfive_mins.addEventListener("click", () => {
    const tfmins_coursr_group = document.querySelector(".min-25");
    const ftmins_coursr_group = document.querySelector(".min-50");
    ftmins_coursr_group.classList.remove("d-none");
    tfmins_coursr_group.classList.add("d-none",);
    fiftyfive_mins.classList.add("course-info-tab-color");
    twentyfive_mins.classList.remove("course-info-tab-color");
});

twentyfive_mins.addEventListener("click", () => {
    const tfmins_coursr_group = document.querySelector(".min-25");
    const ftmins_coursr_group = document.querySelector(".min-50");
    tfmins_coursr_group.classList.remove("d-none");
    ftmins_coursr_group.classList.add("d-none");
    fiftyfive_mins.classList.remove("course-info-tab-color");
    twentyfive_mins.classList.add("course-info-tab-color");
})


