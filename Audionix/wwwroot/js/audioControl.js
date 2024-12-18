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

window.setVolume = (volume) => {
    const audioPlayer = document.getElementById('audioPlayer');
    if (audioPlayer) {
        audioPlayer.volume = volume;
    }
};