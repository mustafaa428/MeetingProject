const token = sessionStorage.getItem('token');
const urlParams = new URLSearchParams(window.location.search);
const meetingId = urlParams.get("meetingId");

let isCameraOn = false;
let isMicOn = true;
let isRecording = false;
let localStream = null;
const peerConnections = {};
const pendingCandidates = {};

const configuration = {
    iceServers: [{ urls: "stun:stun.l.google.com:19302" }]
};

const API_URL = `http://localhost:5059/api/Meet/${meetingId}/AddParticipants`;

// JWT parsing
function parseJwt(token) {
    try {
        return JSON.parse(atob(token.split('.')[1]));
    } catch (e) {
        console.error("Invalid token format:", e);
        return null;
    }
}

const userInfo = token ? parseJwt(token) : null;
const username = userInfo ? userInfo.username || userInfo.email || "Unknown User" : "Unknown User";

// SignalR setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5059/chatHub", {
        accessTokenFactory: () => token,
    })
    .configureLogging(signalR.LogLevel.Information)
    .build();

connection.start().then(async () => {
    console.log("SignalR connected");
    await connection.invoke("JoinMeeting", meetingId);
    connection.invoke("NotifyNewParticipant", meetingId, connection.connectionId);
}).catch(err => console.error("SignalR connection error:", err));

// SignalR event handlers
connection.on("JoinMeetingSuccess", (meetingId) => {
    console.log("Successfully joined meeting:", meetingId);
});

connection.on("UserJoined", (connectionId) => {
    console.log("Another user joined:", connectionId);
});

connection.on("NewParticipantJoined", async (newConnectionId) => {
    await createPeerConnection(newConnectionId, true);
});

connection.on("ReceiveMessage", (user, message) => {
    const chatMessages = document.getElementById('chat-messages');
    const newMessage = document.createElement('li');
    newMessage.textContent = `${user}: ${message}`;
    chatMessages.appendChild(newMessage);
});

connection.on("ReceiveSignal", async (senderId, signal) => {
    const data = JSON.parse(signal);
    if (!peerConnections[senderId]) {
        await createPeerConnection(senderId);
    }

    const pc = peerConnections[senderId];

    if (data.sdp) {
        await pc.setRemoteDescription(new RTCSessionDescription(data.sdp));
        if (data.sdp.type === "offer") {
            const answer = await pc.createAnswer();
            await pc.setLocalDescription(answer);
            await connection.invoke("SendSignal", meetingId, connection.connectionId, JSON.stringify({ sdp: pc.localDescription }), senderId);
        }

        // Add pending ICE candidates
        if (pendingCandidates[senderId]) {
            for (let c of pendingCandidates[senderId]) {
                await pc.addIceCandidate(c);
            }
            pendingCandidates[senderId] = [];
        }
    } else if (data.candidate) {
        const candidate = new RTCIceCandidate(data.candidate);
        if (pc.remoteDescription && pc.remoteDescription.type) {
            await pc.addIceCandidate(candidate);
        } else {
            if (!pendingCandidates[senderId]) {
                pendingCandidates[senderId] = [];
            }
            pendingCandidates[senderId].push(candidate);
        }
    }
});

// Start local video/audio
async function startLocalStream() {
    try {
        localStream = await navigator.mediaDevices.getUserMedia({ video: true, audio: true });
        const videoElement = document.getElementById("video-stream");
        videoElement.srcObject = localStream;
        videoElement.style.display = "block";
        videoElement.style.transform = "scaleX(-1)";
        isCameraOn = true;
    } catch (error) {
        console.error("Stream error:", error);
        alert("Cannot access camera/microphone. Make sure permissions are allowed.");
    }
}

// Peer connection creation
async function createPeerConnection(participantId, isInitiator = false) {
    if (!localStream) {
        console.warn("Local stream not available. Cannot create peer connection.");
        return;
    }

    const pc = new RTCPeerConnection(configuration);
    peerConnections[participantId] = pc;

    localStream.getTracks().forEach(track => pc.addTrack(track, localStream));

    pc.onicecandidate = event => {
        if (event.candidate) {
            connection.invoke("SendSignal", meetingId, connection.connectionId, JSON.stringify({ candidate: event.candidate }), participantId);
        }
    };

    pc.ontrack = event => {
        addRemoteParticipant(participantId, event.streams[0]);
    };

    if (isInitiator) {
        const offer = await pc.createOffer();
        await pc.setLocalDescription(offer);
        connection.invoke("SendSignal", meetingId, connection.connectionId, JSON.stringify({ sdp: pc.localDescription }), participantId);
    }
}

