//密碼修改
function submitPasswordChange() {
    let currentPassword = document.getElementById('currentPasswordInput').value;
    let newPassword = document.getElementById('newPasswordInput').value;
    let confirmPassword = document.getElementById('confirmPasswordInput').value;

    if (newPassword !== confirmPassword) {
        showToast("新密碼與確認密碼不相符");
        return;
    }

    // 呼叫後端 API 來修改密碼
    let requestData = {
        currentPassword: currentPassword,
        newPassword: newPassword,
        confirmNewPassword: confirmPassword  // 確保這裡包含確認密碼
    };

    fetch('/Member/ChangePassword', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(requestData)
    })
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(data => {
            if (data.success) {
                showToast('成功', '密碼修改成功');
                setTimeout(() => window.location.reload(), 2000); // 重新載入頁面
            } else {
                showToast('錯誤', data.message);
            }
        })
        .catch((error) => {
            console.error('Error:', error);
            showToast('錯誤', '密碼修改失敗，請稍後再試');
        });
}

// 即時驗證輸入框
function validateInput(input) {
    const errorSpanId = input.id + "-error";
    let errorMessage = "";

    if (input.value.trim() === "") {
        errorMessage = `${input.previousElementSibling.textContent}是必填的`;
    } else if (input.type === "email" && !validateEmail(input.value)) {
        errorMessage = "請輸入有效的電子郵件地址";
    } else if (input.type === "tel" && !validatePhone(input.value)) {
        errorMessage = "請輸入有效的電話號碼";
    } else if (input.id === "firstnameInput" && input.value.length > 50) {
        errorMessage = "名字不得超過50字元";
    } else if (input.id === "lastnameInput" && input.value.length > 50) {
        errorMessage = "姓氏不得超過50字元";
    } else if (input.id === "nicknameInput" && input.value.length > 50) {
        errorMessage = "暱稱不得超過50字元";
    }

    let errorSpan = document.getElementById(errorSpanId);
    if (!errorSpan) {
        errorSpan = document.createElement("span");
        errorSpan.id = errorSpanId;
        errorSpan.className = "text-danger";
        input.parentNode.appendChild(errorSpan);
    }

    if (errorMessage) {
        errorSpan.textContent = errorMessage;
    } else {
        errorSpan.textContent = "";
    }
}

// 表單驗證邏輯
function validateInput(input) {
    const errorSpanId = input.id + "-error";
    let errorMessage = "";

    if (input.disabled) {
        return; // 如果輸入框為禁用狀態，不進行驗證
    }

    // 檢查是否有標籤存在，如果沒有則設置具人性化的標題
    let label;
    if (input.previousElementSibling && input.previousElementSibling.tagName === 'LABEL') {
        label = input.previousElementSibling.textContent.trim();
    } else {
        // 根據 `input.id` 自定義一個更人性化的標籤
        switch (input.id) {
            case 'firstnameInput':
                label = '名字';
                break;
            case 'lastnameInput':
                label = '姓氏';
                break;
            case 'nicknameInput':
                label = '暱稱';
                break;
            case 'emailInput':
                label = '電子信箱';
                break;
            case 'phoneInput':
                label = '電話';
                break;
            case 'birthdayInput':
                label = '生日';
                break;
            default:
                label = input.id; // 使用 id 作為最後的備選
        }
    }

    // 生成錯誤訊息
    if (input.value.trim() === "") {
        errorMessage = `${label}欄位為必填`;
    } else if (input.type === "email" && !validateEmail(input.value)) {
        errorMessage = "請輸入有效的電子郵件地址";
    } else if (input.type === "tel" && !validatePhone(input.value)) {
        errorMessage = "請輸入有效的電話號碼";
    } else if (input.id === "firstnameInput" && input.value.length > 50) {
        errorMessage = "名字不得超過50字元";
    } else if (input.id === "lastnameInput" && input.value.length > 50) {
        errorMessage = "姓氏不得超過50字元";
    } else if (input.id === "nicknameInput" && input.value.length > 50) {
        errorMessage = "暱稱不得超過50字元";
    }

    // 確保存在錯誤訊息元素
    let errorSpan = document.getElementById(errorSpanId);
    if (!errorSpan) {
        errorSpan = document.createElement("span");
        errorSpan.id = errorSpanId;
        errorSpan.className = "text-danger";
        input.parentNode.appendChild(errorSpan);
    }

    // 顯示或清除錯誤訊息
    if (errorMessage) {
        errorSpan.textContent = errorMessage;
    } else {
        errorSpan.textContent = "";
    }
}

