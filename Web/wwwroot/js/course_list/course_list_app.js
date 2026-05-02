import { generateBookingTable } from '/js/booking_table/booking_table_modal.js';
import { initHoverPopup } from '/js/course_list/course_list_hoverPopup.js';
import { autoPlayYouTubeModal } from '/js/course_list/course_list_youtubeModal.js';
import { initTooltips } from '/js/utilities/tooltips.js';
import { getWeekdayName } from '/js/utilities/weekday_mapping.js';

const courseCardsApp = Vue.createApp({
    data() {
        return {
            courses: [],
            page: 1,
            pageSize: 6,
            totalPages: 0,
            error: null,
            loading: true,
            noCoursesFound: false,
            selectedSubject: null,
            selectedNation: null,
            selectedWeekdays: [],
            selectedTimeslots: [],
            selectedBudget: null,
            selectedSortOption: 'default',
            availableSlots: [], //二維陣列, 元素為各課程的教師時段Array
            bookedSlots: [], //二維陣列, 元素為各課程的被預約時段Array          
            courseCategories: [], //動態科目篩選選單資料
            nations: []  //動態國籍篩選選單資料
        };
    },
    mounted() {
        const params = new URLSearchParams(window.location.search);
        this.page = parseInt(params.get('page')) || 1; //從query string取得page
        this.selectedSubject = params.get('subject') ? decodeURIComponent(params.get('subject')) : null;
        this.selectedNation = params.get('nation') || null;
        const weekdaysParam = params.get('weekdays');
        if (weekdaysParam) {
            this.selectedWeekdays = decodeURIComponent(weekdaysParam).split(',').map(day => parseInt(day));
        };
        const timeslotsParam = params.get('timeslots');
        if (timeslotsParam) {
            this.selectedTimeslots = decodeURIComponent(timeslotsParam).split(',');
        }
        this.selectedBudget = params.get('budget') || null;
        this.fetchCourses();
        this.fetchCategories();
        this.fetchNations();
    },
    updated() {
        //DOM 已更新完後, 重新呼叫slick function & tooltips & modals
        this.$nextTick(() => {
            initHoverPopup();
            initTooltips();
            autoPlayYouTubeModal();
        });
    },
    methods: {
        fetchCoursesDebounced: _.debounce(function () {
            this.fetchCourses();
        }, 500), //延遲500ms觸發fetch

        async fetchCourses() {
            this.loading = true;
            this.noCoursesFound = false;
            try {
                let url = `/api/CourseListApi?page=${this.page}`;
                if (this.selectedSubject) {
                    url += `&subject=${encodeURIComponent(this.selectedSubject)}`;
                }
                if (this.selectedNation) {
                    url += `&nation=${this.selectedNation}`;
                }
                if (this.selectedWeekdays.length > 0) {
                    url += `&weekdays=${this.selectedWeekdays.join(',')}`;
                }
                if (this.selectedTimeslots.length > 0) {
                    url += `&timeslots=${this.selectedTimeslots.join(',')}`;
                }
                if (this.selectedBudget) {
                    url += `&budget=${this.selectedBudget}`;
                }
                if (this.selectedSortOption && this.selectedSortOption !== 'default') {
                    url += `&sortOption=${this.selectedSortOption}`;
                }

                const response = await fetch(url);
                if (response.ok) {
                    const courseData = await response.json();
                    console.log(courseData);
                    this.courses = courseData.courseInfoList;
                    this.totalPages = Math.ceil(courseData.totalCourseQty / this.pageSize);
                    this.availableSlots = [];
                    this.bookedSlots = [];
                    if (this.courses.length > 0) {
                        this.availableSlots = this.courses.map(course => course.availableTimeSlots);
                        this.bookedSlots = this.courses.map(course => course.bookedTimeSlots);
                    } else {
                        this.noCoursesFound = true;
                    }
                } else {
                    throw new Error(`Error: ${response.status} ${response.statusText}`);
                }
            } catch (e) {
                this.error = e;
            } finally {
                this.loading = false;
            }
        },
        goToCourseMainPage(courseId) {
            window.location.href = `/Course/CourseMainPage?courseId=${courseId}`;
        },
        formatPrice(price) {
            return price.toLocaleString('en-US');
        },
        openBookingTable(courseId) {
            this.$nextTick(() => {
                generateBookingTable(new Date(), courseId);

                const bookingTableModal = new bootstrap.Modal(document.getElementById('bookingTableModal'));
                bookingTableModal.show();
            });
        },
        addBookingStatusClass(startHour, endHour, weekday, index) {
            const isAvailable = this.availableSlots[index].some(slot => slot.weekday === weekday && slot.startHour >= startHour && slot.startHour < endHour);

            const today = new Date();
            const currentDayOfWeek = today.getDay();
            const startOfWeek = new Date(today);
            startOfWeek.setDate(today.getDate() - currentDayOfWeek);
            const endOfWeek = new Date(startOfWeek);
            endOfWeek.setDate(startOfWeek.getDate() + 6);
            const isOccupied = this.bookedSlots[index].some(slot => {
                const bookedDate = new Date(slot.date);
                return bookedDate >= startOfWeek && bookedDate <= endOfWeek &&
                    bookedDate.getDay() === weekday && slot.startHour >= startHour && slot.startHour < endHour;
            });
            if (isOccupied) {
                return 'occupied';
            }
            else if (isAvailable) {
                return 'available';
            }
            return '';
        },

        //篩選選單動態資料
        async fetchCategories() {
            try {
                const response = await fetch('/api/CourseCategoryApi');
                if (response.ok) {
                    const courseCategoriesData = await response.json();
                    this.courseCategories = courseCategoriesData;
                }
            } catch (e) {
                this.error = e;
            } finally {

                this.loading = false;
            }
        },
        async fetchNations() {
            try {
                const response = await fetch('/api/NationApi');
                if (response.ok) {
                    const nationNameData = await response.json();
                    this.nations = nationNameData;
                }
            } catch (e) {
                this.error = e;
            } finally {

                this.loading = false;
            }
        },

        //換頁 不刷新頁面
        goToPage(page) {
            if (page > 0 && page <= this.totalPages) {
                this.page = page;
                this.fetchCourses();
                this.updateQueryString();
                history.pushState(null, '', `?page=${this.page}`);

            }
        },
        updateQueryString() {
            const queryParams = new URLSearchParams(window.location.search);
            queryParams.set('page', this.page);
            if (this.selectedSubject) {
                queryParams.set('subject', this.selectedSubject);
            } else {
                queryParams.delete('subject');
            }

            if (this.selectedNation) {
                queryParams.set('nation', this.selectedNation);
            } else {
                queryParams.delete('nation');
            }

            if (this.selectedWeekdays && this.selectedWeekdays.length > 0) {
                queryParams.set('weekdays', this.selectedWeekdays.join(','));
            } else {
                queryParams.delete('weekdays');
            }

            if (this.selectedTimeslots && this.selectedTimeslots.length > 0) {
                queryParams.set('timeslots', this.selectedTimeslots.join(','));
            } else {
                queryParams.delete('timeslots');
            }

            if (this.selectedBudget) {
                queryParams.set('budget', this.selectedBudget);
            } else {
                queryParams.delete('budget');
            }

            if (this.selectedSortOption) {
                queryParams.set('sortOption', this.selectedSortOption);
            } else {
                queryParams.delete('sortOption');
            }

            history.pushState(null, '', '?' + queryParams.toString());
        },

        getWeekdayName,

        //篩選
        //1. 課程種類
        filterBySubject(subject) {
            this.selectedSubject = subject;
            this.applyFilter();
        },
        //2. 國籍
        filterByNation(nation) {
            this.selectedNation = nation;
            this.applyFilter();
        },
        //3. 時段
        filterByWeekdayAndTimeSlot() {
            //v-model已綁定, 不用再push到selectedWeekdays和selectedTimeslots
            this.applyFilter();
        },

        //4. 預算區間
        filterByBudget(budget) {
            this.selectedBudget = budget;
            this.applyFilter();
        },

        applyFilter() {
            this.page = 1;
            this.fetchCoursesDebounced();
            //this.fetchTotalCourseQty();
            this.updateQueryString();
        },

        //取消篩選
        clearSubjectFilter() {
            this.selectedSubject = null;
            this.applyFilter();
        },
        clearNationFilter() {
            this.selectedNation = null;
            this.applyFilter();
        },
        clearWeekdayAndTimeslotFilter() {
            this.selectedWeekdays = [];
            this.selectedTimeslots = [];
            this.applyFilter();
        },
        clearBudgetFilter() {
            this.selectedBudget = null;
            this.applyFilter();
        },
        clearAllFilter() {
            this.selectedSubject = null;
            this.selectedNation = null;
            this.selectedWeekdays = [];
            this.selectedTimeslots = [];
            this.selectedBudget = null;
            this.applyFilter();
        },

        //排序
        //1. 即時推薦
        sortByDefault(e) {
            e.preventDefault();
            this.selectedSortOption = "default";
            this.applyFilter();
        },

        //2.優質教師優先
        sortByVerifiedTutor(e) {
            e.preventDefault();
            this.selectedSortOption = "verifiedTutor";
            this.applyFilter();
        },

        //3. 低價優先
        sortByPriceAscend(e) {
            e.preventDefault();
            this.selectedSortOption = "priceAscend";
            console.log(this.selectedSortOption);
            this.applyFilter();
        },

        //4. 多評價數優先
        sortByReviewsCount(e) {
            e.preventDefault();
            this.selectedSortOption = "reviewsCount";
            this.applyFilter();
        },

        //5. 評分高優先
        sortByRating(e) {
            e.preventDefault();
            this.selectedSortOption = "rating";
            this.applyFilter();
        },


        //關注
        toggleFollow(courseCard) {
            //TODO: 檢查使用者登入狀態, 
            fetch('/api/FindMember/IsLoggedIn')
                .then(response => response.json())
                .then(data => {
                    if (!data.isLoggedIn) {
                        //1. 如果未登入, 導向登入頁 
                        window.location.href = '/Account/Account';
                    } else {
                        //2. 如果已登入, 將memberId存入localStorage並進行關注操作    
                        localStorage.setItem('memberId', data.memberId);

                        const url = courseCard.followingStatus
                            ? `/api/Following/DeleteFollowingCourse`
                            : `/api/Following/AddFollowing`;

                        const watchViewModel = {
                            FollowerId: localStorage.getItem('memberId'),
                            FollowedCourseId: courseCard.courseId
                        };

                        //發送POST請求
                        fetch(url, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json'
                            },
                            body: JSON.stringify(watchViewModel)
                        })
                            .then(response => response.json())
                            .then(data => {
                                if (data.success) {
                                    //更新課程關注狀態
                                    courseCard.followingStatus = !courseCard.followingStatus;

                                    //彈出toastr
                                    if (courseCard.followingStatus) {
                                        toastr.success("已關注此課程!");
                                    } else {
                                        toastr.info("已取消關注此課程!");
                                    }
                                }
                            })
                            .catch(error => {
                                console.error('Error:', error);
                            });
                    }
                })

        }
    }
});

courseCardsApp.mount('#course-cards-app');
