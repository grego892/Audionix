window.initializeAudioPlayer = (dotNetHelper) => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.addEventListener('playing', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Playing');
        });

        audioPlayer.addEventListener('pause', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Paused');
        });

        audioPlayer.addEventListener('waiting', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Buffering');
        });

        audioPlayer.addEventListener('error', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Error');
        });

        audioPlayer.addEventListener('ended', () => {
            dotNetHelper.invokeMethodAsync('UpdateStreamStatus', 'Stopped');
        });
    }
};

window.playAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.load();
        audioPlayer.play();
    }
};

window.pauseAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.stop();
    }
};

window.setVolume = (volume) => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.volume = volume;
    }
};
