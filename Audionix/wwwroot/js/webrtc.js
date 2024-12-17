let localConnection;
let remoteConnection;
let localStream;
let remoteStream;
let sendChannel;
let receiveChannel;

async function startStreaming() {
    localConnection = new RTCPeerConnection();
    remoteConnection = new RTCPeerConnection();

    localStream = await navigator.mediaDevices.getUserMedia({ audio: true });
    localStream.getTracks().forEach(track => localConnection.addTrack(track, localStream));

    localConnection.onicecandidate = e => {
        if (e.candidate) {
            sendIceCandidate(e.candidate);
        }
    };

    remoteConnection.onicecandidate = e => {
        if (e.candidate) {
            sendIceCandidate(e.candidate);
        }
    };

    remoteConnection.ontrack = e => {
        remoteStream = e.streams[0];
        const audioElement = document.createElement('audio');
        audioElement.srcObject = remoteStream;
        audioElement.play();
    };

    const offer = await localConnection.createOffer();
    await localConnection.setLocalDescription(offer);
    sendOffer(offer);
}

async function stopStreaming() {
    localStream.getTracks().forEach(track => track.stop());
    localConnection.close();
    remoteConnection.close();
}

async function handleOffer(offer) {
    await remoteConnection.setRemoteDescription(new RTCSessionDescription(offer));
    const answer = await remoteConnection.createAnswer();
    await remoteConnection.setLocalDescription(answer);
    sendAnswer(answer);
}

async function handleAnswer(answer) {
    await localConnection.setRemoteDescription(new RTCSessionDescription(answer));
}

async function handleIceCandidate(candidate) {
    const iceCandidate = new RTCIceCandidate(candidate);
    await localConnection.addIceCandidate(iceCandidate);
    await remoteConnection.addIceCandidate(iceCandidate);
}

async function sendOffer(offer) {
    await connection.invoke("SendOffer", JSON.stringify(offer));
}

async function sendAnswer(answer) {
    await connection.invoke("SendAnswer", JSON.stringify(answer));
}

async function sendIceCandidate(candidate) {
    await connection.invoke("SendIceCandidate", JSON.stringify(candidate));
}