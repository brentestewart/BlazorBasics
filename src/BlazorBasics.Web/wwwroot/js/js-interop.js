// C# → JS: write text to the system clipboard
export function copyToClipboard(text) {
    return navigator.clipboard.writeText(text);
}

// JS → C#: install a window-resize listener that calls back into .NET
const resizeHandlers = new Map();
let nextHandlerId = 0;

export function listenForResize(dotnetRef) {
    const id = ++nextHandlerId;
    const handler = () => {
        dotnetRef.invokeMethodAsync('OnResize', window.innerWidth, window.innerHeight);
    };
    window.addEventListener('resize', handler);
    resizeHandlers.set(id, handler);
    handler();
    return id;
}

export function stopListening(id) {
    const handler = resizeHandlers.get(id);
    if (handler) {
        window.removeEventListener('resize', handler);
        resizeHandlers.delete(id);
    }
}
