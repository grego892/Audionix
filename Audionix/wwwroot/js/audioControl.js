window.playAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.play();
    }
};

window.pauseAudio = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.pause();
    }
};

window.getAudioStatus = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        return {
            currentTime: audioPlayer.currentTime,
            duration: audioPlayer.duration,
            paused: audioPlayer.paused,
            volume: audioPlayer.volume,
            muted: audioPlayer.muted,
            ended: audioPlayer.ended
        };
    }
    return null;
};

window.getAudioMetadata = () => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        return {
            src: audioPlayer.src,
            currentSrc: audioPlayer.currentSrc,
            networkState: audioPlayer.networkState,
            readyState: audioPlayer.readyState,
            buffered: audioPlayer.buffered
        };
    }
    return null;
};

