//카메라 On
window.setVideo = () => {
    //비디오를 지정하고 화면에 보여준다.
    var video = document.getElementById("video");
    if (navigator.mediaDevices && navigator.mediaDevices.getUserMedia) {
        navigator.mediaDevices.getUserMedia({ video: true }).then((stream) => {
            video.srcObject = stream;
            // 좌우 반전 코드 
            video.style.transform = "scaleX(-1)";
            video.play();
        });
    }
}

let _client;   //전역변수로 mqtt client 할당
let _characteristic;   //전역변수로 ble characteristic 할당
let _Dotnet;  //전역변수로 dotnet 객체 할당
//let _DotnetCallee;  //전역변수로 dotnet callee 객체 할당
let _cameraId;
let _isCamShift = false;
let _isTfjs = false;
let sendToVideo = [];
let size = 1;   // 사진 -> 영상으로 변환할 크기 1 -> 10장 을 1개의 영상으로 변환, 2 -> 20장을 1개의 영상으로 변환

const TOPIC_MOTOR = "camera/update/degree/syn";
const TOPIC_MOTOR_ACK = "camera/update/degree/ack";
const TOPIC_WEBRTC = "call/start";
const TOPIC_WEBRTC_FIN = "call/stop";
const TOPIC_PREVIEW = "camera/update/thumbnail";
const TOPIC_MAKE_VIDEO = "video/create";

//MQTT On
window.SetMqtt = () => {

    const SERV_ADDR = "ictrobot.hknu.ac.kr";
    const SERV_PORT = 8090;

    var client_id = Math.random().toString(36).substring(2, 12); //random한 id 
    //connection **************************
    const client = new Paho.MQTT.Client(SERV_ADDR, Number(SERV_PORT), client_id);
    client.connect({ useSSL: true, onSuccess: onConnect }); //connect the client using SSL 

    let video = document.getElementById("video");
    let canvasOutput = document.getElementById('canvasOutput');
    let fireCanvas = document.getElementById('fireCanvas');

    function onConnect() {
        //구독할 각종 토픽들을 구독한다

        //mqtt client를 전역변수로 할당한다.
        _client = client;

        _client.subscribe(TOPIC_MOTOR);
        _client.subscribe(TOPIC_WEBRTC);
        _client.subscribe(TOPIC_WEBRTC_FIN);
        _client.onMessageArrived = onMessageArrived;
    }

    //콜백 메서드로 메시지가 도착하면 호출 됨.
    function onMessageArrived(message) {
        //메시지 구분 
        console.log("topic: " + message.destinationName + " onMessageArrived: " + message.payloadString);

        // //메시지가 도착하면, 각각의 토픽에 따라서 다른 동작을 한다.
        if (message.destinationName === TOPIC_MOTOR) {
            //모터 제어
            let data = JSON.parse(message.payloadString);
            if (data.CameraId == _cameraId) {
                //JSON 객체로부터 각도를 가져온다.
                let degree = data.Degree + ".";
                if (_characteristic != null) {
                    //BLE로 각도를 전송한다.
                    _characteristic.writeValue(new TextEncoder().encode(degree));
                }
            }
        } else if (message.destinationName === TOPIC_WEBRTC) {
            //webrtc 연결
            try {
                let data = JSON.parse(message.payloadString);
                if (data.CameraId == _cameraId) {

                    if (canvasOutput.getAttribute("hidden") == null) {
                        canvasOutput.setAttribute("hidden", true);
                    }

                    if (fireCanvas.getAttribute("hidden") == null) {
                        fireCanvas.setAttribute("hidden", true);
                    }

                    video.removeAttribute("hidden");

                    //webRTC 연결을 시작한다.
                    _Dotnet.invokeMethodAsync("showWebRTC", true);
                }
            } catch (e) {
                console.log(e);
            }
        } else if (message.destinationName === TOPIC_WEBRTC_FIN) {
            //webrtc 연결 종료
            try {
                let data = JSON.parse(message.payloadString);
                if (data.CameraId == _cameraId) {

                    if (canvasOutput.getAttribute("hidden") != null && _isCamShift == true) {
                        canvasOutput.removeAttribute("hidden");
                        video.setAttribute("hidden", true);
                    }

                    if (fireCanvas.getAttribute("hidden") != null && _isTfjs == true) {
                        fireCanvas.removeAttribute("hidden");
                        video.setAttribute("hidden", true);
                    }
                    //오디오 스트림 제거 
                    video.srcObject.getAudioTracks().forEach(track => track.stop());

                    //webRTC 연결을 종료한다.
                    _Dotnet.invokeMethodAsync("showWebRTC", false);
                }
            } catch (e) {
                console.log(e);
            }
        }
    }

}

