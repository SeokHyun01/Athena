//WebRTC 연결을 위한 클래스
class Camera {
    mediaStream;
    peerConnection;
    remoteVideo;


    constructor(connectionId, userId, cameraId) {
        this.localVideo = document.getElementById("video");
        this.muteButton = document.getElementById("muteButton");
        this.cameraButton = document.getElementById("cameraButton");
        this.camerasSelect = document.getElementById("camerasSelect");

        this.connectionId = connectionId;
        this.userId = userId;
        this.cameraId = cameraId;
    }

    handleMuteClick() {
        this.mediaStream
            .getAudioTracks()
            .forEach((track) => (track.enabled = !track.enabled));

        if (!this.muted) {
            this.muteButton.innerText = "Unmute";
            this.muted = true;

        } else {
            this.muteButton.innerText = "Mute";
            this.muted = false;
        }
    }

    handleCameraClick() {
        this.mediaStream
            .getVideoTracks()
            .forEach((track) => (track.enabled = !track.enabled));

        if (this.cameraOff) {
            if (this.localVideo.style.display == "none") {
                this.localVideo.style.display = "block";
            }

            this.cameraButton.innerText = "Turn Camera Off";
            this.cameraOff = false;

        } else {
            this.localVideo.style.display = "none";

            this.cameraButton.innerText = "Turn Camera On";
            this.cameraOff = true;
        }
    }

    async handleCameraChange() {
        await this.getMedia(this.camerasSelect.value);

        if (this.peerConnection) {
            const videoTrack = this.mediaStream.getVideoTracks()[0];

            const videoSender = this.peerConnection
                .getSenders()
                .find((sender) => sender.track.kind === "video");

            videoSender.replaceTrack(videoTrack);
        }
    }

    addEventListeners() {
        this.muteButton.addEventListener("click", () => this.handleMuteClick());
        this.cameraButton.addEventListener("click", () => this.handleCameraClick());
        this.camerasSelect.addEventListener("input", () => this.handleCameraChange());
    }

    async getCameras() {
        const devices = await navigator.mediaDevices.enumerateDevices();
        const cameras = devices.filter((device) => device.kind === "videoinput");
        const currentCamera = this.mediaStream.getVideoTracks()[0];
        for (const camera of cameras) {
            const option = document.createElement("option");
            option.value = camera.deviceId;
            option.text = camera.label;
            if (currentCamera.label === camera.label) {
                option.selected = true;
            }
            this.camerasSelect.appendChild(option);
        }
    }

    async getMedia(deviceId) {
        const initialConstrains = {
            audio: true,
            video: { facingMode: "user" },
        };
        const cameraConstraints = {
            audio: true,
            video: { deviceId: { exact: deviceId } },
        };

        try {
            this.mediaStream = await navigator.mediaDevices.getUserMedia(
                deviceId ? cameraConstraints : initialConstrains
            );
            this.localVideo.srcObject = this.mediaStream;
            this.localVideo.style.transform = "scaleX(-1)";

            if (!deviceId) {
                await this.getCameras();
            }

        } catch (err) {
            console.log(err);
        }
    }

    async handleIce(data) {
        if (data && data.candidate) {
            const connection = new signalR.HubConnectionBuilder().withUrl("/hubs/signaling").build();
            await connection.start();

            console.log(data.candidate);
            const ice = JSON.stringify(data.candidate);
            connection.send("SendIce", ice, this.connectionId);
            console.log("Connection Id:", this.connectionId);
            console.log("Send ice");

            await connection.stop();
        }
    }

    handleAddStream(data) {
        if (data && data.stream) {
            this.remoteVideo = document.getElementById("remoteVideo");
            this.remoteVideo.srcObject = data.stream;
            //좌우 반전
            this.remoteVideo.style.transform = "scaleX(-1)";

        }
    }

    createRTCPeerConnection() {
        this.peerConnection = new RTCPeerConnection({
            iceServers: [
                {
                    urls: [
                        "stun:stun.l.google.com:19302",
                        "stun:stun1.l.google.com:19302",
                        "stun:stun2.l.google.com:19302",
                        "stun:stun3.l.google.com:19302",
                        "stun:stun4.l.google.com:19302",
                        "stun:stun.stunprotocol.org:3478",
                        "stun:stun.voiparound.com:3478",
                        "stun:stun.voipbuster.com:3478",
                        "stun:stun.voipstunt.com:3478",
                        "stun:stun.voxgratia.org:3478"
                    ],
                },
            ],
        });

        this.peerConnection.addEventListener("icecandidate", (event) => this.handleIce(event));
        this.peerConnection.addEventListener("addstream", (event) => this.handleAddStream(event));
        this.mediaStream.getTracks().forEach((track) => this.peerConnection.addTrack(track, this.mediaStream));
    }
}

let camera;

//WebRTC 연결을 위한 객체 생성
async function initializeCamera(connectionId, userId, cameraId) {
    camera = new Camera(connectionId, userId, cameraId);
    camera.addEventListeners();

    await camera.getMedia();
    camera.createRTCPeerConnection();
}

async function sendOffer() {
    const offer = await camera.peerConnection.createOffer();
    camera.peerConnection.setLocalDescription(offer);

    console.log("Send offer");
    return JSON.stringify(offer);
}

async function sendAnswer(offer) {
    const jsonObject = JSON.parse(offer);
    const receivedOffer = new RTCSessionDescription(jsonObject);

    camera.peerConnection.setRemoteDescription(receivedOffer);
    console.log("Receive offer");

    const answer = await camera.peerConnection.createAnswer();
    camera.peerConnection.setLocalDescription(answer);

    console.log("Send answer");
    return JSON.stringify(answer);
}

function receiveAnswer(answer) {
    const jsonObject = JSON.parse(answer);
    const receivedAnswer = new RTCSessionDescription(jsonObject);

    camera.peerConnection.setRemoteDescription(receivedAnswer);
    console.log("Receive answer");
}

function receiveIce(ice) {
    const receivedIce = JSON.parse(ice);
    camera.peerConnection.addIceCandidate(receivedIce);
    console.log("Receive ice");
}

function getCurrentTime() {
    const today = new Date();
    const year = today.getFullYear().toString();

    let month = today.getMonth() + 1;
    month = month < 10 ? '0' + month.toString() : month.toString();

    let day = today.getDate();
    day = day < 10 ? '0' + day.toString() : day.toString();

    let hour = today.getHours();
    hour = hour < 10 ? '0' + hour.toString() : hour.toString();

    let minutes = today.getMinutes();
    minutes = minutes < 10 ? '0' + minutes.toString() : minutes.toString();

    let seconds = today.getSeconds();
    seconds = seconds < 10 ? '0' + seconds.toString() : seconds.toString();

    return year + month + day + hour + minutes + seconds;
}

function disposeVideo() {
    if(camera != null)
    //카메라 종료
    camera.localVideo.srcObject.getTracks().forEach(track => track.stop());   
}
