document.addEventListener('DOMContentLoaded', function () {
    const speakButton = document.getElementById('speakButton');
    const downloadButton = document.getElementById('downloadButton');
    const textArea = document.querySelector('textarea');
    const audioPlayer = document.getElementById('audioPlayer');

    speakButton.addEventListener('click', async function () {
        const text = textArea.value;
        if (text) {
            const audioUrl = await fetchAudioUrl(text);
            if (audioUrl) {
                audioPlayer.src = audioUrl;
                audioPlayer.play();
            }
        }
    });

    downloadButton.addEventListener('click', async function () {
        const text = textArea.value;
        if (text) {
            const audioUrl = await fetchAudioUrl(text);
            if (audioUrl) {
                const link = document.createElement('a');
                link.href = audioUrl;
                link.download = 'speech.mp3';
                link.click();
            }
        }
    });

    async function fetchAudioUrl(text) {
        const response = await fetch('/TextToSpeechHandler.ashx', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ text })
        });

        if (response.ok) {
            const result = await response.json();
            return result.audioUrl;
        }
        return null;
    }
});