//썸네일 전송
window.SendThumbnail = (_width, _height) => {
   
    //연결이 되었다면, 썸네일 전송을 시작한다.
    setTimeout(sendThumbnail, 500);
    async function sendThumbnail() {
    
        let video = document.getElementById("video");
        let canvas = document.getElementById('canvas_image');
        canvas.width = _width;
        canvas.height = _height;
        let context = canvas.getContext('2d');
        context.drawImage(video, 0, 0, canvas.width, canvas.height); // 비디오 그리기
        result64 = canvas.toDataURL("image/jpeg", 0.8);
     
        let data = new Object();
        data.CameraId = _cameraId;
        data.Thumbnail = result64;
    
        message = new Paho.MQTT.Message(JSON.stringify(data));  //썸네일 내용 CameraId, Thumbnail
        message.destinationName = TOPIC_PREVIEW;    //보낼 토픽
        if (_client != null) {
            _client.send(message);  // MQTT로 썸네일 전송
        } else {
            console.log("mqtt client is not connected");
        }

        setTimeout(sendThumbnail, 500);
    }
}

window.reload = () => {
    location.reload();
}

//opencv.js 를 이용한 움직임 감지
window.camShift = (isCamShift) => {
    _isCamShift = isCamShift;

    // 일정 시간이 지나면, mqtt 전송
    let time1;
    let time2;
    let intervalTime;
    let isFirst = true;
    let count = 0;
    let isMotion = false;

    let video = document.getElementById("video", willReadFrequently = true);
    video.setAttribute('hidden', true);
    let canvasOutput = document.getElementById("canvasOutput", willReadFrequently = true);
    canvasOutput.removeAttribute('hidden');

    let cap = new cv.VideoCapture(video);
    let img_first = new cv.Mat(video.height, video.width, cv.CV_8UC4); let img_first_gray = new cv.Mat();
    let img_second = new cv.Mat(video.height, video.width, cv.CV_8UC4); let img_second_gray = new cv.Mat();
    let img_third = new cv.Mat(video.height, video.width, cv.CV_8UC4); let img_third_gray = new cv.Mat();
    cap.read(img_first); cap.read(img_second);
    cv.flip(img_first, img_first, 1); cv.flip(img_second, img_second, 1);

    const threshold_move = 50;    // 달라진 픽셀 값 기준치 설정 (defalut=50)
    const diff_compare = 20;      // 달라진 픽셀 갯수 기준치 설정 (defalut=10)
    const FPS = 15;               // FPS 설정 (default=15)

    function opencv_js_motion_detect() {
        cap.read(img_third); cv.flip(img_third, img_third, 1);
        let src = img_third.clone(); //src는 원본 이미지를 복사한 것
        //그레이 스케일로 변환
        cv.cvtColor(img_first, img_first_gray, cv.COLOR_RGBA2GRAY, 0);
        cv.cvtColor(img_second, img_second_gray, cv.COLOR_RGBA2GRAY, 0);
        cv.cvtColor(img_third, img_third_gray, cv.COLOR_RGBA2GRAY, 0);
        //차이 비교를 위한 이미지 생성
        let diff_1 = new cv.Mat();
        let diff_2 = new cv.Mat();
        //차이 비교
        cv.absdiff(img_first_gray, img_second_gray, diff_1);
        cv.absdiff(img_second_gray, img_third_gray, diff_2);
        //차이 비교 이미지를 이진화
        let diff_1_thres = new cv.Mat();
        let diff_2_thres = new cv.Mat();
        //threshold_move 값보다 크면 255, 작으면 0
        cv.threshold(diff_1, diff_1_thres, threshold_move, 255, cv.THRESH_BINARY);
        cv.threshold(diff_2, diff_2_thres, threshold_move, 255, cv.THRESH_BINARY);
        //두 차이 비교 이미지의 AND 연산
        let diff = new cv.Mat();
        cv.bitwise_and(diff_1_thres, diff_2_thres, diff);
        //AND 연산 결과에서 0이 아닌 픽셀의 갯수를 구함
        let diff_cnt = cv.countNonZero(diff);
        //AND 연산 결과에서 0이 아닌 픽셀의 좌표를 초기화
        let firstNonZeroIndex = [-1, -1];
        let lastNonZeroIndex = [-1, -1];

        if (diff_cnt > diff_compare) {
            let nZero = new cv.Mat(diff.rows, diff.cols, cv.CV_8UC1); //diff의 행과 열을 가진 nZero 생성
            for (let i = 0; i < diff.rows; i++) {
                for (let j = 0; j < diff.cols; j++) {
                    let index = i * diff.cols + j;
                    if (diff.data[index] !== 0) {
                        nZero.data[index] = diff.data[index];
                        //0이 아닌 픽셀의 좌표를 구함 
                        // 왼쪽 위의 좌표 값은 firstNonZeroIndex, 오른쪽 아래의 좌표 값은 lastNonZeroIndex
                        if (firstNonZeroIndex[0] === -1 || firstNonZeroIndex[0] > i && firstNonZeroIndex[1] > j) {
                            firstNonZeroIndex = [i, j];
                        }

                        if (lastNonZeroIndex[0] === -1 || lastNonZeroIndex[0] < i && lastNonZeroIndex[1] < j) {
                            lastNonZeroIndex = [i, j]
                        }
                    }
                }
            }
            //0이 아닌 픽셀의 좌표를 이용하여 사각형 그리기
            let point1 = new cv.Point(firstNonZeroIndex[1], firstNonZeroIndex[0]);
            let point2 = new cv.Point(lastNonZeroIndex[1], lastNonZeroIndex[0]);

            cv.rectangle(src, point1, point2, [0, 0, 255, 255], 1);
            // cv.putText(src, new Date().toLocaleString() , new cv.Point(10, 10), cv.FONT_HERSHEY_SIMPLEX, 0.3, [0, 0, 255, 255]);
            cv.putText(src, saveTime(), new cv.Point(10, 10), cv.FONT_HERSHEY_SIMPLEX, 0.3, [0, 0, 255, 255]);

            //시간 차이 계산 (단위 : s)
            time1 = new Date().getTime();
            isMotion = true;
            // intervalTime = (time2 - time1) / 1000;
            count++;
        }

        try {
            //시간 비교 
            time2 = new Date().getTime();
            intervalTime = (time1 - time2) / 1000;

            //움직임 감지가 30초 이상이면 count 초기화 , 남은 사진 id값으로 영상 생성
            if (intervalTime > 30) {

                if (sendToVideo.length > 0) {
                    sendMakeVideo();
                    sendToVideo = [];
                }

                count = 0;
                size = 1;
            }

            //움직임 감지 시 mqtt 전송 (30초 이내 5번 이상 움직임 감지 시)
            if (count >= 5 && isMotion) {
                sendMotion(canvasOutput);
                isMotion = false;
            }

            //움직임 감지가 5초이상 없다면 서버에 보고 (단, 한번만 보낸다.)
            if (sendToVideo.length >= (10 * size) && isFirst) {
                sendMakeVideo();
                sendToVideo = [];
                isFirst = false;
                size++;
            } else if (sendToVideo.length < (10 * size) && !isFirst) {
                isFirst = true;
            }

            cv.imshow(canvasOutput, src);

            if (isMotion) {
                time1 = time2;
            }

        } catch (err) {
            console.log(err);
        }
        // 다음 비교를 위해 영상 저장 및 메모리 해제
        src.delete();
        diff_1.delete(); diff_2.delete(); diff_1_thres.delete(); diff_2_thres.delete(); diff.delete();
        img_first.delete();
        img_first = img_second.clone();
        img_second.delete();
        img_second = img_third.clone();
    }

    function async_motion_detect() {
        opencv_js_motion_detect();
        setTimeout(async_motion_detect, 1000 / FPS);
    }

    async function sendMotion(canvasOutput) {

        // 이벤트 내용
        const event = {
            EventHeader: {
                CameraId: _cameraId,
                Created: new Date().toLocaleString(),
                Path: canvasOutput.toDataURL("image/jpeg"),
                IsRequiredObjectDetection: false
            }
        }
        // 이벤트 전송
        const createEventResponse = await fetch("https://ictrobot.hknu.ac.kr:8097/api/Event/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(event)
        });

        // 이벤트 전송 실패 시 에러 발생
        if (!createEventResponse.ok) {
            throw new Error(await createEventResponse.text());
        }

        //전송한 이벤트의 Id를 받음
        const checkId = await createEventResponse.json();
        sendToVideo.push(checkId);
        console.log(checkId);
    }

    async function sendMakeVideo() {
        let data = new Object();
        data.EventHeaderIds = sendToVideo;

        message = new Paho.MQTT.Message(JSON.stringify(data));
        message.destinationName = TOPIC_MAKE_VIDEO;
        _client.send(message);
        console.log("send make video");
    }

    function saveTime() {
        // 현재 시간을 구한다.
        const date = new Date();
        // 데이터의 형태를 지정한다. "년도-달-일 시.분.초" 형태이다.
        const options = {
            year: "numeric",
            month: "2-digit",
            day: "2-digit",
            hour: "2-digit",
            minute: "2-digit",
            second: "2-digit",
            hour12: false,
            timeZone: "Asia/Seoul",
        };
        // 형태에 맞게 시간을 String 형태로 변환한다.
        return new Intl.DateTimeFormat("ko-KR", options).format(date);
    }

    async_motion_detect(); // 비동기로 움직임 감지 시작
}


