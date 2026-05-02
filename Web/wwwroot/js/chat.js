"use strict";

const senderId = window.senderId;
const sender = window.senderName;
const receiverId = window.receiverId;
const receiver = window.receiverName;
const messagesList = document.getElementById("messagesList");

async function startChat() {

    let connection = createSignalRConnection(senderId, receiverId);

    setupSignalREvents(connection, senderId, receiverId);

    try {
        await connection.start();
        console.log("SignalR 連線成功");
        console.log(`檢查 ${senderId} 和 ${receiverId} 是否加入同一個群組`);
    }
    catch (err) {
        console.error("SignalR 連線失敗：", err.toString());
    }

    document.getElementById("chat").style.display = "block";
    await loadChatHistory(senderId, receiverId);

    document.getElementById("sendButton").addEventListener("click", async (event) => {
        event.preventDefault();

        const message = document.getElementById("messageInput").value;

        console.log("Message:", message);

        if (!receiverId || !message) {
            alert("請輸入目標使用者 ID 和訊息內容！");
            return;
        }

        try {
            await connection.invoke("SendPrivateMessage", receiverId, message);
            document.getElementById("messageInput").value = "";

        } catch (err) {
            console.error("發送訊息失敗：", err.toString());
            addMessageToChat("系統", "無法發送訊息，請稍後再試。", true);
        }
    });
}

function createSignalRConnection(senderId, receiverId) {
    return new signalR.HubConnectionBuilder()
        .withUrl(`/chatHub?senderId=${senderId}&receiverId=${receiverId}`)
        .withAutomaticReconnect([0, 2000, 10000, 30000])
        .build();
}

function setupSignalREvents(connection, senderId, receiverId) {
    connection.onreconnecting(error => {
        console.warn(`連線中斷，正在重新連接：${error}`);
    });

    connection.onreconnected(connectionId => {
        console.log(`重新連接成功：${connectionId}`);
    });

    connection.on("ReceiveMessage", (user, message) => {
        console.log(`收到來自 ${user} 的訊息：${message};user的型別是${typeof user} `);
        let userName = (user.toString() == senderId) ? sender : receiver;
        console.log(`UserName是${userName}; sender是${sender}; receiver是${receiver}`)
        const time = new Date().toLocaleString();
        addMessageToChat(userName, message, time, false);
    });
}

async function loadChatHistory(senderId, receiverId) {
    try {
        const response = await fetch(`/api/chat/history?user1=${senderId}&user2=${receiverId}`);
        const messages = await response.json();
        messages.forEach(msg => {
            const time = new Date(msg.timestamp).toLocaleString();
            addMessageToChat(msg.senderName, msg.content, time, false);
        });
    } catch (err) {
        console.error("載入歷史訊息失敗：", err.toString());
    }
}

function addMessageToChat(user, message, time, isSender) {
    const li = document.createElement("li");
    li.classList.add(isSender ? "sent-message" : "received-message", "chat-message", "d-flex");

    const div = document.createElement("div");
    div.classList.add("message-content");

    const strong = document.createElement("strong");
    strong.textContent = user;

    const messageBody = document.createElement("div");
    messageBody.classList.add("message-body");
    messageBody.style.display = "flex";
    messageBody.style.justifyContent = "space-between";
    messageBody.style.alignItems = "center";

    const p = document.createElement("p");
    p.textContent = `${message}`;

    const span = document.createElement("span");
    span.classList.add("message-time");
    span.textContent = time;

    messageBody.appendChild(p);
    messageBody.appendChild(span);

    div.appendChild(strong);
    div.appendChild(messageBody);

    li.appendChild(div);
    messagesList.appendChild(li);

    scrollToBottom();
}

function scrollToBottom() {
    setTimeout(function () {
        messagesList.scrollTop = messagesList.scrollHeight;

    }, 100);
}

window.addEventListener("DOMContentLoaded", startChat);
