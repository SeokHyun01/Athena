function setImageSource(ImageId, source) {
    const image = document.getElementById(ImageId);
    image.src = source;
}

function setInputDisabled(InputId, value) {
    const input = document.getElementById(InputId);
    input.disabled = value;
}

const motionTimeThreshold = 1000;

let userId, cameraId;
let localVideo;
let outputCanvas, outputCtx;
let capture;
let frame, foregroundMask;
let lastMotionTime;
let motionCount = 0;
let motionCountThreshold = 10;
let mqttClient;


async function initializeBackgroundSubtractor(uId, camId) {
    try {
        userId = uId;
        cameraId = camId;

        localVideo = document.getElementById("localVideo");
        const height = localVideo.height;
        const width = localVideo.width;

        await getMedia();
        capture = new cv.VideoCapture(localVideo);

        frame = new cv.Mat(height, width, cv.CV_8UC4);
        foregroundMask = new cv.Mat(height, width, cv.CV_8UC1);

        outputCanvas = document.getElementById("outputCanvas");
        outputCanvas.height = height;
        outputCanvas.width = width;

        outputCtx = outputCanvas.getContext("2d");
        outputCtx.clearRect(0, 0, width, height);

        backgroundSubtractor = new cv.BackgroundSubtractorMOG2(500, 16, true);

        mqttClient = new Paho.MQTT.Client("ictrobot.hknu.ac.kr", Number(8090), `client_${Date.now()}`);

        executeBackgroundSubtractor();

    } catch (err) {
        console.log(err);
    }
}

function executeBackgroundSubtractor() {
    capture.read(frame);
    backgroundSubtractor.apply(frame, foregroundMask);

    cv.threshold(foregroundMask, foregroundMask, 100, 255, cv.THRESH_BINARY);
    const nonZero = cv.countNonZero(foregroundMask);
    const percentNonZero = nonZero / (foregroundMask.rows * foregroundMask.cols);
    if (percentNonZero > 0.01) {
        motionCount++;

        lastMotionTime = Date.now();

    } else {
        const currentMotionTime = Date.now();

        if (currentMotionTime - lastMotionTime > motionTimeThreshold) {
            motionCount = 0;
        }
    }

    if (motionCount >= motionCountThreshold) {
        outputCtx.drawImage(localVideo, 0, 0, localVideo.width, localVideo.height);

        outputCtx.font = "24px Arial";
        outputCtx.fillStyle = "black";
        outputCtx.textAlign = "left";
        const timestamp = Date.now();
        outputCtx.fillText(timestamp, 10, 30);

        const imageDataURL = outputCanvas.toDataURL();

        if (!mqttClient.isConnected()) {
            console.log("Mqtt 클라이언트 연결 중입니다...");
            mqttClient.connect({
                useSSL: true,
                onSuccess: () => onConnected()
            });
        }

        const event = {
            UserId: userId,
            CameraId: cameraId,
            Created: timestamp,
            Image: imageDataURL
        };

        const eventMessage = new Paho.MQTT.Message(JSON.stringify(event));
        console.log(`Event: ${JSON.stringify(event)}`);
        eventMessage.destinationName = "event/create";
        mqttClient.send(eventMessage);

        motionCount = 0;
    }

    requestAnimationFrame(executeBackgroundSubtractor);
}

async function getMedia() {
    const initialConstrains = {
        audio: true,
        video: true,
    };

    try {
        const stream = await navigator.mediaDevices.getUserMedia(initialConstrains);
        localVideo.srcObject = stream;

    } catch (err) {
        throw err;
    }
}

function onConnected() {
    console.log(`Mqtt 클라이언트가 성공적으로 연결됐습니다.`);
}