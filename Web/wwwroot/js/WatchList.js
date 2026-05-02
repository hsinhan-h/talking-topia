const { createApp, ref } = Vue;

const tutorCardComponent = {
    props: ['card'],
    template: `
        <div v-if="card" class="lh-tutor-card py-3 px-4 position-relative" :data-courseid="card.CourseId">
            <div class="lh-tutor-card__header mx-auto d-flex flex-column align-items-center mx-auto">
                <div class="lh-tutor-images__head position-relative mt-1 mb-2">
                    <img class="tutor-headshot rounded-circle" :src="card.TutorHeadShot" />
                    <img class="tutor-nation-icon rounded-circle border border-white position-absolute" :src="card.NationFlagImg.replace('~', '')" />
                    <div class="tutor-online-status"></div>
                </div>
                <div class="tutor-head-brief-pricing">
                    <span class="fifty-min-course-pricing fw-bolder">NT$ {{ card.FiftyminPrice }}</span><span class="text"> / 50分鐘</span>
                </div>
            </div>
            <div class="lh-tutor-card__body">
                <div class="tutor-title-and-ranking d-flex justify-between align-items-center">
                    <span class="tutor-description-title col-8">{{ card.CourseTitle }}</span>
                    <div class="tutor-ranking col-4 d-flex justify-content-end">
                        <i class="bi bi-star-fill pe-1"></i><span>{{ card.Rating }}</span>
                    </div>
                </div>
                <div class="tutor-subtitle mb-3">{{ card.CourseSubTitle }}</div>
                <p class="tutor-intro-text mb-2">{{ card.Description }}</p>
                <div class="course-pricing d-flex justify-content-between mb-2">
                    <div class="trail-course-pricing">
                        NT$ {{ card.TwentyFiveMinPrice }} <div class="text">25分鐘</div>
                    </div>
                    <div class="fifty-min-course-pricing">
                        NT$ {{ card.FiftyminPrice }} <div class="text">50分鐘</div>
                    </div>
                </div>
                <div class="contact-btns-wrapper d-flex gap-1">
                    <button type="button" class="lh-tutor-card__book-btn col-12" @click="$emit('book-course', card.CourseId)">
                        前往課程頁面
                    </button>
                </div>
            </div>
        </div>
    `
};

const watch = createApp({
    components: {
        'tutor-card': tutorCardComponent
    },
    data() {
        return {
            activeTab: 'language',  // 預設顯示語言學習
            languageWatchList: [],
            codingWatchList: [],
            schoolWatchList: [],
            isLoading: true
        };
    },
    methods: {
        switchTab(tab) {
            this.activeTab = tab;
            this.$nextTick(() => {
                this.initializeSlick();
            });
        },
        bookCourse(courseId) {
            window.location.href = `/Course/CourseMainPage?courseId=${courseId}`;
        },
        initializeSlick() {
            const tutorRecommendCarousel = $('.tutor-recommend-carousel-wrapper');
            const arrowPrev = tutorRecommendCarousel.find('.arrow_prev');
            const arrowNext = tutorRecommendCarousel.find('.arrow_next');

            let itemCount = 0;
            if (this.activeTab === 'language') {
                itemCount = this.languageWatchList.length;
            } else if (this.activeTab === 'coding') {
                itemCount = this.codingWatchList.length;
            } else if (this.activeTab === 'school') {
                itemCount = this.schoolWatchList.length;
            }

            if (itemCount === 0) {
                return; // 如果項目數量為 0，則不初始化 Slick，避免出現錯誤
            }

            const slidesToShow = itemCount > 5 ? 5 : itemCount;

            if (!$('.tutor-recommend-carousel').hasClass('slick-initialized')) {
                $('.tutor-recommend-carousel').slick({
                    infinite: true,
                    autoplay: false,
                    slidesToShow: slidesToShow,
                    slidesToScroll: 1,
                    responsive: [
                        {
                            breakpoint: 480,
                            settings: {
                                slidesToShow: 1,
                                slidesToScroll: 1,
                            },
                        }
                    ],
                });
            }
        }
    },
    mounted() {
        // 使用 fetch API 獲取 watchList 資料
        fetch('/api/TutorCard/GetWatchListCard')
            .then(response => response.json())
            .then(data => {
                this.languageWatchList = data.Result.LanguageWatchList || [];
                this.codingWatchList = data.Result.CodingWatchList || [];
                this.schoolWatchList = data.Result.SchoolWatchList || [];

                this.isLoading = false;

                this.$nextTick(() => {
                    this.initializeSlick();
                });
            })
            .catch(error => {
                console.error('Error fetching watchlist data:', error);
                this.isLoading = false;
            });
    }
});

watch.mount('#app');