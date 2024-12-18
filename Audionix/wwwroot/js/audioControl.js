let reconnectAttempts = 0;
const maxReconnectAttempts = 5;
const reconnectInterval = 5000; // 5 seconds

window.initializeAudioPlayer = (dotNetHelper) => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.addEventListener('playing', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Playing');
            reconnectAttempts = 0;
        });

        audioPlayer.addEventListener('pause', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Paused');
        });

        audioPlayer.addEventListener('waiting', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Buffering');
        });

        audioPlayer.addEventListener('error', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Error');
            attemptReconnect();
        });

        audioPlayer.addEventListener('ended', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Stopped');
            attemptReconnect();
        });
    }
};

window.playAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.preload = 'auto';
        audioPlayer.load();
        audioPlayer.play();
    }
};

window.pauseAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.pause();
    }
};

window.setVolume = (volume) => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.volume = volume;
    }
};

function attemptReconnect() {
    if (reconnectAttempts < maxReconnectAttempts) {
        reconnectAttempts++;
        setTimeout(() => {
            console.log(`Reconnect attempt ${reconnectAttempts}`);
            playAudio();
        }, reconnectInterval);
    } else {
        console.log('Max reconnect attempts reached');
    }
}
