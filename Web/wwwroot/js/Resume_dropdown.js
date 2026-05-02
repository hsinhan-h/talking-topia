var data_categories = [
    {
        "name": "語言學習",
        "id": 1,
        "subcategories": [
            {
                "name": "法文",
                "id": 1
            },
            {
                "name": "中文",
                "id": 2
            },
            {
                "name": "日文",
                "id": 3
            },
            {
                "name": "西班牙",
                "id": 4
            },
            {
                "name": "德文",
                "id": 5
            },
            {
                "name": "英文",
                "id": 6
            }
        ]
    },
    {
        "name": "程式設計",
        "id": 2,
        "subcategories": [
            {
                "name": "HTML/CSS",
                "id": 7
            },
            {
                "name": "JavaScript",
                "id": 8
            },
            {
                "name": "C#",
                "id": 9
            },
            {
                "name": "SQL",
                "id": 10
            },
            {
                "name": "Python",
                "id": 11
            },
            {
                "name": "Java",
                "id": 12
            }
        ]
    },
    {
        "name": "升學科目",
        "id": 3,
        "subcategories": [
            {
                "name": "數學",
                "id": 13
            },
            {
                "name": "物理",
                "id": 14
            },
            {
                "name": "化學",
                "id": 15
            },
            {
                "name": "歷史",
                "id": 16
            },
            {
                "name": "地理",
                "id": 17
            },
            {
                "name": "生物",
                "id": 18
            }
        ]
    }
];
const { createApp } = Vue;
const appresume = createApp({
    data() {
        return {
            selectedCategory: '',
            selectedSubcategory: '',
            commonData: {},
            licenses: [
                {
                    ProfessionalLicenseId: null,
                    ProfessionalLicenseName: '',
                    ProfessionalLicenseUrl: null,
                    licenseEditMode: false,
                    uploadStatus: null,
                    isUploading: false,
                }
            ],
            works: [
                {
                    workName: '',
                    workStartDate: '',
                    workEndDate: '',
                    workExperienceFile: null,
                    workExperienceId: null,
                    workEditMode: false,
                    uploadWorkStatus: null,
                    isWorkUploading: false,
                }
            ],
            formSubmitted: false,
            selectedFile: null,
            headShotImage: headImage,
            headImageUpdated: false,
            confirmheadShotImg: true,
            editMode: false,
            HeadImgisUploading: false,
            showValidation: false,
            formIsValid: true,
            checkAIbool: false,
            imagesData:[],
        };
    },
    computed: {
        categories() {
            return this.commonData['raw_data'] || [];
        },
        subcategories() {
            if (this.selectedCategory) {
                return this.commonData['category_' + this.selectedCategory]?.subcategories || [];
            }
            return [];
        },
        selectedCategoryName() {
            if (this.selectedCategory) {
                return this.commonData['category_' + this.selectedCategory]?.name || '';
            }
            return '';
        },
        selectedSubcategoryName() {
            if (this.selectedSubcategory) {
                return this.commonData['subcategory_' + this.selectedSubcategory]?.name || '';
            }
            return '';
        },
        canAddWorkExperience() {
            return this.works.length < 3;
        },
        canAddLicens() {
            return this.licenses.length < 3;
        }
    },
    created() {
        this.convertData(data_categories);
        this.loadHeadShotImage()
    },

    mounted() {
        this.fetchBackendData();
        this.fetchAIimgs();
        window.addEventListener('beforeunload', this.beforeUnloadHandler);
    },
    beforeDestroy() {
        // 在元件銷毀前移除監聽事件
        window.removeEventListener('beforeunload', this.beforeUnloadHandler);
    },
    methods: {
        async fetchAIimgs() {
            const memberId = localStorage.getItem('memberId');

            try {
                const response = await fetch(`/api/UpdateResume/GetApplyListImages?memberId=${memberId}`, {
                    method: 'GET',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                });

                if (!response.ok) {
                    throw new Error(`錯誤: ${response.statusText}`);
                }

                const data = await response.json();

                console.log('API 回傳資料data:', data);
                if (data.success) {
                    this.imagesData = data.data;
                    console.log('API 回傳資料data.data:', data.data);
                } else {
                    this.errorMessage = data.message;
                }
            } catch (error) {
                this.errorMessage = `無法取得圖片資料: ${error.message}`;
            }
        },
        async UpdateFinaleAIImageUrl(imageUrl) {
            try {
                const memberId = localStorage.getItem('memberId');

                if (!memberId) {
                    throw new Error('會員 ID 未找到，請重新登入。');
                }

                const response = await fetch('/api/UpdateResume/UpdateAIimg', {
                    method: 'PUT',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        memberId: parseInt(memberId),
                        url: imageUrl
                    }),
                });

                if (!response.ok) {
                    throw new Error(`錯誤: ${response.statusText}`);
                }

                const result = await response.json();
                console.log('更新結果:', result);

                if (result.success) {
                    toastr.success('圖片更新成功！');
                    this.loadHeadShotImage();

                } else {
                    toastr.error('圖片更新失敗！');
                }
            } catch (error) {
                toastr.error('圖片更新失敗！');
                this.errorMessage = `無法更新圖片資料: ${error.message}`;
            }
        },
        changeToAICheck() {
            const memberId = localStorage.getItem('memberId');
            if (!memberId) {
                console.log("無法獲取會員 ID");
                return;
            }
            fetch(`/api/UpdateResume/CheckAIStatus/`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    memberId: memberId,
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        toastr.success('已收到您的需求');
                        console.log('AI狀態更新成功');
                    } else {
                        toastr.info('您必須成為教師後才有此服務');
                        console.log('AI狀態更新失敗: ' + data.message);
                    }
                })
                .catch(error => {
                    toastr.error('系統維修中');
                    console.error('更新 AI 狀態時出錯:', error);
                });
        },
        convertData(data) {
            let result = {};
            result["raw_data"] = data;
            data.forEach(category => {
                result["category_" + category.id] = category;
                category.subcategories.forEach(subcategory => {
                    result["subcategory_" + subcategory.id] = subcategory;
                });
            });
            this.commonData = result;
        },
        fetchBackendData() {
            fetch('/api/ApplyCourseList/ResumeApplyCouseData', {
                method: 'GET',
                headers: {
                    'Content-Type': 'application/json',
                }
            })
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Network response was not ok');
                    }
                    return response.json();
                })
                .then(data => {
                    if (!data.success) {
                        throw new Error(data.message || 'Unknown error');
                    }

                    const courseList = data.data.applycoursedata.courseList;
                    this.selectedCategory = courseList.applyCourseCategoryId;
                    this.selectedSubcategory = courseList.applySubCategoryId;

                    const licenseList = data.data.professionalLicense;
                    this.licenses = licenseList.professionalLicenseName.map((name, index) => ({
                        ProfessionalLicenseName: name,
                        ProfessionalLicenseUrl: licenseList.professionalLicenseUrl[index] || null,
                        ProfessionalLicenseId: licenseList.professionalLicenseId[index]
                    }));
                    const workexpList = data.data.workexp.workBackground;
                    if (workexpList && workexpList.length > 0) {
                        this.works = workexpList.map(work => ({
                            workName: work.workName || '',
                            workStartDate: work.workStartDate || '',
                            workEndDate: work.workEndDate || '',
                            workExperienceFile: null,
                            workExperienceId: work.workExperienceId || null
                        }));
                    }
                })
                .catch(error => {
                    console.error('Error fetching course list:', error);
                });
        },
        numberToChinese(num) {
            const chineseNumbers = ['零', '一', '二', '三', '四', '五', '六', '七', '八', '九'];

            if (num === 10) {
                return '十';
            }

            let result = '';
            const tens = Math.floor(num / 10);
            const ones = num % 10;

            if (tens === 1) {
                result += '十';
            } else if (tens > 1) {
                result += chineseNumbers[tens] + '十';
            }

            if (ones !== 0) {
                result += chineseNumbers[ones];
            }

            return result;
        },

        addLicense() {
            if (!this.canAddLicens) return;
            const newLicense = {
                ProfessionalLicenseName: '',
                ProfessionalLicenseUrl: null,
                ProfessionalLicenseId: null,
                valid: false,
            };
            this.licenses.push(newLicense);

            fetch('/api/UpdateResume/CreateLicense', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    memberId: localStorage.getItem('memberId')
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        this.licenses[this.licenses.length - 1].ProfessionalLicenseId = data.professionalLicenseId;
                    } else {
                        console.log('Failed to create license: ' + data.message);
                    }
                })
                .catch(error => {
                    console.error('Error creating license:', error);
                });
        },
        enditLcense(index) {
            const updatedLicense = this.licenses[index];
            const memberId = localStorage.getItem('memberId');

            if (!updatedLicense.ProfessionalLicenseName) {
                console.log('請填寫證照名稱');
                this.licenses[index].uploadStatus = false;
                return;
            }

            if (!updatedLicense.ProfessionalLicenseUrl) {
                console.log('請上傳證照文件');
                this.licenses[index].uploadStatus = false;
                return;
            }

            this.licenses[index].licenseEditMode = false;
            this.licenses[index].isUploading = true;

            const formData = new FormData();
            formData.append('memberId', memberId);
            formData.append('ProfessionalLicenseId', updatedLicense.ProfessionalLicenseId || 0);
            formData.append('ProfessionalLicenseName', updatedLicense.ProfessionalLicenseName);

            formData.append('ProfessionalLicenseUrl',
                updatedLicense.ProfessionalLicenseUrl instanceof File
                    ? updatedLicense.ProfessionalLicenseUrl
                    : updatedLicense.ProfessionalLicenseUrl.toString()
            );

            fetch('/api/UpdateResume/UpdateResumeLicenses', {
                method: 'POST',
                body: formData
            })
                .then(response => response.json())
                .then(data => {
                    this.licenses[index].isUploading = false;
                    if (data.success) {
                        console.log('License updated successfully');
                        this.licenses[index].uploadStatus = true;

                        if (!this.licenses[index].ProfessionalLicenseId) {
                            this.licenses[index].ProfessionalLicenseId = data.professionalLicenseId;
                        }
                    } else {
                        console.log('Failed to update license:', data.message);
                        this.licenses[index].uploadStatus = false;
                    }
                })
                .catch(error => {
                    this.licenses[index].isUploading = false;
                    console.error('Error updating license:', error);
                    this.licenses[index].uploadStatus = false;
                });
        },
        removeLicense(index) {
            const licenseToRemove = this.licenses[index];

            if (licenseToRemove.ProfessionalLicenseId) {
                const memberId = localStorage.getItem('memberId');
                fetch('/api/UpdateResume/DeleteLicense', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        memberId: memberId,
                        ProfessionalLicenseId: [licenseToRemove.ProfessionalLicenseId]
                    })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            this.licenses.splice(index, 1);
                        } else {
                            console.log('Failed to delete license: ' + data.message);
                        }
                    })
                    .catch(error => {
                        console.error('Error deleting license:', error);
                    });
            }
            else {
                this.licenses.splice(index, 1);
            }
        },
        handleFileUpload(event, index) {
            const file = event.target.files[0];
            if (file) {
                this.licenses[index].ProfessionalLicenseUrl = file;
            }
        },
        addWorkExperience() {
            if (!this.canAddWorkExperience) return;
            //新增工作經驗欄位
            const newWorkExperience = {
                workName: '',
                workStartDate: '',
                workEndDate: '',
                workExperienceFile: null
            };
            this.works.push(newWorkExperience);
            fetch('/api/UpdateResume/CreateWorkExperience', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    memberId: localStorage.getItem('memberId')
                })
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        this.works[this.works.length - 1].workExperienceId = data.workExperienceId;
                    }
                    else {
                        console.log('Failed to create license: ' + data.message);
                    }
                }).catch(error => {
                    console.error('Error creating license:', error);
                });
        },
        handleWorkFileUpload(event, index) {
            const file = event.target.files[0];
            if (file) {
                this.works[index].workExperienceFile = file;
            }
        },

        // 移除工作經歷欄位
        removeWorkExperience(index) {
            const workexpToRemove = this.works[index];

            if (workexpToRemove.workExperienceId) {
                const memberId = localStorage.getItem('memberId');
                fetch('/api/UpdateResume/DeleteWorkExp', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({
                        memberId: memberId,
                        WorkBackground: [
                            {
                                WorkExperienceId: workexpToRemove.workExperienceId
                            }
                        ]
                    })
                })
                    .then(response => response.json())
                    .then(data => {
                        if (data.success) {
                            this.works.splice(index, 1);
                        } else {
                            console.log('Failed to delete license: ' + data.message);
                        }
                    })
                    .catch(error => {
                        console.error('Error deleting license:', error);
                    });
            }
            else {
                this.works.splice(index, 1);
            }
        },
        editWorkExp(index) {
            const updatedworkexp = this.works[index];
            const memberId = localStorage.getItem('memberId');

            if (!updatedworkexp.workName || !updatedworkexp.workExperienceFile || !updatedworkexp.workStartDate || !updatedworkexp.workEndDate) {
                console.log('請填寫所有必填的工作信息');
                this.works[index].uploadWorkStatus = false;
                return;
            }

            this.works[index].isWorkUploading = true;
            /*this.works[index].isConfirmed = true;// 提交條件*/
            this.works[index].workEditMode = false;

            const formData = new FormData();
            formData.append('memberId', memberId);

            formData.append(`WorkBackground[0].WorkExperienceId`, updatedworkexp.workExperienceId || 0);
            formData.append(`WorkBackground[0].WorkName`, updatedworkexp.workName || '');
            formData.append(`WorkBackground[0].WorkStartDate`, updatedworkexp.workStartDate || '');
            formData.append(`WorkBackground[0].WorkEndDate`, updatedworkexp.workEndDate || '');

            if (updatedworkexp.workExperienceFile instanceof File) {
                formData.append(`WorkBackground[0].WorkExperienceFile`, updatedworkexp.workExperienceFile);
            }

            fetch('/api/UpdateResume/UpdateResumeWorkExp', {
                method: 'POST',
                body: formData,
            })
                .then(response => response.json())
                .then(data => {
                    this.works[index].isWorkUploading = false;
                    if (data.success) {
                        this.works[index].uploadWorkStatus = true;

                        // 更新 WorkExperienceId
                        if (!this.works[index].workExperienceId) {
                            this.works[index].workExperienceId = data.workExperienceId;
                        }

                        console.log('Work experience updated successfully');
                    } else {
                        console.log('Failed to update work experience: ' + data.message);
                        this.works[index].uploadWorkStatus = false;
                    }
                })
                .catch(error => {
                    this.works[index].isWorkUploading = false;
                    console.error('Error updating work experience:', error);
                });
        },

        validateForm() {
            this.formIsValid = true;

            // 檢查大頭貼是否提供
            if (this.headShotImage == null) {
                console.log('請提供大頭貼');
                this.formIsValid = false;
            }

            // 檢查每個證照是否填寫完整
            this.licenses.forEach((license, index) => {
                if (!license.ProfessionalLicenseName) {
                    console.log(`證照 ${index + 1} 欄位未填寫完畢`);
                    this.formIsValid = false;
                }
            });
            this.works.forEach((work, index) => {
                if (
                    !work.workName ||
                    !work.workStartDate ||
                    !work.workEndDate
                ) {
                    console.log(`工作經歷 ${index + 1} 欄位未填寫完畢`);
                    this.formIsValid = false;
                }
            });
            // 如果表單不符合驗證條件，顯示提示視窗
            if (!this.formIsValid) {
                const validationModal = new bootstrap.Modal(document.getElementById('validationModal'));
                validationModal.show();
                return;
            }
            window.removeEventListener('beforeunload', this.beforeUnloadHandler);

            // 表單通過驗證後提交
            this.$refs.form.submit();
        },
        beforeUnloadHandler(e) {
            this.removeIncompleteLicenses();
            this.removeIncompletework();
            e.preventDefault();
            e.returnValue = '您有尚未完成的表單，確定要離開此頁面嗎？';
        },
        removeIncompleteLicenses() {
            // 遍歷所有證照並移除未填寫完整的證照
            this.licenses.forEach((license, index) => {
                if (!license.ProfessionalLicenseName) {
                    this.removeLicense(index);
                }
            });
            console.log('未填寫的證照已移除');
        },
        removeIncompletework() {
            // 遍歷所有證照並移除未填寫完整的證照
            this.works.forEach((work, index) => {
                if (
                    !work.workName ||
                    !work.workStartDate ||
                    !work.workEndDate
                ) {
                    this.removeWorkExperience(index);
                }
            });
            console.log('未填寫的工作已移除');
        },
        checkHeadFile() {
            const HeadImage = this.selectedFile;

            if (!HeadImage) {
                this.confirmheadShotImg = false;
            } else {
                this.confirmheadShotImg = true;
                this.onFileChange();
            }
        },
        onFileChange() {
            const HeadImage = this.selectedFile;
            const memberId = localStorage.getItem('memberId');

            this.headImageUpdated = true;
            this.HeadImgisUploading = true;
            this.confirmheadShotImg = true;


            const formData = new FormData();
            formData.append('memberId', memberId);

            if (HeadImage instanceof File) {
                formData.append('HeadShotImage', HeadImage);
            } else if (HeadImage) {
                formData.append('HeadShotImage', HeadImage.toString());
            } else {
                formData.append('HeadShotImage', '');
            }

            fetch('/api/UpdateResume/UpdateHeadShotImage', {
                method: 'POST',
                body: formData,
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        console.log('Profile picture updated successfully');
                        this.loadHeadShotImage();
                        this.HeadImgisUploading = false;

                    } else {
                        console.log('Failed to update profile picture: ' + data.message);
                        this.HeadImgisUploading = false;
                    }
                })
                .catch(error => {
                    console.error('Error updating profile picture:', error);
                    this.HeadImgisUploading = false;
                });


        },
        HeadImgChange(event) {
            this.selectedFile = event.target.files[0];
        },
        loadHeadShotImage() {
            const memberId = localStorage.getItem('memberId');
            this.editMode = false;
            fetch(`/api/UpdateResume/GetHeadShotImage?memberId=${memberId}`)
                .then(response => response.json())
                .then(data => {
                    if (data.success && data.headShotImage) {
                        this.headShotImage = data.headShotImage;
                        localStorage.setItem('headShotImage', data.headShotImage);
                    } else {
                        this.headShotImage = '';
                    }
                })
                .catch(error => {
                    console.error(`Error fetching headshot image: ${error}`);
                });
        },
        toggleEditMode() {
            this.editMode = !this.editMode;
        },
        toggleLicenseEditMode(index) {
            this.licenses[index].licenseEditMode = !this.licenses[index].licenseEditMode;

            // 如果重新進入編輯模式，則取消“確認上傳”的禁用
            if (this.licenses[index].licenseEditMode) {
                this.licenses[index].isConfirmed = false;
            }
        },
        toggleWorkEditMode(index) {
            this.works[index].workEditMode = !this.works[index].workEditMode;

            // 如果重新進入編輯模式，則取消“確認上傳”的禁用
            if (this.works[index].workEditMode) {
                this.works[index].isConfirmed = false;  // 取消確認狀態
            }
        },
        downloadFile(index) {
            // 獲取會員 ID
            const memberId = localStorage.getItem('memberId');
            const professionalLicenseId = this.licenses[index]?.ProfessionalLicenseId;

            if (!memberId || !professionalLicenseId) {
                console.log("無法獲取會員 ID 或 ProfessionalLicenseId");
                return;
            }

            // 發送 POST 請求以獲取 ProfessionalLicenseUrl
            fetch(`/api/UpdateResume/DownloadImg`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({
                    memberId: memberId,
                    ProlLicenseId: professionalLicenseId,
                })
            })
                .then(response => response.json())
                .then(data => {
                    const modalImg = document.getElementById('modalImage');
                    const modalMessage = document.getElementById('modalMessage');

                    if (data.success && data.professionalLicenseUrl) {
                        const previewUrl = data.professionalLicenseUrl;

                        modalMessage.textContent = '';
                        modalImg.src = previewUrl;
                        modalImg.style.display = 'block';

                    } else {
                        modalMessage.textContent = "您尚未上傳證照圖檔";
                        modalImg.style.display = 'none';  // 隱藏圖片
                    }
                    const modal = new bootstrap.Modal(document.getElementById('imagePreviewModal'));
                    modal.show();
                })
                .catch(error => {
                    console.error(`Error fetching image: ${error}`);

                    const modalMessage = document.getElementById('modalMessage');
                    modalMessage.textContent = "無法下載圖片，請稍後再試";
                    const modalImg = document.getElementById('modalImage');
                    modalImg.style.display = 'none';  // 隱藏圖片

                    // 顯示 Bootstrap modal
                    const modal = new bootstrap.Modal(document.getElementById('imagePreviewModal'));
                    modal.show();
                });
        },
    }
});
appresume.mount('#vue-wrappers');