// Add/remove remote video
function addRemoteParticipant(participantId, stream) {
    const videoSection = document.querySelector('.video-section');
    if (document.getElementById(`remote-video-${participantId}`)) return;
    const video = document.createElement('video');
    video.id = `remote-video-${participantId}`;
    video.autoplay = true;
    video.playsInline = true;
    video.srcObject = stream;
    video.classList.add('remote-video');
    videoSection.appendChild(video);
}
function removeRemoteParticipant(participantId) {
    const video = document.getElementById(`remote-video-${participantId}`);
    if (video) {
        video.srcObject = null;
        video.remove();
    }
}

// Toggle camera
async function toggleCamera() {
    const cameraIcon = document.getElementById('camera-icon');
    const videoElement = document.getElementById('video-stream');
    if (!isCameraOn) {
        await startLocalStream();
        cameraIcon.innerHTML = `<path d="M17 10.5l4-3.5v11l-4-3.5v3.5H3V7h14v3.5z"/>`;
        alert("Camera is now ON");
    } else {
        stopCamera(videoElement);
    }
}
function stopCamera(videoElement) {
    if (localStream) localStream.getTracks().forEach(track => track.stop());
    videoElement.style.display = "none";
    isCameraOn = false;
    document.getElementById('camera-icon').innerHTML = `<path d="M17 10.5l4-3.5v11l-4-3.5v3.5H3V7h10l-1.5-1.5L3 12l8.5 6.5L13 17z"/>`;
    alert("Camera is now OFF");
}

// Toggle microphone
function toggleMic() {
    if (!localStream) {
        alert("Microphone not ready.");
        return;
    }

    isMicOn = !isMicOn;
    localStream.getAudioTracks().forEach(track => track.enabled = isMicOn);
    alert(isMicOn ? "Microphone is now ON" : "Microphone is now OFF");

    const micIcon = document.getElementById('mic-icon');
    const iconPath = isMicOn
        ? `<path d="M12 14a3 3 0 0 0 3-3V5a3 3 0 0 0-6 0v6a3 3 0 0 0 3 3zm5-3a5 5 0 0 1-10 0H5a7 7 0 0 0 14 0h-2zm-5 7v4h2v-4h-2z"/>`
        : `<path d="M12 14a3 3 0 0 0 3-3V5a3 3 0 0 0-6 0v6a3 3 0 0 0 3 3zm-1-3h4v2h-4v-2zm-5-1h14v2H5v-2z"/>`;
    micIcon.innerHTML = iconPath;
}

// Send message
function sendMessage() {
    const input = document.getElementById('chat-input');
    const message = input.value.trim();
    if (message) {
        connection.invoke("SendMessage", meetingId, username, message).catch(err => console.error(err));
        input.value = "";
    }
}

// UI Toggles
function toggleParticipants() {
    document.getElementById('participants-panel').classList.toggle('open');
}
function toggleChat() {
    document.getElementById('chat-panel').classList.toggle('open');
}
function raiseHand() {
    const reactionDisplay = document.getElementById('reaction-display');
    reactionDisplay.textContent = "✋";
    setTimeout(() => reactionDisplay.textContent = "", 3000);
}
function toggleReactionMenu() {
    const menu = document.querySelector('.reaction-menu');
    menu.style.display = menu.style.display === 'flex' ? 'none' : 'flex';
}
function sendReaction(reaction) {
    const display = document.getElementById('reaction-display');
    display.textContent = reaction;
    setTimeout(() => display.textContent = '', 2000);
}
function toggleRecording() {
    isRecording = !isRecording;
    const recordIcon = document.getElementById('record-icon');
    alert(isRecording ? "Recording has started" : "Recording has stopped");
    const firstCircle = recordIcon.querySelector("circle:first-child");
    const secondCircle = recordIcon.querySelector("circle:last-child");
    firstCircle.setAttribute("fill", isRecording ? "red" : "#444");
    secondCircle.setAttribute("fill", isRecording ? "white" : "transparent");
}
function startScreenShare() {
    navigator.mediaDevices.getDisplayMedia({ video: true }).then((stream) => {
        const videoElement = document.getElementById('video-stream');
        videoElement.srcObject = stream;
    }).catch((error) => {
        console.error("Screen share error:", error);
        alert("Unable to start screen share.");
    });
}
function leaveMeeting() {
    const leaveConfirmation = confirm("Are you sure you want to leave the meeting?");
    if (leaveConfirmation) {
        window.location.href = "../home/index.html";
    }
}

// Add participant on load
async function addParticipant() {
    if (!token || !meetingId) return;
    try {
        await fetch(API_URL, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                "Authorization": `Bearer ${token}`,
            },
            body: JSON.stringify({}),
        });
    } catch (error) {
        console.error("Add participant error:", error);
    }
}

window.addEventListener("load", async () => {
    await startLocalStream();
    await addParticipant();
});
