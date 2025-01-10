mergeInto(LibraryManager.library, {

    CopyToClipboardWeb: function (textToCopy) {
        const toCopy = UTF8ToString(textToCopy);
        const textArea = document.createElement("textarea");
        textArea.value = toCopy;
        textArea.style.top = "0";
        textArea.style.left = "0";
        textArea.style.position = "fixed";

        document.body.appendChild(textArea);
        textArea.focus();
        textArea.select();

        try {
            const successful = document.execCommand('copy');
        } catch (err) {
            console.error('Error coping', err);
        }

        document.body.removeChild(textArea);
        
    }
});