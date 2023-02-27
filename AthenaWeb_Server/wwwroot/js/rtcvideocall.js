class RTCVideoCall {
    mediaStream;
    peerConnection;
    remoteVideo;


    constructor(connectionId, userId, cameraId) {
        this.localVideo = document.getElementById("localVideo");

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
            .forEach((track) => {
                track.stop();
            });

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
            video: true,
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


let rtcVideoCall;


async function initializeRTCVideoCall(connectionId, userId, cameraId) {
    rtcVideoCall = new RTCVideoCall(connectionId, userId, cameraId);
    rtcVideoCall.addEventListeners();

    await rtcVideoCall.getMedia();
    rtcVideoCall.createRTCPeerConnection();
}

function disposeRTCVideoCall() {
    rtcVideoCall.mediaStream
        .getTracks()
        .forEach((track) => {
            track.stop();
        });

    rtcVideoCall = null;
}

async function sendOffer() {
    const offer = await rtcVideoCall.peerConnection.createOffer();
    rtcVideoCall.peerConnection.setLocalDescription(offer);

    console.log("Send offer");
    return JSON.stringify(offer);
}

async function sendAnswer(offer) {
    const jsonObject = JSON.parse(offer);
    const receivedOffer = new RTCSessionDescription(jsonObject);

    rtcVideoCall.peerConnection.setRemoteDescription(receivedOffer);
    console.log("Receive offer");

    const answer = await rtcVideoCall.peerConnection.createAnswer();
    rtcVideoCall.peerConnection.setLocalDescription(answer);

    console.log("Send answer");
    return JSON.stringify(answer);
}

function receiveAnswer(answer) {
    const jsonObject = JSON.parse(answer);
    const receivedAnswer = new RTCSessionDescription(jsonObject);

    rtcVideoCall.peerConnection.setRemoteDescription(receivedAnswer);
    console.log("Receive answer");
}

function receiveIce(ice) {
    const receivedIce = JSON.parse(ice);
    rtcVideoCall.peerConnection.addIceCandidate(receivedIce);
    console.log("Receive ice");
}