//tfjs를 이용한 화재 인식 
window.tfjs = (isTfjs) => {
    _isTfjs = isTfjs;

    let tfTime1;
    let tfTime2;
    let tfIntervalTime;
    let fireCount = 0;
    let tfIsFirst = true;
    let FPS = 10; //초당 프레임 수
    let isDetect = false; //화재 감지 여부

    let video = document.getElementById('video');
    video.setAttribute('hidden', true);
    let fireCanvas = document.getElementById('fireCanvas');
    fireCanvas.removeAttribute('hidden');
    let flippedCanvas = document.createElement('canvas');
    fireCanvas.width = video.width; fireCanvas.height = video.height; flippedCanvas.width = video.width; flippedCanvas.height = video.height; //캔버스 크기 설정


    tf.loadGraphModel('model/v8model.json').then(model => {
        setInterval(() => {
            tf.engine().startScope(); //메모리 관리를 위한 스코프 시작
            detect(model);
            tf.engine().endScope(); //스코프 종료
        }, 1000 / FPS);
    });

    function detect(model) {
        let fCtx = flippedCanvas.getContext('2d');
        fCtx.scale(-1, 1);
        fCtx.drawImage(video, -video.width, 0, video.width, video.height); //이미지를 그림
        const _imgData = fCtx.getImageData(0, 0, 640, 640); //이미지 데이터를 가져온다.

        let ctx = fireCanvas.getContext('2d');
        //단, 640 x 640 의 캔버스를 화면에는 비디오의 크기에 맞게 축소한다.
        ctx.drawImage(flippedCanvas, 0, 0, video.width, video.height);

        const tensor = tf.tidy(() => {
            return tf.browser.fromPixels(_imgData).div(255.0).expandDims(0); //이미지를 텐서로 변환한다.
        });

        excute(model, tensor, ctx);
    }

    function excute(model, tensor, ctx) {
        model.executeAsync(tensor,).then(result => { //모델을 통해 예측한다.
            const [boxes, scores, classes, numDetections] = result;
            const boxes_data = boxes.dataSync(); //박스의 좌표
            const scores_data = scores.dataSync(); //박스의 점수
            const classes_data = classes.dataSync(); //박스의 클래스
            const numDetections_data = numDetections.dataSync()[0]; //박스의 갯수
            tf.dispose(result); //메모리 해제

            var i;
            for (i = 0; i < numDetections_data; i++) {
                let [x1, y1, x2, y2] = boxes_data.slice(i * 4, (i + 1) * 4); //박스의 좌표
                // //비율을 수정한다. 426:240 -> 640:640 이므로 426/640 = 0.667 비율을 곱한다. 240/640 = 0.375 비율을 곱한다.
                x1 = x1 * 640;
                y1 = y1 * 640;
                x2 = x2 * 640;
                y2 = y2 * 640;

                const width = x2 - x1; //박스의 넓이
                const height = y2 - y1; //박스의 높이
                let klass = ""; //박스의 클래스
                if (classes_data[i] == 1) {
                    klass = "smoke";
                } else {
                    klass = "fire";
                }
                const score = (scores_data[i] * 100).toFixed() + "%"; //박스의 점수

                //박스를 그린다.
                if (klass == "smoke") {
                    ctx.lineWidth = "3";  //선의 두께 
                    ctx.strokeStyle = "gray";  //선의 색
                    ctx.strokeRect(x1, y1, width, height);  //선을 그린다.

                    ctx.font = "20px Arial"; //글자의 크기와 폰트
                    ctx.fillStyle = "gray"; //글자의 색
                    ctx.fillText(klass + " " + score, x1, y1); //글자를 그린다.
                } else {
                    ctx.lineWidth = "3";
                    ctx.strokeStyle = "red";
                    ctx.strokeRect(x1, y1, width, height);

                    ctx.font = "20px Arial";
                    ctx.fillStyle = "red";
                    ctx.fillText(klass + " " + score, x1, y1);
                }
            }
            //만약 화재가 감지되면 fireCount를 증가시킨다.
            if (numDetections_data > 0) {
                fireCount++;
                tfTime1 = new Date().getTime();
                isDetect = true;
            }

            try {
                tfTime2 = new Date().getTime();
                tfIntervalTime = (tfTime2 - tfTime1) / 1000;

                //만약 30초간 객체가 감지되지 않았다면 count 를 0으로 수정한다.
                if (tfIntervalTime > 30) {
                    if (sendToVideo.length > 0) {
                        sendTfMakeVideo();
                        sendToVideo = [];
                    }
                    fireCount = 0;
                    size = 1;
                }

                //5번 이상 객체가 감지되면 서버에 화재가 감지되었다고 전송한다.
                if (fireCount >= 5 && isDetect) {
                    sendDetect(boxes_data, classes_data, numDetections_data);
                    isDetect = false;
                }

                //사진의 갯수가 10개가 넘어가면 서버에게 사진 -> 영상으로 변환하라 알림.
                if (sendToVideo.length >= (10 * size) && tfIsFirst) {
                    sendTfMakeVideo();
                    sendToVideo = [];
                    tfIsFirst = false;
                    size++;
                } else if (sendToVideo.length < (10 * size) && !tfIsFirst) {
                    tfIsFirst = true;
                }

            } catch (e) {
                console.log(e);
            }
        });
    }

    async function sendDetect(boxes_data, classes_data, numDetections_data) {
        let detections = [];
        //json 배열 만들기
        for (var i = 0; i < numDetections_data; i++) {
            let [x1, y1, x2, y2] = boxes_data.slice(i * 4, (i + 1) * 4);
            let klass = "";
            if (classes_data[i] == 1) {
                klass = "smoke";
            } else {
                klass = "fire";
            }
            detections.push({
                "Left": parseInt((x1 * 640).toFixed()),
                "Top": parseInt((y1 * 640).toFixed()),
                "Right": parseInt((x2 * 640).toFixed()),
                "Bottom": parseInt((y2 * 640).toFixed()),
                "Label": klass
            });
        }

        const objectEvent = {
            EventHeader: {
                CameraId: _cameraId,
                Created: new Date().toLocaleString(),
                Path: flippedCanvas.toDataURL('image/jpeg'),
                IsRequiredObjectDetection: true
            },
            EventBodies: detections
        };
        const content = JSON.stringify(objectEvent);

        const createObjectEventResponse = await fetch("https://ictrobot.hknu.ac.kr:8097/api/Event/Create", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: content
        });

        if (!createObjectEventResponse.ok) {
            const result = await createObjectEventResponse.text();
            console.log(result);
        } else {
            const checkOjbectId = await createObjectEventResponse.json();
            if (checkOjbectId != 0) {
                sendToVideo.push(checkOjbectId);
                console.log(checkOjbectId);
            }
        }
    }

    async function sendTfMakeVideo() {
        let data = new Object();
        data.EventHeaderIds = sendToVideo;

        message = new Paho.MQTT.Message(JSON.stringify(data));
        message.destinationName = TOPIC_MAKE_VIDEO;
        _client.send(message);
    }
}


