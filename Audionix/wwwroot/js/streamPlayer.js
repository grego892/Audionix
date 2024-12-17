const audioContext = new (window.AudioContext || window.webkitAudioContext)();
const source = audioContext.createBufferSource();
const request = new XMLHttpRequest();

request.open('GET', 'https://audionix.djgrego.com:8433/stream', true);
request.responseType = 'arraybuffer';

request.onload = function () {
    audioContext.decodeAudioData(request.response, function (buffer) {
        source.buffer = buffer;
        source.connect(audioContext.destination);
        source.start(0);
    }, function (e) {
        console.error("Error with decoding audio data" + e.err);
    });
};

request.send(); 