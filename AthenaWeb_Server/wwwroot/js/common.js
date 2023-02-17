function setImageSource(ImageId, source) {
    const image = document.getElementById(ImageId);
    image.src = source;
}

function setInputDisabled(InputId, value) {
    const input = document.getElementById(InputId);
    input.disabled = value;
}


class MOG2MotionDetector {
    userId;
    cameraId;
    video;
    outputCanvas;
    outputCtx;
    capture;
    frame;
    foregroundMask;
    backgroundSubtractor;

    motionTimeout;
    eventTimeout;

    animationId;

    mqttClient;


    constructor(userId, cameraId) {
        this.userId = userId;
        this.cameraId = cameraId;

        this.motionCountThreshold = 10;
        this.motionTimeThreshold = 1000;
        this.eventTimeThreshold = 5000;

        this.motionCount = 0;

        this.currentEventHeaderIds = [];

        this.video = document.getElementById("localVideo");
        const height = this.video.height;
        const width = this.video.width;

        this.frame = new cv.Mat(height, width, cv.CV_8UC4);
        this.foregroundMask = new cv.Mat(height, width, cv.CV_8UC1);

        this.outputCanvas = document.getElementById("outputCanvas");
        this.outputCanvas.height = height;
        this.outputCanvas.width = width;

        this.outputCtx = outputCanvas.getContext("2d");
        this.outputCtx.clearRect(0, 0, width, height);

        this.backgroundSubtractor = new cv.BackgroundSubtractorMOG2(500, 16, true);

        this.mqttClient = new Paho.MQTT.Client("ictrobot.hknu.ac.kr", Number(8090), `client_${Date.now()}`);
        this.mqttClient.connect({
            useSSL: true,
            onSuccess: () => this.onConnected()
        });
    }

    onConnected() {
        console.log(`Mqtt 클라이언트가 성공적으로 연결됐습니다.`);
    }

    async executeMOG2MotionDetector() {
        this.capture.read(this.frame);
        this.backgroundSubtractor.apply(this.frame, this.foregroundMask);

        cv.threshold(this.foregroundMask, this.foregroundMask, 100, 255, cv.THRESH_BINARY);
        const nonZero = cv.countNonZero(this.foregroundMask);
        const percentNonZero = nonZero / (this.foregroundMask.rows * this.foregroundMask.cols);
        if (percentNonZero > 0.01) {
            this.motionCount++;

            if (this.motionTimeout) {
                clearTimeout(this.motionTimeout);
                this.motionTimeout = null;
            }
            this.motionTimeout = setTimeout(() => {
                this.motionCount = 0;
                this.motionTimeout = null;
            }, this.motionTimeThreshold);
        }

        if (this.motionCount >= this.motionCountThreshold) {
            await this.createEvent();
            console.log("이벤트가 발생했습니다.");

            this.motionCount = 0;
        }

        this.animationId = requestAnimationFrame(() => this.executeMOG2MotionDetector());
    }

    async createEvent() {
        this.outputCtx.drawImage(this.video, 0, 0, this.video.width, this.video.height);

        this.outputCtx.font = "24px Arial";
        this.outputCtx.fillStyle = "black";
        this.outputCtx.textAlign = "left";
        const timestamp = new Date();
        this.outputCtx.fillText(timestamp, 10, 30);

        const imageDataURL = this.outputCanvas.toDataURL();
        const event = {
            EventHeader: {
                UserId: this.userId,
                CameraId: this.cameraId,
                Created: timestamp,
                Path: imageDataURL,
                IsRequiredObjectDetection: true
            }
        };

        const createEventResponse = await fetch("http://ictrobot.hknu.ac.kr:8096/api/Event/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(event)
        });

        if (!createEventResponse.ok) {
            throw new Error(await createEventResponse.text());
        }

        const currentEventHeaderId = await createEventResponse.json();
        this.currentEventHeaderIds.push(currentEventHeaderId);
        console.log(`currentEventHeaderIds: ${this.currentEventHeaderIds}`)

        if (this.eventTimeout) {
            clearTimeout(this.eventTimeout);
            this.eventTimeout = null;
        }
        this.eventTimeout = setTimeout(() => {
            const createVideo = { EventHeaderIds: this.currentEventHeaderIds };
            const currentEventHeaderIdsMessage = new Paho.MQTT.Message(JSON.stringify(createVideo));
            currentEventHeaderIdsMessage.destinationName = "video/create";
            if (this.mqttClient.isConnected()) {
                this.mqttClient.send(currentEventHeaderIdsMessage);
            }
            this.currentEventHeaderIds.length = 0;
            this.eventTimeout = null;
            console.log("이벤트 종료");
        }, this.eventTimeThreshold);
    }

    static async initialize(userId, cameraId) {
        const mog2MotionDetector = new this(userId, cameraId);
        await mog2MotionDetector.getMedia();
        mog2MotionDetector.capture = new cv.VideoCapture(mog2MotionDetector.video);

        return mog2MotionDetector;
    }

    async getMedia() {
        const initialConstrains = {
            audio: true,
            video: true,
        };
        const stream = await navigator.mediaDevices.getUserMedia(initialConstrains);
        this.video.srcObject = stream;
    }
}


let motionDetector;


async function initializeMOG2MotionDetector(userId, cameraId) {
    try {
        motionDetector = await MOG2MotionDetector.initialize(userId, cameraId);

        motionDetector.executeMOG2MotionDetector();

    } catch (err) {
        console.log(err);
    }
}

function disposeMOG2MotionDetector() {
    cancelAnimationFrame(motionDetector.animationId);
    motionDetector.animationId = null;

    if (motionDetector.motionTimeout) {
        clearTimeout(motionDetector.motionTimeout);
        motionDetector.motionTimeout = null;
    }

    if (motionDetector.eventTimeout) {
        clearTimeout(motionDetector.eventTimeout);
        motionDetector.eventTimeout = null;
    }

    const tracks = motionDetector.video.srcObject.getTracks();
    tracks.forEach(track => {
        track.stop();
    });
    motionDetector.video = null;

    motionDetector.mqttClient.disconnect();
    motionDetector.mqttClient = null;

    motionDetector = null;
}