//블루투스 켜기 
window.SetBluetooth = () => {
    if (!navigator.bluetooth) {
        console.log('Web Bluetooth API is not available in this browser!');
        return false
    } else {
        connect();
    }

    async function connect() {
        navigator.bluetooth.requestDevice({
            filters: [{ services: ["0000ffe0-0000-1000-8000-00805f9b34fb"] }] //아두이노에 연결된 HM10의 표준 UUID
        }).then(device => {
            return device.gatt.connect(); //블루투스 연결
        }).then(server => {
            //서비스는 특성의 모음입니다. 예를 들어 "심장 박동 모니터"라는 서비스에는 "심장 박동 측정값"과 같은 특성이 포함됩니다. 
            //최상위 구조체, 다수의 Characteristics 보유
            return server.getPrimaryService("0000ffe0-0000-1000-8000-00805f9b34fb");
        }).then(service => {
            //특성에는 하나의 값과 특성의 값을 설명하는 0-n 설명자가 포함됩니다. 특성은 일종의 유형으로, 클래스와 유사하다고 생각하면 됩니다. 
            return service.getCharacteristic("0000ffe1-0000-1000-8000-00805f9b34fb");
        }).then(characteristic => {
            _characteristic = characteristic; //전역변수로 선언
            receiveData(characteristic); //데이터 수신
        });
    }


    function receiveData(characteristic) {
        characteristic.startNotifications().then(() => {
            characteristic.addEventListener('characteristicvaluechanged', event => {
                const value = new TextDecoder().decode(event.target.value);
                console.log('Received ' + value);
                //value값 받아서 처리
                let data = value.split('/');
                if (data[1] == "ack") {
                    //json 객체 mqtt로 전송
                    let mqttData = new Object();
                    mqttData.CameraId = _cameraId;
                    mqttData.Degree = Number.parseInt(data[0]);

                    let message = new Paho.MQTT.Message(JSON.stringify(mqttData));
                    message.destinationName = TOPIC_MOTOR_ACK;
                    _client.send(message);
                }
            });
        });

    }

}

//dotnet 객체를 가져온다.
window.dotnetHelper = (objRef, cameraId) => {
    _Dotnet = objRef;
    _cameraId = cameraId;
}

// //dotnet 객체를 가져온다.
// window.dotnetHelperCallee = (objRef) =>{
//    _DotnetCallee = objRef;
// }

// //dotnet에서 callee 종료 요청
// window.stopCallee = () => {
//     console.log("stopCallee 호출!")
//     _DotnetCallee.invokeMethodAsync("DisposeAsyncFromJs");
// }

//만약 페이지를 닫으면 mqtt 연결을 끊는다.
window.onbeforeunload = function () {

    //페이지가 닫히면 남은 이벤트를 비디오로 만든다.
    if (sendToVideo != null && sendToVideo.length > 0) {
        let data = new Object();
        data.EventHeaderIds = sendToVideo;

        message = new Paho.MQTT.Message(JSON.stringify(data));
        message.destinationName = TOPIC_MAKE_VIDEO;
        _client.send(message);
    }

    if (_client != null) {
        _client.disconnect();
    }
}