// 表單提交時進行驗證
function saveProfileData(event) {
    if (event) event.preventDefault(); // 阻止表單默認提交行為

    const inputs = document.querySelectorAll('#app input.form-control');
    let isValid = true;

    // 遍歷所有輸入框進行驗證
    inputs.forEach(input => {
        validateInput(input);
        const errorSpan = document.getElementById(input.id + "-error");
        if (errorSpan && errorSpan.textContent !== "") {
            isValid = false; // 如果有錯誤訊息，則驗證失敗
        }
    });

    if (!isValid) {
        showToast("錯誤", "請檢查所有必填欄位並修正錯誤");
        return;
    }

    // 如果驗證通過，則提交表單
    const gender = document.querySelector('input[name="gender"]:checked')?.value;
    const profileData = {
        Account: document.getElementById('accountInput').value,
        LastName: document.getElementById('lastnameInput').value,
        FirstName: document.getElementById('firstnameInput').value,
        Nickname: document.getElementById('nicknameInput').value,
        Gender: gender ? gender.toString() : "",
        Birthday: document.getElementById('birthdayInput').value,
        Email: document.getElementById('emailInput').value,
        Phone: document.getElementById('phoneInput').value,
        CoursePrefer: collectCoursePreferences()
    };

    $.ajax({
        type: "POST",
        url: '/Member/SaveProfile',
        data: JSON.stringify(profileData),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            if (response.success) {
                showToast("會員資料儲存", "儲存成功！");
                // 更新 Navbar 上的使用者名稱
                document.getElementById('navbar-username').textContent = 'Hi! ' + profileData.FirstName;
                // 切換按鈕顯示狀態
                toggleEditMode();
            } else {
                showToast("錯誤訊息", '儲存失敗，請重試。錯誤原因: ' + response.message);
            }
        },
        error: function (xhr, status, error) {
            showToast("錯誤訊息", '儲存過程中出現錯誤，請稍後再試。錯誤訊息: ' + xhr.responseText);
        }
    });
}

// 電子郵件驗證函數
function validateEmail(email) {
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
}

// 電話驗證函數
function validatePhone(phone) {
    const re = /^[0-9\-()+ ]+$/;
    return re.test(phone);
}

function collectCoursePreferences() {
    const selectedCourses = [];
    document.querySelectorAll('input[type="checkbox"]:checked').forEach(input => {
        selectedCourses.push({ SubjectName: input.value });
    });
    return selectedCourses;
}

// 初始化編輯模式的切換
function toggleEditMode() {
    // 取得所有需要編輯的文字輸入欄位
    const inputs = document.querySelectorAll('#app input.form-control');

    // 取得所有的 checkbox 元素
    const checkboxes = document.querySelectorAll('#app input[type="checkbox"]');

    // 取得所有的 radio button 元素
    const radioButtons = document.querySelectorAll('#app input[type="radio"]');

    //// 切換每個文字輸入欄位的 disabled 屬性
    //inputs.forEach(input => {
    //    // 對於帳號和電子信箱欄位，始終保持 disabled
    //    if (input.id !== 'accountInput' && input.id !== 'emailInput') {
    //        input.disabled = !input.disabled;

    //        // 如果啟用了編輯模式，為輸入框添加即時驗證
    //        if (!input.disabled) {
    //            input.addEventListener('input', function () {
    //                validateInput(input);
    //            });
    //        }
    //    }
    //});

    // 切換每個文字輸入欄位的 disabled 屬性
    inputs.forEach(input => {
        // 讓帳號和電子信箱欄位也可以被編輯
        input.disabled = !input.disabled;

        // 如果啟用了編輯模式，為輸入框添加即時驗證
        if (!input.disabled) {
            input.addEventListener('input', function () {
                validateInput(input);
            });
        }
    });

    // 切換每個 checkbox 的 disabled 屬性
    checkboxes.forEach(checkbox => {
        checkbox.disabled = !checkbox.disabled;
    });

    // 切換每個 radio button 的 disabled 屬性
    radioButtons.forEach(radio => {
        radio.disabled = !radio.disabled;
    });

    // 取得按鈕
    const editButton = document.getElementById('edit-button');
    const saveButton = document.getElementById('save-button');
    const cancelButton = document.getElementById('cancel-button');

    // 檢查目前的狀態，然後切換顯示狀態
    if (editButton.classList.contains('d-none')) {
        // 如果編輯按鈕目前隱藏，則顯示它，並隱藏儲存和取消按鈕
        editButton.classList.remove('d-none');
        saveButton.classList.add('d-none');
        cancelButton.classList.add('d-none');
    } else {
        // 否則，隱藏編輯按鈕，並顯示儲存和取消按鈕
        editButton.classList.add('d-none');
        saveButton.classList.remove('d-none');
        cancelButton.classList.remove('d-none');
    }
}

//showToast
function showToast(header, message) {
    const toastElement = document.getElementById('toast');
    const toastHeader = toastElement.querySelector('.toast-header strong');
    const toastBody = toastElement.querySelector('.toast-body');

    // 設定 toast 標題和訊息
    toastHeader.textContent = header;
    toastBody.textContent = message;

    // 顯示 toast
    const toast = new bootstrap.Toast(toastElement);
    toast.show();
}

